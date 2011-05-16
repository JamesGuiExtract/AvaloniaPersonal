namespace Extract.FileActionManager.FileProcessors
{
    partial class RasterizePdfTaskSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RasterizePdfTaskSettingsDialog));
            this._textPdfFile = new System.Windows.Forms.TextBox();
            this._textConvertedFile = new System.Windows.Forms.TextBox();
            this._checkModifySourceDoc = new System.Windows.Forms.CheckBox();
            this._checkDeletePdf = new System.Windows.Forms.CheckBox();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._groupDeleteFailed = new System.Windows.Forms.GroupBox();
            this._radioIgnoreError = new System.Windows.Forms.RadioButton();
            this._radioFailTask = new System.Windows.Forms.RadioButton();
            this._pathTagsButtonOutputFile = new Extract.Utilities.Forms.PathTagsButton();
            this._browseOutputFile = new Extract.Utilities.Forms.BrowseButton();
            this._pathTagsButtonPdfFile = new Extract.Utilities.Forms.PathTagsButton();
            this._browsePdfFile = new Extract.Utilities.Forms.BrowseButton();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this._groupDeleteFailed.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = System.Drawing.SystemColors.Control;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(44, 13);
            label1.TabIndex = 0;
            label1.Text = "PDF file";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 48);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(73, 13);
            label2.TabIndex = 4;
            label2.Text = "Rasterized file";
            // 
            // _textPdfFile
            // 
            this._textPdfFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textPdfFile.Location = new System.Drawing.Point(12, 25);
            this._textPdfFile.Name = "_textPdfFile";
            this._textPdfFile.Size = new System.Drawing.Size(359, 20);
            this._textPdfFile.TabIndex = 0;
            this._textPdfFile.TextChanged += new System.EventHandler(this.HandlePdfFileTextChanged);
            // 
            // _textConvertedFile
            // 
            this._textConvertedFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textConvertedFile.Location = new System.Drawing.Point(12, 64);
            this._textConvertedFile.Name = "_textConvertedFile";
            this._textConvertedFile.Size = new System.Drawing.Size(359, 20);
            this._textConvertedFile.TabIndex = 3;
            // 
            // _checkModifySourceDoc
            // 
            this._checkModifySourceDoc.AutoSize = true;
            this._checkModifySourceDoc.Location = new System.Drawing.Point(12, 90);
            this._checkModifySourceDoc.Name = "_checkModifySourceDoc";
            this._checkModifySourceDoc.Size = new System.Drawing.Size(279, 17);
            this._checkModifySourceDoc.TabIndex = 7;
            this._checkModifySourceDoc.Text = "Modify SourceDocName in database after conversion";
            this._checkModifySourceDoc.UseVisualStyleBackColor = true;
            this._checkModifySourceDoc.CheckedChanged += new System.EventHandler(this.HandleModifySourceDocCheckChanged);
            // 
            // _checkDeletePdf
            // 
            this._checkDeletePdf.AutoSize = true;
            this._checkDeletePdf.Location = new System.Drawing.Point(12, 113);
            this._checkDeletePdf.Name = "_checkDeletePdf";
            this._checkDeletePdf.Size = new System.Drawing.Size(180, 17);
            this._checkDeletePdf.TabIndex = 6;
            this._checkDeletePdf.Text = "Delete PDF file after rasterization";
            this._checkDeletePdf.UseVisualStyleBackColor = true;
            this._checkDeletePdf.CheckedChanged += new System.EventHandler(this.HandleDeletePdfCheckChanged);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(353, 207);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 9;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.Location = new System.Drawing.Point(272, 207);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 8;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.HandleOkClicked);
            // 
            // _groupDeleteFailed
            // 
            this._groupDeleteFailed.Controls.Add(this._radioIgnoreError);
            this._groupDeleteFailed.Controls.Add(this._radioFailTask);
            this._groupDeleteFailed.Location = new System.Drawing.Point(12, 136);
            this._groupDeleteFailed.Name = "_groupDeleteFailed";
            this._groupDeleteFailed.Size = new System.Drawing.Size(358, 65);
            this._groupDeleteFailed.TabIndex = 10;
            this._groupDeleteFailed.TabStop = false;
            this._groupDeleteFailed.Text = "If the file cannot be deleted";
            // 
            // _radioIgnoreError
            // 
            this._radioIgnoreError.AutoSize = true;
            this._radioIgnoreError.Location = new System.Drawing.Point(6, 42);
            this._radioIgnoreError.Name = "_radioIgnoreError";
            this._radioIgnoreError.Size = new System.Drawing.Size(162, 17);
            this._radioIgnoreError.TabIndex = 1;
            this._radioIgnoreError.TabStop = true;
            this._radioIgnoreError.Text = "Ignore the error and continue";
            this._radioIgnoreError.UseVisualStyleBackColor = true;
            // 
            // _radioFailTask
            // 
            this._radioFailTask.AutoSize = true;
            this._radioFailTask.Location = new System.Drawing.Point(6, 19);
            this._radioFailTask.Name = "_radioFailTask";
            this._radioFailTask.Size = new System.Drawing.Size(175, 17);
            this._radioFailTask.TabIndex = 0;
            this._radioFailTask.TabStop = true;
            this._radioFailTask.Text = "Fail the task and record an error";
            this._radioFailTask.UseVisualStyleBackColor = true;
            // 
            // _pathTagsButtonOutputFile
            // 
            this._pathTagsButtonOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pathTagsButtonOutputFile.Image = ((System.Drawing.Image)(resources.GetObject("_pathTagsButtonOutputFile.Image")));
            this._pathTagsButtonOutputFile.Location = new System.Drawing.Point(377, 64);
            this._pathTagsButtonOutputFile.Name = "_pathTagsButtonOutputFile";
            this._pathTagsButtonOutputFile.Size = new System.Drawing.Size(18, 20);
            this._pathTagsButtonOutputFile.TabIndex = 4;
            this._pathTagsButtonOutputFile.TextControl = this._textConvertedFile;
            this._pathTagsButtonOutputFile.UseVisualStyleBackColor = true;
            // 
            // _browseOutputFile
            // 
            this._browseOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._browseOutputFile.Location = new System.Drawing.Point(401, 64);
            this._browseOutputFile.Name = "_browseOutputFile";
            this._browseOutputFile.Size = new System.Drawing.Size(27, 20);
            this._browseOutputFile.TabIndex = 5;
            this._browseOutputFile.Text = "...";
            this._browseOutputFile.TextControl = this._textConvertedFile;
            this._browseOutputFile.UseVisualStyleBackColor = true;
            // 
            // _pathTagsButtonPdfFile
            // 
            this._pathTagsButtonPdfFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pathTagsButtonPdfFile.Image = ((System.Drawing.Image)(resources.GetObject("_pathTagsButtonPdfFile.Image")));
            this._pathTagsButtonPdfFile.Location = new System.Drawing.Point(377, 25);
            this._pathTagsButtonPdfFile.Name = "_pathTagsButtonPdfFile";
            this._pathTagsButtonPdfFile.Size = new System.Drawing.Size(18, 20);
            this._pathTagsButtonPdfFile.TabIndex = 1;
            this._pathTagsButtonPdfFile.TextControl = this._textPdfFile;
            this._pathTagsButtonPdfFile.UseVisualStyleBackColor = true;
            // 
            // _browsePdfFile
            // 
            this._browsePdfFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._browsePdfFile.Location = new System.Drawing.Point(401, 25);
            this._browsePdfFile.Name = "_browsePdfFile";
            this._browsePdfFile.Size = new System.Drawing.Size(27, 20);
            this._browsePdfFile.TabIndex = 2;
            this._browsePdfFile.Text = "...";
            this._browsePdfFile.TextControl = this._textPdfFile;
            this._browsePdfFile.UseVisualStyleBackColor = true;
            // 
            // RasterizePdfTaskSettingsDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(440, 242);
            this.Controls.Add(this._textPdfFile);
            this.Controls.Add(this._groupDeleteFailed);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this._checkDeletePdf);
            this.Controls.Add(this._checkModifySourceDoc);
            this.Controls.Add(this._pathTagsButtonOutputFile);
            this.Controls.Add(this._browseOutputFile);
            this.Controls.Add(this._textConvertedFile);
            this.Controls.Add(label2);
            this.Controls.Add(this._pathTagsButtonPdfFile);
            this.Controls.Add(this._browsePdfFile);
            this.Controls.Add(label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RasterizePdfTaskSettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Core: Rasterize PDF Settings";
            this._groupDeleteFailed.ResumeLayout(false);
            this._groupDeleteFailed.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Utilities.Forms.PathTagsButton _pathTagsButtonOutputFile;
        private Utilities.Forms.BrowseButton _browseOutputFile;
        private System.Windows.Forms.TextBox _textConvertedFile;
        private Utilities.Forms.BrowseButton _browsePdfFile;
        private Utilities.Forms.PathTagsButton _pathTagsButtonPdfFile;
        private System.Windows.Forms.TextBox _textPdfFile;
        private System.Windows.Forms.CheckBox _checkModifySourceDoc;
        private System.Windows.Forms.CheckBox _checkDeletePdf;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.RadioButton _radioIgnoreError;
        private System.Windows.Forms.RadioButton _radioFailTask;
        private System.Windows.Forms.GroupBox _groupDeleteFailed;
    }
}