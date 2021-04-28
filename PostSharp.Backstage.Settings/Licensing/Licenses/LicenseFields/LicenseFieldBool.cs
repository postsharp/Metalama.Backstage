﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace PostSharp.Backstage.Licensing.Licenses.LicenseFields
{
    [Serializable]
    internal sealed class LicenseFieldBool : LicenseField
    {
        private bool isBuggy;

        public override void Write( BinaryWriter writer )
        {
            if ( this.isBuggy )
            {
                writer.Write( (bool) this.Value ? 1 : 0 );
            }
            else
            {
                writer.Write( (byte) ((bool) this.Value ? 1 : 0) );
            }
        }

        protected override bool TryGetConstantLength( out byte length )
        {
            if ( this.isBuggy )
            {
                throw new InvalidOperationException( "Boolean license fields requiring the length to be serialized should no longer be buggy." );
            }

            length = sizeof( byte );
            return true;
        }

        public override void Read( BinaryReader reader )
        {
            this.Value = reader.ReadByte() != 0;

            // Check if the field was emitted as a int as a result of a bug.
            this.isBuggy = false;
            if ( reader.BaseStream.Length > reader.BaseStream.Position )
            {
                if ( reader.ReadByte() == 0 )
                {
                    reader.ReadByte();
                    reader.ReadByte();
                    this.isBuggy = true;
                }
                else
                {
                    reader.BaseStream.Seek( -1, SeekOrigin.Current );
                }
            }
        }
    }
}