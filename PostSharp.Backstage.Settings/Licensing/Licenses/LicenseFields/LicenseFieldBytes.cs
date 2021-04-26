// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;
using PostSharp.Backstage.Utilities;

namespace PostSharp.Backstage.Licensing.Licenses.LicenseFields
{
    [Serializable]
    internal sealed class LicenseFieldBytes : LicenseField
    {
        public override void Write( BinaryWriter writer )
        {
            byte[] bytes = (byte[]) this.Value;
            if ( bytes.Length > 255 )
                throw new InvalidOperationException( "Cannot have buffers of more than 255 bytes." );

            writer.Write( (byte) bytes.Length );
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
            this.Value = reader.ReadBytes( reader.ReadByte() );
        }

        public override string ToString()
        {
            if ( this.Value == null )
                return "null";
            return HexHelper.FormatBytes( (byte[]) this.Value );
        }
    }
}
