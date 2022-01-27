namespace Extract.FileActionManager.FileProcessors
{
    partial class TransformXmlTaskSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TransformXmlTaskSettingsDialog));
            this.label1 = new System.Windows.Forms.Label();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this.inputXmlTextBox = new System.Windows.Forms.TextBox();
            this.styleSheetGroupBox = new System.Windows.Forms.GroupBox();
            this.styleSheetBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this.styleSheetTextBox = new System.Windows.Forms.TextBox();
            this.styleSheetPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this.specifiedStyleSheetRadioButton = new System.Windows.Forms.RadioButton();
            this.alphaSortRadioButton = new System.Windows.Forms.RadioButton();
            this.inputXmlBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this.inputXmlPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this.outputBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.outputPathTagButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this.label2 = new System.Windows.Forms.Label();
            this.styleSheetGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input XML path";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(361, 227);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 10;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(280, 227);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 9;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // inputXmlTextBox
            // 
            this.inputXmlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputXmlTextBox.Location = new System.Drawing.Point(12, 25);
            this.inputXmlTextBox.Name = "inputXmlTextBox";
            this.inputXmlTextBox.Size = new System.Drawing.Size(367, 20);
            this.inputXmlTextBox.TabIndex = 1;
            // 
            // styleSheetGroupBox
            // 
            this.styleSheetGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.styleSheetGroupBox.Controls.Add(this.styleSheetBrowseButton);
            this.styleSheetGroupBox.Controls.Add(this.styleSheetTextBox);
            this.styleSheetGroupBox.Controls.Add(this.styleSheetPathTagButton);
            this.styleSheetGroupBox.Controls.Add(this.specifiedStyleSheetRadioButton);
            this.styleSheetGroupBox.Controls.Add(this.alphaSortRadioButton);
            this.styleSheetGroupBox.Location = new System.Drawing.Point(12, 52);
            this.styleSheetGroupBox.Name = "styleSheetGroupBox";
            this.styleSheetGroupBox.Size = new System.Drawing.Size(424, 99);
            this.styleSheetGroupBox.TabIndex = 4;
            this.styleSheetGroupBox.TabStop = false;
            this.styleSheetGroupBox.Text = "Transform using";
            // 
            // styleSheetBrowseButton
            // 
            this.styleSheetBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.styleSheetBrowseButton.EnsureFileExists = false;
            this.styleSheetBrowseButton.EnsurePathExists = false;
            this.styleSheetBrowseButton.FileFilter = "XSLT Files (*.xslt)|*.xslt|All files (*.*)|*.*";
            this.styleSheetBrowseButton.Location = new System.Drawing.Point(392, 64);
            this.styleSheetBrowseButton.Name = "styleSheetBrowseButton";
            this.styleSheetBrowseButton.Size = new System.Drawing.Size(26, 21);
            this.styleSheetBrowseButton.TabIndex = 4;
            this.styleSheetBrowseButton.Text = "...";
            this.styleSheetBrowseButton.TextControl = this.styleSheetTextBox;
            this.styleSheetBrowseButton.UseVisualStyleBackColor = true;
            // 
            // styleSheetTextBox
            // 
            this.styleSheetTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.styleSheetTextBox.Location = new System.Drawing.Point(9, 65);
            this.styleSheetTextBox.Name = "styleSheetTextBox";
            this.styleSheetTextBox.Size = new System.Drawing.Size(353, 20);
            this.styleSheetTextBox.TabIndex = 2;
            // 
            // styleSheetPathTagButton
            // 
            this.styleSheetPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.styleSheetPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("styleSheetPathTagButton.Image")));
            this.styleSheetPathTagButton.Location = new System.Drawing.Point(368, 64);
            this.styleSheetPathTagButton.Name = "styleSheetPathTagButton";
            this.styleSheetPathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this.styleSheetPathTagButton.Size = new System.Drawing.Size(18, 21);
            this.styleSheetPathTagButton.TabIndex = 3;
            this.styleSheetPathTagButton.TextControl = this.styleSheetTextBox;
            this.styleSheetPathTagButton.UseVisualStyleBackColor = true;
            // 
            // specifiedStyleSheetRadioButton
            // 
            this.specifiedStyleSheetRadioButton.AutoSize = true;
            this.specifiedStyleSheetRadioButton.Location = new System.Drawing.Point(9, 38);
            this.specifiedStyleSheetRadioButton.Name = "specifiedStyleSheetRadioButton";
            this.specifiedStyleSheetRadioButton.Size = new System.Drawing.Size(119, 17);
            this.specifiedStyleSheetRadioButton.TabIndex = 1;
            this.specifiedStyleSheetRadioButton.Text = "Specified stylesheet";
            this.specifiedStyleSheetRadioButton.UseVisualStyleBackColor = true;
            // 
            // alphaSortRadioButton
            // 
            this.alphaSortRadioButton.AutoSize = true;
            this.alphaSortRadioButton.Checked = true;
            this.alphaSortRadioButton.Location = new System.Drawing.Point(10, 17);
            this.alphaSortRadioButton.Name = "alphaSortRadioButton";
            this.alphaSortRadioButton.Size = new System.Drawing.Size(382, 17);
            this.alphaSortRadioButton.TabIndex = 0;
            this.alphaSortRadioButton.TabStop = true;
            this.alphaSortRadioButton.Text = "Alphabetical sort by node name, @Name, @FieldName (FullText nodes first)";
            this.alphaSortRadioButton.UseVisualStyleBackColor = true;
            // 
            // inputXmlBrowseButton
            // 
            this.inputXmlBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.inputXmlBrowseButton.EnsureFileExists = false;
            this.inputXmlBrowseButton.EnsurePathExists = false;
            this.inputXmlBrowseButton.FileFilter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*";
            this.inputXmlBrowseButton.Location = new System.Drawing.Point(410, 24);
            this.inputXmlBrowseButton.Name = "inputXmlBrowseButton";
            this.inputXmlBrowseButton.Size = new System.Drawing.Size(26, 21);
            this.inputXmlBrowseButton.TabIndex = 3;
            this.inputXmlBrowseButton.Text = "...";
            this.inputXmlBrowseButton.TextControl = this.inputXmlTextBox;
            this.inputXmlBrowseButton.UseVisualStyleBackColor = true;
            // 
            // inputXmlPathTagButton
            // 
            this.inputXmlPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.inputXmlPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("inputXmlPathTagButton.Image")));
            this.inputXmlPathTagButton.Location = new System.Drawing.Point(386, 24);
            this.inputXmlPathTagButton.Name = "inputXmlPathTagButton";
            this.inputXmlPathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this.inputXmlPathTagButton.Size = new System.Drawing.Size(18, 21);
            this.inputXmlPathTagButton.TabIndex = 2;
            this.inputXmlPathTagButton.TextControl = this.inputXmlTextBox;
            this.inputXmlPathTagButton.UseVisualStyleBackColor = true;
            // 
            // outputBrowseButton
            // 
            this.outputBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.outputBrowseButton.EnsureFileExists = false;
            this.outputBrowseButton.EnsurePathExists = false;
            this.outputBrowseButton.FileFilter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*";
            this.outputBrowseButton.Location = new System.Drawing.Point(410, 177);
            this.outputBrowseButton.Name = "outputBrowseButton";
            this.outputBrowseButton.Size = new System.Drawing.Size(26, 21);
            this.outputBrowseButton.TabIndex = 8;
            this.outputBrowseButton.Text = "...";
            this.outputBrowseButton.TextControl = this.outputTextBox;
            this.outputBrowseButton.UseVisualStyleBackColor = true;
            // 
            // outputTextBox
            // 
            this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputTextBox.Location = new System.Drawing.Point(12, 178);
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.Size = new System.Drawing.Size(367, 20);
            this.outputTextBox.TabIndex = 6;
            // 
            // outputPathTagButton
            // 
            this.outputPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.outputPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("outputPathTagButton.Image")));
            this.outputPathTagButton.Location = new System.Drawing.Point(386, 177);
            this.outputPathTagButton.Name = "outputPathTagButton";
            this.outputPathTagButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this.outputPathTagButton.Size = new System.Drawing.Size(18, 21);
            this.outputPathTagButton.TabIndex = 7;
            this.outputPathTagButton.TextControl = this.outputTextBox;
            this.outputPathTagButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 161);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Output path";
            // 
            // TransformXmlTaskSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(448, 261);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.outputBrowseButton);
            this.Controls.Add(this.outputTextBox);
            this.Controls.Add(this.outputPathTagButton);
            this.Controls.Add(this.styleSheetGroupBox);
            this.Controls.Add(this.inputXmlBrowseButton);
            this.Controls.Add(this.inputXmlTextBox);
            this.Controls.Add(this.inputXmlPathTagButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 326);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(463, 300);
            this.Name = "TransformXmlTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Transform XML Task";
            this.styleSheetGroupBox.ResumeLayout(false);
            this.styleSheetGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TextBox inputXmlTextBox;
        private Forms.FileActionManagerPathTagButton inputXmlPathTagButton;
        private Extract.Utilities.Forms.BrowseButton inputXmlBrowseButton;
        private System.Windows.Forms.GroupBox styleSheetGroupBox;
        private System.Windows.Forms.RadioButton specifiedStyleSheetRadioButton;
        private System.Windows.Forms.RadioButton alphaSortRadioButton;
        private System.Windows.Forms.Label label1;
        private Extract.Utilities.Forms.BrowseButton styleSheetBrowseButton;
        private System.Windows.Forms.TextBox styleSheetTextBox;
        private Forms.FileActionManagerPathTagButton styleSheetPathTagButton;
        private Extract.Utilities.Forms.BrowseButton outputBrowseButton;
        private System.Windows.Forms.TextBox outputTextBox;
        private Forms.FileActionManagerPathTagButton outputPathTagButton;
        private System.Windows.Forms.Label label2;
    }
}
