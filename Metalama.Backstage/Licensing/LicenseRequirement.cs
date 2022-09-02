// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;

namespace Metalama.Backstage.Licensing
{
    /// <summary>
    /// License requirement coresponding to Metalama products.
    /// </summary>
    [Flags]
    public enum LicenseRequirement : int
    {
        None = 0,
        Free = 1 << 1,
        Starter = Free | (1 << 2),
        Professional = Starter | (1 << 3),
        Ultimate = Professional | (1 << 4),
        All = int.MaxValue
    }
}