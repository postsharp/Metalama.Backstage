// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using System.Collections.Generic;
using System.Diagnostics;

namespace Metalama.Backstage.Testing.Services
{
    public class TestProcessService : IProcessService
    {
        private readonly List<ProcessStartInfo> _startedProcesses = new();

        public IReadOnlyList<ProcessStartInfo> StartedProcesses => this._startedProcesses;

        public void Start( ProcessStartInfo startInfo )
        {
            this._startedProcesses.Add( startInfo );
        }
    }
}