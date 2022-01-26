// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// These tests are too slow.

// using PostSharp.Backstage.Licensing.Licenses;
// using System;
// using System.Security.Cryptography;
// using Xunit;
// using Xunit.Abstractions;

// namespace PostSharp.Backstage.Licensing.Tests.Licenses
// {
//    public class LicenseKeySerializationTests : LicensingTestsBase
//    {
//        private const string _testPrivateKeyString = "<DSAKeyValue><P>jwKMyxKMFFeA6XTZqn0sS+S/NSVGcTMOoVTjmt+ehb1pNXOrg5tEktha7xnKv0F+KeJgc9lWsK+1AVPuG+nmuVkApRey2O2r14Ot+25YYWT1U7fjEfFj91WBrn/n7sySieJI6YhAGH1TycIxWXjOcDWfBRSGSpb/xeqo2jMBKIy52byAj7+C3aoqXsja9OCeBToahXa/ylbNukKgvXLwIB0eyWHmWrreEcoNEilDd5DWqSeK2bjbVD8S+yR38+ygUcJjEmMXXWM4qtezdgi3YGxKXeaq+3ypJUjm4ufE3t8yBSMBRgWJjGgqFVxQh48gOkvWC6OZYMISAjBuHq7epQ==</P><Q>2RgyTfMjnHWs0NxZT3YcZPQG+PhshzOVDeplOjQm1g8=</Q><G>XWLBisfiKad7LvpZM5VV2jjzMWv4ePzrnJWHHzcg0QU2cTdcdM6zNukVzHpCgUho3POr4GKAmjAvLGV1AI4SE60SdlQRbH9qvYAwEEdT2h4ucvN9/yZHxMD0zcZCJ7vEMU2Nyaiy/+4Ys9N5O5QBDoPBsoncOCFUuR5Aq7Htme+lMUKi5R/xTarmlqxhnGY+BE2mrVoe4j8rIYcN3Jzmb+cWBCgl4lywvDpBJPbQrpbnts+RyFsddnUjoaZEv3/Ywdcusmb0ieM+Vz2+xGPpWpSMFDwJ7eYKGwEkaac4XBxxq/16DZzIs4WBP46Dw5zVbQ/IBLmCQ2vFOiDMPoNwHw==</G><Y>bUECfQ6VdPp4QEZ8vSIWWoCGyjqGiwYkHA0/HJ08aEZaqgw6mXEmxXsdWigwAIVN876xBDcf9tSX8ykZl92sw9snuFxX/qKXBfYh3wwSa87zInmT2pDin6eO9qqWBQHA7grq4G1PVDNUliWrNEj4mplbRQ7ExPARBm/6iQUYYapy3ebkc30wz6w0u4LfUBxXsTsl+DCpjdA7nm8SfSkybp8Sh4JElYYAhUBlYQ2MF99+VQKxjjvk4eIXlG36HIYCKiA6WwyS0YxiU6E97W3Ay60jdQpL+nY9uh1EuCuEeua7wRIko0ajrw1cUpx4tCNGX0SP17cMLjap4bpOJPdjTQ==</Y><Seed>Si9hL7RPCYnQQurajEQ9pq1FEwbvXJXuJL84wALkoNQ=</Seed><PgenCounter>B6k=</PgenCounter><X>a6GhG/qmvCAsLn7j+lRNBsgtXoIrgrLzObF/B20GfqU=</X></DSAKeyValue>";
//        private const string _testPublicKeyString = "<DSAKeyValue><P>jwKMyxKMFFeA6XTZqn0sS+S/NSVGcTMOoVTjmt+ehb1pNXOrg5tEktha7xnKv0F+KeJgc9lWsK+1AVPuG+nmuVkApRey2O2r14Ot+25YYWT1U7fjEfFj91WBrn/n7sySieJI6YhAGH1TycIxWXjOcDWfBRSGSpb/xeqo2jMBKIy52byAj7+C3aoqXsja9OCeBToahXa/ylbNukKgvXLwIB0eyWHmWrreEcoNEilDd5DWqSeK2bjbVD8S+yR38+ygUcJjEmMXXWM4qtezdgi3YGxKXeaq+3ypJUjm4ufE3t8yBSMBRgWJjGgqFVxQh48gOkvWC6OZYMISAjBuHq7epQ==</P><Q>2RgyTfMjnHWs0NxZT3YcZPQG+PhshzOVDeplOjQm1g8=</Q><G>XWLBisfiKad7LvpZM5VV2jjzMWv4ePzrnJWHHzcg0QU2cTdcdM6zNukVzHpCgUho3POr4GKAmjAvLGV1AI4SE60SdlQRbH9qvYAwEEdT2h4ucvN9/yZHxMD0zcZCJ7vEMU2Nyaiy/+4Ys9N5O5QBDoPBsoncOCFUuR5Aq7Htme+lMUKi5R/xTarmlqxhnGY+BE2mrVoe4j8rIYcN3Jzmb+cWBCgl4lywvDpBJPbQrpbnts+RyFsddnUjoaZEv3/Ywdcusmb0ieM+Vz2+xGPpWpSMFDwJ7eYKGwEkaac4XBxxq/16DZzIs4WBP46Dw5zVbQ/IBLmCQ2vFOiDMPoNwHw==</G><Y>bUECfQ6VdPp4QEZ8vSIWWoCGyjqGiwYkHA0/HJ08aEZaqgw6mXEmxXsdWigwAIVN876xBDcf9tSX8ykZl92sw9snuFxX/qKXBfYh3wwSa87zInmT2pDin6eO9qqWBQHA7grq4G1PVDNUliWrNEj4mplbRQ7ExPARBm/6iQUYYapy3ebkc30wz6w0u4LfUBxXsTsl+DCpjdA7nm8SfSkybp8Sh4JElYYAhUBlYQ2MF99+VQKxjjvk4eIXlG36HIYCKiA6WwyS0YxiU6E97W3Ay60jdQpL+nY9uh1EuCuEeua7wRIko0ajrw1cUpx4tCNGX0SP17cMLjap4bpOJPdjTQ==</Y><Seed>Si9hL7RPCYnQQurajEQ9pq1FEwbvXJXuJL84wALkoNQ=</Seed><PgenCounter>B6k=</PgenCounter></DSAKeyValue>";
//        private const LicenseType _unknownLicenseType = (LicenseType) 255;
//        private const LicensedProduct _unknownLicensedProduct = (LicensedProduct) 255;

// private static readonly DSA _testPublicKey = DSA.Create();

// static LicenseKeySerializationTests()
//        {
//            _testPublicKey.FromXmlString(_testPublicKeyString);
//        }

// public LicenseKeySerializationTests( ITestOutputHelper logger )
//            : base( logger )
//        {
//        }

// private static void TestLicenseSerialization( LicenseKeyData licenseKeyData )
//        {
//            var licenseKey = licenseKeyData.SignAndSerialize( 0, _testPrivateKeyString );
//            var deserializedLicenseKeyData = LicenseKeyData.Deserialize( licenseKey );
//            Assert.True( deserializedLicenseKeyData!.VerifySignature( _testPublicKey ) );
//        }

// [Fact]
//        public void UnknownLicenseTypeCanSerialize()
//        {
//            var license = new LicenseKeyData
//            {
//                LicenseId = 1,
//                Product = LicensedProduct.Ultimate
//            };

// license.LicenseType = _unknownLicenseType;

// TestLicenseSerialization( license );
//        }

// [Fact]
//        public void UnknownLicensedProductCanSerialize()
//        {
//            var license = new LicenseKeyData
//            {
//                LicenseId = 1,
//                LicenseType = LicenseType.PerUser
//            };

// license.Product = _unknownLicensedProduct;

// TestLicenseSerialization( license );
//        }

// private static void TestUnknownField<T>( T value )
//        {
//            var license = new LicenseKeyData
//            {
//                LicenseId = 1,
//                LicenseType = LicenseType.PerUser,
//                Product = LicensedProduct.Ultimate
//            };

// license.UnknownMustUnderstandField = value;

// TestLicenseSerialization( license );

// license = new LicenseKeyData
//            {
//                LicenseId = 1,
//                LicenseType = LicenseType.PerUser,
//                Product = LicensedProduct.Ultimate
//            };

// license.UnknownOptionalField = value;

// TestLicenseSerialization( license );
//        }

// [Fact]
//        public void UnknownBoolFieldCanSerialize()
//        {
//            TestUnknownField( false );
//            TestUnknownField( true );
//        }

// [Fact]
//        public void UnknownByteFieldCanSerialize()
//        {
//            TestUnknownField( byte.MinValue );
//            TestUnknownField<byte>( byte.MaxValue / 2 );
//            TestUnknownField( byte.MaxValue );
//        }

// [Fact]
//        public void UnknownInt16FieldCanSerialize()
//        {
//            TestUnknownField( short.MinValue );
//            TestUnknownField<short>( short.MaxValue / 2 );
//            TestUnknownField( short.MaxValue );
//        }

// [Fact]
//        public void UnknownInt32FieldCanSerialize()
//        {
//            TestUnknownField( int.MinValue );
//            TestUnknownField( int.MaxValue / 2 );
//            TestUnknownField( int.MaxValue );
//        }

// [Fact]
//        public void UnknownInt64FieldCanSerialize()
//        {
//            TestUnknownField( long.MinValue );
//            TestUnknownField( long.MaxValue / 2 );
//            TestUnknownField( long.MaxValue );
//        }

// [Fact]
//        public void UnknownDateAndDateTimeFieldCanSerialize()
//        {
//            // The License.UnknownField testing property setter
//            // sets both the date field and the datetime field.
//            TestUnknownField( DateTime.MinValue );
//            TestUnknownField( new DateTime( DateTime.MaxValue.Ticks / 2 ) );
//            TestUnknownField( DateTime.MaxValue );
//        }

// [Fact]
//        public void UnknownStringFieldCanSerialize()
//        {
//            TestUnknownField( "" );
//            TestUnknownField( "\0" );
//            TestUnknownField( "\0\0\0\0\0\0\0" );
//            TestUnknownField( " " );
//            TestUnknownField( "              " );

// // ReSharper disable CommentTypo
//            TestUnknownField( "příliš žluťoučký kůň úpěl ďábelské ódy / PŘÍLIŠ ŽLUŤOUČKÝ KŮŇ ÚPĚL ĎÁBELSKÉ ÓDY" );
//            TestUnknownField( "слишком желтоватый конь вой дьявольская ода" );

// // ReSharper restore CommentTypo
//            TestUnknownField( new string( 'x', byte.MaxValue / 2 ) );
//            TestUnknownField( new string( 'x', byte.MaxValue ) );
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

// TestUnknownField( CreateByteArray( 0 ) );
//            TestUnknownField( CreateByteArray( 1 ) );
//            TestUnknownField( CreateByteArray( 2 ) );
//            TestUnknownField( CreateByteArray( byte.MaxValue / 2 ) );
//            TestUnknownField( CreateByteArray( byte.MaxValue ) );
//        }

// [Fact]
//        public void UnknownGuidFieldCanSerialize()
//        {
//            TestUnknownField( Guid.Empty );
//            TestUnknownField( Guid.NewGuid() );
//        }
//    }
// }