// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Reflection;

namespace PostSharp.Backstage.Settings
{
    [Obfuscation( Exclude = true, ApplyToMembers = true )]
    // TODO: This used to be internal.
    public static class RegistryKeyExtensions
    {
        public static void SetValueNullableBoolean( this IRegistryKey key, string name, bool? value )
        {
            int intValue = ConvertNullableBooleanToDWord( value );

            key.SetDWordValue( name, intValue );
        }

        public static int ConvertNullableBooleanToDWord( bool? value )
        {
            int intValue;

            if ( value.HasValue )
            {
                if ( value.Value )
                    intValue = 1;
                else
                    intValue = 2;
            }
            else
            {
                intValue = 0;
            }
            return intValue;
        }

        public static bool? GetValueNullableBoolean( this IRegistryKey key, string name )
        {
            switch ( (int) key.GetValue( name, 0 ) )
            {
                case 1:
                    return true;

                case 2:
                    return false;

                default:
                    return null;
            }
        }

        private static readonly DateTime referenceDate = new DateTime( 2000, 1, 1 );

        public static void SetValueDateTime( this IRegistryKey registryKey, string name, DateTime value )
        {
            registryKey.SetQWordValue( name, ConvertDateTimeToQWord( value ) );
        }

        public static long ConvertDateTimeToQWord( DateTime value )
        {
            return (long) value.ToUniversalTime().Subtract( referenceDate ).TotalMilliseconds;
        }

        public static DateTime GetValueDateTime( this IRegistryKey registryKey, string name, DateTime defaultValue )
        {
            return GetValueDateTime( registryKey, name ).GetValueOrDefault( defaultValue );
        }

        public static DateTime? GetValueDateTime( this IRegistryKey registryKey, string name )
        {
            long timestamp = (long) registryKey.GetValue( name, 0L );
            const long maxValue = 1000L*60*60*24*365*1000;
            if ( timestamp <= 0 || timestamp > maxValue ) return null;

            return referenceDate.AddMilliseconds( timestamp ).ToLocalTime();
        }
    }
}