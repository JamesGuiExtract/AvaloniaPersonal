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
            this._labIDLabel = new System.Windows.Forms.Label();
            this._labID = new Extract.DataEntry.DataEntryTextBox();
            this._testComponentTable = new Extract.DataEntry.DataEntryTable();
            this._componentName = new Extract.DataEntry.DataEntryTableColumn();
            this._testCode = new Extract.DataEntry.DataEntryTableColumn();
            this._componentValue = new Extract.DataEntry.DataEntryTableColumn();
            this._componentUnits = new Extract.DataEntry.DataEntryTableColumn();
            this._componentRefRange = new Extract.DataEntry.DataEntryTableColumn();
            this._componentFlag = new Extract.DataEntry.DataEntryTableColumn();
            this._componentCorrectFlag = new Extract.DataEntry.DataEntryTableColumn();
            this._componentOriginalName = new Extract.DataEntry.DataEntryTableColumn();
            this._patientInfoGroupBox = new Extract.DataEntry.DataEntryGroupBox();
            this._patientNameTable = new Extract.DataEntry.DataEntryTable();
            this._patientFirstNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._patientMiddleNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._patientLastNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._patientSuffixColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._patientRecordNum = new Extract.DataEntry.DataEntryTextBox();
            this._patientMRLabel = new System.Windows.Forms.Label();
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
            this._laboratoryIdentifier.Size = new System.Drawing.Size(432, 20);
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
            this._testID});
            this._laboratoryTestTable.Location = new System.Drawing.Point(6, 19);
            this._laboratoryTestTable.Name = "_laboratoryTestTable";
            this._laboratoryTestTable.ParentDataEntryControl = null;
            this._laboratoryTestTable.RowFormattingRuleFile = "Rules\\Swiping\\TestRow.rsd.etf";
            this._laboratoryTestTable.RowSwipingEnabled = true;
            this._laboratoryTestTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._laboratoryTestTable.Size = new System.Drawing.Size(585, 86);
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
            this._orderCode.FillWeight = 25F;
            this._orderCode.HeaderText = "Code";
            this._orderCode.Name = "_orderCode";
            this._orderCode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._orderCode.TabStopMode = Extract.DataEntry.TabStopMode.OnlyWhenInvalid;
            this._orderCode.ValidationErrorMessage = "Order code is not recognized";
            this._orderCode.ValidationQuery = "<SQL>SELECT Code FROM LabOrder WHERE Code IS NOT NULL ORDER BY Code</SQL>";
            // 
            // _testID
            // 
            this._testID.AttributeName = "EpicCode";
            this._testID.AutoUpdateQuery = "<SQL>SELECT EpicCode FROM LabOrder WHERE Name = SUBSTRING(<Attribute>../Name</Att" +
    "ribute>,1,50)</SQL>";
            this._testID.FillWeight = 1F;
            this._testID.HeaderText = "Epic Code";
            this._testID.Name = "_testID";
            this._testID.TabStopMode = Extract.DataEntry.TabStopMode.Never;
            this._testID.ValidationErrorMessage = "Order code is not recognized.";
            this._testID.ValidationQuery = "";
            this._testID.Visible = false;
            // 
            // _labIDLabel
            // 
            this._labIDLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._labIDLabel.AutoSize = true;
            this._labIDLabel.Location = new System.Drawing.Point(458, 54);
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
            this._labID.Location = new System.Drawing.Point(461, 70);
            this._labID.Name = "_labID";
            this._labID.ParentDataEntryControl = this._labInfoPassThrough;
            this._labID.Size = new System.Drawing.Size(112, 20);
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
            this._componentCorrectFlag,
            this._componentOriginalName});
            this._testComponentTable.Location = new System.Drawing.Point(6, 163);
            this._testComponentTable.Name = "_testComponentTable";
            this._testComponentTable.ParentDataEntryControl = this._laboratoryTestTable;
            this._testComponentTable.RowFormattingRuleFile = "Rules\\Swiping\\ComponentRow.rsd.etf";
            this._testComponentTable.RowSwipingEnabled = true;
            this._testComponentTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._testComponentTable.Size = new System.Drawing.Size(585, 288);
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
            this._componentValue.ValidationQuery = resources.GetString("_componentValue.ValidationQuery");
            // 
            // _componentUnits
            // 
            this._componentUnits.AttributeName = "Units";
            this._componentUnits.FillWeight = 40F;
            this._componentUnits.HeaderText = "Units";
            this._componentUnits.Name = "_componentUnits";
            this._componentUnits.SmartHintsEnabled = true;
            this._componentUnits.ValidationCorrectsCase = false;
            this._componentUnits.ValidationErrorMessage = "Unrecognized unit designation";
            this._componentUnits.ValidationQuery = resources.GetString("_componentUnits.ValidationQuery");
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
            this._componentFlag.AutoUpdateQuery = "<Attribute>../CorrectFlag</Attribute>";
            this._componentFlag.FillWeight = 35F;
            this._componentFlag.HeaderText = "Flag";
            this._componentFlag.Name = "_componentFlag";
            this._componentFlag.SmartHintsEnabled = true;
            this._componentFlag.UseComboBoxCells = true;
            this._componentFlag.ValidationErrorMessage = "Flag does not correspond with test value and range";
            this._componentFlag.ValidationQuery = "<Query>[BLANK]<SQL>SELECT * FROM Flag ORDER BY Flag</SQL></Query>\r\n<Query Validat" +
    "ionListType=\'ValidationListOnly\' ValidationWarning=\'1\'>\r\n\t<Attribute>../CorrectF" +
    "lag</Attribute>\r\n</Query>";
            // 
            // _componentCorrectFlag
            // 
            this._componentCorrectFlag.AttributeName = "CorrectFlag";
            this._componentCorrectFlag.AutoUpdateQuery = resources.GetString("_componentCorrectFlag.AutoUpdateQuery");
            this._componentCorrectFlag.HeaderText = "Correct Flag";
            this._componentCorrectFlag.Name = "_componentCorrectFlag";
            this._componentCorrectFlag.PersistAttribute = false;
            this._componentCorrectFlag.ValidationErrorMessage = "Invalid value";
            this._componentCorrectFlag.Visible = false;
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
            this._patientNameTable.Size = new System.Drawing.Size(586, 46);
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
            // _patientRecordNum
            // 
            this._patientRecordNum.AttributeName = "MR_Number";
            this._patientRecordNum.ClearClipboardOnPaste = true;
            this._patientRecordNum.Location = new System.Drawing.Point(5, 87);
            this._patientRecordNum.Name = "_patientRecordNum";
            this._patientRecordNum.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientRecordNum.Size = new System.Drawing.Size(188, 20);
            this._patientRecordNum.TabIndex = 4;
            this._patientRecordNum.ValidationErrorMessage = "Invalid medical record number";
            this._patientRecordNum.ValidationPattern = "^([\\w]+-)?\\d+\\s?$";
            // 
            // _patientMRLabel
            // 
            this._patientMRLabel.AutoSize = true;
            this._patientMRLabel.Location = new System.Drawing.Point(2, 71);
            this._patientMRLabel.Name = "_patientMRLabel";
            this._patientMRLabel.Size = new System.Drawing.Size(92, 13);
            this._patientMRLabel.TabIndex = 0;
            this._patientMRLabel.Text = "Medical Record #";
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
            this._testDetailsGroupBox.Location = new System.Drawing.Point(0, 315);
            this._testDetailsGroupBox.Name = "_testDetailsGroupBox";
            this._testDetailsGroupBox.Size = new System.Drawing.Size(593, 650);
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
            this._orderNumber.Location = new System.Drawing.Point(7, 32);
            this._orderNumber.Name = "_orderNumber";
            this._orderNumber.ParentDataEntryControl = this._laboratoryTestTable;
            this._orderNumber.Size = new System.Drawing.Size(201, 20);
            this._orderNumber.SupportsSwiping = false;
            this._orderNumber.TabIndex = 2;
            this._orderNumber.ValidationErrorMessage = "Invalid accession number";
            this._orderNumber.ValidationPattern = "(?i)^[a-z]\\d{8}\\s?$";
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
            this._testComment.AutoUpdateQuery = "<Query Default=\'1\'>Verified by $UserName() on $Now(%m/%d/%Y).</Query>";
            this._testComment.Location = new System.Drawing.Point(6, 110);
            this._testComment.Multiline = true;
            this._testComment.Name = "_testComment";
            this._testComment.ParentDataEntryControl = this._laboratoryTestTable;
            this._testComment.RemoveNewLineChars = false;
            this._testComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._testComment.Size = new System.Drawing.Size(585, 47);
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
            this._componentComment.Size = new System.Drawing.Size(585, 170);
            this._componentComment.TabIndex = 9;
            this._componentComment.TabStopMode = Extract.DataEntry.TabStopMode.OnlyWhenPopulatedOrInvalid;
            this._componentComment.ValidationErrorMessage = "";
            // 
            // _testsGroupBox
            // 
            this._testsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._testsGroupBox.Controls.Add(this._laboratoryTestTable);
            this._testsGroupBox.Location = new System.Drawing.Point(0, 196);
            this._testsGroupBox.Name = "_testsGroupBox";
            this._testsGroupBox.Size = new System.Drawing.Size(593, 113);
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
            this._resultStatus.Size = new System.Drawing.Size(186, 21);
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
            this._messageSequenceNumberFile.AutoUpdateQuery = "<Query><SolutionDirectory/>\\Corepoint Integration\\MessageSequenceNumber.txt</Quer" +
    "y>";
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
            this.Size = new System.Drawing.Size(593, 963);
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

        private Extract.DataEntry.DataEntryTable _testComponentTable;
        private System.Windows.Forms.Label _labIDLabel;
        private Extract.DataEntry.DataEntryTextBox _labID;
        private Extract.DataEntry.DataEntryGroupBox _patientInfoGroupBox;
        private System.Windows.Forms.Label _patientMRLabel;
        private Extract.DataEntry.DataEntryTextBox _patientRecordNum;
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
        private System.Windows.Forms.Label _operatorCommentLabel;
        private Extract.DataEntry.DataEntryTextBox _operatorComments;
        private Extract.DataEntry.DataEntryTextBox _messageSequenceNumberFile;
        private System.Windows.Forms.Label _componentCommentLabel;
        private Extract.DataEntry.DataEntryTextBox _componentComment;
        private DataEntryTableColumn _patientFirstNameColumn;
        private DataEntryTableColumn _patientMiddleNameColumn;
        private DataEntryTableColumn _patientLastNameColumn;
        private DataEntryTableColumn _patientSuffixColumn;
        private DataEntryTable _laboratoryTestTable;
        private DataEntryTableColumn _testName;
        private DataEntryTableColumn _orderCode;
        private DataEntryTableColumn _testID;
        private DataEntryTableColumn _componentName;
        private DataEntryTableColumn _testCode;
        private DataEntryTableColumn _componentValue;
        private DataEntryTableColumn _componentUnits;
        private DataEntryTableColumn _componentRefRange;
        private DataEntryTableColumn _componentFlag;
        private DataEntryTableColumn _componentCorrectFlag;
        private DataEntryTableColumn _componentOriginalName;
    }
}
