// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Text.RegularExpressions;

namespace Metalama.Backstage.Telemetry
{
    // Warning: this file is linked to UserInterface solution. We need to serialize
    // exceptions from debugging server in the same way as ExceptionPackager does without
    // referencing PostSharp.Compiler.Settings.
    internal static class ExceptionSensitiveDataHelper
    {
        private static readonly Regex _userNameRegEx =
            new(
                @"(?<![\.\^0-9a-zA-Z<>_`])(?![0-9]|Microsoft\.|MS\.|System\.|PostSharp\.|Metalama\.|Presentation|EnvDTE|Windows|`)[a-zA-Z0-9\$`@_\?]+(?:\.(?![0-9])\.?[a-zA-Z0-9\$`@<>_]+)+(?![\.\^0-9a-zA-Z`@_\$])" );

        private static readonly Regex _pathRegex = new( @"(?:[a-zA-Z]\:)?\\[^\:;\r\n"",'\]\}]+" );

        /// <exclude />
        public static string RemoveSensitiveData( string? input )
        {
            if ( input == null )
            {
                return "";
            }

            return _pathRegex.Replace( _userNameRegEx.Replace( input, "#user" ), "#path" );
        }
    }
}