﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Testing.Services;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Testing
{
    public abstract class TestsBase
    {
        protected TestTrace Trace => this.Services.Trace;

        protected TestServices Services { get; }

        public TestsBase( ITestOutputHelper logger )
        {
            this.Services = new(logger);
        }
    }
}
