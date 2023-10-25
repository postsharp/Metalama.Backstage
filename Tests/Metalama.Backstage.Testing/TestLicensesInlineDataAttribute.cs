// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace Metalama.Backstage.Testing;

/// <summary>
/// Provides a data source for a data theory, with the data coming from inline values.
/// Strings are translated by accessing the corresponding field in <see cref="TestLicenseKeys"/>.
/// </summary>
[AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
public sealed class TestLicensesInlineDataAttribute : DataAttribute
{
    private readonly object?[] _data;

    public TestLicensesInlineDataAttribute( params object?[] data )
    {
        this._data = data
            .Select( value => value is string s && typeof(TestLicenseKeys).GetProperty( s ) is { } property ? property.GetValue( null ) : value )
            .ToArray();
    }

    public override IEnumerable<object?[]> GetData( MethodInfo testMethod ) => new[] { this._data };
}