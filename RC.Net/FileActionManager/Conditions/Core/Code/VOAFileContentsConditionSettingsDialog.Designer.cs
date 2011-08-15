namespace Extract.FileActionManager.Conditions
{
    partial class VOAFileContentsConditionSettingsDialog
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
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VOAFileContentsConditionSettingsDialog));
            this._containsComboBox = new System.Windows.Forms.ComboBox();
            this._attributeQueryTextBox = new System.Windows.Forms.TextBox();
            this._fieldNameComboBox = new System.Windows.Forms.ComboBox();
            this._comparisonRadioButton = new System.Windows.Forms.RadioButton();
            this._rangeRadioButton = new System.Windows.Forms.RadioButton();
            this._comparisonValueTextBox = new System.Windows.Forms.TextBox();
            this._rangeMinValueTextBox = new System.Windows.Forms.TextBox();
            this._rangeMaxValueTextBox = new System.Windows.Forms.TextBox();
            this._searchMatchRadioButton = new System.Windows.Forms.RadioButton();
            this._searchMatchTypeComboBox = new System.Windows.Forms.ComboBox();
            this._voaFileTextBox = new System.Windows.Forms.TextBox();
            this._searchTextBox = new System.Windows.Forms.TextBox();
            this._inListRadioButton = new System.Windows.Forms.RadioButton();
            this._matchListBox = new System.Windows.Forms.ListBox();
            this._removeButton = new System.Windows.Forms.Button();
            this._addButton = new System.Windows.Forms.Button();
            this._modifyButton = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._comparisonComboBox = new System.Windows.Forms.ComboBox();
            this._caseSensitiveCheckBox = new System.Windows.Forms.CheckBox();
            this._regexCheckBox = new System.Windows.Forms.CheckBox();
            this._metComboBox = new System.Windows.Forms.ComboBox();
            this._regexInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._attributeQueryInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._voaFilePathTags = new Extract.Utilities.Forms.PathTagsButton();
            this._voaFileBrowse = new Extract.Utilities.Forms.BrowseButton();
            this._attributeCountUpDown = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._comparisonInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._rangeInfoTip = new Extract.Utilities.Forms.InfoTip();
            label1 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._attributeCountUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(108, 13);
            label1.TabIndex = 0;
            label1.Text = "Consider condition as";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(222, 63);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(133, 13);
            label4.TabIndex = 8;
            label4.Text = "of the following attribute(s):";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(13, 149);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(102, 13);
            label5.TabIndex = 11;
            label5.Text = "where the attribute\'s";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(252, 203);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(16, 13);
            label6.TabIndex = 19;
            label6.Text = "to";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(165, 229);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(69, 13);
            label7.TabIndex = 24;
            label7.Text = "the following:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(192, 9);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(118, 13);
            label2.TabIndex = 2;
            label2.Text = "if the following VOA file:";
            // 
            // _containsComboBox
            // 
            this._containsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._containsComboBox.FormattingEnabled = true;
            this._containsComboBox.Location = new System.Drawing.Point(13, 60);
            this._containsComboBox.Name = "_containsComboBox";
            this._containsComboBox.Size = new System.Drawing.Size(146, 21);
            this._containsComboBox.TabIndex = 6;
            // 
            // _attributeQueryTextBox
            // 
            this._attributeQueryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeQueryTextBox.Location = new System.Drawing.Point(13, 87);
            this._attributeQueryTextBox.Multiline = true;
            this._attributeQueryTextBox.Name = "_attributeQueryTextBox";
            this._attributeQueryTextBox.Size = new System.Drawing.Size(427, 53);
            this._attributeQueryTextBox.TabIndex = 10;
            // 
            // _fieldNameComboBox
            // 
            this._fieldNameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._fieldNameComboBox.FormattingEnabled = true;
            this._fieldNameComboBox.Location = new System.Drawing.Point(121, 146);
            this._fieldNameComboBox.Name = "_fieldNameComboBox";
            this._fieldNameComboBox.Size = new System.Drawing.Size(82, 21);
            this._fieldNameComboBox.TabIndex = 12;
            // 
            // _comparisonRadioButton
            // 
            this._comparisonRadioButton.AutoSize = true;
            this._comparisonRadioButton.Location = new System.Drawing.Point(14, 175);
            this._comparisonRadioButton.Name = "_comparisonRadioButton";
            this._comparisonRadioButton.Size = new System.Drawing.Size(14, 13);
            this._comparisonRadioButton.TabIndex = 13;
            this._comparisonRadioButton.TabStop = true;
            this._comparisonRadioButton.UseVisualStyleBackColor = true;
            // 
            // _rangeRadioButton
            // 
            this._rangeRadioButton.AutoSize = true;
            this._rangeRadioButton.Location = new System.Drawing.Point(14, 201);
            this._rangeRadioButton.Name = "_rangeRadioButton";
            this._rangeRadioButton.Size = new System.Drawing.Size(91, 17);
            this._rangeRadioButton.TabIndex = 17;
            this._rangeRadioButton.TabStop = true;
            this._rangeRadioButton.Text = "is in the range";
            this._rangeRadioButton.UseVisualStyleBackColor = true;
            // 
            // _comparisonValueTextBox
            // 
            this._comparisonValueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._comparisonValueTextBox.Enabled = false;
            this._comparisonValueTextBox.Location = new System.Drawing.Point(209, 172);
            this._comparisonValueTextBox.Name = "_comparisonValueTextBox";
            this._comparisonValueTextBox.Size = new System.Drawing.Size(209, 20);
            this._comparisonValueTextBox.TabIndex = 15;
            // 
            // _rangeMinValueTextBox
            // 
            this._rangeMinValueTextBox.Enabled = false;
            this._rangeMinValueTextBox.Location = new System.Drawing.Point(111, 200);
            this._rangeMinValueTextBox.Name = "_rangeMinValueTextBox";
            this._rangeMinValueTextBox.Size = new System.Drawing.Size(135, 20);
            this._rangeMinValueTextBox.TabIndex = 18;
            // 
            // _rangeMaxValueTextBox
            // 
            this._rangeMaxValueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._rangeMaxValueTextBox.Enabled = false;
            this._rangeMaxValueTextBox.Location = new System.Drawing.Point(273, 200);
            this._rangeMaxValueTextBox.Name = "_rangeMaxValueTextBox";
            this._rangeMaxValueTextBox.Size = new System.Drawing.Size(145, 20);
            this._rangeMaxValueTextBox.TabIndex = 20;
            // 
            // _searchMatchRadioButton
            // 
            this._searchMatchRadioButton.AutoSize = true;
            this._searchMatchRadioButton.Location = new System.Drawing.Point(14, 229);
            this._searchMatchRadioButton.Name = "_searchMatchRadioButton";
            this._searchMatchRadioButton.Size = new System.Drawing.Size(14, 13);
            this._searchMatchRadioButton.TabIndex = 22;
            this._searchMatchRadioButton.TabStop = true;
            this._searchMatchRadioButton.UseVisualStyleBackColor = true;
            // 
            // _searchMatchTypeComboBox
            // 
            this._searchMatchTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._searchMatchTypeComboBox.Enabled = false;
            this._searchMatchTypeComboBox.FormattingEnabled = true;
            this._searchMatchTypeComboBox.Items.AddRange(new object[] {
            "fully matches",
            "contains a match for"});
            this._searchMatchTypeComboBox.Location = new System.Drawing.Point(34, 226);
            this._searchMatchTypeComboBox.Name = "_searchMatchTypeComboBox";
            this._searchMatchTypeComboBox.Size = new System.Drawing.Size(125, 21);
            this._searchMatchTypeComboBox.TabIndex = 23;
            // 
            // _voaFileTextBox
            // 
            this._voaFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFileTextBox.Location = new System.Drawing.Point(13, 34);
            this._voaFileTextBox.Name = "_voaFileTextBox";
            this._voaFileTextBox.Size = new System.Drawing.Size(370, 20);
            this._voaFileTextBox.TabIndex = 3;
            // 
            // _searchTextBox
            // 
            this._searchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._searchTextBox.Enabled = false;
            this._searchTextBox.Location = new System.Drawing.Point(12, 253);
            this._searchTextBox.Multiline = true;
            this._searchTextBox.Name = "_searchTextBox";
            this._searchTextBox.Size = new System.Drawing.Size(428, 53);
            this._searchTextBox.TabIndex = 25;
            // 
            // _inListRadioButton
            // 
            this._inListRadioButton.AutoSize = true;
            this._inListRadioButton.Location = new System.Drawing.Point(13, 312);
            this._inListRadioButton.Name = "_inListRadioButton";
            this._inListRadioButton.Size = new System.Drawing.Size(241, 17);
            this._inListRadioButton.TabIndex = 26;
            this._inListRadioButton.TabStop = true;
            this._inListRadioButton.Text = "matches one of the entries in the following list:";
            this._inListRadioButton.UseVisualStyleBackColor = true;
            // 
            // _matchListBox
            // 
            this._matchListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._matchListBox.Enabled = false;
            this._matchListBox.FormattingEnabled = true;
            this._matchListBox.Location = new System.Drawing.Point(13, 335);
            this._matchListBox.Name = "_matchListBox";
            this._matchListBox.Size = new System.Drawing.Size(347, 82);
            this._matchListBox.TabIndex = 27;
            this._matchListBox.DoubleClick += new System.EventHandler(this.HandleListModifyButtonClick);
            // 
            // _removeButton
            // 
            this._removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._removeButton.Enabled = false;
            this._removeButton.Location = new System.Drawing.Point(366, 364);
            this._removeButton.Name = "_removeButton";
            this._removeButton.Size = new System.Drawing.Size(75, 23);
            this._removeButton.TabIndex = 29;
            this._removeButton.Text = "Remove";
            this._removeButton.UseVisualStyleBackColor = true;
            this._removeButton.Click += new System.EventHandler(this.HandleListRemoveButtonClick);
            // 
            // _addButton
            // 
            this._addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addButton.Enabled = false;
            this._addButton.Location = new System.Drawing.Point(366, 335);
            this._addButton.Name = "_addButton";
            this._addButton.Size = new System.Drawing.Size(75, 23);
            this._addButton.TabIndex = 28;
            this._addButton.Text = "Add";
            this._addButton.UseVisualStyleBackColor = true;
            this._addButton.Click += new System.EventHandler(this.HandleListAddButtonClick);
            // 
            // _modifyButton
            // 
            this._modifyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._modifyButton.Enabled = false;
            this._modifyButton.Location = new System.Drawing.Point(366, 393);
            this._modifyButton.Name = "_modifyButton";
            this._modifyButton.Size = new System.Drawing.Size(75, 23);
            this._modifyButton.TabIndex = 30;
            this._modifyButton.Text = "Modify";
            this._modifyButton.UseVisualStyleBackColor = true;
            this._modifyButton.Click += new System.EventHandler(this.HandleListModifyButtonClick);
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.Location = new System.Drawing.Point(284, 476);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 34;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(365, 476);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 35;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _comparisonComboBox
            // 
            this._comparisonComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._comparisonComboBox.Enabled = false;
            this._comparisonComboBox.FormattingEnabled = true;
            this._comparisonComboBox.Location = new System.Drawing.Point(34, 172);
            this._comparisonComboBox.Name = "_comparisonComboBox";
            this._comparisonComboBox.Size = new System.Drawing.Size(169, 21);
            this._comparisonComboBox.TabIndex = 14;
            // 
            // _caseSensitiveCheckBox
            // 
            this._caseSensitiveCheckBox.AutoSize = true;
            this._caseSensitiveCheckBox.Location = new System.Drawing.Point(13, 423);
            this._caseSensitiveCheckBox.Name = "_caseSensitiveCheckBox";
            this._caseSensitiveCheckBox.Size = new System.Drawing.Size(94, 17);
            this._caseSensitiveCheckBox.TabIndex = 31;
            this._caseSensitiveCheckBox.Text = "Case sensitive";
            this._caseSensitiveCheckBox.UseVisualStyleBackColor = true;
            // 
            // _regexCheckBox
            // 
            this._regexCheckBox.AutoSize = true;
            this._regexCheckBox.Enabled = false;
            this._regexCheckBox.Location = new System.Drawing.Point(13, 446);
            this._regexCheckBox.Name = "_regexCheckBox";
            this._regexCheckBox.Size = new System.Drawing.Size(221, 17);
            this._regexCheckBox.TabIndex = 32;
            this._regexCheckBox.Text = "Treat search terms as regular expressions";
            this._regexCheckBox.UseVisualStyleBackColor = true;
            // 
            // _metComboBox
            // 
            this._metComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._metComboBox.FormattingEnabled = true;
            this._metComboBox.Items.AddRange(new object[] {
            "met",
            "not met"});
            this._metComboBox.Location = new System.Drawing.Point(127, 6);
            this._metComboBox.Name = "_metComboBox";
            this._metComboBox.Size = new System.Drawing.Size(59, 21);
            this._metComboBox.TabIndex = 1;
            // 
            // _regexInfoTip
            // 
            this._regexInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._regexInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_regexInfoTip.BackgroundImage")));
            this._regexInfoTip.Location = new System.Drawing.Point(243, 447);
            this._regexInfoTip.Name = "_regexInfoTip";
            this._regexInfoTip.Size = new System.Drawing.Size(16, 16);
            this._regexInfoTip.TabIndex = 33;
            this._regexInfoTip.TabStop = false;
            this._regexInfoTip.TipText = "When not performing greater than or less that comparisons, any specified values w" +
    "ill be treated\r\nas reqular expressions that, unless otherwise specified, fully m" +
    "atch the specified attribute field.";
            // 
            // _attributeQueryInfoTip
            // 
            this._attributeQueryInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._attributeQueryInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_attributeQueryInfoTip.BackgroundImage")));
            this._attributeQueryInfoTip.Location = new System.Drawing.Point(361, 63);
            this._attributeQueryInfoTip.Name = "_attributeQueryInfoTip";
            this._attributeQueryInfoTip.Size = new System.Drawing.Size(16, 16);
            this._attributeQueryInfoTip.TabIndex = 9;
            this._attributeQueryInfoTip.TabStop = false;
            this._attributeQueryInfoTip.TipText = resources.GetString("_attributeQueryInfoTip.TipText");
            // 
            // _voaFilePathTags
            // 
            this._voaFilePathTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFilePathTags.Image = ((System.Drawing.Image)(resources.GetObject("_voaFilePathTags.Image")));
            this._voaFilePathTags.Location = new System.Drawing.Point(389, 33);
            this._voaFilePathTags.Name = "_voaFilePathTags";
            this._voaFilePathTags.Size = new System.Drawing.Size(18, 20);
            this._voaFilePathTags.TabIndex = 4;
            this._voaFilePathTags.TextControl = this._voaFileTextBox;
            this._voaFilePathTags.UseVisualStyleBackColor = true;
            // 
            // _voaFileBrowse
            // 
            this._voaFileBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFileBrowse.Location = new System.Drawing.Point(413, 33);
            this._voaFileBrowse.Name = "_voaFileBrowse";
            this._voaFileBrowse.Size = new System.Drawing.Size(27, 20);
            this._voaFileBrowse.TabIndex = 5;
            this._voaFileBrowse.Text = "...";
            this._voaFileBrowse.TextControl = this._voaFileTextBox;
            this._voaFileBrowse.UseVisualStyleBackColor = true;
            // 
            // _attributeCountUpDown
            // 
            this._attributeCountUpDown.Location = new System.Drawing.Point(165, 61);
            this._attributeCountUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this._attributeCountUpDown.Name = "_attributeCountUpDown";
            this._attributeCountUpDown.Size = new System.Drawing.Size(51, 20);
            this._attributeCountUpDown.TabIndex = 7;
            this._attributeCountUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._attributeCountUpDown.UserTextCorrected += new System.EventHandler<System.EventArgs>(this.HandleAttributeCountCorrected);
            // 
            // _comparisonInfoTip
            // 
            this._comparisonInfoTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._comparisonInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._comparisonInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_comparisonInfoTip.BackgroundImage")));
            this._comparisonInfoTip.Location = new System.Drawing.Point(424, 172);
            this._comparisonInfoTip.Name = "_comparisonInfoTip";
            this._comparisonInfoTip.Size = new System.Drawing.Size(16, 16);
            this._comparisonInfoTip.TabIndex = 16;
            this._comparisonInfoTip.TabStop = false;
            this._comparisonInfoTip.TipText = resources.GetString("_comparisonInfoTip.TipText");
            // 
            // _rangeInfoTip
            // 
            this._rangeInfoTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._rangeInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._rangeInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_rangeInfoTip.BackgroundImage")));
            this._rangeInfoTip.Location = new System.Drawing.Point(424, 200);
            this._rangeInfoTip.Name = "_rangeInfoTip";
            this._rangeInfoTip.Size = new System.Drawing.Size(16, 16);
            this._rangeInfoTip.TabIndex = 21;
            this._rangeInfoTip.TabStop = false;
            this._rangeInfoTip.TipText = resources.GetString("_rangeInfoTip.TipText");
            // 
            // VOAFileContentsConditionSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(452, 511);
            this.Controls.Add(this._rangeInfoTip);
            this.Controls.Add(this._comparisonInfoTip);
            this.Controls.Add(this._regexInfoTip);
            this.Controls.Add(this._attributeQueryInfoTip);
            this.Controls.Add(label2);
            this.Controls.Add(this._metComboBox);
            this.Controls.Add(this._regexCheckBox);
            this.Controls.Add(this._caseSensitiveCheckBox);
            this.Controls.Add(this._comparisonComboBox);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this._modifyButton);
            this.Controls.Add(this._addButton);
            this.Controls.Add(this._removeButton);
            this.Controls.Add(this._matchListBox);
            this.Controls.Add(this._inListRadioButton);
            this.Controls.Add(this._searchTextBox);
            this.Controls.Add(this._voaFilePathTags);
            this.Controls.Add(this._voaFileBrowse);
            this.Controls.Add(this._voaFileTextBox);
            this.Controls.Add(label7);
            this.Controls.Add(this._searchMatchTypeComboBox);
            this.Controls.Add(this._searchMatchRadioButton);
            this.Controls.Add(label6);
            this.Controls.Add(this._rangeMaxValueTextBox);
            this.Controls.Add(this._rangeMinValueTextBox);
            this.Controls.Add(this._comparisonValueTextBox);
            this.Controls.Add(this._rangeRadioButton);
            this.Controls.Add(this._comparisonRadioButton);
            this.Controls.Add(this._fieldNameComboBox);
            this.Controls.Add(label5);
            this.Controls.Add(this._attributeQueryTextBox);
            this.Controls.Add(label4);
            this.Controls.Add(this._attributeCountUpDown);
            this.Controls.Add(this._containsComboBox);
            this.Controls.Add(label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(458, 539);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(458, 539);
            this.Name = "VOAFileContentsConditionSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Core: VOA File Contents Condition";
            ((System.ComponentModel.ISupportInitialize)(this._attributeCountUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox _containsComboBox;
        private Utilities.Forms.BetterNumericUpDown _attributeCountUpDown;
        private System.Windows.Forms.TextBox _attributeQueryTextBox;
        private System.Windows.Forms.ComboBox _fieldNameComboBox;
        private System.Windows.Forms.RadioButton _comparisonRadioButton;
        private System.Windows.Forms.RadioButton _rangeRadioButton;
        private System.Windows.Forms.TextBox _comparisonValueTextBox;
        private System.Windows.Forms.TextBox _rangeMinValueTextBox;
        private System.Windows.Forms.TextBox _rangeMaxValueTextBox;
        private System.Windows.Forms.RadioButton _searchMatchRadioButton;
        private System.Windows.Forms.ComboBox _searchMatchTypeComboBox;
        private System.Windows.Forms.TextBox _voaFileTextBox;
        private Utilities.Forms.PathTagsButton _voaFilePathTags;
        private Utilities.Forms.BrowseButton _voaFileBrowse;
        private System.Windows.Forms.TextBox _searchTextBox;
        private System.Windows.Forms.RadioButton _inListRadioButton;
        private System.Windows.Forms.ListBox _matchListBox;
        private System.Windows.Forms.Button _removeButton;
        private System.Windows.Forms.Button _addButton;
        private System.Windows.Forms.Button _modifyButton;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.ComboBox _comparisonComboBox;
        private System.Windows.Forms.CheckBox _caseSensitiveCheckBox;
        private System.Windows.Forms.CheckBox _regexCheckBox;
        private System.Windows.Forms.ComboBox _metComboBox;
        private Utilities.Forms.InfoTip _attributeQueryInfoTip;
        private Utilities.Forms.InfoTip _regexInfoTip;
        private Utilities.Forms.InfoTip _comparisonInfoTip;
        private Utilities.Forms.InfoTip _rangeInfoTip;
    }
}