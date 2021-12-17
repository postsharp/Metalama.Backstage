﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Consumption.Sources;
using PostSharp.Backstage.Testing.Services;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.LicenseSources
{
    public class FileLicenseSourceTests : LicensingTestsBase
    {
        private const string _licenseFilePath = "postsharp.lic";

        public FileLicenseSourceTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public void NonexistentFileIsReported()
        {
            FileLicenseSource source = new( _licenseFilePath, this.Services );

            Assert.Empty( source.GetLicenses() );
            this.Diagnostics.AssertNoErrors();
            this.Diagnostics.AssertNoWarnings();
        }

        [Fact]
        public void EmptyFilePasses()
        {
            this.FileSystem.Mock.AddFile( _licenseFilePath, new MockFileData( "" ) );

            FileLicenseSource source = new( _licenseFilePath, this.Services );

            Assert.Empty( source.GetLicenses() );
            this.Diagnostics.AssertClean();
        }

        [Fact]
        public void OneLicenseStringPasses()
        {
            this.FileSystem.Mock.AddFile( _licenseFilePath, new MockFileData( TestLicenseKeys.Ultimate ) );

            FileLicenseSource source = new( _licenseFilePath, this.Services );

            Assert.Equal( $"License '{TestLicenseKeys.Ultimate}'", source.GetLicenses().Single().ToString() );
            this.Diagnostics.AssertClean();
        }

        [Fact]
        public void EmptyLinesSkipped()
        {
            this.FileSystem.Mock.AddFile( _licenseFilePath, new MockFileDataEx( "", TestLicenseKeys.Ultimate, "", "", TestLicenseKeys.Logging, "" ) );

            FileLicenseSource source = new( _licenseFilePath, this.Services );

            var licenses = source.GetLicenses().ToArray();
            Assert.Equal( 2, licenses.Length );
            Assert.Equal( $"License '{TestLicenseKeys.Ultimate}'", licenses[0].ToString() );
            Assert.Equal( $"License '{TestLicenseKeys.Logging}'", licenses[1].ToString() );

            this.Diagnostics.AssertClean();
        }
    }
}