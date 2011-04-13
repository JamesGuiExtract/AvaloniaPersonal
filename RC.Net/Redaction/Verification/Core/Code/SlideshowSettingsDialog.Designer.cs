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
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.TextBox textBox1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SlideshowSettingsDialog));
            System.Windows.Forms.GroupBox groupBox3;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.GroupBox groupBox1;
            this._requireRunKeyRadioButton = new System.Windows.Forms.RadioButton();
            this._promptIntervalUpDown = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._promptRandomlyRadioButton = new System.Windows.Forms.RadioButton();
            this._forceFitToPageModeCheckBox = new System.Windows.Forms.CheckBox();
            this._label1 = new System.Windows.Forms.Label();
            this._applyDocumentTagCheckBox = new System.Windows.Forms.CheckBox();
            this._tagNameComboBox = new System.Windows.Forms.ComboBox();
            this._setFileActionStatusCheckBox = new System.Windows.Forms.CheckBox();
            this._documentConditionButton = new System.Windows.Forms.Button();
            this._actionNameComboBox = new Extract.Utilities.Forms.BetterComboBox();
            this._documentConditionTextBox = new System.Windows.Forms.TextBox();
            this._actionStatusComboBox = new System.Windows.Forms.ComboBox();
            this._pauseOnDocumentConditionCheckBox = new System.Windows.Forms.CheckBox();
            this._actionNamePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this.label3 = new System.Windows.Forms.Label();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            groupBox2 = new System.Windows.Forms.GroupBox();
            textBox1 = new System.Windows.Forms.TextBox();
            groupBox3 = new System.Windows.Forms.GroupBox();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._promptIntervalUpDown)).BeginInit();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(textBox1);
            groupBox2.Location = new System.Drawing.Point(6, 202);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(501, 91);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Run key";
            // 
            // textBox1
            // 
            textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            textBox1.BackColor = System.Drawing.SystemColors.Control;
            textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textBox1.Location = new System.Drawing.Point(9, 19);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(486, 65);
            textBox1.TabIndex = 0;
            textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // groupBox3
            // 
            groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox3.Controls.Add(this._requireRunKeyRadioButton);
            groupBox3.Controls.Add(label2);
            groupBox3.Controls.Add(this._promptIntervalUpDown);
            groupBox3.Controls.Add(label1);
            groupBox3.Controls.Add(this._promptRandomlyRadioButton);
            groupBox3.Location = new System.Drawing.Point(6, 299);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(501, 83);
            groupBox3.TabIndex = 4;
            groupBox3.TabStop = false;
            groupBox3.Text = "Confirm user alertness";
            // 
            // _requireRunKeyRadioButton
            // 
            this._requireRunKeyRadioButton.AutoSize = true;
            this._requireRunKeyRadioButton.Location = new System.Drawing.Point(24, 56);
            this._requireRunKeyRadioButton.Name = "_requireRunKeyRadioButton";
            this._requireRunKeyRadioButton.Size = new System.Drawing.Size(223, 17);
            this._requireRunKeyRadioButton.TabIndex = 3;
            this._requireRunKeyRadioButton.TabStop = true;
            this._requireRunKeyRadioButton.Text = "Require the operator to use the \"run key\".";
            this._requireRunKeyRadioButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(456, 34);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(39, 13);
            label2.TabIndex = 3;
            label2.Text = "pages.";
            // 
            // _promptIntervalUpDown
            // 
            this._promptIntervalUpDown.Location = new System.Drawing.Point(404, 32);
            this._promptIntervalUpDown.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this._promptIntervalUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._promptIntervalUpDown.Name = "_promptIntervalUpDown";
            this._promptIntervalUpDown.Size = new System.Drawing.Size(46, 20);
            this._promptIntervalUpDown.TabIndex = 2;
            this._promptIntervalUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._promptIntervalUpDown.UserTextCorrected += new System.EventHandler<System.EventArgs>(this.HandlePromptIntervalTextCorrected);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 16);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(418, 13);
            label1.TabIndex = 0;
            label1.Text = "In order to protect against documents being committed without being properly revi" +
    "ewed:";
            // 
            // _promptRandomlyRadioButton
            // 
            this._promptRandomlyRadioButton.AutoSize = true;
            this._promptRandomlyRadioButton.Location = new System.Drawing.Point(24, 32);
            this._promptRandomlyRadioButton.Name = "_promptRandomlyRadioButton";
            this._promptRandomlyRadioButton.Size = new System.Drawing.Size(366, 17);
            this._promptRandomlyRadioButton.TabIndex = 1;
            this._promptRandomlyRadioButton.TabStop = true;
            this._promptRandomlyRadioButton.Text = "Randomly prompt the operator to answer a simple question at most every";
            this._promptRandomlyRadioButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._forceFitToPageModeCheckBox);
            groupBox1.Controls.Add(this._label1);
            groupBox1.Controls.Add(this._applyDocumentTagCheckBox);
            groupBox1.Controls.Add(this._tagNameComboBox);
            groupBox1.Controls.Add(this._setFileActionStatusCheckBox);
            groupBox1.Controls.Add(this._documentConditionButton);
            groupBox1.Controls.Add(this._actionNameComboBox);
            groupBox1.Controls.Add(this._documentConditionTextBox);
            groupBox1.Controls.Add(this._actionStatusComboBox);
            groupBox1.Controls.Add(this._pauseOnDocumentConditionCheckBox);
            groupBox1.Controls.Add(this._actionNamePathTagsButton);
            groupBox1.Controls.Add(this.label3);
            groupBox1.Location = new System.Drawing.Point(6, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(501, 184);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "General";
            // 
            // _forceFitToPageModeCheckBox
            // 
            this._forceFitToPageModeCheckBox.AutoSize = true;
            this._forceFitToPageModeCheckBox.Location = new System.Drawing.Point(7, 16);
            this._forceFitToPageModeCheckBox.Name = "_forceFitToPageModeCheckBox";
            this._forceFitToPageModeCheckBox.Size = new System.Drawing.Size(293, 17);
            this._forceFitToPageModeCheckBox.TabIndex = 6;
            this._forceFitToPageModeCheckBox.Text = "Switch to fit-to-page mode when the slideshow is started.";
            this._forceFitToPageModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // _label1
            // 
            this._label1.AutoSize = true;
            this._label1.Location = new System.Drawing.Point(6, 36);
            this._label1.Name = "_label1";
            this._label1.Size = new System.Drawing.Size(377, 13);
            this._label1.TabIndex = 7;
            this._label1.Text = "When automatically advancing to the next document without user intervention:";
            // 
            // _applyDocumentTagCheckBox
            // 
            this._applyDocumentTagCheckBox.AutoSize = true;
            this._applyDocumentTagCheckBox.Checked = true;
            this._applyDocumentTagCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._applyDocumentTagCheckBox.Location = new System.Drawing.Point(24, 60);
            this._applyDocumentTagCheckBox.Name = "_applyDocumentTagCheckBox";
            this._applyDocumentTagCheckBox.Size = new System.Drawing.Size(138, 17);
            this._applyDocumentTagCheckBox.TabIndex = 8;
            this._applyDocumentTagCheckBox.Text = "Apply the document tag";
            this._applyDocumentTagCheckBox.UseVisualStyleBackColor = true;
            // 
            // _tagNameComboBox
            // 
            this._tagNameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._tagNameComboBox.FormattingEnabled = true;
            this._tagNameComboBox.Location = new System.Drawing.Point(171, 58);
            this._tagNameComboBox.Name = "_tagNameComboBox";
            this._tagNameComboBox.Size = new System.Drawing.Size(158, 21);
            this._tagNameComboBox.TabIndex = 9;
            // 
            // _setFileActionStatusCheckBox
            // 
            this._setFileActionStatusCheckBox.AutoSize = true;
            this._setFileActionStatusCheckBox.Location = new System.Drawing.Point(24, 83);
            this._setFileActionStatusCheckBox.Name = "_setFileActionStatusCheckBox";
            this._setFileActionStatusCheckBox.Size = new System.Drawing.Size(133, 17);
            this._setFileActionStatusCheckBox.TabIndex = 10;
            this._setFileActionStatusCheckBox.Text = "Set file action status of";
            this._setFileActionStatusCheckBox.UseVisualStyleBackColor = true;
            // 
            // _documentConditionButton
            // 
            this._documentConditionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._documentConditionButton.Location = new System.Drawing.Point(411, 155);
            this._documentConditionButton.Name = "_documentConditionButton";
            this._documentConditionButton.Size = new System.Drawing.Size(84, 23);
            this._documentConditionButton.TabIndex = 5;
            this._documentConditionButton.Text = "Commands>";
            this._documentConditionButton.UseVisualStyleBackColor = true;
            this._documentConditionButton.Click += new System.EventHandler(this.HandleDocumentConditionClick);
            // 
            // _actionNameComboBox
            // 
            this._actionNameComboBox.FormattingEnabled = true;
            this._actionNameComboBox.HideSelection = false;
            this._actionNameComboBox.Location = new System.Drawing.Point(42, 106);
            this._actionNameComboBox.Name = "_actionNameComboBox";
            this._actionNameComboBox.Size = new System.Drawing.Size(184, 21);
            this._actionNameComboBox.TabIndex = 11;
            // 
            // _documentConditionTextBox
            // 
            this._documentConditionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._documentConditionTextBox.Enabled = false;
            this._documentConditionTextBox.Location = new System.Drawing.Point(24, 157);
            this._documentConditionTextBox.Name = "_documentConditionTextBox";
            this._documentConditionTextBox.Size = new System.Drawing.Size(381, 20);
            this._documentConditionTextBox.TabIndex = 4;
            this._documentConditionTextBox.DoubleClick += new System.EventHandler(this.HandleDocumentConditionTextBoxDoubleClick);
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
            this._actionStatusComboBox.Location = new System.Drawing.Point(278, 106);
            this._actionStatusComboBox.Name = "_actionStatusComboBox";
            this._actionStatusComboBox.Size = new System.Drawing.Size(158, 21);
            this._actionStatusComboBox.TabIndex = 2;
            // 
            // _pauseOnDocumentConditionCheckBox
            // 
            this._pauseOnDocumentConditionCheckBox.AutoSize = true;
            this._pauseOnDocumentConditionCheckBox.Location = new System.Drawing.Point(7, 133);
            this._pauseOnDocumentConditionCheckBox.Name = "_pauseOnDocumentConditionCheckBox";
            this._pauseOnDocumentConditionCheckBox.Size = new System.Drawing.Size(336, 17);
            this._pauseOnDocumentConditionCheckBox.TabIndex = 3;
            this._pauseOnDocumentConditionCheckBox.Text = "Stop slideshow when the document meets the following condition:";
            this._pauseOnDocumentConditionCheckBox.UseVisualStyleBackColor = true;
            // 
            // _actionNamePathTagsButton
            // 
            this._actionNamePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_actionNamePathTagsButton.Image")));
            this._actionNamePathTagsButton.Location = new System.Drawing.Point(232, 106);
            this._actionNamePathTagsButton.Name = "_actionNamePathTagsButton";
            this._actionNamePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._actionNamePathTagsButton.TabIndex = 0;
            this._actionNamePathTagsButton.UseVisualStyleBackColor = true;
            this._actionNamePathTagsButton.TagSelected += new System.EventHandler<Extract.Utilities.Forms.TagSelectedEventArgs>(this.HandleActionNamePathTagsButtonTagSelected);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(256, 109);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "to";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(437, 387);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(70, 23);
            this._cancelButton.TabIndex = 1;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(361, 387);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(70, 23);
            this._okButton.TabIndex = 0;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // SlideshowSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(519, 422);
            this.Controls.Add(groupBox3);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SlideshowSettingsDialog";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Slideshow settings";
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._promptIntervalUpDown)).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.RadioButton _promptRandomlyRadioButton;
        private Utilities.Forms.BetterNumericUpDown _promptIntervalUpDown;
        private System.Windows.Forms.Label label3;
        private Utilities.Forms.PathTagsButton _actionNamePathTagsButton;
        private System.Windows.Forms.CheckBox _pauseOnDocumentConditionCheckBox;
        private System.Windows.Forms.ComboBox _actionStatusComboBox;
        private System.Windows.Forms.TextBox _documentConditionTextBox;
        private Utilities.Forms.BetterComboBox _actionNameComboBox;
        private System.Windows.Forms.Button _documentConditionButton;
        private System.Windows.Forms.CheckBox _setFileActionStatusCheckBox;
        private System.Windows.Forms.ComboBox _tagNameComboBox;
        private System.Windows.Forms.CheckBox _applyDocumentTagCheckBox;
        private System.Windows.Forms.Label _label1;
        private System.Windows.Forms.CheckBox _forceFitToPageModeCheckBox;
        private System.Windows.Forms.RadioButton _requireRunKeyRadioButton;
    }
}