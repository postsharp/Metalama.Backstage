// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
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
                extendedContext.Console.WriteError( e.ToString() );

                return 2;
            }
        }

        protected abstract void Execute( ExtendedCommandContext context, T settings );
    }
}