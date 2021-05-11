// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using PostSharp.Cli.Session;

namespace PostSharp.Cli.Commands.Licensing
{
    internal class LicenseStringsOrdinalDictionary : OrdinalDictionary
    {
        private const string _name = "LICENSE";

        public LicenseStringsOrdinalDictionary( IServiceProvider services ) :
            base( _name, services )
        {
        }

        public static OrdinalDictionary Load( IServiceProvider services )
        {
            return Load( _name, services );
        }
    }
}
