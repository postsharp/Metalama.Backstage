// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace PostSharp.Backstage.Licensing
{
    internal interface IReadOnlyLicenseFeatureContainer
    {
        bool IsEmpty { get; }

        LicensedPackages LicensedPackages { get; }

        LicensedPackages LicensedPackagesWithInheritanceAllowed { get; }

        LicensedPackages LicensedPackagesPerUsage { get; }

        LicensedPackages LicensedPackagesPerUsageWithInheritanceAllowed { get; }

        IReadOnlyDictionary<string, string> AuditedLicenses { get; }
        ISet<License> UsedPerUsageLicenses { get; }

        bool SatisfiesRequirement( LicensedPackages requirement, bool perUsage, bool inherited );

        LicensedPackages GetLicensedPackages( bool perUsage, bool inherited );
    }

    [Serializable]
    internal class LicenseFeatureContainer : IReadOnlyLicenseFeatureContainer
    {
        private readonly Dictionary<string, string> auditedLicenses = new Dictionary<string, string>();
        private readonly ISet<License> usedPerUsageLicenses = new HashSet<License>();

        public bool IsEmpty => this.LicensedPackages == Licensing.LicensedPackages.None &&
                               this.LicensedPackagesWithInheritanceAllowed == Licensing.LicensedPackages.None &&
                               this.LicensedPackagesPerUsage == Licensing.LicensedPackages.None &&
                               this.LicensedPackagesPerUsageWithInheritanceAllowed == Licensing.LicensedPackages.None &&
                               this.auditedLicenses.Count == 0 &&
                               this.usedPerUsageLicenses.Count == 0;

        public LicensedPackages LicensedPackages { get; set; } = LicensedPackages.None;
        public LicensedPackages LicensedPackagesWithInheritanceAllowed { get; set; } = LicensedPackages.None;
        public LicensedPackages LicensedPackagesPerUsage { get; set; } = LicensedPackages.None;
        public LicensedPackages LicensedPackagesPerUsageWithInheritanceAllowed { get; set; } = LicensedPackages.None;

        public IReadOnlyDictionary<string, string> AuditedLicenses => this.auditedLicenses;
        public ISet<License> UsedPerUsageLicenses => this.usedPerUsageLicenses;

        public LicensedPackages GetLicensedPackages( bool perUsage, bool inherited )
        {
            if ( perUsage )
                return inherited ? this.LicensedPackagesPerUsageWithInheritanceAllowed : this.LicensedPackagesPerUsage;
            else
                return inherited ? this.LicensedPackagesWithInheritanceAllowed : this.LicensedPackages;
        }

        public bool SatisfiesRequirement( LicensedPackages requirement, bool perUsage, bool inherited )
        {
            return this.GetLicensedPackages( perUsage, inherited ).Includes( requirement );
        }

        public void Clear()
        {
            this.LicensedPackages = LicensedPackages.None;
            this.LicensedPackagesWithInheritanceAllowed = LicensedPackages.None;
            this.LicensedPackagesPerUsage = LicensedPackages.None;
            this.LicensedPackagesPerUsageWithInheritanceAllowed = LicensedPackages.None;
            this.auditedLicenses.Clear();
            this.usedPerUsageLicenses.Clear();
        }

        private void Add( LicensedPackages licensedPackage, bool perUsage = false, bool allowInheritance = false )
        {
            if ( perUsage )
                this.LicensedPackagesPerUsage |= licensedPackage;
            else
                this.LicensedPackages |= licensedPackage;

            if ( !allowInheritance )
                return;

            if ( perUsage )
                this.LicensedPackagesPerUsageWithInheritanceAllowed |= licensedPackage;
            else
                this.LicensedPackagesWithInheritanceAllowed |= licensedPackage;
        }

        public void Add( License license )
        {
            this.Add( license.GetLicensedPackages(), license.LicenseType == LicenseType.PerUsage, license.AllowInheritance.GetValueOrDefault() );

            if ( license.IsAudited() )
                this.auditedLicenses[license.LicenseUniqueId] = license.LicenseString;

            if ( license.LicenseType == LicenseType.PerUsage )
                this.usedPerUsageLicenses.Add( license );
        }

        public void AddAuditedLicenses( IReadOnlyDictionary<string, string> auditedLicenses )
        {
            // We do not check collisions here because it would be expensive.
            // (The reason for a collision must be a hack.)
            foreach ( KeyValuePair<string, string> auditedLicense in auditedLicenses )
            {
                this.auditedLicenses[auditedLicense.Key] = auditedLicense.Value;
            }
        }

        public void AddUsedPerUsageLicense( License usedLicense )
        {
            this.usedPerUsageLicenses.Add( usedLicense );
        }

        public void AddAll( IReadOnlyLicenseFeatureContainer features )
        {
            this.LicensedPackages |= features.LicensedPackages;
            this.LicensedPackagesWithInheritanceAllowed |= features.LicensedPackagesWithInheritanceAllowed;
            this.LicensedPackagesPerUsage |= features.LicensedPackagesPerUsage;
            this.LicensedPackagesPerUsageWithInheritanceAllowed |= features.LicensedPackagesPerUsageWithInheritanceAllowed;

            this.AddAuditedLicenses( features.AuditedLicenses );
            // Only one per-usage license should ever be present.
            if (features.UsedPerUsageLicenses.Any())
                this.AddUsedPerUsageLicense( features.UsedPerUsageLicenses.First() );
        }
    }
}
