namespace Extract.Utilities.Email
{
    partial class SmtpEmailSettingsDialog
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
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonTest = new System.Windows.Forms.Button();
            this._emailSettingsControl = new Extract.Utilities.Email.EmailSettingsControl();
            this.SuspendLayout();
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(293, 372);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 2;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.Location = new System.Drawing.Point(212, 372);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 1;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.HandleOkButtonClicked);
            // 
            // _buttonTest
            // 
            this._buttonTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonTest.Location = new System.Drawing.Point(111, 372);
            this._buttonTest.Name = "_buttonTest";
            this._buttonTest.Size = new System.Drawing.Size(95, 23);
            this._buttonTest.TabIndex = 0;
            this._buttonTest.Text = "Send test email";
            this._buttonTest.UseVisualStyleBackColor = true;
            this._buttonTest.Click += new System.EventHandler(this.HandleTestEmailClick);
            // 
            // _emailSettingsControl
            // 
            this._emailSettingsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailSettingsControl.Location = new System.Drawing.Point(7, 2);
            this._emailSettingsControl.Name = "_emailSettingsControl";
            this._emailSettingsControl.Size = new System.Drawing.Size(363, 363);
            this._emailSettingsControl.TabIndex = 3;
            this._emailSettingsControl.SettingsChanged += new System.EventHandler<System.EventArgs>(this.HandleEmailSettingsControl_SettingsStateChanged);
            // 
            // SmtpEmailSettingsDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(380, 407);
            this.Controls.Add(this._emailSettingsControl);
            this.Controls.Add(this._buttonTest);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SmtpEmailSettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Email Settings";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonTest;
        private EmailSettingsControl _emailSettingsControl;

    }
}