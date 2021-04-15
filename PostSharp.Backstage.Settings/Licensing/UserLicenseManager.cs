// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Settings;
using PostSharp.Backstage.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PostSharp.Backstage.Licensing
{
    public class UserLicenseManager
    {
        private readonly Dictionary<string, LicenseSetData> _cachedLicenses = new( StringComparer.OrdinalIgnoreCase );

        private readonly LicenseFeatureContainer userLicensedFeaturesLocal = new LicenseFeatureContainer();
        private readonly Dictionary<string, LicenseFeatureContainer> namespaceLimitedUserLicensedFeaturesLocal = new Dictionary<string, LicenseFeatureContainer>();
        private readonly Dictionary<LicensedPackages, bool> userLicenseRequirementsWithInheritanceCachedOnlyCache = new Dictionary<LicensedPackages, bool>();
        private readonly Dictionary<LicensedPackages, bool> userLicenseRequirementsWithoutInheritanceCachedOnlyCache = new Dictionary<LicensedPackages, bool>();
        private readonly Dictionary<LicensedPackages, bool> userLicenseRequirementsWithInheritanceCache = new Dictionary<LicensedPackages, bool>();
        private readonly Dictionary<LicensedPackages, bool> userLicenseRequirementsWithoutInheritanceCache = new Dictionary<LicensedPackages, bool>();
        private readonly Dictionary<LicensedPackages, bool> userLicenseRequirementsPerUsageWithInheritanceCachedOnlyCache = new Dictionary<LicensedPackages, bool>();
        private readonly Dictionary<LicensedPackages, bool> userLicenseRequirementsPerUsageWithoutInheritanceCachedOnlyCache = new Dictionary<LicensedPackages, bool>();
        private readonly Dictionary<LicensedPackages, bool> userLicenseRequirementsPerUsageWithInheritanceCache = new Dictionary<LicensedPackages, bool>();
        private readonly Dictionary<LicensedPackages, bool> userLicenseRequirementsPerUsageWithoutInheritanceCache = new Dictionary<LicensedPackages, bool>();

        private EventHandler<LicenseEventArgs> licenseMessageEmittedClients;
        private EventHandler licenseRequiredClients;
        private readonly LicenseFeatureContainer userLicensedFeatures = new LicenseFeatureContainer();
        private readonly LicenseFeatureContainer namespaceLimitedUserLicensedFeatures = new LicenseFeatureContainer();
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

        private readonly UserSettings _userSettings;
        private readonly IServiceLocator _serviceProvider;
        private readonly ITrace _licensingTrace;

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IApplicationInfoService _applicationInfoService;
        private readonly IDiagnosticsSink _diagnosticsSink;

        private readonly LicenseCache _licenseCache;
        private readonly LicenseRegistrar _licenseRegistrar;
        private readonly LicenseSourceMonitor _sourceMonitor;

        internal bool EvaluationLicenseExpired { get; private set; }

        public UserLicenseManager( UserSettings userSettings, IServiceLocator serviceProvider, ITrace licensingTrace )
        {
            this._userSettings = userSettings;
            this._serviceProvider = serviceProvider;
            this._licensingTrace = licensingTrace;

            this._dateTimeProvider = this._serviceProvider.GetService<IDateTimeProvider>();
            this._applicationInfoService = this._serviceProvider.GetService<IApplicationInfoService>();
            this._diagnosticsSink = this._serviceProvider.GetService<IDiagnosticsSink>();

            this._licenseCache = LicenseCacheFactory.Create( this._dateTimeProvider, this._licensingTrace );
            this._licenseRegistrar = LicenseRegistrarFactory.Create( this._userSettings, this._applicationInfoService, this._licensingTrace );
            this._sourceMonitor = LicenseSourceMonitorFactory.Create();
        }

        private void ClearLicenses()
        {
            this._licensingTrace?.WriteLine( "Clearing licensed features." );
            this.userLicensedFeatures.Clear();
            this.namespaceLimitedUserLicensedFeatures.Clear();
            this.userLicenses.Clear();
            this.unusedLicenseKeys.Clear();
            this.unusedUncachedLicenseKeys.Clear();
            this.usedNamespaceLimitedLicenseKeys.Clear();
            this.unusedLicenseServers.Clear();
            this.unusedUncachedLicenseServers.Clear();
        }

        protected void AddAuditedLicenses( IReadOnlyDictionary<string, string> auditedLicenses )
        {
            this.userLicensedFeaturesLocal.AddAuditedLicenses( auditedLicenses );
        }

        public IReadOnlyDictionary<string, string> AuditableLicenses => this.userLicensedFeaturesLocal.AuditedLicenses;

        private void AddUnattendedLicense()
        {
            throw new NotImplementedException();

            //this._licensingTrace?.WriteLine( "Checking for unattended build." );

            //if ( !ProcessUtilities.IsCurrentProcessUnattended )
            //{
            //    this._licensingTrace?.WriteLine( "The build is not unattended." );
            //}
            //else
            //{
            //    this._licensingTrace?.WriteLine( "The build is unattended. Adding license for unattended build." );
            //    License license = LicenseRegistrationHelper.CreateUnattendedLicense();
            //    this.AddUnusedLicenseConfiguration(
            //        new LicenseConfiguration( LicenseRegistrationHelper.UnattendedLicenseString, license, LicenseSource.Internal, "Unattended Build" ) );
            //}

            //this.addUnattendedLicenseCalled = true;
        }

        private bool TryGetLease( bool cachedOnly = false )
        {
            while ( true )
            {
                if ( this.unusedLicenseServers.Count > 0 )
                {
                    var licenseConfiguration = this.unusedLicenseServers.PopFirst();
                    if ( this.TryGetLease( licenseConfiguration, true ) )
                    {
                        return true;
                    }
                }
                else if ( !cachedOnly && this.unusedUncachedLicenseServers.Count > 0 )
                {
                    var licenseConfiguration = this.unusedUncachedLicenseServers.PopFirst();
                    if ( this.TryGetLease( licenseConfiguration, false ) )
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

        public bool TryGetLease( LicenseConfiguration licenseConfiguration, bool cached )
        {
            using ( IRegistryKey registryKey = LicensingRegistryHelper.GetLeasedLicenseRegistryKey( licenseConfiguration ) )
            {
                try
                {
                    LicenseLease lease;
                    if ( cached )
                    {
                        this._licensingTrace?.WriteLine( "Trying to get a cached license lease from \"{0}\".", licenseConfiguration );
                        lease = this.TryGetCachedLease( diagnosticsSink, licenseConfiguration, registryKey, UserSettings.GetCurrentDateTime() );
                    }
                    else
                    {
                        this._licensingTrace?.WriteLine( "Trying to download a license lease from \"{0}\".", licenseConfiguration );
                        lease = LicenseServerClient.TryDownloadLease( diagnosticsSink, licenseConfiguration.LicenseString, registryKey );
                    }

                    if ( lease == null )
                    {
                        this._licensingTrace?.WriteLine( "No license {0} license lease obtained from \"{1}\".",
                                                            cached ? "cached" : "non-cached",
                                                            licenseConfiguration );
                        return false;
                    }

                    License license = License.Deserialize( lease.LicenseString );
                    if ( license == null )
                    {
                        this._licensingTrace?.WriteLine( "Failed to deserialize a {0} license lease from \"{1}\".",
                                                            cached ? "cached" : "non-cached",
                                                            licenseConfiguration );
                        return false;
                    }

                    if ( !license.IsLicenseServerEligible() )
                    {
                        this._licensingTrace?.WriteLine( "A {0} license lease of license id ({1}) obtained from \"{2}\" is not license server eligible.",
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

                    this._licensingTrace?.WriteLine( "Adding a {0} license lease of license id ({1}) obtained from \"{2}\".",
                                                        cached ? "cached" : "non-cached",
                                                        license.LicenseId,
                                                        licenseConfiguration );

                    this.unusedLicenseKeys.Add( licenseConfiguration );

                    return true;
                }
                catch ( SystemException e )
                {
                    this._licensingTrace?.WriteLine( "Failed to access the registry while working with a {0} license lease obtained from \"{1}\": {2}",
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

        private bool TryGetCachedLease( LicenseConfiguration licenseConfiguration, DateTime now, out LicenseLease? lease )
        {
            if ( this._licenseRegistrar.TryGetLease( licenseConfiguration.LicenseString, now, out lease ) )
            {
                return true;
            }
            else
            {
                this.unusedUncachedLicenseServers.Add( licenseConfiguration );
                return false;
            }
        }

        public void LoadUserLicenses()
        {
            this.LoadUserLicenses( null, LicenseSource.Programmatic, null );
        }

        public void LoadUserLicenses( string[] licenseStrings, LicenseSource source, string sourceDescription )
        {
            var loadFromRegistry = true;


            // Load given license strings.
            if ( licenseStrings != null )
            {
                foreach ( var rawLicenseString in licenseStrings )
                {
                    this._licensingTrace?.WriteLine( "SharedUserLicenseManager: loading license string {{{0}}} from {1} ({2}).", rawLicenseString, sourceDescription, source );

                    var licenseString = License.CleanLicenseString( rawLicenseString );
                    loadFromRegistry = false;
                    LicenseConfiguration licenseConfiguration;

                    // We allow to add the same license string from different sources,
                    // to be able to check the source of license keys later.
                    if (
                        this.userLicenses.Any(
                            configuration => string.Equals( configuration.LicenseString, licenseString, StringComparison.OrdinalIgnoreCase )
                                             && configuration.Source == source ) )
                    {
                        this._licensingTrace?.WriteLine( "License {{{0}}} from {1} ({2}) has been added already.", licenseString, sourceDescription, source );
                        continue;
                    }


                    // Test some diagnostic "magic" licenses.
                    switch ( licenseString.ToLowerInvariant() )
                    {
                        case "none":
                        case "clear":
                            this.ClearLicenses();
                            this._licensingTrace?.WriteLine( "Licenses explicitly cleared." );
                            continue;
                        default:
                            // NOTE (bug 6239) we pass rawLicenseString for parsing in case the string contains the license server URL
                            if (!this._licenseRegistrar.TryParseLicenseString( rawLicenseString, source, sourceDescription, out licenseConfiguration ))
                            {
                                this._licensingTrace?.WriteLine( "Failed to parse license string \"{0}\" from {1} ({2}).", rawLicenseString, sourceDescription, source );
                                this._diagnosticsSink.ReportWarning( $"Cannot parse license key string '{rawLicenseString}'." ); // PS0301
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

        protected internal void AddUnusedLicenseConfiguration( LicenseConfiguration licenseConfiguration )
        {
            if ( licenseConfiguration.IsLicenseServerUrl )
            {
                this._licensingTrace?.WriteLine( "For further consideration, adding license server {0}.", licenseConfiguration );
                this.unusedLicenseServers.Add( licenseConfiguration );
            }
            else
            {
                this._licensingTrace?.WriteLine( "For further consideration, adding license {0}.", licenseConfiguration );
                if ( licenseConfiguration.License.LicenseType == LicenseType.PerUsage )
                    this.unusedPerUsageLicenseKeys.Add( licenseConfiguration );
                else
                    this.unusedLicenseKeys.Add( licenseConfiguration );
            }

            this.userLicenses.Add( licenseConfiguration );
        }

        private bool TryLoadNextCachedOrInternalLicenseKey( string ns, bool perUsage )
        {
            var unusedKeys = perUsage ? this.unusedPerUsageLicenseKeys : this.unusedLicenseKeys;
            while ( unusedKeys.Count > 0 )
            {
                var licenseConfiguration = unusedKeys.PopFirst();
                
                if ( this.TryLoadLicenseKey( licenseConfiguration, ns, true ) )
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryLoadLowestUncachedLicenseKey( string ns, bool perUsage )
        {
            var unusedUncachedKeys = perUsage ? this.unusedUncachedPerUsageLicenseKeys : this.unusedUncachedLicenseKeys;

            while ( unusedUncachedKeys.Count > 0 )
            {
                LicenseConfiguration lowestUncachedLicenseKey = null;

                foreach ( var key in unusedUncachedKeys )
                {
                    if ( lowestUncachedLicenseKey == null ||
                         lowestUncachedLicenseKey.License.LicenseType.IsBetterThan( key.License.LicenseType ) )
                    {
                        lowestUncachedLicenseKey = key;
                        continue;
                    }
                }

                unusedUncachedKeys.Remove( lowestUncachedLicenseKey );

                if ( this.TryLoadLicenseKey( lowestUncachedLicenseKey, ns, false ) )
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryLoadLicenseKey( LicenseConfiguration licenseConfiguration, string ns, bool cached )
        {
            this._licensingTrace?.WriteLine( "Trying to load {0} license \"{1}\".", cached ? "cached" : "non-cached", licenseConfiguration );

            var license = licenseConfiguration.License;

            if ( license == null )
            {
                throw new InvalidOperationException( string.Format( CultureInfo.InvariantCulture, "License not set for \"{0}\".", licenseConfiguration ) );
            }

            if ( license.LicenseType == LicenseType.PerUsage && licenseConfiguration.Source != LicenseSource.Configuration )
            {
                this._diagnosticsSink.ReportError( $"Per-usage licenses can only be used in configuration files. License '{license.LicenseId}' not loaded." ); // PS0303
                return false;
            }

            var errors = new List<string>();
            bool licenseIsValid;
            if ( licenseConfiguration.Source != LicenseSource.Internal )
            {
                License[] licenses = { license };
                var hash = LicenseSetData.GetHash( license.LicenseString );
                LicenseSetData licenseSetData;

                if ( cached )
                {
                    if (!LicenseSetData.TryGetCachedLicenseSetDataFromHash(licenses, hash, this._licenseCache, this._diagnosticsSink, this._licensingTrace, out licenseSetData))
                    {
                        this._licensingTrace?.WriteLine( "License \"{0}\" is not cached.", licenseConfiguration );
                        if ( license.LicenseType == LicenseType.PerUsage )
                            this.unusedUncachedPerUsageLicenseKeys.Add( licenseConfiguration );
                        else
                            this.unusedUncachedLicenseKeys.Add( licenseConfiguration );
                        return false;
                    }
                }
                else
                {
                    licenseSetData = new LicenseSetData( licenses, null, hash, this._licenseCache, this._dateTimeProvider, this._diagnosticsSink, this._licensingTrace );
                }

                licenseIsValid = licenseSetData!.Verify( null, errors );
            }
            else
            {
                // We don't verify internally-generated license keys because they
                // are trusted and they are not signed.
                licenseIsValid = license.Validate( null, this._dateTimeProvider, out var errorDescription );
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

                this._licensingTrace?.WriteLine( "The license {0} from {1} is invalid: {2}",
                                                    license.LicenseUniqueId,
                                                    licenseConfiguration.SourceDescription,
                                                    string.Join( " ", errors ) );

                this.userLicenses.Remove( licenseConfiguration );

                return false;
            }

            if ( (licenseConfiguration.License.GetAllowedLicenseSources() & licenseConfiguration.Source) != licenseConfiguration.Source )
            {
                this._diagnosticsSink?.ReportError( $"License error. The license {license.LicenseUniqueId} is not allowed to be loaded from {licenseConfiguration.SourceDescription}." ); // PS0260

                this._licensingTrace?.WriteLine( "The license key {0} is not valid because the source is not valid: expected {0}, but got {1}.", licenseConfiguration,
                                                    licenseConfiguration.License.GetAllowedLicenseSources(), licenseConfiguration.Source );

                this.userLicenses.Remove( licenseConfiguration );

                return false;
            }


            if ( license.IsLimitedByNamespace )
            {
                this.usedNamespaceLimitedLicenseKeys.Add( licenseConfiguration );

                if ( license.AllowsNamespace( ns ) )
                {
                    this._licensingTrace?.WriteLine( "Adding features for license \"{0}\" limited by namespace \"{1}\".",
                                                        licenseConfiguration, license.Namespace );

                    this.namespaceLimitedUserLicensedFeatures.Add( license );
                }
                else
                {
                    this._licensingTrace?.WriteLine( "License \"{0}\" is not allowed to be used in namespace \"{1}\".",
                                                        licenseConfiguration, ns );
                    return false;
                }
            }
            else
            {
                this._licensingTrace?.WriteLine( "Adding features for license \"{0}\".", licenseConfiguration );
                this.userLicensedFeatures.Add( license );
            }

            this.OnUserLicenseLoaded( license );

            this._licensingTrace?.WriteLine( "Loaded {0} license \"{1}\".",
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
                return new ReportedLicense( "None", "None" );
            }
            else
            {
                var bestLicenseConfiguration = this.userLicenses.OrderBy( configuration => RankLicense( configuration.License ) ).First();
                return bestLicenseConfiguration.GetReportedLicense();
            }
        }

        public void StartMonitorLicenseSource()
        {
            throw new NotImplementedException();
        }

        /// <exclude />
        public IList<LicenseConfiguration> UserLicenses { get; }

        private Dictionary<LicensedPackages, bool> GetCache( bool perUsage, bool inherited, bool cachedLicensesOnly )
        {
            if ( perUsage )
            {
                if ( inherited )
                {
                    return cachedLicensesOnly ? this.userLicenseRequirementsPerUsageWithInheritanceCachedOnlyCache : this.userLicenseRequirementsPerUsageWithInheritanceCache;
                }
                else
                {
                    return cachedLicensesOnly ? this.userLicenseRequirementsPerUsageWithoutInheritanceCachedOnlyCache : this.userLicenseRequirementsPerUsageWithoutInheritanceCache;
                }
            }
            else
            {
                if ( inherited )
                {
                    return cachedLicensesOnly ? this.userLicenseRequirementsWithInheritanceCachedOnlyCache : this.userLicenseRequirementsWithInheritanceCache;
                }
                else
                {
                    return cachedLicensesOnly ? this.userLicenseRequirementsWithoutInheritanceCachedOnlyCache : this.userLicenseRequirementsWithoutInheritanceCache;
                }
            }
        }

        internal bool TryGetAssemblyLicensedFeaturesFromMemoryCache( string path, string assemblyName, LicenseType[] requiredLicenseTypes, out IReadOnlyLicenseFeatureContainer? assemblyLicensedFeatures )
        {
            if ( !this.TryGetAssemblyLicenseFromMemoryCache( path, out var licenseSetData ) )
            {
                assemblyLicensedFeatures = null;
                return false;
            }
            
            assemblyLicensedFeatures = licenseSetData!.GetFilteredLicenseFeatures( assemblyName, requiredLicenseTypes );
            return true;
        }

        private bool TryGetAssemblyLicenseFromMemoryCache( string path, [NotNullWhen( returnValue: true )] out LicenseSetData? assemblyLicense )
        {
            LicensingTrace.Licensing?.WriteLine( "Getting license from assembly '{0}'.", path );

            string canonicalPath = LicenseSetData.GetCanonicalPath( path );

            // Check from in-memory cache.
            LicenseSetData licenseSetData;
            if ( this._cachedLicenses.TryGetValue( canonicalPath, out licenseSetData ) )
            {
                if ( licenseSetData.LastWriteTime == new FileInfo( path ).LastWriteTime )
                {
                    LicensingTrace.Licensing?.WriteLine( "License found in cache." );
                    return licenseSetData;
                }
                else
                {
                    LicensingTrace.Licensing?.WriteLine( "License found in cache, but they were obsolete." );
                    this._cachedLicenses.Remove( canonicalPath );
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        internal IReadOnlyLicenseFeatureContainer ReadAssemblyLicense( string path, byte[] publicKeyToken, IList<string> serializedLicenses, string assemblyName,
                                                                       LicenseType[] requiredLicenseTypes )
        {
            lock ( this.Sync )
            {
                LicensingTrace.Licensing?.WriteLine( "Reading licenses from {{{0}}}. {1} license(s) found.", path, serializedLicenses.Count );

                string canonicalPath = LicenseSetData.GetCanonicalPath( path );
                var emptyLicenseFeatureContainer = new LicenseFeatureContainer();

                // Read the license.
                // Look for the license resource in the assembly.
                var licenses = new List<License>();
                foreach ( var serializedLicense in serializedLicenses )
                {
                    License license = License.Deserialize( serializedLicense );

                    if ( license == null )
                    {
                        CoreMessageSource.Instance.Write( MessageLocation.Unknown, SeverityType.Warning, "PS0126", new object[] { path } );
                        return emptyLicenseFeatureContainer;
                    }

                    if ( license.RequiresRevocationCheck() )
                    {
                        // The users needs to approve the license agreement.
                        if ( base.UserLicenses == null )
                        {
                            CoreMessageSource.Instance.Write( MessageLocation.Unknown, SeverityType.Error, "PS0153", new object[] { path, license.LicenseId } );
                            this.OnLicenseRequired();
                            return emptyLicenseFeatureContainer;
                        }
                    }

                    // Ok.
                    licenses.Add( license );
                }


                var licenseSetData = LicenseSetData.GetLicenseSetDataFromPath( licenses.ToArray(), canonicalPath );

                // Verify the license.
                if ( !licenseSetData.Verify( publicKeyToken, MessageSource.MessageSink ) )
                {
                    return emptyLicenseFeatureContainer;
                }


                this._cachedLicenses[canonicalPath] = licenseSetData;

                return licenseSetData.GetFilteredLicenseFeatures( assemblyName, requiredLicenseTypes );
            }
        }
    }
}
