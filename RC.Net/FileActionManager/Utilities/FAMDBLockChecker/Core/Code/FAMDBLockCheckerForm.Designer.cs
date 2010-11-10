namespace Extract.FileActionManager.Utilities
{
    partial class FAMDBLockCheckerForm
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
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
                if (_endThread != null)
                {
                    _endThread.Set();
                    _threadEnded.WaitOne(60000);

                    _endThread.Dispose();
                    _endThread = null;
                }
                if (_threadEnded != null)
                {
                    _threadEnded.Dispose();
                    _threadEnded = null;
                }
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
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label7;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FAMDBLockCheckerForm));
            this._textServerName = new System.Windows.Forms.TextBox();
            this._textDatabaseName = new System.Windows.Forms.TextBox();
            this._numericTextMilliseconds = new Extract.Utilities.Forms.NumericEntryTextBox();
            this._checkRun = new System.Windows.Forms.CheckBox();
            this._textNumberChecks = new System.Windows.Forms.TextBox();
            this._textLocksSeen = new System.Windows.Forms.TextBox();
            this._textPercentage = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(38, 13);
            label1.TabIndex = 0;
            label1.Text = "Server";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(257, 13);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(53, 13);
            label2.TabIndex = 2;
            label2.Text = "Database";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(13, 59);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(174, 13);
            label3.TabIndex = 4;
            label3.Text = "Check lock on average once every";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(273, 59);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(63, 13);
            label4.TabIndex = 6;
            label4.Text = "milliseconds";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(13, 101);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(53, 13);
            label5.TabIndex = 8;
            label5.Text = "# Checks";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(130, 102);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(72, 13);
            label6.TabIndex = 10;
            label6.Text = "# Locks seen";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(252, 101);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(15, 13);
            label7.TabIndex = 14;
            label7.Text = "%";
            // 
            // _textServerName
            // 
            this._textServerName.Location = new System.Drawing.Point(16, 30);
            this._textServerName.Name = "_textServerName";
            this._textServerName.Size = new System.Drawing.Size(235, 20);
            this._textServerName.TabIndex = 1;
            // 
            // _textDatabaseName
            // 
            this._textDatabaseName.Location = new System.Drawing.Point(260, 30);
            this._textDatabaseName.Name = "_textDatabaseName";
            this._textDatabaseName.Size = new System.Drawing.Size(235, 20);
            this._textDatabaseName.TabIndex = 3;
            // 
            // _numericTextMilliseconds
            // 
            this._numericTextMilliseconds.AllowNegative = false;
            this._numericTextMilliseconds.DisplayExceptions = false;
            this._numericTextMilliseconds.Location = new System.Drawing.Point(193, 56);
            this._numericTextMilliseconds.Name = "_numericTextMilliseconds";
            this._numericTextMilliseconds.Size = new System.Drawing.Size(74, 20);
            this._numericTextMilliseconds.TabIndex = 5;
            // 
            // _checkRun
            // 
            this._checkRun.Appearance = System.Windows.Forms.Appearance.Button;
            this._checkRun.Location = new System.Drawing.Point(16, 75);
            this._checkRun.Name = "_checkRun";
            this._checkRun.Size = new System.Drawing.Size(75, 23);
            this._checkRun.TabIndex = 9;
            this._checkRun.Text = "Run";
            this._checkRun.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._checkRun.UseVisualStyleBackColor = true;
            this._checkRun.Click += new System.EventHandler(this.HandleCheckRunClicked);
            // 
            // _textNumberChecks
            // 
            this._textNumberChecks.Location = new System.Drawing.Point(16, 118);
            this._textNumberChecks.Name = "_textNumberChecks";
            this._textNumberChecks.ReadOnly = true;
            this._textNumberChecks.Size = new System.Drawing.Size(111, 20);
            this._textNumberChecks.TabIndex = 11;
            // 
            // _textLocksSeen
            // 
            this._textLocksSeen.Location = new System.Drawing.Point(133, 118);
            this._textLocksSeen.Name = "_textLocksSeen";
            this._textLocksSeen.ReadOnly = true;
            this._textLocksSeen.Size = new System.Drawing.Size(111, 20);
            this._textLocksSeen.TabIndex = 12;
            // 
            // _textPercentage
            // 
            this._textPercentage.Location = new System.Drawing.Point(251, 117);
            this._textPercentage.Name = "_textPercentage";
            this._textPercentage.ReadOnly = true;
            this._textPercentage.Size = new System.Drawing.Size(42, 20);
            this._textPercentage.TabIndex = 13;
            // 
            // FAMDBLockCheckerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(507, 154);
            this.Controls.Add(label7);
            this.Controls.Add(this._textPercentage);
            this.Controls.Add(this._textLocksSeen);
            this.Controls.Add(this._textNumberChecks);
            this.Controls.Add(label6);
            this.Controls.Add(this._checkRun);
            this.Controls.Add(label5);
            this.Controls.Add(label4);
            this.Controls.Add(this._numericTextMilliseconds);
            this.Controls.Add(label3);
            this.Controls.Add(this._textDatabaseName);
            this.Controls.Add(label2);
            this.Controls.Add(this._textServerName);
            this.Controls.Add(label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FAMDBLockCheckerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FAMDBLockChecker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _textServerName;
        private System.Windows.Forms.TextBox _textDatabaseName;
        private Extract.Utilities.Forms.NumericEntryTextBox _numericTextMilliseconds;
        private System.Windows.Forms.CheckBox _checkRun;
        private System.Windows.Forms.TextBox _textNumberChecks;
        private System.Windows.Forms.TextBox _textLocksSeen;
        private System.Windows.Forms.TextBox _textPercentage;
    }
}

