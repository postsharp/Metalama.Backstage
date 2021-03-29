// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;

namespace PostSharp.Backstage.Utilities
{
    internal static class CollectionExtensions
    {
        internal static T PopFirst<T>( this ICollection<T> collection )
        {
            T item = collection.First();
            collection.Remove( item );
            return item;
        }

        internal static TValue GetOrAdd<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key )
            where TValue: new()
        {
            if ( dictionary.TryGetValue( key, out TValue value ) )
                return value;

            value = new TValue();
            dictionary[key] = value;
            return value;
        }
    }
}
