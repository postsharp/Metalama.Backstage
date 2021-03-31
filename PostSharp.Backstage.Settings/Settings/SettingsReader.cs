// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Settings
{
    public abstract class SettingsReader
    {
        public abstract void Open();

        public abstract bool GetValue( string name, bool defaultValue );

        public abstract bool? GetValue( string name, bool? defaultValue );

        public abstract int GetValue( string name, int defaultValue );

        public abstract string GetValue( string name, string? defaultValue );

        public abstract DateTime GetValue( string name, DateTime defaultValue );

        public abstract DateTime? GetValue( string name, DateTime? defaultValue );

        public abstract void Close();
    }
}
