namespace IDShieldOffice
{
    partial class BatesNumberManagerPropertyPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="BatesNumberManagerPropertyPage"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="BatesNumberManagerPropertyPage"/>.
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
                if (_formatDialog != null)
                {
                    _formatDialog.Dispose();
                }
                if (_appearanceDialog != null)
                {
                    _appearanceDialog.Dispose();
                }
            }

            // Release unmanaged resources

            // Call base dispose method
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
            this._requireBatesCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._changeFormatButton = new System.Windows.Forms.Button();
            this._sampleBatesNumberTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._changeAppearanceButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._requireBatesCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(435, 46);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "General";
            // 
            // _requireBatesCheckBox
            // 
            this._requireBatesCheckBox.AutoSize = true;
            this._requireBatesCheckBox.Location = new System.Drawing.Point(6, 19);
            this._requireBatesCheckBox.Name = "_requireBatesCheckBox";
            this._requireBatesCheckBox.Size = new System.Drawing.Size(253, 17);
            this._requireBatesCheckBox.TabIndex = 0;
            this._requireBatesCheckBox.Text = "Require Bates number before files can be saved or printed";
            this._requireBatesCheckBox.UseVisualStyleBackColor = true;
            this._requireBatesCheckBox.CheckedChanged += new System.EventHandler(this.HandleRequireBatesCheckBoxCheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this._changeFormatButton);
            this.groupBox2.Controls.Add(this._sampleBatesNumberTextBox);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(4, 57);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(435, 68);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Bates number";
            // 
            // _changeFormatButton
            // 
            this._changeFormatButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._changeFormatButton.Location = new System.Drawing.Point(354, 34);
            this._changeFormatButton.Name = "_changeFormatButton";
            this._changeFormatButton.Size = new System.Drawing.Size(75, 23);
            this._changeFormatButton.TabIndex = 2;
            this._changeFormatButton.Text = "Change ...";
            this._changeFormatButton.UseVisualStyleBackColor = true;
            this._changeFormatButton.Click += new System.EventHandler(this.HandleChangeFormatButtonClick);
            // 
            // _sampleBatesNumberTextBox
            // 
            this._sampleBatesNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._sampleBatesNumberTextBox.Enabled = false;
            this._sampleBatesNumberTextBox.Location = new System.Drawing.Point(6, 37);
            this._sampleBatesNumberTextBox.Name = "_sampleBatesNumberTextBox";
            this._sampleBatesNumberTextBox.Size = new System.Drawing.Size(342, 20);
            this._sampleBatesNumberTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(182, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Bates number format for current page";
            // 
            // _changeAppearanceButton
            // 
            this._changeAppearanceButton.Location = new System.Drawing.Point(4, 132);
            this._changeAppearanceButton.Name = "_changeAppearanceButton";
            this._changeAppearanceButton.Size = new System.Drawing.Size(234, 23);
            this._changeAppearanceButton.TabIndex = 2;
            this._changeAppearanceButton.Text = "Change default position and appearance ...";
            this._changeAppearanceButton.UseVisualStyleBackColor = true;
            this._changeAppearanceButton.Click += new System.EventHandler(this.HandleChangeAppearanceButtonClick);
            // 
            // BatesNumberManagerPropertyPage
            // 
            this.Controls.Add(this._changeAppearanceButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "BatesNumberManagerPropertyPage";
            this.Size = new System.Drawing.Size(442, 180);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox _requireBatesCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _changeFormatButton;
        private System.Windows.Forms.TextBox _sampleBatesNumberTextBox;
        private System.Windows.Forms.Button _changeAppearanceButton;
    }
}
