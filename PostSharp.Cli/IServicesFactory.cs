// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.CommandLine;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Cli
{
    public interface IServicesFactory
    {
        (IServiceProvider Services, ITrace Trace) Create( IConsole console, bool verbose );
    }
}
