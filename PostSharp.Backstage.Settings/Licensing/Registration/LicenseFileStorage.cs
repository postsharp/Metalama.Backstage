// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;

namespace PostSharp.Backstage.Licensing.Registration
{
    public class LicenseFileStorage
    {
        private readonly string _path;
        private readonly Dictionary<string, LicenseRegistrationData?> _licenses = new();

        private readonly IFileSystemService _fileSystem;

        public IReadOnlyDictionary<string, LicenseRegistrationData?> Licenses => this._licenses;

        public static LicenseFileStorage Create( string path, IServiceProvider services, ITrace trace )
        {
            var fileSystem = services.GetService<IFileSystemService>();
            var storage = new LicenseFileStorage( path, fileSystem );
            return storage;
        }

        public static LicenseFileStorage OpenOrCreate( string path, IServiceProvider services, ITrace trace )
        {
            var storage = Create( path, services, trace );

            if ( !storage._fileSystem.FileExists( path ) )
            {
                return storage;
            }

            var licenseStrings = storage._fileSystem.ReadAllLines( path );

            var factory = new LicenseFactory( services, trace );

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

        private LicenseFileStorage(string path, IFileSystemService fileSystem)
        {
            this._path = path;
            this._fileSystem = fileSystem;
        }

        public void AddLicense( string licenseString, LicenseRegistrationData data )
        {
            this._licenses[licenseString] = data;
        }

        public void RemoveLicense(string licenseString)
        {
            this._licenses.Remove( licenseString );
        }

        public void Save()
        {
            this._fileSystem.WriteAllLines( this._path, this._licenses.Keys );
        }
    }
}
