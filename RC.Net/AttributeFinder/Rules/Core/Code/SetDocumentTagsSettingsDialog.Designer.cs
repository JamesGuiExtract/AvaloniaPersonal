namespace Extract.AttributeFinder.Rules
{
    partial class SetDocumentTagsSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetDocumentTagsSettingsDialog));
            this._rsdFileNamePathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._rsdFileNameTextBox = new System.Windows.Forms.TextBox();
            this._rsdFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._configureToSetDocType = new System.Windows.Forms.Button();
            this._setStringTagCheckBox = new System.Windows.Forms.CheckBox();
            this._stringTagAttributeSelector = new Utilities.Forms.ConfigurableObjectControl();
            this._stringTagName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this._delimiter = new System.Windows.Forms.TextBox();
            this._tagNameForStringTagValue = new System.Windows.Forms.TextBox();
            this._useTagValueForStringTag = new System.Windows.Forms.RadioButton();
            this._stringTagPathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._specifiedValueForStringTag = new System.Windows.Forms.TextBox();
            this._useSelectedAttributesForStringTag = new System.Windows.Forms.RadioButton();
            this._useSpecifiedValueForStringTag = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._setObjectTagCheckBox = new System.Windows.Forms.CheckBox();
            this._objectTagPathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._specifiedValueForObjectTag = new System.Windows.Forms.TextBox();
            this._useSelectedAttributesForObjectTag = new System.Windows.Forms.RadioButton();
            this._useSpecifiedValueForObjectTag = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._objectTagName = new System.Windows.Forms.TextBox();
            this._objectTagAttributeSelector = new Utilities.Forms.ConfigurableObjectControl();
            this._noTagsIfEmptyCheckBox = new System.Windows.Forms.CheckBox();
            this._generateSourceAttributesWithRSDCheckBox = new System.Windows.Forms.CheckBox();
            _rsdFileGroupBox = new System.Windows.Forms.GroupBox();
            _rsdFileGroupBox.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rsdFileGroupBox
            // 
            _rsdFileGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            _rsdFileGroupBox.Controls.Add(this._rsdFileNamePathTagsButton);
            _rsdFileGroupBox.Controls.Add(this._rsdFileNameBrowseButton);
            _rsdFileGroupBox.Controls.Add(this._rsdFileNameTextBox);
            _rsdFileGroupBox.Location = new System.Drawing.Point(41, 504);
            _rsdFileGroupBox.Name = "_rsdFileGroupBox";
            _rsdFileGroupBox.Size = new System.Drawing.Size(415, 52);
            _rsdFileGroupBox.TabIndex = 8;
            _rsdFileGroupBox.TabStop = false;
            _rsdFileGroupBox.Text = "RSD file name";
            // 
            // _rsdFileNamePathTagsButton
            // 
            this._rsdFileNamePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._rsdFileNamePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_rsdFileNamePathTagsButton.Image")));
            this._rsdFileNamePathTagsButton.Location = new System.Drawing.Point(358, 20);
            this._rsdFileNamePathTagsButton.Name = "_rsdFileNamePathTagsButton";
            this._rsdFileNamePathTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._rsdFileNamePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._rsdFileNamePathTagsButton.TabIndex = 2;
            this._rsdFileNamePathTagsButton.TextControl = this._rsdFileNameTextBox;
            this._rsdFileNamePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _rsdFileNameTextBox
            // 
            this._rsdFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._rsdFileNameTextBox.HideSelection = false;
            this._rsdFileNameTextBox.Location = new System.Drawing.Point(6, 20);
            this._rsdFileNameTextBox.Name = "_rsdFileNameTextBox";
            this._rsdFileNameTextBox.Size = new System.Drawing.Size(346, 20);
            this._rsdFileNameTextBox.TabIndex = 1;
            // 
            // _rsdFileNameBrowseButton
            // 
            this._rsdFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._rsdFileNameBrowseButton.Location = new System.Drawing.Point(382, 20);
            this._rsdFileNameBrowseButton.Name = "_rsdFileNameBrowseButton";
            this._rsdFileNameBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._rsdFileNameBrowseButton.TabIndex = 3;
            this._rsdFileNameBrowseButton.Text = "...";
            this._rsdFileNameBrowseButton.TextControl = this._rsdFileNameTextBox;
            this._rsdFileNameBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(381, 564);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 5;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(300, 564);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 4;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _configureToSetDocType
            // 
            this._configureToSetDocType.Location = new System.Drawing.Point(12, 12);
            this._configureToSetDocType.Name = "_configureToSetDocType";
            this._configureToSetDocType.Size = new System.Drawing.Size(200, 23);
            this._configureToSetDocType.TabIndex = 1;
            this._configureToSetDocType.Text = "Configure to set DocType from VOA";
            this._configureToSetDocType.UseVisualStyleBackColor = true;
            this._configureToSetDocType.Click += new System.EventHandler(this.HandleConfigureToSetDocTypeClick);
            // 
            // _setStringTagCheckBox
            // 
            this._setStringTagCheckBox.AutoSize = true;
            this._setStringTagCheckBox.Checked = true;
            this._setStringTagCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._setStringTagCheckBox.Location = new System.Drawing.Point(9, 15);
            this._setStringTagCheckBox.Name = "_setStringTagCheckBox";
            this._setStringTagCheckBox.Size = new System.Drawing.Size(88, 17);
            this._setStringTagCheckBox.TabIndex = 0;
            this._setStringTagCheckBox.Text = "Set string tag";
            this._setStringTagCheckBox.UseVisualStyleBackColor = true;
            this._setStringTagCheckBox.CheckedChanged += new System.EventHandler(this.HandleCheckChanged);
            // 
            // _stringTagAttributeSelector
            // 
            this._stringTagAttributeSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._stringTagAttributeSelector.CategoryName = "UCLID AF-API Selectors";
            this._stringTagAttributeSelector.ConfigurableObject = null;
            this._stringTagAttributeSelector.Location = new System.Drawing.Point(9, 164);
            this._stringTagAttributeSelector.Name = "_stringTagAttributeSelector";
            this._stringTagAttributeSelector.Size = new System.Drawing.Size(432, 49);
            this._stringTagAttributeSelector.TabIndex = 12;
            // 
            // _stringTagName
            // 
            this._stringTagName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._stringTagName.Location = new System.Drawing.Point(67, 38);
            this._stringTagName.Name = "_stringTagName";
            this._stringTagName.Size = new System.Drawing.Size(342, 20);
            this._stringTagName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Tag name:";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this._delimiter);
            this.groupBox2.Controls.Add(this._tagNameForStringTagValue);
            this.groupBox2.Controls.Add(this._useTagValueForStringTag);
            this.groupBox2.Controls.Add(this._setStringTagCheckBox);
            this.groupBox2.Controls.Add(this._stringTagPathTagsButton);
            this.groupBox2.Controls.Add(this._specifiedValueForStringTag);
            this.groupBox2.Controls.Add(this._useSelectedAttributesForStringTag);
            this.groupBox2.Controls.Add(this._useSpecifiedValueForStringTag);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this._stringTagName);
            this.groupBox2.Controls.Add(this._stringTagAttributeSelector);
            this.groupBox2.Location = new System.Drawing.Point(12, 41);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(444, 219);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(311, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Delimiter:";
            // 
            // _delimiter
            // 
            this._delimiter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._delimiter.Location = new System.Drawing.Point(364, 120);
            this._delimiter.Name = "_delimiter";
            this._delimiter.Size = new System.Drawing.Size(45, 20);
            this._delimiter.TabIndex = 11;
            // 
            // _tagNameForStringTagValue
            // 
            this._tagNameForStringTagValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tagNameForStringTagValue.Location = new System.Drawing.Point(112, 92);
            this._tagNameForStringTagValue.Name = "_tagNameForStringTagValue";
            this._tagNameForStringTagValue.Size = new System.Drawing.Size(297, 20);
            this._tagNameForStringTagValue.TabIndex = 9;
            // 
            // _useTagValueForStringTag
            // 
            this._useTagValueForStringTag.AutoSize = true;
            this._useTagValueForStringTag.Location = new System.Drawing.Point(9, 93);
            this._useTagValueForStringTag.Name = "_useTagValueForStringTag";
            this._useTagValueForStringTag.Size = new System.Drawing.Size(102, 17);
            this._useTagValueForStringTag.TabIndex = 4;
            this._useTagValueForStringTag.Text = "This tag\'s value:";
            this._useTagValueForStringTag.UseVisualStyleBackColor = true;
            this._useTagValueForStringTag.CheckedChanged += new System.EventHandler(this.HandleCheckChanged);
            // 
            // _stringTagPathTagsButton
            // 
            this._stringTagPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._stringTagPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_stringTagPathTagsButton.Image")));
            this._stringTagPathTagsButton.Location = new System.Drawing.Point(415, 65);
            this._stringTagPathTagsButton.Name = "_stringTagPathTagsButton";
            this._stringTagPathTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._stringTagPathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._stringTagPathTagsButton.TabIndex = 8;
            this._stringTagPathTagsButton.TextControl = this._specifiedValueForStringTag;
            this._stringTagPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _specifiedValueForStringTag
            // 
            this._specifiedValueForStringTag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._specifiedValueForStringTag.Location = new System.Drawing.Point(67, 65);
            this._specifiedValueForStringTag.Name = "_specifiedValueForStringTag";
            this._specifiedValueForStringTag.Size = new System.Drawing.Size(342, 20);
            this._specifiedValueForStringTag.TabIndex = 7;
            // 
            // _useSelectedAttributesForStringTag
            // 
            this._useSelectedAttributesForStringTag.AutoSize = true;
            this._useSelectedAttributesForStringTag.Location = new System.Drawing.Point(9, 120);
            this._useSelectedAttributesForStringTag.Name = "_useSelectedAttributesForStringTag";
            this._useSelectedAttributesForStringTag.Size = new System.Drawing.Size(195, 17);
            this._useSelectedAttributesForStringTag.TabIndex = 5;
            this._useSelectedAttributesForStringTag.Text = "Select attributes to use for the value";
            this._useSelectedAttributesForStringTag.UseVisualStyleBackColor = true;
            this._useSelectedAttributesForStringTag.CheckedChanged += new System.EventHandler(this.HandleCheckChanged);
            // 
            // _useSpecifiedValueForStringTag
            // 
            this._useSpecifiedValueForStringTag.AutoSize = true;
            this._useSpecifiedValueForStringTag.Checked = true;
            this._useSpecifiedValueForStringTag.Location = new System.Drawing.Point(9, 66);
            this._useSpecifiedValueForStringTag.Name = "_useSpecifiedValueForStringTag";
            this._useSpecifiedValueForStringTag.Size = new System.Drawing.Size(55, 17);
            this._useSpecifiedValueForStringTag.TabIndex = 3;
            this._useSpecifiedValueForStringTag.TabStop = true;
            this._useSpecifiedValueForStringTag.Text = "Value:";
            this._useSpecifiedValueForStringTag.UseVisualStyleBackColor = true;
            this._useSpecifiedValueForStringTag.CheckedChanged += new System.EventHandler(this.HandleCheckChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 147);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Select the source attributes";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._setObjectTagCheckBox);
            this.groupBox1.Controls.Add(this._objectTagPathTagsButton);
            this.groupBox1.Controls.Add(this._specifiedValueForObjectTag);
            this.groupBox1.Controls.Add(this._useSelectedAttributesForObjectTag);
            this.groupBox1.Controls.Add(this._useSpecifiedValueForObjectTag);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this._objectTagName);
            this.groupBox1.Controls.Add(this._objectTagAttributeSelector);
            this.groupBox1.Location = new System.Drawing.Point(12, 260);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(444, 190);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // _setObjectTagCheckBox
            // 
            this._setObjectTagCheckBox.AutoSize = true;
            this._setObjectTagCheckBox.Location = new System.Drawing.Point(9, 15);
            this._setObjectTagCheckBox.Name = "_setObjectTagCheckBox";
            this._setObjectTagCheckBox.Size = new System.Drawing.Size(92, 17);
            this._setObjectTagCheckBox.TabIndex = 0;
            this._setObjectTagCheckBox.Text = "Set object tag";
            this._setObjectTagCheckBox.UseVisualStyleBackColor = true;
            this._setObjectTagCheckBox.CheckedChanged += new System.EventHandler(this.HandleCheckChanged);
            // 
            // _objectTagPathTagsButton
            // 
            this._objectTagPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._objectTagPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_objectTagPathTagsButton.Image")));
            this._objectTagPathTagsButton.Location = new System.Drawing.Point(415, 65);
            this._objectTagPathTagsButton.Name = "_objectTagPathTagsButton";
            this._objectTagPathTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._objectTagPathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._objectTagPathTagsButton.TabIndex = 6;
            this._objectTagPathTagsButton.TextControl = this._specifiedValueForObjectTag;
            this._objectTagPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _specifiedValueForObjectTag
            // 
            this._specifiedValueForObjectTag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._specifiedValueForObjectTag.Location = new System.Drawing.Point(67, 65);
            this._specifiedValueForObjectTag.Name = "_specifiedValueForObjectTag";
            this._specifiedValueForObjectTag.Size = new System.Drawing.Size(342, 20);
            this._specifiedValueForObjectTag.TabIndex = 5;
            // 
            // _useSelectedAttributesForObjectTag
            // 
            this._useSelectedAttributesForObjectTag.AutoSize = true;
            this._useSelectedAttributesForObjectTag.Location = new System.Drawing.Point(9, 91);
            this._useSelectedAttributesForObjectTag.Name = "_useSelectedAttributesForObjectTag";
            this._useSelectedAttributesForObjectTag.Size = new System.Drawing.Size(195, 17);
            this._useSelectedAttributesForObjectTag.TabIndex = 4;
            this._useSelectedAttributesForObjectTag.Text = "Select attributes to use for the value";
            this._useSelectedAttributesForObjectTag.UseVisualStyleBackColor = true;
            this._useSelectedAttributesForObjectTag.CheckedChanged += new System.EventHandler(this.HandleCheckChanged);
            // 
            // _useSpecifiedValueForObjectTag
            // 
            this._useSpecifiedValueForObjectTag.AutoSize = true;
            this._useSpecifiedValueForObjectTag.Checked = true;
            this._useSpecifiedValueForObjectTag.Location = new System.Drawing.Point(9, 66);
            this._useSpecifiedValueForObjectTag.Name = "_useSpecifiedValueForObjectTag";
            this._useSpecifiedValueForObjectTag.Size = new System.Drawing.Size(55, 17);
            this._useSpecifiedValueForObjectTag.TabIndex = 3;
            this._useSpecifiedValueForObjectTag.TabStop = true;
            this._useSpecifiedValueForObjectTag.Text = "Value:";
            this._useSpecifiedValueForObjectTag.UseVisualStyleBackColor = true;
            this._useSpecifiedValueForObjectTag.CheckedChanged += new System.EventHandler(this.HandleCheckChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(136, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Select the source attributes";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Tag name:";
            // 
            // _objectTagName
            // 
            this._objectTagName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._objectTagName.Location = new System.Drawing.Point(67, 38);
            this._objectTagName.Name = "_objectTagName";
            this._objectTagName.Size = new System.Drawing.Size(342, 20);
            this._objectTagName.TabIndex = 2;
            // 
            // _objectTagAttributeSelector
            // 
            this._objectTagAttributeSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._objectTagAttributeSelector.CategoryName = "UCLID AF-API Selectors";
            this._objectTagAttributeSelector.ConfigurableObject = null;
            this._objectTagAttributeSelector.Location = new System.Drawing.Point(9, 135);
            this._objectTagAttributeSelector.Name = "_objectTagAttributeSelector";
            this._objectTagAttributeSelector.Size = new System.Drawing.Size(432, 49);
            this._objectTagAttributeSelector.TabIndex = 8;
            // 
            // _noTagsIfEmptyCheckBox
            // 
            this._noTagsIfEmptyCheckBox.AutoSize = true;
            this._noTagsIfEmptyCheckBox.Location = new System.Drawing.Point(21, 457);
            this._noTagsIfEmptyCheckBox.Name = "_noTagsIfEmptyCheckBox";
            this._noTagsIfEmptyCheckBox.Size = new System.Drawing.Size(268, 17);
            this._noTagsIfEmptyCheckBox.TabIndex = 6;
            this._noTagsIfEmptyCheckBox.Text = "\tDon\'t create any tags if a tag value would be empty";
            this._noTagsIfEmptyCheckBox.UseVisualStyleBackColor = true;
            // 
            // _generateSourceAttributesWithRSDCheckBox
            // 
            this._generateSourceAttributesWithRSDCheckBox.AutoSize = true;
            this._generateSourceAttributesWithRSDCheckBox.Location = new System.Drawing.Point(21, 481);
            this._generateSourceAttributesWithRSDCheckBox.Name = "_generateSourceAttributesWithRSDCheckBox";
            this._generateSourceAttributesWithRSDCheckBox.Size = new System.Drawing.Size(215, 17);
            this._generateSourceAttributesWithRSDCheckBox.TabIndex = 7;
            this._generateSourceAttributesWithRSDCheckBox.Text = "Generate source attributes with RSD file";
            this._generateSourceAttributesWithRSDCheckBox.UseVisualStyleBackColor = true;
            this._generateSourceAttributesWithRSDCheckBox.CheckedChanged += new System.EventHandler(this.HandleCheckChanged);
            // 
            // SetDocumentTagsSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(468, 599);
            this.Controls.Add(_rsdFileGroupBox);
            this.Controls.Add(this._generateSourceAttributesWithRSDCheckBox);
            this.Controls.Add(this._noTagsIfEmptyCheckBox);
            this.Controls.Add(this._configureToSetDocType);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetDocumentTagsSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Set document tags";
            _rsdFileGroupBox.ResumeLayout(false);
            _rsdFileGroupBox.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _configureToSetDocType;
        private System.Windows.Forms.CheckBox _setStringTagCheckBox;
        private Utilities.Forms.ConfigurableObjectControl _stringTagAttributeSelector;
        private System.Windows.Forms.TextBox _stringTagName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private FileActionManager.Forms.FileActionManagerPathTagButton _stringTagPathTagsButton;
        private System.Windows.Forms.TextBox _specifiedValueForStringTag;
        private System.Windows.Forms.RadioButton _useSelectedAttributesForStringTag;
        private System.Windows.Forms.RadioButton _useSpecifiedValueForStringTag;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _tagNameForStringTagValue;
        private System.Windows.Forms.RadioButton _useTagValueForStringTag;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox _setObjectTagCheckBox;
        private FileActionManager.Forms.FileActionManagerPathTagButton _objectTagPathTagsButton;
        private System.Windows.Forms.TextBox _specifiedValueForObjectTag;
        private System.Windows.Forms.RadioButton _useSelectedAttributesForObjectTag;
        private System.Windows.Forms.RadioButton _useSpecifiedValueForObjectTag;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox _objectTagName;
        private Utilities.Forms.ConfigurableObjectControl _objectTagAttributeSelector;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox _delimiter;
        private System.Windows.Forms.CheckBox _noTagsIfEmptyCheckBox;
        private System.Windows.Forms.CheckBox _generateSourceAttributesWithRSDCheckBox;
        private System.Windows.Forms.GroupBox _rsdFileGroupBox;
        private FileActionManager.Forms.FileActionManagerPathTagButton _rsdFileNamePathTagsButton;
        private System.Windows.Forms.TextBox _rsdFileNameTextBox;
        private Utilities.Forms.BrowseButton _rsdFileNameBrowseButton;
    }
}
