// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial source-available license. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Utilities
{
    // TODO: This used to be internal.
    public static class BitSetHelper
    {
        /// <summary>
        /// Counts the number of bits set in an <see cref="int"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int CountBits( int value )
        {
            int count = 0;
            for ( int i = 0; i < 32; i++ )
            {
                if ( (value & (1 << i)) != 0 )
                {
                    count++;
                }
            }

            return count;
        }
    }
}