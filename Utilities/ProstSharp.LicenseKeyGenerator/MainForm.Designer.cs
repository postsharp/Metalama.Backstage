namespace ProstSharp.LicenseKeyGenerator
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this._serializeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.Location = new System.Drawing.Point(12, 12);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(776, 460);
            this.propertyGrid1.TabIndex = 0;
            // 
            // _serializeButton
            // 
            this._serializeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._serializeButton.Location = new System.Drawing.Point(676, 478);
            this._serializeButton.Name = "_serializeButton";
            this._serializeButton.Size = new System.Drawing.Size(112, 34);
            this._serializeButton.TabIndex = 1;
            this._serializeButton.Text = "Serialize";
            this._serializeButton.UseVisualStyleBackColor = true;
            this._serializeButton.Click += new System.EventHandler(this.OnSerializedButtonClicked);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 524);
            this.Controls.Add(this._serializeButton);
            this.Controls.Add(this.propertyGrid1);
            this.Name = "MainForm";
            this.Text = "PostSharp License Key Generator";
            this.ResumeLayout(false);

        }

        #endregion

        private PropertyGrid propertyGrid1;
        private Button _serializeButton;
    }
}