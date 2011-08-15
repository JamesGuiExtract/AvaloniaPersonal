namespace Extract.FileActionManager.FileProcessors
{
    partial class EncryptDecryptFileTaskSettingsDialog
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.GroupBox groupBox1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EncryptDecryptFileTaskSettingsDialog));
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Panel panel1;
            this._checkOverwriteDestination = new System.Windows.Forms.CheckBox();
            this._textInputFile = new System.Windows.Forms.TextBox();
            this._pathTagsDestination = new Extract.Utilities.Forms.PathTagsButton();
            this._textDestination = new System.Windows.Forms.TextBox();
            this._browseInput = new Extract.Utilities.Forms.BrowseButton();
            this._browseDestination = new Extract.Utilities.Forms.BrowseButton();
            this._pathTagsInput = new Extract.Utilities.Forms.PathTagsButton();
            this._textPassword = new System.Windows.Forms.TextBox();
            this._textPasswordConfirm = new System.Windows.Forms.TextBox();
            this._labelEnableDecrypt = new System.Windows.Forms.Label();
            this._labelEnableEncrypt = new System.Windows.Forms.Label();
            this._radioDecrypt = new System.Windows.Forms.RadioButton();
            this._radioEncrypt = new System.Windows.Forms.RadioButton();
            this._labelPasswordError = new System.Windows.Forms.Label();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._errorPassword = new System.Windows.Forms.ErrorProvider(this.components);
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorPassword)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 16);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(47, 13);
            label1.TabIndex = 0;
            label1.Text = "Input file";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(6, 61);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(76, 13);
            label2.TabIndex = 4;
            label2.Text = "Destination file";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(this._checkOverwriteDestination);
            groupBox1.Controls.Add(this._textInputFile);
            groupBox1.Controls.Add(this._pathTagsDestination);
            groupBox1.Controls.Add(this._browseInput);
            groupBox1.Controls.Add(this._browseDestination);
            groupBox1.Controls.Add(this._pathTagsInput);
            groupBox1.Controls.Add(this._textDestination);
            groupBox1.Controls.Add(label2);
            groupBox1.Location = new System.Drawing.Point(13, 13);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(550, 126);
            groupBox1.TabIndex = 9;
            groupBox1.TabStop = false;
            groupBox1.Text = "File settings";
            // 
            // _checkOverwriteDestination
            // 
            this._checkOverwriteDestination.AutoSize = true;
            this._checkOverwriteDestination.Location = new System.Drawing.Point(9, 103);
            this._checkOverwriteDestination.Name = "_checkOverwriteDestination";
            this._checkOverwriteDestination.Size = new System.Drawing.Size(186, 17);
            this._checkOverwriteDestination.TabIndex = 6;
            this._checkOverwriteDestination.Text = "Overwrite destination file if it exists";
            this._checkOverwriteDestination.UseVisualStyleBackColor = true;
            // 
            // _textInputFile
            // 
            this._textInputFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textInputFile.Location = new System.Drawing.Point(9, 33);
            this._textInputFile.Name = "_textInputFile";
            this._textInputFile.Size = new System.Drawing.Size(478, 20);
            this._textInputFile.TabIndex = 0;
            this._textInputFile.TextChanged += new System.EventHandler(this.HandleInputAndDestinationTextChanged);
            // 
            // _pathTagsDestination
            // 
            this._pathTagsDestination.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pathTagsDestination.Image = ((System.Drawing.Image)(resources.GetObject("_pathTagsDestination.Image")));
            this._pathTagsDestination.Location = new System.Drawing.Point(493, 77);
            this._pathTagsDestination.Name = "_pathTagsDestination";
            this._pathTagsDestination.Size = new System.Drawing.Size(18, 20);
            this._pathTagsDestination.TabIndex = 4;
            this._pathTagsDestination.TextControl = this._textDestination;
            this._pathTagsDestination.UseVisualStyleBackColor = true;
            // 
            // _textDestination
            // 
            this._textDestination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textDestination.Location = new System.Drawing.Point(9, 77);
            this._textDestination.Name = "_textDestination";
            this._textDestination.Size = new System.Drawing.Size(478, 20);
            this._textDestination.TabIndex = 3;
            this._textDestination.TextChanged += new System.EventHandler(this.HandleInputAndDestinationTextChanged);
            // 
            // _browseInput
            // 
            this._browseInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._browseInput.Location = new System.Drawing.Point(517, 33);
            this._browseInput.Name = "_browseInput";
            this._browseInput.Size = new System.Drawing.Size(27, 20);
            this._browseInput.TabIndex = 2;
            this._browseInput.Text = "...";
            this._browseInput.TextControl = this._textInputFile;
            this._browseInput.UseVisualStyleBackColor = true;
            // 
            // _browseDestination
            // 
            this._browseDestination.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._browseDestination.Location = new System.Drawing.Point(517, 77);
            this._browseDestination.Name = "_browseDestination";
            this._browseDestination.Size = new System.Drawing.Size(27, 20);
            this._browseDestination.TabIndex = 5;
            this._browseDestination.Text = "...";
            this._browseDestination.TextControl = this._textDestination;
            this._browseDestination.UseVisualStyleBackColor = true;
            // 
            // _pathTagsInput
            // 
            this._pathTagsInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pathTagsInput.Image = ((System.Drawing.Image)(resources.GetObject("_pathTagsInput.Image")));
            this._pathTagsInput.Location = new System.Drawing.Point(493, 33);
            this._pathTagsInput.Name = "_pathTagsInput";
            this._pathTagsInput.Size = new System.Drawing.Size(18, 20);
            this._pathTagsInput.TabIndex = 1;
            this._pathTagsInput.TextControl = this._textInputFile;
            this._pathTagsInput.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(tableLayoutPanel1);
            groupBox2.Location = new System.Drawing.Point(13, 146);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(550, 127);
            groupBox2.TabIndex = 10;
            groupBox2.TabStop = false;
            groupBox2.Text = "Encryption/decryption settings";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(label3, 0, 0);
            tableLayoutPanel1.Controls.Add(label4, 1, 0);
            tableLayoutPanel1.Controls.Add(this._textPassword, 0, 1);
            tableLayoutPanel1.Controls.Add(this._textPasswordConfirm, 1, 1);
            tableLayoutPanel1.Controls.Add(panel1, 0, 2);
            tableLayoutPanel1.Controls.Add(this._labelPasswordError, 1, 2);
            tableLayoutPanel1.Location = new System.Drawing.Point(9, 17);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(1);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(535, 103);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(3, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(53, 13);
            label3.TabIndex = 0;
            label3.Text = "Password";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(270, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(90, 13);
            label4.TabIndex = 1;
            label4.Text = "Confirm password";
            // 
            // _textPassword
            // 
            this._textPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textPassword.Location = new System.Drawing.Point(3, 16);
            this._textPassword.Name = "_textPassword";
            this._textPassword.Size = new System.Drawing.Size(261, 20);
            this._textPassword.TabIndex = 0;
            this._textPassword.UseSystemPasswordChar = true;
            // 
            // _textPasswordConfirm
            // 
            this._textPasswordConfirm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textPasswordConfirm.Location = new System.Drawing.Point(270, 16);
            this._textPasswordConfirm.Name = "_textPasswordConfirm";
            this._textPasswordConfirm.Size = new System.Drawing.Size(262, 20);
            this._textPasswordConfirm.TabIndex = 1;
            this._textPasswordConfirm.UseSystemPasswordChar = true;
            // 
            // panel1
            // 
            panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            panel1.Controls.Add(this._labelEnableDecrypt);
            panel1.Controls.Add(this._labelEnableEncrypt);
            panel1.Controls.Add(this._radioDecrypt);
            panel1.Controls.Add(this._radioEncrypt);
            panel1.Location = new System.Drawing.Point(3, 42);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(261, 55);
            panel1.TabIndex = 4;
            // 
            // _labelEnableDecrypt
            // 
            this._labelEnableDecrypt.AutoSize = true;
            this._labelEnableDecrypt.Location = new System.Drawing.Point(88, 33);
            this._labelEnableDecrypt.Name = "_labelEnableDecrypt";
            this._labelEnableDecrypt.Size = new System.Drawing.Size(148, 13);
            this._labelEnableDecrypt.TabIndex = 3;
            this._labelEnableDecrypt.Text = "To enable, change password.";
            this._labelEnableDecrypt.Visible = false;
            // 
            // _labelEnableEncrypt
            // 
            this._labelEnableEncrypt.AutoSize = true;
            this._labelEnableEncrypt.Location = new System.Drawing.Point(88, 9);
            this._labelEnableEncrypt.Name = "_labelEnableEncrypt";
            this._labelEnableEncrypt.Size = new System.Drawing.Size(148, 13);
            this._labelEnableEncrypt.TabIndex = 2;
            this._labelEnableEncrypt.Text = "To enable, change password.";
            this._labelEnableEncrypt.Visible = false;
            // 
            // _radioDecrypt
            // 
            this._radioDecrypt.AutoSize = true;
            this._radioDecrypt.Location = new System.Drawing.Point(4, 31);
            this._radioDecrypt.Name = "_radioDecrypt";
            this._radioDecrypt.Size = new System.Drawing.Size(78, 17);
            this._radioDecrypt.TabIndex = 1;
            this._radioDecrypt.TabStop = true;
            this._radioDecrypt.Text = "Decrypt file";
            this._radioDecrypt.UseVisualStyleBackColor = true;
            // 
            // _radioEncrypt
            // 
            this._radioEncrypt.AutoSize = true;
            this._radioEncrypt.Location = new System.Drawing.Point(4, 7);
            this._radioEncrypt.Name = "_radioEncrypt";
            this._radioEncrypt.Size = new System.Drawing.Size(77, 17);
            this._radioEncrypt.TabIndex = 0;
            this._radioEncrypt.TabStop = true;
            this._radioEncrypt.Text = "Encrypt file";
            this._radioEncrypt.UseVisualStyleBackColor = true;
            // 
            // _labelPasswordError
            // 
            this._labelPasswordError.AutoSize = true;
            this._labelPasswordError.Location = new System.Drawing.Point(270, 39);
            this._labelPasswordError.Name = "_labelPasswordError";
            this._labelPasswordError.Size = new System.Drawing.Size(0, 13);
            this._labelPasswordError.TabIndex = 5;
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(488, 279);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 1;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.Location = new System.Drawing.Point(407, 279);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.HandleOkButtonClicked);
            // 
            // _errorPassword
            // 
            this._errorPassword.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this._errorPassword.ContainerControl = this;
            // 
            // EncryptDecryptFileTaskSettingsDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(575, 314);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EncryptDecryptFileTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Core: Encrypt/Decrypt File";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorPassword)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox _textInputFile;
        private Utilities.Forms.BrowseButton _browseInput;
        private Utilities.Forms.PathTagsButton _pathTagsInput;
        private System.Windows.Forms.TextBox _textDestination;
        private Utilities.Forms.BrowseButton _browseDestination;
        private Utilities.Forms.PathTagsButton _pathTagsDestination;
        private System.Windows.Forms.CheckBox _checkOverwriteDestination;
        private System.Windows.Forms.TextBox _textPassword;
        private System.Windows.Forms.TextBox _textPasswordConfirm;
        private System.Windows.Forms.RadioButton _radioDecrypt;
        private System.Windows.Forms.RadioButton _radioEncrypt;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.ErrorProvider _errorPassword;
        private System.Windows.Forms.Label _labelPasswordError;
        private System.Windows.Forms.Label _labelEnableDecrypt;
        private System.Windows.Forms.Label _labelEnableEncrypt;
    }
}