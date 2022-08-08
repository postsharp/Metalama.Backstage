// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Extensibility;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Metalama.Backstage.Maintenance;

internal class TempFileManager : ITempFileManager
{
    private readonly IApplicationInfoProvider _applicationInfoProvider;
    private readonly IStandardDirectories _standardDirectories;

    public TempFileManager( IServiceProvider serviceProvider )
    {
        this._standardDirectories = serviceProvider.GetRequiredBackstageService<IStandardDirectories>();
        this._applicationInfoProvider = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>();
    }

    public string GetTempDirectory( string subdirectory, CleanUpStrategy cleanUpStrategy, Guid? guid )
    {
        var directory = Path.Combine(
            this._standardDirectories.TempDirectory,
            subdirectory,
            this._applicationInfoProvider.CurrentApplication.Version,
            guid?.ToString() ?? "" );

        if ( !Directory.Exists( directory ) )
        {
            Directory.CreateDirectory( directory );
        }

        var cleanUpFilePath = Path.Combine( directory, "cleanup.json" );

        if ( !File.Exists( cleanUpFilePath ) )
        {
            var file = new CleanUpFile() { Strategy = cleanUpStrategy };
            File.WriteAllText( cleanUpFilePath, JsonConvert.SerializeObject( file ) );
        }
        else if ( cleanUpStrategy == CleanUpStrategy.WhenUnused && File.GetLastAccessTime( cleanUpFilePath ) > DateTime.Now.AddDays( -1 ) )
        {
            File.SetLastWriteTime( cleanUpFilePath, DateTime.Now );
        }

        return directory;
    }
}