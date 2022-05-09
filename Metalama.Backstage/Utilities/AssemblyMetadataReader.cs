// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Metalama.Backstage.Utilities
{
    /// <summary>
    /// Reads the <see cref="AssemblyMetadataAttribute"/> defined by the build pipeline.
    /// These attributes are defined by Directory.Build.targets.
    /// </summary>
    public class AssemblyMetadataReader
    {
        private readonly Assembly _assembly;
        private readonly Dictionary<string, string?> _metadata = new( StringComparer.OrdinalIgnoreCase );
        private static readonly ConditionalWeakTable<Assembly, AssemblyMetadataReader> _instances = new();

        private string? _packageVersion;
        private DateTime? _buildDate;

        /// <summary>
        /// Gets the version of the package containing the current assembly.
        /// </summary>
        public string PackageVersion
            => this._packageVersion ??= this.TryGetValue( "PackageVersion", out var version )
                ? version
                : throw new InvalidOperationException(
                    $"The AssemblyMetadataAttribute with key 'PackageVersion' is not defined in assembly '{this._assembly.GetName()}'." );

        /// <summary>
        /// Gets the build date of the package containing the current assembly.
        /// </summary>
        public DateTime BuildDate
            => this._buildDate ??= this.TryGetValue( "PackageBuildDate", out var buildDateString )
                ? DateTime.Parse( buildDateString, CultureInfo.InvariantCulture )
                : throw new InvalidOperationException(
                    $"The AssemblyMetadataAttribute with key 'MetalamaBuildDate' is not defined in assembly '{this._assembly.GetName()}'." );

        private AssemblyMetadataReader( Assembly assembly )
        {
            this._assembly = assembly;

            foreach ( var attribute in assembly.GetCustomAttributes( typeof(AssemblyMetadataAttribute) ).Cast<AssemblyMetadataAttribute>() )
            {
                // In case of duplicates, we just ignore the first one. This happens with attributes describing package versions.
                this._metadata[attribute.Key] = attribute.Value;
            }
        }

        /// <summary>
        /// Gets an <see cref="AssemblyMetadataReader"/> for a given <see cref="Assembly"/>.
        /// </summary>
        public static AssemblyMetadataReader GetInstance( Assembly assembly ) => _instances.GetValue( assembly, a => new AssemblyMetadataReader( a ) );

        public bool TryGetValue( string key, [MaybeNullWhen( false )] out string value ) => this._metadata.TryGetValue( key, out value ) && value != null;

        /// <summary>
        /// Gets the package version with which the current assembly was built.
        /// </summary>
        public string GetPackageVersion( string packageName )
            => this.TryGetValue( "Package:" + packageName, out var version )
                ? version
                : throw new InvalidOperationException(
                    $"The AssemblyMetadataAttribute for package '{packageName}' is not defined in assembly '{this._assembly.GetName()}'." );

        /// <summary>
        /// Gets the unique BuildId for this assembly.
        /// </summary>
        public Guid ModuleId => this._assembly.ManifestModule.ModuleVersionId;

        /// <summary>
        /// Gets the major, minor, build, and revision numbers of the assembly.
        /// </summary>
        public Version AssemblyVersion
            => this._assembly.GetName().Version ?? throw new InvalidOperationException( $"Unknown version of assembly '{this._assembly.GetName()}'." );

        /// <summary>
        /// Gets the unique BuildId for the main assembly.
        /// </summary>
        public string BuildId
            => this.AssemblyVersion.ToString( 4 ) + "-" +
               string.Join( "", this.ModuleId.ToByteArray().Take( 4 ).Select( i => i.ToString( "x2", CultureInfo.InvariantCulture ) ) );
    }
}