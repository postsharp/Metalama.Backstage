// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
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

        IStandardStreamWriter IStandardOut.Out => Out;

        IStandardStreamWriter IStandardError.Error => Error;

        bool IStandardIn.IsInputRedirected => false;

        bool IStandardOut.IsOutputRedirected => false;

        bool IStandardError.IsErrorRedirected => false;

        public TestConsole( IServiceProvider services )
        {
            var logger = services.GetOptionalTraceLogger<TestConsole>()!;
            Out = new TestStreamWriter( logger, "o>" );
            Error = new TestStreamWriter( logger, "e>" );
        }

        public void Clear()
        {
            Out.Clear();
            Error.Clear();
        }

        internal class TestStreamWriter : IStandardStreamWriter
        {
            private readonly StringBuilder _line = new();
            private readonly StringBuilder _output = new();
            private readonly ILogger _logger;
            private readonly string _tracePrefix;

            public TestStreamWriter( ILogger logger, string tracePrefix )
            {
                _logger = logger;
                _tracePrefix = tracePrefix;
            }

            public void Write( string value )
            {
                _output.Append( value );
                _line.Append( value );

                if (value.EndsWith( Environment.NewLine, StringComparison.Ordinal ))
                {
                    var traceMessage = _line.ToString();
                    _line.Clear();
                    traceMessage = traceMessage.Substring( 0, traceMessage.Length - Environment.NewLine.Length );
                    _logger.LogTrace( $"{_tracePrefix} {traceMessage}" );
                }
            }

            public void Clear()
            {
                _line.Clear();
                _output.Clear();
            }

            public override string ToString()
            {
                return _output.ToString();
            }
        }
    }
}