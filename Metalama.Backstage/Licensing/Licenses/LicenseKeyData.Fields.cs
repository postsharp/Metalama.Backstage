// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Licenses.LicenseFields;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Licenses
{
    public partial class LicenseKeyData
    {
        private readonly SortedDictionary<LicenseFieldIndex, LicenseField> _fields = new();

        private object? GetFieldValue( LicenseFieldIndex index )
        {
            if ( this._fields.TryGetValue( index, out var licenseField ) )
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
            var index = (LicenseFieldIndex) (mustUnderstand ? 128 : 253);

            switch ( value.GetType().Name )
            {
                case nameof(Boolean):
                    this.SetFieldValue<LicenseFieldBool>( index, value );

                    break;

                case nameof(Byte):
                    this.SetFieldValue<LicenseFieldByte>( index, value );

                    break;

                case nameof(Int16):
                    this.SetFieldValue<LicenseFieldInt16>( index, value );

                    break;

                case nameof(Int32):
                    this.SetFieldValue<LicenseFieldInt32>( index, value );

                    break;

                case nameof(Int64):
                    this.SetFieldValue<LicenseFieldInt64>( index, value );

                    break;

                case nameof(DateTime):
                    this.SetFieldValue<LicenseFieldDate>( index, ((DateTime) value).Date );
                    this.SetFieldValue<LicenseFieldDateTime>( index, value );

                    break;

                case nameof(String):
                    this.SetFieldValue<LicenseFieldString>( index, value );

                    break;

                case nameof(Byte) + "[]":
                    this.SetFieldValue<LicenseFieldBytes>( index, value );

                    break;

                case nameof(Guid):
                    this.SetFieldValue<LicenseFieldGuid>( index, value );

                    break;

                default:
                    throw new ArgumentException( $"License fields of type {value.GetType()} are not supported." );
            }
        }

        private void SetFieldValue<T>( LicenseFieldIndex index, object? value )
            where T : LicenseField, new()
        {
            if ( value == null )
            {
                this._fields.Remove( index );
            }
            else
            {
                this._fields[index] = new T { Value = value };
            }
        }
    }
}