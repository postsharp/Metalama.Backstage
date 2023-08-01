// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using System;

#if NETFRAMEWORK || NET6_0_OR_GREATER
using JetBrains.Profiler.SelfApi;
using Metalama.Backstage.Maintenance;
#endif

namespace Metalama.Backstage.Diagnostics;

internal class ProfilingService : IBackstageService
{
    private readonly ProcessKind _options;
    private readonly DiagnosticsConfiguration _configuration;
    private readonly ILogger _logger;

#if NETFRAMEWORK || NET6_0_OR_GREATER
    private readonly ITempFileManager _tempFileManager;
#endif
    
    public ProfilingService( IServiceProvider serviceProvider, ProcessKind options )
    {
        this._options = options;
        this._configuration = serviceProvider.GetRequiredBackstageService<IConfigurationManager>().Get<DiagnosticsConfiguration>();
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Profiling" );
        
#if NETFRAMEWORK || NET6_0_OR_GREATER
        this._tempFileManager = serviceProvider.GetRequiredBackstageService<ITempFileManager>();
#endif
    }

    public void Initialize()
    {
        if ( this._configuration.Profiling.Processes.TryGetValue( this._options.ToString(), out var enabled ) && enabled )
        {
            // The implementation is intentionally in a different method to avoid the loading of JetBrains assemblies if profiling
            // is not requested.
            this.StartProfiler();
        }
    }

    private void StartProfiler()
    {
#if NETFRAMEWORK || NET6_0_OR_GREATER
            var directory = this._tempFileManager.GetTempDirectory( "Profiling", CleanUpStrategy.WhenUnused );

            this._logger.Warning?.Log( $"Starting the profiler. Data will be stored in '{directory}'." );

            DotTrace.EnsurePrerequisite();
            DotTrace.Attach( new DotTrace.Config().SaveToDir( directory ) );
            DotTrace.StartCollectingData();
            AppDomain.CurrentDomain.DomainUnload += ( sender, args ) => DotTrace.Detach();
#else
        this._logger.Warning?.Log( $"The profiler was not started because this is the .NET Standard 2.0 build of Metalama.Backstage'." );
#endif
    }
}