// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Threading;
using System.Globalization;
using PostSharp.Backstage.Utilities;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Settings;
using PostSharp.Backstage.Licensing.Helpers;

namespace PostSharp.Backstage.Licensing
{
    public class SharedUserLicenseManager : MarshalByRefObject, IDisposable
    {
        private protected readonly object Sync = new object();

        private EventHandler<LicenseEventArgs> licenseMessageEmittedClients;
        private EventHandler licenseRequiredClients;
        private readonly LicenseFeatureContainer userLicensedFeatures = new LicenseFeatureContainer();
        private readonly LicenseFeatureContainer namespaceLimitedUserLicensedFeatures = new LicenseFeatureContainer();
        private RegistryChangeMonitor currentUserRegistryChangeMonitor;
        private readonly HashSet<LicenseConfiguration> unusedLicenseServers = new HashSet<LicenseConfiguration>();
        private readonly HashSet<LicenseConfiguration> unusedUncachedLicenseServers = new HashSet<LicenseConfiguration>();
        private readonly HashSet<LicenseConfiguration> unusedLicenseKeys = new HashSet<LicenseConfiguration>();
        private readonly HashSet<LicenseConfiguration> unusedUncachedLicenseKeys = new HashSet<LicenseConfiguration>();
        private readonly HashSet<LicenseConfiguration> unusedPerUsageLicenseKeys = new HashSet<LicenseConfiguration>();
        private readonly HashSet<LicenseConfiguration> unusedUncachedPerUsageLicenseKeys = new HashSet<LicenseConfiguration>();
        private readonly List<LicenseConfiguration> userLicenses = new List<LicenseConfiguration>();
        private readonly HashSet<LicenseConfiguration> usedNamespaceLimitedLicenseKeys = new HashSet<LicenseConfiguration>();
        private bool addUnattendedLicenseCalled;
        private bool licenseLookedForInRegistry;


        internal bool EvaluationLicenseExpired { get; private set; }

        public SharedUserLicenseManager()
        {
            this.ClearLicenses();

            this.licenseLookedForInRegistry = false;
            this.UserLicenses = new ReadOnlyCollection<LicenseConfiguration>( this.userLicenses );
        }

        private void AddUnattendedLicense()
        {
            throw new NotImplementedException();

            //LicensingTrace.Licensing?.WriteLine( "Checking for unattended build." );

            //if ( !ProcessUtilities.IsCurrentProcessUnattended )
            //{
            //    LicensingTrace.Licensing?.WriteLine( "The build is not unattended." );
            //}
            //else
            //{
            //    LicensingTrace.Licensing?.WriteLine( "The build is unattended. Adding license for unattended build." );
            //    License license = LicenseRegistrationHelper.CreateUnattendedLicense();
            //    this.AddUnusedLicenseConfiguration(
            //        new LicenseConfiguration( LicenseRegistrationHelper.UnattendedLicenseString, license, LicenseSource.Internal, "Unattended Build" ) );
            //}

            //this.addUnattendedLicenseCalled = true;
        }
        

        private void ClearLicenses()
        {
            LicensingTrace.Licensing?.WriteLine( "Clearing licensed features." );
            this.userLicensedFeatures.Clear();
            this.namespaceLimitedUserLicensedFeatures.Clear();
            this.userLicenses.Clear();
            this.unusedLicenseKeys.Clear();
            this.unusedUncachedLicenseKeys.Clear();
            this.usedNamespaceLimitedLicenseKeys.Clear();
            this.unusedLicenseServers.Clear();
            this.unusedUncachedLicenseServers.Clear();
        }

        internal bool TrySatisfyRequirement( 
            IDiagnosticsSink diagnosticsSink, 
            LicensedPackages requirement, 
            string ns, 
            bool perUsage,
            bool inherited, 
            bool cachedLicensesOnly,
            LicenseSource allowedLicenseSources,
            out IReadOnlyLicenseFeatureContainer returnedUserLicensedFeatures,
            out IReadOnlyLicenseFeatureContainer returnedNamespaceLimitedUserLicensedFeatures )
        {
            LicensingTrace.Licensing?.WriteLine( "SharedUserLicenseManager: trying to satisfy the requirement '{0}'.",
                                                 requirement );

            lock ( this.Sync )
            {
                // Namespace specific license features cannot be shared between AppDomains.
                this.namespaceLimitedUserLicensedFeatures.Clear();

                bool usedNamespaceLimitedLicenseKeysReused = false;
                bool satisfied = false;

                while ( true )
                {
                    if ( this.userLicensedFeatures.SatisfiesRequirement( requirement, perUsage, inherited )
                         || this.namespaceLimitedUserLicensedFeatures.SatisfiesRequirement( requirement, perUsage, inherited ) )
                    {
                        if (  this.IsRequirementSatisfiedFromAnySource( requirement, ns, allowedLicenseSources ) )
                        {
                            satisfied = true;
                            break;
                        }
                    }

                    if ( !usedNamespaceLimitedLicenseKeysReused )
                    {
                        if ( this.usedNamespaceLimitedLicenseKeys.Count > 0 )
                        {
                            foreach ( LicenseConfiguration license in this.usedNamespaceLimitedLicenseKeys )
                            {
                                this.unusedLicenseKeys.Add( license );
                            }

                            this.usedNamespaceLimitedLicenseKeys.Clear();
                        }

                        usedNamespaceLimitedLicenseKeysReused = true;
                    }

                    if ( !this.addUnattendedLicenseCalled )
                    {
                        this.AddUnattendedLicense();
                        continue;
                    }
                    else if ( this.TryLoadNextCachedOrInternalLicenseKey( ns, diagnosticsSink, perUsage ) )
                    {
                        continue;
                    }
                    else if ( cachedLicensesOnly )
                    {
                        break;
                    }
                    else if ( this.TryLoadLowestUncachedLicenseKey( ns, diagnosticsSink, perUsage ) )
                    {
                        continue;
                    }
                    else if ( this.TryGetLease( diagnosticsSink ) )
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                if ( this.userLicensedFeatures.IsEmpty )
                {
                    returnedUserLicensedFeatures = null;
                }
                else
                {
                    returnedUserLicensedFeatures = this.userLicensedFeatures;
                }

                if ( this.namespaceLimitedUserLicensedFeatures.IsEmpty )
                {
                    returnedNamespaceLimitedUserLicensedFeatures = null;
                }
                else
                {
                    returnedNamespaceLimitedUserLicensedFeatures = this.namespaceLimitedUserLicensedFeatures;
                }

                LicensingTrace.Licensing?.WriteLine( "SharedUserLicenseManager: Requirement {0} satisfied = {1}.", requirement, satisfied );


                return satisfied;
            }
        }

        private bool IsRequirementSatisfiedFromAnySource( LicensedPackages requirement, string ns, LicenseSource allowedLicenseSources )
        {
            if ( allowedLicenseSources == LicenseSource.All )
                return true;

            foreach ( LicenseConfiguration licenseConfiguration in this.userLicenses )
            {
                if ( (licenseConfiguration.Source & allowedLicenseSources) != 0
                     && (licenseConfiguration.License.GetLicensedPackages() & requirement) != 0)
                {
                    if ( licenseConfiguration.License.AllowsNamespace( ns ) )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryGetLease( IDiagnosticsSink diagnosticsSink, bool cachedOnly = false )
        {
            while ( true )
            {
                if ( this.unusedLicenseServers.Count > 0 )
                {
                    LicenseConfiguration licenseConfiguration = this.unusedLicenseServers.PopFirst();
                    if ( this.TryGetLease( diagnosticsSink, licenseConfiguration, true ) )
                    {
                        return true;
                    }
                }
                else if ( !cachedOnly && this.unusedUncachedLicenseServers.Count > 0 )
                {
                    LicenseConfiguration licenseConfiguration = this.unusedUncachedLicenseServers.PopFirst();
                    if ( this.TryGetLease( diagnosticsSink, licenseConfiguration, false ) )
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }



        public bool TryGetLease( IDiagnosticsSink diagnosticsSink, LicenseConfiguration licenseConfiguration, bool cached )
        {
            using ( IRegistryKey registryKey = LicensingRegistryHelper.GetLeasedLicenseRegistryKey( licenseConfiguration ) )
            {
                try
                {
                    LicenseLease lease;
                    if ( cached )
                    {
                        LicensingTrace.Licensing?.WriteLine( "Trying to get a cached license lease from \"{0}\".", licenseConfiguration );
                        lease = this.TryGetCachedLease( diagnosticsSink, licenseConfiguration, registryKey, UserSettings.GetCurrentDateTime() );
                    }
                    else
                    {
                        LicensingTrace.Licensing?.WriteLine( "Trying to download a license lease from \"{0}\".", licenseConfiguration );
                        lease = LicenseServerClient.TryDownloadLease( diagnosticsSink, licenseConfiguration.LicenseString, registryKey );
                    }

                    if ( lease == null )
                    {
                        LicensingTrace.Licensing?.WriteLine( "No license {0} license lease obtained from \"{1}\".",
                                                            cached ? "cached" : "non-cached",
                                                            licenseConfiguration );
                        return false;
                    }

                    License license = License.Deserialize( lease.LicenseString );
                    if ( license == null )
                    {
                        LicensingTrace.Licensing?.WriteLine( "Failed to deserialize a {0} license lease from \"{1}\".",
                                                            cached ? "cached" : "non-cached",
                                                            licenseConfiguration );
                        return false;
                    }

                    if ( !license.IsLicenseServerEligible() )
                    {
                        LicensingTrace.Licensing?.WriteLine( "A {0} license lease of license id ({1}) obtained from \"{2}\" is not license server eligible.",
                                                            cached ? "cached" : "non-cached",
                                                            license.LicenseId,
                                                            licenseConfiguration );

                        if ( diagnosticsSink != null )
                        {
                            diagnosticsSink.ReportWarning( $"Cannot use the license {license.LicenseString} from a license server: invalid license type." ); // PS0149
                        }

                        return false;
                    }

                    licenseConfiguration.LicenseLease = lease;
                    licenseConfiguration.License = license;

                    LicensingTrace.Licensing?.WriteLine( "Adding a {0} license lease of license id ({1}) obtained from \"{2}\".",
                                                        cached ? "cached" : "non-cached",
                                                        license.LicenseId,
                                                        licenseConfiguration );

                    this.unusedLicenseKeys.Add( licenseConfiguration );

                    return true;
                }
                catch ( SystemException e )
                {
                    LicensingTrace.Licensing?.WriteLine( "Failed to access the registry while working with a {0} license lease obtained from \"{1}\": {2}",
                                                        cached ? "cached" : "non-cached",
                                                        licenseConfiguration,
                                                        e );

                    if ( diagnosticsSink != null )
                    {
                        diagnosticsSink.ReportError( "Cannot get a lease from the license server: Error accessing the registry." ); // PS0148
                    }

                    return false;
                }
            }
        }

        private LicenseLease TryGetCachedLease( IDiagnosticsSink diagnosticsSink, LicenseConfiguration licenseConfiguration, IRegistryKey registryKey, DateTime now )
        {
            LicenseLease lease = LicenseServerClient.TryGetLease( licenseConfiguration.LicenseString, registryKey, now, diagnosticsSink );

            if ( lease == null )
            {
                this.unusedUncachedLicenseServers.Add( licenseConfiguration );
            }

            return lease;
        }

     

        public void LoadUserLicenses()
        {
            this.LoadUserLicenses( null, LicenseSource.Programmatic, null );
        }

        public void LoadUserLicenses( string[] licenseStrings, LicenseSource source, string sourceDescription, IDiagnosticsSink diagnosticsSink = null )
        {
            diagnosticsSink = diagnosticsSink ?? NullDiagnosticsSink.Instance;

            // Note that tracing is always done for the current Messenger irrespective of 'diagnosticsSink'.
            lock ( this.Sync )
            {
                bool loadFromRegistry = true;


                // Load given license strings.
                if ( licenseStrings != null )
                {
                    foreach ( string rawLicenseString in licenseStrings )
                    {

                        LicensingTrace.Licensing?.WriteLine( "SharedUserLicenseManager: loading license string {{{0}}} from {1} ({2}).", rawLicenseString, sourceDescription, source );

                        string licenseString = License.CleanLicenseString( rawLicenseString );
                        loadFromRegistry = false;
                        LicenseConfiguration licenseConfiguration;

                        // We allow to add the same license string from different sources,
                        // to be able to check the source of license keys later.
                        if (
                            this.userLicenses.Any(
                                configuration => string.Equals( configuration.LicenseString, licenseString, StringComparison.OrdinalIgnoreCase )
                                                 && configuration.Source == source ) )
                        {
                            LicensingTrace.Licensing?.WriteLine( "License {{{0}}} from {1} ({2}) has been added already.", licenseString, sourceDescription,source );
                            continue;
                        }


                        // Test some diagnostic "magic" licenses.
                        switch ( licenseString.ToLowerInvariant() )
                        {
                            case "none":
                            case "clear":
                                this.ClearLicenses();
                                LicensingTrace.Licensing?.WriteLine( "Licenses explicitly cleared." );
                                continue;
                            default:
                                // NOTE (bug 6239) we pass rawLicenseString for parsing in case the string contains the license server URL
                                licenseConfiguration = LicenseRegistrar.ParseLicenseString( rawLicenseString, source, sourceDescription );
                                if ( licenseConfiguration == null )
                                {
                                    LicensingTrace.Licensing?.WriteLine( "Failed to parse license string \"{0}\" from {1} ({2}).", rawLicenseString, sourceDescription, source );
                                    diagnosticsSink.ReportWarning( $"Cannot parse license key string '{rawLicenseString}'." ); // PS0301
                                    continue;
                                }

                                break;
                        }

                        this.AddUnusedLicenseConfiguration( licenseConfiguration );
                    }
                }

                // Load from registry.
                if ( loadFromRegistry && !this.licenseLookedForInRegistry )
                {
                    this.licenseLookedForInRegistry = true;

                    this.LoadLicensesFromRegistry();
                }
            }
        }

        protected internal void AddUnusedLicenseConfiguration( LicenseConfiguration licenseConfiguration )
        {
            if ( licenseConfiguration.IsLicenseServerUrl )
            {
                LicensingTrace.Licensing?.WriteLine( "For further consideration, adding license server {0}.", licenseConfiguration );
                this.unusedLicenseServers.Add( licenseConfiguration );
            }
            else
            {
                LicensingTrace.Licensing?.WriteLine( "For further consideration, adding license {0}.", licenseConfiguration );
                if ( licenseConfiguration.License.LicenseType == LicenseType.PerUsage )
                    this.unusedPerUsageLicenseKeys.Add( licenseConfiguration );
                else
                    this.unusedLicenseKeys.Add( licenseConfiguration );
            }

            this.userLicenses.Add( licenseConfiguration );
        }

        private bool TryLoadNextCachedOrInternalLicenseKey( string ns, IDiagnosticsSink diagnosticsSink, bool perUsage )
        {
            HashSet<LicenseConfiguration> unusedKeys = perUsage ? this.unusedPerUsageLicenseKeys : this.unusedLicenseKeys;
            while ( unusedKeys.Count > 0 )
            {
                LicenseConfiguration licenseConfiguration = unusedKeys.PopFirst();
                if ( this.TryLoadLicenseKey( diagnosticsSink, licenseConfiguration, ns, true ) )
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryLoadLowestUncachedLicenseKey( string ns, IDiagnosticsSink diagnosticsSink, bool perUsage )
        {
            HashSet<LicenseConfiguration> unusedUncachedKeys = perUsage ? this.unusedUncachedPerUsageLicenseKeys : this.unusedUncachedLicenseKeys;

            while ( unusedUncachedKeys.Count > 0 )
            {
                LicenseConfiguration lowestUncachedLicenseKey = null;

                foreach ( LicenseConfiguration key in unusedUncachedKeys )
                {
                    if ( lowestUncachedLicenseKey == null ||
                         ( lowestUncachedLicenseKey.License.LicenseType).IsBetterThan( key.License.LicenseType ) )
                    {
                        lowestUncachedLicenseKey = key;
                        continue;
                    }
                }

                unusedUncachedKeys.Remove( lowestUncachedLicenseKey );

                if ( this.TryLoadLicenseKey( diagnosticsSink, lowestUncachedLicenseKey, ns, false ) )
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryLoadLicenseKey( IDiagnosticsSink diagnosticsSink, LicenseConfiguration licenseConfiguration, string ns, bool cached )
        {
            LicensingTrace.Licensing?.WriteLine( "Trying to load {0} license \"{1}\".", cached ? "cached" : "non-cached", licenseConfiguration );

            License license = licenseConfiguration.License;

            if ( license == null )
            {
                throw new InvalidOperationException( string.Format( CultureInfo.InvariantCulture, "License not set for \"{0}\".", licenseConfiguration ) );
            }

            if ( license.LicenseType == LicenseType.PerUsage && licenseConfiguration.Source != LicenseSource.Configuration )
            {
                diagnosticsSink.ReportError( $"Per-usage licenses can only be used in configuration files. License '{license.LicenseId}' not loaded." ); // PS0303
                return false;
            }

            List<string> errors = new List<string>();
            bool licenseIsValid;
            if ( licenseConfiguration.Source != LicenseSource.Internal )
            {
                License[] licenses = {license};
                string hash = LicenseSetData.GetHash( license.LicenseString );
                LicenseSetData licenseSetData;

                if ( cached )
                {
                    licenseSetData = LicenseSetData.GetCachedLicenseSetDataFromHash( licenses, hash );
                    if ( licenseSetData == null )
                    {
                        LicensingTrace.Licensing?.WriteLine( "License \"{0}\" is not cached.", licenseConfiguration );
                        if ( license.LicenseType == LicenseType.PerUsage )
                            this.unusedUncachedPerUsageLicenseKeys.Add( licenseConfiguration );
                        else
                            this.unusedUncachedLicenseKeys.Add( licenseConfiguration );
                        return false;
                    }
                }
                else
                {
                    licenseSetData = new LicenseSetData( licenses, null, hash );
                }

                licenseIsValid = licenseSetData.Verify( null, null, errors );
            }
            else
            {
                // We don't verify internally-generated license keys because they
                // are trusted and they are not signed.
                string errorDescription;
                licenseIsValid = license.Validate( null, out errorDescription );
                if ( !licenseIsValid )
                {
                    errors.Add( errorDescription );
                }
            }

            if ( !licenseIsValid )
            {
                if ( license.IsEvaluationLicense() )
                {
                    this.EvaluationLicenseExpired = true;
                }

                LicensingTrace.Licensing?.WriteLine( "The license {0} from {1} is invalid: {2}",
                                                    license.LicenseUniqueId,
                                                    licenseConfiguration.SourceDescription,
                                                    string.Join( " ", errors ) );

                this.userLicenses.Remove( licenseConfiguration );

                return false;
            }

            if ( (licenseConfiguration.License.GetAllowedLicenseSources() & licenseConfiguration.Source) != licenseConfiguration.Source )
            {
                if ( diagnosticsSink != null )
                {
                    diagnosticsSink.ReportError( $"License error. The license {license.LicenseUniqueId} is not allowed to be loaded from {licenseConfiguration.SourceDescription}." ); // PS0260
                }

                LicensingTrace.Licensing?.WriteLine( "The license key {0} is not valid because the source is not valid: expected {0}, but got {1}.", licenseConfiguration,
                                                    licenseConfiguration.License.GetAllowedLicenseSources(), licenseConfiguration.Source );

                this.userLicenses.Remove( licenseConfiguration );
                return false;
            }


            if ( license.IsLimitedByNamespace )
            {
                this.usedNamespaceLimitedLicenseKeys.Add( licenseConfiguration );

                if ( license.AllowsNamespace( ns ) )
                {
                    LicensingTrace.Licensing?.WriteLine( "Adding features for license \"{0}\" limited by namespace \"{1}\".",
                                                        licenseConfiguration, license.Namespace );

                    this.namespaceLimitedUserLicensedFeatures.Add( license );
                }
                else
                {
                    LicensingTrace.Licensing?.WriteLine( "License \"{0}\" is not allowed to be used in namespace \"{1}\".",
                                                        licenseConfiguration, ns );
                    return false;
                }
            }
            else
            {
                LicensingTrace.Licensing?.WriteLine( "Adding features for license \"{0}\".", licenseConfiguration );
                this.userLicensedFeatures.Add( license );
            }

            this.OnUserLicenseLoaded( license );

            LicensingTrace.Licensing?.WriteLine( "Loaded {0} license \"{1}\".",
                                                cached ? "cached" : "non-cached",
                                                licenseConfiguration );

            return true;
        }

        private static int RankLicense( License license )
        {
            if ( license == null )
            {
                // License server unknown.
                return 15;
            }

            switch ( license.Product )
            {
                case LicensedProduct.ModelLibrary:
                    return 9;
                case LicensedProduct.ThreadingLibrary:
                    return 10;
                case LicensedProduct.DiagnosticsLibrary:
                    return 11;
                case LicensedProduct.CachingLibrary:
                    return 12;
                default:
                {
#pragma warning disable 618
                    switch ( license.LicenseType )
                    {
                        case LicenseType.Unattended:
                            return 1;
                        case LicenseType.Unmodified:
                            return 2;
                        case LicenseType.Enterprise:
                            return 3;
                        case LicenseType.PerUser:
                            return 4;
                        case LicenseType.PerUsage:
                            return 5;
                        case LicenseType.Site:
                            return 6;
                        case LicenseType.Global:
                            return 7;
                        case LicenseType.Academic:
                            return 8;
                        case LicenseType.Community:
                            return 13;
                        case LicenseType.Evaluation:
                            return 14;
                        case LicenseType.OpenSourceRedistribution:
                            return 16;
                        case LicenseType.CommercialRedistribution:
                            return 17;
                        default:
                            return int.MaxValue;
#pragma warning restore 618
                    }
                }
            }
        }

        public ReportedLicense GetReportedLicense()
        {
            if ( this.userLicenses.Count == 0 )
            {
                return new ReportedLicense("None", "None");
            }
            else
            {
                LicenseConfiguration bestLicenseConfiguration = this.userLicenses.OrderBy( configuration => RankLicense(configuration.License) ).First();
                return bestLicenseConfiguration.GetReportedLicense();
            }
        }

        internal void StartMonitorRegistry()
        {
            // Ensure that the current user key has been created, so that we can monitor it.
            string keyName;
            using ( IRegistryKey registryKey = UserSettings.OpenRegistryKey( false, true ) )
            {
                if ( !registryKey.CanMonitorChanges )
                {
                    return;
                }
                keyName = registryKey.ToString();
            }

            this.currentUserRegistryChangeMonitor = new RegistryChangeMonitor( RegistryChangeMonitor.HKEY_CURRENT_USER, keyName, false );
            this.currentUserRegistryChangeMonitor.Changed += this.OnRegistryChanged;
        }

        private DateTime registryTimestamp = DateTime.MinValue;

        public event EventHandler Changed;

        internal void OnRegistryChanged( DateTime timestamp )
        {
            lock ( this.Sync )
            {
                if ( this.registryTimestamp >= timestamp )
                    return;

                // If we don't sleep here, we may fail to read our new value from registry.
                Thread.Sleep( 1 );

                this.registryTimestamp = timestamp;
                this.LoadLicensesFromRegistry();

                if ( this.Changed != null )
                {
                    this.Changed( this, EventArgs.Empty );
                }
            }
        }

        private void OnRegistryChanged( object sender, EventArgs e )
        {
            DateTime timestamp = LicensingRegistryHelper.GetRegistryTimestamp();
            this.OnRegistryChanged( timestamp );
        }

        private void LoadLicensesFromRegistry()
        {
            // The new set of licenses will contain all licenses which are currently in registry and all others which have been loaded before.
            List<LicenseConfiguration> licenses = new List<LicenseConfiguration>( LicensingRegistryHelper.GetUserLicenseKeysFromRegistry() );
            licenses.AddRange( this.userLicenses.Where( l => l.Source != LicenseSource.AllUsersRegistry && l.Source != LicenseSource.CurrentUserRegistry ) );

            // We don't know which features come from which license,
            // so we need to clear the "cache".
            this.userLicensedFeatures.Clear();
            this.userLicenses.Clear();
            this.unusedLicenseKeys.Clear();
            this.unusedPerUsageLicenseKeys.Clear();
            this.unusedUncachedLicenseKeys.Clear();
            this.unusedUncachedPerUsageLicenseKeys.Clear();
            this.unusedLicenseServers.Clear();
            this.unusedUncachedLicenseServers.Clear();

            foreach ( LicenseConfiguration license in licenses )
            {
                this.AddUnusedLicenseConfiguration( license );
            }
        }

        public void OnLicenseRequired()
        {
            EventHandler handler = this.licenseRequiredClients;
            if ( handler != null )
                handler( this, EventArgs.Empty );
        }

        [Obfuscation( Exclude = true )]
        internal event EventHandler<LicenseEventArgs> LicenseMessageEmitted
        {
            [Obfuscation( Exclude = true )]
            add { this.licenseMessageEmittedClients += value; }

            [Obfuscation( Exclude = true )]
            remove { this.licenseMessageEmittedClients -= value; }
        }

        [Obfuscation( Exclude = true )]
        internal event EventHandler LicenseRequired
        {
            [Obfuscation( Exclude = true )]
            add { this.licenseRequiredClients += value; }

            [Obfuscation( Exclude = true )]
            remove { this.licenseRequiredClients -= value; }
        }

        private void OnUserLicenseLoaded( License license )
        {
            this.EmitLicenseMessage( this._LicenseMessageSource.GetMessage( license ) );
        }

        internal void EmitLicenseMessage( LicenseMessage licenseMessage )
        {

            if ( licenseMessage == null )
            {
                return;
            }

            LicensingTrace.Licensing?.WriteLine( "Emitting message. Severity {0}; Frequency: {1}; Kind: {2} Message: {3}",
                                                licenseMessage.IsError ? "Error" : "Warning",
                                                licenseMessage.Frequency,
                                                licenseMessage.Kind,
                                                licenseMessage.Text );

            if ( this.licenseMessageEmittedClients != null )
            {
                LicenseEventArgs args = new LicenseEventArgs( licenseMessage );
                foreach ( EventHandler<LicenseEventArgs> invocation in this.licenseMessageEmittedClients.GetInvocationList() )
                {
                    try
                    {
                        invocation( this, args );
                    }
                    catch
                    {
                        this.licenseMessageEmittedClients -= invocation;
                    }
                }
            }
        }

        /// <exclude />
        public IList<LicenseConfiguration> UserLicenses { get; }

        protected virtual void Dispose(bool disposing)
        {
            this.currentUserRegistryChangeMonitor?.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }
    }
}
