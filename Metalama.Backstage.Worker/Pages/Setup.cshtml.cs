// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metalama.Backstage.Pages;

#pragma warning disable SA1649

public class SetupPageModel : PageModel
{
    private readonly ILicenseRegistrationService _licenseRegistrationService;
    private readonly ITelemetryConfigurationService _telemetryConfigurationService;

    public SetupPageModel( ILicenseRegistrationService licenseRegistrationService, ITelemetryConfigurationService telemetryConfigurationService )
    {
        this._licenseRegistrationService = licenseRegistrationService;
        this._telemetryConfigurationService = telemetryConfigurationService;
    }

    public IActionResult OnGet()
    {
        return this.Redirect( "/ChooseLicenseKind" );
    }
}