namespace Extract.Utilities.ContextTags
{
    partial class CreateContextForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateContextForm));
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._contextNameTextBox = new System.Windows.Forms.TextBox();
            this._fpsFileDirTextBox = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this._fpsFileDirInfoTip = new Extract.Utilities.Forms.InfoTip();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 50);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(72, 13);
            label1.TabIndex = 6;
            label1.Text = "Context name";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(13, 93);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(56, 13);
            label2.TabIndex = 8;
            label2.Text = "FPSFileDir";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(361, 141);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 2;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(442, 141);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 3;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _contextNameTextBox
            // 
            this._contextNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._contextNameTextBox.Location = new System.Drawing.Point(12, 66);
            this._contextNameTextBox.Name = "_contextNameTextBox";
            this._contextNameTextBox.Size = new System.Drawing.Size(505, 20);
            this._contextNameTextBox.TabIndex = 0;
            // 
            // _fpsFileDirTextBox
            // 
            this._fpsFileDirTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._fpsFileDirTextBox.Location = new System.Drawing.Point(12, 109);
            this._fpsFileDirTextBox.Name = "_fpsFileDirTextBox";
            this._fpsFileDirTextBox.Size = new System.Drawing.Size(505, 20);
            this._fpsFileDirTextBox.TabIndex = 1;
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(16, 13);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ShortcutsEnabled = false;
            this.textBox1.Size = new System.Drawing.Size(501, 34);
            this.textBox1.TabIndex = 9;
            this.textBox1.TabStop = false;
            this.textBox1.Text = "There doesn\'t appear to be a context associated with the current directory. You c" +
    "an create a context for this directory now; otherwise, press cancel.";
            // 
            // _fpsFileDirInfoTip
            // 
            this._fpsFileDirInfoTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._fpsFileDirInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._fpsFileDirInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_fpsFileDirInfoTip.BackgroundImage")));
            this._fpsFileDirInfoTip.Location = new System.Drawing.Point(501, 91);
            this._fpsFileDirInfoTip.Name = "_fpsFileDirInfoTip";
            this._fpsFileDirInfoTip.Size = new System.Drawing.Size(16, 16);
            this._fpsFileDirInfoTip.TabIndex = 10;
            this._fpsFileDirInfoTip.TipText = resources.GetString("_fpsFileDirInfoTip.TipText");
            // 
            // CreateContextForm
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(529, 176);
            this.Controls.Add(this._fpsFileDirInfoTip);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this._fpsFileDirTextBox);
            this.Controls.Add(label2);
            this.Controls.Add(this._contextNameTextBox);
            this.Controls.Add(label1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 214);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(250, 214);
            this.Name = "CreateContextForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create context";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.TextBox _contextNameTextBox;
        private System.Windows.Forms.TextBox _fpsFileDirTextBox;
        private System.Windows.Forms.TextBox textBox1;
        private Forms.InfoTip _fpsFileDirInfoTip;
    }
}