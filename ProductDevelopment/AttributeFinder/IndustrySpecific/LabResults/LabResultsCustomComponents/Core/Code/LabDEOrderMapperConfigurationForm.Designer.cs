namespace Extract.LabResultsCustomComponents
{
    partial class LabDEOrderMapperConfigurationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LabDEOrderMapperConfigurationForm));
            this.label1 = new System.Windows.Forms.Label();
            this._textDatabaseFile = new System.Windows.Forms.TextBox();
            this._buttonTags = new Extract.Utilities.Forms.PathTagsButton();
            this._buttonBrowse = new Extract.Utilities.Forms.BrowseButton();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Database file";
            // 
            // textDatabaseFile
            // 
            this._textDatabaseFile.Location = new System.Drawing.Point(15, 25);
            this._textDatabaseFile.Name = "textDatabaseFile";
            this._textDatabaseFile.Size = new System.Drawing.Size(408, 20);
            this._textDatabaseFile.TabIndex = 1;
            // 
            // buttonTags
            // 
            this._buttonTags.Image = ((System.Drawing.Image)(resources.GetObject("buttonTags.Image")));
            this._buttonTags.Location = new System.Drawing.Point(429, 23);
            this._buttonTags.Name = "buttonTags";
            this._buttonTags.Size = new System.Drawing.Size(22, 22);
            this._buttonTags.TabIndex = 2;
            this._buttonTags.TagSelected += new System.EventHandler<Extract.Utilities.Forms.TagSelectedEventArgs>(this.HandlePathTagsButtonSelected);
            // 
            // buttonBrowse
            // 
            this._buttonBrowse.Location = new System.Drawing.Point(457, 23);
            this._buttonBrowse.Name = "buttonBrowse";
            this._buttonBrowse.TabIndex = 3;
            this._buttonBrowse.UseVisualStyleBackColor = true;
            this._buttonBrowse.FolderBrowser = false;
            this._buttonBrowse.FileFilter = "SQL Compact Database File (*.sdf)|*.sdf||";
            this._buttonBrowse.TextControl = this._textDatabaseFile;
            // 
            // buttonOk
            // 
            this._buttonOk.Location = new System.Drawing.Point(328, 53);
            this._buttonOk.Name = "buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 4;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.HandleOkButtonClicked);
            // 
            // buttonCancel
            // 
            this._buttonCancel.Location = new System.Drawing.Point(409, 53);
            this._buttonCancel.Name = "buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 5;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            this._buttonCancel.Click += new System.EventHandler(this.HandleCancelClicked);
            // 
            // LabDEOrderMapperConfigurationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(496, 88);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonBrowse);
            this.Controls.Add(this._textDatabaseFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._buttonTags);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LabDEOrderMapperConfigurationForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Lab DE Order Mapping Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _textDatabaseFile;
        private Extract.Utilities.Forms.PathTagsButton _buttonTags;
        private Extract.Utilities.Forms.BrowseButton _buttonBrowse;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonCancel;
    }
}