// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;

namespace Metalama.Backstage.Utilities
{
    internal static class CollectionExtensions
    {
        internal static T PopFirst<T>( this ICollection<T> collection )
        {
            var item = collection.First();
            collection.Remove( item );

            return item;
        }

        internal static TValue GetOrAdd<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key )
            where TKey : notnull
            where TValue : new()
        {
            if ( dictionary.TryGetValue( key, out var value ) )
            {
                return value;
            }

            value = new TValue();
            dictionary[key] = value;

            return value;
        }
    }
}