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
            this._templatesDirBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._templatesDirTextBox = new System.Windows.Forms.TextBox();
            this._templatesDirTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._redactionPredictorOptionsTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Templates dir:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._redactionPredictorOptionsTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this._templatesDirBrowseButton);
            this.groupBox1.Controls.Add(this._templatesDirTagButton);
            this.groupBox1.Controls.Add(this._templatesDirTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(520, 104);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // _templatesDirBrowseButton
            // 
            this._templatesDirBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._templatesDirBrowseButton.EnsureFileExists = false;
            this._templatesDirBrowseButton.EnsurePathExists = false;
            this._templatesDirBrowseButton.FolderBrowser = true;
            this._templatesDirBrowseButton.Location = new System.Drawing.Point(483, 31);
            this._templatesDirBrowseButton.Name = "_templatesDirBrowseButton";
            this._templatesDirBrowseButton.Size = new System.Drawing.Size(24, 22);
            this._templatesDirBrowseButton.TabIndex = 8;
            this._templatesDirBrowseButton.Text = "...";
            this._templatesDirBrowseButton.TextControl = this._templatesDirTextBox;
            this._templatesDirBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _templatesDirTextBox
            // 
            this._templatesDirTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._templatesDirTextBox.Location = new System.Drawing.Point(13, 32);
            this._templatesDirTextBox.Name = "_templatesDirTextBox";
            this._templatesDirTextBox.Size = new System.Drawing.Size(434, 20);
            this._templatesDirTextBox.TabIndex = 6;
            // 
            // _templatesDirTagButton
            // 
            this._templatesDirTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._templatesDirTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_templatesDirTagButton.Image")));
            this._templatesDirTagButton.Location = new System.Drawing.Point(453, 31);
            this._templatesDirTagButton.Name = "_templatesDirTagButton";
            this._templatesDirTagButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._templatesDirTagButton.Size = new System.Drawing.Size(24, 22);
            this._templatesDirTagButton.TabIndex = 7;
            this._templatesDirTagButton.TextControl = this._templatesDirTextBox;
            this._templatesDirTagButton.UseVisualStyleBackColor = true;
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
        private Utilities.Forms.PathTagsButton _templatesDirTagButton;
        private System.Windows.Forms.TextBox _templatesDirTextBox;
        private Utilities.Forms.BrowseButton _templatesDirBrowseButton;
        private System.Windows.Forms.TextBox _redactionPredictorOptionsTextBox;
        private System.Windows.Forms.Label label2;
    }
}
