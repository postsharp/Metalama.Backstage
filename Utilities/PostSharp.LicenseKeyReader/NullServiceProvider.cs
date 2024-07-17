// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace PostSharp.LicenseKeyReader;

public class NullServiceProvider : IServiceProvider
{
    public object? GetService( Type serviceType ) => null;
}