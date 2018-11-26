namespace Extract.FileActionManager.FileProcessors
{
    partial class RedactWithNERTaskSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RedactWithNERTaskSettingsDialog));
            this.label1 = new System.Windows.Forms.Label();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._nerModelTextBox = new System.Windows.Forms.TextBox();
            this._nerModelBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._nerModelPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._outputImageBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._outputImageTextBox = new System.Windows.Forms.TextBox();
            this._outputImagePathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._outputVOABrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._outputVOATextBox = new System.Windows.Forms.TextBox();
            this._outputVOAPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "NER Model path";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(361, 253);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 10;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(280, 253);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 9;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _nerModelTextBox
            // 
            this._nerModelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._nerModelTextBox.Location = new System.Drawing.Point(12, 25);
            this._nerModelTextBox.Name = "_nerModelTextBox";
            this._nerModelTextBox.Size = new System.Drawing.Size(367, 20);
            this._nerModelTextBox.TabIndex = 0;
            // 
            // _nerModelBrowseButton
            // 
            this._nerModelBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._nerModelBrowseButton.EnsureFileExists = false;
            this._nerModelBrowseButton.EnsurePathExists = false;
            this._nerModelBrowseButton.FileFilter = "XSD Files (*.xsd)|*.xsd|All files (*.*)|*.*";
            this._nerModelBrowseButton.Location = new System.Drawing.Point(410, 24);
            this._nerModelBrowseButton.Name = "_nerModelBrowseButton";
            this._nerModelBrowseButton.Size = new System.Drawing.Size(26, 21);
            this._nerModelBrowseButton.TabIndex = 2;
            this._nerModelBrowseButton.Text = "...";
            this._nerModelBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _nerModelPathTagButton
            // 
            this._nerModelPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._nerModelPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_nerModelPathTagButton.Image")));
            this._nerModelPathTagButton.Location = new System.Drawing.Point(386, 25);
            this._nerModelPathTagButton.Name = "_nerModelPathTagButton";
            this._nerModelPathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._nerModelPathTagButton.Size = new System.Drawing.Size(18, 21);
            this._nerModelPathTagButton.TabIndex = 1;
            this._nerModelPathTagButton.TextControl = this._nerModelTextBox;
            this._nerModelPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _outputImageBrowseButton
            // 
            this._outputImageBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputImageBrowseButton.EnsureFileExists = false;
            this._outputImageBrowseButton.EnsurePathExists = false;
            this._outputImageBrowseButton.FileFilter = "XSD Files (*.xsd)|*.xsd|All files (*.*)|*.*";
            this._outputImageBrowseButton.Location = new System.Drawing.Point(410, 88);
            this._outputImageBrowseButton.Name = "_outputImageBrowseButton";
            this._outputImageBrowseButton.Size = new System.Drawing.Size(26, 21);
            this._outputImageBrowseButton.TabIndex = 13;
            this._outputImageBrowseButton.Text = "...";
            this._outputImageBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _outputImageTextBox
            // 
            this._outputImageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputImageTextBox.Location = new System.Drawing.Point(12, 89);
            this._outputImageTextBox.Name = "_outputImageTextBox";
            this._outputImageTextBox.Size = new System.Drawing.Size(367, 20);
            this._outputImageTextBox.TabIndex = 11;
            // 
            // _outputImagePathTagButton
            // 
            this._outputImagePathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputImagePathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_outputImagePathTagButton.Image")));
            this._outputImagePathTagButton.Location = new System.Drawing.Point(386, 89);
            this._outputImagePathTagButton.Name = "_outputImagePathTagButton";
            this._outputImagePathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._outputImagePathTagButton.Size = new System.Drawing.Size(18, 21);
            this._outputImagePathTagButton.TabIndex = 12;
            this._outputImagePathTagButton.TextControl = this._outputImageTextBox;
            this._outputImagePathTagButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Output image path";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 124);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Output VOA path";
            // 
            // _outputVOABrowseButton
            // 
            this._outputVOABrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputVOABrowseButton.EnsureFileExists = false;
            this._outputVOABrowseButton.EnsurePathExists = false;
            this._outputVOABrowseButton.FileFilter = "XSD Files (*.xsd)|*.xsd|All files (*.*)|*.*";
            this._outputVOABrowseButton.Location = new System.Drawing.Point(410, 140);
            this._outputVOABrowseButton.Name = "_outputVOABrowseButton";
            this._outputVOABrowseButton.Size = new System.Drawing.Size(26, 21);
            this._outputVOABrowseButton.TabIndex = 17;
            this._outputVOABrowseButton.Text = "...";
            this._outputVOABrowseButton.UseVisualStyleBackColor = true;
            // 
            // _outputVOATextBox
            // 
            this._outputVOATextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputVOATextBox.Location = new System.Drawing.Point(12, 141);
            this._outputVOATextBox.Name = "_outputVOATextBox";
            this._outputVOATextBox.Size = new System.Drawing.Size(367, 20);
            this._outputVOATextBox.TabIndex = 15;
            // 
            // _outputVOAPathTagButton
            // 
            this._outputVOAPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputVOAPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_outputVOAPathTagButton.Image")));
            this._outputVOAPathTagButton.Location = new System.Drawing.Point(386, 141);
            this._outputVOAPathTagButton.Name = "_outputVOAPathTagButton";
            this._outputVOAPathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._outputVOAPathTagButton.Size = new System.Drawing.Size(18, 21);
            this._outputVOAPathTagButton.TabIndex = 16;
            this._outputVOAPathTagButton.TextControl = this._outputVOATextBox;
            this._outputVOAPathTagButton.UseVisualStyleBackColor = true;
            // 
            // RedactWithNERTaskSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(448, 287);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._outputVOABrowseButton);
            this.Controls.Add(this._outputVOATextBox);
            this.Controls.Add(this._outputVOAPathTagButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._outputImageBrowseButton);
            this.Controls.Add(this._outputImageTextBox);
            this.Controls.Add(this._outputImagePathTagButton);
            this.Controls.Add(this._nerModelBrowseButton);
            this.Controls.Add(this._nerModelTextBox);
            this.Controls.Add(this._nerModelPathTagButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 326);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(463, 300);
            this.Name = "RedactWithNERTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Redaction: Redact with NER";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TextBox _nerModelTextBox;
        private Forms.FileActionManagerPathTagButton _nerModelPathTagButton;
        private Utilities.Forms.BrowseButton _nerModelBrowseButton;
        private System.Windows.Forms.Label label1;
        private Utilities.Forms.BrowseButton _outputImageBrowseButton;
        private System.Windows.Forms.TextBox _outputImageTextBox;
        private Forms.FileActionManagerPathTagButton _outputImagePathTagButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private Utilities.Forms.BrowseButton _outputVOABrowseButton;
        private System.Windows.Forms.TextBox _outputVOATextBox;
        private Forms.FileActionManagerPathTagButton _outputVOAPathTagButton;
    }
}
