// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing
{
#pragma warning disable CA1028 // Enum Storage should be Int32
    /// <exclude />
    /// <summary>
    /// Enumeration of licensed products.
    /// </summary>
    public enum LicensedProduct : byte
    {
        /// <summary>
        /// None.
        /// </summary>
        None,

        /// <summary>
        /// PostSharp 2.0.
        /// </summary>
        [Obsolete( "This product is no longer supported." )]
        PostSharp20 = 1,

        /// <summary>
        /// PostSharp 3.0 and future versions with active subscription.
        /// </summary>
        [Obsolete( "Use Ultimate or Framework" )]
        PostSharp30 = 2,

        /// <summary>
        /// PostSharp Ultimate.
        /// </summary>
        Ultimate = 3,

        /// <summary>
        /// PostSharp Framework.
        /// </summary>
        Framework = 4,

        /// <summary>
        /// Caravela.
        /// </summary>
        Caravela = 5,

        /// <summary>
        /// Logging Library.
        /// </summary>
        DiagnosticsLibrary = 12,

        /// <summary>
        /// MVVM Library (former XAML/Model Library).
        /// </summary>
        ModelLibrary = 13,

        /// <summary>
        /// Threading Library.
        /// </summary>
        ThreadingLibrary = 14,

        /// <summary>
        /// Caching Library.
        /// </summary>
        CachingLibrary = 15,

        // 255 is reserved as unknown for testing purposes
    }
}