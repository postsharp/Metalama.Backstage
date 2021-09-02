// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Provides current date and time using <see cref="DateTime.Now" />.
    /// </summary>
    internal class CurrentDateTimeProvider : IDateTimeProvider
    {
        /// <summary>
        /// Gets current date and time using <see cref="DateTime.Now" />.
        /// </summary>
        public DateTime Now => DateTime.Now;
    }
}