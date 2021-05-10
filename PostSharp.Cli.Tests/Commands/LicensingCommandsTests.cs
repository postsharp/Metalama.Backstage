// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Cli.Tests.Commands
{
    public class LicensingCommandsTests : CommandsTestsBase
    {
        private const string _testLicenseKey = "2-ZTQQQQQQQT2ZTQQQQQQQQQQQQQAEGDAFA6KQ7ADVA6JE7BDV62EX37EVVJ8KZSQSKCLTQFGSFVSM8Y4RHVASNSYYC2GXGBB82AEG5YD6HEH2Z5Y8QBFP2HZPXQGKLTET4QQWANS3P3";

        public LicensingCommandsTests( ITestOutputHelper logger )
                    : base( logger )
        {
        }

        [Fact]
        public async Task CleanEnvironmentShowsNoLicensesAsync()
        {
            await this.TestCommandAsync( "license list", "" );
        }

        [Fact]
        public async Task OneLicenseKeyListedAfterRegistration()
        {
            await this.TestCommandAsync( $"license register {_testLicenseKey}", "" );
            await this.TestCommandAsync( "license list", "" );
        }
    }
}
