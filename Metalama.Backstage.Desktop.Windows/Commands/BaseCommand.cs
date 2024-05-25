﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Spectre.Console.Cli;
using System;

namespace Metalama.Backstage.Desktop.Windows.Commands;

public abstract class BaseCommand<T> : Command<T>
    where T : BaseSettings
{
    public override int Execute( CommandContext context, T settings )
    {
        var serviceProvider = App.GetBackstageServices( settings );
        var loggerFactory = serviceProvider.GetLoggerFactory();
        var logger = loggerFactory.GetLogger( this.GetType().Name );
        logger.Info?.Log( $"Executing command {this.GetType().Name}" );

        try
        {
            var result = this.Execute( new ExtendedCommandContext( context, serviceProvider, logger ), settings );
            logger.Info?.Log( $"The command returned {result}." );

            return result;
        }
        catch ( Exception e )
        {
            logger.Error?.Log( e.ToString() );

            throw;
        }
    }

    protected abstract int Execute( ExtendedCommandContext context, T settings );
}