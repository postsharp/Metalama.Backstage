// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Diagnostics;

namespace Metalama.Backstage.Extensibility
{
    internal class ProcessService : IProcessService
    {
        public void Start( ProcessStartInfo startInfo )
        {
            Process.Start( startInfo );
        }
    }
}