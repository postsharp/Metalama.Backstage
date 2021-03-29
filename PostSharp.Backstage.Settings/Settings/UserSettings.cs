// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace PostSharp.Backstage.Settings
{
    // TODO: This used to be internal.
    public static class UserSettings
    {
        internal const string ProductVersion = "3";

        /// <exclude/>
        public const string FeedbackDirectoryName = @"SharpCrafters\PostSharp " + ProductVersion + @"\Feedback";

        private const string feedbackRegistryKeyName = "Software\\" + FeedbackDirectoryName;
        private const string postSharpRegistryKeyName = "Software\\SharpCrafters\\PostSharp " + ProductVersion;
        private const string reportedIssuesRegistryKeyName = postSharpRegistryKeyName + @"\ReportedIssues";


        private static ReportingAction errorReportingAction;
        private static ReportingAction newExceptionReportingAction;
        private static ReportingAction newPerformanceProblemReportingAction;
        private static ReportingAction usageReportingAction;
        private static string email;
        private static DateTime usageReportingLastQuestion;
        private static DateTime lastLicenseExpirationMessageTime;
        private static bool warnAboutSubscriptionExpiration = true;
        private static DateTime lastLicenseInfoMessageTime;
        private static DateTime? perUsageLicensingCountingEnabledTime;
        private static DateTime lastUploadTime;
        private static DateTime lastUnsuccessfulLicenseCheckTime;
        private static string uiProgramPath;
        private static string cliProgramPath;
        private static DateTime installVsxLastQuestion;
        private static bool installVsxAskQuestion = true;
        private static DateTime vsxDetectedTime;
        private static bool? autoDisplayLearnToolWindowUser;

        private static bool oemDeployment;

        private static bool licenseMessagesDisabled;
        private static bool aggressiveExceptionReporting;
        private static bool vsxWelcomePageDisplayed;
        private static bool nuGetWelcomePageDisplayed;
        private static bool displayCopyrightNotices = true;
        private static DateTime lastInstallCompiledImagesQuestionTime = DateTime.MinValue;
        private static bool askInstallCompiledImages = true;
        private static DateTime lastSymbolFileErrorTime = DateTime.MinValue;

        private static DateTime lastPrepareToolTime = DateTime.MinValue;

        static UserSettings()
        {
            
            DefaultApplicationInfoService.Initialize();

            // Never initialize the feedback subsystem on another system than Windows.
            if ( !IsWindows )
                return;

            string toolsDirectory = FileSystemHelper.GetToolsDirectory();

            if ( toolsDirectory != null )
            {
                string postSharpHqPath = Path.Combine( toolsDirectory, "net472", "UserInterface" );
                SetPostSharpProgramPaths( postSharpHqPath );
            }

            Refresh();
        }

        public static DateTime GetCurrentDateTime()
        {
            return SystemServiceLocator.GetService<IDateTimeProvider>().GetCurrentDateTime();
        }

        public static void Refresh()
        {
            // TODO: UserSettings for Linux/Mac.
            if ( !RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                return;
            }

            try
            {
                IRegistryKey userRegistryKey = OpenRegistryKey();
                IRegistryKey machineRegistryKey = OpenRegistryKey( true );
                try
                {

                    ReportExceptions = true;

                    UploadDirectory =
                        Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            FeedbackDirectoryName);
                    UploadJobsDirectory = Path.Combine(UploadDirectory, "Jobs");
                    UploadQueueDirectory = Path.Combine(UploadDirectory, "Queue");

                    try
                    {
                        #region User-level settings (feedback)

                        IRegistryKey feedbackRegistryKey = OpenFeedbackRegistryKey();

                        if (feedbackRegistryKey != null)
                        {
                            using ( feedbackRegistryKey )
                            {
                                try
                                {
                                    errorReportingAction = (ReportingAction) (int) feedbackRegistryKey.GetValue( "ErrorReportingAction", 0 );
                                }
                                catch
                                {
                                }

                                try
                                {
                                    usageReportingAction = (ReportingAction) (int) feedbackRegistryKey.GetValue( "UsageReportingAction", 0 );
                                }
                                catch
                                {
                                }
                                
                                try
                                {
                                    newExceptionReportingAction = (ReportingAction) (int) feedbackRegistryKey.GetValue( "NewExceptionReportingAction", 0 );
                                }
                                catch
                                {
                                }

                                try
                                {
                                    newPerformanceProblemReportingAction = (ReportingAction) (int) feedbackRegistryKey.GetValue( "NewPerformanceProblemReportingAction", 0 );
                                }
                                catch
                                {
                                }

                                try
                                {
                                    usageReportingLastQuestion = feedbackRegistryKey.GetValueDateTime( "UsageReportingLastQuestion", DateTime.MinValue );
                                }
                                catch
                                {
                                }

                                try
                                {
                                    lastUploadTime = feedbackRegistryKey.GetValueDateTime( "LastUploadTime", DateTime.MinValue );
                                }
                                catch
                                {
                                }


                                try
                                {
                                    lastUnsuccessfulLicenseCheckTime =
                                        feedbackRegistryKey.GetValueDateTime( "LastUnsuccessfulLicenseCheckTime", DateTime.MinValue );
                                }
                                catch
                                {
                                }


                                try
                                {
                                    email = (string) feedbackRegistryKey.GetValue( "Email" ) ?? "";
                                }
                                catch
                                {
                                }

                                try
                                {
                                    DeviceId = (string) feedbackRegistryKey.GetValue( "DeviceId" );
                                }
                                catch
                                {
                                }


                                if ( string.IsNullOrEmpty( DeviceId ) )
                                {
                                    ResetDeviceId( feedbackRegistryKey );
                                }

                            }
                        }

                        #endregion

                        #region User-level settings

                        if (userRegistryKey != null)
                        {
                            try
                            {
                                aggressiveExceptionReporting = Convert.ToInt32( userRegistryKey.GetValue( "AggressiveExceptionReporting", 0 ), CultureInfo.InvariantCulture ) != 0;
                            }
                            catch
                            {
                            }

                            try
                            {
                                vsxWelcomePageDisplayed = Convert.ToInt32( userRegistryKey.GetValue( "WelcomePageDisplayed", 0 ), CultureInfo.InvariantCulture) != 0;
                            }
                            catch
                            {
                            }

                            try
                            {
                                nuGetWelcomePageDisplayed = Convert.ToInt32( userRegistryKey.GetValue( "NuGetWelcomePageDisplayed", 0 ), CultureInfo.InvariantCulture ) != 0;
                            }
                            catch
                            {
                            }

                            try
                            {
                                licenseMessagesDisabled = Convert.ToInt32(userRegistryKey.GetValue("LicenseMessagesDisabled", 0), CultureInfo.InvariantCulture) != 0;
                            }
                            catch
                            {
                            }


                            try
                            {
                                lastLicenseExpirationMessageTime = userRegistryKey.GetValueDateTime("LastLicenseExpirationMessageTime", DateTime.MinValue);
                            }
                            catch
                            {
                            }
                            try
                            {
                                warnAboutSubscriptionExpiration = Convert.ToInt32(userRegistryKey.GetValue("WarnAboutSubscriptionExpiration", 1), CultureInfo.InvariantCulture) != 0;
                            }
                            catch
                            {
                            }

                            try
                            {
                                lastLicenseInfoMessageTime = userRegistryKey.GetValueDateTime("LastLicenseInfoMessageTime", DateTime.MinValue);
                            }
                            catch
                            {
                            }

                            try
                            {
                                perUsageLicensingCountingEnabledTime = userRegistryKey.GetValueDateTime( "PerUsageLicensingCountingEnabledTime" );
                            }
                            catch
                            {
                            }


                            try
                            {
                                installVsxLastQuestion = userRegistryKey.GetValueDateTime("InstallVsxLastQuestion", DateTime.MinValue);
                            }
                            catch
                            {
                            }

                            try
                            {
                                installVsxAskQuestion = Convert.ToInt32(userRegistryKey.GetValue("InstallVsxAskQuestion", 1), CultureInfo.InvariantCulture) != 0;
                            }
                            catch
                            {
                            }

                            try
                            {
                                vsxDetectedTime = userRegistryKey.GetValueDateTime( "VsxDetectedTime", DateTime.MinValue );
                            }
                            catch
                            {
                            }

                            try
                            {
                                autoDisplayLearnToolWindowUser = userRegistryKey.GetValueNullableBoolean("AutoDisplayLearnToolWindow");
                            }
                            catch
                            {
                            }


                            try
                            {
                                oemDeployment |= userRegistryKey.GetValue("OEM") != null;
                            }
                            catch
                            {
                            }

                            try
                            {
                                lastInstallCompiledImagesQuestionTime = userRegistryKey.GetValueDateTime("LastInstallCompiledImagesQuestionTime",
                                                                                                          DateTime.MinValue);
                            }
                            catch
                            {
                            }

                            try
                            {
                                lastSymbolFileErrorTime = userRegistryKey.GetValueDateTime( "LastSymbolFileErrorTime",
                                                                                            DateTime.MinValue );
                            }
                            catch
                            {
                            }

                            try
                            {
                                lastPrepareToolTime = userRegistryKey.GetValueDateTime( "LastPrepareToolTime", DateTime.MinValue );
                            }
                            catch
                            {
                            }

                            try
                            {
                                askInstallCompiledImages = Convert.ToInt32(userRegistryKey.GetValue("AskInstallCompiledImages", 1), CultureInfo.InvariantCulture) != 0;
                            }
                            catch
                            {
                            }
                            
                            try
                            {
                                displayCopyrightNotices = 
                                    Convert.ToInt32( userRegistryKey.GetValue( "DisplayCopyrightNotices", 1 ), CultureInfo.InvariantCulture) != 0;
                            }
                            catch
                            {
                            }

                            
                        }


                        #endregion

                        #region Machine-level settings

                        DisableAnyUserInterface = ProcessUtilities.IsCurrentProcessUnattended || DisabledRegistrySettings.IsRegistryForbidden();

                        if (machineRegistryKey != null)
                        {

                            try
                            {
                                int? trayIconDisabledInt = machineRegistryKey.GetValue("TrayIconDisabled", 0) as int?;
                                if (trayIconDisabledInt.HasValue)
                                {
                                    DisableAnyUserInterface |= trayIconDisabledInt.Value != 0;
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                PartnerId = (string)machineRegistryKey.GetValue("PartnerId") ?? "";
                            }
                            catch
                            {
                            }

                            try
                            {
                                oemDeployment |= machineRegistryKey.GetValue("OEM") != null;
                            }
                            catch
                            {
                            }
                        }



                        #endregion
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    userRegistryKey?.Dispose();
                    machineRegistryKey?.Dispose();
                }
            }
            catch (SecurityException)
            {

            }
        }

        private static IRegistryKey OpenFeedbackRegistryKey()
        {
            return SystemServiceLocator.GetService<IRegistryService>().OpenRegistryKey( false, true, feedbackRegistryKeyName ) ??
                   SystemServiceLocator.GetService<IRegistryService>().CreateRegistryKey( false, feedbackRegistryKeyName );
        }

      
        public static void ResetDeviceId()
        {
            // TODO: UserSettings for Linux/Mac.
            if ( !RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                return;
            }

            IRegistryKey key = OpenFeedbackRegistryKey();
            try
            {
                if ( key != null )
                {
                    ResetDeviceId( key );
                }
            }
            finally
            {
                key?.Dispose();
            }
        }

        private static void ResetDeviceId(IRegistryKey feedbackRegistryKey)
        {
            try
            {
                DeviceId = Guid.NewGuid().ToString();
                feedbackRegistryKey.SetValue("DeviceId", DeviceId);
            }
            catch
            {
                // If we fail at this point, return an empty Guid so that we know we cannot rely on it.
                DeviceId = Guid.Empty.ToString();
            }
        }


        private enum RegistryValueKind
        {
            DWord,
            QWord,
            String
        }


        private static void SetRegistryValue(string name, DateTime value, string keyName = postSharpRegistryKeyName)
        {
            SetRegistryValue(name, RegistryKeyExtensions.ConvertDateTimeToQWord(value), RegistryValueKind.QWord, keyName);
        }

        private static void SetRegistryValue(string name, int value, string keyName = postSharpRegistryKeyName)
        {
            SetRegistryValue(name, value, RegistryValueKind.DWord, keyName);
        }

        private static void SetRegistryValue(string name, string value, string keyName = postSharpRegistryKeyName)
        {
            SetRegistryValue(name, value, RegistryValueKind.String, keyName);
        }

        private static void SetRegistryValue(string name, bool value, string keyName = postSharpRegistryKeyName)
        {
            SetRegistryValue(name, value ? 1 : 0, RegistryValueKind.DWord, keyName);
        }

        private static void SetRegistryValue(string name, bool? value, string keyName = postSharpRegistryKeyName)
        {
            SetRegistryValue(name, RegistryKeyExtensions.ConvertNullableBooleanToDWord(value), RegistryValueKind.DWord, keyName);
        }


        private static void SetRegistryValue( string name, object value, RegistryValueKind kind, string keyName = postSharpRegistryKeyName )
        {
            try
            {
                IRegistryKey registryKey = SystemServiceLocator.GetService<IRegistryService>().OpenRegistryKey( false, true, keyName );
                if ( registryKey == null )
                {
                    registryKey = SystemServiceLocator.GetService<IRegistryService>().CreateRegistryKey( false, keyName );
                    if ( registryKey == null )
                        return;
                }

                using ( registryKey )
                {
                    switch ( kind )
                    {
                        case RegistryValueKind.DWord:
                            registryKey.SetDWordValue( name, (int) value );
                            break;
                        case RegistryValueKind.QWord:
                            registryKey.SetQWordValue( name, (long) value );
                            break;
                        case RegistryValueKind.String:
                            registryKey.SetValue( name, (string) value );
                            break;
                        default:
                            throw new ArgumentOutOfRangeException( nameof(kind), kind, null );
                    }
                }
            }
            catch ( SecurityException )
            {
            }
            catch ( IOException )
            {
            }
        }

        private static void DeleteRegistryValue( string name, string keyName = postSharpRegistryKeyName )
        {
            try
            {
                IRegistryKey registryKey = SystemServiceLocator.GetService<IRegistryService>().OpenRegistryKey( false, true, keyName );
                if ( registryKey == null )
                {
                    registryKey = SystemServiceLocator.GetService<IRegistryService>().CreateRegistryKey( false, keyName );
                    if ( registryKey == null )
                        return;
                }

                using ( registryKey )
                {
                    registryKey.DeleteValue( name );
                }
            }
            catch ( SecurityException )
            {
            }
            catch ( IOException )
            {
            }
        }

        public static IList<string> GetLicensesToReport( IReadOnlyDictionary<string, string> licenses )
        {
            // TODO: UserSettings for Linux/Mac.
            if ( !RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                return Array.Empty<string>();
            }

            List<string> licensesToReport = new List<string>();

            try
            {
                const string subKeyName = "LicenseAudit";

                using ( IRegistryKey registryKey = OpenRegistryKey( false, true, subKeyName ) ?? CreateRegistryKey( false, subKeyName ) )
                {

                    foreach ( KeyValuePair<string, string> license in licenses )
                    {
                        string valueName = license.Key.ToString( CultureInfo.InvariantCulture );
                        DateTime? lastReportDate = registryKey.GetValueDateTime( valueName );
                        if ( !lastReportDate.HasValue || lastReportDate < GetCurrentDateTime().AddDays( -1 ) )
                        {
                            registryKey.SetValueDateTime( valueName, GetCurrentDateTime() );
                            licensesToReport.Add( license.Value );
                        }
                    }

                }

                return licensesToReport;
            }
            catch ( SecurityException )
            {
                licensesToReport.Clear();
                return licensesToReport;
            }
            catch ( IOException )
            {
                licensesToReport.Clear();
                return licensesToReport;
            }
        }

        public static void SetPostSharpProgramPaths( params string[] postSharpHqPaths )
        {
            // TODO: UserSettings for Linux/Mac.
            if ( !RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                return;
            }

            foreach ( string postSharpHqPath in postSharpHqPaths )
            {
                if ( !Directory.Exists( postSharpHqPath ) )
                    continue;

                string commandLinePath = Path.Combine( postSharpHqPath, "PostSharp.Settings.exe" );
                string uiPath = Path.Combine( postSharpHqPath, "PostSharp.Settings.UI.exe" );

                if ( File.Exists( commandLinePath ) && File.Exists( uiPath ) )
                {
                    uiProgramPath = uiPath;
                    cliProgramPath = commandLinePath;
                    break;
                }
            }
        }

        /// <exclude />
        public static string UiProgramPath
        {
            get { return uiProgramPath; }
        }

        /// <exclude />
        public static string CliProgramPath
        {
            get { return cliProgramPath; }
        }

        /// <exclude />
        public static string UploadDirectory { get; private set; }

        /// <exclude />
        public static string UploadQueueDirectory { get; internal set; }

        /// <exclude />
        public static string UploadJobsDirectory { get; private set; }

        /// <exclude />
        public static bool DisableAnyUserInterface { get; private set; }

        /// <exclude />
        public static bool LicenseMessagesDisabled
        {
            get { return licenseMessagesDisabled; }
            set
            {
                if (value != licenseMessagesDisabled)
                {
                    licenseMessagesDisabled = value;
                    SetRegistryValue("LicenseMessagesDisabled", value);
                }
            }
        }

        /// <exclude />
        public static bool AggressiveExceptionReporting
        {
            get { return aggressiveExceptionReporting; }
            set
            {
                if ( value != aggressiveExceptionReporting )
                {
                    aggressiveExceptionReporting = value;
                    SetRegistryValue( "AggressiveExceptionReporting", value );
                }
            }
        }

        /// <exclude />
        public static bool VsxWelcomePageDisplayed
        {
            get { return vsxWelcomePageDisplayed; }
            set
            {
                if ( value != vsxWelcomePageDisplayed )
                {
                    vsxWelcomePageDisplayed = value;
                    SetRegistryValue( "WelcomePageDisplayed", value );
                }
            }
        }

        /// <exclude />
        public static bool NuGetWelcomePageDisplayed
        {
            get { return nuGetWelcomePageDisplayed; }
            set
            {
                if ( value != nuGetWelcomePageDisplayed )
                {
                    nuGetWelcomePageDisplayed = value;
                    SetRegistryValue( "NuGetWelcomePageDisplayed", value );
                }
            }
        }

        public static bool DisplayCopyrightNotices
        {
            get { return displayCopyrightNotices; }
            set
            {
                if ( value != displayCopyrightNotices )
                {
                    displayCopyrightNotices = value;
                    SetRegistryValue( "DisplayCopyrightNotices", value );
                }
            }
        }


        public static bool IsOemDeployment { get { return oemDeployment; } }

        public static IRegistryKey OpenRegistryKey( bool allUsers = false, bool writable = false, string subKey = null )
        {
            string keyName = postSharpRegistryKeyName;
            if (subKey != null)
                keyName += "\\" + subKey;

            IRegistryKey registryKey = SystemServiceLocator.GetService<IRegistryService>().OpenRegistryKey( allUsers, writable, keyName );

            if ( registryKey == null && writable )
            {
                registryKey = CreateRegistryKey( allUsers, subKey );
            }

            return registryKey;
        }

        public static IRegistryKey CreateRegistryKey( bool allUsers, string subKey )
        {
            string keyName = postSharpRegistryKeyName + "\\" + subKey;
            return SystemServiceLocator.GetService<IRegistryService>().CreateRegistryKey( allUsers, keyName );
        }

        public static object GetRegistryValue( bool allUsers, string subKeyName, string valueName, object defaultValue )
        {
            using ( IRegistryKey registryKey = SystemServiceLocator.GetService<IRegistryService>().OpenRegistryKey( allUsers, subKey: subKeyName ) )
            {
                if ( registryKey == null )
                {
                    return defaultValue;
                }

                return registryKey.GetValue( valueName, defaultValue );
            }
        }

        public static void DeleteRegistrySubKeyTree( bool allUsers, string keyName )
        {
            using ( IRegistryKey registryKey = SystemServiceLocator.GetService<IRegistryService>().OpenRegistryKey( allUsers, true ) )
            {
                registryKey.DeleteSubKeyTree( keyName );
            }
        }

        public static bool TryGetDateTimeFromRegistry( IRegistryKey registryKey, string keyName, out DateTime dateTime )
        {
            DateTime? storedDateTime = registryKey.GetValueDateTime( keyName );
            if ( storedDateTime.HasValue )
            {
                dateTime = storedDateTime.Value;
                return true;
            }

            dateTime = default( DateTime );
            return false;
        }

        // TODO #28390
        public static bool IsPrerelease { get { return SystemServiceLocator.GetService<IApplicationInfoService>().IsPrerelease; } }

        // TODO #28390
        public static Version Version { get { return SystemServiceLocator.GetService<IApplicationInfoService>().Version; } }

        // TODO #28390
        public static DateTime BuildDate { get { return SystemServiceLocator.GetService<IApplicationInfoService>().BuildDate; } }

        /// <exclude />
        public static string LocalApplicationDataDirectory
        {
            get
            {
                // TODO: Remove (used from ContentDownloader).
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SharpCrafters\\PostSharp " + ProductVersion);
            }
        }

        /// <exclude />
        public static string RoamingApplicationDataDirectory
        {
            get
            {
                // TODO: Remove (not used).
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SharpCrafters\\PostSharp " + ProductVersion);
            }
        }

    
        /// <summary>
        /// Gets a device id that can be reset and does not persist from one device to another.
        /// </summary>
        public static string DeviceId { get; private set; }

        /// <exclude />
        public static string Email
        {
            get { return email; }

            set
            {
                value = value ?? "";
                if (value != email)
                {
                    email = value;
                    SetRegistryValue("Email", value, feedbackRegistryKeyName);
                }
            }
        }

        /// <exclude />
        public static ReportingAction ErrorReportingAction
        {
            get { return errorReportingAction; }
            set
            {
                if (value != errorReportingAction)
                {
                    errorReportingAction = value;
                    SetRegistryValue("ErrorReportingAction", (int)value, feedbackRegistryKeyName);
                }
            }
        }
        
        /// <exclude />
        public static ReportingAction NewExceptionReportingAction
        {
            get { return newExceptionReportingAction; }
            set
            {
                if (value != newExceptionReportingAction)
                {
                    newExceptionReportingAction = value;
                    SetRegistryValue("NewExceptionReportingAction", (int)value, feedbackRegistryKeyName);
                }
            }
        }
        
        /// <exclude />
        public static ReportingAction NewPerformanceProblemReportingAction
        {
            get { return newPerformanceProblemReportingAction; }
            set
            {
                if (value != newPerformanceProblemReportingAction)
                {
                    newPerformanceProblemReportingAction = value;
                    SetRegistryValue("NewPerformanceProblemReportingAction", (int)value, feedbackRegistryKeyName);
                }
            }
        }

        /// <exclude />
        public static ReportingAction UsageReportingAction
        {
            get { return usageReportingAction; }
            set
            {
                if (value != usageReportingAction)
                {
                    usageReportingAction = value;
                    SetRegistryValue("UsageReportingAction", (int)value, feedbackRegistryKeyName);
                }
            }
        }

        /// <exclude />
        public static DateTime UsageReportingLastQuestion
        {
            get { return usageReportingLastQuestion; }

            set
            {
                if (value != usageReportingLastQuestion)
                {
                    usageReportingLastQuestion = value;
                    SetRegistryValue("UsageReportingLastQuestion", value, feedbackRegistryKeyName);
                }
            }
        }

        /// <exclude />
        public static DateTime InstallVsxLastQuestion
        {
            get { return installVsxLastQuestion; }

            set
            {
                if (value != installVsxLastQuestion)
                {
                    installVsxLastQuestion = value;
                    // TODO rename property and registry value to 'LastInstallVsxQuestionTime'?
                    SetRegistryValue("InstallVsxLastQuestion", value);
                }
            }
        }

        /// <exclude />
        public static string PartnerId { get; set; }

        /// <exclude />
        public static bool ReportExceptions { get; set; }

        /// <exclude />
        public static bool IsWindows
        {
            get { return Environment.OSVersion.Platform == PlatformID.Win32NT; }
        }

        /// <exclude />
        public static DateTime LastUploadTime
        {
            get { return lastUploadTime; }

            set
            {
                if (value != lastUploadTime)
                {
                    lastUploadTime = value;
                    SetRegistryValue("LastUploadTime", value, feedbackRegistryKeyName);
                }
            }
        }

        /// <exclude />
        public static DateTime LastUnsuccessfulLicenseCheckTime
        {
            get { return lastUnsuccessfulLicenseCheckTime; }
            set
            {
                if ( value != lastUnsuccessfulLicenseCheckTime )
                {
                    lastUnsuccessfulLicenseCheckTime = value;
                    SetRegistryValue( "LastUnsuccessfulLicenseCheckTime", value, feedbackRegistryKeyName );
                }
            }
        }

        /// <exclude />
        public static DateTime LastLicenseExpirationMessageTime
        {
            get { return lastLicenseExpirationMessageTime; }

            set
            {
                if (value != lastLicenseExpirationMessageTime)
                {
                    lastLicenseExpirationMessageTime = value;
                    SetRegistryValue("LastLicenseExpirationMessageTime", value);
                }
            }
        }
        
        /// <exclude />
        public static bool WarnAboutSubscriptionExpiration
        {
            get { return warnAboutSubscriptionExpiration; }

            set
            {
                if (value != warnAboutSubscriptionExpiration)
                {
                    warnAboutSubscriptionExpiration = value;
                    SetRegistryValue("WarnAboutSubscriptionExpiration", value);
                }
            }
        }

        /// <exclude />
        public static DateTime LastLicenseInfoMessageTime
        {
            get { return lastLicenseInfoMessageTime; }

            set
            {
                if (value != lastLicenseInfoMessageTime)
                {
                    lastLicenseInfoMessageTime = value;
                    SetRegistryValue("LastLicenseInfoMessageTime", value);
                }
            }
        }

        /// <exclude />
        public static readonly TimeSpan PerUsageLicensingCountingPeriod = TimeSpan.FromDays( 1 );

        /// <exclude />
        private static DateTime? PerUsageLicensingCountingEnabledTime
        {
            get
            {
                if ( perUsageLicensingCountingEnabledTime.HasValue )
                {
                    if ( GetCurrentDateTime() - perUsageLicensingCountingEnabledTime.Value > PerUsageLicensingCountingPeriod )
                    {
                        DeleteRegistryValue( "PerUsageLicensingCountingEnabledTime" );
                        perUsageLicensingCountingEnabledTime = null;
                    }
                }

                return perUsageLicensingCountingEnabledTime;
            }

            set
            {
                if ( value != perUsageLicensingCountingEnabledTime )
                {
                    perUsageLicensingCountingEnabledTime = value;

                    if ( !value.HasValue )
                    {
                        DeleteRegistryValue( "PerUsageLicensingCountingEnabledTime" );
                    }
                    else
                    {
                        SetRegistryValue( "PerUsageLicensingCountingEnabledTime", value.Value );
                    }
                }
            }
        }

        // TODO: invalidation in pipe server
        /// <exclude />
        public static bool PerUsageLicensingCountingEnabled
        {
            get => PerUsageLicensingCountingEnabledTime.HasValue;
            set => PerUsageLicensingCountingEnabledTime =  value ? (DateTime?) GetCurrentDateTime() : (DateTime?) null;
        }

        /// <exclude />
        public static bool AutoDisplayLearnToolWindowUser
        {
            get { return autoDisplayLearnToolWindowUser.GetValueOrDefault(true); }
            set
            {
                if (value != AutoDisplayLearnToolWindowUser)
                {
                    autoDisplayLearnToolWindowUser = value;
                    SetRegistryValue("AutoDisplayLearnToolWindow", (bool?)value);
                }
            }
        }

        /// <exclude />
        public static DateTime LastInstallCompiledImagesQuestionTime
        {
            get { return lastInstallCompiledImagesQuestionTime; }

            set
            {
                if (value != lastInstallCompiledImagesQuestionTime)
                {
                    lastInstallCompiledImagesQuestionTime = value;
                    SetRegistryValue("LastInstallCompiledImagesQuestionTime", value);
                }
            }
        }

        /// <exclude />
        public static DateTime LastSymbolFileErrorTime
        {
            get { return lastSymbolFileErrorTime; }

            set
            {
                if (value != lastSymbolFileErrorTime)
                {
                    lastSymbolFileErrorTime = value;
                    SetRegistryValue("LastSymbolFileErrorTime", value);
                }
            }
        }

        /// <exclude />
        public static DateTime LastPrepareToolTime
        {
            get { return lastPrepareToolTime; }

            set
            {
                if ( value != lastPrepareToolTime )
                {
                    lastPrepareToolTime = value;
                    SetRegistryValue( "LastPrepareToolTime", value );
                }
            }
        }

        /// <exclude />
        public static bool AskInstallCompiledImages
        {
            get { return askInstallCompiledImages; }

            set
            {
                if (value != askInstallCompiledImages)
                {
                    askInstallCompiledImages = value;
                    SetRegistryValue("AskInstallCompiledImages", value);
                }
            }
        }

        /// <exclude />
        public static bool InstallVsxAskQuestion
        {
            get { return installVsxAskQuestion; }
            set
            {
                if (value != installVsxAskQuestion)
                {
                    installVsxAskQuestion = value;
                    SetRegistryValue("InstallVsxAskQuestion", value);
                }
            }
        }

        /// <exclude />
        public static DateTime VsxDetectedTime
        {
            get => vsxDetectedTime;
            set
            {
                if ( value != vsxDetectedTime )
                {
                    vsxDetectedTime = value;
                    SetRegistryValue( "VsxDetectedTime", value );
                }
            }
        }

        /// <exclude />
        public static Process StartUiProgram( string arguments, bool startWithElevation = false )
        {
            return StartProgram( uiProgramPath, arguments, startWithElevation );
        }

        /// <exclude />
        public static Process StartCliProgram( string arguments, bool startWithElevation = false, bool openWindow = false, bool ignoreExceptions = false )
        {
            if (!DisableAnyUserInterface && IsWindows && !string.IsNullOrEmpty(cliProgramPath))
            {
                if ( ignoreExceptions )
                {
                    try
                    {
                        return StartCliProgramCore( arguments, startWithElevation, openWindow );
                    }
                    catch
                    {

                        return null;
                    }
                }
                else
                {
                    return StartCliProgramCore(arguments, startWithElevation, openWindow);
                }
            }

            return null;
        }

        private static Process StartCliProgramCore( string arguments, bool startWithElevation, bool openWindow )
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo( cliProgramPath, arguments )
                                                {
                                                    UseShellExecute = true,
                                                };

            if ( !openWindow )
            {
                processStartInfo.CreateNoWindow = true;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }

            if ( Environment.OSVersion.Version.Major >= 6 && startWithElevation )
            {
                processStartInfo.Verb = "runas";
            }

            return Process.Start( processStartInfo );
        }

        private static Process StartProgram( string programPath, string arguments, bool startWithElevation )
        {
            if ( !DisableAnyUserInterface && IsWindows && !string.IsNullOrEmpty( programPath ) )
            {
                try
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo( programPath, arguments )
                                                        {
                                                            UseShellExecute = true,
                                                            CreateNoWindow = true,
                                                            WindowStyle = ProcessWindowStyle.Hidden
                                                        };

                    if ( Environment.OSVersion.Version.Major >= 6 && startWithElevation )
                    {
                        processStartInfo.Verb = "runas";
                    }

                    return Process.Start( processStartInfo );
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        /// <exclude />
        public static int IncrementBuildCountToday()
        {
            IRegistryKey registryKey = SystemServiceLocator.GetService<IRegistryService>().OpenRegistryKey( false, true, feedbackRegistryKeyName );
            if (registryKey == null) return -1;

            try
            {
                using (registryKey)
                {
                    DateTime lastDay = registryKey.GetValueDateTime("BuildCountDate", DateTime.MinValue);
                    registryKey.SetValueDateTime("BuildCountDate", GetCurrentDateTime().Date);

                    if (lastDay != DateTime.MinValue)
                    {
                        if (GetCurrentDateTime().Date == lastDay)
                        {
                            int builds = (int)registryKey.GetValue("BuildCount", 0) + 1;
                            registryKey.SetDWordValue( "BuildCount", builds );
                            return builds;
                        }
                    }

                    registryKey.SetDWordValue( "BuildCount", 1 );
                    return 1;
                }
            }
            catch
            {
                return -1;
            }
        }

        /// <exclude />
        public static bool TryChangeIssueStatus(string hash, ReportingStatus reportingStatus)
        {
            try
            {
                string keyName = reportedIssuesRegistryKeyName + "\\" + hash;
                IRegistryKey registryKey = SystemServiceLocator.GetService<IRegistryService>().OpenRegistryKey( false, true, keyName );

                if (registryKey == null)
                {
                    if (reportingStatus == ReportingStatus.None)
                        return false;

                    registryKey = SystemServiceLocator.GetService<IRegistryService>().CreateRegistryKey( false, keyName );

                    if (registryKey == null) return false;

                    registryKey.SetDWordValue( "Status", (int) reportingStatus );
                    registryKey.SetValueDateTime("LastStatusChangeTime", GetCurrentDateTime());
                    registryKey.Close();
                }
                else
                {
                    if (reportingStatus == ReportingStatus.None)
                    {
                        registryKey.Close();
                        DeleteRegistrySubKeyTree( false, keyName );
                        return true;
                    }

                    ReportingStatus currentStatus = (ReportingStatus)(int)registryKey.GetValue("Status", 0);
                    DateTime lastStatusChangeTime = registryKey.GetValueDateTime("LastStatusChangeTime", DateTime.MinValue);

                    // The status 'pending' is equivalent to 'none' if the issue reporting
                    // is pending for too much time.
                    if (currentStatus == ReportingStatus.Pending &&
                         lastStatusChangeTime.AddHours(1) < GetCurrentDateTime())
                    {
                        currentStatus = ReportingStatus.None;
                    }

                    if (currentStatus == reportingStatus || currentStatus == ReportingStatus.Reported)
                    {
                        registryKey.Close();
                        return false;
                    }
                    else
                    {
                        registryKey.SetDWordValue( "Status", (int) reportingStatus );
                        registryKey.SetValueDateTime("LastStatusChangeTime", GetCurrentDateTime());
                        registryKey.Close();
                        return true;
                    }
                }


                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }


    }
}