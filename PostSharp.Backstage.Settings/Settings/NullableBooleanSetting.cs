// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Settings
{
    public class NullableBooleanSetting : Setting<bool?>
    {
        public NullableBooleanSetting( string name, SettingsReader reader, SettingsWriter writer, bool? defaultValue = null ) 
            : base( name, reader, writer, defaultValue )
        {
        }

        protected override void RefreshImpl( SettingsReader reader )
        {
            this.Value = reader.GetValue( this.Name, this.DefaultValue );
        }

        protected override void SetImpl( bool? value, SettingsWriter writer )
        {
            writer.SetValue( this.Name, value );
        }
    }
}
