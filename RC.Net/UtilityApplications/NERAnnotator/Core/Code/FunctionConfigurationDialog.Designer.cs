namespace Extract.UtilityApplications.NERAnnotation
{
    partial class FunctionConfigurationDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FunctionConfigurationDialog));
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._entityFilteringScriptFileBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._entityFilteringScriptFileTextBox = new System.Windows.Forms.TextBox();
            this._entityFilteringScriptFilePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this.label11 = new System.Windows.Forms.Label();
            this._runEntityFilteringFunctionsCheckBox = new System.Windows.Forms.CheckBox();
            this._preprocessingScriptFilePathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._preprocessingScriptFileTextBox = new System.Windows.Forms.TextBox();
            this._preprocessingScriptFilePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._preprocessingFunctionNameTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._preprocessingFunctionGroupBox = new System.Windows.Forms.GroupBox();
            this._runPreprocessingFunctionCheckBox = new System.Windows.Forms.CheckBox();
            this._entityFilteringFunctionsGroupBox = new System.Windows.Forms.GroupBox();
            this._runCharReplacingFunctionCheckBox = new System.Windows.Forms.CheckBox();
            this._charReplacingFunctionGroupBox = new System.Windows.Forms.GroupBox();
            this._charReplacingScriptFileTextBox = new System.Windows.Forms.TextBox();
            this._charReplacingScriptFileBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this.label3 = new System.Windows.Forms.Label();
            this._charReplacingScriptFilePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this.label4 = new System.Windows.Forms.Label();
            this._charReplacingFunctionNameTextBox = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this._runEntityFilteringFunctionsInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._runPreprocessingFunctionInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._runCharReplacingFunctionInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._preprocessingFunctionGroupBox.SuspendLayout();
            this._entityFilteringFunctionsGroupBox.SuspendLayout();
            this._charReplacingFunctionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(507, 326);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 6;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(588, 326);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 7;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _entityFilteringScriptFileBrowseButton
            // 
            this._entityFilteringScriptFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._entityFilteringScriptFileBrowseButton.EnsureFileExists = false;
            this._entityFilteringScriptFileBrowseButton.EnsurePathExists = false;
            this._entityFilteringScriptFileBrowseButton.Location = new System.Drawing.Point(625, 33);
            this._entityFilteringScriptFileBrowseButton.Name = "_entityFilteringScriptFileBrowseButton";
            this._entityFilteringScriptFileBrowseButton.Size = new System.Drawing.Size(24, 22);
            this._entityFilteringScriptFileBrowseButton.TabIndex = 3;
            this._entityFilteringScriptFileBrowseButton.Text = "...";
            this._entityFilteringScriptFileBrowseButton.TextControl = this._entityFilteringScriptFileTextBox;
            this._entityFilteringScriptFileBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _entityFilteringScriptFileTextBox
            // 
            this._entityFilteringScriptFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._entityFilteringScriptFileTextBox.Location = new System.Drawing.Point(9, 34);
            this._entityFilteringScriptFileTextBox.Name = "_entityFilteringScriptFileTextBox";
            this._entityFilteringScriptFileTextBox.Size = new System.Drawing.Size(586, 20);
            this._entityFilteringScriptFileTextBox.TabIndex = 1;
            this.toolTip1.SetToolTip(this._entityFilteringScriptFileTextBox, "Supported function names");
            // 
            // _entityFilteringScriptFilePathTagsButton
            // 
            this._entityFilteringScriptFilePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._entityFilteringScriptFilePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_entityFilteringScriptFilePathTagsButton.Image")));
            this._entityFilteringScriptFilePathTagsButton.Location = new System.Drawing.Point(601, 33);
            this._entityFilteringScriptFilePathTagsButton.Name = "_entityFilteringScriptFilePathTagsButton";
            this._entityFilteringScriptFilePathTagsButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._entityFilteringScriptFilePathTagsButton.Size = new System.Drawing.Size(18, 22);
            this._entityFilteringScriptFilePathTagsButton.TabIndex = 2;
            this._entityFilteringScriptFilePathTagsButton.TextControl = this._entityFilteringScriptFileTextBox;
            this._entityFilteringScriptFilePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 16);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(50, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Script file";
            // 
            // _runEntityFilteringFunctionsCheckBox
            // 
            this._runEntityFilteringFunctionsCheckBox.AutoSize = true;
            this._runEntityFilteringFunctionsCheckBox.Location = new System.Drawing.Point(8, 119);
            this._runEntityFilteringFunctionsCheckBox.Name = "_runEntityFilteringFunctionsCheckBox";
            this._runEntityFilteringFunctionsCheckBox.Size = new System.Drawing.Size(156, 17);
            this._runEntityFilteringFunctionsCheckBox.TabIndex = 2;
            this._runEntityFilteringFunctionsCheckBox.Text = "Run entity filtering functions";
            this.toolTip1.SetToolTip(this._runEntityFilteringFunctionsCheckBox, "Run various functions to filter/expand the expected data entities");
            this._runEntityFilteringFunctionsCheckBox.UseVisualStyleBackColor = true;
            this._runEntityFilteringFunctionsCheckBox.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
            // 
            // _preprocessingScriptFilePathBrowseButton
            // 
            this._preprocessingScriptFilePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._preprocessingScriptFilePathBrowseButton.EnsureFileExists = true;
            this._preprocessingScriptFilePathBrowseButton.EnsurePathExists = true;
            this._preprocessingScriptFilePathBrowseButton.Location = new System.Drawing.Point(412, 32);
            this._preprocessingScriptFilePathBrowseButton.Name = "_preprocessingScriptFilePathBrowseButton";
            this._preprocessingScriptFilePathBrowseButton.Size = new System.Drawing.Size(24, 22);
            this._preprocessingScriptFilePathBrowseButton.TabIndex = 3;
            this._preprocessingScriptFilePathBrowseButton.Text = "...";
            this._preprocessingScriptFilePathBrowseButton.TextControl = this._preprocessingScriptFileTextBox;
            this._preprocessingScriptFilePathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _preprocessingScriptFileTextBox
            // 
            this._preprocessingScriptFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._preprocessingScriptFileTextBox.Location = new System.Drawing.Point(9, 34);
            this._preprocessingScriptFileTextBox.Name = "_preprocessingScriptFileTextBox";
            this._preprocessingScriptFileTextBox.Size = new System.Drawing.Size(373, 20);
            this._preprocessingScriptFileTextBox.TabIndex = 1;
            this.toolTip1.SetToolTip(this._preprocessingScriptFileTextBox, "AFDocument -> AFDocument");
            // 
            // _preprocessingScriptFilePathTagsButton
            // 
            this._preprocessingScriptFilePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._preprocessingScriptFilePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_preprocessingScriptFilePathTagsButton.Image")));
            this._preprocessingScriptFilePathTagsButton.Location = new System.Drawing.Point(388, 32);
            this._preprocessingScriptFilePathTagsButton.Name = "_preprocessingScriptFilePathTagsButton";
            this._preprocessingScriptFilePathTagsButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._preprocessingScriptFilePathTagsButton.Size = new System.Drawing.Size(18, 22);
            this._preprocessingScriptFilePathTagsButton.TabIndex = 2;
            this._preprocessingScriptFilePathTagsButton.TextControl = this._preprocessingScriptFileTextBox;
            this._preprocessingScriptFilePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _preprocessingFunctionNameTextBox
            // 
            this._preprocessingFunctionNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._preprocessingFunctionNameTextBox.Location = new System.Drawing.Point(442, 34);
            this._preprocessingFunctionNameTextBox.Name = "_preprocessingFunctionNameTextBox";
            this._preprocessingFunctionNameTextBox.Size = new System.Drawing.Size(207, 20);
            this._preprocessingFunctionNameTextBox.TabIndex = 5;
            this.toolTip1.SetToolTip(this._preprocessingFunctionNameTextBox, "AFDocument -> AFDocument");
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(439, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Function name";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Script file";
            // 
            // _preprocessingFunctionGroupBox
            // 
            this._preprocessingFunctionGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._preprocessingFunctionGroupBox.Controls.Add(this._preprocessingScriptFileTextBox);
            this._preprocessingFunctionGroupBox.Controls.Add(this._preprocessingScriptFilePathBrowseButton);
            this._preprocessingFunctionGroupBox.Controls.Add(this.label1);
            this._preprocessingFunctionGroupBox.Controls.Add(this._preprocessingScriptFilePathTagsButton);
            this._preprocessingFunctionGroupBox.Controls.Add(this.label2);
            this._preprocessingFunctionGroupBox.Controls.Add(this._preprocessingFunctionNameTextBox);
            this._preprocessingFunctionGroupBox.Location = new System.Drawing.Point(8, 35);
            this._preprocessingFunctionGroupBox.Name = "_preprocessingFunctionGroupBox";
            this._preprocessingFunctionGroupBox.Size = new System.Drawing.Size(655, 66);
            this._preprocessingFunctionGroupBox.TabIndex = 1;
            this._preprocessingFunctionGroupBox.TabStop = false;
            this.toolTip1.SetToolTip(this._preprocessingFunctionGroupBox, "AFDocument -> AFDocument");
            // 
            // _runPreprocessingFunctionCheckBox
            // 
            this._runPreprocessingFunctionCheckBox.AutoSize = true;
            this._runPreprocessingFunctionCheckBox.Location = new System.Drawing.Point(8, 12);
            this._runPreprocessingFunctionCheckBox.Name = "_runPreprocessingFunctionCheckBox";
            this._runPreprocessingFunctionCheckBox.Size = new System.Drawing.Size(156, 17);
            this._runPreprocessingFunctionCheckBox.TabIndex = 0;
            this._runPreprocessingFunctionCheckBox.Text = "Run preprocessing function";
            this.toolTip1.SetToolTip(this._runPreprocessingFunctionCheckBox, "Run function to transform each page before doing any other processing");
            this._runPreprocessingFunctionCheckBox.UseVisualStyleBackColor = true;
            this._runPreprocessingFunctionCheckBox.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
            // 
            // _entityFilteringFunctionsGroupBox
            // 
            this._entityFilteringFunctionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._entityFilteringFunctionsGroupBox.Controls.Add(this.label11);
            this._entityFilteringFunctionsGroupBox.Controls.Add(this._entityFilteringScriptFileTextBox);
            this._entityFilteringFunctionsGroupBox.Controls.Add(this._entityFilteringScriptFilePathTagsButton);
            this._entityFilteringFunctionsGroupBox.Controls.Add(this._entityFilteringScriptFileBrowseButton);
            this._entityFilteringFunctionsGroupBox.Location = new System.Drawing.Point(8, 142);
            this._entityFilteringFunctionsGroupBox.Name = "_entityFilteringFunctionsGroupBox";
            this._entityFilteringFunctionsGroupBox.Size = new System.Drawing.Size(655, 64);
            this._entityFilteringFunctionsGroupBox.TabIndex = 3;
            this._entityFilteringFunctionsGroupBox.TabStop = false;
            // 
            // _runCharReplacingFunctionCheckBox
            // 
            this._runCharReplacingFunctionCheckBox.AutoSize = true;
            this._runCharReplacingFunctionCheckBox.Location = new System.Drawing.Point(8, 224);
            this._runCharReplacingFunctionCheckBox.Name = "_runCharReplacingFunctionCheckBox";
            this._runCharReplacingFunctionCheckBox.Size = new System.Drawing.Size(335, 17);
            this._runCharReplacingFunctionCheckBox.TabIndex = 4;
            this._runCharReplacingFunctionCheckBox.Text = "Run character replacing function (post-processing annotated text)";
            this.toolTip1.SetToolTip(this._runCharReplacingFunctionCheckBox, "Replace characters in the final, annotated page text (e.g., replace 0-9 with 9 to" +
        " help make model generic)");
            this._runCharReplacingFunctionCheckBox.UseVisualStyleBackColor = true;
            this._runCharReplacingFunctionCheckBox.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
            // 
            // _charReplacingFunctionGroupBox
            // 
            this._charReplacingFunctionGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._charReplacingFunctionGroupBox.Controls.Add(this._charReplacingScriptFileTextBox);
            this._charReplacingFunctionGroupBox.Controls.Add(this._charReplacingScriptFileBrowseButton);
            this._charReplacingFunctionGroupBox.Controls.Add(this.label3);
            this._charReplacingFunctionGroupBox.Controls.Add(this._charReplacingScriptFilePathTagsButton);
            this._charReplacingFunctionGroupBox.Controls.Add(this.label4);
            this._charReplacingFunctionGroupBox.Controls.Add(this._charReplacingFunctionNameTextBox);
            this._charReplacingFunctionGroupBox.Location = new System.Drawing.Point(8, 247);
            this._charReplacingFunctionGroupBox.Name = "_charReplacingFunctionGroupBox";
            this._charReplacingFunctionGroupBox.Size = new System.Drawing.Size(655, 66);
            this._charReplacingFunctionGroupBox.TabIndex = 5;
            this._charReplacingFunctionGroupBox.TabStop = false;
            // 
            // _charReplacingScriptFileTextBox
            // 
            this._charReplacingScriptFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._charReplacingScriptFileTextBox.Location = new System.Drawing.Point(9, 34);
            this._charReplacingScriptFileTextBox.Name = "_charReplacingScriptFileTextBox";
            this._charReplacingScriptFileTextBox.Size = new System.Drawing.Size(373, 20);
            this._charReplacingScriptFileTextBox.TabIndex = 1;
            this.toolTip1.SetToolTip(this._charReplacingScriptFileTextBox, "string -> string");
            // 
            // _charReplacingScriptFileBrowseButton
            // 
            this._charReplacingScriptFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._charReplacingScriptFileBrowseButton.EnsureFileExists = true;
            this._charReplacingScriptFileBrowseButton.EnsurePathExists = true;
            this._charReplacingScriptFileBrowseButton.Location = new System.Drawing.Point(412, 32);
            this._charReplacingScriptFileBrowseButton.Name = "_charReplacingScriptFileBrowseButton";
            this._charReplacingScriptFileBrowseButton.Size = new System.Drawing.Size(24, 22);
            this._charReplacingScriptFileBrowseButton.TabIndex = 3;
            this._charReplacingScriptFileBrowseButton.Text = "...";
            this._charReplacingScriptFileBrowseButton.TextControl = this._charReplacingScriptFileTextBox;
            this._charReplacingScriptFileBrowseButton.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Script file";
            // 
            // _charReplacingScriptFilePathTagsButton
            // 
            this._charReplacingScriptFilePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._charReplacingScriptFilePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_charReplacingScriptFilePathTagsButton.Image")));
            this._charReplacingScriptFilePathTagsButton.Location = new System.Drawing.Point(388, 32);
            this._charReplacingScriptFilePathTagsButton.Name = "_charReplacingScriptFilePathTagsButton";
            this._charReplacingScriptFilePathTagsButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._charReplacingScriptFilePathTagsButton.Size = new System.Drawing.Size(18, 22);
            this._charReplacingScriptFilePathTagsButton.TabIndex = 2;
            this._charReplacingScriptFilePathTagsButton.TextControl = this._charReplacingScriptFileTextBox;
            this._charReplacingScriptFilePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(439, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Function name";
            // 
            // _charReplacingFunctionNameTextBox
            // 
            this._charReplacingFunctionNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._charReplacingFunctionNameTextBox.Location = new System.Drawing.Point(442, 34);
            this._charReplacingFunctionNameTextBox.Name = "_charReplacingFunctionNameTextBox";
            this._charReplacingFunctionNameTextBox.Size = new System.Drawing.Size(207, 20);
            this._charReplacingFunctionNameTextBox.TabIndex = 5;
            this.toolTip1.SetToolTip(this._charReplacingFunctionNameTextBox, "string -> string");
            // 
            // _runEntityFilteringFunctionsInfoTip
            // 
            this._runEntityFilteringFunctionsInfoTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._runEntityFilteringFunctionsInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._runEntityFilteringFunctionsInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_runEntityFilteringFunctionsInfoTip.BackgroundImage")));
            this._runEntityFilteringFunctionsInfoTip.Location = new System.Drawing.Point(647, 119);
            this._runEntityFilteringFunctionsInfoTip.Name = "_runEntityFilteringFunctionsInfoTip";
            this._runEntityFilteringFunctionsInfoTip.Size = new System.Drawing.Size(16, 16);
            this._runEntityFilteringFunctionsInfoTip.TabIndex = 4;
            this._runEntityFilteringFunctionsInfoTip.TipText = null;
            // 
            // _runPreprocessingFunctionInfoTip
            // 
            this._runPreprocessingFunctionInfoTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._runPreprocessingFunctionInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._runPreprocessingFunctionInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_runPreprocessingFunctionInfoTip.BackgroundImage")));
            this._runPreprocessingFunctionInfoTip.Location = new System.Drawing.Point(647, 12);
            this._runPreprocessingFunctionInfoTip.Name = "_runPreprocessingFunctionInfoTip";
            this._runPreprocessingFunctionInfoTip.Size = new System.Drawing.Size(16, 16);
            this._runPreprocessingFunctionInfoTip.TabIndex = 8;
            this._runPreprocessingFunctionInfoTip.TipText = null;
            this.toolTip1.SetToolTip(this._runPreprocessingFunctionInfoTip, "Run function to transform each page before doing any other processing");
            // 
            // _runCharReplacingFunctionInfoTip
            // 
            this._runCharReplacingFunctionInfoTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._runCharReplacingFunctionInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._runCharReplacingFunctionInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_runCharReplacingFunctionInfoTip.BackgroundImage")));
            this._runCharReplacingFunctionInfoTip.Location = new System.Drawing.Point(647, 224);
            this._runCharReplacingFunctionInfoTip.Name = "_runCharReplacingFunctionInfoTip";
            this._runCharReplacingFunctionInfoTip.Size = new System.Drawing.Size(16, 16);
            this._runCharReplacingFunctionInfoTip.TabIndex = 9;
            this._runCharReplacingFunctionInfoTip.TipText = null;
            this.toolTip1.SetToolTip(this._runCharReplacingFunctionInfoTip, "Replace characters in the final, annotated page text (e.g., replace 0-9 with 9 to" +
        " help make model generic)");
            // 
            // FunctionConfigurationDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(675, 361);
            this.ControlBox = false;
            this.Controls.Add(this._runCharReplacingFunctionInfoTip);
            this.Controls.Add(this._runEntityFilteringFunctionsInfoTip);
            this.Controls.Add(this._runPreprocessingFunctionInfoTip);
            this.Controls.Add(this._runCharReplacingFunctionCheckBox);
            this.Controls.Add(this._charReplacingFunctionGroupBox);
            this.Controls.Add(this._entityFilteringFunctionsGroupBox);
            this.Controls.Add(this._runPreprocessingFunctionCheckBox);
            this.Controls.Add(this._preprocessingFunctionGroupBox);
            this.Controls.Add(this._runEntityFilteringFunctionsCheckBox);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.MaximumSize = new System.Drawing.Size(1000, 400);
            this.MinimumSize = new System.Drawing.Size(632, 400);
            this.Name = "FunctionConfigurationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Function configuration";
            this._preprocessingFunctionGroupBox.ResumeLayout(false);
            this._preprocessingFunctionGroupBox.PerformLayout();
            this._entityFilteringFunctionsGroupBox.ResumeLayout(false);
            this._entityFilteringFunctionsGroupBox.PerformLayout();
            this._charReplacingFunctionGroupBox.ResumeLayout(false);
            this._charReplacingFunctionGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private Extract.Utilities.Forms.BrowseButton _entityFilteringScriptFileBrowseButton;
        private System.Windows.Forms.TextBox _entityFilteringScriptFileTextBox;
        private Extract.Utilities.Forms.PathTagsButton _entityFilteringScriptFilePathTagsButton;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox _runEntityFilteringFunctionsCheckBox;
        private Extract.Utilities.Forms.BrowseButton _preprocessingScriptFilePathBrowseButton;
        private System.Windows.Forms.TextBox _preprocessingScriptFileTextBox;
        private Extract.Utilities.Forms.PathTagsButton _preprocessingScriptFilePathTagsButton;
        private System.Windows.Forms.TextBox _preprocessingFunctionNameTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox _preprocessingFunctionGroupBox;
        private System.Windows.Forms.CheckBox _runPreprocessingFunctionCheckBox;
        private System.Windows.Forms.GroupBox _entityFilteringFunctionsGroupBox;
        private System.Windows.Forms.CheckBox _runCharReplacingFunctionCheckBox;
        private System.Windows.Forms.GroupBox _charReplacingFunctionGroupBox;
        private System.Windows.Forms.TextBox _charReplacingScriptFileTextBox;
        private Extract.Utilities.Forms.BrowseButton _charReplacingScriptFileBrowseButton;
        private System.Windows.Forms.Label label3;
        private Extract.Utilities.Forms.PathTagsButton _charReplacingScriptFilePathTagsButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox _charReplacingFunctionNameTextBox;
        private System.Windows.Forms.ToolTip toolTip1;
        private Utilities.Forms.InfoTip _runPreprocessingFunctionInfoTip;
        private Utilities.Forms.InfoTip _runEntityFilteringFunctionsInfoTip;
        private Utilities.Forms.InfoTip _runCharReplacingFunctionInfoTip;
    }
}