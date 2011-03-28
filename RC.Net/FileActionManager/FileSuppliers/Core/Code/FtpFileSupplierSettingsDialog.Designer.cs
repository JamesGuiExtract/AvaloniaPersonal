namespace Extract.FileActionManager.FileSuppliers
{
    partial class FtpFileSupplierSettingsDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FtpFileSupplierSettingsDialog));
            EnterpriseDT.Net.Ftp.Forms.FTPConnectionProperties ftpConnectionProperties1 = new EnterpriseDT.Net.Ftp.Forms.FTPConnectionProperties();
            this._settingsTabControl = new System.Windows.Forms.TabControl();
            this._generalSettingsTabPage = new System.Windows.Forms.TabPage();
            this._browseButton = new Extract.Utilities.Forms.BrowseButton();
            this._localWorkingFolderTextBox = new System.Windows.Forms.TextBox();
            this._pathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._afterDownloadGroupBox = new System.Windows.Forms.GroupBox();
            this._newExtensionTextBox = new System.Windows.Forms.TextBox();
            this._changeRemoteExtensionRadioButton = new System.Windows.Forms.RadioButton();
            this._doNothingRadioButton = new System.Windows.Forms.RadioButton();
            this._deleteRemoteFileRadioButton = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this._pollingIntervalNumericUpDown = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._pollRemoteCheckBox = new System.Windows.Forms.CheckBox();
            this._recursiveDownloadCheckBox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._fileExtensionSpecificationTextBox = new System.Windows.Forms.TextBox();
            this._remoteDownloadFolderTextBox = new System.Windows.Forms.TextBox();
            this._connectionSettingsTabPage = new System.Windows.Forms.TabPage();
            this._testConnectionButton = new System.Windows.Forms.Button();
            this._ftpConnectionEditor = new EnterpriseDT.Net.Ftp.Forms.FTPConnectionEditor();
            this._secureFTPConnection = new EnterpriseDT.Net.Ftp.SecureFTPConnection(this.components);
            this._btnOK = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._settingsTabControl.SuspendLayout();
            this._generalSettingsTabPage.SuspendLayout();
            this._afterDownloadGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pollingIntervalNumericUpDown)).BeginInit();
            this._connectionSettingsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // _settingsTabControl
            // 
            this._settingsTabControl.Controls.Add(this._generalSettingsTabPage);
            this._settingsTabControl.Controls.Add(this._connectionSettingsTabPage);
            this._settingsTabControl.Dock = System.Windows.Forms.DockStyle.Top;
            this._settingsTabControl.Location = new System.Drawing.Point(0, 0);
            this._settingsTabControl.Name = "_settingsTabControl";
            this._settingsTabControl.SelectedIndex = 0;
            this._settingsTabControl.Size = new System.Drawing.Size(457, 391);
            this._settingsTabControl.TabIndex = 0;
            // 
            // _generalSettingsTabPage
            // 
            this._generalSettingsTabPage.Controls.Add(this._browseButton);
            this._generalSettingsTabPage.Controls.Add(this._pathTagsButton);
            this._generalSettingsTabPage.Controls.Add(this._afterDownloadGroupBox);
            this._generalSettingsTabPage.Controls.Add(this.label3);
            this._generalSettingsTabPage.Controls.Add(this._pollingIntervalNumericUpDown);
            this._generalSettingsTabPage.Controls.Add(this._pollRemoteCheckBox);
            this._generalSettingsTabPage.Controls.Add(this._recursiveDownloadCheckBox);
            this._generalSettingsTabPage.Controls.Add(this.label4);
            this._generalSettingsTabPage.Controls.Add(this.label2);
            this._generalSettingsTabPage.Controls.Add(this.label1);
            this._generalSettingsTabPage.Controls.Add(this._localWorkingFolderTextBox);
            this._generalSettingsTabPage.Controls.Add(this._fileExtensionSpecificationTextBox);
            this._generalSettingsTabPage.Controls.Add(this._remoteDownloadFolderTextBox);
            this._generalSettingsTabPage.Location = new System.Drawing.Point(4, 22);
            this._generalSettingsTabPage.Name = "_generalSettingsTabPage";
            this._generalSettingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._generalSettingsTabPage.Size = new System.Drawing.Size(449, 365);
            this._generalSettingsTabPage.TabIndex = 0;
            this._generalSettingsTabPage.Text = "General Settings";
            this._generalSettingsTabPage.UseVisualStyleBackColor = true;
            // 
            // _browseButton
            // 
            this._browseButton.FolderBrowser = true;
            this._browseButton.Location = new System.Drawing.Point(414, 328);
            this._browseButton.Name = "_browseButton";
            this._browseButton.Size = new System.Drawing.Size(27, 20);
            this._browseButton.TabIndex = 12;
            this._browseButton.Text = "...";
            this._browseButton.TextControl = this._localWorkingFolderTextBox;
            this._browseButton.UseVisualStyleBackColor = true;
            // 
            // _localWorkingFolderTextBox
            // 
            this._localWorkingFolderTextBox.Location = new System.Drawing.Point(11, 328);
            this._localWorkingFolderTextBox.Name = "_localWorkingFolderTextBox";
            this._localWorkingFolderTextBox.Size = new System.Drawing.Size(371, 20);
            this._localWorkingFolderTextBox.TabIndex = 10;
            // 
            // _pathTagsButton
            // 
            this._pathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_pathTagsButton.Image")));
            this._pathTagsButton.Location = new System.Drawing.Point(389, 328);
            this._pathTagsButton.Name = "_pathTagsButton";
            this._pathTagsButton.PathTags = new Extract.Utilities.FileActionManagerSupplierPathTags();
            this._pathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._pathTagsButton.TabIndex = 11;
            this._pathTagsButton.TextControl = this._localWorkingFolderTextBox;
            this._pathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _afterDownloadGroupBox
            // 
            this._afterDownloadGroupBox.Controls.Add(this._newExtensionTextBox);
            this._afterDownloadGroupBox.Controls.Add(this._changeRemoteExtensionRadioButton);
            this._afterDownloadGroupBox.Controls.Add(this._doNothingRadioButton);
            this._afterDownloadGroupBox.Controls.Add(this._deleteRemoteFileRadioButton);
            this._afterDownloadGroupBox.Location = new System.Drawing.Point(11, 171);
            this._afterDownloadGroupBox.Name = "_afterDownloadGroupBox";
            this._afterDownloadGroupBox.Size = new System.Drawing.Size(430, 135);
            this._afterDownloadGroupBox.TabIndex = 5;
            this._afterDownloadGroupBox.TabStop = false;
            this._afterDownloadGroupBox.Text = "After downloading the remote file";
            // 
            // _newExtensionTextBox
            // 
            this._newExtensionTextBox.Location = new System.Drawing.Point(36, 42);
            this._newExtensionTextBox.Name = "_newExtensionTextBox";
            this._newExtensionTextBox.Size = new System.Drawing.Size(335, 20);
            this._newExtensionTextBox.TabIndex = 7;
            // 
            // _changeRemoteExtensionRadioButton
            // 
            this._changeRemoteExtensionRadioButton.AutoSize = true;
            this._changeRemoteExtensionRadioButton.Location = new System.Drawing.Point(16, 19);
            this._changeRemoteExtensionRadioButton.Name = "_changeRemoteExtensionRadioButton";
            this._changeRemoteExtensionRadioButton.Size = new System.Drawing.Size(286, 17);
            this._changeRemoteExtensionRadioButton.TabIndex = 6;
            this._changeRemoteExtensionRadioButton.TabStop = true;
            this._changeRemoteExtensionRadioButton.Text = "Append the following to the remote filename\'s extension";
            this._changeRemoteExtensionRadioButton.UseVisualStyleBackColor = true;
            this._changeRemoteExtensionRadioButton.Click += new System.EventHandler(this.HandleCheckBoxOrRadioClick);
            // 
            // _doNothingRadioButton
            // 
            this._doNothingRadioButton.AutoSize = true;
            this._doNothingRadioButton.Location = new System.Drawing.Point(16, 94);
            this._doNothingRadioButton.Name = "_doNothingRadioButton";
            this._doNothingRadioButton.Size = new System.Drawing.Size(187, 17);
            this._doNothingRadioButton.TabIndex = 9;
            this._doNothingRadioButton.TabStop = true;
            this._doNothingRadioButton.Text = "Do nothing (leave remote file as is)";
            this._doNothingRadioButton.UseVisualStyleBackColor = true;
            this._doNothingRadioButton.Click += new System.EventHandler(this.HandleCheckBoxOrRadioClick);
            // 
            // _deleteRemoteFileRadioButton
            // 
            this._deleteRemoteFileRadioButton.AutoSize = true;
            this._deleteRemoteFileRadioButton.Location = new System.Drawing.Point(16, 71);
            this._deleteRemoteFileRadioButton.Name = "_deleteRemoteFileRadioButton";
            this._deleteRemoteFileRadioButton.Size = new System.Drawing.Size(125, 17);
            this._deleteRemoteFileRadioButton.TabIndex = 8;
            this._deleteRemoteFileRadioButton.TabStop = true;
            this._deleteRemoteFileRadioButton.Text = "Delete the remote file";
            this._deleteRemoteFileRadioButton.UseVisualStyleBackColor = true;
            this._deleteRemoteFileRadioButton.Click += new System.EventHandler(this.HandleCheckBoxOrRadioClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(333, 145);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "minute(s)";
            // 
            // _pollingIntervalNumericUpDown
            // 
            this._pollingIntervalNumericUpDown.IntegersOnly = true;
            this._pollingIntervalNumericUpDown.Location = new System.Drawing.Point(282, 145);
            this._pollingIntervalNumericUpDown.Maximum = new decimal(new int[] {
            1440,
            0,
            0,
            0});
            this._pollingIntervalNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._pollingIntervalNumericUpDown.Name = "_pollingIntervalNumericUpDown";
            this._pollingIntervalNumericUpDown.Size = new System.Drawing.Size(45, 20);
            this._pollingIntervalNumericUpDown.TabIndex = 4;
            this._pollingIntervalNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._pollingIntervalNumericUpDown.UserTextCorrected += new System.EventHandler<System.EventArgs>(this.HandlePollingIntervalNumericUpDownUserTextCorrected);
            // 
            // _pollRemoteCheckBox
            // 
            this._pollRemoteCheckBox.AutoSize = true;
            this._pollRemoteCheckBox.Location = new System.Drawing.Point(11, 145);
            this._pollRemoteCheckBox.Name = "_pollRemoteCheckBox";
            this._pollRemoteCheckBox.Size = new System.Drawing.Size(265, 17);
            this._pollRemoteCheckBox.TabIndex = 3;
            this._pollRemoteCheckBox.Text = "Keep polling the remote location for new files every";
            this._pollRemoteCheckBox.UseVisualStyleBackColor = true;
            this._pollRemoteCheckBox.Click += new System.EventHandler(this.HandleCheckBoxOrRadioClick);
            // 
            // _recursiveDownloadCheckBox
            // 
            this._recursiveDownloadCheckBox.AutoSize = true;
            this._recursiveDownloadCheckBox.Location = new System.Drawing.Point(11, 122);
            this._recursiveDownloadCheckBox.Name = "_recursiveDownloadCheckBox";
            this._recursiveDownloadCheckBox.Size = new System.Drawing.Size(245, 17);
            this._recursiveDownloadCheckBox.TabIndex = 2;
            this._recursiveDownloadCheckBox.Text = "Recursively download files from any subfolders";
            this._recursiveDownloadCheckBox.UseVisualStyleBackColor = true;
            this._recursiveDownloadCheckBox.Click += new System.EventHandler(this.HandleCheckBoxOrRadioClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 310);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(102, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Local working folder";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(144, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "File extension specification(s)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(178, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Remote folder to download files from";
            // 
            // _fileExtensionSpecificationTextBox
            // 
            this._fileExtensionSpecificationTextBox.Location = new System.Drawing.Point(8, 85);
            this._fileExtensionSpecificationTextBox.Name = "_fileExtensionSpecificationTextBox";
            this._fileExtensionSpecificationTextBox.Size = new System.Drawing.Size(374, 20);
            this._fileExtensionSpecificationTextBox.TabIndex = 1;
            // 
            // _remoteDownloadFolderTextBox
            // 
            this._remoteDownloadFolderTextBox.Location = new System.Drawing.Point(8, 28);
            this._remoteDownloadFolderTextBox.Name = "_remoteDownloadFolderTextBox";
            this._remoteDownloadFolderTextBox.Size = new System.Drawing.Size(374, 20);
            this._remoteDownloadFolderTextBox.TabIndex = 0;
            // 
            // _connectionSettingsTabPage
            // 
            this._connectionSettingsTabPage.Controls.Add(this._testConnectionButton);
            this._connectionSettingsTabPage.Controls.Add(this._ftpConnectionEditor);
            this._connectionSettingsTabPage.Location = new System.Drawing.Point(4, 22);
            this._connectionSettingsTabPage.Name = "_connectionSettingsTabPage";
            this._connectionSettingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._connectionSettingsTabPage.Size = new System.Drawing.Size(449, 365);
            this._connectionSettingsTabPage.TabIndex = 1;
            this._connectionSettingsTabPage.Text = "Connection Settings";
            this._connectionSettingsTabPage.UseVisualStyleBackColor = true;
            // 
            // _testConnectionButton
            // 
            this._testConnectionButton.Location = new System.Drawing.Point(3, 339);
            this._testConnectionButton.Name = "_testConnectionButton";
            this._testConnectionButton.Size = new System.Drawing.Size(116, 23);
            this._testConnectionButton.TabIndex = 2;
            this._testConnectionButton.Text = "Test Connection";
            this._testConnectionButton.UseVisualStyleBackColor = true;
            this._testConnectionButton.Click += new System.EventHandler(this.HandleTestConnection);
            // 
            // _ftpConnectionEditor
            // 
            this._ftpConnectionEditor.Connection = this._secureFTPConnection;
            this._ftpConnectionEditor.Dock = System.Windows.Forms.DockStyle.Top;
            this._ftpConnectionEditor.HelpBackColor = System.Drawing.SystemColors.Control;
            this._ftpConnectionEditor.HelpForeColor = System.Drawing.SystemColors.ControlText;
            this._ftpConnectionEditor.LineColor = System.Drawing.SystemColors.ScrollBar;
            this._ftpConnectionEditor.Location = new System.Drawing.Point(3, 3);
            this._ftpConnectionEditor.Name = "_ftpConnectionEditor";
            this._ftpConnectionEditor.Properties = ftpConnectionProperties1;
            this._ftpConnectionEditor.Size = new System.Drawing.Size(443, 330);
            this._ftpConnectionEditor.TabIndex = 0;
            this._ftpConnectionEditor.ViewBackColor = System.Drawing.SystemColors.Window;
            this._ftpConnectionEditor.ViewForeColor = System.Drawing.SystemColors.WindowText;
            this._ftpConnectionEditor.Properties = new EnterpriseDT.Net.Ftp.Forms.FTPConnectionProperties(false);
            this._ftpConnectionEditor.Properties.AddCategory("Connection", "Connection", true);
            this._ftpConnectionEditor.Properties.AddProperty("Connection", "Protocol", "Protocol", "File transfer protocol to use.", true, 0);
            this._ftpConnectionEditor.Properties.AddProperty("Connection", "ServerAddress", "Server Address", "The domain-name or IP address of the FTP server.", true, 1);
            this._ftpConnectionEditor.Properties.AddProperty("Connection", "ServerPort", "Server Port", "Port on the server to which to connect the control-channel.", true, 2);
            this._ftpConnectionEditor.Properties.AddProperty("Connection", "UserName", "User Name", "User-name of account on the server.", true, 3);
            this._ftpConnectionEditor.Properties.AddProperty("Connection", "Password", "Password", "Password of account on the server.", true, 4);
            // 
            // _secureFTPConnection
            // 
            this._secureFTPConnection.ClientPrivateKeyBytes = null;
            this._secureFTPConnection.DefaultSyncRules.FilterCallback = null;
            this._secureFTPConnection.LicenseKey = "701-9435-3077-0362";
            this._secureFTPConnection.LicenseOwner = "trialuser";
            this._secureFTPConnection.ParentControl = this;
            // 
            // _btnOK
            // 
            this._btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnOK.Location = new System.Drawing.Point(289, 397);
            this._btnOK.Name = "_btnOK";
            this._btnOK.Size = new System.Drawing.Size(75, 23);
            this._btnOK.TabIndex = 1;
            this._btnOK.Text = "OK";
            this._btnOK.UseVisualStyleBackColor = true;
            this._btnOK.Click += new System.EventHandler(this.HandleOkButtonClicked);
            // 
            // _btnCancel
            // 
            this._btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(370, 397);
            this._btnCancel.MinimumSize = new System.Drawing.Size(75, 23);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 23);
            this._btnCancel.TabIndex = 2;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // FtpFileSupplierSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 431);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnOK);
            this.Controls.Add(this._settingsTabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FtpFileSupplierSettingsDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure SFTP/FTP File Supplier";
            this._settingsTabControl.ResumeLayout(false);
            this._generalSettingsTabPage.ResumeLayout(false);
            this._generalSettingsTabPage.PerformLayout();
            this._afterDownloadGroupBox.ResumeLayout(false);
            this._afterDownloadGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pollingIntervalNumericUpDown)).EndInit();
            this._connectionSettingsTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl _settingsTabControl;
        private System.Windows.Forms.TabPage _generalSettingsTabPage;
        private System.Windows.Forms.TabPage _connectionSettingsTabPage;
        private System.Windows.Forms.Button _btnOK;
        private System.Windows.Forms.Button _btnCancel;
        private EnterpriseDT.Net.Ftp.Forms.FTPConnectionEditor _ftpConnectionEditor;
        private EnterpriseDT.Net.Ftp.SecureFTPConnection _secureFTPConnection;
        private System.Windows.Forms.GroupBox _afterDownloadGroupBox;
        private System.Windows.Forms.TextBox _newExtensionTextBox;
        private System.Windows.Forms.RadioButton _changeRemoteExtensionRadioButton;
        private System.Windows.Forms.RadioButton _doNothingRadioButton;
        private System.Windows.Forms.RadioButton _deleteRemoteFileRadioButton;
        private System.Windows.Forms.Label label3;
        private Utilities.Forms.BetterNumericUpDown _pollingIntervalNumericUpDown;
        private System.Windows.Forms.CheckBox _pollRemoteCheckBox;
        private System.Windows.Forms.CheckBox _recursiveDownloadCheckBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _localWorkingFolderTextBox;
        private System.Windows.Forms.TextBox _fileExtensionSpecificationTextBox;
        private System.Windows.Forms.TextBox _remoteDownloadFolderTextBox;
        private System.Windows.Forms.Button _testConnectionButton;
        private Utilities.Forms.BrowseButton _browseButton;
        private Utilities.Forms.PathTagsButton _pathTagsButton;
    }
}