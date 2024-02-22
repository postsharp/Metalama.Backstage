// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using K4os.Hash.xxHash;
using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Telemetry.Metrics;
using System;
using System.Globalization;
using System.Text;

namespace Metalama.Backstage.Licensing.Audit;

internal class LicenseAuditReport : MetricsBase
{
    public long AuditHashCode { get; }

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

        var buildDate = this.ReportedComponent.BuildDate
                        ?? throw new InvalidOperationException( $"Build date of '{this.ReportedComponent.Name}' application is unknown." );

        var telemetryConfiguration = serviceProvider.GetRequiredBackstageService<IConfigurationManager>().Get<TelemetryConfiguration>();
        var userHash = LicenseCryptography.ComputeStringHash64( Environment.UserName );
        var machineHash = LicenseCryptography.ComputeStringHash64( telemetryConfiguration.DeviceId.ToString() );

        var auditHashCodeBuilder = new XXH64();

        void AddToMetricsAndHashCode( Metric metric )
        {
            this.Metrics.Add( metric );
            auditHashCodeBuilder.Update( Encoding.UTF8.GetBytes( metric.ToString() ?? "" ) );
        }

        // Audit date is not part of the audit hash code. 
        this.Metrics.Add( new LicenseAuditDateMetric( "Date", time.Now.Date ) );
        AddToMetricsAndHashCode( new StringMetric( "Version", this.ReportedComponent.Version ) );
        AddToMetricsAndHashCode( new LicenseAuditDateMetric( "BuildDate", buildDate ) );
        AddToMetricsAndHashCode( new StringMetric( "License", licenseString ) );
        AddToMetricsAndHashCode( new LicenseAuditHashMetric( "User", userHash ) );
        AddToMetricsAndHashCode( new LicenseAuditHashMetric( "Machine", machineHash ) );
        AddToMetricsAndHashCode( new BoolMetric( "CEIP", usageReporter.IsUsageReportingEnabled ) );
        AddToMetricsAndHashCode( new StringMetric( "ApplicationName", this.ReportedComponent.Name ) );

        this.AuditHashCode = unchecked((long) auditHashCodeBuilder.Digest());
    }

    /// <summary>
    /// Date metric implementation based on
    /// PostSharp.Hosting.Program.WriteLicenseAudit method.
    /// </summary>
    private class LicenseAuditDateMetric : Metric
    {
        private readonly DateTime _value;

        public LicenseAuditDateMetric( string name, DateTime value )
            : base( name )
        {
            this._value = value;
        }

        public override string ToString() => this._value.ToString( "d", CultureInfo.InvariantCulture );

        public override bool SetValue( object? value ) => throw new NotImplementedException();
    }

    /// <summary>
    /// Hash metric implementation based on
    /// PostSharp.Hosting.Program.WriteLicenseAudit method.
    /// </summary>
    private class LicenseAuditHashMetric : Metric
    {
        private readonly long _value;

        public LicenseAuditHashMetric( string name, long value )
            : base( name )
        {
            this._value = value;
        }

        public override string ToString() => this._value.ToString( "x", CultureInfo.InvariantCulture );

        public override bool SetValue( object? value ) => throw new NotImplementedException();
    }
}