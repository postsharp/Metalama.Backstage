// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;
using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Dependencies.Model;
using Spectre.Console.Cli;

var product = new Product
{
    ProductName = "PostSharp.Backstage.Settings",
    Solutions = ImmutableArray.Create<Solution>(
                    new DotNetSolution("PostSharp.Backstage.Settings.sln") { SupportsTestCoverage = true, CanFormatCode = true }),
    PublicArtifacts = Pattern.Create(
        "PostSharp.Backstage.Settings.$(PackageVersion).nupkg",
        "PostSharp.Cli.$(PackageVersion).nupkg"),
    Dependencies = ImmutableArray.Create(
        Dependencies.PostSharpEngineering)
};

var commandApp = new CommandApp();

commandApp.AddProductCommands(product);

return commandApp.Run(args);