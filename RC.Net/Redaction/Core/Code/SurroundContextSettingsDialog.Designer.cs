namespace Extract.Redaction
{
    partial class SurroundContextSettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="SurroundContextSettingsDialog"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="SurroundContextSettingsDialog"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources
                if(components != null)
                {
                    components.Dispose();
                }
            }
				
            // Release unmanaged resources

            // Call base dispose method
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
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.Label label1;
            this._dataTypesTextBox = new System.Windows.Forms.TextBox();
            this._extendSpecificTypesRadioButton = new System.Windows.Forms.RadioButton();
            this._extendAllTypesRadioButton = new System.Windows.Forms.RadioButton();
            this._maxWordsNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this._extendHeightCheckBox = new System.Windows.Forms.CheckBox();
            this._redactWordsCheckBox = new System.Windows.Forms.CheckBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._dataFileControl = new Extract.Redaction.DataFileControl();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._maxWordsNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this._dataTypesTextBox);
            groupBox1.Controls.Add(this._extendSpecificTypesRadioButton);
            groupBox1.Controls.Add(this._extendAllTypesRadioButton);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(397, 99);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Extend redactions to surround context";
            // 
            // _dataTypesTextBox
            // 
            this._dataTypesTextBox.Enabled = false;
            this._dataTypesTextBox.Location = new System.Drawing.Point(26, 67);
            this._dataTypesTextBox.Name = "_dataTypesTextBox";
            this._dataTypesTextBox.Size = new System.Drawing.Size(362, 20);
            this._dataTypesTextBox.TabIndex = 2;
            // 
            // _extendSpecificTypesRadioButton
            // 
            this._extendSpecificTypesRadioButton.AutoSize = true;
            this._extendSpecificTypesRadioButton.Location = new System.Drawing.Point(7, 44);
            this._extendSpecificTypesRadioButton.Name = "_extendSpecificTypesRadioButton";
            this._extendSpecificTypesRadioButton.Size = new System.Drawing.Size(306, 17);
            this._extendSpecificTypesRadioButton.TabIndex = 1;
            this._extendSpecificTypesRadioButton.TabStop = true;
            this._extendSpecificTypesRadioButton.Text = "For the following data types (separate types using a comma)";
            this._extendSpecificTypesRadioButton.UseVisualStyleBackColor = true;
            this._extendSpecificTypesRadioButton.CheckedChanged += new System.EventHandler(this.HandleExtendSpecificTypesRadioButtonCheckedChanged);
            // 
            // _extendAllTypesRadioButton
            // 
            this._extendAllTypesRadioButton.AutoSize = true;
            this._extendAllTypesRadioButton.Location = new System.Drawing.Point(7, 20);
            this._extendAllTypesRadioButton.Name = "_extendAllTypesRadioButton";
            this._extendAllTypesRadioButton.Size = new System.Drawing.Size(105, 17);
            this._extendAllTypesRadioButton.TabIndex = 0;
            this._extendAllTypesRadioButton.TabStop = true;
            this._extendAllTypesRadioButton.Text = "For all data types";
            this._extendAllTypesRadioButton.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(this._maxWordsNumericUpDown);
            groupBox2.Controls.Add(this._extendHeightCheckBox);
            groupBox2.Controls.Add(this._redactWordsCheckBox);
            groupBox2.Controls.Add(label1);
            groupBox2.Location = new System.Drawing.Point(12, 118);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(397, 71);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Options";
            // 
            // _maxWordsNumericUpDown
            // 
            this._maxWordsNumericUpDown.Enabled = false;
            this._maxWordsNumericUpDown.Location = new System.Drawing.Point(240, 19);
            this._maxWordsNumericUpDown.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this._maxWordsNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._maxWordsNumericUpDown.Name = "_maxWordsNumericUpDown";
            this._maxWordsNumericUpDown.Size = new System.Drawing.Size(38, 20);
            this._maxWordsNumericUpDown.TabIndex = 2;
            this._maxWordsNumericUpDown.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // _extendHeightCheckBox
            // 
            this._extendHeightCheckBox.AutoSize = true;
            this._extendHeightCheckBox.Location = new System.Drawing.Point(7, 44);
            this._extendHeightCheckBox.Name = "_extendHeightCheckBox";
            this._extendHeightCheckBox.Size = new System.Drawing.Size(270, 17);
            this._extendHeightCheckBox.TabIndex = 1;
            this._extendHeightCheckBox.Text = "Adjust height of redaction to further obscure context";
            this._extendHeightCheckBox.UseVisualStyleBackColor = true;
            // 
            // _redactWordsCheckBox
            // 
            this._redactWordsCheckBox.AutoSize = true;
            this._redactWordsCheckBox.Location = new System.Drawing.Point(7, 20);
            this._redactWordsCheckBox.Name = "_redactWordsCheckBox";
            this._redactWordsCheckBox.Size = new System.Drawing.Size(229, 17);
            this._redactWordsCheckBox.TabIndex = 0;
            this._redactWordsCheckBox.Text = "Randomly redact surrounding context up to";
            this._redactWordsCheckBox.UseVisualStyleBackColor = true;
            this._redactWordsCheckBox.CheckedChanged += new System.EventHandler(this.HandleRedactWordsCheckBoxCheckedChanged);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(278, 21);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(99, 13);
            label1.TabIndex = 3;
            label1.Text = "words on each side";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(334, 270);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 4;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(253, 270);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 3;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _dataFileControl
            // 
            this._dataFileControl.Location = new System.Drawing.Point(12, 196);
            this._dataFileControl.Name = "_dataFileControl";
            this._dataFileControl.Size = new System.Drawing.Size(398, 60);
            this._dataFileControl.TabIndex = 2;
            // 
            // SurroundContextSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(422, 305);
            this.Controls.Add(this._dataFileControl);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(428, 333);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(428, 333);
            this.Name = "SurroundContextSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Redaction: Extend redactions to surround context";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._maxWordsNumericUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TextBox _dataTypesTextBox;
        private System.Windows.Forms.RadioButton _extendSpecificTypesRadioButton;
        private System.Windows.Forms.RadioButton _extendAllTypesRadioButton;
        private System.Windows.Forms.NumericUpDown _maxWordsNumericUpDown;
        private System.Windows.Forms.CheckBox _extendHeightCheckBox;
        private System.Windows.Forms.CheckBox _redactWordsCheckBox;
        private DataFileControl _dataFileControl;
    }
}