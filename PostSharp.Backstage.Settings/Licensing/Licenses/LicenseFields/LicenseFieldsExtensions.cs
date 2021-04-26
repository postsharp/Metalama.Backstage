// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing.Licenses.LicenseFields
{
    internal static class LicenseFieldsExtensions
    {
        /// <summary>
        /// Returns <c>true</c> if a license field must be understood.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <remarks>
        /// Till PostSharp 6.5.16/6.8.9/6.9.2, all license fields had to be understood.
        /// Adding a field used to cause backward incompatibility.
        /// In the next versions and in Caravela, we allow fields of indexes 129-253
        /// to be followed by its length (1 byte).
        /// If such field is unknown, the given number of bytes is ignored.
        /// </remarks>
        public static bool IsMustUnderstandField( LicenseFieldIndex index )
        {
            byte i = (byte) index;

            if ( i < 1 || i > 255 )
            {
                throw new ArgumentOutOfRangeException( nameof( index ) );
            }

            return i <= 128 || i >= 254;
        }
    }

    /// <summary>
    /// Returns <c>true</c> if a license field data contain its length.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <remarks>
    /// Till PostSharp 6.5.16/6.8.9/6.9.2, all license fields had to be known.
    /// Adding a field used to cause backward incompatibility.
    /// In the next versions and in Caravela, we require each new filed index
    /// to be followed by its length (1 byte).
    /// If such field is unknown, the given number of bytes is ignored.
    /// </remarks>
    public static bool IsFieldPrefixedByLength( LicenseFieldIndex index )
    {
        byte i = (byte) index;

        if ( i < 1 || i > 255 )
        {
            throw new ArgumentOutOfRangeException( nameof( index ) );
        }

        return i > 21 && i < 254;
    }
}
