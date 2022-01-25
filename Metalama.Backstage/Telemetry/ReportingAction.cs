// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Reflection;

namespace Metalama.Backstage.Telemetry
{
    /// <exclude />
    [Obfuscation( Exclude = true, ApplyToMembers = true )]
    internal enum ReportingAction
    {
        /// <exclude />
        Ask,

        /// <exclude />
        Yes,

        /// <exclude />
        No
    }
}