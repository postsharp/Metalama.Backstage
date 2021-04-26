// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

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
