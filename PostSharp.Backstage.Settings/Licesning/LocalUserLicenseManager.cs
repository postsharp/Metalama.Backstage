// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Backstage.Licensing.Helpers;
using System.Runtime.InteropServices;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Settings;

#pragma warning disable CA1063 // Some fields don't need to be disposed.

namespace PostSharp.Backstage.Licensing
{
    /// <summary>
    /// Manages PostSharp licenses.
    /// </summary>
    [Obfuscation( Exclude = true )]
    public class LocalUserLicenseManager : IDisposable
    {

        private ICurrentProjectService currentProjectService;

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
        private List<WeakReference> sharedInstances = new List<WeakReference>();
        private readonly object sharedInstancesLock = new object();

        public LocalUserLicenseManager( SharedUserLicenseManager sharedUserLicenseManager, ICurrentProjectService currentProjectService )
        {
            this.CurrentSharedManager = sharedUserLicenseManager;
            this.currentProjectService = currentProjectService;
        }

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
        internal SharedUserLicenseManager CurrentSharedManager { get; private set; }

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

        internal bool IsRequirementSatisfiedByUserLicense( IDiagnosticsSink diagnosticsSink, LicensedPackages licensedPackage )
        {
            // TODO: Licensing for Linux/Mac
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return true;
            }
            
            return this.IsRequirementSatisfiedByUserLicense( diagnosticsSink, cachedLicensesOnly: false, licensedPackage, perUsage: false, inherited: false ) ||
                   this.IsRequirementSatisfiedByUserLicense( diagnosticsSink, cachedLicensesOnly: false, licensedPackage, perUsage: true, inherited: false );
        }

        internal bool IsRequirementSatisfiedByUserLicense( IDiagnosticsSink diagnosticsSink, bool cachedLicensesOnly,
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

            satisfied = this.CurrentSharedManager.TrySatisfyRequirement( diagnosticsSink, licensedPackage, targetAssemblyName, perUsage, inherited, cachedLicensesOnly,
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

        public void LoadUserLicenses( string[] licenseStrings, LicenseSource source, string sourceDescription, IDiagnosticsSink diagnosticsSink = null )
        {
            // TODO: Licensing on Linux.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            this.CurrentSharedManager.LoadUserLicenses( licenseStrings, source, sourceDescription, diagnosticsSink );
        }

        protected string GetTargetAssemblyName()
        {
            return this.currentProjectService.GetTargetAssemblyName();
        }

        public void Dispose()
        {
            this.CurrentSharedManager?.Dispose();
        }
    }
}
