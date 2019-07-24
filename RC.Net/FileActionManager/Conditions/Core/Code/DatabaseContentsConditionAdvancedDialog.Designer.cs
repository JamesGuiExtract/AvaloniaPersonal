namespace Extract.FileActionManager.Conditions
{
    partial class DatabaseContentsConditionAdvancedDialog
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
            System.Windows.Forms.GroupBox groupBox1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseContentsConditionAdvancedDialog));
            this._dataFileLabel = new System.Windows.Forms.Label();
            this._abortOnErrorRadioButton = new System.Windows.Forms.RadioButton();
            this._logErrorRadioButton = new System.Windows.Forms.RadioButton();
            this._ignoreErrorRadioButton = new System.Windows.Forms.RadioButton();
            this._browseButton = new Extract.Utilities.Forms.BrowseButton();
            this._dataFileTextBox = new System.Windows.Forms.TextBox();
            this._pathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _dataFileLabel
            // 
            this._dataFileLabel.AutoSize = true;
            this._dataFileLabel.Location = new System.Drawing.Point(12, 9);
            this._dataFileLabel.Name = "_dataFileLabel";
            this._dataFileLabel.Size = new System.Drawing.Size(71, 13);
            this._dataFileLabel.TabIndex = 0;
            this._dataFileLabel.Text = "VOA filename";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._abortOnErrorRadioButton);
            groupBox1.Controls.Add(this._logErrorRadioButton);
            groupBox1.Controls.Add(this._ignoreErrorRadioButton);
            groupBox1.Location = new System.Drawing.Point(12, 53);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(423, 91);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "If executing the query results in an error";
            // 
            // _abortOnErrorRadioButton
            // 
            this._abortOnErrorRadioButton.AutoSize = true;
            this._abortOnErrorRadioButton.Location = new System.Drawing.Point(7, 66);
            this._abortOnErrorRadioButton.Name = "_abortOnErrorRadioButton";
            this._abortOnErrorRadioButton.Size = new System.Drawing.Size(133, 17);
            this._abortOnErrorRadioButton.TabIndex = 2;
            this._abortOnErrorRadioButton.TabStop = true;
            this._abortOnErrorRadioButton.Text = "Log the error and abort";
            this._abortOnErrorRadioButton.UseVisualStyleBackColor = true;
            // 
            // _logErrorRadioButton
            // 
            this._logErrorRadioButton.AutoSize = true;
            this._logErrorRadioButton.Location = new System.Drawing.Point(7, 43);
            this._logErrorRadioButton.Name = "_logErrorRadioButton";
            this._logErrorRadioButton.Size = new System.Drawing.Size(261, 17);
            this._logErrorRadioButton.TabIndex = 1;
            this._logErrorRadioButton.TabStop = true;
            this._logErrorRadioButton.Text = "Log the error and treat the condition as unsatisfied";
            this._logErrorRadioButton.UseVisualStyleBackColor = true;
            // 
            // _ignoreErrorRadioButton
            // 
            this._ignoreErrorRadioButton.AutoSize = true;
            this._ignoreErrorRadioButton.Location = new System.Drawing.Point(7, 20);
            this._ignoreErrorRadioButton.Name = "_ignoreErrorRadioButton";
            this._ignoreErrorRadioButton.Size = new System.Drawing.Size(273, 17);
            this._ignoreErrorRadioButton.TabIndex = 0;
            this._ignoreErrorRadioButton.TabStop = true;
            this._ignoreErrorRadioButton.Text = "Ignore the error and treat the condition as unsatisfied";
            this._ignoreErrorRadioButton.UseVisualStyleBackColor = true;
            // 
            // _browseButton
            // 
            this._browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._browseButton.EnsureFileExists = false;
            this._browseButton.EnsurePathExists = false;
            this._browseButton.Location = new System.Drawing.Point(408, 24);
            this._browseButton.Name = "_browseButton";
            this._browseButton.Size = new System.Drawing.Size(27, 23);
            this._browseButton.TabIndex = 3;
            this._browseButton.Text = "...";
            this._browseButton.TextControl = this._dataFileTextBox;
            this._browseButton.UseVisualStyleBackColor = true;
            // 
            // _dataFileTextBox
            // 
            this._dataFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataFileTextBox.Location = new System.Drawing.Point(12, 26);
            this._dataFileTextBox.Name = "_dataFileTextBox";
            this._dataFileTextBox.Size = new System.Drawing.Size(366, 20);
            this._dataFileTextBox.TabIndex = 1;
            // 
            // _pathTagsButton
            // 
            this._pathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_pathTagsButton.Image")));
            this._pathTagsButton.Location = new System.Drawing.Point(384, 24);
            this._pathTagsButton.Name = "_pathTagsButton";
            this._pathTagsButton.Size = new System.Drawing.Size(18, 23);
            this._pathTagsButton.TabIndex = 2;
            this._pathTagsButton.TextControl = this._dataFileTextBox;
            this._pathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(279, 154);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 5;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(360, 154);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 6;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // DatabaseContentsConditionAdvancedDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(447, 189);
            this.Controls.Add(this._browseButton);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._pathTagsButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._dataFileLabel);
            this.Controls.Add(this._dataFileTextBox);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(350, 146);
            this.Name = "DatabaseContentsConditionAdvancedDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Database contents condition advanced settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _dataFileTextBox;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _pathTagsButton;
        private Utilities.Forms.BrowseButton _browseButton;
        private System.Windows.Forms.RadioButton _abortOnErrorRadioButton;
        private System.Windows.Forms.RadioButton _logErrorRadioButton;
        private System.Windows.Forms.RadioButton _ignoreErrorRadioButton;
        private System.Windows.Forms.Label _dataFileLabel;
    }
}