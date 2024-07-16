// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Infrastructure;
using System;
using System.Diagnostics;

namespace Metalama.Backstage.Testing
{
    public class TestDateTimeProvider : IDateTimeProvider
    {
        private DateTime? _lastResetTime;
        private Stopwatch? _stopwatch;

        public DateTime Now => this._lastResetTime?.AddMilliseconds( this._stopwatch?.ElapsedMilliseconds ?? 0 ) ?? DateTime.UtcNow;

        public void Set( DateTime now, bool keepRunning = false )
        {
            this._lastResetTime = now;
            this._stopwatch = keepRunning ? Stopwatch.StartNew() : null;
        }

        public void AddTime( TimeSpan timeSpan ) => this.Set( this.Now + timeSpan );
    }
}