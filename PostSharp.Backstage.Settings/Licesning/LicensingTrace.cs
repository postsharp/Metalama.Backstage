// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;
using PostSharp.Extensibility.BuildTimeLogging;
using PostSharp.Sdk.Utilities;

namespace PostSharp.Backstage.Licensing
{
    [ExplicitCrossPackageInternal]
    internal class LicensingTrace
    {
        internal static BuildTimeLogger Licensing;
        internal static BuildTimeLogger LinesOfCode;

#pragma warning disable CA1810 // Initialize reference type static fields inline
        static LicensingTrace()
#pragma warning restore CA1810 // Initialize reference type static fields inline
        {
            BuildTimeLogger.WhenLoggerEnabled("Licensing", logger => Licensing = logger );
            BuildTimeLogger.WhenLoggerEnabled("LinesOfCode", logger => LinesOfCode = logger );
        }

     
    }
}