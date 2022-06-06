﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
    }
}