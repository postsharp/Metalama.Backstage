﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Telemetry.Metrics;
using System;
using System.Globalization;

namespace Metalama.Backstage.Licensing.Audit;

internal class LicenseAuditReport : MetricsBase
{
    public int AuditHashCode { get; }
    
    public IComponentInfo ReportedComponent { get; }

    public LicenseAuditReport(
        IServiceProvider serviceProvider,
        string licenseString )
        : base( serviceProvider, "LicenseAudit" )
    {
        var time = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();

        this.ReportedComponent = serviceProvider
            .GetRequiredBackstageService<IApplicationInfoProvider>()
            .CurrentApplication
            .GetLatestComponentMadeByPostSharp();

        var usageReporter = serviceProvider.GetRequiredBackstageService<IUsageReporter>();
        var buildDate = this.ReportedComponent.BuildDate ?? throw new InvalidOperationException( $"Build date of '{this.ReportedComponent.Name}' application is unknown." );
        var telemetryConfiguration = serviceProvider.GetRequiredBackstageService<IConfigurationManager>().Get<TelemetryConfiguration>();
        var userHash = LicenseCryptography.ComputeStringHash64( Environment.UserName );
        var machineHash = LicenseCryptography.ComputeStringHash64( telemetryConfiguration.DeviceId.ToString() );

        HashCode auditHashCodeBuilder = default;

        void AddToMetricsAndHashCode( Metric metric )
        {
            this.Metrics.Add( metric );
            auditHashCodeBuilder.Add( metric.ToString() );
        }

        // Audit date is not part of the audit hash code. 
        this.Metrics.Add( new LicenseAuditDateMetric( "Date", time.Now.Date ) );
        AddToMetricsAndHashCode( new StringMetric( "Version", this.ReportedComponent.Version ) );
        AddToMetricsAndHashCode( new LicenseAuditDateMetric( "BuildDate", buildDate ) );
        AddToMetricsAndHashCode( new StringMetric( "License", licenseString ) );
        AddToMetricsAndHashCode( new LicenseAuditHashMetric( "User", userHash ) );
        AddToMetricsAndHashCode( new LicenseAuditHashMetric( "Machine", machineHash ) );
        AddToMetricsAndHashCode( new BoolMetric( "CEIP", usageReporter.IsUsageReportingEnabled() ) );
        AddToMetricsAndHashCode( new StringMetric( "ApplicationName", this.ReportedComponent.Name ) );

        this.AuditHashCode = auditHashCodeBuilder.ToHashCode();
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

        public override string ToString() => this.Value.ToString( "d", CultureInfo.InvariantCulture );

        public override bool SetValue( object? value ) => throw new NotImplementedException();
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

        public override string ToString() => this.Value.ToString( "x", CultureInfo.InvariantCulture );

        public override bool SetValue( object? value ) => throw new NotImplementedException();
    }
}