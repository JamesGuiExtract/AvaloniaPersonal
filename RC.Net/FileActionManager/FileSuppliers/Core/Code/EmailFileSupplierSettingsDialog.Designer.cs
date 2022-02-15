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
            this.loginTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._userNameLabel = new System.Windows.Forms.Label();
            this._userNameTextBox = new System.Windows.Forms.TextBox();
            this._passwordLabel = new System.Windows.Forms.Label();
            this._passwordTextBox = new System.Windows.Forms.TextBox();
            this._emailSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._postDownloadFolderTextBox = new System.Windows.Forms.TextBox();
            this._postDownloadFolderLabel = new System.Windows.Forms.Label();
            this._sharedEmailAddressLabel = new System.Windows.Forms.Label();
            this._sharedEmailAddressTextBox = new System.Windows.Forms.TextBox();
            this._inputFolderLabel = new System.Windows.Forms.Label();
            this._inputFolderTextBox = new System.Windows.Forms.TextBox();
            this._downloadSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this._batchSizeLabel = new System.Windows.Forms.Label();
            this._batchSizeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this._loginGroupBox.SuspendLayout();
            this.loginTableLayoutPanel.SuspendLayout();
            this._emailSettingsGroupBox.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this._downloadSettingsGroupBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._batchSizeNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(471, 313);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 4;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(390, 313);
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
            this._downloadDirectoryTextBox.Location = new System.Drawing.Point(93, 3);
            this._downloadDirectoryTextBox.Name = "_downloadDirectoryTextBox";
            this._downloadDirectoryTextBox.Size = new System.Drawing.Size(364, 20);
            this._downloadDirectoryTextBox.TabIndex = 0;
            // 
            // _downloadDirectoryBrowseButton
            // 
            this._downloadDirectoryBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._downloadDirectoryBrowseButton.EnsureFileExists = false;
            this._downloadDirectoryBrowseButton.EnsurePathExists = false;
            this._downloadDirectoryBrowseButton.FolderBrowser = true;
            this._downloadDirectoryBrowseButton.Location = new System.Drawing.Point(487, 3);
            this._downloadDirectoryBrowseButton.Name = "_downloadDirectoryBrowseButton";
            this._downloadDirectoryBrowseButton.Size = new System.Drawing.Size(26, 21);
            this._downloadDirectoryBrowseButton.TabIndex = 2;
            this._downloadDirectoryBrowseButton.Text = "...";
            this._downloadDirectoryBrowseButton.TextControl = this._downloadDirectoryTextBox;
            this._downloadDirectoryBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _downloadDirectoryPathTagButton
            // 
            this._downloadDirectoryPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._downloadDirectoryPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_downloadDirectoryPathTagButton.Image")));
            this._downloadDirectoryPathTagButton.Location = new System.Drawing.Point(463, 3);
            this._downloadDirectoryPathTagButton.Name = "_downloadDirectoryPathTagButton";
            this._downloadDirectoryPathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._downloadDirectoryPathTagButton.Size = new System.Drawing.Size(18, 21);
            this._downloadDirectoryPathTagButton.TabIndex = 1;
            this._downloadDirectoryPathTagButton.TextControl = this._downloadDirectoryTextBox;
            this._downloadDirectoryPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _loginGroupBox
            // 
            this._loginGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._loginGroupBox.Controls.Add(this.loginTableLayoutPanel);
            this._loginGroupBox.Location = new System.Drawing.Point(12, 12);
            this._loginGroupBox.Name = "_loginGroupBox";
            this._loginGroupBox.Size = new System.Drawing.Size(534, 82);
            this._loginGroupBox.TabIndex = 0;
            this._loginGroupBox.TabStop = false;
            this._loginGroupBox.Text = "Login credentials";
            // 
            // loginTableLayoutPanel
            // 
            this.loginTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loginTableLayoutPanel.AutoSize = true;
            this.loginTableLayoutPanel.ColumnCount = 2;
            this.loginTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.loginTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.loginTableLayoutPanel.Controls.Add(this._userNameLabel, 0, 0);
            this.loginTableLayoutPanel.Controls.Add(this._userNameTextBox, 1, 0);
            this.loginTableLayoutPanel.Controls.Add(this._passwordLabel, 0, 1);
            this.loginTableLayoutPanel.Controls.Add(this._passwordTextBox, 1, 1);
            this.loginTableLayoutPanel.Location = new System.Drawing.Point(6, 22);
            this.loginTableLayoutPanel.Margin = new System.Windows.Forms.Padding(9, 6, 9, 9);
            this.loginTableLayoutPanel.Name = "loginTableLayoutPanel";
            this.loginTableLayoutPanel.RowCount = 2;
            this.loginTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.loginTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.loginTableLayoutPanel.Size = new System.Drawing.Size(516, 52);
            this.loginTableLayoutPanel.TabIndex = 1;
            // 
            // _userNameLabel
            // 
            this._userNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._userNameLabel.AutoSize = true;
            this._userNameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._userNameLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._userNameLabel.Location = new System.Drawing.Point(0, 6);
            this._userNameLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this._userNameLabel.Name = "_userNameLabel";
            this._userNameLabel.Size = new System.Drawing.Size(61, 13);
            this._userNameLabel.TabIndex = 0;
            this._userNameLabel.Text = "&User name:";
            // 
            // _userNameTextBox
            // 
            this._userNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._userNameTextBox.Location = new System.Drawing.Point(67, 3);
            this._userNameTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this._userNameTextBox.Name = "_userNameTextBox";
            this._userNameTextBox.Size = new System.Drawing.Size(449, 20);
            this._userNameTextBox.TabIndex = 1;
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
            this._passwordLabel.TabIndex = 2;
            this._passwordLabel.Text = "&Password:";
            // 
            // _passwordTextBox
            // 
            this._passwordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._passwordTextBox.Location = new System.Drawing.Point(67, 29);
            this._passwordTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this._passwordTextBox.Name = "_passwordTextBox";
            this._passwordTextBox.Size = new System.Drawing.Size(449, 20);
            this._passwordTextBox.TabIndex = 3;
            this._passwordTextBox.UseSystemPasswordChar = true;
            this._passwordTextBox.TextChanged += new System.EventHandler(this.SetPassword);
            // 
            // _emailSettingsGroupBox
            // 
            this._emailSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailSettingsGroupBox.Controls.Add(this.tableLayoutPanel1);
            this._emailSettingsGroupBox.Location = new System.Drawing.Point(12, 100);
            this._emailSettingsGroupBox.Name = "_emailSettingsGroupBox";
            this._emailSettingsGroupBox.Size = new System.Drawing.Size(534, 108);
            this._emailSettingsGroupBox.TabIndex = 1;
            this._emailSettingsGroupBox.TabStop = false;
            this._emailSettingsGroupBox.Text = "Email settings";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this._postDownloadFolderTextBox, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this._postDownloadFolderLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this._sharedEmailAddressLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._sharedEmailAddressTextBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this._inputFolderLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this._inputFolderTextBox, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(9, 22);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(9, 6, 9, 9);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(516, 78);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // _postDownloadFolderTextBox
            // 
            this._postDownloadFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._postDownloadFolderTextBox.Location = new System.Drawing.Point(115, 55);
            this._postDownloadFolderTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this._postDownloadFolderTextBox.Name = "_postDownloadFolderTextBox";
            this._postDownloadFolderTextBox.Size = new System.Drawing.Size(401, 20);
            this._postDownloadFolderTextBox.TabIndex = 5;
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
            this._postDownloadFolderLabel.TabIndex = 4;
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
            this._sharedEmailAddressLabel.Size = new System.Drawing.Size(75, 13);
            this._sharedEmailAddressLabel.TabIndex = 0;
            this._sharedEmailAddressLabel.Text = "&Email address:";
            // 
            // _sharedEmailAddressTextBox
            // 
            this._sharedEmailAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._sharedEmailAddressTextBox.Location = new System.Drawing.Point(115, 3);
            this._sharedEmailAddressTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this._sharedEmailAddressTextBox.Name = "_sharedEmailAddressTextBox";
            this._sharedEmailAddressTextBox.Size = new System.Drawing.Size(401, 20);
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
            this._inputFolderLabel.TabIndex = 2;
            this._inputFolderLabel.Text = "&Input folder:";
            // 
            // _inputFolderTextBox
            // 
            this._inputFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._inputFolderTextBox.Location = new System.Drawing.Point(115, 29);
            this._inputFolderTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this._inputFolderTextBox.Name = "_inputFolderTextBox";
            this._inputFolderTextBox.Size = new System.Drawing.Size(401, 20);
            this._inputFolderTextBox.TabIndex = 3;
            // 
            // _downloadSettingsGroupBox
            // 
            this._downloadSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._downloadSettingsGroupBox.Controls.Add(this.tableLayoutPanel2);
            this._downloadSettingsGroupBox.Location = new System.Drawing.Point(12, 214);
            this._downloadSettingsGroupBox.Name = "_downloadSettingsGroupBox";
            this._downloadSettingsGroupBox.Size = new System.Drawing.Size(534, 79);
            this._downloadSettingsGroupBox.TabIndex = 2;
            this._downloadSettingsGroupBox.TabStop = false;
            this._downloadSettingsGroupBox.Text = "Download settings";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this._downloadDirectoryBrowseButton, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this._downloadDirectoryPathTagButton, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this._downloadDirectoryTextBox, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this._batchSizeLabel, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this._batchSizeNumericUpDown, 1, 1);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(9, 22);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(9, 6, 9, 9);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(516, 53);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(0, 7);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "&Download folder:";
            // 
            // _batchSizeLabel
            // 
            this._batchSizeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._batchSizeLabel.AutoSize = true;
            this._batchSizeLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this._batchSizeLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._batchSizeLabel.Location = new System.Drawing.Point(0, 33);
            this._batchSizeLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this._batchSizeLabel.Name = "_batchSizeLabel";
            this._batchSizeLabel.Size = new System.Drawing.Size(59, 13);
            this._batchSizeLabel.TabIndex = 2;
            this._batchSizeLabel.Text = "&Batch size:";
            // 
            // _batchSizeNumericUpDown
            // 
            this._batchSizeNumericUpDown.Location = new System.Drawing.Point(93, 30);
            this._batchSizeNumericUpDown.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this._batchSizeNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._batchSizeNumericUpDown.Name = "_batchSizeNumericUpDown";
            this._batchSizeNumericUpDown.Size = new System.Drawing.Size(364, 20);
            this._batchSizeNumericUpDown.TabIndex = 3;
            this._batchSizeNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._batchSizeNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // EmailFileSupplierSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(558, 347);
            this.Controls.Add(this._downloadSettingsGroupBox);
            this.Controls.Add(this._emailSettingsGroupBox);
            this.Controls.Add(this._loginGroupBox);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 1000);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(463, 300);
            this.Name = "EmailFileSupplierSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Email file supplier settings";
            this._loginGroupBox.ResumeLayout(false);
            this._loginGroupBox.PerformLayout();
            this.loginTableLayoutPanel.ResumeLayout(false);
            this.loginTableLayoutPanel.PerformLayout();
            this._emailSettingsGroupBox.ResumeLayout(false);
            this._emailSettingsGroupBox.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this._downloadSettingsGroupBox.ResumeLayout(false);
            this._downloadSettingsGroupBox.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._batchSizeNumericUpDown)).EndInit();
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
        private System.Windows.Forms.TableLayoutPanel loginTableLayoutPanel;
        private System.Windows.Forms.Label _userNameLabel;
        private System.Windows.Forms.TextBox _userNameTextBox;
        private System.Windows.Forms.Label _passwordLabel;
        private System.Windows.Forms.TextBox _passwordTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox _postDownloadFolderTextBox;
        private System.Windows.Forms.Label _postDownloadFolderLabel;
        private System.Windows.Forms.Label _sharedEmailAddressLabel;
        private System.Windows.Forms.TextBox _sharedEmailAddressTextBox;
        private System.Windows.Forms.Label _inputFolderLabel;
        private System.Windows.Forms.TextBox _inputFolderTextBox;
        private System.Windows.Forms.GroupBox _downloadSettingsGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label _batchSizeLabel;
        private System.Windows.Forms.NumericUpDown _batchSizeNumericUpDown;
    }
}
