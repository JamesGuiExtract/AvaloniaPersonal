namespace Extract.AttributeFinder.Rules
{
    partial class ExtractOcrTextInImageAreaSettingsDialog
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
            System.Windows.Forms.Label label4;
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.GroupBox groupBox3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractOcrTextInImageAreaSettingsDialog));
            this._separateZonesRadioButton = new System.Windows.Forms.RadioButton();
            this._overallBoundsRadioButton = new System.Windows.Forms.RadioButton();
            this._includeExcludeComboBox = new System.Windows.Forms.ComboBox();
            this._spatialEntityComboBox = new Extract.Utilities.Forms.BetterComboBox();
            this._originalImageInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._originalDocumentOcrRadioButton = new System.Windows.Forms.RadioButton();
            this._documentContextRadioButton = new System.Windows.Forms.RadioButton();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            groupBox3 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(104, 22);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(24, 13);
            label3.TabIndex = 1;
            label3.Text = "any";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(8, 43);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(242, 13);
            label4.TabIndex = 3;
            label4.Text = "that intersect the bounds of the area of extraction.";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._separateZonesRadioButton);
            groupBox1.Controls.Add(this._overallBoundsRadioButton);
            groupBox1.Location = new System.Drawing.Point(16, 85);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(349, 66);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Area of extraction";
            // 
            // _separateZonesRadioButton
            // 
            this._separateZonesRadioButton.AutoSize = true;
            this._separateZonesRadioButton.Location = new System.Drawing.Point(7, 41);
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
            this._overallBoundsRadioButton.Location = new System.Drawing.Point(7, 18);
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
            groupBox2.Controls.Add(this._includeExcludeComboBox);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(this._spatialEntityComboBox);
            groupBox2.Location = new System.Drawing.Point(16, 158);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(349, 67);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Text to extract";
            // 
            // _includeExcludeComboBox
            // 
            this._includeExcludeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._includeExcludeComboBox.FormattingEnabled = true;
            this._includeExcludeComboBox.Items.AddRange(new object[] {
            "Include",
            "Exclude"});
            this._includeExcludeComboBox.Location = new System.Drawing.Point(9, 19);
            this._includeExcludeComboBox.Name = "_includeExcludeComboBox";
            this._includeExcludeComboBox.Size = new System.Drawing.Size(89, 21);
            this._includeExcludeComboBox.TabIndex = 0;
            // 
            // _spatialEntityComboBox
            // 
            this._spatialEntityComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._spatialEntityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._spatialEntityComboBox.FormattingEnabled = true;
            this._spatialEntityComboBox.Location = new System.Drawing.Point(134, 19);
            this._spatialEntityComboBox.Name = "_spatialEntityComboBox";
            this._spatialEntityComboBox.Size = new System.Drawing.Size(89, 21);
            this._spatialEntityComboBox.TabIndex = 2;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(this._originalImageInfoTip);
            groupBox3.Controls.Add(this._originalDocumentOcrRadioButton);
            groupBox3.Controls.Add(this._documentContextRadioButton);
            groupBox3.Location = new System.Drawing.Point(16, 13);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(349, 66);
            groupBox3.TabIndex = 0;
            groupBox3.TabStop = false;
            groupBox3.Text = "OCR source";
            // 
            // _originalImageInfoTip
            // 
            this._originalImageInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._originalImageInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_originalImageInfoTip.BackgroundImage")));
            this._originalImageInfoTip.Location = new System.Drawing.Point(104, 43);
            this._originalImageInfoTip.Name = "_originalImageInfoTip";
            this._originalImageInfoTip.Size = new System.Drawing.Size(16, 16);
            this._originalImageInfoTip.TabIndex = 2;
            this._originalImageInfoTip.TipText = "This option will retrieve text from the uss file.\r\nIf the uss file does not exist" +
    ", an exception will be thrown.";
            // 
            // _originalDocumentOcrRadioButton
            // 
            this._originalDocumentOcrRadioButton.AutoSize = true;
            this._originalDocumentOcrRadioButton.Location = new System.Drawing.Point(7, 42);
            this._originalDocumentOcrRadioButton.Name = "_originalDocumentOcrRadioButton";
            this._originalDocumentOcrRadioButton.Size = new System.Drawing.Size(91, 17);
            this._originalDocumentOcrRadioButton.TabIndex = 0;
            this._originalDocumentOcrRadioButton.TabStop = true;
            this._originalDocumentOcrRadioButton.Text = "Original image";
            this._originalDocumentOcrRadioButton.UseVisualStyleBackColor = true;
            // 
            // _documentContextRadioButton
            // 
            this._documentContextRadioButton.AutoSize = true;
            this._documentContextRadioButton.Location = new System.Drawing.Point(7, 19);
            this._documentContextRadioButton.Name = "_documentContextRadioButton";
            this._documentContextRadioButton.Size = new System.Drawing.Size(232, 17);
            this._documentContextRadioButton.TabIndex = 1;
            this._documentContextRadioButton.TabStop = true;
            this._documentContextRadioButton.Text = "Document context passed to this rule object";
            this._documentContextRadioButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(210, 235);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 3;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(291, 235);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 4;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // ExtractOcrTextInImageAreaSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(378, 270);
            this.Controls.Add(groupBox3);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExtractOcrTextInImageAreaSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Extract OCR text in image area settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.ComboBox _includeExcludeComboBox;
        private Extract.Utilities.Forms.BetterComboBox _spatialEntityComboBox;
        private System.Windows.Forms.RadioButton _separateZonesRadioButton;
        private System.Windows.Forms.RadioButton _overallBoundsRadioButton;
        private System.Windows.Forms.RadioButton _originalDocumentOcrRadioButton;
        private System.Windows.Forms.RadioButton _documentContextRadioButton;
        private Utilities.Forms.InfoTip _originalImageInfoTip;
    }
}