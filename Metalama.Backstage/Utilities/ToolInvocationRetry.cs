// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Text.RegularExpressions;

namespace Metalama.Backstage.Utilities;

public record ToolInvocationRetry( Regex? Regex, int? ExitCode );