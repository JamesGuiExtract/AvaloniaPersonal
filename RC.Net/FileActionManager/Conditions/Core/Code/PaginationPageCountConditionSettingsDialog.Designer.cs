namespace Extract.FileActionManager.Conditions
{
    partial class PaginationPageCountConditionSettingsDialog
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
            this._onlyAllowFirstOrLastCheckBox = new System.Windows.Forms.CheckBox();
            this._onlyAllowLastCheckBox = new System.Windows.Forms.CheckBox();
            this._onlyAllowFirstCheckBox = new System.Windows.Forms.CheckBox();
            this.betterNumericUpDown1 = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._deletedPagesCheckBox = new System.Windows.Forms.CheckBox();
            this._deletedPageCountUpDown = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._deletedPagesComparisonComboBox = new Extract.Utilities.Forms.BetterComboBox();
            this._outputPagesCheckBox = new System.Windows.Forms.CheckBox();
            this._outputPageCountUpDown = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._outputPagesComparisonComboBox = new Extract.Utilities.Forms.BetterComboBox();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.betterNumericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._deletedPageCountUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._outputPageCountUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._onlyAllowFirstOrLastCheckBox);
            groupBox1.Controls.Add(this._onlyAllowLastCheckBox);
            groupBox1.Controls.Add(this._onlyAllowFirstCheckBox);
            groupBox1.Controls.Add(this.betterNumericUpDown1);
            groupBox1.Controls.Add(this._deletedPagesCheckBox);
            groupBox1.Controls.Add(this._deletedPageCountUpDown);
            groupBox1.Controls.Add(this._deletedPagesComparisonComboBox);
            groupBox1.Controls.Add(this._outputPagesCheckBox);
            groupBox1.Controls.Add(this._outputPageCountUpDown);
            groupBox1.Controls.Add(this._outputPagesComparisonComboBox);
            groupBox1.Location = new System.Drawing.Point(13, 13);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(521, 154);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "This condition is met only if all checked conditions are true";
            // 
            // _onlyAllowFirstOrLastCheckBox
            // 
            this._onlyAllowFirstOrLastCheckBox.AutoSize = true;
            this._onlyAllowFirstOrLastCheckBox.Location = new System.Drawing.Point(11, 129);
            this._onlyAllowFirstOrLastCheckBox.Name = "_onlyAllowFirstOrLastCheckBox";
            this._onlyAllowFirstOrLastCheckBox.Size = new System.Drawing.Size(298, 17);
            this._onlyAllowFirstOrLastCheckBox.TabIndex = 8;
            this._onlyAllowFirstOrLastCheckBox.Text = "No page other than the first or last is proposed for deletion";
            this._onlyAllowFirstOrLastCheckBox.UseVisualStyleBackColor = true;
            this._onlyAllowFirstOrLastCheckBox.CheckedChanged += new System.EventHandler(this.HandleOnlyAllowFirstOrLastCheckBox_CheckedChanged);
            // 
            // _onlyAllowLastCheckBox
            // 
            this._onlyAllowLastCheckBox.AutoSize = true;
            this._onlyAllowLastCheckBox.Location = new System.Drawing.Point(11, 102);
            this._onlyAllowLastCheckBox.Name = "_onlyAllowLastCheckBox";
            this._onlyAllowLastCheckBox.Size = new System.Drawing.Size(267, 17);
            this._onlyAllowLastCheckBox.TabIndex = 7;
            this._onlyAllowLastCheckBox.Text = "No page other than the last is proposed for deletion";
            this._onlyAllowLastCheckBox.UseVisualStyleBackColor = true;
            this._onlyAllowLastCheckBox.CheckedChanged += new System.EventHandler(this.HandleOnlyAllowLastCheckBox_CheckedChanged);
            // 
            // _onlyAllowFirstCheckBox
            // 
            this._onlyAllowFirstCheckBox.AutoSize = true;
            this._onlyAllowFirstCheckBox.Location = new System.Drawing.Point(11, 75);
            this._onlyAllowFirstCheckBox.Name = "_onlyAllowFirstCheckBox";
            this._onlyAllowFirstCheckBox.Size = new System.Drawing.Size(267, 17);
            this._onlyAllowFirstCheckBox.TabIndex = 6;
            this._onlyAllowFirstCheckBox.Text = "No page other than the first is proposed for deletion";
            this._onlyAllowFirstCheckBox.UseVisualStyleBackColor = true;
            this._onlyAllowFirstCheckBox.CheckedChanged += new System.EventHandler(this.HandleOnlyAllowFirstCheckBox_CheckedChanged);
            // 
            // betterNumericUpDown1
            // 
            this.betterNumericUpDown1.IntegersOnly = true;
            this.betterNumericUpDown1.Location = new System.Drawing.Point(554, 133);
            this.betterNumericUpDown1.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.betterNumericUpDown1.Name = "betterNumericUpDown1";
            this.betterNumericUpDown1.Size = new System.Drawing.Size(69, 20);
            this.betterNumericUpDown1.TabIndex = 9;
            this.betterNumericUpDown1.ThousandsSeparator = true;
            // 
            // _deletedPagesCheckBox
            // 
            this._deletedPagesCheckBox.AutoSize = true;
            this._deletedPagesCheckBox.Location = new System.Drawing.Point(11, 48);
            this._deletedPagesCheckBox.Name = "_deletedPagesCheckBox";
            this._deletedPagesCheckBox.Size = new System.Drawing.Size(239, 17);
            this._deletedPagesCheckBox.TabIndex = 3;
            this._deletedPagesCheckBox.Text = "The number of pages proposed for deletion is";
            this._deletedPagesCheckBox.UseVisualStyleBackColor = true;
            this._deletedPagesCheckBox.CheckedChanged += new System.EventHandler(this.HandleDeletedPagesCheckBox_CheckedChanged);
            // 
            // _deletedPageCountUpDown
            // 
            this._deletedPageCountUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._deletedPageCountUpDown.Enabled = false;
            this._deletedPageCountUpDown.IntegersOnly = true;
            this._deletedPageCountUpDown.Location = new System.Drawing.Point(440, 47);
            this._deletedPageCountUpDown.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this._deletedPageCountUpDown.Name = "_deletedPageCountUpDown";
            this._deletedPageCountUpDown.Size = new System.Drawing.Size(69, 20);
            this._deletedPageCountUpDown.TabIndex = 5;
            this._deletedPageCountUpDown.ThousandsSeparator = true;
            // 
            // _deletedPagesComparisonComboBox
            // 
            this._deletedPagesComparisonComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._deletedPagesComparisonComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._deletedPagesComparisonComboBox.Enabled = false;
            this._deletedPagesComparisonComboBox.FormattingEnabled = true;
            this._deletedPagesComparisonComboBox.Location = new System.Drawing.Point(300, 46);
            this._deletedPagesComparisonComboBox.Name = "_deletedPagesComparisonComboBox";
            this._deletedPagesComparisonComboBox.Size = new System.Drawing.Size(134, 21);
            this._deletedPagesComparisonComboBox.TabIndex = 4;
            // 
            // _outputPagesCheckBox
            // 
            this._outputPagesCheckBox.AutoSize = true;
            this._outputPagesCheckBox.Location = new System.Drawing.Point(11, 21);
            this._outputPagesCheckBox.Name = "_outputPagesCheckBox";
            this._outputPagesCheckBox.Size = new System.Drawing.Size(197, 17);
            this._outputPagesCheckBox.TabIndex = 0;
            this._outputPagesCheckBox.Text = "The number of pages to be output is";
            this._outputPagesCheckBox.UseVisualStyleBackColor = true;
            this._outputPagesCheckBox.CheckedChanged += new System.EventHandler(this.HandleOutputPagesCheckBox_CheckedChanged);
            // 
            // _outputPageCountUpDown
            // 
            this._outputPageCountUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPageCountUpDown.Enabled = false;
            this._outputPageCountUpDown.IntegersOnly = true;
            this._outputPageCountUpDown.Location = new System.Drawing.Point(440, 20);
            this._outputPageCountUpDown.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this._outputPageCountUpDown.Name = "_outputPageCountUpDown";
            this._outputPageCountUpDown.Size = new System.Drawing.Size(69, 20);
            this._outputPageCountUpDown.TabIndex = 2;
            this._outputPageCountUpDown.ThousandsSeparator = true;
            // 
            // _outputPagesComparisonComboBox
            // 
            this._outputPagesComparisonComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPagesComparisonComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._outputPagesComparisonComboBox.Enabled = false;
            this._outputPagesComparisonComboBox.FormattingEnabled = true;
            this._outputPagesComparisonComboBox.Location = new System.Drawing.Point(300, 19);
            this._outputPagesComparisonComboBox.Name = "_outputPagesComparisonComboBox";
            this._outputPagesComparisonComboBox.Size = new System.Drawing.Size(134, 21);
            this._outputPagesComparisonComboBox.TabIndex = 1;
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.Location = new System.Drawing.Point(378, 173);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 1;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(459, 173);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 2;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // PaginationPageCountConditionSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(546, 208);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PaginationPageCountConditionSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Page count condition settings (pagination)";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.betterNumericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._deletedPageCountUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._outputPageCountUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonCancel;
        private Utilities.Forms.BetterComboBox _outputPagesComparisonComboBox;
        private Utilities.Forms.BetterNumericUpDown _outputPageCountUpDown;
        private System.Windows.Forms.CheckBox _outputPagesCheckBox;
        private Utilities.Forms.BetterComboBox _deletedPagesComparisonComboBox;
        private Utilities.Forms.BetterNumericUpDown _deletedPageCountUpDown;
        private System.Windows.Forms.CheckBox _deletedPagesCheckBox;
        private System.Windows.Forms.CheckBox _onlyAllowFirstOrLastCheckBox;
        private System.Windows.Forms.CheckBox _onlyAllowLastCheckBox;
        private System.Windows.Forms.CheckBox _onlyAllowFirstCheckBox;
        private Utilities.Forms.BetterNumericUpDown betterNumericUpDown1;
    }
}