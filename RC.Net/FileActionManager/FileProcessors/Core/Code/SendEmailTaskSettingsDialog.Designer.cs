namespace Extract.FileActionManager.FileProcessors
{
    partial class SendEmailTaskSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SendEmailTaskSettingsDialog));
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._bodyTextBox = new System.Windows.Forms.TextBox();
            this._subjectTextBox = new System.Windows.Forms.TextBox();
            this._carbonCopyRecipient = new System.Windows.Forms.TextBox();
            this._recipientTextBox = new System.Windows.Forms.TextBox();
            this._attachmentsButton = new System.Windows.Forms.Button();
            this._advancedButton = new System.Windows.Forms.Button();
            this._bodyPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._subjectPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._recipientPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._carbonCopyPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._resetErrorSettingsButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 15);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(20, 13);
            label1.TabIndex = 0;
            label1.Text = "To";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(13, 41);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(20, 13);
            label2.TabIndex = 3;
            label2.Text = "Cc";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(13, 67);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(43, 13);
            label3.TabIndex = 6;
            label3.Text = "Subject";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(480, 361);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 14;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(561, 361);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 15;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _bodyTextBox
            // 
            this._bodyTextBox.AcceptsReturn = true;
            this._bodyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._bodyTextBox.Location = new System.Drawing.Point(16, 91);
            this._bodyTextBox.Multiline = true;
            this._bodyTextBox.Name = "_bodyTextBox";
            this._bodyTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._bodyTextBox.Size = new System.Drawing.Size(596, 264);
            this._bodyTextBox.TabIndex = 10;
            // 
            // _subjectTextBox
            // 
            this._subjectTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._subjectTextBox.Location = new System.Drawing.Point(62, 64);
            this._subjectTextBox.Name = "_subjectTextBox";
            this._subjectTextBox.Size = new System.Drawing.Size(418, 20);
            this._subjectTextBox.TabIndex = 7;
            // 
            // _carbonCopyRecipient
            // 
            this._carbonCopyRecipient.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._carbonCopyRecipient.Location = new System.Drawing.Point(62, 38);
            this._carbonCopyRecipient.Name = "_carbonCopyRecipient";
            this._carbonCopyRecipient.Size = new System.Drawing.Size(550, 20);
            this._carbonCopyRecipient.TabIndex = 4;
            // 
            // _recipientTextBox
            // 
            this._recipientTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._recipientTextBox.Location = new System.Drawing.Point(62, 12);
            this._recipientTextBox.Name = "_recipientTextBox";
            this._recipientTextBox.Size = new System.Drawing.Size(550, 20);
            this._recipientTextBox.TabIndex = 1;
            // 
            // _attachmentsButton
            // 
            this._attachmentsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._attachmentsButton.Location = new System.Drawing.Point(510, 62);
            this._attachmentsButton.Name = "_attachmentsButton";
            this._attachmentsButton.Size = new System.Drawing.Size(126, 23);
            this._attachmentsButton.TabIndex = 9;
            this._attachmentsButton.Text = "Attachments (0) ...";
            this._attachmentsButton.UseVisualStyleBackColor = true;
            this._attachmentsButton.Click += new System.EventHandler(this.HandleAttachmentsButton_Click);
            // 
            // _advancedButton
            // 
            this._advancedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._advancedButton.Location = new System.Drawing.Point(16, 361);
            this._advancedButton.Name = "_advancedButton";
            this._advancedButton.Size = new System.Drawing.Size(106, 23);
            this._advancedButton.TabIndex = 12;
            this._advancedButton.Text = "Advanced...";
            this._advancedButton.UseVisualStyleBackColor = true;
            this._advancedButton.Click += new System.EventHandler(this.HandleAdvancedButton_Click);
            // 
            // _bodyPathTagsButton
            // 
            this._bodyPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._bodyPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_bodyPathTagsButton.Image")));
            this._bodyPathTagsButton.Location = new System.Drawing.Point(618, 91);
            this._bodyPathTagsButton.Name = "_bodyPathTagsButton";
            this._bodyPathTagsButton.Size = new System.Drawing.Size(18, 23);
            this._bodyPathTagsButton.TabIndex = 11;
            this._bodyPathTagsButton.TextControl = this._bodyTextBox;
            this._bodyPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _subjectPathTagsButton
            // 
            this._subjectPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._subjectPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_subjectPathTagsButton.Image")));
            this._subjectPathTagsButton.Location = new System.Drawing.Point(486, 62);
            this._subjectPathTagsButton.Name = "_subjectPathTagsButton";
            this._subjectPathTagsButton.Size = new System.Drawing.Size(18, 23);
            this._subjectPathTagsButton.TabIndex = 8;
            this._subjectPathTagsButton.TextControl = this._subjectTextBox;
            this._subjectPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _recipientPathTagsButton
            // 
            this._recipientPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._recipientPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_recipientPathTagsButton.Image")));
            this._recipientPathTagsButton.Location = new System.Drawing.Point(618, 10);
            this._recipientPathTagsButton.Name = "_recipientPathTagsButton";
            this._recipientPathTagsButton.Size = new System.Drawing.Size(18, 23);
            this._recipientPathTagsButton.TabIndex = 2;
            this._recipientPathTagsButton.TextControl = this._recipientTextBox;
            this._recipientPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _carbonCopyPathTagsButton
            // 
            this._carbonCopyPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._carbonCopyPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_carbonCopyPathTagsButton.Image")));
            this._carbonCopyPathTagsButton.Location = new System.Drawing.Point(618, 36);
            this._carbonCopyPathTagsButton.Name = "_carbonCopyPathTagsButton";
            this._carbonCopyPathTagsButton.Size = new System.Drawing.Size(18, 23);
            this._carbonCopyPathTagsButton.TabIndex = 5;
            this._carbonCopyPathTagsButton.TextControl = this._carbonCopyRecipient;
            this._carbonCopyPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _resetErrorSettingsButton
            // 
            this._resetErrorSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._resetErrorSettingsButton.Location = new System.Drawing.Point(128, 361);
            this._resetErrorSettingsButton.Name = "_resetErrorSettingsButton";
            this._resetErrorSettingsButton.Size = new System.Drawing.Size(151, 23);
            this._resetErrorSettingsButton.TabIndex = 13;
            this._resetErrorSettingsButton.Text = "Reset to default settings";
            this._resetErrorSettingsButton.UseVisualStyleBackColor = true;
            this._resetErrorSettingsButton.Visible = false;
            this._resetErrorSettingsButton.Click += new System.EventHandler(this.HandleResetErrorSettingsButton_Click);
            // 
            // SendEmailTaskSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(648, 396);
            this.Controls.Add(this._resetErrorSettingsButton);
            this.Controls.Add(this._carbonCopyPathTagsButton);
            this.Controls.Add(this._recipientPathTagsButton);
            this.Controls.Add(this._advancedButton);
            this.Controls.Add(this._bodyPathTagsButton);
            this.Controls.Add(this._subjectPathTagsButton);
            this.Controls.Add(this._attachmentsButton);
            this.Controls.Add(this._recipientTextBox);
            this.Controls.Add(this._carbonCopyRecipient);
            this.Controls.Add(this._subjectTextBox);
            this.Controls.Add(this._bodyTextBox);
            this.Controls.Add(label3);
            this.Controls.Add(label2);
            this.Controls.Add(label1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 250);
            this.Name = "SendEmailTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Core: Send email settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.TextBox _bodyTextBox;
        private System.Windows.Forms.TextBox _subjectTextBox;
        private System.Windows.Forms.TextBox _carbonCopyRecipient;
        private System.Windows.Forms.TextBox _recipientTextBox;
        private System.Windows.Forms.Button _attachmentsButton;
        private Utilities.Forms.PathTagsButton _subjectPathTagsButton;
        private Utilities.Forms.PathTagsButton _bodyPathTagsButton;
        private System.Windows.Forms.Button _advancedButton;
        private Utilities.Forms.PathTagsButton _recipientPathTagsButton;
        private Utilities.Forms.PathTagsButton _carbonCopyPathTagsButton;
        private System.Windows.Forms.Button _resetErrorSettingsButton;
    }
}