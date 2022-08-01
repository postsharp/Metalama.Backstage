﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;

namespace Metalama.DotNetTools
{
    internal class MetalamaConfigApplicationInfo : ApplicationInfoBase
    {
        public MetalamaConfigApplicationInfo()
            : base( typeof(MetalamaConfigApplicationInfo).Assembly ) { }

        public override string Name => "Metalama Config";

        public override ProcessKind ProcessKind => ProcessKind.MetalamaConfig;

        public override bool IsLongRunningProcess => false;

        public override bool IsUnattendedProcess( ILoggerFactory loggerFactory ) => false;

        public override bool IsTelemetryEnabled
            =>
#if DEBUG
                false;
#else
            true;
#endif
    }
}