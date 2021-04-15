// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Settings;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing
{
    public class LicenseManager : UserLicenseManager
    {
        

        public LicenseManager( UserSettings userSettings, IServiceLocator serviceProvider, ITrace licensingTrace )
            : base( userSettings, serviceProvider, licensingTrace )
        {
        }

        internal bool VcsCheckEnabled { get; set; }
    }
}
