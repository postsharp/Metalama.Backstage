﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Tests.Services
{
    internal class TestServices : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();

        public TestDiagnosticsSink Diagnostics { get; }

        public TestDateTimeProvider Time { get; } = new();

        public TestFileSystemService FileSystem { get; } = new();

        public TestServices( TestTrace trace )
        {
            this.Diagnostics = new( trace );
            this._services.Add( typeof( IDiagnosticsSink ), this.Diagnostics );
            this._services.Add( typeof( IApplicationInfoService ), new ApplicationInfoService( false, new( 0, 1, 0 ), new( 2021, 1, 1 ) ) );
            this._services.Add( typeof( IDateTimeProvider ), this.Time );
            this._services.Add( typeof( IFileSystemService ), this.FileSystem );
        }

        public object? GetService( Type serviceType )
        {
            _ = this._services.TryGetValue( serviceType, out var service );
            return service;
        }

        public void SetService<T>( T service )
            where T : class
        {
            this._services.Add( typeof( T ), service );
        }
    }
}
