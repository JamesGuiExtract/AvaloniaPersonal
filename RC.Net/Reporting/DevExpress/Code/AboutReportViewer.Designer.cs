namespace Extract.ReportingDevExpress
{
    partial class AboutReportViewer
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
            this.okButton = new System.Windows.Forms.Button();
            this.labelCompanyName = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.labelProductName = new System.Windows.Forms.Label();
            this._pictureIcon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this._pictureIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Location = new System.Drawing.Point(151, 92);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 24;
            this.okButton.Text = "&OK";
            // 
            // labelCompanyName
            // 
            this.labelCompanyName.AutoSize = true;
            this.labelCompanyName.Location = new System.Drawing.Point(50, 69);
            this.labelCompanyName.Name = "labelCompanyName";
            this.labelCompanyName.Padding = new System.Windows.Forms.Padding(3);
            this.labelCompanyName.Size = new System.Drawing.Size(88, 19);
            this.labelCompanyName.TabIndex = 0;
            this.labelCompanyName.Text = "Company Name";
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(50, 31);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Padding = new System.Windows.Forms.Padding(3);
            this.labelVersion.Size = new System.Drawing.Size(48, 19);
            this.labelVersion.TabIndex = 1;
            this.labelVersion.Text = "Version";
            // 
            // labelCopyright
            // 
            this.labelCopyright.AutoSize = true;
            this.labelCopyright.Location = new System.Drawing.Point(50, 50);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Padding = new System.Windows.Forms.Padding(3);
            this.labelCopyright.Size = new System.Drawing.Size(57, 19);
            this.labelCopyright.TabIndex = 2;
            this.labelCopyright.Text = "Copyright";
            // 
            // labelProductName
            // 
            this.labelProductName.AutoSize = true;
            this.labelProductName.Location = new System.Drawing.Point(50, 12);
            this.labelProductName.Name = "labelProductName";
            this.labelProductName.Padding = new System.Windows.Forms.Padding(3);
            this.labelProductName.Size = new System.Drawing.Size(81, 19);
            this.labelProductName.TabIndex = 3;
            this.labelProductName.Text = "Product Name";
            // 
            // _pictureIcon
            // 
            this._pictureIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this._pictureIcon.Location = new System.Drawing.Point(12, 18);
            this._pictureIcon.Name = "_pictureIcon";
            this._pictureIcon.Size = new System.Drawing.Size(32, 32);
            this._pictureIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._pictureIcon.TabIndex = 25;
            this._pictureIcon.TabStop = false;
            // 
            // AboutReportViewer
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(238, 127);
            this.Controls.Add(this._pictureIcon);
            this.Controls.Add(this.labelCompanyName);
            this.Controls.Add(this.labelCopyright);
            this.Controls.Add(this.labelProductName);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutReportViewer";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About Report Viewer";
            ((System.ComponentModel.ISupportInitialize)(this._pictureIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label labelProductName;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelCompanyName;
        private System.Windows.Forms.PictureBox _pictureIcon;
    }
}
