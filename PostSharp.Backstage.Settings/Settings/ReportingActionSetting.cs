// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Settings
{
    public class ReportingActionSetting : Setting<ReportingAction>
    {
        public ReportingActionSetting( string name, SettingsReader reader, SettingsWriter writer, ReportingAction defaultValue = default ) 
            : base( name, reader, writer, defaultValue )
        {
        }

        protected override void RefreshImpl( SettingsReader reader )
        {
            this.Value = (ReportingAction) reader.GetInt32Value( this.Name, (int) this.DefaultValue );
        }

        protected override void SetImpl( ReportingAction value, SettingsWriter writer )
        {
            writer.SetValue( this.Name, (int) value );
        }
    }
}
