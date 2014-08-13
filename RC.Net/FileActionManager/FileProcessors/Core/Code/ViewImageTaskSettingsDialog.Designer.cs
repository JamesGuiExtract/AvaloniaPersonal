namespace Extract.FileActionManager.FileProcessors
{
    partial class ViewImageTaskSettingsDialog
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
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._tagSettingsButton = new System.Windows.Forms.Button();
            this._allowTagsCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(161, 50);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 2;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(242, 50);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 3;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _tagSettingsButton
            // 
            this._tagSettingsButton.Location = new System.Drawing.Point(242, 12);
            this._tagSettingsButton.Name = "_tagSettingsButton";
            this._tagSettingsButton.Size = new System.Drawing.Size(75, 23);
            this._tagSettingsButton.TabIndex = 1;
            this._tagSettingsButton.Text = "Settings...";
            this._tagSettingsButton.UseVisualStyleBackColor = true;
            this._tagSettingsButton.Click += new System.EventHandler(this.HandleTagSettingsButtonClick);
            // 
            // _allowTagsCheckBox
            // 
            this._allowTagsCheckBox.AutoSize = true;
            this._allowTagsCheckBox.Location = new System.Drawing.Point(12, 16);
            this._allowTagsCheckBox.Name = "_allowTagsCheckBox";
            this._allowTagsCheckBox.Size = new System.Drawing.Size(217, 17);
            this._allowTagsCheckBox.TabIndex = 0;
            this._allowTagsCheckBox.Text = "Allow user to apply tags to the document";
            this._allowTagsCheckBox.UseVisualStyleBackColor = true;
            this._allowTagsCheckBox.CheckedChanged += new System.EventHandler(this.HandleAllowTagsCheckBox_CheckedChanged);
            // 
            // ViewImageTaskSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(329, 85);
            this.Controls.Add(this._tagSettingsButton);
            this.Controls.Add(this._allowTagsCheckBox);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ViewImageTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Core: View image settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _tagSettingsButton;
        private System.Windows.Forms.CheckBox _allowTagsCheckBox;
    }
}