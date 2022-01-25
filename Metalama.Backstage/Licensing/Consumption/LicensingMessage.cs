// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Licensing.Consumption;

public class LicensingMessage
{
    public string Text { get; }

    public LicensingMessage( string text )
    {
        this.Text = text;
    }
}