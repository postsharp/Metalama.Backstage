// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.DependencyInjection.Logging;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Licensing.Registration.Evaluation;
using System;
using Xunit.Abstractions;

namespace PostSharp.Cli.Tests.Commands.Licensing
{
    public abstract class LicensingCommandsTestsBase : CommandsTestsBase
    {
        protected LicensingCommandsTestsBase( ITestOutputHelper logger, Action<IServiceCollection>? serviceBuilder = null )
            : base(
                logger,
                serviceCollection =>
                {
                    serviceCollection
                        .AddDefaultService<IStandardLicenseFileLocations>()
                        .AddDefaultService<IEvaluationLicenseFilesLocations>();
                    
                    serviceBuilder?.Invoke( serviceCollection );
                } ) { }
    }
}