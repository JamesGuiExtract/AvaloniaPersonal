namespace Extract.Redaction
{
    partial class VOAFileCompareConditionSettingsDialog
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
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VOAFileCompareConditionSettingsDialog));
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._dataFile1TextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._conditionMetComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._dataFile2TextBox = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._overlapThresholdUpDown = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._dataFile1PathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._dataFile2PathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._outputConditionCheckBox = new System.Windows.Forms.CheckBox();
            this._outputFilePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._outputFileTextBox = new System.Windows.Forms.TextBox();
            this._outputDataCheckBox = new System.Windows.Forms.CheckBox();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._overlapThresholdUpDown)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(6, 123);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(307, 13);
            label4.TabIndex = 8;
            label4.Text = "Consider redactions as equal if they mutually overlap each other";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(378, 123);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(46, 13);
            label5.TabIndex = 10;
            label5.Text = "percent.";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(294, 282);
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
            this._cancelButton.Location = new System.Drawing.Point(375, 282);
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
            this._dataFile1TextBox.Size = new System.Drawing.Size(405, 20);
            this._dataFile1TextBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Consider the condition as";
            // 
            // _conditionMetComboBox
            // 
            this._conditionMetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._conditionMetComboBox.FormattingEnabled = true;
            this._conditionMetComboBox.Items.AddRange(new object[] {
            "met",
            "not met"});
            this._conditionMetComboBox.Location = new System.Drawing.Point(137, 19);
            this._conditionMetComboBox.Name = "_conditionMetComboBox";
            this._conditionMetComboBox.Size = new System.Drawing.Size(66, 21);
            this._conditionMetComboBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(209, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(154, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "if the redactions in the data file:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(178, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "match the redactions in the data file:";
            // 
            // _dataFile2TextBox
            // 
            this._dataFile2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFile2TextBox.Location = new System.Drawing.Point(6, 89);
            this._dataFile2TextBox.Name = "_dataFile2TextBox";
            this._dataFile2TextBox.Size = new System.Drawing.Size(405, 20);
            this._dataFile2TextBox.TabIndex = 6;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._conditionMetComboBox);
            this.groupBox1.Controls.Add(label5);
            this.groupBox1.Controls.Add(this._dataFile1TextBox);
            this.groupBox1.Controls.Add(this._overlapThresholdUpDown);
            this.groupBox1.Controls.Add(this._dataFile1PathTagsButton);
            this.groupBox1.Controls.Add(label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._dataFile2PathTagsButton);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this._dataFile2TextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(438, 148);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data comparison";
            // 
            // _overlapThresholdUpDown
            // 
            this._overlapThresholdUpDown.Location = new System.Drawing.Point(331, 121);
            this._overlapThresholdUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._overlapThresholdUpDown.Name = "_overlapThresholdUpDown";
            this._overlapThresholdUpDown.Size = new System.Drawing.Size(43, 20);
            this._overlapThresholdUpDown.TabIndex = 9;
            this._overlapThresholdUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // _dataFile1PathTagsButton
            // 
            this._dataFile1PathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFile1PathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_dataFile1PathTagsButton.Image")));
            this._dataFile1PathTagsButton.Location = new System.Drawing.Point(415, 45);
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
            this._dataFile2PathTagsButton.Location = new System.Drawing.Point(415, 88);
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
            this.groupBox2.Controls.Add(this._outputConditionCheckBox);
            this.groupBox2.Controls.Add(this._outputFilePathTagsButton);
            this.groupBox2.Controls.Add(this._outputFileTextBox);
            this.groupBox2.Controls.Add(this._outputDataCheckBox);
            this.groupBox2.Location = new System.Drawing.Point(12, 167);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(438, 92);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output";
            // 
            // _outputConditionCheckBox
            // 
            this._outputConditionCheckBox.AutoSize = true;
            this._outputConditionCheckBox.Location = new System.Drawing.Point(6, 68);
            this._outputConditionCheckBox.Name = "_outputConditionCheckBox";
            this._outputConditionCheckBox.Size = new System.Drawing.Size(215, 17);
            this._outputConditionCheckBox.TabIndex = 12;
            this._outputConditionCheckBox.Text = "Only create output if the condition is met";
            this._outputConditionCheckBox.UseVisualStyleBackColor = true;
            // 
            // _outputFilePathTagsButton
            // 
            this._outputFilePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputFilePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_outputFilePathTagsButton.Image")));
            this._outputFilePathTagsButton.Location = new System.Drawing.Point(415, 42);
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
            this._outputFileTextBox.Size = new System.Drawing.Size(405, 20);
            this._outputFileTextBox.TabIndex = 7;
            // 
            // _outputDataCheckBox
            // 
            this._outputDataCheckBox.AutoSize = true;
            this._outputDataCheckBox.Location = new System.Drawing.Point(6, 19);
            this._outputDataCheckBox.Name = "_outputDataCheckBox";
            this._outputDataCheckBox.Size = new System.Drawing.Size(161, 17);
            this._outputDataCheckBox.TabIndex = 0;
            this._outputDataCheckBox.Text = "Output a merged VOA file to:";
            this._outputDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // VOAFileCompareConditionSettingsDialog
            // 
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(462, 317);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(468, 345);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(468, 345);
            this.Name = "VOAFileCompareConditionSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Redaction: Compare ID Shield data files condition";
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
        private System.Windows.Forms.ComboBox _conditionMetComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private Utilities.Forms.PathTagsButton _dataFile2PathTagsButton;
        private System.Windows.Forms.TextBox _dataFile2TextBox;
        private Utilities.Forms.BetterNumericUpDown _overlapThresholdUpDown;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox _outputDataCheckBox;
        private Utilities.Forms.PathTagsButton _outputFilePathTagsButton;
        private System.Windows.Forms.TextBox _outputFileTextBox;
        private System.Windows.Forms.CheckBox _outputConditionCheckBox;
    }
}
