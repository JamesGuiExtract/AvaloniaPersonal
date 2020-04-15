namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    partial class AboutForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                if (_logoImage.Image != null)
                {
                    _logoImage.Image.Dispose();
                    _logoImage.Image = null;
                }

                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._labelProductName = new System.Windows.Forms.Label();
            this._labelFrameworkVersion = new System.Windows.Forms.Label();
            this._labelCopyright = new System.Windows.Forms.Label();
            this._labelCompanyName = new System.Windows.Forms.Label();
            this._textBoxDescription = new System.Windows.Forms.TextBox();
            this._licenseTextBox = new System.Windows.Forms.TextBox();
            this._okButton = new System.Windows.Forms.Button();
            this._labelLicenseInformation = new System.Windows.Forms.Label();
            this._linkLabelWebsite = new System.Windows.Forms.LinkLabel();
            this._logoImage = new System.Windows.Forms.PictureBox();
            this._labelProductVersion = new System.Windows.Forms.Label();
            this._tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._logoImage)).BeginInit();
            this.SuspendLayout();
            // 
            // _tableLayoutPanel
            // 
            this._tableLayoutPanel.ColumnCount = 1;
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanel.Controls.Add(this._labelProductName, 0, 1);
            this._tableLayoutPanel.Controls.Add(this._labelFrameworkVersion, 0, 3);
            this._tableLayoutPanel.Controls.Add(this._labelCopyright, 0, 4);
            this._tableLayoutPanel.Controls.Add(this._labelCompanyName, 0, 5);
            this._tableLayoutPanel.Controls.Add(this._textBoxDescription, 0, 6);
            this._tableLayoutPanel.Controls.Add(this._linkLabelWebsite, 0, 7);
            this._tableLayoutPanel.Controls.Add(this._labelLicenseInformation, 0, 8);
            this._tableLayoutPanel.Controls.Add(this._licenseTextBox, 0, 9);
            this._tableLayoutPanel.Controls.Add(this._okButton, 0, 10);
            this._tableLayoutPanel.Controls.Add(this._logoImage, 0, 0);
            this._tableLayoutPanel.Controls.Add(this._labelProductVersion, 0, 2);
            this._tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tableLayoutPanel.Location = new System.Drawing.Point(9, 9);
            this._tableLayoutPanel.Name = "_tableLayoutPanel";
            this._tableLayoutPanel.RowCount = 10;
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75.00172F));
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 24.99828F));
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._tableLayoutPanel.Size = new System.Drawing.Size(566, 519);
            this._tableLayoutPanel.TabIndex = 0;
            // 
            // _labelProductName
            // 
            this._labelProductName.Dock = System.Windows.Forms.DockStyle.Fill;
            this._labelProductName.Location = new System.Drawing.Point(3, 153);
            this._labelProductName.Margin = new System.Windows.Forms.Padding(3);
            this._labelProductName.MaximumSize = new System.Drawing.Size(0, 17);
            this._labelProductName.Name = "_labelProductName";
            this._labelProductName.Size = new System.Drawing.Size(560, 17);
            this._labelProductName.TabIndex = 19;
            this._labelProductName.Text = "Product Name";
            this._labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _labelFrameworkVersion
            // 
            this._labelFrameworkVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this._labelFrameworkVersion.Location = new System.Drawing.Point(3, 199);
            this._labelFrameworkVersion.Margin = new System.Windows.Forms.Padding(3);
            this._labelFrameworkVersion.MaximumSize = new System.Drawing.Size(0, 17);
            this._labelFrameworkVersion.Name = "_labelFrameworkVersion";
            this._labelFrameworkVersion.Size = new System.Drawing.Size(560, 17);
            this._labelFrameworkVersion.TabIndex = 0;
            this._labelFrameworkVersion.Text = "Framework Version";
            this._labelFrameworkVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _labelCopyright
            // 
            this._labelCopyright.Dock = System.Windows.Forms.DockStyle.Fill;
            this._labelCopyright.Location = new System.Drawing.Point(3, 222);
            this._labelCopyright.Margin = new System.Windows.Forms.Padding(3);
            this._labelCopyright.MaximumSize = new System.Drawing.Size(0, 17);
            this._labelCopyright.Name = "_labelCopyright";
            this._labelCopyright.Size = new System.Drawing.Size(560, 17);
            this._labelCopyright.TabIndex = 21;
            this._labelCopyright.Text = "Copyright";
            this._labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _labelCompanyName
            // 
            this._labelCompanyName.Dock = System.Windows.Forms.DockStyle.Fill;
            this._labelCompanyName.Location = new System.Drawing.Point(3, 245);
            this._labelCompanyName.Margin = new System.Windows.Forms.Padding(3);
            this._labelCompanyName.MaximumSize = new System.Drawing.Size(0, 17);
            this._labelCompanyName.Name = "_labelCompanyName";
            this._labelCompanyName.Size = new System.Drawing.Size(560, 17);
            this._labelCompanyName.TabIndex = 22;
            this._labelCompanyName.Text = "Company Name";
            this._labelCompanyName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _textBoxDescription
            // 
            this._textBoxDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this._textBoxDescription.Location = new System.Drawing.Point(3, 268);
            this._textBoxDescription.Multiline = true;
            this._textBoxDescription.Name = "_textBoxDescription";
            this._textBoxDescription.ReadOnly = true;
            this._textBoxDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._textBoxDescription.Size = new System.Drawing.Size(560, 64);
            this._textBoxDescription.TabIndex = 23;
            this._textBoxDescription.TabStop = false;
            this._textBoxDescription.Text = "Description";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._okButton.Location = new System.Drawing.Point(488, 495);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 21);
            this._okButton.TabIndex = 24;
            this._okButton.Text = "&OK";
            // 
            // _labelLicenseInformation
            // 
            this._labelLicenseInformation.AutoSize = true;
            this._labelLicenseInformation.Dock = System.Windows.Forms.DockStyle.Fill;
            this._labelLicenseInformation.Location = new System.Drawing.Point(3, 354);
            this._labelLicenseInformation.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this._labelLicenseInformation.Name = "_labelLicenseInformation";
            this._labelLicenseInformation.Size = new System.Drawing.Size(560, 13);
            this._labelLicenseInformation.TabIndex = 27;
            this._labelLicenseInformation.Text = "License Information";
            this._labelLicenseInformation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _licenseTextBox
            // 
            this._licenseTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._licenseTextBox.Location = new System.Drawing.Point(3, 268);
            this._licenseTextBox.Multiline = true;
            this._licenseTextBox.Name = "_licenseTextBox";
            this._licenseTextBox.ReadOnly = true;
            this._licenseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._licenseTextBox.Size = new System.Drawing.Size(560, 64);
            this._licenseTextBox.TabIndex = 23;
            this._licenseTextBox.TabStop = false;
            // 
            // _linkLabelWebsite
            // 
            this._linkLabelWebsite.AutoSize = true;
            this._linkLabelWebsite.Dock = System.Windows.Forms.DockStyle.Fill;
            this._linkLabelWebsite.Location = new System.Drawing.Point(3, 338);
            this._linkLabelWebsite.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this._linkLabelWebsite.Name = "_linkLabelWebsite";
            this._linkLabelWebsite.Size = new System.Drawing.Size(560, 13);
            this._linkLabelWebsite.TabIndex = 26;
            this._linkLabelWebsite.TabStop = true;
            this._linkLabelWebsite.Text = "Website";
            this._linkLabelWebsite.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._linkLabelWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleLinkLabelWebsiteClick);
            // 
            // _logoImage
            // 
            this._logoImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._logoImage.ErrorImage = null;
            this._logoImage.InitialImage = null;
            this._logoImage.Location = new System.Drawing.Point(3, 3);
            this._logoImage.Name = "_logoImage";
            this._logoImage.Size = new System.Drawing.Size(560, 144);
            this._logoImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this._logoImage.TabIndex = 28;
            this._logoImage.TabStop = false;
            // 
            // _labelProductVersion
            // 
            this._labelProductVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this._labelProductVersion.Location = new System.Drawing.Point(3, 176);
            this._labelProductVersion.Margin = new System.Windows.Forms.Padding(3);
            this._labelProductVersion.MaximumSize = new System.Drawing.Size(0, 17);
            this._labelProductVersion.Name = "_labelProductVersion";
            this._labelProductVersion.Size = new System.Drawing.Size(560, 17);
            this._labelProductVersion.TabIndex = 29;
            this._labelProductVersion.Text = "Product Version";
            this._labelProductVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AboutForm
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 537);
            this.Controls.Add(this._tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            this._tableLayoutPanel.ResumeLayout(false);
            this._tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._logoImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _tableLayoutPanel;
        private System.Windows.Forms.Label _labelProductName;
        private System.Windows.Forms.Label _labelFrameworkVersion;
        private System.Windows.Forms.Label _labelCopyright;
        private System.Windows.Forms.Label _labelCompanyName;
        private System.Windows.Forms.TextBox _textBoxDescription;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.LinkLabel _linkLabelWebsite;
        private System.Windows.Forms.Label _labelLicenseInformation;
        private System.Windows.Forms.TextBox _licenseTextBox;
        private System.Windows.Forms.PictureBox _logoImage;
        private System.Windows.Forms.Label _labelProductVersion;
    }
}
