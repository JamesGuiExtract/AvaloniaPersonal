namespace Extract.FileActionManager.Conditions
{
    partial class DocumentTypeConditionSettingsDialog
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label label6;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocumentTypeConditionSettingsDialog));
            System.Windows.Forms.Label label7;
            this._metComboBox = new System.Windows.Forms.ComboBox();
            this._voaFilePathTags = new Extract.Utilities.Forms.PathTagsButton();
            this._voaFileTextBox = new System.Windows.Forms.TextBox();
            this._voaFileBrowse = new Extract.Utilities.Forms.BrowseButton();
            this._clearButton = new System.Windows.Forms.Button();
            this._removeButton = new System.Windows.Forms.Button();
            this._selectButton = new System.Windows.Forms.Button();
            this._documentTypeListBox = new System.Windows.Forms.ListBox();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(8, 67);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(250, 13);
            label1.TabIndex = 33;
            label1.Text = "has a document type matching one of the following:";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(this._metComboBox);
            groupBox1.Controls.Add(this._voaFilePathTags);
            groupBox1.Controls.Add(this._voaFileBrowse);
            groupBox1.Controls.Add(this._voaFileTextBox);
            groupBox1.Controls.Add(this._clearButton);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(this._removeButton);
            groupBox1.Controls.Add(this._selectButton);
            groupBox1.Controls.Add(this._documentTypeListBox);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(429, 346);
            groupBox1.TabIndex = 37;
            groupBox1.TabStop = false;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(187, 16);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(118, 13);
            label6.TabIndex = 43;
            label6.Text = "if the following VOA file:";
            // 
            // _metComboBox
            // 
            this._metComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._metComboBox.FormattingEnabled = true;
            this._metComboBox.Items.AddRange(new object[] {
            "met",
            "not met"});
            this._metComboBox.Location = new System.Drawing.Point(122, 13);
            this._metComboBox.Name = "_metComboBox";
            this._metComboBox.Size = new System.Drawing.Size(59, 21);
            this._metComboBox.TabIndex = 42;
            // 
            // _voaFilePathTags
            // 
            this._voaFilePathTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFilePathTags.Image = ((System.Drawing.Image)(resources.GetObject("_voaFilePathTags.Image")));
            this._voaFilePathTags.Location = new System.Drawing.Point(372, 40);
            this._voaFilePathTags.Name = "_voaFilePathTags";
            this._voaFilePathTags.Size = new System.Drawing.Size(18, 20);
            this._voaFilePathTags.TabIndex = 40;
            this._voaFilePathTags.TextControl = this._voaFileTextBox;
            this._voaFilePathTags.UseVisualStyleBackColor = true;
            // 
            // _voaFileTextBox
            // 
            this._voaFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFileTextBox.Location = new System.Drawing.Point(8, 41);
            this._voaFileTextBox.Name = "_voaFileTextBox";
            this._voaFileTextBox.Size = new System.Drawing.Size(358, 20);
            this._voaFileTextBox.TabIndex = 39;
            // 
            // _voaFileBrowse
            // 
            this._voaFileBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFileBrowse.Location = new System.Drawing.Point(396, 40);
            this._voaFileBrowse.Name = "_voaFileBrowse";
            this._voaFileBrowse.Size = new System.Drawing.Size(27, 20);
            this._voaFileBrowse.TabIndex = 41;
            this._voaFileBrowse.Text = "...";
            this._voaFileBrowse.TextControl = this._voaFileTextBox;
            this._voaFileBrowse.UseVisualStyleBackColor = true;
            // 
            // _clearButton
            // 
            this._clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._clearButton.Location = new System.Drawing.Point(348, 141);
            this._clearButton.Name = "_clearButton";
            this._clearButton.Size = new System.Drawing.Size(75, 23);
            this._clearButton.TabIndex = 39;
            this._clearButton.Text = "Clear";
            this._clearButton.UseVisualStyleBackColor = true;
            this._clearButton.Click += new System.EventHandler(this.HandleListClearButtonClick);
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(8, 16);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(108, 13);
            label7.TabIndex = 38;
            label7.Text = "Consider condition as";
            // 
            // _removeButton
            // 
            this._removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._removeButton.Location = new System.Drawing.Point(348, 112);
            this._removeButton.Name = "_removeButton";
            this._removeButton.Size = new System.Drawing.Size(75, 23);
            this._removeButton.TabIndex = 38;
            this._removeButton.Text = "Remove";
            this._removeButton.UseVisualStyleBackColor = true;
            this._removeButton.Click += new System.EventHandler(this.HandleListRemoveButtonClick);
            // 
            // _selectButton
            // 
            this._selectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._selectButton.Location = new System.Drawing.Point(348, 83);
            this._selectButton.Name = "_selectButton";
            this._selectButton.Size = new System.Drawing.Size(75, 23);
            this._selectButton.TabIndex = 37;
            this._selectButton.Text = "Select...";
            this._selectButton.UseVisualStyleBackColor = true;
            this._selectButton.Click += new System.EventHandler(this.HandleDocTypeSelectButtonClick);
            // 
            // _documentTypeListBox
            // 
            this._documentTypeListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._documentTypeListBox.FormattingEnabled = true;
            this._documentTypeListBox.Location = new System.Drawing.Point(9, 83);
            this._documentTypeListBox.Name = "_documentTypeListBox";
            this._documentTypeListBox.Size = new System.Drawing.Size(333, 251);
            this._documentTypeListBox.TabIndex = 36;
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.Location = new System.Drawing.Point(285, 397);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 31;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(366, 397);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 32;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // DocumentTypeConditionSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(453, 432);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DocumentTypeConditionSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Core: Document Type Condition";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.ListBox _documentTypeListBox;
        private System.Windows.Forms.Button _clearButton;
        private System.Windows.Forms.Button _removeButton;
        private System.Windows.Forms.Button _selectButton;
        private System.Windows.Forms.ComboBox _metComboBox;
        private Utilities.Forms.PathTagsButton _voaFilePathTags;
        private System.Windows.Forms.TextBox _voaFileTextBox;
        private Utilities.Forms.BrowseButton _voaFileBrowse;
    }
}