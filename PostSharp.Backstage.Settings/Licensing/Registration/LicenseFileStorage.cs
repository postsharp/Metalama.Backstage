// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Extensibility.Extensions;
using PostSharp.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;
using System.IO;

namespace PostSharp.Backstage.Licensing.Registration
{
    /// <summary>
    /// Manages license file for license registration purposes.
    /// </summary>
    public class LicenseFileStorage
    {
        private readonly string _path;
        private readonly Dictionary<string, LicenseRegistrationData?> _licenses = new();

        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Gets licenses contained in the storage.
        /// </summary>
        /// <remarks>
        /// Includes both the licenses loaded from a license file and licenses pending to be stored.
        /// </remarks>
        public IReadOnlyDictionary<string, LicenseRegistrationData?> Licenses => this._licenses;

        /// <summary>
        /// Creates an empty storage.
        /// </summary>
        /// <param name="path">Path of the license file to be created or overwritten.</param>
        /// <param name="services">Services.</param>
        /// <returns>The empty storage.</returns>
        public static LicenseFileStorage Create( string path, IServiceProvider services )
        {
            var fileSystem = services.GetRequiredService<IFileSystem>();
            var storage = new LicenseFileStorage( path, fileSystem );

            return storage;
        }

        /// <summary>
        /// Opens a license file or creates an empty storage is the license file doesn't exist.
        /// </summary>
        /// <param name="path">Path of the license file to be loaded or created.</param>
        /// <param name="services">Services.</param>
        /// <returns></returns>
        public static LicenseFileStorage OpenOrCreate( string path, IServiceProvider services )
        {
            var storage = Create( path, services );

            if ( !storage._fileSystem.FileExists( path ) )
            {
                return storage;
            }

            var licenseStrings = storage._fileSystem.ReadAllLines( path );

            var factory = new LicenseFactory( services );

            foreach ( var licenseString in licenseStrings )
            {
                if ( string.IsNullOrWhiteSpace( licenseString ) )
                {
                    continue;
                }

                LicenseRegistrationData? data = null;

                if ( factory.TryCreate( licenseString, out var license ) )
                {
                    _ = license.TryGetLicenseRegistrationData( out data );
                }

                storage._licenses[licenseString] = data;
            }

            return storage;
        }

        private LicenseFileStorage( string path, IFileSystem fileSystem )
        {
            this._path = path;
            this._fileSystem = fileSystem;
        }

        /// <summary>
        /// Adds a license.
        /// </summary>
        /// <param name="licenseString">String representing the license to be stored in the license file.</param>
        /// <param name="data">Data represented by the <paramref name="licenseString"/>.</param>
        public void AddLicense( string licenseString, LicenseRegistrationData data )
        {
            this._licenses[licenseString] = data;
        }

        /// <summary>
        /// Removes a license.
        /// </summary>
        /// <param name="licenseString">String representing the license to be removed.</param>
        public void RemoveLicense( string licenseString )
        {
            this._licenses.Remove( licenseString );
        }

        /// <summary>
        /// Writes the licenses to a file.
        /// </summary>
        /// <remarks>
        /// Overwrites existing file.
        /// </remarks>
        public void Save()
        {
            var directory = Path.GetDirectoryName( this._path );
            this._fileSystem.CreateDirectory( directory! );
            this._fileSystem.WriteAllLines( this._path, this._licenses.Keys );
        }
    }
}