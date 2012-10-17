namespace Extract.AttributeFinder.Rules
{
    partial class DataEntryPreloaderSettingsDialog
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
            System.Windows.Forms.GroupBox groupBox3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataEntryPreloaderSettingsDialog));
            this._configFileNameTextBox = new System.Windows.Forms.TextBox();
            this._configFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._configFileNamePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            groupBox3 = new System.Windows.Forms.GroupBox();
            groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox3.Controls.Add(this._configFileNamePathTagsButton);
            groupBox3.Controls.Add(this._configFileNameBrowseButton);
            groupBox3.Controls.Add(this._configFileNameTextBox);
            groupBox3.Location = new System.Drawing.Point(12, 12);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(416, 52);
            groupBox3.TabIndex = 6;
            groupBox3.TabStop = false;
            groupBox3.Text = "Configuration file name";
            // 
            // _configFileNameTextBox
            // 
            this._configFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._configFileNameTextBox.HideSelection = false;
            this._configFileNameTextBox.Location = new System.Drawing.Point(6, 20);
            this._configFileNameTextBox.Name = "_configFileNameTextBox";
            this._configFileNameTextBox.Size = new System.Drawing.Size(347, 20);
            this._configFileNameTextBox.TabIndex = 1;
            // 
            // _configFileNameBrowseButton
            // 
            this._configFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._configFileNameBrowseButton.Location = new System.Drawing.Point(383, 20);
            this._configFileNameBrowseButton.Name = "_configFileNameBrowseButton";
            this._configFileNameBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._configFileNameBrowseButton.TabIndex = 3;
            this._configFileNameBrowseButton.Text = "...";
            this._configFileNameBrowseButton.TextControl = this._configFileNameTextBox;
            this._configFileNameBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _configFileNamePathTagsButton
            // 
            this._configFileNamePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._configFileNamePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_configFileNamePathTagsButton.Image")));
            this._configFileNamePathTagsButton.Location = new System.Drawing.Point(359, 20);
            this._configFileNamePathTagsButton.Name = "_configFileNamePathTagsButton";
            this._configFileNamePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._configFileNamePathTagsButton.TabIndex = 2;
            this._configFileNamePathTagsButton.TextControl = this._configFileNameTextBox;
            this._configFileNamePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._okButton.Location = new System.Drawing.Point(272, 70);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 7;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(353, 70);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 8;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // DataEntryPreloaderSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(440, 104);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(groupBox3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DataEntryPreloaderSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Data entry preloader settings";
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Utilities.Forms.PathTagsButton _configFileNamePathTagsButton;
        private System.Windows.Forms.TextBox _configFileNameTextBox;
        private Utilities.Forms.BrowseButton _configFileNameBrowseButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
    }
}