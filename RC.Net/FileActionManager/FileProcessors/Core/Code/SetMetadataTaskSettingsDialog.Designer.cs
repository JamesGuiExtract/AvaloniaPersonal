namespace Extract.FileActionManager.FileProcessors
{
    partial class SetMetadataTaskSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetMetadataTaskSettingsDialog));
            this._fieldNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this._valueTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._valueTextBox = new Extract.Utilities.Forms.BetterMultilineTextBox();
            this._fieldNameTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this.SuspendLayout();
            // 
            // _fieldNameTextBox
            // 
            this._fieldNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._fieldNameTextBox.Location = new System.Drawing.Point(10, 28);
            this._fieldNameTextBox.Name = "_fieldNameTextBox";
            this._fieldNameTextBox.Size = new System.Drawing.Size(408, 20);
            this._fieldNameTextBox.TabIndex = 1;
            this._fieldNameTextBox.LostFocus += HandleFieldNameTextBox_LostFocus;
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Metadata field name:";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(283, 199);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 6;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(364, 199);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 7;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Value to apply:";
            // 
            // _valueTagsButton
            // 
            this._valueTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._valueTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_valueTagsButton.Image")));
            this._valueTagsButton.Location = new System.Drawing.Point(424, 67);
            this._valueTagsButton.Name = "_valueTagsButton";
            this._valueTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._valueTagsButton.Size = new System.Drawing.Size(18, 20);
            this._valueTagsButton.TabIndex = 5;
            this._valueTagsButton.TextControl = this._valueTextBox;
            this._valueTagsButton.UseVisualStyleBackColor = true;
            // 
            // _valueTextBox
            // 
            this._valueTextBox.AcceptsReturn = true;
            this._valueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._valueTextBox.Location = new System.Drawing.Point(10, 69);
            this._valueTextBox.Name = "_valueTextBox";
            this._valueTextBox.Size = new System.Drawing.Size(408, 124);
            this._valueTextBox.TabIndex = 4;
            this._valueTextBox.WordWrap = false;
            // 
            // _fieldNameTagsButton
            // 
            this._fieldNameTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._fieldNameTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_fieldNameTagsButton.Image")));
            this._fieldNameTagsButton.Location = new System.Drawing.Point(424, 27);
            this._fieldNameTagsButton.Name = "_fieldNameTagsButton";
            this._fieldNameTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._fieldNameTagsButton.Size = new System.Drawing.Size(18, 20);
            this._fieldNameTagsButton.TabIndex = 2;
            this._fieldNameTagsButton.TextControl = this._fieldNameTextBox;
            this._fieldNameTagsButton.UseVisualStyleBackColor = true;
            // 
            // SetMetadataTaskSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(453, 232);
            this.Controls.Add(this._valueTextBox);
            this.Controls.Add(this._valueTagsButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._fieldNameTagsButton);
            this.Controls.Add(this._fieldNameTextBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(469, 200);
            this.Name = "SetMetadataTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Core: Set Metadata settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _fieldNameTagsButton;
        private System.Windows.Forms.TextBox _fieldNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label label2;
        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _valueTagsButton;
        private Extract.Utilities.Forms.BetterMultilineTextBox _valueTextBox;
    }
}