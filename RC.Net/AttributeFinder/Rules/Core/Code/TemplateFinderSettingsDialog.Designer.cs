namespace Extract.AttributeFinder.Rules
{
    partial class TemplateFinderSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TemplateFinderSettingsDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._redactionPredictorOptionsTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._templateLibraryBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._templateLibraryTextBox = new System.Windows.Forms.TextBox();
            this._templateLibraryTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Template library:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._redactionPredictorOptionsTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this._templateLibraryBrowseButton);
            this.groupBox1.Controls.Add(this._templateLibraryTagButton);
            this.groupBox1.Controls.Add(this._templateLibraryTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(520, 104);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // _redactionPredictorOptionsTextBox
            // 
            this._redactionPredictorOptionsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._redactionPredictorOptionsTextBox.Location = new System.Drawing.Point(13, 71);
            this._redactionPredictorOptionsTextBox.Name = "_redactionPredictorOptionsTextBox";
            this._redactionPredictorOptionsTextBox.Size = new System.Drawing.Size(494, 20);
            this._redactionPredictorOptionsTextBox.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(208, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Redaction predictor command-line options:";
            // 
            // _templateLibraryBrowseButton
            // 
            this._templateLibraryBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._templateLibraryBrowseButton.EnsureFileExists = false;
            this._templateLibraryBrowseButton.EnsurePathExists = false;
            this._templateLibraryBrowseButton.FolderBrowser = true;
            this._templateLibraryBrowseButton.Location = new System.Drawing.Point(483, 31);
            this._templateLibraryBrowseButton.Name = "_templateLibraryBrowseButton";
            this._templateLibraryBrowseButton.Size = new System.Drawing.Size(24, 22);
            this._templateLibraryBrowseButton.TabIndex = 8;
            this._templateLibraryBrowseButton.Text = "...";
            this._templateLibraryBrowseButton.TextControl = this._templateLibraryTextBox;
            this._templateLibraryBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _templateLibraryTextBox
            // 
            this._templateLibraryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._templateLibraryTextBox.Location = new System.Drawing.Point(13, 32);
            this._templateLibraryTextBox.Name = "_templateLibraryTextBox";
            this._templateLibraryTextBox.Size = new System.Drawing.Size(434, 20);
            this._templateLibraryTextBox.TabIndex = 6;
            // 
            // _templateLibraryTagButton
            // 
            this._templateLibraryTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._templateLibraryTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_templateLibraryTagButton.Image")));
            this._templateLibraryTagButton.Location = new System.Drawing.Point(453, 31);
            this._templateLibraryTagButton.Name = "_templateLibraryTagButton";
            this._templateLibraryTagButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._templateLibraryTagButton.Size = new System.Drawing.Size(24, 22);
            this._templateLibraryTagButton.TabIndex = 7;
            this._templateLibraryTagButton.TextControl = this._templateLibraryTextBox;
            this._templateLibraryTagButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(457, 126);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(376, 126);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 1;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // TemplateFinderSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(544, 161);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 165);
            this.Name = "TemplateFinderSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Template finder settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private Utilities.Forms.PathTagsButton _templateLibraryTagButton;
        private System.Windows.Forms.TextBox _templateLibraryTextBox;
        private Utilities.Forms.BrowseButton _templateLibraryBrowseButton;
        private System.Windows.Forms.TextBox _redactionPredictorOptionsTextBox;
        private System.Windows.Forms.Label label2;
    }
}
