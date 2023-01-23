﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Spectre.Console.Cli;
using System;

namespace Metalama.Backstage.Commands
{
    [PublicAPI]
    [UsedImplicitly( ImplicitUseTargetFlags.WithInheritors )]
    public abstract class BaseCommand<T> : Command<T>
        where T : BaseCommandSettings
    {
#pragma warning disable CS8765
        public sealed override int Execute( CommandContext context, [System.Diagnostics.CodeAnalysis.NotNull] T settings )
#pragma warning restore CS8765
        {
            var extendedContext = new ExtendedCommandContext( context, settings );

            try
            {
                this.Execute( extendedContext, settings );

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

                return -1;
            }
            finally
            {
                try
                {
                    // Report usage.
                    extendedContext.ServiceProvider.GetBackstageService<IUsageReporter>()?.StopSession();

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
                    catch
                    {
                        // We don't want failing telemetry to disturb users.
                    }

                    // We don't want failing telemetry to disturb users.
                }
            }
        }

        protected abstract void Execute( ExtendedCommandContext context, T settings );
    }
}