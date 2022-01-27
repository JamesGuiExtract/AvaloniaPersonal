namespace Extract.FileActionManager.FileProcessors
{
    partial class ModifySourceDocNameInDBSettingsDialog
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
            System.Windows.Forms.Label label3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifySourceDocNameInDBSettingsDialog));
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnOK = new System.Windows.Forms.Button();
            this._browseButton = new Extract.Utilities.Forms.BrowseButton();
            this._renameFileToTextBox = new System.Windows.Forms.TextBox();
            this._fileTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 16);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(216, 13);
            label3.TabIndex = 3;
            label3.Text = "New name for SourceDocName in database";
            // 
            // _btnCancel
            // 
            this._btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(368, 63);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 23);
            this._btnCancel.TabIndex = 1;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _btnOK
            // 
            this._btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnOK.Location = new System.Drawing.Point(287, 63);
            this._btnOK.Name = "_btnOK";
            this._btnOK.Size = new System.Drawing.Size(75, 23);
            this._btnOK.TabIndex = 0;
            this._btnOK.Text = "OK";
            this._btnOK.UseVisualStyleBackColor = true;
            this._btnOK.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _browseButton
            // 
            this._browseButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this._browseButton.FileFilter = "";
            this._browseButton.Location = new System.Drawing.Point(415, 32);
            this._browseButton.MaximumSize = new System.Drawing.Size(27, 20);
            this._browseButton.MinimumSize = new System.Drawing.Size(27, 20);
            this._browseButton.Name = "_browseButton";
            this._browseButton.Size = new System.Drawing.Size(27, 20);
            this._browseButton.TabIndex = 5;
            this._browseButton.Text = "...";
            this._browseButton.TextControl = this._renameFileToTextBox;
            this._browseButton.UseVisualStyleBackColor = true;
            // 
            // _renameFileToTextBox
            // 
            this._renameFileToTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._renameFileToTextBox.Location = new System.Drawing.Point(12, 33);
            this._renameFileToTextBox.Name = "_renameFileToTextBox";
            this._renameFileToTextBox.Size = new System.Drawing.Size(369, 20);
            this._renameFileToTextBox.TabIndex = 4;
            // 
            // _fileTagsButton
            // 
            this._fileTagsButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this._fileTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_fileTagsButton.Image")));
            this._fileTagsButton.Location = new System.Drawing.Point(389, 32);
            this._fileTagsButton.MaximumSize = new System.Drawing.Size(18, 20);
            this._fileTagsButton.MinimumSize = new System.Drawing.Size(18, 20);
            this._fileTagsButton.Name = "_fileTagsButton";
            this._fileTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
			this._fileTagsButton.Size = new System.Drawing.Size(18, 20);
            this._fileTagsButton.TabIndex = 1;
            this._fileTagsButton.TextControl = this._renameFileToTextBox;
            this._fileTagsButton.UseVisualStyleBackColor = true;
            // 
            // ModifySourceDocNameInDBSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnCancel;
            this.ClientSize = new System.Drawing.Size(456, 96);
            this.Controls.Add(this._fileTagsButton);
            this.Controls.Add(this._browseButton);
            this.Controls.Add(this._renameFileToTextBox);
            this.Controls.Add(label3);
            this.Controls.Add(this._btnOK);
            this.Controls.Add(this._btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModifySourceDocNameInDBSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Core: Modify source document name in database settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _fileTagsButton; 
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Button _btnOK;
        private Extract.Utilities.Forms.BrowseButton _browseButton;
        private System.Windows.Forms.TextBox _renameFileToTextBox;

    }
}