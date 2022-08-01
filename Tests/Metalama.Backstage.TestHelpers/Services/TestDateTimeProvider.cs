// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Testing.Services
{
    public class TestDateTimeProvider : IDateTimeProvider
    {
        private DateTime? _now;

        public DateTime Now => this._now ?? DateTime.Now;

        public void Set( DateTime now )
        {
            this._now = now;
        }

        public void Reset()
        {
            this._now = null;
        }
    }
}