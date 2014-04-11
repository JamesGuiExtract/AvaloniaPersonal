namespace Extract.FileActionManager.Utilities
{
    partial class DirectorySelectionForm
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            this._includeSubFoldersCheckBox = new System.Windows.Forms.CheckBox();
            this._browseButton = new Extract.Utilities.Forms.BrowseButton();
            this._directoryTextBox = new System.Windows.Forms.TextBox();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._fileFilterComboBox = new System.Windows.Forms.ComboBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(this._fileFilterComboBox);
            groupBox1.Controls.Add(this._includeSubFoldersCheckBox);
            groupBox1.Controls.Add(this._browseButton);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(this._directoryTextBox);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(426, 123);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            // 
            // _includeSubFoldersCheckBox
            // 
            this._includeSubFoldersCheckBox.AutoSize = true;
            this._includeSubFoldersCheckBox.Location = new System.Drawing.Point(9, 86);
            this._includeSubFoldersCheckBox.Name = "_includeSubFoldersCheckBox";
            this._includeSubFoldersCheckBox.Size = new System.Drawing.Size(186, 17);
            this._includeSubFoldersCheckBox.TabIndex = 5;
            this._includeSubFoldersCheckBox.Text = "Include files from all subdirectories";
            this._includeSubFoldersCheckBox.UseVisualStyleBackColor = true;
            // 
            // _browseButton
            // 
            this._browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._browseButton.FolderBrowser = true;
            this._browseButton.Location = new System.Drawing.Point(393, 31);
            this._browseButton.Name = "_browseButton";
            this._browseButton.Size = new System.Drawing.Size(27, 23);
            this._browseButton.TabIndex = 2;
            this._browseButton.Text = "...";
            this._browseButton.TextControl = this._directoryTextBox;
            this._browseButton.UseVisualStyleBackColor = true;
            // 
            // _directoryTextBox
            // 
            this._directoryTextBox.Location = new System.Drawing.Point(6, 33);
            this._directoryTextBox.Name = "_directoryTextBox";
            this._directoryTextBox.Size = new System.Drawing.Size(381, 20);
            this._directoryTextBox.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 16);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(49, 13);
            label1.TabIndex = 0;
            label1.Text = "Directory";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(279, 141);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(78, 23);
            this._okButton.TabIndex = 1;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(360, 141);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(78, 23);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(6, 62);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(214, 13);
            label2.TabIndex = 3;
            label2.Text = "Display context menu option for files of type:";
            // 
            // _fileFilterComboBox
            // 
            this._fileFilterComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._fileFilterComboBox.FormattingEnabled = true;
            this._fileFilterComboBox.Items.AddRange(new object[] {
            "*.bmp;*.rle;*.dib;*.rst;*.gp4;*.mil;*.cal;*.cg4;*.flc;*.fli;*.gif;*.jpg;*.jpeg;*." +
                "pcx;*.pct;*.png;*.tga;*.tif;*.tiff;*.pdf",
            "*.bmp;*.rle;*.dib",
            "*.gif",
            "*.jpg;*.jpeg",
            "*.pcx",
            "*.pct",
            "*.pdf",
            "*.png",
            "*.tif;*.tiff",
            "*.txt",
            "*.xml",
            "*.*"});
            this._fileFilterComboBox.Location = new System.Drawing.Point(236, 59);
            this._fileFilterComboBox.Name = "_fileFilterComboBox";
            this._fileFilterComboBox.Size = new System.Drawing.Size(151, 21);
            this._fileFilterComboBox.TabIndex = 4;
            // 
            // DirectorySelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(453, 176);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DirectorySelectionForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Source file directory";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Extract.Utilities.Forms.BrowseButton _browseButton;
        private System.Windows.Forms.TextBox _directoryTextBox;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.CheckBox _includeSubFoldersCheckBox;
        private System.Windows.Forms.ComboBox _fileFilterComboBox;
    }
}