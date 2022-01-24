// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;
using System.Xml;

namespace PostSharp.Backstage.Telemetry.Metrics
{
    /// <summary>
    /// A <see cref="Metric"/> storing a <see cref="bool"/> value.
    /// </summary>
    [Serializable]
    public sealed class BoolMetric : Metric
    {
        public BoolMetric( string name )
            : base( name ) { }

        public BoolMetric( string name, bool value )
            : base( name )
        {
            this.Value = value;
        }

        public bool Value { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return XmlConvert.ToString( this.Value );
        }

        /// <inheritdoc />
        public override bool SetValue( object? value )
        {
            if ( value == null )
            {
                return false;
            }

            bool operand;

            try
            {
                operand = Convert.ToBoolean( value, CultureInfo.InvariantCulture );
            }
            catch
            {
                return false;
            }

            this.Value |= operand;

            return true;
        }
    }
}