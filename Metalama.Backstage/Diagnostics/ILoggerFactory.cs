// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Diagnostics
{
    public interface ILoggerFactory : IDisposable
    {
        ILogger GetLogger( string category );
    }
}