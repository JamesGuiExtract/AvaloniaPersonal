namespace Extract.AttributeFinder.Rules
{
    partial class ModifySpatialModeSettingsDialog
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
            Extract.Utilities.Forms.InfoTip infoTip1;
            System.Windows.Forms.GroupBox groupBox2;
            Extract.Utilities.Forms.InfoTip infoTip2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifySpatialModeSettingsDialog));
            this._removeSpatialInfoRadioButton = new System.Windows.Forms.RadioButton();
            this._convertToPseudoSpatialRadioButton = new System.Windows.Forms.RadioButton();
            this._downgradeToHybridRadioButton = new System.Windows.Forms.RadioButton();
            this._zoneCountConditionComboBox = new System.Windows.Forms.ComboBox();
            this._useConditionCheckBox = new System.Windows.Forms.CheckBox();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._modifyRecursivelyCheckBox = new System.Windows.Forms.CheckBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            infoTip1 = new Extract.Utilities.Forms.InfoTip();
            groupBox2 = new System.Windows.Forms.GroupBox();
            infoTip2 = new Extract.Utilities.Forms.InfoTip();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(infoTip2);
            groupBox1.Controls.Add(this._modifyRecursivelyCheckBox);
            groupBox1.Controls.Add(infoTip1);
            groupBox1.Controls.Add(this._removeSpatialInfoRadioButton);
            groupBox1.Controls.Add(this._convertToPseudoSpatialRadioButton);
            groupBox1.Controls.Add(this._downgradeToHybridRadioButton);
            groupBox1.Location = new System.Drawing.Point(13, 13);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(417, 119);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Spatial mode modification";
            // 
            // infoTip1
            // 
            infoTip1.BackColor = System.Drawing.Color.Transparent;
            infoTip1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip1.BackgroundImage")));
            infoTip1.Location = new System.Drawing.Point(167, 43);
            infoTip1.Name = "infoTip1";
            infoTip1.Size = new System.Drawing.Size(16, 16);
            infoTip1.TabIndex = 3;
            infoTip1.TabStop = false;
            infoTip1.TipText = resources.GetString("infoTip1.TipText");
            // 
            // _removeSpatialInfoRadioButton
            // 
            this._removeSpatialInfoRadioButton.AutoSize = true;
            this._removeSpatialInfoRadioButton.Location = new System.Drawing.Point(15, 65);
            this._removeSpatialInfoRadioButton.Name = "_removeSpatialInfoRadioButton";
            this._removeSpatialInfoRadioButton.Size = new System.Drawing.Size(118, 17);
            this._removeSpatialInfoRadioButton.TabIndex = 2;
            this._removeSpatialInfoRadioButton.TabStop = true;
            this._removeSpatialInfoRadioButton.Text = "Remove spatial info";
            this._removeSpatialInfoRadioButton.UseVisualStyleBackColor = true;
            // 
            // _convertToPseudoSpatialRadioButton
            // 
            this._convertToPseudoSpatialRadioButton.AutoSize = true;
            this._convertToPseudoSpatialRadioButton.Location = new System.Drawing.Point(15, 42);
            this._convertToPseudoSpatialRadioButton.Name = "_convertToPseudoSpatialRadioButton";
            this._convertToPseudoSpatialRadioButton.Size = new System.Drawing.Size(145, 17);
            this._convertToPseudoSpatialRadioButton.TabIndex = 1;
            this._convertToPseudoSpatialRadioButton.TabStop = true;
            this._convertToPseudoSpatialRadioButton.Text = "Convert to pseudo-spatial";
            this._convertToPseudoSpatialRadioButton.UseVisualStyleBackColor = true;
            this._convertToPseudoSpatialRadioButton.CheckedChanged += new System.EventHandler(this.HandleConvertToPseudoSpatialCheckChanged);
            // 
            // _downgradeToHybridRadioButton
            // 
            this._downgradeToHybridRadioButton.AutoSize = true;
            this._downgradeToHybridRadioButton.Location = new System.Drawing.Point(15, 19);
            this._downgradeToHybridRadioButton.Name = "_downgradeToHybridRadioButton";
            this._downgradeToHybridRadioButton.Size = new System.Drawing.Size(126, 17);
            this._downgradeToHybridRadioButton.TabIndex = 0;
            this._downgradeToHybridRadioButton.TabStop = true;
            this._downgradeToHybridRadioButton.Text = "Downgrade to hybrid ";
            this._downgradeToHybridRadioButton.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(this._zoneCountConditionComboBox);
            groupBox2.Controls.Add(this._useConditionCheckBox);
            groupBox2.Location = new System.Drawing.Point(13, 138);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(417, 48);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Spatial mode modification condition";
            // 
            // _zoneCountConditionComboBox
            // 
            this._zoneCountConditionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._zoneCountConditionComboBox.FormattingEnabled = true;
            this._zoneCountConditionComboBox.Items.AddRange(new object[] {
            "is a single raster zone",
            "are multiple raster zones"});
            this._zoneCountConditionComboBox.Location = new System.Drawing.Point(222, 18);
            this._zoneCountConditionComboBox.Name = "_zoneCountConditionComboBox";
            this._zoneCountConditionComboBox.Size = new System.Drawing.Size(189, 21);
            this._zoneCountConditionComboBox.TabIndex = 1;
            // 
            // _useConditionCheckBox
            // 
            this._useConditionCheckBox.AutoSize = true;
            this._useConditionCheckBox.Location = new System.Drawing.Point(16, 20);
            this._useConditionCheckBox.Name = "_useConditionCheckBox";
            this._useConditionCheckBox.Size = new System.Drawing.Size(195, 17);
            this._useConditionCheckBox.TabIndex = 0;
            this._useConditionCheckBox.Text = "Only modify the spatial mode if there";
            this._useConditionCheckBox.UseVisualStyleBackColor = true;
            this._useConditionCheckBox.CheckedChanged += new System.EventHandler(this.HandleUseConditionCheckChanged);
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(274, 192);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 8;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(355, 192);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 9;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _modifyRecursivelyCheckBox
            // 
            this._modifyRecursivelyCheckBox.AutoSize = true;
            this._modifyRecursivelyCheckBox.Location = new System.Drawing.Point(15, 96);
            this._modifyRecursivelyCheckBox.Name = "_modifyRecursivelyCheckBox";
            this._modifyRecursivelyCheckBox.Size = new System.Drawing.Size(252, 17);
            this._modifyRecursivelyCheckBox.TabIndex = 4;
            this._modifyRecursivelyCheckBox.Text = "Perform modification recusively on sub-attributes";
            this._modifyRecursivelyCheckBox.UseVisualStyleBackColor = true;
            // 
            // infoTip2
            // 
            infoTip2.BackColor = System.Drawing.Color.Transparent;
            infoTip2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip2.BackgroundImage")));
            infoTip2.Location = new System.Drawing.Point(279, 96);
            infoTip2.Name = "infoTip2";
            infoTip2.Size = new System.Drawing.Size(16, 16);
            infoTip2.TabIndex = 5;
            infoTip2.TabStop = false;
            infoTip2.TipText = resources.GetString("infoTip2.TipText");
            // 
            // ModifySpatialModeSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(442, 227);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModifySpatialModeSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Modify spatial mode settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.RadioButton _convertToPseudoSpatialRadioButton;
        private System.Windows.Forms.RadioButton _downgradeToHybridRadioButton;
        private System.Windows.Forms.RadioButton _removeSpatialInfoRadioButton;
        private System.Windows.Forms.ComboBox _zoneCountConditionComboBox;
        private System.Windows.Forms.CheckBox _useConditionCheckBox;
        private System.Windows.Forms.CheckBox _modifyRecursivelyCheckBox;
    }
}