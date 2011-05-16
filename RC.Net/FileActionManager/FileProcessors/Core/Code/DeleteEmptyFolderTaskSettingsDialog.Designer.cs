namespace Extract.FileActionManager.FileProcessors
{
    partial class DeleteEmptyFolderTaskSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeleteEmptyFolderTaskSettingsDialog));
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this._folderNameTextBox = new System.Windows.Forms.TextBox();
            this._deleteRecursivelyCheckBox = new System.Windows.Forms.CheckBox();
            this._limitRecursionCheckBox = new System.Windows.Forms.CheckBox();
            this._recursionLimitTextBox = new System.Windows.Forms.TextBox();
            this._recursionLimitTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._folderNameTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._recursionLimitBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(289, 127);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 9;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(370, 127);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 10;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(162, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Name of folder to delete if empty:";
            // 
            // _folderNameTextBox
            // 
            this._folderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._folderNameTextBox.Location = new System.Drawing.Point(12, 26);
            this._folderNameTextBox.Name = "_folderNameTextBox";
            this._folderNameTextBox.Size = new System.Drawing.Size(409, 20);
            this._folderNameTextBox.TabIndex = 12;
            // 
            // _deleteRecursivelyCheckBox
            // 
            this._deleteRecursivelyCheckBox.AutoSize = true;
            this._deleteRecursivelyCheckBox.Location = new System.Drawing.Point(15, 52);
            this._deleteRecursivelyCheckBox.Name = "_deleteRecursivelyCheckBox";
            this._deleteRecursivelyCheckBox.Size = new System.Drawing.Size(177, 17);
            this._deleteRecursivelyCheckBox.TabIndex = 14;
            this._deleteRecursivelyCheckBox.Text = "Delete parent folders recursively";
            this._deleteRecursivelyCheckBox.UseVisualStyleBackColor = true;
            // 
            // _limitRecursionCheckBox
            // 
            this._limitRecursionCheckBox.AutoSize = true;
            this._limitRecursionCheckBox.Enabled = false;
            this._limitRecursionCheckBox.Location = new System.Drawing.Point(34, 75);
            this._limitRecursionCheckBox.Name = "_limitRecursionCheckBox";
            this._limitRecursionCheckBox.Size = new System.Drawing.Size(184, 17);
            this._limitRecursionCheckBox.TabIndex = 15;
            this._limitRecursionCheckBox.Text = "Up to but not including this folder:";
            this._limitRecursionCheckBox.UseVisualStyleBackColor = true;
            // 
            // _recursionLimitTextBox
            // 
            this._recursionLimitTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._recursionLimitTextBox.Enabled = false;
            this._recursionLimitTextBox.Location = new System.Drawing.Point(34, 98);
            this._recursionLimitTextBox.Name = "_recursionLimitTextBox";
            this._recursionLimitTextBox.Size = new System.Drawing.Size(355, 20);
            this._recursionLimitTextBox.TabIndex = 16;
            // 
            // _recursionLimitTagsButton
            // 
            this._recursionLimitTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._recursionLimitTagsButton.Enabled = false;
            this._recursionLimitTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_recursionLimitTagsButton.Image")));
            this._recursionLimitTagsButton.Location = new System.Drawing.Point(427, 98);
            this._recursionLimitTagsButton.Name = "_recursionLimitTagsButton";
            this._recursionLimitTagsButton.Size = new System.Drawing.Size(18, 20);
            this._recursionLimitTagsButton.TabIndex = 17;
            this._recursionLimitTagsButton.TextControl = this._recursionLimitTextBox;
            this._recursionLimitTagsButton.UseVisualStyleBackColor = true;
            // 
            // _folderNameTagsButton
            // 
            this._folderNameTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._folderNameTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_folderNameTagsButton.Image")));
            this._folderNameTagsButton.Location = new System.Drawing.Point(427, 26);
            this._folderNameTagsButton.Name = "_folderNameTagsButton";
            this._folderNameTagsButton.Size = new System.Drawing.Size(18, 20);
            this._folderNameTagsButton.TabIndex = 13;
            this._folderNameTagsButton.TextControl = this._folderNameTextBox;
            this._folderNameTagsButton.UseVisualStyleBackColor = true;
            // 
            // _recursionLimitBrowseButton
            // 
            this._recursionLimitBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._recursionLimitBrowseButton.Enabled = false;
            this._recursionLimitBrowseButton.FolderBrowser = true;
            this._recursionLimitBrowseButton.Location = new System.Drawing.Point(395, 98);
            this._recursionLimitBrowseButton.Name = "_recursionLimitBrowseButton";
            this._recursionLimitBrowseButton.Size = new System.Drawing.Size(26, 20);
            this._recursionLimitBrowseButton.TabIndex = 18;
            this._recursionLimitBrowseButton.Text = "...";
            this._recursionLimitBrowseButton.TextControl = this._recursionLimitTextBox;
            this._recursionLimitBrowseButton.UseVisualStyleBackColor = true;
            // 
            // DeleteEmptyFolderTaskSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 162);
            this.Controls.Add(this._recursionLimitBrowseButton);
            this.Controls.Add(this._recursionLimitTagsButton);
            this.Controls.Add(this._recursionLimitTextBox);
            this.Controls.Add(this._limitRecursionCheckBox);
            this.Controls.Add(this._deleteRecursivelyCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._folderNameTagsButton);
            this.Controls.Add(this._folderNameTextBox);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 200);
            this.Name = "DeleteEmptyFolderTaskSettingsDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Core: Delete empty folder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label label1;
        private Utilities.Forms.PathTagsButton _folderNameTagsButton;
        private System.Windows.Forms.TextBox _folderNameTextBox;
        private System.Windows.Forms.CheckBox _deleteRecursivelyCheckBox;
        private System.Windows.Forms.CheckBox _limitRecursionCheckBox;
        private Utilities.Forms.PathTagsButton _recursionLimitTagsButton;
        private System.Windows.Forms.TextBox _recursionLimitTextBox;
        private Utilities.Forms.BrowseButton _recursionLimitBrowseButton;
    }
}