﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace PostSharp.Backstage.Licensing.Licenses.LicenseFields
{
    [Serializable]
    internal sealed class LicenseFieldBool : LicenseField
    {
        private bool _isBuggy;

        public override void Write( BinaryWriter writer )
        {
            if (_isBuggy)
            {
                writer.Write( (bool)Value! ? 1 : 0 );
            }
            else
            {
                writer.Write( (byte)( (bool)Value! ? 1 : 0 ) );
            }
        }

        protected override bool TryGetConstantLength( out byte length )
        {
            if (_isBuggy)
            {
                throw new InvalidOperationException( "Boolean license fields requiring the length to be serialized should no longer be buggy." );
            }

            length = sizeof(byte);

            return true;
        }

        public override void Read( BinaryReader reader )
        {
            Value = reader.ReadByte() != 0;

            // Check if the field was emitted as a int as a result of a bug.
            _isBuggy = false;

            if (reader.BaseStream.Length > reader.BaseStream.Position)
            {
                if (reader.ReadByte() == 0)
                {
                    reader.ReadByte();
                    reader.ReadByte();
                    _isBuggy = true;
                }
                else
                {
                    reader.BaseStream.Seek( -1, SeekOrigin.Current );
                }
            }
        }
    }
}