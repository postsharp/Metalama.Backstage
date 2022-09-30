// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Metalama.Backstage.Utilities
{
    public static class PlatformUtilities
    {
        private const string _dotNetSdkDirectoryEnvironmentVariableName = "MSBuildExtensionsPath";

        public static string GetDotNetSdkVersion( string? dotNetSdkDirectory = null )
        {
            dotNetSdkDirectory ??= Environment.GetEnvironmentVariable( _dotNetSdkDirectoryEnvironmentVariableName );

            if ( string.IsNullOrEmpty( dotNetSdkDirectory ) )
            {
                throw new InvalidOperationException( "Cannot get the .NET SDK Directory." );
            }

            return Path.GetFileName( dotNetSdkDirectory );
        }
        
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
                logger.Trace?.Log( $"We don't know if the current process is {dotnetFileName} because the current processes main module path is unknown." );
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

            // Look in the DotNetSdkDirectory, if we know it.
            dotNetSdkDirectory ??= Environment.GetEnvironmentVariable( _dotNetSdkDirectoryEnvironmentVariableName );

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

            // Search dotnet.exe in well-known locations.
            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                // %ProgramFiles% is expanded to the correct directory according to the current processor architecture.
                // This is good because we always want to get dotnet.exe for the current processor architecture.
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
            }

            // The file was not found.
            logger.Warning?.Log( $"{dotnetFileName} was found nowhere. We hope it will be in the PATH." );

            return "dotnet";
        }
    }
}