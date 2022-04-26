// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Telemetry;
using Metalama.DotNetTools.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace Metalama.DotNetTools
{
    internal static class Program
    {
        private static async Task<int> Main( string[] args )
        {
            var servicesFactory = new CommandServiceProvider();

            try
            {
                var root = new TheRootCommand( servicesFactory );

                return await root.InvokeAsync( args );
            }
            catch ( Exception e )
            {
                try
                {
                    servicesFactory.ServiceProvider?.GetService<IExceptionReporter>()?.ReportException( e );
                }
                catch ( Exception reporterException )
                {
                    throw new AggregateException( e, reporterException );
                }

                return -1;
            }
            finally
            {
                try
                {
                    // Close logs.
                    servicesFactory.ServiceProvider?.GetLoggerFactory().Dispose();

                    // Report usage.
                    servicesFactory.ServiceProvider?.GetService<IUsageSample>()?.Flush();
                }
                catch ( Exception e )
                {
                    try
                    {
                        servicesFactory.ServiceProvider?.GetService<IExceptionReporter>()?.ReportException( e );
                    }
                    catch
                    {
                        // We don't want failing telemetry to disturb users.
                    }

                    // We don't want failing telemetry to disturb users.
                }
            }
        }
    }
}