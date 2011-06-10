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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._numberRetriesControl = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._timeBetweenRetriesControl = new Extract.Utilities.Forms.NumericEntryTextBox();
            this._showAdvancedSettingsCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this._numberConnections)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numberRetriesControl)).BeginInit();
            this.SuspendLayout();
            // 
            // _numberConnections
            // 
            this._numberConnections.Location = new System.Drawing.Point(312, 242);
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
            this._connectionsLabel.Location = new System.Drawing.Point(6, 246);
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
            this._testConnectionButton.Location = new System.Drawing.Point(6, 322);
            this._testConnectionButton.Name = "_testConnectionButton";
            this._testConnectionButton.Size = new System.Drawing.Size(116, 23);
            this._testConnectionButton.TabIndex = 2;
            this._testConnectionButton.Text = "Test Connection";
            this._testConnectionButton.UseVisualStyleBackColor = true;
            this._testConnectionButton.Click += new System.EventHandler(this.HandleTestConnection);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 271);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(186, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Number of retries if FTP operation fails";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 294);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(175, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Time between retries in milliseconds";
            // 
            // _numberRetriesControl
            // 
            this._numberRetriesControl.IntegersOnly = true;
            this._numberRetriesControl.Location = new System.Drawing.Point(275, 269);
            this._numberRetriesControl.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this._numberRetriesControl.Name = "_numberRetriesControl";
            this._numberRetriesControl.Size = new System.Drawing.Size(73, 20);
            this._numberRetriesControl.TabIndex = 7;
            // 
            // _timeBetweenRetriesControl
            // 
            this._timeBetweenRetriesControl.Location = new System.Drawing.Point(275, 296);
            this._timeBetweenRetriesControl.MaximumValue = 1.7976931348623157E+308D;
            this._timeBetweenRetriesControl.MinimumValue = -1.7976931348623157E+308D;
            this._timeBetweenRetriesControl.Name = "_timeBetweenRetriesControl";
            this._timeBetweenRetriesControl.Size = new System.Drawing.Size(73, 20);
            this._timeBetweenRetriesControl.TabIndex = 8;
            // 
            // _showAdvancedSettingsCheckBox
            // 
            this._showAdvancedSettingsCheckBox.AutoSize = true;
            this._showAdvancedSettingsCheckBox.Location = new System.Drawing.Point(6, 224);
            this._showAdvancedSettingsCheckBox.Name = "_showAdvancedSettingsCheckBox";
            this._showAdvancedSettingsCheckBox.Size = new System.Drawing.Size(199, 17);
            this._showAdvancedSettingsCheckBox.TabIndex = 9;
            this._showAdvancedSettingsCheckBox.Text = "Show advanced connection settings";
            this._showAdvancedSettingsCheckBox.UseVisualStyleBackColor = true;
            this._showAdvancedSettingsCheckBox.Click += new System.EventHandler(this.HandleAdvanced);
            // 
            // FtpConnectionSettingsControl
            // 
            this.Controls.Add(this._showAdvancedSettingsCheckBox);
            this.Controls.Add(this._timeBetweenRetriesControl);
            this.Controls.Add(this._numberRetriesControl);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._numberConnections);
            this.Controls.Add(this._connectionsLabel);
            this.Controls.Add(this._testConnectionButton);
            this.Controls.Add(this._ftpConnectionEditor);
            this.Location = new System.Drawing.Point(4, 22);
            this.MinimumSize = new System.Drawing.Size(360, 300);
            this.Name = "FtpConnectionSettingsControl";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(360, 363);
            ((System.ComponentModel.ISupportInitialize)(this._numberConnections)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numberRetriesControl)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        
        private EnterpriseDT.Net.Ftp.Forms.FTPConnectionEditor _ftpConnectionEditor;
        private EnterpriseDT.Net.Ftp.SecureFTPConnection _secureFTPConnection;
        private Utilities.Forms.BetterNumericUpDown _numberConnections; 
        private System.Windows.Forms.Label _connectionsLabel;
        private System.Windows.Forms.Button _testConnectionButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private Utilities.Forms.BetterNumericUpDown _numberRetriesControl;
        private Utilities.Forms.NumericEntryTextBox _timeBetweenRetriesControl;
        private System.Windows.Forms.CheckBox _showAdvancedSettingsCheckBox;
    }
}
