﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using PostSharp.Backstage.Licensing.Licenses.LicenseFields;

namespace PostSharp.Backstage.Licensing.Licenses
{
    internal partial class LicenseKeyData
    {
        private readonly SortedDictionary<LicenseFieldIndex, LicenseField> _fields = new SortedDictionary<LicenseFieldIndex, LicenseField>();

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
                case nameof( Boolean ):
                    this.SetFieldValue<LicenseFieldBool>( index, value );
                    break;

                case nameof( Byte ):
                    this.SetFieldValue<LicenseFieldByte>( index, value );
                    break;

                case nameof( Int16 ):
                    this.SetFieldValue<LicenseFieldInt16>( index, value );
                    break;

                case nameof( Int32 ):
                    this.SetFieldValue<LicenseFieldInt32>( index, value );
                    break;

                case nameof( Int64 ):
                    this.SetFieldValue<LicenseFieldInt64>( index, value );
                    break;

                case nameof( DateTime ):
                    this.SetFieldValue<LicenseFieldDate>( index, ((DateTime) value).Date );
                    this.SetFieldValue<LicenseFieldDateTime>( index, value );
                    break;

                case nameof( String ):
                    this.SetFieldValue<LicenseFieldString>( index, value );
                    break;

                case nameof( Byte ) + "[]":
                    this.SetFieldValue<LicenseFieldBytes>( index, value );
                    break;

                case nameof( Guid ):
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
