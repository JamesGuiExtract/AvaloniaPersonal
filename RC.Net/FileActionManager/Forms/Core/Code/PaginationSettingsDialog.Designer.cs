namespace Extract.FileActionManager.Forms
{
    partial class PaginationSettingsDialog
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
            System.Windows.Forms.Label label4;
            Extract.Utilities.Forms.InfoTip infoTip5;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaginationSettingsDialog));
            Extract.Utilities.Forms.InfoTip infoTip1;
            this._sourceActionComboBox = new System.Windows.Forms.ComboBox();
            this._outputActionComboBox = new System.Windows.Forms.ComboBox();
            this._outputPriorityComboBox = new System.Windows.Forms.ComboBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._expectedPaginationAttributesCheckBox = new System.Windows.Forms.CheckBox();
            this._expectedPaginationAttributesPathTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this._expectedPaginationAttributesTextBox = new Extract.Utilities.Forms.BetterTextBox();
            this._expectedPaginationAttributesBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._outputPathPathTags = new Extract.Utilities.Forms.PathTagsButton();
            this._outputPathTextBox = new Extract.Utilities.Forms.BetterTextBox();
            this._outputPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._requireAllPagesToBeViewedCheckBox = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            infoTip5 = new Extract.Utilities.Forms.InfoTip();
            infoTip1 = new Extract.Utilities.Forms.InfoTip();
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
            label3.TabIndex = 8;
            label3.Text = "Set paginated output documents to pending in this action";
            // 
            // label4
            // 
            label4.Location = new System.Drawing.Point(13, 151);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(509, 30);
            label4.TabIndex = 12;
            label4.Text = "For any document returned to processing, set the priority of the output document " +
    "to the document priority or the following action, whichever is greater";
            // 
            // infoTip5
            // 
            infoTip5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip5.BackColor = System.Drawing.Color.Transparent;
            infoTip5.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip5.BackgroundImage")));
            infoTip5.Location = new System.Drawing.Point(492, 213);
            infoTip5.Name = "infoTip5";
            infoTip5.Size = new System.Drawing.Size(16, 16);
            infoTip5.TabIndex = 17;
            infoTip5.TabStop = false;
            infoTip5.TipText = "These VOA files will not be output for documents created via pagination";
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
            // _sourceActionComboBox
            // 
            this._sourceActionComboBox.FormattingEnabled = true;
            this._sourceActionComboBox.Location = new System.Drawing.Point(16, 75);
            this._sourceActionComboBox.Name = "_sourceActionComboBox";
            this._sourceActionComboBox.Size = new System.Drawing.Size(233, 21);
            this._sourceActionComboBox.TabIndex = 3;
            this._sourceActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _outputActionComboBox
            // 
            this._outputActionComboBox.FormattingEnabled = true;
            this._outputActionComboBox.Location = new System.Drawing.Point(16, 121);
            this._outputActionComboBox.Name = "_outputActionComboBox";
            this._outputActionComboBox.Size = new System.Drawing.Size(233, 21);
            this._outputActionComboBox.TabIndex = 4;
            this._outputActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _outputPriorityComboBox
            // 
            this._outputPriorityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._outputPriorityComboBox.FormattingEnabled = true;
            this._outputPriorityComboBox.Location = new System.Drawing.Point(16, 184);
            this._outputPriorityComboBox.Name = "_outputPriorityComboBox";
            this._outputPriorityComboBox.Size = new System.Drawing.Size(159, 21);
            this._outputPriorityComboBox.TabIndex = 5;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(468, 313);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 11;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(387, 313);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 10;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _expectedPaginationAttributesCheckBox
            // 
            this._expectedPaginationAttributesCheckBox.AutoSize = true;
            this._expectedPaginationAttributesCheckBox.Location = new System.Drawing.Point(16, 215);
            this._expectedPaginationAttributesCheckBox.Name = "_expectedPaginationAttributesCheckBox";
            this._expectedPaginationAttributesCheckBox.Size = new System.Drawing.Size(258, 17);
            this._expectedPaginationAttributesCheckBox.TabIndex = 6;
            this._expectedPaginationAttributesCheckBox.Text = "Output expected pagination VOA files to this path";
            this._expectedPaginationAttributesCheckBox.UseVisualStyleBackColor = true;
            this._expectedPaginationAttributesCheckBox.CheckedChanged += new System.EventHandler(this.HandleExpectedPaginationAttributesCheckBox_CheckedChanged);
            // 
            // _expectedPaginationAttributesPathTagButton
            // 
            this._expectedPaginationAttributesPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._expectedPaginationAttributesPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_expectedPaginationAttributesPathTagButton.Image")));
            this._expectedPaginationAttributesPathTagButton.Location = new System.Drawing.Point(492, 235);
            this._expectedPaginationAttributesPathTagButton.Name = "_expectedPaginationAttributesPathTagButton";
            this._expectedPaginationAttributesPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._expectedPaginationAttributesPathTagButton.Size = new System.Drawing.Size(18, 21);
            this._expectedPaginationAttributesPathTagButton.TabIndex = 7;
            this._expectedPaginationAttributesPathTagButton.TextControl = this._expectedPaginationAttributesTextBox;
            this._expectedPaginationAttributesPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _expectedPaginationAttributesTextBox
            // 
            this._expectedPaginationAttributesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._expectedPaginationAttributesTextBox.Location = new System.Drawing.Point(16, 236);
            this._expectedPaginationAttributesTextBox.Name = "_expectedPaginationAttributesTextBox";
            this._expectedPaginationAttributesTextBox.Required = true;
            this._expectedPaginationAttributesTextBox.Size = new System.Drawing.Size(470, 20);
            this._expectedPaginationAttributesTextBox.TabIndex = 6;
            // 
            // _expectedPaginationAttributesBrowseButton
            // 
            this._expectedPaginationAttributesBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._expectedPaginationAttributesBrowseButton.Location = new System.Drawing.Point(516, 235);
            this._expectedPaginationAttributesBrowseButton.Name = "_expectedPaginationAttributesBrowseButton";
            this._expectedPaginationAttributesBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._expectedPaginationAttributesBrowseButton.TabIndex = 8;
            this._expectedPaginationAttributesBrowseButton.Text = "...";
            this._expectedPaginationAttributesBrowseButton.TextControl = this._expectedPaginationAttributesTextBox;
            this._expectedPaginationAttributesBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _outputPathPathTags
            // 
            this._outputPathPathTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathPathTags.Image = ((System.Drawing.Image)(resources.GetObject("_outputPathPathTags.Image")));
            this._outputPathPathTags.Location = new System.Drawing.Point(492, 28);
            this._outputPathPathTags.Name = "_outputPathPathTags";
            this._outputPathPathTags.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._outputPathPathTags.Size = new System.Drawing.Size(18, 21);
            this._outputPathPathTags.TabIndex = 1;
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
            this._outputPathTextBox.TabIndex = 0;
            // 
            // _outputPathBrowseButton
            // 
            this._outputPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathBrowseButton.Location = new System.Drawing.Point(516, 28);
            this._outputPathBrowseButton.Name = "_outputPathBrowseButton";
            this._outputPathBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._outputPathBrowseButton.TabIndex = 2;
            this._outputPathBrowseButton.Text = "...";
            this._outputPathBrowseButton.TextControl = this._outputPathTextBox;
            this._outputPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _requireAllPagesToBeViewedCheckBox
            // 
            this._requireAllPagesToBeViewedCheckBox.AutoSize = true;
            this._requireAllPagesToBeViewedCheckBox.Location = new System.Drawing.Point(16, 266);
            this._requireAllPagesToBeViewedCheckBox.Name = "_requireAllPagesToBeViewedCheckBox";
            this._requireAllPagesToBeViewedCheckBox.Size = new System.Drawing.Size(172, 17);
            this._requireAllPagesToBeViewedCheckBox.TabIndex = 9;
            this._requireAllPagesToBeViewedCheckBox.Text = "Require all pages to be viewed";
            this._requireAllPagesToBeViewedCheckBox.UseVisualStyleBackColor = true;
            // 
            // PaginationSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(555, 349);
            this.Controls.Add(this._requireAllPagesToBeViewedCheckBox);
            this.Controls.Add(this._expectedPaginationAttributesCheckBox);
            this.Controls.Add(infoTip5);
            this.Controls.Add(this._expectedPaginationAttributesPathTagButton);
            this.Controls.Add(this._expectedPaginationAttributesBrowseButton);
            this.Controls.Add(this._expectedPaginationAttributesTextBox);
            this.Controls.Add(infoTip1);
            this.Controls.Add(this._outputPathPathTags);
            this.Controls.Add(this._outputPathBrowseButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._outputPriorityComboBox);
            this.Controls.Add(label4);
            this.Controls.Add(this._outputActionComboBox);
            this.Controls.Add(label3);
            this.Controls.Add(this._sourceActionComboBox);
            this.Controls.Add(label2);
            this.Controls.Add(this._outputPathTextBox);
            this.Controls.Add(label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(571, 369);
            this.Name = "PaginationSettingsDialog";
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
        private System.Windows.Forms.ComboBox _outputPriorityComboBox;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private Utilities.Forms.PathTagsButton  _outputPathPathTags;
        private Utilities.Forms.BrowseButton _outputPathBrowseButton;
        private Utilities.Forms.PathTagsButton _expectedPaginationAttributesPathTagButton;
        private Utilities.Forms.BetterTextBox _expectedPaginationAttributesTextBox;
        private Utilities.Forms.BrowseButton _expectedPaginationAttributesBrowseButton;
        private System.Windows.Forms.CheckBox _expectedPaginationAttributesCheckBox;
        private System.Windows.Forms.CheckBox _requireAllPagesToBeViewedCheckBox;
    }
}