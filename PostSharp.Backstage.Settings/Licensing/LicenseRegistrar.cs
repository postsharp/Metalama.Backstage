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
    public class LicenseRegistrar
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

        public DateTime PrereleaseEvaluationEndDate => this._applicationInfoService.BuildDate + prereleaseEvaluationPeriodDuration;

        public LicenseRegistrar( UserSettings userSettings, IApplicationInfoService applicationInfoService )
        {
            this._userSettings = userSettings;
            this._applicationInfoService = applicationInfoService;
        }

        internal bool TryParseWellKnownLicenseString( string licenseString, out License license, out string sourceDescription )
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
        public bool TryOpenEvaluationMode( bool dry, out License license )
        {
            license = null;

            EvaluationPeriodStatus evaluationPeriodStatus = this.GetEvaluationPeriodStatus();
            if ( evaluationPeriodStatus == EvaluationPeriodStatus.Forbidden )
                return false;

            using ( IRegistryKey userRegistryKey = UserSettings.OpenRegistryKey( writable: true ) )
            {
                if ( !dry )
                {
                    if ( evaluationPeriodStatus == EvaluationPeriodStatus.New )
                    {
                        DateTime evaluationStartDate = UserSettings.GetCurrentDateTime().Date;

                        userRegistryKey.SetValueDateTime( evaluationStartDateValueName, evaluationStartDate );
                    }

                    license = CreateEvaluationLicense();
                    RegisterLicense( license.Serialize(), false );
                }

                return true;
            }
        }

        internal static CoreLicense CreateUnattendedLicense()
        {
            return new CoreUnattendedLicense();
        }

        internal static CoreLicense CreateUnmodifiedLicense()
        {
            return new CoreUnmodifiedLicense();
        }

        internal static IEnumerable<License> CreatePerUsageCountingLicenses()
        {
            yield return new CorePerUsageCountingLicense( 10, LicensedProduct.Ultimate, LicensedPackages.All & ~LicensedPackages.Diagnostics );
            yield return new CorePerUsageCountingLicense( 11, LicensedProduct.DiagnosticsLibrary, LicensedPackages.Diagnostics );
        }

        public static void CloseEvaluationMode()
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

        internal static void AddLicense(string licenseString, LicenseSource source)
        {
            LocalUserLicenseManager.Instance.CurrentSharedManager.AddUnusedLicenseConfiguration(
                new LicenseConfiguration( licenseString, License.Deserialize( licenseString ), source, source.ToString() ) );
        }

        /// <exclude/>
        internal static bool RegisterLicense( string newLicenseString, bool allUsers )
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
            CloseEvaluationMode();

            LicenseConfiguration newLicense = ParseLicenseString( newLicenseString, LicenseSource.Programmatic, "License Registration" );

            // Don't register a license key that cannot be parsed.
            if ( newLicense == null )
            {
                return false;
            }

            if ( !newLicense.IsLicenseServerUrl )
            {
                // Don't register a license key with an invalid signature.
                if ( !newLicense.License.Validate( null, out string errorMessage ) )
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
            
            if ( allUsers )
            {
                // If we register for all users, we should remove the key for the current user
                // because it takes precedence.
                UnregisterLicense( newLicenseString, false );
            }
            
            Version minPostSharpVersion = null;

            string licenseStringToStoreInStandardLocation = newLicenseString;
            
            if ( newLicense.License != null && newLicense.License.RequiresVersionSpecificStore &&
                 licenseStringToStoreInStandardLocation == newLicenseString )
            {
                minPostSharpVersion = newLicense.License.MinPostSharpVersion;
            }

            IRegistryKey registryKey = LicensingRegistryHelper.GetLicenseRegistryKey( allUsers, true, minPostSharpVersion );

            using ( registryKey )
            {
                // Find available name.
                int registryValueName;
                for ( registryValueName = 0;
                      registryKey.GetValue( registryValueName.ToString( CultureInfo.InvariantCulture ) ) != null;
                      registryValueName++ )
                {
                }

                registryKey.SetValue( registryValueName.ToString( CultureInfo.InvariantCulture ), licenseStringToStoreInStandardLocation );
            }

            if ( UserSettings.UsageReportingAction == ReportingAction.Yes )
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

        internal static bool UnregisterLicense( string license, bool allUsers, bool standardLocationOnly = false )
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
            
            bool hasChange = false;
            
            if ( !standardLocationOnly )
            {
                // Remove license key from the legacy (3.x) location.
                IRegistryKey legacyRegistryKey = UserSettings.OpenRegistryKey( allUsers, true );

                if ( legacyRegistryKey != null )
                {
                    using ( legacyRegistryKey )
                    {
                        if ( legacyRegistryKey.GetValue( "LicenseKey" ) is string value && value.Trim() == license )
                        {
                            hasChange = true;
                            legacyRegistryKey.DeleteValue( "LicenseKey", false );
                        }
                    }
                }
            }

            // Remove license key from standard (>=4.0) locations.
            IRegistryKey registryKey = LicensingRegistryHelper.GetLicenseRegistryKey( allUsers );

            if ( registryKey == null )
            {
                return hasChange;
            }

            using ( registryKey )
            {
                // Remove license key from PostSharp version agnostic location
                if ( DeleteLicenseFromStandardLocation( license, registryKey ) )
                {
                    hasChange = true;
                }

                // Remove license key from PostSharp version dependent location
                string[] versionDependentKeyNames = registryKey.GetSubKeyNames();

                foreach ( string versionDependentKeyName in versionDependentKeyNames )
                {
                    IRegistryKey versionDependentKey = registryKey.OpenSubKey( versionDependentKeyName, true );

                    if ( versionDependentKey == null )
                    {
                        continue;
                    }

                    using ( versionDependentKey )
                    {
                        if ( DeleteLicenseFromStandardLocation( license, versionDependentKey ) )
                        {
                            hasChange = true;
                        }
                    }
                }
            }

            if ( hasChange )
            {
                if ( LocalUserLicenseManager.IsInitialized )
                {
                    LocalUserLicenseManager.Instance.NotifyLicenseChange();
                }
            }

            return hasChange;
        }

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

        internal static LicenseConfiguration ParseLicenseString( string rawLicenseString, LicenseSource source, string sourceDescription )
        {
            if ( rawLicenseString == null )
            {
                return null;
            }

            if ( LicenseServerClient.IsLicenseServerUrl( rawLicenseString ) )
            {
                return new LicenseConfiguration( rawLicenseString, null, source, sourceDescription );
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

            if ( TryParseWellKnownLicenseString( rawLicenseString, out License license, out string internalSourceDescription ) )
            {
                // Well-known license string.
                licenseString = rawLicenseString;
                source = LicenseSource.Internal;
                sourceDescription = internalSourceDescription;
            }
            else
            {
                license = License.Deserialize( rawLicenseString );
                licenseString = license == null ? null : license.LicenseString;
            }

            if ( license == null )
            {
                return null;
            }

            return new LicenseConfiguration( licenseString, license, source, sourceDescription );
        }

        internal static void ReportRegisteredLicense( ReportedLicense reportedLicense )
        {
            if ( !Metrics.LicenseRegistration.Enabled )
            {
                Metrics.LicenseRegistration.Initialize();
            }

            Metrics.LicenseRegistration.ReportedLicensedProduct.Value = reportedLicense.LicensedProduct;
            Metrics.LicenseRegistration.ReportedLicenseType.Value = reportedLicense.LicenseType;
            Metrics.LicenseRegistration.Flush( true );
        }

        /// <exclude/>
        public static long GetCurrentMachineHash()
        {
            return
                CryptoUtilities.ComputeStringHash64(
                    UserSettings.GetRegistryValue( true, @"SOFTWARE\Microsoft\Cryptography", "MachineGuid", null ) as string ?? Environment.MachineName );
        }

        internal static long GetCurrentUserHash()
        {
            return CryptoUtilities.ComputeStringHash64( Environment.UserName );
        }

        /// <exclude/>
        public static bool ClosePostSharpProcesses()
        {
            Process[] processes = Process.GetProcesses();
            bool allProcessedClosed = true;
            foreach ( Process process in processes )
            {
                if ( Regex.IsMatch( process.ProcessName, "^postsharp-.*-srv$", RegexOptions.IgnoreCase ) )
                {
                    try
                    {
                        if ( !process.CloseMainWindow() )
                            process.Kill();
                    }
                    catch ( Exception )
                    {
                        allProcessedClosed = false;
                    }
                }
            }


            return allProcessedClosed;
        }

        internal static void ResetFreeTypesCounters()
        {
            LicensingRegistryHelper.DeleteProcessedTypesCountersKey();
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
        
        internal EvaluationPeriodStatus GetEvaluationPeriodStatus()
        {
            IRegistryKey userRegistryKey = UserSettings.OpenRegistryKey();

            if ( userRegistryKey == null )
                return EvaluationPeriodStatus.New;

            using ( userRegistryKey )
            {
                DateTime? evaluationStartDate = userRegistryKey.GetValueDateTime( evaluationStartDateValueName );

                if ( evaluationStartDate == null )
                    return EvaluationPeriodStatus.New;

                if ( evaluationStartDate < this._userSettings.GetCurrentDateTime().Date.Subtract( evaluationWaitingPeriodDuration ) )
                    return EvaluationPeriodStatus.New;

                if ( evaluationStartDate > this._userSettings.GetCurrentDateTime().Date.Subtract( evaluationPeriodDuration ) || this._applicationInfoService.IsPrerelease )
                    return EvaluationPeriodStatus.Continue;
            }

            return EvaluationPeriodStatus.Forbidden;
        }

        internal CoreLicense CreateEvaluationLicense( )
        {
            using ( IRegistryKey userRegistryKey = UserSettings.OpenRegistryKey( writable: true ) )
            {
                DateTime? evaluationStartDateFromRegistry = userRegistryKey.GetValueDateTime( evaluationStartDateValueName );

                if ( !evaluationStartDateFromRegistry.HasValue )
                {
                    return null;
                }

                DateTime evaluationStartDate = evaluationStartDateFromRegistry.Value;
                DateTime evaluationEndDate = this._applicationInfoService.IsPrerelease ? PrereleaseEvaluationEndDate : evaluationStartDate + evaluationPeriodDuration;

                CoreLicense license = new CoreLicense(LicensedProduct.Ultimate)
                                      {
                                          LicenseGuid = Guid.NewGuid(),
                                          LicenseType = LicenseType.Evaluation,
                                          ValidFrom = evaluationStartDate,
                                          ValidTo = evaluationEndDate,
                                          UserNumber = 1
                                      };

                return license;

            }

        }
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
