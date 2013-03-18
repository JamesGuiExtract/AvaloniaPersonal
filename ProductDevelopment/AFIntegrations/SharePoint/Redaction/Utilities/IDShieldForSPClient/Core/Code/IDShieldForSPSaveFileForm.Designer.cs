namespace Extract.SharePoint.Redaction.Utilities
{
    partial class IDShieldForSPSaveFileForm
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
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IDShieldForSPSaveFileForm));
            this._textSourceFile = new System.Windows.Forms.TextBox();
            this._treeSaveFile = new System.Windows.Forms.TreeView();
            this._treeViewImageList = new System.Windows.Forms.ImageList(this.components);
            this._buttonSave = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._textRedactedFile = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 48);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(61, 13);
            label1.TabIndex = 0;
            label1.Text = "File to save";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 9);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(73, 13);
            label2.TabIndex = 5;
            label2.Text = "Source Image";
            // 
            // _textSourceFile
            // 
            this._textSourceFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._textSourceFile.Location = new System.Drawing.Point(12, 25);
            this._textSourceFile.Name = "_textSourceFile";
            this._textSourceFile.ReadOnly = true;
            this._textSourceFile.Size = new System.Drawing.Size(327, 20);
            this._textSourceFile.TabIndex = 0;
            // 
            // _treeSaveFile
            // 
            this._treeSaveFile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._treeSaveFile.ImageIndex = 0;
            this._treeSaveFile.ImageList = this._treeViewImageList;
            this._treeSaveFile.Location = new System.Drawing.Point(13, 91);
            this._treeSaveFile.Name = "_treeSaveFile";
            this._treeSaveFile.SelectedImageIndex = 0;
            this._treeSaveFile.ShowLines = false;
            this._treeSaveFile.Size = new System.Drawing.Size(327, 331);
            this._treeSaveFile.TabIndex = 2;
            this._treeSaveFile.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.HandleTreeNodeExanded);
            this._treeSaveFile.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.HandleTreeNodeSelectionChanged);
            // 
            // _treeViewImageList
            // 
            this._treeViewImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_treeViewImageList.ImageStream")));
            this._treeViewImageList.TransparentColor = System.Drawing.Color.Transparent;
            this._treeViewImageList.Images.SetKeyName(0, "RedactNowSaveDisk16.png");
            this._treeViewImageList.Images.SetKeyName(1, "RedactNowSaveFolder16.png");
            this._treeViewImageList.Images.SetKeyName(2, "RedactNowWait16.png");
            // 
            // _buttonSave
            // 
            this._buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonSave.Location = new System.Drawing.Point(184, 428);
            this._buttonSave.Name = "_buttonSave";
            this._buttonSave.Size = new System.Drawing.Size(75, 23);
            this._buttonSave.TabIndex = 3;
            this._buttonSave.Text = "Save";
            this._buttonSave.UseVisualStyleBackColor = true;
            this._buttonSave.Click += new System.EventHandler(this.HandleSaveClick);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(265, 427);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 4;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _textRedactedFile
            // 
            this._textRedactedFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._textRedactedFile.Location = new System.Drawing.Point(12, 64);
            this._textRedactedFile.Name = "_textRedactedFile";
            this._textRedactedFile.Size = new System.Drawing.Size(328, 20);
            this._textRedactedFile.TabIndex = 1;
            this._textRedactedFile.TextChanged += new System.EventHandler(this.HandleRedactedFileTextChanged);
            // 
            // IDShieldForSPSaveFileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(352, 463);
            this.Controls.Add(this._textRedactedFile);
            this.Controls.Add(label2);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this._buttonSave);
            this.Controls.Add(this._treeSaveFile);
            this.Controls.Add(this._textSourceFile);
            this.Controls.Add(label1);
            this.MinimumSize = new System.Drawing.Size(360, 490);
            this.Name = "IDShieldForSPSaveFileForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Save Redacted File";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _textSourceFile;
        private System.Windows.Forms.TreeView _treeSaveFile;
        private System.Windows.Forms.Button _buttonSave;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.ImageList _treeViewImageList;
        private System.Windows.Forms.TextBox _textRedactedFile;

    }
}

