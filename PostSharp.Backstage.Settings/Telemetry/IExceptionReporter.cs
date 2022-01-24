// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Telemetry;

public interface IExceptionReporter
{
    void ReportException( Exception e, ExceptionReportingKind exceptionReportingKind = ExceptionReportingKind.Exception );
}