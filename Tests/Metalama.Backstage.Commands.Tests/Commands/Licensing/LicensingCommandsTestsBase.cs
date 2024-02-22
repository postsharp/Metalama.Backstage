// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands.Licensing
{
    public abstract class LicensingCommandsTestsBase : CommandsTestsBase
    {
        protected LicensingCommandsTestsBase( ITestOutputHelper logger )
            : base( logger )
        {
            this.UserDeviceDetection.IsInteractiveDevice = true;
        }
    }
}