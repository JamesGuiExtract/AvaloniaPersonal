namespace Extract.Redaction.Verification
{
    partial class VerificationOptionsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="VerificationOptionsDialog"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="VerificationOptionsDialog"/>.
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
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label3;
            this._autoZoomScaleTrackBar = new System.Windows.Forms.TrackBar();
            this._autoZoomScaleTextBox = new System.Windows.Forms.TextBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._autoZoomCheckBox = new System.Windows.Forms.CheckBox();
            this._autoToolCheckBox = new System.Windows.Forms.CheckBox();
            this._autoToolComboBox = new System.Windows.Forms.ComboBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._autoZoomScaleTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this._autoZoomScaleTrackBar);
            groupBox1.Controls.Add(this._autoZoomScaleTextBox);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new System.Drawing.Point(13, 36);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(266, 83);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Zoom level";
            // 
            // _autoZoomScaleTrackBar
            // 
            this._autoZoomScaleTrackBar.Enabled = false;
            this._autoZoomScaleTrackBar.LargeChange = 3;
            this._autoZoomScaleTrackBar.Location = new System.Drawing.Point(10, 36);
            this._autoZoomScaleTrackBar.Minimum = 1;
            this._autoZoomScaleTrackBar.Name = "_autoZoomScaleTrackBar";
            this._autoZoomScaleTrackBar.Size = new System.Drawing.Size(224, 45);
            this._autoZoomScaleTrackBar.TabIndex = 2;
            this._autoZoomScaleTrackBar.Value = 1;
            this._autoZoomScaleTrackBar.ValueChanged += new System.EventHandler(this.HandleAutoZoomScaleTrackBarValueChanged);
            // 
            // _autoZoomScaleTextBox
            // 
            this._autoZoomScaleTextBox.Enabled = false;
            this._autoZoomScaleTextBox.Location = new System.Drawing.Point(240, 36);
            this._autoZoomScaleTextBox.Name = "_autoZoomScaleTextBox";
            this._autoZoomScaleTextBox.ReadOnly = true;
            this._autoZoomScaleTextBox.Size = new System.Drawing.Size(20, 20);
            this._autoZoomScaleTextBox.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(182, 20);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(52, 13);
            label2.TabIndex = 1;
            label2.Text = "Zoom out";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(7, 20);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(45, 13);
            label1.TabIndex = 0;
            label1.Text = "Zoom in";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(183, 128);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(90, 13);
            label3.TabIndex = 4;
            label3.Text = "tool after highlight";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(204, 164);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 6;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(123, 164);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 5;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _autoZoomCheckBox
            // 
            this._autoZoomCheckBox.AutoSize = true;
            this._autoZoomCheckBox.Location = new System.Drawing.Point(12, 12);
            this._autoZoomCheckBox.Name = "_autoZoomCheckBox";
            this._autoZoomCheckBox.Size = new System.Drawing.Size(205, 17);
            this._autoZoomCheckBox.TabIndex = 0;
            this._autoZoomCheckBox.Text = "Automatically zoom to redactable data";
            this._autoZoomCheckBox.UseVisualStyleBackColor = true;
            this._autoZoomCheckBox.CheckedChanged += new System.EventHandler(this.HandleAutoZoomCheckBoxCheckedChanged);
            // 
            // _autoToolCheckBox
            // 
            this._autoToolCheckBox.AutoSize = true;
            this._autoToolCheckBox.Location = new System.Drawing.Point(12, 127);
            this._autoToolCheckBox.Name = "_autoToolCheckBox";
            this._autoToolCheckBox.Size = new System.Drawing.Size(59, 17);
            this._autoToolCheckBox.TabIndex = 2;
            this._autoToolCheckBox.Text = "Enable";
            this._autoToolCheckBox.UseVisualStyleBackColor = true;
            this._autoToolCheckBox.CheckedChanged += new System.EventHandler(this.HandleAutoToolCheckBoxCheckedChanged);
            // 
            // _autoToolComboBox
            // 
            this._autoToolComboBox.DisplayMember = "selection";
            this._autoToolComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._autoToolComboBox.Enabled = false;
            this._autoToolComboBox.FormattingEnabled = true;
            this._autoToolComboBox.Location = new System.Drawing.Point(77, 125);
            this._autoToolComboBox.Name = "_autoToolComboBox";
            this._autoToolComboBox.Size = new System.Drawing.Size(100, 21);
            this._autoToolComboBox.TabIndex = 3;
            // 
            // VerificationOptionsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(292, 199);
            this.Controls.Add(label3);
            this.Controls.Add(this._autoToolComboBox);
            this.Controls.Add(this._autoToolCheckBox);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._autoZoomCheckBox);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VerificationOptionsDialog";
            this.ShowIcon = false;
            this.Text = "Verification options";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._autoZoomScaleTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.CheckBox _autoZoomCheckBox;
        private System.Windows.Forms.TextBox _autoZoomScaleTextBox;
        private System.Windows.Forms.TrackBar _autoZoomScaleTrackBar;
        private System.Windows.Forms.CheckBox _autoToolCheckBox;
        private System.Windows.Forms.ComboBox _autoToolComboBox;
    }
}