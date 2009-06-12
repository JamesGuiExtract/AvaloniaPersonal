namespace IDShieldOffice
{
    partial class BracketedTextRulePropertyPage
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._curvedBracketsCheckBox = new System.Windows.Forms.CheckBox();
            this._curlyBracketsCheckBox = new System.Windows.Forms.CheckBox();
            this._squareBracketsCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._curvedBracketsCheckBox);
            this.groupBox1.Controls.Add(this._curlyBracketsCheckBox);
            this.groupBox1.Controls.Add(this._squareBracketsCheckBox);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(296, 90);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Search for text within the following bracket types";
            // 
            // curvedBracketsCheckBox
            // 
            this._curvedBracketsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this._curvedBracketsCheckBox.AutoSize = true;
            this._curvedBracketsCheckBox.Location = new System.Drawing.Point(6, 65);
            this._curvedBracketsCheckBox.Name = "curvedBracketsCheckBox";
            this._curvedBracketsCheckBox.Size = new System.Drawing.Size(105, 17);
            this._curvedBracketsCheckBox.TabIndex = 2;
            this._curvedBracketsCheckBox.Text = "Parenthesis ( ... )";
            this._curvedBracketsCheckBox.UseVisualStyleBackColor = true;
            this._curvedBracketsCheckBox.CheckedChanged += new System.EventHandler(this.OnCheckBoxCheckedChanged);
            // 
            // curlyBracketsCheckBox
            // 
            this._curlyBracketsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this._curlyBracketsCheckBox.AutoSize = true;
            this._curlyBracketsCheckBox.Location = new System.Drawing.Point(6, 42);
            this._curlyBracketsCheckBox.Name = "curlyBracketsCheckBox";
            this._curlyBracketsCheckBox.Size = new System.Drawing.Size(119, 17);
            this._curlyBracketsCheckBox.TabIndex = 1;
            this._curlyBracketsCheckBox.Text = "Curly brackets { ... }";
            this._curlyBracketsCheckBox.UseVisualStyleBackColor = true;
            this._curlyBracketsCheckBox.CheckedChanged += new System.EventHandler(this.OnCheckBoxCheckedChanged);
            // 
            // squareBracketCheckBox
            // 
            this._squareBracketsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this._squareBracketsCheckBox.AutoSize = true;
            this._squareBracketsCheckBox.Location = new System.Drawing.Point(6, 19);
            this._squareBracketsCheckBox.Name = "squareBracketCheckBox";
            this._squareBracketsCheckBox.Size = new System.Drawing.Size(128, 17);
            this._squareBracketsCheckBox.TabIndex = 0;
            this._squareBracketsCheckBox.Text = "Square brackets [ ... ]";
            this._squareBracketsCheckBox.UseVisualStyleBackColor = true;
            this._squareBracketsCheckBox.CheckedChanged += new System.EventHandler(this.OnCheckBoxCheckedChanged);
            // 
            // BracketedTextRulePropertyPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "BracketedTextRulePropertyPage";
            this.Size = new System.Drawing.Size(296, 90);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox _curvedBracketsCheckBox;
        private System.Windows.Forms.CheckBox _curlyBracketsCheckBox;
        private System.Windows.Forms.CheckBox _squareBracketsCheckBox;
    }
}
