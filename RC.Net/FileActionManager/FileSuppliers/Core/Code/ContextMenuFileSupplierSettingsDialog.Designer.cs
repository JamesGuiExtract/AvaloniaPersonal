namespace Extract.FileActionManager.FileSuppliers
{
    partial class ContextMenuFileSupplierSettingsDialog
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
            System.Windows.Forms.Label label2;
            this._menuOptionNameTextBox = new System.Windows.Forms.TextBox();
            this._fileFilterComboBox = new System.Windows.Forms.ComboBox();
            this._limitRootCheckBox = new System.Windows.Forms.CheckBox();
            this._rootPathTextBox = new System.Windows.Forms.TextBox();
            this._inclueSubfoldersCheckBox = new System.Windows.Forms.CheckBox();
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnOK = new System.Windows.Forms.Button();
            this._pathBrowse = new Extract.Utilities.Forms.BrowseButton();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(9, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(133, 13);
            label1.TabIndex = 0;
            label1.Text = "Context menu option name";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(10, 59);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(214, 13);
            label2.TabIndex = 2;
            label2.Text = "Display context menu option for files of type:";
            // 
            // _menuOptionNameTextBox
            // 
            this._menuOptionNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._menuOptionNameTextBox.Location = new System.Drawing.Point(12, 29);
            this._menuOptionNameTextBox.Name = "_menuOptionNameTextBox";
            this._menuOptionNameTextBox.Size = new System.Drawing.Size(381, 20);
            this._menuOptionNameTextBox.TabIndex = 1;
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
            this._fileFilterComboBox.Location = new System.Drawing.Point(240, 56);
            this._fileFilterComboBox.Name = "_fileFilterComboBox";
            this._fileFilterComboBox.Size = new System.Drawing.Size(153, 21);
            this._fileFilterComboBox.TabIndex = 3;
            // 
            // _limitRootCheckBox
            // 
            this._limitRootCheckBox.AutoSize = true;
            this._limitRootCheckBox.Location = new System.Drawing.Point(13, 82);
            this._limitRootCheckBox.Name = "_limitRootCheckBox";
            this._limitRootCheckBox.Size = new System.Drawing.Size(268, 17);
            this._limitRootCheckBox.TabIndex = 4;
            this._limitRootCheckBox.Text = "Display context menu only within the following path:";
            this._limitRootCheckBox.UseVisualStyleBackColor = true;
            // 
            // _rootPathTextBox
            // 
            this._rootPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._rootPathTextBox.Location = new System.Drawing.Point(34, 106);
            this._rootPathTextBox.Name = "_rootPathTextBox";
            this._rootPathTextBox.Size = new System.Drawing.Size(359, 20);
            this._rootPathTextBox.TabIndex = 5;
            // 
            // _inclueSubfoldersCheckBox
            // 
            this._inclueSubfoldersCheckBox.AutoSize = true;
            this._inclueSubfoldersCheckBox.Location = new System.Drawing.Point(13, 133);
            this._inclueSubfoldersCheckBox.Name = "_inclueSubfoldersCheckBox";
            this._inclueSubfoldersCheckBox.Size = new System.Drawing.Size(326, 17);
            this._inclueSubfoldersCheckBox.TabIndex = 7;
            this._inclueSubfoldersCheckBox.Text = "Context menu applies to all matching files within selected folders";
            this._inclueSubfoldersCheckBox.UseVisualStyleBackColor = true;
            // 
            // _btnCancel
            // 
            this._btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(352, 163);
            this._btnCancel.MinimumSize = new System.Drawing.Size(75, 23);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 23);
            this._btnCancel.TabIndex = 9;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _btnOK
            // 
            this._btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnOK.Location = new System.Drawing.Point(271, 163);
            this._btnOK.Name = "_btnOK";
            this._btnOK.Size = new System.Drawing.Size(75, 23);
            this._btnOK.TabIndex = 8;
            this._btnOK.Text = "OK";
            this._btnOK.UseVisualStyleBackColor = true;
            this._btnOK.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _pathBrowse
            // 
            this._pathBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pathBrowse.FolderBrowser = true;
            this._pathBrowse.Location = new System.Drawing.Point(399, 105);
            this._pathBrowse.Name = "_pathBrowse";
            this._pathBrowse.Size = new System.Drawing.Size(27, 20);
            this._pathBrowse.TabIndex = 6;
            this._pathBrowse.Text = "...";
            this._pathBrowse.TextControl = this._rootPathTextBox;
            this._pathBrowse.UseVisualStyleBackColor = true;
            // 
            // ContextMenuFileSupplierSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 198);
            this.Controls.Add(label2);
            this.Controls.Add(this._pathBrowse);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnOK);
            this.Controls.Add(this._inclueSubfoldersCheckBox);
            this.Controls.Add(this._rootPathTextBox);
            this.Controls.Add(this._limitRootCheckBox);
            this.Controls.Add(this._fileFilterComboBox);
            this.Controls.Add(label1);
            this.Controls.Add(this._menuOptionNameTextBox);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(455, 236);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(455, 236);
            this.Name = "ContextMenuFileSupplierSettingsDialog";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure: Context menu file supplier";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _menuOptionNameTextBox;
        private System.Windows.Forms.ComboBox _fileFilterComboBox;
        private System.Windows.Forms.CheckBox _limitRootCheckBox;
        private System.Windows.Forms.TextBox _rootPathTextBox;
        private System.Windows.Forms.CheckBox _inclueSubfoldersCheckBox;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Button _btnOK;
        private Utilities.Forms.BrowseButton _pathBrowse;
    }
}