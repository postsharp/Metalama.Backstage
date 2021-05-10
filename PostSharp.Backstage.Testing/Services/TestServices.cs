// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Testing.Services
{
    public class TestServices : ServiceProvider
    {
        public TestDiagnosticsSink Diagnostics { get; }

        public TestDateTimeProvider Time { get; } = new();

        public TestEnvironment Environment { get; } = new();

        public TestFileSystem FileSystem { get; } = new();

        public TestServices( TestTrace trace )
        {
            this.Diagnostics = new( trace );
            this.SetService<IDiagnosticsSink>( this.Diagnostics );
            this.SetService<IApplicationInfoService>( new ApplicationInfoService( false, new( 0, 1, 0 ), new( 2021, 1, 1 ) ) );
            this.SetService<IDateTimeProvider>( this.Time );
            this.SetService<IEnvironment>( this.Environment );
            this.SetService<IFileSystem>( this.FileSystem );
        }

        public new void SetService<T>( T service )
            where T : notnull
        {
            base.SetService<T>( service );
        }
    }
}
