namespace Extract.FAMDBCounterManager
{
    partial class EmailSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EmailSettingsDialog));
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._testSendButton = new System.Windows.Forms.Button();
            this._emailSettingsControl = new Extract.FAMDBCounterManager.EmailSettingsControl();
            this.SuspendLayout();
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(476, 595);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 3;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.Location = new System.Drawing.Point(395, 595);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 2;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.HandleOkButtonClicked);
            // 
            // _testSendButton
            // 
            this._testSendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._testSendButton.Location = new System.Drawing.Point(294, 595);
            this._testSendButton.Name = "_testSendButton";
            this._testSendButton.Size = new System.Drawing.Size(95, 23);
            this._testSendButton.TabIndex = 1;
            this._testSendButton.Text = "Send test email";
            this._testSendButton.UseVisualStyleBackColor = true;
            this._testSendButton.Click += new System.EventHandler(this.HandleTestEmailClick);
            // 
            // _emailSettingsControl
            // 
            this._emailSettingsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailSettingsControl.Location = new System.Drawing.Point(12, 12);
            this._emailSettingsControl.MinimumSize = new System.Drawing.Size(527, 550);
            this._emailSettingsControl.Name = "_emailSettingsControl";
            this._emailSettingsControl.Size = new System.Drawing.Size(539, 574);
            this._emailSettingsControl.TabIndex = 4;
            // 
            // EmailSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(563, 630);
            this.Controls.Add(this._emailSettingsControl);
            this.Controls.Add(this._testSendButton);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonCancel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(570, 600);
            this.Name = "EmailSettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Email Settings";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _testSendButton;
        private EmailSettingsControl _emailSettingsControl;

    }
}