// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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