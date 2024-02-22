// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Diagnostics
{
    public interface ILoggerFactory : IDisposable, IBackstageService
    {
        ILogger GetLogger( string category );

        void Flush();
    }
}