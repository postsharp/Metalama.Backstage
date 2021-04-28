// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Extensibility
{
    // Adding a replaceable layer of abstraction for testing purposes.
    public interface IApplicationInfoService
    {
        bool IsPrerelease { get; }

        Version Version { get; }

        string VersionString { get; }

        DateTime BuildDate { get; }
    }
}
