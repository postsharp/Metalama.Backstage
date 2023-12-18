// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
using System;
using System.Diagnostics.CodeAnalysis;

#if NETFRAMEWORK || NETCOREAPP
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
#endif

namespace Metalama.Backstage.UserInterface;

#pragma warning disable CA1416

internal class WindowsUserInterfaceService : UserInterfaceService
{
    private readonly IToastNotificationService _toastNotificationService;

    public WindowsUserInterfaceService( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        this._toastNotificationService = serviceProvider.GetRequiredBackstageService<IToastNotificationService>();
    }

    protected override void Notify( ToastNotificationKind kind, ref bool notificationReported )
    {
        this._toastNotificationService.Show( new ToastNotification( kind ) );
        notificationReported = true;
    }

#if NETFRAMEWORK || NETCOREAPP
    protected override ProcessStartInfo GetProcessStartInfoForUrl( string url, BrowserMode browserMode )
    {
        if ( browserMode == BrowserMode.Default || !this.TryGetDefaultBrowser( out var browserPath ) )
        {
            return base.GetProcessStartInfoForUrl( url, browserMode );
        }

        switch ( Path.GetFileNameWithoutExtension( browserPath ) )
        {
            // ReSharper disable StringLiteralTypo
            case "msedge": // --window-size does not seem to work on Edge
            case "chrome":
            case "opera":
            case "brave":
            case "vivaldi":
            case "blisk":
            case "browser":
                // ReSharper restore StringLiteralTypo

                // For Chromium-based browsers, we know how to open the page in a new window or in an app window.
                var arg = browserMode switch
                {
                    BrowserMode.Application => $"--app={url} --window-size=400,400",
                    BrowserMode.NewWindow => $"--new-window {url}",
                    _ => throw new ArgumentOutOfRangeException( nameof(browserMode) )
                };

                return new ProcessStartInfo( browserPath, arg );

            default:
                return base.GetProcessStartInfoForUrl( url, browserMode );
        }
    }

    private bool TryGetDefaultBrowser( [NotNullWhen( true )] out string? path )
    {
        try
        {
            using ( var userChoiceKey = Registry.CurrentUser.OpenSubKey( @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice" ) )
            {
                var progIdValue = userChoiceKey?.GetValue( "ProgId" );

                if ( progIdValue == null )
                {
                    path = null;

                    return false;
                }

                var browser = progIdValue.ToString();

                using ( var progIdKey = Registry.ClassesRoot.OpenSubKey( browser + @"\shell\open\command" ) )
                {
#pragma warning disable CA1307
                    path = progIdKey?.GetValue( null )?.ToString()?.Replace( "\"", "" );
#pragma warning restore CA1307

                    if ( string.IsNullOrEmpty( path ) )
                    {
                        path = null;

                        return false;
                    }

                    // Handling paths with arguments. That's not bullet proof but this should be enough.
                    if ( !path.EndsWith( ".exe", StringComparison.OrdinalIgnoreCase ) )
                    {
                        path = path.Substring( 0, path.LastIndexOf( ".exe", StringComparison.OrdinalIgnoreCase ) + 4 );
                    }

                    return true;
                }
            }
        }
        catch ( Exception exception )
        {
            this.Logger.Error?.Log( exception.ToString() );
            path = null;

            return false;
        }
    }
#endif
}