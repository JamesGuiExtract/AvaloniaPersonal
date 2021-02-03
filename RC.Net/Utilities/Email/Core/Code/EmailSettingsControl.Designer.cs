namespace Extract.Utilities.Email
{
    partial class EmailSettingsControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label3;
            this._textEmailSignature = new System.Windows.Forms.TextBox();
            this._textSenderEmail = new Extract.Utilities.Forms.BetterTextBox();
            this._textSenderName = new Extract.Utilities.Forms.BetterTextBox();
            this._groupBox1 = new System.Windows.Forms.GroupBox();
            this._textPort = new Extract.Utilities.Forms.NumericEntryTextBox();
            this._textUserName = new Extract.Utilities.Forms.BetterTextBox();
            this._textSmtpServer = new Extract.Utilities.Forms.BetterTextBox();
            this._checkUseSsl = new System.Windows.Forms.CheckBox();
            this._textPassword = new Extract.Utilities.Forms.BetterTextBox();
            this._checkRequireAuthentication = new System.Windows.Forms.CheckBox();
            this._enableEmailSettingsCheckBox = new System.Windows.Forms.CheckBox();
            this._emailSettingsControlErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            groupBox2 = new System.Windows.Forms.GroupBox();
            label6 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            groupBox2.SuspendLayout();
            this._groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._emailSettingsControlErrorProvider)).BeginInit();
            this.SuspendLayout();
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
            groupBox2.Location = new System.Drawing.Point(3, 161);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(357, 202);
            groupBox2.TabIndex = 14;
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
            this._textEmailSignature.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._textEmailSignature.Size = new System.Drawing.Size(340, 108);
            this._textEmailSignature.TabIndex = 2;
            this._textEmailSignature.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
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
            this._emailSettingsControlErrorProvider.SetError(this._textSenderEmail, "\"Sender email address\"");
            this._textSenderEmail.Location = new System.Drawing.Point(125, 47);
            this._textSenderEmail.Name = "_textSenderEmail";
            this._textSenderEmail.Size = new System.Drawing.Size(226, 20);
            this._textSenderEmail.TabIndex = 1;
            this._textSenderEmail.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            this._textSenderEmail.Required = true;
            // 
            // _textSenderName
            // 
            this._textSenderName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailSettingsControlErrorProvider.SetError(this._textSenderName, "\"Sender display name\"");
            this._textSenderName.Location = new System.Drawing.Point(125, 20);
            this._textSenderName.Name = "_textSenderName";
            this._textSenderName.Size = new System.Drawing.Size(226, 20);
            this._textSenderName.TabIndex = 0;
            this._textSenderName.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            this._textSenderName.Required = true;
            // 
            // label2
            // 
            label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(8, 23);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(108, 13);
            label2.TabIndex = 0;
            label2.Text = "Sender display name:";
            // 
            // label7
            // 
            label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(311, 17);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(26, 13);
            label7.TabIndex = 8;
            label7.Text = "Port";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 18);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(145, 13);
            label1.TabIndex = 0;
            label1.Text = "Outgoing mail (SMTP) server:";
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
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(32, 78);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(61, 13);
            label3.TabIndex = 5;
            label3.Text = "User name:";
            // 
            // _groupBox1
            // 
            this._groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._groupBox1.Controls.Add(this._textPort);
            this._groupBox1.Controls.Add(this._textUserName);
            this._groupBox1.Controls.Add(this._textSmtpServer);
            this._groupBox1.Controls.Add(label7);
            this._groupBox1.Controls.Add(this._checkUseSsl);
            this._groupBox1.Controls.Add(label1);
            this._groupBox1.Controls.Add(this._textPassword);
            this._groupBox1.Controls.Add(label4);
            this._groupBox1.Controls.Add(this._checkRequireAuthentication);
            this._groupBox1.Controls.Add(label3);
            this._groupBox1.Controls.Add(this._enableEmailSettingsCheckBox);
            this._groupBox1.Location = new System.Drawing.Point(3, 3);
            this._groupBox1.Name = "_groupBox1";
            this._groupBox1.Size = new System.Drawing.Size(357, 152);
            this._groupBox1.TabIndex = 13;
            this._groupBox1.TabStop = false;
            this._groupBox1.Text = "Server settings";
            // 
            // _textPort
            // 
            this._textPort.AllowNegative = false;
            this._textPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._emailSettingsControlErrorProvider.SetError(this._textPort, "\"Port\"");
            this._textPort.Location = new System.Drawing.Point(311, 34);
            this._textPort.MaximumValue = 1.7976931348623157E+308D;
            this._textPort.MinimumValue = -1.7976931348623157E+308D;
            this._textPort.Name = "_textPort";
            this._textPort.Size = new System.Drawing.Size(40, 20);
            this._textPort.TabIndex = 2;
            this._textPort.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            // 
            // _textUserName
            // 
            this._textUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailSettingsControlErrorProvider.SetError(this._textUserName, "Authentication User name");
            this._textUserName.Location = new System.Drawing.Point(94, 77);
            this._textUserName.Name = "_textUserName";
            this._textUserName.Size = new System.Drawing.Size(184, 20);
            this._textUserName.TabIndex = 3;
            this._textUserName.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            this._textUserName.Required = true;
            // 
            // _textSmtpServer
            // 
            this._textSmtpServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailSettingsControlErrorProvider.SetError(this._textSmtpServer, "\"Outgoing mail (SMTP) server\"");
            this._textSmtpServer.Location = new System.Drawing.Point(9, 34);
            this._textSmtpServer.Name = "_textSmtpServer";
            this._textSmtpServer.Size = new System.Drawing.Size(296, 20);
            this._textSmtpServer.TabIndex = 1;
            this._textSmtpServer.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            this._textSmtpServer.Required = true;
            // 
            // _checkUseSsl
            // 
            this._checkUseSsl.AutoSize = true;
            this._checkUseSsl.Location = new System.Drawing.Point(35, 129);
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
            this._emailSettingsControlErrorProvider.SetError(this._textPassword, "Authentication Password");
            this._textPassword.Location = new System.Drawing.Point(94, 103);
            this._textPassword.Name = "_textPassword";
            this._textPassword.Size = new System.Drawing.Size(184, 20);
            this._textPassword.TabIndex = 4;
            this._textPassword.UseSystemPasswordChar = true;
            this._textPassword.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            this._textPassword.Required = true;

            // 
            // _checkRequireAuthentication
            // 
            this._checkRequireAuthentication.AutoSize = true;
            this._checkRequireAuthentication.Location = new System.Drawing.Point(11, 60);
            this._checkRequireAuthentication.Name = "_checkRequireAuthentication";
            this._checkRequireAuthentication.Size = new System.Drawing.Size(176, 17);
            this._checkRequireAuthentication.TabIndex = 2;
            this._checkRequireAuthentication.Text = "Requires authentication to send";
            this._checkRequireAuthentication.UseVisualStyleBackColor = true;
            this._checkRequireAuthentication.CheckedChanged += new System.EventHandler(this.HandleRequireAuthenticationCheckChanged);
            // 
            // _enableEmailSettingsCheckBox
            // 
            this._enableEmailSettingsCheckBox.AutoSize = true;
            this._enableEmailSettingsCheckBox.Location = new System.Drawing.Point(6, -1);
            this._enableEmailSettingsCheckBox.Name = "_enableEmailSettingsCheckBox";
            this._enableEmailSettingsCheckBox.Size = new System.Drawing.Size(114, 17);
            this._enableEmailSettingsCheckBox.TabIndex = 0;
            this._enableEmailSettingsCheckBox.Text = "Enable email alerts";
            this._enableEmailSettingsCheckBox.UseVisualStyleBackColor = true;
            this._enableEmailSettingsCheckBox.CheckStateChanged += new System.EventHandler(this.HandleEnableEmailSettingsCheckBox_CheckStateChanged);
            // 
            // _emailSettingsControlErrorProvider
            // 
            this._emailSettingsControlErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this._emailSettingsControlErrorProvider.ContainerControl = this;
            // 
            // EmailSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(groupBox2);
            this.Controls.Add(this._groupBox1);
            this.Name = "EmailSettingsControl";
            this.Size = new System.Drawing.Size(363, 363);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            this._groupBox1.ResumeLayout(false);
            this._groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._emailSettingsControlErrorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox _textEmailSignature;
        private Extract.Utilities.Forms.BetterTextBox _textSenderEmail;
        private Extract.Utilities.Forms.BetterTextBox _textSenderName;
        private Forms.NumericEntryTextBox _textPort;
        private Extract.Utilities.Forms.BetterTextBox _textUserName;
        private Extract.Utilities.Forms.BetterTextBox _textSmtpServer;
        private System.Windows.Forms.CheckBox _checkUseSsl;
        private Extract.Utilities.Forms.BetterTextBox _textPassword;
        private System.Windows.Forms.CheckBox _checkRequireAuthentication;
        private System.Windows.Forms.ErrorProvider _emailSettingsControlErrorProvider;
        private System.Windows.Forms.CheckBox _enableEmailSettingsCheckBox;
        private System.Windows.Forms.GroupBox _groupBox1;
    }
}
