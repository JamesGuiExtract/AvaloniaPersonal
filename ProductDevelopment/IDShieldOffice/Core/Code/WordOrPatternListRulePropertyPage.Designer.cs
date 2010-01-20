namespace Extract.Rules
{
    partial class WordOrPatternListRulePropertyPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="WordOrPatternListRulePropertyPage"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="WordOrPatternListRulePropertyPage"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources
                if (components != null)
                {
                    components.Dispose();
                }
            }

            // Release unmanaged resources

            // Call base dispose method
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this._importButton = new System.Windows.Forms.Button();
            this._wordsOrPatternsTextBox = new System.Windows.Forms.TextBox();
            this._isRegexCheckBox = new System.Windows.Forms.CheckBox();
            this._matchCaseCheckBox = new System.Windows.Forms.CheckBox();
            this._exportButton = new System.Windows.Forms.Button();
            this._regexHelpLinkLabel = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(145, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Words/patterns (one per line)";
            // 
            // _importButton
            // 
            this._importButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._importButton.Location = new System.Drawing.Point(322, 21);
            this._importButton.Name = "_importButton";
            this._importButton.Size = new System.Drawing.Size(75, 23);
            this._importButton.TabIndex = 2;
            this._importButton.Text = "Import ...";
            this._importButton.UseVisualStyleBackColor = true;
            this._importButton.Click += new System.EventHandler(this._importButton_Click);
            // 
            // _wordsOrPatternsTextBox
            // 
            this._wordsOrPatternsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._wordsOrPatternsTextBox.HideSelection = false;
            this._wordsOrPatternsTextBox.Location = new System.Drawing.Point(7, 21);
            this._wordsOrPatternsTextBox.Multiline = true;
            this._wordsOrPatternsTextBox.Name = "_wordsOrPatternsTextBox";
            this._wordsOrPatternsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._wordsOrPatternsTextBox.Size = new System.Drawing.Size(309, 230);
            this._wordsOrPatternsTextBox.TabIndex = 1;
            this._wordsOrPatternsTextBox.WordWrap = false;
            this._wordsOrPatternsTextBox.TextChanged += new System.EventHandler(this._wordsOrPatternsTextBox_TextChanged);
            // 
            // _isRegexCheckBox
            // 
            this._isRegexCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._isRegexCheckBox.AutoSize = true;
            this._isRegexCheckBox.Location = new System.Drawing.Point(7, 280);
            this._isRegexCheckBox.Name = "_isRegexCheckBox";
            this._isRegexCheckBox.Size = new System.Drawing.Size(158, 17);
            this._isRegexCheckBox.TabIndex = 5;
            this._isRegexCheckBox.Text = "Treat as regular expressions";
            this._isRegexCheckBox.UseVisualStyleBackColor = true;
            this._isRegexCheckBox.CheckedChanged += new System.EventHandler(this._isRegexCheckBox_CheckedChanged);
            // 
            // _matchCaseCheckBox
            // 
            this._matchCaseCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._matchCaseCheckBox.AutoSize = true;
            this._matchCaseCheckBox.Location = new System.Drawing.Point(7, 257);
            this._matchCaseCheckBox.Name = "_matchCaseCheckBox";
            this._matchCaseCheckBox.Size = new System.Drawing.Size(82, 17);
            this._matchCaseCheckBox.TabIndex = 4;
            this._matchCaseCheckBox.Text = "Match case";
            this._matchCaseCheckBox.UseVisualStyleBackColor = true;
            this._matchCaseCheckBox.CheckedChanged += new System.EventHandler(this._matchCaseCheckBox_CheckedChanged);
            // 
            // _exportButton
            // 
            this._exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._exportButton.Location = new System.Drawing.Point(322, 50);
            this._exportButton.Name = "_exportButton";
            this._exportButton.Size = new System.Drawing.Size(75, 23);
            this._exportButton.TabIndex = 3;
            this._exportButton.Text = "Export ...";
            this._exportButton.UseVisualStyleBackColor = true;
            this._exportButton.Click += new System.EventHandler(this._exportButton_Click);
            // 
            // _regexHelpLinkLabel
            // 
            this._regexHelpLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._regexHelpLinkLabel.AutoSize = true;
            this._regexHelpLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this._regexHelpLinkLabel.Location = new System.Drawing.Point(162, 281);
            this._regexHelpLinkLabel.Name = "_regexHelpLinkLabel";
            this._regexHelpLinkLabel.Size = new System.Drawing.Size(65, 13);
            this._regexHelpLinkLabel.TabIndex = 6;
            this._regexHelpLinkLabel.TabStop = true;
            this._regexHelpLinkLabel.Text = "What\'s this?";
            this._regexHelpLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleRegexHelpLinkLabelLinkClicked);
            // 
            // WordOrPatternListRulePropertyPage
            // 
            this.Controls.Add(this._regexHelpLinkLabel);
            this.Controls.Add(this._exportButton);
            this.Controls.Add(this._matchCaseCheckBox);
            this.Controls.Add(this._isRegexCheckBox);
            this.Controls.Add(this._wordsOrPatternsTextBox);
            this.Controls.Add(this._importButton);
            this.Controls.Add(this.label1);
            this.Name = "WordOrPatternListRulePropertyPage";
            this.Size = new System.Drawing.Size(400, 300);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _importButton;
        private System.Windows.Forms.TextBox _wordsOrPatternsTextBox;
        private System.Windows.Forms.CheckBox _isRegexCheckBox;
        private System.Windows.Forms.CheckBox _matchCaseCheckBox;
        private System.Windows.Forms.Button _exportButton;
        private System.Windows.Forms.LinkLabel _regexHelpLinkLabel;
    }
}
