﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Testing.Services
{
    public class TestComponentInfo : IComponentInfo
    {
        public TestComponentInfo( string name, bool requiresSubscription, string? version, bool? isPrerelease, DateTime? buildDate, string? company )
        {
            this.Company = company;
            this.Name = name;
            this.Version = version;
            this.IsPrerelease = isPrerelease;
            this.BuildDate = buildDate;
            this.RequiresSubscription = requiresSubscription;
        }

        public string? Company { get; }

        public string Name { get; }

        public string? Version { get; }

        public bool? IsPrerelease { get; }

        public DateTime? BuildDate { get; }

        public bool RequiresSubscription { get; }
    }
}