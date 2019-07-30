using Extract.FileActionManager.Forms;

namespace Extract.UtilityApplications.MachineLearning
{
    partial class TrainingDataCollectorConfigurationDialog
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
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                if (_settings != null)
                {
                    _settings.Dispose();
                }
            }

            try
            {
                _database?.CloseAllDBConnections();
            }
            catch { }
            _database = null;

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Extract.Utilities.ScheduledEvent scheduledEvent1 = new Extract.Utilities.ScheduledEvent();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._deleteMLDataButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this._limitProcessingByDateNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this._manageMLModelsButton = new System.Windows.Forms.Button();
            this._attributeSetNameComboBox = new System.Windows.Forms.ComboBox();
            this._addModelButton = new System.Windows.Forms.Button();
            this._modelNameComboBox = new System.Windows.Forms.ComboBox();
            this._lastIDProcessedNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._changeAnswerButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this._trainingPercentageNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this._overrideTrainingTestingSplitCheckBox = new System.Windows.Forms.CheckBox();
            this._lmModelTypeRadioButton = new System.Windows.Forms.RadioButton();
            this._nerModelTypeRadioButton = new System.Windows.Forms.RadioButton();
            this._dataGeneratorPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._dataGeneratorPathTextBox = new System.Windows.Forms.TextBox();
            this._descriptionTextBox = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this._schedulerControl = new Extract.Utilities.Forms.SchedulerControl();
            this._tabControl = new System.Windows.Forms.TabControl();
            this._settingsTabPage = new System.Windows.Forms.TabPage();
            this._featuresGroupBox = new System.Windows.Forms.GroupBox();
            this._featureRulesetBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._featureRulesetTextBox = new System.Windows.Forms.TextBox();
            this._runRulesetForFeaturesRadioButton = new System.Windows.Forms.RadioButton();
            this._runRulesetIfVoaIsMissingCheckBox = new System.Windows.Forms.CheckBox();
            this._useVoaFileForFeaturesRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._useVoaFileForExpectedsRadioButton = new System.Windows.Forms.RadioButton();
            this._useAttributeSetForExpectedsRadioButton = new System.Windows.Forms.RadioButton();
            this._scheduleTabPage = new System.Windows.Forms.TabPage();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._limitProcessingByDateNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._lastIDProcessedNumericUpDown)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._trainingPercentageNumericUpDown)).BeginInit();
            this._tabControl.SuspendLayout();
            this._settingsTabPage.SuspendLayout();
            this._featuresGroupBox.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this._scheduleTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._deleteMLDataButton);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this._limitProcessingByDateNumericUpDown);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this._manageMLModelsButton);
            this.groupBox1.Controls.Add(this._attributeSetNameComboBox);
            this.groupBox1.Controls.Add(this._addModelButton);
            this.groupBox1.Controls.Add(this._modelNameComboBox);
            this.groupBox1.Controls.Add(this._lastIDProcessedNumericUpDown);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(440, 132);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            // 
            // _deleteMLDataButton
            // 
            this._deleteMLDataButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._deleteMLDataButton.Location = new System.Drawing.Point(299, 99);
            this._deleteMLDataButton.Name = "_deleteMLDataButton";
            this._deleteMLDataButton.Size = new System.Drawing.Size(135, 23);
            this._deleteMLDataButton.TabIndex = 11;
            this._deleteMLDataButton.Text = "Delete ML data...";
            this._deleteMLDataButton.UseVisualStyleBackColor = true;
            this._deleteMLDataButton.Click += new System.EventHandler(this.HandleDeleteMLDataButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(213, 104);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "days";
            // 
            // _limitProcessingByDateNumericUpDown
            // 
            this._limitProcessingByDateNumericUpDown.Location = new System.Drawing.Point(159, 101);
            this._limitProcessingByDateNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._limitProcessingByDateNumericUpDown.Name = "_limitProcessingByDateNumericUpDown";
            this._limitProcessingByDateNumericUpDown.Size = new System.Drawing.Size(47, 20);
            this._limitProcessingByDateNumericUpDown.TabIndex = 9;
            this._limitProcessingByDateNumericUpDown.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this._limitProcessingByDateNumericUpDown.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 103);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(141, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Limit to files stored in the last";
            // 
            // _manageMLModelsButton
            // 
            this._manageMLModelsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._manageMLModelsButton.Location = new System.Drawing.Point(371, 18);
            this._manageMLModelsButton.Name = "_manageMLModelsButton";
            this._manageMLModelsButton.Size = new System.Drawing.Size(63, 23);
            this._manageMLModelsButton.TabIndex = 5;
            this._manageMLModelsButton.Text = "Manage...";
            this._manageMLModelsButton.UseVisualStyleBackColor = true;
            this._manageMLModelsButton.Click += new System.EventHandler(this.Handle_ManageMLModelsButton_Click);
            // 
            // _attributeSetNameComboBox
            // 
            this._attributeSetNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeSetNameComboBox.FormattingEnabled = true;
            this._attributeSetNameComboBox.Location = new System.Drawing.Point(120, 46);
            this._attributeSetNameComboBox.Name = "_attributeSetNameComboBox";
            this._attributeSetNameComboBox.Size = new System.Drawing.Size(244, 21);
            this._attributeSetNameComboBox.TabIndex = 6;
            this._attributeSetNameComboBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _addModelButton
            // 
            this._addModelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addModelButton.Location = new System.Drawing.Point(289, 18);
            this._addModelButton.Name = "_addModelButton";
            this._addModelButton.Size = new System.Drawing.Size(75, 23);
            this._addModelButton.TabIndex = 4;
            this._addModelButton.Text = "Add new...";
            this._addModelButton.UseVisualStyleBackColor = true;
            this._addModelButton.Click += new System.EventHandler(this.HandleAddModelButton_Click);
            // 
            // _modelNameComboBox
            // 
            this._modelNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._modelNameComboBox.FormattingEnabled = true;
            this._modelNameComboBox.Location = new System.Drawing.Point(120, 19);
            this._modelNameComboBox.Name = "_modelNameComboBox";
            this._modelNameComboBox.Size = new System.Drawing.Size(163, 21);
            this._modelNameComboBox.TabIndex = 3;
            this._modelNameComboBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _lastIDProcessedNumericUpDown
            // 
            this._lastIDProcessedNumericUpDown.Location = new System.Drawing.Point(120, 74);
            this._lastIDProcessedNumericUpDown.Name = "_lastIDProcessedNumericUpDown";
            this._lastIDProcessedNumericUpDown.Size = new System.Drawing.Size(120, 20);
            this._lastIDProcessedNumericUpDown.TabIndex = 7;
            this._lastIDProcessedNumericUpDown.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Last ID processed";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Attribute set name";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Model name";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(321, 511);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 2;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(402, 511);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 3;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Click += new System.EventHandler(this.HandleCancelButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this._changeAnswerButton);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this._trainingPercentageNumericUpDown);
            this.groupBox2.Controls.Add(this._overrideTrainingTestingSplitCheckBox);
            this.groupBox2.Controls.Add(this._lmModelTypeRadioButton);
            this.groupBox2.Controls.Add(this._nerModelTypeRadioButton);
            this.groupBox2.Controls.Add(this._dataGeneratorPathBrowseButton);
            this.groupBox2.Controls.Add(this._dataGeneratorPathTextBox);
            this.groupBox2.Location = new System.Drawing.Point(6, 144);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(440, 106);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Settings file";
            // 
            // _changeAnswerButton
            // 
            this._changeAnswerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._changeAnswerButton.Location = new System.Drawing.Point(299, 72);
            this._changeAnswerButton.Name = "_changeAnswerButton";
            this._changeAnswerButton.Size = new System.Drawing.Size(135, 23);
            this._changeAnswerButton.TabIndex = 17;
            this._changeAnswerButton.Text = "Change an answer...";
            this._changeAnswerButton.UseVisualStyleBackColor = true;
            this._changeAnswerButton.Click += new System.EventHandler(this.HandleChangeAnswerButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(179, 75);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Training %";
            // 
            // _trainingPercentageNumericUpDown
            // 
            this._trainingPercentageNumericUpDown.Location = new System.Drawing.Point(240, 73);
            this._trainingPercentageNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._trainingPercentageNumericUpDown.Name = "_trainingPercentageNumericUpDown";
            this._trainingPercentageNumericUpDown.Size = new System.Drawing.Size(42, 20);
            this._trainingPercentageNumericUpDown.TabIndex = 13;
            this._trainingPercentageNumericUpDown.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            this._trainingPercentageNumericUpDown.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _overrideTrainingTestingSplitCheckBox
            // 
            this._overrideTrainingTestingSplitCheckBox.AutoSize = true;
            this._overrideTrainingTestingSplitCheckBox.Location = new System.Drawing.Point(11, 74);
            this._overrideTrainingTestingSplitCheckBox.Name = "_overrideTrainingTestingSplitCheckBox";
            this._overrideTrainingTestingSplitCheckBox.Size = new System.Drawing.Size(160, 17);
            this._overrideTrainingTestingSplitCheckBox.TabIndex = 12;
            this._overrideTrainingTestingSplitCheckBox.Text = "Override training/testing split";
            this._overrideTrainingTestingSplitCheckBox.UseVisualStyleBackColor = true;
            this._overrideTrainingTestingSplitCheckBox.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
            // 
            // _lmModelTypeRadioButton
            // 
            this._lmModelTypeRadioButton.AutoSize = true;
            this._lmModelTypeRadioButton.Location = new System.Drawing.Point(218, 19);
            this._lmModelTypeRadioButton.Name = "_lmModelTypeRadioButton";
            this._lmModelTypeRadioButton.Size = new System.Drawing.Size(144, 17);
            this._lmModelTypeRadioButton.TabIndex = 9;
            this._lmModelTypeRadioButton.TabStop = true;
            this._lmModelTypeRadioButton.Text = "LM data encoder (.lm file)";
            this._lmModelTypeRadioButton.UseVisualStyleBackColor = true;
            this._lmModelTypeRadioButton.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
            // 
            // _nerModelTypeRadioButton
            // 
            this._nerModelTypeRadioButton.AutoSize = true;
            this._nerModelTypeRadioButton.Location = new System.Drawing.Point(11, 19);
            this._nerModelTypeRadioButton.Name = "_nerModelTypeRadioButton";
            this._nerModelTypeRadioButton.Size = new System.Drawing.Size(173, 17);
            this._nerModelTypeRadioButton.TabIndex = 8;
            this._nerModelTypeRadioButton.TabStop = true;
            this._nerModelTypeRadioButton.Text = "NER annotator (*.annotator file)";
            this._nerModelTypeRadioButton.UseVisualStyleBackColor = true;
            this._nerModelTypeRadioButton.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
            // 
            // _dataGeneratorPathBrowseButton
            // 
            this._dataGeneratorPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._dataGeneratorPathBrowseButton.EnsureFileExists = false;
            this._dataGeneratorPathBrowseButton.EnsurePathExists = false;
            this._dataGeneratorPathBrowseButton.Location = new System.Drawing.Point(371, 42);
            this._dataGeneratorPathBrowseButton.Name = "_dataGeneratorPathBrowseButton";
            this._dataGeneratorPathBrowseButton.Size = new System.Drawing.Size(63, 20);
            this._dataGeneratorPathBrowseButton.TabIndex = 11;
            this._dataGeneratorPathBrowseButton.Text = "...";
            this._dataGeneratorPathBrowseButton.TextControl = this._dataGeneratorPathTextBox;
            this._dataGeneratorPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _dataGeneratorPathTextBox
            // 
            this._dataGeneratorPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataGeneratorPathTextBox.Location = new System.Drawing.Point(11, 42);
            this._dataGeneratorPathTextBox.Name = "_dataGeneratorPathTextBox";
            this._dataGeneratorPathTextBox.Size = new System.Drawing.Size(354, 20);
            this._dataGeneratorPathTextBox.TabIndex = 10;
            this._dataGeneratorPathTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _descriptionTextBox
            // 
            this._descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._descriptionTextBox.Location = new System.Drawing.Point(82, 6);
            this._descriptionTextBox.Name = "_descriptionTextBox";
            this._descriptionTextBox.Size = new System.Drawing.Size(390, 20);
            this._descriptionTextBox.TabIndex = 0;
            this._descriptionTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 9);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(60, 13);
            this.label13.TabIndex = 7;
            this.label13.Text = "Description";
            // 
            // _schedulerControl
            // 
            this._schedulerControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._schedulerControl.Location = new System.Drawing.Point(6, 6);
            this._schedulerControl.Name = "_schedulerControl";
            this._schedulerControl.Size = new System.Drawing.Size(378, 153);
            this._schedulerControl.TabIndex = 13;
            scheduledEvent1.Duration = null;
            scheduledEvent1.Enabled = true;
            scheduledEvent1.End = null;
            scheduledEvent1.Exclusions = new Extract.Utilities.ScheduledEvent[0];
            scheduledEvent1.RecurrenceUnit = null;
            scheduledEvent1.Start = new System.DateTime(2018, 3, 23, 13, 37, 55, 0);
            this._schedulerControl.Value = scheduledEvent1;
            this._schedulerControl.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _tabControl
            // 
            this._tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tabControl.Controls.Add(this._settingsTabPage);
            this._tabControl.Controls.Add(this._scheduleTabPage);
            this._tabControl.Location = new System.Drawing.Point(12, 35);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(460, 470);
            this._tabControl.TabIndex = 1;
            // 
            // _settingsTabPage
            // 
            this._settingsTabPage.Controls.Add(this._featuresGroupBox);
            this._settingsTabPage.Controls.Add(this.groupBox3);
            this._settingsTabPage.Controls.Add(this.groupBox1);
            this._settingsTabPage.Controls.Add(this.groupBox2);
            this._settingsTabPage.Location = new System.Drawing.Point(4, 22);
            this._settingsTabPage.Name = "_settingsTabPage";
            this._settingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._settingsTabPage.Size = new System.Drawing.Size(452, 444);
            this._settingsTabPage.TabIndex = 0;
            this._settingsTabPage.Text = "Settings";
            this._settingsTabPage.UseVisualStyleBackColor = true;
            // 
            // _featuresGroupBox
            // 
            this._featuresGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._featuresGroupBox.Controls.Add(this._featureRulesetBrowseButton);
            this._featuresGroupBox.Controls.Add(this._runRulesetForFeaturesRadioButton);
            this._featuresGroupBox.Controls.Add(this._featureRulesetTextBox);
            this._featuresGroupBox.Controls.Add(this._runRulesetIfVoaIsMissingCheckBox);
            this._featuresGroupBox.Controls.Add(this._useVoaFileForFeaturesRadioButton);
            this._featuresGroupBox.Location = new System.Drawing.Point(6, 334);
            this._featuresGroupBox.Name = "_featuresGroupBox";
            this._featuresGroupBox.Size = new System.Drawing.Size(440, 99);
            this._featuresGroupBox.TabIndex = 11;
            this._featuresGroupBox.TabStop = false;
            this._featuresGroupBox.Text = "Candidates/proto-features";
            // 
            // _featureRulesetBrowseButton
            // 
            this._featureRulesetBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._featureRulesetBrowseButton.EnsureFileExists = false;
            this._featureRulesetBrowseButton.EnsurePathExists = false;
            this._featureRulesetBrowseButton.Location = new System.Drawing.Point(368, 65);
            this._featureRulesetBrowseButton.Name = "_featureRulesetBrowseButton";
            this._featureRulesetBrowseButton.Size = new System.Drawing.Size(63, 20);
            this._featureRulesetBrowseButton.TabIndex = 4;
            this._featureRulesetBrowseButton.Text = "...";
            this._featureRulesetBrowseButton.TextControl = this._featureRulesetTextBox;
            this._featureRulesetBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _featureRulesetTextBox
            // 
            this._featureRulesetTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._featureRulesetTextBox.Location = new System.Drawing.Point(8, 65);
            this._featureRulesetTextBox.Name = "_featureRulesetTextBox";
            this._featureRulesetTextBox.Size = new System.Drawing.Size(354, 20);
            this._featureRulesetTextBox.TabIndex = 3;
            this._featureRulesetTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _runRulesetForFeaturesRadioButton
            // 
            this._runRulesetForFeaturesRadioButton.AutoSize = true;
            this._runRulesetForFeaturesRadioButton.Location = new System.Drawing.Point(8, 42);
            this._runRulesetForFeaturesRadioButton.Name = "_runRulesetForFeaturesRadioButton";
            this._runRulesetForFeaturesRadioButton.Size = new System.Drawing.Size(98, 17);
            this._runRulesetForFeaturesRadioButton.TabIndex = 1;
            this._runRulesetForFeaturesRadioButton.TabStop = true;
            this._runRulesetForFeaturesRadioButton.Text = "Run this ruleset";
            this._runRulesetForFeaturesRadioButton.UseVisualStyleBackColor = true;
            this._runRulesetForFeaturesRadioButton.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
            // 
            // _runRulesetIfVoaIsMissingCheckBox
            // 
            this._runRulesetIfVoaIsMissingCheckBox.AutoSize = true;
            this._runRulesetIfVoaIsMissingCheckBox.Location = new System.Drawing.Point(132, 42);
            this._runRulesetIfVoaIsMissingCheckBox.Name = "_runRulesetIfVoaIsMissingCheckBox";
            this._runRulesetIfVoaIsMissingCheckBox.Size = new System.Drawing.Size(230, 17);
            this._runRulesetIfVoaIsMissingCheckBox.TabIndex = 2;
            this._runRulesetIfVoaIsMissingCheckBox.Text = "Run ruleset if (attribute set for) file is missing";
            this._runRulesetIfVoaIsMissingCheckBox.UseVisualStyleBackColor = true;
            this._runRulesetIfVoaIsMissingCheckBox.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
            // 
            // _useVoaFileForFeaturesRadioButton
            // 
            this._useVoaFileForFeaturesRadioButton.AutoSize = true;
            this._useVoaFileForFeaturesRadioButton.Location = new System.Drawing.Point(8, 19);
            this._useVoaFileForFeaturesRadioButton.Name = "_useVoaFileForFeaturesRadioButton";
            this._useVoaFileForFeaturesRadioButton.Size = new System.Drawing.Size(222, 17);
            this._useVoaFileForFeaturesRadioButton.TabIndex = 0;
            this._useVoaFileForFeaturesRadioButton.TabStop = true;
            this._useVoaFileForFeaturesRadioButton.Text = "Use VOA file configured in the settings file";
            this._useVoaFileForFeaturesRadioButton.UseVisualStyleBackColor = true;
            this._useVoaFileForFeaturesRadioButton.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this._useVoaFileForExpectedsRadioButton);
            this.groupBox3.Controls.Add(this._useAttributeSetForExpectedsRadioButton);
            this.groupBox3.Location = new System.Drawing.Point(6, 256);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(440, 72);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Expected values";
            // 
            // _useVoaFileForExpectedsRadioButton
            // 
            this._useVoaFileForExpectedsRadioButton.AutoSize = true;
            this._useVoaFileForExpectedsRadioButton.Location = new System.Drawing.Point(11, 42);
            this._useVoaFileForExpectedsRadioButton.Name = "_useVoaFileForExpectedsRadioButton";
            this._useVoaFileForExpectedsRadioButton.Size = new System.Drawing.Size(307, 17);
            this._useVoaFileForExpectedsRadioButton.TabIndex = 12;
            this._useVoaFileForExpectedsRadioButton.TabStop = true;
            this._useVoaFileForExpectedsRadioButton.Text = "Use the configuration of the settings file for expected values";
            this._useVoaFileForExpectedsRadioButton.UseVisualStyleBackColor = true;
            this._useVoaFileForExpectedsRadioButton.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
            // 
            // _useAttributeSetForExpectedsRadioButton
            // 
            this._useAttributeSetForExpectedsRadioButton.AutoSize = true;
            this._useAttributeSetForExpectedsRadioButton.Location = new System.Drawing.Point(11, 19);
            this._useAttributeSetForExpectedsRadioButton.Name = "_useAttributeSetForExpectedsRadioButton";
            this._useAttributeSetForExpectedsRadioButton.Size = new System.Drawing.Size(237, 17);
            this._useAttributeSetForExpectedsRadioButton.TabIndex = 11;
            this._useAttributeSetForExpectedsRadioButton.TabStop = true;
            this._useAttributeSetForExpectedsRadioButton.Text = "Use attribute set (above) for expected values";
            this._useAttributeSetForExpectedsRadioButton.UseVisualStyleBackColor = true;
            this._useAttributeSetForExpectedsRadioButton.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
            // 
            // _scheduleTabPage
            // 
            this._scheduleTabPage.Controls.Add(this._schedulerControl);
            this._scheduleTabPage.Location = new System.Drawing.Point(4, 22);
            this._scheduleTabPage.Name = "_scheduleTabPage";
            this._scheduleTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._scheduleTabPage.Size = new System.Drawing.Size(452, 444);
            this._scheduleTabPage.TabIndex = 1;
            this._scheduleTabPage.Text = "Schedule";
            this._scheduleTabPage.UseVisualStyleBackColor = true;
            // 
            // TrainingDataCollectorConfigurationDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 540);
            this.Controls.Add(this._tabControl);
            this.Controls.Add(this._descriptionTextBox);
            this.Controls.Add(this.label13);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 579);
            this.Name = "TrainingDataCollectorConfigurationDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Training data collector";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._limitProcessingByDateNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._lastIDProcessedNumericUpDown)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._trainingPercentageNumericUpDown)).EndInit();
            this._tabControl.ResumeLayout(false);
            this._settingsTabPage.ResumeLayout(false);
            this._featuresGroupBox.ResumeLayout(false);
            this._featuresGroupBox.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this._scheduleTabPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.NumericUpDown _lastIDProcessedNumericUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _addModelButton;
        private System.Windows.Forms.ComboBox _modelNameComboBox;
        private System.Windows.Forms.ComboBox _attributeSetNameComboBox;
        private System.Windows.Forms.Button _manageMLModelsButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton _lmModelTypeRadioButton;
        private System.Windows.Forms.RadioButton _nerModelTypeRadioButton;
        private Utilities.Forms.BrowseButton _dataGeneratorPathBrowseButton;
        private System.Windows.Forms.TextBox _dataGeneratorPathTextBox;
        private System.Windows.Forms.TextBox _descriptionTextBox;
        private System.Windows.Forms.Label label13;
        private Utilities.Forms.SchedulerControl _schedulerControl;
        private System.Windows.Forms.TabControl _tabControl;
        private System.Windows.Forms.TabPage _settingsTabPage;
        private System.Windows.Forms.TabPage _scheduleTabPage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown _trainingPercentageNumericUpDown;
        private System.Windows.Forms.CheckBox _overrideTrainingTestingSplitCheckBox;
        private System.Windows.Forms.GroupBox _featuresGroupBox;
        private System.Windows.Forms.RadioButton _useVoaFileForFeaturesRadioButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton _useVoaFileForExpectedsRadioButton;
        private System.Windows.Forms.RadioButton _useAttributeSetForExpectedsRadioButton;
        private Utilities.Forms.BrowseButton _featureRulesetBrowseButton;
        private System.Windows.Forms.TextBox _featureRulesetTextBox;
        private System.Windows.Forms.RadioButton _runRulesetForFeaturesRadioButton;
        private System.Windows.Forms.CheckBox _runRulesetIfVoaIsMissingCheckBox;
        private System.Windows.Forms.Button _changeAnswerButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown _limitProcessingByDateNumericUpDown;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button _deleteMLDataButton;
    }
}