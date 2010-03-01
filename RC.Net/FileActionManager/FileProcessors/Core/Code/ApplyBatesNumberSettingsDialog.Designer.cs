namespace Extract.FileActionManager.FileProcessors
{
    partial class ApplyBatesNumberSettingsDialog
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

            // Dispose of the BatesNumberGenerator
            if (_generator != null)
            {
                _generator.Dispose();
                _generator = null;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApplyBatesNumberSettingsDialog));
            this._appearanceGroupBox = new System.Windows.Forms.GroupBox();
            this._changeAppearanceButton = new System.Windows.Forms.Button();
            this._appearanceSummaryText = new System.Windows.Forms.TextBox();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._fileNameGroupBox = new System.Windows.Forms.GroupBox();
            this._fileTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._browseButton = new Extract.Utilities.Forms.BrowseButton();
            this._fileNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._appearanceGroupBox.SuspendLayout();
            this._fileNameGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _appearanceGroupBox
            // 
            this._appearanceGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._appearanceGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._appearanceGroupBox.Controls.Add(this._changeAppearanceButton);
            this._appearanceGroupBox.Controls.Add(this._appearanceSummaryText);
            this._appearanceGroupBox.Location = new System.Drawing.Point(12, 75);
            this._appearanceGroupBox.Name = "_appearanceGroupBox";
            this._appearanceGroupBox.Size = new System.Drawing.Size(323, 72);
            this._appearanceGroupBox.TabIndex = 2;
            this._appearanceGroupBox.TabStop = false;
            this._appearanceGroupBox.Text = "Position and appearance";
            // 
            // _changeAppearanceButton
            // 
            this._changeAppearanceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._changeAppearanceButton.Location = new System.Drawing.Point(242, 19);
            this._changeAppearanceButton.MaximumSize = new System.Drawing.Size(75, 23);
            this._changeAppearanceButton.MinimumSize = new System.Drawing.Size(75, 23);
            this._changeAppearanceButton.Name = "_changeAppearanceButton";
            this._changeAppearanceButton.Size = new System.Drawing.Size(75, 23);
            this._changeAppearanceButton.TabIndex = 0;
            this._changeAppearanceButton.Text = "Change...";
            this._changeAppearanceButton.UseVisualStyleBackColor = true;
            this._changeAppearanceButton.Click += new System.EventHandler(this.HandleChangeAppearanceButtonClick);
            // 
            // _appearanceSummaryText
            // 
            this._appearanceSummaryText.AcceptsReturn = true;
            this._appearanceSummaryText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._appearanceSummaryText.Location = new System.Drawing.Point(6, 19);
            this._appearanceSummaryText.Multiline = true;
            this._appearanceSummaryText.Name = "_appearanceSummaryText";
            this._appearanceSummaryText.ReadOnly = true;
            this._appearanceSummaryText.Size = new System.Drawing.Size(230, 46);
            this._appearanceSummaryText.TabIndex = 0;
            this._appearanceSummaryText.TabStop = false;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(179, 153);
            this._okButton.MaximumSize = new System.Drawing.Size(75, 23);
            this._okButton.MinimumSize = new System.Drawing.Size(75, 23);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 3;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(260, 153);
            this._cancelButton.MaximumSize = new System.Drawing.Size(75, 23);
            this._cancelButton.MinimumSize = new System.Drawing.Size(75, 23);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 4;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _fileNameGroupBox
            // 
            this._fileNameGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._fileNameGroupBox.Controls.Add(this._fileTagsButton);
            this._fileNameGroupBox.Controls.Add(this._browseButton);
            this._fileNameGroupBox.Controls.Add(this._fileNameTextBox);
            this._fileNameGroupBox.Controls.Add(this.label1);
            this._fileNameGroupBox.Location = new System.Drawing.Point(12, 9);
            this._fileNameGroupBox.Name = "_fileNameGroupBox";
            this._fileNameGroupBox.Size = new System.Drawing.Size(322, 60);
            this._fileNameGroupBox.TabIndex = 0;
            this._fileNameGroupBox.TabStop = false;
            this._fileNameGroupBox.Text = "Filename";
            // 
            // _fileTagsButton
            // 
            this._fileTagsButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this._fileTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_fileTagsButton.Image")));
            this._fileTagsButton.Location = new System.Drawing.Point(265, 32);
            this._fileTagsButton.MaximumSize = new System.Drawing.Size(18, 20);
            this._fileTagsButton.MinimumSize = new System.Drawing.Size(18, 20);
            this._fileTagsButton.Name = "_fileTagsButton";
            this._fileTagsButton.PathTags = new Extract.Utilities.FileActionManagerPathTags();
            this._fileTagsButton.Size = new System.Drawing.Size(18, 20);
            this._fileTagsButton.TabIndex = 2;
            this._fileTagsButton.UseVisualStyleBackColor = true;
            this._fileTagsButton.TagSelected += new System.EventHandler<Extract.Utilities.Forms.TagSelectedEventArgs>(this.HandlePathTagsButtonClick);
            // 
            // _browseButton
            // 
            this._browseButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this._browseButton.FileFilter = "";
            this._browseButton.Location = new System.Drawing.Point(289, 32);
            this._browseButton.MaximumSize = new System.Drawing.Size(27, 20);
            this._browseButton.MinimumSize = new System.Drawing.Size(27, 20);
            this._browseButton.Name = "_browseButton";
            this._browseButton.Size = new System.Drawing.Size(27, 20);
            this._browseButton.TabIndex = 3;
            this._browseButton.Text = "...";
            this._browseButton.TextControl = this._fileNameTextBox;
            this._browseButton.UseVisualStyleBackColor = true;
            // 
            // _fileNameTextBox
            // 
            this._fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._fileNameTextBox.Location = new System.Drawing.Point(6, 33);
            this._fileNameTextBox.Name = "_fileNameTextBox";
            this._fileNameTextBox.Size = new System.Drawing.Size(250, 20);
            this._fileNameTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(175, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Image file to apply Bates number on";
            // 
            // ApplyBatesNumberSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(347, 188);
            this.Controls.Add(this._fileNameGroupBox);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._appearanceGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ApplyBatesNumberSettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Apply Bates Number";
            this._appearanceGroupBox.ResumeLayout(false);
            this._appearanceGroupBox.PerformLayout();
            this._fileNameGroupBox.ResumeLayout(false);
            this._fileNameGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox _appearanceGroupBox;
        private System.Windows.Forms.TextBox _appearanceSummaryText;
        private System.Windows.Forms.Button _changeAppearanceButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.GroupBox _fileNameGroupBox;
        private Extract.Utilities.Forms.PathTagsButton _fileTagsButton;
        private Extract.Utilities.Forms.BrowseButton _browseButton;
        private System.Windows.Forms.TextBox _fileNameTextBox;
        private System.Windows.Forms.Label label1;
    }
}