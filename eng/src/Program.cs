// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Build.Solutions;
using PostSharp.Engineering.BuildTools.Dependencies.Model;
using Spectre.Console.Cli;

var product = new Product( Dependencies.MetalamaBackstage )
{
    Solutions = new Solution[]
    {
        new DotNetSolution( "Metalama.Backstage.sln" ) { SupportsTestCoverage = true, CanFormatCode = true }
    },
    PublicArtifacts = Pattern.Create(
        "Metalama.Backstage.$(PackageVersion).nupkg" ),
    Dependencies = new[] { Dependencies.PostSharpEngineering },
    Configurations = Product.DefaultConfigurations
        .WithValue( BuildConfiguration.Release, new BuildConfigurationInfo(
            MSBuildName: "Release",
            ExportsToTeamCityBuild: true ) )
};

var commandApp = new CommandApp();

commandApp.AddProductCommands( product );

return commandApp.Run( args );