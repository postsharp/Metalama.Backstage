// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using System.Threading.Tasks;

namespace Metalama.Backstage
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var serviceProviderBuilder = new ServiceProviderBuilder()
                .AddStandardDirectories();

            var services = serviceProviderBuilder.ServiceProvider;

            var uploader = new TelemetryUploader( services );

            await uploader.UploadAsync();
        }
    }
}