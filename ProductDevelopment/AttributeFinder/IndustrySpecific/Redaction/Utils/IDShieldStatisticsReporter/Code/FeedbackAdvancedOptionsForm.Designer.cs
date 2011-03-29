namespace Extract.IDShieldStatisticsReporter
{
    partial class FeedbackAdvancedOptionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FeedbackAdvancedOptionsForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._foundDataPathTextBox = new System.Windows.Forms.TextBox();
            this._foundDataBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._foundDataPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._expectedDataPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._expectedDataBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._expectedDataPathTextBox = new System.Windows.Forms.TextBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(194, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "ID Shield data file with found redactions";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(211, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "ID Shield data file with expected redactions";
            // 
            // _foundDataPathTextBox
            // 
            this._foundDataPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._foundDataPathTextBox.Location = new System.Drawing.Point(15, 26);
            this._foundDataPathTextBox.Name = "_foundDataPathTextBox";
            this._foundDataPathTextBox.Size = new System.Drawing.Size(404, 20);
            this._foundDataPathTextBox.TabIndex = 1;
            // 
            // _foundDataBrowseButton
            // 
            this._foundDataBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._foundDataBrowseButton.Location = new System.Drawing.Point(450, 25);
            this._foundDataBrowseButton.Name = "_foundDataBrowseButton";
            this._foundDataBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._foundDataBrowseButton.TabIndex = 3;
            this._foundDataBrowseButton.Text = "...";
            this._foundDataBrowseButton.TextControl = this._foundDataPathTextBox;
            this._foundDataBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _foundDataPathTagsButton
            // 
            this._foundDataPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._foundDataPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_foundDataPathTagsButton.Image")));
            this._foundDataPathTagsButton.Location = new System.Drawing.Point(425, 25);
            this._foundDataPathTagsButton.Name = "_foundDataPathTagsButton";
            this._foundDataPathTagsButton.Size = new System.Drawing.Size(18, 22);
            this._foundDataPathTagsButton.TabIndex = 2;
            this._foundDataPathTagsButton.TextControl = _foundDataPathTextBox;
            this._foundDataPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _expectedDataPathTagsButton
            // 
            this._expectedDataPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._expectedDataPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_expectedDataPathTagsButton.Image")));
            this._expectedDataPathTagsButton.Location = new System.Drawing.Point(425, 65);
            this._expectedDataPathTagsButton.Name = "_expectedDataPathTagsButton";
            this._expectedDataPathTagsButton.Size = new System.Drawing.Size(18, 21);
            this._expectedDataPathTagsButton.TabIndex = 6;
            this._expectedDataPathTagsButton.TextControl = _expectedDataPathTextBox;
            this._expectedDataPathTagsButton.UseVisualStyleBackColor = true;
            //
            // _expectedDataBrowseButton
            // 
            this._expectedDataBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._expectedDataBrowseButton.Location = new System.Drawing.Point(450, 65);
            this._expectedDataBrowseButton.Name = "_expectedDataBrowseButton";
            this._expectedDataBrowseButton.Size = new System.Drawing.Size(27, 21);
            this._expectedDataBrowseButton.TabIndex = 7;
            this._expectedDataBrowseButton.Text = "...";
            this._expectedDataBrowseButton.TextControl = this._expectedDataPathTextBox;
            this._expectedDataBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _expectedDataPathTextBox
            // 
            this._expectedDataPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._expectedDataPathTextBox.Location = new System.Drawing.Point(15, 65);
            this._expectedDataPathTextBox.Name = "_expectedDataPathTextBox";
            this._expectedDataPathTextBox.Size = new System.Drawing.Size(404, 20);
            this._expectedDataPathTextBox.TabIndex = 5;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(402, 92);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 9;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(321, 92);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 8;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.OnOkButtonClick);
            // 
            // FeedbackAdvancedOptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(487, 125);
            this.ControlBox = false;
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._expectedDataPathTagsButton);
            this.Controls.Add(this._expectedDataBrowseButton);
            this.Controls.Add(this._expectedDataPathTextBox);
            this.Controls.Add(this._foundDataPathTagsButton);
            this.Controls.Add(this._foundDataBrowseButton);
            this.Controls.Add(this._foundDataPathTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FeedbackAdvancedOptionsForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Advanced Options";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _foundDataPathTextBox;
        private Extract.Utilities.Forms.BrowseButton _foundDataBrowseButton;
        private Extract.Utilities.Forms.PathTagsButton _foundDataPathTagsButton;
        private Extract.Utilities.Forms.PathTagsButton _expectedDataPathTagsButton;
        private Extract.Utilities.Forms.BrowseButton _expectedDataBrowseButton;
        private System.Windows.Forms.TextBox _expectedDataPathTextBox;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
    }
}