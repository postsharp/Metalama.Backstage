﻿// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;
using System.IO;
using System.Net;

namespace PostSharp.Backstage.Licensing.Helpers
{
    public static class LicenseServerClient
    {
        /// <exclude/>
        internal static bool IsLicenseServerUrl( string licenseString )
        {
            string message;
            return IsLicenseServerUrl( licenseString, out message );
        }

        /// <exclude/>
        internal static bool IsLicenseServerUrl( string licenseString, out string errorMessage )
        {
            if ( !Uri.IsWellFormedUriString( licenseString, UriKind.Absolute ) )
            {
                errorMessage = "Invalid URL.";
                return false;
            }

            Uri uri = new Uri( licenseString, UriKind.Absolute );

            if ( uri.Scheme != Uri.UriSchemeHttp &&
                 uri.Scheme != Uri.UriSchemeHttps )
            {
                errorMessage = "Only HTTP and HTTPS are acceptable protocols.";
                return false;
            }

            if ( !string.IsNullOrEmpty( uri.Query ) )
            {
                errorMessage = "The URL cannot contain a query string.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        internal static LicenseLease TryDownloadLease( IMessageSink messageSink, string url,
                                                       IRegistryKey registryKey, bool renewal = false )
        {
            LicenseLease lease = null;
            try
            {
                lease = DownloadLease( url );
                string serializedLease = lease.Serialize();
                License license = License.Deserialize( lease.LicenseString );

                if ( license.RequiresVersionSpecificStore )
                {
                    registryKey.SetValue( license.MinPostSharpVersion.ToString( 3 ), serializedLease );
                }
                else
                {
                    registryKey.SetValue( null, serializedLease );
                }
            }
            catch ( Exception e )
            {
                if ( messageSink != null )
                {
                    messageSink.Write(
                        LicensingMessageSource.Instance.CreateMessage( MessageLocation.Unknown, renewal ? SeverityType.Error : SeverityType.Warning,
                                                                       "PS0148", null, new object[] {e.Message} ) );
                }

                // Return null to use the last-known good lease.
            }

            return lease;
        }

        private static LicenseLease DownloadLease( string url )
        {
            if ( !url.Contains( "?" ) )
            {
                url = url.TrimEnd( '/' );


                url += string.Format(CultureInfo.InvariantCulture, "/Lease.ashx?user={0}&machine={1}-{2:x}&version={3}&buildDate={4:o}",
                                      Environment.UserName, Environment.MachineName,
                                      LicenseRegistrationHelper.GetCurrentMachineHash(),
                                      UserSettings.VersionString,
                                      UserSettings.BuildDate );
            }

            LicensingTrace.Licensing?.WriteLine( "Leasing a license from the server: {0}.", url );


            string leaseString;

            try
            {
                leaseString = SystemServiceLocator.GetService<IWebClientService>().DownloadString( url );
            }
            catch ( WebException e )
            {
                HttpWebResponse response = (HttpWebResponse) e.Response;
                if ( response != null && response.StatusCode == HttpStatusCode.Forbidden /* Forbidden */ )
                {
                    StreamReader reader = new StreamReader( response.GetResponseStream() );
                    throw new InvalidLicenseException( reader.ReadToEnd(), e );
                }
                throw;
            }

            LicenseLease lease = LicenseLease.Deserialize( leaseString );

            if ( lease == null )
                throw new InvalidLicenseException( "The license server returned an invalid response." );

            return lease;
        }

        /// <exclude/>
        internal static bool TestLicenseServer( string url, out string errorMessage, out License license )
        {
            try
            {
                LicenseLease lease = DownloadLease( url );
                license = License.Deserialize( lease.LicenseString );

                if ( license == null )
                {
                    errorMessage = "The downloaded lease is invalid.";
                    return false;
                }

                if ( !license.Validate( null, out errorMessage ) )
                {
                    return false;
                }

                errorMessage = null;
                return true;
            }
            catch ( Exception e )
            {
                errorMessage = e.Message;
                license = null;
                return false;
            }
        }


        // This method is exposed because it is used to test the license server.
#pragma warning disable CA1054 // Uri parameters should not be strings
        public static LicenseLease TryGetLease( string url, IRegistryKey registryKey, DateTime now, IMessageSink messageSink)
#pragma warning restore CA1054 // Uri parameters should not be strings
        {
            foreach ( string name in registryKey.GetValueNames() )
            {
                // "" is the default value, which is the version agnostic one
                if ( !string.IsNullOrEmpty( name ) )
                {
                    if ( !Version.TryParse( name, out _ ) )
                    {
                        continue;
                    }

                    // We don't check the minimal PostSharp version as in PostSharp 6.5.17+, 6.8.10+, 6.9.3+ and newer
                    // and Caravela, all license keys are backward compatible.
                    // - Licenses with unknown types and products fail validation and thus are not used.
                    // - Unknown license fields are skipped.
                }

                string serializedLease = registryKey.GetValue( name ) as string;
                LicenseLease lease = null;

                if ( !string.IsNullOrEmpty( serializedLease ) )
                {
                    lease = LicenseLease.Deserialize( serializedLease );
                    if ( lease == null || lease.EndTime < now )
                    {
                        LicensingTrace.Licensing?.WriteLine( "The cached leased license is invalid." );
                        registryKey.SetValue( null, "" );
                        lease = null;
                    }
                    else if ( lease.RenewTime < now )
                    {
                        LicenseLease renewedLease = TryDownloadLease( messageSink, url, registryKey, true );
                        if ( renewedLease != null )
                        {
                            lease = renewedLease;
                        }
                    }
                }

                if ( lease != null )
                {
                    return lease;
                }
            }

            return null;
        }
    }
}
