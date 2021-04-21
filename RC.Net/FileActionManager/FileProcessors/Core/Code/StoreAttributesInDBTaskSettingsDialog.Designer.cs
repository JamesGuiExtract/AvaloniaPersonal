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
            this._storeRasterZonesCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._RetrieveRadioButton = new System.Windows.Forms.RadioButton();
            this._StoreRadioButton = new System.Windows.Forms.RadioButton();
            this._doNotSaveEmptyCheckBox = new System.Windows.Forms.CheckBox();
            this._voaFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._voaFileNamePathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._storeDiscreteDataCheckBox = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(113, 13);
            label1.TabIndex = 0;
            label1.Text = "VOA file with attributes";
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
            this._cancelButton.Location = new System.Drawing.Point(361, 253);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 10;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(280, 253);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 9;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _voaFileNameTextBox
            // 
            this._voaFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFileNameTextBox.Location = new System.Drawing.Point(12, 25);
            this._voaFileNameTextBox.Name = "_voaFileNameTextBox";
            this._voaFileNameTextBox.Size = new System.Drawing.Size(367, 20);
            this._voaFileNameTextBox.TabIndex = 0;
            // 
            // _attributeSetNameComboBox
            // 
            this._attributeSetNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeSetNameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._attributeSetNameComboBox.FormattingEnabled = true;
            this._attributeSetNameComboBox.Location = new System.Drawing.Point(12, 64);
            this._attributeSetNameComboBox.Name = "_attributeSetNameComboBox";
            this._attributeSetNameComboBox.Size = new System.Drawing.Size(367, 21);
            this._attributeSetNameComboBox.TabIndex = 3;
            this._attributeSetNameComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleAttributeSetNameComboBox_SelectedIndexChanged);
            // 
            // _storeRasterZonesCheckBox
            // 
            this._storeRasterZonesCheckBox.AutoSize = true;
            this._storeRasterZonesCheckBox.Location = new System.Drawing.Point(12, 201);
            this._storeRasterZonesCheckBox.Name = "_storeRasterZonesCheckBox";
            this._storeRasterZonesCheckBox.Size = new System.Drawing.Size(204, 17);
            this._storeRasterZonesCheckBox.TabIndex = 7;
            this._storeRasterZonesCheckBox.Text = "Store spatial information (raster zones)";
            this._storeRasterZonesCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._RetrieveRadioButton);
            this.groupBox1.Controls.Add(this._StoreRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 97);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(424, 63);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mode";
            // 
            // _RetrieveRadioButton
            // 
            this._RetrieveRadioButton.AutoSize = true;
            this._RetrieveRadioButton.Location = new System.Drawing.Point(9, 38);
            this._RetrieveRadioButton.Name = "_RetrieveRadioButton";
            this._RetrieveRadioButton.Size = new System.Drawing.Size(65, 17);
            this._RetrieveRadioButton.TabIndex = 1;
            this._RetrieveRadioButton.Text = "Retrieve";
            this._RetrieveRadioButton.UseVisualStyleBackColor = true;
            this._RetrieveRadioButton.Click += new System.EventHandler(this.HandleRetrieveRadioButtonClicked);
            // 
            // _StoreRadioButton
            // 
            this._StoreRadioButton.AutoSize = true;
            this._StoreRadioButton.Checked = true;
            this._StoreRadioButton.Location = new System.Drawing.Point(10, 17);
            this._StoreRadioButton.Name = "_StoreRadioButton";
            this._StoreRadioButton.Size = new System.Drawing.Size(50, 17);
            this._StoreRadioButton.TabIndex = 0;
            this._StoreRadioButton.TabStop = true;
            this._StoreRadioButton.Text = "Store";
            this._StoreRadioButton.UseVisualStyleBackColor = true;
            this._StoreRadioButton.Click += new System.EventHandler(this.HandleStoreRadioButtonClicked);
            // 
            // _doNotSaveEmptyCheckBox
            // 
            this._doNotSaveEmptyCheckBox.AutoSize = true;
            this._doNotSaveEmptyCheckBox.Checked = true;
            this._doNotSaveEmptyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._doNotSaveEmptyCheckBox.Location = new System.Drawing.Point(12, 227);
            this._doNotSaveEmptyCheckBox.Name = "_doNotSaveEmptyCheckBox";
            this._doNotSaveEmptyCheckBox.Size = new System.Drawing.Size(161, 17);
            this._doNotSaveEmptyCheckBox.TabIndex = 8;
            this._doNotSaveEmptyCheckBox.Text = "Do not store empty attributes";
            this._doNotSaveEmptyCheckBox.UseVisualStyleBackColor = true;
            // 
            // _voaFileNameBrowseButton
            // 
            this._voaFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFileNameBrowseButton.EnsureFileExists = false;
            this._voaFileNameBrowseButton.EnsurePathExists = false;
            this._voaFileNameBrowseButton.FileFilter = "VOA Files (*.voa;*.evoa;*.eav)|*.voa;*.evoa;*.eav|All files (*.*)|*.*";
            this._voaFileNameBrowseButton.Location = new System.Drawing.Point(410, 25);
            this._voaFileNameBrowseButton.Name = "_voaFileNameBrowseButton";
            this._voaFileNameBrowseButton.Size = new System.Drawing.Size(26, 21);
            this._voaFileNameBrowseButton.TabIndex = 2;
            this._voaFileNameBrowseButton.Text = "...";
            this._voaFileNameBrowseButton.TextControl = this._voaFileNameTextBox;
            this._voaFileNameBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _voaFileNamePathTagButton
            // 
            this._voaFileNamePathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFileNamePathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_voaFileNamePathTagButton.Image")));
            this._voaFileNamePathTagButton.Location = new System.Drawing.Point(386, 25);
            this._voaFileNamePathTagButton.Name = "_voaFileNamePathTagButton";
            this._voaFileNamePathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._voaFileNamePathTagButton.Size = new System.Drawing.Size(18, 21);
            this._voaFileNamePathTagButton.TabIndex = 1;
            this._voaFileNamePathTagButton.TextControl = this._voaFileNameTextBox;
            this._voaFileNamePathTagButton.UseVisualStyleBackColor = true;
            // 
            // _storeDiscreteDataCheckBox
            // 
            this._storeDiscreteDataCheckBox.AutoSize = true;
            this._storeDiscreteDataCheckBox.Location = new System.Drawing.Point(12, 175);
            this._storeDiscreteDataCheckBox.Name = "_storeDiscreteDataCheckBox";
            this._storeDiscreteDataCheckBox.Size = new System.Drawing.Size(288, 17);
            this._storeDiscreteDataCheckBox.TabIndex = 6;
            this._storeDiscreteDataCheckBox.Text = "Store discrete data fields in addition to the binary stream";
            this._storeDiscreteDataCheckBox.UseVisualStyleBackColor = true;
            this._storeDiscreteDataCheckBox.CheckedChanged += new System.EventHandler(this.HandleStoreDiscretDataCheckBoxCheckChanged);
            // 
            // StoreAttributesInDBTaskSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(448, 287);
            this.Controls.Add(this._storeDiscreteDataCheckBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._attributeSetNameComboBox);
            this.Controls.Add(this._storeRasterZonesCheckBox);
            this.Controls.Add(label2);
            this.Controls.Add(this._voaFileNameBrowseButton);
            this.Controls.Add(this._voaFileNameTextBox);
            this.Controls.Add(this._voaFileNamePathTagButton);
            this.Controls.Add(label1);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._doNotSaveEmptyCheckBox);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 326);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(463, 300);
            this.Name = "StoreAttributesInDBTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Core: Store or retrieve attributes in database";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TextBox _voaFileNameTextBox;
        private Forms.FileActionManagerPathTagButton _voaFileNamePathTagButton;
        private Utilities.Forms.BrowseButton _voaFileNameBrowseButton;
        private System.Windows.Forms.CheckBox _storeRasterZonesCheckBox;
        private System.Windows.Forms.ComboBox _attributeSetNameComboBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton _RetrieveRadioButton;
        private System.Windows.Forms.RadioButton _StoreRadioButton;
        private System.Windows.Forms.CheckBox _doNotSaveEmptyCheckBox;
        private System.Windows.Forms.CheckBox _storeDiscreteDataCheckBox;
    }
}