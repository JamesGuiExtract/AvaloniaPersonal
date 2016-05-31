namespace Extract.AttributeFinder.Rules
{
    partial class LearningMachineOutputHandlerSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LearningMachineOutputHandlerSettingsDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._preserveInputAttributesCheckBox = new System.Windows.Forms.CheckBox();
            this._savedMachinePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._savedMachineTextBox = new System.Windows.Forms.TextBox();
            this._savedMachineBrowseButton = new Extract.Utilities.Forms.BrowseButton();
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
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path to saved machine:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._preserveInputAttributesCheckBox);
            this.groupBox1.Controls.Add(this._savedMachinePathTagsButton);
            this.groupBox1.Controls.Add(this._savedMachineBrowseButton);
            this.groupBox1.Controls.Add(this._savedMachineTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(356, 89);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // _preserveInputAttributesCheckBox
            // 
            this._preserveInputAttributesCheckBox.AutoSize = true;
            this._preserveInputAttributesCheckBox.Location = new System.Drawing.Point(13, 58);
            this._preserveInputAttributesCheckBox.Name = "_preserveInputAttributesCheckBox";
            this._preserveInputAttributesCheckBox.Size = new System.Drawing.Size(140, 17);
            this._preserveInputAttributesCheckBox.TabIndex = 3;
            this._preserveInputAttributesCheckBox.Text = "Preserve input attributes";
            this._preserveInputAttributesCheckBox.UseVisualStyleBackColor = true;
            // 
            // _savedMachinePathTagsButton
            // 
            this._savedMachinePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_savedMachinePathTagsButton.Image")));
            this._savedMachinePathTagsButton.Location = new System.Drawing.Point(300, 31);
            this._savedMachinePathTagsButton.Name = "_savedMachinePathTagsButton";
            this._savedMachinePathTagsButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._savedMachinePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._savedMachinePathTagsButton.TabIndex = 1;
            this._savedMachinePathTagsButton.TextControl = this._savedMachineTextBox;
            this._savedMachinePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _savedMachineTextBox
            // 
            this._savedMachineTextBox.Location = new System.Drawing.Point(13, 32);
            this._savedMachineTextBox.Name = "_savedMachineTextBox";
            this._savedMachineTextBox.Size = new System.Drawing.Size(281, 20);
            this._savedMachineTextBox.TabIndex = 0;
            // 
            // _savedMachineBrowseButton
            // 
            this._savedMachineBrowseButton.FileFilter = "Learning machine|*.lm|All files|*.*";
            this._savedMachineBrowseButton.Location = new System.Drawing.Point(324, 31);
            this._savedMachineBrowseButton.Name = "_savedMachineBrowseButton";
            this._savedMachineBrowseButton.Size = new System.Drawing.Size(25, 20);
            this._savedMachineBrowseButton.TabIndex = 2;
            this._savedMachineBrowseButton.Text = "...";
            this._savedMachineBrowseButton.TextControl = this._savedMachineTextBox;
            this._savedMachineBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(293, 111);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(212, 111);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 1;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // LearningMachineOutputHandlerSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(380, 146);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LearningMachineOutputHandlerSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Learning machine output handler settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox _savedMachineTextBox;
        private System.Windows.Forms.CheckBox _preserveInputAttributesCheckBox;
        private Utilities.Forms.PathTagsButton _savedMachinePathTagsButton;
        private Utilities.Forms.BrowseButton _savedMachineBrowseButton;
    }
}
