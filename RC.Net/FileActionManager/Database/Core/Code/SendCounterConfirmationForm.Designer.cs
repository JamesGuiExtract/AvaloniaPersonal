namespace Extract.FileActionManager.Database
{
    partial class SendCounterConfirmationForm
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
            this._confirmationGroupBox = new System.Windows.Forms.GroupBox();
            this._confirmationTextBox = new System.Windows.Forms.TextBox();
            this._confirmationLabel = new System.Windows.Forms.Label();
            this._copyToClipboardButton = new System.Windows.Forms.Button();
            this._sendEmailButton = new System.Windows.Forms.Button();
            this._saveFileButton = new System.Windows.Forms.Button();
            this._closeButton = new System.Windows.Forms.Button();
            this._confirmationGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _confirmationGroupBox
            // 
            this._confirmationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._confirmationGroupBox.Controls.Add(this._confirmationTextBox);
            this._confirmationGroupBox.Location = new System.Drawing.Point(12, 28);
            this._confirmationGroupBox.Name = "_confirmationGroupBox";
            this._confirmationGroupBox.Size = new System.Drawing.Size(588, 303);
            this._confirmationGroupBox.TabIndex = 1;
            this._confirmationGroupBox.TabStop = false;
            this._confirmationGroupBox.Text = "Please send the following confirmation of the update to Extract Systems";
            // 
            // _confirmationTextBox
            // 
            this._confirmationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._confirmationTextBox.Location = new System.Drawing.Point(12, 19);
            this._confirmationTextBox.Multiline = true;
            this._confirmationTextBox.Name = "_confirmationTextBox";
            this._confirmationTextBox.ReadOnly = true;
            this._confirmationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._confirmationTextBox.Size = new System.Drawing.Size(570, 278);
            this._confirmationTextBox.TabIndex = 0;
            this._confirmationTextBox.TabStop = false;
            // 
            // _confirmationLabel
            // 
            this._confirmationLabel.AutoSize = true;
            this._confirmationLabel.Location = new System.Drawing.Point(12, 9);
            this._confirmationLabel.Name = "_confirmationLabel";
            this._confirmationLabel.Size = new System.Drawing.Size(223, 13);
            this._confirmationLabel.TabIndex = 0;
            this._confirmationLabel.Text = "The counter update was successfully applied!";
            // 
            // _copyToClipboardButton
            // 
            this._copyToClipboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._copyToClipboardButton.Location = new System.Drawing.Point(201, 337);
            this._copyToClipboardButton.Name = "_copyToClipboardButton";
            this._copyToClipboardButton.Size = new System.Drawing.Size(129, 23);
            this._copyToClipboardButton.TabIndex = 2;
            this._copyToClipboardButton.Text = "Copy to clipboard";
            this._copyToClipboardButton.UseVisualStyleBackColor = true;
            this._copyToClipboardButton.Click += new System.EventHandler(this.HandleCopyToClipboardButton_Click);
            // 
            // _sendEmailButton
            // 
            this._sendEmailButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._sendEmailButton.Location = new System.Drawing.Point(336, 337);
            this._sendEmailButton.Name = "_sendEmailButton";
            this._sendEmailButton.Size = new System.Drawing.Size(129, 23);
            this._sendEmailButton.TabIndex = 3;
            this._sendEmailButton.Text = "Send email to Extract";
            this._sendEmailButton.UseVisualStyleBackColor = true;
            this._sendEmailButton.Click += new System.EventHandler(this.HandleSendEmailButton_Click);
            // 
            // _saveFileButton
            // 
            this._saveFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._saveFileButton.Location = new System.Drawing.Point(471, 337);
            this._saveFileButton.Name = "_saveFileButton";
            this._saveFileButton.Size = new System.Drawing.Size(129, 23);
            this._saveFileButton.TabIndex = 4;
            this._saveFileButton.Text = "Save to a file";
            this._saveFileButton.UseVisualStyleBackColor = true;
            this._saveFileButton.Click += new System.EventHandler(this.HandleSaveToFile_Click);
            // 
            // _closeButton
            // 
            this._closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._closeButton.Location = new System.Drawing.Point(471, 366);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(129, 23);
            this._closeButton.TabIndex = 5;
            this._closeButton.Text = "Close";
            this._closeButton.UseVisualStyleBackColor = true;
            // 
            // SendCounterConfirmationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._closeButton;
            this.ClientSize = new System.Drawing.Size(612, 401);
            this.Controls.Add(this._closeButton);
            this.Controls.Add(this._copyToClipboardButton);
            this.Controls.Add(this._confirmationGroupBox);
            this.Controls.Add(this._confirmationLabel);
            this.Controls.Add(this._saveFileButton);
            this.Controls.Add(this._sendEmailButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 300);
            this.Name = "SendCounterConfirmationForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Counter Update Confirmation";
            this._confirmationGroupBox.ResumeLayout(false);
            this._confirmationGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _confirmationTextBox;
        private System.Windows.Forms.Button _sendEmailButton;
        private System.Windows.Forms.Button _copyToClipboardButton;
        private System.Windows.Forms.Button _saveFileButton;
        private System.Windows.Forms.Button _closeButton;
        private System.Windows.Forms.GroupBox _confirmationGroupBox;
        private System.Windows.Forms.Label _confirmationLabel;
    }
}