namespace Extract.FileActionManager.FileProcessors
{
    partial class CloudOCRTaskSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CloudOCRTaskSettingsDialog));
            this.label1 = new System.Windows.Forms.Label();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._credentialsJSONFileTextBox = new System.Windows.Forms.TextBox();
            this._credentialsJSONFileBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._credentialsJSONFilePathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._bucketBaseNameTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._generateBucketBaseNameButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(141, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Project credentials JSON file";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(409, 127);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 10;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(328, 127);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 9;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _credentialsJSONFileTextBox
            // 
            this._credentialsJSONFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._credentialsJSONFileTextBox.Location = new System.Drawing.Point(15, 25);
            this._credentialsJSONFileTextBox.Name = "_credentialsJSONFileTextBox";
            this._credentialsJSONFileTextBox.Size = new System.Drawing.Size(388, 20);
            this._credentialsJSONFileTextBox.TabIndex = 0;
            // 
            // _credentialsJSONFileBrowseButton
            // 
            this._credentialsJSONFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._credentialsJSONFileBrowseButton.EnsureFileExists = false;
            this._credentialsJSONFileBrowseButton.EnsurePathExists = false;
            this._credentialsJSONFileBrowseButton.FileFilter = "JSON Files (*.json)|*.json|All files (*.*)|*.*";
            this._credentialsJSONFileBrowseButton.Location = new System.Drawing.Point(444, 24);
            this._credentialsJSONFileBrowseButton.Name = "_credentialsJSONFileBrowseButton";
            this._credentialsJSONFileBrowseButton.Size = new System.Drawing.Size(40, 22);
            this._credentialsJSONFileBrowseButton.TabIndex = 2;
            this._credentialsJSONFileBrowseButton.Text = "...";
            this._credentialsJSONFileBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _credentialsJSONFilePathTagButton
            // 
            this._credentialsJSONFilePathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._credentialsJSONFilePathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_credentialsJSONFilePathTagButton.Image")));
            this._credentialsJSONFilePathTagButton.Location = new System.Drawing.Point(409, 24);
            this._credentialsJSONFilePathTagButton.Name = "_credentialsJSONFilePathTagButton";
            this._credentialsJSONFilePathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._credentialsJSONFilePathTagButton.Size = new System.Drawing.Size(29, 22);
            this._credentialsJSONFilePathTagButton.TabIndex = 1;
            this._credentialsJSONFilePathTagButton.TextControl = this._credentialsJSONFileTextBox;
            this._credentialsJSONFilePathTagButton.UseVisualStyleBackColor = true;
            // 
            // _bucketBaseNameTextBox
            // 
            this._bucketBaseNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._bucketBaseNameTextBox.Location = new System.Drawing.Point(15, 77);
            this._bucketBaseNameTextBox.Name = "_bucketBaseNameTextBox";
            this._bucketBaseNameTextBox.Size = new System.Drawing.Size(388, 20);
            this._bucketBaseNameTextBox.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(381, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Bucket base-name (will add \'-images \' or \'-output\' suffix to get full bucket name" +
    "s)";
            // 
            // _generateBucketBaseNameButton
            // 
            this._generateBucketBaseNameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._generateBucketBaseNameButton.Location = new System.Drawing.Point(409, 76);
            this._generateBucketBaseNameButton.Name = "_generateBucketBaseNameButton";
            this._generateBucketBaseNameButton.Size = new System.Drawing.Size(75, 22);
            this._generateBucketBaseNameButton.TabIndex = 4;
            this._generateBucketBaseNameButton.Text = "Generate";
            this._generateBucketBaseNameButton.UseVisualStyleBackColor = true;
            this._generateBucketBaseNameButton.Click += new System.EventHandler(this.HandleGenerateBucketBaseNameButtonClick);
            // 
            // CloudOCRTaskSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(496, 161);
            this.Controls.Add(this._generateBucketBaseNameButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._bucketBaseNameTextBox);
            this.Controls.Add(this._credentialsJSONFileBrowseButton);
            this.Controls.Add(this._credentialsJSONFileTextBox);
            this.Controls.Add(this._credentialsJSONFilePathTagButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 326);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(463, 200);
            this.Name = "CloudOCRTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cloud OCR Task";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TextBox _credentialsJSONFileTextBox;
        private Forms.FileActionManagerPathTagButton _credentialsJSONFilePathTagButton;
        private Utilities.Forms.BrowseButton _credentialsJSONFileBrowseButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _bucketBaseNameTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button _generateBucketBaseNameButton;
    }
}
