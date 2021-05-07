// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Testing.Services
{
    public class ApplicationInfoService : IApplicationInfoService
    {
        public ApplicationInfoService( bool isPrerelease, Version version, DateTime buildDate )
        {
            this.IsPrerelease = isPrerelease;
            this.Version = version;
            this.BuildDate = buildDate;
        }

        public bool IsPrerelease { get; }

        public Version Version { get; }

        public string VersionString => this.Version.ToString( 3 );

        public DateTime BuildDate { get; }
    }
}
