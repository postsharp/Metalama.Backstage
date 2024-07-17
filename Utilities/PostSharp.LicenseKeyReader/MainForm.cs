// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Licenses;

namespace PostSharp.LicenseKeyReader
{
    public partial class MainForm : Form
    {
        private readonly IServiceProvider _services = new NullServiceProvider();
        
        public MainForm()
        {
            this.InitializeComponent();
        }

        public void ShowError( string message )
        {
            MessageBox.Show( this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
        }

        private void OnReadButtonClicked( object sender, EventArgs e )
        {
            this._propertyGrid.SelectedObject = null;

            if ( !LicenseKeyData.TryDeserialize( this._licenseKeyTextBox.Text, this._services, out var licenseKeyData, out var errorMessage ) )
            {
                this.ShowError( errorMessage );

                return;
            }

            if ( !licenseKeyData.VerifySignature() )
            {
                this.ShowError( "Failed to verify the license key signature" );

                return;
            }

            this._propertyGrid.SelectedObject = licenseKeyData;
        }
    }
}