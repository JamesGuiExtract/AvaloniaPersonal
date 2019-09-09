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
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label7;
            Extract.Utilities.Forms.InfoTip infoTip1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoPaginateTaskSettingsDialog));
            System.Windows.Forms.Label label8;
            this._sourceIfFullyPaginatedActionLabel = new System.Windows.Forms.Label();
            this._sourceIfFullyPaginatedActionComboBox = new System.Windows.Forms.ComboBox();
            this._outputActionComboBox = new System.Windows.Forms.ComboBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._autoRotateCheckBox = new System.Windows.Forms.CheckBox();
            this._sourceIfNotFullyPaginatedActionComboBox = new System.Windows.Forms.ComboBox();
            this._autoPaginatedTagComboBox = new System.Windows.Forms.ComboBox();
            this._documentDataAssemblyPathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._documentDataAssemblyTextBox = new Extract.Utilities.Forms.BetterTextBox();
            this._documentDataAssemblyBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._outputPathPathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._outputPathTextBox = new Extract.Utilities.Forms.BetterTextBox();
            this._outputPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._newDocumentsGroupBox = new System.Windows.Forms.GroupBox();
            this._sourceDocumentsGroupBox = new System.Windows.Forms.GroupBox();
            this._processInputDocumentsGroupBox = new System.Windows.Forms.GroupBox();
            this._inputPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._inputPathTextBox = new Extract.Utilities.Forms.BetterTextBox();
            this._inputPathPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._qualifierConditionConfigurableObjectControl = new Extract.Utilities.Forms.ConfigurableObjectControl();
            this._outputQualifiedDocumentsCheckBox = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            infoTip1 = new Extract.Utilities.Forms.InfoTip();
            label8 = new System.Windows.Forms.Label();
            this._newDocumentsGroupBox.SuspendLayout();
            this._sourceDocumentsGroupBox.SuspendLayout();
            this._processInputDocumentsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 22);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(260, 13);
            label1.TabIndex = 0;
            label1.Text = "Output document path (path for paginated image files)";
            // 
            // _sourceIfFullyPaginatedActionLabel
            // 
            this._sourceIfFullyPaginatedActionLabel.AutoSize = true;
            this._sourceIfFullyPaginatedActionLabel.Location = new System.Drawing.Point(3, 24);
            this._sourceIfFullyPaginatedActionLabel.Name = "_sourceIfFullyPaginatedActionLabel";
            this._sourceIfFullyPaginatedActionLabel.Size = new System.Drawing.Size(263, 13);
            this._sourceIfFullyPaginatedActionLabel.TabIndex = 5;
            this._sourceIfFullyPaginatedActionLabel.Text = "Queue fully-paginated source documents to this action";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(6, 103);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(216, 13);
            label3.TabIndex = 6;
            label3.Text = "Set new documents to pending in this action";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(6, 75);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(382, 13);
            label4.TabIndex = 4;
            label4.Text = "DEP (a data entry configuration file to translate and validate the document data)" +
    "";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(3, 51);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(281, 13);
            label5.TabIndex = 7;
            label5.Text = "Queue not-fully-paginated source documents to this action";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(6, 133);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(492, 13);
            label6.TabIndex = 8;
            label6.Text = "Filter predicate (a condition that, when it evaluates as true, qualifies a propos" +
    "ed document to be output)";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(6, 130);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(173, 13);
            label7.TabIndex = 8;
            label7.Text = "Apply this tag to all new documents";
            // 
            // infoTip1
            // 
            infoTip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip1.BackColor = System.Drawing.Color.Transparent;
            infoTip1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip1.BackgroundImage")));
            infoTip1.Location = new System.Drawing.Point(614, 17);
            infoTip1.Name = "infoTip1";
            infoTip1.Size = new System.Drawing.Size(16, 16);
            infoTip1.TabIndex = 2;
            infoTip1.TabStop = false;
            infoTip1.TipText = resources.GetString("infoTip1.TipText");
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(6, 23);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(162, 13);
            label8.TabIndex = 0;
            label8.Text = "Input data path (path of VOA file)";
            // 
            // _sourceIfFullyPaginatedActionComboBox
            // 
            this._sourceIfFullyPaginatedActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceIfFullyPaginatedActionComboBox.FormattingEnabled = true;
            this._sourceIfFullyPaginatedActionComboBox.Location = new System.Drawing.Point(306, 21);
            this._sourceIfFullyPaginatedActionComboBox.Name = "_sourceIfFullyPaginatedActionComboBox";
            this._sourceIfFullyPaginatedActionComboBox.Size = new System.Drawing.Size(358, 21);
            this._sourceIfFullyPaginatedActionComboBox.TabIndex = 6;
            this._sourceIfFullyPaginatedActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _outputActionComboBox
            // 
            this._outputActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputActionComboBox.FormattingEnabled = true;
            this._outputActionComboBox.Location = new System.Drawing.Point(243, 100);
            this._outputActionComboBox.Name = "_outputActionComboBox";
            this._outputActionComboBox.Size = new System.Drawing.Size(421, 21);
            this._outputActionComboBox.TabIndex = 7;
            this._outputActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(599, 535);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 5;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(518, 535);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 4;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _autoRotateCheckBox
            // 
            this._autoRotateCheckBox.AutoSize = true;
            this._autoRotateCheckBox.Location = new System.Drawing.Point(6, 72);
            this._autoRotateCheckBox.Name = "_autoRotateCheckBox";
            this._autoRotateCheckBox.Size = new System.Drawing.Size(199, 17);
            this._autoRotateCheckBox.TabIndex = 5;
            this._autoRotateCheckBox.Text = "Automatically rotate pages to vertical";
            this._autoRotateCheckBox.UseVisualStyleBackColor = true;
            // 
            // _sourceIfNotFullyPaginatedActionComboBox
            // 
            this._sourceIfNotFullyPaginatedActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceIfNotFullyPaginatedActionComboBox.FormattingEnabled = true;
            this._sourceIfNotFullyPaginatedActionComboBox.Location = new System.Drawing.Point(306, 48);
            this._sourceIfNotFullyPaginatedActionComboBox.Name = "_sourceIfNotFullyPaginatedActionComboBox";
            this._sourceIfNotFullyPaginatedActionComboBox.Size = new System.Drawing.Size(358, 21);
            this._sourceIfNotFullyPaginatedActionComboBox.TabIndex = 8;
            this._sourceIfNotFullyPaginatedActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _autoPaginatedTagComboBox
            // 
            this._autoPaginatedTagComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._autoPaginatedTagComboBox.FormattingEnabled = true;
            this._autoPaginatedTagComboBox.Location = new System.Drawing.Point(243, 127);
            this._autoPaginatedTagComboBox.Name = "_autoPaginatedTagComboBox";
            this._autoPaginatedTagComboBox.Size = new System.Drawing.Size(421, 21);
            this._autoPaginatedTagComboBox.TabIndex = 9;
            // 
            // _documentDataAssemblyPathTags
            // 
            this._documentDataAssemblyPathTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._documentDataAssemblyPathTags.Image = ((System.Drawing.Image)(resources.GetObject("_documentDataAssemblyPathTags.Image")));
            this._documentDataAssemblyPathTags.Location = new System.Drawing.Point(612, 97);
            this._documentDataAssemblyPathTags.Name = "_documentDataAssemblyPathTags";
            this._documentDataAssemblyPathTags.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._documentDataAssemblyPathTags.Size = new System.Drawing.Size(18, 21);
            this._documentDataAssemblyPathTags.TabIndex = 6;
            this._documentDataAssemblyPathTags.TextControl = this._documentDataAssemblyTextBox;
            this._documentDataAssemblyPathTags.UseVisualStyleBackColor = true;
            // 
            // _documentDataAssemblyTextBox
            // 
            this._documentDataAssemblyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._documentDataAssemblyTextBox.Location = new System.Drawing.Point(6, 98);
            this._documentDataAssemblyTextBox.Name = "_documentDataAssemblyTextBox";
            this._documentDataAssemblyTextBox.Size = new System.Drawing.Size(600, 20);
            this._documentDataAssemblyTextBox.TabIndex = 5;
            // 
            // _documentDataAssemblyBrowseButton
            // 
            this._documentDataAssemblyBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._documentDataAssemblyBrowseButton.EnsureFileExists = false;
            this._documentDataAssemblyBrowseButton.EnsurePathExists = false;
            this._documentDataAssemblyBrowseButton.Location = new System.Drawing.Point(637, 97);
            this._documentDataAssemblyBrowseButton.Name = "_documentDataAssemblyBrowseButton";
            this._documentDataAssemblyBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._documentDataAssemblyBrowseButton.TabIndex = 7;
            this._documentDataAssemblyBrowseButton.Text = "...";
            this._documentDataAssemblyBrowseButton.TextControl = this._documentDataAssemblyTextBox;
            this._documentDataAssemblyBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _outputPathPathTags
            // 
            this._outputPathPathTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathPathTags.Image = ((System.Drawing.Image)(resources.GetObject("_outputPathPathTags.Image")));
            this._outputPathPathTags.Location = new System.Drawing.Point(613, 39);
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
            this._outputPathTextBox.Location = new System.Drawing.Point(6, 40);
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
            this._outputPathBrowseButton.Location = new System.Drawing.Point(637, 39);
            this._outputPathBrowseButton.Name = "_outputPathBrowseButton";
            this._outputPathBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._outputPathBrowseButton.TabIndex = 4;
            this._outputPathBrowseButton.Text = "...";
            this._outputPathBrowseButton.TextControl = this._outputPathTextBox;
            this._outputPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _newDocumentsGroupBox
            // 
            this._newDocumentsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._newDocumentsGroupBox.Controls.Add(this._autoPaginatedTagComboBox);
            this._newDocumentsGroupBox.Controls.Add(label1);
            this._newDocumentsGroupBox.Controls.Add(label3);
            this._newDocumentsGroupBox.Controls.Add(label7);
            this._newDocumentsGroupBox.Controls.Add(this._outputActionComboBox);
            this._newDocumentsGroupBox.Controls.Add(this._outputPathBrowseButton);
            this._newDocumentsGroupBox.Controls.Add(this._outputPathPathTags);
            this._newDocumentsGroupBox.Controls.Add(infoTip1);
            this._newDocumentsGroupBox.Controls.Add(this._autoRotateCheckBox);
            this._newDocumentsGroupBox.Controls.Add(this._outputPathTextBox);
            this._newDocumentsGroupBox.Location = new System.Drawing.Point(9, 255);
            this._newDocumentsGroupBox.Name = "_newDocumentsGroupBox";
            this._newDocumentsGroupBox.Size = new System.Drawing.Size(671, 164);
            this._newDocumentsGroupBox.TabIndex = 2;
            this._newDocumentsGroupBox.TabStop = false;
            this._newDocumentsGroupBox.Text = "New documents";
            // 
            // _sourceDocumentsGroupBox
            // 
            this._sourceDocumentsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceDocumentsGroupBox.Controls.Add(this._sourceIfFullyPaginatedActionLabel);
            this._sourceDocumentsGroupBox.Controls.Add(this._sourceIfFullyPaginatedActionComboBox);
            this._sourceDocumentsGroupBox.Controls.Add(label5);
            this._sourceDocumentsGroupBox.Controls.Add(this._sourceIfNotFullyPaginatedActionComboBox);
            this._sourceDocumentsGroupBox.Location = new System.Drawing.Point(9, 425);
            this._sourceDocumentsGroupBox.Name = "_sourceDocumentsGroupBox";
            this._sourceDocumentsGroupBox.Size = new System.Drawing.Size(671, 88);
            this._sourceDocumentsGroupBox.TabIndex = 3;
            this._sourceDocumentsGroupBox.TabStop = false;
            this._sourceDocumentsGroupBox.Text = "Source document";
            // 
            // _processInputDocumentsGroupBox
            // 
            this._processInputDocumentsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._processInputDocumentsGroupBox.Controls.Add(label8);
            this._processInputDocumentsGroupBox.Controls.Add(this._inputPathBrowseButton);
            this._processInputDocumentsGroupBox.Controls.Add(this._inputPathPathTagButton);
            this._processInputDocumentsGroupBox.Controls.Add(this._inputPathTextBox);
            this._processInputDocumentsGroupBox.Controls.Add(this._qualifierConditionConfigurableObjectControl);
            this._processInputDocumentsGroupBox.Controls.Add(label4);
            this._processInputDocumentsGroupBox.Controls.Add(this._documentDataAssemblyTextBox);
            this._processInputDocumentsGroupBox.Controls.Add(this._documentDataAssemblyBrowseButton);
            this._processInputDocumentsGroupBox.Controls.Add(this._documentDataAssemblyPathTags);
            this._processInputDocumentsGroupBox.Controls.Add(label6);
            this._processInputDocumentsGroupBox.Location = new System.Drawing.Point(9, 3);
            this._processInputDocumentsGroupBox.Name = "_processInputDocumentsGroupBox";
            this._processInputDocumentsGroupBox.Size = new System.Drawing.Size(670, 215);
            this._processInputDocumentsGroupBox.TabIndex = 0;
            this._processInputDocumentsGroupBox.TabStop = false;
            this._processInputDocumentsGroupBox.Text = "Process input documents";
            // 
            // _inputPathBrowseButton
            // 
            this._inputPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._inputPathBrowseButton.EnsureFileExists = false;
            this._inputPathBrowseButton.EnsurePathExists = false;
            this._inputPathBrowseButton.Location = new System.Drawing.Point(637, 40);
            this._inputPathBrowseButton.Name = "_inputPathBrowseButton";
            this._inputPathBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._inputPathBrowseButton.TabIndex = 3;
            this._inputPathBrowseButton.Text = "...";
            this._inputPathBrowseButton.TextControl = this._inputPathTextBox;
            this._inputPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _inputPathTextBox
            // 
            this._inputPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._inputPathTextBox.Location = new System.Drawing.Point(6, 41);
            this._inputPathTextBox.Name = "_inputPathTextBox";
            this._inputPathTextBox.Required = true;
            this._inputPathTextBox.Size = new System.Drawing.Size(601, 20);
            this._inputPathTextBox.TabIndex = 1;
            // 
            // _inputPathPathTagButton
            // 
            this._inputPathPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._inputPathPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_inputPathPathTagButton.Image")));
            this._inputPathPathTagButton.Location = new System.Drawing.Point(613, 40);
            this._inputPathPathTagButton.Name = "_inputPathPathTagButton";
            this._inputPathPathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._inputPathPathTagButton.Size = new System.Drawing.Size(18, 21);
            this._inputPathPathTagButton.TabIndex = 2;
            this._inputPathPathTagButton.TextControl = this._inputPathTextBox;
            this._inputPathPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _qualifierConditionConfigurableObjectControl
            // 
            this._qualifierConditionConfigurableObjectControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._qualifierConditionConfigurableObjectControl.CategoryName = "Extract Pagination Conditions";
            this._qualifierConditionConfigurableObjectControl.Location = new System.Drawing.Point(6, 157);
            this._qualifierConditionConfigurableObjectControl.Margin = new System.Windows.Forms.Padding(0);
            this._qualifierConditionConfigurableObjectControl.Name = "_qualifierConditionConfigurableObjectControl";
            this._qualifierConditionConfigurableObjectControl.ShowNoneOption = true;
            this._qualifierConditionConfigurableObjectControl.Size = new System.Drawing.Size(658, 49);
            this._qualifierConditionConfigurableObjectControl.TabIndex = 9;
            this._qualifierConditionConfigurableObjectControl.SelectObjectTypeChanged += new System.EventHandler(this.HandleQualifier_SelectedObjectTypeChanged);
            // 
            // _outputQualifiedDocumentsCheckBox
            // 
            this._outputQualifiedDocumentsCheckBox.AutoSize = true;
            this._outputQualifiedDocumentsCheckBox.Location = new System.Drawing.Point(9, 232);
            this._outputQualifiedDocumentsCheckBox.Name = "_outputQualifiedDocumentsCheckBox";
            this._outputQualifiedDocumentsCheckBox.Size = new System.Drawing.Size(155, 17);
            this._outputQualifiedDocumentsCheckBox.TabIndex = 1;
            this._outputQualifiedDocumentsCheckBox.Text = "Output qualified documents";
            this._outputQualifiedDocumentsCheckBox.UseVisualStyleBackColor = true;
            this._outputQualifiedDocumentsCheckBox.CheckedChanged += new System.EventHandler(this.HandleOutputQualifiedDocumentsCheckBox_CheckedChanged);
            // 
            // AutoPaginateTaskSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(686, 571);
            this.Controls.Add(this._outputQualifiedDocumentsCheckBox);
            this.Controls.Add(this._processInputDocumentsGroupBox);
            this.Controls.Add(this._sourceDocumentsGroupBox);
            this.Controls.Add(this._newDocumentsGroupBox);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(702, 600);
            this.Name = "AutoPaginateTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Auto Paginate Settings";
            this._newDocumentsGroupBox.ResumeLayout(false);
            this._newDocumentsGroupBox.PerformLayout();
            this._sourceDocumentsGroupBox.ResumeLayout(false);
            this._sourceDocumentsGroupBox.PerformLayout();
            this._processInputDocumentsGroupBox.ResumeLayout(false);
            this._processInputDocumentsGroupBox.PerformLayout();
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
        private System.Windows.Forms.ComboBox _autoPaginatedTagComboBox;
        private System.Windows.Forms.GroupBox _newDocumentsGroupBox;
        private System.Windows.Forms.GroupBox _sourceDocumentsGroupBox;
        private System.Windows.Forms.GroupBox _processInputDocumentsGroupBox;
        private Utilities.Forms.ConfigurableObjectControl _qualifierConditionConfigurableObjectControl;
        private System.Windows.Forms.CheckBox _outputQualifiedDocumentsCheckBox;
        private Utilities.Forms.BrowseButton _inputPathBrowseButton;
        private Utilities.Forms.BetterTextBox _inputPathTextBox;
        private Forms.FileActionManagerPathTagButton _inputPathPathTagButton;
        private System.Windows.Forms.Label _sourceIfFullyPaginatedActionLabel;
    }
}