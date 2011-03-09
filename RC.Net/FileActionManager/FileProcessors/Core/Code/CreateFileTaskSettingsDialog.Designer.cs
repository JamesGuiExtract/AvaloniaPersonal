namespace Extract.FileActionManager.FileProcessors
{
    partial class CreateFileTaskSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateFileTaskSettingsDialog));
            this._fileNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._appendRadioButton = new System.Windows.Forms.RadioButton();
            this._overwriteRadioButton = new System.Windows.Forms.RadioButton();
            this._skipWithoutErrorRadioButton = new System.Windows.Forms.RadioButton();
            this._generateErrorRadioButton = new System.Windows.Forms.RadioButton();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this._fileContentsTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._fileNameTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._fileContentsTextBox = new Extract.Utilities.Forms.BetterMultilineTextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _fileNameTextBox
            // 
            this._fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._fileNameTextBox.Location = new System.Drawing.Point(12, 26);
            this._fileNameTextBox.Name = "_fileNameTextBox";
            this._fileNameTextBox.Size = new System.Drawing.Size(408, 20);
            this._fileNameTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name of file to create:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._appendRadioButton);
            this.groupBox1.Controls.Add(this._overwriteRadioButton);
            this.groupBox1.Controls.Add(this._skipWithoutErrorRadioButton);
            this.groupBox1.Controls.Add(this._generateErrorRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 173);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(429, 117);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "If the above file exists";
            // 
            // _appendRadioButton
            // 
            this._appendRadioButton.AutoSize = true;
            this._appendRadioButton.Location = new System.Drawing.Point(7, 92);
            this._appendRadioButton.Name = "_appendRadioButton";
            this._appendRadioButton.Size = new System.Drawing.Size(108, 17);
            this._appendRadioButton.TabIndex = 3;
            this._appendRadioButton.TabStop = true;
            this._appendRadioButton.Text = "Append to the file";
            this._appendRadioButton.UseVisualStyleBackColor = true;
            // 
            // _overwriteRadioButton
            // 
            this._overwriteRadioButton.AutoSize = true;
            this._overwriteRadioButton.Location = new System.Drawing.Point(7, 68);
            this._overwriteRadioButton.Name = "_overwriteRadioButton";
            this._overwriteRadioButton.Size = new System.Drawing.Size(104, 17);
            this._overwriteRadioButton.TabIndex = 2;
            this._overwriteRadioButton.TabStop = true;
            this._overwriteRadioButton.Text = "Overwrite the file";
            this._overwriteRadioButton.UseVisualStyleBackColor = true;
            // 
            // _skipWithoutErrorRadioButton
            // 
            this._skipWithoutErrorRadioButton.AutoSize = true;
            this._skipWithoutErrorRadioButton.Location = new System.Drawing.Point(7, 44);
            this._skipWithoutErrorRadioButton.Name = "_skipWithoutErrorRadioButton";
            this._skipWithoutErrorRadioButton.Size = new System.Drawing.Size(172, 17);
            this._skipWithoutErrorRadioButton.TabIndex = 1;
            this._skipWithoutErrorRadioButton.TabStop = true;
            this._skipWithoutErrorRadioButton.Text = "Skip the operation without error";
            this._skipWithoutErrorRadioButton.UseVisualStyleBackColor = true;
            // 
            // _generateErrorRadioButton
            // 
            this._generateErrorRadioButton.AutoSize = true;
            this._generateErrorRadioButton.Location = new System.Drawing.Point(7, 20);
            this._generateErrorRadioButton.Name = "_generateErrorRadioButton";
            this._generateErrorRadioButton.Size = new System.Drawing.Size(204, 17);
            this._generateErrorRadioButton.TabIndex = 0;
            this._generateErrorRadioButton.TabStop = true;
            this._generateErrorRadioButton.Text = "Skip the operation and record an error";
            this._generateErrorRadioButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(285, 297);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 7;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(366, 297);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 8;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "File contents:";
            // 
            // _fileContentsTagsButton
            // 
            this._fileContentsTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._fileContentsTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_fileContentsTagsButton.Image")));
            this._fileContentsTagsButton.Location = new System.Drawing.Point(426, 65);
            this._fileContentsTagsButton.Name = "_fileContentsTagsButton";
            this._fileContentsTagsButton.Size = new System.Drawing.Size(18, 20);
            this._fileContentsTagsButton.TabIndex = 5;
            this._fileContentsTagsButton.UseVisualStyleBackColor = true;
            // 
            // _fileNameTagsButton
            // 
            this._fileNameTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._fileNameTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_fileNameTagsButton.Image")));
            this._fileNameTagsButton.Location = new System.Drawing.Point(426, 25);
            this._fileNameTagsButton.Name = "_fileNameTagsButton";
            this._fileNameTagsButton.Size = new System.Drawing.Size(18, 20);
            this._fileNameTagsButton.TabIndex = 2;
            this._fileNameTagsButton.UseVisualStyleBackColor = true;
            // 
            // _fileContentsTextBox
            // 
            this._fileContentsTextBox.AcceptsReturn = true;
            this._fileContentsTextBox.Location = new System.Drawing.Point(12, 67);
            this._fileContentsTextBox.Name = "_fileContentsTextBox";
            this._fileContentsTextBox.Size = new System.Drawing.Size(408, 96);
            this._fileContentsTextBox.TabIndex = 9;
            this._fileContentsTextBox.WordWrap = false;
            // 
            // CreateFileTaskSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(453, 332);
            this.Controls.Add(this._fileContentsTextBox);
            this.Controls.Add(this._fileContentsTagsButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._fileNameTagsButton);
            this.Controls.Add(this._fileNameTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateFileTaskSettingsDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure: Create file";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Utilities.Forms.PathTagsButton _fileNameTagsButton;
        private System.Windows.Forms.TextBox _fileNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton _appendRadioButton;
        private System.Windows.Forms.RadioButton _overwriteRadioButton;
        private System.Windows.Forms.RadioButton _skipWithoutErrorRadioButton;
        private System.Windows.Forms.RadioButton _generateErrorRadioButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label label2;
        private Utilities.Forms.PathTagsButton _fileContentsTagsButton;
        private Utilities.Forms.BetterMultilineTextBox _fileContentsTextBox;
    }
}