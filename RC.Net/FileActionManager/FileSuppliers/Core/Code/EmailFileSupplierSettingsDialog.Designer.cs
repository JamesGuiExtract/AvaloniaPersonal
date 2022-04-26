namespace Extract.FileActionManager.FileSuppliers
{
    partial class EmailFileSupplierSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EmailFileSupplierSettingsDialog));
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._downloadDirectoryTextBox = new System.Windows.Forms.TextBox();
            this._downloadDirectoryBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._downloadDirectoryPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._emailSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this._emailSettingsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._inputFolderPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._inputFolderTextBox = new System.Windows.Forms.TextBox();
            this._postDownloadFolderPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._postDownloadFolderTextBox = new System.Windows.Forms.TextBox();
            this._postDownloadFolderLabel = new System.Windows.Forms.Label();
            this._sharedEmailAddressLabel = new System.Windows.Forms.Label();
            this._sharedEmailAddressTextBox = new System.Windows.Forms.TextBox();
            this._inputFolderLabel = new System.Windows.Forms.Label();
            this._sharedEmailAddressInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._inputFolderInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._sharedEmailAddressPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this.failedDownloadFolderPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._failedDownloadFolderTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.failedDownloadFolderInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._postDownloadFolderInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._downloadSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this._downloadSettingsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._downloadDirectoryLabel = new System.Windows.Forms.Label();
            this._downloadDirectoryInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._noteAboutAzureTabLabel = new System.Windows.Forms.Label();
            this._emailSettingsGroupBox.SuspendLayout();
            this._emailSettingsTableLayoutPanel.SuspendLayout();
            this._downloadSettingsGroupBox.SuspendLayout();
            this._downloadSettingsTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(471, 255);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 22;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(390, 255);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 21;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _downloadDirectoryTextBox
            // 
            this._downloadDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._downloadDirectoryTextBox.Location = new System.Drawing.Point(107, 3);
            this._downloadDirectoryTextBox.Name = "_downloadDirectoryTextBox";
            this._downloadDirectoryTextBox.Size = new System.Drawing.Size(328, 20);
            this._downloadDirectoryTextBox.TabIndex = 17;
            // 
            // _downloadDirectoryBrowseButton
            // 
            this._downloadDirectoryBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._downloadDirectoryBrowseButton.EnsureFileExists = false;
            this._downloadDirectoryBrowseButton.EnsurePathExists = false;
            this._downloadDirectoryBrowseButton.FolderBrowser = true;
            this._downloadDirectoryBrowseButton.Location = new System.Drawing.Point(465, 3);
            this._downloadDirectoryBrowseButton.Name = "_downloadDirectoryBrowseButton";
            this._downloadDirectoryBrowseButton.Size = new System.Drawing.Size(26, 20);
            this._downloadDirectoryBrowseButton.TabIndex = 198;
            this._downloadDirectoryBrowseButton.Text = "...";
            this._downloadDirectoryBrowseButton.TextControl = this._downloadDirectoryTextBox;
            this._downloadDirectoryBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _downloadDirectoryPathTagButton
            // 
            this._downloadDirectoryPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._downloadDirectoryPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_downloadDirectoryPathTagButton.Image")));
            this._downloadDirectoryPathTagButton.Location = new System.Drawing.Point(441, 3);
            this._downloadDirectoryPathTagButton.Name = "_downloadDirectoryPathTagButton";
            this._downloadDirectoryPathTagButton.Size = new System.Drawing.Size(18, 20);
            this._downloadDirectoryPathTagButton.TabIndex = 18;
            this._downloadDirectoryPathTagButton.TextControl = this._downloadDirectoryTextBox;
            this._downloadDirectoryPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _emailSettingsGroupBox
            // 
            this._emailSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailSettingsGroupBox.Controls.Add(this._emailSettingsTableLayoutPanel);
            this._emailSettingsGroupBox.Location = new System.Drawing.Point(12, 29);
            this._emailSettingsGroupBox.Name = "_emailSettingsGroupBox";
            this._emailSettingsGroupBox.Size = new System.Drawing.Size(534, 141);
            this._emailSettingsGroupBox.TabIndex = 1;
            this._emailSettingsGroupBox.TabStop = false;
            this._emailSettingsGroupBox.Text = "Email settings";
            // 
            // _emailSettingsTableLayoutPanel
            // 
            this._emailSettingsTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailSettingsTableLayoutPanel.AutoSize = true;
            this._emailSettingsTableLayoutPanel.ColumnCount = 4;
            this._emailSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._emailSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._emailSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._emailSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._emailSettingsTableLayoutPanel.Controls.Add(this._inputFolderPathTagButton, 2, 1);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._postDownloadFolderPathTagButton, 2, 2);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._postDownloadFolderTextBox, 1, 2);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._postDownloadFolderLabel, 0, 2);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._sharedEmailAddressLabel, 0, 0);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._sharedEmailAddressTextBox, 1, 0);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._inputFolderLabel, 0, 1);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._inputFolderTextBox, 1, 1);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._sharedEmailAddressInfoTip, 3, 0);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._inputFolderInfoTip, 3, 1);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._sharedEmailAddressPathTagButton, 2, 0);
            this._emailSettingsTableLayoutPanel.Controls.Add(this.failedDownloadFolderPathTagButton, 2, 3);
            this._emailSettingsTableLayoutPanel.Controls.Add(this.label1, 0, 3);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._failedDownloadFolderTextBox, 1, 3);
            this._emailSettingsTableLayoutPanel.Controls.Add(this.failedDownloadFolderInfoTip, 3, 3);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._postDownloadFolderInfoTip, 3, 2);
            this._emailSettingsTableLayoutPanel.Location = new System.Drawing.Point(9, 22);
            this._emailSettingsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(9, 6, 9, 9);
            this._emailSettingsTableLayoutPanel.Name = "_emailSettingsTableLayoutPanel";
            this._emailSettingsTableLayoutPanel.RowCount = 4;
            this._emailSettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._emailSettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._emailSettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._emailSettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._emailSettingsTableLayoutPanel.Size = new System.Drawing.Size(516, 106);
            this._emailSettingsTableLayoutPanel.TabIndex = 0;
            // 
            // _inputFolderPathTagButton
            // 
            this._inputFolderPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._inputFolderPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_inputFolderPathTagButton.Image")));
            this._inputFolderPathTagButton.Location = new System.Drawing.Point(473, 29);
            this._inputFolderPathTagButton.Name = "_inputFolderPathTagButton";
            this._inputFolderPathTagButton.Size = new System.Drawing.Size(18, 20);
            this._inputFolderPathTagButton.TabIndex = 6;
            this._inputFolderPathTagButton.TextControl = this._inputFolderTextBox;
            this._inputFolderPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _inputFolderTextBox
            // 
            this._inputFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._inputFolderTextBox.Location = new System.Drawing.Point(119, 29);
            this._inputFolderTextBox.Name = "_inputFolderTextBox";
            this._inputFolderTextBox.Size = new System.Drawing.Size(348, 20);
            this._inputFolderTextBox.TabIndex = 5;
            // 
            // _postDownloadFolderPathTagButton
            // 
            this._postDownloadFolderPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._postDownloadFolderPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_postDownloadFolderPathTagButton.Image")));
            this._postDownloadFolderPathTagButton.Location = new System.Drawing.Point(473, 55);
            this._postDownloadFolderPathTagButton.Name = "_postDownloadFolderPathTagButton";
            this._postDownloadFolderPathTagButton.Size = new System.Drawing.Size(18, 20);
            this._postDownloadFolderPathTagButton.TabIndex = 10;
            this._postDownloadFolderPathTagButton.TextControl = this._postDownloadFolderTextBox;
            this._postDownloadFolderPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _postDownloadFolderTextBox
            // 
            this._postDownloadFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._postDownloadFolderTextBox.Location = new System.Drawing.Point(119, 55);
            this._postDownloadFolderTextBox.Name = "_postDownloadFolderTextBox";
            this._postDownloadFolderTextBox.Size = new System.Drawing.Size(348, 20);
            this._postDownloadFolderTextBox.TabIndex = 9;
            // 
            // _postDownloadFolderLabel
            // 
            this._postDownloadFolderLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._postDownloadFolderLabel.AutoSize = true;
            this._postDownloadFolderLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._postDownloadFolderLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._postDownloadFolderLabel.Location = new System.Drawing.Point(0, 58);
            this._postDownloadFolderLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this._postDownloadFolderLabel.Name = "_postDownloadFolderLabel";
            this._postDownloadFolderLabel.Size = new System.Drawing.Size(109, 13);
            this._postDownloadFolderLabel.TabIndex = 8;
            this._postDownloadFolderLabel.Text = "P&ost-download folder:";
            // 
            // _sharedEmailAddressLabel
            // 
            this._sharedEmailAddressLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._sharedEmailAddressLabel.AutoSize = true;
            this._sharedEmailAddressLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._sharedEmailAddressLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._sharedEmailAddressLabel.Location = new System.Drawing.Point(0, 6);
            this._sharedEmailAddressLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this._sharedEmailAddressLabel.Name = "_sharedEmailAddressLabel";
            this._sharedEmailAddressLabel.Size = new System.Drawing.Size(111, 13);
            this._sharedEmailAddressLabel.TabIndex = 0;
            this._sharedEmailAddressLabel.Text = "Shared &email address:";
            // 
            // _sharedEmailAddressTextBox
            // 
            this._sharedEmailAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._sharedEmailAddressTextBox.Location = new System.Drawing.Point(119, 3);
            this._sharedEmailAddressTextBox.Name = "_sharedEmailAddressTextBox";
            this._sharedEmailAddressTextBox.Size = new System.Drawing.Size(348, 20);
            this._sharedEmailAddressTextBox.TabIndex = 1;
            // 
            // _inputFolderLabel
            // 
            this._inputFolderLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._inputFolderLabel.AutoSize = true;
            this._inputFolderLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._inputFolderLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._inputFolderLabel.Location = new System.Drawing.Point(0, 32);
            this._inputFolderLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this._inputFolderLabel.Name = "_inputFolderLabel";
            this._inputFolderLabel.Size = new System.Drawing.Size(63, 13);
            this._inputFolderLabel.TabIndex = 4;
            this._inputFolderLabel.Text = "&Input folder:";
            // 
            // _sharedEmailAddressInfoTip
            // 
            this._sharedEmailAddressInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._sharedEmailAddressInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_sharedEmailAddressInfoTip.BackgroundImage")));
            this._sharedEmailAddressInfoTip.Location = new System.Drawing.Point(497, 3);
            this._sharedEmailAddressInfoTip.Name = "_sharedEmailAddressInfoTip";
            this._sharedEmailAddressInfoTip.Size = new System.Drawing.Size(16, 16);
            this._sharedEmailAddressInfoTip.TabIndex = 3;
            this._sharedEmailAddressInfoTip.TabStop = false;
            this._sharedEmailAddressInfoTip.TipText = "The email address that emails will be read from. This is probably different than " +
    "the username that is used for logging-in to the account";
            // 
            // _inputFolderInfoTip
            // 
            this._inputFolderInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._inputFolderInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_inputFolderInfoTip.BackgroundImage")));
            this._inputFolderInfoTip.Location = new System.Drawing.Point(497, 29);
            this._inputFolderInfoTip.Name = "_inputFolderInfoTip";
            this._inputFolderInfoTip.Size = new System.Drawing.Size(16, 16);
            this._inputFolderInfoTip.TabIndex = 7;
            this._inputFolderInfoTip.TabStop = false;
            this._inputFolderInfoTip.TipText = "The email folder that will be checked for emails to download";
            // 
            // _sharedEmailAddressPathTagButton
            // 
            this._sharedEmailAddressPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sharedEmailAddressPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_sharedEmailAddressPathTagButton.Image")));
            this._sharedEmailAddressPathTagButton.Location = new System.Drawing.Point(473, 3);
            this._sharedEmailAddressPathTagButton.Name = "_sharedEmailAddressPathTagButton";
            this._sharedEmailAddressPathTagButton.Size = new System.Drawing.Size(18, 20);
            this._sharedEmailAddressPathTagButton.TabIndex = 2;
            this._sharedEmailAddressPathTagButton.TextControl = this._sharedEmailAddressTextBox;
            this._sharedEmailAddressPathTagButton.UseVisualStyleBackColor = true;
            // 
            // failedDownloadFolderPathTagButton
            // 
            this.failedDownloadFolderPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.failedDownloadFolderPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("failedDownloadFolderPathTagButton.Image")));
            this.failedDownloadFolderPathTagButton.Location = new System.Drawing.Point(473, 82);
            this.failedDownloadFolderPathTagButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 3);
            this.failedDownloadFolderPathTagButton.Name = "failedDownloadFolderPathTagButton";
            this.failedDownloadFolderPathTagButton.Size = new System.Drawing.Size(18, 20);
            this.failedDownloadFolderPathTagButton.TabIndex = 14;
            this.failedDownloadFolderPathTagButton.TextControl = this._failedDownloadFolderTextBox;
            this.failedDownloadFolderPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _failedDownloadFolderTextBox
            // 
            this._failedDownloadFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._failedDownloadFolderTextBox.Location = new System.Drawing.Point(119, 82);
            this._failedDownloadFolderTextBox.Name = "_failedDownloadFolderTextBox";
            this._failedDownloadFolderTextBox.Size = new System.Drawing.Size(348, 20);
            this._failedDownloadFolderTextBox.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(0, 85);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "&Failed download folder";
            // 
            // failedDownloadFolderInfoTip
            // 
            this.failedDownloadFolderInfoTip.BackColor = System.Drawing.Color.Transparent;
            this.failedDownloadFolderInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("failedDownloadFolderInfoTip.BackgroundImage")));
            this.failedDownloadFolderInfoTip.Location = new System.Drawing.Point(497, 81);
            this.failedDownloadFolderInfoTip.Name = "failedDownloadFolderInfoTip";
            this.failedDownloadFolderInfoTip.Size = new System.Drawing.Size(16, 16);
            this.failedDownloadFolderInfoTip.TabIndex = 15;
            this.failedDownloadFolderInfoTip.TabStop = false;
            this.failedDownloadFolderInfoTip.TipText = "The email folder where emails will be moved if they fail to downloaded\r\n";
            // 
            // _postDownloadFolderInfoTip
            // 
            this._postDownloadFolderInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._postDownloadFolderInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_postDownloadFolderInfoTip.BackgroundImage")));
            this._postDownloadFolderInfoTip.Location = new System.Drawing.Point(497, 55);
            this._postDownloadFolderInfoTip.Name = "_postDownloadFolderInfoTip";
            this._postDownloadFolderInfoTip.Size = new System.Drawing.Size(16, 16);
            this._postDownloadFolderInfoTip.TabIndex = 11;
            this._postDownloadFolderInfoTip.TabStop = false;
            this._postDownloadFolderInfoTip.TipText = "The email folder where emails will be moved after they are downloaded\r\n";
            // 
            // _downloadSettingsGroupBox
            // 
            this._downloadSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._downloadSettingsGroupBox.Controls.Add(this._downloadSettingsTableLayoutPanel);
            this._downloadSettingsGroupBox.Location = new System.Drawing.Point(12, 176);
            this._downloadSettingsGroupBox.Name = "_downloadSettingsGroupBox";
            this._downloadSettingsGroupBox.Size = new System.Drawing.Size(534, 59);
            this._downloadSettingsGroupBox.TabIndex = 2;
            this._downloadSettingsGroupBox.TabStop = false;
            this._downloadSettingsGroupBox.Text = "Download settings";
            // 
            // _downloadSettingsTableLayoutPanel
            // 
            this._downloadSettingsTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._downloadSettingsTableLayoutPanel.AutoSize = true;
            this._downloadSettingsTableLayoutPanel.ColumnCount = 5;
            this._downloadSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._downloadSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._downloadSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._downloadSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._downloadSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._downloadSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._downloadSettingsTableLayoutPanel.Controls.Add(this._downloadDirectoryBrowseButton, 3, 0);
            this._downloadSettingsTableLayoutPanel.Controls.Add(this._downloadDirectoryPathTagButton, 2, 0);
            this._downloadSettingsTableLayoutPanel.Controls.Add(this._downloadDirectoryTextBox, 1, 0);
            this._downloadSettingsTableLayoutPanel.Controls.Add(this._downloadDirectoryLabel, 0, 0);
            this._downloadSettingsTableLayoutPanel.Controls.Add(this._downloadDirectoryInfoTip, 4, 0);
            this._downloadSettingsTableLayoutPanel.Location = new System.Drawing.Point(9, 22);
            this._downloadSettingsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(9, 6, 9, 9);
            this._downloadSettingsTableLayoutPanel.Name = "_downloadSettingsTableLayoutPanel";
            this._downloadSettingsTableLayoutPanel.RowCount = 2;
            this._downloadSettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._downloadSettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._downloadSettingsTableLayoutPanel.Size = new System.Drawing.Size(516, 26);
            this._downloadSettingsTableLayoutPanel.TabIndex = 0;
            // 
            // _downloadDirectoryLabel
            // 
            this._downloadDirectoryLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._downloadDirectoryLabel.AutoSize = true;
            this._downloadDirectoryLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._downloadDirectoryLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._downloadDirectoryLabel.Location = new System.Drawing.Point(0, 6);
            this._downloadDirectoryLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this._downloadDirectoryLabel.Name = "_downloadDirectoryLabel";
            this._downloadDirectoryLabel.Size = new System.Drawing.Size(101, 13);
            this._downloadDirectoryLabel.TabIndex = 16;
            this._downloadDirectoryLabel.Text = "&Download directory:";
            // 
            // _downloadDirectoryInfoTip
            // 
            this._downloadDirectoryInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._downloadDirectoryInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_downloadDirectoryInfoTip.BackgroundImage")));
            this._downloadDirectoryInfoTip.Location = new System.Drawing.Point(497, 3);
            this._downloadDirectoryInfoTip.Name = "_downloadDirectoryInfoTip";
            this._downloadDirectoryInfoTip.Size = new System.Drawing.Size(16, 16);
            this._downloadDirectoryInfoTip.TabIndex = 20;
            this._downloadDirectoryInfoTip.TabStop = false;
            this._downloadDirectoryInfoTip.TipText = "The file system folder where emails will be downloaded to";
            // 
            // _noteAboutAzureTabLabel
            // 
            this._noteAboutAzureTabLabel.AutoSize = true;
            this._noteAboutAzureTabLabel.Location = new System.Drawing.Point(12, 9);
            this._noteAboutAzureTabLabel.Name = "_noteAboutAzureTabLabel";
            this._noteAboutAzureTabLabel.Size = new System.Drawing.Size(387, 13);
            this._noteAboutAzureTabLabel.TabIndex = 18;
            this._noteAboutAzureTabLabel.Text = "Credentials must be configured in DBAdmin (Database->Database options->Azure)";
            // 
            // EmailFileSupplierSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(558, 294);
            this.Controls.Add(this._noteAboutAzureTabLabel);
            this.Controls.Add(this._downloadSettingsGroupBox);
            this.Controls.Add(this._emailSettingsGroupBox);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 1000);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(463, 300);
            this.Name = "EmailFileSupplierSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Email file supplier settings";
            this._emailSettingsGroupBox.ResumeLayout(false);
            this._emailSettingsGroupBox.PerformLayout();
            this._emailSettingsTableLayoutPanel.ResumeLayout(false);
            this._emailSettingsTableLayoutPanel.PerformLayout();
            this._downloadSettingsGroupBox.ResumeLayout(false);
            this._downloadSettingsGroupBox.PerformLayout();
            this._downloadSettingsTableLayoutPanel.ResumeLayout(false);
            this._downloadSettingsTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TextBox _downloadDirectoryTextBox;
        private Forms.FileActionManagerPathTagButton _downloadDirectoryPathTagButton;
        private Utilities.Forms.BrowseButton _downloadDirectoryBrowseButton;
        private System.Windows.Forms.GroupBox _emailSettingsGroupBox;
        private System.Windows.Forms.TableLayoutPanel _emailSettingsTableLayoutPanel;
        private System.Windows.Forms.TextBox _postDownloadFolderTextBox;
        private System.Windows.Forms.Label _postDownloadFolderLabel;
        private System.Windows.Forms.Label _sharedEmailAddressLabel;
        private System.Windows.Forms.TextBox _sharedEmailAddressTextBox;
        private System.Windows.Forms.Label _inputFolderLabel;
        private System.Windows.Forms.TextBox _inputFolderTextBox;
        private System.Windows.Forms.GroupBox _downloadSettingsGroupBox;
        private System.Windows.Forms.TableLayoutPanel _downloadSettingsTableLayoutPanel;
        private System.Windows.Forms.Label _downloadDirectoryLabel;
        private Utilities.Forms.InfoTip _sharedEmailAddressInfoTip;
        private Utilities.Forms.InfoTip _postDownloadFolderInfoTip;
        private Utilities.Forms.InfoTip _inputFolderInfoTip;
        private Utilities.Forms.InfoTip _downloadDirectoryInfoTip;
        private Forms.FileActionManagerPathTagButton _inputFolderPathTagButton;
        private Forms.FileActionManagerPathTagButton _postDownloadFolderPathTagButton;
        private Forms.FileActionManagerPathTagButton _sharedEmailAddressPathTagButton;
        private System.Windows.Forms.Label _noteAboutAzureTabLabel;
        private Forms.FileActionManagerPathTagButton failedDownloadFolderPathTagButton;
        private System.Windows.Forms.TextBox _failedDownloadFolderTextBox;
        private System.Windows.Forms.Label label1;
        private Utilities.Forms.InfoTip failedDownloadFolderInfoTip;
    }
}
