// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Settings;
using PostSharp.Backstage.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace PostSharp.Backstage.Licensing
{
    public abstract class LicenseRegistrar
    {
        internal const string UnattendedLicenseString = "(Unattended)";
        internal const string UnmodifiedLicenseString = "(Unmodified)";

        private const string expiredTestingEvaluationLicenseString = "(ExpiredTestingEvaluation)";
        private const string eternalTestingEvaluationLicenseString = "(EternalTestingEvaluation)";
        
        private const string evaluationStartDateValueName = "Evaluation";

        private static readonly TimeSpan evaluationPeriodDuration = TimeSpan.FromDays( 45 );
        private static readonly TimeSpan prereleaseEvaluationPeriodDuration = TimeSpan.FromDays( 30 );
        private static readonly TimeSpan evaluationWaitingPeriodDuration = TimeSpan.FromDays( 120 );
        
        private readonly UserSettings _userSettings;
        private readonly IApplicationInfoService _applicationInfoService;
        private readonly ITrace _licensingTrace;

        public DateTime PrereleaseEvaluationEndDate => this._applicationInfoService.BuildDate + prereleaseEvaluationPeriodDuration;

        public LicenseRegistrar( UserSettings userSettings, IApplicationInfoService applicationInfoService, ITrace licensingTrace )
        {
            this._userSettings = userSettings;
            this._applicationInfoService = applicationInfoService;
            this._licensingTrace = licensingTrace;
        }

        internal bool TryParseWellKnownLicenseString( string licenseString, out License? license, out string? sourceDescription )
        {
            if ( licenseString == expiredTestingEvaluationLicenseString )
            {
                license = CreateTestEvaluationLicense( false );
                sourceDescription = "Expired Evaluation Test";
                return true;
            }

            if ( licenseString == eternalTestingEvaluationLicenseString )
            {
                license = CreateTestEvaluationLicense( true );
                sourceDescription = "Eternal Evaluation Test";
                return true;
            }

            if ( licenseString == UnattendedLicenseString )
            {
                license = CreateUnattendedLicense();
                sourceDescription = "Unattended Build Test";
                return true;
            }

            if ( licenseString == UnmodifiedLicenseString )
            {
                license = CreateUnmodifiedLicense();
                sourceDescription = "Unmodified Project Test";
                return true;
            }

            license = null;
            sourceDescription = null;
            return false;
        }

        internal CoreLicense CreateTestEvaluationLicense( bool isValid )
        {
            DateTime evaluationStartDate;
            DateTime evaluationEndDate;


            if ( isValid )
            {
                // For testing only
                evaluationStartDate = DateTime.Now.Date;
                evaluationEndDate = evaluationStartDate + evaluationPeriodDuration;
            }
            else
            {
                evaluationEndDate = DateTime.Now.AddDays( -1 );
                evaluationStartDate = evaluationEndDate - evaluationPeriodDuration;
            }

            CoreLicense testEvaluationLicense = new CoreLicense( LicensedProduct.Ultimate, this._applicationInfoService.Version, this._applicationInfoService.BuildDate )
                                                {
                                                    LicenseGuid = Guid.NewGuid(),
                                                    LicenseType = LicenseType.Evaluation,
                                                    Licensee = "Test",
                                                    ValidFrom = evaluationStartDate,
                                                    ValidTo = evaluationEndDate,
                                                    UserNumber = 1
                                                };
            testEvaluationLicense.Serialize();
            return testEvaluationLicense;
        }

        // Used only for testing now.
        internal bool TryOpenEvaluationMode( bool allUsers = false )
        {
            return RegisterLicense( this.CreateTestEvaluationLicense( true ).LicenseString, allUsers );
        }

        /// <summary>
        /// Creates a new license key for a 45-day evaluation license and registers it in the HKCU hive, unless another
        /// trial on this user recently expired.
        /// </summary>
        /// <param name="dry">Dry run mode.</param>
        /// <param name="license">The created license.</param>
        /// <returns>True if the license was created and registered; false if the user's trial already expired and the
        /// user is forbidden from creating a new trial license.</returns>
        public abstract bool TryOpenEvaluationMode( bool dry, out License license );

        internal CoreLicense CreateUnattendedLicense()
        {
            return new CoreUnattendedLicense( this._applicationInfoService.Version, this._applicationInfoService.BuildDate );
        }

        internal CoreLicense CreateUnmodifiedLicense()
        {
            return new CoreUnmodifiedLicense( this._applicationInfoService.Version, this._applicationInfoService.BuildDate  );
        }

        internal IEnumerable<License> CreatePerUsageCountingLicenses()
        {
            yield return new CorePerUsageCountingLicense( 10, LicensedProduct.Ultimate, LicensedPackages.All & ~LicensedPackages.Diagnostics, this._applicationInfoService.Version, this._applicationInfoService.BuildDate );
            yield return new CorePerUsageCountingLicense( 11, LicensedProduct.DiagnosticsLibrary, LicensedPackages.Diagnostics, this._applicationInfoService.Version, this._applicationInfoService.BuildDate );
        }

        public void CloseEvaluationMode()
        {
            using (SharedUserLicenseManager sharedUserLicenseManager = new SharedUserLicenseManager())
            {
                sharedUserLicenseManager.LoadUserLicenses();

                // Unregister all evaluation keys.
                foreach (LicenseConfiguration license in sharedUserLicenseManager.UserLicenses)
                {
                    if (!license.IsLicenseServerUrl && license.License is CoreLicense coreLicense &&
                         coreLicense.LicenseType == LicenseType.Evaluation && 
                         (license.Source == LicenseSource.CurrentUserRegistry || license.Source == LicenseSource.AllUsersRegistry))
                    {
                        UnregisterLicense(license.LicenseString, license.Source == LicenseSource.AllUsersRegistry);
                    }
                }
            }
        }

        internal void AddLicense(string licenseString, LicenseSource source)
        {
            _ = License.TryDeserialize( licenseString, this._applicationInfoService, out var license, this._licensingTrace );

            LocalUserLicenseManager.Instance.CurrentSharedManager.AddUnusedLicenseConfiguration(
                new LicenseConfiguration( licenseString, license, source, source.ToString() ) );
        }

        /// <exclude/>
        internal bool RegisterLicense( string newLicenseString, bool allUsers )
        {
            if ( LocalUserLicenseManager.IsInitialized )
            {
                LocalUserLicenseManager.Instance.ClearLeaseCache();
            }

            if ( newLicenseString == null )
            {
                return false;
            }

            newLicenseString = newLicenseString.Trim();

            // Terminate the evaluation period before registering any license.
            this.CloseEvaluationMode();

            if ( !this.TryParseLicenseString( newLicenseString, LicenseSource.Programmatic, "License Registration", out var newLicense ) )
            {
                return false;
            }

            if ( !newLicense!.IsLicenseServerUrl )
            {
                // Don't register a license key with an invalid signature.
                if ( !newLicense.License!.Validate( null, out string errorMessage ) )
                {
                    Trace.TraceInformation( $"License not registered: {errorMessage}" );
                    return false;
                }

                if ( newLicense.License.LicenseType == LicenseType.PerUsage )
                {
                    Trace.TraceInformation( $"License not registered: Cannot install a per-usage license. Per-usage licenses must be placed in postsharp.config." );
                    return false;
                }
            }

            this.RegisterLicenseImpl( newLicenseString, allUsers, newLicense );

            if ( this._userSettings.UsageReportingAction == ReportingAction.Yes )
            {
                ReportedLicense reportedLicense = newLicense.GetReportedLicense();

                ReportRegisteredLicense( reportedLicense );
            }



            if ( LocalUserLicenseManager.IsInitialized )
            {
                LocalUserLicenseManager.Instance.NotifyLicenseChange();
            }

            return true;
        }

        protected abstract void RegisterLicenseImpl( string newLicenseString, bool allUsers, LicenseConfiguration? newLicense );

        internal bool UnregisterLicense( string license, bool allUsers )
        {
            if ( LocalUserLicenseManager.IsInitialized )
            {
                LocalUserLicenseManager.Instance.ClearLeaseCache();
            }

            if ( license == null )
            {
                return false;
            }

            license = license.Trim();

            var hasChange = this.UnregisterLicenseImpl( license, allUsers );

            if ( hasChange )
            {
                if ( LocalUserLicenseManager.IsInitialized )
                {
                    LocalUserLicenseManager.Instance.NotifyLicenseChange();
                }
            }

            return hasChange;
        }

        protected abstract bool UnregisterLicenseImpl( string license, bool allUsers );

        private static bool DeleteLicenseFromStandardLocation( string license, IRegistryKey registryKey )
        {
            bool hasChange = false;

            foreach ( string valueName in registryKey.GetValueNames() )
            {
                string value = registryKey.GetValue( valueName ) as string;

                if ( value == null )
                {
                    continue;
                }

                value = value.Trim();

                if ( string.Equals( value, license, StringComparison.OrdinalIgnoreCase ) )
                {
                    hasChange = true;
                    registryKey.DeleteValue( valueName, false );
                }
            }

            return hasChange;
        }

        internal bool TryParseLicenseString( string rawLicenseString, LicenseSource source, string sourceDescription, out LicenseConfiguration? licenseConfiguration )
        {
            if ( rawLicenseString == null )
            {
                licenseConfiguration = null;
                return false;
            }

            if ( LicenseServerClient.IsLicenseServerUrl( rawLicenseString ) )
            {
                licenseConfiguration = new LicenseConfiguration( rawLicenseString, null, source, sourceDescription );
                return true;
            }

            string licenseString;

            // License source can be overridden for testing as $<source>$<license>
            if ( rawLicenseString.StartsWith( "$", StringComparison.Ordinal ) )
            {
                string[] testLicenseStringParts = rawLicenseString.Split( '$' );

                if ( testLicenseStringParts.Length == 3 && Enum.TryParse( testLicenseStringParts[1], out source ) )
                {
                    sourceDescription = string.Format( CultureInfo.InvariantCulture, "{0}-Test", source );
                    rawLicenseString = testLicenseStringParts[2];
                }
            }


            if ( this.TryParseWellKnownLicenseString( rawLicenseString, out var license, out var internalSourceDescription ) )
            {
                // Well-known license string.
                licenseString = rawLicenseString;
                source = LicenseSource.Internal;
                sourceDescription = internalSourceDescription!;
            }
            else if ( License.TryDeserialize( rawLicenseString, this._applicationInfoService, out license, this._licensingTrace ) )
            {
                licenseString = license!.LicenseString;
            }
            else
            {
                licenseConfiguration = null;
                return false;
            }

            licenseConfiguration = new( licenseString, license, source, sourceDescription );
            return true;
        }

        internal static void ReportRegisteredLicense( ReportedLicense reportedLicense )
        {
            // TODO: Metrics
        }

        /// <exclude/>
        public abstract long GetCurrentMachineHash();

        internal long GetCurrentUserHash()
        {
            return CryptoUtilities.ComputeStringHash64( Environment.UserName );
        }

        public bool NotifyLicenseRegistration( License license, bool dry, out string message, out bool isError )
        {
            try
            {
                ReportedLicense reportedLicense = license.GetReportedLicense();
                string url =
                    string.Format( CultureInfo.InvariantCulture,
                                   "https://licensing.postsharp.net/LicenseMetering.ashx?license={0}&licenseHash={1:x}&user={2:x}&machine={3:x}&version={4}&licensedProduct={5}&licenseType={6}&source=license-registration",
                                   license.LicenseUniqueId,
                                   CryptoUtilities.ComputeStringHash64( license.Serialize() ),
                                   GetCurrentUserHash(),
                                   GetCurrentMachineHash(),
                                   this._applicationInfoService.Version,
                                   reportedLicense.LicensedProduct,
                                   reportedLicense.LicenseType );
                if ( dry )
                    url += "&dry=1";

                using ( CustomWebClient webClient = new CustomWebClient( TimeSpan.FromSeconds( 10 ) ) )
                {
                    string result = webClient.DownloadString( url );

                    if ( !string.IsNullOrEmpty( result ) && !string.Equals( result.Trim(), "ok", StringComparison.OrdinalIgnoreCase) )
                    {
                        if ( result.StartsWith( "warning:", StringComparison.OrdinalIgnoreCase ) )
                        {
                            isError = false;
                            message = result.Substring( "warning:".Length ).Trim();
                            return false;
                        }
                        else if ( result.StartsWith( "error:", StringComparison.OrdinalIgnoreCase ) )
                        {
                            isError = true;
                            message = result.Substring( "error:".Length ).Trim();
                            return false;
                        }
                    }
                }
            }
            catch ( Exception e )
            {
                Trace.TraceError(e.ToString());
                // We ignore exceptions.
            }

            message = null;
            isError = false;
            return true;
        }

        internal abstract EvaluationPeriodStatus GetEvaluationPeriodStatus();

        internal bool TryCreateEvaluationLicense( out CoreLicense? license )
        {
            if ( !this.TryGetEvaluationStartDate( out var evaluationStartDate ) )
            {
                license = null;
                return false;
            }

            var evaluationEndDate = this._applicationInfoService.IsPrerelease ? this.PrereleaseEvaluationEndDate : evaluationStartDate!.Value + evaluationPeriodDuration;

            license = new( LicensedProduct.Ultimate, this._applicationInfoService.Version, this._applicationInfoService.BuildDate )
            {
                LicenseGuid = Guid.NewGuid(),
                LicenseType = LicenseType.Evaluation,
                ValidFrom = evaluationStartDate,
                ValidTo = evaluationEndDate,
                UserNumber = 1,
            };

            return true;
        }

        protected abstract bool TryGetEvaluationStartDate( out DateTime? evaluationStartDate );

        private class CustomWebClient : WebClient
        {
            private readonly TimeSpan timeout;

            public CustomWebClient( TimeSpan timeout )
            {
                this.timeout = timeout;
            }

            protected override WebRequest GetWebRequest( Uri uri )
            {
                HttpWebRequest w = (HttpWebRequest) base.GetWebRequest( uri );
                w.Timeout = (int) this.timeout.TotalMilliseconds;
                w.ReadWriteTimeout = (int) this.timeout.TotalMilliseconds;
                return w;
            }
        }
    }
}
