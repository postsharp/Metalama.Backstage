// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Licensing.Registration;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.Registration
{
    public class LicenseFileStorageTests : LicenseRegistrationTestsBase
    {
        public LicenseFileStorageTests( ITestOutputHelper logger )
            : base( logger ) { }

        private ParsedLicensingConfiguration OpenOrCreateStorage()
        {
            return ParsedLicensingConfiguration.OpenOrCreate( this.ServiceProvider );
        }

        private void AssertFileContains( params string[] expectedLicenseStrings )
        {
            Assert.Equal( expectedLicenseStrings, this.ReadStoredLicenseStrings() );
        }

        private void AssertStorageContains(
            ParsedLicensingConfiguration storage, string? expectedLicenseString )
        {
            Assert.Equal( expectedLicenseString, storage.LicenseString );

            if ( expectedLicenseString == null )
            {
                Assert.Null( storage.LicenseData );
            }
            else
            {
                if ( !this.LicenseFactory.TryCreate( expectedLicenseString, out var expectedLicense ) )
                {
                    Assert.Null( storage.LicenseData );

                    return;
                }

                if ( !expectedLicense.TryGetLicenseRegistrationData( out var expectedData ) )
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
            storage.StoreLicense( licenseString, data );
        }

        [Fact]
        public void ExistingStorageSucceedsToRead()
        {
            this.SetStoredLicenseStrings( "dummy" );
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
            this.AssertFileContains();
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
            this.SetStoredLicenseStrings( TestLicenses.PostSharpUltimate );

            var storage = this.OpenOrCreateStorage();

            this.AssertStorageContains( storage, TestLicenses.PostSharpUltimate );
        }

        [Fact]
        public void InvalidLicenseKeyCanBeRetrieved()
        {
            this.SetStoredLicenseStrings( "dummy" );

            var storage = this.OpenOrCreateStorage();

            this.AssertStorageContains( storage, "dummy" );
            this.AssertFileContains( "dummy" );
        }

        [Fact]
        public void PreviousInvalidLicenseKeyIsReplaced()
        {
            this.SetStoredLicenseStrings( "dummy" );

            var storage = this.OpenOrCreateStorage();
            this.Add( storage, TestLicenses.PostSharpUltimate );

            this.AssertStorageContains( storage, TestLicenses.PostSharpUltimate );
            this.AssertFileContains( TestLicenses.PostSharpUltimate );
        }

        [Fact]
        public void PreviousValidLicenseKeysAreReplaced()
        {
            this.SetStoredLicenseStrings( TestLicenses.PostSharpUltimate, TestLicenses.PostSharpFramework );

            var storage = this.OpenOrCreateStorage();
            this.Add( storage, TestLicenses.MetalamaStarterPersonal );

            this.AssertStorageContains( storage, TestLicenses.MetalamaStarterPersonal );
            this.AssertFileContains( TestLicenses.MetalamaStarterPersonal );
        }

        [Fact]
        public void NewLinesAreSkipped()
        {
            this.SetStoredLicenseStrings( TestLicenses.MetalamaProfessionalBusiness, "", TestLicenses.PostSharpUltimate, "", TestLicenses.MetalamaUltimateBusiness, "" );

            var storage = this.OpenOrCreateStorage();

            this.AssertStorageContains( storage, TestLicenses.MetalamaProfessionalBusiness );
        }
    }
}