// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace PostSharp.Backstage.Licensing.Licenses.LicenseFields
{
    [Serializable]
    internal sealed class LicenseFieldInt64 : LicenseField
    {
        public override void Write( BinaryWriter writer )
        {
            writer.Write( (long) this.Value! );
        }

        protected override bool TryGetConstantLength( out byte length )
        {
            length = sizeof( long );
            return true;
        }

        public override void Read( BinaryReader reader )
        {
            this.Value = reader.ReadInt64();
        }
    }
}
