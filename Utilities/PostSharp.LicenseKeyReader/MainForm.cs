using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;

namespace PostSharp.LicenseKeyReader
{
    public partial class MainForm : Form
    {
        private readonly LicenseFactory _licenseFactory;
        private readonly IBackstageDiagnosticSink _diagnostics;

        public MainForm()
        {
            this.InitializeComponent();

            var log = ServiceProvider.Instance.GetRequiredService<EventBackEndLogger>();
            log.DiagnosticsReported += ( sender, message ) => this.logTextBox.AppendText( message );

            this._licenseFactory = ServiceProvider.Instance.GetRequiredService<LicenseFactory>();
            this._diagnostics = ServiceProvider.Instance.GetRequiredService<IBackstageDiagnosticSink>();
        }

        private void OnReadButtonClicked( object sender, EventArgs e )
        {
            try
            {
                this.propertyGrid.SelectedObject = null;
                this.logTextBox.Text = "";

                if ( !this._licenseFactory.TryCreate( this.licenseKeyTextBox.Text, out var license ) )
                {
                    return;
                }

                if ( !((License) license).TryGetLicenseKeyData( out var licenseKeyData ) )
                {
                    return;
                }

                if ( !licenseKeyData.VerifySignature() )
                {
                    this._diagnostics.ReportError( "Failed to verify the signature" );
                    return;
                }

                this.propertyGrid.SelectedObject = licenseKeyData;
            }
            catch ( Exception exception )
            {
                this._diagnostics.ReportError( $"Exception: {exception}" );
            }
        }
    }
}