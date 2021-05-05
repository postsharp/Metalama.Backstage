// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO.Abstractions.TestingHelpers;
using System.Text;

namespace PostSharp.Backstage.Licensing.Tests.Services
{
    public class MockFileDataEx : MockFileData
    {
        public MockFileDataEx( string textContents )
            : base( textContents )
        {
        }

        public MockFileDataEx( byte[] contents )
            : base( contents )
        {
        }

        public MockFileDataEx( MockFileData template )
            : base( template )
        {
        }

        public MockFileDataEx( string textContents, Encoding encoding )
            : base( textContents, encoding )
        {
        }

        public MockFileDataEx( params string[] content )
            : this( string.Join( Environment.NewLine, content ) )
        {
        }
    }
}
