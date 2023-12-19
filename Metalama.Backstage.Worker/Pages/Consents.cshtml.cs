// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Pages.Shared;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.UserInterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Metalama.Backstage.Pages;

#pragma warning disable SA1649

public class ConsentsPageModel : PageModel
{
    private readonly ILicenseRegistrationService _licenseRegistrationService;
    private readonly ITelemetryConfigurationService _telemetryConfigurationService;
    private readonly IIdeExtensionStatusService _ideExtensionStatusService;
    private readonly IToastNotificationStatusService _toastNotificationStatusService;

    public ConsentsPageModel(
        ILicenseRegistrationService licenseRegistrationService,
        ITelemetryConfigurationService telemetryConfigurationService,
        IIdeExtensionStatusService ideExtensionStatusService,
        IToastNotificationStatusService toastNotificationStatusService )
    {
        this._licenseRegistrationService = licenseRegistrationService;
        this._telemetryConfigurationService = telemetryConfigurationService;
        this._ideExtensionStatusService = ideExtensionStatusService;
        this._toastNotificationStatusService = toastNotificationStatusService;
    }

    public List<string> ErrorMessages { get; } = new();

    public string RecaptchaSiteKey { get; set; } = "abcd";

    public string? RecaptchaResponse { get; set; }

    [BindProperty]
    public bool EnableTelemetry { get; set; }

    [BindProperty]
    [EmailAddress]
    public string EmailAddress { get; set; } = "test@example.com";

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
        switch ( GlobalState.LicenseKind )
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

            case LicenseKind.Skip:
                {
                    this._toastNotificationStatusService.Mute( ToastNotificationKinds.RequiresLicense );

                    break;
                }

            default:
                {
                    if ( GlobalState.LicenseKey == null )
                    {
                        this.ErrorMessages.Add( "No license key was provided." );

                        return this.Page();
                    }

                    if ( !this._licenseRegistrationService.TryRegisterLicense( GlobalState.LicenseKey, out var errorMessage ) )
                    {
                        this.ErrorMessages.Add( errorMessage );

                        return this.Page();
                    }

                    break;
                }
        }

        // Configure telemetry.
        this._telemetryConfigurationService.SetStatus( this.EnableTelemetry );

        // Should we recommend to install Visual Studio
        if ( this._ideExtensionStatusService.ShouldRecommendToInstallVisualStudioExtension )
        {
            return this.Redirect( "/InstallVsx" );
        }

        return this.Redirect( "/Okay" );
    }
}