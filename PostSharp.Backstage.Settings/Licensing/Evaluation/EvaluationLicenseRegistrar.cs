// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Backstage.Licensing.Evaluation
{
    public class EvaluationLicenseRegistrar : ILicenseAutoRegistrar
    {
        internal static TimeSpan EvaluationPeriod { get; } = TimeSpan.FromDays( 45 );

        // TODO (Do not register a license - allow prerelease to build without a license for 30 days?)
        internal static TimeSpan PrereleaseEvaluationPeriod { get; } = TimeSpan.FromDays( 30 );

        internal static TimeSpan NoEvaluationPeriod { get; } = TimeSpan.FromDays( 120 );

        private readonly IServiceProvider _services;
        private readonly IDateTimeProvider _time;
        private readonly ITrace _trace;

        public EvaluationLicenseRegistrar( IServiceProvider services, ITrace trace )
        {
            this._services = services;
            this._time = services.GetService<IDateTimeProvider>();
            this._trace = trace;
        }

        public bool TryRegisterLicense()
        {
            if ( !this.IsEvaluationEligible() )
            {
                return false;
            }

            if ( !this.TryRegisterEvaluationLicenseImpl() )
            {
                this._trace.WriteLine(
                    "Evaluation license registration finished with errors which might be caused by concurrent evaluation license registration. " +
                    "If a concurrent evaluation license registration has succeeded, it will be used now." );
            }

            return true;
        }

        private bool IsEvaluationEligible()
        {
            void TraceFailure( string message )
            {
                this._trace.WriteLine( "Failed to find the latest trial license: {0}", message );
            }

            this._trace.WriteLine( "Checking for trial license eligibility." );

            try
            {
                var evaluationStorage = LicenseFileStorage.OpenOrCreate( StandardEvaluationLicenseFilesLocations.EvaluationLicenseFile, this._services, this._trace );

                if ( evaluationStorage.Licenses.Count == 0 )
                {
                    this._trace.WriteLine( "No trial license found." );
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
                    this._trace.WriteLine( "Evaluation license registration can be repeated." );
                    return true;
                }
                else
                {
                    this._trace.WriteLine( "Evaluation license requested recently." );
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
                this._trace.WriteLine( "Failed to register evaluation license: {0}", message );
            }

            this._trace.WriteLine( "Registering evaluation license." );

            string licenseKey;
            LicenseRegistrationData data;

            try
            {
                var factory = new SelfSignedLicenseFactory( this._services );
                (licenseKey, data) = factory.CreateEvaluationLicense();

                var retryCount = 0;

                while ( true )
                {
                    try
                    {
                        var userStorage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.UserLicenseFile, this._services, this._trace );

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
            catch (Exception e)
            {
                TraceFailure( e.ToString() );
                return false;
            }

            // Prevent repetitive evaluation license registration.
            try
            {
                // We overwrite existing storage.
                var evaluationStorage = LicenseFileStorage.Create( StandardEvaluationLicenseFilesLocations.EvaluationLicenseFile, this._services, this._trace );
                evaluationStorage.AddLicense( licenseKey, data );
                evaluationStorage.Save();
            }
            catch (Exception e)
            {
                // We don't want to disclose the evaluation license file path here.
                this._trace.WriteLine( "Failed to store evaluation license information: {0}", e.GetType() );
                
                // We failed to prevent repetitive evaluation license registration, but the license has been registered already.
                return true;
            }

            return true;
        }
    }
}
