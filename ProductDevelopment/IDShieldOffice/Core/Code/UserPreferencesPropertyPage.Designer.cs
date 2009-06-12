namespace IDShieldOffice
{
    partial class UserPreferencesPropertyPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="UserPreferencesPropertyPage"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="UserPreferencesPropertyPage"/>.
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
            this._batesNumberPage = new System.Windows.Forms.TabPage();
            this._outputFilesPage = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._exampleLinkLabel = new System.Windows.Forms.LinkLabel();
            this._useOutputPath = new System.Windows.Forms.CheckBox();
            this._outputPathTextBox = new System.Windows.Forms.TextBox();
            this._pathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._sampleOutputFileName = new System.Windows.Forms.TextBox();
            this._labelSampleOutputFileName = new System.Windows.Forms.Label();
            this._sampleInputFileName = new System.Windows.Forms.TextBox();
            this._labelSampleInputFileName = new System.Windows.Forms.Label();
            this._outputFormatComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._saveIdsoWithImageCheckBox = new System.Windows.Forms.CheckBox();
            this._generalPage = new System.Windows.Forms.TabPage();
            this._verifyAllPagesCheckBox = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._redactionFillColorComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._ocrTradeoffTrackBar = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this._outputFilesPage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this._generalPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._ocrTradeoffTrackBar)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _batesNumberPage
            // 
            this._batesNumberPage.BackColor = System.Drawing.SystemColors.Control;
            this._batesNumberPage.Location = new System.Drawing.Point(4, 22);
            this._batesNumberPage.Name = "_batesNumberPage";
            this._batesNumberPage.Padding = new System.Windows.Forms.Padding(3);
            this._batesNumberPage.Size = new System.Drawing.Size(484, 262);
            this._batesNumberPage.TabIndex = 3;
            this._batesNumberPage.Text = "Bates number";
            // 
            // _outputFilesPage
            // 
            this._outputFilesPage.BackColor = System.Drawing.SystemColors.Control;
            this._outputFilesPage.Controls.Add(this.groupBox2);
            this._outputFilesPage.Controls.Add(this.groupBox1);
            this._outputFilesPage.Location = new System.Drawing.Point(4, 22);
            this._outputFilesPage.Name = "_outputFilesPage";
            this._outputFilesPage.Padding = new System.Windows.Forms.Padding(3);
            this._outputFilesPage.Size = new System.Drawing.Size(484, 262);
            this._outputFilesPage.TabIndex = 1;
            this._outputFilesPage.Text = "Output files";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this._exampleLinkLabel);
            this.groupBox2.Controls.Add(this._useOutputPath);
            this.groupBox2.Controls.Add(this._outputPathTextBox);
            this.groupBox2.Controls.Add(this._pathTagsButton);
            this.groupBox2.Controls.Add(this._sampleOutputFileName);
            this.groupBox2.Controls.Add(this._labelSampleOutputFileName);
            this.groupBox2.Controls.Add(this._sampleInputFileName);
            this.groupBox2.Controls.Add(this._labelSampleInputFileName);
            this.groupBox2.Controls.Add(this._outputFormatComboBox);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(7, 61);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(471, 197);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Default save format and location";
            // 
            // _exampleLinkLabel
            // 
            this._exampleLinkLabel.AutoSize = true;
            this._exampleLinkLabel.Location = new System.Drawing.Point(383, 64);
            this._exampleLinkLabel.Name = "_exampleLinkLabel";
            this._exampleLinkLabel.Size = new System.Drawing.Size(76, 13);
            this._exampleLinkLabel.TabIndex = 3;
            this._exampleLinkLabel.TabStop = true;
            this._exampleLinkLabel.Text = "See examples.";
            this._exampleLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleLinkLabelSeeExamplesClicked);
            // 
            // _useOutputPath
            // 
            this._useOutputPath.AutoSize = true;
            this._useOutputPath.Location = new System.Drawing.Point(9, 63);
            this._useOutputPath.Name = "_useOutputPath";
            this._useOutputPath.Size = new System.Drawing.Size(368, 17);
            this._useOutputPath.TabIndex = 2;
            this._useOutputPath.Text = "Use the following expression as the default output path for redacted files.";
            this._useOutputPath.UseVisualStyleBackColor = true;
            this._useOutputPath.CheckedChanged += new System.EventHandler(this.HandleUseOutputPathCheckBoxChanged);
            // 
            // _outputPathTextBox
            // 
            this._outputPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathTextBox.Enabled = false;
            this._outputPathTextBox.HideSelection = false;
            this._outputPathTextBox.Location = new System.Drawing.Point(9, 83);
            this._outputPathTextBox.Name = "_outputPathTextBox";
            this._outputPathTextBox.Size = new System.Drawing.Size(394, 20);
            this._outputPathTextBox.TabIndex = 4;
            this._outputPathTextBox.TextChanged += new System.EventHandler(this.HandleOutputPathTextBoxTextChanged);
            // 
            // _pathTagsButton
            // 
            this._pathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pathTagsButton.Enabled = false;
            this._pathTagsButton.Location = new System.Drawing.Point(409, 81);
            this._pathTagsButton.Name = "_pathTagsButton";
            this._pathTagsButton.Size = new System.Drawing.Size(56, 23);
            this._pathTagsButton.TabIndex = 5;
            this._pathTagsButton.Text = "Tags >";
            this._pathTagsButton.UseVisualStyleBackColor = true;
            this._pathTagsButton.TagSelected += new System.EventHandler<Extract.Utilities.Forms.TagSelectedEventArgs>(this.HandlePathTagsButtonTagSelected);
            // 
            // _sampleOutputFileName
            // 
            this._sampleOutputFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._sampleOutputFileName.Location = new System.Drawing.Point(9, 168);
            this._sampleOutputFileName.Name = "_sampleOutputFileName";
            this._sampleOutputFileName.ReadOnly = true;
            this._sampleOutputFileName.Size = new System.Drawing.Size(455, 20);
            this._sampleOutputFileName.TabIndex = 9;
            this._sampleOutputFileName.Visible = false;
            // 
            // _labelSampleOutputFileName
            // 
            this._labelSampleOutputFileName.AutoSize = true;
            this._labelSampleOutputFileName.Location = new System.Drawing.Point(6, 152);
            this._labelSampleOutputFileName.Name = "_labelSampleOutputFileName";
            this._labelSampleOutputFileName.Size = new System.Drawing.Size(168, 13);
            this._labelSampleOutputFileName.TabIndex = 8;
            this._labelSampleOutputFileName.Text = "Sample redacted output file name:";
            this._labelSampleOutputFileName.Visible = false;
            // 
            // _sampleInputFileName
            // 
            this._sampleInputFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._sampleInputFileName.Location = new System.Drawing.Point(9, 126);
            this._sampleInputFileName.Name = "_sampleInputFileName";
            this._sampleInputFileName.ReadOnly = true;
            this._sampleInputFileName.Size = new System.Drawing.Size(455, 20);
            this._sampleInputFileName.TabIndex = 7;
            this._sampleInputFileName.Visible = false;
            // 
            // _labelSampleInputFileName
            // 
            this._labelSampleInputFileName.AutoSize = true;
            this._labelSampleInputFileName.Location = new System.Drawing.Point(6, 110);
            this._labelSampleInputFileName.Name = "_labelSampleInputFileName";
            this._labelSampleInputFileName.Size = new System.Drawing.Size(152, 13);
            this._labelSampleInputFileName.TabIndex = 6;
            this._labelSampleInputFileName.Text = "Sample original input file name:";
            this._labelSampleInputFileName.Visible = false;
            // 
            // _outputFormatComboBox
            // 
            this._outputFormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._outputFormatComboBox.FormattingEnabled = true;
            this._outputFormatComboBox.Items.AddRange(new object[] {
            "TIF",
            "PDF",
            "IDSO"});
            this._outputFormatComboBox.Location = new System.Drawing.Point(9, 36);
            this._outputFormatComboBox.Name = "_outputFormatComboBox";
            this._outputFormatComboBox.Size = new System.Drawing.Size(121, 21);
            this._outputFormatComboBox.TabIndex = 1;
            this._outputFormatComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleOutputFormatComboBoxSelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(99, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Default save format";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._saveIdsoWithImageCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(7, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(471, 47);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Audit files";
            // 
            // _saveIdsoWithImageCheckBox
            // 
            this._saveIdsoWithImageCheckBox.AutoSize = true;
            this._saveIdsoWithImageCheckBox.Location = new System.Drawing.Point(7, 20);
            this._saveIdsoWithImageCheckBox.Name = "_saveIdsoWithImageCheckBox";
            this._saveIdsoWithImageCheckBox.Size = new System.Drawing.Size(394, 17);
            this._saveIdsoWithImageCheckBox.TabIndex = 0;
            this._saveIdsoWithImageCheckBox.Text = "Automatically save ID Shield Office (.idso) files whenever a document is saved";
            this._saveIdsoWithImageCheckBox.UseVisualStyleBackColor = true;
            this._saveIdsoWithImageCheckBox.CheckedChanged += new System.EventHandler(this.HandleSaveIdsoWithImageCheckBoxCheckedChanged);
            // 
            // _generalPage
            // 
            this._generalPage.BackColor = System.Drawing.SystemColors.Control;
            this._generalPage.Controls.Add(this._verifyAllPagesCheckBox);
            this._generalPage.Controls.Add(this.label10);
            this._generalPage.Controls.Add(this.label4);
            this._generalPage.Controls.Add(this._redactionFillColorComboBox);
            this._generalPage.Controls.Add(this.label3);
            this._generalPage.Controls.Add(this.label2);
            this._generalPage.Controls.Add(this._ocrTradeoffTrackBar);
            this._generalPage.Controls.Add(this.label1);
            this._generalPage.Location = new System.Drawing.Point(4, 22);
            this._generalPage.Name = "_generalPage";
            this._generalPage.Padding = new System.Windows.Forms.Padding(3);
            this._generalPage.Size = new System.Drawing.Size(484, 262);
            this._generalPage.TabIndex = 0;
            this._generalPage.Text = "General";
            // 
            // _verifyAllPagesCheckBox
            // 
            this._verifyAllPagesCheckBox.AutoSize = true;
            this._verifyAllPagesCheckBox.Location = new System.Drawing.Point(12, 86);
            this._verifyAllPagesCheckBox.Name = "_verifyAllPagesCheckBox";
            this._verifyAllPagesCheckBox.Size = new System.Drawing.Size(349, 17);
            this._verifyAllPagesCheckBox.TabIndex = 7;
            this._verifyAllPagesCheckBox.Text = "Require visiting all pages before a redacted document can be output";
            this._verifyAllPagesCheckBox.UseVisualStyleBackColor = true;
            this._verifyAllPagesCheckBox.CheckedChanged += new System.EventHandler(this.HandleVerifyAllPagesCheckBoxCheckedChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.SystemColors.Control;
            this.label10.Location = new System.Drawing.Point(155, 39);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(52, 13);
            this.label10.TabIndex = 6;
            this.label10.Text = "Balanced";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 62);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(126, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Default redaction fill color";
            // 
            // _redactionFillColorComboBox
            // 
            this._redactionFillColorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._redactionFillColorComboBox.Location = new System.Drawing.Point(141, 59);
            this._redactionFillColorComboBox.Name = "_redactionFillColorComboBox";
            this._redactionFillColorComboBox.Size = new System.Drawing.Size(121, 21);
            this._redactionFillColorComboBox.TabIndex = 4;
            this._redactionFillColorComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleRedactionFillColorComboBoxSelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(251, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Fast";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(83, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Accurate";
            // 
            // _ocrTradeoffTrackBar
            // 
            this._ocrTradeoffTrackBar.LargeChange = 1;
            this._ocrTradeoffTrackBar.Location = new System.Drawing.Point(83, 7);
            this._ocrTradeoffTrackBar.Maximum = 2;
            this._ocrTradeoffTrackBar.Name = "_ocrTradeoffTrackBar";
            this._ocrTradeoffTrackBar.Size = new System.Drawing.Size(195, 45);
            this._ocrTradeoffTrackBar.TabIndex = 1;
            this._ocrTradeoffTrackBar.ValueChanged += new System.EventHandler(this.HandleOcrTradeoffTrackBarValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "OCR tradeoff";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this._generalPage);
            this.tabControl1.Controls.Add(this._outputFilesPage);
            this.tabControl1.Controls.Add(this._batesNumberPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(492, 288);
            this.tabControl1.TabIndex = 0;
            // 
            // UserPreferencesPropertyPage
            // 
            this.AutoSize = true;
            this.Controls.Add(this.tabControl1);
            this.MinimumSize = new System.Drawing.Size(452, 288);
            this.Name = "UserPreferencesPropertyPage";
            this.Size = new System.Drawing.Size(492, 288);
            this._outputFilesPage.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this._generalPage.ResumeLayout(false);
            this._generalPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._ocrTradeoffTrackBar)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage _batesNumberPage;
        private System.Windows.Forms.TabPage _outputFilesPage;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox _sampleOutputFileName;
        private System.Windows.Forms.Label _labelSampleOutputFileName;
        private System.Windows.Forms.TextBox _sampleInputFileName;
        private System.Windows.Forms.Label _labelSampleInputFileName;
        private System.Windows.Forms.ComboBox _outputFormatComboBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox _saveIdsoWithImageCheckBox;
        private System.Windows.Forms.TabPage _generalPage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox _redactionFillColorComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar _ocrTradeoffTrackBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Label label10;
        private Extract.Utilities.Forms.PathTagsButton _pathTagsButton;
        private System.Windows.Forms.TextBox _outputPathTextBox;
        private System.Windows.Forms.CheckBox _verifyAllPagesCheckBox;
        private System.Windows.Forms.CheckBox _useOutputPath;
        private System.Windows.Forms.LinkLabel _exampleLinkLabel;

    }
}
