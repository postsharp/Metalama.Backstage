// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage.Testing.Services
{
    public class TestPlatformInfo : IPlatformInfo
    {
        public string? DotNetSdkDirectory => @"C:\TestProgramFiles\dotnet.exe";
    }
}