namespace Extract.UtilityApplications.PaginationUtility.Demo_Pagination_FullDEP
{
    partial class Demo_Pagination_FullDEPPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label Name;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Demo_Pagination_FullDEPPanel));
            Extract.DataEntry.HighlightColor highlightColor1 = new Extract.DataEntry.HighlightColor();
            Extract.DataEntry.HighlightColor highlightColor2 = new Extract.DataEntry.HighlightColor();
            this._patientInfoGroupBox = new Extract.DataEntry.DataEntryGroupBox();
            this._patientNameTable = new Extract.DataEntry.DataEntryTable();
            this._patientFirstNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._patientMiddleNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._patientLastNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._patientSuffixColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._genderLabel = new System.Windows.Forms.Label();
            this._patientGender = new Extract.DataEntry.DataEntryComboBox();
            this._patientBirthDate = new Extract.DataEntry.DataEntryTextBox();
            this._birthDateLabel = new System.Windows.Forms.Label();
            this._patientRecordNum = new Extract.DataEntry.DataEntryTextBox();
            this._patientMRLabel = new System.Windows.Forms.Label();
            this._refreshButton = new System.Windows.Forms.Button();
            this._encounterGroupBox = new System.Windows.Forms.GroupBox();
            this._encounterTable = new Extract.DataEntry.DataEntryTable();
            this._encounterCSN = new Extract.DataEntry.DataEntryTableColumn();
            this._encounterPicker = new Extract.DataEntry.LabDE.EncounterPickerTableColumn();
            this._encounterDate = new Extract.DataEntry.DataEntryTableColumn();
            this._encounterTime = new Extract.DataEntry.DataEntryTableColumn();
            this._encounterDepartment = new Extract.DataEntry.DataEntryTableColumn();
            this._encounterType = new Extract.DataEntry.DataEntryTableColumn();
            this._encounterProvider = new Extract.DataEntry.DataEntryTableColumn();
            Name = new System.Windows.Forms.Label();
            this._patientInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._patientNameTable)).BeginInit();
            this._encounterGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._encounterTable)).BeginInit();
            this.SuspendLayout();
            // 
            // Name
            // 
            Name.AutoSize = true;
            Name.Location = new System.Drawing.Point(4, 16);
            Name.Name = "Name";
            Name.Size = new System.Drawing.Size(35, 13);
            Name.TabIndex = 37;
            Name.Text = "Name";
            // 
            // _patientInfoGroupBox
            // 
            this._patientInfoGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._patientInfoGroupBox.AttributeName = "PatientInfo";
            this._patientInfoGroupBox.Controls.Add(this._patientNameTable);
            this._patientInfoGroupBox.Controls.Add(this._genderLabel);
            this._patientInfoGroupBox.Controls.Add(this._patientGender);
            this._patientInfoGroupBox.Controls.Add(this._patientBirthDate);
            this._patientInfoGroupBox.Controls.Add(this._birthDateLabel);
            this._patientInfoGroupBox.Controls.Add(this._patientRecordNum);
            this._patientInfoGroupBox.Controls.Add(this._patientMRLabel);
            this._patientInfoGroupBox.Location = new System.Drawing.Point(3, 3);
            this._patientInfoGroupBox.Name = "_patientInfoGroupBox";
            this._patientInfoGroupBox.ParentDataEntryControl = null;
            this._patientInfoGroupBox.Size = new System.Drawing.Size(699, 126);
            this._patientInfoGroupBox.TabIndex = 5;
            this._patientInfoGroupBox.TabStop = false;
            this._patientInfoGroupBox.Text = "Patient Information";
            // 
            // _patientNameTable
            // 
            this._patientNameTable.AllowDrop = true;
            this._patientNameTable.AllowTabbingByRow = true;
            this._patientNameTable.AllowUserToAddRows = false;
            this._patientNameTable.AllowUserToDeleteRows = false;
            this._patientNameTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._patientNameTable.AttributeName = "Name";
            this._patientNameTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._patientNameTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._patientNameTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this._patientNameTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._patientNameTable.ColumnHintsEnabled = false;
            this._patientNameTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._patientFirstNameColumn,
            this._patientMiddleNameColumn,
            this._patientLastNameColumn,
            this._patientSuffixColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._patientNameTable.DefaultCellStyle = dataGridViewCellStyle2;
            this._patientNameTable.Location = new System.Drawing.Point(7, 20);
            this._patientNameTable.MinimumNumberOfRows = 1;
            this._patientNameTable.Name = "_patientNameTable";
            this._patientNameTable.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientNameTable.RowFormattingRuleFile = "Rules\\Swiping\\name.rsd.etf";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._patientNameTable.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this._patientNameTable.RowSwipingEnabled = true;
            this._patientNameTable.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this._patientNameTable.Size = new System.Drawing.Size(689, 46);
            this._patientNameTable.TabIndex = 0;
            // 
            // _patientFirstNameColumn
            // 
            this._patientFirstNameColumn.AttributeName = "First";
            this._patientFirstNameColumn.FillWeight = 75F;
            this._patientFirstNameColumn.HeaderText = "First Name";
            this._patientFirstNameColumn.Name = "_patientFirstNameColumn";
            this._patientFirstNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._patientFirstNameColumn.ValidationCorrectsCase = false;
            this._patientFirstNameColumn.ValidationErrorMessage = "Patient first name must be specified";
            this._patientFirstNameColumn.ValidationPattern = "\\S";
            this._patientFirstNameColumn.ValidationQuery = resources.GetString("_patientFirstNameColumn.ValidationQuery");
            // 
            // _patientMiddleNameColumn
            // 
            this._patientMiddleNameColumn.AttributeName = "Middle";
            this._patientMiddleNameColumn.FillWeight = 75F;
            this._patientMiddleNameColumn.HeaderText = "Middle Name";
            this._patientMiddleNameColumn.Name = "_patientMiddleNameColumn";
            this._patientMiddleNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._patientMiddleNameColumn.ValidationCorrectsCase = false;
            this._patientMiddleNameColumn.ValidationErrorMessage = "The specified name does not match for this patient\'s record";
            this._patientMiddleNameColumn.ValidationQuery = resources.GetString("_patientMiddleNameColumn.ValidationQuery");
            // 
            // _patientLastNameColumn
            // 
            this._patientLastNameColumn.AttributeName = "Last";
            this._patientLastNameColumn.HeaderText = "Last Name";
            this._patientLastNameColumn.Name = "_patientLastNameColumn";
            this._patientLastNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._patientLastNameColumn.ValidationCorrectsCase = false;
            this._patientLastNameColumn.ValidationErrorMessage = "Patient last name must be specified.";
            this._patientLastNameColumn.ValidationPattern = "\\S";
            this._patientLastNameColumn.ValidationQuery = resources.GetString("_patientLastNameColumn.ValidationQuery");
            // 
            // _patientSuffixColumn
            // 
            this._patientSuffixColumn.AttributeName = "Suffix";
            this._patientSuffixColumn.FillWeight = 50F;
            this._patientSuffixColumn.HeaderText = "Suffix";
            this._patientSuffixColumn.Name = "_patientSuffixColumn";
            this._patientSuffixColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._patientSuffixColumn.ValidationCorrectsCase = false;
            this._patientSuffixColumn.ValidationErrorMessage = "";
            // 
            // _genderLabel
            // 
            this._genderLabel.AutoSize = true;
            this._genderLabel.Location = new System.Drawing.Point(149, 77);
            this._genderLabel.Name = "_genderLabel";
            this._genderLabel.Size = new System.Drawing.Size(42, 13);
            this._genderLabel.TabIndex = 10;
            this._genderLabel.Text = "Gender";
            // 
            // _patientGender
            // 
            this._patientGender.AttributeName = "Gender";
            this._patientGender.Location = new System.Drawing.Point(152, 92);
            this._patientGender.Name = "_patientGender";
            this._patientGender.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientGender.Size = new System.Drawing.Size(54, 21);
            this._patientGender.TabIndex = 2;
            this._patientGender.ValidationErrorMessage = "Invalid value";
            this._patientGender.ValidationQuery = resources.GetString("_patientGender.ValidationQuery");
            // 
            // _patientBirthDate
            // 
            this._patientBirthDate.AttributeName = "DOB";
            this._patientBirthDate.AutoUpdateQuery = "<Expression>LabDEUtils.FormatDate(<Attribute>.</Attribute>)\r\n</Expression>\r\n";
            this._patientBirthDate.Location = new System.Drawing.Point(7, 93);
            this._patientBirthDate.Name = "_patientBirthDate";
            this._patientBirthDate.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientBirthDate.Size = new System.Drawing.Size(125, 20);
            this._patientBirthDate.TabIndex = 1;
            this._patientBirthDate.ValidationErrorMessage = "Date of birth must be a valid date in the format MM/DD/YYYY";
            this._patientBirthDate.ValidationPattern = "^\\d{2}/\\d{2}/\\d{4}$";
            this._patientBirthDate.ValidationQuery = resources.GetString("_patientBirthDate.ValidationQuery");
            // 
            // _birthDateLabel
            // 
            this._birthDateLabel.AutoSize = true;
            this._birthDateLabel.Location = new System.Drawing.Point(5, 77);
            this._birthDateLabel.Name = "_birthDateLabel";
            this._birthDateLabel.Size = new System.Drawing.Size(66, 13);
            this._birthDateLabel.TabIndex = 0;
            this._birthDateLabel.Text = "Date of Birth";
            // 
            // _patientRecordNum
            // 
            this._patientRecordNum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._patientRecordNum.AttributeName = "MR_Number";
            this._patientRecordNum.AutoUpdateQuery = resources.GetString("_patientRecordNum.AutoUpdateQuery");
            this._patientRecordNum.ClearClipboardOnPaste = true;
            this._patientRecordNum.Location = new System.Drawing.Point(224, 93);
            this._patientRecordNum.Name = "_patientRecordNum";
            this._patientRecordNum.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientRecordNum.Size = new System.Drawing.Size(459, 20);
            this._patientRecordNum.TabIndex = 3;
            this._patientRecordNum.ValidationErrorMessage = "Invalid medical record number";
            this._patientRecordNum.ValidationPattern = "^\\d+\\s?$";
            this._patientRecordNum.ValidationQuery = resources.GetString("_patientRecordNum.ValidationQuery");
            // 
            // _patientMRLabel
            // 
            this._patientMRLabel.AutoSize = true;
            this._patientMRLabel.Location = new System.Drawing.Point(221, 77);
            this._patientMRLabel.Name = "_patientMRLabel";
            this._patientMRLabel.Size = new System.Drawing.Size(92, 13);
            this._patientMRLabel.TabIndex = 0;
            this._patientMRLabel.Text = "Medical Record #";
            // 
            // _refreshButton
            // 
            this._refreshButton.Location = new System.Drawing.Point(3, 215);
            this._refreshButton.Name = "_refreshButton";
            this._refreshButton.Size = new System.Drawing.Size(188, 23);
            this._refreshButton.TabIndex = 13;
            this._refreshButton.TabStop = false;
            this._refreshButton.Text = "Refresh database data";
            this._refreshButton.UseVisualStyleBackColor = true;
            this._refreshButton.Click += new System.EventHandler(this.HandleRefreshButton_Click);
            // 
            // _encounterGroupBox
            // 
            this._encounterGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._encounterGroupBox.Controls.Add(this._encounterTable);
            this._encounterGroupBox.Location = new System.Drawing.Point(3, 135);
            this._encounterGroupBox.Name = "_encounterGroupBox";
            this._encounterGroupBox.Size = new System.Drawing.Size(699, 74);
            this._encounterGroupBox.TabIndex = 14;
            this._encounterGroupBox.TabStop = false;
            this._encounterGroupBox.Text = "Encounter Information";
            // 
            // _encounterTable
            // 
            this._encounterTable.AllowUserToAddRows = false;
            this._encounterTable.AllowUserToDeleteRows = false;
            this._encounterTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._encounterTable.AttributeName = "Encounter";
            this._encounterTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this._encounterTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._encounterTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._encounterCSN,
            this._encounterPicker,
            this._encounterDate,
            this._encounterTime,
            this._encounterDepartment,
            this._encounterType,
            this._encounterProvider});
            this._encounterTable.Location = new System.Drawing.Point(8, 20);
            this._encounterTable.MinimumNumberOfRows = 1;
            this._encounterTable.Name = "_encounterTable";
            this._encounterTable.ParentDataEntryControl = null;
            this._encounterTable.RowAutoPopulationEnabled = false;
            this._encounterTable.Size = new System.Drawing.Size(688, 46);
            this._encounterTable.TabIndex = 0;
            // 
            // _encounterCSN
            // 
            this._encounterCSN.AttributeName = "CSN";
            this._encounterCSN.HeaderText = "CSN";
            this._encounterCSN.Name = "_encounterCSN";
            this._encounterCSN.ValidationErrorMessage = "Invalid value";
            // 
            // _encounterPicker
            // 
            this._encounterPicker.AutoSelectionFilter = null;
            this._encounterPicker.AutoSelectionRecord = null;
            this._encounterPicker.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._encounterPicker.ColorQueryConditions = "Red: COUNT(*) = 0\r\nYellow: COUNT(CASE WHEN ([CSN] = \'*\') THEN 1 END) >= COUNT(CAS" +
    "E WHEN ([FileCount] = 0) THEN 1 END)\r\nLime: COUNT(CASE WHEN ([FileCount] = 0) TH" +
    "EN 1 END) > 0";
            this._encounterPicker.HeaderText = "";
            this._encounterPicker.MinimumWidth = 20;
            this._encounterPicker.Name = "_encounterPicker";
            this._encounterPicker.RecordIdAttribute = null;
            this._encounterPicker.RecordIdColumn = "_encounterCSN";
            this._encounterPicker.RecordMatchCriteria = "{/PatientInfo/MR_Number} = [PatientMRN]\r\n{Department} = [Department]";
            this._encounterPicker.RecordQueryColumns = resources.GetString("_encounterPicker.RecordQueryColumns");
            this._encounterPicker.Width = 20;
            // 
            // _encounterDate
            // 
            this._encounterDate.AttributeName = "Date";
            this._encounterDate.HeaderText = "Date";
            this._encounterDate.Name = "_encounterDate";
            this._encounterDate.ValidationErrorMessage = "Invalid value";
            // 
            // _encounterTime
            // 
            this._encounterTime.AttributeName = "Time";
            this._encounterTime.HeaderText = "Time";
            this._encounterTime.Name = "_encounterTime";
            this._encounterTime.ValidationErrorMessage = "Invalid value";
            // 
            // _encounterDepartment
            // 
            this._encounterDepartment.AttributeName = "Department";
            this._encounterDepartment.HeaderText = "Department";
            this._encounterDepartment.Name = "_encounterDepartment";
            this._encounterDepartment.ValidationErrorMessage = "Invalid value";
            // 
            // _encounterType
            // 
            this._encounterType.AttributeName = "Type";
            this._encounterType.HeaderText = "Type";
            this._encounterType.Name = "_encounterType";
            this._encounterType.ValidationErrorMessage = "Invalid value";
            // 
            // _encounterProvider
            // 
            this._encounterProvider.AttributeName = "Provider";
            this._encounterProvider.HeaderText = "Provider";
            this._encounterProvider.Name = "_encounterProvider";
            this._encounterProvider.ValidationErrorMessage = "Invalid value";
            // 
            // Demo_Pagination_FullDEPPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this._encounterGroupBox);
            this.Controls.Add(this._refreshButton);
            this.Controls.Add(this._patientInfoGroupBox);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            highlightColor1.Color = System.Drawing.Color.LightSalmon;
            highlightColor1.MaxOcrConfidence = 89;
            highlightColor2.Color = System.Drawing.Color.LightGreen;
            highlightColor2.MaxOcrConfidence = 100;
            this.HighlightColors = new Extract.DataEntry.HighlightColor[] {
        highlightColor1,
        highlightColor2};
            this.MinimumSize = new System.Drawing.Size(500, 0);
            this.Name = "Demo_Pagination_FullDEPPanel";
            this.Size = new System.Drawing.Size(705, 242);
            this._patientInfoGroupBox.ResumeLayout(false);
            this._patientInfoGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._patientNameTable)).EndInit();
            this._encounterGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._encounterTable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Extract.DataEntry.DataEntryGroupBox _patientInfoGroupBox;
        private Extract.DataEntry.DataEntryTable _patientNameTable;
        private System.Windows.Forms.Label _genderLabel;
        private Extract.DataEntry.DataEntryComboBox _patientGender;
        private Extract.DataEntry.DataEntryTextBox _patientBirthDate;
        private System.Windows.Forms.Label _birthDateLabel;
        private Extract.DataEntry.DataEntryTextBox _patientRecordNum;
        private System.Windows.Forms.Label _patientMRLabel;
        private System.Windows.Forms.Button _refreshButton;
        private Extract.DataEntry.DataEntryTableColumn _patientFirstNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _patientMiddleNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _patientLastNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _patientSuffixColumn;
        private System.Windows.Forms.GroupBox _encounterGroupBox;
        private DataEntry.DataEntryTable _encounterTable;
        private DataEntry.DataEntryTableColumn _encounterCSN;
        private DataEntry.LabDE.EncounterPickerTableColumn _encounterPicker;
        private DataEntry.DataEntryTableColumn _encounterDate;
        private DataEntry.DataEntryTableColumn _encounterTime;
        private DataEntry.DataEntryTableColumn _encounterDepartment;
        private DataEntry.DataEntryTableColumn _encounterType;
        private DataEntry.DataEntryTableColumn _encounterProvider;
    }
}
