// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage.Licensing
{
    internal static class ComponentInfoLicenseExtensions
    {
        public static bool IsPreviewLicenseEligible( this IComponentInfo component )
            => (component.IsPrerelease ?? false) && component.BuildDate.HasValue && component.RequiresSubscription;
    }
}