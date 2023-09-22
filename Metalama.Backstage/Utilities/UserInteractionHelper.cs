// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Runtime.InteropServices;

namespace Metalama.Backstage.Utilities;

internal static class UserInteractionHelper
{
    [StructLayout( LayoutKind.Sequential )]
    private struct LastInputInfo
    {
        public uint Size;
        public uint Time;
    }

    // Importing GetLastInputInfo from User32.dll
    [DllImport( "User32.dll" )]
    private static extern bool GetLastInputInfo( ref LastInputInfo lastInputInfo );

    // Method to get the last input time in seconds
    public static TimeSpan? GetLastInputTime()
    {
        try
        {
            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                var lastInputInfo = default(LastInputInfo);
                lastInputInfo.Size = (uint) Marshal.SizeOf( lastInputInfo );

                if ( GetLastInputInfo( ref lastInputInfo ) )
                {
                    return TimeSpan.FromMilliseconds( (uint) Environment.TickCount - lastInputInfo.Time );
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}