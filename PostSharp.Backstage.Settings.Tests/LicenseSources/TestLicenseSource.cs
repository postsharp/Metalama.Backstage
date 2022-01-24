﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Consumption.Sources;
using PostSharp.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing.Tests.LicenseSources
{
    internal class TestLicenseSource : ILicenseSource, IUsable
    {
        private readonly ILicense[] _licenses;

        public string Id { get; }

        public bool IsUsed { get; set; }

        public TestLicenseSource( string id, params ILicense[] licenses )
        {
            this.Id = id;
            this._licenses = licenses;
        }

        public IEnumerable<ILicense> GetLicenses( Action<LicensingMessage> reportWarning )
        {
            this.IsUsed = true;

            return this._licenses;
        }
    }
}