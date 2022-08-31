// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Extensibility;
using System.Linq;

namespace Metalama.Backstage.Licensing
{
    internal static class ApplicationInfoLicenseExtensions
    {
        public static bool IsPreviewLicenseEligible( this IApplicationInfo application )
            => ((IComponentInfo) application).IsPreviewLicenseEligible() || application.Components.Any( c => c.IsPreviewLicenseEligible() );
    }
}