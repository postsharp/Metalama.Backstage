// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Tests.ConfigurationManager;

internal class TestConfigurationManager : IConfigurationManager
{
    private readonly Dictionary<Type, ConfigurationFile> _files = new();

    public TestConfigurationManager( IServiceProvider serviceProvider, params ConfigurationFile[] files )
    {
        this.Logger = serviceProvider.GetLoggerFactory().GetLogger( "TestConfigurationManager" );

        foreach ( var file in files )
        {
            this.Set( file );
        }
    }

    public void Dispose() { }

    public ILogger Logger { get; }

    public string GetFileName( Type type ) => throw new NotSupportedException();

    public void Set( ConfigurationFile file ) => this._files[file.GetType()] = file;

    public ConfigurationFile Get( Type type, bool ignoreCache = false )
    {
        if ( !this._files.TryGetValue( type, out var file ) )
        {
            file = (ConfigurationFile) Activator.CreateInstance( type )!;
        }

        return file;
    }

    public bool TryUpdate( ConfigurationFile value, DateTime? lastModified )
    {
        var oldFile = this.Get( value.GetType() );

        if ( oldFile.LastModified != lastModified )
        {
            return false;
        }

        value.LastModified = DateTime.Now;
        this._files[value.GetType()] = value;

        return true;
    }
}