﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing.Services;
using System;

namespace Metalama.Backstage.Licensing.Tests.Licensing;

internal static class TestLicensingConfigurationHelpers
{
    public static string? ReadStoredLicenseString( IServiceProvider serviceProvider )
        => serviceProvider.GetRequiredBackstageService<IConfigurationManager>().Get<LicensingConfiguration>().License;

    public static void SetStoredLicenseString( IServiceProvider serviceProvider, string licenseString )
    {
        var configuration = new LicensingConfiguration { License = licenseString };

        ((TestFileSystem) serviceProvider.GetRequiredBackstageService<IFileSystem>()).Mock.AddFile(
            serviceProvider.GetRequiredBackstageService<IConfigurationManager>().GetFilePath<LicensingConfiguration>(),
            new MockFileDataEx( configuration.ToJson() ) );
    }
}