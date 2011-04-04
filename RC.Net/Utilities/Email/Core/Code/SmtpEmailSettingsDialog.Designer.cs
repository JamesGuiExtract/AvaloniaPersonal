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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label2;
            this._textPort = new Extract.Utilities.Forms.NumericEntryTextBox();
            this._textUserName = new System.Windows.Forms.TextBox();
            this._textSmtpServer = new System.Windows.Forms.TextBox();
            this._checkUseSsl = new System.Windows.Forms.CheckBox();
            this._textPassword = new System.Windows.Forms.TextBox();
            this._checkRequireAuthentication = new System.Windows.Forms.CheckBox();
            this._textEmailSignature = new System.Windows.Forms.TextBox();
            this._textSenderEmail = new System.Windows.Forms.TextBox();
            this._textSenderName = new System.Windows.Forms.TextBox();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonTest = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label7 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label6 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 16);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(145, 13);
            label1.TabIndex = 0;
            label1.Text = "Outgoing mail (SMTP) server:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(32, 78);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(61, 13);
            label3.TabIndex = 5;
            label3.Text = "User name:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(32, 104);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(56, 13);
            label4.TabIndex = 6;
            label4.Text = "Password:";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._textPort);
            groupBox1.Controls.Add(this._textUserName);
            groupBox1.Controls.Add(this._textSmtpServer);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(this._checkUseSsl);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(this._textPassword);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(this._checkRequireAuthentication);
            groupBox1.Controls.Add(label3);
            groupBox1.Location = new System.Drawing.Point(13, 13);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(357, 150);
            groupBox1.TabIndex = 8;
            groupBox1.TabStop = false;
            groupBox1.Text = "Server settings";
            // 
            // _textPort
            // 
            this._textPort.AllowNegative = false;
            this._textPort.Location = new System.Drawing.Point(311, 32);
            this._textPort.Name = "_textPort";
            this._textPort.Size = new System.Drawing.Size(40, 20);
            this._textPort.TabIndex = 1;
            // 
            // _textUserName
            // 
            this._textUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textUserName.Location = new System.Drawing.Point(94, 75);
            this._textUserName.Name = "_textUserName";
            this._textUserName.Size = new System.Drawing.Size(184, 20);
            this._textUserName.TabIndex = 3;
            this._textUserName.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            // 
            // _textSmtpServer
            // 
            this._textSmtpServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textSmtpServer.Location = new System.Drawing.Point(9, 32);
            this._textSmtpServer.Name = "_textSmtpServer";
            this._textSmtpServer.Size = new System.Drawing.Size(296, 20);
            this._textSmtpServer.TabIndex = 0;
            this._textSmtpServer.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(311, 15);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(26, 13);
            label7.TabIndex = 8;
            label7.Text = "Port";
            // 
            // _checkUseSsl
            // 
            this._checkUseSsl.AutoSize = true;
            this._checkUseSsl.Location = new System.Drawing.Point(35, 127);
            this._checkUseSsl.Name = "_checkUseSsl";
            this._checkUseSsl.Size = new System.Drawing.Size(243, 17);
            this._checkUseSsl.TabIndex = 5;
            this._checkUseSsl.Text = "Use SSL (must be supported by SMTP server)";
            this._checkUseSsl.UseVisualStyleBackColor = true;
            // 
            // _textPassword
            // 
            this._textPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textPassword.Location = new System.Drawing.Point(94, 101);
            this._textPassword.Name = "_textPassword";
            this._textPassword.Size = new System.Drawing.Size(184, 20);
            this._textPassword.TabIndex = 4;
            this._textPassword.UseSystemPasswordChar = true;
            this._textPassword.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            // 
            // _checkRequireAuthentication
            // 
            this._checkRequireAuthentication.AutoSize = true;
            this._checkRequireAuthentication.Location = new System.Drawing.Point(11, 58);
            this._checkRequireAuthentication.Name = "_checkRequireAuthentication";
            this._checkRequireAuthentication.Size = new System.Drawing.Size(176, 17);
            this._checkRequireAuthentication.TabIndex = 2;
            this._checkRequireAuthentication.Text = "Requires authentication to send";
            this._checkRequireAuthentication.UseVisualStyleBackColor = true;
            this._checkRequireAuthentication.CheckedChanged += new System.EventHandler(this.HandleRequireAuthenticationCheckChanged);
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(this._textEmailSignature);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(this._textSenderEmail);
            groupBox2.Controls.Add(this._textSenderName);
            groupBox2.Controls.Add(label2);
            groupBox2.Location = new System.Drawing.Point(13, 169);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(357, 202);
            groupBox2.TabIndex = 9;
            groupBox2.TabStop = false;
            groupBox2.Text = "Sender information";
            // 
            // _textEmailSignature
            // 
            this._textEmailSignature.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textEmailSignature.Location = new System.Drawing.Point(11, 86);
            this._textEmailSignature.Multiline = true;
            this._textEmailSignature.Name = "_textEmailSignature";
            this._textEmailSignature.Size = new System.Drawing.Size(340, 110);
            this._textEmailSignature.TabIndex = 2;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(8, 70);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(81, 13);
            label6.TabIndex = 5;
            label6.Text = "Email signature:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(8, 50);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(111, 13);
            label5.TabIndex = 4;
            label5.Text = "Sender email address:";
            // 
            // _textSenderEmail
            // 
            this._textSenderEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textSenderEmail.Location = new System.Drawing.Point(125, 47);
            this._textSenderEmail.Name = "_textSenderEmail";
            this._textSenderEmail.Size = new System.Drawing.Size(226, 20);
            this._textSenderEmail.TabIndex = 1;
            this._textSenderEmail.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            // 
            // _textSenderName
            // 
            this._textSenderName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textSenderName.Location = new System.Drawing.Point(125, 20);
            this._textSenderName.Name = "_textSenderName";
            this._textSenderName.Size = new System.Drawing.Size(226, 20);
            this._textSenderName.TabIndex = 0;
            this._textSenderName.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(8, 23);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(108, 13);
            label2.TabIndex = 0;
            label2.Text = "Sender display name:";
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(295, 377);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 2;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.Location = new System.Drawing.Point(214, 377);
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
            this._buttonTest.Location = new System.Drawing.Point(113, 377);
            this._buttonTest.Name = "_buttonTest";
            this._buttonTest.Size = new System.Drawing.Size(95, 23);
            this._buttonTest.TabIndex = 0;
            this._buttonTest.Text = "Send test email";
            this._buttonTest.UseVisualStyleBackColor = true;
            this._buttonTest.Click += new System.EventHandler(this.HandleTestEmailClick);
            // 
            // SmtpEmailSettingsDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(382, 412);
            this.Controls.Add(this._buttonTest);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SmtpEmailSettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Email Settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox _textSmtpServer;
        private System.Windows.Forms.CheckBox _checkRequireAuthentication;
        private System.Windows.Forms.TextBox _textUserName;
        private System.Windows.Forms.TextBox _textPassword;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonTest;
        private System.Windows.Forms.CheckBox _checkUseSsl;
        private System.Windows.Forms.TextBox _textEmailSignature;
        private System.Windows.Forms.TextBox _textSenderEmail;
        private System.Windows.Forms.TextBox _textSenderName;
        private Forms.NumericEntryTextBox _textPort;

    }
}