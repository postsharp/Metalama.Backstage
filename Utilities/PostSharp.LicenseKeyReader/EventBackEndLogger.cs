// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using System.Text;

namespace PostSharp.LicenseKeyReader
{
    internal sealed class EventBackEndLogger : IBackstageDiagnosticSink
    {
        public event EventHandler<string>? DiagnosticsReported;

        private static string FormatMessage( string kind, string message, IDiagnosticsLocation? location )
        {
            StringBuilder messageBuilder = new();
            messageBuilder.Append( DateTime.Now ).Append( " " ).Append( kind ).Append( ": " ).Append( message );

            if ( location != null )
            {
                messageBuilder.Append( "; location: " ).Append( location.ToString() );
            }

            return messageBuilder.ToString();
        }

        public void ReportError( string message, IDiagnosticsLocation? location = null )
        {
            this.DiagnosticsReported?.Invoke( this, FormatMessage( "Error", message, location ) );
        }

        public void ReportWarning( string message, IDiagnosticsLocation? location = null )
        {
            this.DiagnosticsReported?.Invoke( this, FormatMessage( "Error", message, location ) );
        }
    }
}