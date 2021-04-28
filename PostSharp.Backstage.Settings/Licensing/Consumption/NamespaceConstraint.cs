// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing.Consumption
{
    internal class NamespaceConstraint
    {
        public string AllowedNamespace { get; }

        public NamespaceConstraint( string allowedNamespace )
        {
            if ( string.IsNullOrEmpty( allowedNamespace ) )
            {
                throw new ArgumentException( "Missing namespace.", nameof( allowedNamespace ) );
            }

            this.AllowedNamespace = allowedNamespace;
        }

        public bool AllowsNamespace( string requiredNamespace )
        {
            if ( !requiredNamespace.StartsWith( this.AllowedNamespace, StringComparison.OrdinalIgnoreCase ) )
            {
                return false;
            }

            if ( requiredNamespace.Length == this.AllowedNamespace.Length )
            {
                return true;
            }

            var delimiter = requiredNamespace[this.AllowedNamespace.Length];

            if (
                // If there is not '.' after the namespace name,
                // it means the namespace name is different
                // and it only begins with the required name.
                delimiter == '.'

                // When we get a assembly full name,
                // there is a ',' after the short name.
                || delimiter == ',' )
            {
                return true;
            }

            return false;
        }
    }
}
