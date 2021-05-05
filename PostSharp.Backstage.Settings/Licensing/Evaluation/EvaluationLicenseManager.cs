﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Backstage.Licensing.Evaluation
{
    public class EvaluationLicenseManager : ILicenseAutoRegistrar
    {
        // TODO: Correct?
        internal static TimeSpan EvaluationPeriod { get; } = TimeSpan.FromDays( 45 );

        // TODO: Correct?
        internal static TimeSpan NoEvaluationPeriod { get; } = TimeSpan.FromDays( 180 );

        private readonly IServiceProvider _services;
        private readonly IDateTimeProvider _time;
        private readonly ITrace _trace;

        public EvaluationLicenseManager( IServiceProvider services, ITrace trace )
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
                var evaluationStorage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.EvaluationLicenseFile, this._services, this._trace );

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
                TraceFailure( $"{e.GetType()}{Environment.NewLine}{e.StackTrace}" );
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
            }
            catch (Exception e)
            {
                TraceFailure( e.ToString() );
                return false;
            }

            try
            {
                // We overwrite existing storage.
                var evaluationStorage = LicenseFileStorage.Create( StandardLicenseFilesLocations.EvaluationLicenseFile, this._services, this._trace );
                evaluationStorage.AddLicense( licenseKey, data );
                evaluationStorage.Save();
            }
            catch (Exception e)
            {
                // We don't want to disclose the evaluation license file path here.
                TraceFailure( $"{e.GetType()}{Environment.NewLine}{e.StackTrace}" );
                
                // We failed to prevent repetitive evaluation license registration, but the license has been registered already.
                return true;
            }

            return true;
        }
    }
}
