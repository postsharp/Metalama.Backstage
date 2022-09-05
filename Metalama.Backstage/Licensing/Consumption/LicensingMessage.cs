// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Licensing.Consumption;

public class LicensingMessage
{
    public string Text { get; }

    public bool IsError { get; }

    public LicensingMessage( string text, bool isError = false )
    {
        this.Text = text;
        this.IsError = isError;
    }
}