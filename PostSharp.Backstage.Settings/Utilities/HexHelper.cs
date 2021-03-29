// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System.Globalization;
using System.Text;

namespace PostSharp.Backstage.Utilities
{
    // TODO: This used to be internal.
    public static class HexHelper
    {
        public static byte[] TryParseBytes( string str, int start, int count )
        {
            if ( (count%2) != 0 ) return null;

            byte[] bytes = new byte[count/2];

            for ( int i = 0; i < count/2; i ++ )
            {
                byte result;
                if ( !byte.TryParse( str.Substring( start+2*i, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result ) )
                    return null;

                bytes[i] = result;
            }

            return bytes;
        }

        private static readonly char[] hexChars = new[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};

        /// <summary>
        /// Formats an array of bytes into an hexadecimal string, using a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="builder">The <see cref="StringBuilder"/> into which the string has to be written.</param>
        public static void FormatBytes(byte[] bytes, StringBuilder builder)
        {
           
            if (bytes != null && bytes.Length > 0)
            {
                int finalSize = builder.Length + bytes.Length * 2;

                if (builder.Capacity < finalSize)
                    builder.Capacity = finalSize*2;

                foreach (byte b in bytes)
                {
                    builder.Append(hexChars[b >> 4]);
                    builder.Append( hexChars[ b & 0xf ]);
                }
            }
            else
            {
                builder.Append("null");
            }
        }

        /// <summary>
        /// Formats an array of bytes into a hexadecimal string.
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <returns>The hexadecimal string corresponding to <paramref name="bytes"/>.</returns>
        public static string FormatBytes(byte[] bytes, string nullString = "null")
        {
            if (bytes == null || bytes.Length == 0)
            {
                return nullString;
            }

            StringBuilder builder = new StringBuilder(20);
            FormatBytes(bytes, builder);
            return builder.ToString();
        }
    }
}