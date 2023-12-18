// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System.Threading.Tasks;

namespace Metalama.Backstage.UserInterface;

public interface IUserInterfaceService : IBackstageService
{
    void Initialize();

    void OpenExternalWebPage( string url, BrowserMode browserMode );

    Task OpenConfigurationWebPageAsync( string path );
}

public enum BrowserMode
{
    Default,
    NewWindow,
    Application
}