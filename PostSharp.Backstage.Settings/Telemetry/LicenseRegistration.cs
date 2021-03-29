using System;

namespace PostSharp.Backstage.Telemetry
{
    // TODO
    public class LicenseRegistration
    {
        public bool Enabled { get; set; }
        public ReportedValue ReportedLicensedProduct { get; set; }
        public ReportedValue ReportedLicenseType { get; set; }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Flush(bool v)
        {
            throw new NotImplementedException();
        }
    }
}
