namespace Extract.IDShieldStatisticsReporter
{
    partial class IDShieldStatisticsReporterForm
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
            this._tabControl = new System.Windows.Forms.TabControl();
            this._feedbackDataTab = new System.Windows.Forms.TabPage();
            this._feedbackGroupBox = new System.Windows.Forms.GroupBox();
            this._feedbackAdvancedOptions = new System.Windows.Forms.Button();
            this._feedbackBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._feedbackFolderTextBox = new System.Windows.Forms.TextBox();
            this._label1 = new System.Windows.Forms.Label();
            this._analyzeTab = new System.Windows.Forms.TabPage();
            this._analyzeButton = new System.Windows.Forms.Button();
            this._analysisGroupBox = new System.Windows.Forms.GroupBox();
            this._verificationFileConditionButton = new System.Windows.Forms.Button();
            this._automatedFileConditionButton = new System.Windows.Forms.Button();
            this._redactLCDataCheckBox = new System.Windows.Forms.CheckBox();
            this._redactMCDataCheckBox = new System.Windows.Forms.CheckBox();
            this._redactHCDataCheckBox = new System.Windows.Forms.CheckBox();
            this._label3 = new System.Windows.Forms.Label();
            this._dataTypesTextBox = new System.Windows.Forms.TextBox();
            this._limitTypesCheckBox = new System.Windows.Forms.CheckBox();
            this._analysisTypeComboBox = new System.Windows.Forms.ComboBox();
            this._label2 = new System.Windows.Forms.Label();
            this._reviewTab = new System.Windows.Forms.TabPage();
            this._reviewTabControl = new System.Windows.Forms.TabControl();
            this._statisticsTab = new System.Windows.Forms.TabPage();
            this._statisticsTextBox = new System.Windows.Forms.TextBox();
            this._fileListTab = new System.Windows.Forms.TabPage();
            this._fileListCountTextBox = new System.Windows.Forms.TextBox();
            this._label7 = new System.Windows.Forms.Label();
            this._fileListListBox = new System.Windows.Forms.ListBox();
            this._label6 = new System.Windows.Forms.Label();
            this._fileListSelectionComboBox = new System.Windows.Forms.ComboBox();
            this._label5 = new System.Windows.Forms.Label();
            this._resultsSelectionComboBox = new System.Windows.Forms.ComboBox();
            this._label4 = new System.Windows.Forms.Label();
            this._tabControl.SuspendLayout();
            this._feedbackDataTab.SuspendLayout();
            this._feedbackGroupBox.SuspendLayout();
            this._analyzeTab.SuspendLayout();
            this._analysisGroupBox.SuspendLayout();
            this._reviewTab.SuspendLayout();
            this._reviewTabControl.SuspendLayout();
            this._statisticsTab.SuspendLayout();
            this._fileListTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // _tabControl
            // 
            this._tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._tabControl.Controls.Add(this._feedbackDataTab);
            this._tabControl.Controls.Add(this._analyzeTab);
            this._tabControl.Controls.Add(this._reviewTab);
            this._tabControl.Location = new System.Drawing.Point(13, 13);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(496, 291);
            this._tabControl.TabIndex = 0;
            // 
            // _feedbackDataTab
            // 
            this._feedbackDataTab.Controls.Add(this._feedbackGroupBox);
            this._feedbackDataTab.Location = new System.Drawing.Point(4, 22);
            this._feedbackDataTab.Name = "_feedbackDataTab";
            this._feedbackDataTab.Padding = new System.Windows.Forms.Padding(3);
            this._feedbackDataTab.Size = new System.Drawing.Size(488, 265);
            this._feedbackDataTab.TabIndex = 0;
            this._feedbackDataTab.Text = "Feedback data";
            this._feedbackDataTab.UseVisualStyleBackColor = true;
            // 
            // _feedbackGroupBox
            // 
            this._feedbackGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._feedbackGroupBox.Controls.Add(this._feedbackAdvancedOptions);
            this._feedbackGroupBox.Controls.Add(this._feedbackBrowseButton);
            this._feedbackGroupBox.Controls.Add(this._feedbackFolderTextBox);
            this._feedbackGroupBox.Controls.Add(this._label1);
            this._feedbackGroupBox.Location = new System.Drawing.Point(7, 7);
            this._feedbackGroupBox.Name = "_feedbackGroupBox";
            this._feedbackGroupBox.Size = new System.Drawing.Size(475, 93);
            this._feedbackGroupBox.TabIndex = 0;
            this._feedbackGroupBox.TabStop = false;
            this._feedbackGroupBox.Text = "File and folder options";
            // 
            // _feedbackAdvancedOptions
            // 
            this._feedbackAdvancedOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._feedbackAdvancedOptions.Location = new System.Drawing.Point(351, 63);
            this._feedbackAdvancedOptions.Name = "_feedbackAdvancedOptions";
            this._feedbackAdvancedOptions.Size = new System.Drawing.Size(118, 23);
            this._feedbackAdvancedOptions.TabIndex = 3;
            this._feedbackAdvancedOptions.Text = "Advanced options...";
            this._feedbackAdvancedOptions.UseVisualStyleBackColor = true;
            this._feedbackAdvancedOptions.Click += new System.EventHandler(this.OnFeedbackAdvancedOptionsClick);
            // 
            // _feedbackBrowseButton
            // 
            this._feedbackBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._feedbackBrowseButton.FolderBrowser = true;
            this._feedbackBrowseButton.Location = new System.Drawing.Point(444, 35);
            this._feedbackBrowseButton.Name = "_feedbackBrowseButton";
            this._feedbackBrowseButton.Size = new System.Drawing.Size(25, 23);
            this._feedbackBrowseButton.TabIndex = 2;
            this._feedbackBrowseButton.Text = "...";
            this._feedbackBrowseButton.TextControl = this._feedbackFolderTextBox;
            this._feedbackBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _feedbackFolderTextBox
            // 
            this._feedbackFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._feedbackFolderTextBox.Location = new System.Drawing.Point(9, 37);
            this._feedbackFolderTextBox.Name = "_feedbackFolderTextBox";
            this._feedbackFolderTextBox.Size = new System.Drawing.Size(429, 20);
            this._feedbackFolderTextBox.TabIndex = 1;
            this._feedbackFolderTextBox.TextChanged += new System.EventHandler(this.OnFeedbackFolderTextBoxTextChanged);
            // 
            // _label1
            // 
            this._label1.AutoSize = true;
            this._label1.Location = new System.Drawing.Point(6, 21);
            this._label1.Name = "_label1";
            this._label1.Size = new System.Drawing.Size(245, 13);
            this._label1.TabIndex = 0;
            this._label1.Text = "Select feedback data folder to analyze results from";
            // 
            // _analyzeTab
            // 
            this._analyzeTab.Controls.Add(this._analyzeButton);
            this._analyzeTab.Controls.Add(this._analysisGroupBox);
            this._analyzeTab.Location = new System.Drawing.Point(4, 22);
            this._analyzeTab.Name = "_analyzeTab";
            this._analyzeTab.Padding = new System.Windows.Forms.Padding(3);
            this._analyzeTab.Size = new System.Drawing.Size(488, 265);
            this._analyzeTab.TabIndex = 1;
            this._analyzeTab.Text = "Analyze";
            this._analyzeTab.UseVisualStyleBackColor = true;
            // 
            // _analyzeButton
            // 
            this._analyzeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._analyzeButton.Location = new System.Drawing.Point(407, 236);
            this._analyzeButton.Name = "_analyzeButton";
            this._analyzeButton.Size = new System.Drawing.Size(75, 23);
            this._analyzeButton.TabIndex = 1;
            this._analyzeButton.Text = "Analyze";
            this._analyzeButton.UseVisualStyleBackColor = true;
            this._analyzeButton.Click += new System.EventHandler(this.OnAnalyzeButtonClick);
            // 
            // _analysisGroupBox
            // 
            this._analysisGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._analysisGroupBox.Controls.Add(this._verificationFileConditionButton);
            this._analysisGroupBox.Controls.Add(this._automatedFileConditionButton);
            this._analysisGroupBox.Controls.Add(this._redactLCDataCheckBox);
            this._analysisGroupBox.Controls.Add(this._redactMCDataCheckBox);
            this._analysisGroupBox.Controls.Add(this._redactHCDataCheckBox);
            this._analysisGroupBox.Controls.Add(this._label3);
            this._analysisGroupBox.Controls.Add(this._dataTypesTextBox);
            this._analysisGroupBox.Controls.Add(this._limitTypesCheckBox);
            this._analysisGroupBox.Controls.Add(this._analysisTypeComboBox);
            this._analysisGroupBox.Controls.Add(this._label2);
            this._analysisGroupBox.Location = new System.Drawing.Point(7, 7);
            this._analysisGroupBox.Name = "_analysisGroupBox";
            this._analysisGroupBox.Size = new System.Drawing.Size(475, 222);
            this._analysisGroupBox.TabIndex = 0;
            this._analysisGroupBox.TabStop = false;
            this._analysisGroupBox.Text = "Analysis options";
            // 
            // _verificationFileConditionButton
            // 
            this._verificationFileConditionButton.Location = new System.Drawing.Point(242, 191);
            this._verificationFileConditionButton.Name = "_verificationFileConditionButton";
            this._verificationFileConditionButton.Size = new System.Drawing.Size(227, 23);
            this._verificationFileConditionButton.TabIndex = 9;
            this._verificationFileConditionButton.Text = "Verification file selection condition...";
            this._verificationFileConditionButton.UseVisualStyleBackColor = true;
            this._verificationFileConditionButton.Click += new System.EventHandler(this.OnVerificationFileConditionButtonClick);
            // 
            // _automatedFileConditionButton
            // 
            this._automatedFileConditionButton.Location = new System.Drawing.Point(9, 191);
            this._automatedFileConditionButton.Name = "_automatedFileConditionButton";
            this._automatedFileConditionButton.Size = new System.Drawing.Size(227, 23);
            this._automatedFileConditionButton.TabIndex = 8;
            this._automatedFileConditionButton.Text = "Automated file selection condition...";
            this._automatedFileConditionButton.UseVisualStyleBackColor = true;
            this._automatedFileConditionButton.Click += new System.EventHandler(this.OnAutomatedFileConditionButtonClick);
            // 
            // _redactLCDataCheckBox
            // 
            this._redactLCDataCheckBox.AutoSize = true;
            this._redactLCDataCheckBox.Location = new System.Drawing.Point(9, 167);
            this._redactLCDataCheckBox.Name = "_redactLCDataCheckBox";
            this._redactLCDataCheckBox.Size = new System.Drawing.Size(170, 17);
            this._redactLCDataCheckBox.TabIndex = 7;
            this._redactLCDataCheckBox.Text = "Low confidence sensitive data";
            this._redactLCDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // _redactMCDataCheckBox
            // 
            this._redactMCDataCheckBox.AutoSize = true;
            this._redactMCDataCheckBox.Location = new System.Drawing.Point(9, 144);
            this._redactMCDataCheckBox.Name = "_redactMCDataCheckBox";
            this._redactMCDataCheckBox.Size = new System.Drawing.Size(187, 17);
            this._redactMCDataCheckBox.TabIndex = 6;
            this._redactMCDataCheckBox.Text = "Medium confidence sensitive data";
            this._redactMCDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // _redactHCDataCheckBox
            // 
            this._redactHCDataCheckBox.AutoSize = true;
            this._redactHCDataCheckBox.Location = new System.Drawing.Point(9, 121);
            this._redactHCDataCheckBox.Name = "_redactHCDataCheckBox";
            this._redactHCDataCheckBox.Size = new System.Drawing.Size(172, 17);
            this._redactHCDataCheckBox.TabIndex = 5;
            this._redactHCDataCheckBox.Text = "High confidence sensitive data";
            this._redactHCDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // _label3
            // 
            this._label3.AutoSize = true;
            this._label3.Location = new System.Drawing.Point(6, 102);
            this._label3.Name = "_label3";
            this._label3.Size = new System.Drawing.Size(165, 13);
            this._label3.TabIndex = 4;
            this._label3.Text = "Default to redacting the following:";
            // 
            // _dataTypesTextBox
            // 
            this._dataTypesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._dataTypesTextBox.Location = new System.Drawing.Point(9, 69);
            this._dataTypesTextBox.Name = "_dataTypesTextBox";
            this._dataTypesTextBox.Size = new System.Drawing.Size(460, 20);
            this._dataTypesTextBox.TabIndex = 3;
            // 
            // _limitTypesCheckBox
            // 
            this._limitTypesCheckBox.AutoSize = true;
            this._limitTypesCheckBox.Location = new System.Drawing.Point(9, 46);
            this._limitTypesCheckBox.Name = "_limitTypesCheckBox";
            this._limitTypesCheckBox.Size = new System.Drawing.Size(138, 17);
            this._limitTypesCheckBox.TabIndex = 2;
            this._limitTypesCheckBox.Text = "Limit data tested to type";
            this._limitTypesCheckBox.UseVisualStyleBackColor = true;
            this._limitTypesCheckBox.CheckedChanged += new System.EventHandler(this.OnLimitTypesCheckBoxCheckedChanged);
            // 
            // _analysisTypeComboBox
            // 
            this._analysisTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._analysisTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._analysisTypeComboBox.FormattingEnabled = true;
            this._analysisTypeComboBox.Items.AddRange(new object[] {
            "Automated redaction",
            "Standard verification",
            "Hybrid"});
            this._analysisTypeComboBox.Location = new System.Drawing.Point(113, 19);
            this._analysisTypeComboBox.Name = "_analysisTypeComboBox";
            this._analysisTypeComboBox.Size = new System.Drawing.Size(356, 21);
            this._analysisTypeComboBox.TabIndex = 1;
            this._analysisTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.OnAnalysisTypeComboBoxSelectedIndexChanged);
            // 
            // _label2
            // 
            this._label2.AutoSize = true;
            this._label2.Location = new System.Drawing.Point(6, 22);
            this._label2.Name = "_label2";
            this._label2.Size = new System.Drawing.Size(100, 13);
            this._label2.TabIndex = 0;
            this._label2.Text = "Select analysis type";
            // 
            // _reviewTab
            // 
            this._reviewTab.Controls.Add(this._reviewTabControl);
            this._reviewTab.Controls.Add(this._resultsSelectionComboBox);
            this._reviewTab.Controls.Add(this._label4);
            this._reviewTab.Location = new System.Drawing.Point(4, 22);
            this._reviewTab.Name = "_reviewTab";
            this._reviewTab.Size = new System.Drawing.Size(488, 265);
            this._reviewTab.TabIndex = 2;
            this._reviewTab.Text = "Review";
            this._reviewTab.UseVisualStyleBackColor = true;
            // 
            // _reviewTabControl
            // 
            this._reviewTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._reviewTabControl.Controls.Add(this._statisticsTab);
            this._reviewTabControl.Controls.Add(this._fileListTab);
            this._reviewTabControl.Location = new System.Drawing.Point(6, 40);
            this._reviewTabControl.Name = "_reviewTabControl";
            this._reviewTabControl.SelectedIndex = 0;
            this._reviewTabControl.Size = new System.Drawing.Size(479, 222);
            this._reviewTabControl.TabIndex = 2;
            // 
            // _statisticsTab
            // 
            this._statisticsTab.Controls.Add(this._statisticsTextBox);
            this._statisticsTab.Location = new System.Drawing.Point(4, 22);
            this._statisticsTab.Name = "_statisticsTab";
            this._statisticsTab.Padding = new System.Windows.Forms.Padding(3);
            this._statisticsTab.Size = new System.Drawing.Size(471, 196);
            this._statisticsTab.TabIndex = 0;
            this._statisticsTab.Text = "Statistics";
            this._statisticsTab.UseVisualStyleBackColor = true;
            // 
            // _statisticsTextBox
            // 
            this._statisticsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._statisticsTextBox.Location = new System.Drawing.Point(7, 7);
            this._statisticsTextBox.Multiline = true;
            this._statisticsTextBox.Name = "_statisticsTextBox";
            this._statisticsTextBox.ReadOnly = true;
            this._statisticsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._statisticsTextBox.Size = new System.Drawing.Size(458, 183);
            this._statisticsTextBox.TabIndex = 0;
            // 
            // _fileListTab
            // 
            this._fileListTab.Controls.Add(this._fileListCountTextBox);
            this._fileListTab.Controls.Add(this._label7);
            this._fileListTab.Controls.Add(this._fileListListBox);
            this._fileListTab.Controls.Add(this._label6);
            this._fileListTab.Controls.Add(this._fileListSelectionComboBox);
            this._fileListTab.Controls.Add(this._label5);
            this._fileListTab.Location = new System.Drawing.Point(4, 22);
            this._fileListTab.Name = "_fileListTab";
            this._fileListTab.Padding = new System.Windows.Forms.Padding(3);
            this._fileListTab.Size = new System.Drawing.Size(471, 196);
            this._fileListTab.TabIndex = 1;
            this._fileListTab.Text = "File lists";
            this._fileListTab.UseVisualStyleBackColor = true;
            // 
            // _fileListCountTextBox
            // 
            this._fileListCountTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._fileListCountTextBox.Location = new System.Drawing.Point(369, 168);
            this._fileListCountTextBox.Name = "_fileListCountTextBox";
            this._fileListCountTextBox.ReadOnly = true;
            this._fileListCountTextBox.Size = new System.Drawing.Size(95, 20);
            this._fileListCountTextBox.TabIndex = 5;
            // 
            // _label7
            // 
            this._label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._label7.AutoSize = true;
            this._label7.Location = new System.Drawing.Point(145, 171);
            this._label7.Name = "_label7";
            this._label7.Size = new System.Drawing.Size(218, 13);
            this._label7.TabIndex = 4;
            this._label7.Text = "Total number of entries in the selected file list";
            // 
            // _fileListListBox
            // 
            this._fileListListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._fileListListBox.Location = new System.Drawing.Point(6, 71);
            this._fileListListBox.Name = "_fileListListBox";
            this._fileListListBox.Size = new System.Drawing.Size(458, 82);
            this._fileListListBox.TabIndex = 3;
            this._fileListListBox.DoubleClick += new System.EventHandler(this.OnFileListListBoxDoubleClick);
            // 
            // _label6
            // 
            this._label6.AutoSize = true;
            this._label6.Location = new System.Drawing.Point(3, 55);
            this._label6.Name = "_label6";
            this._label6.Size = new System.Drawing.Size(328, 13);
            this._label6.TabIndex = 2;
            this._label6.Text = "Entries in selected file list (double click entry to see detailed analysis)";
            // 
            // _fileListSelectionComboBox
            // 
            this._fileListSelectionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._fileListSelectionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._fileListSelectionComboBox.FormattingEnabled = true;
            this._fileListSelectionComboBox.Location = new System.Drawing.Point(6, 26);
            this._fileListSelectionComboBox.Name = "_fileListSelectionComboBox";
            this._fileListSelectionComboBox.Size = new System.Drawing.Size(458, 21);
            this._fileListSelectionComboBox.TabIndex = 1;
            this._fileListSelectionComboBox.SelectedIndexChanged += new System.EventHandler(this.OnFileListSelectionComboBoxSelectedIndexChanged);
            // 
            // _label5
            // 
            this._label5.AutoSize = true;
            this._label5.Location = new System.Drawing.Point(3, 10);
            this._label5.Name = "_label5";
            this._label5.Size = new System.Drawing.Size(68, 13);
            this._label5.TabIndex = 0;
            this._label5.Text = "Select file list";
            // 
            // _resultsSelectionComboBox
            // 
            this._resultsSelectionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._resultsSelectionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._resultsSelectionComboBox.FormattingEnabled = true;
            this._resultsSelectionComboBox.Location = new System.Drawing.Point(115, 12);
            this._resultsSelectionComboBox.Name = "_resultsSelectionComboBox";
            this._resultsSelectionComboBox.Size = new System.Drawing.Size(360, 21);
            this._resultsSelectionComboBox.TabIndex = 1;
            this._resultsSelectionComboBox.SelectedIndexChanged += new System.EventHandler(this.OnResultsSelectionComboBoxSelectedIndexChanged);
            // 
            // _label4
            // 
            this._label4.AutoSize = true;
            this._label4.Location = new System.Drawing.Point(3, 15);
            this._label4.Name = "_label4";
            this._label4.Size = new System.Drawing.Size(106, 13);
            this._label4.TabIndex = 0;
            this._label4.Text = "Review anaylsis from";
            // 
            // IDShieldStatisticsReporterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(521, 316);
            this.Controls.Add(this._tabControl);
            this.MinimumSize = new System.Drawing.Size(529, 350);
            this.Name = "IDShieldStatisticsReporterForm";
            this.Text = "ID Shield Statistics Reporter";
            this._tabControl.ResumeLayout(false);
            this._feedbackDataTab.ResumeLayout(false);
            this._feedbackGroupBox.ResumeLayout(false);
            this._feedbackGroupBox.PerformLayout();
            this._analyzeTab.ResumeLayout(false);
            this._analysisGroupBox.ResumeLayout(false);
            this._analysisGroupBox.PerformLayout();
            this._reviewTab.ResumeLayout(false);
            this._reviewTab.PerformLayout();
            this._reviewTabControl.ResumeLayout(false);
            this._statisticsTab.ResumeLayout(false);
            this._statisticsTab.PerformLayout();
            this._fileListTab.ResumeLayout(false);
            this._fileListTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl _tabControl;
        private System.Windows.Forms.TabPage _feedbackDataTab;
        private System.Windows.Forms.TabPage _analyzeTab;
        private System.Windows.Forms.TabPage _reviewTab;
        private System.Windows.Forms.GroupBox _feedbackGroupBox;
        private System.Windows.Forms.TextBox _feedbackFolderTextBox;
        private System.Windows.Forms.Label _label1;
        private System.Windows.Forms.Button _feedbackAdvancedOptions;
        private Extract.Utilities.Forms.BrowseButton _feedbackBrowseButton;
        private System.Windows.Forms.GroupBox _analysisGroupBox;
        private System.Windows.Forms.Label _label2;
        private System.Windows.Forms.Label _label3;
        private System.Windows.Forms.TextBox _dataTypesTextBox;
        private System.Windows.Forms.CheckBox _limitTypesCheckBox;
        private System.Windows.Forms.ComboBox _analysisTypeComboBox;
        private System.Windows.Forms.Button _verificationFileConditionButton;
        private System.Windows.Forms.Button _automatedFileConditionButton;
        private System.Windows.Forms.CheckBox _redactLCDataCheckBox;
        private System.Windows.Forms.CheckBox _redactMCDataCheckBox;
        private System.Windows.Forms.CheckBox _redactHCDataCheckBox;
        private System.Windows.Forms.Button _analyzeButton;
        private System.Windows.Forms.Label _label4;
        private System.Windows.Forms.ComboBox _resultsSelectionComboBox;
        private System.Windows.Forms.TabControl _reviewTabControl;
        private System.Windows.Forms.TabPage _statisticsTab;
        private System.Windows.Forms.TextBox _statisticsTextBox;
        private System.Windows.Forms.TabPage _fileListTab;
        private System.Windows.Forms.ComboBox _fileListSelectionComboBox;
        private System.Windows.Forms.Label _label5;
        private System.Windows.Forms.Label _label6;
        private System.Windows.Forms.ListBox _fileListListBox;
        private System.Windows.Forms.TextBox _fileListCountTextBox;
        private System.Windows.Forms.Label _label7;
    }
}

