﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Backstage.Diagnostics;

public interface ILogWriter
{
    void Log( string message );
}