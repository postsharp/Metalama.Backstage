// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing
{
    internal abstract class LicenseReader : MarshalByRefObject
    {
        public abstract string[] ReadLicenses();
    }
}