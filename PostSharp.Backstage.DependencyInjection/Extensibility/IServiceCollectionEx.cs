// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.DependencyInjection.Extensibility
{
    public interface IServiceCollectionEx : IServiceCollection, IBackstageServiceCollection { }
}