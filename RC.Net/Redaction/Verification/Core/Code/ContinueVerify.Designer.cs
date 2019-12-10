namespace Extract.Redaction.Verification
{
    partial class ContinueVerify
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
            this._fromBeginningRadioButton = new System.Windows.Forms.RadioButton();
            this._priorSessionRadioButton = new System.Windows.Forms.RadioButton();
            this._okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _fromBeginningRadioButton
            // 
            this._fromBeginningRadioButton.AutoSize = true;
            this._fromBeginningRadioButton.Location = new System.Drawing.Point(12, 35);
            this._fromBeginningRadioButton.Name = "_fromBeginningRadioButton";
            this._fromBeginningRadioButton.Size = new System.Drawing.Size(195, 17);
            this._fromBeginningRadioButton.TabIndex = 5;
            this._fromBeginningRadioButton.Text = "From the beginning of the document";
            this._fromBeginningRadioButton.UseVisualStyleBackColor = true;
            // 
            // _priorSessionRadioButton
            // 
            this._priorSessionRadioButton.AutoSize = true;
            this._priorSessionRadioButton.Checked = true;
            this._priorSessionRadioButton.Location = new System.Drawing.Point(12, 12);
            this._priorSessionRadioButton.Name = "_priorSessionRadioButton";
            this._priorSessionRadioButton.Size = new System.Drawing.Size(109, 17);
            this._priorSessionRadioButton.TabIndex = 4;
            this._priorSessionRadioButton.TabStop = true;
            this._priorSessionRadioButton.Text = "From prior session";
            this._priorSessionRadioButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(204, 58);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 3;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // ContinueVerify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(286, 89);
            this.ControlBox = false;
            this.Controls.Add(this._fromBeginningRadioButton);
            this.Controls.Add(this._priorSessionRadioButton);
            this.Controls.Add(this._okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ContinueVerify";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Continue verification";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton _fromBeginningRadioButton;
        private System.Windows.Forms.RadioButton _priorSessionRadioButton;
        private System.Windows.Forms.Button _okButton;
    }
}