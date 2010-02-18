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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.GroupBox groupBox2;
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
            this._displayPasswordsCheck = new System.Windows.Forms.CheckBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label2 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this._userPasswordText2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(this._userPasswordText1);
            groupBox1.Controls.Add(this._enableUserPasswordCheckBox);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(443, 76);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "User password";
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
            this._enableUserPasswordCheckBox.Size = new System.Drawing.Size(130, 17);
            this._enableUserPasswordCheckBox.TabIndex = 0;
            this._enableUserPasswordCheckBox.Text = "Enable user password";
            this._enableUserPasswordCheckBox.UseVisualStyleBackColor = true;
            this._enableUserPasswordCheckBox.Click += new System.EventHandler(this.HandleEnablePasswordCheckboxClicked);
            // 
            // groupBox2
            // 
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
            groupBox2.Size = new System.Drawing.Size(443, 256);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Owner password and permissions to set";
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
            this._allowFillInFormFieldsCheck.Size = new System.Drawing.Size(151, 17);
            this._allowFillInFormFieldsCheck.TabIndex = 8;
            this._allowFillInFormFieldsCheck.Text = "Filling in existing form fields";
            this._allowFillInFormFieldsCheck.UseVisualStyleBackColor = true;
            // 
            // _allowAddOrModifyAnnotationsCheck
            // 
            this._allowAddOrModifyAnnotationsCheck.AutoSize = true;
            this._allowAddOrModifyAnnotationsCheck.Location = new System.Drawing.Point(25, 137);
            this._allowAddOrModifyAnnotationsCheck.Name = "_allowAddOrModifyAnnotationsCheck";
            this._allowAddOrModifyAnnotationsCheck.Size = new System.Drawing.Size(333, 17);
            this._allowAddOrModifyAnnotationsCheck.TabIndex = 7;
            this._allowAddOrModifyAnnotationsCheck.Text = "Addition or modification of text annotations and filling in form fields";
            this._allowAddOrModifyAnnotationsCheck.UseVisualStyleBackColor = true;
            // 
            // _allowCopyAndExtractionCheck
            // 
            this._allowCopyAndExtractionCheck.AutoSize = true;
            this._allowCopyAndExtractionCheck.Location = new System.Drawing.Point(25, 114);
            this._allowCopyAndExtractionCheck.Name = "_allowCopyAndExtractionCheck";
            this._allowCopyAndExtractionCheck.Size = new System.Drawing.Size(412, 17);
            this._allowCopyAndExtractionCheck.TabIndex = 6;
            this._allowCopyAndExtractionCheck.Text = "Copying / extraction of text and graphics including support to users with disabil" +
                "ities";
            this._allowCopyAndExtractionCheck.UseVisualStyleBackColor = true;
            // 
            // _allowDocumentModificationsCheck
            // 
            this._allowDocumentModificationsCheck.AutoSize = true;
            this._allowDocumentModificationsCheck.Location = new System.Drawing.Point(25, 91);
            this._allowDocumentModificationsCheck.Name = "_allowDocumentModificationsCheck";
            this._allowDocumentModificationsCheck.Size = new System.Drawing.Size(139, 17);
            this._allowDocumentModificationsCheck.TabIndex = 5;
            this._allowDocumentModificationsCheck.Text = "Document modifications";
            this._allowDocumentModificationsCheck.UseVisualStyleBackColor = true;
            // 
            // _allowLowQualityPrintCheck
            // 
            this._allowLowQualityPrintCheck.AutoSize = true;
            this._allowLowQualityPrintCheck.Location = new System.Drawing.Point(25, 68);
            this._allowLowQualityPrintCheck.Name = "_allowLowQualityPrintCheck";
            this._allowLowQualityPrintCheck.Size = new System.Drawing.Size(149, 17);
            this._allowLowQualityPrintCheck.TabIndex = 4;
            this._allowLowQualityPrintCheck.Text = "Printing in low level quality";
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
            this._enableOwnerPasswordCheckBox.Size = new System.Drawing.Size(139, 17);
            this._enableOwnerPasswordCheckBox.TabIndex = 0;
            this._enableOwnerPasswordCheckBox.Text = "Enable owner password";
            this._enableOwnerPasswordCheckBox.UseVisualStyleBackColor = true;
            this._enableOwnerPasswordCheckBox.Click += new System.EventHandler(this.HandleEnablePasswordCheckboxClicked);
            // 
            // _btnOk
            // 
            this._btnOk.Location = new System.Drawing.Point(299, 356);
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
            this._btnCancel.Location = new System.Drawing.Point(380, 356);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 23);
            this._btnCancel.TabIndex = 6;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _displayPasswordsCheck
            // 
            this._displayPasswordsCheck.AutoSize = true;
            this._displayPasswordsCheck.Location = new System.Drawing.Point(18, 360);
            this._displayPasswordsCheck.Name = "_displayPasswordsCheck";
            this._displayPasswordsCheck.Size = new System.Drawing.Size(113, 17);
            this._displayPasswordsCheck.TabIndex = 7;
            this._displayPasswordsCheck.Text = "Display passwords";
            this._displayPasswordsCheck.UseVisualStyleBackColor = true;
            this._displayPasswordsCheck.Click += new System.EventHandler(this.HandleDisplayPasswordsChecked);
            // 
            // PdfPasswordSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 391);
            this.Controls.Add(this._displayPasswordsCheck);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PdfPasswordSettingsDialog";
            this.Text = "PdfPasswordSettingsDialog";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.CheckBox _displayPasswordsCheck;
    }
}