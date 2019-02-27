namespace Extract.AttributeFinder.Rules
{
    partial class MicrFinderSettingsDialog
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.GroupBox groupBox1;
            Extract.Utilities.Forms.InfoTip infoTip4;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MicrFinderSettingsDialog));
            System.Windows.Forms.Label label4;
            Extract.Utilities.Forms.InfoTip infoTip3;
            Extract.Utilities.Forms.InfoTip infoTip1;
            Extract.Utilities.Forms.InfoTip infoTip2;
            this._filterCharsWhenSplittingCheckBox = new System.Windows.Forms.CheckBox();
            this.fileActionManagerPathTagButton1 = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._micrSplitterRegexTextBox = new System.Windows.Forms.TextBox();
            this._micrSplitterRegexBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._splitAccountCheckBox = new System.Windows.Forms.CheckBox();
            this._splitAmountCheckBox = new System.Windows.Forms.CheckBox();
            this._splitRoutingCheckBox = new System.Windows.Forms.CheckBox();
            this._splitCheckCheckBox = new System.Windows.Forms.CheckBox();
            this._lowConfidenceLabel1 = new System.Windows.Forms.Label();
            this._lowConfidenceLabel2 = new System.Windows.Forms.Label();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._highConfidenceUpDown = new System.Windows.Forms.NumericUpDown();
            this._lowConfidenceUpDown = new System.Windows.Forms.NumericUpDown();
            this._lowConfidenceCheckBox = new System.Windows.Forms.CheckBox();
            this._filterRegExTextBox = new System.Windows.Forms.TextBox();
            this._regexFilterFileNamePathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._regexFilterFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._configFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._micrSplitterTextBox = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.browseButton1 = new Extract.Utilities.Forms.BrowseButton();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            infoTip4 = new Extract.Utilities.Forms.InfoTip();
            label4 = new System.Windows.Forms.Label();
            infoTip3 = new Extract.Utilities.Forms.InfoTip();
            infoTip1 = new Extract.Utilities.Forms.InfoTip();
            infoTip2 = new Extract.Utilities.Forms.InfoTip();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._highConfidenceUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._lowConfidenceUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(29, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(234, 13);
            label1.TabIndex = 0;
            label1.Text = "Select all zones with a confidence of at least";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(324, 14);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(49, 13);
            label2.TabIndex = 2;
            label2.Text = "percent.";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 96);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(457, 13);
            label3.TabIndex = 8;
            label3.Text = "Discard any zone that does not match on the following regular expression (if spec" +
    "ified):";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._filterCharsWhenSplittingCheckBox);
            groupBox1.Controls.Add(infoTip4);
            groupBox1.Controls.Add(this.fileActionManagerPathTagButton1);
            groupBox1.Controls.Add(this._micrSplitterRegexBrowseButton);
            groupBox1.Controls.Add(this._micrSplitterRegexTextBox);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(infoTip3);
            groupBox1.Controls.Add(this._splitAccountCheckBox);
            groupBox1.Controls.Add(this._splitAmountCheckBox);
            groupBox1.Controls.Add(this._splitRoutingCheckBox);
            groupBox1.Controls.Add(this._splitCheckCheckBox);
            groupBox1.Location = new System.Drawing.Point(12, 140);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(567, 194);
            groupBox1.TabIndex = 13;
            groupBox1.TabStop = false;
            groupBox1.Text = "Split found MICR text into the following sub-attributes";
            // 
            // _filterCharsWhenSplittingCheckBox
            // 
            this._filterCharsWhenSplittingCheckBox.AutoSize = true;
            this._filterCharsWhenSplittingCheckBox.Location = new System.Drawing.Point(15, 163);
            this._filterCharsWhenSplittingCheckBox.Name = "_filterCharsWhenSplittingCheckBox";
            this._filterCharsWhenSplittingCheckBox.Size = new System.Drawing.Size(357, 17);
            this._filterCharsWhenSplittingCheckBox.TabIndex = 10;
            this._filterCharsWhenSplittingCheckBox.Text = "Remove special MICR chars and spaces from sub-attribute values";
            this._filterCharsWhenSplittingCheckBox.UseVisualStyleBackColor = true;
            // 
            // infoTip4
            // 
            infoTip4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip4.BackColor = System.Drawing.Color.Transparent;
            infoTip4.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip4.BackgroundImage")));
            infoTip4.Location = new System.Drawing.Point(546, 112);
            infoTip4.Name = "infoTip4";
            infoTip4.Size = new System.Drawing.Size(16, 16);
            infoTip4.TabIndex = 6;
            infoTip4.TipText = resources.GetString("infoTip4.TipText");
            // 
            // fileActionManagerPathTagButton1
            // 
            this.fileActionManagerPathTagButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fileActionManagerPathTagButton1.Image = ((System.Drawing.Image)(resources.GetObject("fileActionManagerPathTagButton1.Image")));
            this.fileActionManagerPathTagButton1.Location = new System.Drawing.Point(510, 131);
            this.fileActionManagerPathTagButton1.Name = "fileActionManagerPathTagButton1";
            this.fileActionManagerPathTagButton1.Size = new System.Drawing.Size(18, 20);
            this.fileActionManagerPathTagButton1.TabIndex = 8;
            this.fileActionManagerPathTagButton1.TextControl = this._micrSplitterRegexTextBox;
            this.fileActionManagerPathTagButton1.UseVisualStyleBackColor = true;
            // 
            // _micrSplitterRegexTextBox
            // 
            this._micrSplitterRegexTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._micrSplitterRegexTextBox.Location = new System.Drawing.Point(15, 131);
            this._micrSplitterRegexTextBox.Name = "_micrSplitterRegexTextBox";
            this._micrSplitterRegexTextBox.Size = new System.Drawing.Size(489, 22);
            this._micrSplitterRegexTextBox.TabIndex = 7;
            // 
            // _micrSplitterRegexBrowseButton
            // 
            this._micrSplitterRegexBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._micrSplitterRegexBrowseButton.EnsureFileExists = false;
            this._micrSplitterRegexBrowseButton.EnsurePathExists = false;
            this._micrSplitterRegexBrowseButton.Location = new System.Drawing.Point(534, 131);
            this._micrSplitterRegexBrowseButton.Name = "_micrSplitterRegexBrowseButton";
            this._micrSplitterRegexBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._micrSplitterRegexBrowseButton.TabIndex = 9;
            this._micrSplitterRegexBrowseButton.Text = "...";
            this._micrSplitterRegexBrowseButton.TextControl = this._micrSplitterRegexTextBox;
            this._micrSplitterRegexBrowseButton.UseVisualStyleBackColor = true;
            this._micrSplitterRegexBrowseButton.PathSelected += new System.EventHandler<Extract.Utilities.Forms.PathSelectedEventArgs>(this.HandleMicrSplitterRegexBrowseButton_PathSelected);
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(12, 115);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(297, 13);
            label4.TabIndex = 5;
            label4.Text = "Parse MICR lines using the following regular expression:";
            // 
            // infoTip3
            // 
            infoTip3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip3.BackColor = System.Drawing.Color.Transparent;
            infoTip3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip3.BackgroundImage")));
            infoTip3.Location = new System.Drawing.Point(545, 14);
            infoTip3.Name = "infoTip3";
            infoTip3.Size = new System.Drawing.Size(16, 16);
            infoTip3.TabIndex = 1;
            infoTip3.TipText = resources.GetString("infoTip3.TipText");
            // 
            // _splitAccountCheckBox
            // 
            this._splitAccountCheckBox.AutoSize = true;
            this._splitAccountCheckBox.Location = new System.Drawing.Point(15, 46);
            this._splitAccountCheckBox.Name = "_splitAccountCheckBox";
            this._splitAccountCheckBox.Size = new System.Drawing.Size(111, 17);
            this._splitAccountCheckBox.TabIndex = 2;
            this._splitAccountCheckBox.Text = "Account number";
            this._splitAccountCheckBox.UseVisualStyleBackColor = true;
            // 
            // _splitAmountCheckBox
            // 
            this._splitAmountCheckBox.AutoSize = true;
            this._splitAmountCheckBox.Location = new System.Drawing.Point(15, 92);
            this._splitAmountCheckBox.Name = "_splitAmountCheckBox";
            this._splitAmountCheckBox.Size = new System.Drawing.Size(67, 17);
            this._splitAmountCheckBox.TabIndex = 4;
            this._splitAmountCheckBox.Text = "Amount";
            this._splitAmountCheckBox.UseVisualStyleBackColor = true;
            // 
            // _splitRoutingCheckBox
            // 
            this._splitRoutingCheckBox.AutoSize = true;
            this._splitRoutingCheckBox.Location = new System.Drawing.Point(15, 23);
            this._splitRoutingCheckBox.Name = "_splitRoutingCheckBox";
            this._splitRoutingCheckBox.Size = new System.Drawing.Size(111, 17);
            this._splitRoutingCheckBox.TabIndex = 0;
            this._splitRoutingCheckBox.Text = "Routing number";
            this._splitRoutingCheckBox.UseVisualStyleBackColor = true;
            // 
            // _splitCheckCheckBox
            // 
            this._splitCheckCheckBox.AutoSize = true;
            this._splitCheckCheckBox.Location = new System.Drawing.Point(15, 69);
            this._splitCheckCheckBox.Name = "_splitCheckCheckBox";
            this._splitCheckCheckBox.Size = new System.Drawing.Size(100, 17);
            this._splitCheckCheckBox.TabIndex = 3;
            this._splitCheckCheckBox.Text = "Check number";
            this._splitCheckCheckBox.UseVisualStyleBackColor = true;
            // 
            // infoTip1
            // 
            infoTip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip1.BackColor = System.Drawing.Color.Transparent;
            infoTip1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip1.BackgroundImage")));
            infoTip1.Location = new System.Drawing.Point(558, 92);
            infoTip1.Name = "infoTip1";
            infoTip1.Size = new System.Drawing.Size(16, 16);
            infoTip1.TabIndex = 9;
            infoTip1.TipText = resources.GetString("infoTip1.TipText");
            // 
            // infoTip2
            // 
            infoTip2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip2.BackColor = System.Drawing.Color.Transparent;
            infoTip2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip2.BackgroundImage")));
            infoTip2.Location = new System.Drawing.Point(558, 44);
            infoTip2.Name = "infoTip2";
            infoTip2.Size = new System.Drawing.Size(16, 16);
            infoTip2.TabIndex = 6;
            infoTip2.TipText = "MICR confidence will be compared to uss file data from same area on page.";
            // 
            // _lowConfidenceLabel1
            // 
            this._lowConfidenceLabel1.AutoSize = true;
            this._lowConfidenceLabel1.Location = new System.Drawing.Point(337, 44);
            this._lowConfidenceLabel1.Name = "_lowConfidenceLabel1";
            this._lowConfidenceLabel1.Size = new System.Drawing.Size(101, 13);
            this._lowConfidenceLabel1.TabIndex = 5;
            this._lowConfidenceLabel1.Text = "percent as long as";
            // 
            // _lowConfidenceLabel2
            // 
            this._lowConfidenceLabel2.AutoSize = true;
            this._lowConfidenceLabel2.Location = new System.Drawing.Point(29, 66);
            this._lowConfidenceLabel2.Name = "_lowConfidenceLabel2";
            this._lowConfidenceLabel2.Size = new System.Drawing.Size(313, 13);
            this._lowConfidenceLabel2.TabIndex = 7;
            this._lowConfidenceLabel2.Text = "the text also has a higher confidence than the default OCR.";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(423, 352);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 14;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(504, 352);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 15;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _highConfidenceUpDown
            // 
            this._highConfidenceUpDown.Location = new System.Drawing.Point(267, 12);
            this._highConfidenceUpDown.Name = "_highConfidenceUpDown";
            this._highConfidenceUpDown.Size = new System.Drawing.Size(51, 22);
            this._highConfidenceUpDown.TabIndex = 1;
            // 
            // _lowConfidenceUpDown
            // 
            this._lowConfidenceUpDown.Location = new System.Drawing.Point(280, 43);
            this._lowConfidenceUpDown.Name = "_lowConfidenceUpDown";
            this._lowConfidenceUpDown.Size = new System.Drawing.Size(51, 22);
            this._lowConfidenceUpDown.TabIndex = 4;
            // 
            // _lowConfidenceCheckBox
            // 
            this._lowConfidenceCheckBox.AutoSize = true;
            this._lowConfidenceCheckBox.Location = new System.Drawing.Point(12, 43);
            this._lowConfidenceCheckBox.Name = "_lowConfidenceCheckBox";
            this._lowConfidenceCheckBox.Size = new System.Drawing.Size(262, 17);
            this._lowConfidenceCheckBox.TabIndex = 3;
            this._lowConfidenceCheckBox.Text = "Also select zones with a confidence of at least";
            this._lowConfidenceCheckBox.UseVisualStyleBackColor = true;
            this._lowConfidenceCheckBox.Click += new System.EventHandler(this.HandleLowConfidenceCheckBox_Click);
            // 
            // _filterRegExTextBox
            // 
            this._filterRegExTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._filterRegExTextBox.Location = new System.Drawing.Point(15, 112);
            this._filterRegExTextBox.Name = "_filterRegExTextBox";
            this._filterRegExTextBox.Size = new System.Drawing.Size(507, 22);
            this._filterRegExTextBox.TabIndex = 10;
            // 
            // _regexFilterFileNamePathTagsButton
            // 
            this._regexFilterFileNamePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._regexFilterFileNamePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_regexFilterFileNamePathTagsButton.Image")));
            this._regexFilterFileNamePathTagsButton.Location = new System.Drawing.Point(528, 113);
            this._regexFilterFileNamePathTagsButton.Name = "_regexFilterFileNamePathTagsButton";
            this._regexFilterFileNamePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._regexFilterFileNamePathTagsButton.TabIndex = 11;
            this._regexFilterFileNamePathTagsButton.TextControl = this._filterRegExTextBox;
            this._regexFilterFileNamePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _regexFilterFileNameBrowseButton
            // 
            this._regexFilterFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._regexFilterFileNameBrowseButton.EnsureFileExists = false;
            this._regexFilterFileNameBrowseButton.EnsurePathExists = false;
            this._regexFilterFileNameBrowseButton.Location = new System.Drawing.Point(552, 113);
            this._regexFilterFileNameBrowseButton.Name = "_regexFilterFileNameBrowseButton";
            this._regexFilterFileNameBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._regexFilterFileNameBrowseButton.TabIndex = 12;
            this._regexFilterFileNameBrowseButton.Text = "...";
            this._regexFilterFileNameBrowseButton.TextControl = this._filterRegExTextBox;
            this._regexFilterFileNameBrowseButton.UseVisualStyleBackColor = true;
            this._regexFilterFileNameBrowseButton.PathSelected += new System.EventHandler<Extract.Utilities.Forms.PathSelectedEventArgs>(this.HandlRegexFilterFileNameBrowseButton_PathSelected);
            // 
            // _configFileNameBrowseButton
            // 
            this._configFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._configFileNameBrowseButton.EnsureFileExists = false;
            this._configFileNameBrowseButton.EnsurePathExists = false;
            this._configFileNameBrowseButton.Location = new System.Drawing.Point(490, 114);
            this._configFileNameBrowseButton.Name = "_configFileNameBrowseButton";
            this._configFileNameBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._configFileNameBrowseButton.TabIndex = 16;
            this._configFileNameBrowseButton.Text = "...";
            this._configFileNameBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _micrSplitterTextBox
            // 
            this._micrSplitterTextBox.Location = new System.Drawing.Point(15, 131);
            this._micrSplitterTextBox.Name = "_micrSplitterTextBox";
            this._micrSplitterTextBox.Size = new System.Drawing.Size(427, 20);
            this._micrSplitterTextBox.TabIndex = 18;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(15, 163);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(357, 17);
            this.checkBox1.TabIndex = 17;
            this.checkBox1.Text = "Remove special MICR chars and spaces from sub-attribute values";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // browseButton1
            // 
            this.browseButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton1.EnsureFileExists = false;
            this.browseButton1.EnsurePathExists = false;
            this.browseButton1.Location = new System.Drawing.Point(534, 131);
            this.browseButton1.Name = "browseButton1";
            this.browseButton1.Size = new System.Drawing.Size(27, 20);
            this.browseButton1.TabIndex = 9;
            this.browseButton1.Text = "...";
            this.browseButton1.TextControl = this._micrSplitterRegexTextBox;
            this.browseButton1.UseVisualStyleBackColor = true;
            this.browseButton1.PathSelected += new System.EventHandler<Extract.Utilities.Forms.PathSelectedEventArgs>(this.HandleMicrSplitterRegexBrowseButton_PathSelected);
            // 
            // MicrFinderSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(591, 387);
            this.Controls.Add(this._regexFilterFileNamePathTagsButton);
            this.Controls.Add(this._regexFilterFileNameBrowseButton);
            this.Controls.Add(this._filterRegExTextBox);
            this.Controls.Add(infoTip2);
            this.Controls.Add(infoTip1);
            this.Controls.Add(groupBox1);
            this.Controls.Add(label3);
            this.Controls.Add(this._lowConfidenceCheckBox);
            this.Controls.Add(this._lowConfidenceLabel1);
            this.Controls.Add(this._lowConfidenceUpDown);
            this.Controls.Add(this._lowConfidenceLabel2);
            this.Controls.Add(label2);
            this.Controls.Add(this._highConfidenceUpDown);
            this.Controls.Add(label1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MicrFinderSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MICR finder settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._highConfidenceUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._lowConfidenceUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.NumericUpDown _highConfidenceUpDown;
        private System.Windows.Forms.NumericUpDown _lowConfidenceUpDown;
        private System.Windows.Forms.CheckBox _lowConfidenceCheckBox;
        private System.Windows.Forms.Label _lowConfidenceLabel2;
        private System.Windows.Forms.Label _lowConfidenceLabel1;
        private System.Windows.Forms.CheckBox _splitRoutingCheckBox;
        private System.Windows.Forms.CheckBox _splitAccountCheckBox;
        private System.Windows.Forms.CheckBox _splitCheckCheckBox;
        private System.Windows.Forms.CheckBox _splitAmountCheckBox;
        private System.Windows.Forms.TextBox _filterRegExTextBox;
        private FileActionManager.Forms.FileActionManagerPathTagButton _regexFilterFileNamePathTagsButton;
        private Utilities.Forms.BrowseButton _regexFilterFileNameBrowseButton;
        private Utilities.Forms.BrowseButton _configFileNameBrowseButton;
        private FileActionManager.Forms.FileActionManagerPathTagButton fileActionManagerPathTagButton1;
        private System.Windows.Forms.TextBox _micrSplitterRegexTextBox;
        private Utilities.Forms.BrowseButton _micrSplitterRegexBrowseButton;
        private System.Windows.Forms.TextBox _micrSplitterTextBox;
        private System.Windows.Forms.CheckBox _filterCharsWhenSplittingCheckBox;
        private System.Windows.Forms.CheckBox checkBox1;
        private Utilities.Forms.BrowseButton browseButton1;
    }
}