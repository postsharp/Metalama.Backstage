// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using PostSharp.Backstage.Licensing.Licenses;

namespace ProstSharp.LicenseKeyGenerator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            this.InitializeComponent();
            this._propertyGrid.SelectedObject = new LicenseKeyData();
        }

        private void OnSerializedButtonClicked( object sender, EventArgs e )
        {
            var kvUri = "https://postsharpbusinesssystkv.vault.azure.net/";
            var client = new SecretClient( new Uri( kvUri ), new DefaultAzureCredential() );
            var privateKey = client.GetSecret( "Licensing-PrivateKey0" ).Value.Value;

            var licenseKeyData = (LicenseKeyData) this._propertyGrid.SelectedObject;
            licenseKeyData.SignAndSerialize( 0, privateKey );

            if ( !licenseKeyData.VerifySignature() )
            {
                throw new InvalidOperationException();
            }

            var licenseKey = licenseKeyData.Serialize();

            var deserializedLicenseKeyData = LicenseKeyData.Deserialize( licenseKey );

            if ( !deserializedLicenseKeyData.VerifySignature() )
            {
                throw new InvalidOperationException();
            }

            this._propertyGrid.SelectedObject = licenseKeyData;
        }
    }
}