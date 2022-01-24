// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Telemetry.Metrics
{
    /// <summary>
    /// A <see cref="Metric"/> storing a <see cref="string"/> value.
    /// </summary>
    [Serializable]
    public sealed class StringMetric : Metric
    {
        public StringMetric( string name ) : base( name ) { }

        public StringMetric( string name, string value )
            : base( name )
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the metric value.
        /// </summary>
        public string? Value { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Value ?? "null";
        }

        /// <inheritdoc />
        public override bool SetValue( object? value )
        {
            throw new NotSupportedException();
        }
    }
}