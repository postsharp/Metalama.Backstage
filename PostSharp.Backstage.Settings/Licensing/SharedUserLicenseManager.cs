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

        


        

        public SharedUserLicenseManager()
        {
            this.ClearLicenses();

            this.licenseLookedForInRegistry = false;
            this.UserLicenses = new ReadOnlyCollection<LicenseConfiguration>( this.userLicenses );
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
