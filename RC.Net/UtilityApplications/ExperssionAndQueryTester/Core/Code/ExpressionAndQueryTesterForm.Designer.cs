namespace Extract.UtilityApplications.ExpressionAndQueryTester
{
    /// <summary>
    /// 
    /// </summary>
    partial class ExpressionAndQueryTesterForm
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExpressionAndQueryTesterForm));
            this._targetAttributesLabel = new System.Windows.Forms.Label();
            this._expressionOrQueryScintillaBox = new ScintillaNET.Scintilla();
            this.panel1 = new System.Windows.Forms.Panel();
            this._voaFileNameTextBox = new System.Windows.Forms.TextBox();
            this._testExpressionRadioButton = new System.Windows.Forms.RadioButton();
            this._testQueryRadioButton = new System.Windows.Forms.RadioButton();
            this._targetAttributeTextBox = new System.Windows.Forms.TextBox();
            this._splitContainer = new System.Windows.Forms.SplitContainer();
            this._resultsDataGridView = new System.Windows.Forms.DataGridView();
            this.panel2 = new System.Windows.Forms.Panel();
            this._testButton = new System.Windows.Forms.Button();
            this._rsdOrDbFileNameTextBox = new System.Windows.Forms.TextBox();
            this._rsdOrDbFilenameLabel = new System.Windows.Forms.Label();
            this._clearResultButton = new System.Windows.Forms.Button();
            this._targetOrRootAttributesInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._rsdOrDbFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._voaFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            label1 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._expressionOrQueryScintillaBox)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._resultsDataGridView)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(133, 13);
            label1.TabIndex = 3;
            label1.Text = "VOA filename (data to test)";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(4, 4);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(37, 13);
            label3.TabIndex = 0;
            label3.Text = "Result";
            // 
            // _targetAttributesLabel
            // 
            this._targetAttributesLabel.AutoSize = true;
            this._targetAttributesLabel.Location = new System.Drawing.Point(207, 58);
            this._targetAttributesLabel.Name = "_targetAttributesLabel";
            this._targetAttributesLabel.Size = new System.Drawing.Size(98, 13);
            this._targetAttributesLabel.TabIndex = 8;
            this._targetAttributesLabel.Text = "Attribute(s) to score";
            // 
            // _expressionOrQueryScintillaBox
            // 
            this._expressionOrQueryScintillaBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._expressionOrQueryScintillaBox.Annotations.Visibility = ScintillaNET.AnnotationsVisibility.Standard;
            this._expressionOrQueryScintillaBox.ConfigurationManager.Language = "cs";
            this._expressionOrQueryScintillaBox.Indentation.ShowGuides = true;
            this._expressionOrQueryScintillaBox.Indentation.SmartIndentType = ScintillaNET.SmartIndent.Simple;
            this._expressionOrQueryScintillaBox.Indentation.TabWidth = 4;
            this._expressionOrQueryScintillaBox.IsBraceMatching = true;
            this._expressionOrQueryScintillaBox.LineWrap.Mode = ScintillaNET.WrapMode.Word;
            this._expressionOrQueryScintillaBox.LineWrap.VisualFlags = ScintillaNET.WrapVisualFlag.Start;
            this._expressionOrQueryScintillaBox.LineWrap.VisualFlagsLocation = ScintillaNET.WrapVisualLocation.StartByText;
            this._expressionOrQueryScintillaBox.Location = new System.Drawing.Point(-1, -1);
            this._expressionOrQueryScintillaBox.Margins.FoldMarginColor = System.Drawing.Color.LightSteelBlue;
            this._expressionOrQueryScintillaBox.Margins.FoldMarginHighlightColor = System.Drawing.Color.LightSteelBlue;
            this._expressionOrQueryScintillaBox.Margins.Margin1.Width = 0;
            this._expressionOrQueryScintillaBox.Name = "_expressionOrQueryScintillaBox";
            this._expressionOrQueryScintillaBox.Size = new System.Drawing.Size(713, 201);
            this._expressionOrQueryScintillaBox.TabIndex = 0;
            this._expressionOrQueryScintillaBox.TextChanged += new System.EventHandler(this.HandleTextChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this._expressionOrQueryScintillaBox);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(713, 201);
            this.panel1.TabIndex = 1;
            // 
            // _voaFileNameTextBox
            // 
            this._voaFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFileNameTextBox.Location = new System.Drawing.Point(12, 29);
            this._voaFileNameTextBox.Name = "_voaFileNameTextBox";
            this._voaFileNameTextBox.Size = new System.Drawing.Size(680, 20);
            this._voaFileNameTextBox.TabIndex = 0;
            this._voaFileNameTextBox.TextChanged += new System.EventHandler(this.HandleTextChanged);
            // 
            // _testExpressionRadioButton
            // 
            this._testExpressionRadioButton.AutoSize = true;
            this._testExpressionRadioButton.Checked = true;
            this._testExpressionRadioButton.Location = new System.Drawing.Point(12, 56);
            this._testExpressionRadioButton.Name = "_testExpressionRadioButton";
            this._testExpressionRadioButton.Size = new System.Drawing.Size(181, 17);
            this._testExpressionRadioButton.TabIndex = 5;
            this._testExpressionRadioButton.TabStop = true;
            this._testExpressionRadioButton.Text = "Test RSD data scorer expression";
            this._testExpressionRadioButton.UseVisualStyleBackColor = true;
            this._testExpressionRadioButton.CheckedChanged += new System.EventHandler(this.HandleTestTypeCheckedChanged);
            // 
            // _testQueryRadioButton
            // 
            this._testQueryRadioButton.AutoSize = true;
            this._testQueryRadioButton.Location = new System.Drawing.Point(12, 79);
            this._testQueryRadioButton.Name = "_testQueryRadioButton";
            this._testQueryRadioButton.Size = new System.Drawing.Size(125, 17);
            this._testQueryRadioButton.TabIndex = 6;
            this._testQueryRadioButton.TabStop = true;
            this._testQueryRadioButton.Text = "Test data entry query";
            this._testQueryRadioButton.UseVisualStyleBackColor = true;
            // 
            // _targetAttributeTextBox
            // 
            this._targetAttributeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._targetAttributeTextBox.Location = new System.Drawing.Point(210, 74);
            this._targetAttributeTextBox.Name = "_targetAttributeTextBox";
            this._targetAttributeTextBox.Size = new System.Drawing.Size(515, 20);
            this._targetAttributeTextBox.TabIndex = 2;
            // 
            // _splitContainer
            // 
            this._splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._splitContainer.Location = new System.Drawing.Point(12, 145);
            this._splitContainer.Name = "_splitContainer";
            this._splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _splitContainer.Panel1
            // 
            this._splitContainer.Panel1.Controls.Add(this.panel1);
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.Controls.Add(this._resultsDataGridView);
            this._splitContainer.Panel2.Controls.Add(this.panel2);
            this._splitContainer.Size = new System.Drawing.Size(713, 445);
            this._splitContainer.SplitterDistance = 201;
            this._splitContainer.TabIndex = 9;
            // 
            // _resultsDataGridView
            // 
            this._resultsDataGridView.AllowUserToAddRows = false;
            this._resultsDataGridView.AllowUserToDeleteRows = false;
            this._resultsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._resultsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._resultsDataGridView.ColumnHeadersVisible = false;
            this._resultsDataGridView.Location = new System.Drawing.Point(0, 20);
            this._resultsDataGridView.Name = "_resultsDataGridView";
            this._resultsDataGridView.ReadOnly = true;
            this._resultsDataGridView.Size = new System.Drawing.Size(713, 220);
            this._resultsDataGridView.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(label3);
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(713, 20);
            this.panel2.TabIndex = 0;
            // 
            // _testButton
            // 
            this._testButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._testButton.Location = new System.Drawing.Point(650, 596);
            this._testButton.Name = "_testButton";
            this._testButton.Size = new System.Drawing.Size(75, 23);
            this._testButton.TabIndex = 11;
            this._testButton.Text = "Test";
            this._testButton.UseVisualStyleBackColor = true;
            this._testButton.Click += new System.EventHandler(this.HandleTestButtonClick);
            // 
            // _rsdOrDbFileNameTextBox
            // 
            this._rsdOrDbFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._rsdOrDbFileNameTextBox.Location = new System.Drawing.Point(210, 114);
            this._rsdOrDbFileNameTextBox.Name = "_rsdOrDbFileNameTextBox";
            this._rsdOrDbFileNameTextBox.Size = new System.Drawing.Size(482, 20);
            this._rsdOrDbFileNameTextBox.TabIndex = 3;
            this._rsdOrDbFileNameTextBox.TextChanged += new System.EventHandler(this.HandleTextChanged);
            // 
            // _rsdOrDbFilenameLabel
            // 
            this._rsdOrDbFilenameLabel.AutoSize = true;
            this._rsdOrDbFilenameLabel.Location = new System.Drawing.Point(207, 98);
            this._rsdOrDbFilenameLabel.Name = "_rsdOrDbFilenameLabel";
            this._rsdOrDbFilenameLabel.Size = new System.Drawing.Size(75, 13);
            this._rsdOrDbFilenameLabel.TabIndex = 12;
            this._rsdOrDbFilenameLabel.Text = "RSD Filename";
            // 
            // _clearResultButton
            // 
            this._clearResultButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._clearResultButton.Location = new System.Drawing.Point(569, 596);
            this._clearResultButton.Name = "_clearResultButton";
            this._clearResultButton.Size = new System.Drawing.Size(75, 23);
            this._clearResultButton.TabIndex = 10;
            this._clearResultButton.Text = "Clear Result";
            this._clearResultButton.UseVisualStyleBackColor = true;
            this._clearResultButton.Click += new System.EventHandler(this.HandleClearResultButtonClick);
            // 
            // _targetOrRootAttributesInfoTip
            // 
            this._targetOrRootAttributesInfoTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._targetOrRootAttributesInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._targetOrRootAttributesInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_targetOrRootAttributesInfoTip.BackgroundImage")));
            this._targetOrRootAttributesInfoTip.Location = new System.Drawing.Point(709, 57);
            this._targetOrRootAttributesInfoTip.Name = "_targetOrRootAttributesInfoTip";
            this._targetOrRootAttributesInfoTip.Size = new System.Drawing.Size(16, 16);
            this._targetOrRootAttributesInfoTip.TabIndex = 15;
            this._targetOrRootAttributesInfoTip.TipText = "Leave blank to score all attribute or specify an attribute query to\r\nspecify a su" +
    "bset of attributes to score.";
            // 
            // _rsdOrDbFileNameBrowseButton
            // 
            this._rsdOrDbFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._rsdOrDbFileNameBrowseButton.FileFilter = "Ruleset definition files (*.rsd;*.rsd.etf)|*.rsd;*.rsd.etf";
            this._rsdOrDbFileNameBrowseButton.Location = new System.Drawing.Point(698, 113);
            this._rsdOrDbFileNameBrowseButton.Name = "_rsdOrDbFileNameBrowseButton";
            this._rsdOrDbFileNameBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._rsdOrDbFileNameBrowseButton.TabIndex = 4;
            this._rsdOrDbFileNameBrowseButton.Text = "...";
            this._rsdOrDbFileNameBrowseButton.TextControl = this._rsdOrDbFileNameTextBox;
            this._rsdOrDbFileNameBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _voaFileNameBrowseButton
            // 
            this._voaFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._voaFileNameBrowseButton.FileFilter = "VOA Files (*.voa)|*.voa";
            this._voaFileNameBrowseButton.Location = new System.Drawing.Point(698, 28);
            this._voaFileNameBrowseButton.Name = "_voaFileNameBrowseButton";
            this._voaFileNameBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._voaFileNameBrowseButton.TabIndex = 1;
            this._voaFileNameBrowseButton.Text = "...";
            this._voaFileNameBrowseButton.TextControl = this._voaFileNameTextBox;
            this._voaFileNameBrowseButton.UseVisualStyleBackColor = true;
            // 
            // ExpressionAndQueryTesterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(737, 631);
            this.Controls.Add(this._targetOrRootAttributesInfoTip);
            this.Controls.Add(this._clearResultButton);
            this.Controls.Add(this._rsdOrDbFileNameBrowseButton);
            this.Controls.Add(this._rsdOrDbFilenameLabel);
            this.Controls.Add(this._rsdOrDbFileNameTextBox);
            this.Controls.Add(this._testButton);
            this.Controls.Add(this._splitContainer);
            this.Controls.Add(this._targetAttributesLabel);
            this.Controls.Add(this._targetAttributeTextBox);
            this.Controls.Add(this._testQueryRadioButton);
            this.Controls.Add(this._testExpressionRadioButton);
            this.Controls.Add(this._voaFileNameBrowseButton);
            this.Controls.Add(label1);
            this.Controls.Add(this._voaFileNameTextBox);
            this.MinimumSize = new System.Drawing.Size(400, 325);
            this.Name = "ExpressionAndQueryTesterForm";
            this.Text = "Expression and query tester";
            ((System.ComponentModel.ISupportInitialize)(this._expressionOrQueryScintillaBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._resultsDataGridView)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ScintillaNET.Scintilla _expressionOrQueryScintillaBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox _voaFileNameTextBox;
        private Extract.Utilities.Forms.BrowseButton _voaFileNameBrowseButton;
        private System.Windows.Forms.RadioButton _testExpressionRadioButton;
        private System.Windows.Forms.RadioButton _testQueryRadioButton;
        private System.Windows.Forms.TextBox _targetAttributeTextBox;
        private System.Windows.Forms.SplitContainer _splitContainer;
        private System.Windows.Forms.Button _testButton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView _resultsDataGridView;
        private System.Windows.Forms.Label _targetAttributesLabel;
        private System.Windows.Forms.TextBox _rsdOrDbFileNameTextBox;
        private System.Windows.Forms.Label _rsdOrDbFilenameLabel;
        private Utilities.Forms.BrowseButton _rsdOrDbFileNameBrowseButton;
        private System.Windows.Forms.Button _clearResultButton;
        private Utilities.Forms.InfoTip _targetOrRootAttributesInfoTip;
    }
}

