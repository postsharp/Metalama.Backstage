// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.UserInterface;
using System;

namespace Metalama.Backstage.Welcome;

public sealed class WelcomeService : IBackstageService
{
    private readonly IConfigurationManager _configurationManager;
    private readonly WebLinks _webLinks;

    internal WelcomeService( IServiceProvider serviceProvider )
    {
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._webLinks = serviceProvider.GetRequiredBackstageService<WebLinks>();
    }

    public string? GetWelcomePageUrlAndRemember()
    {
        if ( this._configurationManager.UpdateIf<WelcomeConfiguration>( c => !c.WelcomePageDisplayed, c => c with { WelcomePageDisplayed = true } ) )
        {
            return this._webLinks.AfterSetup;
        }
        else
        {
            return null;
        }
    }
}