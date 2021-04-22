﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing
{
    public interface ILicenseSource
    {
        public string Id { get; }

        IEnumerable<string> LicenseKeys { get; }
    }
}
