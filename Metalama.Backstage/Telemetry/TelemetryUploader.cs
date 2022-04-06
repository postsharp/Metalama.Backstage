// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Backstage.Telemetry
{
    public sealed class TelemetryUploader
    {
        private readonly TelemetryConfiguration _configuration;
        private readonly IConfigurationManager _configurationManager;
        private readonly IStandardDirectories _directories;
        private readonly IDateTimeProvider _time;
        private readonly ILogger _logger;

        private readonly Uri _requestUri = new( "https://localhost:7031/upload" );

        public TelemetryUploader( IServiceProvider serviceProvider )
        {
            this._configurationManager = serviceProvider.GetRequiredService<IConfigurationManager>();
            this._configuration = this._configurationManager.Get<TelemetryConfiguration>();

            this._directories = serviceProvider.GetRequiredService<IStandardDirectories>();
            this._time = serviceProvider.GetRequiredService<IDateTimeProvider>();
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

            using (
                var keyStream = typeof( TelemetryUploader )
                    .Assembly
                    .GetManifestResourceStream( "Metalama.Backstage.Telemetry.Diagnostics-Public.csp" )! )
            {
                publicKey = new byte[keyStream.Length];
                keyStream.Read( publicKey, 0, (int) keyStream.Length );
            }

            // Compute a hash of the public key.
#pragma warning disable CA5350 // Weak hash.
            var sha1 = SHA1.Create();
#pragma warning restore CA5350
            var publicKeyHash = sha1.ComputeHash( publicKey );

            // Encrypt the random key using the public key.
            var cspParameters = new CspParameters( 1 ) { KeyNumber = (int) KeyNumber.Exchange };
            var rsa = new RSACryptoServiceProvider( cspParameters );
            rsa.ImportCspBlob( publicKey );
            var encryptedSymmetricKey = rsa.Encrypt( symmetricKey, false );

            var rijndael = Rijndael.Create();
            rijndael.GenerateIV();

            var writer = new BinaryWriter( outputStream );

            // Write the version.
            writer.Write( 0 );

            // Write the public key hash.
            writer.Write( publicKeyHash.Length );
            writer.Write( publicKeyHash );

            // Write the encrypted key
            writer.Write( encryptedSymmetricKey.Length );
            writer.Write( encryptedSymmetricKey );

            // Write the initial vector.
            writer.Write( rijndael.IV.Length );
            writer.Write( rijndael.IV );

            // Encrypt the package content.
            return new CryptoStream(
                outputStream,
                rijndael.CreateEncryptor( symmetricKey, rijndael.IV ),
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
                    this._logger?.Info?.Log( $"Packing '{file}'." );

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
                        this._logger?.Error?.Log( $"Cannot pack file '{file}': {e.Message}" );
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

        public void StartUpload( bool force = false )
        {
            var now = this._time.Now;
            var lastUploadTime = this._configuration.LastUploadTime;

            if ( !force &&
                 lastUploadTime != null &&
                 lastUploadTime.Value.AddDays( 1 ) >= now )
            {
                this._logger?.Info?.Log( $"It's not time to upload the telemetry yet. Now: {now} Last upload time: {lastUploadTime}" );
                return;
            }

            const string mutexName = "Global\\Metalama.Backstage.Telemetry.TelemetryUploader";

            this._logger?.Trace?.Log( $"Acquiring '{mutexName}' mutex." );

            using var m = new Mutex( false, mutexName );

            if ( !m.WaitOne( 1 ) )
            {
                this._logger?.Info?.Log( "Another upload is already being started." );
                return;
            }

            try
            {
                this._configuration.ConfigurationManager.Update<TelemetryConfiguration>( c => c.LastUploadTime = this._time.Now );

                var assemblyPath = this.GetType().Assembly.Location;

                string executableFileName;
                string arguments;

                if ( Path.GetExtension( assemblyPath ) == ".exe" )
                {
                    executableFileName = assemblyPath;
                    arguments = "";
                }
                else
                {
                    executableFileName = "dotnet";
                    arguments = assemblyPath;
                }

                var processStartInfo = new ProcessStartInfo()
                {
                    FileName = executableFileName,
                    Arguments = arguments,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                this._logger?.Info?.Log( $"Starting '{executableFileName} {arguments}' from '{assemblyPath}'." );

                Process.Start( processStartInfo );
            }
            finally
            {
                m.ReleaseMutex();
            }
        }

        public async Task UploadAsync()
        {
            if ( !Directory.Exists( this._directories.TelemetryUploadQueueDirectory ) )
            {
                this._logger?.Info?.Log( $"The telemetry upload queue directory '{this._directories.TelemetryUploadQueueDirectory}' doesn't exist. Assuming there's nothing to upload." );
                return;
            }

            var packageId = Guid.NewGuid().ToString();
            var packageName = packageId + ".psf";
            var packagePath = Path.Combine( this._directories.TelemetryUploadPackagesDirectory, packageName );

            IEnumerable<string> filesToDelete;

            try
            {
                // TODO: Stream the data directly to HTTP
                this.CreatePackage( Directory.GetFiles( this._directories.TelemetryUploadQueueDirectory ), packagePath, out filesToDelete );

                if ( !File.Exists( packagePath ) )
                {
                    this._logger?.Info?.Log( $"The package '{packagePath}' has not been created. Assuming there's nothing to upload." );
                    return;
                }

                using var formData = new MultipartFormDataContent();
                using var packageFile = File.OpenRead( packagePath );
                var streamContent = new StreamContent( packageFile );
                formData.Add( streamContent, packageId, packageName );

                using var client = new HttpClient();
                await client.PutAsync( this._requestUri, formData );
            }
            catch ( Exception exception )
            {
                this._logger.Error?.Log( exception.ToString() );

                return;
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