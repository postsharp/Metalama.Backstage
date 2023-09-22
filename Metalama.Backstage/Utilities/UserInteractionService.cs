// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Runtime.InteropServices;

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Local
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable SA1401

namespace Metalama.Backstage.Utilities;

internal class UserInteractionService : IUserInteractionService
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

    private delegate bool MonitorEnumDelegate( IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData );

    [DllImport( "user32.dll" )]
    private static extern bool EnumDisplayMonitors( IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData );

    [DllImport( "user32.dll", SetLastError = true )]
    private static extern bool GetMonitorInfo( IntPtr hMonitor, ref MonitorInfo lpmi );

    [StructLayout( LayoutKind.Sequential )]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
    private class MonitorInfo
    {
        public int Size = Marshal.SizeOf( typeof(MonitorInfo) );
        public Rect Monitor;
        public Rect WorkArea;
        public uint Flags;
    }

    public int? GetTotalMonitorWidth()
    {
        try
        {
            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                var totalWidth = 0;

                EnumDisplayMonitors(
                    IntPtr.Zero,
                    IntPtr.Zero,
                    ( IntPtr _, IntPtr _, ref Rect rect, IntPtr _ ) =>
                    {
                        totalWidth += rect.Right - rect.Left;

                        return true;
                    },
                    IntPtr.Zero );

                return totalWidth;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    // Method to get the last input time in seconds
    public TimeSpan? GetLastInputTime()
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