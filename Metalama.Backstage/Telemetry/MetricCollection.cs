// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Metalama.Backstage.Telemetry
{
    /// <summary>
    /// Collection of metrics (<see cref="Metric"/>).
    /// </summary>
    [Serializable]
    public sealed class MetricCollection : KeyedCollection<string, Metric>
    {
        /// <inheritdoc />
        protected override string GetKeyForItem( Metric item )
        {
            return item.Name;
        }

        /// <summary>
        /// Writes the content of the current collection to a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="TextWriter"/>.</param>
        public void Write( TextWriter writer )
        {
            var first = true;

            foreach ( var metric in this )
            {
                if ( first )
                {
                    first = false;
                }
                else
                {
                    writer.Write( ';' );
                }

                writer.Write( metric.Name );
                writer.Write( '=' );
                metric.WriteValue( writer );
            }
        }

        public override int GetHashCode()
        {
            HashCode seed = default;

            return this.Items.Aggregate(
                seed,
                ( h, i ) =>
                {
                    h.Add( i );

                    return h;
                },
                h => h.ToHashCode() );
        }
    }
}