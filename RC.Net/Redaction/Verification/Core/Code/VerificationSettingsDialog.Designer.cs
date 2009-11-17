namespace Extract.Redaction.Verification
{
    partial class VerificationSettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="VerificationSettingsDialog"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="VerificationSettingsDialog"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources
                if (components != null)
                {
                    components.Dispose();
                }
            }

            // Release unmanaged resources

            // Call base dispose method
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.GroupBox groupBox1;
            this._feedbackSettingsButton = new System.Windows.Forms.Button();
            this._collectFeedbackCheckBox = new System.Windows.Forms.CheckBox();
            this._requireExemptionsCheckBox = new System.Windows.Forms.CheckBox();
            this._requireTypeCheckBox = new System.Windows.Forms.CheckBox();
            this._verifyAllPagesCheckBox = new System.Windows.Forms.CheckBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._dataFileControl = new Extract.Redaction.DataFileControl();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this._feedbackSettingsButton);
            groupBox1.Controls.Add(this._collectFeedbackCheckBox);
            groupBox1.Controls.Add(this._requireExemptionsCheckBox);
            groupBox1.Controls.Add(this._requireTypeCheckBox);
            groupBox1.Controls.Add(this._verifyAllPagesCheckBox);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(368, 121);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "General";
            // 
            // _feedbackSettingsButton
            // 
            this._feedbackSettingsButton.Location = new System.Drawing.Point(274, 88);
            this._feedbackSettingsButton.Name = "_feedbackSettingsButton";
            this._feedbackSettingsButton.Size = new System.Drawing.Size(75, 23);
            this._feedbackSettingsButton.TabIndex = 0;
            this._feedbackSettingsButton.Text = "Settings...";
            this._feedbackSettingsButton.UseVisualStyleBackColor = true;
            this._feedbackSettingsButton.Click += new System.EventHandler(this.HandleFeedbackSettingsButtonClick);
            // 
            // _collectFeedbackCheckBox
            // 
            this._collectFeedbackCheckBox.AutoSize = true;
            this._collectFeedbackCheckBox.Location = new System.Drawing.Point(7, 92);
            this._collectFeedbackCheckBox.Name = "_collectFeedbackCheckBox";
            this._collectFeedbackCheckBox.Size = new System.Drawing.Size(261, 17);
            this._collectFeedbackCheckBox.TabIndex = 4;
            this._collectFeedbackCheckBox.Text = "Enable collection of redaction accuracy feedback";
            this._collectFeedbackCheckBox.UseVisualStyleBackColor = true;
            this._collectFeedbackCheckBox.CheckedChanged += new System.EventHandler(this.HandleCollectFeedbackCheckBoxCheckedChanged);
            // 
            // _requireExemptionsCheckBox
            // 
            this._requireExemptionsCheckBox.AutoSize = true;
            this._requireExemptionsCheckBox.Location = new System.Drawing.Point(7, 68);
            this._requireExemptionsCheckBox.Name = "_requireExemptionsCheckBox";
            this._requireExemptionsCheckBox.Size = new System.Drawing.Size(289, 17);
            this._requireExemptionsCheckBox.TabIndex = 3;
            this._requireExemptionsCheckBox.Text = "Require users to specify exemption codes for redactions";
            this._requireExemptionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // _requireTypeCheckBox
            // 
            this._requireTypeCheckBox.AutoSize = true;
            this._requireTypeCheckBox.Location = new System.Drawing.Point(7, 44);
            this._requireTypeCheckBox.Name = "_requireTypeCheckBox";
            this._requireTypeCheckBox.Size = new System.Drawing.Size(276, 17);
            this._requireTypeCheckBox.TabIndex = 2;
            this._requireTypeCheckBox.Text = "Require users to specify redaction type for redactions";
            this._requireTypeCheckBox.UseVisualStyleBackColor = true;
            // 
            // _verifyAllPagesCheckBox
            // 
            this._verifyAllPagesCheckBox.AutoSize = true;
            this._verifyAllPagesCheckBox.Location = new System.Drawing.Point(7, 20);
            this._verifyAllPagesCheckBox.Name = "_verifyAllPagesCheckBox";
            this._verifyAllPagesCheckBox.Size = new System.Drawing.Size(198, 17);
            this._verifyAllPagesCheckBox.TabIndex = 1;
            this._verifyAllPagesCheckBox.Text = "Include all pages in redaction review";
            this._verifyAllPagesCheckBox.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(306, 214);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 3;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(225, 214);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 2;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _dataFileControl
            // 
            this._dataFileControl.Location = new System.Drawing.Point(12, 140);
            this._dataFileControl.Name = "_dataFileControl";
            this._dataFileControl.Size = new System.Drawing.Size(369, 60);
            this._dataFileControl.TabIndex = 1;
            // 
            // VerificationSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(392, 249);
            this.Controls.Add(this._dataFileControl);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VerificationSettingsDialog";
            this.ShowIcon = false;
            this.Text = "Verify redactions settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox _verifyAllPagesCheckBox;
        private System.Windows.Forms.Button _feedbackSettingsButton;
        private System.Windows.Forms.CheckBox _collectFeedbackCheckBox;
        private System.Windows.Forms.CheckBox _requireExemptionsCheckBox;
        private System.Windows.Forms.CheckBox _requireTypeCheckBox;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private DataFileControl _dataFileControl;
    }
}