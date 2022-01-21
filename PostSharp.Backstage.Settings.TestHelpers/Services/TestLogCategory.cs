﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Logging;

namespace PostSharp.Backstage.Testing.Services
{
    internal class TestLogCategory : ILogCategory
    {
        public string Name => "Test";
    }
}