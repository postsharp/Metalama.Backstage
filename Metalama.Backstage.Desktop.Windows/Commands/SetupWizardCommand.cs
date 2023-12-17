// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Tools;
using Spectre.Console.Cli;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Metalama.Backstage.Desktop.Windows.Commands;

internal class SetupWizardCommand : AsyncCommand<BaseSettings>
{
    public const string Name = "setup";

    public override async Task<int> ExecuteAsync( CommandContext context, BaseSettings settings )
    {
        // Start the web server.
        var serviceProvider = App.GetBackstageServices( settings );
        const int port = 5252;
        var webServer = serviceProvider.GetRequiredBackstageService<IBackstageToolsExecutor>().Start( BackstageTool.Worker, $"web --port {port} " );

        // Wait until the server has started.
        var baseAddress = new Uri( $"https://localhost:{port}/" );
        Console.WriteLine( baseAddress );
        var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds( 1 ) };

        var stopwatch = Stopwatch.StartNew();

        while ( !(await httpClient.GetAsync( baseAddress )).IsSuccessStatusCode )
        {
            Console.WriteLine( "Waiting for the HTTP server." );

            if ( stopwatch.Elapsed.TotalSeconds > 30 )
            {
                // Something went wrong.
                Console.WriteLine( "Cannot start the HTTP server." );

                return 1;
            }
        }

        System.Windows.Application.Current.Dispatcher.Invoke(
            () =>
            {
                var window = new WebBrowserWindow() { Url = new Uri( baseAddress, "Register" ) };
                window.ShowDialog();
                webServer.Kill();
            } );

        return 0;
    }
}