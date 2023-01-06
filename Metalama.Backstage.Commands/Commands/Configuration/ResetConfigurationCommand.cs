﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Commands.Commands.Configuration;

internal class ResetConfigurationCommand : BaseConfigurationCommand
{
    protected override void Execute( ExtendedCommandContext context, ConfigurationCommandSettings settings, ConfigurationFileCommandAdapter adapter )
        => adapter.Reset( context );
}