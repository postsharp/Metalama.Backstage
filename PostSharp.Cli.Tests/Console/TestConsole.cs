// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Text;
using PostSharp.Backstage.Extensibility;
using Xunit;

namespace PostSharp.Cli.Tests.Console
{
    internal class TestConsole : IConsole
    {
        public TestStreamWriter Out { get; }

        public TestStreamWriter Error { get; }        

        IStandardStreamWriter IStandardOut.Out => this.Out;

        IStandardStreamWriter IStandardError.Error => this.Error;

        bool IStandardIn.IsInputRedirected => false;

        bool IStandardOut.IsOutputRedirected => false;

        bool IStandardError.IsErrorRedirected => false;

        public TestConsole(ITrace trace)
        {
            this.Out = new TestStreamWriter( trace );
            this.Error = new TestStreamWriter( trace );
        }

        internal class TestStreamWriter : IStandardStreamWriter
        {
            private readonly StringBuilder _stringBuilder = new StringBuilder();
            private readonly ITrace _trace;

            public TestStreamWriter( ITrace trace )
            {
                this._trace = trace;
            }

            public void Write( string value )
            {
                this._stringBuilder.Append( value );
                this._trace.WriteLine( value );
            }

            public override string ToString() => this._stringBuilder.ToString();
        }
    }
}
