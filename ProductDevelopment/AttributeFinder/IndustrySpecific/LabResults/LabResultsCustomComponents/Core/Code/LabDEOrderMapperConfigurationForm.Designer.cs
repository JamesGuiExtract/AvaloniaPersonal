namespace Extract.LabResultsCustomComponents
{
    partial class LabDEOrderMapperConfigurationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LabDEOrderMapperConfigurationForm));
            this.label1 = new System.Windows.Forms.Label();
            this._textDatabaseFile = new System.Windows.Forms.TextBox();
            this._buttonTags = new Extract.Utilities.Forms.PathTagsButton();
            this._buttonBrowse = new Extract.Utilities.Forms.BrowseButton();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._checkRequireMandatoryTests = new System.Windows.Forms.CheckBox();
            this._checkUseFilledRequirement = new System.Windows.Forms.CheckBox();
            this._checkUseOutstandingOrders = new System.Windows.Forms.CheckBox();
            this._checkEliminateDuplicateTestSubAttributes = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Database file";
            // 
            // _textDatabaseFile
            // 
            this._textDatabaseFile.Location = new System.Drawing.Point(15, 25);
            this._textDatabaseFile.Name = "_textDatabaseFile";
            this._textDatabaseFile.Size = new System.Drawing.Size(408, 20);
            this._textDatabaseFile.TabIndex = 1;
            // 
            // _buttonTags
            // 
            this._buttonTags.Image = ((System.Drawing.Image)(resources.GetObject("_buttonTags.Image")));
            this._buttonTags.Location = new System.Drawing.Point(429, 24);
            this._buttonTags.Name = "_buttonTags";
            this._buttonTags.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._buttonTags.Size = new System.Drawing.Size(22, 22);
            this._buttonTags.TabIndex = 2;
            this._buttonTags.TagSelected += new System.EventHandler<Extract.Utilities.Forms.TagSelectedEventArgs>(this.HandlePathTagsButtonSelected);
            // 
            // _buttonBrowse
            // 
            this._buttonBrowse.FileFilter = "SQL Compact Database File (*.sdf)|*.sdf||";
            this._buttonBrowse.Location = new System.Drawing.Point(457, 24);
            this._buttonBrowse.Name = "_buttonBrowse";
            this._buttonBrowse.Size = new System.Drawing.Size(27, 22);
            this._buttonBrowse.TabIndex = 3;
            this._buttonBrowse.Text = "...";
            this._buttonBrowse.TextControl = this._textDatabaseFile;
            this._buttonBrowse.UseVisualStyleBackColor = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.Location = new System.Drawing.Point(328, 173);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 8;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.HandleOkButtonClicked);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(409, 173);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 9;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            this._buttonCancel.Click += new System.EventHandler(this.HandleCancelClicked);
            // 
            // _checkRequireMandatoryTests
            // 
            this._checkRequireMandatoryTests.AutoSize = true;
            this._checkRequireMandatoryTests.Location = new System.Drawing.Point(15, 68);
            this._checkRequireMandatoryTests.Name = "_checkRequireMandatoryTests";
            this._checkRequireMandatoryTests.Size = new System.Drawing.Size(317, 17);
            this._checkRequireMandatoryTests.TabIndex = 4;
            this._checkRequireMandatoryTests.Text = "Require mandatory tests during second pass of order mapping";
            this._checkRequireMandatoryTests.UseVisualStyleBackColor = true;
            // 
            // _checkUseFilledRequirement
            // 
            this._checkUseFilledRequirement.AutoSize = true;
            this._checkUseFilledRequirement.Location = new System.Drawing.Point(15, 91);
            this._checkUseFilledRequirement.Name = "_checkUseFilledRequirement";
            this._checkUseFilledRequirement.Size = new System.Drawing.Size(253, 17);
            this._checkUseFilledRequirement.TabIndex = 5;
            this._checkUseFilledRequirement.Text = "Use FilledRequirement column to validate orders";
            // 
            // _checkUseOutstandingOrders
            // 
            this._checkUseOutstandingOrders.AutoSize = true;
            this._checkUseOutstandingOrders.Location = new System.Drawing.Point(15, 114);
            this._checkUseOutstandingOrders.Name = "_checkUseOutstandingOrders";
            this._checkUseOutstandingOrders.Size = new System.Drawing.Size(433, 17);
            this._checkUseOutstandingOrders.TabIndex = 6;
            this._checkUseOutstandingOrders.Text = "Prefer known, outstanding orders (code matches a Test/OutstandingOrderCode value)" +
    "";
            this._checkUseOutstandingOrders.UseVisualStyleBackColor = true;
            // 
            // _checkEliminateDuplicateTestSubAttributes
            // 
            this._checkEliminateDuplicateTestSubAttributes.AutoSize = true;
            this._checkEliminateDuplicateTestSubAttributes.Location = new System.Drawing.Point(15, 137);
            this._checkEliminateDuplicateTestSubAttributes.Name = "_checkEliminateDuplicateTestSubAttributes";
            this._checkEliminateDuplicateTestSubAttributes.Size = new System.Drawing.Size(247, 17);
            this._checkEliminateDuplicateTestSubAttributes.TabIndex = 7;
            this._checkEliminateDuplicateTestSubAttributes.Text = "Eliminate duplicate sub-attributes after mapping";
            this._checkEliminateDuplicateTestSubAttributes.UseVisualStyleBackColor = true;
            // 
            // LabDEOrderMapperConfigurationForm
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(496, 209);
            this.Controls.Add(this._checkEliminateDuplicateTestSubAttributes);
            this.Controls.Add(this._checkUseFilledRequirement);
            this.Controls.Add(this._checkUseOutstandingOrders);
            this.Controls.Add(this._checkRequireMandatoryTests);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonBrowse);
            this.Controls.Add(this._textDatabaseFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._buttonTags);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LabDEOrderMapperConfigurationForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Lab DE Order Mapping Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _textDatabaseFile;
        private Extract.Utilities.Forms.PathTagsButton _buttonTags;
        private Extract.Utilities.Forms.BrowseButton _buttonBrowse;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.CheckBox _checkRequireMandatoryTests;
        private System.Windows.Forms.CheckBox _checkUseFilledRequirement;
        private System.Windows.Forms.CheckBox _checkUseOutstandingOrders;
        private System.Windows.Forms.CheckBox _checkEliminateDuplicateTestSubAttributes;
    }
}
