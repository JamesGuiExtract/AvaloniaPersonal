namespace Extract.Demo_Pagination
{
    partial class BloodTypePanel
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
            System.Windows.Forms.Label label1;
            this._bloodTypeTextBox = new System.Windows.Forms.TextBox();
            this._rhTextBox = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = System.Drawing.Color.White;
            label5.Location = new System.Drawing.Point(20, 6);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(64, 13);
            label5.TabIndex = 14;
            label5.Text = "Blood Type:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = System.Drawing.Color.White;
            label1.Location = new System.Drawing.Point(142, 6);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(24, 13);
            label1.TabIndex = 16;
            label1.Text = "Rh:";
            // 
            // _bloodTypeTextBox
            // 
            this._bloodTypeTextBox.Location = new System.Drawing.Point(91, 3);
            this._bloodTypeTextBox.Name = "_bloodTypeTextBox";
            this._bloodTypeTextBox.Size = new System.Drawing.Size(45, 20);
            this._bloodTypeTextBox.TabIndex = 15;
            // 
            // _rhTextBox
            // 
            this._rhTextBox.Location = new System.Drawing.Point(172, 3);
            this._rhTextBox.Name = "_rhTextBox";
            this._rhTextBox.Size = new System.Drawing.Size(56, 20);
            this._rhTextBox.TabIndex = 17;
            // 
            // BloodTypePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Controls.Add(this._rhTextBox);
            this.Controls.Add(label1);
            this.Controls.Add(this._bloodTypeTextBox);
            this.Controls.Add(label5);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "BloodTypePanel";
            this.Size = new System.Drawing.Size(229, 27);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _bloodTypeTextBox;
        private System.Windows.Forms.TextBox _rhTextBox;

    }
}
