namespace Extract.Imaging
{
    partial class PdfPasswordSettingsDialog
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

                if (_errorProvider != null)
                {
                    _errorProvider.Dispose();
                    _errorProvider = null;
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
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label2;
            this._userPasswordText2 = new System.Windows.Forms.TextBox();
            this._userPasswordText1 = new System.Windows.Forms.TextBox();
            this._enableUserPasswordCheckBox = new System.Windows.Forms.CheckBox();
            this._allowHighQualityPrintingCheck = new System.Windows.Forms.CheckBox();
            this._allowDocumentAssemblyCheck = new System.Windows.Forms.CheckBox();
            this._allowAccessibilityCheck = new System.Windows.Forms.CheckBox();
            this._allowFillInFormFieldsCheck = new System.Windows.Forms.CheckBox();
            this._allowAddOrModifyAnnotationsCheck = new System.Windows.Forms.CheckBox();
            this._allowCopyAndExtractionCheck = new System.Windows.Forms.CheckBox();
            this._allowDocumentModificationsCheck = new System.Windows.Forms.CheckBox();
            this._allowLowQualityPrintCheck = new System.Windows.Forms.CheckBox();
            this._ownerPasswordText2 = new System.Windows.Forms.TextBox();
            this._ownerPasswordText1 = new System.Windows.Forms.TextBox();
            this._enableOwnerPasswordCheckBox = new System.Windows.Forms.CheckBox();
            this._btnOk = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label3 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label4 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(this._userPasswordText2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(this._userPasswordText1);
            groupBox1.Controls.Add(this._enableUserPasswordCheckBox);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(529, 76);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "User password";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(22, 20);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(111, 13);
            label3.TabIndex = 4;
            label3.Text = "Enable user password";
            // 
            // _userPasswordText2
            // 
            this._userPasswordText2.Location = new System.Drawing.Point(233, 42);
            this._userPasswordText2.Name = "_userPasswordText2";
            this._userPasswordText2.Size = new System.Drawing.Size(180, 20);
            this._userPasswordText2.TabIndex = 3;
            this._userPasswordText2.UseSystemPasswordChar = true;
            this._userPasswordText2.TextChanged += new System.EventHandler(this.HandleUserPasswordTextChanged);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(230, 20);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(109, 13);
            label1.TabIndex = 2;
            label1.Text = "Enter password again";
            // 
            // _userPasswordText1
            // 
            this._userPasswordText1.Location = new System.Drawing.Point(25, 42);
            this._userPasswordText1.Name = "_userPasswordText1";
            this._userPasswordText1.Size = new System.Drawing.Size(180, 20);
            this._userPasswordText1.TabIndex = 1;
            this._userPasswordText1.UseSystemPasswordChar = true;
            this._userPasswordText1.TextChanged += new System.EventHandler(this.HandleUserPasswordTextChanged);
            // 
            // _enableUserPasswordCheckBox
            // 
            this._enableUserPasswordCheckBox.AutoSize = true;
            this._enableUserPasswordCheckBox.Location = new System.Drawing.Point(6, 19);
            this._enableUserPasswordCheckBox.Name = "_enableUserPasswordCheckBox";
            this._enableUserPasswordCheckBox.Size = new System.Drawing.Size(15, 14);
            this._enableUserPasswordCheckBox.TabIndex = 0;
            this._enableUserPasswordCheckBox.UseVisualStyleBackColor = true;
            this._enableUserPasswordCheckBox.Click += new System.EventHandler(this.HandleEnablePasswordCheckboxClicked);
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(this._allowHighQualityPrintingCheck);
            groupBox2.Controls.Add(this._allowDocumentAssemblyCheck);
            groupBox2.Controls.Add(this._allowAccessibilityCheck);
            groupBox2.Controls.Add(this._allowFillInFormFieldsCheck);
            groupBox2.Controls.Add(this._allowAddOrModifyAnnotationsCheck);
            groupBox2.Controls.Add(this._allowCopyAndExtractionCheck);
            groupBox2.Controls.Add(this._allowDocumentModificationsCheck);
            groupBox2.Controls.Add(this._allowLowQualityPrintCheck);
            groupBox2.Controls.Add(this._ownerPasswordText2);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(this._ownerPasswordText1);
            groupBox2.Controls.Add(this._enableOwnerPasswordCheckBox);
            groupBox2.Location = new System.Drawing.Point(12, 94);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(529, 256);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Owner password and permissions to set";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(22, 20);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(120, 13);
            label4.TabIndex = 12;
            label4.Text = "Enable owner password";
            // 
            // _allowHighQualityPrintingCheck
            // 
            this._allowHighQualityPrintingCheck.AutoSize = true;
            this._allowHighQualityPrintingCheck.Location = new System.Drawing.Point(25, 229);
            this._allowHighQualityPrintingCheck.Name = "_allowHighQualityPrintingCheck";
            this._allowHighQualityPrintingCheck.Size = new System.Drawing.Size(255, 17);
            this._allowHighQualityPrintingCheck.TabIndex = 11;
            this._allowHighQualityPrintingCheck.Text = "Allow printing of a high resolution faithful print out";
            this._allowHighQualityPrintingCheck.UseVisualStyleBackColor = true;
            this._allowHighQualityPrintingCheck.CheckedChanged += new System.EventHandler(this.HandleCheckBoxChanged);
            // 
            // _allowDocumentAssemblyCheck
            // 
            this._allowDocumentAssemblyCheck.AutoSize = true;
            this._allowDocumentAssemblyCheck.Location = new System.Drawing.Point(25, 206);
            this._allowDocumentAssemblyCheck.Name = "_allowDocumentAssemblyCheck";
            this._allowDocumentAssemblyCheck.Size = new System.Drawing.Size(168, 17);
            this._allowDocumentAssemblyCheck.TabIndex = 10;
            this._allowDocumentAssemblyCheck.Text = "Allow assembling of document";
            this._allowDocumentAssemblyCheck.UseVisualStyleBackColor = true;
            // 
            // _allowAccessibilityCheck
            // 
            this._allowAccessibilityCheck.AutoSize = true;
            this._allowAccessibilityCheck.Location = new System.Drawing.Point(25, 183);
            this._allowAccessibilityCheck.Name = "_allowAccessibilityCheck";
            this._allowAccessibilityCheck.Size = new System.Drawing.Size(334, 17);
            this._allowAccessibilityCheck.TabIndex = 9;
            this._allowAccessibilityCheck.Text = "Allow access to text and graphics to support users with disabilities";
            this._allowAccessibilityCheck.UseVisualStyleBackColor = true;
            // 
            // _allowFillInFormFieldsCheck
            // 
            this._allowFillInFormFieldsCheck.AutoSize = true;
            this._allowFillInFormFieldsCheck.Location = new System.Drawing.Point(25, 160);
            this._allowFillInFormFieldsCheck.Name = "_allowFillInFormFieldsCheck";
            this._allowFillInFormFieldsCheck.Size = new System.Drawing.Size(176, 17);
            this._allowFillInFormFieldsCheck.TabIndex = 8;
            this._allowFillInFormFieldsCheck.Text = "Allow filling in existing form fields";
            this._allowFillInFormFieldsCheck.UseVisualStyleBackColor = true;
            // 
            // _allowAddOrModifyAnnotationsCheck
            // 
            this._allowAddOrModifyAnnotationsCheck.AutoSize = true;
            this._allowAddOrModifyAnnotationsCheck.Location = new System.Drawing.Point(25, 137);
            this._allowAddOrModifyAnnotationsCheck.Name = "_allowAddOrModifyAnnotationsCheck";
            this._allowAddOrModifyAnnotationsCheck.Size = new System.Drawing.Size(360, 17);
            this._allowAddOrModifyAnnotationsCheck.TabIndex = 7;
            this._allowAddOrModifyAnnotationsCheck.Text = "Allow addition or modification of text annotations and filling in form fields";
            this._allowAddOrModifyAnnotationsCheck.UseVisualStyleBackColor = true;
            this._allowAddOrModifyAnnotationsCheck.CheckedChanged += new System.EventHandler(this.HandleCheckBoxChanged);
            // 
            // _allowCopyAndExtractionCheck
            // 
            this._allowCopyAndExtractionCheck.AutoSize = true;
            this._allowCopyAndExtractionCheck.Location = new System.Drawing.Point(25, 114);
            this._allowCopyAndExtractionCheck.Name = "_allowCopyAndExtractionCheck";
            this._allowCopyAndExtractionCheck.Size = new System.Drawing.Size(439, 17);
            this._allowCopyAndExtractionCheck.TabIndex = 6;
            this._allowCopyAndExtractionCheck.Text = "Allow copying / extraction of text and graphics including support to users with d" +
                "isabilities";
            this._allowCopyAndExtractionCheck.UseVisualStyleBackColor = true;
            this._allowCopyAndExtractionCheck.CheckedChanged += new System.EventHandler(this.HandleCheckBoxChanged);
            // 
            // _allowDocumentModificationsCheck
            // 
            this._allowDocumentModificationsCheck.AutoSize = true;
            this._allowDocumentModificationsCheck.Location = new System.Drawing.Point(25, 91);
            this._allowDocumentModificationsCheck.Name = "_allowDocumentModificationsCheck";
            this._allowDocumentModificationsCheck.Size = new System.Drawing.Size(165, 17);
            this._allowDocumentModificationsCheck.TabIndex = 5;
            this._allowDocumentModificationsCheck.Text = "Allow document modifications";
            this._allowDocumentModificationsCheck.UseVisualStyleBackColor = true;
            this._allowDocumentModificationsCheck.CheckedChanged += new System.EventHandler(this.HandleCheckBoxChanged);
            // 
            // _allowLowQualityPrintCheck
            // 
            this._allowLowQualityPrintCheck.AutoSize = true;
            this._allowLowQualityPrintCheck.Location = new System.Drawing.Point(25, 68);
            this._allowLowQualityPrintCheck.Name = "_allowLowQualityPrintCheck";
            this._allowLowQualityPrintCheck.Size = new System.Drawing.Size(176, 17);
            this._allowLowQualityPrintCheck.TabIndex = 4;
            this._allowLowQualityPrintCheck.Text = "Allow printing in low level quality";
            this._allowLowQualityPrintCheck.UseVisualStyleBackColor = true;
            // 
            // _ownerPasswordText2
            // 
            this._ownerPasswordText2.Location = new System.Drawing.Point(233, 42);
            this._ownerPasswordText2.Name = "_ownerPasswordText2";
            this._ownerPasswordText2.Size = new System.Drawing.Size(180, 20);
            this._ownerPasswordText2.TabIndex = 3;
            this._ownerPasswordText2.UseSystemPasswordChar = true;
            this._ownerPasswordText2.TextChanged += new System.EventHandler(this.HandleOwnerPasswordTextChanged);
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(230, 20);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(109, 13);
            label2.TabIndex = 2;
            label2.Text = "Enter password again";
            // 
            // _ownerPasswordText1
            // 
            this._ownerPasswordText1.Location = new System.Drawing.Point(25, 42);
            this._ownerPasswordText1.Name = "_ownerPasswordText1";
            this._ownerPasswordText1.Size = new System.Drawing.Size(180, 20);
            this._ownerPasswordText1.TabIndex = 1;
            this._ownerPasswordText1.UseSystemPasswordChar = true;
            this._ownerPasswordText1.TextChanged += new System.EventHandler(this.HandleOwnerPasswordTextChanged);
            // 
            // _enableOwnerPasswordCheckBox
            // 
            this._enableOwnerPasswordCheckBox.AutoSize = true;
            this._enableOwnerPasswordCheckBox.Location = new System.Drawing.Point(6, 19);
            this._enableOwnerPasswordCheckBox.Name = "_enableOwnerPasswordCheckBox";
            this._enableOwnerPasswordCheckBox.Size = new System.Drawing.Size(15, 14);
            this._enableOwnerPasswordCheckBox.TabIndex = 0;
            this._enableOwnerPasswordCheckBox.UseVisualStyleBackColor = true;
            this._enableOwnerPasswordCheckBox.Click += new System.EventHandler(this.HandleEnablePasswordCheckboxClicked);
            // 
            // _btnOk
            // 
            this._btnOk.Location = new System.Drawing.Point(385, 356);
            this._btnOk.Name = "_btnOk";
            this._btnOk.Size = new System.Drawing.Size(75, 23);
            this._btnOk.TabIndex = 5;
            this._btnOk.Text = "OK";
            this._btnOk.UseVisualStyleBackColor = true;
            this._btnOk.Click += new System.EventHandler(this.HandleOkButtonClicked);
            // 
            // _btnCancel
            // 
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(466, 356);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 23);
            this._btnCancel.TabIndex = 6;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // PdfPasswordSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 391);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PdfPasswordSettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Pdf Security Settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox _userPasswordText1;
        private System.Windows.Forms.CheckBox _enableUserPasswordCheckBox;
        private System.Windows.Forms.TextBox _userPasswordText2;
        private System.Windows.Forms.CheckBox _allowHighQualityPrintingCheck;
        private System.Windows.Forms.CheckBox _allowDocumentAssemblyCheck;
        private System.Windows.Forms.CheckBox _allowAccessibilityCheck;
        private System.Windows.Forms.CheckBox _allowFillInFormFieldsCheck;
        private System.Windows.Forms.CheckBox _allowAddOrModifyAnnotationsCheck;
        private System.Windows.Forms.CheckBox _allowCopyAndExtractionCheck;
        private System.Windows.Forms.CheckBox _allowDocumentModificationsCheck;
        private System.Windows.Forms.CheckBox _allowLowQualityPrintCheck;
        private System.Windows.Forms.TextBox _ownerPasswordText2;
        private System.Windows.Forms.TextBox _ownerPasswordText1;
        private System.Windows.Forms.CheckBox _enableOwnerPasswordCheckBox;
        private System.Windows.Forms.Button _btnOk;
        private System.Windows.Forms.Button _btnCancel;
    }
}