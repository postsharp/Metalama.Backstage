﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

namespace Metalama.Backstage.Diagnostics;

public interface ILogWriter
{
    void Log( string message );
}