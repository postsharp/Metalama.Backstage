// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.ComponentModel;

namespace Metalama.Backstage.UserInterface;

[ConfigurationFile( "toastNotifications.json" )]
[Description( "Toast notifications." )]
public record ToastNotificationsConfiguration : ConfigurationFile
{
    [JsonProperty( "notifications" )]
    public ImmutableDictionary<string, ToastNotificationConfiguration> Notifications { get; init; } =
        ImmutableDictionary<string, ToastNotificationConfiguration>.Empty.WithComparers( StringComparer.OrdinalIgnoreCase );
}