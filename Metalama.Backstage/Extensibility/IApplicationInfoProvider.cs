// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Backstage.Extensibility
{
    // TODO: For licensing, we need info about all applications together.
    // TODO: Split IApplicationInfo to application, component and process info.
    public interface IApplicationInfoProvider
    {
        IApplicationInfo CurrentApplication { get; }

        IDisposable WithApplication( IApplicationInfo applicationInfo );
    }
}