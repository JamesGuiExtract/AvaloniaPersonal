namespace Extract.FileActionManager.FileProcessors
{
    partial class ModifyPdfFileSettingsDialog
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
            System.Windows.Forms.GroupBox groupBox1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifyPdfFileSettingsDialog));
            System.Windows.Forms.Label label1;
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.GroupBox groupBox3;
            System.Windows.Forms.GroupBox groupBox4;
            System.Windows.Forms.GroupBox groupBox5;
            this._pdfFileBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._pdfFileTextBox = new System.Windows.Forms.TextBox();
            this._pdfFilePathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._removeAnnotationsCheckBox = new System.Windows.Forms.CheckBox();
            this._addHyperLinksCheckBox = new System.Windows.Forms.CheckBox();
            this._hyperlinkAddressTextBox = new System.Windows.Forms.TextBox();
            this._useStaticAddressRadioButton = new System.Windows.Forms.RadioButton();
            this._useValueAsAddressRadioButton = new System.Windows.Forms.RadioButton();
            this._hyperlinkAttributesTextBox = new System.Windows.Forms.TextBox();
            this._dataFileTextBox = new System.Windows.Forms.TextBox();
            this._dataFilePathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._dataFileBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._addHighlightsCheckBox = new System.Windows.Forms.CheckBox();
            this._highlightAttributesTextBox = new System.Windows.Forms.TextBox();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            groupBox3 = new System.Windows.Forms.GroupBox();
            groupBox4 = new System.Windows.Forms.GroupBox();
            groupBox5 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._pdfFileBrowseButton);
            groupBox1.Controls.Add(this._pdfFilePathTagsButton);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(this._pdfFileTextBox);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(429, 66);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "General settings";
            // 
            // _pdfFileBrowseButton
            // 
            this._pdfFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pdfFileBrowseButton.FileFilter = "PDF Files (*.pdf)|*.pdf||";
            this._pdfFileBrowseButton.Location = new System.Drawing.Point(393, 32);
            this._pdfFileBrowseButton.Name = "_pdfFileBrowseButton";
            this._pdfFileBrowseButton.Size = new System.Drawing.Size(27, 23);
            this._pdfFileBrowseButton.TabIndex = 3;
            this._pdfFileBrowseButton.Text = "...";
            this._pdfFileBrowseButton.TextControl = this._pdfFileTextBox;
            this._pdfFileBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _pdfFileTextBox
            // 
            this._pdfFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._pdfFileTextBox.Location = new System.Drawing.Point(9, 34);
            this._pdfFileTextBox.Name = "_pdfFileTextBox";
            this._pdfFileTextBox.Size = new System.Drawing.Size(354, 20);
            this._pdfFileTextBox.TabIndex = 1;
            // 
            // _pdfFilePathTagsButton
            // 
            this._pdfFilePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pdfFilePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_pdfFilePathTagsButton.Image")));
            this._pdfFilePathTagsButton.Location = new System.Drawing.Point(369, 32);
            this._pdfFilePathTagsButton.Name = "_pdfFilePathTagsButton";
            this._pdfFilePathTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._pdfFilePathTagsButton.Size = new System.Drawing.Size(18, 23);
            this._pdfFilePathTagsButton.TabIndex = 2;
            this._pdfFilePathTagsButton.TextControl = this._pdfFileTextBox;
            this._pdfFilePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 16);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(140, 13);
            label1.TabIndex = 0;
            label1.Text = "Modify the following PDF file";
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(this._removeAnnotationsCheckBox);
            groupBox2.Location = new System.Drawing.Point(12, 84);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(429, 48);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Annotations";
            // 
            // _removeAnnotationsCheckBox
            // 
            this._removeAnnotationsCheckBox.AutoSize = true;
            this._removeAnnotationsCheckBox.Location = new System.Drawing.Point(9, 19);
            this._removeAnnotationsCheckBox.Name = "_removeAnnotationsCheckBox";
            this._removeAnnotationsCheckBox.Size = new System.Drawing.Size(345, 17);
            this._removeAnnotationsCheckBox.TabIndex = 0;
            this._removeAnnotationsCheckBox.Text = "Remove all pre-existing annotations and hyperlinks from the PDF file";
            this._removeAnnotationsCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox3.Controls.Add(this._addHyperLinksCheckBox);
            groupBox3.Controls.Add(this._hyperlinkAddressTextBox);
            groupBox3.Controls.Add(this._useStaticAddressRadioButton);
            groupBox3.Controls.Add(this._useValueAsAddressRadioButton);
            groupBox3.Controls.Add(this._hyperlinkAttributesTextBox);
            groupBox3.Location = new System.Drawing.Point(12, 139);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(429, 143);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Hyperlinks";
            // 
            // _addHyperLinksCheckBox
            // 
            this._addHyperLinksCheckBox.AutoSize = true;
            this._addHyperLinksCheckBox.Location = new System.Drawing.Point(9, 19);
            this._addHyperLinksCheckBox.Name = "_addHyperLinksCheckBox";
            this._addHyperLinksCheckBox.Size = new System.Drawing.Size(390, 17);
            this._addHyperLinksCheckBox.TabIndex = 0;
            this._addHyperLinksCheckBox.Text = "Add hyperlinks to attributes with any of the following names (comma delimited)";
            this._addHyperLinksCheckBox.UseVisualStyleBackColor = true;
            // 
            // _hyperlinkAddressTextBox
            // 
            this._hyperlinkAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._hyperlinkAddressTextBox.Location = new System.Drawing.Point(28, 115);
            this._hyperlinkAddressTextBox.Name = "_hyperlinkAddressTextBox";
            this._hyperlinkAddressTextBox.Size = new System.Drawing.Size(392, 20);
            this._hyperlinkAddressTextBox.TabIndex = 4;
            // 
            // _useStaticAddressRadioButton
            // 
            this._useStaticAddressRadioButton.AutoSize = true;
            this._useStaticAddressRadioButton.Location = new System.Drawing.Point(9, 91);
            this._useStaticAddressRadioButton.Name = "_useStaticAddressRadioButton";
            this._useStaticAddressRadioButton.Size = new System.Drawing.Size(227, 17);
            this._useStaticAddressRadioButton.TabIndex = 3;
            this._useStaticAddressRadioButton.TabStop = true;
            this._useStaticAddressRadioButton.Text = "Use the following address for the hyperlink:";
            this._useStaticAddressRadioButton.UseVisualStyleBackColor = true;
            // 
            // _useValueAsAddressRadioButton
            // 
            this._useValueAsAddressRadioButton.AutoSize = true;
            this._useValueAsAddressRadioButton.Location = new System.Drawing.Point(9, 68);
            this._useValueAsAddressRadioButton.Name = "_useValueAsAddressRadioButton";
            this._useValueAsAddressRadioButton.Size = new System.Drawing.Size(249, 17);
            this._useValueAsAddressRadioButton.TabIndex = 2;
            this._useValueAsAddressRadioButton.TabStop = true;
            this._useValueAsAddressRadioButton.Text = "Use the attribute value as the hyperlink address";
            this._useValueAsAddressRadioButton.UseVisualStyleBackColor = true;
            // 
            // _hyperlinkAttributesTextBox
            // 
            this._hyperlinkAttributesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._hyperlinkAttributesTextBox.Location = new System.Drawing.Point(9, 42);
            this._hyperlinkAttributesTextBox.Name = "_hyperlinkAttributesTextBox";
            this._hyperlinkAttributesTextBox.Size = new System.Drawing.Size(411, 20);
            this._hyperlinkAttributesTextBox.TabIndex = 1;
            // 
            // groupBox4
            // 
            groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox4.Controls.Add(this._dataFileTextBox);
            groupBox4.Controls.Add(this._dataFilePathTagsButton);
            groupBox4.Controls.Add(this._dataFileBrowseButton);
            groupBox4.Location = new System.Drawing.Point(13, 365);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new System.Drawing.Size(428, 50);
            groupBox4.TabIndex = 4;
            groupBox4.TabStop = false;
            groupBox4.Text = "Data filename";
            // 
            // _dataFileTextBox
            // 
            this._dataFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFileTextBox.Location = new System.Drawing.Point(9, 18);
            this._dataFileTextBox.Name = "_dataFileTextBox";
            this._dataFileTextBox.Size = new System.Drawing.Size(354, 20);
            this._dataFileTextBox.TabIndex = 0;
            // 
            // _dataFilePathTagsButton
            // 
            this._dataFilePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFilePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_dataFilePathTagsButton.Image")));
            this._dataFilePathTagsButton.Location = new System.Drawing.Point(369, 16);
            this._dataFilePathTagsButton.Name = "_dataFilePathTagsButton";
            this._dataFilePathTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._dataFilePathTagsButton.Size = new System.Drawing.Size(18, 23);
            this._dataFilePathTagsButton.TabIndex = 1;
            this._dataFilePathTagsButton.TextControl = this._dataFileTextBox;
            this._dataFilePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _dataFileBrowseButton
            // 
            this._dataFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFileBrowseButton.Location = new System.Drawing.Point(393, 16);
            this._dataFileBrowseButton.Name = "_dataFileBrowseButton";
            this._dataFileBrowseButton.Size = new System.Drawing.Size(27, 23);
            this._dataFileBrowseButton.TabIndex = 2;
            this._dataFileBrowseButton.Text = "...";
            this._dataFileBrowseButton.TextControl = this._dataFileTextBox;
            this._dataFileBrowseButton.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox5.Controls.Add(this._addHighlightsCheckBox);
            groupBox5.Controls.Add(this._highlightAttributesTextBox);
            groupBox5.Location = new System.Drawing.Point(12, 288);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new System.Drawing.Size(429, 71);
            groupBox5.TabIndex = 3;
            groupBox5.TabStop = false;
            groupBox5.Text = "Highlights";
            // 
            // _addHighlightsCheckBox
            // 
            this._addHighlightsCheckBox.AutoSize = true;
            this._addHighlightsCheckBox.Location = new System.Drawing.Point(9, 19);
            this._addHighlightsCheckBox.Name = "_addHighlightsCheckBox";
            this._addHighlightsCheckBox.Size = new System.Drawing.Size(387, 17);
            this._addHighlightsCheckBox.TabIndex = 0;
            this._addHighlightsCheckBox.Text = "Add highlights to attributes with any of the following names (comma delimited)";
            this._addHighlightsCheckBox.UseVisualStyleBackColor = true;
            // 
            // _highlightAttributesTextBox
            // 
            this._highlightAttributesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._highlightAttributesTextBox.Location = new System.Drawing.Point(9, 42);
            this._highlightAttributesTextBox.Name = "_highlightAttributesTextBox";
            this._highlightAttributesTextBox.Size = new System.Drawing.Size(411, 20);
            this._highlightAttributesTextBox.TabIndex = 1;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(285, 424);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 5;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClicked);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(366, 424);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 6;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(9, 19);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(387, 17);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "Add highlights to attributes with any of the following names (comma delimited)";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // ModifyPdfFileSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(455, 458);
            this.Controls.Add(groupBox5);
            this.Controls.Add(groupBox4);
            this.Controls.Add(groupBox3);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModifyPdfFileSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Core: Modify PDF file settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox _pdfFileTextBox;
        private Extract.Utilities.Forms.BrowseButton _pdfFileBrowseButton;
        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _pdfFilePathTagsButton;
        private System.Windows.Forms.CheckBox _removeAnnotationsCheckBox;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.TextBox _hyperlinkAttributesTextBox;
        private Extract.Utilities.Forms.BrowseButton _dataFileBrowseButton;
        private System.Windows.Forms.TextBox _dataFileTextBox;
        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _dataFilePathTagsButton;
        private System.Windows.Forms.RadioButton _useValueAsAddressRadioButton;
        private System.Windows.Forms.TextBox _hyperlinkAddressTextBox;
        private System.Windows.Forms.RadioButton _useStaticAddressRadioButton;
        private System.Windows.Forms.CheckBox _addHyperLinksCheckBox;
        private System.Windows.Forms.CheckBox _addHighlightsCheckBox;
        private System.Windows.Forms.TextBox _highlightAttributesTextBox;
        private System.Windows.Forms.CheckBox checkBox1;

    }
}