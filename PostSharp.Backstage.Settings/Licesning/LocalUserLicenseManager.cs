// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Platform;
using PostSharp.Sdk.User;
using PostSharp.Backstage.Licensing.Helpers;
using PostSharp.Sdk.Services;
using System.Runtime.InteropServices;

#pragma warning disable CA1063 // Some fields don't need to be disposed.

namespace PostSharp.Backstage.Licensing
{
    /// <summary>
    /// Manages PostSharp licenses.
    /// </summary>
    [Obfuscation( Exclude = true )]
    public class LocalUserLicenseManager : IDisposable
    {

        private static ICurrentProjectService currentProjectService;

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
        private RemotingAccessor<SharedUserLicenseManager> currentSharedManager;

        private List<WeakReference> sharedInstances = new List<WeakReference>();
        private readonly object sharedInstancesLock = new object();

        static LocalUserLicenseManager()
        {
            DefaultApplicationInfoService.Initialize();
        }

        /// <summary>
        /// Singleton instance.
        /// </summary>
        /// <exclude />
        [Obfuscation( Exclude = true )]
        public static LocalUserLicenseManager Instance { get; private set; }

        protected void AddAuditedLicenses( IReadOnlyDictionary<string, string> auditedLicenses )
        {
            this.userLicensedFeaturesLocal.AddAuditedLicenses( auditedLicenses );
        }
        
#pragma warning disable CA1721 // Property names should not match get methods
        public IReadOnlyDictionary<string, string> AuditableLicenses => this.userLicensedFeaturesLocal.AuditedLicenses;

        [Obsolete("Use the AuditableLicenses property.")]
        public IReadOnlyDictionary<string, string> GetAuditableLicenses() => this.AuditableLicenses;
#pragma warning restore CA1721 // Property names should not match get methods

        public ISet<License> UsedPerUsageLicenses => this.userLicensedFeaturesLocal.UsedPerUsageLicenses;

        /// <exclude />
        [Obfuscation( Exclude = true )]
        internal SharedUserLicenseManager CurrentSharedManager
        {
            get => this.currentSharedManager == null ? null : this.currentSharedManager.Value;
            private set => this.currentSharedManager = new RemotingAccessor<SharedUserLicenseManager>( value, false );
        }

        public static bool IsInitialized => Instance?.CurrentSharedManager != null;

        public static void Initialize( params string[] userLicenseKeys )
        {
            Initialize<LocalUserLicenseManager>( userLicenseKeys );
        }

        protected virtual SharedUserLicenseManager CreateSharedManager()
        {
            LicensingTrace.Licensing?.WriteLine( "Creating a shared user license manager." );
            return new SharedUserLicenseManager();
        }

        private void RegisterSharedManager( SharedUserLicenseManager sharedManager )
        {
            LicensingTrace.Licensing?.WriteLine( "Registering a shared user license manager." );

            this.CurrentSharedManager = sharedManager;

            lock ( this.sharedInstancesLock )
            {
                List<WeakReference> validInstances = new List<WeakReference>( this.sharedInstances.Count );
                foreach ( WeakReference instance in this.sharedInstances )
                {
                    if ( instance.IsAlive )
                        validInstances.Add( instance );
                }
                validInstances.Add( new WeakReference( sharedManager ) );
                this.sharedInstances = validInstances;
            }
        }

        /// <summary>
        /// Initializes the <see cref="LocalUserLicenseManager"/>.
        /// </summary>
        /// <param name="userLicenseKeys">User license strings.</param>
        public static void Initialize<T>( params string[] userLicenseKeys )
            where T : LocalUserLicenseManager, new()
        {
            LicensingTrace.Licensing?.WriteLine( "Initializing the local user license manager without checking the calling assembly with {0} user licenses.",
                                                userLicenseKeys.Length );

            Instance = new T();

            SharedUserLicenseManager sharedManager = Instance.CreateSharedManager();
            sharedManager.LoadUserLicenses( userLicenseKeys, LicenseSource.Programmatic, "License Manager" );
            Instance.RegisterSharedManager( sharedManager );
        }

        [Obfuscation( Exclude = true )]
        internal static void InitializeDirect<T>()
            where T : LocalUserLicenseManager, new()
        {
            LicensingTrace.Licensing?.WriteLine( "Initializing the local user license manager." );

            Instance = new T();
            Instance.RegisterSharedManager( Instance.CreateSharedManager() );
            Instance.userLicensedFeaturesLocal.Clear();
        }

        [Obfuscation( Exclude = true )]
        internal static void InitializeLocalOnly<T>( SharedUserLicenseManager sharedManager )
            where T : LocalUserLicenseManager, new()
        {
            LicensingTrace.Licensing?.WriteLine( "Initializing the local user license manager." );

            // The type object cannot be provided by a factory method of the shared manager
            // because of remoting.
            Instance = new T();
            Instance.CurrentSharedManager = sharedManager;
        }


        /// <exclude />
        public IList<LicenseConfiguration> UserLicenses
        {
            get { return this.CurrentSharedManager.UserLicenses; }
        }

        [Obfuscation( Exclude = true )]
        internal event EventHandler<LicenseEventArgs> LicenseMessageEmitted
        {
            [Obfuscation( Exclude = true )]
            add { this.CurrentSharedManager.LicenseMessageEmitted += value; }

            [Obfuscation( Exclude = true )]
            remove { this.CurrentSharedManager.LicenseMessageEmitted -= value; }
        }

        [Obfuscation(Exclude = true)]
        internal event EventHandler LicenseRequired
        {
            [Obfuscation(Exclude = true)]
            add { this.CurrentSharedManager.LicenseRequired += value; }

            [Obfuscation(Exclude = true)]
            remove { this.CurrentSharedManager.LicenseRequired -= value; }
        }
        

        internal void StartMonitorRegistry()
        {
            this.CurrentSharedManager.StartMonitorRegistry();
        }

        internal void NotifyLicenseChange()
        {
            try
            {
                DateTime timestamp = UserSettings.GetCurrentDateTime();
                DateTime lastTimestamp = LicensingRegistryHelper.GetRegistryTimestamp();
                if ( lastTimestamp >= timestamp )
                    timestamp = lastTimestamp.AddMilliseconds( 1 );

                // Notify in-process listeners.
                foreach ( WeakReference instance in this.sharedInstances )
                {
                    SharedUserLicenseManager licenseManager = (SharedUserLicenseManager) instance.Target;
                    licenseManager?.OnRegistryChanged( timestamp );
                }

                // Notify out-of-process listeners.
                LicensingRegistryHelper.SetRegistryTimestamp( timestamp );
            }
#if DEBUG
            finally
            {
            }
#else
            catch
            {
            }
#endif
        }

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

        internal bool IsRequirementSatisfiedByUserLicense( IMessageSink messageSink, LicensedPackages licensedPackage )
        {
            // TODO: Licensing for Linux/Mac
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return true;
            }
            
            return this.IsRequirementSatisfiedByUserLicense( messageSink, cachedLicensesOnly: false, licensedPackage, perUsage: false, inherited: false ) ||
                   this.IsRequirementSatisfiedByUserLicense( messageSink, cachedLicensesOnly: false, licensedPackage, perUsage: true, inherited: false );
        }

        internal bool IsRequirementSatisfiedByUserLicense( IMessageSink messageSink, bool cachedLicensesOnly,
                                              LicensedPackages licensedPackage, bool perUsage = false, bool inherited = false )
        {
            Dictionary<LicensedPackages, bool> cache = this.GetCache(perUsage, inherited, cachedLicensesOnly);

            if ( cache.TryGetValue( licensedPackage, out bool satisfied ) )
                return satisfied;

            LicensingTrace.Licensing?.WriteLine( "LocalUserLicenseManager: Checking whether the package '{0}' is licensed.", licensedPackage );


            if ( this.userLicensedFeaturesLocal.SatisfiesRequirement( licensedPackage, perUsage, inherited ) )
            {
                LicensingTrace.Licensing?.WriteLine( "LocalUserLicenseManager: The licensed package requirement '{0}' is satisfied by local user licenses.", licensedPackage );
                cache[licensedPackage] = true;
                return true;
            }

            string targetAssemblyName = GetTargetAssemblyName();

            if ( this.namespaceLimitedUserLicensedFeaturesLocal.TryGetValue( targetAssemblyName, out LicenseFeatureContainer currentNamespaceLimitedUserLicenseFeaturesLocal )
                 && currentNamespaceLimitedUserLicenseFeaturesLocal.SatisfiesRequirement( licensedPackage, perUsage, inherited ) )
            {
                LicensingTrace.Licensing?.WriteLine(
                    "LocalUserLicenseManager: The license package requirement '{0}' is satisfied by a license constrained to the assembly {1}.",
                    licensedPackage, targetAssemblyName );
                return true;
            }

            LicensingTrace.Licensing?.WriteLine( "LocalUserLicenseManager: No local licensing info. Asking the shared license manager." );

            satisfied = this.CurrentSharedManager.TrySatisfyRequirement( messageSink, licensedPackage, targetAssemblyName, perUsage, inherited, cachedLicensesOnly,
                                                                         LicenseSource.All,
                                                                         out IReadOnlyLicenseFeatureContainer userLicensedFeaturesShared,
                                                                         out IReadOnlyLicenseFeatureContainer namespaceLimitedUserLicensedFeaturesShared );

            if ( userLicensedFeaturesShared != null )
                this.userLicensedFeaturesLocal.AddAll( userLicensedFeaturesShared );

            if ( namespaceLimitedUserLicensedFeaturesShared != null )
            {
                if ( currentNamespaceLimitedUserLicenseFeaturesLocal == null )
                {
                    currentNamespaceLimitedUserLicenseFeaturesLocal = new LicenseFeatureContainer();
                    this.namespaceLimitedUserLicensedFeaturesLocal[targetAssemblyName] = currentNamespaceLimitedUserLicenseFeaturesLocal;
                }

                currentNamespaceLimitedUserLicenseFeaturesLocal.AddAll( namespaceLimitedUserLicensedFeaturesShared );
            }

            cache[licensedPackage] = satisfied;
            return satisfied;
        }

        internal void ClearLeaseCache()
        {
            LicensingTrace.Licensing?.WriteLine( "Clearing the license lease cache." );

            LicensingRegistryHelper.ClearLeaseCacheFromRegistry();

            this.userLicensedFeaturesLocal.Clear();
            this.userLicenseRequirementsWithInheritanceCache.Clear();
            this.userLicenseRequirementsWithInheritanceCachedOnlyCache.Clear();
            this.userLicenseRequirementsWithoutInheritanceCache.Clear();
            this.userLicenseRequirementsWithoutInheritanceCachedOnlyCache.Clear();
            this.userLicenseRequirementsPerUsageWithInheritanceCache.Clear();
            this.userLicenseRequirementsPerUsageWithInheritanceCachedOnlyCache.Clear();
            this.userLicenseRequirementsPerUsageWithoutInheritanceCache.Clear();
            this.userLicenseRequirementsPerUsageWithoutInheritanceCachedOnlyCache.Clear();
        }

        public ReportedLicense GetReportedLicense()
        {
            return this.CurrentSharedManager.GetReportedLicense();
        }

        public void LoadUserLicenses()
        {
            this.LoadUserLicenses( null, LicenseSource.Programmatic, null );
        }

        public void LoadUserLicenses( string[] licenseStrings, LicenseSource source, string sourceDescription, IMessageSink messageSink = null )
        {
            // TODO: Licensing on Linux.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            this.CurrentSharedManager.LoadUserLicenses( licenseStrings, source, sourceDescription, messageSink );
        }

        protected static string GetTargetAssemblyName()
        {
            if ( currentProjectService == null )
            {
                DefaultCurrentProjectService.Initialize();
                currentProjectService = SystemServiceLocator.GetService<ICurrentProjectService>();
            }

            return currentProjectService.GetTargetAssemblyName();
        }

        public void Dispose()
        {
            this.currentSharedManager?.Dispose();
        }
    }
}
