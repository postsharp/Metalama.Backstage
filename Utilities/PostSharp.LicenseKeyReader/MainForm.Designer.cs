namespace PostSharp.LicenseKeyReader
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
            this.licenseKeyTextBox = new System.Windows.Forms.TextBox();
            this.readButton = new System.Windows.Forms.Button();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // licenseKeyTextBox
            // 
            this.licenseKeyTextBox.AcceptsReturn = true;
            this.licenseKeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.licenseKeyTextBox.Location = new System.Drawing.Point(12, 12);
            this.licenseKeyTextBox.Multiline = true;
            this.licenseKeyTextBox.Name = "licenseKeyTextBox";
            this.licenseKeyTextBox.Size = new System.Drawing.Size(1059, 110);
            this.licenseKeyTextBox.TabIndex = 0;
            // 
            // readButton
            // 
            this.readButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.readButton.Location = new System.Drawing.Point(959, 126);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(112, 34);
            this.readButton.TabIndex = 1;
            this.readButton.Text = "Read";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Click += new System.EventHandler(this.OnReadButtonClicked);
            // 
            // propertyGrid
            // 
            this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid.Enabled = false;
            this.propertyGrid.Location = new System.Drawing.Point(12, 128);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(1059, 478);
            this.propertyGrid.TabIndex = 2;
            // 
            // logTextBox
            // 
            this.logTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logTextBox.Location = new System.Drawing.Point(12, 612);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.logTextBox.Size = new System.Drawing.Size(1059, 168);
            this.logTextBox.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1083, 792);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.readButton);
            this.Controls.Add(this.licenseKeyTextBox);
            this.Controls.Add(this.propertyGrid);
            this.Name = "MainForm";
            this.Text = "PostSharp License Key Reader";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox licenseKeyTextBox;
        private Button readButton;
        private PropertyGrid propertyGrid;
        private TextBox logTextBox;
    }
}