// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;
using PostSharp.Backstage.Licensing.Registration;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Registration
{
    public class LicenseFileStorageTests : LicenseRegistrationTestsBase
    {
        public LicenseFileStorageTests( ITestOutputHelper logger )
            : base( logger )
        {
        }

        private LicenseFileStorage CreateStorage() => LicenseFileStorage.Create( this.LicenseFiles.UserLicenseFile, this.Services );

        private LicenseFileStorage OpenOrCreateStorage() => LicenseFileStorage.OpenOrCreate( this.LicenseFiles.UserLicenseFile, this.Services );

        private void AssertFileContains( params string[] expectedLicenseStrings ) => Assert.Equal( expectedLicenseStrings, this.ReadStoredLicenseStrings() );

        private void AssertStorageContains( LicenseFileStorage storage, params string[] expectedLicenseStrings )
        {
            Assert.Equal( expectedLicenseStrings.Length, storage.Licenses.Count );

            foreach ( var expectedLicenseString in expectedLicenseStrings )
            {
                Assert.True( storage.Licenses.TryGetValue( expectedLicenseString, out var actualData ), $"License is missing: '{expectedLicenseString}'" );

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

        private void Add( LicenseFileStorage storage, string licenseString )
        {
            var data = this.GetLicenseRegistrationData( licenseString );
            storage.AddLicense( licenseString, data );
        }

        [Fact]
        public void NonexistentStorageFailsToRead()
        {
            Assert.Throws<FileNotFoundException>( () => this.ReadStoredLicenseStrings() );
        }

        [Fact]
        public void ExistingStorageSucceedsToRead()
        {
            this.SetStoredLicenseStrings( "dummy" );
            this.AssertFileContains( "dummy" );
        }

        [Fact]
        public void EmptyStorageCanBeCreated()
        {
            var storage = this.OpenOrCreateStorage();
            storage.Save();

            this.AssertFileContains();
        }

        [Fact]
        public void ExistingStorageCanBeOverwritten()
        {
            this.SetStoredLicenseStrings( "dummy" );
            
            var storage = this.CreateStorage();
            storage.Save();
            
            this.AssertFileContains();
        }

        [Fact]
        public void NonEmptyStorageCanBeCreated()
        {
            var storage = this.OpenOrCreateStorage();
            this.Add( storage, TestLicenseKeys.Ultimate );
            storage.Save();

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
            storage.Save();

            this.AssertStorageContains( storage, "dummy", TestLicenseKeys.Ultimate );
        }

        [Fact]
        public void ValidLicenseKeyCanBeAppended()
        {
            this.SetStoredLicenseStrings( TestLicenseKeys.Ultimate, TestLicenseKeys.Logging );

            var storage = this.OpenOrCreateStorage();
            this.Add( storage, TestLicenseKeys.Caching );
            storage.Save();

            this.AssertStorageContains( storage, TestLicenseKeys.Ultimate, TestLicenseKeys.Logging, TestLicenseKeys.Caching );
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
