namespace Extract.Demo_Pagination
{
    partial class InsurancePanel
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
            System.Windows.Forms.Label label5;
            this._insuranceProviderTextBox = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = System.Drawing.Color.White;
            label5.Location = new System.Drawing.Point(-3, 6);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(99, 13);
            label5.TabIndex = 14;
            label5.Text = "Insurance Provider:";
            // 
            // _insuranceProviderTextBox
            // 
            this._insuranceProviderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._insuranceProviderTextBox.Location = new System.Drawing.Point(102, 3);
            this._insuranceProviderTextBox.Name = "_insuranceProviderTextBox";
            this._insuranceProviderTextBox.Size = new System.Drawing.Size(253, 20);
            this._insuranceProviderTextBox.TabIndex = 15;
            // 
            // BloodTypePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Controls.Add(this._insuranceProviderTextBox);
            this.Controls.Add(label5);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "BloodTypePanel";
            this.Size = new System.Drawing.Size(355, 27);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _insuranceProviderTextBox;

    }
}
