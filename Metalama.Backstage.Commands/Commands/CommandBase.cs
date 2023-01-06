// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Spectre.Console.Cli;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Backstage.Commands.Commands
{
    internal abstract class CommandBase<T> : Command<T>
        where T : CommonCommandSettings
    {
        public sealed override int Execute( [NotNull] CommandContext context, [NotNull] T settings )
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