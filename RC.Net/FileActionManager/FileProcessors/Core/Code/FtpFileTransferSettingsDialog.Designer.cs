namespace Extract.FileActionManager.FileProcessors
{
    partial class FtpFileTransferSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FtpFileTransferSettingsDialog));
            this._settingsTabControl = new System.Windows.Forms.TabControl();
            this._generalSettingsTabPage = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._localFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._localFileNameTextBox = new System.Windows.Forms.TextBox();
            this._localFileNamePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._remoteFileNamePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._remoteFileNameTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._deleteFileRadioButton = new System.Windows.Forms.RadioButton();
            this._downloadFileRadioButton = new System.Windows.Forms.RadioButton();
            this._uploadFileRadioButton = new System.Windows.Forms.RadioButton();
            this._connectionSettingsTabPage = new System.Windows.Forms.TabPage();
            this._ftpConnectionSettingsControl = new Extract.FileActionManager.Forms.FtpConnectionSettingsControl(this.components);
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._settingsTabControl.SuspendLayout();
            this._generalSettingsTabPage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this._connectionSettingsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // _settingsTabControl
            // 
            this._settingsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._settingsTabControl.Controls.Add(this._generalSettingsTabPage);
            this._settingsTabControl.Controls.Add(this._connectionSettingsTabPage);
            this._settingsTabControl.Location = new System.Drawing.Point(4, 4);
            this._settingsTabControl.Name = "_settingsTabControl";
            this._settingsTabControl.SelectedIndex = 0;
            this._settingsTabControl.Size = new System.Drawing.Size(469, 378);
            this._settingsTabControl.TabIndex = 0;
            // 
            // _generalSettingsTabPage
            // 
            this._generalSettingsTabPage.Controls.Add(this.groupBox2);
            this._generalSettingsTabPage.Controls.Add(this.groupBox1);
            this._generalSettingsTabPage.Location = new System.Drawing.Point(4, 22);
            this._generalSettingsTabPage.Name = "_generalSettingsTabPage";
            this._generalSettingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._generalSettingsTabPage.Size = new System.Drawing.Size(461, 352);
            this._generalSettingsTabPage.TabIndex = 0;
            this._generalSettingsTabPage.Text = "General Settings";
            this._generalSettingsTabPage.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this._localFileNameBrowseButton);
            this.groupBox2.Controls.Add(this._localFileNamePathTagsButton);
            this.groupBox2.Controls.Add(this._remoteFileNamePathTagsButton);
            this.groupBox2.Controls.Add(this._localFileNameTextBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this._remoteFileNameTextBox);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(6, 105);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(449, 123);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "File paths";
            // 
            // _localFileNameBrowseButton
            // 
            this._localFileNameBrowseButton.Location = new System.Drawing.Point(399, 82);
            this._localFileNameBrowseButton.Name = "_localFileNameBrowseButton";
            this._localFileNameBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._localFileNameBrowseButton.TabIndex = 3;
            this._localFileNameBrowseButton.Text = "...";
            this._localFileNameBrowseButton.TextControl = this._localFileNameTextBox;
            this._localFileNameBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _localFileNameTextBox
            // 
            this._localFileNameTextBox.Location = new System.Drawing.Point(10, 83);
            this._localFileNameTextBox.Name = "_localFileNameTextBox";
            this._localFileNameTextBox.Size = new System.Drawing.Size(358, 20);
            this._localFileNameTextBox.TabIndex = 1;
            // 
            // _localFileNamePathTagsButton
            // 
            this._localFileNamePathTagsButton.CausesValidation = false;
            this._localFileNamePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_localFileNamePathTagsButton.Image")));
            this._localFileNamePathTagsButton.Location = new System.Drawing.Point(374, 82);
            this._localFileNamePathTagsButton.Name = "_localFileNamePathTagsButton";
            this._localFileNamePathTagsButton.PathTags = new Extract.Utilities.FileActionManagerPathTags();
            this._localFileNamePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._localFileNamePathTagsButton.TabIndex = 2;
            this._localFileNamePathTagsButton.TextControl = this._localFileNameTextBox;
            this._localFileNamePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _remoteFileNamePathTagsButton
            // 
            this._remoteFileNamePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_remoteFileNamePathTagsButton.Image")));
            this._remoteFileNamePathTagsButton.Location = new System.Drawing.Point(374, 37);
            this._remoteFileNamePathTagsButton.Name = "_remoteFileNamePathTagsButton";
            this._remoteFileNamePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._remoteFileNamePathTagsButton.TabIndex = 2;
            this._remoteFileNamePathTagsButton.TextControl = this._remoteFileNameTextBox;
            this._remoteFileNamePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _remoteFileNameTextBox
            // 
            this._remoteFileNameTextBox.Location = new System.Drawing.Point(10, 37);
            this._remoteFileNameTextBox.Name = "_remoteFileNameTextBox";
            this._remoteFileNameTextBox.Size = new System.Drawing.Size(358, 20);
            this._remoteFileNameTextBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Local filename";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Remote filename";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._deleteFileRadioButton);
            this.groupBox1.Controls.Add(this._downloadFileRadioButton);
            this.groupBox1.Controls.Add(this._uploadFileRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(449, 93);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Select file operation";
            // 
            // _deleteFileRadioButton
            // 
            this._deleteFileRadioButton.AutoSize = true;
            this._deleteFileRadioButton.Location = new System.Drawing.Point(6, 66);
            this._deleteFileRadioButton.Name = "_deleteFileRadioButton";
            this._deleteFileRadioButton.Size = new System.Drawing.Size(154, 17);
            this._deleteFileRadioButton.TabIndex = 0;
            this._deleteFileRadioButton.TabStop = true;
            this._deleteFileRadioButton.Text = "Delete file on remote server";
            this._deleteFileRadioButton.UseVisualStyleBackColor = true;
            this._deleteFileRadioButton.Click += new System.EventHandler(this.HandleRadioClick);
            // 
            // _downloadFileRadioButton
            // 
            this._downloadFileRadioButton.AutoSize = true;
            this._downloadFileRadioButton.Location = new System.Drawing.Point(6, 43);
            this._downloadFileRadioButton.Name = "_downloadFileRadioButton";
            this._downloadFileRadioButton.Size = new System.Drawing.Size(179, 17);
            this._downloadFileRadioButton.TabIndex = 0;
            this._downloadFileRadioButton.TabStop = true;
            this._downloadFileRadioButton.Text = "Download file from remote server";
            this._downloadFileRadioButton.UseVisualStyleBackColor = true;
            this._downloadFileRadioButton.Click += new System.EventHandler(this.HandleRadioClick);
            // 
            // _uploadFileRadioButton
            // 
            this._uploadFileRadioButton.AutoSize = true;
            this._uploadFileRadioButton.Location = new System.Drawing.Point(7, 20);
            this._uploadFileRadioButton.Name = "_uploadFileRadioButton";
            this._uploadFileRadioButton.Size = new System.Drawing.Size(154, 17);
            this._uploadFileRadioButton.TabIndex = 0;
            this._uploadFileRadioButton.TabStop = true;
            this._uploadFileRadioButton.Text = "Upload file to remote server";
            this._uploadFileRadioButton.UseVisualStyleBackColor = true;
            this._uploadFileRadioButton.Click += new System.EventHandler(this.HandleRadioClick);
            // 
            // _connectionSettingsTabPage
            // 
            this._connectionSettingsTabPage.Controls.Add(this._ftpConnectionSettingsControl);
            this._connectionSettingsTabPage.Location = new System.Drawing.Point(4, 22);
            this._connectionSettingsTabPage.Name = "_connectionSettingsTabPage";
            this._connectionSettingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._connectionSettingsTabPage.Size = new System.Drawing.Size(461, 352);
            this._connectionSettingsTabPage.TabIndex = 1;
            this._connectionSettingsTabPage.Text = "Connection Settings";
            this._connectionSettingsTabPage.UseVisualStyleBackColor = true;
            // 
            // _ftpConnectionSettingsControl
            // 
            this._ftpConnectionSettingsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._ftpConnectionSettingsControl.Location = new System.Drawing.Point(3, 3);
            this._ftpConnectionSettingsControl.MinimumSize = new System.Drawing.Size(380, 300);
            this._ftpConnectionSettingsControl.Name = "_ftpConnectionSettingsControl";
            this._ftpConnectionSettingsControl.NumberOfConnections = 1;
            this._ftpConnectionSettingsControl.NumberOfRetriesBeforeFailure = 0;
            this._ftpConnectionSettingsControl.Padding = new System.Windows.Forms.Padding(3);
            this._ftpConnectionSettingsControl.ShowConnectionsControl = false;
            this._ftpConnectionSettingsControl.Size = new System.Drawing.Size(458, 348);
            this._ftpConnectionSettingsControl.TabIndex = 0;
            this._ftpConnectionSettingsControl.TimeBetweenRetries = 0;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(317, 392);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 1;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClicked);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(398, 392);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // FtpFileTransferSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 426);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._settingsTabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MinimumSize = new System.Drawing.Size(485, 416);
            this.Name = "FtpFileTransferSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Core: Transfer file via FTP/SFTP";
            this._settingsTabControl.ResumeLayout(false);
            this._generalSettingsTabPage.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this._connectionSettingsTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl _settingsTabControl;
        private System.Windows.Forms.TabPage _generalSettingsTabPage;
        private System.Windows.Forms.TabPage _connectionSettingsTabPage;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox _remoteFileNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton _deleteFileRadioButton;
        private System.Windows.Forms.RadioButton _downloadFileRadioButton;
        private System.Windows.Forms.RadioButton _uploadFileRadioButton;
        private Forms.FtpConnectionSettingsControl _ftpConnectionSettingsControl;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.TextBox _localFileNameTextBox;
        private System.Windows.Forms.Label label2;
        private Utilities.Forms.PathTagsButton _localFileNamePathTagsButton;
        private Utilities.Forms.PathTagsButton _remoteFileNamePathTagsButton;
        private Utilities.Forms.BrowseButton _localFileNameBrowseButton;
    }
}