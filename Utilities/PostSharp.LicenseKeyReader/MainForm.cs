// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing.Licenses;

namespace PostSharp.LicenseKeyReader
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            this.InitializeComponent();
        }

        private void OnReadButtonClicked( object sender, EventArgs e )
        {
            try
            {
                this._propertyGrid.SelectedObject = null;

                var licenseKeyData = LicenseKeyData.Deserialize( this._licenseKeyTextBox.Text );

                if ( !licenseKeyData.VerifySignature() )
                {
                    throw new InvalidOperationException( "Failed to verify the license key signature" );
                }

                this._propertyGrid.SelectedObject = licenseKeyData;
            }
            catch ( Exception exception )
            {
                MessageBox.Show( this, exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }
    }
}