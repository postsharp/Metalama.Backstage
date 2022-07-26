// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Backstage.Extensibility
{
    internal sealed class PlatformInfo : IPlatformInfo
    {
        public string? DotNetSdkDirectory { get; }

        public PlatformInfo( string? dotNetSdkDirectory )
        {
            this.DotNetSdkDirectory = dotNetSdkDirectory;
        }
    }
}