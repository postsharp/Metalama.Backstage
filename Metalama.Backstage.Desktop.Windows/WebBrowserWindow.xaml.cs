// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Windows;
using Uri = System.Uri;

namespace Metalama.Backstage.Desktop.Windows;

public partial class WebBrowserWindow : Window
{
    public WebBrowserWindow()
    {
        this.InitializeComponent();
    }

    public Uri Url
    {
        get => this.webView.Source;
        set => this.webView.Source = value;
    }
}