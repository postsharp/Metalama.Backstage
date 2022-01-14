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
            this.propertyGrid1.SelectedObject = new LicenseKeyData();
        }

        private void OnSerializedButtonClicked( object sender, EventArgs e )
        {
            var kvUri = "https://postsharpbusinesssystkv.vault.azure.net/";
            var client = new SecretClient( new Uri( kvUri ), new DefaultAzureCredential() );
            var privateKey = client.GetSecret( "Licensing-PrivateKey0" ).Value.Value;

            var licenseKeyData = (LicenseKeyData) this.propertyGrid1.SelectedObject;
            licenseKeyData.Sign( 0, privateKey );
            var licenseKey = licenseKeyData.Serialize();
            licenseKeyData = LicenseKeyData.Deserialize(licenseKey);
            
            if (!licenseKeyData.VerifySignature())
            {
                throw new InvalidOperationException();
            }

            this.propertyGrid1.SelectedObject = licenseKeyData;
        }
    }
}