// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Desktop.Windows.Commands;

internal record OpenWebPageActionViewModel( string Text, string Url ) : NotificationActionViewModel( Text );