namespace Extract.FileActionManager.FileProcessors
{
    partial class AutoPaginateTaskSettingsDialog
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label7;
            Extract.Utilities.Forms.InfoTip infoTip1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoPaginateTaskSettingsDialog));
            this._sourceIfFullyPaginatedActionComboBox = new System.Windows.Forms.ComboBox();
            this._outputActionComboBox = new System.Windows.Forms.ComboBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._autoRotateCheckBox = new System.Windows.Forms.CheckBox();
            this._sourceIfNotFullyPaginatedActionComboBox = new System.Windows.Forms.ComboBox();
            this._autoPaginatedTagComboBox = new System.Windows.Forms.ComboBox();
            this._qualifierConditionConfigurableObjectControl = new Extract.Utilities.Forms.ConfigurableObjectControl();
            this._documentDataAssemblyPathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._documentDataAssemblyTextBox = new Extract.Utilities.Forms.BetterTextBox();
            this._documentDataAssemblyBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._outputPathPathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._outputPathTextBox = new Extract.Utilities.Forms.BetterTextBox();
            this._outputPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            infoTip1 = new Extract.Utilities.Forms.InfoTip();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(162, 13);
            label1.TabIndex = 0;
            label1.Text = "Paginated document output path";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(13, 58);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(572, 13);
            label2.TabIndex = 5;
            label2.Text = "After committing documents, set the original source document to pending in this a" +
    "ction if it is fully paginated automatically";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(13, 150);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(276, 13);
            label3.TabIndex = 9;
            label3.Text = "Set paginated output documents to pending in this action";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(13, 196);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(213, 13);
            label4.TabIndex = 11;
            label4.Text = "Specify a data entry configuration file to use";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(13, 103);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(632, 13);
            label5.TabIndex = 7;
            label5.Text = "After committing documents, set the original source document to pending in this a" +
    "ction if it could NOT be fully paginated automatically";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(13, 243);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(452, 13);
            label6.TabIndex = 15;
            label6.Text = "Specify a condition that, when it evaluates as true, qualifies a proposed documen" +
    "t to be output";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(13, 311);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(361, 13);
            label7.TabIndex = 20;
            label7.Text = "Specify a tag to be applied to all output documents generated automatically";
            // 
            // infoTip1
            // 
            infoTip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip1.BackColor = System.Drawing.Color.Transparent;
            infoTip1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip1.BackgroundImage")));
            infoTip1.Location = new System.Drawing.Point(624, 6);
            infoTip1.Name = "infoTip1";
            infoTip1.Size = new System.Drawing.Size(16, 16);
            infoTip1.TabIndex = 2;
            infoTip1.TabStop = false;
            infoTip1.TipText = resources.GetString("infoTip1.TipText");
            // 
            // _sourceIfFullyPaginatedActionComboBox
            // 
            this._sourceIfFullyPaginatedActionComboBox.FormattingEnabled = true;
            this._sourceIfFullyPaginatedActionComboBox.Location = new System.Drawing.Point(16, 75);
            this._sourceIfFullyPaginatedActionComboBox.Name = "_sourceIfFullyPaginatedActionComboBox";
            this._sourceIfFullyPaginatedActionComboBox.Size = new System.Drawing.Size(233, 21);
            this._sourceIfFullyPaginatedActionComboBox.TabIndex = 6;
            this._sourceIfFullyPaginatedActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _outputActionComboBox
            // 
            this._outputActionComboBox.FormattingEnabled = true;
            this._outputActionComboBox.Location = new System.Drawing.Point(16, 166);
            this._outputActionComboBox.Name = "_outputActionComboBox";
            this._outputActionComboBox.Size = new System.Drawing.Size(233, 21);
            this._outputActionComboBox.TabIndex = 10;
            this._outputActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(599, 355);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 24;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(518, 355);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 23;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _autoRotateCheckBox
            // 
            this._autoRotateCheckBox.AutoSize = true;
            this._autoRotateCheckBox.Location = new System.Drawing.Point(16, 356);
            this._autoRotateCheckBox.Name = "_autoRotateCheckBox";
            this._autoRotateCheckBox.Size = new System.Drawing.Size(199, 17);
            this._autoRotateCheckBox.TabIndex = 22;
            this._autoRotateCheckBox.Text = "Automatically rotate pages to vertical";
            this._autoRotateCheckBox.UseVisualStyleBackColor = true;
            // 
            // _sourceIfNotFullyPaginatedActionComboBox
            // 
            this._sourceIfNotFullyPaginatedActionComboBox.FormattingEnabled = true;
            this._sourceIfNotFullyPaginatedActionComboBox.Location = new System.Drawing.Point(16, 120);
            this._sourceIfNotFullyPaginatedActionComboBox.Name = "_sourceIfNotFullyPaginatedActionComboBox";
            this._sourceIfNotFullyPaginatedActionComboBox.Size = new System.Drawing.Size(233, 21);
            this._sourceIfNotFullyPaginatedActionComboBox.TabIndex = 8;
            // 
            // _autoPaginatedTagComboBox
            // 
            this._autoPaginatedTagComboBox.FormattingEnabled = true;
            this._autoPaginatedTagComboBox.Location = new System.Drawing.Point(16, 327);
            this._autoPaginatedTagComboBox.Name = "_autoPaginatedTagComboBox";
            this._autoPaginatedTagComboBox.Size = new System.Drawing.Size(233, 21);
            this._autoPaginatedTagComboBox.TabIndex = 21;
            // 
            // _qualifierConditionConfigurableObjectControl
            // 
            this._qualifierConditionConfigurableObjectControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._qualifierConditionConfigurableObjectControl.CategoryName = "Extract Pagination Conditions";
            this._qualifierConditionConfigurableObjectControl.Location = new System.Drawing.Point(11, 259);
            this._qualifierConditionConfigurableObjectControl.Margin = new System.Windows.Forms.Padding(0);
            this._qualifierConditionConfigurableObjectControl.Name = "_qualifierConditionConfigurableObjectControl";
            this._qualifierConditionConfigurableObjectControl.ShowNoneOption = true;
            this._qualifierConditionConfigurableObjectControl.Size = new System.Drawing.Size(671, 49);
            this._qualifierConditionConfigurableObjectControl.TabIndex = 16;
            this._qualifierConditionConfigurableObjectControl.SelectObjectTypeChanged += new System.EventHandler(this.HandleQualifier_SelectedObjectTypeChanged);
            // 
            // _documentDataAssemblyPathTags
            // 
            this._documentDataAssemblyPathTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._documentDataAssemblyPathTags.Image = ((System.Drawing.Image)(resources.GetObject("_documentDataAssemblyPathTags.Image")));
            this._documentDataAssemblyPathTags.Location = new System.Drawing.Point(623, 211);
            this._documentDataAssemblyPathTags.Name = "_documentDataAssemblyPathTags";
            this._documentDataAssemblyPathTags.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._documentDataAssemblyPathTags.Size = new System.Drawing.Size(18, 21);
            this._documentDataAssemblyPathTags.TabIndex = 13;
            this._documentDataAssemblyPathTags.TextControl = this._documentDataAssemblyTextBox;
            this._documentDataAssemblyPathTags.UseVisualStyleBackColor = true;
            // 
            // _documentDataAssemblyTextBox
            // 
            this._documentDataAssemblyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._documentDataAssemblyTextBox.Location = new System.Drawing.Point(16, 212);
            this._documentDataAssemblyTextBox.Name = "_documentDataAssemblyTextBox";
            this._documentDataAssemblyTextBox.Size = new System.Drawing.Size(601, 20);
            this._documentDataAssemblyTextBox.TabIndex = 12;
            // 
            // _documentDataAssemblyBrowseButton
            // 
            this._documentDataAssemblyBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._documentDataAssemblyBrowseButton.EnsureFileExists = false;
            this._documentDataAssemblyBrowseButton.EnsurePathExists = false;
            this._documentDataAssemblyBrowseButton.Location = new System.Drawing.Point(647, 211);
            this._documentDataAssemblyBrowseButton.Name = "_documentDataAssemblyBrowseButton";
            this._documentDataAssemblyBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._documentDataAssemblyBrowseButton.TabIndex = 14;
            this._documentDataAssemblyBrowseButton.Text = "...";
            this._documentDataAssemblyBrowseButton.TextControl = this._documentDataAssemblyTextBox;
            this._documentDataAssemblyBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _outputPathPathTags
            // 
            this._outputPathPathTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathPathTags.Image = ((System.Drawing.Image)(resources.GetObject("_outputPathPathTags.Image")));
            this._outputPathPathTags.Location = new System.Drawing.Point(623, 28);
            this._outputPathPathTags.Name = "_outputPathPathTags";
            this._outputPathPathTags.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._outputPathPathTags.Size = new System.Drawing.Size(18, 21);
            this._outputPathPathTags.TabIndex = 3;
            this._outputPathPathTags.TextControl = this._outputPathTextBox;
            this._outputPathPathTags.UseVisualStyleBackColor = true;
            // 
            // _outputPathTextBox
            // 
            this._outputPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathTextBox.Location = new System.Drawing.Point(16, 29);
            this._outputPathTextBox.Name = "_outputPathTextBox";
            this._outputPathTextBox.Required = true;
            this._outputPathTextBox.Size = new System.Drawing.Size(601, 20);
            this._outputPathTextBox.TabIndex = 1;
            // 
            // _outputPathBrowseButton
            // 
            this._outputPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathBrowseButton.EnsureFileExists = false;
            this._outputPathBrowseButton.EnsurePathExists = false;
            this._outputPathBrowseButton.Location = new System.Drawing.Point(647, 28);
            this._outputPathBrowseButton.Name = "_outputPathBrowseButton";
            this._outputPathBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._outputPathBrowseButton.TabIndex = 4;
            this._outputPathBrowseButton.Text = "...";
            this._outputPathBrowseButton.TextControl = this._outputPathTextBox;
            this._outputPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // AutoPaginateTaskSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(686, 391);
            this.Controls.Add(this._autoPaginatedTagComboBox);
            this.Controls.Add(label7);
            this.Controls.Add(label6);
            this.Controls.Add(this._qualifierConditionConfigurableObjectControl);
            this.Controls.Add(this._sourceIfNotFullyPaginatedActionComboBox);
            this.Controls.Add(label5);
            this.Controls.Add(this._autoRotateCheckBox);
            this.Controls.Add(this._documentDataAssemblyPathTags);
            this.Controls.Add(this._documentDataAssemblyBrowseButton);
            this.Controls.Add(this._documentDataAssemblyTextBox);
            this.Controls.Add(label4);
            this.Controls.Add(infoTip1);
            this.Controls.Add(this._outputPathPathTags);
            this.Controls.Add(this._outputPathBrowseButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._outputActionComboBox);
            this.Controls.Add(label3);
            this.Controls.Add(this._sourceIfFullyPaginatedActionComboBox);
            this.Controls.Add(label2);
            this.Controls.Add(this._outputPathTextBox);
            this.Controls.Add(label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(702, 430);
            this.Name = "AutoPaginateTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Auto Paginate Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Extract.Utilities.Forms.BetterTextBox _outputPathTextBox;
        private System.Windows.Forms.ComboBox _sourceIfFullyPaginatedActionComboBox;
        private System.Windows.Forms.ComboBox _outputActionComboBox;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _outputPathPathTags;
        private Utilities.Forms.BrowseButton _outputPathBrowseButton;
        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _documentDataAssemblyPathTags;
        private Utilities.Forms.BetterTextBox _documentDataAssemblyTextBox;
        private Utilities.Forms.BrowseButton _documentDataAssemblyBrowseButton;
        private System.Windows.Forms.CheckBox _autoRotateCheckBox;
        private System.Windows.Forms.ComboBox _sourceIfNotFullyPaginatedActionComboBox;
        private Utilities.Forms.ConfigurableObjectControl _qualifierConditionConfigurableObjectControl;
        private System.Windows.Forms.ComboBox _autoPaginatedTagComboBox;
    }
}