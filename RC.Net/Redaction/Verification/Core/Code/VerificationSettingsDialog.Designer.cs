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
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.Label label1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerificationSettingsDialog));
            System.Windows.Forms.GroupBox groupBox3;
            System.Windows.Forms.GroupBox groupBox4;
            this._seamlessNavigationCheckBox = new System.Windows.Forms.CheckBox();
            this._verifyAllItemsCheckBox = new System.Windows.Forms.CheckBox();
            this._launchFullScreenCheckBox = new System.Windows.Forms.CheckBox();
            this._enableInputEventTrackingCheckBox = new System.Windows.Forms.CheckBox();
            this._feedbackSettingsButton = new System.Windows.Forms.Button();
            this._collectFeedbackCheckBox = new System.Windows.Forms.CheckBox();
            this._requireExemptionsCheckBox = new System.Windows.Forms.CheckBox();
            this._requireTypeCheckBox = new System.Windows.Forms.CheckBox();
            this._verifyAllPagesCheckBox = new System.Windows.Forms.CheckBox();
            this._promptForSaveUntilCommit = new System.Windows.Forms.CheckBox();
            this._actionNamePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._actionStatusComboBox = new System.Windows.Forms.ComboBox();
            this._actionNameComboBox = new Extract.Utilities.Forms.BetterComboBox();
            this._fileActionCheckBox = new System.Windows.Forms.CheckBox();
            this._backdropImagePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._backdropImageTextBox = new System.Windows.Forms.TextBox();
            this._backdropImageBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._backdropImageCheckBox = new System.Windows.Forms.CheckBox();
            this._slideshowSettingsButton = new System.Windows.Forms.Button();
            this._enableSlideshowCheckBox = new System.Windows.Forms.CheckBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._dataFileControl = new Extract.Redaction.DataFileControl();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            groupBox3 = new System.Windows.Forms.GroupBox();
            groupBox4 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._seamlessNavigationCheckBox);
            groupBox1.Controls.Add(this._verifyAllItemsCheckBox);
            groupBox1.Controls.Add(this._launchFullScreenCheckBox);
            groupBox1.Controls.Add(this._enableInputEventTrackingCheckBox);
            groupBox1.Controls.Add(this._feedbackSettingsButton);
            groupBox1.Controls.Add(this._collectFeedbackCheckBox);
            groupBox1.Controls.Add(this._requireExemptionsCheckBox);
            groupBox1.Controls.Add(this._requireTypeCheckBox);
            groupBox1.Controls.Add(this._verifyAllPagesCheckBox);
            groupBox1.Controls.Add(this._promptForSaveUntilCommit);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(429, 228);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "General";
            // 
            // _seamlessNavigationCheckBox
            // 
            this._seamlessNavigationCheckBox.AutoSize = true;
            this._seamlessNavigationCheckBox.Location = new System.Drawing.Point(7, 112);
            this._seamlessNavigationCheckBox.Name = "_seamlessNavigationCheckBox";
            this._seamlessNavigationCheckBox.Size = new System.Drawing.Size(283, 17);
            this._seamlessNavigationCheckBox.TabIndex = 4;
            this._seamlessNavigationCheckBox.Text = "Enable seamless page navigation between documents";
            this._seamlessNavigationCheckBox.UseVisualStyleBackColor = true;
            // 
            // _verifyAllItemsCheckBox
            // 
            this._verifyAllItemsCheckBox.AutoSize = true;
            this._verifyAllItemsCheckBox.Location = new System.Drawing.Point(7, 43);
            this._verifyAllItemsCheckBox.Name = "_verifyAllItemsCheckBox";
            this._verifyAllItemsCheckBox.Size = new System.Drawing.Size(300, 17);
            this._verifyAllItemsCheckBox.TabIndex = 1;
            this._verifyAllItemsCheckBox.Text = "Require users to review every suggested redaction or clue";
            this._verifyAllItemsCheckBox.UseVisualStyleBackColor = true;
            // 
            // _launchFullScreenCheckBox
            // 
            this._launchFullScreenCheckBox.AutoSize = true;
            this._launchFullScreenCheckBox.Location = new System.Drawing.Point(7, 183);
            this._launchFullScreenCheckBox.Name = "_launchFullScreenCheckBox";
            this._launchFullScreenCheckBox.Size = new System.Drawing.Size(236, 17);
            this._launchFullScreenCheckBox.TabIndex = 8;
            this._launchFullScreenCheckBox.Text = "Open verification window in full screen mode";
            this._launchFullScreenCheckBox.UseVisualStyleBackColor = true;
            // 
            // _enableInputEventTrackingCheckBox
            // 
            this._enableInputEventTrackingCheckBox.AutoSize = true;
            this._enableInputEventTrackingCheckBox.Location = new System.Drawing.Point(7, 159);
            this._enableInputEventTrackingCheckBox.Name = "_enableInputEventTrackingCheckBox";
            this._enableInputEventTrackingCheckBox.Size = new System.Drawing.Size(156, 17);
            this._enableInputEventTrackingCheckBox.TabIndex = 7;
            this._enableInputEventTrackingCheckBox.Text = "Enable input event tracking";
            this._enableInputEventTrackingCheckBox.UseVisualStyleBackColor = true;
            // 
            // _feedbackSettingsButton
            // 
            this._feedbackSettingsButton.Location = new System.Drawing.Point(274, 132);
            this._feedbackSettingsButton.Name = "_feedbackSettingsButton";
            this._feedbackSettingsButton.Size = new System.Drawing.Size(75, 23);
            this._feedbackSettingsButton.TabIndex = 6;
            this._feedbackSettingsButton.Text = "Settings...";
            this._feedbackSettingsButton.UseVisualStyleBackColor = true;
            this._feedbackSettingsButton.Click += new System.EventHandler(this.HandleFeedbackSettingsButtonClick);
            // 
            // _collectFeedbackCheckBox
            // 
            this._collectFeedbackCheckBox.AutoSize = true;
            this._collectFeedbackCheckBox.Location = new System.Drawing.Point(7, 136);
            this._collectFeedbackCheckBox.Name = "_collectFeedbackCheckBox";
            this._collectFeedbackCheckBox.Size = new System.Drawing.Size(261, 17);
            this._collectFeedbackCheckBox.TabIndex = 5;
            this._collectFeedbackCheckBox.Text = "Enable collection of redaction accuracy feedback";
            this._collectFeedbackCheckBox.UseVisualStyleBackColor = true;
            this._collectFeedbackCheckBox.CheckedChanged += new System.EventHandler(this.HandleCollectFeedbackCheckBoxCheckedChanged);
            // 
            // _requireExemptionsCheckBox
            // 
            this._requireExemptionsCheckBox.AutoSize = true;
            this._requireExemptionsCheckBox.Location = new System.Drawing.Point(7, 89);
            this._requireExemptionsCheckBox.Name = "_requireExemptionsCheckBox";
            this._requireExemptionsCheckBox.Size = new System.Drawing.Size(289, 17);
            this._requireExemptionsCheckBox.TabIndex = 3;
            this._requireExemptionsCheckBox.Text = "Require users to specify exemption codes for redactions";
            this._requireExemptionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // _requireTypeCheckBox
            // 
            this._requireTypeCheckBox.AutoSize = true;
            this._requireTypeCheckBox.Location = new System.Drawing.Point(7, 66);
            this._requireTypeCheckBox.Name = "_requireTypeCheckBox";
            this._requireTypeCheckBox.Size = new System.Drawing.Size(276, 17);
            this._requireTypeCheckBox.TabIndex = 2;
            this._requireTypeCheckBox.Text = "Require users to specify redaction type for redactions";
            this._requireTypeCheckBox.UseVisualStyleBackColor = true;
            // 
            // _verifyAllPagesCheckBox
            // 
            this._verifyAllPagesCheckBox.AutoSize = true;
            this._verifyAllPagesCheckBox.Location = new System.Drawing.Point(7, 20);
            this._verifyAllPagesCheckBox.Name = "_verifyAllPagesCheckBox";
            this._verifyAllPagesCheckBox.Size = new System.Drawing.Size(239, 17);
            this._verifyAllPagesCheckBox.TabIndex = 0;
            this._verifyAllPagesCheckBox.Text = "Require users to visit all pages in a document";
            this._verifyAllPagesCheckBox.UseVisualStyleBackColor = true;
            // 
            // _promptForSaveUntilCommit
            // 
            this._promptForSaveUntilCommit.AutoSize = true;
            this._promptForSaveUntilCommit.Location = new System.Drawing.Point(7, 205);
            this._promptForSaveUntilCommit.Name = "_promptForSaveUntilCommit";
            this._promptForSaveUntilCommit.Size = new System.Drawing.Size(398, 17);
            this._promptForSaveUntilCommit.TabIndex = 9;
            this._promptForSaveUntilCommit.Text = "Prompt to save changes before navigating away from uncommitted documents.";
            this._promptForSaveUntilCommit.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(this._actionNamePathTagsButton);
            groupBox2.Controls.Add(this._actionStatusComboBox);
            groupBox2.Controls.Add(this._actionNameComboBox);
            groupBox2.Controls.Add(this._fileActionCheckBox);
            groupBox2.Location = new System.Drawing.Point(12, 448);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(429, 78);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "After committing a document";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(221, 46);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(16, 13);
            label1.TabIndex = 3;
            label1.Text = "to";
            // 
            // _actionNamePathTagsButton
            // 
            this._actionNamePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_actionNamePathTagsButton.Image")));
            this._actionNamePathTagsButton.Location = new System.Drawing.Point(197, 43);
            this._actionNamePathTagsButton.Name = "_actionNamePathTagsButton";
            this._actionNamePathTagsButton.PathTags = new Extract.Utilities.FileActionManagerPathTags();
            this._actionNamePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._actionNamePathTagsButton.TabIndex = 2;
            this._actionNamePathTagsButton.UseVisualStyleBackColor = true;
            this._actionNamePathTagsButton.TagSelected += new System.EventHandler<Extract.Utilities.Forms.TagSelectedEventArgs>(this.HandleActionNamePathTagsButtonTagSelected);
            // 
            // _actionStatusComboBox
            // 
            this._actionStatusComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._actionStatusComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._actionStatusComboBox.FormattingEnabled = true;
            this._actionStatusComboBox.Items.AddRange(new object[] {
            "Completed",
            "Failed",
            "Pending",
            "Skipped",
            "Unattempted"});
            this._actionStatusComboBox.Location = new System.Drawing.Point(243, 43);
            this._actionStatusComboBox.Name = "_actionStatusComboBox";
            this._actionStatusComboBox.Size = new System.Drawing.Size(179, 21);
            this._actionStatusComboBox.TabIndex = 4;
            // 
            // _actionNameComboBox
            // 
            this._actionNameComboBox.FormattingEnabled = true;
            this._actionNameComboBox.HideSelection = false;
            this._actionNameComboBox.Location = new System.Drawing.Point(7, 43);
            this._actionNameComboBox.Name = "_actionNameComboBox";
            this._actionNameComboBox.Size = new System.Drawing.Size(184, 21);
            this._actionNameComboBox.TabIndex = 1;
            // 
            // _fileActionCheckBox
            // 
            this._fileActionCheckBox.AutoSize = true;
            this._fileActionCheckBox.Location = new System.Drawing.Point(7, 19);
            this._fileActionCheckBox.Name = "_fileActionCheckBox";
            this._fileActionCheckBox.Size = new System.Drawing.Size(133, 17);
            this._fileActionCheckBox.TabIndex = 0;
            this._fileActionCheckBox.Text = "Set file action status of";
            this._fileActionCheckBox.UseVisualStyleBackColor = true;
            this._fileActionCheckBox.CheckedChanged += new System.EventHandler(this.HandleFileActionCheckBoxCheckedChanged);
            // 
            // groupBox3
            // 
            groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox3.Controls.Add(this._backdropImagePathTagsButton);
            groupBox3.Controls.Add(this._backdropImageBrowseButton);
            groupBox3.Controls.Add(this._backdropImageTextBox);
            groupBox3.Controls.Add(this._backdropImageCheckBox);
            groupBox3.Location = new System.Drawing.Point(11, 313);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(429, 76);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Image location";
            // 
            // _backdropImagePathTagsButton
            // 
            this._backdropImagePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._backdropImagePathTagsButton.Enabled = false;
            this._backdropImagePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_backdropImagePathTagsButton.Image")));
            this._backdropImagePathTagsButton.Location = new System.Drawing.Point(372, 41);
            this._backdropImagePathTagsButton.Name = "_backdropImagePathTagsButton";
            this._backdropImagePathTagsButton.PathTags = new Extract.Utilities.FileActionManagerPathTags();
            this._backdropImagePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._backdropImagePathTagsButton.TabIndex = 2;
            this._backdropImagePathTagsButton.TextControl = this._backdropImageTextBox;
            this._backdropImagePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _backdropImageTextBox
            // 
            this._backdropImageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._backdropImageTextBox.Enabled = false;
            this._backdropImageTextBox.HideSelection = false;
            this._backdropImageTextBox.Location = new System.Drawing.Point(6, 42);
            this._backdropImageTextBox.Name = "_backdropImageTextBox";
            this._backdropImageTextBox.Size = new System.Drawing.Size(360, 20);
            this._backdropImageTextBox.TabIndex = 1;
            // 
            // _backdropImageBrowseButton
            // 
            this._backdropImageBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._backdropImageBrowseButton.Enabled = false;
            this._backdropImageBrowseButton.Location = new System.Drawing.Point(396, 41);
            this._backdropImageBrowseButton.Name = "_backdropImageBrowseButton";
            this._backdropImageBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._backdropImageBrowseButton.TabIndex = 3;
            this._backdropImageBrowseButton.Text = "...";
            this._backdropImageBrowseButton.UseVisualStyleBackColor = true;
            this._backdropImageBrowseButton.PathSelected += new System.EventHandler<Extract.Utilities.Forms.PathSelectedEventArgs>(this.HandleBackdropImageBrowseButtonPathSelected);
            // 
            // _backdropImageCheckBox
            // 
            this._backdropImageCheckBox.AutoSize = true;
            this._backdropImageCheckBox.Location = new System.Drawing.Point(7, 19);
            this._backdropImageCheckBox.Name = "_backdropImageCheckBox";
            this._backdropImageCheckBox.Size = new System.Drawing.Size(260, 17);
            this._backdropImageCheckBox.TabIndex = 0;
            this._backdropImageCheckBox.Text = "Use image as backdrop for verification if available";
            this._backdropImageCheckBox.UseVisualStyleBackColor = true;
            this._backdropImageCheckBox.CheckedChanged += new System.EventHandler(this.HandleBackdropImageCheckBoxCheckedChanged);
            // 
            // groupBox4
            // 
            groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox4.Controls.Add(this._slideshowSettingsButton);
            groupBox4.Controls.Add(this._enableSlideshowCheckBox);
            groupBox4.Location = new System.Drawing.Point(12, 395);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new System.Drawing.Size(429, 47);
            groupBox4.TabIndex = 3;
            groupBox4.TabStop = false;
            groupBox4.Text = "Slideshow";
            // 
            // _slideshowSettingsButton
            // 
            this._slideshowSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._slideshowSettingsButton.Location = new System.Drawing.Point(348, 15);
            this._slideshowSettingsButton.Name = "_slideshowSettingsButton";
            this._slideshowSettingsButton.Size = new System.Drawing.Size(75, 23);
            this._slideshowSettingsButton.TabIndex = 1;
            this._slideshowSettingsButton.Text = "Settings...";
            this._slideshowSettingsButton.UseVisualStyleBackColor = true;
            this._slideshowSettingsButton.Click += new System.EventHandler(this.HandleSlideshowSettingsButtonClick);
            // 
            // _enableSlideshowCheckBox
            // 
            this._enableSlideshowCheckBox.AutoSize = true;
            this._enableSlideshowCheckBox.Checked = true;
            this._enableSlideshowCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._enableSlideshowCheckBox.Location = new System.Drawing.Point(6, 19);
            this._enableSlideshowCheckBox.Name = "_enableSlideshowCheckBox";
            this._enableSlideshowCheckBox.Size = new System.Drawing.Size(187, 17);
            this._enableSlideshowCheckBox.TabIndex = 0;
            this._enableSlideshowCheckBox.Text = "Enable slideshow in verification UI";
            this._enableSlideshowCheckBox.UseVisualStyleBackColor = true;
            this._enableSlideshowCheckBox.CheckedChanged += new System.EventHandler(this.HandleEnableSlideshowCheckBoxCheckedChanged);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(366, 532);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 6;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(285, 532);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 5;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _dataFileControl
            // 
            this._dataFileControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFileControl.Location = new System.Drawing.Point(11, 246);
            this._dataFileControl.Name = "_dataFileControl";
            this._dataFileControl.Size = new System.Drawing.Size(429, 60);
            this._dataFileControl.TabIndex = 1;
            // 
            // VerificationSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(453, 567);
            this.Controls.Add(groupBox4);
            this.Controls.Add(groupBox3);
            this.Controls.Add(groupBox2);
            this.Controls.Add(this._dataFileControl);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VerificationSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Redaction: Verify redactions settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox _verifyAllPagesCheckBox;
        private System.Windows.Forms.Button _feedbackSettingsButton;
        private System.Windows.Forms.CheckBox _collectFeedbackCheckBox;
        private System.Windows.Forms.CheckBox _requireExemptionsCheckBox;
        private System.Windows.Forms.CheckBox _requireTypeCheckBox;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private DataFileControl _dataFileControl;
        private System.Windows.Forms.CheckBox _enableInputEventTrackingCheckBox;
        private System.Windows.Forms.CheckBox _fileActionCheckBox;
        private System.Windows.Forms.ComboBox _actionStatusComboBox;
        private Extract.Utilities.Forms.BetterComboBox _actionNameComboBox;
        private Extract.Utilities.Forms.PathTagsButton _actionNamePathTagsButton;
        private System.Windows.Forms.CheckBox _backdropImageCheckBox;
        private Extract.Utilities.Forms.PathTagsButton _backdropImagePathTagsButton;
        private Extract.Utilities.Forms.BrowseButton _backdropImageBrowseButton;
        private System.Windows.Forms.TextBox _backdropImageTextBox;
        private System.Windows.Forms.CheckBox _launchFullScreenCheckBox;
        private System.Windows.Forms.Button _slideshowSettingsButton;
        private System.Windows.Forms.CheckBox _enableSlideshowCheckBox;
        private System.Windows.Forms.CheckBox _seamlessNavigationCheckBox;
        private System.Windows.Forms.CheckBox _verifyAllItemsCheckBox;
        private System.Windows.Forms.CheckBox _promptForSaveUntilCommit;
    }
}