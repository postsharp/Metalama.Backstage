// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Text;

namespace PostSharp.Backstage.Utilities
{
    internal static class HashUtilities
    {
        public static string HashString( string s ) =>
            HexHelper.FormatBytes( new MD5Managed().ComputeHash( Encoding.UTF8.GetBytes( s ) ) );
    }
}