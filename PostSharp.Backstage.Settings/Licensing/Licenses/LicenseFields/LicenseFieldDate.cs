// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace PostSharp.Backstage.Licensing.Licenses.LicenseFields
{
    [Serializable]
    internal class LicenseFieldDate : LicenseField
    {
        private static readonly DateTime referenceDate = new DateTime( 2010, 1, 1 );

        public override void Write( BinaryWriter writer )
        {
            ushort days = (ushort) ((DateTime) this.Value).Subtract( referenceDate ).TotalDays;
            writer.Write( days );
        }

        protected override bool TryGetConstantLength( out byte length )
        {
            length = sizeof( ushort );
            return true;
        }

        public override void Read( BinaryReader reader )
        {
            ushort days = reader.ReadUInt16();
            this.Value = referenceDate.AddDays( days );
        }
    }
}
