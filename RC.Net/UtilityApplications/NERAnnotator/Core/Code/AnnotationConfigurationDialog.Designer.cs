namespace Extract.UtilityApplications.NERAnnotation
{
    partial class AnnotationConfigurationDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnnotationConfigurationDialog));
            this._processButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._duplicateButton = new System.Windows.Forms.Button();
            this._downButton = new Extract.Utilities.Forms.ExtractDownButton();
            this._upButton = new Extract.Utilities.Forms.ExtractUpButton();
            this._entityDefinitionDataGridView = new System.Windows.Forms.DataGridView();
            this._removeButton = new System.Windows.Forms.Button();
            this._addButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._randomlyTakeFromTrainingSetRadioButton = new System.Windows.Forms.RadioButton();
            this._useSpecifiedTestingSetRadioButton = new System.Windows.Forms.RadioButton();
            this._randomSeedForSetDivisionTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this._percentToUseForTestingSetTextBox = new System.Windows.Forms.TextBox();
            this._testingInputTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._trainingInputTextBox = new System.Windows.Forms.TextBox();
            this._sentenceDetectorGroupBox = new System.Windows.Forms.GroupBox();
            this._sentenceDetectorPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._sentenceDetectorPathTextBox = new System.Windows.Forms.TextBox();
            this._sentenceDetectorPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._splitIntoSentencesCheckBox = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this._tokenizerPathTextBox = new System.Windows.Forms.TextBox();
            this._tokenizerGroupBox = new System.Windows.Forms.GroupBox();
            this._learnableTokenizerRadioButton = new System.Windows.Forms.RadioButton();
            this._simpleTokenizerRadioButton = new System.Windows.Forms.RadioButton();
            this._whitespaceTokenizerRadioButton = new System.Windows.Forms.RadioButton();
            this._tokenizerPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._tokenizerPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this.label10 = new System.Windows.Forms.Label();
            this.newMachineButton = new System.Windows.Forms.Button();
            this.saveMachineButton = new System.Windows.Forms.Button();
            this.saveMachineAsButton = new System.Windows.Forms.Button();
            this.openMachineButton = new System.Windows.Forms.Button();
            this._outputSeparateFileForEachCategoryCheckBox = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this._randomSeedForPageInclusionTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this._percentNonInterestingPagesToIncludeTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this._outputFileBaseNameTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this._typesVoaFunctionTextBox = new System.Windows.Forms.TextBox();
            this._typesVoaFunctionTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this.label1 = new System.Windows.Forms.Label();
            this._workingDirTextBox = new System.Windows.Forms.TextBox();
            this._failIfOutputFileExistsCheckBox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this._fkbVersionTextBox = new System.Windows.Forms.TextBox();
            this._configurePreprocessingFunctionButton = new System.Windows.Forms.Button();
            this._processParallelButton = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._entityDefinitionDataGridView)).BeginInit();
            this.groupBox1.SuspendLayout();
            this._sentenceDetectorGroupBox.SuspendLayout();
            this._tokenizerGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _processButton
            // 
            this._processButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._processButton.Location = new System.Drawing.Point(7, 638);
            this._processButton.Name = "_processButton";
            this._processButton.Size = new System.Drawing.Size(150, 23);
            this._processButton.TabIndex = 13;
            this._processButton.Text = "Process pages serially";
            this._processButton.UseVisualStyleBackColor = true;
            this._processButton.Click += new System.EventHandler(this.HandleProcessButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this._duplicateButton);
            this.groupBox2.Controls.Add(this._downButton);
            this.groupBox2.Controls.Add(this._upButton);
            this.groupBox2.Controls.Add(this._entityDefinitionDataGridView);
            this.groupBox2.Controls.Add(this._removeButton);
            this.groupBox2.Controls.Add(this._addButton);
            this.groupBox2.Location = new System.Drawing.Point(7, 462);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(602, 170);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Label each token with a definition where the matching attribute overlaps it spati" +
    "ally";
            // 
            // _duplicateButton
            // 
            this._duplicateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._duplicateButton.Location = new System.Drawing.Point(494, 55);
            this._duplicateButton.Name = "_duplicateButton";
            this._duplicateButton.Size = new System.Drawing.Size(87, 23);
            this._duplicateButton.TabIndex = 2;
            this._duplicateButton.Text = "Duplicate";
            this._duplicateButton.UseVisualStyleBackColor = true;
            this._duplicateButton.Click += new System.EventHandler(this.HandleDuplicateButton_Click);
            // 
            // _downButton
            // 
            this._downButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._downButton.Image = ((System.Drawing.Image)(resources.GetObject("_downButton.Image")));
            this._downButton.Location = new System.Drawing.Point(546, 113);
            this._downButton.Name = "_downButton";
            this._downButton.Size = new System.Drawing.Size(35, 35);
            this._downButton.TabIndex = 5;
            this._downButton.UseVisualStyleBackColor = true;
            this._downButton.Click += new System.EventHandler(this.HandleDownButton_Click);
            // 
            // _upButton
            // 
            this._upButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._upButton.Image = ((System.Drawing.Image)(resources.GetObject("_upButton.Image")));
            this._upButton.Location = new System.Drawing.Point(494, 113);
            this._upButton.Name = "_upButton";
            this._upButton.Size = new System.Drawing.Size(35, 35);
            this._upButton.TabIndex = 4;
            this._upButton.UseVisualStyleBackColor = true;
            this._upButton.Click += new System.EventHandler(this.HandleUpButton_Click);
            // 
            // _entityDefinitionDataGridView
            // 
            this._entityDefinitionDataGridView.AllowUserToAddRows = false;
            this._entityDefinitionDataGridView.AllowUserToDeleteRows = false;
            this._entityDefinitionDataGridView.AllowUserToOrderColumns = true;
            this._entityDefinitionDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._entityDefinitionDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._entityDefinitionDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this._entityDefinitionDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._entityDefinitionDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._entityDefinitionDataGridView.Location = new System.Drawing.Point(9, 26);
            this._entityDefinitionDataGridView.MultiSelect = false;
            this._entityDefinitionDataGridView.Name = "_entityDefinitionDataGridView";
            this._entityDefinitionDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this._entityDefinitionDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._entityDefinitionDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this._entityDefinitionDataGridView.Size = new System.Drawing.Size(470, 132);
            this._entityDefinitionDataGridView.TabIndex = 0;
            this._entityDefinitionDataGridView.TabStop = false;
            this._entityDefinitionDataGridView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleEntityDefinitionDataGridView_RowEnter);
            // 
            // _removeButton
            // 
            this._removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._removeButton.Location = new System.Drawing.Point(494, 84);
            this._removeButton.Name = "_removeButton";
            this._removeButton.Size = new System.Drawing.Size(87, 23);
            this._removeButton.TabIndex = 3;
            this._removeButton.Text = "Remove";
            this._removeButton.UseVisualStyleBackColor = true;
            this._removeButton.Click += new System.EventHandler(this.HandleRemoveButton_Click);
            // 
            // _addButton
            // 
            this._addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addButton.Location = new System.Drawing.Point(494, 26);
            this._addButton.Name = "_addButton";
            this._addButton.Size = new System.Drawing.Size(87, 23);
            this._addButton.TabIndex = 1;
            this._addButton.Text = "Add";
            this._addButton.UseVisualStyleBackColor = true;
            this._addButton.Click += new System.EventHandler(this.HandleAddButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._randomlyTakeFromTrainingSetRadioButton);
            this.groupBox1.Controls.Add(this._useSpecifiedTestingSetRadioButton);
            this.groupBox1.Controls.Add(this._randomSeedForSetDivisionTextBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this._percentToUseForTestingSetTextBox);
            this.groupBox1.Controls.Add(this._testingInputTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this._trainingInputTextBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 32);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(597, 106);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Input images (image file list or recursive folder search based on .uss file exist" +
    "ence)";
            // 
            // _randomlyTakeFromTrainingSetRadioButton
            // 
            this._randomlyTakeFromTrainingSetRadioButton.AutoSize = true;
            this._randomlyTakeFromTrainingSetRadioButton.Location = new System.Drawing.Point(9, 78);
            this._randomlyTakeFromTrainingSetRadioButton.Name = "_randomlyTakeFromTrainingSetRadioButton";
            this._randomlyTakeFromTrainingSetRadioButton.Size = new System.Drawing.Size(96, 17);
            this._randomlyTakeFromTrainingSetRadioButton.TabIndex = 4;
            this._randomlyTakeFromTrainingSetRadioButton.TabStop = true;
            this._randomlyTakeFromTrainingSetRadioButton.Text = "Randomly take";
            this._randomlyTakeFromTrainingSetRadioButton.UseVisualStyleBackColor = true;
            this._randomlyTakeFromTrainingSetRadioButton.CheckedChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _useSpecifiedTestingSetRadioButton
            // 
            this._useSpecifiedTestingSetRadioButton.AutoSize = true;
            this._useSpecifiedTestingSetRadioButton.Location = new System.Drawing.Point(9, 50);
            this._useSpecifiedTestingSetRadioButton.Name = "_useSpecifiedTestingSetRadioButton";
            this._useSpecifiedTestingSetRadioButton.Size = new System.Drawing.Size(77, 17);
            this._useSpecifiedTestingSetRadioButton.TabIndex = 2;
            this._useSpecifiedTestingSetRadioButton.TabStop = true;
            this._useSpecifiedTestingSetRadioButton.Text = "Testing set";
            this._useSpecifiedTestingSetRadioButton.UseVisualStyleBackColor = true;
            this._useSpecifiedTestingSetRadioButton.CheckedChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _randomSeedForSetDivisionTextBox
            // 
            this._randomSeedForSetDivisionTextBox.Location = new System.Drawing.Point(547, 77);
            this._randomSeedForSetDivisionTextBox.Name = "_randomSeedForSetDivisionTextBox";
            this._randomSeedForSetDivisionTextBox.Size = new System.Drawing.Size(41, 20);
            this._randomSeedForSetDivisionTextBox.TabIndex = 3;
            this._randomSeedForSetDivisionTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(158, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(370, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "% of the pages of the train set to be the test set using, optional, random seed:";
            // 
            // _percentToUseForTestingSetTextBox
            // 
            this._percentToUseForTestingSetTextBox.Location = new System.Drawing.Point(111, 77);
            this._percentToUseForTestingSetTextBox.Name = "_percentToUseForTestingSetTextBox";
            this._percentToUseForTestingSetTextBox.Size = new System.Drawing.Size(41, 20);
            this._percentToUseForTestingSetTextBox.TabIndex = 2;
            this._percentToUseForTestingSetTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _testingInputTextBox
            // 
            this._testingInputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._testingInputTextBox.Location = new System.Drawing.Point(92, 49);
            this._testingInputTextBox.Name = "_testingInputTextBox";
            this._testingInputTextBox.Size = new System.Drawing.Size(496, 20);
            this._testingInputTextBox.TabIndex = 1;
            this._testingInputTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Training set";
            // 
            // _trainingInputTextBox
            // 
            this._trainingInputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._trainingInputTextBox.Location = new System.Drawing.Point(74, 21);
            this._trainingInputTextBox.Name = "_trainingInputTextBox";
            this._trainingInputTextBox.Size = new System.Drawing.Size(514, 20);
            this._trainingInputTextBox.TabIndex = 0;
            this._trainingInputTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _sentenceDetectorGroupBox
            // 
            this._sentenceDetectorGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sentenceDetectorGroupBox.Controls.Add(this._sentenceDetectorPathBrowseButton);
            this._sentenceDetectorGroupBox.Controls.Add(this._sentenceDetectorPathTagsButton);
            this._sentenceDetectorGroupBox.Controls.Add(this._splitIntoSentencesCheckBox);
            this._sentenceDetectorGroupBox.Controls.Add(this._sentenceDetectorPathTextBox);
            this._sentenceDetectorGroupBox.Controls.Add(this.label9);
            this._sentenceDetectorGroupBox.Location = new System.Drawing.Point(7, 305);
            this._sentenceDetectorGroupBox.Name = "_sentenceDetectorGroupBox";
            this._sentenceDetectorGroupBox.Size = new System.Drawing.Size(602, 69);
            this._sentenceDetectorGroupBox.TabIndex = 10;
            this._sentenceDetectorGroupBox.TabStop = false;
            this._sentenceDetectorGroupBox.Text = "Sentence detector";
            // 
            // _sentenceDetectorPathBrowseButton
            // 
            this._sentenceDetectorPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sentenceDetectorPathBrowseButton.EnsureFileExists = false;
            this._sentenceDetectorPathBrowseButton.EnsurePathExists = false;
            this._sentenceDetectorPathBrowseButton.Location = new System.Drawing.Point(571, 39);
            this._sentenceDetectorPathBrowseButton.Name = "_sentenceDetectorPathBrowseButton";
            this._sentenceDetectorPathBrowseButton.Size = new System.Drawing.Size(24, 22);
            this._sentenceDetectorPathBrowseButton.TabIndex = 4;
            this._sentenceDetectorPathBrowseButton.Text = "...";
            this._sentenceDetectorPathBrowseButton.TextControl = this._sentenceDetectorPathTextBox;
            this._sentenceDetectorPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _sentenceDetectorPathTextBox
            // 
            this._sentenceDetectorPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sentenceDetectorPathTextBox.Location = new System.Drawing.Point(162, 40);
            this._sentenceDetectorPathTextBox.Name = "_sentenceDetectorPathTextBox";
            this._sentenceDetectorPathTextBox.Size = new System.Drawing.Size(379, 20);
            this._sentenceDetectorPathTextBox.TabIndex = 2;
            this._sentenceDetectorPathTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _sentenceDetectorPathTagsButton
            // 
            this._sentenceDetectorPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sentenceDetectorPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_sentenceDetectorPathTagsButton.Image")));
            this._sentenceDetectorPathTagsButton.Location = new System.Drawing.Point(547, 39);
            this._sentenceDetectorPathTagsButton.Name = "_sentenceDetectorPathTagsButton";
            this._sentenceDetectorPathTagsButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._sentenceDetectorPathTagsButton.Size = new System.Drawing.Size(18, 22);
            this._sentenceDetectorPathTagsButton.TabIndex = 3;
            this._sentenceDetectorPathTagsButton.TextControl = this._sentenceDetectorPathTextBox;
            this._sentenceDetectorPathTagsButton.UseVisualStyleBackColor = true;
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
            this._splitIntoSentencesCheckBox.CheckedChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 43);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(150, 13);
            this.label9.TabIndex = 1;
            this.label9.Text = "Sentence detector model path";
            // 
            // _tokenizerPathTextBox
            // 
            this._tokenizerPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tokenizerPathTextBox.Location = new System.Drawing.Point(121, 47);
            this._tokenizerPathTextBox.Name = "_tokenizerPathTextBox";
            this._tokenizerPathTextBox.Size = new System.Drawing.Size(420, 20);
            this._tokenizerPathTextBox.TabIndex = 4;
            this._tokenizerPathTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
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
            this._tokenizerGroupBox.Controls.Add(this.label10);
            this._tokenizerGroupBox.Location = new System.Drawing.Point(7, 380);
            this._tokenizerGroupBox.Name = "_tokenizerGroupBox";
            this._tokenizerGroupBox.Size = new System.Drawing.Size(602, 76);
            this._tokenizerGroupBox.TabIndex = 11;
            this._tokenizerGroupBox.TabStop = false;
            this._tokenizerGroupBox.Text = "Tokenizer";
            // 
            // _learnableTokenizerRadioButton
            // 
            this._learnableTokenizerRadioButton.AutoSize = true;
            this._learnableTokenizerRadioButton.Location = new System.Drawing.Point(256, 20);
            this._learnableTokenizerRadioButton.Name = "_learnableTokenizerRadioButton";
            this._learnableTokenizerRadioButton.Size = new System.Drawing.Size(122, 17);
            this._learnableTokenizerRadioButton.TabIndex = 2;
            this._learnableTokenizerRadioButton.TabStop = true;
            this._learnableTokenizerRadioButton.Text = "Learnable Tokenizer";
            this._learnableTokenizerRadioButton.UseVisualStyleBackColor = true;
            this._learnableTokenizerRadioButton.CheckedChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _simpleTokenizerRadioButton
            // 
            this._simpleTokenizerRadioButton.AutoSize = true;
            this._simpleTokenizerRadioButton.Location = new System.Drawing.Point(144, 20);
            this._simpleTokenizerRadioButton.Name = "_simpleTokenizerRadioButton";
            this._simpleTokenizerRadioButton.Size = new System.Drawing.Size(106, 17);
            this._simpleTokenizerRadioButton.TabIndex = 1;
            this._simpleTokenizerRadioButton.TabStop = true;
            this._simpleTokenizerRadioButton.Text = "Simple Tokenizer";
            this._simpleTokenizerRadioButton.UseVisualStyleBackColor = true;
            this._simpleTokenizerRadioButton.CheckedChanged += new System.EventHandler(this.HandleValueChanged);
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
            this._whitespaceTokenizerRadioButton.CheckedChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _tokenizerPathBrowseButton
            // 
            this._tokenizerPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._tokenizerPathBrowseButton.EnsureFileExists = false;
            this._tokenizerPathBrowseButton.EnsurePathExists = false;
            this._tokenizerPathBrowseButton.Location = new System.Drawing.Point(572, 46);
            this._tokenizerPathBrowseButton.Name = "_tokenizerPathBrowseButton";
            this._tokenizerPathBrowseButton.Size = new System.Drawing.Size(24, 22);
            this._tokenizerPathBrowseButton.TabIndex = 6;
            this._tokenizerPathBrowseButton.Text = "...";
            this._tokenizerPathBrowseButton.TextControl = this._tokenizerPathTextBox;
            this._tokenizerPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _tokenizerPathTagsButton
            // 
            this._tokenizerPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._tokenizerPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_tokenizerPathTagsButton.Image")));
            this._tokenizerPathTagsButton.Location = new System.Drawing.Point(548, 46);
            this._tokenizerPathTagsButton.Name = "_tokenizerPathTagsButton";
            this._tokenizerPathTagsButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._tokenizerPathTagsButton.Size = new System.Drawing.Size(18, 22);
            this._tokenizerPathTagsButton.TabIndex = 5;
            this._tokenizerPathTagsButton.TextControl = this._tokenizerPathTextBox;
            this._tokenizerPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 51);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(109, 13);
            this.label10.TabIndex = 3;
            this.label10.Text = "Tokenizer model path";
            // 
            // newMachineButton
            // 
            this.newMachineButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.newMachineButton.Location = new System.Drawing.Point(351, 638);
            this.newMachineButton.Name = "newMachineButton";
            this.newMachineButton.Size = new System.Drawing.Size(60, 23);
            this.newMachineButton.TabIndex = 15;
            this.newMachineButton.Text = "New";
            this.newMachineButton.UseVisualStyleBackColor = true;
            this.newMachineButton.Click += new System.EventHandler(this.HandleNewButton_Click);
            // 
            // saveMachineButton
            // 
            this.saveMachineButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveMachineButton.Location = new System.Drawing.Point(483, 638);
            this.saveMachineButton.Name = "saveMachineButton";
            this.saveMachineButton.Size = new System.Drawing.Size(60, 23);
            this.saveMachineButton.TabIndex = 17;
            this.saveMachineButton.Text = "Save";
            this.saveMachineButton.UseVisualStyleBackColor = true;
            this.saveMachineButton.Click += new System.EventHandler(this.HandleSaveButton_Click);
            // 
            // saveMachineAsButton
            // 
            this.saveMachineAsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveMachineAsButton.Location = new System.Drawing.Point(549, 638);
            this.saveMachineAsButton.Name = "saveMachineAsButton";
            this.saveMachineAsButton.Size = new System.Drawing.Size(60, 23);
            this.saveMachineAsButton.TabIndex = 18;
            this.saveMachineAsButton.Text = "Save As";
            this.saveMachineAsButton.UseVisualStyleBackColor = true;
            this.saveMachineAsButton.Click += new System.EventHandler(this.HandleSaveAsButton_Click);
            // 
            // openMachineButton
            // 
            this.openMachineButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.openMachineButton.Location = new System.Drawing.Point(417, 638);
            this.openMachineButton.Name = "openMachineButton";
            this.openMachineButton.Size = new System.Drawing.Size(60, 23);
            this.openMachineButton.TabIndex = 16;
            this.openMachineButton.Text = "Open";
            this.openMachineButton.UseVisualStyleBackColor = true;
            this.openMachineButton.Click += new System.EventHandler(this.HandleOpenButton_Click);
            // 
            // _outputSeparateFileForEachCategoryCheckBox
            // 
            this._outputSeparateFileForEachCategoryCheckBox.AutoSize = true;
            this._outputSeparateFileForEachCategoryCheckBox.Enabled = false;
            this._outputSeparateFileForEachCategoryCheckBox.Location = new System.Drawing.Point(15, 237);
            this._outputSeparateFileForEachCategoryCheckBox.Name = "_outputSeparateFileForEachCategoryCheckBox";
            this._outputSeparateFileForEachCategoryCheckBox.Size = new System.Drawing.Size(209, 17);
            this._outputSeparateFileForEachCategoryCheckBox.TabIndex = 6;
            this._outputSeparateFileForEachCategoryCheckBox.Text = "Output separate files for each category";
            this._outputSeparateFileForEachCategoryCheckBox.UseVisualStyleBackColor = true;
            this._outputSeparateFileForEachCategoryCheckBox.CheckedChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 209);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Include";
            // 
            // _randomSeedForPageInclusionTextBox
            // 
            this._randomSeedForPageInclusionTextBox.Location = new System.Drawing.Point(532, 206);
            this._randomSeedForPageInclusionTextBox.Name = "_randomSeedForPageInclusionTextBox";
            this._randomSeedForPageInclusionTextBox.Size = new System.Drawing.Size(41, 20);
            this._randomSeedForPageInclusionTextBox.TabIndex = 5;
            this._randomSeedForPageInclusionTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(107, 209);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(392, 13);
            this.label7.TabIndex = 10;
            this.label7.Text = "% of uninteresting pages (no queried attribute exists) using, optional, random se" +
    "ed:";
            // 
            // _percentNonInterestingPagesToIncludeTextBox
            // 
            this._percentNonInterestingPagesToIncludeTextBox.Location = new System.Drawing.Point(60, 206);
            this._percentNonInterestingPagesToIncludeTextBox.Name = "_percentNonInterestingPagesToIncludeTextBox";
            this._percentNonInterestingPagesToIncludeTextBox.Size = new System.Drawing.Size(41, 20);
            this._percentNonInterestingPagesToIncludeTextBox.TabIndex = 4;
            this._percentNonInterestingPagesToIncludeTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 180);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(288, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Output file base name (.train.txt or .test.txt will be appended)";
            // 
            // _outputFileBaseNameTextBox
            // 
            this._outputFileBaseNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputFileBaseNameTextBox.Location = new System.Drawing.Point(318, 177);
            this._outputFileBaseNameTextBox.Name = "_outputFileBaseNameTextBox";
            this._outputFileBaseNameTextBox.Size = new System.Drawing.Size(284, 20);
            this._outputFileBaseNameTextBox.TabIndex = 3;
            this._outputFileBaseNameTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 152);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(185, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Path tag function of category VOA file";
            // 
            // _typesVoaFunctionTextBox
            // 
            this._typesVoaFunctionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._typesVoaFunctionTextBox.Location = new System.Drawing.Point(209, 149);
            this._typesVoaFunctionTextBox.Name = "_typesVoaFunctionTextBox";
            this._typesVoaFunctionTextBox.Size = new System.Drawing.Size(369, 20);
            this._typesVoaFunctionTextBox.TabIndex = 1;
            this._typesVoaFunctionTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _typesVoaFunctionTagButton
            // 
            this._typesVoaFunctionTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._typesVoaFunctionTagButton.Image = ((System.Drawing.Image)(resources.GetObject("_typesVoaFunctionTagButton.Image")));
            this._typesVoaFunctionTagButton.Location = new System.Drawing.Point(584, 148);
            this._typesVoaFunctionTagButton.Name = "_typesVoaFunctionTagButton";
            this._typesVoaFunctionTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._typesVoaFunctionTagButton.Size = new System.Drawing.Size(18, 22);
            this._typesVoaFunctionTagButton.TabIndex = 2;
            this._typesVoaFunctionTagButton.TextControl = this._typesVoaFunctionTextBox;
            this._typesVoaFunctionTagButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Working dir:";
            // 
            // _workingDirTextBox
            // 
            this._workingDirTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._workingDirTextBox.Location = new System.Drawing.Point(81, 6);
            this._workingDirTextBox.Name = "_workingDirTextBox";
            this._workingDirTextBox.ReadOnly = true;
            this._workingDirTextBox.Size = new System.Drawing.Size(519, 20);
            this._workingDirTextBox.TabIndex = 0;
            // 
            // _failIfOutputFileExistsCheckBox
            // 
            this._failIfOutputFileExistsCheckBox.AutoSize = true;
            this._failIfOutputFileExistsCheckBox.Location = new System.Drawing.Point(263, 237);
            this._failIfOutputFileExistsCheckBox.Name = "_failIfOutputFileExistsCheckBox";
            this._failIfOutputFileExistsCheckBox.Size = new System.Drawing.Size(165, 17);
            this._failIfOutputFileExistsCheckBox.TabIndex = 7;
            this._failIfOutputFileExistsCheckBox.Text = "Fail if output file already exists";
            this._failIfOutputFileExistsCheckBox.UseVisualStyleBackColor = true;
            this._failIfOutputFileExistsCheckBox.CheckedChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 275);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "FKB Version";
            // 
            // _fkbVersionTextBox
            // 
            this._fkbVersionTextBox.Location = new System.Drawing.Point(86, 271);
            this._fkbVersionTextBox.Name = "_fkbVersionTextBox";
            this._fkbVersionTextBox.Size = new System.Drawing.Size(144, 20);
            this._fkbVersionTextBox.TabIndex = 8;
            this._fkbVersionTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _configurePreprocessingFunctionButton
            // 
            this._configurePreprocessingFunctionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._configurePreprocessingFunctionButton.Location = new System.Drawing.Point(440, 270);
            this._configurePreprocessingFunctionButton.Name = "_configurePreprocessingFunctionButton";
            this._configurePreprocessingFunctionButton.Size = new System.Drawing.Size(164, 23);
            this._configurePreprocessingFunctionButton.TabIndex = 9;
            this._configurePreprocessingFunctionButton.Text = "Configure helper functions...";
            this._configurePreprocessingFunctionButton.UseVisualStyleBackColor = true;
            this._configurePreprocessingFunctionButton.Click += new System.EventHandler(this.HandleConfigureFunctionsButton_Click);
            // 
            // _processParallelButton
            // 
            this._processParallelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._processParallelButton.Location = new System.Drawing.Point(163, 638);
            this._processParallelButton.Name = "_processParallelButton";
            this._processParallelButton.Size = new System.Drawing.Size(163, 23);
            this._processParallelButton.TabIndex = 14;
            this._processParallelButton.Text = "Process pages in parallel";
            this._processParallelButton.UseVisualStyleBackColor = true;
            this._processParallelButton.Click += new System.EventHandler(this.HandleProcessParallelButton_Click);
            // 
            // AnnotationConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 668);
            this.Controls.Add(this._processParallelButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._fkbVersionTextBox);
            this.Controls.Add(this._configurePreprocessingFunctionButton);
            this.Controls.Add(this._failIfOutputFileExistsCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._workingDirTextBox);
            this.Controls.Add(this._outputSeparateFileForEachCategoryCheckBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this._randomSeedForPageInclusionTextBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this._percentNonInterestingPagesToIncludeTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this._outputFileBaseNameTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this._typesVoaFunctionTextBox);
            this.Controls.Add(this._typesVoaFunctionTagButton);
            this.Controls.Add(this.newMachineButton);
            this.Controls.Add(this.saveMachineButton);
            this.Controls.Add(this.saveMachineAsButton);
            this.Controls.Add(this.openMachineButton);
            this.Controls.Add(this._sentenceDetectorGroupBox);
            this.Controls.Add(this._tokenizerGroupBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this._processButton);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(632, 707);
            this.Name = "AnnotationConfigurationDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create annotated data for NER training/testing";
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._entityDefinitionDataGridView)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this._sentenceDetectorGroupBox.ResumeLayout(false);
            this._sentenceDetectorGroupBox.PerformLayout();
            this._tokenizerGroupBox.ResumeLayout(false);
            this._tokenizerGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _processButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button _duplicateButton;
        private Utilities.Forms.ExtractDownButton _downButton;
        private Utilities.Forms.ExtractUpButton _upButton;
        private System.Windows.Forms.DataGridView _entityDefinitionDataGridView;
        private System.Windows.Forms.Button _removeButton;
        private System.Windows.Forms.Button _addButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox _testingInputTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _trainingInputTextBox;
        private System.Windows.Forms.TextBox _randomSeedForSetDivisionTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox _percentToUseForTestingSetTextBox;
        private System.Windows.Forms.GroupBox _sentenceDetectorGroupBox;
        private Utilities.Forms.BrowseButton _sentenceDetectorPathBrowseButton;
        private System.Windows.Forms.TextBox _tokenizerPathTextBox;
        private Utilities.Forms.PathTagsButton _sentenceDetectorPathTagsButton;
        private System.Windows.Forms.CheckBox _splitIntoSentencesCheckBox;
        private System.Windows.Forms.TextBox _sentenceDetectorPathTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox _tokenizerGroupBox;
        private System.Windows.Forms.RadioButton _learnableTokenizerRadioButton;
        private System.Windows.Forms.RadioButton _simpleTokenizerRadioButton;
        private System.Windows.Forms.RadioButton _whitespaceTokenizerRadioButton;
        private Utilities.Forms.BrowseButton _tokenizerPathBrowseButton;
        private Utilities.Forms.PathTagsButton _tokenizerPathTagsButton;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button newMachineButton;
        private System.Windows.Forms.Button saveMachineButton;
        private System.Windows.Forms.Button saveMachineAsButton;
        private System.Windows.Forms.Button openMachineButton;
        private System.Windows.Forms.RadioButton _randomlyTakeFromTrainingSetRadioButton;
        private System.Windows.Forms.RadioButton _useSpecifiedTestingSetRadioButton;
        private System.Windows.Forms.CheckBox _outputSeparateFileForEachCategoryCheckBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox _randomSeedForPageInclusionTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox _percentNonInterestingPagesToIncludeTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox _outputFileBaseNameTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox _typesVoaFunctionTextBox;
        private Utilities.Forms.PathTagsButton _typesVoaFunctionTagButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _workingDirTextBox;
        private System.Windows.Forms.CheckBox _failIfOutputFileExistsCheckBox;
        private System.Windows.Forms.TextBox _fkbVersionTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button _configurePreprocessingFunctionButton;
        private System.Windows.Forms.Button _processParallelButton;
    }
}