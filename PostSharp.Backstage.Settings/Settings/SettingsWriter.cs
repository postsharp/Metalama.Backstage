// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Settings
{
    public abstract class SettingsWriter
    {
        public abstract void Open();

        public abstract void SetValue( string name, bool value );

        public abstract void SetValue( string name, bool? value );

        public abstract void SetValue( string name, int value );

        public abstract void SetValue( string name, string value );

        public abstract void SetValue( string name, DateTime value );

        public abstract void SetValue( string name, DateTime? value );

        public abstract void Close();
    }
}
