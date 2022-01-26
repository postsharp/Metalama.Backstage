// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace Metalama.Backstage.Telemetry
{
    /// <summary>
    /// Encapsulates a metric, i.e. anything that can be measured.
    /// </summary>
    [Serializable]
    public abstract class Metric
    {
        protected Metric( string name )
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the metric name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Writes the value of the current metric to a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">A <see cref="TextWriter"/>.</param>
        public virtual void WriteValue( TextWriter textWriter )
        {
            textWriter.Write( this.ToString() );
        }

        public abstract bool SetValue( object? value );
    }
}