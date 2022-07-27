// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;

namespace Metalama.Backstage.Extensibility
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