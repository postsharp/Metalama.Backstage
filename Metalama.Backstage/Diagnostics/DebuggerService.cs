// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Diagnostics;

namespace Metalama.Backstage.Diagnostics
{
    internal static class DebuggerService
    {
        private static readonly object _attachDebuggerSync = new();
        private static volatile bool _attachDebuggerRequested;

        public static void Launch( DiagnosticsConfiguration configuration, ProcessKind processKind )
        {
            if ( configuration.Debugger.Processes.TryGetValue( processKind, out var enabled ) && enabled )
            {
                lock ( _attachDebuggerSync )
                {
                    if ( !_attachDebuggerRequested )
                    {
                        // We try to request to attach the debugger a single time, even if the user refuses or if the debugger gets
                        // detached. It makes a better debugging experience.
                        _attachDebuggerRequested = true;

                        if ( !Debugger.IsAttached )
                        {
                            Debugger.Launch();
                        }
                    }
                }
            }
        }
    }
}