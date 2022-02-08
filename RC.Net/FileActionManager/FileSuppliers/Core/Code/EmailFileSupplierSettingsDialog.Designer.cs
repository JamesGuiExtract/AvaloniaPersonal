namespace Extract.FileActionManager.FileSuppliers
{
    partial class EmailFileSupplierSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EmailFileSupplierSettingsDialog));
            this.label1 = new System.Windows.Forms.Label();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._voaFileNameTextBox = new System.Windows.Forms.TextBox();
            this._voaFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._voaFileNamePathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "VOA file with attributes";
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
            // _voaFileNameBrowseButton
            // 
            this._voaFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFileNameBrowseButton.EnsureFileExists = false;
            this._voaFileNameBrowseButton.EnsurePathExists = false;
            this._voaFileNameBrowseButton.FileFilter = "XSD Files (*.xsd)|*.xsd|All files (*.*)|*.*";
            this._voaFileNameBrowseButton.Location = new System.Drawing.Point(410, 24);
            this._voaFileNameBrowseButton.Name = "_voaFileNameBrowseButton";
            this._voaFileNameBrowseButton.Size = new System.Drawing.Size(26, 21);
            this._voaFileNameBrowseButton.TabIndex = 2;
            this._voaFileNameBrowseButton.Text = "...";
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
            // EmailFileSupplierSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(448, 287);
            this.Controls.Add(this._voaFileNameBrowseButton);
            this.Controls.Add(this._voaFileNameTextBox);
            this.Controls.Add(this._voaFileNamePathTagButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 326);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(463, 300);
            this.Name = "EmailFileSupplierSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "File supplier settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TextBox _voaFileNameTextBox;
        private Forms.FileActionManagerPathTagButton _voaFileNamePathTagButton;
        private Utilities.Forms.BrowseButton _voaFileNameBrowseButton;
        private System.Windows.Forms.Label label1;
    }
}
