// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Extensibility
{
    public interface IPlatformInfo : IBackstageService
    {
        string? DotNetSdkDirectory { get; }

        string DotNetExePath { get; }

        string? DotNetSdkVersion { get; }
    }
}