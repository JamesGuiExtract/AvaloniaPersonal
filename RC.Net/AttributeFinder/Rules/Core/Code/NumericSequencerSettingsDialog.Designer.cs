namespace Extract.AttributeFinder.Rules
{
    partial class NumericSequencerSettingsDialog
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
            this._expandRadioButton = new System.Windows.Forms.RadioButton();
            this._contractRadioButton = new System.Windows.Forms.RadioButton();
            this._sortCheckBox = new System.Windows.Forms.CheckBox();
            this._eliminateDuplicatesCheckBox = new System.Windows.Forms.CheckBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._sortComboBox = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _expandRadioButton
            // 
            this._expandRadioButton.AutoSize = true;
            this._expandRadioButton.Location = new System.Drawing.Point(13, 13);
            this._expandRadioButton.Name = "_expandRadioButton";
            this._expandRadioButton.Size = new System.Drawing.Size(151, 17);
            this._expandRadioButton.TabIndex = 0;
            this._expandRadioButton.TabStop = true;
            this._expandRadioButton.Text = "Expand numeric sequence";
            this._expandRadioButton.UseVisualStyleBackColor = true;
            // 
            // _contractRadioButton
            // 
            this._contractRadioButton.AutoSize = true;
            this._contractRadioButton.Location = new System.Drawing.Point(13, 36);
            this._contractRadioButton.Name = "_contractRadioButton";
            this._contractRadioButton.Size = new System.Drawing.Size(155, 17);
            this._contractRadioButton.TabIndex = 1;
            this._contractRadioButton.TabStop = true;
            this._contractRadioButton.Text = "Contract numeric sequence";
            this._contractRadioButton.UseVisualStyleBackColor = true;
            // 
            // _sortCheckBox
            // 
            this._sortCheckBox.AutoSize = true;
            this._sortCheckBox.Location = new System.Drawing.Point(13, 60);
            this._sortCheckBox.Name = "_sortCheckBox";
            this._sortCheckBox.Size = new System.Drawing.Size(106, 17);
            this._sortCheckBox.TabIndex = 2;
            this._sortCheckBox.Text = "Sort sequence in";
            this._sortCheckBox.UseVisualStyleBackColor = true;
            // 
            // _eliminateDuplicatesCheckBox
            // 
            this._eliminateDuplicatesCheckBox.AutoSize = true;
            this._eliminateDuplicatesCheckBox.Location = new System.Drawing.Point(13, 84);
            this._eliminateDuplicatesCheckBox.Name = "_eliminateDuplicatesCheckBox";
            this._eliminateDuplicatesCheckBox.Size = new System.Drawing.Size(119, 17);
            this._eliminateDuplicatesCheckBox.TabIndex = 5;
            this._eliminateDuplicatesCheckBox.Text = "Eliminate duplicates";
            this._eliminateDuplicatesCheckBox.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(223, 110);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 7;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(142, 110);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 6;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _sortComboBox
            // 
            this._sortComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._sortComboBox.FormattingEnabled = true;
            this._sortComboBox.Items.AddRange(new object[] {
            "Ascending",
            "Descending"});
            this._sortComboBox.Location = new System.Drawing.Point(125, 58);
            this._sortComboBox.Name = "_sortComboBox";
            this._sortComboBox.Size = new System.Drawing.Size(91, 21);
            this._sortComboBox.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(222, 61);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(31, 13);
            label1.TabIndex = 4;
            label1.Text = "order";
            // 
            // NumericSequencerSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(310, 145);
            this.Controls.Add(label1);
            this.Controls.Add(this._sortComboBox);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._eliminateDuplicatesCheckBox);
            this.Controls.Add(this._sortCheckBox);
            this.Controls.Add(this._contractRadioButton);
            this.Controls.Add(this._expandRadioButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NumericSequencerSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Numeric sequence expander/contracter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton _expandRadioButton;
        private System.Windows.Forms.RadioButton _contractRadioButton;
        private System.Windows.Forms.CheckBox _sortCheckBox;
        private System.Windows.Forms.CheckBox _eliminateDuplicatesCheckBox;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.ComboBox _sortComboBox;
    }
}