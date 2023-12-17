// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Program;

namespace Metalama.Backstage.Tools;

public static class BackstageProgramsExtensions
{
    public static ServiceProviderBuilder AddPrograms( this ServiceProviderBuilder builder )
    {
        builder.AddService( typeof(IWorkerProgram), serviceProvider => new WorkerProgram( serviceProvider ) );

        return builder;
    }
}