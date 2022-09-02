// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;

namespace Metalama.Backstage.Licensing
{
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
        PostSharpUltimate = 3,

        /// <summary>
        /// PostSharp Framework.
        /// </summary>
        PostSharpFramework = 4,

        /// <summary>
        /// Metalama Ultimate.
        /// </summary>
        MetalamaUltimate = 5,

        /// <summary>
        /// Metalama Professional.
        /// </summary>
        MetalamaProfessional = 6,

        /// <summary>
        /// Metalama Starter.
        /// </summary>
        MetalamaStarter = 7,

        /// <summary>
        /// Metalama Free.
        /// </summary>
        MetalamaFree = 8,

        /// <summary>
        /// PostSharp Logging Library.
        /// </summary>
        PostSharpLoggingLibrary = 12,

        /// <summary>
        /// PostSharp MVVM Library (former XAML/Model Library).
        /// </summary>
        PostSharpMvvmLibrary = 13,

        /// <summary>
        /// PostSharp Threading Library.
        /// </summary>
        PostSharpThreadingLibrary = 14,

        /// <summary>
        /// PostSharp Caching Library.
        /// </summary>
        PostSharpCachingLibrary = 15

        // 255 is reserved as unknown for testing purposes
    }
}