namespace Extract.Redaction.Verification
{
    partial class VerificationSettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="VerificationSettingsDialog"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="VerificationSettingsDialog"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources
                if (components != null)
                {
                    components.Dispose();
                }
            }

            // Release unmanaged resources

            // Call base dispose method
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerificationSettingsDialog));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._feedbackSettingsButton = new System.Windows.Forms.Button();
            this._collectFeedbackCheckBox = new System.Windows.Forms.CheckBox();
            this._requireExemptionsCheckBox = new System.Windows.Forms.CheckBox();
            this._requireTypeCheckBox = new System.Windows.Forms.CheckBox();
            this._verifyAllPagesCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._dataFilePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._dataFileBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._dataFileTextBox = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._metadataPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._metadataBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._metadataFileTextBox = new System.Windows.Forms.TextBox();
            this._onlyRedactionsRadioButton = new System.Windows.Forms.RadioButton();
            this._alwaysOutputMetadataRadioButton = new System.Windows.Forms.RadioButton();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._feedbackSettingsButton);
            this.groupBox1.Controls.Add(this._collectFeedbackCheckBox);
            this.groupBox1.Controls.Add(this._requireExemptionsCheckBox);
            this.groupBox1.Controls.Add(this._requireTypeCheckBox);
            this.groupBox1.Controls.Add(this._verifyAllPagesCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(368, 121);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "General";
            // 
            // _feedbackSettingsButton
            // 
            this._feedbackSettingsButton.Location = new System.Drawing.Point(274, 88);
            this._feedbackSettingsButton.Name = "_feedbackSettingsButton";
            this._feedbackSettingsButton.Size = new System.Drawing.Size(75, 23);
            this._feedbackSettingsButton.TabIndex = 4;
            this._feedbackSettingsButton.Text = "Settings...";
            this._feedbackSettingsButton.UseVisualStyleBackColor = true;
            this._feedbackSettingsButton.Click += new System.EventHandler(this.HandleFeedbackSettingsButtonClick);
            // 
            // _collectFeedbackCheckBox
            // 
            this._collectFeedbackCheckBox.AutoSize = true;
            this._collectFeedbackCheckBox.Location = new System.Drawing.Point(7, 92);
            this._collectFeedbackCheckBox.Name = "_collectFeedbackCheckBox";
            this._collectFeedbackCheckBox.Size = new System.Drawing.Size(261, 17);
            this._collectFeedbackCheckBox.TabIndex = 3;
            this._collectFeedbackCheckBox.Text = "Enable collection of redaction accuracy feedback";
            this._collectFeedbackCheckBox.UseVisualStyleBackColor = true;
            this._collectFeedbackCheckBox.CheckedChanged += new System.EventHandler(this.HandleCollectFeedbackCheckBoxCheckedChanged);
            // 
            // _requireExemptionsCheckBox
            // 
            this._requireExemptionsCheckBox.AutoSize = true;
            this._requireExemptionsCheckBox.Location = new System.Drawing.Point(7, 68);
            this._requireExemptionsCheckBox.Name = "_requireExemptionsCheckBox";
            this._requireExemptionsCheckBox.Size = new System.Drawing.Size(289, 17);
            this._requireExemptionsCheckBox.TabIndex = 2;
            this._requireExemptionsCheckBox.Text = "Require users to specify exemption codes for redactions";
            this._requireExemptionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // _requireTypeCheckBox
            // 
            this._requireTypeCheckBox.AutoSize = true;
            this._requireTypeCheckBox.Location = new System.Drawing.Point(7, 44);
            this._requireTypeCheckBox.Name = "_requireTypeCheckBox";
            this._requireTypeCheckBox.Size = new System.Drawing.Size(276, 17);
            this._requireTypeCheckBox.TabIndex = 1;
            this._requireTypeCheckBox.Text = "Require users to specify redaction type for redactions";
            this._requireTypeCheckBox.UseVisualStyleBackColor = true;
            // 
            // _verifyAllPagesCheckBox
            // 
            this._verifyAllPagesCheckBox.AutoSize = true;
            this._verifyAllPagesCheckBox.Location = new System.Drawing.Point(7, 20);
            this._verifyAllPagesCheckBox.Name = "_verifyAllPagesCheckBox";
            this._verifyAllPagesCheckBox.Size = new System.Drawing.Size(198, 17);
            this._verifyAllPagesCheckBox.TabIndex = 0;
            this._verifyAllPagesCheckBox.Text = "Include all pages in redaction review";
            this._verifyAllPagesCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this._dataFilePathTagsButton);
            this.groupBox2.Controls.Add(this._dataFileBrowseButton);
            this.groupBox2.Controls.Add(this._dataFileTextBox);
            this.groupBox2.Location = new System.Drawing.Point(12, 140);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(368, 50);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "ID Shield data file location";
            // 
            // _dataFilePathTagsButton
            // 
            this._dataFilePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_dataFilePathTagsButton.Image")));
            this._dataFilePathTagsButton.Location = new System.Drawing.Point(311, 20);
            this._dataFilePathTagsButton.Name = "_dataFilePathTagsButton";
            this._dataFilePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._dataFilePathTagsButton.TabIndex = 2;
            this._dataFilePathTagsButton.UseVisualStyleBackColor = true;
            this._dataFilePathTagsButton.TagSelected += new System.EventHandler<Extract.Utilities.Forms.TagSelectedEventArgs>(this.HandleDataFilePathTagsButtonTagSelected);
            // 
            // _dataFileBrowseButton
            // 
            this._dataFileBrowseButton.DefaultFilterIndex = -1;
            this._dataFileBrowseButton.FileFilter = "VOA files (*.voa)|*.voa||";
            this._dataFileBrowseButton.FileOrFolderPath = null;
            this._dataFileBrowseButton.FolderBrowser = false;
            this._dataFileBrowseButton.Location = new System.Drawing.Point(335, 20);
            this._dataFileBrowseButton.Name = "_dataFileBrowseButton";
            this._dataFileBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._dataFileBrowseButton.TabIndex = 1;
            this._dataFileBrowseButton.Text = "...";
            this._dataFileBrowseButton.TextControl = null;
            this._dataFileBrowseButton.UseVisualStyleBackColor = true;
            this._dataFileBrowseButton.PathSelected += new System.EventHandler<Extract.Utilities.Forms.PathSelectedEventArgs>(this.HandleDataFileBrowseButtonPathSelected);
            // 
            // _dataFileTextBox
            // 
            this._dataFileTextBox.Location = new System.Drawing.Point(7, 20);
            this._dataFileTextBox.Name = "_dataFileTextBox";
            this._dataFileTextBox.Size = new System.Drawing.Size(298, 20);
            this._dataFileTextBox.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this._metadataPathTagsButton);
            this.groupBox3.Controls.Add(this._metadataBrowseButton);
            this.groupBox3.Controls.Add(this._metadataFileTextBox);
            this.groupBox3.Controls.Add(this._onlyRedactionsRadioButton);
            this.groupBox3.Controls.Add(this._alwaysOutputMetadataRadioButton);
            this.groupBox3.Location = new System.Drawing.Point(12, 197);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(368, 100);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Metadata output";
            // 
            // _metadataPathTagsButton
            // 
            this._metadataPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_metadataPathTagsButton.Image")));
            this._metadataPathTagsButton.Location = new System.Drawing.Point(311, 68);
            this._metadataPathTagsButton.Name = "_metadataPathTagsButton";
            this._metadataPathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._metadataPathTagsButton.TabIndex = 4;
            this._metadataPathTagsButton.UseVisualStyleBackColor = true;
            this._metadataPathTagsButton.TagSelected += new System.EventHandler<Extract.Utilities.Forms.TagSelectedEventArgs>(this.HandleMetadataPathTagsButtonTagSelected);
            // 
            // _metadataBrowseButton
            // 
            this._metadataBrowseButton.DefaultFilterIndex = -1;
            this._metadataBrowseButton.FileFilter = null;
            this._metadataBrowseButton.FileOrFolderPath = null;
            this._metadataBrowseButton.FolderBrowser = true;
            this._metadataBrowseButton.Location = new System.Drawing.Point(335, 68);
            this._metadataBrowseButton.Name = "_metadataBrowseButton";
            this._metadataBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._metadataBrowseButton.TabIndex = 3;
            this._metadataBrowseButton.Text = "...";
            this._metadataBrowseButton.TextControl = null;
            this._metadataBrowseButton.UseVisualStyleBackColor = true;
            this._metadataBrowseButton.PathSelected += new System.EventHandler<Extract.Utilities.Forms.PathSelectedEventArgs>(this.HandleMetadataBrowseButtonPathSelected);
            // 
            // _metadataFileTextBox
            // 
            this._metadataFileTextBox.Location = new System.Drawing.Point(7, 68);
            this._metadataFileTextBox.Name = "_metadataFileTextBox";
            this._metadataFileTextBox.Size = new System.Drawing.Size(298, 20);
            this._metadataFileTextBox.TabIndex = 2;
            // 
            // _onlyRedactionsRadioButton
            // 
            this._onlyRedactionsRadioButton.AutoSize = true;
            this._onlyRedactionsRadioButton.Location = new System.Drawing.Point(7, 44);
            this._onlyRedactionsRadioButton.Name = "_onlyRedactionsRadioButton";
            this._onlyRedactionsRadioButton.Size = new System.Drawing.Size(339, 17);
            this._onlyRedactionsRadioButton.TabIndex = 1;
            this._onlyRedactionsRadioButton.TabStop = true;
            this._onlyRedactionsRadioButton.Text = "Create metadata output only for documents that contain redactions";
            this._onlyRedactionsRadioButton.UseVisualStyleBackColor = true;
            // 
            // _alwaysOutputMetadataRadioButton
            // 
            this._alwaysOutputMetadataRadioButton.AutoSize = true;
            this._alwaysOutputMetadataRadioButton.Location = new System.Drawing.Point(7, 20);
            this._alwaysOutputMetadataRadioButton.Name = "_alwaysOutputMetadataRadioButton";
            this._alwaysOutputMetadataRadioButton.Size = new System.Drawing.Size(219, 17);
            this._alwaysOutputMetadataRadioButton.TabIndex = 0;
            this._alwaysOutputMetadataRadioButton.TabStop = true;
            this._alwaysOutputMetadataRadioButton.Text = "Create metadata output for all documents";
            this._alwaysOutputMetadataRadioButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(305, 304);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 3;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Location = new System.Drawing.Point(224, 304);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 4;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // VerificationSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(392, 334);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VerificationSettingsDialog";
            this.ShowIcon = false;
            this.Text = "Verify redactions settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox _verifyAllPagesCheckBox;
        private System.Windows.Forms.Button _feedbackSettingsButton;
        private System.Windows.Forms.CheckBox _collectFeedbackCheckBox;
        private System.Windows.Forms.CheckBox _requireExemptionsCheckBox;
        private System.Windows.Forms.CheckBox _requireTypeCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private Extract.Utilities.Forms.PathTagsButton _dataFilePathTagsButton;
        private Extract.Utilities.Forms.BrowseButton _dataFileBrowseButton;
        private System.Windows.Forms.TextBox _dataFileTextBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton _alwaysOutputMetadataRadioButton;
        private Extract.Utilities.Forms.PathTagsButton _metadataPathTagsButton;
        private Extract.Utilities.Forms.BrowseButton _metadataBrowseButton;
        private System.Windows.Forms.TextBox _metadataFileTextBox;
        private System.Windows.Forms.RadioButton _onlyRedactionsRadioButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
    }
}