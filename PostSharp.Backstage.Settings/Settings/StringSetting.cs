// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Settings
{
    public class StringSetting : Setting<string>
    {
        public StringSetting( string name, SettingsReader reader, SettingsWriter writer, string? defaultValue = null ) 
            : base( name, reader, writer, defaultValue )
        {
        }

        protected override void RefreshImpl( SettingsReader reader )
        {
            this.Value = reader.GetValue( this.Name, this.DefaultValue );
        }

        protected override void SetImpl( string value, SettingsWriter writer )
        {
            writer.SetValue( this.Value, value );
        }
    }
}
