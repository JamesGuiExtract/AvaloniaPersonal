namespace Extract.FileActionManager.FileProcessors
{
    partial class SplitMimeFileSettingsDialog
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
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label1;
            Extract.Utilities.Forms.InfoTip infoTip1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplitMimeFileSettingsDialog));
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._queueGroupBox = new System.Windows.Forms.GroupBox();
            this._sourceIfFullyPaginatedActionLabel = new System.Windows.Forms.Label();
            this._sourceActionComboBox = new System.Windows.Forms.ComboBox();
            this._outputActionComboBox = new System.Windows.Forms.ComboBox();
            this._outputFolderGroupBox = new System.Windows.Forms.GroupBox();
            this._outputDirBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._outputDirTextBox = new System.Windows.Forms.TextBox();
            this._outputDirPathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            label3 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            infoTip1 = new Extract.Utilities.Forms.InfoTip();
            this._queueGroupBox.SuspendLayout();
            this._outputFolderGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(6, 27);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(146, 13);
            label3.TabIndex = 0;
            label3.Text = "Queue new files to this action";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 22);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(364, 13);
            label1.TabIndex = 0;
            label1.Text = "Output document folder (directory for extracted email body and attachments)";
            // 
            // infoTip1
            // 
            infoTip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip1.BackColor = System.Drawing.Color.Transparent;
            infoTip1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip1.BackgroundImage")));
            infoTip1.Location = new System.Drawing.Point(422, 17);
            infoTip1.Name = "infoTip1";
            infoTip1.Size = new System.Drawing.Size(16, 16);
            infoTip1.TabIndex = 2;
            infoTip1.TabStop = false;
            infoTip1.TipText = "The output file names will be based on the input file name, e.g.,SourceDocName_bo" +
    "dy_text.html or SourceDocName_attachment_001_AttachmentName.pdf";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(415, 204);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 3;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(334, 204);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 2;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _queueGroupBox
            // 
            this._queueGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._queueGroupBox.Controls.Add(this._sourceIfFullyPaginatedActionLabel);
            this._queueGroupBox.Controls.Add(this._sourceActionComboBox);
            this._queueGroupBox.Controls.Add(this._outputActionComboBox);
            this._queueGroupBox.Controls.Add(label3);
            this._queueGroupBox.Location = new System.Drawing.Point(11, 94);
            this._queueGroupBox.Name = "_queueGroupBox";
            this._queueGroupBox.Size = new System.Drawing.Size(479, 91);
            this._queueGroupBox.TabIndex = 1;
            this._queueGroupBox.TabStop = false;
            // 
            // _sourceIfFullyPaginatedActionLabel
            // 
            this._sourceIfFullyPaginatedActionLabel.AutoSize = true;
            this._sourceIfFullyPaginatedActionLabel.Location = new System.Drawing.Point(6, 59);
            this._sourceIfFullyPaginatedActionLabel.Name = "_sourceIfFullyPaginatedActionLabel";
            this._sourceIfFullyPaginatedActionLabel.Size = new System.Drawing.Size(186, 13);
            this._sourceIfFullyPaginatedActionLabel.TabIndex = 2;
            this._sourceIfFullyPaginatedActionLabel.Text = "Queue source, email, file to this action";
            // 
            // _sourceActionComboBox
            // 
            this._sourceActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceActionComboBox.FormattingEnabled = true;
            this._sourceActionComboBox.Location = new System.Drawing.Point(206, 56);
            this._sourceActionComboBox.Name = "_sourceActionComboBox";
            this._sourceActionComboBox.Size = new System.Drawing.Size(266, 21);
            this._sourceActionComboBox.TabIndex = 3;
            this._sourceActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _outputActionComboBox
            // 
            this._outputActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputActionComboBox.FormattingEnabled = true;
            this._outputActionComboBox.Location = new System.Drawing.Point(206, 24);
            this._outputActionComboBox.Name = "_outputActionComboBox";
            this._outputActionComboBox.Size = new System.Drawing.Size(266, 21);
            this._outputActionComboBox.TabIndex = 1;
            this._outputActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
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
            this._outputFolderGroupBox.Location = new System.Drawing.Point(12, 12);
            this._outputFolderGroupBox.Name = "_outputFolderGroupBox";
            this._outputFolderGroupBox.Size = new System.Drawing.Size(479, 76);
            this._outputFolderGroupBox.TabIndex = 0;
            this._outputFolderGroupBox.TabStop = false;
            // 
            // _outputDirBrowseButton
            // 
            this._outputDirBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputDirBrowseButton.EnsureFileExists = false;
            this._outputDirBrowseButton.EnsurePathExists = false;
            this._outputDirBrowseButton.FolderBrowser = true;
            this._outputDirBrowseButton.Location = new System.Drawing.Point(445, 39);
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
            this._outputDirTextBox.Size = new System.Drawing.Size(409, 20);
            this._outputDirTextBox.TabIndex = 1;
            // 
            // _outputDirPathTags
            // 
            this._outputDirPathTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputDirPathTags.Image = ((System.Drawing.Image)(resources.GetObject("_outputDirPathTags.Image")));
            this._outputDirPathTags.Location = new System.Drawing.Point(421, 39);
            this._outputDirPathTags.Name = "_outputDirPathTags";
            this._outputDirPathTags.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._outputDirPathTags.Size = new System.Drawing.Size(18, 21);
            this._outputDirPathTags.TabIndex = 3;
            this._outputDirPathTags.TextControl = this._outputDirTextBox;
            this._outputDirPathTags.UseVisualStyleBackColor = true;
            // 
            // SplitMimeFileSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(502, 238);
            this.Controls.Add(this._queueGroupBox);
            this.Controls.Add(this._outputFolderGroupBox);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 550);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(463, 277);
            this.Name = "SplitMimeFileSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Split MIME File Task";
            this._queueGroupBox.ResumeLayout(false);
            this._queueGroupBox.PerformLayout();
            this._outputFolderGroupBox.ResumeLayout(false);
            this._outputFolderGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.GroupBox _queueGroupBox;
        private System.Windows.Forms.Label _sourceIfFullyPaginatedActionLabel;
        private System.Windows.Forms.ComboBox _sourceActionComboBox;
        private System.Windows.Forms.ComboBox _outputActionComboBox;
        private System.Windows.Forms.GroupBox _outputFolderGroupBox;
        private Extract.Utilities.Forms.BrowseButton _outputDirBrowseButton;
        private System.Windows.Forms.TextBox _outputDirTextBox;
        private Forms.FileActionManagerPathTagButton _outputDirPathTags;
    }
}
