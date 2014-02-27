namespace Extract.FileActionManager.Conditions
{
    partial class PageCountConditionSettingsDialog
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
            System.Windows.Forms.Label label7;
            this._pageCountUpDown = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._comparisonComboBox = new Extract.Utilities.Forms.BetterComboBox();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._useDBPageCountCheckBox = new System.Windows.Forms.CheckBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label7 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pageCountUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._useDBPageCountCheckBox);
            groupBox1.Controls.Add(this._pageCountUpDown);
            groupBox1.Controls.Add(this._comparisonComboBox);
            groupBox1.Controls.Add(label7);
            groupBox1.Location = new System.Drawing.Point(13, 13);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(629, 63);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            // 
            // _pageCountUpDown
            // 
            this._pageCountUpDown.IntegersOnly = true;
            this._pageCountUpDown.Location = new System.Drawing.Point(548, 14);
            this._pageCountUpDown.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this._pageCountUpDown.Name = "_pageCountUpDown";
            this._pageCountUpDown.Size = new System.Drawing.Size(69, 20);
            this._pageCountUpDown.TabIndex = 2;
            this._pageCountUpDown.ThousandsSeparator = true;
            // 
            // _comparisonComboBox
            // 
            this._comparisonComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._comparisonComboBox.FormattingEnabled = true;
            this._comparisonComboBox.Location = new System.Drawing.Point(408, 13);
            this._comparisonComboBox.Name = "_comparisonComboBox";
            this._comparisonComboBox.Size = new System.Drawing.Size(134, 21);
            this._comparisonComboBox.TabIndex = 1;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(8, 17);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(380, 13);
            label7.TabIndex = 0;
            label7.Text = "Consider this condition as met if the number of pages in the current document is";
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.Location = new System.Drawing.Point(486, 84);
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
            this._buttonCancel.Location = new System.Drawing.Point(567, 84);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 2;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _useDBPageCountCheckBox
            // 
            this._useDBPageCountCheckBox.AutoSize = true;
            this._useDBPageCountCheckBox.Location = new System.Drawing.Point(8, 40);
            this._useDBPageCountCheckBox.Name = "_useDBPageCountCheckBox";
            this._useDBPageCountCheckBox.Size = new System.Drawing.Size(299, 17);
            this._useDBPageCountCheckBox.TabIndex = 3;
            this._useDBPageCountCheckBox.Text = "Use the page count from the database if available. (faster)";
            this._useDBPageCountCheckBox.UseVisualStyleBackColor = true;
            // 
            // PageCountConditionSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(654, 119);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PageCountConditionSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Page count condition settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pageCountUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonCancel;
        private Extract.Utilities.Forms.BetterComboBox _comparisonComboBox;
        private Utilities.Forms.BetterNumericUpDown _pageCountUpDown;
        private System.Windows.Forms.CheckBox _useDBPageCountCheckBox;
    }
}