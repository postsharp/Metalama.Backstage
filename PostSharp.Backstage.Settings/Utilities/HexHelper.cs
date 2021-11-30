// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace PostSharp.Backstage.Utilities
{
    internal static class HexHelper
    {
        public static bool TryParseBytes( string str, int start, int count, [MaybeNullWhen( false )] out byte[] bytes )
        {
            if ( count % 2 != 0 )
            {
                bytes = null;

                return false;
            }

            bytes = new byte[count / 2];

            for ( var i = 0; i < count / 2; i++ )
            {
                if ( !byte.TryParse( str.Substring( start + (2 * i), 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result ) )
                {
                    return false;
                }

                bytes[i] = result;
            }

            return true;
        }

        private static readonly char[] _hexChars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        /// <summary>
        /// Formats an array of bytes into an hexadecimal string, using a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="builder">The <see cref="StringBuilder"/> into which the string has to be written.</param>
        public static void FormatBytes( byte[] bytes, StringBuilder builder )
        {
            if ( bytes != null && bytes.Length > 0 )
            {
                var finalSize = builder.Length + (bytes.Length * 2);

                if ( builder.Capacity < finalSize )
                {
                    builder.Capacity = finalSize * 2;
                }

                foreach ( var b in bytes )
                {
                    builder.Append( _hexChars[b >> 4] );
                    builder.Append( _hexChars[b & 0xf] );
                }
            }
            else
            {
                builder.Append( "null" );
            }
        }

        /// <summary>
        /// Formats an array of bytes into a hexadecimal string.
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <returns>The hexadecimal string corresponding to <paramref name="bytes"/>.</returns>
        public static string FormatBytes( byte[] bytes, string nullString = "null" )
        {
            if ( bytes == null || bytes.Length == 0 )
            {
                return nullString;
            }

            var builder = new StringBuilder( 20 );
            FormatBytes( bytes, builder );

            return builder.ToString();
        }
    }
}