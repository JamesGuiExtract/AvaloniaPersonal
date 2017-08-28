namespace Extract.AttributeFinder.Rules
{
    partial class SNERFinderSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SNERFinderSettingsDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this._typesToReturnTextBox = new System.Windows.Forms.TextBox();
            this._classifierPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._classifierPathTextBox = new System.Windows.Forms.TextBox();
            this._classifierPathTagButton = new Extract.Utilities.Forms.PathTagsButton();
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
            this.label1.Size = new System.Drawing.Size(374, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path to classifier file (e.g., <RSDFileDir>\\english.muc.7class.distsim.crf.ser.gz" +
    "):";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this._typesToReturnTextBox);
            this.groupBox1.Controls.Add(this._classifierPathBrowseButton);
            this.groupBox1.Controls.Add(this._classifierPathTagButton);
            this.groupBox1.Controls.Add(this._classifierPathTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(580, 120);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(439, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Entity types to return: (Empty for all types, else CSV, e.g., Location, Person, O" +
    "rganization, ...)";
            // 
            // _typesToReturnTextBox
            // 
            this._typesToReturnTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._typesToReturnTextBox.Location = new System.Drawing.Point(13, 85);
            this._typesToReturnTextBox.Name = "_typesToReturnTextBox";
            this._typesToReturnTextBox.Size = new System.Drawing.Size(556, 20);
            this._typesToReturnTextBox.TabIndex = 3;
            // 
            // _classifierPathBrowseButton
            // 
            this._classifierPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._classifierPathBrowseButton.Location = new System.Drawing.Point(545, 32);
            this._classifierPathBrowseButton.Name = "_classifierPathBrowseButton";
            this._classifierPathBrowseButton.Size = new System.Drawing.Size(24, 22);
            this._classifierPathBrowseButton.TabIndex = 2;
            this._classifierPathBrowseButton.Text = "...";
            this._classifierPathBrowseButton.TextControl = this._classifierPathTextBox;
            this._classifierPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _classifierPathTextBox
            // 
            this._classifierPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._classifierPathTextBox.Location = new System.Drawing.Point(13, 33);
            this._classifierPathTextBox.Name = "_classifierPathTextBox";
            this._classifierPathTextBox.Size = new System.Drawing.Size(496, 20);
            this._classifierPathTextBox.TabIndex = 0;
            // 
            // _classifierPathTagButton
            // 
            this._classifierPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._classifierPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_classifierPathTagButton.Image")));
            this._classifierPathTagButton.Location = new System.Drawing.Point(515, 32);
            this._classifierPathTagButton.Name = "_classifierPathTagButton";
            this._classifierPathTagButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._classifierPathTagButton.Size = new System.Drawing.Size(24, 22);
            this._classifierPathTagButton.TabIndex = 1;
            this._classifierPathTagButton.TextControl = this._classifierPathTextBox;
            this._classifierPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(517, 142);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(436, 142);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 1;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // SNERFinderSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(604, 177);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(620, 216);
            this.Name = "SNERFinderSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SNER finder settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private Utilities.Forms.PathTagsButton _classifierPathTagButton;
        private System.Windows.Forms.TextBox _classifierPathTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _typesToReturnTextBox;
        private Utilities.Forms.BrowseButton _classifierPathBrowseButton;
    }
}
