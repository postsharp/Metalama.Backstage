// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Configuration
{
    public interface IConfigurationManager : IDisposable
    {
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