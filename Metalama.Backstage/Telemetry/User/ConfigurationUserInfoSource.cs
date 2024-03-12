// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Backstage.Telemetry.User;

internal class ConfigurationUserInfoSource : UserInfoSource
{
    private readonly IFileSystem _fileSystem;
    
    private readonly IConfigurationManager _configurationManager;
    
    public ConfigurationUserInfoSource( IServiceProvider serviceProvider )
    {
        this._fileSystem = serviceProvider.GetRequiredBackstageService<IFileSystem>();
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
    }

    public override bool TryGetUserInfo( [NotNullWhen( true )] out UserInfo? userInfo )
    {
        if ( !this._fileSystem.FileExists( this._configurationManager.GetFilePath<UserInfo>() ) )
        {
            userInfo = null;

            return false;
        }
        else
        {
            userInfo = this._configurationManager.Get<UserInfo>();

            return true;
        }
    }

    public void SaveEmailAddress( string emailAddress ) => this._configurationManager.Update<UserInfo>( i => i with { EmailAddress = emailAddress } );
}