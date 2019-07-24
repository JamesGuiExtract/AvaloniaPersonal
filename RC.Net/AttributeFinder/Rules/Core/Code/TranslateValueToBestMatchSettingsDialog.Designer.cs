namespace Extract.AttributeFinder.Rules
{
    partial class TranslateValueToBestMatchSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TranslateValueToBestMatchSettingsDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._attributeSelectorControl = new Utilities.Forms.ConfigurableObjectControl();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._sourceListPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._sourceListPathTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._sourceListPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._synonymMapPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._synonymMapPathTextBox = new System.Windows.Forms.TextBox();
            this._synonymMapPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._minimumMatchScoreNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._setTypeToUntranslatedRadioButton = new System.Windows.Forms.RadioButton();
            this._removeAttributeRadioButton = new System.Windows.Forms.RadioButton();
            this._clearValueRadioButton = new System.Windows.Forms.RadioButton();
            this._doNothingRadioButton = new System.Windows.Forms.RadioButton();
            this._createScoreSubattributeCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._minimumMatchScoreNumericUpDown)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select the attributes to be translated:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._attributeSelectorControl);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(460, 89);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // _attributeSelectorControl
            // 
            this._attributeSelectorControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeSelectorControl.CategoryName = "UCLID AF-API Selectors";
            this._attributeSelectorControl.ConfigurableObject = null;
            this._attributeSelectorControl.Location = new System.Drawing.Point(5, 32);
            this._attributeSelectorControl.Name = "_attributeSelectorControl";
            this._attributeSelectorControl.Size = new System.Drawing.Size(449, 48);
            this._attributeSelectorControl.TabIndex = 1;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(397, 392);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 15;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(316, 392);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 14;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _sourceListPathBrowseButton
            // 
            this._sourceListPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceListPathBrowseButton.EnsureFileExists = true;
            this._sourceListPathBrowseButton.EnsurePathExists = true;
            this._sourceListPathBrowseButton.Location = new System.Drawing.Point(447, 119);
            this._sourceListPathBrowseButton.Name = "_sourceListPathBrowseButton";
            this._sourceListPathBrowseButton.Size = new System.Drawing.Size(24, 22);
            this._sourceListPathBrowseButton.TabIndex = 5;
            this._sourceListPathBrowseButton.Text = "...";
            this._sourceListPathBrowseButton.TextControl = this._sourceListPathTextBox;
            this._sourceListPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _sourceListPathTextBox
            // 
            this._sourceListPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceListPathTextBox.Location = new System.Drawing.Point(25, 120);
            this._sourceListPathTextBox.Name = "_sourceListPathTextBox";
            this._sourceListPathTextBox.Size = new System.Drawing.Size(386, 20);
            this._sourceListPathTextBox.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "List of translation targets:";
            // 
            // _sourceListPathTagsButton
            // 
            this._sourceListPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceListPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_sourceListPathTagsButton.Image")));
            this._sourceListPathTagsButton.Location = new System.Drawing.Point(417, 119);
            this._sourceListPathTagsButton.Name = "_sourceListPathTagsButton";
            this._sourceListPathTagsButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._sourceListPathTagsButton.Size = new System.Drawing.Size(24, 22);
            this._sourceListPathTagsButton.TabIndex = 4;
            this._sourceListPathTagsButton.TextControl = this._sourceListPathTextBox;
            this._sourceListPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _synonymMapPathBrowseButton
            // 
            this._synonymMapPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._synonymMapPathBrowseButton.EnsureFileExists = true;
            this._synonymMapPathBrowseButton.EnsurePathExists = true;
            this._synonymMapPathBrowseButton.Location = new System.Drawing.Point(447, 162);
            this._synonymMapPathBrowseButton.Name = "_synonymMapPathBrowseButton";
            this._synonymMapPathBrowseButton.Size = new System.Drawing.Size(24, 22);
            this._synonymMapPathBrowseButton.TabIndex = 9;
            this._synonymMapPathBrowseButton.Text = "...";
            this._synonymMapPathBrowseButton.TextControl = this._synonymMapPathTextBox;
            this._synonymMapPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _synonymMapPathTextBox
            // 
            this._synonymMapPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._synonymMapPathTextBox.Location = new System.Drawing.Point(25, 163);
            this._synonymMapPathTextBox.Name = "_synonymMapPathTextBox";
            this._synonymMapPathTextBox.Size = new System.Drawing.Size(386, 20);
            this._synonymMapPathTextBox.TabIndex = 7;
            // 
            // _synonymMapPathTagsButton
            // 
            this._synonymMapPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._synonymMapPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_synonymMapPathTagsButton.Image")));
            this._synonymMapPathTagsButton.Location = new System.Drawing.Point(417, 162);
            this._synonymMapPathTagsButton.Name = "_synonymMapPathTagsButton";
            this._synonymMapPathTagsButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._synonymMapPathTagsButton.Size = new System.Drawing.Size(24, 22);
            this._synonymMapPathTagsButton.TabIndex = 8;
            this._synonymMapPathTagsButton.TextControl = this._synonymMapPathTextBox;
            this._synonymMapPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 147);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Synonym map (CSV):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 191);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(112, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Minimum match score:";
            // 
            // _minimumMatchScoreNumericUpDown
            // 
            this._minimumMatchScoreNumericUpDown.DecimalPlaces = 2;
            this._minimumMatchScoreNumericUpDown.Location = new System.Drawing.Point(25, 207);
            this._minimumMatchScoreNumericUpDown.Name = "_minimumMatchScoreNumericUpDown";
            this._minimumMatchScoreNumericUpDown.Size = new System.Drawing.Size(103, 20);
            this._minimumMatchScoreNumericUpDown.TabIndex = 11;
            this._minimumMatchScoreNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this._setTypeToUntranslatedRadioButton);
            this.groupBox2.Controls.Add(this._removeAttributeRadioButton);
            this.groupBox2.Controls.Add(this._clearValueRadioButton);
            this.groupBox2.Controls.Add(this._doNothingRadioButton);
            this.groupBox2.Location = new System.Drawing.Point(25, 233);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(447, 112);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "If minimum score is not met:";
            // 
            // _setTypeToUntranslatedRadioButton
            // 
            this._setTypeToUntranslatedRadioButton.AutoSize = true;
            this._setTypeToUntranslatedRadioButton.Location = new System.Drawing.Point(6, 88);
            this._setTypeToUntranslatedRadioButton.Name = "_setTypeToUntranslatedRadioButton";
            this._setTypeToUntranslatedRadioButton.Size = new System.Drawing.Size(137, 17);
            this._setTypeToUntranslatedRadioButton.TabIndex = 3;
            this._setTypeToUntranslatedRadioButton.TabStop = true;
            this._setTypeToUntranslatedRadioButton.Text = "Set type to untranslated";
            this._setTypeToUntranslatedRadioButton.UseVisualStyleBackColor = true;
            // 
            // _removeAttributeRadioButton
            // 
            this._removeAttributeRadioButton.AutoSize = true;
            this._removeAttributeRadioButton.Location = new System.Drawing.Point(6, 65);
            this._removeAttributeRadioButton.Name = "_removeAttributeRadioButton";
            this._removeAttributeRadioButton.Size = new System.Drawing.Size(106, 17);
            this._removeAttributeRadioButton.TabIndex = 2;
            this._removeAttributeRadioButton.TabStop = true;
            this._removeAttributeRadioButton.Text = "Remove attribute";
            this._removeAttributeRadioButton.UseVisualStyleBackColor = true;
            // 
            // _clearValueRadioButton
            // 
            this._clearValueRadioButton.AutoSize = true;
            this._clearValueRadioButton.Location = new System.Drawing.Point(6, 42);
            this._clearValueRadioButton.Name = "_clearValueRadioButton";
            this._clearValueRadioButton.Size = new System.Drawing.Size(78, 17);
            this._clearValueRadioButton.TabIndex = 1;
            this._clearValueRadioButton.TabStop = true;
            this._clearValueRadioButton.Text = "Clear value";
            this._clearValueRadioButton.UseVisualStyleBackColor = true;
            // 
            // _doNothingRadioButton
            // 
            this._doNothingRadioButton.AutoSize = true;
            this._doNothingRadioButton.Location = new System.Drawing.Point(6, 19);
            this._doNothingRadioButton.Name = "_doNothingRadioButton";
            this._doNothingRadioButton.Size = new System.Drawing.Size(77, 17);
            this._doNothingRadioButton.TabIndex = 0;
            this._doNothingRadioButton.TabStop = true;
            this._doNothingRadioButton.Text = "Do nothing";
            this._doNothingRadioButton.UseVisualStyleBackColor = true;
            // 
            // _createScoreSubattributeCheckBox
            // 
            this._createScoreSubattributeCheckBox.AutoSize = true;
            this._createScoreSubattributeCheckBox.Location = new System.Drawing.Point(25, 353);
            this._createScoreSubattributeCheckBox.Name = "_createScoreSubattributeCheckBox";
            this._createScoreSubattributeCheckBox.Size = new System.Drawing.Size(311, 17);
            this._createScoreSubattributeCheckBox.TabIndex = 13;
            this._createScoreSubattributeCheckBox.Text = "Create best match score subattribute (named \"MatchScore\")";
            this._createScoreSubattributeCheckBox.UseVisualStyleBackColor = true;
            // 
            // TranslateValueToBestMatchSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 427);
            this.Controls.Add(this._createScoreSubattributeCheckBox);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this._minimumMatchScoreNumericUpDown);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._synonymMapPathBrowseButton);
            this.Controls.Add(this._synonymMapPathTagsButton);
            this.Controls.Add(this._synonymMapPathTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._sourceListPathBrowseButton);
            this.Controls.Add(this._sourceListPathTagsButton);
            this.Controls.Add(this._sourceListPathTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 466);
            this.Name = "TranslateValueToBestMatchSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Translate value to best match";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._minimumMatchScoreNumericUpDown)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private Utilities.Forms.ConfigurableObjectControl _attributeSelectorControl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private Utilities.Forms.BrowseButton _sourceListPathBrowseButton;
        private System.Windows.Forms.TextBox _sourceListPathTextBox;
        private Utilities.Forms.PathTagsButton _sourceListPathTagsButton;
        private System.Windows.Forms.Label label2;
        private Utilities.Forms.BrowseButton _synonymMapPathBrowseButton;
        private System.Windows.Forms.TextBox _synonymMapPathTextBox;
        private Utilities.Forms.PathTagsButton _synonymMapPathTagsButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown _minimumMatchScoreNumericUpDown;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton _setTypeToUntranslatedRadioButton;
        private System.Windows.Forms.RadioButton _removeAttributeRadioButton;
        private System.Windows.Forms.RadioButton _clearValueRadioButton;
        private System.Windows.Forms.RadioButton _doNothingRadioButton;
        private System.Windows.Forms.CheckBox _createScoreSubattributeCheckBox;
    }
}
