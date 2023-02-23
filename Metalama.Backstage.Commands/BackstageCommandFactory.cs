﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands.Configuration;
using Metalama.Backstage.Commands.Licensing;
using Metalama.Backstage.Commands.Maintenance;
using Metalama.Backstage.Commands.Telemetry;
using Spectre.Console.Cli;
using System;

namespace Metalama.Backstage.Commands;

public static class BackstageCommandFactory
{
    public static void ConfigureCommandApp(
        CommandApp app,
        BackstageCommandOptions options,
        Action<IConfigurator>? configureMoreCommands = null,
        Action<string, IConfigurator<CommandSettings>>? configureBranch = null )
    {
        app.Configure(
            appConfig =>
            {
                appConfig.AddCommand<WelcomeCommand>( "welcome" )
                    .WithData( options )
                    .WithDescription( "Executes the first-day initialization." )
                    .IsHidden();

                appConfig.AddBranch(
                    "license",
                    license =>
                    {
                        license.SetDescription( "Register or register a license key, switch to Metalama Free or Metalama Trial, analyzes credit consumption." );

                        license.AddCommand<ListLicensesCommand>( "list" )
                            .WithData( options )
                            .WithDescription( "Lists registered license." );

                        license.AddCommand<RegisterLicenseCommand>( "register" )
                            .WithData( options )
                            .WithDescription( "Registers a license key." );

                        license.AddCommand<UnregisterCommand>( "unregister" )
                            .WithData( options )
                            .WithDescription( "Unregisters a license key." );

                        license.AddCommand<RegisterTrialCommand>( "try" )
                            .WithData( options )
                            .WithDescription( "Starts the trial period." );

                        license.AddCommand<RegisterFreeCommand>( "free" )
                            .WithData( options )
                            .WithDescription( "Switches to Metalama Free." );

                        configureBranch?.Invoke( "license", license );
                    } );

                appConfig.AddBranch(
                    "config",
                    config =>
                    {
                        config.SetDescription( "Lists, edits, prints, resets configuration settings." );

                        config.AddCommand<ListConfigurationsCommand>( "list" )
                            .WithData( options )
                            .WithDescription( "Lists the supported JSON configuration files" );

                        config.AddCommand<EditConfigurationCommand>( "edit" )
                            .WithData( options )
                            .WithDescription( "Opens a given JSON configuration file in the default text editor." );

                        config.AddCommand<ResetConfigurationCommand>( "reset" )
                            .WithData( options )
                            .WithDescription( "Resets a given configuration file to its default value." );

                        config.AddCommand<PrintConfigurationCommand>( "print" )
                            .WithData( options )
                            .WithDescription( "Prints the content of the given configuration file to the console." );

                        config.AddCommand<ValidateConfigurationCommand>( "validate" )
                            .WithData( options )
                            .WithDescription( "Validates the content of the given configuration file." );

                        configureBranch?.Invoke( "config", config );
                    } );

                appConfig.AddCommand<CleanUpCommand>( "cleanup" )
                    .WithData( options )
                    .WithDescription( "Cleans up temporary files generated by Metalama." );

                appConfig.AddCommand<KillCommand>( "kill" )
                    .WithData( options )
                    .WithDescription( "Shuts down or kills processes that may be locking Metalama packages." );

                appConfig.AddBranch(
                    "telemetry",
                    telemetry =>
                    {
                        telemetry.SetDescription( "Manages the telemetry options and upload queue." );

                        telemetry.AddCommand<EnableTelemetryCommand>( "enable" ).WithData( options ).WithDescription( "Enables telemetry." );
                        telemetry.AddCommand<DisableTelemetryCommand>( "disable" ).WithData( options ).WithDescription( "Disables telemetry." );

                        telemetry.AddCommand<ResetDeviceIdCommand>( "reset-device-id" )
                            .WithDescription( "Resets the identifier that telemetry uses to uniquely identify the current device." );

                        telemetry.AddCommand<UploadTelemetryCommand>( "upload" )
                            .WithData( options )
                            .WithDescription( "Uploads all telemetry data present in the queue." );

                        telemetry.AddCommand<TelemetryStatusCommand>( "status" )
                            .WithData( options )
                            .WithDescription( "Displays the configuration and the status of telemetry." );

                        configureBranch?.Invoke( "telemetry", telemetry );
                    } );

                configureMoreCommands?.Invoke( appConfig );
            } );
    }
}