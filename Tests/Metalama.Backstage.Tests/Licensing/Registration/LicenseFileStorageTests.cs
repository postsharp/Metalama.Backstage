// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Licensing.Registration
{
    public class LicenseFileStorageTests : LicensingTestsBase
    {
        public LicenseFileStorageTests( ITestOutputHelper logger )
            : base( logger ) { }

        private ParsedLicensingConfiguration OpenOrCreateStorage()
        {
            return ParsedLicensingConfiguration.OpenOrCreate( this.ServiceProvider );
        }

        private void AssertFileContains( string? expectedLicenseString )
        {
            Assert.Equal( expectedLicenseString, this.ReadStoredLicenseString() );
        }

        private void AssertStorageContains( ParsedLicensingConfiguration storage, string? expectedLicenseString )
        {
            Assert.Equal( expectedLicenseString, storage.LicenseString );

            if ( expectedLicenseString == null )
            {
                Assert.Null( storage.LicenseData );
            }
            else
            {
                if ( !this.LicenseFactory.TryCreate( expectedLicenseString, out var expectedLicense, out _ ) )
                {
                    Assert.Null( storage.LicenseData );

                    return;
                }

                if ( !expectedLicense.TryGetLicenseRegistrationData( out var expectedData, out _ ) )
                {
                    Assert.Null( storage.LicenseData );

                    return;
                }

                Assert.Equal( expectedData, storage.LicenseData );
            }
        }

        private void Add( ParsedLicensingConfiguration storage, string licenseString )
        {
            var data = this.GetLicenseRegistrationData( licenseString );
            storage.SetLicense( licenseString, data );
        }

        [Fact]
        public void ExistingStorageSucceedsToRead()
        {
            this.SetStoredLicenseString( "dummy" );
            this.AssertFileContains( "dummy" );
        }

        [Fact]
        public void NonExistentStorageCanBeRead()
        {
            var storage = this.OpenOrCreateStorage();
            this.AssertStorageContains( storage, null );
        }

        [Fact]
        public void EmptyStorageCanBeCreated()
        {
            this.OpenOrCreateStorage();
            this.AssertFileContains( null );
        }

        [Fact]
        public void NonEmptyStorageCanBeCreated()
        {
            var storage = this.OpenOrCreateStorage();
            this.Add( storage, TestLicenses.PostSharpUltimate );

            this.AssertFileContains( TestLicenses.PostSharpUltimate );
        }

        [Fact]
        public void ValidLicenseKeyCanBeRetrieved()
        {
            this.SetStoredLicenseString( TestLicenses.PostSharpUltimate );

            var storage = this.OpenOrCreateStorage();

            this.AssertStorageContains( storage, TestLicenses.PostSharpUltimate );
        }

        [Fact]
        public void InvalidLicenseKeyCanBeRetrieved()
        {
            this.SetStoredLicenseString( "dummy" );

            var storage = this.OpenOrCreateStorage();

            this.AssertStorageContains( storage, "dummy" );
            this.AssertFileContains( "dummy" );
        }

        [Fact]
        public void PreviousInvalidLicenseKeyIsReplaced()
        {
            this.SetStoredLicenseString( "dummy" );

            var storage = this.OpenOrCreateStorage();
            this.Add( storage, TestLicenses.PostSharpUltimate );

            this.AssertStorageContains( storage, TestLicenses.PostSharpUltimate );
            this.AssertFileContains( TestLicenses.PostSharpUltimate );
        }

        [Fact]
        public void PreviousValidLicenseKeysAreReplaced()
        {
            this.SetStoredLicenseString( TestLicenses.PostSharpUltimate );

            var storage = this.OpenOrCreateStorage();
            this.Add( storage, TestLicenses.MetalamaStarterPersonal );

            this.AssertStorageContains( storage, TestLicenses.MetalamaStarterPersonal );
            this.AssertFileContains( TestLicenses.MetalamaStarterPersonal );
        }
    }
}