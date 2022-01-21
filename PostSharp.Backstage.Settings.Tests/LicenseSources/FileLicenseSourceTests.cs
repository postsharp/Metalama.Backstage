// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Consumption.Sources;
using System.IO.Abstractions.TestingHelpers;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.LicenseSources
{
    public class FileLicenseSourceTests : LicensingTestsBase
    {
        private const string _licenseFilePath = "licensing.json";

        public FileLicenseSourceTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Fact]
        public void NonexistentFileIsReported()
        {
            FileLicenseSource source = new( this.Services );

            Assert.Empty( source.GetLicenses() );
            this.Diagnostics.AssertNoErrors();
            this.Diagnostics.AssertNoWarnings();
        }

        [Fact]
        public void EmptyFilePasses()
        {
            this.FileSystem.Mock.AddFile( _licenseFilePath, new MockFileData( "" ) );

            FileLicenseSource source = new( this.Services );

            Assert.Empty( source.GetLicenses() );
            this.Diagnostics.AssertClean();
        }
    }
}