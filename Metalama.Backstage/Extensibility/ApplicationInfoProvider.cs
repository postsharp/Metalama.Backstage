// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Backstage.Extensibility
{
    internal class ApplicationInfoProvider : IApplicationInfoProvider
    {
        public IApplicationInfo CurrentApplication { get; set; }

        public ApplicationInfoProvider( IApplicationInfo initialApplication )
        {
            this.CurrentApplication = initialApplication;
        }
    }
}