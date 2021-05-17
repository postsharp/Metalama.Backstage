// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Backstage.Licensing.Evaluation
{
    /// <summary>
    /// Registers an evaluation (trial) license.
    /// </summary>
    public class EvaluationLicenseRegistrar : IFirstRunLicenseActivator
    {
        /// <summary>
        /// Gets the time span of the evaluation license validity.
        /// </summary>
        internal static TimeSpan EvaluationPeriod { get; } = TimeSpan.FromDays( 45 );

        // TODO (Do not register a license - allow prerelease to build without a license for 30 days?)
        internal static TimeSpan PrereleaseEvaluationPeriod { get; } = TimeSpan.FromDays( 30 );

        /// <summary>
        /// Gets the time span from the end of an evaluation license validity
        /// in which a new evaluation license cannot be registered.
        /// </summary>
        internal static TimeSpan NoEvaluationPeriod { get; } = TimeSpan.FromDays( 120 );

        private readonly IServiceProvider _services;
        private readonly IDateTimeProvider _time;
        private readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationLicenseRegistrar"/> class.
        /// </summary>
        /// <param name="services">Services.</param>
        public EvaluationLicenseRegistrar( IServiceProvider services )
        {
            this._services = services;
            this._time = services.GetRequiredService<IDateTimeProvider>();
            this._logger = services.GetOptionalTraceLogger<EvaluationLicenseRegistrar>();
        }

        /// <summary>
        /// Attempts to register an evaluation license.
        /// </summary>
        /// <returns>
        /// A value indicating whether the license has been registered.
        /// Success is indicated when a new community license is registered
        /// as well as when an existing community license is registered already.
        /// </returns>
        public bool TryRegisterLicense()
        {
            if ( !this.IsEvaluationEligible() )
            {
                return false;
            }

            if ( !this.TryRegisterEvaluationLicenseImpl() )
            {
                this._logger?.LogTrace(
                    "Evaluation license registration finished with errors which might be caused by concurrent evaluation license registration. " +
                    "If a concurrent evaluation license registration has succeeded, it will be used now." );
            }

            return true;
        }

        private bool IsEvaluationEligible()
        {
            void TraceFailure( string message )
            {
                this._logger?.LogTrace( $"Failed to find the latest trial license: {message}" );
            }

            this._logger?.LogTrace( "Checking for trial license eligibility." );

            try
            {
                var evaluationStorage = LicenseFileStorage.OpenOrCreate( StandardEvaluationLicenseFilesLocations.EvaluationLicenseFile, this._services );

                if ( evaluationStorage.Licenses.Count == 0 )
                {
                    this._logger?.LogTrace( "No trial license found." );
                    return true;
                }

                if ( evaluationStorage.Licenses.Count > 1 )
                {
                    TraceFailure( "Invalid count." );
                    return false;
                }

                var data = evaluationStorage.Licenses.Values.Single();

                if ( data == null )
                {
                    TraceFailure( "Invalid data." );
                    return false;
                }

                if ( data.LicenseType != LicenseType.Evaluation )
                {
                    TraceFailure( "Invalid license type." );
                    return false;
                }

                if ( data.ValidTo == null )
                {
                    TraceFailure( "Invalid validity." );
                    return false;
                }

                if ( data.ValidTo + NoEvaluationPeriod < this._time.Now )
                {
                    this._logger?.LogTrace( "Evaluation license registration can be repeated." );
                    return true;
                }
                else
                {
                    this._logger?.LogTrace( "Evaluation license requested recently." );
                    return false;
                }
            }
            catch ( Exception e )
            {
                // We don't want to disclose the evaluation license file path here.
                TraceFailure( $"{e.GetType()}" );
                return false;
            }
        }

        private bool TryRegisterEvaluationLicenseImpl()
        {
            void TraceFailure( string message )
            {
                this._logger?.LogTrace( $"Failed to register evaluation license: {message}" );
            }

            this._logger?.LogTrace( "Registering evaluation license." );

            string licenseKey;
            LicenseRegistrationData data;

            try
            {
                var factory = new UnsignedLicenseFactory( this._services );
                (licenseKey, data) = factory.CreateEvaluationLicense();

                var retryCount = 0;

                while ( true )
                {
                    try
                    {
                        var userStorage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.UserLicenseFile, this._services );

                        if ( userStorage.Licenses.Values.Any( l => l != null && l.LicenseType == LicenseType.Evaluation && l.ValidTo >= data.ValidFrom ) )
                        {
                            // This may happen when concurrent processes try to register an evaluation license at the same time.
                            TraceFailure( "A valid evaluation license is registered already." );

                            // We failed to register the license, but there is a valid license already.
                            return true;
                        }

                        userStorage.AddLicense( licenseKey, data );
                        userStorage.Save();

                        break;
                    }
                    catch ( IOException e )
                    {
                        if ( ++retryCount < 10 )
                        {
                            TraceFailure( $"Attempt #1: {e.Message} Retrying." );
                            Thread.Sleep( 500 );
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            catch ( Exception e )
            {
                TraceFailure( e.ToString() );
                return false;
            }

            // Prevent repetitive evaluation license registration.
            try
            {
                // We overwrite existing storage.
                var evaluationStorage = LicenseFileStorage.Create( StandardEvaluationLicenseFilesLocations.EvaluationLicenseFile, this._services );
                evaluationStorage.AddLicense( licenseKey, data );
                evaluationStorage.Save();
            }
            catch ( Exception e )
            {
                // We don't want to disclose the evaluation license file path here.
                this._logger?.LogTrace( $"Failed to store evaluation license information: {e.GetType()}" );

                // We failed to prevent repetitive evaluation license registration, but the license has been registered already.
                return true;
            }

            return true;
        }
    }
}
