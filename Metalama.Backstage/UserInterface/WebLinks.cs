// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage.UserInterface;

#pragma warning disable CA1822

public class WebLinks : IBackstageService
{
    private const string _notImplemented = "https://www.postsharp.net/error/not-implemented";
    
    public string AfterSetup( bool wasPageDisplayed ) => GetLink( "metalama-after-activation", wasPageDisplayed ? "fresh=0" : null );

    public string GetTeamTeamTrial => GetLink( "metalama-team-evaluation" );

    public string VisualStudioMarketplace => GetLink( "metalama-download-vsx" );

    public string PrivacyPolicy => GetLink( "metalama-privacy-policy" );

    public string LicenseAgreement => GetLink( "metalama-license-agreement" );

    public string Documentation => GetLink( "metalama-documentation" );

    public string InstallVsx => this.VisualStudioMarketplace;

    public string RenewSubscription => GetLink( "metalama-renew-subscription" );

    public string UnhandledException => _notImplemented;

    public string DotNetTool => GetLink( "metalama-dotnet_tool" );

    public string NewsletterGetCaptchaSiteKeyApi => "https://licensing.postsharp.net/GetCaptchaSiteKey.ashx";

    public string NewsletterSubscribeApi => "https://licensing.postsharp.net/MetalamaNewsletter.ashx";

    private static string GetLink( string alias, string? queryString = null )
    {
        var url = $"https://www.postsharp.net/links/{alias}?mtm_campaign=setup&mtm_source=app";

        if ( !string.IsNullOrEmpty( queryString ) )
        {
            url += "&" + queryString;
        }

        return url;
    }
}