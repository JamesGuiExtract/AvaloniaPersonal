namespace Extract.FileActionManager.Conditions
{
    partial class PaginationDataValidityConditionSettingsDialog
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
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
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
            System.Windows.Forms.GroupBox groupBox1;
            this._ifNoWarningsCheckBox = new System.Windows.Forms.CheckBox();
            this._ifNoErrorsCheckBox = new System.Windows.Forms.CheckBox();
            this.betterNumericUpDown1 = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.betterNumericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._ifNoWarningsCheckBox);
            groupBox1.Controls.Add(this._ifNoErrorsCheckBox);
            groupBox1.Controls.Add(this.betterNumericUpDown1);
            groupBox1.Location = new System.Drawing.Point(13, 13);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(314, 72);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "This condition is met only if";
            // 
            // _ifNoWarningsCheckBox
            // 
            this._ifNoWarningsCheckBox.AutoSize = true;
            this._ifNoWarningsCheckBox.Location = new System.Drawing.Point(8, 48);
            this._ifNoWarningsCheckBox.Name = "_ifNoWarningsCheckBox";
            this._ifNoWarningsCheckBox.Size = new System.Drawing.Size(161, 17);
            this._ifNoWarningsCheckBox.TabIndex = 1;
            this._ifNoWarningsCheckBox.Text = "If there are no data warnings";
            this._ifNoWarningsCheckBox.UseVisualStyleBackColor = true;
            // 
            // _ifNoErrorsCheckBox
            // 
            this._ifNoErrorsCheckBox.AutoSize = true;
            this._ifNoErrorsCheckBox.Location = new System.Drawing.Point(8, 21);
            this._ifNoErrorsCheckBox.Name = "_ifNoErrorsCheckBox";
            this._ifNoErrorsCheckBox.Size = new System.Drawing.Size(140, 17);
            this._ifNoErrorsCheckBox.TabIndex = 0;
            this._ifNoErrorsCheckBox.Text = "There are no data errors";
            this._ifNoErrorsCheckBox.UseVisualStyleBackColor = true;
            // 
            // betterNumericUpDown1
            // 
            this.betterNumericUpDown1.IntegersOnly = true;
            this.betterNumericUpDown1.Location = new System.Drawing.Point(554, 133);
            this.betterNumericUpDown1.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.betterNumericUpDown1.Name = "betterNumericUpDown1";
            this.betterNumericUpDown1.Size = new System.Drawing.Size(69, 20);
            this.betterNumericUpDown1.TabIndex = 9;
            this.betterNumericUpDown1.ThousandsSeparator = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.Location = new System.Drawing.Point(171, 91);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 1;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(252, 91);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 2;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // PaginationDataValidityConditionSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(339, 126);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PaginationDataValidityConditionSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Data validity condition settings (pagination)";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.betterNumericUpDown1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.CheckBox _ifNoWarningsCheckBox;
        private System.Windows.Forms.CheckBox _ifNoErrorsCheckBox;
        private Utilities.Forms.BetterNumericUpDown betterNumericUpDown1;
    }
}