// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;

namespace Metalama.Backstage.Extensibility
{
    public abstract class ComponentInfoBase : IComponentInfo
    {
        public abstract string? Company { get; }

        public abstract string Name { get; }

        public abstract string? Version { get; }

        public abstract bool? IsPrerelease { get; }

        public abstract DateTime? BuildDate { get; }

        /// <inheritdoc />
        public bool RequiresSubscription => this.Company == "PostSharp Technologies";
    }
}