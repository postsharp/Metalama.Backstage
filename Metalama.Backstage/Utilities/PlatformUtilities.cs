// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Metalama.Backstage.Utilities
{
    public static class PlatformUtilities
    {
        public static string GetDotNetPath( ILogger logger, string? dotNetSdkDirectory = null )
        {
            var dotnetFileName = RuntimeInformation.IsOSPlatform( OSPlatform.Windows )
                ? "dotnet.exe"
                : "dotnet";

            logger.Trace?.Log( $"Looking for {dotnetFileName} path." );

            // See if the current process is dotnet.exe.

            var currentProcessPath = Process.GetCurrentProcess().MainModule?.FileName;

            if ( currentProcessPath == null )
            {
                logger.Trace?.Log( $"We don't know if the current process is {dotnetFileName} because the current process'es main module path is unknown." );
            }
            else
            {
                var currentProcessFileName = Path.GetFileName( currentProcessPath );

                if ( currentProcessFileName == dotnetFileName )
                {
                    logger.Trace?.Log( $"The current process '{currentProcessPath}' is {dotnetFileName}." );

                    return currentProcessPath;
                }
                else
                {
                    logger.Trace?.Log( $"The current process '{currentProcessPath}' is not {dotnetFileName}." );
                }
            }

            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                // Find dotnet.exe at well-known locations.

                var dotnetPath = Environment.ExpandEnvironmentVariables( "%ProgramFiles%\\dotnet\\dotnet.exe" );

                if ( File.Exists( dotnetPath ) )
                {
                    logger.Trace?.Log( $"dotnet.exe found in '{dotnetPath}'." );

                    return dotnetPath;
                }
                else
                {
                    logger.Trace?.Log( $"Looked for dotnet.exe in '{dotnetPath}' but it did not exist." );
                }

                dotnetPath = Environment.ExpandEnvironmentVariables( "%ProgramFiles(x86)%\\dotnet\\dotnet.exe" );

                if ( File.Exists( dotnetPath ) )
                {
                    logger.Trace?.Log( $"dotnet.exe found in '{dotnetPath}'." );

                    return dotnetPath;
                }
                else
                {
                    logger.Trace?.Log( $"Looked for dotnet.exe in '{dotnetPath}' but it did not exist." );
                }
            }

            // Look in the DotNetSdkDirectory, if we know it.

            if ( dotNetSdkDirectory != null )
            {
                for ( var directory = dotNetSdkDirectory; directory != null; directory = Path.GetDirectoryName( directory ) )
                {
                    var dotnetPath = Path.Combine( directory, dotnetFileName );

                    if ( File.Exists( dotnetPath ) )
                    {
                        logger.Trace?.Log( $"{dotnetFileName} found in '{dotnetPath}'." );

                        return dotnetPath;
                    }
                    else
                    {
                        logger.Trace?.Log( $"Looked for {dotnetFileName} in '{dotnetPath}', but it did not exist." );
                    }
                }
            }

            // The file was not found.
            logger.Trace?.Log( $"{dotnetFileName} was found nowhere. We hope it will be in the path." );

            return "dotnet";
        }
    }
}