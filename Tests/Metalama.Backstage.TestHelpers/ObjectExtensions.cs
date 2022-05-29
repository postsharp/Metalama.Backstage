// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Newtonsoft.Json;
using System;
using System.IO;

namespace Metalama.Backstage.Testing
{
    public static class ObjectExtensions
    {
        public static bool DeepEquals( this object @this, object other )
        {
            string Serialize(object o)
            {
                var textWriter = new StringWriter();
                var serializer = JsonSerializer.Create();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize( textWriter, o );

                return textWriter.ToString();
            }

            return Serialize( @this ).Equals( Serialize( other ), StringComparison.Ordinal );
        }
    }
}