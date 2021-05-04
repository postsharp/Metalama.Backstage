// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Licensing.Tests.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Registration
{
    public class LicenseFileStorageTests : LicenseRegistrationTestsBase
    {
        private const string _path = "caravela.lic";

        public LicenseFileStorageTests( ITestOutputHelper logger )
            : base( logger )
        {
        }

        private string[] ReadStoredLicenseStrings()
        {
            var fileSystem = this.Services.GetService<IFileSystemService>();
            var content = fileSystem.ReadAllLines( _path );
            return content;
        }

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
            MockFileSystem fileSystemMock = new();
            TestFileSystemService fileSystem = new( fileSystemMock );
            this.Services.SetService<IFileSystemService>( fileSystem );

            Assert.Throws<FileNotFoundException>( () => this.ReadStoredLicenseStrings() );
        }

        [Fact]
        public void ExistingStorageSucceedsToRead()
        {
            MockFileSystem fileSystemMock = new();
            fileSystemMock.AddFile( _path, new MockFileData( "dummy" ) );
            TestFileSystemService fileSystem = new( fileSystemMock );
            this.Services.SetService<IFileSystemService>( fileSystem );

            this.AssertFileContains( "dummy" );
        }

        [Fact]
        public void EmptyStorageCanBeCreated()
        {
            MockFileSystem fileSystemMock = new();
            TestFileSystemService fileSystem = new( fileSystemMock );
            this.Services.SetService<IFileSystemService>( fileSystem );

            var storage = LicenseFileStorage.OpenOrCreate( _path, this.Services, this.Trace );
            storage.Save();

            this.AssertFileContains();
        }

        [Fact]
        public void ExistingStorageCanBeOverwritten()
        {
            MockFileSystem fileSystemMock = new();
            fileSystemMock.AddFile( _path, new MockFileData( "dummy" ) );
            TestFileSystemService fileSystem = new( fileSystemMock );
            this.Services.SetService<IFileSystemService>( fileSystem );

            var storage = LicenseFileStorage.Create( _path, this.Services, this.Trace );
            storage.Save();
            
            this.AssertFileContains();
        }

        [Fact]
        public void NonEmptyStorageCanBeCreated()
        {
            MockFileSystem fileSystemMock = new();
            TestFileSystemService fileSystem = new( fileSystemMock );
            this.Services.SetService<IFileSystemService>( fileSystem );

            var storage = LicenseFileStorage.OpenOrCreate( _path, this.Services, this.Trace );
            this.Add( storage, TestLicenseKeys.Ultimate );
            storage.Save();

            this.AssertFileContains( TestLicenseKeys.Ultimate );
        }

        [Fact]
        public void ValidLicenseKeyCanBeRetrieved()
        {
            MockFileSystem fileSystemMock = new();
            TestFileSystemService fileSystem = new( fileSystemMock );
            fileSystemMock.AddFile( _path, new MockFileData( TestLicenseKeys.Ultimate ) );
            this.Services.SetService<IFileSystemService>( fileSystem );

            var storage = LicenseFileStorage.OpenOrCreate( _path, this.Services, this.Trace );

            this.AssertStorageContains( storage, TestLicenseKeys.Ultimate );
        }

        [Fact]
        public void InvalidLicenseKeyCanBeRetrieved()
        {
            MockFileSystem fileSystemMock = new();
            TestFileSystemService fileSystem = new( fileSystemMock );
            fileSystemMock.AddFile( _path, new MockFileData( "dummy" ) );
            this.Services.SetService<IFileSystemService>( fileSystem );

            var storage = LicenseFileStorage.OpenOrCreate( _path, this.Services, this.Trace );

            this.AssertStorageContains( storage, "dummy" );
        }

        [Fact]
        public void ValidLicenseKeyCanBeAppended()
        {
            MockFileSystem fileSystemMock = new();
            TestFileSystemService fileSystem = new( fileSystemMock );
            fileSystemMock.AddFile( _path, new MockFileData( $"{TestLicenseKeys.Ultimate}{Environment.NewLine}{TestLicenseKeys.Logging}" ) );
            this.Services.SetService<IFileSystemService>( fileSystem );

            var storage = LicenseFileStorage.OpenOrCreate( _path, this.Services, this.Trace );
            this.Add( storage, TestLicenseKeys.Caching );
            storage.Save();

            this.AssertStorageContains( storage, TestLicenseKeys.Ultimate, TestLicenseKeys.Logging, TestLicenseKeys.Caching );
            this.AssertFileContains( TestLicenseKeys.Ultimate, TestLicenseKeys.Logging, TestLicenseKeys.Caching );
        }

        [Fact]
        public void NewLinesAreSkipped()
        {
            MockFileSystem fileSystemMock = new();
            TestFileSystemService fileSystem = new( fileSystemMock );
            fileSystemMock.AddFile( _path, new MockFileData( $"{Environment.NewLine}{TestLicenseKeys.Ultimate}{Environment.NewLine}{Environment.NewLine}{TestLicenseKeys.Logging}{Environment.NewLine}" ) );
            this.Services.SetService<IFileSystemService>( fileSystem );

            var storage = LicenseFileStorage.OpenOrCreate( _path, this.Services, this.Trace );

            this.AssertStorageContains( storage, TestLicenseKeys.Ultimate, TestLicenseKeys.Logging );
        }
    }
}
