// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using System;
using System.Reflection;

namespace Metalama.Backstage.Extensibility
{
    public abstract class ApplicationInfoBase : IApplicationInfo
    {
        public ApplicationInfoBase(Assembly metadataAssembly)
        {
            var reader = AssemblyMetadataReader.GetInstance( metadataAssembly );
            this.Version = reader.PackageVersion;
#pragma warning disable CA1307
            this.IsPrerelease = this.Version.Contains( "-" );
#pragma warning restore CA1307
            this.BuildDate = reader.BuildDate;
        }

        public abstract string Name { get; }

        public string Version { get; }

        public bool IsPrerelease { get; }

        public DateTime BuildDate { get; }

        public abstract ProcessKind ProcessKind { get; }

        public abstract bool IsLongRunningProcess { get; }

        public abstract bool IsUnattendedProcess( ILoggerFactory loggerFactory );
    }
}