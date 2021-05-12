﻿// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System.Reflection;

namespace PostSharp.Backstage.Settings
{
    /// <exclude />
    [Obfuscation( Exclude = true, ApplyToMembers = true )]
    // TODO: This used to be internal.
    public enum ReportingAction
    {
        /// <exclude />
        Ask,
        /// <exclude />
        Yes,
        /// <exclude />
        No
    }
}