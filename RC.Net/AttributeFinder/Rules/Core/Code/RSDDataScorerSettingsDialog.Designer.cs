namespace Extract.AttributeFinder.Rules
{
    partial class RSDDataScorerSettingsDialog
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
            System.Windows.Forms.GroupBox groupBox1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RSDDataScorerSettingsDialog));
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._rsdFileNameTextBox = new System.Windows.Forms.TextBox();
            this._scoreExpressionTextBox = new System.Windows.Forms.TextBox();
            this._scoreExpressionInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._rsdFileNamePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._rsdFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            groupBox3 = new System.Windows.Forms.GroupBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox3.SuspendLayout();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(287, 153);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 3;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(368, 153);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 4;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _rsdFileNameTextBox
            // 
            this._rsdFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._rsdFileNameTextBox.HideSelection = false;
            this._rsdFileNameTextBox.Location = new System.Drawing.Point(6, 20);
            this._rsdFileNameTextBox.Name = "_rsdFileNameTextBox";
            this._rsdFileNameTextBox.Size = new System.Drawing.Size(362, 20);
            this._rsdFileNameTextBox.TabIndex = 1;
            // 
            // groupBox3
            // 
            groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox3.Controls.Add(this._rsdFileNamePathTagsButton);
            groupBox3.Controls.Add(this._rsdFileNameBrowseButton);
            groupBox3.Controls.Add(this._rsdFileNameTextBox);
            groupBox3.Location = new System.Drawing.Point(12, 12);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(431, 52);
            groupBox3.TabIndex = 5;
            groupBox3.TabStop = false;
            groupBox3.Text = "RSD file name";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this._scoreExpressionInfoTip);
            groupBox1.Controls.Add(this._scoreExpressionTextBox);
            groupBox1.Location = new System.Drawing.Point(12, 71);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(431, 74);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "Score generation expression";
            // 
            // _scoreExpressionTextBox
            // 
            this._scoreExpressionTextBox.Location = new System.Drawing.Point(6, 19);
            this._scoreExpressionTextBox.Multiline = true;
            this._scoreExpressionTextBox.Name = "_scoreExpressionTextBox";
            this._scoreExpressionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._scoreExpressionTextBox.Size = new System.Drawing.Size(397, 49);
            this._scoreExpressionTextBox.TabIndex = 0;
            // 
            // _scoreExpressionInfoTip
            // 
            this._scoreExpressionInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._scoreExpressionInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_scoreExpressionInfoTip.BackgroundImage")));
            this._scoreExpressionInfoTip.Location = new System.Drawing.Point(409, 19);
            this._scoreExpressionInfoTip.Name = "_scoreExpressionInfoTip";
            this._scoreExpressionInfoTip.Size = new System.Drawing.Size(16, 16);
            this._scoreExpressionInfoTip.TabIndex = 1;
            this._scoreExpressionInfoTip.TipText = resources.GetString("_scoreExpressionInfoTip.TipText");
            // 
            // _rsdFileNamePathTagsButton
            // 
            this._rsdFileNamePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._rsdFileNamePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_rsdFileNamePathTagsButton.Image")));
            this._rsdFileNamePathTagsButton.Location = new System.Drawing.Point(374, 20);
            this._rsdFileNamePathTagsButton.Name = "_rsdFileNamePathTagsButton";
            this._rsdFileNamePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._rsdFileNamePathTagsButton.TabIndex = 2;
            this._rsdFileNamePathTagsButton.TextControl = this._rsdFileNameTextBox;
            this._rsdFileNamePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _rsdFileNameBrowseButton
            // 
            this._rsdFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._rsdFileNameBrowseButton.Location = new System.Drawing.Point(398, 20);
            this._rsdFileNameBrowseButton.Name = "_rsdFileNameBrowseButton";
            this._rsdFileNameBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._rsdFileNameBrowseButton.TabIndex = 3;
            this._rsdFileNameBrowseButton.Text = "...";
            this._rsdFileNameBrowseButton.TextControl = this._rsdFileNameTextBox;
            this._rsdFileNameBrowseButton.UseVisualStyleBackColor = true;
            // 
            // RSDDataScorerSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(455, 188);
            this.Controls.Add(groupBox1);
            this.Controls.Add(groupBox3);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RSDDataScorerSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RSD data scorer settings";
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private Utilities.Forms.PathTagsButton _rsdFileNamePathTagsButton;
        private System.Windows.Forms.TextBox _rsdFileNameTextBox;
        private Utilities.Forms.BrowseButton _rsdFileNameBrowseButton;
        private System.Windows.Forms.TextBox _scoreExpressionTextBox;
        private Utilities.Forms.InfoTip _scoreExpressionInfoTip;
    }
}