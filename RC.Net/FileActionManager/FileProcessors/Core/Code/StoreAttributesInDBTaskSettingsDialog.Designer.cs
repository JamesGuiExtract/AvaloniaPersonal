namespace Extract.FileActionManager.FileProcessors
{
    partial class StoreAttributesInDBTaskSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StoreAttributesInDBTaskSettingsDialog));
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._voaFileNameTextBox = new System.Windows.Forms.TextBox();
            this._attributeSetNameComboBox = new System.Windows.Forms.ComboBox();
            this._voaFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._storeRasterZonesCheckBox = new System.Windows.Forms.CheckBox();
            this._attributeSetNamePathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._voaFileNamePathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(154, 13);
            label1.TabIndex = 0;
            label1.Text = "VOA file with attributes to store ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 48);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(92, 13);
            label2.TabIndex = 4;
            label2.Text = "Attribute set name";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(360, 118);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 9;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(279, 118);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 8;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _voaFileNameTextBox
            // 
            this._voaFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFileNameTextBox.Location = new System.Drawing.Point(13, 25);
            this._voaFileNameTextBox.Name = "_voaFileNameTextBox";
            this._voaFileNameTextBox.Size = new System.Drawing.Size(366, 20);
            this._voaFileNameTextBox.TabIndex = 1;
            // 
            // _attributeSetNameComboBox
            // 
            this._attributeSetNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeSetNameComboBox.FormattingEnabled = true;
            this._attributeSetNameComboBox.Location = new System.Drawing.Point(15, 64);
            this._attributeSetNameComboBox.Name = "_attributeSetNameComboBox";
            this._attributeSetNameComboBox.Size = new System.Drawing.Size(364, 21);
            this._attributeSetNameComboBox.TabIndex = 5;
            this._attributeSetNameComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleAttributeSetNameComboBox_SelectedIndexChanged);
            // 
            // _voaFileNameBrowseButton
            // 
            this._voaFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFileNameBrowseButton.FileFilter = "XSD Files (*.xsd)|*.xsd|All files (*.*)|*.*";
            this._voaFileNameBrowseButton.Location = new System.Drawing.Point(409, 24);
            this._voaFileNameBrowseButton.Name = "_voaFileNameBrowseButton";
            this._voaFileNameBrowseButton.Size = new System.Drawing.Size(26, 21);
            this._voaFileNameBrowseButton.TabIndex = 3;
            this._voaFileNameBrowseButton.Text = "...";
            this._voaFileNameBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _storeRasterZonesCheckBox
            // 
            this._storeRasterZonesCheckBox.AutoSize = true;
            this._storeRasterZonesCheckBox.Location = new System.Drawing.Point(15, 90);
            this._storeRasterZonesCheckBox.Name = "_storeRasterZonesCheckBox";
            this._storeRasterZonesCheckBox.Size = new System.Drawing.Size(204, 17);
            this._storeRasterZonesCheckBox.TabIndex = 7;
            this._storeRasterZonesCheckBox.Text = "Store spatial information (raster zones)";
            this._storeRasterZonesCheckBox.UseVisualStyleBackColor = true;
            // 
            // _attributeSetNamePathTagButton
            // 
            this._attributeSetNamePathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeSetNamePathTagButton.ComboBox = this._attributeSetNameComboBox;
            this._attributeSetNamePathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_attributeSetNamePathTagButton.Image")));
            this._attributeSetNamePathTagButton.Location = new System.Drawing.Point(385, 64);
            this._attributeSetNamePathTagButton.Name = "_attributeSetNamePathTagButton";
            this._attributeSetNamePathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._attributeSetNamePathTagButton.Size = new System.Drawing.Size(18, 21);
            this._attributeSetNamePathTagButton.TabIndex = 6;
            this._attributeSetNamePathTagButton.UseVisualStyleBackColor = true;
            // 
            // _voaFileNamePathTagButton
            // 
            this._voaFileNamePathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFileNamePathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_voaFileNamePathTagButton.Image")));
            this._voaFileNamePathTagButton.Location = new System.Drawing.Point(385, 25);
            this._voaFileNamePathTagButton.Name = "_voaFileNamePathTagButton";
            this._voaFileNamePathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._voaFileNamePathTagButton.Size = new System.Drawing.Size(18, 21);
            this._voaFileNamePathTagButton.TabIndex = 2;
            this._voaFileNamePathTagButton.TextControl = this._voaFileNameTextBox;
            this._voaFileNamePathTagButton.UseVisualStyleBackColor = true;
            // 
            // StoreAttributesInDBTaskSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(447, 153);
            this.Controls.Add(this._attributeSetNameComboBox);
            this.Controls.Add(this._storeRasterZonesCheckBox);
            this.Controls.Add(label2);
            this.Controls.Add(this._voaFileNameBrowseButton);
            this.Controls.Add(this._attributeSetNamePathTagButton);
            this.Controls.Add(this._voaFileNameTextBox);
            this.Controls.Add(this._voaFileNamePathTagButton);
            this.Controls.Add(label1);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 191);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(463, 191);
            this.Name = "StoreAttributesInDBTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Core: Store attributes in DB";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TextBox _voaFileNameTextBox;
        private Forms.FileActionManagerPathTagButton _voaFileNamePathTagButton;
        private Forms.FileActionManagerPathTagButton _attributeSetNamePathTagButton;
        private Utilities.Forms.BrowseButton _voaFileNameBrowseButton;
        private System.Windows.Forms.CheckBox _storeRasterZonesCheckBox;
        private System.Windows.Forms.ComboBox _attributeSetNameComboBox;
    }
}