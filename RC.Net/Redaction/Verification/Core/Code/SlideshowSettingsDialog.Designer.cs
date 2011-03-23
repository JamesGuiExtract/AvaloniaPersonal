namespace Extract.Redaction.Verification
{
    partial class SlideshowSettingsDialog
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
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }

            _miscUtils = null;
            _documentCondition = null;

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SlideshowSettingsDialog));
            this.label3 = new System.Windows.Forms.Label();
            this._applyDocumentTagCheckBox = new System.Windows.Forms.CheckBox();
            this._tagNameComboBox = new System.Windows.Forms.ComboBox();
            this._actionNamePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._actionStatusComboBox = new System.Windows.Forms.ComboBox();
            this._setFileActionStatusCheckBox = new System.Windows.Forms.CheckBox();
            this._pauseOnDocumentConditionCheckBox = new System.Windows.Forms.CheckBox();
            this._documentConditionTextBox = new System.Windows.Forms.TextBox();
            this._documentConditionButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._label1 = new System.Windows.Forms.Label();
            this._actionNameComboBox = new Extract.Utilities.Forms.BetterComboBox();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(263, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "to";
            // 
            // _applyDocumentTagCheckBox
            // 
            this._applyDocumentTagCheckBox.AutoSize = true;
            this._applyDocumentTagCheckBox.Checked = true;
            this._applyDocumentTagCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._applyDocumentTagCheckBox.Location = new System.Drawing.Point(31, 37);
            this._applyDocumentTagCheckBox.Name = "_applyDocumentTagCheckBox";
            this._applyDocumentTagCheckBox.Size = new System.Drawing.Size(138, 17);
            this._applyDocumentTagCheckBox.TabIndex = 4;
            this._applyDocumentTagCheckBox.Text = "Apply the document tag";
            this._applyDocumentTagCheckBox.UseVisualStyleBackColor = true;
            this._applyDocumentTagCheckBox.CheckedChanged += new System.EventHandler(this.HandleDocumentTagCheckBoxCheckedChanged);
            // 
            // _tagNameComboBox
            // 
            this._tagNameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._tagNameComboBox.FormattingEnabled = true;
            this._tagNameComboBox.Location = new System.Drawing.Point(178, 35);
            this._tagNameComboBox.Name = "_tagNameComboBox";
            this._tagNameComboBox.Size = new System.Drawing.Size(158, 21);
            this._tagNameComboBox.TabIndex = 5;
            // 
            // _actionNamePathTagsButton
            // 
            this._actionNamePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_actionNamePathTagsButton.Image")));
            this._actionNamePathTagsButton.Location = new System.Drawing.Point(239, 83);
            this._actionNamePathTagsButton.Name = "_actionNamePathTagsButton";
            this._actionNamePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._actionNamePathTagsButton.TabIndex = 2;
            this._actionNamePathTagsButton.UseVisualStyleBackColor = true;
            this._actionNamePathTagsButton.TagSelected += new System.EventHandler<Extract.Utilities.Forms.TagSelectedEventArgs>(this.HandleActionNamePathTagsButtonTagSelected);
            // 
            // _actionStatusComboBox
            // 
            this._actionStatusComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._actionStatusComboBox.FormattingEnabled = true;
            this._actionStatusComboBox.Items.AddRange(new object[] {
            "Completed",
            "Failed",
            "Pending",
            "Skipped",
            "Unattempted"});
            this._actionStatusComboBox.Location = new System.Drawing.Point(285, 83);
            this._actionStatusComboBox.Name = "_actionStatusComboBox";
            this._actionStatusComboBox.Size = new System.Drawing.Size(130, 21);
            this._actionStatusComboBox.TabIndex = 10;
            // 
            // _setFileActionStatusCheckBox
            // 
            this._setFileActionStatusCheckBox.AutoSize = true;
            this._setFileActionStatusCheckBox.Location = new System.Drawing.Point(31, 60);
            this._setFileActionStatusCheckBox.Name = "_setFileActionStatusCheckBox";
            this._setFileActionStatusCheckBox.Size = new System.Drawing.Size(133, 17);
            this._setFileActionStatusCheckBox.TabIndex = 6;
            this._setFileActionStatusCheckBox.Text = "Set file action status of";
            this._setFileActionStatusCheckBox.UseVisualStyleBackColor = true;
            this._setFileActionStatusCheckBox.CheckedChanged += new System.EventHandler(this.HandleSetFileActionStatusCheckBoxCheckedChanged);
            // 
            // _pauseOnDocumentConditionCheckBox
            // 
            this._pauseOnDocumentConditionCheckBox.AutoSize = true;
            this._pauseOnDocumentConditionCheckBox.Location = new System.Drawing.Point(12, 110);
            this._pauseOnDocumentConditionCheckBox.Name = "_pauseOnDocumentConditionCheckBox";
            this._pauseOnDocumentConditionCheckBox.Size = new System.Drawing.Size(336, 17);
            this._pauseOnDocumentConditionCheckBox.TabIndex = 11;
            this._pauseOnDocumentConditionCheckBox.Text = "Stop slideshow when the document meets the following condition:";
            this._pauseOnDocumentConditionCheckBox.UseVisualStyleBackColor = true;
            this._pauseOnDocumentConditionCheckBox.CheckedChanged += new System.EventHandler(this.HandleCheckDocumentConditionCheckChanged);
            // 
            // _documentConditionTextBox
            // 
            this._documentConditionTextBox.Enabled = false;
            this._documentConditionTextBox.Location = new System.Drawing.Point(31, 134);
            this._documentConditionTextBox.Name = "_documentConditionTextBox";
            this._documentConditionTextBox.Size = new System.Drawing.Size(283, 20);
            this._documentConditionTextBox.TabIndex = 12;
            this._documentConditionTextBox.DoubleClick += new System.EventHandler(this.HandleDocumentConditionTextBoxDoubleClick);
            // 
            // _documentConditionButton
            // 
            this._documentConditionButton.Location = new System.Drawing.Point(331, 132);
            this._documentConditionButton.Name = "_documentConditionButton";
            this._documentConditionButton.Size = new System.Drawing.Size(84, 23);
            this._documentConditionButton.TabIndex = 13;
            this._documentConditionButton.Text = "Commands>";
            this._documentConditionButton.UseVisualStyleBackColor = true;
            this._documentConditionButton.Click += new System.EventHandler(this.HandleDocumentConditionClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(345, 170);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(70, 23);
            this._cancelButton.TabIndex = 14;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(269, 170);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(70, 23);
            this._okButton.TabIndex = 15;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _label1
            // 
            this._label1.AutoSize = true;
            this._label1.Location = new System.Drawing.Point(13, 13);
            this._label1.Name = "_label1";
            this._label1.Size = new System.Drawing.Size(377, 13);
            this._label1.TabIndex = 16;
            this._label1.Text = "When automatically advancing to the next document without user intervention:";
            // 
            // _actionNameComboBox
            // 
            this._actionNameComboBox.FormattingEnabled = true;
            this._actionNameComboBox.HideSelection = false;
            this._actionNameComboBox.Location = new System.Drawing.Point(49, 83);
            this._actionNameComboBox.Name = "_actionNameComboBox";
            this._actionNameComboBox.Size = new System.Drawing.Size(184, 21);
            this._actionNameComboBox.TabIndex = 7;
            // 
            // SlideshowSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(427, 205);
            this.Controls.Add(this._label1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._documentConditionButton);
            this.Controls.Add(this._documentConditionTextBox);
            this.Controls.Add(this._pauseOnDocumentConditionCheckBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._actionNamePathTagsButton);
            this.Controls.Add(this._actionStatusComboBox);
            this.Controls.Add(this._actionNameComboBox);
            this.Controls.Add(this._setFileActionStatusCheckBox);
            this.Controls.Add(this._tagNameComboBox);
            this.Controls.Add(this._applyDocumentTagCheckBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SlideshowSettingsDialog";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Slideshow settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox _applyDocumentTagCheckBox;
        private System.Windows.Forms.ComboBox _tagNameComboBox;
        private Extract.Utilities.Forms.PathTagsButton _actionNamePathTagsButton;
        private System.Windows.Forms.ComboBox _actionStatusComboBox;
        private Extract.Utilities.Forms.BetterComboBox _actionNameComboBox;
        private System.Windows.Forms.CheckBox _setFileActionStatusCheckBox;
        private System.Windows.Forms.CheckBox _pauseOnDocumentConditionCheckBox;
        private System.Windows.Forms.TextBox _documentConditionTextBox;
        private System.Windows.Forms.Button _documentConditionButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label _label1;
    }
}