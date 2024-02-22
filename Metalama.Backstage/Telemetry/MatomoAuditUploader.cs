// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Licensing.Audit;
using System;
using System.Threading.Tasks;

namespace Metalama.Backstage.Telemetry;

internal class MatomoAuditUploader : IBackstageService
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public MatomoAuditUploader( IServiceProvider serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Metrics" );
        this._httpClientFactory = serviceProvider.GetRequiredBackstageService<IHttpClientFactory>();
    }

    public async Task UploadAsync( LicenseAuditTelemetryReport report )
    {
        var http = this._httpClientFactory.Create();

        var request =
#pragma warning disable CA1307
            $"https://postsharp.matomo.cloud/matomo.php?idsite=6&rec=1&_id={report.DeviceHash:x}&dimension1={report.License.LicensedProduct}&dimension2={report.License.LicenseType}&dimension3={report.ApplicationName?.Replace( " ", "" )}&dimension4={report.AssemblyVersion?.ToString( 2 )}";
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