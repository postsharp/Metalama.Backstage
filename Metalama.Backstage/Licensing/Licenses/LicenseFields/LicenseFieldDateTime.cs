﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace Metalama.Backstage.Licensing.Licenses.LicenseFields
{
    [Serializable]
    internal class LicenseFieldDateTime : LicenseField
    {
        public override void Write( BinaryWriter writer )
        {
            var data = ((DateTime) this.Value!).ToUniversalTime().ToBinary();
            writer.Write( data );
        }

        protected override bool TryGetConstantLength( out byte length )
        {
            length = sizeof(long);

            return true;
        }

        public override void Read( BinaryReader reader )
        {
            var data = reader.ReadInt64();
            this.Value = DateTime.FromBinary( data ).ToLocalTime();
        }
    }
}