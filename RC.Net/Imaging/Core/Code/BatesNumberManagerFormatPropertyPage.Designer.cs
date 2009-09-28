namespace Extract.Imaging
{
    partial class BatesNumberManagerFormatPropertyPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="BatesNumberManagerFormatPropertyPage"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="BatesNumberManagerFormatPropertyPage"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources
                if (components != null)
                {
                    components.Dispose();
                }
                if (_nextNumberFileDialog != null)
                {
                    _nextNumberFileDialog.Dispose();
                }
            }

            // Release unmanaged resources

            // Call base dispose method
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._suffixTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this._prefixTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this._digitsUpDown = new System.Windows.Forms.NumericUpDown();
            this._nextNumberFileButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this._sampleNextNumberTextBox = new System.Windows.Forms.TextBox();
            this._nextNumberSpecifiedTextBox = new System.Windows.Forms.TextBox();
            this._zeroPadCheckBox = new System.Windows.Forms.CheckBox();
            this._nextNumberFileTextBox = new System.Windows.Forms.TextBox();
            this._nextNumberFileRadioButton = new System.Windows.Forms.RadioButton();
            this._nextNumberSpecifiedRadioButton = new System.Windows.Forms.RadioButton();
            this._pageNumberSeparatorTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this._pageDigitsUpDown = new System.Windows.Forms.NumericUpDown();
            this._useBatesForEachPageRadioButton = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this._zeroPadPageNumberCheckBox = new System.Windows.Forms.CheckBox();
            this._usePageNumberRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this._sampleBatesNumberTextBox = new System.Windows.Forms.TextBox();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._digitsUpDown)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pageDigitsUpDown)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this._suffixTextBox);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this._prefixTextBox);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Location = new System.Drawing.Point(3, 258);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(424, 51);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Prefixes and suffixes";
            // 
            // _suffixTextBox
            // 
            this._suffixTextBox.Location = new System.Drawing.Point(218, 20);
            this._suffixTextBox.Name = "_suffixTextBox";
            this._suffixTextBox.Size = new System.Drawing.Size(126, 20);
            this._suffixTextBox.TabIndex = 3;
            this._suffixTextBox.TextChanged += new System.EventHandler(this.HandleSuffixTextBoxTextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(179, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(33, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Suffix";
            // 
            // _prefixTextBox
            // 
            this._prefixTextBox.Location = new System.Drawing.Point(47, 20);
            this._prefixTextBox.Name = "_prefixTextBox";
            this._prefixTextBox.Size = new System.Drawing.Size(126, 20);
            this._prefixTextBox.TabIndex = 1;
            this._prefixTextBox.TextChanged += new System.EventHandler(this.HandlePrefixTextBoxTextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Prefix";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this._digitsUpDown);
            this.groupBox1.Controls.Add(this._nextNumberFileButton);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this._sampleNextNumberTextBox);
            this.groupBox1.Controls.Add(this._nextNumberSpecifiedTextBox);
            this.groupBox1.Controls.Add(this._zeroPadCheckBox);
            this.groupBox1.Controls.Add(this._nextNumberFileTextBox);
            this.groupBox1.Controls.Add(this._nextNumberFileRadioButton);
            this.groupBox1.Controls.Add(this._nextNumberSpecifiedRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(424, 124);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Next number";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(123, 97);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "digits";
            // 
            // _digitsUpDown
            // 
            this._digitsUpDown.Location = new System.Drawing.Point(87, 95);
            this._digitsUpDown.Maximum = new decimal(new int[] {
            19,
            0,
            0,
            0});
            this._digitsUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._digitsUpDown.Name = "_digitsUpDown";
            this._digitsUpDown.Size = new System.Drawing.Size(34, 20);
            this._digitsUpDown.TabIndex = 8;
            this._digitsUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._digitsUpDown.ValueChanged += new System.EventHandler(this.HandleDigitsUpDownValueChanged);
            // 
            // _nextNumberFileButton
            // 
            this._nextNumberFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._nextNumberFileButton.Enabled = false;
            this._nextNumberFileButton.Location = new System.Drawing.Point(277, 66);
            this._nextNumberFileButton.Name = "_nextNumberFileButton";
            this._nextNumberFileButton.Size = new System.Drawing.Size(28, 23);
            this._nextNumberFileButton.TabIndex = 5;
            this._nextNumberFileButton.Text = "...";
            this._nextNumberFileButton.UseVisualStyleBackColor = true;
            this._nextNumberFileButton.Click += new System.EventHandler(this.HandleNextNumberFileButtonClick);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(308, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Next number";
            // 
            // _sampleNextNumberTextBox
            // 
            this._sampleNextNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sampleNextNumberTextBox.Enabled = false;
            this._sampleNextNumberTextBox.Location = new System.Drawing.Point(311, 68);
            this._sampleNextNumberTextBox.Name = "_sampleNextNumberTextBox";
            this._sampleNextNumberTextBox.Size = new System.Drawing.Size(107, 20);
            this._sampleNextNumberTextBox.TabIndex = 6;
            this._sampleNextNumberTextBox.Leave += new System.EventHandler(this.HandleSampleNextNumberTextBoxLeave);
            // 
            // _nextNumberSpecifiedTextBox
            // 
            this._nextNumberSpecifiedTextBox.Location = new System.Drawing.Point(95, 19);
            this._nextNumberSpecifiedTextBox.Name = "_nextNumberSpecifiedTextBox";
            this._nextNumberSpecifiedTextBox.Size = new System.Drawing.Size(143, 20);
            this._nextNumberSpecifiedTextBox.TabIndex = 1;
            this._nextNumberSpecifiedTextBox.TextChanged += new System.EventHandler(this.HandleNextNumberSpecifiedTextBoxTextChanged);
            // 
            // _zeroPadCheckBox
            // 
            this._zeroPadCheckBox.AutoSize = true;
            this._zeroPadCheckBox.Checked = true;
            this._zeroPadCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._zeroPadCheckBox.Location = new System.Drawing.Point(7, 95);
            this._zeroPadCheckBox.Name = "_zeroPadCheckBox";
            this._zeroPadCheckBox.Size = new System.Drawing.Size(81, 17);
            this._zeroPadCheckBox.TabIndex = 7;
            this._zeroPadCheckBox.Text = "Zero pad to";
            this._zeroPadCheckBox.UseVisualStyleBackColor = true;
            this._zeroPadCheckBox.CheckedChanged += new System.EventHandler(this.HandleZeroPadCheckBoxCheckedChanged);
            // 
            // _nextNumberFileTextBox
            // 
            this._nextNumberFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._nextNumberFileTextBox.Enabled = false;
            this._nextNumberFileTextBox.Location = new System.Drawing.Point(26, 68);
            this._nextNumberFileTextBox.Name = "_nextNumberFileTextBox";
            this._nextNumberFileTextBox.Size = new System.Drawing.Size(245, 20);
            this._nextNumberFileTextBox.TabIndex = 4;
            this._nextNumberFileTextBox.TextChanged += new System.EventHandler(this.HandleNextNumberFileTextBoxTextChanged);
            // 
            // _nextNumberFileRadioButton
            // 
            this._nextNumberFileRadioButton.AutoSize = true;
            this._nextNumberFileRadioButton.Location = new System.Drawing.Point(7, 44);
            this._nextNumberFileRadioButton.Name = "_nextNumberFileRadioButton";
            this._nextNumberFileRadioButton.Size = new System.Drawing.Size(160, 17);
            this._nextNumberFileRadioButton.TabIndex = 2;
            this._nextNumberFileRadioButton.Text = "From shared next number file";
            this._nextNumberFileRadioButton.UseVisualStyleBackColor = true;
            // 
            // _nextNumberSpecifiedRadioButton
            // 
            this._nextNumberSpecifiedRadioButton.AutoSize = true;
            this._nextNumberSpecifiedRadioButton.Checked = true;
            this._nextNumberSpecifiedRadioButton.Location = new System.Drawing.Point(7, 20);
            this._nextNumberSpecifiedRadioButton.Name = "_nextNumberSpecifiedRadioButton";
            this._nextNumberSpecifiedRadioButton.Size = new System.Drawing.Size(82, 17);
            this._nextNumberSpecifiedRadioButton.TabIndex = 0;
            this._nextNumberSpecifiedRadioButton.TabStop = true;
            this._nextNumberSpecifiedRadioButton.Text = "As specified";
            this._nextNumberSpecifiedRadioButton.UseVisualStyleBackColor = true;
            this._nextNumberSpecifiedRadioButton.CheckedChanged += new System.EventHandler(this.HandleNextNumberSpecifiedRadioButtonCheckedChanged);
            // 
            // _pageNumberSeparatorTextBox
            // 
            this._pageNumberSeparatorTextBox.Location = new System.Drawing.Point(171, 68);
            this._pageNumberSeparatorTextBox.Name = "_pageNumberSeparatorTextBox";
            this._pageNumberSeparatorTextBox.Size = new System.Drawing.Size(93, 20);
            this._pageNumberSeparatorTextBox.TabIndex = 5;
            this._pageNumberSeparatorTextBox.TextChanged += new System.EventHandler(this.HandlePageNumberSeparatorTextBoxTextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this._pageNumberSeparatorTextBox);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this._pageDigitsUpDown);
            this.groupBox2.Controls.Add(this._useBatesForEachPageRadioButton);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this._zeroPadPageNumberCheckBox);
            this.groupBox2.Controls.Add(this._usePageNumberRadioButton);
            this.groupBox2.Location = new System.Drawing.Point(3, 133);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.groupBox2.Size = new System.Drawing.Size(424, 119);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Bates number format";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(207, 44);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "digits";
            // 
            // _pageDigitsUpDown
            // 
            this._pageDigitsUpDown.Location = new System.Drawing.Point(171, 42);
            this._pageDigitsUpDown.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this._pageDigitsUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._pageDigitsUpDown.Name = "_pageDigitsUpDown";
            this._pageDigitsUpDown.Size = new System.Drawing.Size(34, 20);
            this._pageDigitsUpDown.TabIndex = 2;
            this._pageDigitsUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._pageDigitsUpDown.ValueChanged += new System.EventHandler(this.HandlePageDigitsUpDownValueChanged);
            // 
            // _useBatesForEachPageRadioButton
            // 
            this._useBatesForEachPageRadioButton.AutoSize = true;
            this._useBatesForEachPageRadioButton.Location = new System.Drawing.Point(7, 94);
            this._useBatesForEachPageRadioButton.Name = "_useBatesForEachPageRadioButton";
            this._useBatesForEachPageRadioButton.Size = new System.Drawing.Size(176, 17);
            this._useBatesForEachPageRadioButton.TabIndex = 6;
            this._useBatesForEachPageRadioButton.Text = "Use next number for every page";
            this._useBatesForEachPageRadioButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Page number separator";
            // 
            // _zeroPadPageNumberCheckBox
            // 
            this._zeroPadPageNumberCheckBox.AutoSize = true;
            this._zeroPadPageNumberCheckBox.Checked = true;
            this._zeroPadPageNumberCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._zeroPadPageNumberCheckBox.Location = new System.Drawing.Point(26, 43);
            this._zeroPadPageNumberCheckBox.Name = "_zeroPadPageNumberCheckBox";
            this._zeroPadPageNumberCheckBox.Size = new System.Drawing.Size(146, 17);
            this._zeroPadPageNumberCheckBox.TabIndex = 1;
            this._zeroPadPageNumberCheckBox.Text = "Zero pad page number to";
            this._zeroPadPageNumberCheckBox.UseVisualStyleBackColor = true;
            this._zeroPadPageNumberCheckBox.CheckedChanged += new System.EventHandler(this.HandleZeroPadPageNumberCheckBoxCheckedChanged);
            // 
            // _usePageNumberRadioButton
            // 
            this._usePageNumberRadioButton.AutoSize = true;
            this._usePageNumberRadioButton.Checked = true;
            this._usePageNumberRadioButton.Location = new System.Drawing.Point(7, 20);
            this._usePageNumberRadioButton.Name = "_usePageNumberRadioButton";
            this._usePageNumberRadioButton.Size = new System.Drawing.Size(373, 17);
            this._usePageNumberRadioButton.TabIndex = 0;
            this._usePageNumberRadioButton.TabStop = true;
            this._usePageNumberRadioButton.Text = "Use next number for each document and add separator with page number";
            this._usePageNumberRadioButton.UseVisualStyleBackColor = true;
            this._usePageNumberRadioButton.CheckedChanged += new System.EventHandler(this.HandleUsePageNumberRadioButtonCheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this._sampleBatesNumberTextBox);
            this.groupBox4.Location = new System.Drawing.Point(3, 315);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(424, 51);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Sample Bates number";
            // 
            // _sampleBatesNumberTextBox
            // 
            this._sampleBatesNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._sampleBatesNumberTextBox.Enabled = false;
            this._sampleBatesNumberTextBox.Location = new System.Drawing.Point(7, 20);
            this._sampleBatesNumberTextBox.Name = "_sampleBatesNumberTextBox";
            this._sampleBatesNumberTextBox.Size = new System.Drawing.Size(411, 20);
            this._sampleBatesNumberTextBox.TabIndex = 0;
            // 
            // BatesNumberManagerFormatPropertyPage
            // 
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox4);
            this.Name = "BatesNumberManagerFormatPropertyPage";
            this.Size = new System.Drawing.Size(430, 366);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._digitsUpDown)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pageDigitsUpDown)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox _suffixTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox _prefixTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown _digitsUpDown;
        private System.Windows.Forms.Button _nextNumberFileButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox _sampleNextNumberTextBox;
        private System.Windows.Forms.TextBox _nextNumberSpecifiedTextBox;
        private System.Windows.Forms.CheckBox _zeroPadCheckBox;
        private System.Windows.Forms.TextBox _nextNumberFileTextBox;
        private System.Windows.Forms.RadioButton _nextNumberFileRadioButton;
        private System.Windows.Forms.RadioButton _nextNumberSpecifiedRadioButton;
        private System.Windows.Forms.TextBox _pageNumberSeparatorTextBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown _pageDigitsUpDown;
        private System.Windows.Forms.RadioButton _useBatesForEachPageRadioButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox _zeroPadPageNumberCheckBox;
        private System.Windows.Forms.RadioButton _usePageNumberRadioButton;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox _sampleBatesNumberTextBox;
    }
}
