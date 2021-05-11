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

        public TestConsole( ITrace trace )
        {
            this.Out = new TestStreamWriter( trace, "o>" );
            this.Error = new TestStreamWriter( trace, "e>" );
        }

        internal class TestStreamWriter : IStandardStreamWriter
        {
            private readonly StringBuilder _line = new StringBuilder();
            private readonly StringBuilder _output = new StringBuilder();
            private readonly ITrace _trace;
            private readonly string _tracePrefix;

            public TestStreamWriter( ITrace trace, string tracePrefix )
            {
                this._trace = trace;
                this._tracePrefix = tracePrefix;
            }

            public void Write( string value )
            {
                this._output.Append( value );
                this._line.Append( value );

                if ( value.EndsWith( Environment.NewLine ) )
                {
                    var traceMessage = this._line.ToString();
                    this._line.Clear();
                    traceMessage = traceMessage.Substring( 0, traceMessage.Length - Environment.NewLine.Length );
                    this._trace.WriteLine( $"{this._tracePrefix} {traceMessage}" );
                }
            }

            public override string ToString() => this._output.ToString();
        }
    }
}
