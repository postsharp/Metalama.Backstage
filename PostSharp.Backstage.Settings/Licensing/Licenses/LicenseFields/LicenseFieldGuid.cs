// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace PostSharp.Backstage.Licensing.Licenses.LicenseFields
{
    [Serializable]
    internal sealed class LicenseFieldGuid : LicenseField
    {
        private const byte sizeOfGuidByteArray = 16;

        public override void Write( BinaryWriter writer )
        {
            Guid guid = (Guid) this.Value;
            writer.Write( guid.ToByteArray() );
        }

        protected override bool TryGetConstantLength( out byte length )
        {
            length = sizeOfGuidByteArray;
            return true;
        }

        public override void Read( BinaryReader reader )
        {
            this.Value = new Guid( reader.ReadBytes( sizeOfGuidByteArray ) );
        }

        public override string ToString()
        {
            if ( this.Value == null )
                return "null";

            return ((Guid) this.Value).ToString();
        }
    }
}
