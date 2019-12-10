namespace Extract.Redaction.Verification
{
    partial class ExemptionCodeListDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="ExemptionCodeListDialog"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ExemptionCodeListDialog"/>.
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.ColumnHeader checkBoxColumnHeader;
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._categoryComboBox = new System.Windows.Forms.ComboBox();
            this._codesListView = new System.Windows.Forms.ListView();
            this._codeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this._summaryColumnHeader = new System.Windows.Forms.ColumnHeader();
            this._descriptionTextBox = new System.Windows.Forms.TextBox();
            this._otherTextCheckBox = new System.Windows.Forms.CheckBox();
            this._otherTextTextBox = new System.Windows.Forms.TextBox();
            this._sampleTextBox = new System.Windows.Forms.TextBox();
            this._clearButton = new System.Windows.Forms.Button();
            this._applyLastButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            checkBoxColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(135, 13);
            label1.TabIndex = 0;
            label1.Text = "Exemption code categories";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(13, 54);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(120, 13);
            label2.TabIndex = 2;
            label2.Text = "Select exemption codes";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 222);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(185, 13);
            label3.TabIndex = 4;
            label3.Text = "Detailed explanation of selected code";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(13, 383);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(173, 13);
            label4.TabIndex = 8;
            label4.Text = "Exemption code or reason selected";
            // 
            // checkBoxColumnHeader
            // 
            checkBoxColumnHeader.Text = "";
            checkBoxColumnHeader.Width = 20;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(356, 433);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 13;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(275, 433);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 12;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _categoryComboBox
            // 
            this._categoryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._categoryComboBox.FormattingEnabled = true;
            this._categoryComboBox.Location = new System.Drawing.Point(16, 30);
            this._categoryComboBox.Name = "_categoryComboBox";
            this._categoryComboBox.Size = new System.Drawing.Size(415, 21);
            this._categoryComboBox.TabIndex = 1;
            this._categoryComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleCategoryComboBoxSelectedIndexChanged);
            // 
            // _codesListView
            // 
            this._codesListView.CheckBoxes = true;
            this._codesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            checkBoxColumnHeader,
            this._codeColumnHeader,
            this._summaryColumnHeader});
            this._codesListView.FullRowSelect = true;
            this._codesListView.Location = new System.Drawing.Point(16, 70);
            this._codesListView.Name = "_codesListView";
            this._codesListView.Size = new System.Drawing.Size(415, 149);
            this._codesListView.TabIndex = 3;
            this._codesListView.UseCompatibleStateImageBehavior = false;
            this._codesListView.View = System.Windows.Forms.View.Details;
            this._codesListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.HandleCodesListViewItemChecked);
            this._codesListView.SelectedIndexChanged += new System.EventHandler(this.HandleCodesListViewSelectedIndexChanged);
            // 
            // _codeColumnHeader
            // 
            this._codeColumnHeader.Text = "Code";
            this._codeColumnHeader.Width = 70;
            // 
            // _summaryColumnHeader
            // 
            this._summaryColumnHeader.Text = "Summary";
            this._summaryColumnHeader.Width = 300;
            // 
            // _descriptionTextBox
            // 
            this._descriptionTextBox.Location = new System.Drawing.Point(16, 238);
            this._descriptionTextBox.Multiline = true;
            this._descriptionTextBox.Name = "_descriptionTextBox";
            this._descriptionTextBox.ReadOnly = true;
            this._descriptionTextBox.Size = new System.Drawing.Size(415, 85);
            this._descriptionTextBox.TabIndex = 5;
            // 
            // _otherTextCheckBox
            // 
            this._otherTextCheckBox.AutoSize = true;
            this._otherTextCheckBox.Location = new System.Drawing.Point(16, 332);
            this._otherTextCheckBox.Name = "_otherTextCheckBox";
            this._otherTextCheckBox.Size = new System.Drawing.Size(271, 17);
            this._otherTextCheckBox.TabIndex = 6;
            this._otherTextCheckBox.Text = "Additionally associate this exemption code or reason";
            this._otherTextCheckBox.UseVisualStyleBackColor = true;
            this._otherTextCheckBox.CheckedChanged += new System.EventHandler(this.HandleOtherTextCheckBoxCheckedChanged);
            // 
            // _otherTextTextBox
            // 
            this._otherTextTextBox.Location = new System.Drawing.Point(16, 355);
            this._otherTextTextBox.Name = "_otherTextTextBox";
            this._otherTextTextBox.Size = new System.Drawing.Size(415, 20);
            this._otherTextTextBox.TabIndex = 7;
            this._otherTextTextBox.TextChanged += new System.EventHandler(this.HandleOtherTextTextBoxTextChanged);
            // 
            // _sampleTextBox
            // 
            this._sampleTextBox.Location = new System.Drawing.Point(16, 399);
            this._sampleTextBox.Name = "_sampleTextBox";
            this._sampleTextBox.ReadOnly = true;
            this._sampleTextBox.Size = new System.Drawing.Size(415, 20);
            this._sampleTextBox.TabIndex = 9;
            // 
            // _clearButton
            // 
            this._clearButton.Location = new System.Drawing.Point(16, 433);
            this._clearButton.Name = "_clearButton";
            this._clearButton.Size = new System.Drawing.Size(75, 23);
            this._clearButton.TabIndex = 10;
            this._clearButton.Text = "Clear All";
            this._clearButton.UseVisualStyleBackColor = true;
            this._clearButton.Click += new System.EventHandler(this.HandleClearButtonClick);
            // 
            // _applyLastButton
            // 
            this._applyLastButton.Location = new System.Drawing.Point(98, 433);
            this._applyLastButton.Name = "_applyLastButton";
            this._applyLastButton.Size = new System.Drawing.Size(75, 23);
            this._applyLastButton.TabIndex = 11;
            this._applyLastButton.Text = "Apply Last";
            this._applyLastButton.UseVisualStyleBackColor = true;
            this._applyLastButton.Click += new System.EventHandler(this.HandleApplyLastButtonClick);
            // 
            // ExemptionCodeListDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(444, 468);
            this.Controls.Add(this._applyLastButton);
            this.Controls.Add(this._clearButton);
            this.Controls.Add(this._sampleTextBox);
            this.Controls.Add(label4);
            this.Controls.Add(this._otherTextTextBox);
            this.Controls.Add(this._otherTextCheckBox);
            this.Controls.Add(this._descriptionTextBox);
            this.Controls.Add(label3);
            this.Controls.Add(this._codesListView);
            this.Controls.Add(label2);
            this.Controls.Add(this._categoryComboBox);
            this.Controls.Add(label1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExemptionCodeListDialog";
            this.ShowIcon = false;
            this.Text = "Exemption codes";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.ComboBox _categoryComboBox;
        private System.Windows.Forms.ListView _codesListView;
        private System.Windows.Forms.TextBox _descriptionTextBox;
        private System.Windows.Forms.CheckBox _otherTextCheckBox;
        private System.Windows.Forms.TextBox _otherTextTextBox;
        private System.Windows.Forms.TextBox _sampleTextBox;
        private System.Windows.Forms.Button _clearButton;
        private System.Windows.Forms.Button _applyLastButton;
        private System.Windows.Forms.ColumnHeader _codeColumnHeader;
        private System.Windows.Forms.ColumnHeader _summaryColumnHeader;
    }
}