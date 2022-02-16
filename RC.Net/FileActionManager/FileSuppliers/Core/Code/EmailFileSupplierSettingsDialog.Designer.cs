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
            this._loginGroupBox = new System.Windows.Forms.GroupBox();
            this._loginTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._usernameLabel = new System.Windows.Forms.Label();
            this._usernameTextBox = new System.Windows.Forms.TextBox();
            this._passwordLabel = new System.Windows.Forms.Label();
            this._passwordTextBox = new System.Windows.Forms.TextBox();
            this._usernameInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._passwordInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._emailSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this._emailSettingsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._postDownloadFolderTextBox = new System.Windows.Forms.TextBox();
            this._postDownloadFolderLabel = new System.Windows.Forms.Label();
            this._sharedEmailAddressLabel = new System.Windows.Forms.Label();
            this._sharedEmailAddressTextBox = new System.Windows.Forms.TextBox();
            this._inputFolderLabel = new System.Windows.Forms.Label();
            this._inputFolderTextBox = new System.Windows.Forms.TextBox();
            this._sharedEmailAddressInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._inputFolderInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._postDownloadFolderInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._downloadSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this._downloadSettingsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._downloadDirectoryLabel = new System.Windows.Forms.Label();
            this._downloadDirectoryInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._loginGroupBox.SuspendLayout();
            this._loginTableLayoutPanel.SuspendLayout();
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
            this._cancelButton.Location = new System.Drawing.Point(471, 292);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 4;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(390, 292);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 3;
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
            this._downloadDirectoryTextBox.TabIndex = 1;
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
            this._downloadDirectoryBrowseButton.TabIndex = 3;
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
            this._downloadDirectoryPathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._downloadDirectoryPathTagButton.Size = new System.Drawing.Size(18, 20);
            this._downloadDirectoryPathTagButton.TabIndex = 2;
            this._downloadDirectoryPathTagButton.TextControl = this._downloadDirectoryTextBox;
            this._downloadDirectoryPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _loginGroupBox
            // 
            this._loginGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._loginGroupBox.Controls.Add(this._loginTableLayoutPanel);
            this._loginGroupBox.Location = new System.Drawing.Point(12, 12);
            this._loginGroupBox.Name = "_loginGroupBox";
            this._loginGroupBox.Size = new System.Drawing.Size(534, 82);
            this._loginGroupBox.TabIndex = 0;
            this._loginGroupBox.TabStop = false;
            this._loginGroupBox.Text = "Login credentials";
            // 
            // _loginTableLayoutPanel
            // 
            this._loginTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._loginTableLayoutPanel.AutoSize = true;
            this._loginTableLayoutPanel.ColumnCount = 3;
            this._loginTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._loginTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._loginTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._loginTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._loginTableLayoutPanel.Controls.Add(this._usernameLabel, 0, 0);
            this._loginTableLayoutPanel.Controls.Add(this._usernameTextBox, 1, 0);
            this._loginTableLayoutPanel.Controls.Add(this._passwordLabel, 0, 1);
            this._loginTableLayoutPanel.Controls.Add(this._passwordTextBox, 1, 1);
            this._loginTableLayoutPanel.Controls.Add(this._usernameInfoTip, 2, 0);
            this._loginTableLayoutPanel.Controls.Add(this._passwordInfoTip, 2, 1);
            this._loginTableLayoutPanel.Location = new System.Drawing.Point(6, 22);
            this._loginTableLayoutPanel.Margin = new System.Windows.Forms.Padding(9, 6, 9, 9);
            this._loginTableLayoutPanel.Name = "_loginTableLayoutPanel";
            this._loginTableLayoutPanel.RowCount = 2;
            this._loginTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._loginTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._loginTableLayoutPanel.Size = new System.Drawing.Size(516, 52);
            this._loginTableLayoutPanel.TabIndex = 0;
            // 
            // _usernameLabel
            // 
            this._usernameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._usernameLabel.AutoSize = true;
            this._usernameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._usernameLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._usernameLabel.Location = new System.Drawing.Point(0, 6);
            this._usernameLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this._usernameLabel.Name = "_usernameLabel";
            this._usernameLabel.Size = new System.Drawing.Size(58, 13);
            this._usernameLabel.TabIndex = 0;
            this._usernameLabel.Text = "&Username:";
            // 
            // _usernameTextBox
            // 
            this._usernameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._usernameTextBox.Location = new System.Drawing.Point(64, 3);
            this._usernameTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this._usernameTextBox.Name = "_usernameTextBox";
            this._usernameTextBox.Size = new System.Drawing.Size(430, 20);
            this._usernameTextBox.TabIndex = 1;
            // 
            // _passwordLabel
            // 
            this._passwordLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._passwordLabel.AutoSize = true;
            this._passwordLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._passwordLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._passwordLabel.Location = new System.Drawing.Point(0, 32);
            this._passwordLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this._passwordLabel.Name = "_passwordLabel";
            this._passwordLabel.Size = new System.Drawing.Size(56, 13);
            this._passwordLabel.TabIndex = 3;
            this._passwordLabel.Text = "&Password:";
            // 
            // _passwordTextBox
            // 
            this._passwordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._passwordTextBox.Location = new System.Drawing.Point(64, 29);
            this._passwordTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this._passwordTextBox.Name = "_passwordTextBox";
            this._passwordTextBox.Size = new System.Drawing.Size(430, 20);
            this._passwordTextBox.TabIndex = 4;
            this._passwordTextBox.UseSystemPasswordChar = true;
            // 
            // _usernameInfoTip
            // 
            this._usernameInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._usernameInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_usernameInfoTip.BackgroundImage")));
            this._usernameInfoTip.Location = new System.Drawing.Point(497, 3);
            this._usernameInfoTip.Name = "_usernameInfoTip";
            this._usernameInfoTip.Size = new System.Drawing.Size(16, 16);
            this._usernameInfoTip.TabIndex = 2;
            this._usernameInfoTip.TabStop = false;
            this._usernameInfoTip.TipText = "The name used to connect to the email account. Must include the domain, e.g., joh" +
    "n.doe@company.com";
            // 
            // _passwordInfoTip
            // 
            this._passwordInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._passwordInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_passwordInfoTip.BackgroundImage")));
            this._passwordInfoTip.Location = new System.Drawing.Point(497, 29);
            this._passwordInfoTip.Name = "_passwordInfoTip";
            this._passwordInfoTip.Size = new System.Drawing.Size(16, 16);
            this._passwordInfoTip.TabIndex = 5;
            this._passwordInfoTip.TabStop = false;
            this._passwordInfoTip.TipText = "The password will be securely saved with this configuration";
            // 
            // _emailSettingsGroupBox
            // 
            this._emailSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailSettingsGroupBox.Controls.Add(this._emailSettingsTableLayoutPanel);
            this._emailSettingsGroupBox.Location = new System.Drawing.Point(12, 100);
            this._emailSettingsGroupBox.Name = "_emailSettingsGroupBox";
            this._emailSettingsGroupBox.Size = new System.Drawing.Size(534, 108);
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
            this._emailSettingsTableLayoutPanel.ColumnCount = 3;
            this._emailSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._emailSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._emailSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._emailSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._emailSettingsTableLayoutPanel.Controls.Add(this._postDownloadFolderTextBox, 1, 2);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._postDownloadFolderLabel, 0, 2);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._sharedEmailAddressLabel, 0, 0);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._sharedEmailAddressTextBox, 1, 0);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._inputFolderLabel, 0, 1);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._inputFolderTextBox, 1, 1);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._sharedEmailAddressInfoTip, 2, 0);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._inputFolderInfoTip, 2, 1);
            this._emailSettingsTableLayoutPanel.Controls.Add(this._postDownloadFolderInfoTip, 2, 2);
            this._emailSettingsTableLayoutPanel.Location = new System.Drawing.Point(9, 22);
            this._emailSettingsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(9, 6, 9, 9);
            this._emailSettingsTableLayoutPanel.Name = "_emailSettingsTableLayoutPanel";
            this._emailSettingsTableLayoutPanel.RowCount = 3;
            this._emailSettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._emailSettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._emailSettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._emailSettingsTableLayoutPanel.Size = new System.Drawing.Size(516, 78);
            this._emailSettingsTableLayoutPanel.TabIndex = 0;
            // 
            // _postDownloadFolderTextBox
            // 
            this._postDownloadFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._postDownloadFolderTextBox.Location = new System.Drawing.Point(117, 55);
            this._postDownloadFolderTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this._postDownloadFolderTextBox.Name = "_postDownloadFolderTextBox";
            this._postDownloadFolderTextBox.Size = new System.Drawing.Size(377, 20);
            this._postDownloadFolderTextBox.TabIndex = 7;
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
            this._postDownloadFolderLabel.TabIndex = 6;
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
            this._sharedEmailAddressTextBox.Location = new System.Drawing.Point(117, 3);
            this._sharedEmailAddressTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this._sharedEmailAddressTextBox.Name = "_sharedEmailAddressTextBox";
            this._sharedEmailAddressTextBox.Size = new System.Drawing.Size(377, 20);
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
            this._inputFolderLabel.TabIndex = 3;
            this._inputFolderLabel.Text = "&Input folder:";
            // 
            // _inputFolderTextBox
            // 
            this._inputFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._inputFolderTextBox.Location = new System.Drawing.Point(117, 29);
            this._inputFolderTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this._inputFolderTextBox.Name = "_inputFolderTextBox";
            this._inputFolderTextBox.Size = new System.Drawing.Size(377, 20);
            this._inputFolderTextBox.TabIndex = 4;
            // 
            // _sharedEmailAddressInfoTip
            // 
            this._sharedEmailAddressInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._sharedEmailAddressInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_sharedEmailAddressInfoTip.BackgroundImage")));
            this._sharedEmailAddressInfoTip.Location = new System.Drawing.Point(497, 3);
            this._sharedEmailAddressInfoTip.Name = "_sharedEmailAddressInfoTip";
            this._sharedEmailAddressInfoTip.Size = new System.Drawing.Size(16, 16);
            this._sharedEmailAddressInfoTip.TabIndex = 2;
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
            this._inputFolderInfoTip.TabIndex = 5;
            this._inputFolderInfoTip.TabStop = false;
            this._inputFolderInfoTip.TipText = "The email folder that will be checked for emails to download";
            // 
            // _postDownloadFolderInfoTip
            // 
            this._postDownloadFolderInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._postDownloadFolderInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_postDownloadFolderInfoTip.BackgroundImage")));
            this._postDownloadFolderInfoTip.Location = new System.Drawing.Point(497, 55);
            this._postDownloadFolderInfoTip.Name = "_postDownloadFolderInfoTip";
            this._postDownloadFolderInfoTip.Size = new System.Drawing.Size(16, 16);
            this._postDownloadFolderInfoTip.TabIndex = 8;
            this._postDownloadFolderInfoTip.TabStop = false;
            this._postDownloadFolderInfoTip.TipText = "The email folder where emails will be moved after they are downloaded\r\n";
            // 
            // _downloadSettingsGroupBox
            // 
            this._downloadSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._downloadSettingsGroupBox.Controls.Add(this._downloadSettingsTableLayoutPanel);
            this._downloadSettingsGroupBox.Location = new System.Drawing.Point(12, 214);
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
            this._downloadDirectoryLabel.TabIndex = 0;
            this._downloadDirectoryLabel.Text = "&Download directory:";
            // 
            // _downloadDirectoryInfoTip
            // 
            this._downloadDirectoryInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._downloadDirectoryInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_downloadDirectoryInfoTip.BackgroundImage")));
            this._downloadDirectoryInfoTip.Location = new System.Drawing.Point(497, 3);
            this._downloadDirectoryInfoTip.Name = "_downloadDirectoryInfoTip";
            this._downloadDirectoryInfoTip.Size = new System.Drawing.Size(16, 16);
            this._downloadDirectoryInfoTip.TabIndex = 4;
            this._downloadDirectoryInfoTip.TabStop = false;
            this._downloadDirectoryInfoTip.TipText = "The file system folder where emails will be downloaded to";
            // 
            // EmailFileSupplierSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(558, 331);
            this.Controls.Add(this._downloadSettingsGroupBox);
            this.Controls.Add(this._emailSettingsGroupBox);
            this.Controls.Add(this._loginGroupBox);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 1000);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(463, 370);
            this.Name = "EmailFileSupplierSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Email file supplier settings";
            this._loginGroupBox.ResumeLayout(false);
            this._loginGroupBox.PerformLayout();
            this._loginTableLayoutPanel.ResumeLayout(false);
            this._loginTableLayoutPanel.PerformLayout();
            this._emailSettingsGroupBox.ResumeLayout(false);
            this._emailSettingsGroupBox.PerformLayout();
            this._emailSettingsTableLayoutPanel.ResumeLayout(false);
            this._emailSettingsTableLayoutPanel.PerformLayout();
            this._downloadSettingsGroupBox.ResumeLayout(false);
            this._downloadSettingsGroupBox.PerformLayout();
            this._downloadSettingsTableLayoutPanel.ResumeLayout(false);
            this._downloadSettingsTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TextBox _downloadDirectoryTextBox;
        private Forms.FileActionManagerPathTagButton _downloadDirectoryPathTagButton;
        private Utilities.Forms.BrowseButton _downloadDirectoryBrowseButton;
        private System.Windows.Forms.GroupBox _loginGroupBox;
        private System.Windows.Forms.GroupBox _emailSettingsGroupBox;
        private System.Windows.Forms.TableLayoutPanel _loginTableLayoutPanel;
        private System.Windows.Forms.Label _usernameLabel;
        private System.Windows.Forms.TextBox _usernameTextBox;
        private System.Windows.Forms.Label _passwordLabel;
        private System.Windows.Forms.TextBox _passwordTextBox;
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
        private Utilities.Forms.InfoTip _passwordInfoTip;
        private Utilities.Forms.InfoTip _usernameInfoTip;
        private Utilities.Forms.InfoTip _sharedEmailAddressInfoTip;
        private Utilities.Forms.InfoTip _postDownloadFolderInfoTip;
        private Utilities.Forms.InfoTip _inputFolderInfoTip;
        private Utilities.Forms.InfoTip _downloadDirectoryInfoTip;
    }
}
