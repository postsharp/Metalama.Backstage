// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Settings
{
    public class UserSettings
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly SettingsReader _feedbackSettingsReader;
        private readonly SettingsReader _userSettingsReader;
        private readonly SettingsReader _machineSettingsReader;

        private readonly Setting[] _feedbackSettings;
        private readonly Setting[] _userSettings;
        private readonly Setting[] _machineSettings;

        protected UserSettings(
            SettingsReader feedbackSettingsReader,
            SettingsWriter feedbackSettingsWriter,
            SettingsReader userSettingsReader,
            SettingsWriter userSettingsWriter,
            SettingsReader machineSettingsReader,
            SettingsWriter machineSettingsWriter,
            IDateTimeProvider dateTimeProvider )
        {
            this._dateTimeProvider = dateTimeProvider;
            this._feedbackSettingsReader = feedbackSettingsReader;
            this._userSettingsReader = userSettingsReader;
            this._machineSettingsReader = machineSettingsReader;

            this._feedbackSettings = new Setting[]
            {
                this.DeviceId = new( "DeviceId", feedbackSettingsReader, feedbackSettingsWriter ),
                this.ErrorReportingAction = new( "ErrorReportingAction", feedbackSettingsReader, feedbackSettingsWriter, 0 ),
                this.NewExceptionReportingAction = new( "NewExceptionReportingAction", feedbackSettingsReader, feedbackSettingsWriter, 0 ),
                this.NewPerformanceProblemReportingAction = new( "NewPerformanceProblemReportingAction", feedbackSettingsReader, feedbackSettingsWriter, 0 ),
                this.UsageReportingAction = new( "UsageReportingAction", feedbackSettingsReader, feedbackSettingsWriter, 0 ),
                this.Email = new( "Email", feedbackSettingsReader, feedbackSettingsWriter, "" ),
                this.UsageReportingLastQuestion = new( "UsageReportingLastQuestion", feedbackSettingsReader, feedbackSettingsWriter, DateTime.MinValue ),
                this.LastUploadTime = new( "LastUploadTime", feedbackSettingsReader, feedbackSettingsWriter, DateTime.MinValue ),
                this.LastUnsuccessfulLicenseCheckTime = new( "LastUnsuccessfulLicenseCheckTime", feedbackSettingsReader, feedbackSettingsWriter, DateTime.MinValue )
            };

            this._userSettings = new Setting[]
            {
                this.WarnAboutSubscriptionExpiration = new( "WarnAboutSubscriptionExpiration", userSettingsReader, userSettingsWriter, true ),
                this.LastLicenseInfoMessageTime = new( "LastLicenseInfoMessageTime", userSettingsReader, userSettingsWriter, DateTime.MinValue ),
                this.LastLicenseExpirationMessageTime = new( "LastLicenseExpirationMessageTime", userSettingsReader, userSettingsWriter, DateTime.MinValue ),
                this.InstallVsxLastQuestion = new( "InstallVsxLastQuestion", userSettingsReader, userSettingsWriter, DateTime.MinValue ),
                this.LicenseMessagesDisabled = new( "LicenseMessagesDisabled", userSettingsReader, userSettingsWriter, false )
            };

            this._machineSettings = new Setting[]
            {
            };
        }

        public DateTime GetCurrentDateTime()
        {
            return this._dateTimeProvider.GetCurrentDateTime();
        }

        public void Refresh()
        {
            static void Refresh(IEnumerable<Setting> settings, SettingsReader reader)
            {
                try
                {
                    reader.Open();

                    try
                    {
                        foreach ( var setting in settings )
                        {
                            setting.Refresh( reader );
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                catch
                {
                }
            }

            Refresh( this._feedbackSettings, this._feedbackSettingsReader );
            Refresh( this._userSettings, this._userSettingsReader );
            Refresh( this._machineSettings, this._machineSettingsReader );

            try
            {
                if ( string.IsNullOrEmpty( this.DeviceId ) )
                {
                    this.ResetDeviceId();
                }
            }
            finally
            {
                this._machineSettingsReader.Close();
                this._userSettingsReader.Close();
                this._feedbackSettingsReader.Close();
            }
        }
      
        public void ResetDeviceId()
        {
            var failed = false;

            try
            {
                var deviceId = Guid.NewGuid().ToString();
                this.DeviceId.Set( deviceId );
            }
            catch
            {
                failed = true;
            }

            if ( failed || !this.DeviceId.HasValue )
            {
                // If we fail at this point, return an empty Guid so that we know we cannot rely on it.
                this.DeviceId.Set( Guid.Empty.ToString() );
            }
        }

        /// <exclude />
        public readonly BooleanSetting LicenseMessagesDisabled;

        /// <summary>
        /// Gets a device id that can be reset and does not persist from one device to another.
        /// </summary>
        public readonly StringSetting DeviceId;

        /// <exclude />
        public readonly StringSetting Email;

        /// <exclude />
        public readonly ReportingActionSetting ErrorReportingAction;

        /// <exclude />
        public readonly ReportingActionSetting NewExceptionReportingAction;

        /// <exclude />
        public readonly ReportingActionSetting NewPerformanceProblemReportingAction;

        /// <exclude />
        public readonly ReportingActionSetting UsageReportingAction;

        /// <exclude />
        public readonly DateTimeSetting UsageReportingLastQuestion;

        /// <exclude />
        public readonly DateTimeSetting InstallVsxLastQuestion;

        /// <exclude />
        public readonly DateTimeSetting LastUploadTime;

        /// <exclude />
        public readonly DateTimeSetting LastUnsuccessfulLicenseCheckTime;

        /// <exclude />
        public readonly DateTimeSetting LastLicenseExpirationMessageTime;

        /// <exclude />
        public readonly BooleanSetting WarnAboutSubscriptionExpiration;

        /// <exclude />
        public readonly DateTimeSetting LastLicenseInfoMessageTime;

        // TODO: implement back the following:
        // {
        //    get
        //    {
        //        if ( perUsageLicensingCountingEnabledTime.HasValue )
        //        {
        //            if ( GetCurrentDateTime() - perUsageLicensingCountingEnabledTime.Value > PerUsageLicensingCountingPeriod )
        //            {
        //                DeleteRegistryValue( "PerUsageLicensingCountingEnabledTime" );
        //                perUsageLicensingCountingEnabledTime = null;
        //            }
        //        }
        //
        //        return perUsageLicensingCountingEnabledTime;
        //    }
        //
        //    set
        //    {
        //        if ( value != perUsageLicensingCountingEnabledTime )
        //        {
        //            perUsageLicensingCountingEnabledTime = value;
        //
        //            if ( !value.HasValue )
        //            {
        //                DeleteRegistryValue( "PerUsageLicensingCountingEnabledTime" );
        //            }
        //            else
        //            {
        //                SetRegistryValue( "PerUsageLicensingCountingEnabledTime", value.Value );
        //            }
        //        }
        //    }
        // }
    }
}