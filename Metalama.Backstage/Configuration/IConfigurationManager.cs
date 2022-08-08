// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Configuration
{
    public interface IConfigurationManager : IDisposable, IBackstageService
    {
        ILogger Logger { get; }

        string GetFileName( Type type );

        ConfigurationFile Get( Type type, bool ignoreCache = false );

        /// <summary>
        /// Try to update a settings file if the base revision matches the expected value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lastModified"></param>
        /// <returns></returns>
        bool TryUpdate( ConfigurationFile value, DateTime? lastModified );
    }
}