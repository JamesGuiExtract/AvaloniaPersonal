namespace Extract.FileActionManager.FileProcessors
{
    partial class ValidateXmlTaskSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ValidateXmlTaskSettingsDialog));
            this._treatWarningsAsErrorCheckBox = new System.Windows.Forms.CheckBox();
            this._schemaFilenameTextBox = new System.Windows.Forms.TextBox();
            this._schemaFileNamePathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._schemaFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._requireInlineSchemaCheckBox = new System.Windows.Forms.CheckBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._xmlFileNamePathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._xmlFileNameTextBox = new System.Windows.Forms.TextBox();
            this._validateInlineSchemaRadioButton = new System.Windows.Forms.RadioButton();
            this._validateSpecifiedSchemaRadioButton = new System.Windows.Forms.RadioButton();
            this._noSchemaValidationRadioButton = new System.Windows.Forms.RadioButton();
            label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(14, 10);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(71, 13);
            label1.TabIndex = 0;
            label1.Text = "XML filename";
            // 
            // _treatWarningsAsErrorCheckBox
            // 
            this._treatWarningsAsErrorCheckBox.AutoSize = true;
            this._treatWarningsAsErrorCheckBox.Location = new System.Drawing.Point(17, 52);
            this._treatWarningsAsErrorCheckBox.Name = "_treatWarningsAsErrorCheckBox";
            this._treatWarningsAsErrorCheckBox.Size = new System.Drawing.Size(139, 17);
            this._treatWarningsAsErrorCheckBox.TabIndex = 3;
            this._treatWarningsAsErrorCheckBox.Text = "Treat warnings as errors";
            this._treatWarningsAsErrorCheckBox.UseVisualStyleBackColor = true;
            // 
            // _schemaFilenameTextBox
            // 
            this._schemaFilenameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._schemaFilenameTextBox.Location = new System.Drawing.Point(15, 144);
            this._schemaFilenameTextBox.Name = "_schemaFilenameTextBox";
            this._schemaFilenameTextBox.Size = new System.Drawing.Size(364, 20);
            this._schemaFilenameTextBox.TabIndex = 7;
            // 
            // _schemaFileNamePathTagButton
            // 
            this._schemaFileNamePathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._schemaFileNamePathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_schemaFileNamePathTagButton.Image")));
            this._schemaFileNamePathTagButton.Location = new System.Drawing.Point(385, 143);
            this._schemaFileNamePathTagButton.Name = "_schemaFileNamePathTagButton";
            this._schemaFileNamePathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._schemaFileNamePathTagButton.Size = new System.Drawing.Size(18, 21);
            this._schemaFileNamePathTagButton.TabIndex = 8;
            this._schemaFileNamePathTagButton.TextControl = this._schemaFilenameTextBox;
            this._schemaFileNamePathTagButton.UseVisualStyleBackColor = true;
            // 
            // _schemaFileNameBrowseButton
            // 
            this._schemaFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._schemaFileNameBrowseButton.FileFilter = "XSD Files (*.xsd)|*.xsd|All files (*.*)|*.*";
            this._schemaFileNameBrowseButton.Location = new System.Drawing.Point(409, 143);
            this._schemaFileNameBrowseButton.Name = "_schemaFileNameBrowseButton";
            this._schemaFileNameBrowseButton.Size = new System.Drawing.Size(26, 21);
            this._schemaFileNameBrowseButton.TabIndex = 9;
            this._schemaFileNameBrowseButton.Text = "...";
            this._schemaFileNameBrowseButton.TextControl = this._schemaFilenameTextBox;
            this._schemaFileNameBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _requireInlineSchemaCheckBox
            // 
            this._requireInlineSchemaCheckBox.AutoSize = true;
            this._requireInlineSchemaCheckBox.Location = new System.Drawing.Point(39, 98);
            this._requireInlineSchemaCheckBox.Name = "_requireInlineSchemaCheckBox";
            this._requireInlineSchemaCheckBox.Size = new System.Drawing.Size(292, 17);
            this._requireInlineSchemaCheckBox.TabIndex = 5;
            this._requireInlineSchemaCheckBox.Text = "Consider XML file invalid if in-line schema is not specified";
            this._requireInlineSchemaCheckBox.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(360, 190);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 12;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(279, 190);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 11;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _xmlFileNamePathTagButton
            // 
            this._xmlFileNamePathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._xmlFileNamePathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_xmlFileNamePathTagButton.Image")));
            this._xmlFileNamePathTagButton.Location = new System.Drawing.Point(417, 25);
            this._xmlFileNamePathTagButton.Name = "_xmlFileNamePathTagButton";
            this._xmlFileNamePathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._xmlFileNamePathTagButton.Size = new System.Drawing.Size(18, 21);
            this._xmlFileNamePathTagButton.TabIndex = 2;
            this._xmlFileNamePathTagButton.TextControl = this._xmlFileNameTextBox;
            this._xmlFileNamePathTagButton.UseVisualStyleBackColor = true;
            // 
            // _xmlFileNameTextBox
            // 
            this._xmlFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._xmlFileNameTextBox.Location = new System.Drawing.Point(15, 26);
            this._xmlFileNameTextBox.Name = "_xmlFileNameTextBox";
            this._xmlFileNameTextBox.Size = new System.Drawing.Size(396, 20);
            this._xmlFileNameTextBox.TabIndex = 1;
            // 
            // _validateInlineSchemaRadioButton
            // 
            this._validateInlineSchemaRadioButton.AutoSize = true;
            this._validateInlineSchemaRadioButton.Location = new System.Drawing.Point(17, 75);
            this._validateInlineSchemaRadioButton.Name = "_validateInlineSchemaRadioButton";
            this._validateInlineSchemaRadioButton.Size = new System.Drawing.Size(223, 17);
            this._validateInlineSchemaRadioButton.TabIndex = 4;
            this._validateInlineSchemaRadioButton.TabStop = true;
            this._validateInlineSchemaRadioButton.Text = "Validate against in-line schema if specified";
            this._validateInlineSchemaRadioButton.UseVisualStyleBackColor = true;
            // 
            // _validateSpecifiedSchemaRadioButton
            // 
            this._validateSpecifiedSchemaRadioButton.AutoSize = true;
            this._validateSpecifiedSchemaRadioButton.Location = new System.Drawing.Point(17, 121);
            this._validateSpecifiedSchemaRadioButton.Name = "_validateSpecifiedSchemaRadioButton";
            this._validateSpecifiedSchemaRadioButton.Size = new System.Drawing.Size(185, 17);
            this._validateSpecifiedSchemaRadioButton.TabIndex = 6;
            this._validateSpecifiedSchemaRadioButton.TabStop = true;
            this._validateSpecifiedSchemaRadioButton.Text = "Validate against specified schema";
            this._validateSpecifiedSchemaRadioButton.UseVisualStyleBackColor = true;
            // 
            // _noSchemaValidationRadioButton
            // 
            this._noSchemaValidationRadioButton.AutoSize = true;
            this._noSchemaValidationRadioButton.Location = new System.Drawing.Point(17, 170);
            this._noSchemaValidationRadioButton.Name = "_noSchemaValidationRadioButton";
            this._noSchemaValidationRadioButton.Size = new System.Drawing.Size(259, 17);
            this._noSchemaValidationRadioButton.TabIndex = 10;
            this._noSchemaValidationRadioButton.TabStop = true;
            this._noSchemaValidationRadioButton.Text = "Validate XML syntax only; do not validate schema";
            this._noSchemaValidationRadioButton.UseVisualStyleBackColor = true;
            // 
            // ValidateXmlTaskSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(447, 225);
            this.Controls.Add(this._noSchemaValidationRadioButton);
            this.Controls.Add(this._validateSpecifiedSchemaRadioButton);
            this.Controls.Add(this._validateInlineSchemaRadioButton);
            this.Controls.Add(this._xmlFileNameTextBox);
            this.Controls.Add(this._xmlFileNamePathTagButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._requireInlineSchemaCheckBox);
            this.Controls.Add(this._schemaFileNameBrowseButton);
            this.Controls.Add(this._schemaFileNamePathTagButton);
            this.Controls.Add(this._schemaFilenameTextBox);
            this.Controls.Add(this._treatWarningsAsErrorCheckBox);
            this.Controls.Add(label1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 263);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(375, 263);
            this.Name = "ValidateXmlTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Core: Validate XML settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox _treatWarningsAsErrorCheckBox;
        private System.Windows.Forms.TextBox _schemaFilenameTextBox;
        private Forms.FileActionManagerPathTagButton _schemaFileNamePathTagButton;
        private Extract.Utilities.Forms.BrowseButton _schemaFileNameBrowseButton;
        private System.Windows.Forms.CheckBox _requireInlineSchemaCheckBox;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private Forms.FileActionManagerPathTagButton _xmlFileNamePathTagButton;
        private System.Windows.Forms.TextBox _xmlFileNameTextBox;
        private System.Windows.Forms.RadioButton _validateInlineSchemaRadioButton;
        private System.Windows.Forms.RadioButton _validateSpecifiedSchemaRadioButton;
        private System.Windows.Forms.RadioButton _noSchemaValidationRadioButton;
    }
}