// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Metalama.Backstage.Worker.WebServer;

internal class WebServerCommand : AsyncCommand<WebServerCommandSettings>
{
    public override async Task<int> ExecuteAsync( CommandContext context, WebServerCommandSettings settings )
    {
        var appData = (AppData) context.Data!;

        var builder = WebApplication.CreateBuilder( new WebApplicationOptions() { ApplicationName = "Metalama.Backstage.Worker" } );

        builder.Services.AddCors();
        builder.Services.AddControllers();
        builder.Services.AddRazorPages();

        // Inject backstage services into the ASP.NET service collection.
        foreach ( var service in appData.ServiceCollection )
        {
            builder.Services.Add( service );
        }

        builder.WebHost.ConfigureKestrel( serverOptions => serverOptions.ListenLocalhost( settings.Port, listenOptions => listenOptions.UseHttps() ) );

        // Add services to the container.
        var app = builder.Build();

        app.UseCors();
        app.UseHttpsRedirection();

        // If the program was started from the wrong directory, fix the path of static files.
        var contentRootPath = builder.Environment.ContentRootPath;

        if ( !Directory.Exists( Path.Combine( contentRootPath, "wwwroot" ) ) )
        {
            var binaryDirectory = Path.GetDirectoryName( this.GetType().Assembly.Location )!;
            contentRootPath = Path.Combine( binaryDirectory, "wwwroot" );
            app.UseStaticFiles( new StaticFileOptions() { FileProvider = new PhysicalFileProvider( contentRootPath ) } );
        }
        else
        {
            app.UseStaticFiles();
        }

        // Configure the HTTP request pipeline.
        if ( !app.Environment.IsDevelopment() )
        {
            app.UseExceptionHandler( "/Error" );

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseRouting();
        app.UseAuthorization();
        app.MapRazorPages();
        app.MapGet( "ping", KeepAlive );

        var serverTask = app.RunAsync();
        var shutDownTime = DateTime.Now.AddMinutes( 1 );

        while ( shutDownTime > DateTime.Now )
        {
            if ( serverTask.IsCompleted )
            {
                // This would happen if the server cannot start.
                await serverTask;
                
                break;
            }
            
            await Task.Delay( shutDownTime - DateTime.Now );
        }

        await app.StopAsync();

        return 0;

        void KeepAlive()
        {
            shutDownTime = DateTime.Now.AddMinutes( 1 );
        }
    }
}