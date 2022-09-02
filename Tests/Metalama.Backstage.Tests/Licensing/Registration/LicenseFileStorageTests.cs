// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
            ParsedLicensingConfiguration storage,
            params string[] expectedLicenseStrings )
        {
            Assert.Equal( expectedLicenseStrings.Length, storage.Licenses.Count );

            foreach ( var expectedLicenseString in expectedLicenseStrings )
            {
                Assert.True(
                    storage.TryGetRegistrationData( expectedLicenseString, out var actualData ),
                    $"License is missing: '{expectedLicenseString}'" );

                if ( !this.LicenseFactory.TryCreate( expectedLicenseString, out var expectedLicense ) )
                {
                    Assert.Null( actualData );

                    continue;
                }

                if ( !expectedLicense.TryGetLicenseRegistrationData( out var expectedData ) )
                {
                    Assert.Null( actualData );

                    continue;
                }

                Assert.Equal( expectedData, actualData );
            }
        }

        private void Add( ParsedLicensingConfiguration storage, string licenseString )
        {
            var data = this.GetLicenseRegistrationData( licenseString );
            storage.AddLicense( licenseString, data );
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

            Assert.Empty( storage.Licenses );
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
            this.Add( storage, TestLicenseKeys.Ultimate );

            this.AssertFileContains( TestLicenseKeys.Ultimate );
        }

        [Fact]
        public void ValidLicenseKeyCanBeRetrieved()
        {
            this.SetStoredLicenseStrings( TestLicenseKeys.Ultimate );

            var storage = this.OpenOrCreateStorage();

            this.AssertStorageContains( storage, TestLicenseKeys.Ultimate );
        }

        [Fact]
        public void InvalidLicenseKeyCanBeRetrieved()
        {
            this.SetStoredLicenseStrings( "dummy" );

            var storage = this.OpenOrCreateStorage();

            this.AssertStorageContains( storage, "dummy" );
        }

        [Fact]
        public void InvalidLicenseKeyIsPreserved()
        {
            this.SetStoredLicenseStrings( "dummy" );

            var storage = this.OpenOrCreateStorage();
            this.Add( storage, TestLicenseKeys.Ultimate );

            this.AssertStorageContains( storage, "dummy", TestLicenseKeys.Ultimate );
        }

        [Fact]
        public void ValidLicenseKeyCanBeAppended()
        {
            this.SetStoredLicenseStrings( TestLicenseKeys.Ultimate, TestLicenseKeys.Logging );

            var storage = this.OpenOrCreateStorage();
            this.Add( storage, TestLicenseKeys.Caching );

            this.AssertStorageContains(
                storage,
                TestLicenseKeys.Ultimate,
                TestLicenseKeys.Logging,
                TestLicenseKeys.Caching );

            this.AssertFileContains( TestLicenseKeys.Ultimate, TestLicenseKeys.Logging, TestLicenseKeys.Caching );
        }

        [Fact]
        public void NewLinesAreSkipped()
        {
            this.SetStoredLicenseStrings( "", TestLicenseKeys.Ultimate, "", TestLicenseKeys.Logging, "" );

            var storage = this.OpenOrCreateStorage();

            this.AssertStorageContains( storage, TestLicenseKeys.Ultimate, TestLicenseKeys.Logging );
        }
    }
}