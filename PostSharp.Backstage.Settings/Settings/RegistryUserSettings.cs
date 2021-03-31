// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace PostSharp.Backstage.Settings
{
    public sealed class RegistryUserSettings : UserSettings
    {
        private const string ProductVersion = "3";

        /// <exclude/>
        public const string FeedbackDirectoryName = @"SharpCrafters\PostSharp " + ProductVersion + @"\Feedback";

        private const string feedbackRegistryKeyName = @"Software\\" + FeedbackDirectoryName;
        private const string postSharpRegistryKeyName = "Software\\SharpCrafters\\PostSharp " + ProductVersion;
        private const string reportedIssuesRegistryKeyName = postSharpRegistryKeyName + @"\ReportedIssues";


    }
}
