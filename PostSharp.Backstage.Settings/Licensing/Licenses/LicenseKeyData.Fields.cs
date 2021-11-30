// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Licenses.LicenseFields;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing.Licenses
{
    internal partial class LicenseKeyData
    {
        private readonly SortedDictionary<LicenseFieldIndex, LicenseField> _fields = new();

        private object? GetFieldValue( LicenseFieldIndex index )
        {
            if (_fields.TryGetValue( index, out var licenseField ))
            {
                return licenseField.Value;
            }
            else
            {
                return null;
            }
        }

        // Used for testing
        private void SetUnknownFieldValue( bool mustUnderstand, object value )
        {
            var index = (LicenseFieldIndex)( mustUnderstand ? 128 : 253 );

            switch (value.GetType().Name)
            {
                case nameof(Boolean):
                    SetFieldValue<LicenseFieldBool>( index, value );

                    break;

                case nameof(Byte):
                    SetFieldValue<LicenseFieldByte>( index, value );

                    break;

                case nameof(Int16):
                    SetFieldValue<LicenseFieldInt16>( index, value );

                    break;

                case nameof(Int32):
                    SetFieldValue<LicenseFieldInt32>( index, value );

                    break;

                case nameof(Int64):
                    SetFieldValue<LicenseFieldInt64>( index, value );

                    break;

                case nameof(DateTime):
                    SetFieldValue<LicenseFieldDate>( index, ( (DateTime)value ).Date );
                    SetFieldValue<LicenseFieldDateTime>( index, value );

                    break;

                case nameof(String):
                    SetFieldValue<LicenseFieldString>( index, value );

                    break;

                case nameof(Byte) + "[]":
                    SetFieldValue<LicenseFieldBytes>( index, value );

                    break;

                case nameof(Guid):
                    SetFieldValue<LicenseFieldGuid>( index, value );

                    break;

                default:
                    throw new ArgumentException( $"License fields of type {value.GetType()} are not supported." );
            }
        }

        private void SetFieldValue<T>( LicenseFieldIndex index, object? value )
            where T : LicenseField, new()
        {
            if (value == null)
            {
                _fields.Remove( index );
            }
            else
            {
                _fields[index] = new T { Value = value };
            }
        }
    }
}