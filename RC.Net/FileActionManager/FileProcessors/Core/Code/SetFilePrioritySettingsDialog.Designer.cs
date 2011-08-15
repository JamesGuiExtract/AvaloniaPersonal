namespace Extract.FileActionManager.FileProcessors
{
    partial class SetFilePrioritySettingsDialog
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
            System.Windows.Forms.GroupBox _fileNameGroupBox;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetFilePrioritySettingsDialog));
            System.Windows.Forms.Label label3;
            System.Windows.Forms.GroupBox _priorityGroupBox;
            System.Windows.Forms.Label label1;
            this._fileTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._fileNameTextBox = new System.Windows.Forms.TextBox();
            this._browseButton = new Extract.Utilities.Forms.BrowseButton();
            this._priorityComboBox = new System.Windows.Forms.ComboBox();
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnOK = new System.Windows.Forms.Button();
            _fileNameGroupBox = new System.Windows.Forms.GroupBox();
            label3 = new System.Windows.Forms.Label();
            _priorityGroupBox = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            _fileNameGroupBox.SuspendLayout();
            _priorityGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _fileNameGroupBox
            // 
            _fileNameGroupBox.Controls.Add(this._fileTagsButton);
            _fileNameGroupBox.Controls.Add(this._browseButton);
            _fileNameGroupBox.Controls.Add(this._fileNameTextBox);
            _fileNameGroupBox.Controls.Add(label3);
            _fileNameGroupBox.Location = new System.Drawing.Point(12, 12);
            _fileNameGroupBox.Name = "_fileNameGroupBox";
            _fileNameGroupBox.Size = new System.Drawing.Size(322, 60);
            _fileNameGroupBox.TabIndex = 8;
            _fileNameGroupBox.TabStop = false;
            _fileNameGroupBox.Text = "Filename";
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
            this._fileTagsButton.TabIndex = 1;
            this._fileTagsButton.TextControl = this._fileNameTextBox;
            this._fileTagsButton.UseVisualStyleBackColor = true;
            // 
            // _fileNameTextBox
            // 
            this._fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._fileNameTextBox.Location = new System.Drawing.Point(6, 33);
            this._fileNameTextBox.Name = "_fileNameTextBox";
            this._fileNameTextBox.Size = new System.Drawing.Size(250, 20);
            this._fileNameTextBox.TabIndex = 0;
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
            this._browseButton.TabIndex = 2;
            this._browseButton.Text = "...";
            this._browseButton.TextControl = this._fileNameTextBox;
            this._browseButton.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(6, 16);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(122, 13);
            label3.TabIndex = 0;
            label3.Text = "File to change priority for";
            // 
            // _priorityGroupBox
            // 
            _priorityGroupBox.Controls.Add(this._priorityComboBox);
            _priorityGroupBox.Controls.Add(label1);
            _priorityGroupBox.Location = new System.Drawing.Point(12, 78);
            _priorityGroupBox.Name = "_priorityGroupBox";
            _priorityGroupBox.Size = new System.Drawing.Size(322, 60);
            _priorityGroupBox.TabIndex = 9;
            _priorityGroupBox.TabStop = false;
            _priorityGroupBox.Text = "Priority";
            // 
            // _priorityComboBox
            // 
            this._priorityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._priorityComboBox.FormattingEnabled = true;
            this._priorityComboBox.Location = new System.Drawing.Point(9, 31);
            this._priorityComboBox.Name = "_priorityComboBox";
            this._priorityComboBox.Size = new System.Drawing.Size(247, 21);
            this._priorityComboBox.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 16);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(98, 13);
            label1.TabIndex = 0;
            label1.Text = "Priority to set for file";
            // 
            // _btnCancel
            // 
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(259, 144);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 23);
            this._btnCancel.TabIndex = 1;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _btnOK
            // 
            this._btnOK.Location = new System.Drawing.Point(178, 144);
            this._btnOK.Name = "_btnOK";
            this._btnOK.Size = new System.Drawing.Size(75, 23);
            this._btnOK.TabIndex = 0;
            this._btnOK.Text = "OK";
            this._btnOK.UseVisualStyleBackColor = true;
            this._btnOK.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // SetFilePrioritySettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnCancel;
            this.ClientSize = new System.Drawing.Size(347, 177);
            this.Controls.Add(this._btnOK);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(_priorityGroupBox);
            this.Controls.Add(_fileNameGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetFilePrioritySettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Core: Set file priority";
            _fileNameGroupBox.ResumeLayout(false);
            _fileNameGroupBox.PerformLayout();
            _priorityGroupBox.ResumeLayout(false);
            _priorityGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Utilities.Forms.PathTagsButton _fileTagsButton;
        private Utilities.Forms.BrowseButton _browseButton;
        private System.Windows.Forms.TextBox _fileNameTextBox;
        private System.Windows.Forms.ComboBox _priorityComboBox;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Button _btnOK;

    }
}