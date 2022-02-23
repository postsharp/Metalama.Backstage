// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Telemetry;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Metalama.Backstage.Licensing.Tests.Telemetry
{
    public class ExceptionSensitiveDataHelperTests
    {
        [Theory]
        [InlineData( "   at MyNamespace.MyType..ctor(MyNamespace.MyType param) in :line 16707565", "   at #user(#user param) in :line 16707565" )]
        [InlineData( "   at MyNamespace.MyType.<>c__DisplayClass11_0(MyNamespace.MyType param) in :line 16707565", "   at #user(#user param) in :line 16707565" )]
        [InlineData( "   at MyNamespace.MyType.<MethodName>d__25.MoveNext(MyNamespace.MyType param) in :line 16707565", "   at #user(#user param) in :line 16707565" )]
        [InlineData( "   at MyNamespace.MyType.MyMethod(MyNamespace.MyType param) in :line 16707565", "   at #user(#user param) in :line 16707565" )]
        [InlineData( "   at MyNamespace.MyType.MyMethod(String param) in :line 16707565", "   at #user(String param) in :line 16707565" )]
        [InlineData( "   at MyNamespace.MyType.MyMethod(PostSharp.Type param) in :line 16707565", "   at #user(PostSharp.Type param) in :line 16707565" )]
        [InlineData( "   at MyNamespace.PostSharp.MyMethod(MyNamespace.MyType param) in :line 16707565", "   at #user(#user param) in :line 16707565" )]
        [InlineData( "   at PostSharpMyNamespace.MyType.MyMethod(PostSharpMyNamespace.MyType param) in :line 16707565", "   at #user(#user param) in :line 16707565" )]
        [InlineData( "   at MetalamaMyNamespace.MyType.MyMethod(MetalamaMyNamespace.MyType param) in :line 16707565", "   at #user(#user param) in :line 16707565" )]
        [InlineData( "   at MicrosoftMyNamespace.MyType.MyMethod(MicrosoftMyNamespace.MyType param) in :line 16707565", "   at #user(#user param) in :line 16707565" )]
        [InlineData( "   at MSMyNamespace.MyType.MyMethod(MSMyNamespace.MyType param) in :line 16707565", "   at #user(#user param) in :line 16707565" )]
        [InlineData( "   at SystemMyNamespace.MyType.MyMethod(SystemMyNamespace.MyType param) in :line 16707565", "   at #user(#user param) in :line 16707565" )]
        public void SensitiveDataIsRemoved( string input, string expectedOutput )
        {
            var actualOutput = ExceptionSensitiveDataHelper.RemoveSensitiveData( input );

            Assert.Equal( expectedOutput, actualOutput );
        }

        [Theory]
        [InlineData( "   at PostSharp.Type..ctor(String param) in :line 16707565" )]
        [InlineData( "   at PostSharp.Type.<>c__DisplayClass11_0(String param) in :line 16707565" )]
        [InlineData( "   at PostSharp.Type.<MethodName>d__25.MoveNext(String param) in :line 16707565" )]
        [InlineData( "   at PostSharp.Type.Method(String message) in :line 16707565" )]
        [InlineData( "   at Metalama.Type.Method(String message) in :line 16707565" )]
        [InlineData( "   at Microsoft.Type.Method(String message) in :line 16707565" )]
        [InlineData( "   at MS.Type.Method(String message) in :line 16707565" )]
        [InlineData( "   at System.Type.Method(String message) in :line 16707565" )]
        [InlineData( "   at Presentation.Type.Method(String message) in :line 16707565" )]
        [InlineData( "   at PresentationFramerok.Type.Method(String message) in :line 16707565" )]
        [InlineData( "   at EnvDTE.Type.Method(String message) in :line 16707565" )]
        [InlineData( "   at EnvDTENamespace.Type.Method(String message) in :line 16707565" )]
        [InlineData( "   at Windows.Type.Method(String message) in :line 16707565" )]
        [InlineData( "   at WindowsNamespace.Type.Method(String message) in :line 16707565" )]
        public void NonSensitiveDataIsNotRemoved( string input )
        {
            var actualOutput = ExceptionSensitiveDataHelper.RemoveSensitiveData( input );

            Assert.Equal( input, actualOutput );
        }

        // Always solve failures of the other tests before solving failures of this one.
        [Fact]
        public void SensitiveDataIsRemovedFromMultilineInput()
        {
            IEnumerable<string[]> GetInlineData(string theoryMethodName)
            {
                var theoryMethod = this.GetType().GetMethod( theoryMethodName )!;

                return theoryMethod.GetCustomAttributes<InlineDataAttribute>()
                    .Select( a => a.GetData( theoryMethod ).Single().Cast<string>().ToArray() );
            }

            StringBuilder inputBuilder = new();
            StringBuilder expectedResultBuilder = new();

            foreach ( var data in GetInlineData( nameof( this.SensitiveDataIsRemoved ) ) )
            {
                inputBuilder.AppendLine( data[0] );
                expectedResultBuilder.AppendLine( data[1] );
            }

            foreach ( var data in GetInlineData( nameof( this.NonSensitiveDataIsNotRemoved ) ) )
            {
                inputBuilder.AppendLine( data[0] );
                expectedResultBuilder.AppendLine( data[0] );
            }

            var actualOutput = ExceptionSensitiveDataHelper.RemoveSensitiveData( inputBuilder.ToString() );

            Assert.Equal( actualOutput, expectedResultBuilder.ToString() );
        }
    }
}