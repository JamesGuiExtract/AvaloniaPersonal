namespace Extract.FileActionManager.Database
{
    partial class GenerateCounterRequestForm
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
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label1;
            this._requestTextBox = new System.Windows.Forms.TextBox();
            this._reasonTextBox = new System.Windows.Forms.TextBox();
            this._phoneTextBox = new System.Windows.Forms.TextBox();
            this._emailTextBox = new System.Windows.Forms.TextBox();
            this._organizationTextBox = new System.Windows.Forms.TextBox();
            this._copyToClipboardButton = new System.Windows.Forms.Button();
            this._saveFileButton = new System.Windows.Forms.Button();
            this._sendEmailButton = new System.Windows.Forms.Button();
            this._closeButton = new System.Windows.Forms.Button();
            groupBox2 = new System.Windows.Forms.GroupBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(this._requestTextBox);
            groupBox2.Location = new System.Drawing.Point(13, 251);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(588, 220);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Request";
            // 
            // _requestTextBox
            // 
            this._requestTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._requestTextBox.Location = new System.Drawing.Point(5, 19);
            this._requestTextBox.Multiline = true;
            this._requestTextBox.Name = "_requestTextBox";
            this._requestTextBox.ReadOnly = true;
            this._requestTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._requestTextBox.Size = new System.Drawing.Size(577, 195);
            this._requestTextBox.TabIndex = 0;
            this._requestTextBox.TabStop = false;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._reasonTextBox);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(this._phoneTextBox);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(this._emailTextBox);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(this._organizationTextBox);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(589, 232);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Fill in the information for the request";
            // 
            // _reasonTextBox
            // 
            this._reasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._reasonTextBox.Location = new System.Drawing.Point(6, 156);
            this._reasonTextBox.Multiline = true;
            this._reasonTextBox.Name = "_reasonTextBox";
            this._reasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._reasonTextBox.Size = new System.Drawing.Size(577, 70);
            this._reasonTextBox.TabIndex = 7;
            this._reasonTextBox.TextChanged += new System.EventHandler(this.Handle_TextChanged);
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(3, 140);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(97, 13);
            label4.TabIndex = 6;
            label4.Text = "Reason for request";
            // 
            // _phoneTextBox
            // 
            this._phoneTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._phoneTextBox.Location = new System.Drawing.Point(6, 117);
            this._phoneTextBox.Name = "_phoneTextBox";
            this._phoneTextBox.Size = new System.Drawing.Size(577, 20);
            this._phoneTextBox.TabIndex = 5;
            this._phoneTextBox.Text = "608-821-6532";
            this._phoneTextBox.TextChanged += new System.EventHandler(this.Handle_TextChanged);
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(3, 101);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(38, 13);
            label3.TabIndex = 4;
            label3.Text = "Phone";
            // 
            // _emailTextBox
            // 
            this._emailTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailTextBox.Location = new System.Drawing.Point(6, 77);
            this._emailTextBox.Name = "_emailTextBox";
            this._emailTextBox.Size = new System.Drawing.Size(577, 20);
            this._emailTextBox.TabIndex = 3;
            this._emailTextBox.Text = "support@extractsystems.com";
            this._emailTextBox.TextChanged += new System.EventHandler(this.Handle_TextChanged);
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 60);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(235, 13);
            label2.TabIndex = 2;
            label2.Text = "Email (will be cc\'d if email delivery option is used)";
            // 
            // _organizationTextBox
            // 
            this._organizationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._organizationTextBox.Location = new System.Drawing.Point(6, 37);
            this._organizationTextBox.Name = "_organizationTextBox";
            this._organizationTextBox.Size = new System.Drawing.Size(577, 20);
            this._organizationTextBox.TabIndex = 1;
            this._organizationTextBox.Text = "Extract Systems";
            this._organizationTextBox.TextChanged += new System.EventHandler(this.Handle_TextChanged);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 21);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(66, 13);
            label1.TabIndex = 0;
            label1.Text = "Organization";
            // 
            // _copyToClipboardButton
            // 
            this._copyToClipboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._copyToClipboardButton.Location = new System.Drawing.Point(202, 477);
            this._copyToClipboardButton.Name = "_copyToClipboardButton";
            this._copyToClipboardButton.Size = new System.Drawing.Size(129, 23);
            this._copyToClipboardButton.TabIndex = 2;
            this._copyToClipboardButton.Text = "Copy to clipboard";
            this._copyToClipboardButton.UseVisualStyleBackColor = true;
            this._copyToClipboardButton.Click += new System.EventHandler(this.HandleCopyToClipboardButton_Click);
            // 
            // _saveFileButton
            // 
            this._saveFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._saveFileButton.Location = new System.Drawing.Point(472, 477);
            this._saveFileButton.Name = "_saveFileButton";
            this._saveFileButton.Size = new System.Drawing.Size(129, 23);
            this._saveFileButton.TabIndex = 4;
            this._saveFileButton.Text = "Save request to a file";
            this._saveFileButton.UseVisualStyleBackColor = true;
            this._saveFileButton.Click += new System.EventHandler(this.HandleSaveToFile_Click);
            // 
            // _sendEmailButton
            // 
            this._sendEmailButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._sendEmailButton.Location = new System.Drawing.Point(337, 477);
            this._sendEmailButton.Name = "_sendEmailButton";
            this._sendEmailButton.Size = new System.Drawing.Size(129, 23);
            this._sendEmailButton.TabIndex = 3;
            this._sendEmailButton.Text = "Send email to Extract";
            this._sendEmailButton.UseVisualStyleBackColor = true;
            this._sendEmailButton.Click += new System.EventHandler(this.HandleSendEmailButton_Click);
            // 
            // _closeButton
            // 
            this._closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._closeButton.Location = new System.Drawing.Point(472, 506);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(129, 23);
            this._closeButton.TabIndex = 5;
            this._closeButton.Text = "Close";
            this._closeButton.UseVisualStyleBackColor = true;
            // 
            // GenerateCounterRequestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._closeButton;
            this.ClientSize = new System.Drawing.Size(613, 541);
            this.Controls.Add(this._closeButton);
            this.Controls.Add(groupBox2);
            this.Controls.Add(this._copyToClipboardButton);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._saveFileButton);
            this.Controls.Add(this._sendEmailButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 500);
            this.Name = "GenerateCounterRequestForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generate Counter Update Request";
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox _requestTextBox;
        private System.Windows.Forms.Button _copyToClipboardButton;
        private System.Windows.Forms.TextBox _reasonTextBox;
        private System.Windows.Forms.TextBox _phoneTextBox;
        private System.Windows.Forms.TextBox _emailTextBox;
        private System.Windows.Forms.TextBox _organizationTextBox;
        private System.Windows.Forms.Button _saveFileButton;
        private System.Windows.Forms.Button _sendEmailButton;
        private System.Windows.Forms.Button _closeButton;
    }
}