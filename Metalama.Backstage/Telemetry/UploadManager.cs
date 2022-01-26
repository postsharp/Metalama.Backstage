// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;

namespace Metalama.Backstage.Telemetry
{
    internal sealed class UploadManager
    {
        private readonly IStandardDirectories _directories;
        private readonly ILogger _logger;

        // private readonly Uri _serverUrl = new( "https://bits.postsharp.net" );

        public UploadManager( IServiceProvider serviceProvider )
        {
            this._directories = serviceProvider.GetRequiredService<IStandardDirectories>();
            this._logger = serviceProvider.GetLoggerFactory().Telemetry();
        }

        public void EnqueueFile( string file )
        {
            var directory = this._directories.TelemetryUploadQueueDirectory;

            if ( !Directory.Exists( directory ) )
            {
                Directory.CreateDirectory( directory );
            }

            File.Move( file, Path.Combine( directory, Path.GetFileName( file ) ) );
        }

        public void EnqueueContent( string fileName, string contents )
        {
            if ( !Directory.Exists( this._directories.TelemetryUploadQueueDirectory ) )
            {
                Directory.CreateDirectory( this._directories.TelemetryUploadQueueDirectory );
            }

            File.WriteAllText( Path.Combine( this._directories.TelemetryUploadQueueDirectory, fileName ), contents, Encoding.UTF8 );
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
                var keyStream = typeof(UploadManager).Assembly.GetManifestResourceStream( "PostSharp.Settings.Ceip.Diagnostics-Public.csp" )! )
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

            foreach ( var file in files )
            {
                Console.WriteLine( "Packaging '{0}'.", file );

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
                    this._logger.Error?.Log( "Cannot package file " + file + ": " + e.Message );
                    Console.WriteLine( "Cannot package file '{0}': {1}", file, e.Message );
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

        public void Upload()
        {
            if ( !Directory.Exists( this._directories.TelemetryUploadQueueDirectory ) )
            {
                return;
            }

            // Open the registry key now so if we fail, we fail safe.
            {
                var packageName = Guid.NewGuid().ToString() + ".psf";
                var packagePath = Path.Combine( this._directories.TelemetryUploadPackagesDirectory, packageName );

                IEnumerable<string> filesToDelete;

                try
                {
                    this.CreatePackage( Directory.GetFiles( this._directories.TelemetryUploadQueueDirectory ), packagePath, out filesToDelete );

                    // TODO: Upload the package.
                }
                catch ( Exception exception )
                {
                    this._logger.Error?.Log( exception.ToString() );

                    return;
                }

                // Delete the files that have just been zipped.
                foreach ( var file in filesToDelete )
                {
                    File.Delete( file );
                }
            }
        }
    }
}