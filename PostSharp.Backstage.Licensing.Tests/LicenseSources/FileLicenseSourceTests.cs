// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Sources;
using PostSharp.Backstage.Licensing.Tests.Services;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.LicenseSources
{
    public class FileLicenseSourceTests : LicensingTestsBase
    {
        private const string _licenseFilePath = "postsharp.lic";

        public FileLicenseSourceTests( ITestOutputHelper logger )
            : base( logger )
        {
        }

        [Fact]
        public void NotExistingFileIsReported()
        {
            MockFileSystem fileSystemMock = new();
            TestFileSystemService fileSystem = new( fileSystemMock );
            this.Services.SetService<IFileSystemService>( fileSystem );

            FileLicenseSource source = new( _licenseFilePath, this.Services, this.Trace );

            Assert.Empty( source.GetLicenses() );
            this.Services.Diagnostics.AssertNoErrors();
            Assert.Equal( "Failed to load licenses from 'postsharp.lic': Could not find file 'postsharp.lic'.", this.Services.Diagnostics.Warnings.Single() );
        }

        [Fact]
        public void EmptyFilePasses()
        {
            MockFileSystem fileSystemMock = new();
            fileSystemMock.AddFile( _licenseFilePath, new MockFileData( "" ) );
            TestFileSystemService fileSystem = new( fileSystemMock );
            this.Services.SetService<IFileSystemService>( fileSystem );

            FileLicenseSource source = new( _licenseFilePath, this.Services, this.Trace );

            Assert.Empty( source.GetLicenses() );
            this.Services.Diagnostics.AssertClean();
        }

        [Fact]
        public void OneLicenseStringPasses()
        {
            MockFileSystem fileSystemMock = new();
            fileSystemMock.AddFile( _licenseFilePath, new MockFileData( TestLicenseKeys.Ultimate ) );
            TestFileSystemService fileSystem = new( fileSystemMock );
            this.Services.SetService<IFileSystemService>( fileSystem );

            FileLicenseSource source = new( _licenseFilePath, this.Services, this.Trace );

            Assert.Equal( $"License '{TestLicenseKeys.Ultimate}'", source.GetLicenses().Single().ToString() );
            this.Services.Diagnostics.AssertClean();
        }

        [Fact]
        public void EmptyLinesSkipped()
        {
            var nl = Environment.NewLine;

            MockFileSystem fileSystemMock = new();
            fileSystemMock.AddFile( _licenseFilePath, new MockFileData( $"{nl}{TestLicenseKeys.Ultimate}{nl}{nl}{TestLicenseKeys.Logging}{nl}" ) );
            TestFileSystemService fileSystem = new( fileSystemMock );
            this.Services.SetService<IFileSystemService>( fileSystem );

            FileLicenseSource source = new( _licenseFilePath, this.Services, this.Trace );

            var licenses = source.GetLicenses().ToArray();
            Assert.Equal( 2, licenses.Length );
            Assert.Equal( $"License '{TestLicenseKeys.Ultimate}'", licenses[0].ToString() );
            Assert.Equal( $"License '{TestLicenseKeys.Logging}'", licenses[1].ToString() );

            this.Services.Diagnostics.AssertClean();
        }
    }
}
