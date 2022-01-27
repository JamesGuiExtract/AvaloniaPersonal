namespace Extract.FileActionManager.FileProcessors
{
    partial class ExtractImageAreaTaskSettingsDialog
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
            System.Windows.Forms.Label label4;
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.Label label2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractImageAreaTaskSettingsDialog));
            this._separateZonesRadioButton = new System.Windows.Forms.RadioButton();
            this._overallBoundsRadioButton = new System.Windows.Forms.RadioButton();
            this._allAreasRadioButton = new System.Windows.Forms.RadioButton();
            this._firstAreaRadioButton = new System.Windows.Forms.RadioButton();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._dataFileTextBox = new System.Windows.Forms.TextBox();
            this._dataFileBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._attributeQueryInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._attributeQueryTextBox = new System.Windows.Forms.TextBox();
            this._outputFileTextBox = new System.Windows.Forms.TextBox();
            this._outputFileBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._outputFileInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._outputFilePathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._dataFilePathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._allowOutputAppendCheckBox = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label2 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(9, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(279, 13);
            label1.TabIndex = 0;
            label1.Text = "Extract image area based on the attributes in this data file:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(10, 61);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(247, 13);
            label4.TabIndex = 4;
            label4.Text = "Extract the image areas of the following attribute(s):";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._separateZonesRadioButton);
            groupBox1.Controls.Add(this._overallBoundsRadioButton);
            groupBox1.Location = new System.Drawing.Point(12, 142);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(448, 66);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Extraction method";
            // 
            // _separateZonesRadioButton
            // 
            this._separateZonesRadioButton.AutoSize = true;
            this._separateZonesRadioButton.Location = new System.Drawing.Point(7, 43);
            this._separateZonesRadioButton.Name = "_separateZonesRadioButton";
            this._separateZonesRadioButton.Size = new System.Drawing.Size(177, 17);
            this._separateZonesRadioButton.TabIndex = 1;
            this._separateZonesRadioButton.TabStop = true;
            this._separateZonesRadioButton.Text = "Use each raster zone separately";
            this._separateZonesRadioButton.UseVisualStyleBackColor = true;
            // 
            // _overallBoundsRadioButton
            // 
            this._overallBoundsRadioButton.AutoSize = true;
            this._overallBoundsRadioButton.Location = new System.Drawing.Point(7, 20);
            this._overallBoundsRadioButton.Name = "_overallBoundsRadioButton";
            this._overallBoundsRadioButton.Size = new System.Drawing.Size(205, 17);
            this._overallBoundsRadioButton.TabIndex = 0;
            this._overallBoundsRadioButton.TabStop = true;
            this._overallBoundsRadioButton.Text = "Use the overall bounds of the attribute";
            this._overallBoundsRadioButton.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(this._allAreasRadioButton);
            groupBox2.Controls.Add(this._firstAreaRadioButton);
            groupBox2.Location = new System.Drawing.Point(13, 214);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(446, 66);
            groupBox2.TabIndex = 8;
            groupBox2.TabStop = false;
            groupBox2.Text = "Multiple area handling";
            // 
            // _allAreasRadioButton
            // 
            this._allAreasRadioButton.AutoSize = true;
            this._allAreasRadioButton.Location = new System.Drawing.Point(7, 43);
            this._allAreasRadioButton.Name = "_allAreasRadioButton";
            this._allAreasRadioButton.Size = new System.Drawing.Size(147, 17);
            this._allAreasRadioButton.TabIndex = 1;
            this._allAreasRadioButton.TabStop = true;
            this._allAreasRadioButton.Text = "Extract all qualifying areas";
            this._allAreasRadioButton.UseVisualStyleBackColor = true;
            // 
            // _firstAreaRadioButton
            // 
            this._firstAreaRadioButton.AutoSize = true;
            this._firstAreaRadioButton.Location = new System.Drawing.Point(7, 20);
            this._firstAreaRadioButton.Name = "_firstAreaRadioButton";
            this._firstAreaRadioButton.Size = new System.Drawing.Size(188, 17);
            this._firstAreaRadioButton.TabIndex = 0;
            this._firstAreaRadioButton.TabStop = true;
            this._firstAreaRadioButton.Text = "Extract only the first qualifying area";
            this._firstAreaRadioButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(9, 289);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(185, 13);
            label2.TabIndex = 9;
            label2.Text = "Output the extracted image area(s) to:";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(303, 358);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 14;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(384, 358);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 15;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _dataFileTextBox
            // 
            this._dataFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFileTextBox.Location = new System.Drawing.Point(11, 28);
            this._dataFileTextBox.Name = "_dataFileTextBox";
            this._dataFileTextBox.Size = new System.Drawing.Size(391, 20);
            this._dataFileTextBox.TabIndex = 1;
            // 
            // _dataFileBrowseButton
            // 
            this._dataFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFileBrowseButton.Location = new System.Drawing.Point(432, 27);
            this._dataFileBrowseButton.Name = "_dataFileBrowseButton";
            this._dataFileBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._dataFileBrowseButton.TabIndex = 3;
            this._dataFileBrowseButton.Text = "...";
            this._dataFileBrowseButton.TextControl = this._dataFileTextBox;
            this._dataFileBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _attributeQueryInfoTip
            // 
            this._attributeQueryInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._attributeQueryInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_attributeQueryInfoTip.BackgroundImage")));
            this._attributeQueryInfoTip.Location = new System.Drawing.Point(271, 60);
            this._attributeQueryInfoTip.Name = "_attributeQueryInfoTip";
            this._attributeQueryInfoTip.Size = new System.Drawing.Size(16, 16);
            this._attributeQueryInfoTip.TabIndex = 5;
            this._attributeQueryInfoTip.TabStop = false;
            this._attributeQueryInfoTip.TipText = resources.GetString("_attributeQueryInfoTip.TipText");
            // 
            // _attributeQueryTextBox
            // 
            this._attributeQueryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeQueryTextBox.Location = new System.Drawing.Point(11, 82);
            this._attributeQueryTextBox.Multiline = true;
            this._attributeQueryTextBox.Name = "_attributeQueryTextBox";
            this._attributeQueryTextBox.Size = new System.Drawing.Size(448, 53);
            this._attributeQueryTextBox.TabIndex = 6;
            // 
            // _outputFileTextBox
            // 
            this._outputFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputFileTextBox.Location = new System.Drawing.Point(11, 308);
            this._outputFileTextBox.Name = "_outputFileTextBox";
            this._outputFileTextBox.Size = new System.Drawing.Size(391, 20);
            this._outputFileTextBox.TabIndex = 11;
            this._outputFileTextBox.TextChanged += new System.EventHandler(this.HandleOutputFileTextChanged);
            // 
            // _outputFileBrowseButton
            // 
            this._outputFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputFileBrowseButton.Location = new System.Drawing.Point(432, 307);
            this._outputFileBrowseButton.Name = "_outputFileBrowseButton";
            this._outputFileBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._outputFileBrowseButton.TabIndex = 13;
            this._outputFileBrowseButton.Text = "...";
            this._outputFileBrowseButton.TextControl = this._outputFileTextBox;
            this._outputFileBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _outputFileInfoTip
            // 
            this._outputFileInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._outputFileInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_outputFileInfoTip.BackgroundImage")));
            this._outputFileInfoTip.Location = new System.Drawing.Point(205, 287);
            this._outputFileInfoTip.Name = "_outputFileInfoTip";
            this._outputFileInfoTip.Size = new System.Drawing.Size(16, 16);
            this._outputFileInfoTip.TabIndex = 10;
            this._outputFileInfoTip.TabStop = false;
            this._outputFileInfoTip.TipText = resources.GetString("_outputFileInfoTip.TipText");
            // 
            // _outputFilePathTagsButton
            // 
            this._outputFilePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputFilePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_outputFilePathTagsButton.Image")));
            this._outputFilePathTagsButton.Location = new System.Drawing.Point(408, 307);
            this._outputFilePathTagsButton.Name = "_outputFilePathTagsButton";
            this._outputFilePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._outputFilePathTagsButton.TabIndex = 12;
            this._outputFilePathTagsButton.TextControl = this._outputFileTextBox;
            this._outputFilePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _dataFilePathTagsButton
            // 
            this._dataFilePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFilePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_dataFilePathTagsButton.Image")));
            this._dataFilePathTagsButton.Location = new System.Drawing.Point(408, 27);
            this._dataFilePathTagsButton.Name = "_dataFilePathTagsButton";
            this._dataFilePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._dataFilePathTagsButton.TabIndex = 2;
            this._dataFilePathTagsButton.TextControl = this._dataFileTextBox;
            this._dataFilePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _allowOutputAppendCheckBox
            // 
            this._allowOutputAppendCheckBox.AutoSize = true;
            this._allowOutputAppendCheckBox.Location = new System.Drawing.Point(13, 335);
            this._allowOutputAppendCheckBox.Name = "_allowOutputAppendCheckBox";
            this._allowOutputAppendCheckBox.Size = new System.Drawing.Size(360, 17);
            this._allowOutputAppendCheckBox.TabIndex = 16;
            this._allowOutputAppendCheckBox.Text = "If the above image already exists, append area(s) as additional page(s) ";
            this._allowOutputAppendCheckBox.UseVisualStyleBackColor = true;
            // 
            // ExtractImageAreaTaskSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(472, 393);
            this.Controls.Add(this._allowOutputAppendCheckBox);
            this.Controls.Add(this._outputFileInfoTip);
            this.Controls.Add(label2);
            this.Controls.Add(this._outputFilePathTagsButton);
            this.Controls.Add(this._outputFileBrowseButton);
            this.Controls.Add(this._outputFileTextBox);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._attributeQueryInfoTip);
            this.Controls.Add(this._attributeQueryTextBox);
            this.Controls.Add(label4);
            this.Controls.Add(label1);
            this.Controls.Add(this._dataFilePathTagsButton);
            this.Controls.Add(this._dataFileBrowseButton);
            this.Controls.Add(this._dataFileTextBox);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExtractImageAreaTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Core: Extract image area settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _dataFilePathTagsButton;
        private System.Windows.Forms.TextBox _dataFileTextBox;
        private Extract.Utilities.Forms.BrowseButton _dataFileBrowseButton;
        private Extract.Utilities.Forms.InfoTip _attributeQueryInfoTip;
        private System.Windows.Forms.TextBox _attributeQueryTextBox;
        private System.Windows.Forms.RadioButton _overallBoundsRadioButton;
        private System.Windows.Forms.RadioButton _separateZonesRadioButton;
        private System.Windows.Forms.RadioButton _allAreasRadioButton;
        private System.Windows.Forms.RadioButton _firstAreaRadioButton;
        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _outputFilePathTagsButton;
        private System.Windows.Forms.TextBox _outputFileTextBox;
        private Extract.Utilities.Forms.BrowseButton _outputFileBrowseButton;
        private Extract.Utilities.Forms.InfoTip _outputFileInfoTip;
        private System.Windows.Forms.CheckBox _allowOutputAppendCheckBox;
    }
}