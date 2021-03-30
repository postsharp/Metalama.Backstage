// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Reflection;

namespace PostSharp.Backstage.Licensing
{
    /// <summary>
    /// Message emitted by the <see cref="UserLicenseManager"/>.
    /// </summary>
    [Serializable]
    [Obfuscation( Exclude = true )]
    internal sealed class LicenseMessage
    {
        internal LicenseMessage( LicenseMessageKind kind, string text, bool isError, TimeSpan frequency )
        {
            this.Kind = kind;
            this.Text = text;
            this.IsError = isError;
            this.Frequency = frequency;
        }

        public LicenseMessageKind Kind { get; private set; }

        /// <summary>
        /// Gets the message text.
        /// </summary>
        [Obfuscation( Exclude = true )]
        public string Text { [Obfuscation( Exclude = true )]
        get; private set; }

        /// <summary>
        /// Gets the message severity.
        /// </summary>
        [Obfuscation( Exclude = true )]
        public bool IsError { [Obfuscation( Exclude = true )]
        get; private set; }

        [Obfuscation( Exclude = true )]
        public TimeSpan Frequency { [Obfuscation( Exclude = true )]
        get; private set; }

        public override string ToString()
        {
            return this.Text;
        }
    }

    internal enum LicenseMessageKind
    {
        InsufficientLicense,
        HappyMonday,
        Expired,
        Expiring,
        Invalid,
        SubscriptionExpiring
    }

    [Serializable]
    [Obfuscation( Exclude = true )]
    internal sealed class LicenseEventArgs : EventArgs
    {
        internal LicenseEventArgs( LicenseMessage message )
        {
            Message = message;
        }

        [Obfuscation( Exclude = true )]
        public LicenseMessage Message { get; private set; }
    }
}