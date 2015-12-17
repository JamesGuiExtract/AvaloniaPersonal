namespace Extract.SQLCDBEditor
{
    partial class DisplayImportErrors
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
            this.ImportErrorsTextBox = new System.Windows.Forms.TextBox();
            this.ContinueButton = new System.Windows.Forms.Button();
            this._CancelButton = new System.Windows.Forms.Button();
            this.resultTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ImportErrorsTextBox
            // 
            this.ImportErrorsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ImportErrorsTextBox.Location = new System.Drawing.Point(12, 12);
            this.ImportErrorsTextBox.Multiline = true;
            this.ImportErrorsTextBox.Name = "ImportErrorsTextBox";
            this.ImportErrorsTextBox.ReadOnly = true;
            this.ImportErrorsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ImportErrorsTextBox.Size = new System.Drawing.Size(640, 278);
            this.ImportErrorsTextBox.TabIndex = 0;
            this.ImportErrorsTextBox.TabStop = false;
            // 
            // ContinueButton
            // 
            this.ContinueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ContinueButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ContinueButton.Location = new System.Drawing.Point(496, 296);
            this.ContinueButton.Name = "ContinueButton";
            this.ContinueButton.Size = new System.Drawing.Size(75, 23);
            this.ContinueButton.TabIndex = 1;
            this.ContinueButton.Text = "Continue";
            this.ContinueButton.UseVisualStyleBackColor = true;
            // 
            // _CancelButton
            // 
            this._CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._CancelButton.Location = new System.Drawing.Point(577, 296);
            this._CancelButton.Name = "_CancelButton";
            this._CancelButton.Size = new System.Drawing.Size(75, 23);
            this._CancelButton.TabIndex = 2;
            this._CancelButton.Text = "Cancel";
            this._CancelButton.UseVisualStyleBackColor = true;
            // 
            // resultTextBox
            // 
            this.resultTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.resultTextBox.Location = new System.Drawing.Point(12, 299);
            this.resultTextBox.Name = "resultTextBox";
            this.resultTextBox.ReadOnly = true;
            this.resultTextBox.Size = new System.Drawing.Size(478, 13);
            this.resultTextBox.TabIndex = 3;
            // 
            // DisplayImportErrors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 331);
            this.Controls.Add(this.resultTextBox);
            this.Controls.Add(this._CancelButton);
            this.Controls.Add(this.ContinueButton);
            this.Controls.Add(this.ImportErrorsTextBox);
            this.MinimumSize = new System.Drawing.Size(680, 369);
            this.Name = "DisplayImportErrors";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import results contain errors";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ImportErrorsTextBox;
        private System.Windows.Forms.Button ContinueButton;
        private System.Windows.Forms.Button _CancelButton;
        private System.Windows.Forms.TextBox resultTextBox;
    }
}