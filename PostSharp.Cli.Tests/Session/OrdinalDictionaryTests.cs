// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Testing;
using PostSharp.Cli.Session;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Cli.Tests.Session
{
    public class OrdinalDictionaryTests : TestsBase
    {
        private const string _name = "TEST";

        private OrdinalDictionary? _ordinals;

        public OrdinalDictionaryTests( ITestOutputHelper logger )
            : base( logger )
        {
        }

        private void Create()
        {
            this._ordinals = new( _name, this.Services );
        }

        private void Load()
        {
            this._ordinals = OrdinalDictionary.Load( _name, this.Services );
        }

        private void AssertContains( params (int Ordinal, string Value)[] expectedData )
        {
            Assert.Equal( expectedData.Length, this._ordinals!.Count );

            foreach ( var expectedItem in expectedData )
            {
                Assert.True( this._ordinals!.TryGetValue( expectedItem.Ordinal, out var actualValue ) );
                Assert.Equal( expectedItem.Value, actualValue );
            }
        }

        [Fact]
        public void LoadsWithNoSession()
        {
            this.Load();
            this.AssertContains();
        }

        [Fact]
        public void SavesWithNoSession()
        {
            this.Create();
            this._ordinals!.Add( 1, "one" );
            this._ordinals!.Add( 2, "two" );
            this._ordinals!.Add( 3, "three" );
            this._ordinals.Save();

            this.Load();
            this.AssertContains( (1, "one"), (2, "two"), (3, "three") );
        }

        [Fact]
        public void OverwritesExistingSession()
        {
            this.Create();
            this._ordinals!.Add( 1, "one" );
            this._ordinals!.Add( 2, "two" );
            this._ordinals!.Add( 3, "three" );
            this._ordinals.Save();

            this.Create();
            this._ordinals!.Add( 4, "four" );
            this._ordinals!.Add( 5, "five" );
            this._ordinals!.Add( 6, "six" );
            this._ordinals.Save();

            this.Load();
            this.AssertContains( (4, "four"), (5, "five"), (6, "six") );
        }
    }
}
