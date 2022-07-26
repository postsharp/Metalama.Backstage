// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Backstage.Configuration;

[AttributeUsage( AttributeTargets.Class )]
internal class ConfigurationFileAttribute : Attribute
{
    public ConfigurationFileAttribute( string fileName )
    {
        this.FileName = fileName;
    }

    public string FileName { get; }
}