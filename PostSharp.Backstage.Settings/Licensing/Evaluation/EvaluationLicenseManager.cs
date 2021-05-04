// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Backstage.Licensing.Evaluation
{
    public class EvaluationLicenseManager
    {
        private readonly IServiceProvider _services;
        private readonly IDateTimeProvider _time;
        private readonly ITrace _trace;

        public EvaluationLicenseManager( IServiceProvider services, ITrace trace )
        {
            this._services = services;
            this._time = services.GetService<IDateTimeProvider>();
            this._trace = trace;
        }

        public bool TryRegisterEvaluationLicense()
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

                // TODO: How many days?
                if ( data.ValidTo < this._time.Now + TimeSpan.FromDays( 180 ) )
                {
                    TraceFailure( "Evaluation requested recently." );
                    return false;
                }
                else
                {
                    this._trace.WriteLine( "Evaluation license registration can be repeated." );
                    return true;
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
                var factory = new SelfSignedLicenseFactory( this._time );
                (licenseKey, data) = factory.CreateEvaluationLicense();

                var userStorage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.UserLicenseFile, this._services, this._trace );

                if ( userStorage.Licenses.Values.Any( l => l != null && l.LicenseType == LicenseType.Evaluation && l.ValidTo >= data.ValidFrom ) )
                {
                    // This may happen when concurrent processes try to register an evaluation license at the same time.
                    TraceFailure( "A valid evaluation license is registered already." );
                    return false;
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
                return false;
            }

            return true;
        }
    }
}
