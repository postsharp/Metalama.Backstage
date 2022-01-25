// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections;
using System.Xml;

namespace Metalama.Backstage.Telemetry
{
    // Warning: this file is linked to UserInterface solution. We need to serialize
    // exceptions from debugging server in the same way as ExceptionPackager does without
    // referencing PostSharp.Compiler.Settings.
    internal static class ExceptionXmlFormatter
    {
        public static void WriteException( XmlWriter writer, Exception e )
        {
            writer.WriteElementString( "Type", ExceptionSensitiveDataHelper.RemoveSensitiveData( e.GetType().FullName ) );
            writer.WriteElementString( "Message", ExceptionSensitiveDataHelper.RemoveSensitiveData( e.Message ) );
            writer.WriteElementString( "Source", ExceptionSensitiveDataHelper.RemoveSensitiveData( e.Source ) );

            if ( e.Data != null )
            {
                writer.WriteStartElement( "Data" );

                foreach ( DictionaryEntry data in e.Data )
                {
                    writer.WriteStartElement( "Item" );

                    if ( data.Key != null )
                    {
                        writer.WriteElementString( "Key", ExceptionSensitiveDataHelper.RemoveSensitiveData( data.Key.ToString() ) );
                    }

                    if ( data.Value != null )
                    {
                        if ( data.Value is Array array )
                        {
                            writer.WriteStartElement( "Array" );

                            for ( var i = 0; i < array.Length; i++ )
                            {
                                var value = array.GetValue( i );

                                if ( value is Exception exception )
                                {
                                    writer.WriteStartElement( "Item" );
                                    WriteException( writer, exception );
                                    writer.WriteEndElement();
                                }
                                else
                                {
                                    writer.WriteElementString( "Item", ExceptionSensitiveDataHelper.RemoveSensitiveData( value.ToString() ) );
                                }
                            }

                            writer.WriteEndElement();
                        }
                        else if ( data.Value is Exception exception )
                        {
                            writer.WriteStartElement( "Value" );
                            WriteException( writer, exception );
                            writer.WriteEndElement();
                        }
                        else
                        {
                            writer.WriteElementString( "Value", ExceptionSensitiveDataHelper.RemoveSensitiveData( data.Value.ToString() ) );
                        }
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteElementString(
                "StackTrace",
                Environment.NewLine + ExceptionSensitiveDataHelper.RemoveSensitiveData( e.StackTrace ) + Environment.NewLine );

            if ( e.InnerException != null )
            {
                writer.WriteStartElement( "InnerException" );
                WriteException( writer, e.InnerException );
                writer.WriteEndElement();
            }

            if ( e is AggregateException aggregate )
            {
                writer.WriteStartElement( "InnerExceptions" );

                foreach ( var innerException in aggregate.InnerExceptions )
                {
                    if ( innerException == null )
                    {
                        continue;
                    }

                    writer.WriteStartElement( "Exception" );
                    WriteException( writer, innerException );
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }
    }
}