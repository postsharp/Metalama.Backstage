// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Utilities;

// This service is intentionally not a part of ProcessUtilities.IsUnattendedProcess to avoid licensing enforcement
// to depend on variable factors like last user input or monitor size.
internal interface IUserInteractionService : IBackstageService
{
    int? GetTotalMonitorWidth();

    TimeSpan? GetLastInputTime();
}