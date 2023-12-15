// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
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
        app.UseStaticFiles();

        // Configure the HTTP request pipeline.
        if ( !app.Environment.IsDevelopment() )
        {
            app.UseExceptionHandler( "/Error" );

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthorization();
        app.MapRazorPages();

        await app.RunAsync();

        return 0;
    }
}