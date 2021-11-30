// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;
using System.Text;

namespace PostSharp.Backstage.Licensing.Licenses.LicenseFields
{
    [Serializable]
    internal sealed class LicenseFieldString : LicenseField
    {
        public override void Write( BinaryWriter writer )
        {
            var bytes = Encoding.UTF8.GetBytes( ( (string)Value! ).Normalize() );

            if (bytes.Length > 255)
            {
                throw new InvalidOperationException( "Cannot have strings of more than 255 characters." );
            }

            writer.Write( (byte)bytes.Length );
            writer.Write( bytes );
        }

        protected override bool TryGetConstantLength( out byte length )
        {
            // The length of this field is variable.
            length = 0;

            return false;
        }

        public override void Read( BinaryReader reader )
        {
            var bytes = reader.ReadBytes( reader.ReadByte() );
            Value = Encoding.UTF8.GetString( bytes );
        }
    }
}