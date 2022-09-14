// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Telemetry.Metrics;
using System;

namespace Metalama.Backstage.Licensing.Audit;

internal class LicenseAuditReport : MetricsBase
{
    public LicenseAuditReport(
        IServiceProvider serviceProvider,
        string licenseString )
        : base( serviceProvider, "LicenseAudit" )
    {
        var time = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();

        var application = serviceProvider
            .GetRequiredBackstageService<IApplicationInfoProvider>()
            .CurrentApplication
            .GetLatestComponentLicensedByBuildDate();

        var usageReporter = serviceProvider.GetRequiredBackstageService<IUsageReporter>();
        var buildDate = application.BuildDate ?? throw new InvalidOperationException( $"Build date of '{application.Name}' application is unknown." );
        var telemetryConfiguration = serviceProvider.GetRequiredBackstageService<IConfigurationManager>().Get<TelemetryConfiguration>();
        var userHash = LicenseCryptography.ComputeStringHash64( Environment.UserName );
        var machineHash = LicenseCryptography.ComputeStringHash64( telemetryConfiguration.DeviceId.ToString() );

        this.Metrics.Add( new LicenseAuditDateMetric( "Date", time.Now ) );
        this.Metrics.Add( new StringMetric( "Version", application.Version ) );
        this.Metrics.Add( new LicenseAuditDateMetric( "BuildDate", buildDate ) );
        this.Metrics.Add( new StringMetric( "License", licenseString ) );
        this.Metrics.Add( new LicenseAuditHashMetric( "User", userHash ) );
        this.Metrics.Add( new LicenseAuditHashMetric( "Machine", machineHash ) );
        this.Metrics.Add( new BoolMetric( "CEIP", usageReporter.IsUsageReportingEnabled() ) );
        this.Metrics.Add( new StringMetric( "ApplicationName", application.Name ) );
    }

    /// <summary>
    /// Date metric implementation based on
    /// PostSharp.Hosting.Program.WriteLicenseAudit method.
    /// </summary>
    private class LicenseAuditDateMetric : Metric
    {
        public LicenseAuditDateMetric( string name ) : base( name ) { }

        public LicenseAuditDateMetric( string name, DateTime value )
            : base( name )
        {
            this.Value = value;
        }

        public DateTime Value { get; set; }

        public override string ToString() => $"{this.Value:d}";

        public override bool SetValue( object? value ) => throw new NotImplementedException();

        protected override void BuildHashCode( HashCode hashCode ) => hashCode.Add( this.Value );
    }

    /// <summary>
    /// Hash metric implementation based on
    /// PostSharp.Hosting.Program.WriteLicenseAudit method.
    /// </summary>
    private class LicenseAuditHashMetric : Metric
    {
        public LicenseAuditHashMetric( string name ) : base( name ) { }

        public LicenseAuditHashMetric( string name, long value )
            : base( name )
        {
            this.Value = value;
        }

        public long Value { get; set; }

        public override string ToString() => $"{this.Value:x}";

        public override bool SetValue( object? value ) => throw new NotImplementedException();

        protected override void BuildHashCode( HashCode hashCode ) => hashCode.Add( this.Value );
    }
}