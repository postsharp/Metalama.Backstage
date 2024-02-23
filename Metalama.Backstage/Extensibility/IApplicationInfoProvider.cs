// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Extensibility
{
    // TODO: For licensing, we need info about all applications together.
    // TODO: Split IApplicationInfo to application, component and process info.
    public interface IApplicationInfoProvider : IBackstageService
    {
        IApplicationInfo CurrentApplication { get; }
    }
}