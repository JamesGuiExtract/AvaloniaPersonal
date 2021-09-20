
namespace Extract.FileActionManager.Forms
{
    partial class PasswordComplexityRequirementsDialog
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this._uppercaseCheckBox = new System.Windows.Forms.CheckBox();
            this._lowercaseCheckBox = new System.Windows.Forms.CheckBox();
            this._numberCheckBox = new System.Windows.Forms.CheckBox();
            this._punctuationCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this._lengthNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._lengthNumericUpDown)).BeginInit();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this._uppercaseCheckBox);
            this.flowLayoutPanel1.Controls.Add(this._lowercaseCheckBox);
            this.flowLayoutPanel1.Controls.Add(this._numberCheckBox);
            this.flowLayoutPanel1.Controls.Add(this._punctuationCheckBox);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this._lengthNumericUpDown);
            this.flowLayoutPanel1.Controls.Add(this.flowLayoutPanel2);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(283, 214);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(276, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Require characters from each of the following categories:";
            // 
            // _uppercaseCheckBox
            // 
            this._uppercaseCheckBox.AutoSize = true;
            this._uppercaseCheckBox.Location = new System.Drawing.Point(3, 22);
            this._uppercaseCheckBox.Name = "_uppercaseCheckBox";
            this._uppercaseCheckBox.Size = new System.Drawing.Size(133, 17);
            this._uppercaseCheckBox.TabIndex = 1;
            this._uppercaseCheckBox.Text = "Upper case letter (A-Z)";
            this._uppercaseCheckBox.UseVisualStyleBackColor = true;
            // 
            // _lowercaseCheckBox
            // 
            this._lowercaseCheckBox.AutoSize = true;
            this._lowercaseCheckBox.Location = new System.Drawing.Point(3, 45);
            this._lowercaseCheckBox.Name = "_lowercaseCheckBox";
            this._lowercaseCheckBox.Size = new System.Drawing.Size(130, 17);
            this._lowercaseCheckBox.TabIndex = 2;
            this._lowercaseCheckBox.Text = "Lower case letter (a-z)";
            this._lowercaseCheckBox.UseVisualStyleBackColor = true;
            // 
            // _numberCheckBox
            // 
            this._numberCheckBox.AutoSize = true;
            this._numberCheckBox.Location = new System.Drawing.Point(3, 68);
            this._numberCheckBox.Name = "_numberCheckBox";
            this._numberCheckBox.Size = new System.Drawing.Size(71, 17);
            this._numberCheckBox.TabIndex = 3;
            this._numberCheckBox.Text = "Digit (0-9)";
            this._numberCheckBox.UseVisualStyleBackColor = true;
            // 
            // _punctuationCheckBox
            // 
            this._punctuationCheckBox.AutoSize = true;
            this._punctuationCheckBox.Location = new System.Drawing.Point(3, 91);
            this._punctuationCheckBox.Name = "_punctuationCheckBox";
            this._punctuationCheckBox.Size = new System.Drawing.Size(161, 17);
            this._punctuationCheckBox.TabIndex = 4;
            this._punctuationCheckBox.Text = "Punctuation  (!, $, #, %, etc.)";
            this._punctuationCheckBox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 123);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 12, 3, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Length requirement:";
            // 
            // _lengthNumericUpDown
            // 
            this._lengthNumericUpDown.Location = new System.Drawing.Point(3, 145);
            this._lengthNumericUpDown.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this._lengthNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._lengthNumericUpDown.Name = "_lengthNumericUpDown";
            this._lengthNumericUpDown.Size = new System.Drawing.Size(120, 20);
            this._lengthNumericUpDown.TabIndex = 6;
            this._lengthNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // _cancelButton
            // 
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(198, 3);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 1;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(117, 3);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 0;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this._cancelButton);
            this.flowLayoutPanel2.Controls.Add(this._okButton);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 180);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(276, 29);
            this.flowLayoutPanel2.TabIndex = 2;
            // 
            // ConfigurePasswordComplexityRequirements
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(310, 240);
            this.ControlBox = false;
            this.Controls.Add(this.flowLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ConfigurePasswordComplexityRequirements";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure password complexity requirements";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._lengthNumericUpDown)).EndInit();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox _uppercaseCheckBox;
        private System.Windows.Forms.CheckBox _lowercaseCheckBox;
        private System.Windows.Forms.CheckBox _numberCheckBox;
        private System.Windows.Forms.CheckBox _punctuationCheckBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown _lengthNumericUpDown;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
    }
}