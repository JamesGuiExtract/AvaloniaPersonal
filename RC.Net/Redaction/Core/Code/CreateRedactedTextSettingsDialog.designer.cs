﻿namespace Extract.Redaction
{
    partial class CreateRedactedTextSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateRedactedTextSettingsDialog));
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._dataToRedactGroupBox = new System.Windows.Forms.GroupBox();
            this._manualDataCheckBox = new System.Windows.Forms.CheckBox();
            this._mediumConfidenceDataCheckBox = new System.Windows.Forms.CheckBox();
            this._otherDataCheckBox = new System.Windows.Forms.CheckBox();
            this._lowConfidenceDataCheckBox = new System.Windows.Forms.CheckBox();
            this._highConfidenceDataCheckBox = new System.Windows.Forms.CheckBox();
            this._dataTypesTextBox = new System.Windows.Forms.TextBox();
            this._redactSpecificTypesRadioButton = new System.Windows.Forms.RadioButton();
            this._redactAllTypesRadioButton = new System.Windows.Forms.RadioButton();
            this._redactionMethodGroupBox = new System.Windows.Forms.GroupBox();
            this._replacementValueTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._charsToReplaceComboBox = new Extract.Utilities.Forms.BetterComboBox();
            this._surroundTextRadioButton = new System.Windows.Forms.RadioButton();
            this._xmlElementTextBox = new System.Windows.Forms.TextBox();
            this._replaceCharactersRadioButton = new System.Windows.Forms.RadioButton();
            this._outputGroupBox = new System.Windows.Forms.GroupBox();
            this._outputPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._outputLocationTextBox = new System.Windows.Forms.TextBox();
            this._dataFileControl = new Extract.Redaction.DataFileControl();
            this._dataToRedactGroupBox.SuspendLayout();
            this._redactionMethodGroupBox.SuspendLayout();
            this._outputGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(254, 406);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 9;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(335, 406);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 10;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _dataToRedactGroupBox
            // 
            this._dataToRedactGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._dataToRedactGroupBox.Controls.Add(this._manualDataCheckBox);
            this._dataToRedactGroupBox.Controls.Add(this._mediumConfidenceDataCheckBox);
            this._dataToRedactGroupBox.Controls.Add(this._otherDataCheckBox);
            this._dataToRedactGroupBox.Controls.Add(this._lowConfidenceDataCheckBox);
            this._dataToRedactGroupBox.Controls.Add(this._highConfidenceDataCheckBox);
            this._dataToRedactGroupBox.Controls.Add(this._dataTypesTextBox);
            this._dataToRedactGroupBox.Controls.Add(this._redactSpecificTypesRadioButton);
            this._dataToRedactGroupBox.Controls.Add(this._redactAllTypesRadioButton);
            this._dataToRedactGroupBox.Location = new System.Drawing.Point(12, 13);
            this._dataToRedactGroupBox.Name = "_dataToRedactGroupBox";
            this._dataToRedactGroupBox.Size = new System.Drawing.Size(398, 163);
            this._dataToRedactGroupBox.TabIndex = 14;
            this._dataToRedactGroupBox.TabStop = false;
            this._dataToRedactGroupBox.Text = "Data categories to redact";
            // 
            // _manualDataCheckBox
            // 
            this._manualDataCheckBox.AutoSize = true;
            this._manualDataCheckBox.Location = new System.Drawing.Point(189, 90);
            this._manualDataCheckBox.Name = "_manualDataCheckBox";
            this._manualDataCheckBox.Size = new System.Drawing.Size(113, 17);
            this._manualDataCheckBox.TabIndex = 21;
            this._manualDataCheckBox.Text = "Manual redactions";
            this._manualDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // _mediumConfidenceDataCheckBox
            // 
            this._mediumConfidenceDataCheckBox.AutoSize = true;
            this._mediumConfidenceDataCheckBox.Location = new System.Drawing.Point(189, 67);
            this._mediumConfidenceDataCheckBox.Name = "_mediumConfidenceDataCheckBox";
            this._mediumConfidenceDataCheckBox.Size = new System.Drawing.Size(143, 17);
            this._mediumConfidenceDataCheckBox.TabIndex = 20;
            this._mediumConfidenceDataCheckBox.Text = "Medium confidence data";
            this._mediumConfidenceDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // _otherDataCheckBox
            // 
            this._otherDataCheckBox.AutoSize = true;
            this._otherDataCheckBox.Location = new System.Drawing.Point(26, 113);
            this._otherDataCheckBox.Name = "_otherDataCheckBox";
            this._otherDataCheckBox.Size = new System.Drawing.Size(200, 17);
            this._otherDataCheckBox.TabIndex = 19;
            this._otherDataCheckBox.Text = "Other (separate names with commas)";
            this._otherDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // _lowConfidenceDataCheckBox
            // 
            this._lowConfidenceDataCheckBox.AutoSize = true;
            this._lowConfidenceDataCheckBox.Location = new System.Drawing.Point(26, 90);
            this._lowConfidenceDataCheckBox.Name = "_lowConfidenceDataCheckBox";
            this._lowConfidenceDataCheckBox.Size = new System.Drawing.Size(126, 17);
            this._lowConfidenceDataCheckBox.TabIndex = 18;
            this._lowConfidenceDataCheckBox.Text = "Low confidence data";
            this._lowConfidenceDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // _highConfidenceDataCheckBox
            // 
            this._highConfidenceDataCheckBox.AutoSize = true;
            this._highConfidenceDataCheckBox.Location = new System.Drawing.Point(26, 67);
            this._highConfidenceDataCheckBox.Name = "_highConfidenceDataCheckBox";
            this._highConfidenceDataCheckBox.Size = new System.Drawing.Size(128, 17);
            this._highConfidenceDataCheckBox.TabIndex = 17;
            this._highConfidenceDataCheckBox.Text = "High confidence data";
            this._highConfidenceDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // _dataTypesTextBox
            // 
            this._dataTypesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._dataTypesTextBox.Enabled = false;
            this._dataTypesTextBox.Location = new System.Drawing.Point(26, 136);
            this._dataTypesTextBox.Name = "_dataTypesTextBox";
            this._dataTypesTextBox.Size = new System.Drawing.Size(362, 20);
            this._dataTypesTextBox.TabIndex = 16;
            // 
            // _redactSpecificTypesRadioButton
            // 
            this._redactSpecificTypesRadioButton.AutoSize = true;
            this._redactSpecificTypesRadioButton.Location = new System.Drawing.Point(6, 43);
            this._redactSpecificTypesRadioButton.Name = "_redactSpecificTypesRadioButton";
            this._redactSpecificTypesRadioButton.Size = new System.Drawing.Size(67, 17);
            this._redactSpecificTypesRadioButton.TabIndex = 15;
            this._redactSpecificTypesRadioButton.TabStop = true;
            this._redactSpecificTypesRadioButton.Text = "Selected";
            this._redactSpecificTypesRadioButton.UseVisualStyleBackColor = true;
            // 
            // _redactAllTypesRadioButton
            // 
            this._redactAllTypesRadioButton.AutoSize = true;
            this._redactAllTypesRadioButton.Location = new System.Drawing.Point(6, 19);
            this._redactAllTypesRadioButton.Name = "_redactAllTypesRadioButton";
            this._redactAllTypesRadioButton.Size = new System.Drawing.Size(36, 17);
            this._redactAllTypesRadioButton.TabIndex = 14;
            this._redactAllTypesRadioButton.TabStop = true;
            this._redactAllTypesRadioButton.Text = "All";
            this._redactAllTypesRadioButton.UseVisualStyleBackColor = true;
            // 
            // _redactionMethodGroupBox
            // 
            this._redactionMethodGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._redactionMethodGroupBox.Controls.Add(this._replacementValueTextBox);
            this._redactionMethodGroupBox.Controls.Add(this.label1);
            this._redactionMethodGroupBox.Controls.Add(this._charsToReplaceComboBox);
            this._redactionMethodGroupBox.Controls.Add(this._surroundTextRadioButton);
            this._redactionMethodGroupBox.Controls.Add(this._xmlElementTextBox);
            this._redactionMethodGroupBox.Controls.Add(this._replaceCharactersRadioButton);
            this._redactionMethodGroupBox.Location = new System.Drawing.Point(12, 182);
            this._redactionMethodGroupBox.Name = "_redactionMethodGroupBox";
            this._redactionMethodGroupBox.Size = new System.Drawing.Size(398, 94);
            this._redactionMethodGroupBox.TabIndex = 15;
            this._redactionMethodGroupBox.TabStop = false;
            this._redactionMethodGroupBox.Text = "Redaction method";
            // 
            // _replacementValueTextBox
            // 
            this._replacementValueTextBox.Location = new System.Drawing.Point(366, 18);
            this._replacementValueTextBox.MaxLength = 1;
            this._replacementValueTextBox.Name = "_replacementValueTextBox";
            this._replacementValueTextBox.Size = new System.Drawing.Size(22, 20);
            this._replacementValueTextBox.TabIndex = 19;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(176, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(175, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "characters in the sensitive text with:";
            // 
            // _charsToReplaceComboBox
            // 
            this._charsToReplaceComboBox.FormattingEnabled = true;
            this._charsToReplaceComboBox.Items.AddRange(new object[] {
            "all",
            "alpha numeric"});
            this._charsToReplaceComboBox.Location = new System.Drawing.Point(74, 18);
            this._charsToReplaceComboBox.Name = "_charsToReplaceComboBox";
            this._charsToReplaceComboBox.Size = new System.Drawing.Size(96, 21);
            this._charsToReplaceComboBox.TabIndex = 17;
            // 
            // _surroundTextRadioButton
            // 
            this._surroundTextRadioButton.AutoSize = true;
            this._surroundTextRadioButton.Location = new System.Drawing.Point(6, 42);
            this._surroundTextRadioButton.Name = "_surroundTextRadioButton";
            this._surroundTextRadioButton.Size = new System.Drawing.Size(272, 17);
            this._surroundTextRadioButton.TabIndex = 15;
            this._surroundTextRadioButton.TabStop = true;
            this._surroundTextRadioButton.Text = "Surround sensitive text with an XML element named:";
            this._surroundTextRadioButton.UseVisualStyleBackColor = true;
            // 
            // _xmlElementTextBox
            // 
            this._xmlElementTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._xmlElementTextBox.Location = new System.Drawing.Point(25, 65);
            this._xmlElementTextBox.Name = "_xmlElementTextBox";
            this._xmlElementTextBox.Size = new System.Drawing.Size(363, 20);
            this._xmlElementTextBox.TabIndex = 16;
            // 
            // _replaceCharactersRadioButton
            // 
            this._replaceCharactersRadioButton.AutoSize = true;
            this._replaceCharactersRadioButton.Location = new System.Drawing.Point(6, 19);
            this._replaceCharactersRadioButton.Name = "_replaceCharactersRadioButton";
            this._replaceCharactersRadioButton.Size = new System.Drawing.Size(65, 17);
            this._replaceCharactersRadioButton.TabIndex = 14;
            this._replaceCharactersRadioButton.TabStop = true;
            this._replaceCharactersRadioButton.Text = "Replace";
            this._replaceCharactersRadioButton.UseVisualStyleBackColor = true;
            // 
            // _outputGroupBox
            // 
            this._outputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._outputGroupBox.Controls.Add(this._outputPathTagsButton);
            this._outputGroupBox.Controls.Add(this._outputLocationTextBox);
            this._outputGroupBox.Location = new System.Drawing.Point(12, 349);
            this._outputGroupBox.Name = "_outputGroupBox";
            this._outputGroupBox.Size = new System.Drawing.Size(398, 49);
            this._outputGroupBox.TabIndex = 17;
            this._outputGroupBox.TabStop = false;
            this._outputGroupBox.Text = "Output location";
            // 
            // _outputPathTagsButton
            // 
            this._outputPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_outputPathTagsButton.Image")));
            this._outputPathTagsButton.Location = new System.Drawing.Point(374, 19);
            this._outputPathTagsButton.Name = "_outputPathTagsButton";
            this._outputPathTagsButton.PathTags = new Extract.Utilities.FileActionManagerPathTags();
            this._outputPathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._outputPathTagsButton.TabIndex = 4;
            this._outputPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _outputLocationTextBox
            // 
            this._outputLocationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._outputLocationTextBox.Location = new System.Drawing.Point(6, 20);
            this._outputLocationTextBox.Name = "_outputLocationTextBox";
            this._outputLocationTextBox.Size = new System.Drawing.Size(362, 20);
            this._outputLocationTextBox.TabIndex = 3;
            // 
            // _dataFileControl
            // 
            this._dataFileControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFileControl.Location = new System.Drawing.Point(12, 282);
            this._dataFileControl.Name = "_dataFileControl";
            this._dataFileControl.Size = new System.Drawing.Size(398, 60);
            this._dataFileControl.TabIndex = 8;
            // 
            // CreateRedactedTextSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(422, 441);
            this.Controls.Add(this._outputGroupBox);
            this.Controls.Add(this._redactionMethodGroupBox);
            this.Controls.Add(this._dataToRedactGroupBox);
            this.Controls.Add(this._dataFileControl);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateRedactedTextSettingsDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Redaction: Create redacted text";
            this._dataToRedactGroupBox.ResumeLayout(false);
            this._dataToRedactGroupBox.PerformLayout();
            this._redactionMethodGroupBox.ResumeLayout(false);
            this._redactionMethodGroupBox.PerformLayout();
            this._outputGroupBox.ResumeLayout(false);
            this._outputGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DataFileControl _dataFileControl;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.GroupBox _dataToRedactGroupBox;
        private System.Windows.Forms.TextBox _dataTypesTextBox;
        private System.Windows.Forms.RadioButton _redactSpecificTypesRadioButton;
        private System.Windows.Forms.RadioButton _redactAllTypesRadioButton;
        private System.Windows.Forms.GroupBox _redactionMethodGroupBox;
        private System.Windows.Forms.RadioButton _surroundTextRadioButton;
        private System.Windows.Forms.TextBox _xmlElementTextBox;
        private System.Windows.Forms.RadioButton _replaceCharactersRadioButton;
        private System.Windows.Forms.GroupBox _outputGroupBox;
        private Utilities.Forms.PathTagsButton _outputPathTagsButton;
        private System.Windows.Forms.TextBox _outputLocationTextBox;
        private System.Windows.Forms.CheckBox _lowConfidenceDataCheckBox;
        private System.Windows.Forms.CheckBox _highConfidenceDataCheckBox;
        private System.Windows.Forms.CheckBox _otherDataCheckBox;
        private System.Windows.Forms.CheckBox _manualDataCheckBox;
        private System.Windows.Forms.CheckBox _mediumConfidenceDataCheckBox;
        private Extract.Utilities.Forms.BetterComboBox _charsToReplaceComboBox;
        private System.Windows.Forms.TextBox _replacementValueTextBox;
        private System.Windows.Forms.Label label1;

    }
}