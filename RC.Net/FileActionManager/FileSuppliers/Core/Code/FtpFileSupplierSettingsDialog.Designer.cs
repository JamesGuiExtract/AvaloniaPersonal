﻿namespace Extract.FileActionManager.FileSuppliers
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
            System.Windows.Forms.GroupBox groupBox1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FtpFileSupplierSettingsDialog));
            this._pollingIntervalNumericUpDown = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._pollingTimesTextBox = new System.Windows.Forms.TextBox();
            this._pollAtSetTimesRadioButton = new System.Windows.Forms.RadioButton();
            this._pollContinuouslyRadioButton = new System.Windows.Forms.RadioButton();
            this._downloadOnceRadioButton = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this._settingsTabControl = new System.Windows.Forms.TabControl();
            this._generalSettingsTabPage = new System.Windows.Forms.TabPage();
            this._fileFilterComboBox = new System.Windows.Forms.ComboBox();
            this._browseButton = new Extract.Utilities.Forms.BrowseButton();
            this._localWorkingFolderTextBox = new System.Windows.Forms.TextBox();
            this._localPathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._recursiveDownloadCheckBox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._remoteDownloadFolderTextBox = new System.Windows.Forms.TextBox();
            this._connectionSettingsTabPage = new System.Windows.Forms.TabPage();
            this._ftpConnectionSettingsControl = new Extract.FileActionManager.Forms.FtpConnectionSettingsControl(this.components);
            this._secureFTPConnection = new EnterpriseDT.Net.Ftp.SecureFTPConnection(this.components);
            this._btnOK = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._remotePathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pollingIntervalNumericUpDown)).BeginInit();
            this._settingsTabControl.SuspendLayout();
            this._generalSettingsTabPage.SuspendLayout();
            this._connectionSettingsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this._pollingIntervalNumericUpDown);
            groupBox1.Controls.Add(this._pollingTimesTextBox);
            groupBox1.Controls.Add(this._pollAtSetTimesRadioButton);
            groupBox1.Controls.Add(this._pollContinuouslyRadioButton);
            groupBox1.Controls.Add(this._downloadOnceRadioButton);
            groupBox1.Controls.Add(this.label3);
            groupBox1.Location = new System.Drawing.Point(11, 145);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(456, 122);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "Frequency of downloads";
            // 
            // _pollingIntervalNumericUpDown
            // 
            this._pollingIntervalNumericUpDown.IntegersOnly = true;
            this._pollingIntervalNumericUpDown.Location = new System.Drawing.Point(291, 44);
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
            this._pollingIntervalNumericUpDown.Size = new System.Drawing.Size(49, 20);
            this._pollingIntervalNumericUpDown.TabIndex = 2;
            this._pollingIntervalNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._pollingIntervalNumericUpDown.UserTextCorrected += new System.EventHandler<System.EventArgs>(this.HandlePollingIntervalNumericUpDownUserTextCorrected);
            // 
            // _pollingTimesTextBox
            // 
            this._pollingTimesTextBox.Location = new System.Drawing.Point(36, 92);
            this._pollingTimesTextBox.Name = "_pollingTimesTextBox";
            this._pollingTimesTextBox.Size = new System.Drawing.Size(414, 20);
            this._pollingTimesTextBox.TabIndex = 5;
            // 
            // _pollAtSetTimesRadioButton
            // 
            this._pollAtSetTimesRadioButton.AutoSize = true;
            this._pollAtSetTimesRadioButton.Location = new System.Drawing.Point(16, 68);
            this._pollAtSetTimesRadioButton.Name = "_pollAtSetTimesRadioButton";
            this._pollAtSetTimesRadioButton.Size = new System.Drawing.Size(315, 17);
            this._pollAtSetTimesRadioButton.TabIndex = 4;
            this._pollAtSetTimesRadioButton.TabStop = true;
            this._pollAtSetTimesRadioButton.Text = "Poll the remote location for new files at these times of the day:";
            this._pollAtSetTimesRadioButton.UseVisualStyleBackColor = true;
            this._pollAtSetTimesRadioButton.CheckedChanged += new System.EventHandler(this.HandlePollingMethodCheckedChanged);
            // 
            // _pollContinuouslyRadioButton
            // 
            this._pollContinuouslyRadioButton.AutoSize = true;
            this._pollContinuouslyRadioButton.Location = new System.Drawing.Point(16, 44);
            this._pollContinuouslyRadioButton.Name = "_pollContinuouslyRadioButton";
            this._pollContinuouslyRadioButton.Size = new System.Drawing.Size(264, 17);
            this._pollContinuouslyRadioButton.TabIndex = 1;
            this._pollContinuouslyRadioButton.TabStop = true;
            this._pollContinuouslyRadioButton.Text = "Keep polling the remote location for new files every";
            this._pollContinuouslyRadioButton.UseVisualStyleBackColor = true;
            this._pollContinuouslyRadioButton.CheckedChanged += new System.EventHandler(this.HandlePollingMethodCheckedChanged);
            // 
            // _downloadOnceRadioButton
            // 
            this._downloadOnceRadioButton.AutoSize = true;
            this._downloadOnceRadioButton.Location = new System.Drawing.Point(16, 20);
            this._downloadOnceRadioButton.Name = "_downloadOnceRadioButton";
            this._downloadOnceRadioButton.Size = new System.Drawing.Size(140, 17);
            this._downloadOnceRadioButton.TabIndex = 0;
            this._downloadOnceRadioButton.TabStop = true;
            this._downloadOnceRadioButton.Text = "Download files just once";
            this._downloadOnceRadioButton.UseVisualStyleBackColor = true;
            this._downloadOnceRadioButton.CheckedChanged += new System.EventHandler(this.HandlePollingMethodCheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(346, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "minute(s)";
            // 
            // _settingsTabControl
            // 
            this._settingsTabControl.Controls.Add(this._generalSettingsTabPage);
            this._settingsTabControl.Controls.Add(this._connectionSettingsTabPage);
            this._settingsTabControl.Dock = System.Windows.Forms.DockStyle.Top;
            this._settingsTabControl.Location = new System.Drawing.Point(0, 0);
            this._settingsTabControl.Name = "_settingsTabControl";
            this._settingsTabControl.SelectedIndex = 0;
            this._settingsTabControl.Size = new System.Drawing.Size(483, 402);
            this._settingsTabControl.TabIndex = 0;
            // 
            // _generalSettingsTabPage
            // 
            this._generalSettingsTabPage.Controls.Add(this._remotePathTagsButton);
            this._generalSettingsTabPage.Controls.Add(this._fileFilterComboBox);
            this._generalSettingsTabPage.Controls.Add(groupBox1);
            this._generalSettingsTabPage.Controls.Add(this._browseButton);
            this._generalSettingsTabPage.Controls.Add(this._localPathTagsButton);
            this._generalSettingsTabPage.Controls.Add(this._recursiveDownloadCheckBox);
            this._generalSettingsTabPage.Controls.Add(this.label4);
            this._generalSettingsTabPage.Controls.Add(this.label2);
            this._generalSettingsTabPage.Controls.Add(this.label1);
            this._generalSettingsTabPage.Controls.Add(this._localWorkingFolderTextBox);
            this._generalSettingsTabPage.Controls.Add(this._remoteDownloadFolderTextBox);
            this._generalSettingsTabPage.Location = new System.Drawing.Point(4, 22);
            this._generalSettingsTabPage.Name = "_generalSettingsTabPage";
            this._generalSettingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._generalSettingsTabPage.Size = new System.Drawing.Size(475, 376);
            this._generalSettingsTabPage.TabIndex = 0;
            this._generalSettingsTabPage.Text = "General Settings";
            this._generalSettingsTabPage.UseVisualStyleBackColor = true;
            // 
            // _fileFilterComboBox
            // 
            this._fileFilterComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._fileFilterComboBox.FormattingEnabled = true;
            this._fileFilterComboBox.Items.AddRange(new object[] {
            "*.bmp;*.rle;*.dib;*.rst;*.gp4;*.mil;*.cal;*.cg4;*.flc;*.fli;*.gif;*.jpg;*.jpeg;*." +
                "pcx;*.pct;*.png;*.tga;*.tif;*.tiff;*.pdf",
            "*.bmp;*.rle;*.dib",
            "*.gif",
            "*.jpg;*.jpeg",
            "*.pcx",
            "*.pct",
            "*.pdf",
            "*.png",
            "*.tif;*.tiff",
            "*.txt",
            "*.xml",
            "*.*"});
            this._fileFilterComboBox.Location = new System.Drawing.Point(8, 85);
            this._fileFilterComboBox.Name = "_fileFilterComboBox";
            this._fileFilterComboBox.Size = new System.Drawing.Size(374, 21);
            this._fileFilterComboBox.TabIndex = 3;
            // 
            // _browseButton
            // 
            this._browseButton.FolderBrowser = true;
            this._browseButton.Location = new System.Drawing.Point(440, 288);
            this._browseButton.Name = "_browseButton";
            this._browseButton.Size = new System.Drawing.Size(27, 20);
            this._browseButton.TabIndex = 9;
            this._browseButton.Text = "...";
            this._browseButton.TextControl = this._localWorkingFolderTextBox;
            this._browseButton.UseVisualStyleBackColor = true;
            // 
            // _localWorkingFolderTextBox
            // 
            this._localWorkingFolderTextBox.Location = new System.Drawing.Point(8, 288);
            this._localWorkingFolderTextBox.Name = "_localWorkingFolderTextBox";
            this._localWorkingFolderTextBox.Size = new System.Drawing.Size(402, 20);
            this._localWorkingFolderTextBox.TabIndex = 7;
            // 
            // _localPathTagsButton
            // 
            this._localPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_localPathTagsButton.Image")));
            this._localPathTagsButton.Location = new System.Drawing.Point(416, 288);
            this._localPathTagsButton.Name = "_localPathTagsButton";
            this._localPathTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._localPathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._localPathTagsButton.TabIndex = 8;
            this._localPathTagsButton.TextControl = this._localWorkingFolderTextBox;
            this._localPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _recursiveDownloadCheckBox
            // 
            this._recursiveDownloadCheckBox.AutoSize = true;
            this._recursiveDownloadCheckBox.Location = new System.Drawing.Point(11, 122);
            this._recursiveDownloadCheckBox.Name = "_recursiveDownloadCheckBox";
            this._recursiveDownloadCheckBox.Size = new System.Drawing.Size(245, 17);
            this._recursiveDownloadCheckBox.TabIndex = 4;
            this._recursiveDownloadCheckBox.Text = "Recursively download files from any subfolders";
            this._recursiveDownloadCheckBox.UseVisualStyleBackColor = true;
            this._recursiveDownloadCheckBox.Click += new System.EventHandler(this.HandleCheckBoxOrRadioClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 270);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(102, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Local working folder";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(144, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "File extension specification(s)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(178, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Remote folder to download files from";
            // 
            // _remoteDownloadFolderTextBox
            // 
            this._remoteDownloadFolderTextBox.Location = new System.Drawing.Point(8, 28);
            this._remoteDownloadFolderTextBox.Name = "_remoteDownloadFolderTextBox";
            this._remoteDownloadFolderTextBox.Size = new System.Drawing.Size(374, 20);
            this._remoteDownloadFolderTextBox.TabIndex = 1;
            // 
            // _connectionSettingsTabPage
            // 
            this._connectionSettingsTabPage.Controls.Add(this._ftpConnectionSettingsControl);
            this._connectionSettingsTabPage.Location = new System.Drawing.Point(4, 22);
            this._connectionSettingsTabPage.Name = "_connectionSettingsTabPage";
            this._connectionSettingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._connectionSettingsTabPage.Size = new System.Drawing.Size(475, 376);
            this._connectionSettingsTabPage.TabIndex = 1;
            this._connectionSettingsTabPage.Text = "Connection Settings";
            this._connectionSettingsTabPage.UseVisualStyleBackColor = true;
            // 
            // _ftpConnectionSettingsControl
            // 
            this._ftpConnectionSettingsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._ftpConnectionSettingsControl.Location = new System.Drawing.Point(3, 3);
            this._ftpConnectionSettingsControl.MinimumSize = new System.Drawing.Size(450, 370);
            this._ftpConnectionSettingsControl.Name = "_ftpConnectionSettingsControl";
            this._ftpConnectionSettingsControl.NumberOfConnections = 1;
            this._ftpConnectionSettingsControl.NumberOfRetriesBeforeFailure = 0;
            this._ftpConnectionSettingsControl.Padding = new System.Windows.Forms.Padding(3);
            this._ftpConnectionSettingsControl.ReestablishConnectionBeforeRetry = true;
            this._ftpConnectionSettingsControl.ShowConnectionsControl = false;
            this._ftpConnectionSettingsControl.Size = new System.Drawing.Size(469, 370);
            this._ftpConnectionSettingsControl.TabIndex = 3;
            this._ftpConnectionSettingsControl.TimeBetweenRetries = 0;
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
            this._btnOK.Location = new System.Drawing.Point(315, 408);
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
            this._btnCancel.Location = new System.Drawing.Point(396, 408);
            this._btnCancel.MinimumSize = new System.Drawing.Size(75, 23);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 23);
            this._btnCancel.TabIndex = 2;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _remotePathTagsButton
            // 
            this._remotePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_remotePathTagsButton.Image")));
            this._remotePathTagsButton.Location = new System.Drawing.Point(388, 27);
            this._remotePathTagsButton.Name = "_remotePathTagsButton";
            this._remotePathTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._remotePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._remotePathTagsButton.TabIndex = 2;
            this._remotePathTagsButton.TextControl = this._remoteDownloadFolderTextBox;
            this._remotePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // FtpFileSupplierSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnCancel;
            this.ClientSize = new System.Drawing.Size(483, 443);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnOK);
            this.Controls.Add(this._settingsTabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FtpFileSupplierSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Files from FTP site settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pollingIntervalNumericUpDown)).EndInit();
            this._settingsTabControl.ResumeLayout(false);
            this._generalSettingsTabPage.ResumeLayout(false);
            this._generalSettingsTabPage.PerformLayout();
            this._connectionSettingsTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl _settingsTabControl;
        private System.Windows.Forms.TabPage _generalSettingsTabPage;
        private System.Windows.Forms.TabPage _connectionSettingsTabPage;
        private System.Windows.Forms.Button _btnOK;
        private System.Windows.Forms.Button _btnCancel;
        private EnterpriseDT.Net.Ftp.SecureFTPConnection _secureFTPConnection;
        private System.Windows.Forms.Label label3;
        private Utilities.Forms.BetterNumericUpDown _pollingIntervalNumericUpDown;
        private System.Windows.Forms.CheckBox _recursiveDownloadCheckBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _localWorkingFolderTextBox;
        private System.Windows.Forms.TextBox _remoteDownloadFolderTextBox;
        private Utilities.Forms.BrowseButton _browseButton;
        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _localPathTagsButton;
        private Forms.FtpConnectionSettingsControl _ftpConnectionSettingsControl;
        private System.Windows.Forms.TextBox _pollingTimesTextBox;
        private System.Windows.Forms.RadioButton _pollAtSetTimesRadioButton;
        private System.Windows.Forms.RadioButton _pollContinuouslyRadioButton;
        private System.Windows.Forms.RadioButton _downloadOnceRadioButton;
        private System.Windows.Forms.ComboBox _fileFilterComboBox;
        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _remotePathTagsButton;
    }
}
