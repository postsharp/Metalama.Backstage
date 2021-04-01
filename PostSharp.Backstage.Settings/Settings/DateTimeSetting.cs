// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Settings
{
    public class DateTimeSetting : Setting<DateTime>
    {
        public DateTimeSetting( string name, SettingsReader reader, SettingsWriter writer, DateTime defaultValue = default )
            : base( name, reader, writer, defaultValue )
        {
        }

        protected override void RefreshImpl( SettingsReader reader )
        {
            this.Value = reader.GetValue( this.Name, this.DefaultValue );
        }

        protected override void SetImpl( DateTime value, SettingsWriter writer )
        {
            writer.SetValue( this.Name, value );
        }
    }
}
