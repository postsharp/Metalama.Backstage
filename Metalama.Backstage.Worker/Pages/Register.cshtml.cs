// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Worker.WebServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Metalama.Backstage.Pages;

#pragma warning disable SA1649

public class RegisterModel : PageModel
{
    private readonly ILicenseRegistrationService _licenseRegistrationService;
    private readonly ITelemetryConfigurationService _telemetryConfigurationService;

    public RegisterModel( ILicenseRegistrationService licenseRegistrationService, ITelemetryConfigurationService telemetryConfigurationService )
    {
        this._licenseRegistrationService = licenseRegistrationService;
        this._telemetryConfigurationService = telemetryConfigurationService;
    }

    public List<string> ErrorMessages { get; } = new();

    public string RecaptchaSiteKey { get; set; } = "abcd";

    public string? RecaptchaResponse { get; set; }

    [BindProperty]
    [EmailAddress]
    public string EmailAddress { get; set; } = "test@example.com";

    [BindProperty]
    public LicenseKind LicenseKind { get; set; } = LicenseKind.Trial;

    [BindProperty]
    public string? License { get; set; }

    [BindProperty]
    public bool EnableTelemetry { get; set; }

    [BindProperty]
    public bool SubscribeToNewsletter { get; set; }

    [BindProperty]
    public bool AcceptLicense { get; set; }

    public IActionResult OnPost()
    {
        if ( !this.ModelState.IsValid )
        {
            this.ErrorMessages.AddRange( this.ModelState.SelectMany( e => e.Value?.Errors ?? Enumerable.Empty<ModelError>() ).Select( e => e.ErrorMessage ) );

            return this.Page();
        }

        if ( !this.AcceptLicense )
        {
            this.ErrorMessages.Add( "You must accept the license agreement." );

            return this.Page();
        }

        // Register the license.
        switch ( this.LicenseKind )
        {
            case LicenseKind.Trial:
                {
                    if ( !this._licenseRegistrationService.TryRegisterTrialEdition( out var errorMessage ) )
                    {
                        this.ErrorMessages.Add( errorMessage );

                        return this.Page();
                    }

                    break;
                }

            case LicenseKind.Free:
                {
                    if ( !this._licenseRegistrationService.TryRegisterFreeEdition( out var errorMessage ) )
                    {
                        this.ErrorMessages.Add( errorMessage );

                        return this.Page();
                    }

                    break;
                }

            default:
                {
                    if ( string.IsNullOrEmpty( this.License ) )
                    {
                        this.ErrorMessages.Add( "The license key must be provided." );

                        return this.Page();
                    }

                    if ( !this._licenseRegistrationService.TryRegisterLicense( this.License, out var errorMessage ) )
                    {
                        this.ErrorMessages.Add( errorMessage );

                        return this.Page();
                    }

                    break;
                }
        }

        // Configure telemetry.
        this._telemetryConfigurationService.SetStatus( this.EnableTelemetry );

        return this.Redirect( "/Okay" );
    }
}