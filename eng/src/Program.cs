// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Build.Solutions;
using PostSharp.Engineering.BuildTools.Dependencies.Definitions;
using PostSharp.Engineering.BuildTools.Utilities;
using Spectre.Console.Cli;
using MetalamaDependencies = PostSharp.Engineering.BuildTools.Dependencies.Definitions.MetalamaDependencies.V2024_2;

var product = new Product( MetalamaDependencies.MetalamaBackstage )
{
    Solutions =
    [
        new DotNetSolution( "Metalama.Backstage.sln" ) { SupportsTestCoverage = true, CanFormatCode = true }
    ],
    PublicArtifacts = Pattern.Create(
        "Metalama.Backstage.$(PackageVersion).nupkg",
        "Metalama.Backstage.Tools.$(PackageVersion).nupkg"), // Required by Metalama.Testing.AspectTesting via Metalama.Framework.Engine.
    Dependencies = [DevelopmentDependencies.PostSharpEngineering]
};

product.PrepareCompleted += args => TestLicenseKeyDownloader.Download( args.Context );

var commandApp = new CommandApp();

commandApp.AddProductCommands( product );

return commandApp.Run( args );