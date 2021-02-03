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
            System.Windows.Forms.GroupBox groupBox1;
            Extract.Utilities.Forms.InfoTip infoTip4;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MicrFinderSettingsDialog));
            System.Windows.Forms.Label label4;
            Extract.Utilities.Forms.InfoTip infoTip3;
            Extract.Utilities.Forms.InfoTip infoTip2;
            Extract.Utilities.Forms.InfoTip infoTip1;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label1;
            this._filterCharsWhenSplittingCheckBox = new System.Windows.Forms.CheckBox();
            this._micrSplitterRegexPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._micrSplitterRegexTextBox = new System.Windows.Forms.TextBox();
            this._micrSplitterRegexBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._splitAccountCheckBox = new System.Windows.Forms.CheckBox();
            this._splitAmountCheckBox = new System.Windows.Forms.CheckBox();
            this._splitRoutingCheckBox = new System.Windows.Forms.CheckBox();
            this._splitCheckCheckBox = new System.Windows.Forms.CheckBox();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._configFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._micrSplitterTextBox = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.browseButton1 = new Extract.Utilities.Forms.BrowseButton();
            this._inheritOCRParametersCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._regexFilterFileNamePathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._filterRegExTextBox = new System.Windows.Forms.TextBox();
            this._regexFilterFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._lowConfidenceCheckBox = new System.Windows.Forms.CheckBox();
            this._lowConfidenceLabel1 = new System.Windows.Forms.Label();
            this._lowConfidenceUpDown = new System.Windows.Forms.NumericUpDown();
            this._lowConfidenceLabel2 = new System.Windows.Forms.Label();
            this._highConfidenceUpDown = new System.Windows.Forms.NumericUpDown();
            this._returnUnrecognizedCharactersCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._engineGdPictureRadioButton = new System.Windows.Forms.RadioButton();
            this._engineKofaxRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this._searchPagesWithTextRadioButton = new System.Windows.Forms.RadioButton();
            this._searchAllPagesRadioButton = new System.Windows.Forms.RadioButton();
            groupBox1 = new System.Windows.Forms.GroupBox();
            infoTip4 = new Extract.Utilities.Forms.InfoTip();
            label4 = new System.Windows.Forms.Label();
            infoTip3 = new Extract.Utilities.Forms.InfoTip();
            infoTip2 = new Extract.Utilities.Forms.InfoTip();
            infoTip1 = new Extract.Utilities.Forms.InfoTip();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._lowConfidenceUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._highConfidenceUpDown)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._filterCharsWhenSplittingCheckBox);
            groupBox1.Controls.Add(infoTip4);
            groupBox1.Controls.Add(this._micrSplitterRegexPathTagButton);
            groupBox1.Controls.Add(this._micrSplitterRegexBrowseButton);
            groupBox1.Controls.Add(this._micrSplitterRegexTextBox);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(infoTip3);
            groupBox1.Controls.Add(this._splitAccountCheckBox);
            groupBox1.Controls.Add(this._splitAmountCheckBox);
            groupBox1.Controls.Add(this._splitRoutingCheckBox);
            groupBox1.Controls.Add(this._splitCheckCheckBox);
            groupBox1.Location = new System.Drawing.Point(11, 306);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(578, 194);
            groupBox1.TabIndex = 4;
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
            infoTip4.Location = new System.Drawing.Point(557, 112);
            infoTip4.Name = "infoTip4";
            infoTip4.Size = new System.Drawing.Size(16, 16);
            infoTip4.TabIndex = 6;
            infoTip4.TipText = resources.GetString("infoTip4.TipText");
            // 
            // _micrSplitterRegexPathTagButton
            // 
            this._micrSplitterRegexPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._micrSplitterRegexPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_micrSplitterRegexPathTagButton.Image")));
            this._micrSplitterRegexPathTagButton.Location = new System.Drawing.Point(521, 131);
            this._micrSplitterRegexPathTagButton.Name = "_micrSplitterRegexPathTagButton";
            this._micrSplitterRegexPathTagButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._micrSplitterRegexPathTagButton.Size = new System.Drawing.Size(18, 20);
            this._micrSplitterRegexPathTagButton.TabIndex = 8;
            this._micrSplitterRegexPathTagButton.TextControl = this._micrSplitterRegexTextBox;
            this._micrSplitterRegexPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _micrSplitterRegexTextBox
            // 
            this._micrSplitterRegexTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._micrSplitterRegexTextBox.Location = new System.Drawing.Point(15, 131);
            this._micrSplitterRegexTextBox.Name = "_micrSplitterRegexTextBox";
            this._micrSplitterRegexTextBox.Size = new System.Drawing.Size(500, 22);
            this._micrSplitterRegexTextBox.TabIndex = 7;
            // 
            // _micrSplitterRegexBrowseButton
            // 
            this._micrSplitterRegexBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._micrSplitterRegexBrowseButton.EnsureFileExists = false;
            this._micrSplitterRegexBrowseButton.EnsurePathExists = false;
            this._micrSplitterRegexBrowseButton.Location = new System.Drawing.Point(545, 131);
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
            infoTip3.Location = new System.Drawing.Point(556, 14);
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
            // infoTip2
            // 
            infoTip2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip2.BackColor = System.Drawing.Color.Transparent;
            infoTip2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip2.BackgroundImage")));
            infoTip2.Location = new System.Drawing.Point(464, 54);
            infoTip2.Name = "infoTip2";
            infoTip2.Size = new System.Drawing.Size(16, 16);
            infoTip2.TabIndex = 19;
            infoTip2.TipText = "MICR confidence will be compared to uss file data from same area on page.";
            // 
            // infoTip1
            // 
            infoTip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip1.BackColor = System.Drawing.Color.Transparent;
            infoTip1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip1.BackgroundImage")));
            infoTip1.Location = new System.Drawing.Point(481, 105);
            infoTip1.Name = "infoTip1";
            infoTip1.Size = new System.Drawing.Size(16, 16);
            infoTip1.TabIndex = 22;
            infoTip1.TipText = resources.GetString("infoTip1.TipText");
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(6, 111);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(457, 13);
            label3.TabIndex = 21;
            label3.Text = "Discard any zone that does not match on the following regular expression (if spec" +
    "ified):";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(323, 18);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(49, 13);
            label2.TabIndex = 15;
            label2.Text = "percent.";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 18);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(234, 13);
            label1.TabIndex = 13;
            label1.Text = "Select all zones with a confidence of at least";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(434, 539);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 6;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(515, 539);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 7;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
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
            // _inheritOCRParametersCheckBox
            // 
            this._inheritOCRParametersCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._inheritOCRParametersCheckBox.AutoSize = true;
            this._inheritOCRParametersCheckBox.Location = new System.Drawing.Point(26, 506);
            this._inheritOCRParametersCheckBox.Name = "_inheritOCRParametersCheckBox";
            this._inheritOCRParametersCheckBox.Size = new System.Drawing.Size(204, 17);
            this._inheritOCRParametersCheckBox.TabIndex = 5;
            this._inheritOCRParametersCheckBox.Text = "Inherit OCR parameters (advanced)";
            this._inheritOCRParametersCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this._regexFilterFileNamePathTagsButton);
            this.groupBox2.Controls.Add(this._regexFilterFileNameBrowseButton);
            this.groupBox2.Controls.Add(this._filterRegExTextBox);
            this.groupBox2.Controls.Add(infoTip2);
            this.groupBox2.Controls.Add(infoTip1);
            this.groupBox2.Controls.Add(label3);
            this.groupBox2.Controls.Add(this._lowConfidenceCheckBox);
            this.groupBox2.Controls.Add(this._lowConfidenceLabel1);
            this.groupBox2.Controls.Add(this._lowConfidenceUpDown);
            this.groupBox2.Controls.Add(this._lowConfidenceLabel2);
            this.groupBox2.Controls.Add(label2);
            this.groupBox2.Controls.Add(this._highConfidenceUpDown);
            this.groupBox2.Controls.Add(label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 139);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(577, 161);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Filter";
            // 
            // _regexFilterFileNamePathTagsButton
            // 
            this._regexFilterFileNamePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._regexFilterFileNamePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_regexFilterFileNamePathTagsButton.Image")));
            this._regexFilterFileNamePathTagsButton.Location = new System.Drawing.Point(520, 127);
            this._regexFilterFileNamePathTagsButton.Name = "_regexFilterFileNamePathTagsButton";
            this._regexFilterFileNamePathTagsButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._regexFilterFileNamePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._regexFilterFileNamePathTagsButton.TabIndex = 24;
            this._regexFilterFileNamePathTagsButton.TextControl = this._filterRegExTextBox;
            this._regexFilterFileNamePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _filterRegExTextBox
            // 
            this._filterRegExTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._filterRegExTextBox.Location = new System.Drawing.Point(6, 127);
            this._filterRegExTextBox.Name = "_filterRegExTextBox";
            this._filterRegExTextBox.Size = new System.Drawing.Size(509, 22);
            this._filterRegExTextBox.TabIndex = 23;
            // 
            // _regexFilterFileNameBrowseButton
            // 
            this._regexFilterFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._regexFilterFileNameBrowseButton.EnsureFileExists = false;
            this._regexFilterFileNameBrowseButton.EnsurePathExists = false;
            this._regexFilterFileNameBrowseButton.Location = new System.Drawing.Point(544, 127);
            this._regexFilterFileNameBrowseButton.Name = "_regexFilterFileNameBrowseButton";
            this._regexFilterFileNameBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._regexFilterFileNameBrowseButton.TabIndex = 25;
            this._regexFilterFileNameBrowseButton.Text = "...";
            this._regexFilterFileNameBrowseButton.TextControl = this._filterRegExTextBox;
            this._regexFilterFileNameBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _lowConfidenceCheckBox
            // 
            this._lowConfidenceCheckBox.AutoSize = true;
            this._lowConfidenceCheckBox.Location = new System.Drawing.Point(9, 56);
            this._lowConfidenceCheckBox.Name = "_lowConfidenceCheckBox";
            this._lowConfidenceCheckBox.Size = new System.Drawing.Size(262, 17);
            this._lowConfidenceCheckBox.TabIndex = 16;
            this._lowConfidenceCheckBox.Text = "Also select zones with a confidence of at least";
            this._lowConfidenceCheckBox.UseVisualStyleBackColor = true;
            // 
            // _lowConfidenceLabel1
            // 
            this._lowConfidenceLabel1.AutoSize = true;
            this._lowConfidenceLabel1.Location = new System.Drawing.Point(348, 57);
            this._lowConfidenceLabel1.Name = "_lowConfidenceLabel1";
            this._lowConfidenceLabel1.Size = new System.Drawing.Size(101, 13);
            this._lowConfidenceLabel1.TabIndex = 18;
            this._lowConfidenceLabel1.Text = "percent as long as";
            // 
            // _lowConfidenceUpDown
            // 
            this._lowConfidenceUpDown.Location = new System.Drawing.Point(279, 55);
            this._lowConfidenceUpDown.Name = "_lowConfidenceUpDown";
            this._lowConfidenceUpDown.Size = new System.Drawing.Size(51, 22);
            this._lowConfidenceUpDown.TabIndex = 17;
            // 
            // _lowConfidenceLabel2
            // 
            this._lowConfidenceLabel2.AutoSize = true;
            this._lowConfidenceLabel2.Location = new System.Drawing.Point(6, 82);
            this._lowConfidenceLabel2.Name = "_lowConfidenceLabel2";
            this._lowConfidenceLabel2.Size = new System.Drawing.Size(313, 13);
            this._lowConfidenceLabel2.TabIndex = 20;
            this._lowConfidenceLabel2.Text = "the text also has a higher confidence than the default OCR.";
            // 
            // _highConfidenceUpDown
            // 
            this._highConfidenceUpDown.Location = new System.Drawing.Point(258, 16);
            this._highConfidenceUpDown.Name = "_highConfidenceUpDown";
            this._highConfidenceUpDown.Size = new System.Drawing.Size(51, 22);
            this._highConfidenceUpDown.TabIndex = 14;
            // 
            // _returnUnrecognizedCharactersCheckBox
            // 
            this._returnUnrecognizedCharactersCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._returnUnrecognizedCharactersCheckBox.AutoSize = true;
            this._returnUnrecognizedCharactersCheckBox.Location = new System.Drawing.Point(21, 116);
            this._returnUnrecognizedCharactersCheckBox.Name = "_returnUnrecognizedCharactersCheckBox";
            this._returnUnrecognizedCharactersCheckBox.Size = new System.Drawing.Size(224, 17);
            this._returnUnrecognizedCharactersCheckBox.TabIndex = 2;
            this._returnUnrecognizedCharactersCheckBox.Text = "Include unrecognized characters (as ^)";
            this._returnUnrecognizedCharactersCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this._engineGdPictureRadioButton);
            this.groupBox3.Controls.Add(this._engineKofaxRadioButton);
            this.groupBox3.Location = new System.Drawing.Point(11, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(578, 46);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Engine";
            this.groupBox3.Enabled = false;
            // 
            // _engineGdPictureRadioButton
            // 
            this._engineGdPictureRadioButton.AutoSize = true;
            this._engineGdPictureRadioButton.Location = new System.Drawing.Point(66, 21);
            this._engineGdPictureRadioButton.Name = "_engineGdPictureRadioButton";
            this._engineGdPictureRadioButton.Size = new System.Drawing.Size(75, 17);
            this._engineGdPictureRadioButton.TabIndex = 1;
            this._engineGdPictureRadioButton.TabStop = true;
            this._engineGdPictureRadioButton.Text = "GdPicture";
            this._engineGdPictureRadioButton.UseVisualStyleBackColor = true;
            this._engineGdPictureRadioButton.CheckedChanged += new System.EventHandler(this.EngineRadioButton_CheckedChanged);
            // 
            // _engineKofaxRadioButton
            // 
            this._engineKofaxRadioButton.AutoSize = true;
            this._engineKofaxRadioButton.Location = new System.Drawing.Point(7, 21);
            this._engineKofaxRadioButton.Name = "_engineKofaxRadioButton";
            this._engineKofaxRadioButton.Size = new System.Drawing.Size(53, 17);
            this._engineKofaxRadioButton.TabIndex = 0;
            this._engineKofaxRadioButton.TabStop = true;
            this._engineKofaxRadioButton.Text = "Kofax";
            this._engineKofaxRadioButton.UseVisualStyleBackColor = true;
            this._engineKofaxRadioButton.CheckedChanged += new System.EventHandler(this.EngineRadioButton_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this._searchPagesWithTextRadioButton);
            this.groupBox4.Controls.Add(this._searchAllPagesRadioButton);
            this.groupBox4.Location = new System.Drawing.Point(11, 64);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(578, 46);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Scope";
            // 
            // _searchPagesWithTextRadioButton
            // 
            this._searchPagesWithTextRadioButton.AutoSize = true;
            this._searchPagesWithTextRadioButton.Location = new System.Drawing.Point(85, 21);
            this._searchPagesWithTextRadioButton.Name = "_searchPagesWithTextRadioButton";
            this._searchPagesWithTextRadioButton.Size = new System.Drawing.Size(191, 17);
            this._searchPagesWithTextRadioButton.TabIndex = 1;
            this._searchPagesWithTextRadioButton.TabStop = true;
            this._searchPagesWithTextRadioButton.Text = "Only pages with recognized text";
            this._searchPagesWithTextRadioButton.UseVisualStyleBackColor = true;
            // 
            // _searchAllPagesRadioButton
            // 
            this._searchAllPagesRadioButton.AutoSize = true;
            this._searchAllPagesRadioButton.Location = new System.Drawing.Point(7, 21);
            this._searchAllPagesRadioButton.Name = "_searchAllPagesRadioButton";
            this._searchAllPagesRadioButton.Size = new System.Drawing.Size(72, 17);
            this._searchAllPagesRadioButton.TabIndex = 0;
            this._searchAllPagesRadioButton.TabStop = true;
            this._searchAllPagesRadioButton.Text = "All pages";
            this._searchAllPagesRadioButton.UseVisualStyleBackColor = true;
            // 
            // MicrFinderSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(602, 574);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this._returnUnrecognizedCharactersCheckBox);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this._inheritOCRParametersCheckBox);
            this.Controls.Add(groupBox1);
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
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._lowConfidenceUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._highConfidenceUpDown)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.CheckBox _splitRoutingCheckBox;
        private System.Windows.Forms.CheckBox _splitAccountCheckBox;
        private System.Windows.Forms.CheckBox _splitCheckCheckBox;
        private System.Windows.Forms.CheckBox _splitAmountCheckBox;
        private Utilities.Forms.BrowseButton _configFileNameBrowseButton;
        private FileActionManager.Forms.FileActionManagerPathTagButton _micrSplitterRegexPathTagButton;
        private System.Windows.Forms.TextBox _micrSplitterRegexTextBox;
        private Utilities.Forms.BrowseButton _micrSplitterRegexBrowseButton;
        private System.Windows.Forms.TextBox _micrSplitterTextBox;
        private System.Windows.Forms.CheckBox _filterCharsWhenSplittingCheckBox;
        private System.Windows.Forms.CheckBox checkBox1;
        private Utilities.Forms.BrowseButton browseButton1;
        private System.Windows.Forms.CheckBox _inheritOCRParametersCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private FileActionManager.Forms.FileActionManagerPathTagButton _regexFilterFileNamePathTagsButton;
        private System.Windows.Forms.TextBox _filterRegExTextBox;
        private Utilities.Forms.BrowseButton _regexFilterFileNameBrowseButton;
        private System.Windows.Forms.CheckBox _lowConfidenceCheckBox;
        private System.Windows.Forms.Label _lowConfidenceLabel1;
        private System.Windows.Forms.NumericUpDown _lowConfidenceUpDown;
        private System.Windows.Forms.Label _lowConfidenceLabel2;
        private System.Windows.Forms.NumericUpDown _highConfidenceUpDown;
        private System.Windows.Forms.CheckBox _returnUnrecognizedCharactersCheckBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton _engineGdPictureRadioButton;
        private System.Windows.Forms.RadioButton _engineKofaxRadioButton;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton _searchPagesWithTextRadioButton;
        private System.Windows.Forms.RadioButton _searchAllPagesRadioButton;
    }
}