// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Backstage.Extensibility
{
    internal class ApplicationInfoProvider : IApplicationInfoProvider
    {
        public IApplicationInfo CurrentApplication { get; private set; }

        public IDisposable WithApplication( IApplicationInfo applicationInfo )
        {
            var cookie = new WithApplicationDispose( this );
            this.CurrentApplication = applicationInfo;

            return cookie;
        }

        public ApplicationInfoProvider( IApplicationInfo initialApplication )
        {
            this.CurrentApplication = initialApplication;
        }

        private class WithApplicationDispose : IDisposable
        {
            private readonly ApplicationInfoProvider _parent;
            private readonly IApplicationInfo _previousApplication;

            public WithApplicationDispose( ApplicationInfoProvider parent )
            {
                this._parent = parent;
                this._previousApplication = parent.CurrentApplication;
            }

            public void Dispose()
            {
                this._parent.CurrentApplication = this._previousApplication;
            }
        }
    }
}