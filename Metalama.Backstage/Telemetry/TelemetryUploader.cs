// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
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
    public sealed class TelemetryUploader
    {
        private readonly TelemetryConfiguration _configuration;
        private readonly IStandardDirectories _directories;
        private readonly IDateTimeProvider _time;
        private readonly IPlatformInfo _platformInfo;
        private readonly ILogger _logger;

        private readonly Uri _requestUri = new( "https://bits.postsharp.net:44301/upload" );

        public TelemetryUploader( IServiceProvider serviceProvider )
        {
            var configurationManager = serviceProvider.GetRequiredService<IConfigurationManager>();
            this._configuration = configurationManager.Get<TelemetryConfiguration>();

            this._directories = serviceProvider.GetRequiredService<IStandardDirectories>();
            this._time = serviceProvider.GetRequiredService<IDateTimeProvider>();
            this._platformInfo = serviceProvider.GetRequiredService<IPlatformInfo>();
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
                keyStream.Read( publicKey, 0, (int) keyStream.Length );

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
                    this._logger.Info?.Log( $"Packing '{file}'." );

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
                    this._logger.Info?.Log( "No file found." );

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

        private void StartWorker( string workerDirectory, string targetFramework )
        {
            string executableFileName;
            string arguments;

            if ( ProcessUtilities.IsNetCore() )
            {
                executableFileName = PlatformUtilities.GetDotNetPath( this._logger, this._platformInfo.DotNetSdkDirectory );
                arguments = $"\"{Path.Combine( workerDirectory, "Metalama.Backstage.Worker.dll" )}\"";
            }
            else
            {
                executableFileName = Path.Combine( workerDirectory, "Metalama.Backstage.Worker.exe" );
                arguments = "";
            }

            var processStartInfo = new ProcessStartInfo()
            {
                FileName = executableFileName,
                Arguments = arguments,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            this._logger.Info?.Log( $"Starting '{executableFileName}{(arguments == "" ? "" : " ")}{arguments}'." );

            Process.Start( processStartInfo );
        }

        /// <summary>
        /// Starts the telemetry upload in a background process avoiding the current processed being blocked during the update. 
        /// </summary>
        /// <param name="force">Starts the upload even when it's been started recently.</param>
        /// <remarks>
        /// The upload is started once per day. If the upload has been started in the past 24 hours, this method has no effect,
        /// unless the <paramref name="force"/> parameter is set to <c>true</c>.
        /// </remarks>
        public void StartUpload( bool force = false )
        {
            var now = this._time.Now;
            var lastUploadTime = this._configuration.LastUploadTime;

            if ( !force &&
                 lastUploadTime != null &&
                 lastUploadTime.Value.AddDays( 1 ) >= now )
            {
                this._logger.Info?.Log( $"It's not time to upload the telemetry yet. Now: {now} Last upload time: {lastUploadTime}" );

                return;
            }

            this._logger.Trace?.Log( "Acquiring mutex." );

            if ( !MutexHelper.WithGlobalLock( "TelemetryUploader", TimeSpan.FromMilliseconds( 1 ), out var mutex ) )
            {
                this._logger.Info?.Log( "Another upload is already being started." );

                return;
            }

            try
            {
                this._configuration.ConfigurationManager.Update<TelemetryConfiguration>( c => c.LastUploadTime = this._time.Now );

                var targetFramework = ProcessUtilities.IsNetCore()
                    ? "net6.0"
                    : "netframework4.7.2";

                var configuration =
#if DEBUG
                    "Debug";
#else
                    "Release";
#endif

                var version = AssemblyMetadataReader.GetInstance( typeof( TelemetryUploader ).Assembly ).PackageVersion;

                var workerDirectory = Path.Combine(
                    this._directories.ApplicationDataDirectory,
                    "Worker",
                    version,
                    configuration,
                    targetFramework );

                this.ExtractWorker( workerDirectory, targetFramework );

                this.StartWorker( workerDirectory, targetFramework );
            }
            finally
            {
                mutex.Dispose();
            }
        }

        private static string ComputeHash(string s)
        {
            var sha = SHA512.Create();
            var data = sha.ComputeHash( Encoding.UTF8.GetBytes( s ) );
            var builder = new StringBuilder();

            for ( var i = 0; i < data.Length; i++ )
            {
                builder.Append( data[i].ToString( "x2", CultureInfo.InvariantCulture ) );
            }

            return builder.ToString();
        }

        /// <summary>
        /// Uploads the telemetry.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="StartUpload"/> method to upload the telemetry without blocking the current process.
        /// </remarks>
        public async Task UploadAsync()
        {
            if ( !Directory.Exists( this._directories.TelemetryUploadQueueDirectory ) )
            {
                this._logger.Info?.Log( $"The telemetry upload queue directory '{this._directories.TelemetryUploadQueueDirectory}' doesn't exist. Assuming there's nothing to upload." );

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
                    this._logger.Info?.Log( $"No files found to be uploaded in '{this._directories.TelemetryUploadQueueDirectory}'." );

                    return;
                }
                
                // TODO: Stream the data directly to HTTP
                this.CreatePackage( files, packagePath, out filesToDelete );

                using var formData = new MultipartFormDataContent();
                using var packageFile = File.OpenRead( packagePath );

                var streamContent = new StreamContent( packageFile );
                formData.Add( streamContent, packageId, packageName );

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