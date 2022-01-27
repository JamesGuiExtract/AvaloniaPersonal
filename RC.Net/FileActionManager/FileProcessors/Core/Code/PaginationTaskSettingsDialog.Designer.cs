namespace Extract.FileActionManager.FileProcessors
{
    partial class PaginationTaskSettingsDialog
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            Extract.Utilities.Forms.InfoTip infoTip1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaginationTaskSettingsDialog));
            System.Windows.Forms.Label label4;
            Extract.Utilities.Forms.InfoTip infoTip5;
            this._sourceActionComboBox = new System.Windows.Forms.ComboBox();
            this._outputActionComboBox = new System.Windows.Forms.ComboBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._outputPathPathTags = new Extract.Utilities.Forms.PathTagsButton();
            this._outputPathTextBox = new Extract.Utilities.Forms.BetterTextBox();
            this._outputPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._documentDataAssemblyPathTags = new Extract.Utilities.Forms.PathTagsButton();
            this._documentDataAssemblyTextBox = new Extract.Utilities.Forms.BetterTextBox();
            this._documentDataAssemblyBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._expectedPaginationAttributesCheckBox = new System.Windows.Forms.CheckBox();
            this._expectedPaginationAttributesPathTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this._expectedPaginationAttributesTextBox = new Extract.Utilities.Forms.BetterTextBox();
            this._expectedPaginationAttributesBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._singleDocModeCheckBox = new System.Windows.Forms.CheckBox();
            this._autoRotateCheckBox = new System.Windows.Forms.CheckBox();
            this._defaultToCollapsedCheckBox = new System.Windows.Forms.CheckBox();
            this._selectAllVisibleCheckBox = new System.Windows.Forms.CheckBox();
            this._loadNextDocumentVisibleCheckBox = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            infoTip1 = new Extract.Utilities.Forms.InfoTip();
            label4 = new System.Windows.Forms.Label();
            infoTip5 = new Extract.Utilities.Forms.InfoTip();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(162, 13);
            label1.TabIndex = 0;
            label1.Text = "Paginated document output path";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(13, 58);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(411, 13);
            label2.TabIndex = 5;
            label2.Text = "After committing documents, set the original source document to pending in this a" +
    "ction";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(13, 105);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(276, 13);
            label3.TabIndex = 7;
            label3.Text = "Set paginated output documents to pending in this action";
            // 
            // infoTip1
            // 
            infoTip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip1.BackColor = System.Drawing.Color.Transparent;
            infoTip1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip1.BackgroundImage")));
            infoTip1.Location = new System.Drawing.Point(493, 6);
            infoTip1.Name = "infoTip1";
            infoTip1.Size = new System.Drawing.Size(16, 16);
            infoTip1.TabIndex = 2;
            infoTip1.TabStop = false;
            infoTip1.TipText = resources.GetString("infoTip1.TipText");
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(13, 151);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(216, 13);
            label4.TabIndex = 9;
            label4.Text = "Specify a data entry configuration file to use:";
            // 
            // infoTip5
            // 
            infoTip5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip5.BackColor = System.Drawing.Color.Transparent;
            infoTip5.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip5.BackgroundImage")));
            infoTip5.Location = new System.Drawing.Point(492, 195);
            infoTip5.Name = "infoTip5";
            infoTip5.Size = new System.Drawing.Size(16, 16);
            infoTip5.TabIndex = 15;
            infoTip5.TabStop = false;
            infoTip5.TipText = "These VOA files will not be output for documents created via pagination";
            // 
            // _sourceActionComboBox
            // 
            this._sourceActionComboBox.FormattingEnabled = true;
            this._sourceActionComboBox.Location = new System.Drawing.Point(16, 75);
            this._sourceActionComboBox.Name = "_sourceActionComboBox";
            this._sourceActionComboBox.Size = new System.Drawing.Size(233, 21);
            this._sourceActionComboBox.TabIndex = 6;
            this._sourceActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _outputActionComboBox
            // 
            this._outputActionComboBox.FormattingEnabled = true;
            this._outputActionComboBox.Location = new System.Drawing.Point(16, 121);
            this._outputActionComboBox.Name = "_outputActionComboBox";
            this._outputActionComboBox.Size = new System.Drawing.Size(233, 21);
            this._outputActionComboBox.TabIndex = 8;
            this._outputActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(468, 356);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 24;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(387, 356);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 23;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _outputPathPathTags
            // 
            this._outputPathPathTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathPathTags.Image = ((System.Drawing.Image)(resources.GetObject("_outputPathPathTags.Image")));
            this._outputPathPathTags.Location = new System.Drawing.Point(492, 28);
            this._outputPathPathTags.Name = "_outputPathPathTags";
            this._outputPathPathTags.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._outputPathPathTags.Size = new System.Drawing.Size(18, 21);
            this._outputPathPathTags.TabIndex = 3;
            this._outputPathPathTags.TextControl = this._outputPathTextBox;
            this._outputPathPathTags.UseVisualStyleBackColor = true;
            // 
            // _outputPathTextBox
            // 
            this._outputPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathTextBox.Location = new System.Drawing.Point(16, 29);
            this._outputPathTextBox.Name = "_outputPathTextBox";
            this._outputPathTextBox.Required = true;
            this._outputPathTextBox.Size = new System.Drawing.Size(470, 20);
            this._outputPathTextBox.TabIndex = 1;
            // 
            // _outputPathBrowseButton
            // 
            this._outputPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathBrowseButton.Location = new System.Drawing.Point(516, 28);
            this._outputPathBrowseButton.Name = "_outputPathBrowseButton";
            this._outputPathBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._outputPathBrowseButton.TabIndex = 4;
            this._outputPathBrowseButton.Text = "...";
            this._outputPathBrowseButton.TextControl = this._outputPathTextBox;
            this._outputPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _documentDataAssemblyPathTags
            // 
            this._documentDataAssemblyPathTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._documentDataAssemblyPathTags.Image = ((System.Drawing.Image)(resources.GetObject("_documentDataAssemblyPathTags.Image")));
            this._documentDataAssemblyPathTags.Location = new System.Drawing.Point(492, 166);
            this._documentDataAssemblyPathTags.Name = "_documentDataAssemblyPathTags";
            this._documentDataAssemblyPathTags.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._documentDataAssemblyPathTags.Size = new System.Drawing.Size(18, 21);
            this._documentDataAssemblyPathTags.TabIndex = 11;
            this._documentDataAssemblyPathTags.TextControl = this._documentDataAssemblyTextBox;
            this._documentDataAssemblyPathTags.UseVisualStyleBackColor = true;
            // 
            // _documentDataAssemblyTextBox
            // 
            this._documentDataAssemblyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._documentDataAssemblyTextBox.Location = new System.Drawing.Point(16, 167);
            this._documentDataAssemblyTextBox.Name = "_documentDataAssemblyTextBox";
            this._documentDataAssemblyTextBox.Size = new System.Drawing.Size(470, 20);
            this._documentDataAssemblyTextBox.TabIndex = 10;
            // 
            // _documentDataAssemblyBrowseButton
            // 
            this._documentDataAssemblyBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._documentDataAssemblyBrowseButton.Location = new System.Drawing.Point(516, 166);
            this._documentDataAssemblyBrowseButton.Name = "_documentDataAssemblyBrowseButton";
            this._documentDataAssemblyBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._documentDataAssemblyBrowseButton.TabIndex = 12;
            this._documentDataAssemblyBrowseButton.Text = "...";
            this._documentDataAssemblyBrowseButton.TextControl = this._documentDataAssemblyTextBox;
            this._documentDataAssemblyBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _expectedPaginationAttributesCheckBox
            // 
            this._expectedPaginationAttributesCheckBox.AutoSize = true;
            this._expectedPaginationAttributesCheckBox.Location = new System.Drawing.Point(16, 197);
            this._expectedPaginationAttributesCheckBox.Name = "_expectedPaginationAttributesCheckBox";
            this._expectedPaginationAttributesCheckBox.Size = new System.Drawing.Size(258, 17);
            this._expectedPaginationAttributesCheckBox.TabIndex = 13;
            this._expectedPaginationAttributesCheckBox.Text = "Output expected pagination VOA files to this path";
            this._expectedPaginationAttributesCheckBox.UseVisualStyleBackColor = true;
            this._expectedPaginationAttributesCheckBox.CheckedChanged += new System.EventHandler(this.HandleExpectedPaginationAttributesCheckBox_CheckedChanged);
            // 
            // _expectedPaginationAttributesPathTagButton
            // 
            this._expectedPaginationAttributesPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._expectedPaginationAttributesPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_expectedPaginationAttributesPathTagButton.Image")));
            this._expectedPaginationAttributesPathTagButton.Location = new System.Drawing.Point(492, 217);
            this._expectedPaginationAttributesPathTagButton.Name = "_expectedPaginationAttributesPathTagButton";
            this._expectedPaginationAttributesPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._expectedPaginationAttributesPathTagButton.Size = new System.Drawing.Size(18, 21);
            this._expectedPaginationAttributesPathTagButton.TabIndex = 16;
            this._expectedPaginationAttributesPathTagButton.TextControl = this._expectedPaginationAttributesTextBox;
            this._expectedPaginationAttributesPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _expectedPaginationAttributesTextBox
            // 
            this._expectedPaginationAttributesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._expectedPaginationAttributesTextBox.Location = new System.Drawing.Point(16, 218);
            this._expectedPaginationAttributesTextBox.Name = "_expectedPaginationAttributesTextBox";
            this._expectedPaginationAttributesTextBox.Required = true;
            this._expectedPaginationAttributesTextBox.Size = new System.Drawing.Size(470, 20);
            this._expectedPaginationAttributesTextBox.TabIndex = 14;
            // 
            // _expectedPaginationAttributesBrowseButton
            // 
            this._expectedPaginationAttributesBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._expectedPaginationAttributesBrowseButton.Location = new System.Drawing.Point(516, 217);
            this._expectedPaginationAttributesBrowseButton.Name = "_expectedPaginationAttributesBrowseButton";
            this._expectedPaginationAttributesBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._expectedPaginationAttributesBrowseButton.TabIndex = 17;
            this._expectedPaginationAttributesBrowseButton.Text = "...";
            this._expectedPaginationAttributesBrowseButton.TextControl = this._expectedPaginationAttributesTextBox;
            this._expectedPaginationAttributesBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _singleDocModeCheckBox
            // 
            this._singleDocModeCheckBox.AutoSize = true;
            this._singleDocModeCheckBox.Location = new System.Drawing.Point(16, 248);
            this._singleDocModeCheckBox.Name = "_singleDocModeCheckBox";
            this._singleDocModeCheckBox.Size = new System.Drawing.Size(374, 17);
            this._singleDocModeCheckBox.TabIndex = 18;
            this._singleDocModeCheckBox.Text = "Single source document mode (verify only one source document at a time)";
            this._singleDocModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // _autoRotateCheckBox
            // 
            this._autoRotateCheckBox.AutoSize = true;
            this._autoRotateCheckBox.Location = new System.Drawing.Point(16, 294);
            this._autoRotateCheckBox.Name = "_autoRotateCheckBox";
            this._autoRotateCheckBox.Size = new System.Drawing.Size(199, 17);
            this._autoRotateCheckBox.TabIndex = 20;
            this._autoRotateCheckBox.Text = "Automatically rotate pages to vertical";
            this._autoRotateCheckBox.UseVisualStyleBackColor = true;
            // 
            // _defaultToCollapsedCheckBox
            // 
            this._defaultToCollapsedCheckBox.AutoSize = true;
            this._defaultToCollapsedCheckBox.Location = new System.Drawing.Point(16, 271);
            this._defaultToCollapsedCheckBox.Name = "_defaultToCollapsedCheckBox";
            this._defaultToCollapsedCheckBox.Size = new System.Drawing.Size(204, 17);
            this._defaultToCollapsedCheckBox.TabIndex = 19;
            this._defaultToCollapsedCheckBox.Text = "Document pages collapsed by default";
            this._defaultToCollapsedCheckBox.UseVisualStyleBackColor = true;
            // 
            // _selectAllVisibleCheckBox
            // 
            this._selectAllVisibleCheckBox.AutoSize = true;
            this._selectAllVisibleCheckBox.Location = new System.Drawing.Point(16, 317);
            this._selectAllVisibleCheckBox.Name = "_selectAllVisibleCheckBox";
            this._selectAllVisibleCheckBox.Size = new System.Drawing.Size(129, 17);
            this._selectAllVisibleCheckBox.TabIndex = 21;
            this._selectAllVisibleCheckBox.Text = "Show select all option";
            this._selectAllVisibleCheckBox.UseVisualStyleBackColor = true;
            // 
            // _loadNextDocumentVisibleCheckBox
            // 
            this._loadNextDocumentVisibleCheckBox.AutoSize = true;
            this._loadNextDocumentVisibleCheckBox.Location = new System.Drawing.Point(16, 340);
            this._loadNextDocumentVisibleCheckBox.Name = "_loadNextDocumentVisibleCheckBox";
            this._loadNextDocumentVisibleCheckBox.Size = new System.Drawing.Size(182, 17);
            this._loadNextDocumentVisibleCheckBox.TabIndex = 22;
            this._loadNextDocumentVisibleCheckBox.Text = "Show load next document button";
            this._loadNextDocumentVisibleCheckBox.UseVisualStyleBackColor = true;
            // 
            // PaginationTaskSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(555, 392);
            this.Controls.Add(this._loadNextDocumentVisibleCheckBox);
            this.Controls.Add(this._selectAllVisibleCheckBox);
            this.Controls.Add(this._defaultToCollapsedCheckBox);
            this.Controls.Add(this._autoRotateCheckBox);
            this.Controls.Add(this._singleDocModeCheckBox);
            this.Controls.Add(this._expectedPaginationAttributesCheckBox);
            this.Controls.Add(infoTip5);
            this.Controls.Add(this._expectedPaginationAttributesPathTagButton);
            this.Controls.Add(this._expectedPaginationAttributesBrowseButton);
            this.Controls.Add(this._expectedPaginationAttributesTextBox);
            this.Controls.Add(this._documentDataAssemblyPathTags);
            this.Controls.Add(this._documentDataAssemblyBrowseButton);
            this.Controls.Add(this._documentDataAssemblyTextBox);
            this.Controls.Add(label4);
            this.Controls.Add(infoTip1);
            this.Controls.Add(this._outputPathPathTags);
            this.Controls.Add(this._outputPathBrowseButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._outputActionComboBox);
            this.Controls.Add(label3);
            this.Controls.Add(this._sourceActionComboBox);
            this.Controls.Add(label2);
            this.Controls.Add(this._outputPathTextBox);
            this.Controls.Add(label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(571, 430);
            this.Name = "PaginationTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pagination Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Extract.Utilities.Forms.BetterTextBox _outputPathTextBox;
        private System.Windows.Forms.ComboBox _sourceActionComboBox;
        private System.Windows.Forms.ComboBox _outputActionComboBox;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private Extract.Utilities.Forms.PathTagsButton _outputPathPathTags;
        private Extract.Utilities.Forms.BrowseButton _outputPathBrowseButton;
        private Extract.Utilities.Forms.PathTagsButton _documentDataAssemblyPathTags;
        private Extract.Utilities.Forms.BetterTextBox _documentDataAssemblyTextBox;
        private Extract.Utilities.Forms.BrowseButton _documentDataAssemblyBrowseButton;
        private System.Windows.Forms.CheckBox _expectedPaginationAttributesCheckBox;
        private Extract.Utilities.Forms.PathTagsButton _expectedPaginationAttributesPathTagButton;
        private Extract.Utilities.Forms.BetterTextBox _expectedPaginationAttributesTextBox;
        private Extract.Utilities.Forms.BrowseButton _expectedPaginationAttributesBrowseButton;
        private System.Windows.Forms.CheckBox _singleDocModeCheckBox;
        private System.Windows.Forms.CheckBox _autoRotateCheckBox;
        private System.Windows.Forms.CheckBox _defaultToCollapsedCheckBox;
        private System.Windows.Forms.CheckBox _selectAllVisibleCheckBox;
        private System.Windows.Forms.CheckBox _loadNextDocumentVisibleCheckBox;
    }
}