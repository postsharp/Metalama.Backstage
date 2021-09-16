// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// These tests are too slow.

// using PostSharp.Backstage.Licensing.Licenses;
// using PostSharp.Backstage.Licensing.Cryptography;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Security.Cryptography;
// using System.Text;
// using System.Threading.Tasks;
// using PostSharp.Backstage.Licensing.Tests.Services;
// using Xunit.Abstractions;
// using Xunit;

namespace PostSharp.Backstage.Licensing.Tests.Licenses
{
    public class LicenseKeySerializationTests // : LicensingTestsBase
    {
// private const LicenseType _unknownLicenseType = (LicenseType) 255;
//        private const LicensedProduct _unknownLicensedProduct = (LicensedProduct) 255;

// public LicenseKeySerializationTests( ITestOutputHelper logger )
//            : base( logger )
//        {
//        }

// private void TestLicenseSerialization( LicenseKeyData licenseKeyData )
//        {
//            licenseKeyData.MinPostSharpVersion = LicenseKeyData.MinPostSharpVersionValidationRemovedPostSharpVersion;

// string licenseKey;
//            string publicKeyString;

// using ( var privateKey = DSA.Create() )
//            {
//                licenseKeyData.Sign( 0, privateKey.ToXmlString2( true ) );
//                licenseKey = licenseKeyData.Serialize();
//                publicKeyString = privateKey.ToXmlString2( false );
//            }

// License license = new( licenseKey, this.Services, this.Trace );

// Assert.True( license.TryGetLicenseKeyData( out var deserializedLicenseKeyData ) );

// using ( var publicKey = DSA.Create() )
//            {
//                publicKey.FromXmlString2( publicKeyString );
//                Assert.True( deserializedLicenseKeyData!.VerifySignature( publicKey ) );
//            }
//        }

// [Fact]
//        public void UnknownLicenseTypeCanSerialize()
//        {
//            var license = new LicenseKeyData
//            {
//                LicenseId = 1,
//                Version = 2,
//                Product = LicensedProduct.Ultimate
//            };

// license.LicenseType = _unknownLicenseType;

// this.TestLicenseSerialization( license );
//        }

// [Fact]
//        public void UnknownLicensedProductCanSerialize()
//        {
//            var license = new LicenseKeyData
//            {
//                LicenseId = 1,
//                Version = 2,
//                LicenseType = LicenseType.PerUser
//            };

// license.Product = _unknownLicensedProduct;

// this.TestLicenseSerialization( license );
//        }

// private void TestUnknownField<T>( T value )
//        {
//            var license = new LicenseKeyData
//            {
//                LicenseId = 1,
//                Version = 2,
//                LicenseType = LicenseType.PerUser,
//                Product = LicensedProduct.Ultimate
//            };

// license.UnknownMustUnderstandField = value;

// this.TestLicenseSerialization( license );

// license = new LicenseKeyData
//            {
//                LicenseId = 1,
//                Version = 2,
//                LicenseType = LicenseType.PerUser,
//                Product = LicensedProduct.Ultimate
//            };

// license.UnknownOptionalField = value;

// this.TestLicenseSerialization( license );
//        }

// [Fact]
//        public void UnknownBoolFieldCanSerialize()
//        {
//            this.TestUnknownField( false );
//            this.TestUnknownField( true );
//        }

// [Fact]
//        public void UnknownByteFieldCanSerialize()
//        {
//            this.TestUnknownField( byte.MinValue );
//            this.TestUnknownField<byte>( byte.MaxValue / 2 );
//            this.TestUnknownField( byte.MaxValue );
//        }

// [Fact]
//        public void UnknownInt16FieldCanSerialize()
//        {
//            this.TestUnknownField( short.MinValue );
//            this.TestUnknownField<short>( short.MaxValue / 2 );
//            this.TestUnknownField( short.MaxValue );
//        }

// [Fact]
//        public void UnknownInt32FieldCanSerialize()
//        {
//            this.TestUnknownField( int.MinValue );
//            this.TestUnknownField( int.MaxValue / 2 );
//            this.TestUnknownField( int.MaxValue );
//        }

// [Fact]
//        public void UnknownInt64FieldCanSerialize()
//        {
//            this.TestUnknownField( long.MinValue );
//            this.TestUnknownField( long.MaxValue / 2 );
//            this.TestUnknownField( long.MaxValue );
//        }

// [Fact]
//        public void UnknownDateAndDateTimeFieldCanSerialize()
//        {
//            // The License.UnknownField testing property setter
//            // sets both the date field and the datetime field.
//            this.TestUnknownField( DateTime.MinValue );
//            this.TestUnknownField( new DateTime( DateTime.MaxValue.Ticks / 2 ) );
//            this.TestUnknownField( DateTime.MaxValue );
//        }

// [Fact]
//        public void UnknownStringFieldCanSerialize()
//        {
//            this.TestUnknownField( "" );
//            this.TestUnknownField( "\0" );
//            this.TestUnknownField( "\0\0\0\0\0\0\0" );
//            this.TestUnknownField( " " );
//            this.TestUnknownField( "              " );
// ReSharper disable CommentTypo
//            this.TestUnknownField( "příliš žluťoučký kůň úpěl ďábelské ódy / PŘÍLIŠ ŽLUŤOUČKÝ KŮŇ ÚPĚL ĎÁBELSKÉ ÓDY" );
//            this.TestUnknownField( "слишком желтоватый конь вой дьявольская ода" );
// ReSharper restore CommentTypo
//            this.TestUnknownField( new string( 'x', byte.MaxValue / 2 ) );
//            this.TestUnknownField( new string( 'x', byte.MaxValue ) );
//        }

// [Fact]
//        public void UnknownBytesFieldCanSerialize()
//        {
//            static byte[] CreateByteArray( byte length )
//            {
//                var array = new byte[length];

// for ( byte i = 0; i < length; i++ )
//                {
//                    array[i] = i;
//                }

// return array;
//            }

// this.TestUnknownField( CreateByteArray( 0 ) );
//            this.TestUnknownField( CreateByteArray( 1 ) );
//            this.TestUnknownField( CreateByteArray( 2 ) );
//            this.TestUnknownField( CreateByteArray( byte.MaxValue / 2 ) );
//            this.TestUnknownField( CreateByteArray( byte.MaxValue ) );
//        }

// [Fact]
//        public void UnknownGuidFieldCanSerialize()
//        {
//            this.TestUnknownField( Guid.Empty );
//            this.TestUnknownField( Guid.NewGuid() );
//        }

// private static void TestBackwardIncompatibleSerialization( LicenseKeyData licenseKeyData )
//        {
//            Assert.Throws<InvalidOperationException>( () => licenseKeyData.Serialize() );
//        }

// [Fact]
//        public void SerializationOnIncompatibleProductFails()
//        {
//            var license = new LicenseKeyData
//            {
//                LicenseId = 1,
//                Version = 2,
//                LicenseType = LicenseType.PerUser
//            };

// license.Product = _unknownLicensedProduct;

// TestBackwardIncompatibleSerialization( license );
//        }

// [Fact]
//        public void SerializationOnIncompatibleFieldFails()
//        {
//            var license = new LicenseKeyData
//            {
//                LicenseId = 1,
//                Version = 2,
//                LicenseType = LicenseType.PerUser,
//                Product = LicensedProduct.Ultimate
//            };

// license.UnknownMustUnderstandField = "unknown-must-understand";

// TestBackwardIncompatibleSerialization( license );

// license = new LicenseKeyData
//            {
//                LicenseId = 1,
//                Version = 2,
//                LicenseType = LicenseType.PerUser,
//                Product = LicensedProduct.Ultimate
//            };

// license.UnknownOptionalField = "unknown-optional";

// TestBackwardIncompatibleSerialization( license );
//        }
//    }
    }
}