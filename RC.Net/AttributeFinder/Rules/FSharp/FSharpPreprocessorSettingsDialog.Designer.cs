namespace Extract.AttributeFinder.Rules
{
    partial class FSharpPreprocessorSettingsDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FSharpPreprocessorSettingsDialog));
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this._pathToScriptFileTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._functionNameTextBox = new System.Windows.Forms.TextBox();
            this._pathToScriptFilePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._pathToScriptFilePathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._collectibleCheckBox = new System.Windows.Forms.CheckBox();
            this._collectibleInfoTip = new Extract.Utilities.Forms.InfoTip();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(342, 135);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 9;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(261, 135);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 8;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path to script file:";
            // 
            // _pathToScriptFileTextBox
            // 
            this._pathToScriptFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._pathToScriptFileTextBox.Location = new System.Drawing.Point(15, 25);
            this._pathToScriptFileTextBox.Name = "_pathToScriptFileTextBox";
            this._pathToScriptFileTextBox.Size = new System.Drawing.Size(344, 20);
            this._pathToScriptFileTextBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Function name:";
            // 
            // _functionNameTextBox
            // 
            this._functionNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._functionNameTextBox.Location = new System.Drawing.Point(15, 76);
            this._functionNameTextBox.Name = "_functionNameTextBox";
            this._functionNameTextBox.Size = new System.Drawing.Size(344, 20);
            this._functionNameTextBox.TabIndex = 5;
            // 
            // _pathToScriptFilePathTagsButton
            // 
            this._pathToScriptFilePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pathToScriptFilePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_pathToScriptFilePathTagsButton.Image")));
            this._pathToScriptFilePathTagsButton.Location = new System.Drawing.Point(365, 24);
            this._pathToScriptFilePathTagsButton.Name = "_pathToScriptFilePathTagsButton";
            this._pathToScriptFilePathTagsButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._pathToScriptFilePathTagsButton.Size = new System.Drawing.Size(18, 22);
            this._pathToScriptFilePathTagsButton.TabIndex = 2;
            this._pathToScriptFilePathTagsButton.TextControl = this._pathToScriptFileTextBox;
            this._pathToScriptFilePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _pathToScriptFilePathBrowseButton
            // 
            this._pathToScriptFilePathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pathToScriptFilePathBrowseButton.EnsureFileExists = true;
            this._pathToScriptFilePathBrowseButton.EnsurePathExists = true;
            this._pathToScriptFilePathBrowseButton.Location = new System.Drawing.Point(389, 24);
            this._pathToScriptFilePathBrowseButton.Name = "_pathToScriptFilePathBrowseButton";
            this._pathToScriptFilePathBrowseButton.Size = new System.Drawing.Size(24, 22);
            this._pathToScriptFilePathBrowseButton.TabIndex = 3;
            this._pathToScriptFilePathBrowseButton.Text = "...";
            this._pathToScriptFilePathBrowseButton.TextControl = this._pathToScriptFileTextBox;
            this._pathToScriptFilePathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _collectibleCheckBox
            // 
            this._collectibleCheckBox.AutoSize = true;
            this._collectibleCheckBox.Location = new System.Drawing.Point(15, 111);
            this._collectibleCheckBox.Name = "_collectibleCheckBox";
            this._collectibleCheckBox.Size = new System.Drawing.Size(200, 17);
            this._collectibleCheckBox.TabIndex = 6;
            this._collectibleCheckBox.Text = "Make generated assembly collectible";
            this._collectibleCheckBox.UseVisualStyleBackColor = true;
            // 
            // infoTip1
            // 
            this._collectibleInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._collectibleInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip1.BackgroundImage")));
            this._collectibleInfoTip.Location = new System.Drawing.Point(224, 102);
            this._collectibleInfoTip.Name = "infoTip1";
            this._collectibleInfoTip.Size = new System.Drawing.Size(16, 16);
            this._collectibleInfoTip.TabIndex = 7;
            this._collectibleInfoTip.TipText = null;
            // 
            // FSharpPreprocessorSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(429, 170);
            this.ControlBox = false;
            this.Controls.Add(this._collectibleInfoTip);
            this.Controls.Add(this._collectibleCheckBox);
            this.Controls.Add(this._pathToScriptFilePathBrowseButton);
            this.Controls.Add(this._pathToScriptFilePathTagsButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._functionNameTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._pathToScriptFileTextBox);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(445, 179);
            this.Name = "FSharpPreprocessorSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FSharp preprocessor settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _pathToScriptFileTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _functionNameTextBox;
        private Utilities.Forms.PathTagsButton _pathToScriptFilePathTagsButton;
        private Utilities.Forms.BrowseButton _pathToScriptFilePathBrowseButton;
        private System.Windows.Forms.CheckBox _collectibleCheckBox;
        private System.Windows.Forms.ToolTip toolTip1;
        private Utilities.Forms.InfoTip _collectibleInfoTip;
    }
}
