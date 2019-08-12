namespace Extract.AttributeFinder.Rules
{
    partial class AutoShrinkRedactionZonesSettingsDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._attributeSelectorControl = new Extract.Utilities.Forms.ConfigurableObjectControl();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._autoExpandBeforeAutoShrinkCheckBox = new System.Windows.Forms.CheckBox();
            this._maxPixelsToExpandNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._maxPixelsToExpandNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(166, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select the attributes to be shrunk:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._attributeSelectorControl);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(409, 89);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // _attributeSelectorControl
            // 
            this._attributeSelectorControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeSelectorControl.CategoryName = "UCLID AF-API Selectors";
            this._attributeSelectorControl.Location = new System.Drawing.Point(5, 32);
            this._attributeSelectorControl.Name = "_attributeSelectorControl";
            this._attributeSelectorControl.Size = new System.Drawing.Size(398, 48);
            this._attributeSelectorControl.TabIndex = 1;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(346, 173);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 5;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(265, 173);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 4;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _autoExpandBeforeAutoShrinkCheckBox
            // 
            this._autoExpandBeforeAutoShrinkCheckBox.AutoSize = true;
            this._autoExpandBeforeAutoShrinkCheckBox.Location = new System.Drawing.Point(12, 107);
            this._autoExpandBeforeAutoShrinkCheckBox.Name = "_autoExpandBeforeAutoShrinkCheckBox";
            this._autoExpandBeforeAutoShrinkCheckBox.Size = new System.Drawing.Size(370, 17);
            this._autoExpandBeforeAutoShrinkCheckBox.TabIndex = 1;
            this._autoExpandBeforeAutoShrinkCheckBox.Text = "Expand zones to first white row/column in each direction before shrinking";
            this._autoExpandBeforeAutoShrinkCheckBox.UseVisualStyleBackColor = true;
            this._autoExpandBeforeAutoShrinkCheckBox.CheckedChanged += new System.EventHandler(this._autoExpandBeforeAutoShrinkCheckBox_CheckedChanged);
            // 
            // _maxPixelsToExpandNumericUpDown
            // 
            this._maxPixelsToExpandNumericUpDown.Location = new System.Drawing.Point(317, 130);
            this._maxPixelsToExpandNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._maxPixelsToExpandNumericUpDown.Name = "_maxPixelsToExpandNumericUpDown";
            this._maxPixelsToExpandNumericUpDown.Size = new System.Drawing.Size(54, 20);
            this._maxPixelsToExpandNumericUpDown.TabIndex = 3;
            this._maxPixelsToExpandNumericUpDown.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 132);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(256, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Maximum number of pixel to expand in each direction";
            // 
            // AutoShrinkRedactionZonesSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(433, 208);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._maxPixelsToExpandNumericUpDown);
            this.Controls.Add(this._autoExpandBeforeAutoShrinkCheckBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AutoShrinkRedactionZonesSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Auto-shrink redaction zones settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._maxPixelsToExpandNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private Utilities.Forms.ConfigurableObjectControl _attributeSelectorControl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox _autoExpandBeforeAutoShrinkCheckBox;
        private System.Windows.Forms.NumericUpDown _maxPixelsToExpandNumericUpDown;
        private System.Windows.Forms.Label label2;
    }
}
