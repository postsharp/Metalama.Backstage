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

        
        private List<WeakReference> sharedInstances = new List<WeakReference>();
        private readonly object sharedInstancesLock = new object();

        public LocalUserLicenseManager( SharedUserLicenseManager sharedUserLicenseManager, ICurrentProjectService currentProjectService )
        {
            this.CurrentSharedManager = sharedUserLicenseManager;
            this.currentProjectService = currentProjectService;
        }

        
        
#pragma warning disable CA1721 // Property names should not match get methods
        

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

        




        

        public void Dispose()
        {
            this.CurrentSharedManager?.Dispose();
        }
    }
}
