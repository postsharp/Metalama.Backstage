// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Extensibility;

internal static class VersionExtensions
{
    public static bool IsDevelopmentVersion( string version )
    {
        var versionParts = version.Split( '-' );

        return versionParts.Length > 1 && versionParts[1] is "dev" or "local";
    }
}