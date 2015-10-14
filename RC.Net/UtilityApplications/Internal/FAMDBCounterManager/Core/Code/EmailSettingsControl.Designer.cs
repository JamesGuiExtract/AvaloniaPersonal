namespace Extract.FAMDBCounterManager
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
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.GroupBox groupBox3;
            System.Windows.Forms.GroupBox groupBox4;
            System.Windows.Forms.GroupBox groupBox5;
            this._textSenderEmail = new System.Windows.Forms.TextBox();
            this._textSenderName = new System.Windows.Forms.TextBox();
            this._textUserName = new System.Windows.Forms.TextBox();
            this._textSmtpServer = new System.Windows.Forms.TextBox();
            this._checkUseSsl = new System.Windows.Forms.CheckBox();
            this._textPassword = new System.Windows.Forms.TextBox();
            this._checkRequireAuthentication = new System.Windows.Forms.CheckBox();
            this._editableBodyTextBox = new System.Windows.Forms.TextBox();
            this._textSubjectTemplate = new System.Windows.Forms.TextBox();
            this._readOnlyBodyTextBox = new System.Windows.Forms.TextBox();
            this._textPort = new Extract.FAMDBCounterManager.NumericEntryTextBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label5 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label7 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            groupBox3 = new System.Windows.Forms.GroupBox();
            groupBox4 = new System.Windows.Forms.GroupBox();
            groupBox5 = new System.Windows.Forms.GroupBox();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(this._textSenderEmail);
            groupBox2.Controls.Add(this._textSenderName);
            groupBox2.Controls.Add(label2);
            groupBox2.Location = new System.Drawing.Point(0, 159);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(521, 76);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Sender information";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(8, 50);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(111, 13);
            label5.TabIndex = 2;
            label5.Text = "Sender email address:";
            // 
            // _textSenderEmail
            // 
            this._textSenderEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textSenderEmail.Location = new System.Drawing.Point(125, 47);
            this._textSenderEmail.Name = "_textSenderEmail";
            this._textSenderEmail.Size = new System.Drawing.Size(390, 20);
            this._textSenderEmail.TabIndex = 3;
            this._textSenderEmail.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            // 
            // _textSenderName
            // 
            this._textSenderName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textSenderName.Location = new System.Drawing.Point(125, 20);
            this._textSenderName.Name = "_textSenderName";
            this._textSenderName.Size = new System.Drawing.Size(390, 20);
            this._textSenderName.TabIndex = 1;
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
            groupBox1.Location = new System.Drawing.Point(0, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(521, 150);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Server settings";
            // 
            // _textUserName
            // 
            this._textUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textUserName.Location = new System.Drawing.Point(94, 75);
            this._textUserName.Name = "_textUserName";
            this._textUserName.Size = new System.Drawing.Size(375, 20);
            this._textUserName.TabIndex = 6;
            this._textUserName.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            // 
            // _textSmtpServer
            // 
            this._textSmtpServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textSmtpServer.Location = new System.Drawing.Point(9, 32);
            this._textSmtpServer.Name = "_textSmtpServer";
            this._textSmtpServer.Size = new System.Drawing.Size(460, 20);
            this._textSmtpServer.TabIndex = 1;
            this._textSmtpServer.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            // 
            // label7
            // 
            label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(475, 15);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(26, 13);
            label7.TabIndex = 2;
            label7.Text = "Port";
            // 
            // _checkUseSsl
            // 
            this._checkUseSsl.AutoSize = true;
            this._checkUseSsl.Location = new System.Drawing.Point(35, 127);
            this._checkUseSsl.Name = "_checkUseSsl";
            this._checkUseSsl.Size = new System.Drawing.Size(243, 17);
            this._checkUseSsl.TabIndex = 9;
            this._checkUseSsl.Text = "Use SSL (must be supported by SMTP server)";
            this._checkUseSsl.UseVisualStyleBackColor = true;
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
            // _textPassword
            // 
            this._textPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textPassword.Location = new System.Drawing.Point(94, 101);
            this._textPassword.Name = "_textPassword";
            this._textPassword.Size = new System.Drawing.Size(375, 20);
            this._textPassword.TabIndex = 8;
            this._textPassword.UseSystemPasswordChar = true;
            this._textPassword.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(32, 104);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(56, 13);
            label4.TabIndex = 7;
            label4.Text = "Password:";
            // 
            // _checkRequireAuthentication
            // 
            this._checkRequireAuthentication.AutoSize = true;
            this._checkRequireAuthentication.Location = new System.Drawing.Point(11, 58);
            this._checkRequireAuthentication.Name = "_checkRequireAuthentication";
            this._checkRequireAuthentication.Size = new System.Drawing.Size(176, 17);
            this._checkRequireAuthentication.TabIndex = 4;
            this._checkRequireAuthentication.Text = "Requires authentication to send";
            this._checkRequireAuthentication.UseVisualStyleBackColor = true;
            this._checkRequireAuthentication.CheckedChanged += new System.EventHandler(this.HandleRequireAuthenticationCheckChanged);
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
            // groupBox3
            // 
            groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox3.Controls.Add(this._editableBodyTextBox);
            groupBox3.Location = new System.Drawing.Point(0, 295);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(521, 122);
            groupBox3.TabIndex = 3;
            groupBox3.TabStop = false;
            groupBox3.Text = "Editable body template";
            // 
            // _editableBodyTextBox
            // 
            this._editableBodyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._editableBodyTextBox.Location = new System.Drawing.Point(11, 19);
            this._editableBodyTextBox.Multiline = true;
            this._editableBodyTextBox.Name = "_editableBodyTextBox";
            this._editableBodyTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._editableBodyTextBox.Size = new System.Drawing.Size(504, 97);
            this._editableBodyTextBox.TabIndex = 0;
            // 
            // groupBox4
            // 
            groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox4.Controls.Add(this._textSubjectTemplate);
            groupBox4.Location = new System.Drawing.Point(0, 241);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new System.Drawing.Size(521, 48);
            groupBox4.TabIndex = 2;
            groupBox4.TabStop = false;
            groupBox4.Text = "Subject template";
            // 
            // _textSubjectTemplate
            // 
            this._textSubjectTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textSubjectTemplate.Location = new System.Drawing.Point(11, 19);
            this._textSubjectTemplate.Name = "_textSubjectTemplate";
            this._textSubjectTemplate.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._textSubjectTemplate.Size = new System.Drawing.Size(504, 20);
            this._textSubjectTemplate.TabIndex = 0;
            // 
            // groupBox5
            // 
            groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox5.Controls.Add(this._readOnlyBodyTextBox);
            groupBox5.Location = new System.Drawing.Point(0, 417);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new System.Drawing.Size(521, 154);
            groupBox5.TabIndex = 4;
            groupBox5.TabStop = false;
            groupBox5.Text = "Read-only body template";
            // 
            // _readOnlyBodyTextBox
            // 
            this._readOnlyBodyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._readOnlyBodyTextBox.Location = new System.Drawing.Point(11, 19);
            this._readOnlyBodyTextBox.Multiline = true;
            this._readOnlyBodyTextBox.Name = "_readOnlyBodyTextBox";
            this._readOnlyBodyTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._readOnlyBodyTextBox.Size = new System.Drawing.Size(504, 129);
            this._readOnlyBodyTextBox.TabIndex = 0;
            // 
            // _textPort
            // 
            this._textPort.AllowNegative = false;
            this._textPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._textPort.Location = new System.Drawing.Point(475, 32);
            this._textPort.MaximumValue = 1.7976931348623157E+308D;
            this._textPort.MinimumValue = -1.7976931348623157E+308D;
            this._textPort.Name = "_textPort";
            this._textPort.Size = new System.Drawing.Size(40, 20);
            this._textPort.TabIndex = 3;
            this._textPort.TextChanged += new System.EventHandler(this.HandleTextBoxTextChanged);
            // 
            // EmailSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(groupBox5);
            this.Controls.Add(groupBox4);
            this.Controls.Add(groupBox3);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.Name = "EmailSettingsControl";
            this.Size = new System.Drawing.Size(521, 574);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox _textSenderEmail;
        private System.Windows.Forms.TextBox _textSenderName;
        private NumericEntryTextBox _textPort;
        private System.Windows.Forms.TextBox _textUserName;
        private System.Windows.Forms.TextBox _textSmtpServer;
        private System.Windows.Forms.CheckBox _checkUseSsl;
        private System.Windows.Forms.TextBox _textPassword;
        private System.Windows.Forms.CheckBox _checkRequireAuthentication;
        private System.Windows.Forms.TextBox _editableBodyTextBox;
        private System.Windows.Forms.TextBox _textSubjectTemplate;
        private System.Windows.Forms.TextBox _readOnlyBodyTextBox;
    }
}
