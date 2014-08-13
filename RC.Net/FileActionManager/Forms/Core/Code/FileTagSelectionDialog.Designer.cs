namespace Extract.FileActionManager.Forms
{
    partial class FileTagSelectionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileTagSelectionDialog));
            this._allTagsCheckBox = new System.Windows.Forms.CheckBox();
            this._selectedTagsCheckBox = new System.Windows.Forms.CheckBox();
            this._selectedTagsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this._tagFilterCheckBox = new System.Windows.Forms.CheckBox();
            this._tagFilterTextBox = new System.Windows.Forms.TextBox();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.infoTip1 = new Extract.Utilities.Forms.InfoTip();
            this.SuspendLayout();
            // 
            // _allTagsCheckBox
            // 
            this._allTagsCheckBox.AutoSize = true;
            this._allTagsCheckBox.Location = new System.Drawing.Point(13, 13);
            this._allTagsCheckBox.Name = "_allTagsCheckBox";
            this._allTagsCheckBox.Size = new System.Drawing.Size(141, 17);
            this._allTagsCheckBox.TabIndex = 0;
            this._allTagsCheckBox.Text = "Display all available tags";
            this._allTagsCheckBox.UseVisualStyleBackColor = true;
            this._allTagsCheckBox.CheckedChanged += new System.EventHandler(this.HandleAllTagsCheckBox_CheckedChanged);
            // 
            // _selectedTagsCheckBox
            // 
            this._selectedTagsCheckBox.AutoSize = true;
            this._selectedTagsCheckBox.Location = new System.Drawing.Point(13, 36);
            this._selectedTagsCheckBox.Name = "_selectedTagsCheckBox";
            this._selectedTagsCheckBox.Size = new System.Drawing.Size(115, 17);
            this._selectedTagsCheckBox.TabIndex = 1;
            this._selectedTagsCheckBox.Text = "Display these tags:";
            this._selectedTagsCheckBox.UseVisualStyleBackColor = true;
            this._selectedTagsCheckBox.CheckedChanged += new System.EventHandler(this.HandleSelectedTagsCheckBox_CheckedChanged);
            // 
            // _selectedTagsCheckedListBox
            // 
            this._selectedTagsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._selectedTagsCheckedListBox.CheckOnClick = true;
            this._selectedTagsCheckedListBox.Enabled = false;
            this._selectedTagsCheckedListBox.FormattingEnabled = true;
            this._selectedTagsCheckedListBox.Location = new System.Drawing.Point(12, 59);
            this._selectedTagsCheckedListBox.Name = "_selectedTagsCheckedListBox";
            this._selectedTagsCheckedListBox.Size = new System.Drawing.Size(304, 184);
            this._selectedTagsCheckedListBox.TabIndex = 2;
            // 
            // _tagFilterCheckBox
            // 
            this._tagFilterCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._tagFilterCheckBox.AutoSize = true;
            this._tagFilterCheckBox.Location = new System.Drawing.Point(13, 249);
            this._tagFilterCheckBox.Name = "_tagFilterCheckBox";
            this._tagFilterCheckBox.Size = new System.Drawing.Size(274, 17);
            this._tagFilterCheckBox.TabIndex = 3;
            this._tagFilterCheckBox.Text = " Display any tags that match these wildcard patterns:";
            this._tagFilterCheckBox.UseVisualStyleBackColor = true;
            this._tagFilterCheckBox.CheckedChanged += new System.EventHandler(this.HandleTagFilterCheckBox_CheckedChanged);
            // 
            // _tagFilterTextBox
            // 
            this._tagFilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tagFilterTextBox.Enabled = false;
            this._tagFilterTextBox.Location = new System.Drawing.Point(13, 273);
            this._tagFilterTextBox.Name = "_tagFilterTextBox";
            this._tagFilterTextBox.Size = new System.Drawing.Size(303, 20);
            this._tagFilterTextBox.TabIndex = 4;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(160, 305);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 6;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(241, 305);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 7;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // infoTip1
            // 
            this.infoTip1.BackColor = System.Drawing.Color.Transparent;
            this.infoTip1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip1.BackgroundImage")));
            this.infoTip1.Location = new System.Drawing.Point(300, 250);
            this.infoTip1.Name = "infoTip1";
            this.infoTip1.Size = new System.Drawing.Size(16, 16);
            this.infoTip1.TabIndex = 8;
            this.infoTip1.TipText = "The specified wildcard pattern accepts the char \'?\' to represent any single chara" +
    "cter or \'*\' to represent zero or more characters. Multiple wildcard patterns can" +
    " be delimited with commas.";
            // 
            // FileTagSelectionDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(328, 340);
            this.Controls.Add(this.infoTip1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._tagFilterTextBox);
            this.Controls.Add(this._tagFilterCheckBox);
            this.Controls.Add(this._selectedTagsCheckedListBox);
            this.Controls.Add(this._selectedTagsCheckBox);
            this.Controls.Add(this._allTagsCheckBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FileTagSelectionDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Tags";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox _allTagsCheckBox;
        private System.Windows.Forms.CheckBox _selectedTagsCheckBox;
        private System.Windows.Forms.CheckedListBox _selectedTagsCheckedListBox;
        private System.Windows.Forms.CheckBox _tagFilterCheckBox;
        private System.Windows.Forms.TextBox _tagFilterTextBox;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private Utilities.Forms.InfoTip infoTip1;
    }
}