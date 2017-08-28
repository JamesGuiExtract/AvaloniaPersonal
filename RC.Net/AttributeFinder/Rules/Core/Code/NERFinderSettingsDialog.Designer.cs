namespace Extract.AttributeFinder.Rules
{
    partial class NERFinderSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NERFinderSettingsDialog));
            this.label1 = new System.Windows.Forms.Label();
            this._nameFinderGroupBox = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this._typesToReturnTextBox = new System.Windows.Forms.TextBox();
            this._classifierPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._nameFinderPathTextBox = new System.Windows.Forms.TextBox();
            this._classifierPathTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this._tokenizerPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._tokenizerPathTextBox = new System.Windows.Forms.TextBox();
            this._tokenizerPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this.label3 = new System.Windows.Forms.Label();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._tokenizerGroupBox = new System.Windows.Forms.GroupBox();
            this._learnableTokenizerRadioButton = new System.Windows.Forms.RadioButton();
            this._simpleTokenizerRadioButton = new System.Windows.Forms.RadioButton();
            this._whitespaceTokenizerRadioButton = new System.Windows.Forms.RadioButton();
            this._sentenceDetectorGroupBox = new System.Windows.Forms.GroupBox();
            this._splitIntoSentencesCheckBox = new System.Windows.Forms.CheckBox();
            this.browseButton1 = new Extract.Utilities.Forms.BrowseButton();
            this._sentenceDetectorPathTextBox = new System.Windows.Forms.TextBox();
            this.pathTagsButton1 = new Extract.Utilities.Forms.PathTagsButton();
            this.label5 = new System.Windows.Forms.Label();
            this._toolKitGroupBox = new System.Windows.Forms.GroupBox();
            this._stanfordNerRadioButton = new System.Windows.Forms.RadioButton();
            this._openNlpRadioButton = new System.Windows.Forms.RadioButton();
            this._nameFinderGroupBox.SuspendLayout();
            this._tokenizerGroupBox.SuspendLayout();
            this._sentenceDetectorGroupBox.SuspendLayout();
            this._toolKitGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name finder model path";
            // 
            // _nameFinderGroupBox
            // 
            this._nameFinderGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._nameFinderGroupBox.Controls.Add(this.label2);
            this._nameFinderGroupBox.Controls.Add(this._typesToReturnTextBox);
            this._nameFinderGroupBox.Controls.Add(this._classifierPathBrowseButton);
            this._nameFinderGroupBox.Controls.Add(this._classifierPathTagButton);
            this._nameFinderGroupBox.Controls.Add(this._nameFinderPathTextBox);
            this._nameFinderGroupBox.Controls.Add(this.label1);
            this._nameFinderGroupBox.Location = new System.Drawing.Point(12, 328);
            this._nameFinderGroupBox.Name = "_nameFinderGroupBox";
            this._nameFinderGroupBox.Size = new System.Drawing.Size(580, 112);
            this._nameFinderGroupBox.TabIndex = 3;
            this._nameFinderGroupBox.TabStop = false;
            this._nameFinderGroupBox.Text = "Name finder";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(439, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Entity types to return: (Empty for all types, else CSV, e.g., Location, Person, O" +
    "rganization, ...)";
            // 
            // _typesToReturnTextBox
            // 
            this._typesToReturnTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._typesToReturnTextBox.Location = new System.Drawing.Point(9, 79);
            this._typesToReturnTextBox.Name = "_typesToReturnTextBox";
            this._typesToReturnTextBox.Size = new System.Drawing.Size(556, 20);
            this._typesToReturnTextBox.TabIndex = 5;
            // 
            // _classifierPathBrowseButton
            // 
            this._classifierPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._classifierPathBrowseButton.Location = new System.Drawing.Point(542, 36);
            this._classifierPathBrowseButton.Name = "_classifierPathBrowseButton";
            this._classifierPathBrowseButton.Size = new System.Drawing.Size(24, 22);
            this._classifierPathBrowseButton.TabIndex = 3;
            this._classifierPathBrowseButton.Text = "...";
            this._classifierPathBrowseButton.TextControl = this._nameFinderPathTextBox;
            this._classifierPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _nameFinderPathTextBox
            // 
            this._nameFinderPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._nameFinderPathTextBox.Location = new System.Drawing.Point(9, 37);
            this._nameFinderPathTextBox.Name = "_nameFinderPathTextBox";
            this._nameFinderPathTextBox.Size = new System.Drawing.Size(502, 20);
            this._nameFinderPathTextBox.TabIndex = 1;
            // 
            // _classifierPathTagButton
            // 
            this._classifierPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._classifierPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_classifierPathTagButton.Image")));
            this._classifierPathTagButton.Location = new System.Drawing.Point(518, 36);
            this._classifierPathTagButton.Name = "_classifierPathTagButton";
            this._classifierPathTagButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._classifierPathTagButton.Size = new System.Drawing.Size(18, 22);
            this._classifierPathTagButton.TabIndex = 2;
            this._classifierPathTagButton.TextControl = this._nameFinderPathTextBox;
            this._classifierPathTagButton.UseVisualStyleBackColor = true;
            // 
            // _tokenizerPathBrowseButton
            // 
            this._tokenizerPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._tokenizerPathBrowseButton.Location = new System.Drawing.Point(545, 108);
            this._tokenizerPathBrowseButton.Name = "_tokenizerPathBrowseButton";
            this._tokenizerPathBrowseButton.Size = new System.Drawing.Size(24, 22);
            this._tokenizerPathBrowseButton.TabIndex = 6;
            this._tokenizerPathBrowseButton.Text = "...";
            this._tokenizerPathBrowseButton.TextControl = this._tokenizerPathTextBox;
            this._tokenizerPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _tokenizerPathTextBox
            // 
            this._tokenizerPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tokenizerPathTextBox.Location = new System.Drawing.Point(6, 109);
            this._tokenizerPathTextBox.Name = "_tokenizerPathTextBox";
            this._tokenizerPathTextBox.Size = new System.Drawing.Size(508, 20);
            this._tokenizerPathTextBox.TabIndex = 4;
            // 
            // _tokenizerPathTagsButton
            // 
            this._tokenizerPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._tokenizerPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_tokenizerPathTagsButton.Image")));
            this._tokenizerPathTagsButton.Location = new System.Drawing.Point(521, 108);
            this._tokenizerPathTagsButton.Name = "_tokenizerPathTagsButton";
            this._tokenizerPathTagsButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._tokenizerPathTagsButton.Size = new System.Drawing.Size(18, 22);
            this._tokenizerPathTagsButton.TabIndex = 5;
            this._tokenizerPathTagsButton.TextControl = this._tokenizerPathTextBox;
            this._tokenizerPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(109, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Tokenizer model path";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(517, 446);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 5;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(436, 446);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 4;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _tokenizerGroupBox
            // 
            this._tokenizerGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tokenizerGroupBox.Controls.Add(this._learnableTokenizerRadioButton);
            this._tokenizerGroupBox.Controls.Add(this._simpleTokenizerRadioButton);
            this._tokenizerGroupBox.Controls.Add(this._whitespaceTokenizerRadioButton);
            this._tokenizerGroupBox.Controls.Add(this._tokenizerPathBrowseButton);
            this._tokenizerGroupBox.Controls.Add(this._tokenizerPathTextBox);
            this._tokenizerGroupBox.Controls.Add(this._tokenizerPathTagsButton);
            this._tokenizerGroupBox.Controls.Add(this.label3);
            this._tokenizerGroupBox.Location = new System.Drawing.Point(12, 182);
            this._tokenizerGroupBox.Name = "_tokenizerGroupBox";
            this._tokenizerGroupBox.Size = new System.Drawing.Size(580, 140);
            this._tokenizerGroupBox.TabIndex = 2;
            this._tokenizerGroupBox.TabStop = false;
            this._tokenizerGroupBox.Text = "Tokenizer";
            // 
            // _learnableTokenizerRadioButton
            // 
            this._learnableTokenizerRadioButton.AutoSize = true;
            this._learnableTokenizerRadioButton.Location = new System.Drawing.Point(6, 65);
            this._learnableTokenizerRadioButton.Name = "_learnableTokenizerRadioButton";
            this._learnableTokenizerRadioButton.Size = new System.Drawing.Size(122, 17);
            this._learnableTokenizerRadioButton.TabIndex = 2;
            this._learnableTokenizerRadioButton.TabStop = true;
            this._learnableTokenizerRadioButton.Text = "Learnable Tokenizer";
            this._learnableTokenizerRadioButton.UseVisualStyleBackColor = true;
            this._learnableTokenizerRadioButton.CheckedChanged += new System.EventHandler(this.LearnableTokenizerRadioButton_CheckedChanged);
            // 
            // _simpleTokenizerRadioButton
            // 
            this._simpleTokenizerRadioButton.AutoSize = true;
            this._simpleTokenizerRadioButton.Location = new System.Drawing.Point(6, 42);
            this._simpleTokenizerRadioButton.Name = "_simpleTokenizerRadioButton";
            this._simpleTokenizerRadioButton.Size = new System.Drawing.Size(106, 17);
            this._simpleTokenizerRadioButton.TabIndex = 1;
            this._simpleTokenizerRadioButton.TabStop = true;
            this._simpleTokenizerRadioButton.Text = "Simple Tokenizer";
            this._simpleTokenizerRadioButton.UseVisualStyleBackColor = true;
            // 
            // _whitespaceTokenizerRadioButton
            // 
            this._whitespaceTokenizerRadioButton.AutoSize = true;
            this._whitespaceTokenizerRadioButton.Location = new System.Drawing.Point(6, 19);
            this._whitespaceTokenizerRadioButton.Name = "_whitespaceTokenizerRadioButton";
            this._whitespaceTokenizerRadioButton.Size = new System.Drawing.Size(132, 17);
            this._whitespaceTokenizerRadioButton.TabIndex = 0;
            this._whitespaceTokenizerRadioButton.TabStop = true;
            this._whitespaceTokenizerRadioButton.Text = "Whitespace Tokenizer";
            this._whitespaceTokenizerRadioButton.UseVisualStyleBackColor = true;
            // 
            // _sentenceDetectorGroupBox
            // 
            this._sentenceDetectorGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sentenceDetectorGroupBox.Controls.Add(this._splitIntoSentencesCheckBox);
            this._sentenceDetectorGroupBox.Controls.Add(this.browseButton1);
            this._sentenceDetectorGroupBox.Controls.Add(this.pathTagsButton1);
            this._sentenceDetectorGroupBox.Controls.Add(this._sentenceDetectorPathTextBox);
            this._sentenceDetectorGroupBox.Controls.Add(this.label5);
            this._sentenceDetectorGroupBox.Location = new System.Drawing.Point(12, 87);
            this._sentenceDetectorGroupBox.Name = "_sentenceDetectorGroupBox";
            this._sentenceDetectorGroupBox.Size = new System.Drawing.Size(580, 89);
            this._sentenceDetectorGroupBox.TabIndex = 1;
            this._sentenceDetectorGroupBox.TabStop = false;
            this._sentenceDetectorGroupBox.Text = "Sentence detector";
            // 
            // _splitIntoSentencesCheckBox
            // 
            this._splitIntoSentencesCheckBox.AutoSize = true;
            this._splitIntoSentencesCheckBox.Location = new System.Drawing.Point(6, 19);
            this._splitIntoSentencesCheckBox.Name = "_splitIntoSentencesCheckBox";
            this._splitIntoSentencesCheckBox.Size = new System.Drawing.Size(144, 17);
            this._splitIntoSentencesCheckBox.TabIndex = 0;
            this._splitIntoSentencesCheckBox.Text = "Split input into sentences";
            this._splitIntoSentencesCheckBox.UseVisualStyleBackColor = true;
            this._splitIntoSentencesCheckBox.CheckedChanged += new System.EventHandler(this.SplitIntoSentencesCheckBox_CheckedChanged);
            // 
            // browseButton1
            // 
            this.browseButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton1.Location = new System.Drawing.Point(542, 55);
            this.browseButton1.Name = "browseButton1";
            this.browseButton1.Size = new System.Drawing.Size(24, 22);
            this.browseButton1.TabIndex = 4;
            this.browseButton1.Text = "...";
            this.browseButton1.TextControl = this._sentenceDetectorPathTextBox;
            this.browseButton1.UseVisualStyleBackColor = true;
            // 
            // _sentenceDetectorPathTextBox
            // 
            this._sentenceDetectorPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sentenceDetectorPathTextBox.Location = new System.Drawing.Point(9, 56);
            this._sentenceDetectorPathTextBox.Name = "_sentenceDetectorPathTextBox";
            this._sentenceDetectorPathTextBox.Size = new System.Drawing.Size(502, 20);
            this._sentenceDetectorPathTextBox.TabIndex = 2;
            // 
            // pathTagsButton1
            // 
            this.pathTagsButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pathTagsButton1.Image = ((System.Drawing.Image)(resources.GetObject("pathTagsButton1.Image")));
            this.pathTagsButton1.Location = new System.Drawing.Point(518, 55);
            this.pathTagsButton1.Name = "pathTagsButton1";
            this.pathTagsButton1.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this.pathTagsButton1.Size = new System.Drawing.Size(18, 22);
            this.pathTagsButton1.TabIndex = 3;
            this.pathTagsButton1.TextControl = this._sentenceDetectorPathTextBox;
            this.pathTagsButton1.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 39);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(150, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Sentence detector model path";
            // 
            // _toolKitGroupBox
            // 
            this._toolKitGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._toolKitGroupBox.Controls.Add(this._stanfordNerRadioButton);
            this._toolKitGroupBox.Controls.Add(this._openNlpRadioButton);
            this._toolKitGroupBox.Location = new System.Drawing.Point(12, 12);
            this._toolKitGroupBox.Name = "_toolKitGroupBox";
            this._toolKitGroupBox.Size = new System.Drawing.Size(580, 69);
            this._toolKitGroupBox.TabIndex = 0;
            this._toolKitGroupBox.TabStop = false;
            this._toolKitGroupBox.Text = "NLP Toolkit";
            // 
            // _stanfordNerRadioButton
            // 
            this._stanfordNerRadioButton.AutoSize = true;
            this._stanfordNerRadioButton.Location = new System.Drawing.Point(6, 42);
            this._stanfordNerRadioButton.Name = "_stanfordNerRadioButton";
            this._stanfordNerRadioButton.Size = new System.Drawing.Size(91, 17);
            this._stanfordNerRadioButton.TabIndex = 1;
            this._stanfordNerRadioButton.TabStop = true;
            this._stanfordNerRadioButton.Text = "Stanford NER";
            this._stanfordNerRadioButton.UseVisualStyleBackColor = true;
            this._stanfordNerRadioButton.CheckedChanged += new System.EventHandler(this.StanfordNerRadioButton_CheckedChanged);
            // 
            // _openNlpRadioButton
            // 
            this._openNlpRadioButton.AutoSize = true;
            this._openNlpRadioButton.Location = new System.Drawing.Point(6, 19);
            this._openNlpRadioButton.Name = "_openNlpRadioButton";
            this._openNlpRadioButton.Size = new System.Drawing.Size(72, 17);
            this._openNlpRadioButton.TabIndex = 0;
            this._openNlpRadioButton.TabStop = true;
            this._openNlpRadioButton.Text = "OpenNLP";
            this._openNlpRadioButton.UseVisualStyleBackColor = true;
            // 
            // NERFinderSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(604, 481);
            this.Controls.Add(this._toolKitGroupBox);
            this.Controls.Add(this._sentenceDetectorGroupBox);
            this.Controls.Add(this._tokenizerGroupBox);
            this.Controls.Add(this._nameFinderGroupBox);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1024, 520);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(620, 520);
            this.Name = "NERFinderSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Named entity recognition finder settings";
            this._nameFinderGroupBox.ResumeLayout(false);
            this._nameFinderGroupBox.PerformLayout();
            this._tokenizerGroupBox.ResumeLayout(false);
            this._tokenizerGroupBox.PerformLayout();
            this._sentenceDetectorGroupBox.ResumeLayout(false);
            this._sentenceDetectorGroupBox.PerformLayout();
            this._toolKitGroupBox.ResumeLayout(false);
            this._toolKitGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox _nameFinderGroupBox;
        private Utilities.Forms.PathTagsButton _classifierPathTagButton;
        private System.Windows.Forms.TextBox _nameFinderPathTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _typesToReturnTextBox;
        private Utilities.Forms.BrowseButton _classifierPathBrowseButton;
        private Utilities.Forms.BrowseButton _tokenizerPathBrowseButton;
        private System.Windows.Forms.TextBox _tokenizerPathTextBox;
        private Utilities.Forms.PathTagsButton _tokenizerPathTagsButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox _tokenizerGroupBox;
        private System.Windows.Forms.RadioButton _learnableTokenizerRadioButton;
        private System.Windows.Forms.RadioButton _simpleTokenizerRadioButton;
        private System.Windows.Forms.RadioButton _whitespaceTokenizerRadioButton;
        private System.Windows.Forms.GroupBox _sentenceDetectorGroupBox;
        private System.Windows.Forms.CheckBox _splitIntoSentencesCheckBox;
        private Utilities.Forms.BrowseButton browseButton1;
        private System.Windows.Forms.TextBox _sentenceDetectorPathTextBox;
        private Utilities.Forms.PathTagsButton pathTagsButton1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox _toolKitGroupBox;
        private System.Windows.Forms.RadioButton _stanfordNerRadioButton;
        private System.Windows.Forms.RadioButton _openNlpRadioButton;
    }
}
