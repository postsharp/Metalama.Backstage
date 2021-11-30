// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Registration;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Registration
{
    public class LicenseFileStorageTests : LicenseRegistrationTestsBase
    {
        public LicenseFileStorageTests( ITestOutputHelper logger )
            : base( logger ) { }

        private LicenseFileStorage CreateStorage()
        {
            return LicenseFileStorage.Create( LicenseFiles.UserLicenseFile, Services );
        }

        private LicenseFileStorage OpenOrCreateStorage()
        {
            return LicenseFileStorage.OpenOrCreate( LicenseFiles.UserLicenseFile, Services );
        }

        private void AssertFileContains( params string[] expectedLicenseStrings )
        {
            Assert.Equal( expectedLicenseStrings, ReadStoredLicenseStrings() );
        }

        private void AssertStorageContains( LicenseFileStorage storage, params string[] expectedLicenseStrings )
        {
            Assert.Equal( expectedLicenseStrings.Length, storage.Licenses.Count );

            foreach (var expectedLicenseString in expectedLicenseStrings)
            {
                Assert.True( storage.Licenses.TryGetValue( expectedLicenseString, out var actualData ), $"License is missing: '{expectedLicenseString}'" );

                if (!LicenseFactory.TryCreate( expectedLicenseString, out var expectedLicense ))
                {
                    Assert.Null( actualData );

                    continue;
                }

                if (!expectedLicense.TryGetLicenseRegistrationData( out var expectedData ))
                {
                    Assert.Null( actualData );

                    continue;
                }

                Assert.Equal( expectedData, actualData );
            }
        }

        private void Add( LicenseFileStorage storage, string licenseString )
        {
            var data = GetLicenseRegistrationData( licenseString );
            storage.AddLicense( licenseString, data );
        }

        [Fact]
        public void NonexistentStorageFailsToRead()
        {
            Assert.Throws<FileNotFoundException>( () => ReadStoredLicenseStrings() );
        }

        [Fact]
        public void ExistingStorageSucceedsToRead()
        {
            SetStoredLicenseStrings( "dummy" );
            AssertFileContains( "dummy" );
        }

        [Fact]
        public void EmptyStorageCanBeCreated()
        {
            var storage = OpenOrCreateStorage();
            storage.Save();

            AssertFileContains();
        }

        [Fact]
        public void ExistingStorageCanBeOverwritten()
        {
            SetStoredLicenseStrings( "dummy" );

            var storage = CreateStorage();
            storage.Save();

            AssertFileContains();
        }

        [Fact]
        public void NonEmptyStorageCanBeCreated()
        {
            var storage = OpenOrCreateStorage();
            Add( storage, TestLicenseKeys.Ultimate );
            storage.Save();

            AssertFileContains( TestLicenseKeys.Ultimate );
        }

        [Fact]
        public void ValidLicenseKeyCanBeRetrieved()
        {
            SetStoredLicenseStrings( TestLicenseKeys.Ultimate );

            var storage = OpenOrCreateStorage();

            AssertStorageContains( storage, TestLicenseKeys.Ultimate );
        }

        [Fact]
        public void InvalidLicenseKeyCanBeRetrieved()
        {
            SetStoredLicenseStrings( "dummy" );

            var storage = OpenOrCreateStorage();

            AssertStorageContains( storage, "dummy" );
        }

        [Fact]
        public void InvalidLicenseKeyIsPreserved()
        {
            SetStoredLicenseStrings( "dummy" );

            var storage = OpenOrCreateStorage();
            Add( storage, TestLicenseKeys.Ultimate );
            storage.Save();

            AssertStorageContains( storage, "dummy", TestLicenseKeys.Ultimate );
        }

        [Fact]
        public void ValidLicenseKeyCanBeAppended()
        {
            SetStoredLicenseStrings( TestLicenseKeys.Ultimate, TestLicenseKeys.Logging );

            var storage = OpenOrCreateStorage();
            Add( storage, TestLicenseKeys.Caching );
            storage.Save();

            AssertStorageContains( storage, TestLicenseKeys.Ultimate, TestLicenseKeys.Logging, TestLicenseKeys.Caching );
            AssertFileContains( TestLicenseKeys.Ultimate, TestLicenseKeys.Logging, TestLicenseKeys.Caching );
        }

        [Fact]
        public void NewLinesAreSkipped()
        {
            SetStoredLicenseStrings( "", TestLicenseKeys.Ultimate, "", TestLicenseKeys.Logging, "" );

            var storage = OpenOrCreateStorage();

            AssertStorageContains( storage, TestLicenseKeys.Ultimate, TestLicenseKeys.Logging );
        }
    }
}