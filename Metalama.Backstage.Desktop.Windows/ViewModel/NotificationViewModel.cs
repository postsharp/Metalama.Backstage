﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Desktop.Windows.Commands;

internal record NotificationViewModel( string Kind, string Title, string Body, NotificationActionViewModel Action );