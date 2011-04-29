namespace Extract.FileActionManager.Forms
{
    partial class FtpConnectionSettingsControl
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
            EnterpriseDT.Net.Ftp.Forms.FTPConnectionProperties ftpConnectionProperties1 = new EnterpriseDT.Net.Ftp.Forms.FTPConnectionProperties();
            this._numberConnections = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._connectionsLabel = new System.Windows.Forms.Label();
            this._ftpConnectionEditor = new EnterpriseDT.Net.Ftp.Forms.FTPConnectionEditor();
            this._secureFTPConnection = new EnterpriseDT.Net.Ftp.SecureFTPConnection(this.components);
            this._testConnectionButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._numberConnections)).BeginInit();
            this.SuspendLayout();
            // 
            // _numberConnections
            // 
            this._numberConnections.Location = new System.Drawing.Point(315, 251);
            this._numberConnections.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this._numberConnections.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._numberConnections.Name = "_numberConnections";
            this._numberConnections.Size = new System.Drawing.Size(36, 20);
            this._numberConnections.TabIndex = 4;
            this._numberConnections.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._numberConnections.UserTextCorrected += new System.EventHandler<System.EventArgs>(this.HandleNumberConnectionsNumericUpDownUserTextCorrected);
            // 
            // _connectionsLabel
            // 
            this._connectionsLabel.AutoSize = true;
            this._connectionsLabel.Location = new System.Drawing.Point(3, 255);
            this._connectionsLabel.Name = "_connectionsLabel";
            this._connectionsLabel.Size = new System.Drawing.Size(288, 13);
            this._connectionsLabel.TabIndex = 3;
            this._connectionsLabel.Text = "Maximum number of simultaneous connections to the server";
            // 
            // _ftpConnectionEditor
            // 
            this._ftpConnectionEditor.Connection = this._secureFTPConnection;
            this._ftpConnectionEditor.Dock = System.Windows.Forms.DockStyle.Top;
            this._ftpConnectionEditor.HelpBackColor = System.Drawing.SystemColors.Control;
            this._ftpConnectionEditor.HelpForeColor = System.Drawing.SystemColors.ControlText;
            this._ftpConnectionEditor.LineColor = System.Drawing.SystemColors.ScrollBar;
            this._ftpConnectionEditor.Location = new System.Drawing.Point(3, 3);
            this._ftpConnectionEditor.Name = "_ftpConnectionEditor";
            this._ftpConnectionEditor.Properties = ftpConnectionProperties1;
            this._ftpConnectionEditor.Size = new System.Drawing.Size(354, 215);
            this._ftpConnectionEditor.TabIndex = 0;
            this._ftpConnectionEditor.ViewBackColor = System.Drawing.SystemColors.Window;
            this._ftpConnectionEditor.ViewForeColor = System.Drawing.SystemColors.WindowText;
            this._ftpConnectionEditor.Properties = new EnterpriseDT.Net.Ftp.Forms.FTPConnectionProperties(false);
            this._ftpConnectionEditor.Properties.AddCategory("Connection", "Connection", true);
            this._ftpConnectionEditor.Properties.AddProperty("Connection", "Protocol", "Protocol", "File transfer protocol to use.", true, 0);
            this._ftpConnectionEditor.Properties.AddProperty("Connection", "ServerAddress", "Server Address", "The domain-name or IP address of the FTP server.", true, 1);
            this._ftpConnectionEditor.Properties.AddProperty("Connection", "ServerPort", "Server Port", "Port on the server to which to connect the control-channel.", true, 2);
            this._ftpConnectionEditor.Properties.AddProperty("Connection", "UserName", "User Name", "User-name of account on the server.", true, 3);
            this._ftpConnectionEditor.Properties.AddProperty("Connection", "Password", "Password", "Password of account on the server.", true, 4);
            // 
            // _secureFTPConnection
            // 
            this._secureFTPConnection.ClientPrivateKeyBytes = null;
            this._secureFTPConnection.DefaultSyncRules.FilterCallback = null;
            this._secureFTPConnection.LicenseKey = "701-9435-3077-0362";
            this._secureFTPConnection.LicenseOwner = "trialuser";
            this._secureFTPConnection.ParentControl = this;
            // 
            // _testConnectionButton
            // 
            this._testConnectionButton.Location = new System.Drawing.Point(3, 273);
            this._testConnectionButton.Name = "_testConnectionButton";
            this._testConnectionButton.Size = new System.Drawing.Size(116, 23);
            this._testConnectionButton.TabIndex = 2;
            this._testConnectionButton.Text = "Test Connection";
            this._testConnectionButton.UseVisualStyleBackColor = true;
            this._testConnectionButton.Click += new System.EventHandler(this.HandleTestConnection);
            // 
            // FtpConnectionSettingsControl
            // 
            this.Controls.Add(this._numberConnections);
            this.Controls.Add(this._connectionsLabel);
            this.Controls.Add(this._testConnectionButton);
            this.Controls.Add(this._ftpConnectionEditor);
            this.Location = new System.Drawing.Point(4, 22);
            this.MinimumSize = new System.Drawing.Size(360, 300);
            this.Name = "FtpConnectionSettingsControl";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(360, 300);
            ((System.ComponentModel.ISupportInitialize)(this._numberConnections)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        
        private EnterpriseDT.Net.Ftp.Forms.FTPConnectionEditor _ftpConnectionEditor;
        private EnterpriseDT.Net.Ftp.SecureFTPConnection _secureFTPConnection;
        private Utilities.Forms.BetterNumericUpDown _numberConnections; 
        private System.Windows.Forms.Label _connectionsLabel;
        private System.Windows.Forms.Button _testConnectionButton;
    }
}
