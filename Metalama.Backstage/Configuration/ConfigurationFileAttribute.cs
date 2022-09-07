// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Backstage.Configuration;

[AttributeUsage( AttributeTargets.Class )]
public class ConfigurationFileAttribute : Attribute
{
    public ConfigurationFileAttribute( string fileName )
    {
        this.FileName = fileName;
    }

    public string FileName { get; }
}