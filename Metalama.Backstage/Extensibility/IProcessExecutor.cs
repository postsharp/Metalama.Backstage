﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Diagnostics;

namespace Metalama.Backstage.Extensibility;

public interface IProcessExecutor : IBackstageService
{
    Process? Start( ProcessStartInfo startInfo );
}