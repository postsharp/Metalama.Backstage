﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// There is a copy of this code in Metalama.Compiler.Shared and partially in Metalama ResourceExtractor.

using System;
using System.IO;

namespace Metalama.Backstage.Utilities;

public static class MetalamaPathUtilities
{
    private static readonly string? _overriddenTempPath;

    static MetalamaPathUtilities()
    {
        var overriddenTempPath = Environment.GetEnvironmentVariable( "METALAMA_TEMP" );
        _overriddenTempPath = string.IsNullOrEmpty( overriddenTempPath ) ? null : overriddenTempPath;
    }

    public static string GetTempPath() => _overriddenTempPath ?? Path.GetTempPath();

    public static string GetTempFileName()
    {
        if ( _overriddenTempPath == null )
        {
            return Path.GetTempFileName();
        }

        // https://stackoverflow.com/a/10152460/4100001
        var attempt = 0;

        while ( true )
        {
            var path = Path.Combine( _overriddenTempPath, $"{Guid.NewGuid()}.tmp" );

            try
            {
                using ( var newFile = new FileStream( path, FileMode.Create ) )
                {
                    newFile.Close();
                }
            }
            catch ( IOException ) when ( ++attempt < 10 ) { continue; }

            return path;
        }
    }
}