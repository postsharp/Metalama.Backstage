// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Extensibility
{
    public interface ICurrentProjectService
    {
        string GetTargetAssemblyName();
    }
}
