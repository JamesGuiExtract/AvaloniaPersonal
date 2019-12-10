namespace Extract.Redaction.Verification
{
    partial class FeedbackSettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="FeedbackSettingsDialog"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="FeedbackSettingsDialog"/>.
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FeedbackSettingsDialog));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._collectOriginalDocumentCheckBox = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this._dataFolderPathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._dataFolderBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._dataFolderTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this._uniqueFileNamesRadioButton = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this._originalFileNamesRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._collectCorrectedCheckBox = new System.Windows.Forms.CheckBox();
            this._collectRedactedCheckBox = new System.Windows.Forms.CheckBox();
            this._collectAllCheckBox = new System.Windows.Forms.CheckBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._collectOriginalDocumentCheckBox);
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this._dataFolderPathTagsButton);
            this.groupBox1.Controls.Add(this._dataFolderBrowseButton);
            this.groupBox1.Controls.Add(this._dataFolderTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(368, 112);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data storage and options";
            // 
            // _collectOriginalDocumentCheckBox
            // 
            this._collectOriginalDocumentCheckBox.AutoSize = true;
            this._collectOriginalDocumentCheckBox.Location = new System.Drawing.Point(10, 87);
            this._collectOriginalDocumentCheckBox.Name = "_collectOriginalDocumentCheckBox";
            this._collectOriginalDocumentCheckBox.Size = new System.Drawing.Size(147, 17);
            this._collectOriginalDocumentCheckBox.TabIndex = 5;
            this._collectOriginalDocumentCheckBox.Text = "Include original document";
            this._collectOriginalDocumentCheckBox.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Enabled = false;
            this.checkBox1.Location = new System.Drawing.Point(10, 64);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(162, 17);
            this.checkBox1.TabIndex = 4;
            this.checkBox1.Text = "Include redaction information";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // _dataFolderPathTagsButton
            // 
            this._dataFolderPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_dataFolderPathTagsButton.Image")));
            this._dataFolderPathTagsButton.Location = new System.Drawing.Point(311, 37);
            this._dataFolderPathTagsButton.Name = "_dataFolderPathTagsButton";
            this._dataFolderPathTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._dataFolderPathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._dataFolderPathTagsButton.TabIndex = 2;
            this._dataFolderPathTagsButton.TextControl = _dataFolderTextBox;
            this._dataFolderPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _dataFolderBrowseButton
            // 
            this._dataFolderBrowseButton.FolderBrowser = true;
            this._dataFolderBrowseButton.Location = new System.Drawing.Point(335, 37);
            this._dataFolderBrowseButton.Name = "_dataFolderBrowseButton";
            this._dataFolderBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._dataFolderBrowseButton.TabIndex = 3;
            this._dataFolderBrowseButton.Text = "...";
            this._dataFolderBrowseButton.UseVisualStyleBackColor = true;
            this._dataFolderBrowseButton.PathSelected += new System.EventHandler<Extract.Utilities.Forms.PathSelectedEventArgs>(this.HandleDataFolderBrowseButtonPathSelected);
            // 
            // _dataFolderTextBox
            // 
            this._dataFolderTextBox.Location = new System.Drawing.Point(10, 37);
            this._dataFolderTextBox.Name = "_dataFolderTextBox";
            this._dataFolderTextBox.Size = new System.Drawing.Size(295, 20);
            this._dataFolderTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Feedback data folder";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this._uniqueFileNamesRadioButton);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this._originalFileNamesRadioButton);
            this.groupBox2.Location = new System.Drawing.Point(13, 131);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(367, 100);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Filenames to use for feedback data";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(269, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "(Original filenames are only unique to a particular folder.)";
            // 
            // _uniqueFileNamesRadioButton
            // 
            this._uniqueFileNamesRadioButton.AutoSize = true;
            this._uniqueFileNamesRadioButton.Location = new System.Drawing.Point(9, 57);
            this._uniqueFileNamesRadioButton.Name = "_uniqueFileNamesRadioButton";
            this._uniqueFileNamesRadioButton.Size = new System.Drawing.Size(229, 17);
            this._uniqueFileNamesRadioButton.TabIndex = 1;
            this._uniqueFileNamesRadioButton.TabStop = true;
            this._uniqueFileNamesRadioButton.Text = "ID Shield should generate unique filenames";
            this._uniqueFileNamesRadioButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(267, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "(Original filenames are unique across the entire system.)";
            // 
            // _originalFileNamesRadioButton
            // 
            this._originalFileNamesRadioButton.AutoSize = true;
            this._originalFileNamesRadioButton.Location = new System.Drawing.Point(9, 20);
            this._originalFileNamesRadioButton.Name = "_originalFileNamesRadioButton";
            this._originalFileNamesRadioButton.Size = new System.Drawing.Size(127, 17);
            this._originalFileNamesRadioButton.TabIndex = 0;
            this._originalFileNamesRadioButton.TabStop = true;
            this._originalFileNamesRadioButton.Text = "Use original filenames";
            this._originalFileNamesRadioButton.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this._collectCorrectedCheckBox);
            this.groupBox3.Controls.Add(this._collectRedactedCheckBox);
            this.groupBox3.Controls.Add(this._collectAllCheckBox);
            this.groupBox3.Location = new System.Drawing.Point(13, 238);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(367, 92);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Collect feedback for";
            // 
            // _collectCorrectedCheckBox
            // 
            this._collectCorrectedCheckBox.AutoSize = true;
            this._collectCorrectedCheckBox.Location = new System.Drawing.Point(9, 67);
            this._collectCorrectedCheckBox.Name = "_collectCorrectedCheckBox";
            this._collectCorrectedCheckBox.Size = new System.Drawing.Size(266, 17);
            this._collectCorrectedCheckBox.TabIndex = 2;
            this._collectCorrectedCheckBox.Text = "All verified documents that contain user corrections";
            this._collectCorrectedCheckBox.UseVisualStyleBackColor = true;
            // 
            // _collectRedactedCheckBox
            // 
            this._collectRedactedCheckBox.AutoSize = true;
            this._collectRedactedCheckBox.Location = new System.Drawing.Point(9, 43);
            this._collectRedactedCheckBox.Name = "_collectRedactedCheckBox";
            this._collectRedactedCheckBox.Size = new System.Drawing.Size(308, 17);
            this._collectRedactedCheckBox.TabIndex = 1;
            this._collectRedactedCheckBox.Text = "All verified documents that have (or had) redactions or clues";
            this._collectRedactedCheckBox.UseVisualStyleBackColor = true;
            // 
            // _collectAllCheckBox
            // 
            this._collectAllCheckBox.AutoSize = true;
            this._collectAllCheckBox.Location = new System.Drawing.Point(9, 20);
            this._collectAllCheckBox.Name = "_collectAllCheckBox";
            this._collectAllCheckBox.Size = new System.Drawing.Size(129, 17);
            this._collectAllCheckBox.TabIndex = 0;
            this._collectAllCheckBox.Text = "All verified documents";
            this._collectAllCheckBox.UseVisualStyleBackColor = true;
            this._collectAllCheckBox.CheckedChanged += new System.EventHandler(this.HandleCollectAllCheckBoxCheckedChanged);
            // 
            // _cancelButton
            // 
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(304, 337);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 4;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Location = new System.Drawing.Point(223, 337);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 3;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // FeedbackSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(392, 366);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FeedbackSettingsDialog";
            this.ShowIcon = false;
            this.Text = "Feedback collection settings";
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox _collectOriginalDocumentCheckBox;
        private System.Windows.Forms.CheckBox checkBox1;
        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _dataFolderPathTagsButton;
        private Extract.Utilities.Forms.BrowseButton _dataFolderBrowseButton;
        private System.Windows.Forms.TextBox _dataFolderTextBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton _uniqueFileNamesRadioButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton _originalFileNamesRadioButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox _collectRedactedCheckBox;
        private System.Windows.Forms.CheckBox _collectAllCheckBox;
        private System.Windows.Forms.CheckBox _collectCorrectedCheckBox;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
    }
}