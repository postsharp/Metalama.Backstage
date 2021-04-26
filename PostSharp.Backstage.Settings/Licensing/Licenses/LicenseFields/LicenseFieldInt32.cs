// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace PostSharp.Backstage.Licensing.Licenses.LicenseFields
{
    [Serializable]
    internal sealed class LicenseFieldInt32 : LicenseField
    {
        public override void Write( BinaryWriter writer )
        {
            writer.Write( (int) this.Value );
        }

        protected override bool TryGetConstantLength( out byte length )
        {
            length = sizeof( int );
            return true;
        }

        public override void Read( BinaryReader reader )
        {
            this.Value = reader.ReadInt32();
        }
    }
}
