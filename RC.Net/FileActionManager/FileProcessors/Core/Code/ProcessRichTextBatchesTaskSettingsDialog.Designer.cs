namespace Extract.FileActionManager.FileProcessors
{
    partial class ProcessRichTextBatchesTaskSettingsDialog
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
            System.Windows.Forms.Label label3;
            Extract.Utilities.Forms.InfoTip infoTip1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProcessRichTextBatchesTaskSettingsDialog));
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            Extract.Utilities.Forms.InfoTip infoTip2;
            Extract.Utilities.Forms.InfoTip infoTip3;
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._sourceIfFullyPaginatedActionLabel = new System.Windows.Forms.Label();
            this._sourceActionComboBox = new System.Windows.Forms.ComboBox();
            this._outputFolderGroupBox = new System.Windows.Forms.GroupBox();
            this._outputDirBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._outputDirTextBox = new System.Windows.Forms.TextBox();
            this._outputDirPathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._outputActionComboBox = new System.Windows.Forms.ComboBox();
            this._typeGroupBox = new System.Windows.Forms.GroupBox();
            this._updateBatchWithRedactedFilesRadioButton = new System.Windows.Forms.RadioButton();
            this._divideBatchIntoFilesRadioButton = new System.Windows.Forms.RadioButton();
            this._queueGroupBox = new System.Windows.Forms.GroupBox();
            this._redactedFileBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._redactedOutputFileTextBox = new System.Windows.Forms.TextBox();
            this._redactedFilePathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._redactionGroupBox = new System.Windows.Forms.GroupBox();
            this._redactionActionComboBox = new System.Windows.Forms.ComboBox();
            this._updatedBatchFileTextBox = new System.Windows.Forms.TextBox();
            this._updatedBatchFileBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._updatedBatchFilePathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            label1 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            infoTip1 = new Extract.Utilities.Forms.InfoTip();
            label2 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            infoTip2 = new Extract.Utilities.Forms.InfoTip();
            infoTip3 = new Extract.Utilities.Forms.InfoTip();
            this._outputFolderGroupBox.SuspendLayout();
            this._typeGroupBox.SuspendLayout();
            this._queueGroupBox.SuspendLayout();
            this._redactionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 22);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(227, 13);
            label1.TabIndex = 0;
            label1.Text = "Output document folder (directory for RTF files)";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(6, 27);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(210, 13);
            label3.TabIndex = 0;
            label3.Text = "Queue new, RTF, documents to this action";
            // 
            // infoTip1
            // 
            infoTip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip1.BackColor = System.Drawing.Color.Transparent;
            infoTip1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip1.BackgroundImage")));
            infoTip1.Location = new System.Drawing.Point(606, 17);
            infoTip1.Name = "infoTip1";
            infoTip1.Size = new System.Drawing.Size(16, 16);
            infoTip1.TabIndex = 2;
            infoTip1.TabStop = false;
            infoTip1.TipText = "<SubBatchNumber> (each sub-batch will have up to 1000 output files)";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(7, 67);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(344, 13);
            label2.TabIndex = 3;
            label2.Text = "Path of redacted RTF files (path tag function of output document name)";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(7, 120);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(352, 13);
            label4.TabIndex = 8;
            label4.Text = "Path for updated batch files (path tag function of source document name)";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(6, 34);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(281, 13);
            label5.TabIndex = 0;
            label5.Text = "Require all output documents to be Complete in this action";
            // 
            // infoTip2
            // 
            infoTip2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip2.BackColor = System.Drawing.Color.Transparent;
            infoTip2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip2.BackgroundImage")));
            infoTip2.Location = new System.Drawing.Point(293, 19);
            infoTip2.Name = "infoTip2";
            infoTip2.Size = new System.Drawing.Size(16, 16);
            infoTip2.TabIndex = 1;
            infoTip2.TabStop = false;
            infoTip2.TipText = "\"all output documents\" means all documents where Pagination.SourceFileID = the pr" +
    "ocessing file\'s ID";
            // 
            // infoTip3
            // 
            infoTip3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip3.BackColor = System.Drawing.Color.Transparent;
            infoTip3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip3.BackgroundImage")));
            infoTip3.Location = new System.Drawing.Point(357, 58);
            infoTip3.Name = "infoTip3";
            infoTip3.Size = new System.Drawing.Size(16, 16);
            infoTip3.TabIndex = 5;
            infoTip3.TabStop = false;
            infoTip3.TipText = "\"output document name\" is calculated by combining the output document folder abov" +
    "e with information in the batch file (e.g., line number and sub-file number)";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(599, 477);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 5;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(518, 477);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 4;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _sourceIfFullyPaginatedActionLabel
            // 
            this._sourceIfFullyPaginatedActionLabel.AutoSize = true;
            this._sourceIfFullyPaginatedActionLabel.Location = new System.Drawing.Point(6, 59);
            this._sourceIfFullyPaginatedActionLabel.Name = "_sourceIfFullyPaginatedActionLabel";
            this._sourceIfFullyPaginatedActionLabel.Size = new System.Drawing.Size(228, 13);
            this._sourceIfFullyPaginatedActionLabel.TabIndex = 2;
            this._sourceIfFullyPaginatedActionLabel.Text = "Queue source, batch, documents to this action";
            // 
            // _sourceActionComboBox
            // 
            this._sourceActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceActionComboBox.FormattingEnabled = true;
            this._sourceActionComboBox.Location = new System.Drawing.Point(243, 56);
            this._sourceActionComboBox.Name = "_sourceActionComboBox";
            this._sourceActionComboBox.Size = new System.Drawing.Size(413, 21);
            this._sourceActionComboBox.TabIndex = 3;
            this._sourceActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _outputFolderGroupBox
            // 
            this._outputFolderGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputFolderGroupBox.Controls.Add(label1);
            this._outputFolderGroupBox.Controls.Add(this._outputDirBrowseButton);
            this._outputFolderGroupBox.Controls.Add(this._outputDirPathTags);
            this._outputFolderGroupBox.Controls.Add(infoTip1);
            this._outputFolderGroupBox.Controls.Add(this._outputDirTextBox);
            this._outputFolderGroupBox.Location = new System.Drawing.Point(11, 91);
            this._outputFolderGroupBox.Name = "_outputFolderGroupBox";
            this._outputFolderGroupBox.Size = new System.Drawing.Size(663, 77);
            this._outputFolderGroupBox.TabIndex = 1;
            this._outputFolderGroupBox.TabStop = false;
            // 
            // _outputDirBrowseButton
            // 
            this._outputDirBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputDirBrowseButton.EnsureFileExists = false;
            this._outputDirBrowseButton.EnsurePathExists = false;
            this._outputDirBrowseButton.FolderBrowser = true;
            this._outputDirBrowseButton.Location = new System.Drawing.Point(629, 39);
            this._outputDirBrowseButton.Name = "_outputDirBrowseButton";
            this._outputDirBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._outputDirBrowseButton.TabIndex = 4;
            this._outputDirBrowseButton.Text = "...";
            this._outputDirBrowseButton.TextControl = this._outputDirTextBox;
            this._outputDirBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _outputDirTextBox
            // 
            this._outputDirTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputDirTextBox.Location = new System.Drawing.Point(6, 40);
            this._outputDirTextBox.Name = "_outputDirTextBox";
            this._outputDirTextBox.Size = new System.Drawing.Size(593, 20);
            this._outputDirTextBox.TabIndex = 1;
            // 
            // _outputDirPathTags
            // 
            this._outputDirPathTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputDirPathTags.Image = ((System.Drawing.Image)(resources.GetObject("_outputDirPathTags.Image")));
            this._outputDirPathTags.Location = new System.Drawing.Point(605, 39);
            this._outputDirPathTags.Name = "_outputDirPathTags";
            this._outputDirPathTags.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._outputDirPathTags.Size = new System.Drawing.Size(18, 21);
            this._outputDirPathTags.TabIndex = 3;
            this._outputDirPathTags.TextControl = this._outputDirTextBox;
            this._outputDirPathTags.UseVisualStyleBackColor = true;
            // 
            // _outputActionComboBox
            // 
            this._outputActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputActionComboBox.FormattingEnabled = true;
            this._outputActionComboBox.Location = new System.Drawing.Point(243, 24);
            this._outputActionComboBox.Name = "_outputActionComboBox";
            this._outputActionComboBox.Size = new System.Drawing.Size(413, 21);
            this._outputActionComboBox.TabIndex = 1;
            this._outputActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _typeGroupBox
            // 
            this._typeGroupBox.Controls.Add(this._updateBatchWithRedactedFilesRadioButton);
            this._typeGroupBox.Controls.Add(this._divideBatchIntoFilesRadioButton);
            this._typeGroupBox.Location = new System.Drawing.Point(12, 12);
            this._typeGroupBox.Name = "_typeGroupBox";
            this._typeGroupBox.Size = new System.Drawing.Size(662, 73);
            this._typeGroupBox.TabIndex = 0;
            this._typeGroupBox.TabStop = false;
            // 
            // _updateBatchWithRedactedFilesRadioButton
            // 
            this._updateBatchWithRedactedFilesRadioButton.AutoSize = true;
            this._updateBatchWithRedactedFilesRadioButton.Location = new System.Drawing.Point(9, 42);
            this._updateBatchWithRedactedFilesRadioButton.Name = "_updateBatchWithRedactedFilesRadioButton";
            this._updateBatchWithRedactedFilesRadioButton.Size = new System.Drawing.Size(178, 17);
            this._updateBatchWithRedactedFilesRadioButton.TabIndex = 1;
            this._updateBatchWithRedactedFilesRadioButton.TabStop = true;
            this._updateBatchWithRedactedFilesRadioButton.Text = "Update batch with redacted files";
            this._updateBatchWithRedactedFilesRadioButton.UseVisualStyleBackColor = true;
            // 
            // _divideBatchIntoFilesRadioButton
            // 
            this._divideBatchIntoFilesRadioButton.AutoSize = true;
            this._divideBatchIntoFilesRadioButton.Location = new System.Drawing.Point(9, 19);
            this._divideBatchIntoFilesRadioButton.Name = "_divideBatchIntoFilesRadioButton";
            this._divideBatchIntoFilesRadioButton.Size = new System.Drawing.Size(126, 17);
            this._divideBatchIntoFilesRadioButton.TabIndex = 0;
            this._divideBatchIntoFilesRadioButton.TabStop = true;
            this._divideBatchIntoFilesRadioButton.Text = "Divide batch into files";
            this._divideBatchIntoFilesRadioButton.UseVisualStyleBackColor = true;
            this._divideBatchIntoFilesRadioButton.CheckedChanged += new System.EventHandler(this._divideBatchIntoFilesRadioButton_CheckedChanged);
            // 
            // _queueGroupBox
            // 
            this._queueGroupBox.Controls.Add(this._sourceIfFullyPaginatedActionLabel);
            this._queueGroupBox.Controls.Add(this._sourceActionComboBox);
            this._queueGroupBox.Controls.Add(this._outputActionComboBox);
            this._queueGroupBox.Controls.Add(label3);
            this._queueGroupBox.Location = new System.Drawing.Point(11, 355);
            this._queueGroupBox.Name = "_queueGroupBox";
            this._queueGroupBox.Size = new System.Drawing.Size(663, 99);
            this._queueGroupBox.TabIndex = 3;
            this._queueGroupBox.TabStop = false;
            // 
            // _redactedFileBrowseButton
            // 
            this._redactedFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._redactedFileBrowseButton.EnsureFileExists = false;
            this._redactedFileBrowseButton.EnsurePathExists = false;
            this._redactedFileBrowseButton.Location = new System.Drawing.Point(629, 86);
            this._redactedFileBrowseButton.Name = "_redactedFileBrowseButton";
            this._redactedFileBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._redactedFileBrowseButton.TabIndex = 7;
            this._redactedFileBrowseButton.Text = "...";
            this._redactedFileBrowseButton.TextControl = this._redactedOutputFileTextBox;
            this._redactedFileBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _redactedOutputFileTextBox
            // 
            this._redactedOutputFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._redactedOutputFileTextBox.Location = new System.Drawing.Point(6, 87);
            this._redactedOutputFileTextBox.Name = "_redactedOutputFileTextBox";
            this._redactedOutputFileTextBox.Size = new System.Drawing.Size(593, 20);
            this._redactedOutputFileTextBox.TabIndex = 4;
            // 
            // _redactedFilePathTagButton
            // 
            this._redactedFilePathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._redactedFilePathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_redactedFilePathTagButton.Image")));
            this._redactedFilePathTagButton.Location = new System.Drawing.Point(606, 86);
            this._redactedFilePathTagButton.Name = "_redactedFilePathTagButton";
            this._redactedFilePathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._redactedFilePathTagButton.Size = new System.Drawing.Size(18, 21);
            this._redactedFilePathTagButton.TabIndex = 6;
            this._redactedFilePathTagButton.TextControl = this._redactedOutputFileTextBox;
            this._redactedFilePathTagButton.UseVisualStyleBackColor = true;
            // 
            // _redactionGroupBox
            // 
            this._redactionGroupBox.Controls.Add(infoTip3);
            this._redactionGroupBox.Controls.Add(infoTip2);
            this._redactionGroupBox.Controls.Add(this._redactionActionComboBox);
            this._redactionGroupBox.Controls.Add(label5);
            this._redactionGroupBox.Controls.Add(label4);
            this._redactionGroupBox.Controls.Add(this._updatedBatchFileTextBox);
            this._redactionGroupBox.Controls.Add(this._updatedBatchFileBrowseButton);
            this._redactionGroupBox.Controls.Add(this._updatedBatchFilePathTagButton);
            this._redactionGroupBox.Controls.Add(label2);
            this._redactionGroupBox.Controls.Add(this._redactedOutputFileTextBox);
            this._redactionGroupBox.Controls.Add(this._redactedFileBrowseButton);
            this._redactionGroupBox.Controls.Add(this._redactedFilePathTagButton);
            this._redactionGroupBox.Location = new System.Drawing.Point(11, 174);
            this._redactionGroupBox.Name = "_redactionGroupBox";
            this._redactionGroupBox.Size = new System.Drawing.Size(663, 175);
            this._redactionGroupBox.TabIndex = 2;
            this._redactionGroupBox.TabStop = false;
            this._redactionGroupBox.Text = "Create/update batch file";
            // 
            // _redactionActionComboBox
            // 
            this._redactionActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._redactionActionComboBox.FormattingEnabled = true;
            this._redactionActionComboBox.Location = new System.Drawing.Point(320, 31);
            this._redactionActionComboBox.Name = "_redactionActionComboBox";
            this._redactionActionComboBox.Size = new System.Drawing.Size(336, 21);
            this._redactionActionComboBox.TabIndex = 2;
            // 
            // _updatedBatchFileTextBox
            // 
            this._updatedBatchFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._updatedBatchFileTextBox.Location = new System.Drawing.Point(7, 141);
            this._updatedBatchFileTextBox.Name = "_updatedBatchFileTextBox";
            this._updatedBatchFileTextBox.Size = new System.Drawing.Size(593, 20);
            this._updatedBatchFileTextBox.TabIndex = 9;
            // 
            // _updatedBatchFileBrowseButton
            // 
            this._updatedBatchFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._updatedBatchFileBrowseButton.EnsureFileExists = false;
            this._updatedBatchFileBrowseButton.EnsurePathExists = false;
            this._updatedBatchFileBrowseButton.Location = new System.Drawing.Point(630, 140);
            this._updatedBatchFileBrowseButton.Name = "_updatedBatchFileBrowseButton";
            this._updatedBatchFileBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._updatedBatchFileBrowseButton.TabIndex = 11;
            this._updatedBatchFileBrowseButton.Text = "...";
            this._updatedBatchFileBrowseButton.TextControl = this._updatedBatchFileTextBox;
            this._updatedBatchFileBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _updatedBatchFilePathTagButton
            // 
            this._updatedBatchFilePathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._updatedBatchFilePathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_updatedBatchFilePathTagButton.Image")));
            this._updatedBatchFilePathTagButton.Location = new System.Drawing.Point(607, 140);
            this._updatedBatchFilePathTagButton.Name = "_updatedBatchFilePathTagButton";
            this._updatedBatchFilePathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._updatedBatchFilePathTagButton.Size = new System.Drawing.Size(18, 21);
            this._updatedBatchFilePathTagButton.TabIndex = 10;
            this._updatedBatchFilePathTagButton.TextControl = this._updatedBatchFileTextBox;
            this._updatedBatchFilePathTagButton.UseVisualStyleBackColor = true;
            // 
            // ProcessRichTextBatchesTaskSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(686, 511);
            this.Controls.Add(this._redactionGroupBox);
            this.Controls.Add(this._queueGroupBox);
            this.Controls.Add(this._typeGroupBox);
            this.Controls.Add(this._outputFolderGroupBox);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(702, 600);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(463, 245);
            this.Name = "ProcessRichTextBatchesTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Process RTF Batches Task";
            this._outputFolderGroupBox.ResumeLayout(false);
            this._outputFolderGroupBox.PerformLayout();
            this._typeGroupBox.ResumeLayout(false);
            this._typeGroupBox.PerformLayout();
            this._queueGroupBox.ResumeLayout(false);
            this._queueGroupBox.PerformLayout();
            this._redactionGroupBox.ResumeLayout(false);
            this._redactionGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label _sourceIfFullyPaginatedActionLabel;
        private System.Windows.Forms.ComboBox _sourceActionComboBox;
        private System.Windows.Forms.GroupBox _outputFolderGroupBox;
        private System.Windows.Forms.ComboBox _outputActionComboBox;
        private Extract.Utilities.Forms.BrowseButton _outputDirBrowseButton;
        private System.Windows.Forms.TextBox _outputDirTextBox;
        private Forms.FileActionManagerPathTagButton _outputDirPathTags;
        private System.Windows.Forms.GroupBox _typeGroupBox;
        private System.Windows.Forms.RadioButton _updateBatchWithRedactedFilesRadioButton;
        private System.Windows.Forms.RadioButton _divideBatchIntoFilesRadioButton;
        private System.Windows.Forms.GroupBox _queueGroupBox;
        private Extract.Utilities.Forms.BrowseButton _redactedFileBrowseButton;
        private System.Windows.Forms.TextBox _redactedOutputFileTextBox;
        private Forms.FileActionManagerPathTagButton _redactedFilePathTagButton;
        private System.Windows.Forms.GroupBox _redactionGroupBox;
        private System.Windows.Forms.ComboBox _redactionActionComboBox;
        private System.Windows.Forms.TextBox _updatedBatchFileTextBox;
        private Extract.Utilities.Forms.BrowseButton _updatedBatchFileBrowseButton;
        private Forms.FileActionManagerPathTagButton _updatedBatchFilePathTagButton;
    }
}
