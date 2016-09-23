namespace Extract.Demo_Pagination
{
    partial class ReferralPanel
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
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label1;
            this._referringProviderMiddleTextBox = new System.Windows.Forms.TextBox();
            this._referringProviderLastTextBox = new System.Windows.Forms.TextBox();
            this._referringProviderFirstTextBox = new System.Windows.Forms.TextBox();
            this._referringProviderAddressTextBox = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _referringProviderMiddleTextBox
            // 
            this._referringProviderMiddleTextBox.Location = new System.Drawing.Point(272, 3);
            this._referringProviderMiddleTextBox.Name = "_referringProviderMiddleTextBox";
            this._referringProviderMiddleTextBox.Size = new System.Drawing.Size(108, 20);
            this._referringProviderMiddleTextBox.TabIndex = 7;
            // 
            // _referringProviderLastTextBox
            // 
            this._referringProviderLastTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._referringProviderLastTextBox.Location = new System.Drawing.Point(386, 3);
            this._referringProviderLastTextBox.Name = "_referringProviderLastTextBox";
            this._referringProviderLastTextBox.Size = new System.Drawing.Size(146, 20);
            this._referringProviderLastTextBox.TabIndex = 8;
            // 
            // _referringProviderFirstTextBox
            // 
            this._referringProviderFirstTextBox.Location = new System.Drawing.Point(139, 3);
            this._referringProviderFirstTextBox.Name = "_referringProviderFirstTextBox";
            this._referringProviderFirstTextBox.Size = new System.Drawing.Size(127, 20);
            this._referringProviderFirstTextBox.TabIndex = 6;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = System.Drawing.Color.White;
            label2.Location = new System.Drawing.Point(-3, 6);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(126, 13);
            label2.TabIndex = 5;
            label2.Text = "Referring Provider Name:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = System.Drawing.Color.White;
            label1.Location = new System.Drawing.Point(-3, 32);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(136, 13);
            label1.TabIndex = 9;
            label1.Text = "Referring Provider Address:";
            // 
            // _referringProviderAddressTextBox
            // 
            this._referringProviderAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._referringProviderAddressTextBox.Location = new System.Drawing.Point(139, 29);
            this._referringProviderAddressTextBox.Name = "_referringProviderAddressTextBox";
            this._referringProviderAddressTextBox.Size = new System.Drawing.Size(393, 20);
            this._referringProviderAddressTextBox.TabIndex = 10;
            // 
            // ReferralPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Controls.Add(this._referringProviderAddressTextBox);
            this.Controls.Add(label1);
            this.Controls.Add(this._referringProviderMiddleTextBox);
            this.Controls.Add(this._referringProviderLastTextBox);
            this.Controls.Add(this._referringProviderFirstTextBox);
            this.Controls.Add(label2);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(500, 53);
            this.Name = "ReferralPanel";
            this.Size = new System.Drawing.Size(532, 53);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _referringProviderMiddleTextBox;
        private System.Windows.Forms.TextBox _referringProviderLastTextBox;
        private System.Windows.Forms.TextBox _referringProviderFirstTextBox;
        private System.Windows.Forms.TextBox _referringProviderAddressTextBox;
    }
}
