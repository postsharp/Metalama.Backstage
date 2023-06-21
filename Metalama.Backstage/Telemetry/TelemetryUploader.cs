// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Backstage.Telemetry
{
    internal sealed class TelemetryUploader : ITelemetryUploader
    {
        private readonly IStandardDirectories _directories;
        private readonly IDateTimeProvider _time;
        private readonly IPlatformInfo _platformInfo;
        private readonly ILogger _logger;
        private readonly Uri _requestUri = new( "https://bits.postsharp.net:44301/upload" );
        private readonly IConfigurationManager _configurationManager;
        private readonly ITempFileManager _tempFileManager;

        public TelemetryUploader( IServiceProvider serviceProvider )
        {
            this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();

            this._directories = serviceProvider.GetRequiredBackstageService<IStandardDirectories>();
            this._time = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
            this._platformInfo = serviceProvider.GetRequiredBackstageService<IPlatformInfo>();
            this._tempFileManager = serviceProvider.GetRequiredBackstageService<ITempFileManager>();
            this._logger = serviceProvider.GetLoggerFactory().Telemetry();
        }

        private static void CopyStream( Stream inputStream, Stream outputStream )
        {
            const int bufferLen = 16 * 1024;
            var buffer = new byte[bufferLen];
            int bytesRead;

            while ( (bytesRead = inputStream.Read( buffer, 0, bufferLen )) > 0 )
            {
                outputStream.Write( buffer, 0, bytesRead );
            }
        }

        private static void EncryptFile( string inputFile, string outputFile )
        {
            using ( Stream inputStream = File.OpenRead( inputFile ) )
            using ( Stream outputStream = File.Create( outputFile ) )
            {
                EncryptStream( inputStream, outputStream );
            }
        }

        private static void EncryptStream( Stream inputStream, Stream outputStream )
        {
            var cryptoStream = GetCryptoStream( outputStream );
            CopyStream( inputStream, cryptoStream );
            cryptoStream.FlushFinalBlock();
        }

        private static CryptoStream GetCryptoStream( Stream outputStream )
        {
            // Create a symmetric random key.
            var symmetricKey = new byte[256 / 8];
            var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes( symmetricKey );

            // Retrieve the public key.
            byte[] publicKey;
            string publicKeyXml;

            using (
                var keyStream = typeof(TelemetryUploader)
                    .Assembly
                    .GetManifestResourceStream( "Metalama.Backstage.public.key" ) )
            {
                if ( keyStream == null )
                {
                    throw new InvalidOperationException( "Public key not found." );
                }

                publicKey = new byte[keyStream.Length];
                var totalBytesRead = 0;

                while ( totalBytesRead < keyStream.Length )
                {
                    totalBytesRead += keyStream.Read( publicKey, totalBytesRead, (int) keyStream.Length - totalBytesRead );
                }

                keyStream.Position = 0;

                using var keyReader = new StreamReader( keyStream );
                publicKeyXml = keyReader.ReadToEnd();
            }

            // Compute a hash of the public key.
            var sha = SHA512.Create();
            var publicKeyHash = sha.ComputeHash( publicKey );

            // Encrypt the random key using the public key.
            using var rsa = RSA.Create();
            rsa.FromXmlString( publicKeyXml );

            var encryptedSymmetricKey = rsa.Encrypt( symmetricKey, RSAEncryptionPadding.Pkcs1 );

            var aes = Aes.Create();
            aes.GenerateIV();

            var writer = new BinaryWriter( outputStream );

            // Write the version.
            // 0 = Windows-specific PostSharp implementation.
            // 1 = Metalama multi-platform implementation. 
            writer.Write( 1 );

            // Write the public key hash.
            writer.Write( publicKeyHash.Length );
            writer.Write( publicKeyHash );

            // Write the encrypted key
            writer.Write( encryptedSymmetricKey.Length );
            writer.Write( encryptedSymmetricKey );

            // Write the initial vector.
            writer.Write( aes.IV.Length );
            writer.Write( aes.IV );

            // Encrypt the package content.
            return new CryptoStream(
                outputStream,
                aes.CreateEncryptor( symmetricKey, aes.IV ),
                CryptoStreamMode.Write );
        }

        private void CreatePackage( IEnumerable<string> files, string outputPath, out IEnumerable<string> filesToDelete )
        {
            var filesToDeleteLocal = new List<string>();
            string? tempPackagePath = null;
            Package? package = null;

            try
            {
                foreach ( var file in files )
                {
                    this._logger.Trace?.Log( $"Packing '{file}'." );

                    // Attempt to open that file. Skip the file if we can't open it.
                    try
                    {
                        using ( Stream stream = File.Open( file, FileMode.Open, FileAccess.Read, FileShare.None ) )
                        {
                            // Create a ZIP package if it does not exist yet.
                            if ( package == null )
                            {
                                tempPackagePath = Path.GetTempFileName();
                                package = Package.Open( tempPackagePath, FileMode.Create );
                            }

                            string? mime = null;

                            // Add the file to the zip.
                            var packagePart =
                                package.CreatePart(
                                    new Uri( "/" + Uri.EscapeDataString( Path.GetFileName( file ) ), UriKind.Relative ),
                                    mime ?? MediaTypeNames.Application.Octet,
                                    CompressionOption.Maximum );

                            using ( var packagePartStream = packagePart.GetStream( FileMode.Create ) )
                            {
                                CopyStream( stream, packagePartStream );
                            }
                        }

                        filesToDeleteLocal.Add( file );
                    }
                    catch ( Exception e )
                    {
                        this._logger.Error?.Log( $"Cannot pack file '{file}': {e.Message}" );
                    }
                }

                filesToDelete = filesToDeleteLocal;

                if ( package == null )
                {
                    // We did not find any file.
                    this._logger.Trace?.Log( "No file found." );

                    return;
                }

                package.Close();

                // Encrypt the package.
                EncryptFile( tempPackagePath!, outputPath );
            }
            finally
            {
                if ( tempPackagePath != null && File.Exists( tempPackagePath ) )
                {
                    File.Delete( tempPackagePath );
                }
            }
        }

        private void ExtractWorker( string workerDirectory, string targetFramework )
        {
            var touchFile = Path.Combine( workerDirectory, "unzipped.touch" );

            if ( !File.Exists( touchFile ) )
            {
                using ( MutexHelper.WithGlobalLock( touchFile ) )
                {
                    if ( !File.Exists( touchFile ) )
                    {
                        Directory.CreateDirectory( workerDirectory );

                        var zipResourceName = $"Metalama.Backstage.Metalama.Backstage.Worker.{targetFramework}.zip";
                        var assembly = this.GetType().Assembly;
                        using var resourceStream = assembly.GetManifestResourceStream( zipResourceName );

                        if ( resourceStream == null )
                        {
                            throw new InvalidOperationException( $"Resource '{zipResourceName}' not found in '{assembly.Location}'." );
                        }

                        using var zipStream = new ZipArchive( resourceStream );
                        zipStream.ExtractToDirectory( workerDirectory );

                        File.WriteAllText( touchFile, "" );
                    }
                }
            }
        }

        private void StartWorker( string workerDirectory )
        {
            string executableFileName;
            string arguments;

            if ( ProcessUtilities.IsNetCore() )
            {
                executableFileName = this._platformInfo.DotNetExePath;
                arguments = $"\"{Path.Combine( workerDirectory, "Metalama.Backstage.Worker.dll" )}\"";
            }
            else
            {
                executableFileName = Path.Combine( workerDirectory, "Metalama.Backstage.Worker.exe" );
                arguments = "";
            }

            var processStartInfo = new ProcessStartInfo()
            {
                FileName = executableFileName, Arguments = arguments, UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden
            };

            this._logger.Info?.Log( $"Starting '{executableFileName}{(arguments == "" ? "" : " ")}{arguments}'." );

            Process.Start( processStartInfo );
        }

        /// <inheritdoc />
        public void StartUpload( bool force = false )
        {
            var now = this._time.Now;

            this._logger.Trace?.Log( "Acquiring mutex." );

            if ( !this._configurationManager.UpdateIf<TelemetryConfiguration>(
                    c => force ||
                         c.LastUploadTime == null ||
                         c.LastUploadTime.Value.AddDays( 1 ) < now,
                    c => c with { LastUploadTime = this._time.Now } ) )
            {
                this._logger.Trace?.Log( $"It's not time to upload the telemetry yet." );

                return;
            }

            var targetFramework = ProcessUtilities.IsNetCore()
                ? "net6.0"
                : "netframework4.7.2";

            var version = AssemblyMetadataReader.GetInstance( typeof(TelemetryUploader).Assembly ).PackageVersion;

            if ( version == null )
            {
                throw new InvalidOperationException( $"Unknown version of '{typeof(TelemetryUploader).Assembly}' assembly package." );
            }

            var workerDirectory =
                this._tempFileManager.GetTempDirectory( "BackstageWorker", subdirectory: targetFramework, cleanUpStrategy: CleanUpStrategy.WhenUnused );

            this.ExtractWorker( workerDirectory, targetFramework );

            this.StartWorker( workerDirectory );
        }

        private static string ComputeHash( string s )
        {
            var sha = SHA512.Create();
            var data = sha.ComputeHash( Encoding.UTF8.GetBytes( s ) );
            var builder = new StringBuilder();

            foreach ( var t in data )
            {
                builder.Append( t.ToString( "x2", CultureInfo.InvariantCulture ) );
            }

            return builder.ToString();
        }

        /// <inheritdoc />
        public async Task UploadAsync()
        {
            if ( !Directory.Exists( this._directories.TelemetryUploadQueueDirectory ) )
            {
                this._logger.Trace?.Log(
                    $"The telemetry upload queue directory '{this._directories.TelemetryUploadQueueDirectory}' doesn't exist. Assuming there's nothing to upload." );

                return;
            }

            Directory.CreateDirectory( this._directories.TelemetryUploadPackagesDirectory );

            var packageId = Guid.NewGuid().ToString();
            var packageName = packageId + ".psf";
            var packagePath = Path.Combine( this._directories.TelemetryUploadPackagesDirectory, packageName );

            IEnumerable<string> filesToDelete;

            try
            {
                var files = Directory.GetFiles( this._directories.TelemetryUploadQueueDirectory );

                if ( files.Length == 0 )
                {
                    this._logger.Trace?.Log( $"No files found to be uploaded in '{this._directories.TelemetryUploadQueueDirectory}'." );

                    return;
                }

                // TODO: Stream the data directly to HTTP
                this.CreatePackage( files, packagePath, out filesToDelete );

                using var formData = new MultipartFormDataContent();

                // ReSharper disable once UseAwaitUsing
                using var packageFile = File.OpenRead( packagePath );

                var streamContent = new StreamContent( packageFile );
                formData.Add( streamContent, packageId, packageName );

                // ReSharper disable once StringLiteralTypo
                const string salt = @"<27e\)$a<=b9&zyVwjzaJ`!WW`rwHh~;Z5QAC.J5TQ`.NY"")]~FGA);AKSSmbV$M";
                var check = ComputeHash( packageName + salt );

                using var client = new HttpClient();
                var response = await client.PutAsync( $"{this._requestUri}?check={check}", formData );

                if ( !response.IsSuccessStatusCode )
                {
                    throw new InvalidOperationException( $"Request failed: {response.StatusCode} {response.ReasonPhrase}" );
                }
            }
            catch ( Exception exception )
            {
                this._logger.Error?.Log( exception.ToString() );

                throw;
            }
            finally
            {
                if ( File.Exists( packagePath ) )
                {
                    File.Delete( packagePath );
                }
            }

            // Delete the files that have just been sent.
            foreach ( var file in filesToDelete )
            {
                File.Delete( file );
            }
        }
    }
}