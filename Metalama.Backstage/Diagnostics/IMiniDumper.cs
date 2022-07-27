// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Backstage.Diagnostics;

public interface IMiniDumper
{
    bool MustWrite( Exception exception );

    string? Write();
}