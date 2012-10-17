namespace Extract.NetDMSCustomComponents
{
    partial class NetDMSConnectionSettingsControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            this._passwordTextBox = new System.Windows.Forms.TextBox();
            this._serverTextBox = new System.Windows.Forms.TextBox();
            this._portTextBox = new System.Windows.Forms.TextBox();
            this._userTextBox = new System.Windows.Forms.TextBox();
            this._testConnectionButton = new System.Windows.Forms.Button();
            this._loadLastConnectionButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(-3, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(38, 13);
            label1.TabIndex = 0;
            label1.Text = "Server";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(-3, 128);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(53, 13);
            label5.TabIndex = 6;
            label5.Text = "Password";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(-3, 44);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(26, 13);
            label2.TabIndex = 2;
            label2.Text = "Port";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(-3, 87);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(29, 13);
            label3.TabIndex = 4;
            label3.Text = "User";
            // 
            // _passwordTextBox
            // 
            this._passwordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._passwordTextBox.Location = new System.Drawing.Point(0, 146);
            this._passwordTextBox.Name = "_passwordTextBox";
            this._passwordTextBox.Size = new System.Drawing.Size(430, 20);
            this._passwordTextBox.TabIndex = 7;
            this._passwordTextBox.UseSystemPasswordChar = true;
            this._passwordTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HandlePasswordTextBox_KeyDown);
            this._passwordTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HandlePasswordTextBox_KeyPress);
            this._passwordTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.HandlePasswordTextBox_KeyUp);
            // 
            // _serverTextBox
            // 
            this._serverTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._serverTextBox.Location = new System.Drawing.Point(0, 17);
            this._serverTextBox.Name = "_serverTextBox";
            this._serverTextBox.Size = new System.Drawing.Size(430, 20);
            this._serverTextBox.TabIndex = 1;
            // 
            // _portTextBox
            // 
            this._portTextBox.Location = new System.Drawing.Point(0, 61);
            this._portTextBox.MaxLength = 5;
            this._portTextBox.Name = "_portTextBox";
            this._portTextBox.Size = new System.Drawing.Size(59, 20);
            this._portTextBox.TabIndex = 3;
            this._portTextBox.TextChanged += new System.EventHandler(this.HandlePortTextChanged);
            // 
            // _userTextBox
            // 
            this._userTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._userTextBox.Location = new System.Drawing.Point(0, 105);
            this._userTextBox.Name = "_userTextBox";
            this._userTextBox.Size = new System.Drawing.Size(430, 20);
            this._userTextBox.TabIndex = 5;
            // 
            // _testConnectionButton
            // 
            this._testConnectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._testConnectionButton.Location = new System.Drawing.Point(257, 172);
            this._testConnectionButton.Name = "_testConnectionButton";
            this._testConnectionButton.Size = new System.Drawing.Size(173, 23);
            this._testConnectionButton.TabIndex = 17;
            this._testConnectionButton.Text = "Test Connection";
            this._testConnectionButton.UseVisualStyleBackColor = true;
            this._testConnectionButton.Click += new System.EventHandler(this.HandleTestConnectionButton_Click);
            // 
            // _loadLastConnectionButton
            // 
            this._loadLastConnectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._loadLastConnectionButton.Location = new System.Drawing.Point(78, 172);
            this._loadLastConnectionButton.Name = "_loadLastConnectionButton";
            this._loadLastConnectionButton.Size = new System.Drawing.Size(173, 23);
            this._loadLastConnectionButton.TabIndex = 16;
            this._loadLastConnectionButton.Text = "Load last successful connection";
            this._loadLastConnectionButton.UseVisualStyleBackColor = true;
            this._loadLastConnectionButton.Click += new System.EventHandler(this.HandleLoadLastConnectionButton_Click);
            // 
            // NetDMSConnectionSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._loadLastConnectionButton);
            this.Controls.Add(this._testConnectionButton);
            this.Controls.Add(label1);
            this.Controls.Add(this._passwordTextBox);
            this.Controls.Add(this._serverTextBox);
            this.Controls.Add(label5);
            this.Controls.Add(label2);
            this.Controls.Add(this._portTextBox);
            this.Controls.Add(label3);
            this.Controls.Add(this._userTextBox);
            this.Name = "NetDMSConnectionSettingsControl";
            this.Size = new System.Drawing.Size(430, 195);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _passwordTextBox;
        private System.Windows.Forms.TextBox _serverTextBox;
        private System.Windows.Forms.TextBox _portTextBox;
        private System.Windows.Forms.TextBox _userTextBox;
        private System.Windows.Forms.Button _testConnectionButton;
        private System.Windows.Forms.Button _loadLastConnectionButton;
    }
}
