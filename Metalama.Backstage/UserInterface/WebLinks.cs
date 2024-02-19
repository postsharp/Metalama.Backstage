// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage.UserInterface;

#pragma warning disable CA1822

public class WebLinks : IBackstageService
{
    private const string _notImplemented = "https://www.postsharp.net/error/not-implemented";

    public string AfterSetup => _notImplemented;

    public string GetTeamTeamTrial => _notImplemented;

    public string VisualStudioMarketplace => _notImplemented;

    public string PrivacyPolicy => _notImplemented;

    public string LicenseAgreement => _notImplemented;

    public string Documentation => _notImplemented;

    public string InstallVsx => _notImplemented;

    public string RenewSubscription => _notImplemented;

    public string UnhandledException => _notImplemented;

    public string DotNetTool => "https://doc.metalama.net/conceptual/installing/dotnet-tool";

    public string NewsletterGetCaptchaSiteKeyApi => "https://licensing.postsharp.net/GetCaptchaSiteKey.ashx";

    public string NewsletterSubscribeApi => "https://licensing.postsharp.net/MetalamaNewsletter.ashx";
}