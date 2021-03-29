// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Settings
{
    public interface IRegistryKey : IDisposable
    {
        string[] GetSubKeyNames();

        IRegistryKey OpenSubKey( string subKey );

        IRegistryKey OpenSubKey( string name, bool writable );

        IRegistryKey CreateSubKey( string subKey );

        void DeleteSubKey( string subKey );

        void DeleteSubKey( string subKey, bool throwOnMissingSubKey );

        void DeleteSubKeyTree( string subKey );

        void DeleteSubKeyTree( string subKey, bool throwOnMissingSubKey );

        string[] GetValueNames();

        object GetValue(string name);

        object GetValue(string name, object defaultValue);

        void SetValue( string name, string value );

        void SetDWordValue( string name, int value );

        void SetQWordValue( string name, long value );

        void DeleteValue(string name);

        void DeleteValue(string name, bool throwOnMissingSubKey);

        void Close();

        /// <summary>
        /// Returns true if this is a real registry key that can be monitored using Win32 API.
        /// </summary>
        bool CanMonitorChanges { get; }
    }
}
