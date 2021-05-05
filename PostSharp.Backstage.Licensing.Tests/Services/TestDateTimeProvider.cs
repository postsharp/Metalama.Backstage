﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Tests.Services
{
    internal class TestDateTimeProvider : IDateTimeProvider
    {
        private DateTime? _now;

        public DateTime Now => this._now ?? DateTime.Now;

        public void Set( DateTime now )
        {
            this._now = now;
        }

        public void Reset()
        {
            this._now = null;
        }
    }
}
