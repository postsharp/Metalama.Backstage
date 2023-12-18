﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Telemetry;
using Spectre.Console.Cli;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Metalama.Backstage.Commands;

[PublicAPI]
[UsedImplicitly( ImplicitUseTargetFlags.WithInheritors )]
public abstract class BaseAsyncCommand<T> : AsyncCommand<T>
    where T : BaseCommandSettings
{
#pragma warning disable CS8765
    public override async Task<int> ExecuteAsync( CommandContext context, T settings )
#pragma warning restore CS8765
    {
        if ( settings.Debug )
        {
            Debugger.Launch();
        }

        var extendedContext = new ExtendedCommandContext( context, settings, this.AddBackstageOptions );
        var canIgnoreRecoverableExceptions = extendedContext.ServiceProvider.GetRequiredBackstageService<IRecoverableExceptionService>().CanIgnore;

        try
        {
            // We don't report usage of commands.

            await this.ExecuteAsync( extendedContext, settings );

            return 0;
        }
        catch ( CommandException e )
        {
            extendedContext.Console.WriteError( e.Message );

            return e.ReturnCode;
        }
        catch ( Exception e )
        {
            try
            {
                extendedContext.ServiceProvider.GetBackstageService<IExceptionReporter>()?.ReportException( e );
            }
            catch ( Exception reporterException )
            {
                throw new AggregateException( e, reporterException );
            }

            throw;
        }
        finally
        {
            try
            {
                // Close logs.
                // Logging has to be disposed as the last one, so it could be used until now.
                extendedContext.ServiceProvider.GetLoggerFactory().Dispose();
            }
            catch ( Exception e )
            {
                try
                {
                    extendedContext.ServiceProvider.GetBackstageService<IExceptionReporter>()?.ReportException( e );
                }
                catch when ( canIgnoreRecoverableExceptions )
                {
                    // We don't want failing telemetry to disturb users.
                }

                // We don't want failing telemetry to disturb users.
                if ( !canIgnoreRecoverableExceptions )
                {
                    throw;
                }
            }
        }
    }

    protected abstract Task ExecuteAsync( ExtendedCommandContext context, T settings );

    protected virtual BackstageInitializationOptions AddBackstageOptions( BackstageInitializationOptions options ) => options;
}