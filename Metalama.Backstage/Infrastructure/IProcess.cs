﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Backstage.Infrastructure;

public interface IProcess : IDisposable
{
    event Action Exited;

    bool HasExited { get; }
}