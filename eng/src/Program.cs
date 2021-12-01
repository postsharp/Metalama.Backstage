// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build.Model;
using Spectre.Console.Cli;
using System.Collections.Immutable;

namespace BuildCaravela
{
    internal class Program
    {
        private static int Main( string[] args )
        {
            var privateSource = new NugetSource( "%INTERNAL_NUGET_PUSH_URL%", "%INTERNAL_NUGET_API_KEY%" );
            var publicSource = new NugetSource( "https://api.nuget.org/v3/index.json", "%NUGET_ORG_API_KEY%" );

            // These packages are published to internal and private feeds.
            var publicPackages = new ParametricString[]
            {
                "PostSharp.Backstage.Settings.$(PackageVersion).nupkg",
                "PostSharp.Cli.$(PackageVersion).nupkg"
            };

            var publicPublishing = new NugetPublishTarget(
                Pattern.Empty.Add( publicPackages ),
                privateSource,
                publicSource );

            // Currently we have no private package in this product.

            var product = new Product
            {
                ProductName = "PostSharp.Backstage.Settings",
                Solutions = ImmutableArray.Create<Solution>(
                    new DotNetSolution( "PostSharp.Backstage.Settings.sln" ) { SupportsTestCoverage = true, CanFormatCode = true } ),
                PublishingTargets = ImmutableArray.Create<PublishingTarget>( publicPublishing ),
                Dependencies = ImmutableArray<ProductDependency>.Empty
            };
            var commandApp = new CommandApp();
            commandApp.AddProductCommands( product );

            return commandApp.Run( args );
        }
    }
}