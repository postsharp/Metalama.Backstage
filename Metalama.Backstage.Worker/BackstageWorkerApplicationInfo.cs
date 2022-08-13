// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage
{
    internal class BackstageWorkerApplicationInfo : ApplicationInfoBase
    {
        public BackstageWorkerApplicationInfo()
            : base( typeof(BackstageWorkerApplicationInfo).Assembly ) { }

        public override string Name => "Metalama Backstage Worker";

        public override ProcessKind ProcessKind => ProcessKind.BackstageWorker;

        public override bool IsLongRunningProcess => false;

        public override bool IsUnattendedProcess( ILoggerFactory loggerFactory ) => true;

        public override bool IsTelemetryEnabled
            =>
#if DEBUG
                false;
#else
            true;
#endif
    }
}