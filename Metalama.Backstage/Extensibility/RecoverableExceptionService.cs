// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Backstage.Extensibility;

internal sealed class RecoverableExceptionService : IRecoverableExceptionService
{
    public RecoverableExceptionService( IServiceProvider serviceProvider )
    {
        var environmentVariables = serviceProvider.GetRequiredBackstageService<IEnvironmentVariableProvider>();
        this.CanIgnore = environmentVariables.GetEnvironmentVariable( "IS_POSTSHARP_OWNED" ) == null;
    }

    public bool CanIgnore { get; }
}