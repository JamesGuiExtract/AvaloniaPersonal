namespace Extract.Redaction
{
    partial class VOAFileMergeTaskSettingsDialog
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VOAFileMergeTaskSettingsDialog));
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._dataFile1TextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._dataFile2TextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._useMutualOverlapComboBox = new System.Windows.Forms.ComboBox();
            this._overlapThresholdUpDown = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._dataFile1PathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._dataFile2PathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this._outputFilePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._outputFileTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._overlapThresholdUpDown)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(327, 259);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 11;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(408, 259);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 12;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _dataFile1TextBox
            // 
            this._dataFile1TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFile1TextBox.Location = new System.Drawing.Point(6, 46);
            this._dataFile1TextBox.Name = "_dataFile1TextBox";
            this._dataFile1TextBox.Size = new System.Drawing.Size(438, 20);
            this._dataFile1TextBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(168, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Merge redaction from the data file:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(150, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "with redactions in the data file:";
            // 
            // _dataFile2TextBox
            // 
            this._dataFile2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFile2TextBox.Location = new System.Drawing.Point(6, 89);
            this._dataFile2TextBox.Name = "_dataFile2TextBox";
            this._dataFile2TextBox.Size = new System.Drawing.Size(438, 20);
            this._dataFile2TextBox.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 123);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(151, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Consider redactions as equal if";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(400, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "percent.";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._useMutualOverlapComboBox);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this._dataFile1TextBox);
            this.groupBox1.Controls.Add(this._overlapThresholdUpDown);
            this.groupBox1.Controls.Add(this._dataFile1PathTagsButton);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._dataFile2PathTagsButton);
            this.groupBox1.Controls.Add(this._dataFile2TextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(11, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(471, 148);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source";
            // 
            // _useMutualOverlapComboBox
            // 
            this._useMutualOverlapComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._useMutualOverlapComboBox.FormattingEnabled = true;
            this._useMutualOverlapComboBox.Items.AddRange(new object[] {
            "they mutually overlap each other",
            "one overlaps the other by"});
            this._useMutualOverlapComboBox.Location = new System.Drawing.Point(165, 120);
            this._useMutualOverlapComboBox.Name = "_useMutualOverlapComboBox";
            this._useMutualOverlapComboBox.Size = new System.Drawing.Size(182, 21);
            this._useMutualOverlapComboBox.TabIndex = 15;
            // 
            // _overlapThresholdUpDown
            // 
            this._overlapThresholdUpDown.Location = new System.Drawing.Point(353, 121);
            this._overlapThresholdUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._overlapThresholdUpDown.Name = "_overlapThresholdUpDown";
            this._overlapThresholdUpDown.Size = new System.Drawing.Size(43, 20);
            this._overlapThresholdUpDown.TabIndex = 9;
            this._overlapThresholdUpDown.Value = new decimal(new int[] {
            75,
            0,
            0,
            0});
            // 
            // _dataFile1PathTagsButton
            // 
            this._dataFile1PathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFile1PathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_dataFile1PathTagsButton.Image")));
            this._dataFile1PathTagsButton.Location = new System.Drawing.Point(448, 45);
            this._dataFile1PathTagsButton.Name = "_dataFile1PathTagsButton";
            this._dataFile1PathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._dataFile1PathTagsButton.TabIndex = 4;
            this._dataFile1PathTagsButton.TextControl = this._dataFile1TextBox;
            this._dataFile1PathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _dataFile2PathTagsButton
            // 
            this._dataFile2PathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFile2PathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_dataFile2PathTagsButton.Image")));
            this._dataFile2PathTagsButton.Location = new System.Drawing.Point(448, 88);
            this._dataFile2PathTagsButton.Name = "_dataFile2PathTagsButton";
            this._dataFile2PathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._dataFile2PathTagsButton.TabIndex = 7;
            this._dataFile2PathTagsButton.TextControl = this._dataFile2TextBox;
            this._dataFile2PathTagsButton.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this._outputFilePathTagsButton);
            this.groupBox2.Controls.Add(this._outputFileTextBox);
            this.groupBox2.Location = new System.Drawing.Point(12, 167);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(471, 72);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(142, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Output a merged VOA file to:";
            // 
            // _outputFilePathTagsButton
            // 
            this._outputFilePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputFilePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_outputFilePathTagsButton.Image")));
            this._outputFilePathTagsButton.Location = new System.Drawing.Point(448, 42);
            this._outputFilePathTagsButton.Name = "_outputFilePathTagsButton";
            this._outputFilePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._outputFilePathTagsButton.TabIndex = 11;
            this._outputFilePathTagsButton.TextControl = this._outputFileTextBox;
            this._outputFilePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _outputFileTextBox
            // 
            this._outputFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputFileTextBox.Location = new System.Drawing.Point(6, 42);
            this._outputFileTextBox.Name = "_outputFileTextBox";
            this._outputFileTextBox.Size = new System.Drawing.Size(438, 20);
            this._outputFileTextBox.TabIndex = 7;
            // 
            // VOAFileMergeTaskSettingsDialog
            // 
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(495, 294);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VOAFileMergeTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Redaction: Merge ID Shield data files";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._overlapThresholdUpDown)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private Utilities.Forms.PathTagsButton _dataFile1PathTagsButton;
        private System.Windows.Forms.TextBox _dataFile1TextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private Utilities.Forms.PathTagsButton _dataFile2PathTagsButton;
        private System.Windows.Forms.TextBox _dataFile2TextBox;
        private System.Windows.Forms.Label label4;
        private Utilities.Forms.BetterNumericUpDown _overlapThresholdUpDown;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private Utilities.Forms.PathTagsButton _outputFilePathTagsButton;
        private System.Windows.Forms.TextBox _outputFileTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox _useMutualOverlapComboBox;
    }
}
