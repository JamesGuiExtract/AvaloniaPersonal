namespace Extract.DataEntry.DEP.UWTransplantCenter
{
    partial class UWTransplantCenterPanel
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
            System.Windows.Forms.Label Name;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label _componentCommentLabel;
            System.Windows.Forms.Label label2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UWTransplantCenterPanel));
            Extract.DataEntry.HighlightColor highlightColor1 = new Extract.DataEntry.HighlightColor();
            Extract.DataEntry.HighlightColor highlightColor2 = new Extract.DataEntry.HighlightColor();
            this._labNameLabel = new System.Windows.Forms.Label();
            this._laboratoryIdentifier = new Extract.DataEntry.DataEntryTextBox();
            this._labInfoPassThrough = new Extract.DataEntry.DataEntryTextBox();
            this._laboratoryTestTable = new Extract.DataEntry.DataEntryTable();
            this._labID = new Extract.DataEntry.DataEntryTextBox();
            this._testComponentTable = new Extract.DataEntry.DataEntryTable();
            this._componentName = new Extract.DataEntry.DataEntryTableColumn();
            this._testCode = new Extract.DataEntry.DataEntryTableColumn();
            this._componentValue = new Extract.DataEntry.DataEntryTableColumn();
            this._componentUnits = new Extract.DataEntry.DataEntryTableColumn();
            this._componentRefRange = new Extract.DataEntry.DataEntryTableColumn();
            this._componentCalculatedFlag = new Extract.DataEntry.DataEntryTableColumn();
            this._componentFlag = new Extract.DataEntry.DataEntryTableColumn();
            this._componentStatusColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._componentOriginalName = new Extract.DataEntry.DataEntryTableColumn();
            this._patientInfoGroupBox = new Extract.DataEntry.DataEntryGroupBox();
            this._patientNameTable = new Extract.DataEntry.DataEntryTable();
            this._patientLastNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._patientFirstNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._patientBirthDate = new Extract.DataEntry.DataEntryTextBox();
            this._birthDateLabel = new System.Windows.Forms.Label();
            this._patientRecordNum = new Extract.DataEntry.DataEntryTextBox();
            this._patientMRLabel = new System.Windows.Forms.Label();
            this._orderingPhysicianLastNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._orderingPhysicianFirstNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._orderingPhysicianMiddleName = new Extract.DataEntry.DataEntryTableColumn();
            this._orderingPhysicianCodeColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._testDetailsGroupBox = new System.Windows.Forms.GroupBox();
            this._labStreetFilterTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._labCityFilterTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._componentComment = new Extract.DataEntry.DataEntryTextBox();
            this._labNameFilterTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._testCommentLabel = new System.Windows.Forms.Label();
            this._testComment = new Extract.DataEntry.DataEntryTextBox();
            this._testsGroupBox = new System.Windows.Forms.GroupBox();
            this._operatorCommentLabel = new System.Windows.Forms.Label();
            this._operatorComments = new Extract.DataEntry.DataEntryTextBox();
            this._messageSequenceNumberFile = new Extract.DataEntry.DataEntryTextBox();
            this._filename = new Extract.DataEntry.DataEntryTextBox();
            this._hasCoverPageComboBox = new Extract.DataEntry.DataEntryComboBox();
            this._orderNumberColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._testName = new Extract.DataEntry.DataEntryTableColumn();
            this._orderCode = new Extract.DataEntry.DataEntryTableColumn();
            this._testID = new Extract.DataEntry.DataEntryTableColumn();
            this._componentDate = new Extract.DataEntry.DataEntryTableColumn();
            this._laboratoryTestTime = new Extract.DataEntry.DataEntryTableColumn();
            this._componentResultDate = new Extract.DataEntry.DataEntryTableColumn();
            this._componentResultTime = new Extract.DataEntry.DataEntryTableColumn();
            Name = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            _componentCommentLabel = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._laboratoryTestTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._testComponentTable)).BeginInit();
            this._patientInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._patientNameTable)).BeginInit();
            this._testDetailsGroupBox.SuspendLayout();
            this._testsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // Name
            // 
            Name.AutoSize = true;
            Name.Location = new System.Drawing.Point(71, 20);
            Name.Name = "Name";
            Name.Size = new System.Drawing.Size(40, 13);
            Name.TabIndex = 0;
            Name.Text = "Name";
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(263, 20);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(30, 13);
            label1.TabIndex = 12;
            label1.Text = "City";
            // 
            // label3
            // 
            label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(408, 20);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(42, 13);
            label3.TabIndex = 14;
            label3.Text = "Street";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(6, 39);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(59, 13);
            label6.TabIndex = 18;
            label6.Text = "Lab Filter";
            // 
            // _componentCommentLabel
            // 
            _componentCommentLabel.AutoSize = true;
            _componentCommentLabel.Location = new System.Drawing.Point(4, 461);
            _componentCommentLabel.Name = "_componentCommentLabel";
            _componentCommentLabel.Size = new System.Drawing.Size(63, 13);
            _componentCommentLabel.TabIndex = 19;
            _componentCommentLabel.Text = "Comment";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(6, 83);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(235, 13);
            label2.TabIndex = 10;
            label2.Text = "Does the document have a cover page?";
            // 
            // _labNameLabel
            // 
            this._labNameLabel.AutoSize = true;
            this._labNameLabel.Location = new System.Drawing.Point(5, 61);
            this._labNameLabel.Name = "_labNameLabel";
            this._labNameLabel.Size = new System.Drawing.Size(64, 13);
            this._labNameLabel.TabIndex = 3;
            this._labNameLabel.Text = "Lab Name";
            // 
            // _laboratoryIdentifier
            // 
            this._laboratoryIdentifier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._laboratoryIdentifier.AttributeName = "LabIdentifier";
            this._laboratoryIdentifier.AutoUpdateQuery = resources.GetString("_laboratoryIdentifier.AutoUpdateQuery");
            this._laboratoryIdentifier.Location = new System.Drawing.Point(6, 77);
            this._laboratoryIdentifier.Name = "_laboratoryIdentifier";
            this._laboratoryIdentifier.ParentDataEntryControl = this._labInfoPassThrough;
            this._laboratoryIdentifier.Size = new System.Drawing.Size(568, 21);
            this._laboratoryIdentifier.TabIndex = 4;
            this._laboratoryIdentifier.ValidationErrorMessage = "Missing or unknown laboratory name";
            this._laboratoryIdentifier.ValidationPattern = "\\w";
            this._laboratoryIdentifier.ValidationQuery = resources.GetString("_laboratoryIdentifier.ValidationQuery");
            // 
            // _labInfoPassThrough
            // 
            this._labInfoPassThrough.AttributeName = "LabInfo";
            this._labInfoPassThrough.Location = new System.Drawing.Point(351, 6);
            this._labInfoPassThrough.Name = "_labInfoPassThrough";
            this._labInfoPassThrough.ParentDataEntryControl = this._laboratoryTestTable;
            this._labInfoPassThrough.Size = new System.Drawing.Size(16, 21);
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
            this._orderNumberColumn,
            this._testName,
            this._orderCode,
            this._testID,
            this._componentDate,
            this._laboratoryTestTime,
            this._componentResultDate,
            this._componentResultTime});
            this._laboratoryTestTable.Location = new System.Drawing.Point(6, 19);
            this._laboratoryTestTable.Name = "_laboratoryTestTable";
            this._laboratoryTestTable.ParentDataEntryControl = null;
            this._laboratoryTestTable.RowFormattingRuleFile = "Rules\\Swiping\\TestRow.rsd.etf";
            this._laboratoryTestTable.RowSwipingEnabled = true;
            this._laboratoryTestTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._laboratoryTestTable.Size = new System.Drawing.Size(581, 95);
            this._laboratoryTestTable.TabIndex = 1;
            // 
            // _labID
            // 
            this._labID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._labID.AttributeName = "LabCode";
            this._labID.AutoUpdateQuery = "<SQL>SELECT LabCode FROM LabAddresses WHERE LabName = SUBSTRING(<Attribute>../Lab" +
    "Identifier</Attribute>,1,100)</SQL>";
            this._labID.Location = new System.Drawing.Point(466, 6);
            this._labID.Name = "_labID";
            this._labID.ParentDataEntryControl = this._labInfoPassThrough;
            this._labID.Size = new System.Drawing.Size(11, 21);
            this._labID.SupportsSwiping = false;
            this._labID.TabIndex = 2;
            this._labID.TabStopMode = Extract.DataEntry.TabStopMode.OnlyWhenInvalid;
            this._labID.ValidationErrorMessage = "Missing or unknown laboratory ID";
            this._labID.ValidationQuery = "<SQL>SELECT LabCode FROM LabAddresses WHERE LabCode IS NOT NULL AND LEN(LabCode) " +
    "> 0 ORDER BY LabCode</SQL>";
            this._labID.Visible = false;
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
            this._componentCalculatedFlag,
            this._componentFlag,
            this._componentStatusColumn,
            this._componentOriginalName});
            this._testComponentTable.Location = new System.Drawing.Point(5, 170);
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
            this._testCode.Visible = false;
            // 
            // _componentValue
            // 
            this._componentValue.AttributeName = "Value";
            this._componentValue.AutoUpdateQuery = resources.GetString("_componentValue.AutoUpdateQuery");
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
            this._componentUnits.AutoUpdateQuery = "<Expression><SQL Name=\'Units\'>SELECT [Unit] FROM LabTest WHERE [TestCode] = <Attr" +
    "ibute>../TestCode</Attribute></SQL> == \'\'\r\n? <Attribute>.</Attribute>\r\n: <Units/" +
    ">\r\n</Expression>";
            this._componentUnits.FillWeight = 40F;
            this._componentUnits.HeaderText = "Units";
            this._componentUnits.Name = "_componentUnits";
            this._componentUnits.SmartHintsEnabled = true;
            this._componentUnits.ValidationCorrectsCase = false;
            this._componentUnits.ValidationErrorMessage = "Unrecognized unit designation.";
            this._componentUnits.ValidationQuery = "[BLANK]\r\n<SQL>SELECT Unit FROM Unit ORDER BY Unit</SQL>";
            // 
            // _componentRefRange
            // 
            this._componentRefRange.AttributeName = "Range";
            this._componentRefRange.AutoUpdateQuery = resources.GetString("_componentRefRange.AutoUpdateQuery");
            this._componentRefRange.FillWeight = 65F;
            this._componentRefRange.FormattingRuleFile = "Rules\\Swiping\\Range.rsd.etf";
            this._componentRefRange.HeaderText = "Ref. Range";
            this._componentRefRange.Name = "_componentRefRange";
            this._componentRefRange.SmartHintsEnabled = true;
            this._componentRefRange.ValidationErrorMessage = "Value is not allowed.";
            // 
            // _componentCalculatedFlag
            // 
            this._componentCalculatedFlag.AttributeName = "CalculatedFlag";
            this._componentCalculatedFlag.AutoUpdateQuery = resources.GetString("_componentCalculatedFlag.AutoUpdateQuery");
            this._componentCalculatedFlag.FillWeight = 1F;
            this._componentCalculatedFlag.HeaderText = "CalculatedFlag";
            this._componentCalculatedFlag.Name = "_componentCalculatedFlag";
            this._componentCalculatedFlag.PersistAttribute = false;
            this._componentCalculatedFlag.ValidationErrorMessage = "Invalid value";
            this._componentCalculatedFlag.Visible = false;
            // 
            // _componentFlag
            // 
            this._componentFlag.AttributeName = "Flag";
            this._componentFlag.AutoUpdateQuery = "<Expression>\r\n\t(<Attribute Name=\'CalculatedFlag\'>../CalculatedFlag</Attribute> ==" +
    " \'?\') ? <Attribute>.</Attribute> :\r\n\t(<CalculatedFlag/> == \'\') ? \'[BLANK]\' :\r\n\t<" +
    "CalculatedFlag/>\r\n</Expression>";
            this._componentFlag.FillWeight = 35F;
            this._componentFlag.HeaderText = "Flag";
            this._componentFlag.Name = "_componentFlag";
            this._componentFlag.SmartHintsEnabled = true;
            this._componentFlag.UseComboBoxCells = true;
            this._componentFlag.ValidationErrorMessage = "Flag does not correspond with test value and range";
            this._componentFlag.ValidationQuery = "<Expression>\r\n\t(<Attribute>../CalculatedFlag</Attribute> != \'A\') ? \'[BLANK]\' : \'\'" +
    "\r\n</Expression>\r\n<Expression>\r\n\t(<Attribute>../CalculatedFlag</Attribute> != \'\')" +
    " ? \'A\' : \'\'\r\n</Expression>";
            // 
            // _componentStatusColumn
            // 
            this._componentStatusColumn.AttributeName = "Status";
            this._componentStatusColumn.AutoUpdateQuery = "<Query Default=\'True\'>F</Query>";
            this._componentStatusColumn.FillWeight = 30F;
            this._componentStatusColumn.HeaderText = "Status";
            this._componentStatusColumn.Name = "_componentStatusColumn";
            this._componentStatusColumn.UseComboBoxCells = true;
            this._componentStatusColumn.ValidationErrorMessage = "Invalid value";
            this._componentStatusColumn.ValidationQuery = "C\r\nF";
            // 
            // _componentOriginalName
            // 
            this._componentOriginalName.AttributeName = "OriginalName";
            this._componentOriginalName.AutoUpdateQuery = "<Expression>\r\n(<Attribute Name=\'OriginalName\'>.</Attribute> == \'\') ? <Attribute S" +
    "patialMode=\'Force\'>..</Attribute> : <OriginalName SpatialMode=\'Force\'/>\r\n</Expre" +
    "ssion>";
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
            this._patientInfoGroupBox.Controls.Add(this._patientBirthDate);
            this._patientInfoGroupBox.Controls.Add(this._birthDateLabel);
            this._patientInfoGroupBox.Controls.Add(this._patientRecordNum);
            this._patientInfoGroupBox.Controls.Add(this._patientMRLabel);
            this._patientInfoGroupBox.Location = new System.Drawing.Point(0, 107);
            this._patientInfoGroupBox.Name = "_patientInfoGroupBox";
            this._patientInfoGroupBox.ParentDataEntryControl = null;
            this._patientInfoGroupBox.Size = new System.Drawing.Size(593, 114);
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
            this._patientLastNameColumn,
            this._patientFirstNameColumn});
            this._patientNameTable.Location = new System.Drawing.Point(7, 19);
            this._patientNameTable.MinimumNumberOfRows = 1;
            this._patientNameTable.Name = "_patientNameTable";
            this._patientNameTable.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientNameTable.RowFormattingRuleFile = "Rules\\Swiping\\name.rsd.etf";
            this._patientNameTable.RowSwipingEnabled = true;
            this._patientNameTable.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this._patientNameTable.Size = new System.Drawing.Size(567, 46);
            this._patientNameTable.TabIndex = 1;
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
            // _patientBirthDate
            // 
            this._patientBirthDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._patientBirthDate.AttributeName = "DOB";
            this._patientBirthDate.AutoUpdateQuery = resources.GetString("_patientBirthDate.AutoUpdateQuery");
            this._patientBirthDate.Location = new System.Drawing.Point(7, 84);
            this._patientBirthDate.Name = "_patientBirthDate";
            this._patientBirthDate.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientBirthDate.Size = new System.Drawing.Size(360, 21);
            this._patientBirthDate.TabIndex = 3;
            this._patientBirthDate.ValidationErrorMessage = "Date of birth must be a valid date in the format MM/DD/YYYY";
            this._patientBirthDate.ValidationPattern = "^((0?[1-9])|(1[0-2]))/((0?[1-9])|(1[0-9])|(2[0-9])|(3[01]))/(18|19|20)\\d{2}$";
            // 
            // _birthDateLabel
            // 
            this._birthDateLabel.AutoSize = true;
            this._birthDateLabel.Location = new System.Drawing.Point(5, 68);
            this._birthDateLabel.Name = "_birthDateLabel";
            this._birthDateLabel.Size = new System.Drawing.Size(80, 13);
            this._birthDateLabel.TabIndex = 0;
            this._birthDateLabel.Text = "Date of Birth";
            // 
            // _patientRecordNum
            // 
            this._patientRecordNum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._patientRecordNum.AttributeName = "MR_Number";
            this._patientRecordNum.ClearClipboardOnPaste = true;
            this._patientRecordNum.Location = new System.Drawing.Point(388, 84);
            this._patientRecordNum.Name = "_patientRecordNum";
            this._patientRecordNum.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientRecordNum.Size = new System.Drawing.Size(186, 21);
            this._patientRecordNum.TabIndex = 5;
            this._patientRecordNum.ValidationErrorMessage = "MRN missing or invalid";
            this._patientRecordNum.ValidationPattern = "^\\d{7,8}\\s?$";
            // 
            // _patientMRLabel
            // 
            this._patientMRLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._patientMRLabel.AutoSize = true;
            this._patientMRLabel.Location = new System.Drawing.Point(385, 68);
            this._patientMRLabel.Name = "_patientMRLabel";
            this._patientMRLabel.Size = new System.Drawing.Size(32, 13);
            this._patientMRLabel.TabIndex = 0;
            this._patientMRLabel.Text = "MRN";
            // 
            // _orderingPhysicianLastNameColumn
            // 
            this._orderingPhysicianLastNameColumn.AttributeName = "Last";
            this._orderingPhysicianLastNameColumn.AutoUpdateQuery = "OUTSIDE";
            this._orderingPhysicianLastNameColumn.HeaderText = "Last Name";
            this._orderingPhysicianLastNameColumn.Name = "_orderingPhysicianLastNameColumn";
            this._orderingPhysicianLastNameColumn.TabStopMode = Extract.DataEntry.TabStopMode.Never;
            this._orderingPhysicianLastNameColumn.ValidationErrorMessage = ".";
            this._orderingPhysicianLastNameColumn.ValidationQuery = "";
            // 
            // _orderingPhysicianFirstNameColumn
            // 
            this._orderingPhysicianFirstNameColumn.AttributeName = "First";
            this._orderingPhysicianFirstNameColumn.AutoUpdateQuery = "PROVIDER";
            this._orderingPhysicianFirstNameColumn.FillWeight = 75F;
            this._orderingPhysicianFirstNameColumn.HeaderText = "First Name";
            this._orderingPhysicianFirstNameColumn.Name = "_orderingPhysicianFirstNameColumn";
            this._orderingPhysicianFirstNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._orderingPhysicianFirstNameColumn.TabStopMode = Extract.DataEntry.TabStopMode.Never;
            this._orderingPhysicianFirstNameColumn.ValidationErrorMessage = "";
            this._orderingPhysicianFirstNameColumn.ValidationQuery = "";
            // 
            // _orderingPhysicianMiddleName
            // 
            this._orderingPhysicianMiddleName.AttributeName = null;
            this._orderingPhysicianMiddleName.Name = "_orderingPhysicianMiddleName";
            this._orderingPhysicianMiddleName.ValidationErrorMessage = "Invalid value";
            // 
            // _orderingPhysicianCodeColumn
            // 
            this._orderingPhysicianCodeColumn.AttributeName = null;
            this._orderingPhysicianCodeColumn.Name = "_orderingPhysicianCodeColumn";
            this._orderingPhysicianCodeColumn.ValidationErrorMessage = "Invalid value";
            // 
            // _testDetailsGroupBox
            // 
            this._testDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._testDetailsGroupBox.Controls.Add(_componentCommentLabel);
            this._testDetailsGroupBox.Controls.Add(label6);
            this._testDetailsGroupBox.Controls.Add(this._labStreetFilterTextBox);
            this._testDetailsGroupBox.Controls.Add(label3);
            this._testDetailsGroupBox.Controls.Add(this._labCityFilterTextBox);
            this._testDetailsGroupBox.Controls.Add(this._componentComment);
            this._testDetailsGroupBox.Controls.Add(label1);
            this._testDetailsGroupBox.Controls.Add(this._labInfoPassThrough);
            this._testDetailsGroupBox.Controls.Add(this._labNameFilterTextBox);
            this._testDetailsGroupBox.Controls.Add(this._labNameLabel);
            this._testDetailsGroupBox.Controls.Add(Name);
            this._testDetailsGroupBox.Controls.Add(this._laboratoryIdentifier);
            this._testDetailsGroupBox.Controls.Add(this._testCommentLabel);
            this._testDetailsGroupBox.Controls.Add(this._testComment);
            this._testDetailsGroupBox.Controls.Add(this._labID);
            this._testDetailsGroupBox.Controls.Add(this._testComponentTable);
            this._testDetailsGroupBox.Location = new System.Drawing.Point(0, 356);
            this._testDetailsGroupBox.Name = "_testDetailsGroupBox";
            this._testDetailsGroupBox.Size = new System.Drawing.Size(593, 613);
            this._testDetailsGroupBox.TabIndex = 9;
            this._testDetailsGroupBox.TabStop = false;
            this._testDetailsGroupBox.Text = "Selected Order Details";
            // 
            // _labStreetFilterTextBox
            // 
            this._labStreetFilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._labStreetFilterTextBox.AttributeName = "StreetFilter";
            this._labStreetFilterTextBox.AutoUpdateQuery = "";
            this._labStreetFilterTextBox.Location = new System.Drawing.Point(411, 36);
            this._labStreetFilterTextBox.Name = "_labStreetFilterTextBox";
            this._labStreetFilterTextBox.ParentDataEntryControl = this._labInfoPassThrough;
            this._labStreetFilterTextBox.PersistAttribute = false;
            this._labStreetFilterTextBox.Size = new System.Drawing.Size(163, 21);
            this._labStreetFilterTextBox.TabIndex = 3;
            this._labStreetFilterTextBox.TabStopMode = Extract.DataEntry.TabStopMode.Never;
            this._labStreetFilterTextBox.ValidationErrorMessage = "";
            this._labStreetFilterTextBox.ValidationQuery = "";
            // 
            // _labCityFilterTextBox
            // 
            this._labCityFilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._labCityFilterTextBox.AttributeName = "CityFilter";
            this._labCityFilterTextBox.AutoUpdateQuery = "";
            this._labCityFilterTextBox.Location = new System.Drawing.Point(266, 36);
            this._labCityFilterTextBox.Name = "_labCityFilterTextBox";
            this._labCityFilterTextBox.ParentDataEntryControl = this._labInfoPassThrough;
            this._labCityFilterTextBox.PersistAttribute = false;
            this._labCityFilterTextBox.Size = new System.Drawing.Size(139, 21);
            this._labCityFilterTextBox.TabIndex = 2;
            this._labCityFilterTextBox.TabStopMode = Extract.DataEntry.TabStopMode.Never;
            this._labCityFilterTextBox.ValidationErrorMessage = "";
            this._labCityFilterTextBox.ValidationQuery = "";
            // 
            // _componentComment
            // 
            this._componentComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._componentComment.AttributeName = "Comment";
            this._componentComment.AutoUpdateQuery = "";
            this._componentComment.Location = new System.Drawing.Point(5, 477);
            this._componentComment.Multiline = true;
            this._componentComment.Name = "_componentComment";
            this._componentComment.ParentDataEntryControl = this._testComponentTable;
            this._componentComment.RemoveNewLineChars = false;
            this._componentComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._componentComment.Size = new System.Drawing.Size(569, 128);
            this._componentComment.TabIndex = 8;
            this._componentComment.TabStopMode = Extract.DataEntry.TabStopMode.OnlyWhenPopulatedOrInvalid;
            this._componentComment.ValidationErrorMessage = "";
            // 
            // _labNameFilterTextBox
            // 
            this._labNameFilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._labNameFilterTextBox.AttributeName = "NameFilter";
            this._labNameFilterTextBox.AutoUpdateQuery = "";
            this._labNameFilterTextBox.Location = new System.Drawing.Point(74, 36);
            this._labNameFilterTextBox.Name = "_labNameFilterTextBox";
            this._labNameFilterTextBox.ParentDataEntryControl = this._labInfoPassThrough;
            this._labNameFilterTextBox.PersistAttribute = false;
            this._labNameFilterTextBox.Size = new System.Drawing.Size(186, 21);
            this._labNameFilterTextBox.TabIndex = 1;
            this._labNameFilterTextBox.TabStopMode = Extract.DataEntry.TabStopMode.Never;
            this._labNameFilterTextBox.ValidationErrorMessage = "";
            this._labNameFilterTextBox.ValidationQuery = "";
            // 
            // _testCommentLabel
            // 
            this._testCommentLabel.AutoSize = true;
            this._testCommentLabel.Location = new System.Drawing.Point(5, 101);
            this._testCommentLabel.Name = "_testCommentLabel";
            this._testCommentLabel.Size = new System.Drawing.Size(63, 13);
            this._testCommentLabel.TabIndex = 7;
            this._testCommentLabel.Text = "Comment";
            // 
            // _testComment
            // 
            this._testComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._testComment.AttributeName = "Comment";
            this._testComment.AutoUpdateQuery = "<Query Default=\'1\'>Verified by $FullUserName() on $Now(%m/%d/%Y).</Query>";
            this._testComment.Location = new System.Drawing.Point(5, 117);
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
            // _testsGroupBox
            // 
            this._testsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._testsGroupBox.Controls.Add(this._laboratoryTestTable);
            this._testsGroupBox.Location = new System.Drawing.Point(0, 227);
            this._testsGroupBox.Name = "_testsGroupBox";
            this._testsGroupBox.Size = new System.Drawing.Size(593, 123);
            this._testsGroupBox.TabIndex = 8;
            this._testsGroupBox.TabStop = false;
            this._testsGroupBox.Text = "Orders";
            // 
            // _operatorCommentLabel
            // 
            this._operatorCommentLabel.AutoSize = true;
            this._operatorCommentLabel.Location = new System.Drawing.Point(6, 11);
            this._operatorCommentLabel.Name = "_operatorCommentLabel";
            this._operatorCommentLabel.Size = new System.Drawing.Size(185, 13);
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
            this._operatorComments.Size = new System.Drawing.Size(567, 46);
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
            this._messageSequenceNumberFile.Size = new System.Drawing.Size(12, 21);
            this._messageSequenceNumberFile.TabIndex = 0;
            this._messageSequenceNumberFile.ValidationErrorMessage = "Invalid value";
            this._messageSequenceNumberFile.Visible = false;
            // 
            // _filename
            // 
            this._filename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._filename.AttributeName = "Filename";
            this._filename.AutoUpdateQuery = "<SourceDocName/>";
            this._filename.BackColor = System.Drawing.SystemColors.Control;
            this._filename.ForeColor = System.Drawing.Color.Black;
            this._filename.Location = new System.Drawing.Point(226, 0);
            this._filename.Name = "_filename";
            this._filename.ReadOnly = true;
            this._filename.Size = new System.Drawing.Size(14, 21);
            this._filename.SupportsSwiping = false;
            this._filename.TabIndex = 3;
            this._filename.TabStop = false;
            this._filename.TabStopMode = Extract.DataEntry.TabStopMode.Never;
            this._filename.ValidationErrorMessage = "";
            this._filename.Visible = false;
            // 
            // _hasCoverPageComboBox
            // 
            this._hasCoverPageComboBox.AttributeName = "ClueOnFirstPage";
            this._hasCoverPageComboBox.AutoUpdateQuery = "<Expression>(<Attribute Name=\'Value\'>.</Attribute>==\'Yes\' or <Value/>==\'No\') ? <V" +
    "alue/> : \'[BLANK]\'</Expression>";
            this._hasCoverPageComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this._hasCoverPageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._hasCoverPageComboBox.FormattingEnabled = true;
            this._hasCoverPageComboBox.Location = new System.Drawing.Point(247, 80);
            this._hasCoverPageComboBox.Name = "_hasCoverPageComboBox";
            this._hasCoverPageComboBox.Size = new System.Drawing.Size(64, 22);
            this._hasCoverPageComboBox.TabIndex = 2;
            this._hasCoverPageComboBox.ValidationErrorMessage = "Specify whether the first page is a cover page";
            this._hasCoverPageComboBox.ValidationPattern = "Yes|No";
            this._hasCoverPageComboBox.ValidationQuery = "<Query ValidationListType=\'AutoCompleteOnly\'>\r\n[BLANK]\r\nYes\r\nNo\r\n</Query>";
            this._hasCoverPageComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.HandleHasCoverPageComboBox_DrawItem);
            // 
            // _orderNumberColumn
            // 
            this._orderNumberColumn.AttributeName = "OrderNumber";
            this._orderNumberColumn.FillWeight = 75F;
            this._orderNumberColumn.HeaderText = "Order Number";
            this._orderNumberColumn.Name = "_orderNumberColumn";
            this._orderNumberColumn.ValidationErrorMessage = "Order Number missing or invalid";
            this._orderNumberColumn.ValidationPattern = "^\\d{8,10}\\s?$";
            // 
            // _testName
            // 
            this._testName.AttributeName = "Name";
            this._testName.AutoUpdateQuery = resources.GetString("_testName.AutoUpdateQuery");
            this._testName.HeaderText = "Order Name";
            this._testName.Name = "_testName";
            this._testName.ValidationErrorMessage = "Order name is not recognized.";
            this._testName.ValidationQuery = resources.GetString("_testName.ValidationQuery");
            this._testName.Visible = false;
            // 
            // _orderCode
            // 
            this._orderCode.AttributeName = "OrderCode";
            this._orderCode.AutoUpdateQuery = "<SQL>SELECT DISTINCT [Code] FROM LabOrder</SQL>";
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
            this._testID.HeaderText = "Epic Code";
            this._testID.Name = "_testID";
            this._testID.TabStopMode = Extract.DataEntry.TabStopMode.OnlyWhenInvalid;
            this._testID.ValidationErrorMessage = "Order code is not recognized.";
            this._testID.ValidationQuery = "<SQL>SELECT EpicCode FROM LabOrder WHERE EpicCode IS NOT NULL ORDER BY EpicCode</" +
    "SQL>";
            this._testID.Visible = false;
            // 
            // _componentDate
            // 
            this._componentDate.AttributeName = "CollectionDate";
            this._componentDate.AutoUpdateQuery = resources.GetString("_componentDate.AutoUpdateQuery");
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
            this._laboratoryTestTime.AutoUpdateQuery = resources.GetString("_laboratoryTestTime.AutoUpdateQuery");
            this._laboratoryTestTime.FillWeight = 60F;
            this._laboratoryTestTime.FormattingRuleFile = "Rules\\Swiping\\CollectionTime.rsd.etf";
            this._laboratoryTestTime.HeaderText = "Collection Time";
            this._laboratoryTestTime.Name = "_laboratoryTestTime";
            this._laboratoryTestTime.ValidationErrorMessage = "Collection time must be a valid time formatted HH:MM";
            this._laboratoryTestTime.ValidationPattern = "^((0?[0-9])|(1[0-9])|(2[0-3])):[0-5][0-9]$";
            // 
            // _componentResultDate
            // 
            this._componentResultDate.AttributeName = "ResultDate";
            this._componentResultDate.AutoUpdateQuery = "<Query Default=\'1\'><Attribute>../CollectionDate</Attribute></Query>\r\n<Query><Attr" +
    "ibute>../../ResultDate</Attribute></Query>";
            this._componentResultDate.FillWeight = 60F;
            this._componentResultDate.FormattingRuleFile = "Rules\\Swiping\\ResultDate.rsd.etf";
            this._componentResultDate.HeaderText = "Result Date";
            this._componentResultDate.Name = "_componentResultDate";
            this._componentResultDate.ValidationErrorMessage = "Result date must be a valid date formatted MM/DD/YYYY";
            this._componentResultDate.ValidationPattern = "^((0?[1-9])|(1[0-2]))/((0?[1-9])|(1[0-9])|(2[0-9])|(3[01]))/(19|20)\\d{2}$";
            this._componentResultDate.Visible = false;
            // 
            // _componentResultTime
            // 
            this._componentResultTime.AttributeName = "ResultTime";
            this._componentResultTime.AutoUpdateQuery = resources.GetString("_componentResultTime.AutoUpdateQuery");
            this._componentResultTime.FillWeight = 60F;
            this._componentResultTime.HeaderText = "Result Time";
            this._componentResultTime.Name = "_componentResultTime";
            this._componentResultTime.ValidationErrorMessage = "Result time must be a valid time formatted HH:MM";
            this._componentResultTime.ValidationPattern = "(^$)|(^((0?[0-9])|(1[0-9])|(2[0-3])):[0-5][0-9]$)";
            this._componentResultTime.Visible = false;
            // 
            // UWTransplantCenterPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CommentControl = this._operatorComments;
            this.Controls.Add(this._hasCoverPageComboBox);
            this.Controls.Add(label2);
            this.Controls.Add(this._filename);
            this.Controls.Add(this._messageSequenceNumberFile);
            this.Controls.Add(this._operatorCommentLabel);
            this.Controls.Add(this._testsGroupBox);
            this.Controls.Add(this._testDetailsGroupBox);
            this.Controls.Add(this._patientInfoGroupBox);
            this.Controls.Add(this._operatorComments);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            highlightColor1.Color = System.Drawing.Color.LightSalmon;
            highlightColor1.MaxOcrConfidence = 89;
            highlightColor2.Color = System.Drawing.Color.LightGreen;
            highlightColor2.MaxOcrConfidence = 100;
            this.HighlightColors = new Extract.DataEntry.HighlightColor[] {
        highlightColor1,
        highlightColor2};
            this.MinimumSize = new System.Drawing.Size(500, 0);
            this.Name = "UWTransplantCenterPanel";
            this.Size = new System.Drawing.Size(593, 971);
            ((System.ComponentModel.ISupportInitialize)(this._laboratoryTestTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._testComponentTable)).EndInit();
            this._patientInfoGroupBox.ResumeLayout(false);
            this._patientInfoGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._patientNameTable)).EndInit();
            this._testDetailsGroupBox.ResumeLayout(false);
            this._testDetailsGroupBox.PerformLayout();
            this._testsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Extract.DataEntry.DataEntryTable _laboratoryTestTable;
        private Extract.DataEntry.DataEntryTable _testComponentTable;
        private Extract.DataEntry.DataEntryTextBox _labID;
        private Extract.DataEntry.DataEntryGroupBox _patientInfoGroupBox;
        private System.Windows.Forms.Label _patientMRLabel;
        private Extract.DataEntry.DataEntryTextBox _patientRecordNum;
        private Extract.DataEntry.DataEntryTextBox _patientBirthDate;
        private System.Windows.Forms.Label _birthDateLabel;
        private System.Windows.Forms.GroupBox _testDetailsGroupBox;
        private System.Windows.Forms.GroupBox _testsGroupBox;
        private System.Windows.Forms.Label _testCommentLabel;
        private Extract.DataEntry.DataEntryTextBox _testComment;
        private Extract.DataEntry.DataEntryTextBox _laboratoryIdentifier;
        private System.Windows.Forms.Label _labNameLabel;
        private Extract.DataEntry.DataEntryTable _patientNameTable;
        private Extract.DataEntry.DataEntryTextBox _labInfoPassThrough;
        private System.Windows.Forms.Label _operatorCommentLabel;
        private Extract.DataEntry.DataEntryTextBox _operatorComments;
        private Extract.DataEntry.DataEntryTextBox _messageSequenceNumberFile;
        private DataEntryTextBox _componentComment;
        private DataEntryTextBox _filename;
        private DataEntryTableColumn _orderingPhysicianLastNameColumn;
        private DataEntryTableColumn _orderingPhysicianFirstNameColumn;
        private DataEntryTableColumn _orderingPhysicianMiddleName;
        private DataEntryTableColumn _orderingPhysicianCodeColumn;
        private DataEntryTextBox _labStreetFilterTextBox;
        private DataEntryTextBox _labCityFilterTextBox;
        private DataEntryTextBox _labNameFilterTextBox;
        private DataEntryComboBox _hasCoverPageComboBox;
        private DataEntryTableColumn _patientLastNameColumn;
        private DataEntryTableColumn _patientFirstNameColumn;
        private DataEntryTableColumn _componentName;
        private DataEntryTableColumn _testCode;
        private DataEntryTableColumn _componentValue;
        private DataEntryTableColumn _componentUnits;
        private DataEntryTableColumn _componentRefRange;
        private DataEntryTableColumn _componentCalculatedFlag;
        private DataEntryTableColumn _componentFlag;
        private DataEntryTableColumn _componentStatusColumn;
        private DataEntryTableColumn _componentOriginalName;
        private DataEntryTableColumn _orderNumberColumn;
        private DataEntryTableColumn _testName;
        private DataEntryTableColumn _orderCode;
        private DataEntryTableColumn _testID;
        private DataEntryTableColumn _componentDate;
        private DataEntryTableColumn _laboratoryTestTime;
        private DataEntryTableColumn _componentResultDate;
        private DataEntryTableColumn _componentResultTime;
    }
}
