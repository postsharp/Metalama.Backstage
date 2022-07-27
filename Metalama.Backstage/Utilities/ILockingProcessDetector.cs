// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System.Collections.Generic;
using System.Diagnostics;

namespace Metalama.Backstage.Utilities;

public interface ILockingProcessDetector
{
    IReadOnlyList<Process> GetProcessesUsingFiles( IReadOnlyList<string> filePaths );
}