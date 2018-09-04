namespace Extract.UtilityApplications.LearningMachineEditor
{
    partial class LearningMachineConfiguration
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
                _trainingTestingDialog?.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LearningMachineConfiguration));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.machineStateToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.dangerModeToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.configurationTabControl = new System.Windows.Forms.TabControl();
            this.inputConfigurationTabPage = new System.Windows.Forms.TabPage();
            this.machineUsageGroupBox = new System.Windows.Forms.GroupBox();
            this.deletionRadioButton = new System.Windows.Forms.RadioButton();
            this.attributeCategorizationRadioButton = new System.Windows.Forms.RadioButton();
            this.paginationRadioButton = new System.Windows.Forms.RadioButton();
            this.documentCategorizationRadioButton = new System.Windows.Forms.RadioButton();
            this.inputConfigurationGroupBox = new System.Windows.Forms.GroupBox();
            this.attributeCategorizationInputPanel = new System.Windows.Forms.Panel();
            this.label10 = new System.Windows.Forms.Label();
            this.attributeCategorizationNegativeClassNameTextBox = new System.Windows.Forms.TextBox();
            this.attributeCategorizationCreateCandidateVoaButton = new System.Windows.Forms.Button();
            this.attributeCategorizationRandomNumberSeedLabel = new System.Windows.Forms.Label();
            this.attributeCategorizationRandomNumberSeedTextBox = new System.Windows.Forms.TextBox();
            this.attributeCategorizationTrainingPercentageLabel = new System.Windows.Forms.Label();
            this.attributeCategorizationTrainingPercentageTextBox = new System.Windows.Forms.TextBox();
            this.attributeCategorizationCandidateVoaLabel = new System.Windows.Forms.Label();
            this.attributeCategorizationCandidateVoaPathTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this.attributeCategorizationCandidateVoaTextBox = new System.Windows.Forms.TextBox();
            this.attributeCategorizationFileListOrFolderLabel = new System.Windows.Forms.Label();
            this.attributeCategorizationFileListOrFolderBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this.attributeCategorizationFileListOrFolderTextBox = new System.Windows.Forms.TextBox();
            this.documentCategorizationFolderInputPanel = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.documentCategorizationFolderNegativeClassNameTextBox = new System.Windows.Forms.TextBox();
            this.documentCategorizationFolderFeatureVoaLabel = new System.Windows.Forms.Label();
            this.documentCategorizationFolderFeatureVoaPathTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this.documentCategorizationFolderFeatureVoaTextBox = new System.Windows.Forms.TextBox();
            this.documentCategorizationFolderRandomNumberSeedLabel = new System.Windows.Forms.Label();
            this.documentCategorizationFolderRandomNumberSeedTextBox = new System.Windows.Forms.TextBox();
            this.documentCategorizationFolderTrainingPercentageLabel = new System.Windows.Forms.Label();
            this.documentCategorizationFolderTrainingPercentageTextBox = new System.Windows.Forms.TextBox();
            this.documentCategorizationFolderAnswerLabel = new System.Windows.Forms.Label();
            this.documentCategorizationFolderAnswerPathTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this.documentCategorizationFolderAnswerTextBox = new System.Windows.Forms.TextBox();
            this.documentCategorizationInputFolderLabel = new System.Windows.Forms.Label();
            this.documentCategorizationInputFolderBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this.documentCategorizationInputFolderTextBox = new System.Windows.Forms.TextBox();
            this.folderSearchRadioButton = new System.Windows.Forms.RadioButton();
            this.paginationInputPanel = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.paginationNegativeClassNameTextBox = new System.Windows.Forms.TextBox();
            this.paginationAnswerVoaLabel = new System.Windows.Forms.Label();
            this.paginationAnswerVoaPathTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this.paginationAnswerVoaTextBox = new System.Windows.Forms.TextBox();
            this.paginationRandomNumberSeedLabel = new System.Windows.Forms.Label();
            this.paginationRandomNumberSeedTextBox = new System.Windows.Forms.TextBox();
            this.paginationTrainingPercentageLabel = new System.Windows.Forms.Label();
            this.paginationTrainingPercentageTextBox = new System.Windows.Forms.TextBox();
            this.paginationFeatureVoaLabel = new System.Windows.Forms.Label();
            this.paginationFeatureVoaPathTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this.paginationFeatureVoaTextBox = new System.Windows.Forms.TextBox();
            this.paginationFileListOrFolderLabel = new System.Windows.Forms.Label();
            this.paginationFileListOrFolderBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this.paginationFileListOrFolderTextBox = new System.Windows.Forms.TextBox();
            this.textFileOrCsvRadioButton = new System.Windows.Forms.RadioButton();
            this.documentCategorizationCsvInputPanel = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.documentCategorizationCsvFeatureVoaLabel = new System.Windows.Forms.Label();
            this.documentCategorizationCsvNegativeClassNameTextBox = new System.Windows.Forms.TextBox();
            this.documentCategorizationCsvFeatureVoaPathTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this.documentCategorizationCsvFeatureVoaTextBox = new System.Windows.Forms.TextBox();
            this.documentCategorizationCsvRandomNumberSeedLabel = new System.Windows.Forms.Label();
            this.documentCategorizationCsvRandomNumberSeedTextBox = new System.Windows.Forms.TextBox();
            this.documentCategorizationCsvBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this.documentCategorizationCsvTextBox = new System.Windows.Forms.TextBox();
            this.trainingPercentageLabel = new System.Windows.Forms.Label();
            this.documentCategorizationCsvTrainingPercentageTextBox = new System.Windows.Forms.TextBox();
            this.documentCategorizationCsvLabel = new System.Windows.Forms.Label();
            this.featureConfigurationTabPage = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.writeDataToCsvButton = new System.Windows.Forms.Button();
            this.standardizeFeaturesForCsvOutputCheckBox = new System.Windows.Forms.CheckBox();
            this.editFeaturesButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.computeFeaturesButton = new System.Windows.Forms.Button();
            this.csvOutputBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this.csvOutputTextBox = new System.Windows.Forms.TextBox();
            this.viewAnswerListButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.maxFeaturesPerVectorizerTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.maxShingleSizeForAttributeFeaturesTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.attributesToTokenizeFilterTextBox = new System.Windows.Forms.TextBox();
            this.tokenizeAttributesFilterCheckBox = new System.Windows.Forms.CheckBox();
            this.useFeatureAttributeFilterLabel = new System.Windows.Forms.Label();
            this.attributeFeatureFilterComboBox = new System.Windows.Forms.ComboBox();
            this.attributeFeatureFilterTextBox = new System.Windows.Forms.TextBox();
            this.useAttributeFeatureFilterCheckBox = new System.Windows.Forms.CheckBox();
            this.autoBagOfWordsGroupBox = new System.Windows.Forms.GroupBox();
            this.useFeatureHashingForAutoBagOfWordsCheckBox = new System.Windows.Forms.CheckBox();
            this.paginationPagesLabel = new System.Windows.Forms.Label();
            this.specifiedPagesTextBox = new System.Windows.Forms.TextBox();
            this.specifiedPagesCheckBox = new System.Windows.Forms.CheckBox();
            this.maxFeaturesTextBox = new System.Windows.Forms.TextBox();
            this.maxFeaturesLabel = new System.Windows.Forms.Label();
            this.maxShingleSizeTextBox = new System.Windows.Forms.TextBox();
            this.maxShingleSizeLabel = new System.Windows.Forms.Label();
            this.useAutoBagOfWordsCheckBox = new System.Windows.Forms.CheckBox();
            this.machineConfigurationTabPage = new System.Windows.Forms.TabPage();
            this.probabilisticSvmPanel = new System.Windows.Forms.Panel();
            this.calibrateForProbabilitiesCheckBox = new System.Windows.Forms.CheckBox();
            this.useUnknownCheckBox = new System.Windows.Forms.CheckBox();
            this.unknownCutoffTextBox = new System.Windows.Forms.TextBox();
            this.translateUnknownTextBox = new System.Windows.Forms.TextBox();
            this.translateUnknownCheckbox = new System.Windows.Forms.CheckBox();
            this.svmPanel = new System.Windows.Forms.Panel();
            this.svmScoreTypeGroupBox = new System.Windows.Forms.GroupBox();
            this.svmUsePrecisionRadioButton = new System.Windows.Forms.RadioButton();
            this.svmUseRecallRadioButton = new System.Windows.Forms.RadioButton();
            this.svmUseF1ScoreRadioButton = new System.Windows.Forms.RadioButton();
            this.svmConditionallyApplyWeightRatioCheckBox = new System.Windows.Forms.CheckBox();
            this.svmCacheSizeTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.svmWeightRatioTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.svmAutoComplexityCheckBox = new System.Windows.Forms.CheckBox();
            this.svmComplexityTextBox = new System.Windows.Forms.TextBox();
            this.svmComplexityLabel = new System.Windows.Forms.Label();
            this.machineTypeLabel = new System.Windows.Forms.Label();
            this.neuralNetPanel = new System.Windows.Forms.Panel();
            this.numberOfCandidateNetwordsTextBox = new System.Windows.Forms.TextBox();
            this.numberOfCandidateNetworksLabel = new System.Windows.Forms.Label();
            this.useCrossValidationSetsCheckBox = new System.Windows.Forms.CheckBox();
            this.maximumTrainingIterationsTextBox = new System.Windows.Forms.TextBox();
            this.maximumTrainingIterationsLabel = new System.Windows.Forms.Label();
            this.sigmoidAlphaTextBox = new System.Windows.Forms.TextBox();
            this.sigmoidAlphaLabel = new System.Windows.Forms.Label();
            this.sizeOfHiddenLayersTextBox = new System.Windows.Forms.TextBox();
            this.sizeOfHiddenLayersLabel = new System.Windows.Forms.Label();
            this.machineTypeComboBox = new System.Windows.Forms.ComboBox();
            this.saveMachineAsButton = new System.Windows.Forms.Button();
            this.trainTestButton = new System.Windows.Forms.Button();
            this.configurationErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.openMachineButton = new System.Windows.Forms.Button();
            this.saveMachineButton = new System.Windows.Forms.Button();
            this.newMachineButton = new System.Windows.Forms.Button();
            this.dangerModeButton = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.configurationTabControl.SuspendLayout();
            this.inputConfigurationTabPage.SuspendLayout();
            this.machineUsageGroupBox.SuspendLayout();
            this.inputConfigurationGroupBox.SuspendLayout();
            this.attributeCategorizationInputPanel.SuspendLayout();
            this.documentCategorizationFolderInputPanel.SuspendLayout();
            this.paginationInputPanel.SuspendLayout();
            this.documentCategorizationCsvInputPanel.SuspendLayout();
            this.featureConfigurationTabPage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.autoBagOfWordsGroupBox.SuspendLayout();
            this.machineConfigurationTabPage.SuspendLayout();
            this.probabilisticSvmPanel.SuspendLayout();
            this.svmPanel.SuspendLayout();
            this.svmScoreTypeGroupBox.SuspendLayout();
            this.neuralNetPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.configurationErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.machineStateToolStripStatusLabel,
            this.dangerModeToolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 711);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1362, 22);
            this.statusStrip1.TabIndex = 3;
            // 
            // machineStateToolStripStatusLabel
            // 
            this.machineStateToolStripStatusLabel.Name = "machineStateToolStripStatusLabel";
            this.machineStateToolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // dangerModeToolStripStatusLabel
            // 
            this.dangerModeToolStripStatusLabel.Name = "dangerModeToolStripStatusLabel";
            this.dangerModeToolStripStatusLabel.Size = new System.Drawing.Size(71, 17);
            this.dangerModeToolStripStatusLabel.Text = "[Safe mode]";
            // 
            // configurationTabControl
            // 
            this.configurationTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.configurationTabControl.Controls.Add(this.inputConfigurationTabPage);
            this.configurationTabControl.Controls.Add(this.featureConfigurationTabPage);
            this.configurationTabControl.Controls.Add(this.machineConfigurationTabPage);
            this.configurationErrorProvider.SetIconAlignment(this.configurationTabControl, System.Windows.Forms.ErrorIconAlignment.TopLeft);
            this.configurationTabControl.Location = new System.Drawing.Point(0, 5);
            this.configurationTabControl.Name = "configurationTabControl";
            this.configurationTabControl.SelectedIndex = 0;
            this.configurationTabControl.Size = new System.Drawing.Size(1362, 674);
            this.configurationTabControl.TabIndex = 0;
            this.configurationTabControl.SelectedIndexChanged += new System.EventHandler(this.HandleConfigurationTabControl_SelectedIndexChanged);
            // 
            // inputConfigurationTabPage
            // 
            this.inputConfigurationTabPage.Controls.Add(this.machineUsageGroupBox);
            this.inputConfigurationTabPage.Controls.Add(this.inputConfigurationGroupBox);
            this.inputConfigurationTabPage.Location = new System.Drawing.Point(4, 22);
            this.inputConfigurationTabPage.Name = "inputConfigurationTabPage";
            this.inputConfigurationTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.inputConfigurationTabPage.Size = new System.Drawing.Size(1354, 648);
            this.inputConfigurationTabPage.TabIndex = 0;
            this.inputConfigurationTabPage.Text = "Input configuration";
            this.inputConfigurationTabPage.UseVisualStyleBackColor = true;
            // 
            // machineUsageGroupBox
            // 
            this.machineUsageGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.machineUsageGroupBox.Controls.Add(this.deletionRadioButton);
            this.machineUsageGroupBox.Controls.Add(this.attributeCategorizationRadioButton);
            this.machineUsageGroupBox.Controls.Add(this.paginationRadioButton);
            this.machineUsageGroupBox.Controls.Add(this.documentCategorizationRadioButton);
            this.machineUsageGroupBox.Location = new System.Drawing.Point(3, 6);
            this.machineUsageGroupBox.Name = "machineUsageGroupBox";
            this.machineUsageGroupBox.Size = new System.Drawing.Size(1343, 90);
            this.machineUsageGroupBox.TabIndex = 0;
            this.machineUsageGroupBox.TabStop = false;
            this.machineUsageGroupBox.Text = "Machine learning engine usage";
            // 
            // deletionRadioButton
            // 
            this.deletionRadioButton.AutoSize = true;
            this.deletionRadioButton.Location = new System.Drawing.Point(97, 42);
            this.deletionRadioButton.Name = "deletionRadioButton";
            this.deletionRadioButton.Size = new System.Drawing.Size(64, 17);
            this.deletionRadioButton.TabIndex = 3;
            this.deletionRadioButton.Text = "Deletion";
            this.deletionRadioButton.UseVisualStyleBackColor = true;
            // 
            // attributeCategorizationRadioButton
            // 
            this.attributeCategorizationRadioButton.AutoSize = true;
            this.attributeCategorizationRadioButton.Location = new System.Drawing.Point(16, 65);
            this.attributeCategorizationRadioButton.Name = "attributeCategorizationRadioButton";
            this.attributeCategorizationRadioButton.Size = new System.Drawing.Size(133, 17);
            this.attributeCategorizationRadioButton.TabIndex = 2;
            this.attributeCategorizationRadioButton.Text = "Attribute categorization";
            this.attributeCategorizationRadioButton.UseVisualStyleBackColor = true;
            this.attributeCategorizationRadioButton.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // paginationRadioButton
            // 
            this.paginationRadioButton.AutoSize = true;
            this.paginationRadioButton.Location = new System.Drawing.Point(16, 42);
            this.paginationRadioButton.Name = "paginationRadioButton";
            this.paginationRadioButton.Size = new System.Drawing.Size(75, 17);
            this.paginationRadioButton.TabIndex = 1;
            this.paginationRadioButton.Text = "Pagination";
            this.paginationRadioButton.UseVisualStyleBackColor = true;
            this.paginationRadioButton.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // documentCategorizationRadioButton
            // 
            this.documentCategorizationRadioButton.AutoSize = true;
            this.documentCategorizationRadioButton.Checked = true;
            this.documentCategorizationRadioButton.Location = new System.Drawing.Point(16, 19);
            this.documentCategorizationRadioButton.Name = "documentCategorizationRadioButton";
            this.documentCategorizationRadioButton.Size = new System.Drawing.Size(143, 17);
            this.documentCategorizationRadioButton.TabIndex = 0;
            this.documentCategorizationRadioButton.TabStop = true;
            this.documentCategorizationRadioButton.Text = "Document categorization";
            this.documentCategorizationRadioButton.UseVisualStyleBackColor = true;
            this.documentCategorizationRadioButton.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // inputConfigurationGroupBox
            // 
            this.inputConfigurationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputConfigurationGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.inputConfigurationGroupBox.Controls.Add(this.attributeCategorizationInputPanel);
            this.inputConfigurationGroupBox.Controls.Add(this.documentCategorizationFolderInputPanel);
            this.inputConfigurationGroupBox.Controls.Add(this.folderSearchRadioButton);
            this.inputConfigurationGroupBox.Controls.Add(this.paginationInputPanel);
            this.inputConfigurationGroupBox.Controls.Add(this.textFileOrCsvRadioButton);
            this.inputConfigurationGroupBox.Controls.Add(this.documentCategorizationCsvInputPanel);
            this.inputConfigurationGroupBox.Location = new System.Drawing.Point(3, 98);
            this.inputConfigurationGroupBox.Name = "inputConfigurationGroupBox";
            this.inputConfigurationGroupBox.Size = new System.Drawing.Size(1343, 544);
            this.inputConfigurationGroupBox.TabIndex = 1;
            this.inputConfigurationGroupBox.TabStop = false;
            this.inputConfigurationGroupBox.Text = "Training/testing files";
            // 
            // attributeCategorizationInputPanel
            // 
            this.attributeCategorizationInputPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.attributeCategorizationInputPanel.Controls.Add(this.label10);
            this.attributeCategorizationInputPanel.Controls.Add(this.attributeCategorizationNegativeClassNameTextBox);
            this.attributeCategorizationInputPanel.Controls.Add(this.attributeCategorizationCreateCandidateVoaButton);
            this.attributeCategorizationInputPanel.Controls.Add(this.attributeCategorizationRandomNumberSeedLabel);
            this.attributeCategorizationInputPanel.Controls.Add(this.attributeCategorizationRandomNumberSeedTextBox);
            this.attributeCategorizationInputPanel.Controls.Add(this.attributeCategorizationTrainingPercentageLabel);
            this.attributeCategorizationInputPanel.Controls.Add(this.attributeCategorizationTrainingPercentageTextBox);
            this.attributeCategorizationInputPanel.Controls.Add(this.attributeCategorizationCandidateVoaLabel);
            this.attributeCategorizationInputPanel.Controls.Add(this.attributeCategorizationCandidateVoaPathTagButton);
            this.attributeCategorizationInputPanel.Controls.Add(this.attributeCategorizationCandidateVoaTextBox);
            this.attributeCategorizationInputPanel.Controls.Add(this.attributeCategorizationFileListOrFolderLabel);
            this.attributeCategorizationInputPanel.Controls.Add(this.attributeCategorizationFileListOrFolderBrowseButton);
            this.attributeCategorizationInputPanel.Controls.Add(this.attributeCategorizationFileListOrFolderTextBox);
            this.attributeCategorizationInputPanel.Location = new System.Drawing.Point(1094, 22);
            this.attributeCategorizationInputPanel.Name = "attributeCategorizationInputPanel";
            this.attributeCategorizationInputPanel.Size = new System.Drawing.Size(536, 244);
            this.attributeCategorizationInputPanel.TabIndex = 5;
            this.attributeCategorizationInputPanel.Visible = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(156, 140);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(289, 13);
            this.label10.TabIndex = 16;
            this.label10.Text = "Negative-class name (e.g., for determining Precision/Recall)";
            // 
            // attributeCategorizationNegativeClassNameTextBox
            // 
            this.attributeCategorizationNegativeClassNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.attributeCategorizationNegativeClassNameTextBox.Location = new System.Drawing.Point(159, 158);
            this.attributeCategorizationNegativeClassNameTextBox.Name = "attributeCategorizationNegativeClassNameTextBox";
            this.attributeCategorizationNegativeClassNameTextBox.Size = new System.Drawing.Size(336, 20);
            this.attributeCategorizationNegativeClassNameTextBox.TabIndex = 11;
            // 
            // attributeCategorizationCreateCandidateVoaButton
            // 
            this.attributeCategorizationCreateCandidateVoaButton.Location = new System.Drawing.Point(10, 107);
            this.attributeCategorizationCreateCandidateVoaButton.Name = "attributeCategorizationCreateCandidateVoaButton";
            this.attributeCategorizationCreateCandidateVoaButton.Size = new System.Drawing.Size(213, 23);
            this.attributeCategorizationCreateCandidateVoaButton.TabIndex = 14;
            this.attributeCategorizationCreateCandidateVoaButton.Text = "Create labeled candidate attribute files...";
            this.attributeCategorizationCreateCandidateVoaButton.UseVisualStyleBackColor = true;
            this.attributeCategorizationCreateCandidateVoaButton.Click += new System.EventHandler(this.HandleAttributeCategorizationCreateCandidateVoaButton_Click);
            // 
            // attributeCategorizationRandomNumberSeedLabel
            // 
            this.attributeCategorizationRandomNumberSeedLabel.AutoSize = true;
            this.attributeCategorizationRandomNumberSeedLabel.Location = new System.Drawing.Point(8, 185);
            this.attributeCategorizationRandomNumberSeedLabel.Name = "attributeCategorizationRandomNumberSeedLabel";
            this.attributeCategorizationRandomNumberSeedLabel.Size = new System.Drawing.Size(111, 13);
            this.attributeCategorizationRandomNumberSeedLabel.TabIndex = 12;
            this.attributeCategorizationRandomNumberSeedLabel.Text = "Random number seed";
            // 
            // attributeCategorizationRandomNumberSeedTextBox
            // 
            this.attributeCategorizationRandomNumberSeedTextBox.Location = new System.Drawing.Point(10, 203);
            this.attributeCategorizationRandomNumberSeedTextBox.Name = "attributeCategorizationRandomNumberSeedTextBox";
            this.attributeCategorizationRandomNumberSeedTextBox.Size = new System.Drawing.Size(71, 20);
            this.attributeCategorizationRandomNumberSeedTextBox.TabIndex = 13;
            this.attributeCategorizationRandomNumberSeedTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.attributeCategorizationRandomNumberSeedTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.attributeCategorizationRandomNumberSeedTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // attributeCategorizationTrainingPercentageLabel
            // 
            this.attributeCategorizationTrainingPercentageLabel.AutoSize = true;
            this.attributeCategorizationTrainingPercentageLabel.Location = new System.Drawing.Point(8, 140);
            this.attributeCategorizationTrainingPercentageLabel.Name = "attributeCategorizationTrainingPercentageLabel";
            this.attributeCategorizationTrainingPercentageLabel.Size = new System.Drawing.Size(73, 13);
            this.attributeCategorizationTrainingPercentageLabel.TabIndex = 9;
            this.attributeCategorizationTrainingPercentageLabel.Text = "Training set %";
            // 
            // attributeCategorizationTrainingPercentageTextBox
            // 
            this.attributeCategorizationTrainingPercentageTextBox.Location = new System.Drawing.Point(10, 158);
            this.attributeCategorizationTrainingPercentageTextBox.Name = "attributeCategorizationTrainingPercentageTextBox";
            this.attributeCategorizationTrainingPercentageTextBox.Size = new System.Drawing.Size(24, 20);
            this.attributeCategorizationTrainingPercentageTextBox.TabIndex = 10;
            this.attributeCategorizationTrainingPercentageTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.attributeCategorizationTrainingPercentageTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.attributeCategorizationTrainingPercentageTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // attributeCategorizationCandidateVoaLabel
            // 
            this.attributeCategorizationCandidateVoaLabel.AutoSize = true;
            this.attributeCategorizationCandidateVoaLabel.Location = new System.Drawing.Point(8, 59);
            this.attributeCategorizationCandidateVoaLabel.Name = "attributeCategorizationCandidateVoaLabel";
            this.attributeCategorizationCandidateVoaLabel.Size = new System.Drawing.Size(452, 13);
            this.attributeCategorizationCandidateVoaLabel.TabIndex = 3;
            this.attributeCategorizationCandidateVoaLabel.Text = "Labeled candidate attribute files (relative path to VOA or EAV file for each <Sou" +
    "rceDocName>)";
            // 
            // attributeCategorizationCandidateVoaPathTagButton
            // 
            this.attributeCategorizationCandidateVoaPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.attributeCategorizationCandidateVoaPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("attributeCategorizationCandidateVoaPathTagButton.Image")));
            this.attributeCategorizationCandidateVoaPathTagButton.Location = new System.Drawing.Point(501, 75);
            this.attributeCategorizationCandidateVoaPathTagButton.Name = "attributeCategorizationCandidateVoaPathTagButton";
            this.attributeCategorizationCandidateVoaPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this.attributeCategorizationCandidateVoaPathTagButton.Size = new System.Drawing.Size(24, 24);
            this.attributeCategorizationCandidateVoaPathTagButton.TabIndex = 5;
            this.attributeCategorizationCandidateVoaPathTagButton.TextControl = this.attributeCategorizationCandidateVoaTextBox;
            this.attributeCategorizationCandidateVoaPathTagButton.UseVisualStyleBackColor = true;
            // 
            // attributeCategorizationCandidateVoaTextBox
            // 
            this.attributeCategorizationCandidateVoaTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.attributeCategorizationCandidateVoaTextBox.Location = new System.Drawing.Point(10, 77);
            this.attributeCategorizationCandidateVoaTextBox.Name = "attributeCategorizationCandidateVoaTextBox";
            this.attributeCategorizationCandidateVoaTextBox.Size = new System.Drawing.Size(485, 20);
            this.attributeCategorizationCandidateVoaTextBox.TabIndex = 4;
            this.attributeCategorizationCandidateVoaTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.attributeCategorizationCandidateVoaTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // attributeCategorizationFileListOrFolderLabel
            // 
            this.attributeCategorizationFileListOrFolderLabel.AutoSize = true;
            this.attributeCategorizationFileListOrFolderLabel.Location = new System.Drawing.Point(8, 13);
            this.attributeCategorizationFileListOrFolderLabel.Name = "attributeCategorizationFileListOrFolderLabel";
            this.attributeCategorizationFileListOrFolderLabel.Size = new System.Drawing.Size(98, 13);
            this.attributeCategorizationFileListOrFolderLabel.TabIndex = 0;
            this.attributeCategorizationFileListOrFolderLabel.Text = "Train/testing file list";
            // 
            // attributeCategorizationFileListOrFolderBrowseButton
            // 
            this.attributeCategorizationFileListOrFolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.attributeCategorizationFileListOrFolderBrowseButton.EnsureFileExists = false;
            this.attributeCategorizationFileListOrFolderBrowseButton.EnsurePathExists = false;
            this.attributeCategorizationFileListOrFolderBrowseButton.Location = new System.Drawing.Point(501, 30);
            this.attributeCategorizationFileListOrFolderBrowseButton.Name = "attributeCategorizationFileListOrFolderBrowseButton";
            this.attributeCategorizationFileListOrFolderBrowseButton.Size = new System.Drawing.Size(24, 24);
            this.attributeCategorizationFileListOrFolderBrowseButton.TabIndex = 2;
            this.attributeCategorizationFileListOrFolderBrowseButton.Text = "...";
            this.attributeCategorizationFileListOrFolderBrowseButton.TextControl = this.attributeCategorizationFileListOrFolderTextBox;
            this.attributeCategorizationFileListOrFolderBrowseButton.UseVisualStyleBackColor = true;
            // 
            // attributeCategorizationFileListOrFolderTextBox
            // 
            this.attributeCategorizationFileListOrFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.attributeCategorizationFileListOrFolderTextBox.Location = new System.Drawing.Point(10, 32);
            this.attributeCategorizationFileListOrFolderTextBox.Name = "attributeCategorizationFileListOrFolderTextBox";
            this.attributeCategorizationFileListOrFolderTextBox.Size = new System.Drawing.Size(485, 20);
            this.attributeCategorizationFileListOrFolderTextBox.TabIndex = 1;
            this.attributeCategorizationFileListOrFolderTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.attributeCategorizationFileListOrFolderTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // documentCategorizationFolderInputPanel
            // 
            this.documentCategorizationFolderInputPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.documentCategorizationFolderInputPanel.Controls.Add(this.label9);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationFolderNegativeClassNameTextBox);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationFolderFeatureVoaLabel);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationFolderFeatureVoaPathTagButton);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationFolderFeatureVoaTextBox);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationFolderRandomNumberSeedLabel);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationFolderRandomNumberSeedTextBox);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationFolderTrainingPercentageLabel);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationFolderTrainingPercentageTextBox);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationFolderAnswerLabel);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationFolderAnswerPathTagButton);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationFolderAnswerTextBox);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationInputFolderLabel);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationInputFolderBrowseButton);
            this.documentCategorizationFolderInputPanel.Controls.Add(this.documentCategorizationInputFolderTextBox);
            this.documentCategorizationFolderInputPanel.Location = new System.Drawing.Point(550, 272);
            this.documentCategorizationFolderInputPanel.Name = "documentCategorizationFolderInputPanel";
            this.documentCategorizationFolderInputPanel.Size = new System.Drawing.Size(535, 246);
            this.documentCategorizationFolderInputPanel.TabIndex = 4;
            this.documentCategorizationFolderInputPanel.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(154, 151);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(289, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Negative-class name (e.g., for determining Precision/Recall)";
            // 
            // documentCategorizationFolderNegativeClassNameTextBox
            // 
            this.documentCategorizationFolderNegativeClassNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.documentCategorizationFolderNegativeClassNameTextBox.Location = new System.Drawing.Point(157, 169);
            this.documentCategorizationFolderNegativeClassNameTextBox.Name = "documentCategorizationFolderNegativeClassNameTextBox";
            this.documentCategorizationFolderNegativeClassNameTextBox.Size = new System.Drawing.Size(336, 20);
            this.documentCategorizationFolderNegativeClassNameTextBox.TabIndex = 11;
            this.documentCategorizationFolderNegativeClassNameTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.documentCategorizationFolderNegativeClassNameTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // documentCategorizationFolderFeatureVoaLabel
            // 
            this.documentCategorizationFolderFeatureVoaLabel.AutoSize = true;
            this.documentCategorizationFolderFeatureVoaLabel.Location = new System.Drawing.Point(8, 59);
            this.documentCategorizationFolderFeatureVoaLabel.Name = "documentCategorizationFolderFeatureVoaLabel";
            this.documentCategorizationFolderFeatureVoaLabel.Size = new System.Drawing.Size(400, 13);
            this.documentCategorizationFolderFeatureVoaLabel.TabIndex = 3;
            this.documentCategorizationFolderFeatureVoaLabel.Text = "Feature-attribute files (relative path to VOA or EAV file for each <SourceDocName" +
    ">)";
            // 
            // documentCategorizationFolderFeatureVoaPathTagButton
            // 
            this.documentCategorizationFolderFeatureVoaPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.documentCategorizationFolderFeatureVoaPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("documentCategorizationFolderFeatureVoaPathTagButton.Image")));
            this.documentCategorizationFolderFeatureVoaPathTagButton.Location = new System.Drawing.Point(501, 75);
            this.documentCategorizationFolderFeatureVoaPathTagButton.Name = "documentCategorizationFolderFeatureVoaPathTagButton";
            this.documentCategorizationFolderFeatureVoaPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this.documentCategorizationFolderFeatureVoaPathTagButton.Size = new System.Drawing.Size(24, 24);
            this.documentCategorizationFolderFeatureVoaPathTagButton.TabIndex = 5;
            this.documentCategorizationFolderFeatureVoaPathTagButton.TextControl = this.documentCategorizationFolderFeatureVoaTextBox;
            this.documentCategorizationFolderFeatureVoaPathTagButton.UseVisualStyleBackColor = true;
            // 
            // documentCategorizationFolderFeatureVoaTextBox
            // 
            this.documentCategorizationFolderFeatureVoaTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.documentCategorizationFolderFeatureVoaTextBox.Location = new System.Drawing.Point(10, 77);
            this.documentCategorizationFolderFeatureVoaTextBox.Name = "documentCategorizationFolderFeatureVoaTextBox";
            this.documentCategorizationFolderFeatureVoaTextBox.Size = new System.Drawing.Size(483, 20);
            this.documentCategorizationFolderFeatureVoaTextBox.TabIndex = 4;
            this.documentCategorizationFolderFeatureVoaTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.documentCategorizationFolderFeatureVoaTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // documentCategorizationFolderRandomNumberSeedLabel
            // 
            this.documentCategorizationFolderRandomNumberSeedLabel.AutoSize = true;
            this.documentCategorizationFolderRandomNumberSeedLabel.Location = new System.Drawing.Point(8, 196);
            this.documentCategorizationFolderRandomNumberSeedLabel.Name = "documentCategorizationFolderRandomNumberSeedLabel";
            this.documentCategorizationFolderRandomNumberSeedLabel.Size = new System.Drawing.Size(111, 13);
            this.documentCategorizationFolderRandomNumberSeedLabel.TabIndex = 12;
            this.documentCategorizationFolderRandomNumberSeedLabel.Text = "Random number seed";
            // 
            // documentCategorizationFolderRandomNumberSeedTextBox
            // 
            this.documentCategorizationFolderRandomNumberSeedTextBox.Location = new System.Drawing.Point(10, 214);
            this.documentCategorizationFolderRandomNumberSeedTextBox.Name = "documentCategorizationFolderRandomNumberSeedTextBox";
            this.documentCategorizationFolderRandomNumberSeedTextBox.Size = new System.Drawing.Size(71, 20);
            this.documentCategorizationFolderRandomNumberSeedTextBox.TabIndex = 13;
            this.documentCategorizationFolderRandomNumberSeedTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.documentCategorizationFolderRandomNumberSeedTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.documentCategorizationFolderRandomNumberSeedTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // documentCategorizationFolderTrainingPercentageLabel
            // 
            this.documentCategorizationFolderTrainingPercentageLabel.AutoSize = true;
            this.documentCategorizationFolderTrainingPercentageLabel.Location = new System.Drawing.Point(8, 151);
            this.documentCategorizationFolderTrainingPercentageLabel.Name = "documentCategorizationFolderTrainingPercentageLabel";
            this.documentCategorizationFolderTrainingPercentageLabel.Size = new System.Drawing.Size(73, 13);
            this.documentCategorizationFolderTrainingPercentageLabel.TabIndex = 9;
            this.documentCategorizationFolderTrainingPercentageLabel.Text = "Training set %";
            // 
            // documentCategorizationFolderTrainingPercentageTextBox
            // 
            this.documentCategorizationFolderTrainingPercentageTextBox.Location = new System.Drawing.Point(10, 169);
            this.documentCategorizationFolderTrainingPercentageTextBox.Name = "documentCategorizationFolderTrainingPercentageTextBox";
            this.documentCategorizationFolderTrainingPercentageTextBox.Size = new System.Drawing.Size(24, 20);
            this.documentCategorizationFolderTrainingPercentageTextBox.TabIndex = 10;
            this.documentCategorizationFolderTrainingPercentageTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.documentCategorizationFolderTrainingPercentageTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.documentCategorizationFolderTrainingPercentageTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // documentCategorizationFolderAnswerLabel
            // 
            this.documentCategorizationFolderAnswerLabel.AutoSize = true;
            this.documentCategorizationFolderAnswerLabel.Location = new System.Drawing.Point(8, 105);
            this.documentCategorizationFolderAnswerLabel.Name = "documentCategorizationFolderAnswerLabel";
            this.documentCategorizationFolderAnswerLabel.Size = new System.Drawing.Size(254, 13);
            this.documentCategorizationFolderAnswerLabel.TabIndex = 6;
            this.documentCategorizationFolderAnswerLabel.Text = "Answers (a path tag function of <SourceDocName>)";
            // 
            // documentCategorizationFolderAnswerPathTagButton
            // 
            this.documentCategorizationFolderAnswerPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.documentCategorizationFolderAnswerPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("documentCategorizationFolderAnswerPathTagButton.Image")));
            this.documentCategorizationFolderAnswerPathTagButton.Location = new System.Drawing.Point(501, 121);
            this.documentCategorizationFolderAnswerPathTagButton.Name = "documentCategorizationFolderAnswerPathTagButton";
            this.documentCategorizationFolderAnswerPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this.documentCategorizationFolderAnswerPathTagButton.Size = new System.Drawing.Size(24, 24);
            this.documentCategorizationFolderAnswerPathTagButton.TabIndex = 8;
            this.documentCategorizationFolderAnswerPathTagButton.TextControl = this.documentCategorizationFolderAnswerTextBox;
            this.documentCategorizationFolderAnswerPathTagButton.UseVisualStyleBackColor = true;
            // 
            // documentCategorizationFolderAnswerTextBox
            // 
            this.documentCategorizationFolderAnswerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.documentCategorizationFolderAnswerTextBox.Location = new System.Drawing.Point(10, 123);
            this.documentCategorizationFolderAnswerTextBox.Name = "documentCategorizationFolderAnswerTextBox";
            this.documentCategorizationFolderAnswerTextBox.Size = new System.Drawing.Size(483, 20);
            this.documentCategorizationFolderAnswerTextBox.TabIndex = 7;
            this.documentCategorizationFolderAnswerTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.documentCategorizationFolderAnswerTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // documentCategorizationInputFolderLabel
            // 
            this.documentCategorizationInputFolderLabel.AutoSize = true;
            this.documentCategorizationInputFolderLabel.Location = new System.Drawing.Point(8, 13);
            this.documentCategorizationInputFolderLabel.Name = "documentCategorizationInputFolderLabel";
            this.documentCategorizationInputFolderLabel.Size = new System.Drawing.Size(88, 13);
            this.documentCategorizationInputFolderLabel.TabIndex = 0;
            this.documentCategorizationInputFolderLabel.Text = "Train/testing files";
            // 
            // documentCategorizationInputFolderBrowseButton
            // 
            this.documentCategorizationInputFolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.documentCategorizationInputFolderBrowseButton.EnsureFileExists = false;
            this.documentCategorizationInputFolderBrowseButton.EnsurePathExists = false;
            this.documentCategorizationInputFolderBrowseButton.FolderBrowser = true;
            this.documentCategorizationInputFolderBrowseButton.Location = new System.Drawing.Point(501, 30);
            this.documentCategorizationInputFolderBrowseButton.Name = "documentCategorizationInputFolderBrowseButton";
            this.documentCategorizationInputFolderBrowseButton.Size = new System.Drawing.Size(24, 24);
            this.documentCategorizationInputFolderBrowseButton.TabIndex = 2;
            this.documentCategorizationInputFolderBrowseButton.Text = "...";
            this.documentCategorizationInputFolderBrowseButton.TextControl = this.documentCategorizationInputFolderTextBox;
            this.documentCategorizationInputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.documentCategorizationInputFolderBrowseButton.Click += new System.EventHandler(this.HandleBrowseButtonClick);
            // 
            // documentCategorizationInputFolderTextBox
            // 
            this.documentCategorizationInputFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.documentCategorizationInputFolderTextBox.Location = new System.Drawing.Point(10, 32);
            this.documentCategorizationInputFolderTextBox.Name = "documentCategorizationInputFolderTextBox";
            this.documentCategorizationInputFolderTextBox.Size = new System.Drawing.Size(483, 20);
            this.documentCategorizationInputFolderTextBox.TabIndex = 1;
            this.documentCategorizationInputFolderTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.documentCategorizationInputFolderTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // folderSearchRadioButton
            // 
            this.folderSearchRadioButton.AutoSize = true;
            this.folderSearchRadioButton.Checked = true;
            this.folderSearchRadioButton.Location = new System.Drawing.Point(16, 44);
            this.folderSearchRadioButton.Name = "folderSearchRadioButton";
            this.folderSearchRadioButton.Size = new System.Drawing.Size(137, 17);
            this.folderSearchRadioButton.TabIndex = 1;
            this.folderSearchRadioButton.TabStop = true;
            this.folderSearchRadioButton.Text = "Recursive folder search";
            this.folderSearchRadioButton.UseVisualStyleBackColor = true;
            // 
            // paginationInputPanel
            // 
            this.paginationInputPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.paginationInputPanel.Controls.Add(this.label8);
            this.paginationInputPanel.Controls.Add(this.paginationNegativeClassNameTextBox);
            this.paginationInputPanel.Controls.Add(this.paginationAnswerVoaLabel);
            this.paginationInputPanel.Controls.Add(this.paginationAnswerVoaPathTagButton);
            this.paginationInputPanel.Controls.Add(this.paginationAnswerVoaTextBox);
            this.paginationInputPanel.Controls.Add(this.paginationRandomNumberSeedLabel);
            this.paginationInputPanel.Controls.Add(this.paginationRandomNumberSeedTextBox);
            this.paginationInputPanel.Controls.Add(this.paginationTrainingPercentageLabel);
            this.paginationInputPanel.Controls.Add(this.paginationTrainingPercentageTextBox);
            this.paginationInputPanel.Controls.Add(this.paginationFeatureVoaLabel);
            this.paginationInputPanel.Controls.Add(this.paginationFeatureVoaPathTagButton);
            this.paginationInputPanel.Controls.Add(this.paginationFeatureVoaTextBox);
            this.paginationInputPanel.Controls.Add(this.paginationFileListOrFolderLabel);
            this.paginationInputPanel.Controls.Add(this.paginationFileListOrFolderBrowseButton);
            this.paginationInputPanel.Controls.Add(this.paginationFileListOrFolderTextBox);
            this.paginationInputPanel.Location = new System.Drawing.Point(548, 22);
            this.paginationInputPanel.Name = "paginationInputPanel";
            this.paginationInputPanel.Size = new System.Drawing.Size(536, 244);
            this.paginationInputPanel.TabIndex = 3;
            this.paginationInputPanel.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(156, 151);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(106, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Negative-class name";
            // 
            // paginationNegativeClassNameTextBox
            // 
            this.paginationNegativeClassNameTextBox.Location = new System.Drawing.Point(159, 169);
            this.paginationNegativeClassNameTextBox.Name = "paginationNegativeClassNameTextBox";
            this.paginationNegativeClassNameTextBox.Size = new System.Drawing.Size(336, 20);
            this.paginationNegativeClassNameTextBox.TabIndex = 11;
            this.paginationNegativeClassNameTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.paginationNegativeClassNameTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // paginationAnswerVoaLabel
            // 
            this.paginationAnswerVoaLabel.AutoSize = true;
            this.paginationAnswerVoaLabel.Location = new System.Drawing.Point(8, 105);
            this.paginationAnswerVoaLabel.Name = "paginationAnswerVoaLabel";
            this.paginationAnswerVoaLabel.Size = new System.Drawing.Size(342, 13);
            this.paginationAnswerVoaLabel.TabIndex = 6;
            this.paginationAnswerVoaLabel.Text = "Answers (relative path to VOA or EAV file for each <SourceDocName>)";
            // 
            // paginationAnswerVoaPathTagButton
            // 
            this.paginationAnswerVoaPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.paginationAnswerVoaPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("paginationAnswerVoaPathTagButton.Image")));
            this.paginationAnswerVoaPathTagButton.Location = new System.Drawing.Point(501, 121);
            this.paginationAnswerVoaPathTagButton.Name = "paginationAnswerVoaPathTagButton";
            this.paginationAnswerVoaPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this.paginationAnswerVoaPathTagButton.Size = new System.Drawing.Size(24, 24);
            this.paginationAnswerVoaPathTagButton.TabIndex = 8;
            this.paginationAnswerVoaPathTagButton.TextControl = this.paginationAnswerVoaTextBox;
            this.paginationAnswerVoaPathTagButton.UseVisualStyleBackColor = true;
            // 
            // paginationAnswerVoaTextBox
            // 
            this.paginationAnswerVoaTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.paginationAnswerVoaTextBox.Location = new System.Drawing.Point(10, 123);
            this.paginationAnswerVoaTextBox.Name = "paginationAnswerVoaTextBox";
            this.paginationAnswerVoaTextBox.Size = new System.Drawing.Size(485, 20);
            this.paginationAnswerVoaTextBox.TabIndex = 7;
            this.paginationAnswerVoaTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.paginationAnswerVoaTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // paginationRandomNumberSeedLabel
            // 
            this.paginationRandomNumberSeedLabel.AutoSize = true;
            this.paginationRandomNumberSeedLabel.Location = new System.Drawing.Point(8, 196);
            this.paginationRandomNumberSeedLabel.Name = "paginationRandomNumberSeedLabel";
            this.paginationRandomNumberSeedLabel.Size = new System.Drawing.Size(111, 13);
            this.paginationRandomNumberSeedLabel.TabIndex = 12;
            this.paginationRandomNumberSeedLabel.Text = "Random number seed";
            // 
            // paginationRandomNumberSeedTextBox
            // 
            this.paginationRandomNumberSeedTextBox.Location = new System.Drawing.Point(10, 214);
            this.paginationRandomNumberSeedTextBox.Name = "paginationRandomNumberSeedTextBox";
            this.paginationRandomNumberSeedTextBox.Size = new System.Drawing.Size(71, 20);
            this.paginationRandomNumberSeedTextBox.TabIndex = 13;
            this.paginationRandomNumberSeedTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.paginationRandomNumberSeedTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.paginationRandomNumberSeedTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // paginationTrainingPercentageLabel
            // 
            this.paginationTrainingPercentageLabel.AutoSize = true;
            this.paginationTrainingPercentageLabel.Location = new System.Drawing.Point(8, 151);
            this.paginationTrainingPercentageLabel.Name = "paginationTrainingPercentageLabel";
            this.paginationTrainingPercentageLabel.Size = new System.Drawing.Size(73, 13);
            this.paginationTrainingPercentageLabel.TabIndex = 9;
            this.paginationTrainingPercentageLabel.Text = "Training set %";
            // 
            // paginationTrainingPercentageTextBox
            // 
            this.paginationTrainingPercentageTextBox.Location = new System.Drawing.Point(10, 169);
            this.paginationTrainingPercentageTextBox.Name = "paginationTrainingPercentageTextBox";
            this.paginationTrainingPercentageTextBox.Size = new System.Drawing.Size(24, 20);
            this.paginationTrainingPercentageTextBox.TabIndex = 10;
            this.paginationTrainingPercentageTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.paginationTrainingPercentageTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.paginationTrainingPercentageTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // paginationFeatureVoaLabel
            // 
            this.paginationFeatureVoaLabel.AutoSize = true;
            this.paginationFeatureVoaLabel.Location = new System.Drawing.Point(8, 59);
            this.paginationFeatureVoaLabel.Name = "paginationFeatureVoaLabel";
            this.paginationFeatureVoaLabel.Size = new System.Drawing.Size(400, 13);
            this.paginationFeatureVoaLabel.TabIndex = 3;
            this.paginationFeatureVoaLabel.Text = "Feature-attribute files (relative path to VOA or EAV file for each <SourceDocName" +
    ">)";
            // 
            // paginationFeatureVoaPathTagButton
            // 
            this.paginationFeatureVoaPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.paginationFeatureVoaPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("paginationFeatureVoaPathTagButton.Image")));
            this.paginationFeatureVoaPathTagButton.Location = new System.Drawing.Point(501, 75);
            this.paginationFeatureVoaPathTagButton.Name = "paginationFeatureVoaPathTagButton";
            this.paginationFeatureVoaPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this.paginationFeatureVoaPathTagButton.Size = new System.Drawing.Size(24, 24);
            this.paginationFeatureVoaPathTagButton.TabIndex = 5;
            this.paginationFeatureVoaPathTagButton.TextControl = this.paginationFeatureVoaTextBox;
            this.paginationFeatureVoaPathTagButton.UseVisualStyleBackColor = true;
            // 
            // paginationFeatureVoaTextBox
            // 
            this.paginationFeatureVoaTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.paginationFeatureVoaTextBox.Location = new System.Drawing.Point(10, 77);
            this.paginationFeatureVoaTextBox.Name = "paginationFeatureVoaTextBox";
            this.paginationFeatureVoaTextBox.Size = new System.Drawing.Size(485, 20);
            this.paginationFeatureVoaTextBox.TabIndex = 4;
            this.paginationFeatureVoaTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.paginationFeatureVoaTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // paginationFileListOrFolderLabel
            // 
            this.paginationFileListOrFolderLabel.AutoSize = true;
            this.paginationFileListOrFolderLabel.Location = new System.Drawing.Point(8, 13);
            this.paginationFileListOrFolderLabel.Name = "paginationFileListOrFolderLabel";
            this.paginationFileListOrFolderLabel.Size = new System.Drawing.Size(98, 13);
            this.paginationFileListOrFolderLabel.TabIndex = 0;
            this.paginationFileListOrFolderLabel.Text = "Train/testing file list";
            // 
            // paginationFileListOrFolderBrowseButton
            // 
            this.paginationFileListOrFolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.paginationFileListOrFolderBrowseButton.EnsureFileExists = false;
            this.paginationFileListOrFolderBrowseButton.EnsurePathExists = false;
            this.paginationFileListOrFolderBrowseButton.Location = new System.Drawing.Point(501, 30);
            this.paginationFileListOrFolderBrowseButton.Name = "paginationFileListOrFolderBrowseButton";
            this.paginationFileListOrFolderBrowseButton.Size = new System.Drawing.Size(24, 24);
            this.paginationFileListOrFolderBrowseButton.TabIndex = 2;
            this.paginationFileListOrFolderBrowseButton.Text = "...";
            this.paginationFileListOrFolderBrowseButton.TextControl = this.paginationFileListOrFolderTextBox;
            this.paginationFileListOrFolderBrowseButton.UseVisualStyleBackColor = true;
            this.paginationFileListOrFolderBrowseButton.Click += new System.EventHandler(this.HandleBrowseButtonClick);
            // 
            // paginationFileListOrFolderTextBox
            // 
            this.paginationFileListOrFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.paginationFileListOrFolderTextBox.Location = new System.Drawing.Point(10, 32);
            this.paginationFileListOrFolderTextBox.Name = "paginationFileListOrFolderTextBox";
            this.paginationFileListOrFolderTextBox.Size = new System.Drawing.Size(485, 20);
            this.paginationFileListOrFolderTextBox.TabIndex = 1;
            this.paginationFileListOrFolderTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.paginationFileListOrFolderTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // textFileOrCsvRadioButton
            // 
            this.textFileOrCsvRadioButton.AutoSize = true;
            this.textFileOrCsvRadioButton.Location = new System.Drawing.Point(16, 21);
            this.textFileOrCsvRadioButton.Name = "textFileOrCsvRadioButton";
            this.textFileOrCsvRadioButton.Size = new System.Drawing.Size(98, 17);
            this.textFileOrCsvRadioButton.TabIndex = 0;
            this.textFileOrCsvRadioButton.Text = "Text file or CSV";
            this.textFileOrCsvRadioButton.UseVisualStyleBackColor = true;
            this.textFileOrCsvRadioButton.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // documentCategorizationCsvInputPanel
            // 
            this.documentCategorizationCsvInputPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.documentCategorizationCsvInputPanel.Controls.Add(this.label3);
            this.documentCategorizationCsvInputPanel.Controls.Add(this.documentCategorizationCsvFeatureVoaLabel);
            this.documentCategorizationCsvInputPanel.Controls.Add(this.documentCategorizationCsvNegativeClassNameTextBox);
            this.documentCategorizationCsvInputPanel.Controls.Add(this.documentCategorizationCsvFeatureVoaPathTagButton);
            this.documentCategorizationCsvInputPanel.Controls.Add(this.documentCategorizationCsvFeatureVoaTextBox);
            this.documentCategorizationCsvInputPanel.Controls.Add(this.documentCategorizationCsvRandomNumberSeedLabel);
            this.documentCategorizationCsvInputPanel.Controls.Add(this.documentCategorizationCsvRandomNumberSeedTextBox);
            this.documentCategorizationCsvInputPanel.Controls.Add(this.documentCategorizationCsvBrowseButton);
            this.documentCategorizationCsvInputPanel.Controls.Add(this.trainingPercentageLabel);
            this.documentCategorizationCsvInputPanel.Controls.Add(this.documentCategorizationCsvTrainingPercentageTextBox);
            this.documentCategorizationCsvInputPanel.Controls.Add(this.documentCategorizationCsvLabel);
            this.documentCategorizationCsvInputPanel.Controls.Add(this.documentCategorizationCsvTextBox);
            this.documentCategorizationCsvInputPanel.Location = new System.Drawing.Point(6, 67);
            this.documentCategorizationCsvInputPanel.Name = "documentCategorizationCsvInputPanel";
            this.documentCategorizationCsvInputPanel.Size = new System.Drawing.Size(532, 450);
            this.documentCategorizationCsvInputPanel.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(152, 105);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(289, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Negative-class name (e.g., for determining Precision/Recall)";
            // 
            // documentCategorizationCsvFeatureVoaLabel
            // 
            this.documentCategorizationCsvFeatureVoaLabel.AutoSize = true;
            this.documentCategorizationCsvFeatureVoaLabel.Location = new System.Drawing.Point(8, 59);
            this.documentCategorizationCsvFeatureVoaLabel.Name = "documentCategorizationCsvFeatureVoaLabel";
            this.documentCategorizationCsvFeatureVoaLabel.Size = new System.Drawing.Size(400, 13);
            this.documentCategorizationCsvFeatureVoaLabel.TabIndex = 3;
            this.documentCategorizationCsvFeatureVoaLabel.Text = "Feature-attribute files (relative path to VOA or EAV file for each <SourceDocName" +
    ">)";
            // 
            // documentCategorizationCsvNegativeClassNameTextBox
            // 
            this.documentCategorizationCsvNegativeClassNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.documentCategorizationCsvNegativeClassNameTextBox.Location = new System.Drawing.Point(155, 123);
            this.documentCategorizationCsvNegativeClassNameTextBox.Name = "documentCategorizationCsvNegativeClassNameTextBox";
            this.documentCategorizationCsvNegativeClassNameTextBox.Size = new System.Drawing.Size(336, 20);
            this.documentCategorizationCsvNegativeClassNameTextBox.TabIndex = 8;
            this.documentCategorizationCsvNegativeClassNameTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.documentCategorizationCsvNegativeClassNameTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // documentCategorizationCsvFeatureVoaPathTagButton
            // 
            this.documentCategorizationCsvFeatureVoaPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.documentCategorizationCsvFeatureVoaPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("documentCategorizationCsvFeatureVoaPathTagButton.Image")));
            this.documentCategorizationCsvFeatureVoaPathTagButton.Location = new System.Drawing.Point(497, 75);
            this.documentCategorizationCsvFeatureVoaPathTagButton.Name = "documentCategorizationCsvFeatureVoaPathTagButton";
            this.documentCategorizationCsvFeatureVoaPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this.documentCategorizationCsvFeatureVoaPathTagButton.Size = new System.Drawing.Size(24, 24);
            this.documentCategorizationCsvFeatureVoaPathTagButton.TabIndex = 5;
            this.documentCategorizationCsvFeatureVoaPathTagButton.TextControl = this.documentCategorizationCsvFeatureVoaTextBox;
            this.documentCategorizationCsvFeatureVoaPathTagButton.UseVisualStyleBackColor = true;
            // 
            // documentCategorizationCsvFeatureVoaTextBox
            // 
            this.documentCategorizationCsvFeatureVoaTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.documentCategorizationCsvFeatureVoaTextBox.Location = new System.Drawing.Point(10, 77);
            this.documentCategorizationCsvFeatureVoaTextBox.Name = "documentCategorizationCsvFeatureVoaTextBox";
            this.documentCategorizationCsvFeatureVoaTextBox.Size = new System.Drawing.Size(481, 20);
            this.documentCategorizationCsvFeatureVoaTextBox.TabIndex = 4;
            this.documentCategorizationCsvFeatureVoaTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.documentCategorizationCsvFeatureVoaTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // documentCategorizationCsvRandomNumberSeedLabel
            // 
            this.documentCategorizationCsvRandomNumberSeedLabel.AutoSize = true;
            this.documentCategorizationCsvRandomNumberSeedLabel.Location = new System.Drawing.Point(8, 150);
            this.documentCategorizationCsvRandomNumberSeedLabel.Name = "documentCategorizationCsvRandomNumberSeedLabel";
            this.documentCategorizationCsvRandomNumberSeedLabel.Size = new System.Drawing.Size(111, 13);
            this.documentCategorizationCsvRandomNumberSeedLabel.TabIndex = 9;
            this.documentCategorizationCsvRandomNumberSeedLabel.Text = "Random number seed";
            // 
            // documentCategorizationCsvRandomNumberSeedTextBox
            // 
            this.documentCategorizationCsvRandomNumberSeedTextBox.Location = new System.Drawing.Point(10, 168);
            this.documentCategorizationCsvRandomNumberSeedTextBox.Name = "documentCategorizationCsvRandomNumberSeedTextBox";
            this.documentCategorizationCsvRandomNumberSeedTextBox.Size = new System.Drawing.Size(71, 20);
            this.documentCategorizationCsvRandomNumberSeedTextBox.TabIndex = 10;
            this.documentCategorizationCsvRandomNumberSeedTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.documentCategorizationCsvRandomNumberSeedTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.documentCategorizationCsvRandomNumberSeedTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // documentCategorizationCsvBrowseButton
            // 
            this.documentCategorizationCsvBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.documentCategorizationCsvBrowseButton.EnsureFileExists = false;
            this.documentCategorizationCsvBrowseButton.EnsurePathExists = false;
            this.documentCategorizationCsvBrowseButton.Location = new System.Drawing.Point(497, 30);
            this.documentCategorizationCsvBrowseButton.Name = "documentCategorizationCsvBrowseButton";
            this.documentCategorizationCsvBrowseButton.Size = new System.Drawing.Size(24, 24);
            this.documentCategorizationCsvBrowseButton.TabIndex = 2;
            this.documentCategorizationCsvBrowseButton.Text = "...";
            this.documentCategorizationCsvBrowseButton.TextControl = this.documentCategorizationCsvTextBox;
            this.documentCategorizationCsvBrowseButton.UseVisualStyleBackColor = true;
            this.documentCategorizationCsvBrowseButton.Click += new System.EventHandler(this.HandleBrowseButtonClick);
            // 
            // documentCategorizationCsvTextBox
            // 
            this.documentCategorizationCsvTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.documentCategorizationCsvTextBox.Location = new System.Drawing.Point(10, 32);
            this.documentCategorizationCsvTextBox.Name = "documentCategorizationCsvTextBox";
            this.documentCategorizationCsvTextBox.Size = new System.Drawing.Size(481, 20);
            this.documentCategorizationCsvTextBox.TabIndex = 1;
            this.documentCategorizationCsvTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.documentCategorizationCsvTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // trainingPercentageLabel
            // 
            this.trainingPercentageLabel.AutoSize = true;
            this.trainingPercentageLabel.Location = new System.Drawing.Point(8, 105);
            this.trainingPercentageLabel.Name = "trainingPercentageLabel";
            this.trainingPercentageLabel.Size = new System.Drawing.Size(73, 13);
            this.trainingPercentageLabel.TabIndex = 6;
            this.trainingPercentageLabel.Text = "Training set %";
            // 
            // documentCategorizationCsvTrainingPercentageTextBox
            // 
            this.documentCategorizationCsvTrainingPercentageTextBox.Location = new System.Drawing.Point(10, 123);
            this.documentCategorizationCsvTrainingPercentageTextBox.Name = "documentCategorizationCsvTrainingPercentageTextBox";
            this.documentCategorizationCsvTrainingPercentageTextBox.Size = new System.Drawing.Size(24, 20);
            this.documentCategorizationCsvTrainingPercentageTextBox.TabIndex = 7;
            this.documentCategorizationCsvTrainingPercentageTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.documentCategorizationCsvTrainingPercentageTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.documentCategorizationCsvTrainingPercentageTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // documentCategorizationCsvLabel
            // 
            this.documentCategorizationCsvLabel.AutoSize = true;
            this.documentCategorizationCsvLabel.Location = new System.Drawing.Point(8, 13);
            this.documentCategorizationCsvLabel.Name = "documentCategorizationCsvLabel";
            this.documentCategorizationCsvLabel.Size = new System.Drawing.Size(91, 13);
            this.documentCategorizationCsvLabel.TabIndex = 0;
            this.documentCategorizationCsvLabel.Text = "Train/testing CSV";
            // 
            // featureConfigurationTabPage
            // 
            this.featureConfigurationTabPage.Controls.Add(this.groupBox2);
            this.featureConfigurationTabPage.Controls.Add(this.groupBox1);
            this.featureConfigurationTabPage.Controls.Add(this.autoBagOfWordsGroupBox);
            this.featureConfigurationTabPage.Location = new System.Drawing.Point(4, 22);
            this.featureConfigurationTabPage.Name = "featureConfigurationTabPage";
            this.featureConfigurationTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.featureConfigurationTabPage.Size = new System.Drawing.Size(1354, 648);
            this.featureConfigurationTabPage.TabIndex = 1;
            this.featureConfigurationTabPage.Text = "Encoder configuration";
            this.featureConfigurationTabPage.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.writeDataToCsvButton);
            this.groupBox2.Controls.Add(this.standardizeFeaturesForCsvOutputCheckBox);
            this.groupBox2.Controls.Add(this.editFeaturesButton);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.computeFeaturesButton);
            this.groupBox2.Controls.Add(this.csvOutputBrowseButton);
            this.groupBox2.Controls.Add(this.viewAnswerListButton);
            this.groupBox2.Controls.Add(this.csvOutputTextBox);
            this.groupBox2.Location = new System.Drawing.Point(3, 429);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1345, 213);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            // 
            // writeDataToCsvButton
            // 
            this.writeDataToCsvButton.Location = new System.Drawing.Point(10, 104);
            this.writeDataToCsvButton.Name = "writeDataToCsvButton";
            this.writeDataToCsvButton.Size = new System.Drawing.Size(101, 23);
            this.writeDataToCsvButton.TabIndex = 9;
            this.writeDataToCsvButton.Text = "Write data to csv";
            this.writeDataToCsvButton.UseVisualStyleBackColor = true;
            this.writeDataToCsvButton.Click += new System.EventHandler(this.HandleWriteDataToCsvButton_Click);
            // 
            // standardizeFeaturesForCsvOutputCheckBox
            // 
            this.standardizeFeaturesForCsvOutputCheckBox.AutoSize = true;
            this.standardizeFeaturesForCsvOutputCheckBox.Checked = true;
            this.standardizeFeaturesForCsvOutputCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.standardizeFeaturesForCsvOutputCheckBox.Location = new System.Drawing.Point(117, 108);
            this.standardizeFeaturesForCsvOutputCheckBox.Name = "standardizeFeaturesForCsvOutputCheckBox";
            this.standardizeFeaturesForCsvOutputCheckBox.Size = new System.Drawing.Size(328, 17);
            this.standardizeFeaturesForCsvOutputCheckBox.TabIndex = 13;
            this.standardizeFeaturesForCsvOutputCheckBox.Text = "Standardize features (subtract mean and divide by std deviation)";
            this.standardizeFeaturesForCsvOutputCheckBox.UseVisualStyleBackColor = true;
            this.standardizeFeaturesForCsvOutputCheckBox.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // editFeaturesButton
            // 
            this.editFeaturesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.editFeaturesButton.Enabled = false;
            this.editFeaturesButton.Location = new System.Drawing.Point(1127, 18);
            this.editFeaturesButton.Name = "editFeaturesButton";
            this.editFeaturesButton.Size = new System.Drawing.Size(101, 23);
            this.editFeaturesButton.TabIndex = 4;
            this.editFeaturesButton.Text = "Edit feature list...";
            this.editFeaturesButton.UseVisualStyleBackColor = true;
            this.editFeaturesButton.Click += new System.EventHandler(this.HandleEditFeaturesButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(340, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "CSV base name (.train.csv or .test.csv will be appended to base-name)";
            // 
            // computeFeaturesButton
            // 
            this.computeFeaturesButton.Location = new System.Drawing.Point(10, 18);
            this.computeFeaturesButton.Name = "computeFeaturesButton";
            this.computeFeaturesButton.Size = new System.Drawing.Size(220, 23);
            this.computeFeaturesButton.TabIndex = 3;
            this.computeFeaturesButton.Text = "Compute vectorizer(s) and answer domain";
            this.computeFeaturesButton.UseVisualStyleBackColor = true;
            this.computeFeaturesButton.Click += new System.EventHandler(this.HandleComputeFeaturesButton_Click);
            // 
            // csvOutputBrowseButton
            // 
            this.csvOutputBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.csvOutputBrowseButton.EnsureFileExists = false;
            this.csvOutputBrowseButton.EnsurePathExists = false;
            this.csvOutputBrowseButton.Location = new System.Drawing.Point(1313, 77);
            this.csvOutputBrowseButton.Name = "csvOutputBrowseButton";
            this.csvOutputBrowseButton.Size = new System.Drawing.Size(24, 22);
            this.csvOutputBrowseButton.TabIndex = 11;
            this.csvOutputBrowseButton.Text = "...";
            this.csvOutputBrowseButton.TextControl = this.csvOutputTextBox;
            this.csvOutputBrowseButton.UseVisualStyleBackColor = true;
            // 
            // csvOutputTextBox
            // 
            this.csvOutputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.csvOutputTextBox.Location = new System.Drawing.Point(10, 78);
            this.csvOutputTextBox.Name = "csvOutputTextBox";
            this.csvOutputTextBox.Size = new System.Drawing.Size(1295, 20);
            this.csvOutputTextBox.TabIndex = 10;
            this.csvOutputTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.csvOutputTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // viewAnswerListButton
            // 
            this.viewAnswerListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.viewAnswerListButton.Enabled = false;
            this.viewAnswerListButton.Location = new System.Drawing.Point(1234, 18);
            this.viewAnswerListButton.Name = "viewAnswerListButton";
            this.viewAnswerListButton.Size = new System.Drawing.Size(101, 23);
            this.viewAnswerListButton.TabIndex = 0;
            this.viewAnswerListButton.Text = "View answer list...";
            this.viewAnswerListButton.UseVisualStyleBackColor = true;
            this.viewAnswerListButton.Click += new System.EventHandler(this.HandleViewAnswerListButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.maxFeaturesPerVectorizerTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.maxShingleSizeForAttributeFeaturesTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.attributesToTokenizeFilterTextBox);
            this.groupBox1.Controls.Add(this.tokenizeAttributesFilterCheckBox);
            this.groupBox1.Controls.Add(this.useFeatureAttributeFilterLabel);
            this.groupBox1.Controls.Add(this.attributeFeatureFilterComboBox);
            this.groupBox1.Controls.Add(this.attributeFeatureFilterTextBox);
            this.groupBox1.Controls.Add(this.useAttributeFeatureFilterCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(3, 156);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1345, 267);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Attribute feature vectorizers";
            // 
            // maxFeaturesPerVectorizerTextBox
            // 
            this.maxFeaturesPerVectorizerTextBox.Location = new System.Drawing.Point(10, 113);
            this.maxFeaturesPerVectorizerTextBox.Name = "maxFeaturesPerVectorizerTextBox";
            this.maxFeaturesPerVectorizerTextBox.Size = new System.Drawing.Size(132, 20);
            this.maxFeaturesPerVectorizerTextBox.TabIndex = 5;
            this.maxFeaturesPerVectorizerTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.maxFeaturesPerVectorizerTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.maxFeaturesPerVectorizerTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(282, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Max features per vectorizer (applies to DiscreteTerms only)";
            // 
            // maxShingleSizeForAttributeFeaturesTextBox
            // 
            this.maxShingleSizeForAttributeFeaturesTextBox.Location = new System.Drawing.Point(10, 235);
            this.maxShingleSizeForAttributeFeaturesTextBox.Name = "maxShingleSizeForAttributeFeaturesTextBox";
            this.maxShingleSizeForAttributeFeaturesTextBox.Size = new System.Drawing.Size(26, 20);
            this.maxShingleSizeForAttributeFeaturesTextBox.TabIndex = 9;
            this.maxShingleSizeForAttributeFeaturesTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.maxShingleSizeForAttributeFeaturesTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.maxShingleSizeForAttributeFeaturesTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 216);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Max shingle size";
            // 
            // attributesToTokenizeFilterTextBox
            // 
            this.attributesToTokenizeFilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.attributesToTokenizeFilterTextBox.Enabled = false;
            this.attributesToTokenizeFilterTextBox.Location = new System.Drawing.Point(10, 165);
            this.attributesToTokenizeFilterTextBox.Multiline = true;
            this.attributesToTokenizeFilterTextBox.Name = "attributesToTokenizeFilterTextBox";
            this.attributesToTokenizeFilterTextBox.Size = new System.Drawing.Size(1323, 44);
            this.attributesToTokenizeFilterTextBox.TabIndex = 7;
            this.attributesToTokenizeFilterTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.attributesToTokenizeFilterTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // tokenizeAttributesFilterCheckBox
            // 
            this.tokenizeAttributesFilterCheckBox.AutoSize = true;
            this.tokenizeAttributesFilterCheckBox.Checked = true;
            this.tokenizeAttributesFilterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tokenizeAttributesFilterCheckBox.Location = new System.Drawing.Point(10, 142);
            this.tokenizeAttributesFilterCheckBox.Name = "tokenizeAttributesFilterCheckBox";
            this.tokenizeAttributesFilterCheckBox.Size = new System.Drawing.Size(330, 17);
            this.tokenizeAttributesFilterCheckBox.TabIndex = 6;
            this.tokenizeAttributesFilterCheckBox.Text = "Tokenize/make shingles from the attributes that match this query";
            this.tokenizeAttributesFilterCheckBox.UseVisualStyleBackColor = true;
            this.tokenizeAttributesFilterCheckBox.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // useFeatureAttributeFilterLabel
            // 
            this.useFeatureAttributeFilterLabel.AutoSize = true;
            this.useFeatureAttributeFilterLabel.Location = new System.Drawing.Point(247, 21);
            this.useFeatureAttributeFilterLabel.Name = "useFeatureAttributeFilterLabel";
            this.useFeatureAttributeFilterLabel.Size = new System.Drawing.Size(52, 13);
            this.useFeatureAttributeFilterLabel.TabIndex = 2;
            this.useFeatureAttributeFilterLabel.Text = "this query";
            // 
            // attributeFeatureFilterComboBox
            // 
            this.attributeFeatureFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.attributeFeatureFilterComboBox.FormattingEnabled = true;
            this.attributeFeatureFilterComboBox.Location = new System.Drawing.Point(164, 18);
            this.attributeFeatureFilterComboBox.Name = "attributeFeatureFilterComboBox";
            this.attributeFeatureFilterComboBox.Size = new System.Drawing.Size(77, 21);
            this.attributeFeatureFilterComboBox.TabIndex = 1;
            this.attributeFeatureFilterComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // attributeFeatureFilterTextBox
            // 
            this.attributeFeatureFilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.attributeFeatureFilterTextBox.Enabled = false;
            this.attributeFeatureFilterTextBox.Location = new System.Drawing.Point(10, 43);
            this.attributeFeatureFilterTextBox.Multiline = true;
            this.attributeFeatureFilterTextBox.Name = "attributeFeatureFilterTextBox";
            this.attributeFeatureFilterTextBox.Size = new System.Drawing.Size(1323, 44);
            this.attributeFeatureFilterTextBox.TabIndex = 3;
            this.attributeFeatureFilterTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.attributeFeatureFilterTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // useAttributeFeatureFilterCheckBox
            // 
            this.useAttributeFeatureFilterCheckBox.AutoSize = true;
            this.useAttributeFeatureFilterCheckBox.Checked = true;
            this.useAttributeFeatureFilterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useAttributeFeatureFilterCheckBox.Location = new System.Drawing.Point(10, 20);
            this.useAttributeFeatureFilterCheckBox.Name = "useAttributeFeatureFilterCheckBox";
            this.useAttributeFeatureFilterCheckBox.Size = new System.Drawing.Size(134, 17);
            this.useAttributeFeatureFilterCheckBox.TabIndex = 0;
            this.useAttributeFeatureFilterCheckBox.Text = "Only use attributes that";
            this.useAttributeFeatureFilterCheckBox.UseVisualStyleBackColor = true;
            this.useAttributeFeatureFilterCheckBox.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // autoBagOfWordsGroupBox
            // 
            this.autoBagOfWordsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.autoBagOfWordsGroupBox.Controls.Add(this.useFeatureHashingForAutoBagOfWordsCheckBox);
            this.autoBagOfWordsGroupBox.Controls.Add(this.paginationPagesLabel);
            this.autoBagOfWordsGroupBox.Controls.Add(this.specifiedPagesTextBox);
            this.autoBagOfWordsGroupBox.Controls.Add(this.specifiedPagesCheckBox);
            this.autoBagOfWordsGroupBox.Controls.Add(this.maxFeaturesTextBox);
            this.autoBagOfWordsGroupBox.Controls.Add(this.maxFeaturesLabel);
            this.autoBagOfWordsGroupBox.Controls.Add(this.maxShingleSizeTextBox);
            this.autoBagOfWordsGroupBox.Controls.Add(this.maxShingleSizeLabel);
            this.autoBagOfWordsGroupBox.Controls.Add(this.useAutoBagOfWordsCheckBox);
            this.autoBagOfWordsGroupBox.Location = new System.Drawing.Point(3, 6);
            this.autoBagOfWordsGroupBox.Name = "autoBagOfWordsGroupBox";
            this.autoBagOfWordsGroupBox.Size = new System.Drawing.Size(1346, 144);
            this.autoBagOfWordsGroupBox.TabIndex = 1;
            this.autoBagOfWordsGroupBox.TabStop = false;
            this.autoBagOfWordsGroupBox.Text = "Auto-BoW";
            // 
            // useFeatureHashingForAutoBagOfWordsCheckBox
            // 
            this.useFeatureHashingForAutoBagOfWordsCheckBox.AutoSize = true;
            this.useFeatureHashingForAutoBagOfWordsCheckBox.Location = new System.Drawing.Point(11, 121);
            this.useFeatureHashingForAutoBagOfWordsCheckBox.Name = "useFeatureHashingForAutoBagOfWordsCheckBox";
            this.useFeatureHashingForAutoBagOfWordsCheckBox.Size = new System.Drawing.Size(317, 17);
            this.useFeatureHashingForAutoBagOfWordsCheckBox.TabIndex = 8;
            this.useFeatureHashingForAutoBagOfWordsCheckBox.Text = "Use feature hashing (instead of computing a fixed vocabulary)";
            this.useFeatureHashingForAutoBagOfWordsCheckBox.UseVisualStyleBackColor = true;
            this.useFeatureHashingForAutoBagOfWordsCheckBox.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // paginationPagesLabel
            // 
            this.paginationPagesLabel.AutoSize = true;
            this.paginationPagesLabel.Location = new System.Drawing.Point(8, 83);
            this.paginationPagesLabel.MaximumSize = new System.Drawing.Size(500, 50);
            this.paginationPagesLabel.Name = "paginationPagesLabel";
            this.paginationPagesLabel.Size = new System.Drawing.Size(464, 26);
            this.paginationPagesLabel.TabIndex = 7;
            this.paginationPagesLabel.Text = "* For pagination this is a single number interpreted as the number of pages aroun" +
    "d the candidate. Number before = floor(ρ/2). Number after = ceiling(ρ/2)";
            // 
            // specifiedPagesTextBox
            // 
            this.specifiedPagesTextBox.Enabled = false;
            this.specifiedPagesTextBox.Location = new System.Drawing.Point(10, 60);
            this.specifiedPagesTextBox.Name = "specifiedPagesTextBox";
            this.specifiedPagesTextBox.Size = new System.Drawing.Size(171, 20);
            this.specifiedPagesTextBox.TabIndex = 2;
            this.specifiedPagesTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.specifiedPagesTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // specifiedPagesCheckBox
            // 
            this.specifiedPagesCheckBox.AutoSize = true;
            this.specifiedPagesCheckBox.Enabled = false;
            this.specifiedPagesCheckBox.Location = new System.Drawing.Point(10, 40);
            this.specifiedPagesCheckBox.Name = "specifiedPagesCheckBox";
            this.specifiedPagesCheckBox.Size = new System.Drawing.Size(175, 17);
            this.specifiedPagesCheckBox.TabIndex = 1;
            this.specifiedPagesCheckBox.Text = "Use only text from these pages*";
            this.specifiedPagesCheckBox.UseVisualStyleBackColor = true;
            this.specifiedPagesCheckBox.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // maxFeaturesTextBox
            // 
            this.maxFeaturesTextBox.Location = new System.Drawing.Point(304, 60);
            this.maxFeaturesTextBox.Name = "maxFeaturesTextBox";
            this.maxFeaturesTextBox.Size = new System.Drawing.Size(44, 20);
            this.maxFeaturesTextBox.TabIndex = 6;
            this.maxFeaturesTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.maxFeaturesTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.maxFeaturesTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // maxFeaturesLabel
            // 
            this.maxFeaturesLabel.AutoSize = true;
            this.maxFeaturesLabel.Location = new System.Drawing.Point(301, 41);
            this.maxFeaturesLabel.Name = "maxFeaturesLabel";
            this.maxFeaturesLabel.Size = new System.Drawing.Size(68, 13);
            this.maxFeaturesLabel.TabIndex = 5;
            this.maxFeaturesLabel.Text = "Max features";
            // 
            // maxShingleSizeTextBox
            // 
            this.maxShingleSizeTextBox.Location = new System.Drawing.Point(210, 60);
            this.maxShingleSizeTextBox.Name = "maxShingleSizeTextBox";
            this.maxShingleSizeTextBox.Size = new System.Drawing.Size(26, 20);
            this.maxShingleSizeTextBox.TabIndex = 4;
            this.maxShingleSizeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.maxShingleSizeTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.maxShingleSizeTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // maxShingleSizeLabel
            // 
            this.maxShingleSizeLabel.AutoSize = true;
            this.maxShingleSizeLabel.Location = new System.Drawing.Point(207, 41);
            this.maxShingleSizeLabel.Name = "maxShingleSizeLabel";
            this.maxShingleSizeLabel.Size = new System.Drawing.Size(84, 13);
            this.maxShingleSizeLabel.TabIndex = 3;
            this.maxShingleSizeLabel.Text = "Max shingle size";
            // 
            // useAutoBagOfWordsCheckBox
            // 
            this.useAutoBagOfWordsCheckBox.AutoSize = true;
            this.useAutoBagOfWordsCheckBox.Checked = true;
            this.useAutoBagOfWordsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useAutoBagOfWordsCheckBox.Location = new System.Drawing.Point(10, 17);
            this.useAutoBagOfWordsCheckBox.Name = "useAutoBagOfWordsCheckBox";
            this.useAutoBagOfWordsCheckBox.Size = new System.Drawing.Size(428, 17);
            this.useAutoBagOfWordsCheckBox.TabIndex = 0;
            this.useAutoBagOfWordsCheckBox.Text = "Automatically generate Bag-of-Words features from document shingles (word n-grams" +
    ")";
            this.useAutoBagOfWordsCheckBox.UseVisualStyleBackColor = true;
            this.useAutoBagOfWordsCheckBox.Click += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // machineConfigurationTabPage
            // 
            this.machineConfigurationTabPage.Controls.Add(this.probabilisticSvmPanel);
            this.machineConfigurationTabPage.Controls.Add(this.svmPanel);
            this.machineConfigurationTabPage.Controls.Add(this.machineTypeLabel);
            this.machineConfigurationTabPage.Controls.Add(this.neuralNetPanel);
            this.machineConfigurationTabPage.Controls.Add(this.machineTypeComboBox);
            this.machineConfigurationTabPage.Location = new System.Drawing.Point(4, 22);
            this.machineConfigurationTabPage.Name = "machineConfigurationTabPage";
            this.machineConfigurationTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.machineConfigurationTabPage.Size = new System.Drawing.Size(1354, 648);
            this.machineConfigurationTabPage.TabIndex = 2;
            this.machineConfigurationTabPage.Text = "Machine configuration";
            this.machineConfigurationTabPage.UseVisualStyleBackColor = true;
            // 
            // probabilisticSvmPanel
            // 
            this.probabilisticSvmPanel.Controls.Add(this.calibrateForProbabilitiesCheckBox);
            this.probabilisticSvmPanel.Controls.Add(this.useUnknownCheckBox);
            this.probabilisticSvmPanel.Controls.Add(this.unknownCutoffTextBox);
            this.probabilisticSvmPanel.Controls.Add(this.translateUnknownTextBox);
            this.probabilisticSvmPanel.Controls.Add(this.translateUnknownCheckbox);
            this.probabilisticSvmPanel.Location = new System.Drawing.Point(548, 340);
            this.probabilisticSvmPanel.Name = "probabilisticSvmPanel";
            this.probabilisticSvmPanel.Size = new System.Drawing.Size(558, 89);
            this.probabilisticSvmPanel.TabIndex = 4;
            // 
            // calibrateForProbabilitiesCheckBox
            // 
            this.calibrateForProbabilitiesCheckBox.AutoSize = true;
            this.calibrateForProbabilitiesCheckBox.Checked = true;
            this.calibrateForProbabilitiesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.calibrateForProbabilitiesCheckBox.Location = new System.Drawing.Point(7, 13);
            this.calibrateForProbabilitiesCheckBox.Name = "calibrateForProbabilitiesCheckBox";
            this.calibrateForProbabilitiesCheckBox.Size = new System.Drawing.Size(222, 17);
            this.calibrateForProbabilitiesCheckBox.TabIndex = 0;
            this.calibrateForProbabilitiesCheckBox.Text = "Calibrate machine to produce probabilities";
            this.calibrateForProbabilitiesCheckBox.UseVisualStyleBackColor = true;
            this.calibrateForProbabilitiesCheckBox.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // useUnknownCheckBox
            // 
            this.useUnknownCheckBox.AutoSize = true;
            this.useUnknownCheckBox.Checked = true;
            this.useUnknownCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useUnknownCheckBox.Location = new System.Drawing.Point(7, 36);
            this.useUnknownCheckBox.Name = "useUnknownCheckBox";
            this.useUnknownCheckBox.Size = new System.Drawing.Size(280, 17);
            this.useUnknownCheckBox.TabIndex = 1;
            this.useUnknownCheckBox.Text = "Use Unknown category when max probability is below";
            this.useUnknownCheckBox.UseVisualStyleBackColor = true;
            this.useUnknownCheckBox.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // unknownCutoffTextBox
            // 
            this.unknownCutoffTextBox.Location = new System.Drawing.Point(293, 33);
            this.unknownCutoffTextBox.Name = "unknownCutoffTextBox";
            this.unknownCutoffTextBox.Size = new System.Drawing.Size(44, 20);
            this.unknownCutoffTextBox.TabIndex = 2;
            this.unknownCutoffTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.unknownCutoffTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.unknownCutoffTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // translateUnknownTextBox
            // 
            this.translateUnknownTextBox.Location = new System.Drawing.Point(237, 56);
            this.translateUnknownTextBox.Name = "translateUnknownTextBox";
            this.translateUnknownTextBox.Size = new System.Drawing.Size(296, 20);
            this.translateUnknownTextBox.TabIndex = 4;
            this.translateUnknownTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.translateUnknownTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // translateUnknownCheckbox
            // 
            this.translateUnknownCheckbox.AutoSize = true;
            this.translateUnknownCheckbox.Checked = true;
            this.translateUnknownCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.translateUnknownCheckbox.Location = new System.Drawing.Point(8, 59);
            this.translateUnknownCheckbox.Name = "translateUnknownCheckbox";
            this.translateUnknownCheckbox.Size = new System.Drawing.Size(223, 17);
            this.translateUnknownCheckbox.TabIndex = 3;
            this.translateUnknownCheckbox.Text = "Translate Unknown category to this name";
            this.translateUnknownCheckbox.UseVisualStyleBackColor = true;
            this.translateUnknownCheckbox.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // svmPanel
            // 
            this.svmPanel.Controls.Add(this.svmScoreTypeGroupBox);
            this.svmPanel.Controls.Add(this.svmConditionallyApplyWeightRatioCheckBox);
            this.svmPanel.Controls.Add(this.svmCacheSizeTextBox);
            this.svmPanel.Controls.Add(this.label6);
            this.svmPanel.Controls.Add(this.svmWeightRatioTextBox);
            this.svmPanel.Controls.Add(this.label5);
            this.svmPanel.Controls.Add(this.svmAutoComplexityCheckBox);
            this.svmPanel.Controls.Add(this.svmComplexityTextBox);
            this.svmPanel.Controls.Add(this.svmComplexityLabel);
            this.svmPanel.Location = new System.Drawing.Point(548, 54);
            this.svmPanel.Name = "svmPanel";
            this.svmPanel.Size = new System.Drawing.Size(558, 280);
            this.svmPanel.TabIndex = 3;
            // 
            // svmScoreTypeGroupBox
            // 
            this.svmScoreTypeGroupBox.Controls.Add(this.svmUsePrecisionRadioButton);
            this.svmScoreTypeGroupBox.Controls.Add(this.svmUseRecallRadioButton);
            this.svmScoreTypeGroupBox.Controls.Add(this.svmUseF1ScoreRadioButton);
            this.svmScoreTypeGroupBox.Location = new System.Drawing.Point(7, 72);
            this.svmScoreTypeGroupBox.Name = "svmScoreTypeGroupBox";
            this.svmScoreTypeGroupBox.Size = new System.Drawing.Size(538, 89);
            this.svmScoreTypeGroupBox.TabIndex = 3;
            this.svmScoreTypeGroupBox.TabStop = false;
            // 
            // svmUsePrecisionRadioButton
            // 
            this.svmUsePrecisionRadioButton.AutoSize = true;
            this.svmUsePrecisionRadioButton.Location = new System.Drawing.Point(6, 36);
            this.svmUsePrecisionRadioButton.Name = "svmUsePrecisionRadioButton";
            this.svmUsePrecisionRadioButton.Size = new System.Drawing.Size(244, 17);
            this.svmUsePrecisionRadioButton.TabIndex = 1;
            this.svmUsePrecisionRadioButton.TabStop = true;
            this.svmUsePrecisionRadioButton.Text = "Use precision (only works for 2-class problems)";
            this.svmUsePrecisionRadioButton.UseVisualStyleBackColor = true;
            this.svmUsePrecisionRadioButton.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // svmUseRecallRadioButton
            // 
            this.svmUseRecallRadioButton.AutoSize = true;
            this.svmUseRecallRadioButton.Location = new System.Drawing.Point(6, 59);
            this.svmUseRecallRadioButton.Name = "svmUseRecallRadioButton";
            this.svmUseRecallRadioButton.Size = new System.Drawing.Size(227, 17);
            this.svmUseRecallRadioButton.TabIndex = 2;
            this.svmUseRecallRadioButton.TabStop = true;
            this.svmUseRecallRadioButton.Text = "Use recall (only works for 2-class problems)";
            this.svmUseRecallRadioButton.UseVisualStyleBackColor = true;
            this.svmUseRecallRadioButton.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // svmUseF1ScoreRadioButton
            // 
            this.svmUseF1ScoreRadioButton.AutoSize = true;
            this.svmUseF1ScoreRadioButton.Location = new System.Drawing.Point(6, 13);
            this.svmUseF1ScoreRadioButton.Name = "svmUseF1ScoreRadioButton";
            this.svmUseF1ScoreRadioButton.Size = new System.Drawing.Size(233, 17);
            this.svmUseF1ScoreRadioButton.TabIndex = 0;
            this.svmUseF1ScoreRadioButton.TabStop = true;
            this.svmUseF1ScoreRadioButton.Text = "Use overall agreement/F1-score to compare";
            this.svmUseF1ScoreRadioButton.UseVisualStyleBackColor = true;
            this.svmUseF1ScoreRadioButton.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // svmConditionallyApplyWeightRatioCheckBox
            // 
            this.svmConditionallyApplyWeightRatioCheckBox.AutoSize = true;
            this.svmConditionallyApplyWeightRatioCheckBox.Checked = true;
            this.svmConditionallyApplyWeightRatioCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.svmConditionallyApplyWeightRatioCheckBox.Location = new System.Drawing.Point(7, 211);
            this.svmConditionallyApplyWeightRatioCheckBox.Name = "svmConditionallyApplyWeightRatioCheckBox";
            this.svmConditionallyApplyWeightRatioCheckBox.Size = new System.Drawing.Size(538, 17);
            this.svmConditionallyApplyWeightRatioCheckBox.TabIndex = 6;
            this.svmConditionallyApplyWeightRatioCheckBox.Text = "Only apply weight ratio when the positive class of the classifier being trained i" +
    "s the designated Negative class";
            this.svmConditionallyApplyWeightRatioCheckBox.UseVisualStyleBackColor = true;
            this.svmConditionallyApplyWeightRatioCheckBox.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // svmCacheSizeTextBox
            // 
            this.svmCacheSizeTextBox.Location = new System.Drawing.Point(8, 251);
            this.svmCacheSizeTextBox.Name = "svmCacheSizeTextBox";
            this.svmCacheSizeTextBox.Size = new System.Drawing.Size(64, 20);
            this.svmCacheSizeTextBox.TabIndex = 8;
            this.svmCacheSizeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.svmCacheSizeTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.svmCacheSizeTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 235);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(412, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "SMO cache size (blank for default = training set size, smaller = slower but less " +
    "memory)";
            // 
            // svmWeightRatioTextBox
            // 
            this.svmWeightRatioTextBox.Location = new System.Drawing.Point(7, 187);
            this.svmWeightRatioTextBox.Name = "svmWeightRatioTextBox";
            this.svmWeightRatioTextBox.Size = new System.Drawing.Size(64, 20);
            this.svmWeightRatioTextBox.TabIndex = 5;
            this.svmWeightRatioTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.svmWeightRatioTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.svmWeightRatioTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 171);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(481, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Positive to negative class weight ratio (what portion of Complexity will be appli" +
    "ed to the positive class)";
            // 
            // svmAutoComplexityCheckBox
            // 
            this.svmAutoComplexityCheckBox.AutoSize = true;
            this.svmAutoComplexityCheckBox.Checked = true;
            this.svmAutoComplexityCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.svmAutoComplexityCheckBox.Location = new System.Drawing.Point(7, 52);
            this.svmAutoComplexityCheckBox.Name = "svmAutoComplexityCheckBox";
            this.svmAutoComplexityCheckBox.Size = new System.Drawing.Size(471, 17);
            this.svmAutoComplexityCheckBox.TabIndex = 2;
            this.svmAutoComplexityCheckBox.Text = "Choose complexity parameter automatically using a cross validation set (increases" +
    " training time)";
            this.svmAutoComplexityCheckBox.UseVisualStyleBackColor = true;
            this.svmAutoComplexityCheckBox.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // svmComplexityTextBox
            // 
            this.svmComplexityTextBox.Location = new System.Drawing.Point(7, 22);
            this.svmComplexityTextBox.Name = "svmComplexityTextBox";
            this.svmComplexityTextBox.Size = new System.Drawing.Size(64, 20);
            this.svmComplexityTextBox.TabIndex = 1;
            this.svmComplexityTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.svmComplexityTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.svmComplexityTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // svmComplexityLabel
            // 
            this.svmComplexityLabel.AutoSize = true;
            this.svmComplexityLabel.Location = new System.Drawing.Point(4, 6);
            this.svmComplexityLabel.Name = "svmComplexityLabel";
            this.svmComplexityLabel.Size = new System.Drawing.Size(277, 13);
            this.svmComplexityLabel.TabIndex = 0;
            this.svmComplexityLabel.Text = "Complexity (Higher = higher variance/lower bias classifier)";
            // 
            // machineTypeLabel
            // 
            this.machineTypeLabel.AutoSize = true;
            this.machineTypeLabel.Location = new System.Drawing.Point(3, 7);
            this.machineTypeLabel.Name = "machineTypeLabel";
            this.machineTypeLabel.Size = new System.Drawing.Size(71, 13);
            this.machineTypeLabel.TabIndex = 0;
            this.machineTypeLabel.Text = "Machine type";
            // 
            // neuralNetPanel
            // 
            this.neuralNetPanel.Controls.Add(this.numberOfCandidateNetwordsTextBox);
            this.neuralNetPanel.Controls.Add(this.numberOfCandidateNetworksLabel);
            this.neuralNetPanel.Controls.Add(this.useCrossValidationSetsCheckBox);
            this.neuralNetPanel.Controls.Add(this.maximumTrainingIterationsTextBox);
            this.neuralNetPanel.Controls.Add(this.maximumTrainingIterationsLabel);
            this.neuralNetPanel.Controls.Add(this.sigmoidAlphaTextBox);
            this.neuralNetPanel.Controls.Add(this.sigmoidAlphaLabel);
            this.neuralNetPanel.Controls.Add(this.sizeOfHiddenLayersTextBox);
            this.neuralNetPanel.Controls.Add(this.sizeOfHiddenLayersLabel);
            this.neuralNetPanel.Location = new System.Drawing.Point(1, 54);
            this.neuralNetPanel.Name = "neuralNetPanel";
            this.neuralNetPanel.Size = new System.Drawing.Size(541, 208);
            this.neuralNetPanel.TabIndex = 2;
            // 
            // numberOfCandidateNetwordsTextBox
            // 
            this.numberOfCandidateNetwordsTextBox.Location = new System.Drawing.Point(8, 175);
            this.numberOfCandidateNetwordsTextBox.Name = "numberOfCandidateNetwordsTextBox";
            this.numberOfCandidateNetwordsTextBox.Size = new System.Drawing.Size(32, 20);
            this.numberOfCandidateNetwordsTextBox.TabIndex = 8;
            this.numberOfCandidateNetwordsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numberOfCandidateNetwordsTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.numberOfCandidateNetwordsTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // numberOfCandidateNetworksLabel
            // 
            this.numberOfCandidateNetworksLabel.AutoSize = true;
            this.numberOfCandidateNetworksLabel.Location = new System.Drawing.Point(5, 159);
            this.numberOfCandidateNetworksLabel.Name = "numberOfCandidateNetworksLabel";
            this.numberOfCandidateNetworksLabel.Size = new System.Drawing.Size(189, 13);
            this.numberOfCandidateNetworksLabel.TabIndex = 7;
            this.numberOfCandidateNetworksLabel.Text = "Number of candidate networks to build";
            // 
            // useCrossValidationSetsCheckBox
            // 
            this.useCrossValidationSetsCheckBox.AutoSize = true;
            this.useCrossValidationSetsCheckBox.Checked = true;
            this.useCrossValidationSetsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useCrossValidationSetsCheckBox.Location = new System.Drawing.Point(8, 136);
            this.useCrossValidationSetsCheckBox.Name = "useCrossValidationSetsCheckBox";
            this.useCrossValidationSetsCheckBox.Size = new System.Drawing.Size(476, 17);
            this.useCrossValidationSetsCheckBox.TabIndex = 6;
            this.useCrossValidationSetsCheckBox.Text = "Use cross validation sets to determine when to stop training (will reduce trainin" +
    "g set size by 20%)";
            this.useCrossValidationSetsCheckBox.UseVisualStyleBackColor = true;
            this.useCrossValidationSetsCheckBox.CheckedChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // maximumTrainingIterationsTextBox
            // 
            this.maximumTrainingIterationsTextBox.Location = new System.Drawing.Point(8, 106);
            this.maximumTrainingIterationsTextBox.Name = "maximumTrainingIterationsTextBox";
            this.maximumTrainingIterationsTextBox.Size = new System.Drawing.Size(32, 20);
            this.maximumTrainingIterationsTextBox.TabIndex = 5;
            this.maximumTrainingIterationsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.maximumTrainingIterationsTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.maximumTrainingIterationsTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // maximumTrainingIterationsLabel
            // 
            this.maximumTrainingIterationsLabel.AutoSize = true;
            this.maximumTrainingIterationsLabel.Location = new System.Drawing.Point(5, 90);
            this.maximumTrainingIterationsLabel.Name = "maximumTrainingIterationsLabel";
            this.maximumTrainingIterationsLabel.Size = new System.Drawing.Size(133, 13);
            this.maximumTrainingIterationsLabel.TabIndex = 4;
            this.maximumTrainingIterationsLabel.Text = "Maximum training iterations";
            // 
            // sigmoidAlphaTextBox
            // 
            this.sigmoidAlphaTextBox.Location = new System.Drawing.Point(8, 64);
            this.sigmoidAlphaTextBox.Name = "sigmoidAlphaTextBox";
            this.sigmoidAlphaTextBox.Size = new System.Drawing.Size(32, 20);
            this.sigmoidAlphaTextBox.TabIndex = 3;
            this.sigmoidAlphaTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.sigmoidAlphaTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.sigmoidAlphaTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // sigmoidAlphaLabel
            // 
            this.sigmoidAlphaLabel.AutoSize = true;
            this.sigmoidAlphaLabel.Location = new System.Drawing.Point(5, 48);
            this.sigmoidAlphaLabel.Name = "sigmoidAlphaLabel";
            this.sigmoidAlphaLabel.Size = new System.Drawing.Size(249, 13);
            this.sigmoidAlphaLabel.TabIndex = 2;
            this.sigmoidAlphaLabel.Text = "Sigmoid activation function alpha value (steepness)";
            // 
            // sizeOfHiddenLayersTextBox
            // 
            this.sizeOfHiddenLayersTextBox.Location = new System.Drawing.Point(8, 22);
            this.sizeOfHiddenLayersTextBox.Name = "sizeOfHiddenLayersTextBox";
            this.sizeOfHiddenLayersTextBox.Size = new System.Drawing.Size(67, 20);
            this.sizeOfHiddenLayersTextBox.TabIndex = 1;
            this.sizeOfHiddenLayersTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.sizeOfHiddenLayersTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            this.sizeOfHiddenLayersTextBox.Leave += new System.EventHandler(this.HandleTextBox_Leave);
            // 
            // sizeOfHiddenLayersLabel
            // 
            this.sizeOfHiddenLayersLabel.AutoSize = true;
            this.sizeOfHiddenLayersLabel.Location = new System.Drawing.Point(5, 6);
            this.sizeOfHiddenLayersLabel.Name = "sizeOfHiddenLayersLabel";
            this.sizeOfHiddenLayersLabel.Size = new System.Drawing.Size(327, 13);
            this.sizeOfHiddenLayersLabel.TabIndex = 0;
            this.sizeOfHiddenLayersLabel.Text = "Size of hidden layer(s) (if more than one, separate sizes with comma)";
            // 
            // machineTypeComboBox
            // 
            this.machineTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.machineTypeComboBox.FormattingEnabled = true;
            this.machineTypeComboBox.Location = new System.Drawing.Point(5, 25);
            this.machineTypeComboBox.Name = "machineTypeComboBox";
            this.machineTypeComboBox.Size = new System.Drawing.Size(195, 21);
            this.machineTypeComboBox.TabIndex = 1;
            this.machineTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleControlStateChanged);
            // 
            // saveMachineAsButton
            // 
            this.saveMachineAsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveMachineAsButton.Location = new System.Drawing.Point(1297, 683);
            this.saveMachineAsButton.Name = "saveMachineAsButton";
            this.saveMachineAsButton.Size = new System.Drawing.Size(60, 23);
            this.saveMachineAsButton.TabIndex = 5;
            this.saveMachineAsButton.Text = "Save As";
            this.saveMachineAsButton.UseVisualStyleBackColor = true;
            this.saveMachineAsButton.Click += new System.EventHandler(this.HandleSaveMachineAsButton_Click);
            // 
            // trainTestButton
            // 
            this.trainTestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.trainTestButton.Location = new System.Drawing.Point(5, 683);
            this.trainTestButton.Name = "trainTestButton";
            this.trainTestButton.Size = new System.Drawing.Size(94, 23);
            this.trainTestButton.TabIndex = 1;
            this.trainTestButton.Text = "Train/Test...";
            this.trainTestButton.UseVisualStyleBackColor = true;
            this.trainTestButton.Click += new System.EventHandler(this.HandleTrainTestButton_Click);
            // 
            // configurationErrorProvider
            // 
            this.configurationErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.configurationErrorProvider.ContainerControl = this;
            // 
            // openMachineButton
            // 
            this.openMachineButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.openMachineButton.Location = new System.Drawing.Point(1165, 683);
            this.openMachineButton.Name = "openMachineButton";
            this.openMachineButton.Size = new System.Drawing.Size(60, 23);
            this.openMachineButton.TabIndex = 3;
            this.openMachineButton.Text = "Open";
            this.openMachineButton.UseVisualStyleBackColor = true;
            this.openMachineButton.Click += new System.EventHandler(this.HandleOpenMachineButton_Click);
            // 
            // saveMachineButton
            // 
            this.saveMachineButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveMachineButton.Location = new System.Drawing.Point(1231, 683);
            this.saveMachineButton.Name = "saveMachineButton";
            this.saveMachineButton.Size = new System.Drawing.Size(60, 23);
            this.saveMachineButton.TabIndex = 4;
            this.saveMachineButton.Text = "Save";
            this.saveMachineButton.UseVisualStyleBackColor = true;
            this.saveMachineButton.Click += new System.EventHandler(this.HandleSaveMachineButton_Click);
            // 
            // newMachineButton
            // 
            this.newMachineButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.newMachineButton.Location = new System.Drawing.Point(1099, 683);
            this.newMachineButton.Name = "newMachineButton";
            this.newMachineButton.Size = new System.Drawing.Size(60, 23);
            this.newMachineButton.TabIndex = 2;
            this.newMachineButton.Text = "New";
            this.newMachineButton.UseVisualStyleBackColor = true;
            this.newMachineButton.Click += new System.EventHandler(this.HandleNewMachineButton_Click);
            // 
            // dangerModeButton
            // 
            this.dangerModeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.dangerModeButton.Location = new System.Drawing.Point(1007, 683);
            this.dangerModeButton.Name = "dangerModeButton";
            this.dangerModeButton.Size = new System.Drawing.Size(86, 23);
            this.dangerModeButton.TabIndex = 6;
            this.dangerModeButton.Text = "Danger mode";
            this.dangerModeButton.UseVisualStyleBackColor = true;
            this.dangerModeButton.Click += new System.EventHandler(this.HandleDangerModeButton_Click);
            // 
            // LearningMachineConfiguration
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1362, 733);
            this.Controls.Add(this.dangerModeButton);
            this.Controls.Add(this.newMachineButton);
            this.Controls.Add(this.saveMachineButton);
            this.Controls.Add(this.configurationTabControl);
            this.Controls.Add(this.saveMachineAsButton);
            this.Controls.Add(this.openMachineButton);
            this.Controls.Add(this.trainTestButton);
            this.Controls.Add(this.statusStrip1);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(572, 698);
            this.Name = "LearningMachineConfiguration";
            this.Text = "Learning Machine Editor";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.configurationTabControl.ResumeLayout(false);
            this.inputConfigurationTabPage.ResumeLayout(false);
            this.machineUsageGroupBox.ResumeLayout(false);
            this.machineUsageGroupBox.PerformLayout();
            this.inputConfigurationGroupBox.ResumeLayout(false);
            this.inputConfigurationGroupBox.PerformLayout();
            this.attributeCategorizationInputPanel.ResumeLayout(false);
            this.attributeCategorizationInputPanel.PerformLayout();
            this.documentCategorizationFolderInputPanel.ResumeLayout(false);
            this.documentCategorizationFolderInputPanel.PerformLayout();
            this.paginationInputPanel.ResumeLayout(false);
            this.paginationInputPanel.PerformLayout();
            this.documentCategorizationCsvInputPanel.ResumeLayout(false);
            this.documentCategorizationCsvInputPanel.PerformLayout();
            this.featureConfigurationTabPage.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.autoBagOfWordsGroupBox.ResumeLayout(false);
            this.autoBagOfWordsGroupBox.PerformLayout();
            this.machineConfigurationTabPage.ResumeLayout(false);
            this.machineConfigurationTabPage.PerformLayout();
            this.probabilisticSvmPanel.ResumeLayout(false);
            this.probabilisticSvmPanel.PerformLayout();
            this.svmPanel.ResumeLayout(false);
            this.svmPanel.PerformLayout();
            this.svmScoreTypeGroupBox.ResumeLayout(false);
            this.svmScoreTypeGroupBox.PerformLayout();
            this.neuralNetPanel.ResumeLayout(false);
            this.neuralNetPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.configurationErrorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel machineStateToolStripStatusLabel;
        private System.Windows.Forms.TabControl configurationTabControl;
        private System.Windows.Forms.TabPage inputConfigurationTabPage;
        private System.Windows.Forms.TabPage featureConfigurationTabPage;
        private System.Windows.Forms.GroupBox inputConfigurationGroupBox;
        private System.Windows.Forms.Panel documentCategorizationFolderInputPanel;
        private System.Windows.Forms.Label documentCategorizationFolderTrainingPercentageLabel;
        private System.Windows.Forms.TextBox documentCategorizationFolderTrainingPercentageTextBox;
        private System.Windows.Forms.Label documentCategorizationFolderAnswerLabel;
        private Extract.Utilities.Forms.PathTagsButton documentCategorizationFolderAnswerPathTagButton;
        private System.Windows.Forms.TextBox documentCategorizationFolderAnswerTextBox;
        private System.Windows.Forms.Label documentCategorizationInputFolderLabel;
        private Extract.Utilities.Forms.BrowseButton documentCategorizationInputFolderBrowseButton;
        private System.Windows.Forms.TextBox documentCategorizationInputFolderTextBox;
        private System.Windows.Forms.RadioButton folderSearchRadioButton;
        private System.Windows.Forms.Panel paginationInputPanel;
        private System.Windows.Forms.Label paginationTrainingPercentageLabel;
        private System.Windows.Forms.TextBox paginationTrainingPercentageTextBox;
        private System.Windows.Forms.Label paginationFeatureVoaLabel;
        private Extract.Utilities.Forms.PathTagsButton paginationFeatureVoaPathTagButton;
        private System.Windows.Forms.TextBox paginationFeatureVoaTextBox;
        private System.Windows.Forms.Label paginationFileListOrFolderLabel;
        private Extract.Utilities.Forms.BrowseButton paginationFileListOrFolderBrowseButton;
        private System.Windows.Forms.TextBox paginationFileListOrFolderTextBox;
        private System.Windows.Forms.RadioButton textFileOrCsvRadioButton;
        private System.Windows.Forms.Button editFeaturesButton;
        private System.Windows.Forms.GroupBox autoBagOfWordsGroupBox;
        private System.Windows.Forms.TextBox maxFeaturesTextBox;
        private System.Windows.Forms.Label maxFeaturesLabel;
        private System.Windows.Forms.TextBox maxShingleSizeTextBox;
        private System.Windows.Forms.Label maxShingleSizeLabel;
        private System.Windows.Forms.CheckBox useAutoBagOfWordsCheckBox;
        private System.Windows.Forms.GroupBox machineUsageGroupBox;
        private System.Windows.Forms.RadioButton paginationRadioButton;
        private System.Windows.Forms.RadioButton documentCategorizationRadioButton;
        private System.Windows.Forms.Panel documentCategorizationCsvInputPanel;
        private Extract.Utilities.Forms.BrowseButton documentCategorizationCsvBrowseButton;
        private System.Windows.Forms.Label trainingPercentageLabel;
        private System.Windows.Forms.TextBox documentCategorizationCsvTrainingPercentageTextBox;
        private System.Windows.Forms.Label documentCategorizationCsvLabel;
        private System.Windows.Forms.TextBox documentCategorizationCsvTextBox;
        private System.Windows.Forms.Label documentCategorizationFolderRandomNumberSeedLabel;
        private System.Windows.Forms.TextBox documentCategorizationFolderRandomNumberSeedTextBox;
        private System.Windows.Forms.Label paginationRandomNumberSeedLabel;
        private System.Windows.Forms.TextBox paginationRandomNumberSeedTextBox;
        private System.Windows.Forms.Label documentCategorizationCsvRandomNumberSeedLabel;
        private System.Windows.Forms.TextBox documentCategorizationCsvRandomNumberSeedTextBox;
        private System.Windows.Forms.TextBox specifiedPagesTextBox;
        private System.Windows.Forms.CheckBox specifiedPagesCheckBox;
        private System.Windows.Forms.Button computeFeaturesButton;
        private System.Windows.Forms.TabPage machineConfigurationTabPage;
        private System.Windows.Forms.Panel svmPanel;
        private System.Windows.Forms.TextBox unknownCutoffTextBox;
        private System.Windows.Forms.CheckBox useUnknownCheckBox;
        private System.Windows.Forms.CheckBox calibrateForProbabilitiesCheckBox;
        private System.Windows.Forms.CheckBox svmAutoComplexityCheckBox;
        private System.Windows.Forms.TextBox svmComplexityTextBox;
        private System.Windows.Forms.Label svmComplexityLabel;
        private System.Windows.Forms.Label machineTypeLabel;
        private System.Windows.Forms.Panel neuralNetPanel;
        private System.Windows.Forms.TextBox numberOfCandidateNetwordsTextBox;
        private System.Windows.Forms.Label numberOfCandidateNetworksLabel;
        private System.Windows.Forms.CheckBox useCrossValidationSetsCheckBox;
        private System.Windows.Forms.TextBox maximumTrainingIterationsTextBox;
        private System.Windows.Forms.Label maximumTrainingIterationsLabel;
        private System.Windows.Forms.TextBox sigmoidAlphaTextBox;
        private System.Windows.Forms.Label sigmoidAlphaLabel;
        private System.Windows.Forms.TextBox sizeOfHiddenLayersTextBox;
        private System.Windows.Forms.Label sizeOfHiddenLayersLabel;
        private System.Windows.Forms.ComboBox machineTypeComboBox;
        private System.Windows.Forms.Button saveMachineAsButton;
        private System.Windows.Forms.Button trainTestButton;
        private System.Windows.Forms.Button viewAnswerListButton;
        private System.Windows.Forms.Label documentCategorizationFolderFeatureVoaLabel;
        private Extract.Utilities.Forms.PathTagsButton documentCategorizationFolderFeatureVoaPathTagButton;
        private System.Windows.Forms.TextBox documentCategorizationFolderFeatureVoaTextBox;
        private System.Windows.Forms.Label documentCategorizationCsvFeatureVoaLabel;
        private Extract.Utilities.Forms.PathTagsButton documentCategorizationCsvFeatureVoaPathTagButton;
        private System.Windows.Forms.TextBox documentCategorizationCsvFeatureVoaTextBox;
        private System.Windows.Forms.TextBox paginationAnswerVoaTextBox;
        private Extract.Utilities.Forms.PathTagsButton paginationAnswerVoaPathTagButton;
        private System.Windows.Forms.Label paginationAnswerVoaLabel;
        private System.Windows.Forms.ErrorProvider configurationErrorProvider;
        private System.Windows.Forms.Button openMachineButton;
        private System.Windows.Forms.Button saveMachineButton;
        private System.Windows.Forms.Button newMachineButton;
        private System.Windows.Forms.Panel attributeCategorizationInputPanel;
        private System.Windows.Forms.Button attributeCategorizationCreateCandidateVoaButton;
        private System.Windows.Forms.Label attributeCategorizationRandomNumberSeedLabel;
        private System.Windows.Forms.TextBox attributeCategorizationRandomNumberSeedTextBox;
        private System.Windows.Forms.Label attributeCategorizationTrainingPercentageLabel;
        private System.Windows.Forms.TextBox attributeCategorizationTrainingPercentageTextBox;
        private System.Windows.Forms.Label attributeCategorizationCandidateVoaLabel;
        private Utilities.Forms.PathTagsButton attributeCategorizationCandidateVoaPathTagButton;
        private System.Windows.Forms.TextBox attributeCategorizationCandidateVoaTextBox;
        private System.Windows.Forms.Label attributeCategorizationFileListOrFolderLabel;
        private Utilities.Forms.BrowseButton attributeCategorizationFileListOrFolderBrowseButton;
        private System.Windows.Forms.TextBox attributeCategorizationFileListOrFolderTextBox;
        private System.Windows.Forms.RadioButton attributeCategorizationRadioButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox maxFeaturesPerVectorizerTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox maxShingleSizeForAttributeFeaturesTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox attributesToTokenizeFilterTextBox;
        private System.Windows.Forms.CheckBox tokenizeAttributesFilterCheckBox;
        private System.Windows.Forms.Label useFeatureAttributeFilterLabel;
        private System.Windows.Forms.ComboBox attributeFeatureFilterComboBox;
        private System.Windows.Forms.TextBox attributeFeatureFilterTextBox;
        private System.Windows.Forms.CheckBox useAttributeFeatureFilterCheckBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox documentCategorizationCsvNegativeClassNameTextBox;
        private System.Windows.Forms.TextBox svmWeightRatioTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox svmCacheSizeTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox attributeCategorizationNegativeClassNameTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox documentCategorizationFolderNegativeClassNameTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox paginationNegativeClassNameTextBox;
        private System.Windows.Forms.RadioButton svmUseF1ScoreRadioButton;
        private System.Windows.Forms.TextBox translateUnknownTextBox;
        private System.Windows.Forms.CheckBox translateUnknownCheckbox;
        private System.Windows.Forms.Panel probabilisticSvmPanel;
        private System.Windows.Forms.RadioButton svmUseRecallRadioButton;
        private System.Windows.Forms.RadioButton svmUsePrecisionRadioButton;
        private System.Windows.Forms.CheckBox svmConditionallyApplyWeightRatioCheckBox;
        private System.Windows.Forms.GroupBox svmScoreTypeGroupBox;
        private System.Windows.Forms.Label label4;
        private Utilities.Forms.BrowseButton csvOutputBrowseButton;
        private System.Windows.Forms.TextBox csvOutputTextBox;
        private System.Windows.Forms.Button writeDataToCsvButton;
        private System.Windows.Forms.CheckBox standardizeFeaturesForCsvOutputCheckBox;
        private System.Windows.Forms.Label paginationPagesLabel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox useFeatureHashingForAutoBagOfWordsCheckBox;
        private System.Windows.Forms.RadioButton deletionRadioButton;
        private System.Windows.Forms.Button dangerModeButton;
        private System.Windows.Forms.ToolStripStatusLabel dangerModeToolStripStatusLabel;
    }
}

