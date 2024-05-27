// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Telemetry;
using Spectre.Console.Cli;
using System;
using System.Diagnostics;

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
            if ( settings.Debug )
            {
                Debugger.Launch();
            }

            var extendedContext = new ExtendedCommandContext( context, settings, this.AddBackstageOptions );
            var logger = extendedContext.ServiceProvider.GetLoggerFactory().GetLogger( this.GetType().Name );
            logger.Info?.Log( $"Executing command {this.GetType().Name}" );

            try
            {
                // We don't report usage of commands.

                this.Execute( extendedContext, settings );
                logger.Info?.Log( $"The command succeeded." );

                return 0;
            }
            catch ( CommandException e )
            {
                extendedContext.Console.WriteError( e.Message );
                logger.Warning?.Log( $"The command returned {e.ReturnCode}: {e.Message}." );

                return e.ReturnCode;
            }
            catch ( Exception e )
            {
                try
                {
                    logger.Error?.Log( e.ToString() );
                    extendedContext.ServiceProvider.GetBackstageService<IExceptionReporter>()?.ReportException( e );
                }
                catch ( Exception reporterException )
                {
                    throw new AggregateException( e, reporterException );
                }

                throw;
            }
        }

        protected abstract void Execute( ExtendedCommandContext context, T settings );

        protected virtual BackstageInitializationOptions AddBackstageOptions( BackstageInitializationOptions options ) => options;
    }
}