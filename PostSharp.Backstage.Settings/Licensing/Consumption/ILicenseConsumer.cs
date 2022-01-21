// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Consumption
{
    /// <summary>
    /// Provides information about a type consuming requiring a licensed feature.
    /// </summary>
    public interface ILicenseConsumer
    {
        /// <summary>
        /// Gets the namespace of the type requiring a licensed feature.
        /// </summary>
        public string? TargetTypeNamespace { get; }

        /// <summary>
        /// Gets the name of the type requiring a licensed feature.
        /// </summary>
        public string? TargetTypeName { get; }

        /// <summary>
        /// Gets <see cref="IBackstageDiagnosticSink" /> specific to the location of the licensed feature request.
        /// </summary>
        public IBackstageDiagnosticSink DiagnosticsSink { get; }

        /// <summary>
        /// Gets <see cref="IDiagnosticLocation" /> of the licensed feature request.
        /// </summary>
        public IDiagnosticLocation? DiagnosticLocation { get; }
    }
}