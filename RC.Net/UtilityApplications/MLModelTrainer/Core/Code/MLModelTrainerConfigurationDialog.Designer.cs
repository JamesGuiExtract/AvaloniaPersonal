using Extract.AttributeFinder;
using Extract.FileActionManager.Forms;

namespace Extract.UtilityApplications.MachineLearning
{
    partial class MLModelTrainerConfigurationDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MLModelTrainerConfigurationDialog));
            Extract.Utilities.ScheduledEvent scheduledEvent1 = new Extract.Utilities.ScheduledEvent();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._manageMLModelsButton = new System.Windows.Forms.Button();
            this._addModelButton = new System.Windows.Forms.Button();
            this._modelNameComboBox = new System.Windows.Forms.ComboBox();
            this._modelDestinationPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._modelDestinationPathTextBox = new System.Windows.Forms.TextBox();
            this._modelPathLabel = new System.Windows.Forms.Label();
            this._trainingCommandPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._trainingCommandTextBox = new System.Windows.Forms.TextBox();
            this._testingCommandPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._testingCommandTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._updateCommandTextBox = new System.Windows.Forms.TextBox();
            this._maxTestingRecordsNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this._minF1ScoreNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this._maxTrainingRecordsNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this._allowableAccuracyDropNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this._lastIDProcessedNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this._lastF1ScoreNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._deleteMLDataButton = new System.Windows.Forms.Button();
            this._markOldDataForDeletionCheckBox = new System.Windows.Forms.CheckBox();
            this._emailSubjectTextBox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this._emailAddressesTextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this._descriptionTextBox = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this._settingsTabPage = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this._changeAnswerButton = new System.Windows.Forms.Button();
            this._lmModelTypeRadioButton = new System.Windows.Forms.RadioButton();
            this._nerModelTypeRadioButton = new System.Windows.Forms.RadioButton();
            this._scheduleTabPage = new System.Windows.Forms.TabPage();
            this._schedulerControl = new Extract.Utilities.Forms.SchedulerControl();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._maxTestingRecordsNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._minF1ScoreNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._maxTrainingRecordsNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._allowableAccuracyDropNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._lastIDProcessedNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._lastF1ScoreNumericUpDown)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this._settingsTabPage.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this._scheduleTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._manageMLModelsButton);
            this.groupBox1.Controls.Add(this._addModelButton);
            this.groupBox1.Controls.Add(this._modelNameComboBox);
            this.groupBox1.Controls.Add(this._modelDestinationPathBrowseButton);
            this.groupBox1.Controls.Add(this._modelPathLabel);
            this.groupBox1.Controls.Add(this._modelDestinationPathTextBox);
            this.groupBox1.Controls.Add(this._trainingCommandPathTagsButton);
            this.groupBox1.Controls.Add(this._testingCommandPathTagsButton);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._trainingCommandTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this._testingCommandTextBox);
            this.groupBox1.Location = new System.Drawing.Point(7, 62);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(608, 129);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // _manageMLModelsButton
            // 
            this._manageMLModelsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._manageMLModelsButton.Location = new System.Drawing.Point(536, 18);
            this._manageMLModelsButton.Name = "_manageMLModelsButton";
            this._manageMLModelsButton.Size = new System.Drawing.Size(63, 23);
            this._manageMLModelsButton.TabIndex = 6;
            this._manageMLModelsButton.Text = "Manage...";
            this._manageMLModelsButton.UseVisualStyleBackColor = true;
            this._manageMLModelsButton.Click += new System.EventHandler(this.Handle_ManageMLModelsButton_Click);
            // 
            // _addModelButton
            // 
            this._addModelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addModelButton.Location = new System.Drawing.Point(454, 18);
            this._addModelButton.Name = "_addModelButton";
            this._addModelButton.Size = new System.Drawing.Size(75, 23);
            this._addModelButton.TabIndex = 5;
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
            this._modelNameComboBox.Size = new System.Drawing.Size(328, 21);
            this._modelNameComboBox.TabIndex = 4;
            this._modelNameComboBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _modelDestinationPathBrowseButton
            // 
            this._modelDestinationPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._modelDestinationPathBrowseButton.EnsureFileExists = false;
            this._modelDestinationPathBrowseButton.EnsurePathExists = false;
            this._modelDestinationPathBrowseButton.Location = new System.Drawing.Point(570, 98);
            this._modelDestinationPathBrowseButton.Name = "_modelDestinationPathBrowseButton";
            this._modelDestinationPathBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._modelDestinationPathBrowseButton.TabIndex = 13;
            this._modelDestinationPathBrowseButton.Text = "...";
            this._modelDestinationPathBrowseButton.TextControl = this._modelDestinationPathTextBox;
            this._modelDestinationPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _modelDestinationPathTextBox
            // 
            this._modelDestinationPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._modelDestinationPathTextBox.Location = new System.Drawing.Point(120, 98);
            this._modelDestinationPathTextBox.Name = "_modelDestinationPathTextBox";
            this._modelDestinationPathTextBox.Size = new System.Drawing.Size(443, 20);
            this._modelDestinationPathTextBox.TabIndex = 11;
            this._modelDestinationPathTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _modelPathLabel
            // 
            this._modelPathLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._modelPathLabel.AutoSize = true;
            this._modelPathLabel.Location = new System.Drawing.Point(6, 100);
            this._modelPathLabel.Name = "_modelPathLabel";
            this._modelPathLabel.Size = new System.Drawing.Size(84, 13);
            this._modelPathLabel.TabIndex = 10;
            this._modelPathLabel.Text = "Destination path";
            // 
            // _trainingCommandPathTagsButton
            // 
            this._trainingCommandPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._trainingCommandPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_trainingCommandPathTagsButton.Image")));
            this._trainingCommandPathTagsButton.Location = new System.Drawing.Point(569, 46);
            this._trainingCommandPathTagsButton.Name = "_trainingCommandPathTagsButton";
            this._trainingCommandPathTagsButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._trainingCommandPathTagsButton.Size = new System.Drawing.Size(28, 20);
            this._trainingCommandPathTagsButton.TabIndex = 8;
            this._trainingCommandPathTagsButton.TextControl = this._trainingCommandTextBox;
            this._trainingCommandPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _trainingCommandTextBox
            // 
            this._trainingCommandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._trainingCommandTextBox.Location = new System.Drawing.Point(120, 46);
            this._trainingCommandTextBox.Name = "_trainingCommandTextBox";
            this._trainingCommandTextBox.Size = new System.Drawing.Size(443, 20);
            this._trainingCommandTextBox.TabIndex = 7;
            this._trainingCommandTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _testingCommandPathTagsButton
            // 
            this._testingCommandPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._testingCommandPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_testingCommandPathTagsButton.Image")));
            this._testingCommandPathTagsButton.Location = new System.Drawing.Point(569, 72);
            this._testingCommandPathTagsButton.Name = "_testingCommandPathTagsButton";
            this._testingCommandPathTagsButton.PathTags = new Extract.AttributeFinder.AttributeFinderPathTags();
            this._testingCommandPathTagsButton.Size = new System.Drawing.Size(28, 20);
            this._testingCommandPathTagsButton.TabIndex = 10;
            this._testingCommandPathTagsButton.TextControl = this._testingCommandTextBox;
            this._testingCommandPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _testingCommandTextBox
            // 
            this._testingCommandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._testingCommandTextBox.Location = new System.Drawing.Point(120, 72);
            this._testingCommandTextBox.Name = "_testingCommandTextBox";
            this._testingCommandTextBox.Size = new System.Drawing.Size(443, 20);
            this._testingCommandTextBox.TabIndex = 9;
            this._testingCommandTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Model name";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Training command";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Testing command";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(487, 542);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 24;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(568, 542);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 25;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Click += new System.EventHandler(this.HandleCancelButton_Click);
            // 
            // _updateCommandTextBox
            // 
            this._updateCommandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._updateCommandTextBox.Location = new System.Drawing.Point(120, 97);
            this._updateCommandTextBox.Name = "_updateCommandTextBox";
            this._updateCommandTextBox.Size = new System.Drawing.Size(448, 20);
            this._updateCommandTextBox.TabIndex = 7;
            // 
            // _maxTestingRecordsNumericUpDown
            // 
            this._maxTestingRecordsNumericUpDown.Location = new System.Drawing.Point(160, 40);
            this._maxTestingRecordsNumericUpDown.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this._maxTestingRecordsNumericUpDown.Name = "_maxTestingRecordsNumericUpDown";
            this._maxTestingRecordsNumericUpDown.Size = new System.Drawing.Size(120, 20);
            this._maxTestingRecordsNumericUpDown.TabIndex = 15;
            this._maxTestingRecordsNumericUpDown.ThousandsSeparator = true;
            this._maxTestingRecordsNumericUpDown.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 42);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(123, 13);
            this.label10.TabIndex = 2;
            this.label10.Text = "Maximum testing records";
            // 
            // _minF1ScoreNumericUpDown
            // 
            this._minF1ScoreNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._minF1ScoreNumericUpDown.DecimalPlaces = 6;
            this._minF1ScoreNumericUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this._minF1ScoreNumericUpDown.Location = new System.Drawing.Point(530, 40);
            this._minF1ScoreNumericUpDown.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._minF1ScoreNumericUpDown.Name = "_minF1ScoreNumericUpDown";
            this._minF1ScoreNumericUpDown.Size = new System.Drawing.Size(69, 20);
            this._minF1ScoreNumericUpDown.TabIndex = 17;
            this._minF1ScoreNumericUpDown.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(382, 42);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(139, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "Minimum allowable F1 score";
            // 
            // _maxTrainingRecordsNumericUpDown
            // 
            this._maxTrainingRecordsNumericUpDown.Location = new System.Drawing.Point(160, 14);
            this._maxTrainingRecordsNumericUpDown.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this._maxTrainingRecordsNumericUpDown.Name = "_maxTrainingRecordsNumericUpDown";
            this._maxTrainingRecordsNumericUpDown.Size = new System.Drawing.Size(120, 20);
            this._maxTrainingRecordsNumericUpDown.TabIndex = 14;
            this._maxTrainingRecordsNumericUpDown.ThousandsSeparator = true;
            this._maxTrainingRecordsNumericUpDown.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 16);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(126, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Maximum training records";
            // 
            // _allowableAccuracyDropNumericUpDown
            // 
            this._allowableAccuracyDropNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._allowableAccuracyDropNumericUpDown.DecimalPlaces = 6;
            this._allowableAccuracyDropNumericUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this._allowableAccuracyDropNumericUpDown.Location = new System.Drawing.Point(530, 14);
            this._allowableAccuracyDropNumericUpDown.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._allowableAccuracyDropNumericUpDown.Name = "_allowableAccuracyDropNumericUpDown";
            this._allowableAccuracyDropNumericUpDown.Size = new System.Drawing.Size(69, 20);
            this._allowableAccuracyDropNumericUpDown.TabIndex = 16;
            this._allowableAccuracyDropNumericUpDown.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(398, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(123, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Allowable accuracy drop";
            // 
            // _lastIDProcessedNumericUpDown
            // 
            this._lastIDProcessedNumericUpDown.Location = new System.Drawing.Point(120, 14);
            this._lastIDProcessedNumericUpDown.Name = "_lastIDProcessedNumericUpDown";
            this._lastIDProcessedNumericUpDown.Size = new System.Drawing.Size(120, 20);
            this._lastIDProcessedNumericUpDown.TabIndex = 21;
            this._lastIDProcessedNumericUpDown.ThousandsSeparator = true;
            this._lastIDProcessedNumericUpDown.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Last ID processed";
            // 
            // _lastF1ScoreNumericUpDown
            // 
            this._lastF1ScoreNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._lastF1ScoreNumericUpDown.DecimalPlaces = 6;
            this._lastF1ScoreNumericUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this._lastF1ScoreNumericUpDown.Location = new System.Drawing.Point(530, 14);
            this._lastF1ScoreNumericUpDown.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._lastF1ScoreNumericUpDown.Name = "_lastF1ScoreNumericUpDown";
            this._lastF1ScoreNumericUpDown.Size = new System.Drawing.Size(69, 20);
            this._lastF1ScoreNumericUpDown.TabIndex = 22;
            this._lastF1ScoreNumericUpDown.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(401, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(120, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Last measured F1 score";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this._lastF1ScoreNumericUpDown);
            this.groupBox2.Controls.Add(this._lastIDProcessedNumericUpDown);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(7, 403);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(608, 44);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this._deleteMLDataButton);
            this.groupBox3.Controls.Add(this._markOldDataForDeletionCheckBox);
            this.groupBox3.Controls.Add(this._emailSubjectTextBox);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this._emailAddressesTextBox);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this._allowableAccuracyDropNumericUpDown);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this._minF1ScoreNumericUpDown);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this._maxTrainingRecordsNumericUpDown);
            this.groupBox3.Controls.Add(this._maxTestingRecordsNumericUpDown);
            this.groupBox3.Location = new System.Drawing.Point(7, 200);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(608, 197);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            // 
            // _deleteMLDataButton
            // 
            this._deleteMLDataButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._deleteMLDataButton.Location = new System.Drawing.Point(463, 66);
            this._deleteMLDataButton.Name = "_deleteMLDataButton";
            this._deleteMLDataButton.Size = new System.Drawing.Size(135, 23);
            this._deleteMLDataButton.TabIndex = 21;
            this._deleteMLDataButton.Text = "Delete ML data...";
            this._deleteMLDataButton.UseVisualStyleBackColor = true;
            this._deleteMLDataButton.Click += new System.EventHandler(this.HandleDeleteMLDataButton_Click);
            // 
            // _markOldDataForDeletionCheckBox
            // 
            this._markOldDataForDeletionCheckBox.AutoSize = true;
            this._markOldDataForDeletionCheckBox.Location = new System.Drawing.Point(10, 66);
            this._markOldDataForDeletionCheckBox.Name = "_markOldDataForDeletionCheckBox";
            this._markOldDataForDeletionCheckBox.Size = new System.Drawing.Size(248, 17);
            this._markOldDataForDeletionCheckBox.TabIndex = 20;
            this._markOldDataForDeletionCheckBox.Text = "On success, mark old, unused data for deletion";
            this._markOldDataForDeletionCheckBox.UseVisualStyleBackColor = true;
            this._markOldDataForDeletionCheckBox.CheckedChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _emailSubjectTextBox
            // 
            this._emailSubjectTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailSubjectTextBox.Location = new System.Drawing.Point(8, 167);
            this._emailSubjectTextBox.Name = "_emailSubjectTextBox";
            this._emailSubjectTextBox.Size = new System.Drawing.Size(591, 20);
            this._emailSubjectTextBox.TabIndex = 19;
            this._emailSubjectTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 146);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(69, 13);
            this.label12.TabIndex = 10;
            this.label12.Text = "Email subject";
            // 
            // _emailAddressesTextBox
            // 
            this._emailAddressesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailAddressesTextBox.Location = new System.Drawing.Point(7, 116);
            this._emailAddressesTextBox.Name = "_emailAddressesTextBox";
            this._emailAddressesTextBox.Size = new System.Drawing.Size(591, 20);
            this._emailAddressesTextBox.TabIndex = 18;
            this._emailAddressesTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(5, 95);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(289, 13);
            this.label11.TabIndex = 8;
            this.label11.Text = "On failure, send email to (separate addresses with a comma)";
            // 
            // _descriptionTextBox
            // 
            this._descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._descriptionTextBox.Location = new System.Drawing.Point(80, 9);
            this._descriptionTextBox.Name = "_descriptionTextBox";
            this._descriptionTextBox.Size = new System.Drawing.Size(563, 20);
            this._descriptionTextBox.TabIndex = 1;
            this._descriptionTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 9);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(60, 13);
            this.label13.TabIndex = 5;
            this.label13.Text = "Description";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this._settingsTabPage);
            this.tabControl1.Controls.Add(this._scheduleTabPage);
            this.tabControl1.Location = new System.Drawing.Point(15, 48);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(630, 481);
            this.tabControl1.TabIndex = 2;
            // 
            // _settingsTabPage
            // 
            this._settingsTabPage.BackColor = System.Drawing.SystemColors.Control;
            this._settingsTabPage.Controls.Add(this.groupBox1);
            this._settingsTabPage.Controls.Add(this.groupBox2);
            this._settingsTabPage.Controls.Add(this.groupBox3);
            this._settingsTabPage.Controls.Add(this.groupBox4);
            this._settingsTabPage.Location = new System.Drawing.Point(4, 22);
            this._settingsTabPage.Name = "_settingsTabPage";
            this._settingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._settingsTabPage.Size = new System.Drawing.Size(622, 455);
            this._settingsTabPage.TabIndex = 0;
            this._settingsTabPage.Text = "Settings";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this._changeAnswerButton);
            this.groupBox4.Controls.Add(this._lmModelTypeRadioButton);
            this.groupBox4.Controls.Add(this._nerModelTypeRadioButton);
            this.groupBox4.Location = new System.Drawing.Point(7, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(608, 62);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Model type";
            // 
            // _changeAnswerButton
            // 
            this._changeAnswerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._changeAnswerButton.Location = new System.Drawing.Point(454, 19);
            this._changeAnswerButton.Name = "_changeAnswerButton";
            this._changeAnswerButton.Size = new System.Drawing.Size(144, 23);
            this._changeAnswerButton.TabIndex = 16;
            this._changeAnswerButton.Text = "Change an answer...";
            this._changeAnswerButton.UseVisualStyleBackColor = true;
            this._changeAnswerButton.Click += new System.EventHandler(this.HandleChangeAnswerButton_Click);
            // 
            // _lmModelTypeRadioButton
            // 
            this._lmModelTypeRadioButton.AutoSize = true;
            this._lmModelTypeRadioButton.Location = new System.Drawing.Point(245, 19);
            this._lmModelTypeRadioButton.Name = "_lmModelTypeRadioButton";
            this._lmModelTypeRadioButton.Size = new System.Drawing.Size(110, 17);
            this._lmModelTypeRadioButton.TabIndex = 15;
            this._lmModelTypeRadioButton.TabStop = true;
            this._lmModelTypeRadioButton.Text = "Learning Machine";
            this._lmModelTypeRadioButton.UseVisualStyleBackColor = true;
            this._lmModelTypeRadioButton.CheckedChanged += new System.EventHandler(this.Handle_ModelTypeRadioButton_CheckedChanged);
            // 
            // _nerModelTypeRadioButton
            // 
            this._nerModelTypeRadioButton.AutoSize = true;
            this._nerModelTypeRadioButton.Location = new System.Drawing.Point(8, 19);
            this._nerModelTypeRadioButton.Name = "_nerModelTypeRadioButton";
            this._nerModelTypeRadioButton.Size = new System.Drawing.Size(201, 17);
            this._nerModelTypeRadioButton.TabIndex = 14;
            this._nerModelTypeRadioButton.TabStop = true;
            this._nerModelTypeRadioButton.Text = "Open NLP Named Entity Recognition";
            this._nerModelTypeRadioButton.UseVisualStyleBackColor = true;
            this._nerModelTypeRadioButton.CheckedChanged += new System.EventHandler(this.Handle_ModelTypeRadioButton_CheckedChanged);
            // 
            // _scheduleTabPage
            // 
            this._scheduleTabPage.BackColor = System.Drawing.SystemColors.Control;
            this._scheduleTabPage.Controls.Add(this._schedulerControl);
            this._scheduleTabPage.Location = new System.Drawing.Point(4, 22);
            this._scheduleTabPage.Name = "_scheduleTabPage";
            this._scheduleTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._scheduleTabPage.Size = new System.Drawing.Size(622, 455);
            this._scheduleTabPage.TabIndex = 1;
            this._scheduleTabPage.Text = "Schedule";
            // 
            // _schedulerControl
            // 
            this._schedulerControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._schedulerControl.Location = new System.Drawing.Point(6, 6);
            this._schedulerControl.Name = "_schedulerControl";
            this._schedulerControl.Size = new System.Drawing.Size(383, 153);
            this._schedulerControl.TabIndex = 23;
            scheduledEvent1.Duration = null;
            scheduledEvent1.Enabled = true;
            scheduledEvent1.End = null;
            scheduledEvent1.Exclusions = new Extract.Utilities.ScheduledEvent[0];
            scheduledEvent1.RecurrenceUnit = null;
            scheduledEvent1.Start = new System.DateTime(2018, 3, 23, 11, 16, 13, 0);
            this._schedulerControl.Value = scheduledEvent1;
            this._schedulerControl.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // MLModelTrainerConfigurationDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(655, 572);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this._descriptionTextBox);
            this.Controls.Add(this.label13);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(671, 611);
            this.Name = "MLModelTrainerConfigurationDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ML model trainer";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._maxTestingRecordsNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._minF1ScoreNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._maxTrainingRecordsNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._allowableAccuracyDropNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._lastIDProcessedNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._lastF1ScoreNumericUpDown)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this._settingsTabPage.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this._scheduleTabPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _testingCommandTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _trainingCommandTextBox;
        private Utilities.Forms.PathTagsButton _trainingCommandPathTagsButton;
        private Utilities.Forms.PathTagsButton _testingCommandPathTagsButton;
        private System.Windows.Forms.TextBox _modelDestinationPathTextBox;
        private System.Windows.Forms.Label _modelPathLabel;
        private Utilities.Forms.BrowseButton _modelDestinationPathBrowseButton;
        private System.Windows.Forms.TextBox _updateCommandTextBox;
        private System.Windows.Forms.ComboBox _modelNameComboBox;
        private System.Windows.Forms.Button _addModelButton;
        private System.Windows.Forms.Button _manageMLModelsButton;
        private System.Windows.Forms.NumericUpDown _lastIDProcessedNumericUpDown;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown _lastF1ScoreNumericUpDown;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown _allowableAccuracyDropNumericUpDown;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown _maxTrainingRecordsNumericUpDown;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown _minF1ScoreNumericUpDown;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown _maxTestingRecordsNumericUpDown;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox _emailAddressesTextBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox _emailSubjectTextBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox _descriptionTextBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage _settingsTabPage;
        private System.Windows.Forms.TabPage _scheduleTabPage;
        private Utilities.Forms.SchedulerControl _schedulerControl;
        private System.Windows.Forms.RadioButton _nerModelTypeRadioButton;
        private System.Windows.Forms.RadioButton _lmModelTypeRadioButton;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox _markOldDataForDeletionCheckBox;
        private System.Windows.Forms.Button _changeAnswerButton;
        private System.Windows.Forms.Button _deleteMLDataButton;
    }
}