namespace Extract.Demo_Pagination
{
    partial class RadiologyPanel
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
            this._procedureTextBox = new System.Windows.Forms.TextBox();
            this._impressionTextBox = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = System.Drawing.Color.White;
            label2.Location = new System.Drawing.Point(-3, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(59, 13);
            label2.TabIndex = 6;
            label2.Text = "Procedure:";
            // 
            // _procedureTextBox
            // 
            this._procedureTextBox.Location = new System.Drawing.Point(0, 17);
            this._procedureTextBox.Name = "_procedureTextBox";
            this._procedureTextBox.Size = new System.Drawing.Size(165, 20);
            this._procedureTextBox.TabIndex = 7;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = System.Drawing.Color.White;
            label1.Location = new System.Drawing.Point(173, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(60, 13);
            label1.TabIndex = 8;
            label1.Text = "Impression:";
            // 
            // _impressionTextBox
            // 
            this._impressionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._impressionTextBox.Location = new System.Drawing.Point(176, 16);
            this._impressionTextBox.Multiline = true;
            this._impressionTextBox.Name = "_impressionTextBox";
            this._impressionTextBox.Size = new System.Drawing.Size(356, 64);
            this._impressionTextBox.TabIndex = 9;
            // 
            // RadiologyPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Controls.Add(this._impressionTextBox);
            this.Controls.Add(label1);
            this.Controls.Add(this._procedureTextBox);
            this.Controls.Add(label2);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(500, 53);
            this.Name = "RadiologyPanel";
            this.Size = new System.Drawing.Size(532, 80);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _procedureTextBox;
        private System.Windows.Forms.TextBox _impressionTextBox;


    }
}
