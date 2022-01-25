// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Diagnostics;
using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Text;

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

        public TestConsole( IServiceProvider services )
        {
            var logger = services.GetLoggerFactory().GetLogger( "Console" );
            this.Out = new TestStreamWriter( logger, "o>" );
            this.Error = new TestStreamWriter( logger, "e>" );
        }

        public void Clear()
        {
            this.Out.Clear();
            this.Error.Clear();
        }

        internal class TestStreamWriter : IStandardStreamWriter
        {
            private readonly StringBuilder _line = new();
            private readonly StringBuilder _output = new();
            private readonly ILogger _logger;
            private readonly string _tracePrefix;

            public TestStreamWriter( ILogger logger, string tracePrefix )
            {
                this._logger = logger;
                this._tracePrefix = tracePrefix;
            }

            public void Write( string value )
            {
                this._output.Append( value );
                this._line.Append( value );

                if ( value.EndsWith( Environment.NewLine, StringComparison.Ordinal ) )
                {
                    var traceMessage = this._line.ToString();
                    this._line.Clear();
                    traceMessage = traceMessage.Substring( 0, traceMessage.Length - Environment.NewLine.Length );
                    this._logger.Trace?.Log( $"{this._tracePrefix} {traceMessage}" );
                }
            }

            public void Clear()
            {
                this._line.Clear();
                this._output.Clear();
            }

            public override string ToString()
            {
                return this._output.ToString();
            }
        }
    }
}