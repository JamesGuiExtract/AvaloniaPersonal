namespace Extract.Redaction.Verification
{
    partial class SlideshowUserOptionsDialog
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
            this.label2 = new System.Windows.Forms.Label();
            this._intervalUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._autoStartCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this._intervalUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(345, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "seconds.";
            // 
            // _intervalUpDown
            // 
            this._intervalUpDown.Location = new System.Drawing.Point(298, 7);
            this._intervalUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this._intervalUpDown.Name = "_intervalUpDown";
            this._intervalUpDown.Size = new System.Drawing.Size(41, 20);
            this._intervalUpDown.TabIndex = 4;
            this._intervalUpDown.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(280, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Automatically advance to the next page or document after";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(249, 56);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(70, 23);
            this._okButton.TabIndex = 17;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(325, 56);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(70, 23);
            this._cancelButton.TabIndex = 16;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _autoStartCheckBox
            // 
            this._autoStartCheckBox.AutoSize = true;
            this._autoStartCheckBox.Checked = true;
            this._autoStartCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._autoStartCheckBox.Location = new System.Drawing.Point(12, 33);
            this._autoStartCheckBox.Name = "_autoStartCheckBox";
            this._autoStartCheckBox.Size = new System.Drawing.Size(309, 17);
            this._autoStartCheckBox.TabIndex = 18;
            this._autoStartCheckBox.Text = "Automatically start slideshow when verification session starts";
            this._autoStartCheckBox.UseVisualStyleBackColor = true;
            // 
            // SlideshowUserOptionsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 91);
            this.Controls.Add(this._autoStartCheckBox);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._intervalUpDown);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SlideshowUserOptionsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Slideshow options";
            ((System.ComponentModel.ISupportInitialize)(this._intervalUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown _intervalUpDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.CheckBox _autoStartCheckBox;
    }
}