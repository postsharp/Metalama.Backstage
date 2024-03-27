﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Audit;
using System;
using System.Threading.Tasks;

namespace Metalama.Backstage.Telemetry;

internal class MatomoAuditUploader : IBackstageService
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RandomNumberGenerator _randomNumberGenerator;

    public MatomoAuditUploader( IServiceProvider serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Metrics" );
        this._httpClientFactory = serviceProvider.GetRequiredBackstageService<IHttpClientFactory>();
        this._randomNumberGenerator = serviceProvider.GetRequiredBackstageService<RandomNumberGenerator>();
    }

    public async Task UploadAsync( LicenseAuditTelemetryReport report, bool isNewUser )
    {
        var http = this._httpClientFactory.Create();

        var licensedProduct = report.License.LicensedProduct switch
        {
            LicensedProduct.Framework => "PostSharpFramework",
            LicensedProduct.Ultimate => "PostSharpUltimate",
            _ => report.License.LicensedProduct.ToString()
        };

        // Note that we are intentionally and "randomly" reporting the version of the first component that
        // triggered audit, because prioritize having just one hit per day over having accurate version reporting
        // (at least for Matomo reporting).
        var reportedVersion = report.AssemblyVersion?.ToString( 2 );

        var request =
#pragma warning disable CA1307
            $"https://postsharp.matomo.cloud/matomo.php?idsite=6"
            + $"&rec=1&_id={report.DeviceHash:x}"
            + $"&dimension1={licensedProduct}"
            + $"&dimension2={report.License.LicenseType}"
            + $"&dimension3=Metalama"
            + $"&dimension4={reportedVersion}"
            + $"&new_visit={(isNewUser ? 1 : 0)}"
            + $"&rand={this._randomNumberGenerator.NextInt64():x}";
#pragma warning restore CA1307

        try
        {
            var response = await http.GetAsync( request );

            if ( !response.IsSuccessStatusCode )
            {
                this._logger.Warning?.Log( $"License audit to Matomo returned {response.ReasonPhrase}." );
            }
        }
        catch ( Exception e )
        {
            this._logger.Error?.Log( $"Cannot audit to Matomo: {e.Message}" );
        }
    }
}