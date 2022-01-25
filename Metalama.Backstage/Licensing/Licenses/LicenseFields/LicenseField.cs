// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;

namespace Metalama.Backstage.Licensing.Licenses.LicenseFields
{
    internal abstract class LicenseField
    {
        public object? Value { get; set; }

        public abstract void Write( BinaryWriter writer );

        protected abstract bool TryGetConstantLength( out byte length );

        /// <summary>
        /// Writes the length of the field if it has a constant length.
        /// </summary>
        /// <remarks>
        /// For fields with dynamic length, the length is written as part of the payload
        /// to the same position.
        /// </remarks>
        /// <param name="writer"></param>
        public void WriteConstantLength( BinaryWriter writer )
        {
            if ( !this.TryGetConstantLength( out var length ) )
            {
                return;
            }

            writer.Write( length );
        }

        public abstract void Read( BinaryReader reader );

        public override string ToString()
        {
            return this.Value == null ? "null" : this.Value.ToString();
        }

        public LicenseField Clone()
        {
            return (LicenseField) this.MemberwiseClone();
        }
    }
}