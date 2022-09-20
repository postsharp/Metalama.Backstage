// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Utilities;
using System;

namespace Metalama.Backstage.Licensing.Registration.Evaluation;

/// <summary>
/// Registers an evaluation (trial) license.
/// </summary>
public class EvaluationLicenseRegistrar
{
    /// <summary>
    /// Gets the time span of the evaluation license validity.
    /// </summary>
    internal static TimeSpan EvaluationPeriod { get; } = TimeSpan.FromDays( 45 );

    // TODO: How to license pre-release versions?
    internal static TimeSpan PrereleaseEvaluationPeriod { get; } = TimeSpan.FromDays( 30 );

    /// <summary>
    /// Gets the time span from the end of an evaluation license validity
    /// in which a new evaluation license cannot be registered.
    /// </summary>
    internal static TimeSpan NoEvaluationPeriod { get; } = TimeSpan.FromDays( 120 );

    private readonly IServiceProvider _services;
    private readonly IDateTimeProvider _time;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluationLicenseRegistrar"/> class.
    /// </summary>
    /// <param name="services">Services.</param>
    public EvaluationLicenseRegistrar( IServiceProvider services )
    {
        this._services = services;
        this._time = services.GetRequiredBackstageService<IDateTimeProvider>();
        this._logger = services.GetLoggerFactory().Licensing();
    }

    /// <summary>
    /// Attempts to register an evaluation license.
    /// </summary>
    /// <returns>
    /// A value indicating whether the license has been registered.
    /// </returns>
    public bool TryActivateLicense()
    {
        this._logger.Trace?.Log( "Attempting to register an evaluation license." );

        using ( MutexHelper.WithGlobalLock( "Evaluation" ) )
        {
            var configuration = ParsedLicensingConfiguration.OpenOrCreate( this._services );

            // If the configuration file contains an evaluation license created today, this may be a race condition with
            // another process also trying to activate the evaluation mode. In this case, we just pretend we have succeeded.
            if ( configuration.LicenseData is { LicenseType: LicenseType.Evaluation } &&
                 configuration.LicenseData?.ValidFrom == this._time.Now.Date )
            {
                this._logger.Trace?.Log( "Another process started the evaluation period today." );

                return true;
            }

            // If the configuration file contains any license, we won't register the evaluation license.
            if ( configuration.LicenseData != null &&
                 (configuration.LicenseData.ValidTo == null || configuration.LicenseData.ValidTo.Value >= this._time.Now) )
            {
                this._logger.Warning?.Log( "You cannot start the evaluation mode because another license key is registered in the user profile." );

                return false;
            }

            if ( configuration.LastEvaluationStartDate != null )
            {
                var nextEvaluationMinStartDate =
                    configuration.LastEvaluationStartDate.Value + NoEvaluationPeriod + EvaluationPeriod;

                if ( nextEvaluationMinStartDate >= this._time.Now )
                {
                    this._logger.Warning?.Log( $"You cannot start the evaluation mode until {nextEvaluationMinStartDate}." );

                    return false;
                }
            }

            var factory = new UnsignedLicenseFactory( this._services );
            var (licenseKey, data) = factory.CreateEvaluationLicense();

            configuration.SetLicense( licenseKey, data );
            configuration.LastEvaluationStartDate = this._time.Now;
        }

        return true;
    }
}