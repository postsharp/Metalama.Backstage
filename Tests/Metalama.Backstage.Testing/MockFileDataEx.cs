// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.IO.Abstractions.TestingHelpers;

namespace Metalama.Backstage.Testing
{
    public class MockFileDataEx : MockFileData
    {
        public MockFileDataEx( string textContents )
            : base( textContents ) { }
    }
}