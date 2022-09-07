// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Backstage.Diagnostics;

public interface IMiniDumper
{
    bool MustWrite( Exception exception );

    string? Write();
}