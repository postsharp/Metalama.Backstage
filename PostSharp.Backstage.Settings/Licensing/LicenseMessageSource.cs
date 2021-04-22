// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using System;
using System.Globalization;

namespace PostSharp.Backstage.Licensing
{
    public class LicenseMessageSource
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        internal LicenseMessage GetMessage(License license)
        {
            if ( license.ValidTo.HasValue )
            {
                // Trial license have expiration times.

                int daysLeft = (int) license.ValidTo.Value.Subtract( this._dateTimeProvider.Now ).TotalDays;

                TimeSpan frequency = TimeSpan.FromHours( daysLeft > 3 ? 4 : 0.25 );

                string licenseName = license.LicenseType == LicenseType.Evaluation ? "trial" : "license";

                if ( daysLeft <= 0 )
                {
                    // This branch is never reached because the message is not emitted from this method in case the method is invalid.
                    return new LicenseMessage(
                        LicenseMessageKind.Expired,
                        string.Format( CultureInfo.InvariantCulture, "Your {0} of {1} has expired.", licenseName, license.GetProductName( true ) ),
                        isError: true,
                        frequency );
                }
                else
                {
                    return new LicenseMessage(
                        LicenseMessageKind.Expiring,
                        string.Format( CultureInfo.InvariantCulture, "Your {2} of {1} will expire in {0} day(s).", daysLeft, license.GetProductName( true ), licenseName ),
                        isError: false,
                        frequency );
                }
            }
            // TODO
            /*else if ( this._userSettings.WarnAboutSubscriptionExpiration && license.SubscriptionEndDate.HasValue )
            {
                int daysLeft = (int) license.SubscriptionEndDate.Value.Subtract( this._dateTimeProvider.GetCurrentDateTime() ).TotalDays;
                if ( daysLeft >= 60 )
                {
                    // License is not expiring soon.
                    return null;
                }
                TimeSpan frequency = (daysLeft > 7 ? TimeSpan.FromDays( 7 ) : TimeSpan.FromDays( 1 ));
                string expirationVerbPhrase;
                if ( daysLeft > 1 )
                {
                    expirationVerbPhrase = "is set to expire in " + daysLeft + " days";
                }
                else if ( daysLeft == 1 )
                {
                    expirationVerbPhrase = "is set to expire in " + daysLeft + " day";
                }
                else if ( daysLeft == 0 )
                {
                    expirationVerbPhrase = "is set to expire in less than 1 day";
                }
                else // (daysLeft < 0)
                {
                    expirationVerbPhrase = "has expired";
                }
                string message = "PostSharp support and update license " + expirationVerbPhrase + ". Renew now to continue without a lapse in functionality.";
                return new LicenseMessage(
                       LicenseMessageKind.SubscriptionExpiring,
                       message,
                       isError: false,
                       frequency );

            }*/
            return null;
        }
    }
}
