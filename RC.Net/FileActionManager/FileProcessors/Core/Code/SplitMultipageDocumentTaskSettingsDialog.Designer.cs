namespace Extract.FileActionManager.FileProcessors
{
    partial class SplitMultipageDocumentTaskSettingsDialog
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
            System.Windows.Forms.Label label3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplitMultipageDocumentTaskSettingsDialog));
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._outputPathTextBox = new System.Windows.Forms.TextBox();
            this._voaPathTextBox = new System.Windows.Forms.TextBox();
            this._outputPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._voaPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(291, 13);
            label1.TabIndex = 0;
            label1.Text = "Output filename specification (must include <PageNumber>):";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 48);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(71, 13);
            label2.TabIndex = 3;
            label2.Text = "VOA filename";
            // 
            // label3
            // 
            label3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            label3.Location = new System.Drawing.Point(9, 87);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(477, 49);
            label3.TabIndex = 6;
            label3.Text = "Note: This expression is used to define both the source and output voa filename w" +
    "here the <SourceDocName> becomes the ouputFileName for the output voa.";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(435, 139);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 8;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(354, 139);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 7;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _outputPathTextBox
            // 
            this._outputPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathTextBox.Location = new System.Drawing.Point(12, 25);
            this._outputPathTextBox.Name = "_outputPathTextBox";
            this._outputPathTextBox.Size = new System.Drawing.Size(474, 20);
            this._outputPathTextBox.TabIndex = 1;
            this._outputPathTextBox.Text = "Test";
            // 
            // _voaPathTextBox
            // 
            this._voaPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._voaPathTextBox.Location = new System.Drawing.Point(12, 64);
            this._voaPathTextBox.Name = "_voaPathTextBox";
            this._voaPathTextBox.Size = new System.Drawing.Size(474, 20);
            // 
            // _outputPathTagButton
            // 
            this._outputPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_outputPathTagButton.Image")));
            this._outputPathTagButton.Location = new System.Drawing.Point(492, 24);
            this._outputPathTagButton.Name = "_outputPathTagButton";
            this._outputPathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._outputPathTagButton.Size = new System.Drawing.Size(18, 21);
            this._outputPathTagButton.TabIndex = 2;
            this._outputPathTagButton.TextControl = this._outputPathTextBox;
            this._outputPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _voaPathTagButton
            // 
            this._voaPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._voaPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_voaPathTagButton.Image")));
            this._voaPathTagButton.Location = new System.Drawing.Point(492, 64);
            this._voaPathTagButton.Name = "_voaPathTagButton";
            this._voaPathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._voaPathTagButton.Size = new System.Drawing.Size(18, 21);
            this._voaPathTagButton.TabIndex = 5;
            this._voaPathTagButton.TextControl = this._voaPathTextBox;
            this._voaPathTagButton.UseVisualStyleBackColor = true;
            // 
            // SplitMultipageDocumentTaskSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(522, 173);
            this.Controls.Add(this._voaPathTagButton);
            this.Controls.Add(this._outputPathTagButton);
            this.Controls.Add(label3);
            this.Controls.Add(this._voaPathTextBox);
            this.Controls.Add(label2);
            this.Controls.Add(this._outputPathTextBox);
            this.Controls.Add(label1);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 326);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(351, 211);
            this.Name = "SplitMultipageDocumentTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Core: Split multi-page document";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TextBox _outputPathTextBox;
        private System.Windows.Forms.TextBox _voaPathTextBox;
        private Forms.FileActionManagerPathTagButton _outputPathTagButton;
        private Forms.FileActionManagerPathTagButton _voaPathTagButton;
    }
}