// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
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
        public void NonexistentFileIsReported()
        {
            FileLicenseSource source = new( _licenseFilePath, this.Services, this.Trace );

            Assert.Empty( source.GetLicenses() );
            this.Services.Diagnostics.AssertNoErrors();
            Assert.Equal( "Failed to load licenses from 'postsharp.lic': Could not find file 'postsharp.lic'.", this.Services.Diagnostics.Warnings.Single() );
        }

        [Fact]
        public void EmptyFilePasses()
        {
            this.Services.FileSystem.Mock.AddFile( _licenseFilePath, new MockFileData( "" ) );

            FileLicenseSource source = new( _licenseFilePath, this.Services, this.Trace );

            Assert.Empty( source.GetLicenses() );
            this.Services.Diagnostics.AssertClean();
        }

        [Fact]
        public void OneLicenseStringPasses()
        {
            this.Services.FileSystem.Mock.AddFile( _licenseFilePath, new MockFileData( TestLicenseKeys.Ultimate ) );

            FileLicenseSource source = new( _licenseFilePath, this.Services, this.Trace );

            Assert.Equal( $"License '{TestLicenseKeys.Ultimate}'", source.GetLicenses().Single().ToString() );
            this.Services.Diagnostics.AssertClean();
        }

        [Fact]
        public void EmptyLinesSkipped()
        {
            this.Services.FileSystem.Mock.AddFile( _licenseFilePath, new MockFileDataEx( "", TestLicenseKeys.Ultimate, "", "", TestLicenseKeys.Logging, "" ) );

            FileLicenseSource source = new( _licenseFilePath, this.Services, this.Trace );

            var licenses = source.GetLicenses().ToArray();
            Assert.Equal( 2, licenses.Length );
            Assert.Equal( $"License '{TestLicenseKeys.Ultimate}'", licenses[0].ToString() );
            Assert.Equal( $"License '{TestLicenseKeys.Logging}'", licenses[1].ToString() );

            this.Services.Diagnostics.AssertClean();
        }
    }
}
