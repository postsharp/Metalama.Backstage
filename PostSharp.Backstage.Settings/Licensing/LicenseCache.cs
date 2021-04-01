// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing
{
    internal abstract class LicenseCache
    {
        protected readonly IDateTimeProvider DateTimeProvider;
        protected readonly ITrace LicensingTrace;

        protected LicenseCache( IDateTimeProvider dateTimeProvider, ITrace licensingTrace )
        {
            this.DateTimeProvider = dateTimeProvider;
            this.LicensingTrace = licensingTrace;
        }

        public abstract bool IsValidHash( string hash, out DateTime lastVerificationTime, out IDateTimeProvider dateTimeProvider );

        public abstract bool IsValidPath( string path, out string hash, out DateTime lastWriteTime, out DateTime lastVerificationTime, out IDateTimeProvider dateTimeProvider );

        public abstract void SetVerificationResult( string hash, bool valid, string? path = null, DateTime? lastWriteTime = null );
    }
}
