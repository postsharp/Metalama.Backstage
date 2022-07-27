// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;

namespace Metalama.Backstage.Telemetry;

public interface IExceptionReporter
{
    void ReportException( Exception reportedException, ExceptionReportingKind exceptionReportingKind = ExceptionReportingKind.Exception );
}