namespace Extract.DataEntry.DEP.HealthNetworkLabs
{
    partial class HealthNetworkLabsPanel
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HealthNetworkLabsPanel));
            Extract.DataEntry.HighlightColor highlightColor1 = new Extract.DataEntry.HighlightColor();
            Extract.DataEntry.HighlightColor highlightColor2 = new Extract.DataEntry.HighlightColor();
            this._labNameLabel = new System.Windows.Forms.Label();
            this._laboratoryIdentifier = new Extract.DataEntry.DataEntryTextBox();
            this._labInfoPassThrough = new Extract.DataEntry.DataEntryTextBox();
            this._laboratoryTestTable = new Extract.DataEntry.DataEntryTable();
            this._testName = new Extract.DataEntry.DataEntryTableColumn();
            this._orderCode = new Extract.DataEntry.DataEntryTableColumn();
            this._testID = new Extract.DataEntry.DataEntryTableColumn();
            this._componentDate = new Extract.DataEntry.DataEntryTableColumn();
            this._laboratoryTestTime = new Extract.DataEntry.DataEntryTableColumn();
            this._labIDLabel = new System.Windows.Forms.Label();
            this._labID = new Extract.DataEntry.DataEntryTextBox();
            this._testComponentTable = new Extract.DataEntry.DataEntryTable();
            this._componentName = new Extract.DataEntry.DataEntryTableColumn();
            this._testCode = new Extract.DataEntry.DataEntryTableColumn();
            this._componentValue = new Extract.DataEntry.DataEntryTableColumn();
            this._componentUnits = new Extract.DataEntry.DataEntryTableColumn();
            this._componentRefRange = new Extract.DataEntry.DataEntryTableColumn();
            this._componentFlag = new Extract.DataEntry.DataEntryTableColumn();
            this._componentOriginalName = new Extract.DataEntry.DataEntryTableColumn();
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
            this._physicianInfoGroupBox = new Extract.DataEntry.DataEntryGroupBox();
            this._orderingPhysicianTable = new Extract.DataEntry.DataEntryTable();
            this._orderingPhysicianLastNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._orderingPhysicianFirstNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._orderingPhysicianMiddleName = new Extract.DataEntry.DataEntryTableColumn();
            this._orderingPhysicianCodeColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._testDetailsGroupBox = new System.Windows.Forms.GroupBox();
            this._orderNumberLabel = new System.Windows.Forms.Label();
            this._orderNumber = new Extract.DataEntry.DataEntryTextBox();
            this._testCommentLabel = new System.Windows.Forms.Label();
            this._testComment = new Extract.DataEntry.DataEntryTextBox();
            this._componentCommentLabel = new System.Windows.Forms.Label();
            this._componentComment = new Extract.DataEntry.DataEntryTextBox();
            this._testsGroupBox = new System.Windows.Forms.GroupBox();
            this._resultStatus = new Extract.DataEntry.DataEntryComboBox();
            this._resultStatusLabel = new System.Windows.Forms.Label();
            this._operatorCommentLabel = new System.Windows.Forms.Label();
            this._operatorComments = new Extract.DataEntry.DataEntryTextBox();
            this._messageSequenceNumberFile = new Extract.DataEntry.DataEntryTextBox();
            ((System.ComponentModel.ISupportInitialize)(this._laboratoryTestTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._testComponentTable)).BeginInit();
            this._patientInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._patientNameTable)).BeginInit();
            this._physicianInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._orderingPhysicianTable)).BeginInit();
            this._testDetailsGroupBox.SuspendLayout();
            this._testsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _labNameLabel
            // 
            this._labNameLabel.AutoSize = true;
            this._labNameLabel.Location = new System.Drawing.Point(6, 54);
            this._labNameLabel.Name = "_labNameLabel";
            this._labNameLabel.Size = new System.Drawing.Size(56, 13);
            this._labNameLabel.TabIndex = 3;
            this._labNameLabel.Text = "Lab Name";
            // 
            // _laboratoryIdentifier
            // 
            this._laboratoryIdentifier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._laboratoryIdentifier.AttributeName = "LabIdentifier";
            this._laboratoryIdentifier.AutoUpdateQuery = resources.GetString("_laboratoryIdentifier.AutoUpdateQuery");
            this._laboratoryIdentifier.Location = new System.Drawing.Point(7, 70);
            this._laboratoryIdentifier.Name = "_laboratoryIdentifier";
            this._laboratoryIdentifier.ParentDataEntryControl = this._labInfoPassThrough;
            this._laboratoryIdentifier.Size = new System.Drawing.Size(428, 20);
            this._laboratoryIdentifier.SupportsSwiping = false;
            this._laboratoryIdentifier.TabIndex = 1;
            this._laboratoryIdentifier.ValidationErrorMessage = "";
            this._laboratoryIdentifier.ValidationQuery = "[BLANK]\r\n<SQL>SELECT LabName FROM LabAddresses ORDER BY LabName</SQL>";
            // 
            // _labInfoPassThrough
            // 
            this._labInfoPassThrough.AttributeName = "LabInfo";
            this._labInfoPassThrough.Location = new System.Drawing.Point(208, 54);
            this._labInfoPassThrough.Name = "_labInfoPassThrough";
            this._labInfoPassThrough.ParentDataEntryControl = this._laboratoryTestTable;
            this._labInfoPassThrough.Size = new System.Drawing.Size(16, 20);
            this._labInfoPassThrough.SupportsSwiping = false;
            this._labInfoPassThrough.TabIndex = 5;
            this._labInfoPassThrough.ValidationErrorMessage = "";
            this._labInfoPassThrough.Visible = false;
            // 
            // _laboratoryTestTable
            // 
            this._laboratoryTestTable.AllowDrop = true;
            this._laboratoryTestTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._laboratoryTestTable.AttributeName = "Test";
            this._laboratoryTestTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._laboratoryTestTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this._laboratoryTestTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._laboratoryTestTable.ColumnHintsEnabled = false;
            this._laboratoryTestTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._testName,
            this._orderCode,
            this._testID,
            this._componentDate,
            this._laboratoryTestTime});
            this._laboratoryTestTable.Location = new System.Drawing.Point(6, 19);
            this._laboratoryTestTable.Name = "_laboratoryTestTable";
            this._laboratoryTestTable.ParentDataEntryControl = null;
            this._laboratoryTestTable.RowFormattingRuleFile = "Rules\\Swiping\\TestRow.rsd.etf";
            this._laboratoryTestTable.RowSwipingEnabled = true;
            this._laboratoryTestTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._laboratoryTestTable.Size = new System.Drawing.Size(581, 178);
            this._laboratoryTestTable.TabIndex = 1;
            // 
            // _testName
            // 
            this._testName.AttributeName = "Name";
            this._testName.AutoUpdateQuery = resources.GetString("_testName.AutoUpdateQuery");
            this._testName.HeaderText = "Order Name";
            this._testName.Name = "_testName";
            this._testName.ValidationErrorMessage = "Order name is not recognized.";
            this._testName.ValidationQuery = resources.GetString("_testName.ValidationQuery");
            // 
            // _orderCode
            // 
            this._orderCode.AttributeName = "OrderCode";
            this._orderCode.AutoUpdateQuery = "<SQL>SELECT Code FROM LabOrder WHERE Name = SUBSTRING(<Attribute>../Name</Attribu" +
    "te>,1,50)</SQL>";
            this._orderCode.FillWeight = 1F;
            this._orderCode.HeaderText = "Order Code";
            this._orderCode.Name = "_orderCode";
            this._orderCode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._orderCode.TabStopMode = Extract.DataEntry.TabStopMode.Never;
            this._orderCode.ValidationErrorMessage = "Bad value";
            this._orderCode.Visible = false;
            // 
            // _testID
            // 
            this._testID.AttributeName = "EpicCode";
            this._testID.AutoUpdateQuery = "<SQL>SELECT EpicCode FROM LabOrder WHERE Name = SUBSTRING(<Attribute>../Name</Att" +
    "ribute>,1,50)</SQL>";
            this._testID.FillWeight = 45F;
            this._testID.HeaderText = "Code";
            this._testID.Name = "_testID";
            this._testID.TabStopMode = Extract.DataEntry.TabStopMode.OnlyWhenInvalid;
            this._testID.ValidationErrorMessage = "Order code is not recognized.";
            this._testID.ValidationQuery = "<SQL>SELECT EpicCode FROM LabOrder WHERE EpicCode IS NOT NULL ORDER BY EpicCode</" +
    "SQL>";
            // 
            // _componentDate
            // 
            this._componentDate.AttributeName = "CollectionDate";
            this._componentDate.FillWeight = 60F;
            this._componentDate.FormattingRuleFile = "Rules\\Swiping\\CollectionDate.rsd.etf";
            this._componentDate.HeaderText = "Collection Date";
            this._componentDate.Name = "_componentDate";
            this._componentDate.ValidationErrorMessage = "Collection date must be a valid date formatted MM/DD/YYYY";
            this._componentDate.ValidationPattern = "^((0?[1-9])|(1[0-2]))/((0?[1-9])|(1[0-9])|(2[0-9])|(3[01]))/(19|20)\\d{2}$";
            // 
            // _laboratoryTestTime
            // 
            this._laboratoryTestTime.AttributeName = "CollectionTime";
            this._laboratoryTestTime.FillWeight = 60F;
            this._laboratoryTestTime.FormattingRuleFile = "Rules\\Swiping\\CollectionTime.rsd.etf";
            this._laboratoryTestTime.HeaderText = "Collection Time";
            this._laboratoryTestTime.Name = "_laboratoryTestTime";
            this._laboratoryTestTime.ValidationErrorMessage = "Collection time must be a valid time formatted HH:MM";
            this._laboratoryTestTime.ValidationPattern = "^((0?[0-9])|(1[0-9])|(2[0-3])):[0-5][0-9]$";
            // 
            // _labIDLabel
            // 
            this._labIDLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._labIDLabel.AutoSize = true;
            this._labIDLabel.Location = new System.Drawing.Point(454, 54);
            this._labIDLabel.Name = "_labIDLabel";
            this._labIDLabel.Size = new System.Drawing.Size(53, 13);
            this._labIDLabel.TabIndex = 0;
            this._labIDLabel.Text = "Lab Code";
            // 
            // _labID
            // 
            this._labID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._labID.AttributeName = "LabCode";
            this._labID.AutoUpdateQuery = "<SQL>SELECT LabCode FROM LabAddresses WHERE LabName = SUBSTRING(<Attribute>../Lab" +
    "Identifier</Attribute>,1,50)</SQL>";
            this._labID.Location = new System.Drawing.Point(457, 70);
            this._labID.Name = "_labID";
            this._labID.ParentDataEntryControl = this._labInfoPassThrough;
            this._labID.Size = new System.Drawing.Size(117, 20);
            this._labID.SupportsSwiping = false;
            this._labID.TabIndex = 2;
            this._labID.TabStopMode = Extract.DataEntry.TabStopMode.OnlyWhenInvalid;
            this._labID.ValidationErrorMessage = "Missing or unknown laboratory ID";
            this._labID.ValidationQuery = "<SQL>SELECT LabCode FROM LabAddresses WHERE LabCode IS NOT NULL AND LEN(LabCode) " +
    "> 0 ORDER BY LabCode</SQL>";
            // 
            // _testComponentTable
            // 
            this._testComponentTable.AllowDrop = true;
            this._testComponentTable.AllowSpatialRowSorting = true;
            this._testComponentTable.AllowTabbingByRow = true;
            this._testComponentTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._testComponentTable.AttributeName = "Component";
            this._testComponentTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._testComponentTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this._testComponentTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._testComponentTable.ColumnHintsEnabled = false;
            this._testComponentTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._componentName,
            this._testCode,
            this._componentValue,
            this._componentUnits,
            this._componentRefRange,
            this._componentFlag,
            this._componentOriginalName});
            this._testComponentTable.Location = new System.Drawing.Point(6, 163);
            this._testComponentTable.Name = "_testComponentTable";
            this._testComponentTable.ParentDataEntryControl = this._laboratoryTestTable;
            this._testComponentTable.RowFormattingRuleFile = "Rules\\Swiping\\ComponentRow.rsd.etf";
            this._testComponentTable.RowSwipingEnabled = true;
            this._testComponentTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._testComponentTable.Size = new System.Drawing.Size(581, 288);
            this._testComponentTable.TabIndex = 7;
            // 
            // _componentName
            // 
            this._componentName.AttributeName = ".";
            this._componentName.AutoUpdateQuery = resources.GetString("_componentName.AutoUpdateQuery");
            this._componentName.FillWeight = 80F;
            this._componentName.HeaderText = "Test Name";
            this._componentName.Name = "_componentName";
            this._componentName.SmartHintsEnabled = true;
            this._componentName.ValidationErrorMessage = "Test name is invalid for the current order";
            this._componentName.ValidationQuery = resources.GetString("_componentName.ValidationQuery");
            // 
            // _testCode
            // 
            this._testCode.AttributeName = "TestCode";
            this._testCode.AutoUpdateQuery = resources.GetString("_testCode.AutoUpdateQuery");
            this._testCode.FillWeight = 40F;
            this._testCode.HeaderText = "Code";
            this._testCode.Name = "_testCode";
            this._testCode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._testCode.TabStopMode = Extract.DataEntry.TabStopMode.OnlyWhenInvalid;
            this._testCode.ValidationErrorMessage = "Test code is invalid for the current order.";
            this._testCode.ValidationQuery = resources.GetString("_testCode.ValidationQuery");
            // 
            // _componentValue
            // 
            this._componentValue.AttributeName = "Value";
            this._componentValue.FillWeight = 40F;
            this._componentValue.HeaderText = "Value";
            this._componentValue.Name = "_componentValue";
            this._componentValue.SmartHintsEnabled = true;
            this._componentValue.ValidationErrorMessage = "Test value must be specified.";
            this._componentValue.ValidationPattern = "\\S";
            // 
            // _componentUnits
            // 
            this._componentUnits.AttributeName = "Units";
            this._componentUnits.FillWeight = 40F;
            this._componentUnits.HeaderText = "Units";
            this._componentUnits.Name = "_componentUnits";
            this._componentUnits.SmartHintsEnabled = true;
            this._componentUnits.ValidationCorrectsCase = false;
            this._componentUnits.ValidationErrorMessage = "Unrecognized unit designation.";
            this._componentUnits.ValidationQuery = "[BLANK]\r\n<SQL>SELECT * FROM Unit ORDER BY Unit</SQL>";
            // 
            // _componentRefRange
            // 
            this._componentRefRange.AttributeName = "Range";
            this._componentRefRange.FillWeight = 65F;
            this._componentRefRange.HeaderText = "Ref. Range";
            this._componentRefRange.Name = "_componentRefRange";
            this._componentRefRange.SmartHintsEnabled = true;
            this._componentRefRange.ValidationErrorMessage = "Value is not allowed.";
            // 
            // _componentFlag
            // 
            this._componentFlag.AttributeName = "Flag";
            this._componentFlag.FillWeight = 35F;
            this._componentFlag.HeaderText = "Flag";
            this._componentFlag.Name = "_componentFlag";
            this._componentFlag.SmartHintsEnabled = true;
            this._componentFlag.UseComboBoxCells = true;
            this._componentFlag.ValidationErrorMessage = "Unrecognized Flag";
            this._componentFlag.ValidationQuery = "[BLANK]\r\n<SQL>SELECT * FROM Flag ORDER BY Flag</SQL>";
            // 
            // _componentOriginalName
            // 
            this._componentOriginalName.AttributeName = "OriginalName";
            this._componentOriginalName.AutoUpdateQuery = "<Query Default=\'1\'><Attribute>..</Attribute></Query>";
            this._componentOriginalName.HeaderText = "Test Name On Document";
            this._componentOriginalName.Name = "_componentOriginalName";
            this._componentOriginalName.ValidationErrorMessage = "Missing AKA in the Order Mapper Database";
            this._componentOriginalName.ValidationQuery = "";
            this._componentOriginalName.Visible = false;
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
            this._patientInfoGroupBox.Location = new System.Drawing.Point(0, 79);
            this._patientInfoGroupBox.Name = "_patientInfoGroupBox";
            this._patientInfoGroupBox.ParentDataEntryControl = null;
            this._patientInfoGroupBox.Size = new System.Drawing.Size(593, 111);
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
            this._patientNameTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._patientNameTable.ColumnHintsEnabled = false;
            this._patientNameTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._patientFirstNameColumn,
            this._patientMiddleNameColumn,
            this._patientLastNameColumn,
            this._patientSuffixColumn});
            this._patientNameTable.Location = new System.Drawing.Point(5, 19);
            this._patientNameTable.MinimumNumberOfRows = 1;
            this._patientNameTable.Name = "_patientNameTable";
            this._patientNameTable.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientNameTable.RowFormattingRuleFile = "Rules\\Swiping\\name.rsd.etf";
            this._patientNameTable.RowSwipingEnabled = true;
            this._patientNameTable.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this._patientNameTable.Size = new System.Drawing.Size(564, 46);
            this._patientNameTable.TabIndex = 1;
            // 
            // _patientFirstNameColumn
            // 
            this._patientFirstNameColumn.AttributeName = "First";
            this._patientFirstNameColumn.FillWeight = 75F;
            this._patientFirstNameColumn.HeaderText = "First Name";
            this._patientFirstNameColumn.Name = "_patientFirstNameColumn";
            this._patientFirstNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._patientFirstNameColumn.ValidationErrorMessage = "Patient first name must be specified";
            this._patientFirstNameColumn.ValidationPattern = "\\S";
            // 
            // _patientMiddleNameColumn
            // 
            this._patientMiddleNameColumn.AttributeName = "Middle";
            this._patientMiddleNameColumn.FillWeight = 75F;
            this._patientMiddleNameColumn.HeaderText = "Middle Name";
            this._patientMiddleNameColumn.Name = "_patientMiddleNameColumn";
            this._patientMiddleNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._patientMiddleNameColumn.ValidationErrorMessage = "Bad value";
            // 
            // _patientLastNameColumn
            // 
            this._patientLastNameColumn.AttributeName = "Last";
            this._patientLastNameColumn.HeaderText = "Last Name";
            this._patientLastNameColumn.Name = "_patientLastNameColumn";
            this._patientLastNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._patientLastNameColumn.ValidationErrorMessage = "Patient last name must be specified.";
            this._patientLastNameColumn.ValidationPattern = "\\S";
            // 
            // _patientSuffixColumn
            // 
            this._patientSuffixColumn.AttributeName = "Suffix";
            this._patientSuffixColumn.FillWeight = 50F;
            this._patientSuffixColumn.HeaderText = "Suffix";
            this._patientSuffixColumn.Name = "_patientSuffixColumn";
            this._patientSuffixColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._patientSuffixColumn.ValidationErrorMessage = "";
            // 
            // _genderLabel
            // 
            this._genderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._genderLabel.AutoSize = true;
            this._genderLabel.Location = new System.Drawing.Point(272, 68);
            this._genderLabel.Name = "_genderLabel";
            this._genderLabel.Size = new System.Drawing.Size(42, 13);
            this._genderLabel.TabIndex = 10;
            this._genderLabel.Text = "Gender";
            // 
            // _patientGender
            // 
            this._patientGender.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._patientGender.AttributeName = "Gender";
            this._patientGender.Location = new System.Drawing.Point(275, 84);
            this._patientGender.Name = "_patientGender";
            this._patientGender.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientGender.Size = new System.Drawing.Size(58, 21);
            this._patientGender.TabIndex = 3;
            this._patientGender.ValidationErrorMessage = "Specify \"M\" for male, \"F\" for female or \"U\" for unknown";
            this._patientGender.ValidationQuery = "<SQL>SELECT * FROM Gender</SQL>";
            // 
            // _patientBirthDate
            // 
            this._patientBirthDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._patientBirthDate.AttributeName = "DOB";
            this._patientBirthDate.Location = new System.Drawing.Point(6, 84);
            this._patientBirthDate.Name = "_patientBirthDate";
            this._patientBirthDate.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientBirthDate.Size = new System.Drawing.Size(249, 20);
            this._patientBirthDate.TabIndex = 2;
            this._patientBirthDate.ValidationErrorMessage = "Date of birth must be a valid date in the format MM/DD/YYYY";
            this._patientBirthDate.ValidationPattern = "(^$)|(^((0?[1-9])|(1[0-2]))/((0?[1-9])|(1[0-9])|(2[0-9])|(3[01]))/(18|19|20)\\d{2}" +
    "$)";
            // 
            // _birthDateLabel
            // 
            this._birthDateLabel.AutoSize = true;
            this._birthDateLabel.Location = new System.Drawing.Point(4, 68);
            this._birthDateLabel.Name = "_birthDateLabel";
            this._birthDateLabel.Size = new System.Drawing.Size(66, 13);
            this._birthDateLabel.TabIndex = 0;
            this._birthDateLabel.Text = "Date of Birth";
            // 
            // _patientRecordNum
            // 
            this._patientRecordNum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._patientRecordNum.AttributeName = "MR_Number";
            this._patientRecordNum.ClearClipboardOnPaste = true;
            this._patientRecordNum.Location = new System.Drawing.Point(353, 84);
            this._patientRecordNum.Name = "_patientRecordNum";
            this._patientRecordNum.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientRecordNum.Size = new System.Drawing.Size(218, 20);
            this._patientRecordNum.TabIndex = 4;
            this._patientRecordNum.ValidationErrorMessage = "Invalid medical record number";
            this._patientRecordNum.ValidationPattern = "(?i)^[MZ]?\\d+\\s?$";
            // 
            // _patientMRLabel
            // 
            this._patientMRLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._patientMRLabel.AutoSize = true;
            this._patientMRLabel.Location = new System.Drawing.Point(351, 68);
            this._patientMRLabel.Name = "_patientMRLabel";
            this._patientMRLabel.Size = new System.Drawing.Size(92, 13);
            this._patientMRLabel.TabIndex = 0;
            this._patientMRLabel.Text = "Medical Record #";
            // 
            // _physicianInfoGroupBox
            // 
            this._physicianInfoGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._physicianInfoGroupBox.AttributeName = "PhysicianInfo";
            this._physicianInfoGroupBox.Controls.Add(this._orderingPhysicianTable);
            this._physicianInfoGroupBox.Location = new System.Drawing.Point(0, 196);
            this._physicianInfoGroupBox.Name = "_physicianInfoGroupBox";
            this._physicianInfoGroupBox.ParentDataEntryControl = null;
            this._physicianInfoGroupBox.Size = new System.Drawing.Size(593, 70);
            this._physicianInfoGroupBox.TabIndex = 6;
            this._physicianInfoGroupBox.TabStop = false;
            this._physicianInfoGroupBox.Text = "Ordering Physician";
            // 
            // _orderingPhysicianTable
            // 
            this._orderingPhysicianTable.AllowDrop = true;
            this._orderingPhysicianTable.AllowTabbingByRow = true;
            this._orderingPhysicianTable.AllowUserToAddRows = false;
            this._orderingPhysicianTable.AllowUserToDeleteRows = false;
            this._orderingPhysicianTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._orderingPhysicianTable.AttributeName = "OrderingPhysicianName";
            this._orderingPhysicianTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._orderingPhysicianTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this._orderingPhysicianTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._orderingPhysicianTable.ColumnHintsEnabled = false;
            this._orderingPhysicianTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._orderingPhysicianLastNameColumn,
            this._orderingPhysicianFirstNameColumn,
            this._orderingPhysicianMiddleName,
            this._orderingPhysicianCodeColumn});
            this._orderingPhysicianTable.CompatibleAttributeNames.Add("OtherPhysicianName");
            this._orderingPhysicianTable.Location = new System.Drawing.Point(6, 19);
            this._orderingPhysicianTable.MinimumNumberOfRows = 1;
            this._orderingPhysicianTable.Name = "_orderingPhysicianTable";
            this._orderingPhysicianTable.ParentDataEntryControl = this._physicianInfoGroupBox;
            this._orderingPhysicianTable.RowFormattingRuleFile = "Rules\\Swiping\\name.rsd.etf";
            this._orderingPhysicianTable.RowSwipingEnabled = true;
            this._orderingPhysicianTable.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this._orderingPhysicianTable.Size = new System.Drawing.Size(581, 45);
            this._orderingPhysicianTable.TabIndex = 1;
            // 
            // _orderingPhysicianLastNameColumn
            // 
            this._orderingPhysicianLastNameColumn.AttributeName = "Last";
            this._orderingPhysicianLastNameColumn.AutoUpdateQuery = "<SQL>SELECT LastName FROM Physician WHERE Code = SUBSTRING(<Attribute>../Code</At" +
    "tribute>,1,8)</SQL>";
            this._orderingPhysicianLastNameColumn.HeaderText = "Last Name";
            this._orderingPhysicianLastNameColumn.Name = "_orderingPhysicianLastNameColumn";
            this._orderingPhysicianLastNameColumn.ValidationErrorMessage = "";
            this._orderingPhysicianLastNameColumn.ValidationQuery = "<SQL>SELECT LastName FROM Physician ORDER BY LastName</SQL>";
            // 
            // _orderingPhysicianFirstNameColumn
            // 
            this._orderingPhysicianFirstNameColumn.AttributeName = "First";
            this._orderingPhysicianFirstNameColumn.AutoUpdateQuery = "<SQL>SELECT FirstName FROM Physician WHERE Code = SUBSTRING(<Attribute>../Code</A" +
    "ttribute>,1,8)</SQL>";
            this._orderingPhysicianFirstNameColumn.FillWeight = 75F;
            this._orderingPhysicianFirstNameColumn.HeaderText = "First Name";
            this._orderingPhysicianFirstNameColumn.Name = "_orderingPhysicianFirstNameColumn";
            this._orderingPhysicianFirstNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._orderingPhysicianFirstNameColumn.ValidationErrorMessage = "";
            this._orderingPhysicianFirstNameColumn.ValidationQuery = "<SQL>SELECT FirstName FROM Physician WHERE LastName LIKE SUBSTRING(<Attribute>../" +
    "Last</Attribute>,1,50) ORDER BY FirstName</SQL>";
            // 
            // _orderingPhysicianMiddleName
            // 
            this._orderingPhysicianMiddleName.AttributeName = "Middle";
            this._orderingPhysicianMiddleName.AutoUpdateQuery = "<SQL>SELECT MiddleName FROM Physician WHERE Code = SUBSTRING(<Attribute>../Code</" +
    "Attribute>,1,8)</SQL>";
            this._orderingPhysicianMiddleName.FillWeight = 50F;
            this._orderingPhysicianMiddleName.HeaderText = "Middle Name";
            this._orderingPhysicianMiddleName.Name = "_orderingPhysicianMiddleName";
            this._orderingPhysicianMiddleName.ValidationErrorMessage = "";
            this._orderingPhysicianMiddleName.ValidationQuery = "<SQL>SELECT MiddleName FROM Physician WHERE LastName LIKE SUBSTRING(<Attribute>.." +
    "/Last</Attribute>,1,50) AND FirstName LIKE SUBSTRING(<Attribute>../First</Attrib" +
    "ute>,1,30) ORDER BY MiddleName</SQL>";
            // 
            // _orderingPhysicianCodeColumn
            // 
            this._orderingPhysicianCodeColumn.AttributeName = "Code";
            this._orderingPhysicianCodeColumn.AutoUpdateQuery = resources.GetString("_orderingPhysicianCodeColumn.AutoUpdateQuery");
            this._orderingPhysicianCodeColumn.FillWeight = 30F;
            this._orderingPhysicianCodeColumn.HeaderText = "Code";
            this._orderingPhysicianCodeColumn.Name = "_orderingPhysicianCodeColumn";
            this._orderingPhysicianCodeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._orderingPhysicianCodeColumn.TabStopMode = Extract.DataEntry.TabStopMode.OnlyWhenInvalid;
            this._orderingPhysicianCodeColumn.ValidationErrorMessage = "Physician code is missing or does not correspond with the specified name.";
            this._orderingPhysicianCodeColumn.ValidationQuery = resources.GetString("_orderingPhysicianCodeColumn.ValidationQuery");
            // 
            // _testDetailsGroupBox
            // 
            this._testDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._testDetailsGroupBox.Controls.Add(this._labInfoPassThrough);
            this._testDetailsGroupBox.Controls.Add(this._labNameLabel);
            this._testDetailsGroupBox.Controls.Add(this._orderNumberLabel);
            this._testDetailsGroupBox.Controls.Add(this._orderNumber);
            this._testDetailsGroupBox.Controls.Add(this._laboratoryIdentifier);
            this._testDetailsGroupBox.Controls.Add(this._labIDLabel);
            this._testDetailsGroupBox.Controls.Add(this._testCommentLabel);
            this._testDetailsGroupBox.Controls.Add(this._testComment);
            this._testDetailsGroupBox.Controls.Add(this._labID);
            this._testDetailsGroupBox.Controls.Add(this._testComponentTable);
            this._testDetailsGroupBox.Controls.Add(this._componentCommentLabel);
            this._testDetailsGroupBox.Controls.Add(this._componentComment);
            this._testDetailsGroupBox.Location = new System.Drawing.Point(0, 481);
            this._testDetailsGroupBox.Name = "_testDetailsGroupBox";
            this._testDetailsGroupBox.Size = new System.Drawing.Size(593, 540);
            this._testDetailsGroupBox.TabIndex = 8;
            this._testDetailsGroupBox.TabStop = false;
            this._testDetailsGroupBox.Text = "Selected Order Details";
            // 
            // _orderNumberLabel
            // 
            this._orderNumberLabel.AutoSize = true;
            this._orderNumberLabel.Location = new System.Drawing.Point(4, 16);
            this._orderNumberLabel.Name = "_orderNumberLabel";
            this._orderNumberLabel.Size = new System.Drawing.Size(96, 13);
            this._orderNumberLabel.TabIndex = 9;
            this._orderNumberLabel.Text = "Accession Number";
            // 
            // _orderNumber
            // 
            this._orderNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._orderNumber.AttributeName = "OrderNumber";
            this._orderNumber.ClearClipboardOnPaste = true;
            this._orderNumber.FormattingRuleFile = "Rules\\Swiping\\ResultDate.rsd.etf";
            this._orderNumber.Location = new System.Drawing.Point(7, 32);
            this._orderNumber.Name = "_orderNumber";
            this._orderNumber.ParentDataEntryControl = this._laboratoryTestTable;
            this._orderNumber.Size = new System.Drawing.Size(197, 20);
            this._orderNumber.SupportsSwiping = false;
            this._orderNumber.TabIndex = 2;
            this._orderNumber.ValidationErrorMessage = "Order Number must be specified";
            this._orderNumber.ValidationPattern = "^\\d+\\s?$";
            // 
            // _testCommentLabel
            // 
            this._testCommentLabel.AutoSize = true;
            this._testCommentLabel.Location = new System.Drawing.Point(6, 94);
            this._testCommentLabel.Name = "_testCommentLabel";
            this._testCommentLabel.Size = new System.Drawing.Size(51, 13);
            this._testCommentLabel.TabIndex = 7;
            this._testCommentLabel.Text = "Comment";
            // 
            // _testComment
            // 
            this._testComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._testComment.AttributeName = "Comment";
            this._testComment.AutoUpdateQuery = "<Query Default=\'1\'>Verified by $FullUserName() on $Now(%m/%d/%Y).</Query>";
            this._testComment.Location = new System.Drawing.Point(6, 110);
            this._testComment.Multiline = true;
            this._testComment.Name = "_testComment";
            this._testComment.ParentDataEntryControl = this._laboratoryTestTable;
            this._testComment.RemoveNewLineChars = false;
            this._testComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._testComment.Size = new System.Drawing.Size(569, 47);
            this._testComment.TabIndex = 6;
            this._testComment.TabStopMode = Extract.DataEntry.TabStopMode.OnlyWhenPopulatedOrInvalid;
            this._testComment.ValidationErrorMessage = "";
            // 
            // _componentCommentLabel
            // 
            this._componentCommentLabel.AutoSize = true;
            this._componentCommentLabel.Location = new System.Drawing.Point(6, 457);
            this._componentCommentLabel.Name = "_componentCommentLabel";
            this._componentCommentLabel.Size = new System.Drawing.Size(141, 13);
            this._componentCommentLabel.TabIndex = 8;
            this._componentCommentLabel.Text = "Result Component Comment";
            // 
            // _componentComment
            // 
            this._componentComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._componentComment.AttributeName = "Comment";
            this._componentComment.Location = new System.Drawing.Point(6, 473);
            this._componentComment.Multiline = true;
            this._componentComment.Name = "_componentComment";
            this._componentComment.ParentDataEntryControl = this._testComponentTable;
            this._componentComment.RemoveNewLineChars = false;
            this._componentComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._componentComment.Size = new System.Drawing.Size(581, 60);
            this._componentComment.TabIndex = 9;
            this._componentComment.TabStopMode = Extract.DataEntry.TabStopMode.OnlyWhenPopulatedOrInvalid;
            this._componentComment.ValidationErrorMessage = "";
            // 
            // _testsGroupBox
            // 
            this._testsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._testsGroupBox.Controls.Add(this._laboratoryTestTable);
            this._testsGroupBox.Location = new System.Drawing.Point(0, 272);
            this._testsGroupBox.Name = "_testsGroupBox";
            this._testsGroupBox.Size = new System.Drawing.Size(593, 203);
            this._testsGroupBox.TabIndex = 7;
            this._testsGroupBox.TabStop = false;
            this._testsGroupBox.Text = "Orders";
            // 
            // _resultStatus
            // 
            this._resultStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._resultStatus.AttributeName = "ResultStatus";
            this._resultStatus.DisplayMember = "Original";
            this._resultStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._resultStatus.Items.AddRange(new object[] {
            "Corrected",
            "Final",
            "Preliminary",
            "Reviewed"});
            this._resultStatus.Location = new System.Drawing.Point(387, 27);
            this._resultStatus.Name = "_resultStatus";
            this._resultStatus.Size = new System.Drawing.Size(183, 21);
            this._resultStatus.SupportsSwiping = false;
            this._resultStatus.TabIndex = 2;
            this._resultStatus.TabStopMode = Extract.DataEntry.TabStopMode.Never;
            this._resultStatus.ValidationErrorMessage = "";
            this._resultStatus.ValueMember = "Original";
            // 
            // _resultStatusLabel
            // 
            this._resultStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._resultStatusLabel.AutoSize = true;
            this._resultStatusLabel.Location = new System.Drawing.Point(384, 11);
            this._resultStatusLabel.Name = "_resultStatusLabel";
            this._resultStatusLabel.Size = new System.Drawing.Size(70, 13);
            this._resultStatusLabel.TabIndex = 0;
            this._resultStatusLabel.Text = "Result Status";
            // 
            // _operatorCommentLabel
            // 
            this._operatorCommentLabel.AutoSize = true;
            this._operatorCommentLabel.Location = new System.Drawing.Point(6, 11);
            this._operatorCommentLabel.Name = "_operatorCommentLabel";
            this._operatorCommentLabel.Size = new System.Drawing.Size(146, 13);
            this._operatorCommentLabel.TabIndex = 6;
            this._operatorCommentLabel.Text = "Comments (for operator\'s use)";
            // 
            // _operatorComments
            // 
            this._operatorComments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._operatorComments.AttributeName = "OperatorComments";
            this._operatorComments.Enabled = false;
            this._operatorComments.Location = new System.Drawing.Point(7, 27);
            this._operatorComments.Multiline = true;
            this._operatorComments.Name = "_operatorComments";
            this._operatorComments.RemoveNewLineChars = false;
            this._operatorComments.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._operatorComments.Size = new System.Drawing.Size(371, 46);
            this._operatorComments.TabIndex = 1;
            this._operatorComments.TabStopMode = Extract.DataEntry.TabStopMode.OnlyWhenPopulatedOrInvalid;
            this._operatorComments.ValidationErrorMessage = "Invalid value";
            // 
            // _messageSequenceNumberFile
            // 
            this._messageSequenceNumberFile.AttributeName = "MessageSequenceNumberFile";
            this._messageSequenceNumberFile.AutoUpdateQuery = "<Query><SolutionDirectory/>\\MessageSequenceNumber.txt</Query>";
            this._messageSequenceNumberFile.Location = new System.Drawing.Point(208, 0);
            this._messageSequenceNumberFile.Name = "_messageSequenceNumberFile";
            this._messageSequenceNumberFile.Size = new System.Drawing.Size(100, 20);
            this._messageSequenceNumberFile.TabIndex = 0;
            this._messageSequenceNumberFile.ValidationErrorMessage = "Invalid value";
            this._messageSequenceNumberFile.Visible = false;
            // 
            // HealthNetworkLabsPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CommentControl = this._operatorComments;
            this.Controls.Add(this._messageSequenceNumberFile);
            this.Controls.Add(this._operatorCommentLabel);
            this.Controls.Add(this._resultStatusLabel);
            this.Controls.Add(this._resultStatus);
            this.Controls.Add(this._testsGroupBox);
            this.Controls.Add(this._testDetailsGroupBox);
            this.Controls.Add(this._physicianInfoGroupBox);
            this.Controls.Add(this._patientInfoGroupBox);
            this.Controls.Add(this._operatorComments);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            highlightColor1.Color = System.Drawing.Color.LightSalmon;
            highlightColor1.MaxOcrConfidence = 89;
            highlightColor2.Color = System.Drawing.Color.LightGreen;
            highlightColor2.MaxOcrConfidence = 100;
            this.HighlightColors = new Extract.DataEntry.HighlightColor[] {
        highlightColor1,
        highlightColor2};
            this.MinimumSize = new System.Drawing.Size(500, 0);
            this.Name = "HealthNetworkLabsPanel";
            this.Size = new System.Drawing.Size(593, 1025);
            ((System.ComponentModel.ISupportInitialize)(this._laboratoryTestTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._testComponentTable)).EndInit();
            this._patientInfoGroupBox.ResumeLayout(false);
            this._patientInfoGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._patientNameTable)).EndInit();
            this._physicianInfoGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._orderingPhysicianTable)).EndInit();
            this._testDetailsGroupBox.ResumeLayout(false);
            this._testDetailsGroupBox.PerformLayout();
            this._testsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Extract.DataEntry.DataEntryTable _laboratoryTestTable;
        private Extract.DataEntry.DataEntryTable _testComponentTable;
        private System.Windows.Forms.Label _labIDLabel;
        private Extract.DataEntry.DataEntryTextBox _labID;
        private Extract.DataEntry.DataEntryGroupBox _patientInfoGroupBox;
        private System.Windows.Forms.Label _patientMRLabel;
        private Extract.DataEntry.DataEntryTextBox _patientRecordNum;
        private Extract.DataEntry.DataEntryTextBox _patientBirthDate;
        private System.Windows.Forms.Label _birthDateLabel;
        private System.Windows.Forms.Label _genderLabel;
        private Extract.DataEntry.DataEntryComboBox _patientGender;
        private Extract.DataEntry.DataEntryGroupBox _physicianInfoGroupBox;
        private System.Windows.Forms.GroupBox _testDetailsGroupBox;
        private System.Windows.Forms.GroupBox _testsGroupBox;
        private System.Windows.Forms.Label _testCommentLabel;
        private Extract.DataEntry.DataEntryTextBox _testComment;
        private Extract.DataEntry.DataEntryComboBox _resultStatus;
        private System.Windows.Forms.Label _resultStatusLabel;
        private Extract.DataEntry.DataEntryTextBox _laboratoryIdentifier;
        private Extract.DataEntry.DataEntryTextBox _orderNumber;
        private System.Windows.Forms.Label _orderNumberLabel;
        private System.Windows.Forms.Label _labNameLabel;
        private Extract.DataEntry.DataEntryTable _patientNameTable;
        private Extract.DataEntry.DataEntryTextBox _labInfoPassThrough;
        private Extract.DataEntry.DataEntryTable _orderingPhysicianTable;
        private System.Windows.Forms.Label _operatorCommentLabel;
        private Extract.DataEntry.DataEntryTextBox _operatorComments;
        private Extract.DataEntry.DataEntryTableColumn _patientFirstNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _patientMiddleNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _patientLastNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _patientSuffixColumn;
        private Extract.DataEntry.DataEntryTextBox _messageSequenceNumberFile;
        private Extract.DataEntry.DataEntryTableColumn _orderingPhysicianLastNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _orderingPhysicianFirstNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _orderingPhysicianMiddleName;
        private Extract.DataEntry.DataEntryTableColumn _orderingPhysicianCodeColumn;
        private DataEntryTableColumn _testName;
        private DataEntryTableColumn _orderCode;
        private DataEntryTableColumn _testID;
        private DataEntryTableColumn _componentDate;
        private DataEntryTableColumn _laboratoryTestTime;
        private DataEntryTableColumn _componentName;
        private DataEntryTableColumn _testCode;
        private DataEntryTableColumn _componentValue;
        private DataEntryTableColumn _componentUnits;
        private DataEntryTableColumn _componentRefRange;
        private DataEntryTableColumn _componentFlag;
        private DataEntryTableColumn _componentOriginalName;
        private System.Windows.Forms.Label _componentCommentLabel;
        private Extract.DataEntry.DataEntryTextBox _componentComment;
    }
}
