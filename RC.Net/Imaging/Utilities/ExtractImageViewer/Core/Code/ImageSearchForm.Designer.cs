namespace Extract.Imaging.Utilities.ExtractImageViewer
{
    partial class ImageSearchForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            this._textRootFolder = new System.Windows.Forms.TextBox();
            this._browseRootFolder = new Extract.Utilities.Forms.BrowseButton();
            this._comboImageExtension = new Extract.Utilities.Forms.BetterComboBox();
            this._textImageNameEnd = new System.Windows.Forms.TextBox();
            this._btnFindAndOpen = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(87, 13);
            label1.TabIndex = 0;
            label1.Text = "Select root folder";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 48);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(167, 13);
            label2.TabIndex = 3;
            label2.Text = "Select image extension (e.g. \".tif\")";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 88);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(118, 13);
            label3.TabIndex = 5;
            label3.Text = "Select image name end";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(12, 101);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(251, 13);
            label4.TabIndex = 6;
            label4.Text = "(e.g. To find Test01.tif and not Test02.tif specify 01)";
            // 
            // _textRootFolder
            // 
            this._textRootFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._textRootFolder.Location = new System.Drawing.Point(12, 25);
            this._textRootFolder.Name = "_textRootFolder";
            this._textRootFolder.Size = new System.Drawing.Size(274, 20);
            this._textRootFolder.TabIndex = 1;
            // 
            // _browseRootFolder
            // 
            this._browseRootFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._browseRootFolder.FolderBrowser = true;
            this._browseRootFolder.Location = new System.Drawing.Point(292, 24);
            this._browseRootFolder.Name = "_browseRootFolder";
            this._browseRootFolder.Size = new System.Drawing.Size(27, 20);
            this._browseRootFolder.TabIndex = 2;
            this._browseRootFolder.Text = "...";
            this._browseRootFolder.TextControl = this._textRootFolder;
            this._browseRootFolder.UseVisualStyleBackColor = true;
            // 
            // _comboImageExtension
            // 
            this._comboImageExtension.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._comboImageExtension.FormattingEnabled = true;
            this._comboImageExtension.Items.AddRange(new object[] {
            ".tif",
            ".tiff",
            ".pdf"});
            this._comboImageExtension.Location = new System.Drawing.Point(12, 64);
            this._comboImageExtension.Name = "_comboImageExtension";
            this._comboImageExtension.Size = new System.Drawing.Size(307, 21);
            this._comboImageExtension.TabIndex = 4;
            // 
            // _textImageNameEnd
            // 
            this._textImageNameEnd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._textImageNameEnd.Location = new System.Drawing.Point(12, 117);
            this._textImageNameEnd.Name = "_textImageNameEnd";
            this._textImageNameEnd.Size = new System.Drawing.Size(307, 20);
            this._textImageNameEnd.TabIndex = 7;
            // 
            // _btnFindAndOpen
            // 
            this._btnFindAndOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnFindAndOpen.Location = new System.Drawing.Point(204, 143);
            this._btnFindAndOpen.Name = "_btnFindAndOpen";
            this._btnFindAndOpen.Size = new System.Drawing.Size(115, 23);
            this._btnFindAndOpen.TabIndex = 8;
            this._btnFindAndOpen.Text = "Find && Open Image";
            this._btnFindAndOpen.UseVisualStyleBackColor = true;
            this._btnFindAndOpen.Click += new System.EventHandler(this.HandleFindAndOpenClick);
            // 
            // ImageSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 178);
            this.Controls.Add(this._btnFindAndOpen);
            this.Controls.Add(this._textImageNameEnd);
            this.Controls.Add(label4);
            this.Controls.Add(label3);
            this.Controls.Add(this._comboImageExtension);
            this.Controls.Add(label2);
            this.Controls.Add(this._browseRootFolder);
            this.Controls.Add(this._textRootFolder);
            this.Controls.Add(label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImageSearchForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Image Search Form";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _textRootFolder;
        private Extract.Utilities.Forms.BrowseButton _browseRootFolder;
        private Extract.Utilities.Forms.BetterComboBox _comboImageExtension;
        private System.Windows.Forms.TextBox _textImageNameEnd;
        private System.Windows.Forms.Button _btnFindAndOpen;
    }
}