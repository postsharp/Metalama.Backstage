// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;

namespace PostSharp.Backstage.Licensing.LicenseServer
{
    /// <summary>
    /// Represents a lease of license by a specific user.
    /// </summary>
    [Serializable]
    public sealed class LicenseLease
    {
        private LicenseLease()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="LicenseLease"/>.
        /// </summary>
        /// <param name="licenseString">String serialization of the license (license key).</param>
        /// <param name="startTime">Start time of the lease.</param>
        /// <param name="endTime">End time of the lease.</param>
        /// <param name="renewTime">Time when the lease should be renewed.</param>
        public LicenseLease( string licenseString, DateTime startTime, DateTime endTime, DateTime renewTime)
        {
            if (licenseString == null) throw new ArgumentNullException(nameof(licenseString));
            this.LicenseString = licenseString;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.RenewTime = renewTime;
        }

        /// <summary>
        /// Gets the string serialization of the license (license key).
        /// </summary>
        public string LicenseString { get; private set; }

        /// <summary>
        /// Gets the start time of the lease.
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// Gets the end time of the lease.
        /// </summary>
        public DateTime EndTime { get; private set; }

        /// <summary>
        /// Gets the time from which the lease should be renewed.
        /// </summary>
        public DateTime RenewTime { get; private set; }

        /// <summary>
        /// Serializes the current <see cref="LicenseLease"/> into a string.
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            StringBuilder writer = new StringBuilder();
            writer.Append( "License: " + this.LicenseString);
            writer.Append( "; StartTime: " + XmlConvert.ToString( this.StartTime, XmlDateTimeSerializationMode.Utc ) );
            writer.Append( "; EndTime: " + XmlConvert.ToString( this.EndTime, XmlDateTimeSerializationMode.Utc ) );
            writer.Append( "; RenewTime: " + XmlConvert.ToString( this.RenewTime, XmlDateTimeSerializationMode.Utc ) );

            return writer.ToString();
        }

        /// <summary>
        /// Deserializes a string into a new <see cref="LicenseLease"/>.
        /// </summary>
        /// <param name="serializedLicenseLease">A serialized <see cref="LicenseLease"/>, produced by <see cref="Serialize"/>.</param>
        /// <returns>The <see cref="LicenseLease"/> built from <paramref name="serializedLicenseLease"/>, or <c>null</c> 
        /// if the string could not be deserialized.</returns>
        public static bool TryDeserialize(string serializedLicenseLease, IDateTimeProvider dateTimeProvider, [MaybeNullWhen( returnValue: false )] out LicenseLease lease)
        {
            try
            {
                lease = new LicenseLease();
                string[] parts = serializedLicenseLease.Split( ';' );
                DateTime? startTime = null, endTime = null, renewTime = null;

                foreach ( string part in parts )
                {
                    int pos = part.IndexOf( ':' );
                    if ( pos < 0 || pos == part.Length - 1 )
                        continue;

                    string key = part.Substring( 0, pos ).Trim();
                    string value = part.Substring( pos + 1, part.Length - pos - 1 ).Trim();

                    switch ( key.ToLowerInvariant() )
                    {
                        case "license":
                            lease.LicenseString = value;
                            break;

                        case "starttime":
                            startTime = XmlConvert.ToDateTime( value, XmlDateTimeSerializationMode.Utc ).ToLocalTime();
                            break;

                        case "endtime":
                            endTime = XmlConvert.ToDateTime( value, XmlDateTimeSerializationMode.Utc ).ToLocalTime();
                            break;

                        case "renewtime":
                            renewTime = XmlConvert.ToDateTime( value, XmlDateTimeSerializationMode.Utc ).ToLocalTime();
                            break;
                    }
                }


                if ( lease.LicenseString == null )
                {
                    lease = null;
                    return false;
                }

                if ( !startTime.HasValue )
                    startTime = dateTimeProvider.Now;

                if ( !endTime.HasValue )
                    endTime = startTime.Value.AddDays( 1 );

                if ( !renewTime.HasValue )
                    renewTime = endTime;

                lease.StartTime = startTime.Value;
                lease.EndTime = endTime.Value;
                lease.RenewTime = renewTime.Value;

                return true;
            }
            catch ( FormatException )
            {
                lease = null;
                return false;
            }
        }
    }
}