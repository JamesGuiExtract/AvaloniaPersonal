using Extract.FileActionManager.Forms;

namespace Extract.UtilityApplications.TrainingCoordinator
{
    partial class TrainingCoordinatorConfigurationDialog
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
            this.components = new System.ComponentModel.Container();
            Extract.Utilities.ScheduledEvent scheduledEvent1 = new Extract.Utilities.ScheduledEvent();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrainingCoordinatorConfigurationDialog));
            this._globalSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._maxModelBackupsNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this._resetProcessedStatusButton = new System.Windows.Forms.Button();
            this._descriptionTextBox = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._schedulerControl = new Extract.Utilities.Forms.SchedulerControl();
            this._minimumRecordsRequiredNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this._rootDirectoryBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._rootDirectoryTextBox = new System.Windows.Forms.TextBox();
            this._deleteDataCheckBox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._exportButton = new System.Windows.Forms.Button();
            this._importButton = new System.Windows.Forms.Button();
            this._projectNameTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._runStopButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this._modifyTrainerButton = new System.Windows.Forms.Button();
            this._duplicateTrainerButton = new System.Windows.Forms.Button();
            this._downTrainerButton = new Extract.Utilities.Forms.ExtractDownButton();
            this._upTrainerButton = new Extract.Utilities.Forms.ExtractUpButton();
            this._modelTrainersDataGridView = new System.Windows.Forms.DataGridView();
            this._removeTrainerButton = new System.Windows.Forms.Button();
            this._addTrainerButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._modifyCollectorButton = new System.Windows.Forms.Button();
            this._duplicateCollectorButton = new System.Windows.Forms.Button();
            this._downCollectorButton = new Extract.Utilities.Forms.ExtractDownButton();
            this._upCollectorButton = new Extract.Utilities.Forms.ExtractUpButton();
            this._dataCollectorsDataGridView = new System.Windows.Forms.DataGridView();
            this._removeCollectorButton = new System.Windows.Forms.Button();
            this._addCollectorButton = new System.Windows.Forms.Button();
            this._servicesAndLogTabControl = new System.Windows.Forms.TabControl();
            this._mlServicesTabPage = new System.Windows.Forms.TabPage();
            this._logTabPage = new System.Windows.Forms.TabPage();
            this._logTextBox = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this._globalSettingsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._maxModelBackupsNumericUpDown)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._minimumRecordsRequiredNumericUpDown)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._modelTrainersDataGridView)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dataCollectorsDataGridView)).BeginInit();
            this._servicesAndLogTabControl.SuspendLayout();
            this._mlServicesTabPage.SuspendLayout();
            this._logTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // _globalSettingsGroupBox
            // 
            this._globalSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._globalSettingsGroupBox.Controls.Add(this.label5);
            this._globalSettingsGroupBox.Controls.Add(this.label4);
            this._globalSettingsGroupBox.Controls.Add(this._maxModelBackupsNumericUpDown);
            this._globalSettingsGroupBox.Controls.Add(this._resetProcessedStatusButton);
            this._globalSettingsGroupBox.Controls.Add(this._descriptionTextBox);
            this._globalSettingsGroupBox.Controls.Add(this.label13);
            this._globalSettingsGroupBox.Controls.Add(this.groupBox3);
            this._globalSettingsGroupBox.Controls.Add(this._minimumRecordsRequiredNumericUpDown);
            this._globalSettingsGroupBox.Controls.Add(this._rootDirectoryBrowseButton);
            this._globalSettingsGroupBox.Controls.Add(this._deleteDataCheckBox);
            this._globalSettingsGroupBox.Controls.Add(this.label3);
            this._globalSettingsGroupBox.Controls.Add(this._rootDirectoryTextBox);
            this._globalSettingsGroupBox.Controls.Add(this.label1);
            this._globalSettingsGroupBox.Controls.Add(this._exportButton);
            this._globalSettingsGroupBox.Controls.Add(this._importButton);
            this._globalSettingsGroupBox.Controls.Add(this._projectNameTextBox);
            this._globalSettingsGroupBox.Controls.Add(this.label2);
            this._globalSettingsGroupBox.Location = new System.Drawing.Point(11, 12);
            this._globalSettingsGroupBox.Name = "_globalSettingsGroupBox";
            this._globalSettingsGroupBox.Size = new System.Drawing.Size(763, 307);
            this._globalSettingsGroupBox.TabIndex = 0;
            this._globalSettingsGroupBox.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(126, 234);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(180, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "backup copies of supplanted models";
            this.toolTip1.SetToolTip(this.label5, "Stored next to original in folder named \"__ml_model_backups__/yyyy-MM-dd.HH.mmUTC" +
        "\"");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 234);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Keep up to";
            this.toolTip1.SetToolTip(this.label4, "Stored next to original in folder named \"__ml_model_backups__/yyyy-MM-dd.HH.mmUTC" +
        "\"");
            // 
            // _maxModelBackupsNumericUpDown
            // 
            this._maxModelBackupsNumericUpDown.Location = new System.Drawing.Point(71, 232);
            this._maxModelBackupsNumericUpDown.Name = "_maxModelBackupsNumericUpDown";
            this._maxModelBackupsNumericUpDown.Size = new System.Drawing.Size(48, 20);
            this._maxModelBackupsNumericUpDown.TabIndex = 6;
            this.toolTip1.SetToolTip(this._maxModelBackupsNumericUpDown, "Stored next to original in folder named \"__ml_model_backups__/yyyy-MM-dd.HH.mmUTC" +
        "\"");
            this._maxModelBackupsNumericUpDown.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _resetProcessedStatusButton
            // 
            this._resetProcessedStatusButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._resetProcessedStatusButton.Location = new System.Drawing.Point(207, 271);
            this._resetProcessedStatusButton.Name = "_resetProcessedStatusButton";
            this._resetProcessedStatusButton.Size = new System.Drawing.Size(132, 23);
            this._resetProcessedStatusButton.TabIndex = 9;
            this._resetProcessedStatusButton.Text = "Reset processed status";
            this._resetProcessedStatusButton.UseVisualStyleBackColor = true;
            this._resetProcessedStatusButton.Click += new System.EventHandler(this.HandleResetProcessedStatusButton_Click);
            // 
            // _descriptionTextBox
            // 
            this._descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._descriptionTextBox.Location = new System.Drawing.Point(10, 78);
            this._descriptionTextBox.Name = "_descriptionTextBox";
            this._descriptionTextBox.Size = new System.Drawing.Size(329, 20);
            this._descriptionTextBox.TabIndex = 2;
            this._descriptionTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(7, 62);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(60, 13);
            this.label13.TabIndex = 3;
            this.label13.Text = "Description";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this._schedulerControl);
            this.groupBox3.Location = new System.Drawing.Point(354, 62);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(398, 198);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Schedule";
            // 
            // _schedulerControl
            // 
            this._schedulerControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._schedulerControl.Location = new System.Drawing.Point(6, 19);
            this._schedulerControl.Name = "_schedulerControl";
            this._schedulerControl.Size = new System.Drawing.Size(378, 153);
            this._schedulerControl.TabIndex = 0;
            scheduledEvent1.Duration = null;
            scheduledEvent1.Enabled = true;
            scheduledEvent1.End = null;
            scheduledEvent1.Exclusions = new Extract.Utilities.ScheduledEvent[0];
            scheduledEvent1.RecurrenceUnit = null;
            scheduledEvent1.Start = new System.DateTime(2018, 3, 23, 13, 37, 55, 0);
            this._schedulerControl.Value = scheduledEvent1;
            this._schedulerControl.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _minimumRecordsRequiredNumericUpDown
            // 
            this._minimumRecordsRequiredNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._minimumRecordsRequiredNumericUpDown.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this._minimumRecordsRequiredNumericUpDown.Location = new System.Drawing.Point(9, 172);
            this._minimumRecordsRequiredNumericUpDown.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this._minimumRecordsRequiredNumericUpDown.Name = "_minimumRecordsRequiredNumericUpDown";
            this._minimumRecordsRequiredNumericUpDown.Size = new System.Drawing.Size(330, 20);
            this._minimumRecordsRequiredNumericUpDown.TabIndex = 4;
            this._minimumRecordsRequiredNumericUpDown.ThousandsSeparator = true;
            this._minimumRecordsRequiredNumericUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this._minimumRecordsRequiredNumericUpDown.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _rootDirectoryBrowseButton
            // 
            this._rootDirectoryBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._rootDirectoryBrowseButton.EnsureFileExists = false;
            this._rootDirectoryBrowseButton.EnsurePathExists = false;
            this._rootDirectoryBrowseButton.FolderBrowser = true;
            this._rootDirectoryBrowseButton.Location = new System.Drawing.Point(703, 32);
            this._rootDirectoryBrowseButton.Name = "_rootDirectoryBrowseButton";
            this._rootDirectoryBrowseButton.Size = new System.Drawing.Size(49, 21);
            this._rootDirectoryBrowseButton.TabIndex = 1;
            this._rootDirectoryBrowseButton.Text = "...";
            this._rootDirectoryBrowseButton.TextControl = this._rootDirectoryTextBox;
            this._rootDirectoryBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _rootDirectoryTextBox
            // 
            this._rootDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._rootDirectoryTextBox.Location = new System.Drawing.Point(9, 32);
            this._rootDirectoryTextBox.Name = "_rootDirectoryTextBox";
            this._rootDirectoryTextBox.Size = new System.Drawing.Size(688, 20);
            this._rootDirectoryTextBox.TabIndex = 0;
            this._rootDirectoryTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _deleteDataCheckBox
            // 
            this._deleteDataCheckBox.AutoSize = true;
            this._deleteDataCheckBox.Location = new System.Drawing.Point(10, 206);
            this._deleteDataCheckBox.Name = "_deleteDataCheckBox";
            this._deleteDataCheckBox.Size = new System.Drawing.Size(289, 17);
            this._deleteDataCheckBox.TabIndex = 5;
            this._deleteDataCheckBox.Text = "Delete ML data marked for deletion after training models";
            this._deleteDataCheckBox.UseVisualStyleBackColor = true;
            this._deleteDataCheckBox.CheckedChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 155);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(202, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Minimum new records required for training";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Root directory";
            // 
            // _exportButton
            // 
            this._exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._exportButton.Location = new System.Drawing.Point(90, 271);
            this._exportButton.Name = "_exportButton";
            this._exportButton.Size = new System.Drawing.Size(75, 23);
            this._exportButton.TabIndex = 8;
            this._exportButton.Text = "Export...";
            this._exportButton.UseVisualStyleBackColor = true;
            this._exportButton.Click += new System.EventHandler(this.HandleExportButton_Click);
            // 
            // _importButton
            // 
            this._importButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._importButton.Location = new System.Drawing.Point(9, 271);
            this._importButton.Name = "_importButton";
            this._importButton.Size = new System.Drawing.Size(75, 23);
            this._importButton.TabIndex = 7;
            this._importButton.Text = "Import...";
            this._importButton.UseVisualStyleBackColor = true;
            this._importButton.Click += new System.EventHandler(this.HandleImportButton_Click);
            // 
            // _projectNameTextBox
            // 
            this._projectNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._projectNameTextBox.Location = new System.Drawing.Point(9, 124);
            this._projectNameTextBox.Name = "_projectNameTextBox";
            this._projectNameTextBox.Size = new System.Drawing.Size(330, 20);
            this._projectNameTextBox.TabIndex = 3;
            this._projectNameTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(239, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Project name (used as prefix for MLModel names)";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(652, 585);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(60, 23);
            this._okButton.TabIndex = 3;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(718, 585);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(60, 23);
            this._cancelButton.TabIndex = 4;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Click += new System.EventHandler(this.HandleCancelButton_Click);
            // 
            // _runStopButton
            // 
            this._runStopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._runStopButton.Location = new System.Drawing.Point(7, 585);
            this._runStopButton.Name = "_runStopButton";
            this._runStopButton.Size = new System.Drawing.Size(84, 23);
            this._runStopButton.TabIndex = 2;
            this._runStopButton.Text = "Run now";
            this._runStopButton.UseVisualStyleBackColor = true;
            this._runStopButton.Click += new System.EventHandler(this.HandleRunStopButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox4, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 216F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(749, 216);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this._modifyTrainerButton);
            this.groupBox4.Controls.Add(this._duplicateTrainerButton);
            this.groupBox4.Controls.Add(this._downTrainerButton);
            this.groupBox4.Controls.Add(this._upTrainerButton);
            this.groupBox4.Controls.Add(this._modelTrainersDataGridView);
            this.groupBox4.Controls.Add(this._removeTrainerButton);
            this.groupBox4.Controls.Add(this._addTrainerButton);
            this.groupBox4.Location = new System.Drawing.Point(377, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(369, 210);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Model trainers";
            // 
            // _modifyTrainerButton
            // 
            this._modifyTrainerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._modifyTrainerButton.Location = new System.Drawing.Point(276, 55);
            this._modifyTrainerButton.Name = "_modifyTrainerButton";
            this._modifyTrainerButton.Size = new System.Drawing.Size(87, 23);
            this._modifyTrainerButton.TabIndex = 2;
            this._modifyTrainerButton.Text = "Modify";
            this._modifyTrainerButton.UseVisualStyleBackColor = true;
            this._modifyTrainerButton.Click += new System.EventHandler(this.HandleModifyButton_Click);
            // 
            // _duplicateTrainerButton
            // 
            this._duplicateTrainerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._duplicateTrainerButton.Location = new System.Drawing.Point(276, 84);
            this._duplicateTrainerButton.Name = "_duplicateTrainerButton";
            this._duplicateTrainerButton.Size = new System.Drawing.Size(87, 23);
            this._duplicateTrainerButton.TabIndex = 3;
            this._duplicateTrainerButton.Text = "Duplicate";
            this._duplicateTrainerButton.UseVisualStyleBackColor = true;
            this._duplicateTrainerButton.Click += new System.EventHandler(this.HandleDuplicateButton_Click);
            // 
            // _downTrainerButton
            // 
            this._downTrainerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._downTrainerButton.Image = ((System.Drawing.Image)(resources.GetObject("_downTrainerButton.Image")));
            this._downTrainerButton.Location = new System.Drawing.Point(328, 142);
            this._downTrainerButton.Name = "_downTrainerButton";
            this._downTrainerButton.Size = new System.Drawing.Size(35, 35);
            this._downTrainerButton.TabIndex = 6;
            this._downTrainerButton.UseVisualStyleBackColor = true;
            this._downTrainerButton.Click += new System.EventHandler(this.HandleDownButton_Click);
            // 
            // _upTrainerButton
            // 
            this._upTrainerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._upTrainerButton.Image = ((System.Drawing.Image)(resources.GetObject("_upTrainerButton.Image")));
            this._upTrainerButton.Location = new System.Drawing.Point(276, 142);
            this._upTrainerButton.Name = "_upTrainerButton";
            this._upTrainerButton.Size = new System.Drawing.Size(35, 35);
            this._upTrainerButton.TabIndex = 5;
            this._upTrainerButton.UseVisualStyleBackColor = true;
            this._upTrainerButton.Click += new System.EventHandler(this.HandleUpButton_Click);
            // 
            // _modelTrainersDataGridView
            // 
            this._modelTrainersDataGridView.AllowUserToAddRows = false;
            this._modelTrainersDataGridView.AllowUserToDeleteRows = false;
            this._modelTrainersDataGridView.AllowUserToOrderColumns = true;
            this._modelTrainersDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._modelTrainersDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._modelTrainersDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this._modelTrainersDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._modelTrainersDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._modelTrainersDataGridView.Location = new System.Drawing.Point(9, 19);
            this._modelTrainersDataGridView.MultiSelect = false;
            this._modelTrainersDataGridView.Name = "_modelTrainersDataGridView";
            this._modelTrainersDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this._modelTrainersDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._modelTrainersDataGridView.Size = new System.Drawing.Size(261, 185);
            this._modelTrainersDataGridView.TabIndex = 0;
            this._modelTrainersDataGridView.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.HandleModelTrainersDataGridView_CellMouseDoubleClick);
            this._modelTrainersDataGridView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleDataGridView_RowEnter);
            // 
            // _removeTrainerButton
            // 
            this._removeTrainerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._removeTrainerButton.Location = new System.Drawing.Point(276, 113);
            this._removeTrainerButton.Name = "_removeTrainerButton";
            this._removeTrainerButton.Size = new System.Drawing.Size(87, 23);
            this._removeTrainerButton.TabIndex = 4;
            this._removeTrainerButton.Text = "Remove";
            this._removeTrainerButton.UseVisualStyleBackColor = true;
            this._removeTrainerButton.Click += new System.EventHandler(this.HandleRemoveButton_Click);
            // 
            // _addTrainerButton
            // 
            this._addTrainerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addTrainerButton.Location = new System.Drawing.Point(276, 26);
            this._addTrainerButton.Name = "_addTrainerButton";
            this._addTrainerButton.Size = new System.Drawing.Size(87, 23);
            this._addTrainerButton.TabIndex = 1;
            this._addTrainerButton.Text = "Add";
            this._addTrainerButton.UseVisualStyleBackColor = true;
            this._addTrainerButton.Click += new System.EventHandler(this.HandleAddButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this._modifyCollectorButton);
            this.groupBox2.Controls.Add(this._duplicateCollectorButton);
            this.groupBox2.Controls.Add(this._downCollectorButton);
            this.groupBox2.Controls.Add(this._upCollectorButton);
            this.groupBox2.Controls.Add(this._dataCollectorsDataGridView);
            this.groupBox2.Controls.Add(this._removeCollectorButton);
            this.groupBox2.Controls.Add(this._addCollectorButton);
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(368, 210);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Data collectors";
            // 
            // _modifyCollectorButton
            // 
            this._modifyCollectorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._modifyCollectorButton.Location = new System.Drawing.Point(275, 55);
            this._modifyCollectorButton.Name = "_modifyCollectorButton";
            this._modifyCollectorButton.Size = new System.Drawing.Size(87, 23);
            this._modifyCollectorButton.TabIndex = 2;
            this._modifyCollectorButton.Text = "Modify";
            this._modifyCollectorButton.UseVisualStyleBackColor = true;
            this._modifyCollectorButton.Click += new System.EventHandler(this.HandleModifyButton_Click);
            // 
            // _duplicateCollectorButton
            // 
            this._duplicateCollectorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._duplicateCollectorButton.Location = new System.Drawing.Point(275, 84);
            this._duplicateCollectorButton.Name = "_duplicateCollectorButton";
            this._duplicateCollectorButton.Size = new System.Drawing.Size(87, 23);
            this._duplicateCollectorButton.TabIndex = 3;
            this._duplicateCollectorButton.Text = "Duplicate";
            this._duplicateCollectorButton.UseVisualStyleBackColor = true;
            this._duplicateCollectorButton.Click += new System.EventHandler(this.HandleDuplicateButton_Click);
            // 
            // _downCollectorButton
            // 
            this._downCollectorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._downCollectorButton.Image = ((System.Drawing.Image)(resources.GetObject("_downCollectorButton.Image")));
            this._downCollectorButton.Location = new System.Drawing.Point(327, 142);
            this._downCollectorButton.Name = "_downCollectorButton";
            this._downCollectorButton.Size = new System.Drawing.Size(35, 35);
            this._downCollectorButton.TabIndex = 6;
            this._downCollectorButton.UseVisualStyleBackColor = true;
            this._downCollectorButton.Click += new System.EventHandler(this.HandleDownButton_Click);
            // 
            // _upCollectorButton
            // 
            this._upCollectorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._upCollectorButton.Image = ((System.Drawing.Image)(resources.GetObject("_upCollectorButton.Image")));
            this._upCollectorButton.Location = new System.Drawing.Point(275, 142);
            this._upCollectorButton.Name = "_upCollectorButton";
            this._upCollectorButton.Size = new System.Drawing.Size(35, 35);
            this._upCollectorButton.TabIndex = 5;
            this._upCollectorButton.UseVisualStyleBackColor = true;
            this._upCollectorButton.Click += new System.EventHandler(this.HandleUpButton_Click);
            // 
            // _dataCollectorsDataGridView
            // 
            this._dataCollectorsDataGridView.AllowUserToAddRows = false;
            this._dataCollectorsDataGridView.AllowUserToDeleteRows = false;
            this._dataCollectorsDataGridView.AllowUserToOrderColumns = true;
            this._dataCollectorsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataCollectorsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._dataCollectorsDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this._dataCollectorsDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._dataCollectorsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dataCollectorsDataGridView.Location = new System.Drawing.Point(6, 19);
            this._dataCollectorsDataGridView.MultiSelect = false;
            this._dataCollectorsDataGridView.Name = "_dataCollectorsDataGridView";
            this._dataCollectorsDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this._dataCollectorsDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._dataCollectorsDataGridView.Size = new System.Drawing.Size(263, 185);
            this._dataCollectorsDataGridView.TabIndex = 0;
            this._dataCollectorsDataGridView.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.HandleDataCollectorsDataGridView_CellMouseDoubleClick);
            this._dataCollectorsDataGridView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleDataGridView_RowEnter);
            // 
            // _removeCollectorButton
            // 
            this._removeCollectorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._removeCollectorButton.Location = new System.Drawing.Point(275, 113);
            this._removeCollectorButton.Name = "_removeCollectorButton";
            this._removeCollectorButton.Size = new System.Drawing.Size(87, 23);
            this._removeCollectorButton.TabIndex = 4;
            this._removeCollectorButton.Text = "Remove";
            this._removeCollectorButton.UseVisualStyleBackColor = true;
            this._removeCollectorButton.Click += new System.EventHandler(this.HandleRemoveButton_Click);
            // 
            // _addCollectorButton
            // 
            this._addCollectorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addCollectorButton.Location = new System.Drawing.Point(275, 26);
            this._addCollectorButton.Name = "_addCollectorButton";
            this._addCollectorButton.Size = new System.Drawing.Size(87, 23);
            this._addCollectorButton.TabIndex = 1;
            this._addCollectorButton.Text = "Add";
            this._addCollectorButton.UseVisualStyleBackColor = true;
            this._addCollectorButton.Click += new System.EventHandler(this.HandleAddButton_Click);
            // 
            // _servicesAndLogTabControl
            // 
            this._servicesAndLogTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._servicesAndLogTabControl.Controls.Add(this._mlServicesTabPage);
            this._servicesAndLogTabControl.Controls.Add(this._logTabPage);
            this._servicesAndLogTabControl.Location = new System.Drawing.Point(11, 325);
            this._servicesAndLogTabControl.Name = "_servicesAndLogTabControl";
            this._servicesAndLogTabControl.SelectedIndex = 0;
            this._servicesAndLogTabControl.Size = new System.Drawing.Size(767, 254);
            this._servicesAndLogTabControl.TabIndex = 1;
            this._servicesAndLogTabControl.SelectedIndexChanged += new System.EventHandler(this.HandleServicesAndLogTabControl_SelectedIndexChanged);
            // 
            // _mlServicesTabPage
            // 
            this._mlServicesTabPage.Controls.Add(this.tableLayoutPanel1);
            this._mlServicesTabPage.Location = new System.Drawing.Point(4, 22);
            this._mlServicesTabPage.Name = "_mlServicesTabPage";
            this._mlServicesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._mlServicesTabPage.Size = new System.Drawing.Size(759, 228);
            this._mlServicesTabPage.TabIndex = 0;
            this._mlServicesTabPage.Text = "Machine learning services";
            this._mlServicesTabPage.UseVisualStyleBackColor = true;
            // 
            // _logTabPage
            // 
            this._logTabPage.Controls.Add(this._logTextBox);
            this._logTabPage.Location = new System.Drawing.Point(4, 22);
            this._logTabPage.Name = "_logTabPage";
            this._logTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._logTabPage.Size = new System.Drawing.Size(759, 228);
            this._logTabPage.TabIndex = 1;
            this._logTabPage.Text = "Log";
            this._logTabPage.UseVisualStyleBackColor = true;
            // 
            // _logTextBox
            // 
            this._logTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._logTextBox.Location = new System.Drawing.Point(6, 6);
            this._logTextBox.Multiline = true;
            this._logTextBox.Name = "_logTextBox";
            this._logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._logTextBox.Size = new System.Drawing.Size(750, 216);
            this._logTextBox.TabIndex = 0;
            this._logTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // TrainingCoordinatorConfigurationDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(785, 615);
            this.Controls.Add(this._servicesAndLogTabControl);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._globalSettingsGroupBox);
            this.Controls.Add(this._runStopButton);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(740, 550);
            this.Name = "TrainingCoordinatorConfigurationDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Training coordinator";
            this._globalSettingsGroupBox.ResumeLayout(false);
            this._globalSettingsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._maxModelBackupsNumericUpDown)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._minimumRecordsRequiredNumericUpDown)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._modelTrainersDataGridView)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._dataCollectorsDataGridView)).EndInit();
            this._servicesAndLogTabControl.ResumeLayout(false);
            this._mlServicesTabPage.ResumeLayout(false);
            this._logTabPage.ResumeLayout(false);
            this._logTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox _globalSettingsGroupBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.TextBox _projectNameTextBox;
        private System.Windows.Forms.CheckBox _deleteDataCheckBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox _rootDirectoryTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _exportButton;
        private System.Windows.Forms.Button _importButton;
        private System.Windows.Forms.Button _runStopButton;
        private System.Windows.Forms.NumericUpDown _minimumRecordsRequiredNumericUpDown;
        private Utilities.Forms.BrowseButton _rootDirectoryBrowseButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox3;
        private Utilities.Forms.SchedulerControl _schedulerControl;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button _modifyTrainerButton;
        private System.Windows.Forms.Button _duplicateTrainerButton;
        private Utilities.Forms.ExtractDownButton _downTrainerButton;
        private Utilities.Forms.ExtractUpButton _upTrainerButton;
        private System.Windows.Forms.DataGridView _modelTrainersDataGridView;
        private System.Windows.Forms.Button _removeTrainerButton;
        private System.Windows.Forms.Button _addTrainerButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button _modifyCollectorButton;
        private System.Windows.Forms.Button _duplicateCollectorButton;
        private Utilities.Forms.ExtractDownButton _downCollectorButton;
        private Utilities.Forms.ExtractUpButton _upCollectorButton;
        private System.Windows.Forms.DataGridView _dataCollectorsDataGridView;
        private System.Windows.Forms.Button _removeCollectorButton;
        private System.Windows.Forms.Button _addCollectorButton;
        private System.Windows.Forms.TabControl _servicesAndLogTabControl;
        private System.Windows.Forms.TabPage _mlServicesTabPage;
        private System.Windows.Forms.TabPage _logTabPage;
        private System.Windows.Forms.TextBox _descriptionTextBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox _logTextBox;
        private System.Windows.Forms.Button _resetProcessedStatusButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown _maxModelBackupsNumericUpDown;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}