namespace Extract.FileActionManager.FileProcessors
{
    partial class ModifyPdfFileSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifyPdfFileSettingsDialog));
            System.Windows.Forms.Label label1;
            System.Windows.Forms.GroupBox groupBox2;
            this._pdfFileBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._pdfFileTextBox = new System.Windows.Forms.TextBox();
            this._pdfFilePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._removeAnnotationsCheckBox = new System.Windows.Forms.CheckBox();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this._pdfFileBrowseButton);
            groupBox1.Controls.Add(this._pdfFilePathTagsButton);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(this._pdfFileTextBox);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(420, 66);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "General settings";
            // 
            // _pdfFileBrowseButton
            // 
            this._pdfFileBrowseButton.FileFilter = "PDF Files (*.pdf)|*.pdf||";
            this._pdfFileBrowseButton.Location = new System.Drawing.Point(385, 34);
            this._pdfFileBrowseButton.Name = "_pdfFileBrowseButton";
            this._pdfFileBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._pdfFileBrowseButton.TabIndex = 3;
            this._pdfFileBrowseButton.Text = "...";
            this._pdfFileBrowseButton.TextControl = this._pdfFileTextBox;
            this._pdfFileBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _pdfFileTextBox
            // 
            this._pdfFileTextBox.Location = new System.Drawing.Point(6, 34);
            this._pdfFileTextBox.Name = "_pdfFileTextBox";
            this._pdfFileTextBox.Size = new System.Drawing.Size(347, 20);
            this._pdfFileTextBox.TabIndex = 0;
            // 
            // _pdfFilePathTagsButton
            // 
            this._pdfFilePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_pdfFilePathTagsButton.Image")));
            this._pdfFilePathTagsButton.Location = new System.Drawing.Point(361, 34);
            this._pdfFilePathTagsButton.Name = "_pdfFilePathTagsButton";
            this._pdfFilePathTagsButton.PathTags = new Extract.Utilities.FileActionManagerPathTags();
            this._pdfFilePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._pdfFilePathTagsButton.TabIndex = 2;
            this._pdfFilePathTagsButton.TextControl = _pdfFileTextBox;
            this._pdfFilePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 16);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(140, 13);
            label1.TabIndex = 1;
            label1.Text = "Modify the following PDF file";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(this._removeAnnotationsCheckBox);
            groupBox2.Location = new System.Drawing.Point(12, 84);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(420, 48);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Annotations";
            // 
            // _removeAnnotationsCheckBox
            // 
            this._removeAnnotationsCheckBox.AutoSize = true;
            this._removeAnnotationsCheckBox.Checked = true;
            this._removeAnnotationsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._removeAnnotationsCheckBox.Enabled = false;
            this._removeAnnotationsCheckBox.Location = new System.Drawing.Point(6, 19);
            this._removeAnnotationsCheckBox.Name = "_removeAnnotationsCheckBox";
            this._removeAnnotationsCheckBox.Size = new System.Drawing.Size(218, 17);
            this._removeAnnotationsCheckBox.TabIndex = 0;
            this._removeAnnotationsCheckBox.Text = "Remove all annotations from the PDF file";
            this._removeAnnotationsCheckBox.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Location = new System.Drawing.Point(276, 138);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 2;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClicked);
            // 
            // _cancelButton
            // 
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(357, 138);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 3;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // ModifyPdfFileSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 170);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModifyPdfFileSettingsDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Core: Modify PDF file";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox _pdfFileTextBox;
        private Extract.Utilities.Forms.BrowseButton _pdfFileBrowseButton;
        private Extract.Utilities.Forms.PathTagsButton _pdfFilePathTagsButton;
        private System.Windows.Forms.CheckBox _removeAnnotationsCheckBox;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;

    }
}