namespace Extract.LabDE.StandardLabDE
{
    partial class StandardLabDEPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StandardLabDEPanel));
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow1 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow2 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow3 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow4 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.HighlightColor highlightColor1 = new Extract.DataEntry.HighlightColor();
            Extract.DataEntry.HighlightColor highlightColor2 = new Extract.DataEntry.HighlightColor();
            this._labNameLabel = new System.Windows.Forms.Label();
            this._laboratoryIdentifier = new Extract.DataEntry.DataEntryComboBox();
            this._laboratoryTestTable = new Extract.DataEntry.DataEntryTable();
            this._testName = new Extract.DataEntry.DataEntryTableColumn();
            this._testID = new Extract.DataEntry.DataEntryTableColumn();
            this._componentDate = new Extract.DataEntry.DataEntryTableColumn();
            this._laboratoryTestTime = new Extract.DataEntry.DataEntryTableColumn();
            this._labIDLabel = new System.Windows.Forms.Label();
            this._labID = new Extract.DataEntry.DataEntryTextBox();
            this._labAddress = new Extract.DataEntry.DataEntryTwoColumnTable();
            this._testComponentTable = new Extract.DataEntry.DataEntryTable();
            this._componentName = new Extract.DataEntry.DataEntryTableColumn();
            this._testCode = new Extract.DataEntry.DataEntryTableColumn();
            this._componentValue = new Extract.DataEntry.DataEntryTableColumn();
            this._componentUnits = new Extract.DataEntry.DataEntryTableColumn();
            this._componentRefRange = new Extract.DataEntry.DataEntryTableColumn();
            this._componentFlag = new Extract.DataEntry.DataEntryTableColumn();
            this._componentComment = new Extract.DataEntry.DataEntryTableColumn();
            this._patientInfoGroupBox = new Extract.DataEntry.DataEntryGroupBox();
            this._patientNameTable = new Extract.DataEntry.DataEntryTable();
            this._patientLastNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._patientFirstNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._patientMiddleNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._genderLabel = new System.Windows.Forms.Label();
            this._patientGender = new Extract.DataEntry.DataEntryComboBox();
            this._patientBirthDate = new Extract.DataEntry.DataEntryTextBox();
            this._birthDateLabel = new System.Windows.Forms.Label();
            this._patientRecordNum = new Extract.DataEntry.DataEntryTextBox();
            this._patientMRLabel = new System.Windows.Forms.Label();
            this._physicianInfoGroupBox = new Extract.DataEntry.DataEntryGroupBox();
            this._physicianTable = new Extract.DataEntry.DataEntryTable();
            this._physicianLastNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._physicianFirstNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._physicianMiddleNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._physicianTypeColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._physicianCode = new Extract.DataEntry.DataEntryTableColumn();
            this._testDetailsGroupBox = new System.Windows.Forms.GroupBox();
            this._orderNumberLabel = new System.Windows.Forms.Label();
            this._orderNumber = new Extract.DataEntry.DataEntryTextBox();
            this._testCommentLabel = new System.Windows.Forms.Label();
            this._testComment = new Extract.DataEntry.DataEntryTextBox();
            this._testResultTimeLabel = new System.Windows.Forms.Label();
            this._testResultTime = new Extract.DataEntry.DataEntryTextBox();
            this._resultDateLabel = new System.Windows.Forms.Label();
            this._testResultDate = new Extract.DataEntry.DataEntryTextBox();
            this._testsGroupBox = new System.Windows.Forms.GroupBox();
            this._resultStatus = new Extract.DataEntry.DataEntryComboBox();
            this._resultStatusLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._laboratoryTestTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._labAddress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._testComponentTable)).BeginInit();
            this._patientInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._patientNameTable)).BeginInit();
            this._physicianInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._physicianTable)).BeginInit();
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
            this._laboratoryIdentifier.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._laboratoryIdentifier.AutoUpdateQuery = resources.GetString("_laboratoryIdentifier.AutoUpdateQuery");
            this._laboratoryIdentifier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._laboratoryIdentifier.FormattingEnabled = true;
            this._laboratoryIdentifier.FormattingRuleFile = null;
            this._laboratoryIdentifier.Location = new System.Drawing.Point(7, 70);
            this._laboratoryIdentifier.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._laboratoryIdentifier.Name = "_laboratoryIdentifier";
            this._laboratoryIdentifier.ParentDataEntryControl = this._laboratoryTestTable;
            this._laboratoryIdentifier.Size = new System.Drawing.Size(447, 21);
            this._laboratoryIdentifier.SupportsSwiping = false;
            this._laboratoryIdentifier.TabIndex = 5;
            this._laboratoryIdentifier.TabStopRequired = true;
            this._laboratoryIdentifier.ValidationErrorMessage = "Bad value";
            this._laboratoryIdentifier.ValidationListFileName = null;
            this._laboratoryIdentifier.ValidationPattern = null;
            this._laboratoryIdentifier.ValidationQuery = "[BLANK]\r\n<SQL>SELECT LabName FROM LabAddresses ORDER BY LabName</SQL>";
            // 
            // _laboratoryTestTable
            // 
            this._laboratoryTestTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._laboratoryTestTable.AttributeName = "Test";
            this._laboratoryTestTable.CellSwipingEnabled = true;
            this._laboratoryTestTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this._laboratoryTestTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._laboratoryTestTable.ColumnHintsEnabled = true;
            this._laboratoryTestTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._testName,
            this._testID,
            this._componentDate,
            this._laboratoryTestTime});
            this._laboratoryTestTable.Location = new System.Drawing.Point(6, 19);
            this._laboratoryTestTable.MinimumNumberOfRows = 0;
            this._laboratoryTestTable.Name = "_laboratoryTestTable";
            this._laboratoryTestTable.ParentDataEntryControl = null;
            this._laboratoryTestTable.RowFormattingRuleFile = "Rules\\Swiping\\TestRow.rsd.etf";
            this._laboratoryTestTable.RowHintsEnabled = true;
            this._laboratoryTestTable.RowSwipingEnabled = true;
            this._laboratoryTestTable.Size = new System.Drawing.Size(581, 132);
            this._laboratoryTestTable.SmartHintsEnabled = true;
            this._laboratoryTestTable.TabIndex = 1;
            // 
            // _testName
            // 
            this._testName.AttributeName = "Name";
            this._testName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._testName.AutoUpdateQuery = "<SQL>SELECT Name FROM LabOrder WHERE EpicCode = \'{\'../EpicOrderCode}\'</SQL>";
            this._testName.FormattingRuleFile = null;
            this._testName.HeaderText = "Order Name";
            this._testName.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._testName.Name = "_testName";
            this._testName.TabStopRequired = true;
            this._testName.UseComboBoxCells = false;
            this._testName.ValidationErrorMessage = "Order name is not recognized.";
            this._testName.ValidationListFileName = null;
            this._testName.ValidationPattern = null;
            this._testName.ValidationQuery = "<SQL>SELECT Name FROM LabOrder WHERE EpicCode IS NOT NULL ORDER BY Name</SQL>";
            // 
            // _testID
            // 
            this._testID.AttributeName = "EpicOrderCode";
            this._testID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._testID.AutoUpdateQuery = "<SQL>SELECT EpicCode FROM LabOrder WHERE Name = \'{\'../Name}\'</SQL>";
            this._testID.FormattingRuleFile = null;
            this._testID.HeaderText = "Epic Code";
            this._testID.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._testID.Name = "_testID";
            this._testID.TabStopRequired = true;
            this._testID.UseComboBoxCells = false;
            this._testID.ValidationErrorMessage = "Order code is not recognized.";
            this._testID.ValidationListFileName = null;
            this._testID.ValidationPattern = null;
            this._testID.ValidationQuery = "<SQL>SELECT EpicCode FROM LabOrder WHERE EpicCode IS NOT NULL ORDER BY EpicCode</" +
                "SQL>";
            this._testID.Width = 80;
            // 
            // _componentDate
            // 
            this._componentDate.AttributeName = "CollectionDate";
            this._componentDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._componentDate.AutoUpdateQuery = null;
            this._componentDate.FormattingRuleFile = "Rules\\Swiping\\CollectionDate.rsd.etf";
            this._componentDate.HeaderText = "Collection Date";
            this._componentDate.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentDate.Name = "_componentDate";
            this._componentDate.TabStopRequired = true;
            this._componentDate.UseComboBoxCells = false;
            this._componentDate.ValidationErrorMessage = "Value is not allowed.";
            this._componentDate.ValidationListFileName = null;
            this._componentDate.ValidationPattern = null;
            this._componentDate.ValidationQuery = null;
            this._componentDate.Width = 105;
            // 
            // _laboratoryTestTime
            // 
            this._laboratoryTestTime.AttributeName = "CollectionTime";
            this._laboratoryTestTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._laboratoryTestTime.AutoUpdateQuery = null;
            this._laboratoryTestTime.FormattingRuleFile = "Rules\\Swiping\\CollectionTime.rsd.etf";
            this._laboratoryTestTime.HeaderText = "Collection Time";
            this._laboratoryTestTime.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._laboratoryTestTime.Name = "_laboratoryTestTime";
            this._laboratoryTestTime.TabStopRequired = true;
            this._laboratoryTestTime.UseComboBoxCells = false;
            this._laboratoryTestTime.ValidationErrorMessage = "Bad value";
            this._laboratoryTestTime.ValidationListFileName = null;
            this._laboratoryTestTime.ValidationPattern = null;
            this._laboratoryTestTime.ValidationQuery = null;
            this._laboratoryTestTime.Width = 105;
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
            this._labID.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._labID.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._labID.AutoUpdateQuery = "<SQL>SELECT LabCode FROM LabAddresses WHERE LabName = \'{\'../LabIdentifier}\'</SQL>" +
                "";
            this._labID.FormattingRuleFile = null;
            this._labID.Location = new System.Drawing.Point(457, 70);
            this._labID.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._labID.Name = "_labID";
            this._labID.ParentDataEntryControl = this._laboratoryTestTable;
            this._labID.Size = new System.Drawing.Size(117, 20);
            this._labID.SupportsSwiping = true;
            this._labID.TabIndex = 6;
            this._labID.TabStopRequired = true;
            this._labID.ValidationErrorMessage = "Unknown laboratory ID (Specify \"N/A\" if the laboratory ID is not available)";
            this._labID.ValidationListFileName = "";
            this._labID.ValidationPattern = null;
            this._labID.ValidationQuery = "<SQL>SELECT LabCode FROM LabAddresses ORDER BY LabCode</SQL>";
            // 
            // _labAddress
            // 
            this._labAddress.AttributeName = "LabAddress";
            this._labAddress.CellSwipingEnabled = true;
            this._labAddress.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this._labAddress.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._labAddress.ColumnHintsEnabled = true;
            this._labAddress.DisplayName = "Address";
            this._labAddress.Location = new System.Drawing.Point(6, 11);
            this._labAddress.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._labAddress.Name = "_labAddress";
            this._labAddress.ParentDataEntryControl = this._laboratoryTestTable;
            this._labAddress.RowHeadersWidth = 86;
            dataEntryTableRow1.AttributeName = "Address1";
            dataEntryTableRow1.AutoUpdateQuery = "<SQL>SELECT Address1 FROM LabAddresses WHERE LabCode = \'{../../LabCode}\'</SQL>";
            dataEntryTableRow1.FormattingRuleFile = null;
            dataEntryTableRow1.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            dataEntryTableRow1.Name = "Street";
            dataEntryTableRow1.TabStopRequired = true;
            dataEntryTableRow1.UseComboBoxCells = false;
            dataEntryTableRow1.ValidationErrorMessage = "Bad value";
            dataEntryTableRow1.ValidationListFileName = null;
            dataEntryTableRow1.ValidationPattern = null;
            dataEntryTableRow1.ValidationQuery = null;
            dataEntryTableRow2.AttributeName = "City";
            dataEntryTableRow2.AutoUpdateQuery = "<SQL>SELECT City FROM LabAddresses WHERE LabCode = \'{../../LabCode}\'</SQL>";
            dataEntryTableRow2.FormattingRuleFile = null;
            dataEntryTableRow2.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            dataEntryTableRow2.Name = "City";
            dataEntryTableRow2.TabStopRequired = true;
            dataEntryTableRow2.UseComboBoxCells = false;
            dataEntryTableRow2.ValidationErrorMessage = "Bad value";
            dataEntryTableRow2.ValidationListFileName = null;
            dataEntryTableRow2.ValidationPattern = null;
            dataEntryTableRow2.ValidationQuery = null;
            dataEntryTableRow3.AttributeName = "State";
            dataEntryTableRow3.AutoUpdateQuery = "<SQL>SELECT State FROM LabAddresses WHERE LabCode = \'{../../LabCode}\'</SQL>";
            dataEntryTableRow3.FormattingRuleFile = null;
            dataEntryTableRow3.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            dataEntryTableRow3.Name = "State";
            dataEntryTableRow3.TabStopRequired = true;
            dataEntryTableRow3.UseComboBoxCells = false;
            dataEntryTableRow3.ValidationErrorMessage = "";
            dataEntryTableRow3.ValidationListFileName = null;
            dataEntryTableRow3.ValidationPattern = null;
            dataEntryTableRow3.ValidationQuery = null;
            dataEntryTableRow4.AttributeName = "ZipCode";
            dataEntryTableRow4.AutoUpdateQuery = "<SQL>SELECT Zip FROM LabAddresses WHERE LabCode = \'{../../LabCode}\'</SQL>";
            dataEntryTableRow4.FormattingRuleFile = null;
            dataEntryTableRow4.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            dataEntryTableRow4.Name = "Zip";
            dataEntryTableRow4.TabStopRequired = true;
            dataEntryTableRow4.UseComboBoxCells = false;
            dataEntryTableRow4.ValidationErrorMessage = "";
            dataEntryTableRow4.ValidationListFileName = null;
            dataEntryTableRow4.ValidationPattern = null;
            dataEntryTableRow4.ValidationQuery = null;
            this._labAddress.Rows.Add(dataEntryTableRow1);
            this._labAddress.Rows.Add(dataEntryTableRow2);
            this._labAddress.Rows.Add(dataEntryTableRow3);
            this._labAddress.Rows.Add(dataEntryTableRow4);
            this._labAddress.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this._labAddress.Size = new System.Drawing.Size(64, 26);
            this._labAddress.TabIndex = 0;
            this._labAddress.TableFormattingRuleFile = null;
            this._labAddress.TableSwipingEnabled = false;
            this._labAddress.TabStop = false;
            this._labAddress.Visible = false;
            // 
            // _testComponentTable
            // 
            this._testComponentTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._testComponentTable.AttributeName = "Component";
            this._testComponentTable.CellSwipingEnabled = true;
            this._testComponentTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this._testComponentTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._testComponentTable.ColumnHintsEnabled = true;
            this._testComponentTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._componentName,
            this._testCode,
            this._componentValue,
            this._componentUnits,
            this._componentRefRange,
            this._componentFlag,
            this._componentComment});
            this._testComponentTable.Location = new System.Drawing.Point(6, 163);
            this._testComponentTable.MinimumNumberOfRows = 0;
            this._testComponentTable.Name = "_testComponentTable";
            this._testComponentTable.ParentDataEntryControl = this._laboratoryTestTable;
            this._testComponentTable.RowFormattingRuleFile = "Rules\\Swiping\\ComponentRow.rsd.etf";
            this._testComponentTable.RowHintsEnabled = true;
            this._testComponentTable.RowSwipingEnabled = true;
            this._testComponentTable.Size = new System.Drawing.Size(581, 225);
            this._testComponentTable.SmartHintsEnabled = true;
            this._testComponentTable.TabIndex = 8;
            // 
            // _componentName
            // 
            this._componentName.AttributeName = ".";
            this._componentName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._componentName.AutoUpdateQuery = "<SQL>SELECT Test.Name FROM Test INNER JOIN LabOrder on (LabOrder.code = Test.Orde" +
                "rCode) WHERE Test.Code = \'{\'TestCode}\' AND LabOrder.EpicCode = \'{\'../EpicOrderCo" +
                "de}\'</SQL>";
            this._componentName.FormattingRuleFile = null;
            this._componentName.HeaderText = "Test Name";
            this._componentName.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentName.Name = "_componentName";
            this._componentName.TabStopRequired = true;
            this._componentName.UseComboBoxCells = false;
            this._componentName.ValidationErrorMessage = "Test name is invalid for the current order";
            this._componentName.ValidationListFileName = null;
            this._componentName.ValidationPattern = null;
            this._componentName.ValidationQuery = "<SQL>SELECT Test.Name FROM Test INNER JOIN LabOrder on (LabOrder.Code = Test.Orde" +
                "rCode) WHERE LabOrder.EpicCode = \'{\'../EpicOrderCode}\' ORDER BY Test.Name</SQL>";
            // 
            // _testCode
            // 
            this._testCode.AttributeName = "TestCode";
            this._testCode.AutoUpdateQuery = "<SQL>SELECT Test.Code FROM Test INNER JOIN LabOrder on (LabOrder.code = Test.Orde" +
                "rCode) WHERE Test.Name = \'{\'..}\' AND LabOrder.EpicCode = \'{\'../../EpicOrderCode}" +
                "\'</SQL>";
            this._testCode.FormattingRuleFile = null;
            this._testCode.HeaderText = "Code";
            this._testCode.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._testCode.Name = "_testCode";
            this._testCode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._testCode.TabStopRequired = true;
            this._testCode.UseComboBoxCells = false;
            this._testCode.ValidationErrorMessage = "Test code is invalid for the current order.";
            this._testCode.ValidationListFileName = null;
            this._testCode.ValidationPattern = null;
            this._testCode.ValidationQuery = "<SQL>SELECT Test.Code FROM Test INNER JOIN LabOrder on (LabOrder.Code = Test.Orde" +
                "rCode) WHERE LabOrder.EpicCode = \'{\'../../EpicOrderCode}\' ORDER BY Test.Code</SQ" +
                "L>";
            this._testCode.Width = 60;
            // 
            // _componentValue
            // 
            this._componentValue.AttributeName = "Value";
            this._componentValue.AutoUpdateQuery = null;
            this._componentValue.FormattingRuleFile = null;
            this._componentValue.HeaderText = "Value";
            this._componentValue.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentValue.Name = "_componentValue";
            this._componentValue.TabStopRequired = true;
            this._componentValue.UseComboBoxCells = false;
            this._componentValue.ValidationErrorMessage = "Test value must be specified.";
            this._componentValue.ValidationListFileName = null;
            this._componentValue.ValidationPattern = "\\S";
            this._componentValue.ValidationQuery = null;
            this._componentValue.Width = 60;
            // 
            // _componentUnits
            // 
            this._componentUnits.AttributeName = "Units";
            this._componentUnits.AutoUpdateQuery = null;
            this._componentUnits.FormattingRuleFile = null;
            this._componentUnits.HeaderText = "Units";
            this._componentUnits.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentUnits.Name = "_componentUnits";
            this._componentUnits.TabStopRequired = true;
            this._componentUnits.UseComboBoxCells = false;
            this._componentUnits.ValidationErrorMessage = "Unrecognized unit designation.";
            this._componentUnits.ValidationListFileName = "Validation Files\\Units.txt";
            this._componentUnits.ValidationPattern = null;
            this._componentUnits.ValidationQuery = null;
            this._componentUnits.Width = 65;
            // 
            // _componentRefRange
            // 
            this._componentRefRange.AttributeName = "Range";
            this._componentRefRange.AutoUpdateQuery = null;
            this._componentRefRange.FormattingRuleFile = null;
            this._componentRefRange.HeaderText = "Ref. Range";
            this._componentRefRange.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentRefRange.Name = "_componentRefRange";
            this._componentRefRange.TabStopRequired = true;
            this._componentRefRange.UseComboBoxCells = false;
            this._componentRefRange.ValidationErrorMessage = "Value is not allowed.";
            this._componentRefRange.ValidationListFileName = null;
            this._componentRefRange.ValidationPattern = null;
            this._componentRefRange.ValidationQuery = null;
            this._componentRefRange.Width = 80;
            // 
            // _componentFlag
            // 
            this._componentFlag.AttributeName = "Flag";
            this._componentFlag.AutoUpdateQuery = null;
            this._componentFlag.FormattingRuleFile = null;
            this._componentFlag.HeaderText = "Flag";
            this._componentFlag.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentFlag.Name = "_componentFlag";
            this._componentFlag.TabStopRequired = true;
            this._componentFlag.UseComboBoxCells = true;
            this._componentFlag.ValidationErrorMessage = "Value is not allowed.";
            this._componentFlag.ValidationListFileName = "Validation Files\\Flags.txt";
            this._componentFlag.ValidationPattern = null;
            this._componentFlag.ValidationQuery = null;
            this._componentFlag.Width = 45;
            // 
            // _componentComment
            // 
            this._componentComment.AttributeName = "Comment";
            this._componentComment.AutoUpdateQuery = null;
            this._componentComment.FormattingRuleFile = null;
            this._componentComment.HeaderText = "Comment";
            this._componentComment.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentComment.Name = "_componentComment";
            this._componentComment.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._componentComment.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this._componentComment.TabStopRequired = false;
            this._componentComment.UseComboBoxCells = false;
            this._componentComment.ValidationErrorMessage = "Bad value";
            this._componentComment.ValidationListFileName = null;
            this._componentComment.ValidationPattern = null;
            this._componentComment.ValidationQuery = "";
            this._componentComment.Width = 75;
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
            this._patientInfoGroupBox.Location = new System.Drawing.Point(0, 54);
            this._patientInfoGroupBox.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._patientInfoGroupBox.Name = "_patientInfoGroupBox";
            this._patientInfoGroupBox.ParentDataEntryControl = null;
            this._patientInfoGroupBox.Size = new System.Drawing.Size(593, 111);
            this._patientInfoGroupBox.TabIndex = 2;
            this._patientInfoGroupBox.TabStop = false;
            this._patientInfoGroupBox.Text = "Patient Information";
            // 
            // _patientNameTable
            // 
            this._patientNameTable.AllowUserToAddRows = false;
            this._patientNameTable.AllowUserToDeleteRows = false;
            this._patientNameTable.AttributeName = "Name";
            this._patientNameTable.CellSwipingEnabled = true;
            this._patientNameTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this._patientNameTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._patientNameTable.ColumnHintsEnabled = true;
            this._patientNameTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._patientLastNameColumn,
            this._patientFirstNameColumn,
            this._patientMiddleNameColumn});
            this._patientNameTable.Location = new System.Drawing.Point(7, 19);
            this._patientNameTable.MinimumNumberOfRows = 1;
            this._patientNameTable.Name = "_patientNameTable";
            this._patientNameTable.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientNameTable.RowFormattingRuleFile = "Rules\\Swiping\\name.rsd.etf";
            this._patientNameTable.RowHintsEnabled = true;
            this._patientNameTable.RowSwipingEnabled = true;
            this._patientNameTable.Size = new System.Drawing.Size(569, 46);
            this._patientNameTable.SmartHintsEnabled = true;
            this._patientNameTable.TabIndex = 1;
            // 
            // _patientLastNameColumn
            // 
            this._patientLastNameColumn.AttributeName = "Last";
            this._patientLastNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._patientLastNameColumn.AutoUpdateQuery = null;
            this._patientLastNameColumn.FormattingRuleFile = null;
            this._patientLastNameColumn.HeaderText = "Last Name";
            this._patientLastNameColumn.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._patientLastNameColumn.Name = "_patientLastNameColumn";
            this._patientLastNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._patientLastNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this._patientLastNameColumn.TabStopRequired = true;
            this._patientLastNameColumn.UseComboBoxCells = false;
            this._patientLastNameColumn.ValidationErrorMessage = "Patient last name must be specified.";
            this._patientLastNameColumn.ValidationListFileName = null;
            this._patientLastNameColumn.ValidationPattern = "\\S";
            this._patientLastNameColumn.ValidationQuery = null;
            // 
            // _patientFirstNameColumn
            // 
            this._patientFirstNameColumn.AttributeName = "First";
            this._patientFirstNameColumn.AutoUpdateQuery = null;
            this._patientFirstNameColumn.FormattingRuleFile = null;
            this._patientFirstNameColumn.HeaderText = "First Name";
            this._patientFirstNameColumn.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._patientFirstNameColumn.Name = "_patientFirstNameColumn";
            this._patientFirstNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._patientFirstNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this._patientFirstNameColumn.TabStopRequired = true;
            this._patientFirstNameColumn.UseComboBoxCells = false;
            this._patientFirstNameColumn.ValidationErrorMessage = "Patient first name must be specified";
            this._patientFirstNameColumn.ValidationListFileName = null;
            this._patientFirstNameColumn.ValidationPattern = "\\S";
            this._patientFirstNameColumn.ValidationQuery = null;
            this._patientFirstNameColumn.Width = 150;
            // 
            // _patientMiddleNameColumn
            // 
            this._patientMiddleNameColumn.AttributeName = "Middle";
            this._patientMiddleNameColumn.AutoUpdateQuery = null;
            this._patientMiddleNameColumn.FormattingRuleFile = null;
            this._patientMiddleNameColumn.HeaderText = "Middle Name";
            this._patientMiddleNameColumn.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._patientMiddleNameColumn.Name = "_patientMiddleNameColumn";
            this._patientMiddleNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._patientMiddleNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this._patientMiddleNameColumn.TabStopRequired = true;
            this._patientMiddleNameColumn.UseComboBoxCells = false;
            this._patientMiddleNameColumn.ValidationErrorMessage = "Bad value";
            this._patientMiddleNameColumn.ValidationListFileName = null;
            this._patientMiddleNameColumn.ValidationPattern = null;
            this._patientMiddleNameColumn.ValidationQuery = null;
            this._patientMiddleNameColumn.Width = 150;
            // 
            // _genderLabel
            // 
            this._genderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._genderLabel.AutoSize = true;
            this._genderLabel.Location = new System.Drawing.Point(310, 68);
            this._genderLabel.Name = "_genderLabel";
            this._genderLabel.Size = new System.Drawing.Size(42, 13);
            this._genderLabel.TabIndex = 10;
            this._genderLabel.Text = "Gender";
            // 
            // _patientGender
            // 
            this._patientGender.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._patientGender.AttributeName = "Gender";
            this._patientGender.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._patientGender.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._patientGender.AutoUpdateQuery = null;
            this._patientGender.FormattingRuleFile = null;
            this._patientGender.Location = new System.Drawing.Point(313, 84);
            this._patientGender.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._patientGender.Name = "_patientGender";
            this._patientGender.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientGender.Size = new System.Drawing.Size(54, 21);
            this._patientGender.SupportsSwiping = true;
            this._patientGender.TabIndex = 3;
            this._patientGender.TabStopRequired = true;
            this._patientGender.ValidationErrorMessage = "Specify \"M\" for male, \"F\" for female or \"U\" for unknown";
            this._patientGender.ValidationListFileName = "Validation Files\\Gender.txt";
            this._patientGender.ValidationPattern = null;
            this._patientGender.ValidationQuery = null;
            // 
            // _patientBirthDate
            // 
            this._patientBirthDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._patientBirthDate.AttributeName = "DOB";
            this._patientBirthDate.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._patientBirthDate.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._patientBirthDate.AutoUpdateQuery = null;
            this._patientBirthDate.FormattingRuleFile = null;
            this._patientBirthDate.Location = new System.Drawing.Point(7, 84);
            this._patientBirthDate.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._patientBirthDate.Name = "_patientBirthDate";
            this._patientBirthDate.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientBirthDate.Size = new System.Drawing.Size(283, 20);
            this._patientBirthDate.SupportsSwiping = true;
            this._patientBirthDate.TabIndex = 2;
            this._patientBirthDate.TabStopRequired = true;
            this._patientBirthDate.ValidationErrorMessage = "Date of birth must be specified in the format MM/DD/YYYY";
            this._patientBirthDate.ValidationListFileName = null;
            this._patientBirthDate.ValidationPattern = "(^$)|(^\\d{2}/\\d{2}/\\d{4}$)";
            this._patientBirthDate.ValidationQuery = null;
            // 
            // _birthDateLabel
            // 
            this._birthDateLabel.AutoSize = true;
            this._birthDateLabel.Location = new System.Drawing.Point(5, 68);
            this._birthDateLabel.Name = "_birthDateLabel";
            this._birthDateLabel.Size = new System.Drawing.Size(66, 13);
            this._birthDateLabel.TabIndex = 0;
            this._birthDateLabel.Text = "Date of Birth";
            // 
            // _patientRecordNum
            // 
            this._patientRecordNum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._patientRecordNum.AttributeName = "MR_Number";
            this._patientRecordNum.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._patientRecordNum.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._patientRecordNum.AutoUpdateQuery = null;
            this._patientRecordNum.FormattingRuleFile = null;
            this._patientRecordNum.Location = new System.Drawing.Point(388, 84);
            this._patientRecordNum.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._patientRecordNum.Name = "_patientRecordNum";
            this._patientRecordNum.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientRecordNum.Size = new System.Drawing.Size(189, 20);
            this._patientRecordNum.SupportsSwiping = true;
            this._patientRecordNum.TabIndex = 4;
            this._patientRecordNum.TabStopRequired = true;
            this._patientRecordNum.ValidationErrorMessage = "";
            this._patientRecordNum.ValidationListFileName = null;
            this._patientRecordNum.ValidationPattern = null;
            this._patientRecordNum.ValidationQuery = null;
            // 
            // _patientMRLabel
            // 
            this._patientMRLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._patientMRLabel.AutoSize = true;
            this._patientMRLabel.Location = new System.Drawing.Point(385, 68);
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
            this._physicianInfoGroupBox.Controls.Add(this._physicianTable);
            this._physicianInfoGroupBox.Location = new System.Drawing.Point(0, 171);
            this._physicianInfoGroupBox.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._physicianInfoGroupBox.Name = "_physicianInfoGroupBox";
            this._physicianInfoGroupBox.ParentDataEntryControl = null;
            this._physicianInfoGroupBox.Size = new System.Drawing.Size(593, 117);
            this._physicianInfoGroupBox.TabIndex = 3;
            this._physicianInfoGroupBox.TabStop = false;
            this._physicianInfoGroupBox.Text = "Physicians";
            // 
            // _physicianTable
            // 
            this._physicianTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._physicianTable.AttributeName = "Name";
            this._physicianTable.CellSwipingEnabled = true;
            this._physicianTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this._physicianTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._physicianTable.ColumnHintsEnabled = false;
            this._physicianTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._physicianLastNameColumn,
            this._physicianFirstNameColumn,
            this._physicianMiddleNameColumn,
            this._physicianTypeColumn,
            this._physicianCode});
            this._physicianTable.Location = new System.Drawing.Point(6, 19);
            this._physicianTable.MinimumNumberOfRows = 0;
            this._physicianTable.Name = "_physicianTable";
            this._physicianTable.ParentDataEntryControl = this._physicianInfoGroupBox;
            this._physicianTable.RowFormattingRuleFile = "Rules\\Swiping\\name.rsd.etf";
            this._physicianTable.RowHintsEnabled = true;
            this._physicianTable.RowSwipingEnabled = true;
            this._physicianTable.Size = new System.Drawing.Size(581, 90);
            this._physicianTable.SmartHintsEnabled = false;
            this._physicianTable.TabIndex = 1;
            // 
            // _physicianLastNameColumn
            // 
            this._physicianLastNameColumn.AttributeName = "Last";
            this._physicianLastNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._physicianLastNameColumn.AutoUpdateQuery = "<SQL>SELECT  LastName FROM Physician WHERE Code = \'{\'../Code}\'</SQL>";
            this._physicianLastNameColumn.FormattingRuleFile = null;
            this._physicianLastNameColumn.HeaderText = "Last Name";
            this._physicianLastNameColumn.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._physicianLastNameColumn.Name = "_physicianLastNameColumn";
            this._physicianLastNameColumn.TabStopRequired = true;
            this._physicianLastNameColumn.UseComboBoxCells = false;
            this._physicianLastNameColumn.ValidationErrorMessage = "";
            this._physicianLastNameColumn.ValidationListFileName = null;
            this._physicianLastNameColumn.ValidationPattern = null;
            this._physicianLastNameColumn.ValidationQuery = "[BLANK]\r\n<SQL>SELECT  LastName FROM Physician ORDER BY LastName</SQL>";
            // 
            // _physicianFirstNameColumn
            // 
            this._physicianFirstNameColumn.AttributeName = "First";
            this._physicianFirstNameColumn.AutoUpdateQuery = "<SQL>SELECT FirstName FROM Physician WHERE Code = \'{\'../Code}\'</SQL>";
            this._physicianFirstNameColumn.FormattingRuleFile = null;
            this._physicianFirstNameColumn.HeaderText = "First Name";
            this._physicianFirstNameColumn.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._physicianFirstNameColumn.Name = "_physicianFirstNameColumn";
            this._physicianFirstNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._physicianFirstNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this._physicianFirstNameColumn.TabStopRequired = true;
            this._physicianFirstNameColumn.UseComboBoxCells = false;
            this._physicianFirstNameColumn.ValidationErrorMessage = "";
            this._physicianFirstNameColumn.ValidationListFileName = null;
            this._physicianFirstNameColumn.ValidationPattern = null;
            this._physicianFirstNameColumn.ValidationQuery = "[BLANK]\r\n<SQL>SELECT  FirstName FROM Physician WHERE LastName LIKE \'{\'../Last}\' O" +
                "RDER BY FirstName</SQL>";
            this._physicianFirstNameColumn.Width = 125;
            // 
            // _physicianMiddleNameColumn
            // 
            this._physicianMiddleNameColumn.AttributeName = "Middle";
            this._physicianMiddleNameColumn.AutoUpdateQuery = "<SQL>SELECT MiddleName FROM Physician WHERE Code = \'{\'../Code}\'</SQL>";
            this._physicianMiddleNameColumn.FormattingRuleFile = null;
            this._physicianMiddleNameColumn.HeaderText = "Middle Name";
            this._physicianMiddleNameColumn.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._physicianMiddleNameColumn.Name = "_physicianMiddleNameColumn";
            this._physicianMiddleNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._physicianMiddleNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this._physicianMiddleNameColumn.TabStopRequired = true;
            this._physicianMiddleNameColumn.UseComboBoxCells = false;
            this._physicianMiddleNameColumn.ValidationErrorMessage = "";
            this._physicianMiddleNameColumn.ValidationListFileName = null;
            this._physicianMiddleNameColumn.ValidationPattern = null;
            this._physicianMiddleNameColumn.ValidationQuery = "[BLANK]\r\n<SQL>SELECT  MiddleName FROM Physician WHERE LastName LIKE \'{\'../Last}\' " +
                "ORDER BY MiddleName</SQL>";
            this._physicianMiddleNameColumn.Width = 110;
            // 
            // _physicianTypeColumn
            // 
            this._physicianTypeColumn.AttributeName = "Physician_Type";
            this._physicianTypeColumn.AutoUpdateQuery = null;
            this._physicianTypeColumn.FormattingRuleFile = null;
            this._physicianTypeColumn.HeaderText = "Type";
            this._physicianTypeColumn.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._physicianTypeColumn.Name = "_physicianTypeColumn";
            this._physicianTypeColumn.TabStopRequired = true;
            this._physicianTypeColumn.UseComboBoxCells = false;
            this._physicianTypeColumn.ValidationErrorMessage = "Bad value";
            this._physicianTypeColumn.ValidationListFileName = null;
            this._physicianTypeColumn.ValidationPattern = null;
            this._physicianTypeColumn.ValidationQuery = null;
            this._physicianTypeColumn.Width = 75;
            // 
            // _physicianCode
            // 
            this._physicianCode.AttributeName = "Code";
            this._physicianCode.AutoUpdateQuery = resources.GetString("_physicianCode.AutoUpdateQuery");
            this._physicianCode.FormattingRuleFile = null;
            this._physicianCode.HeaderText = "Code";
            this._physicianCode.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._physicianCode.Name = "_physicianCode";
            this._physicianCode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._physicianCode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this._physicianCode.TabStopRequired = true;
            this._physicianCode.UseComboBoxCells = false;
            this._physicianCode.ValidationErrorMessage = "Bad value";
            this._physicianCode.ValidationListFileName = null;
            this._physicianCode.ValidationPattern = null;
            this._physicianCode.ValidationQuery = resources.GetString("_physicianCode.ValidationQuery");
            this._physicianCode.Width = 75;
            // 
            // _testDetailsGroupBox
            // 
            this._testDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._testDetailsGroupBox.Controls.Add(this._labNameLabel);
            this._testDetailsGroupBox.Controls.Add(this._orderNumberLabel);
            this._testDetailsGroupBox.Controls.Add(this._orderNumber);
            this._testDetailsGroupBox.Controls.Add(this._laboratoryIdentifier);
            this._testDetailsGroupBox.Controls.Add(this._labIDLabel);
            this._testDetailsGroupBox.Controls.Add(this._testCommentLabel);
            this._testDetailsGroupBox.Controls.Add(this._testComment);
            this._testDetailsGroupBox.Controls.Add(this._labID);
            this._testDetailsGroupBox.Controls.Add(this._testResultTimeLabel);
            this._testDetailsGroupBox.Controls.Add(this._testResultTime);
            this._testDetailsGroupBox.Controls.Add(this._resultDateLabel);
            this._testDetailsGroupBox.Controls.Add(this._testResultDate);
            this._testDetailsGroupBox.Controls.Add(this._testComponentTable);
            this._testDetailsGroupBox.Location = new System.Drawing.Point(0, 459);
            this._testDetailsGroupBox.Name = "_testDetailsGroupBox";
            this._testDetailsGroupBox.Size = new System.Drawing.Size(593, 393);
            this._testDetailsGroupBox.TabIndex = 5;
            this._testDetailsGroupBox.TabStop = false;
            this._testDetailsGroupBox.Text = "Selected Order Details";
            // 
            // _orderNumberLabel
            // 
            this._orderNumberLabel.AutoSize = true;
            this._orderNumberLabel.Location = new System.Drawing.Point(4, 16);
            this._orderNumberLabel.Name = "_orderNumberLabel";
            this._orderNumberLabel.Size = new System.Drawing.Size(73, 13);
            this._orderNumberLabel.TabIndex = 9;
            this._orderNumberLabel.Text = "Order Number";
            // 
            // _orderNumber
            // 
            this._orderNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._orderNumber.AttributeName = "OrderNumber";
            this._orderNumber.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._orderNumber.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._orderNumber.AutoUpdateQuery = null;
            this._orderNumber.FormattingRuleFile = "Rules\\Swiping\\ResultDate.rsd.etf";
            this._orderNumber.Location = new System.Drawing.Point(7, 32);
            this._orderNumber.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._orderNumber.Name = "_orderNumber";
            this._orderNumber.ParentDataEntryControl = this._laboratoryTestTable;
            this._orderNumber.Size = new System.Drawing.Size(197, 20);
            this._orderNumber.SupportsSwiping = true;
            this._orderNumber.TabIndex = 2;
            this._orderNumber.TabStopRequired = true;
            this._orderNumber.ValidationErrorMessage = "Order Number must be specified";
            this._orderNumber.ValidationListFileName = null;
            this._orderNumber.ValidationPattern = "\\S";
            this._orderNumber.ValidationQuery = null;
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
            this._testComment.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._testComment.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._testComment.AutoUpdateQuery = "";
            this._testComment.FormattingRuleFile = null;
            this._testComment.Location = new System.Drawing.Point(6, 110);
            this._testComment.Multiline = true;
            this._testComment.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._testComment.Name = "_testComment";
            this._testComment.ParentDataEntryControl = this._laboratoryTestTable;
            this._testComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._testComment.Size = new System.Drawing.Size(569, 47);
            this._testComment.SupportsSwiping = true;
            this._testComment.TabIndex = 7;
            this._testComment.TabStopRequired = false;
            this._testComment.ValidationErrorMessage = "";
            this._testComment.ValidationListFileName = null;
            this._testComment.ValidationPattern = null;
            this._testComment.ValidationQuery = null;
            // 
            // _testResultTimeLabel
            // 
            this._testResultTimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._testResultTimeLabel.AutoSize = true;
            this._testResultTimeLabel.Location = new System.Drawing.Point(418, 16);
            this._testResultTimeLabel.Name = "_testResultTimeLabel";
            this._testResultTimeLabel.Size = new System.Drawing.Size(63, 13);
            this._testResultTimeLabel.TabIndex = 0;
            this._testResultTimeLabel.Text = "Result Time";
            // 
            // _testResultTime
            // 
            this._testResultTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._testResultTime.AttributeName = "ResultTime";
            this._testResultTime.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._testResultTime.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._testResultTime.AutoUpdateQuery = null;
            this._testResultTime.FormattingRuleFile = "Rules\\Swiping\\ResultTime.rsd.etf";
            this._testResultTime.Location = new System.Drawing.Point(421, 32);
            this._testResultTime.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._testResultTime.Name = "_testResultTime";
            this._testResultTime.ParentDataEntryControl = this._laboratoryTestTable;
            this._testResultTime.Size = new System.Drawing.Size(153, 20);
            this._testResultTime.SupportsSwiping = true;
            this._testResultTime.TabIndex = 4;
            this._testResultTime.TabStopRequired = true;
            this._testResultTime.ValidationErrorMessage = "";
            this._testResultTime.ValidationListFileName = null;
            this._testResultTime.ValidationPattern = null;
            this._testResultTime.ValidationQuery = null;
            // 
            // _resultDateLabel
            // 
            this._resultDateLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._resultDateLabel.AutoSize = true;
            this._resultDateLabel.Location = new System.Drawing.Point(224, 16);
            this._resultDateLabel.Name = "_resultDateLabel";
            this._resultDateLabel.Size = new System.Drawing.Size(63, 13);
            this._resultDateLabel.TabIndex = 0;
            this._resultDateLabel.Text = "Result Date";
            // 
            // _testResultDate
            // 
            this._testResultDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._testResultDate.AttributeName = "ResultDate";
            this._testResultDate.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._testResultDate.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._testResultDate.AutoUpdateQuery = null;
            this._testResultDate.FormattingRuleFile = "Rules\\Swiping\\ResultDate.rsd.etf";
            this._testResultDate.Location = new System.Drawing.Point(227, 32);
            this._testResultDate.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._testResultDate.Name = "_testResultDate";
            this._testResultDate.ParentDataEntryControl = this._laboratoryTestTable;
            this._testResultDate.Size = new System.Drawing.Size(171, 20);
            this._testResultDate.SupportsSwiping = true;
            this._testResultDate.TabIndex = 3;
            this._testResultDate.TabStopRequired = true;
            this._testResultDate.ValidationErrorMessage = "";
            this._testResultDate.ValidationListFileName = null;
            this._testResultDate.ValidationPattern = null;
            this._testResultDate.ValidationQuery = null;
            // 
            // _testsGroupBox
            // 
            this._testsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._testsGroupBox.Controls.Add(this._laboratoryTestTable);
            this._testsGroupBox.Location = new System.Drawing.Point(0, 294);
            this._testsGroupBox.Name = "_testsGroupBox";
            this._testsGroupBox.Size = new System.Drawing.Size(593, 159);
            this._testsGroupBox.TabIndex = 5;
            this._testsGroupBox.TabStop = false;
            this._testsGroupBox.Text = "Orders";
            // 
            // _resultStatus
            // 
            this._resultStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._resultStatus.AttributeName = "ResultStatus";
            this._resultStatus.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._resultStatus.AutoUpdateQuery = null;
            this._resultStatus.DisplayMember = "Original";
            this._resultStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._resultStatus.FormattingRuleFile = null;
            this._resultStatus.Items.AddRange(new object[] {
            "Original",
            "Reviewed"});
            this._resultStatus.Location = new System.Drawing.Point(387, 27);
            this._resultStatus.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._resultStatus.Name = "_resultStatus";
            this._resultStatus.ParentDataEntryControl = null;
            this._resultStatus.Size = new System.Drawing.Size(189, 21);
            this._resultStatus.SupportsSwiping = false;
            this._resultStatus.TabIndex = 1;
            this._resultStatus.TabStopRequired = true;
            this._resultStatus.ValidationErrorMessage = "";
            this._resultStatus.ValidationListFileName = "";
            this._resultStatus.ValidationPattern = null;
            this._resultStatus.ValidationQuery = null;
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
            // StandardLabDEPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this._labAddress);
            this.Controls.Add(this._resultStatusLabel);
            this.Controls.Add(this._resultStatus);
            this.Controls.Add(this._testsGroupBox);
            this.Controls.Add(this._testDetailsGroupBox);
            this.Controls.Add(this._physicianInfoGroupBox);
            this.Controls.Add(this._patientInfoGroupBox);
            highlightColor1.Color = System.Drawing.Color.LightSalmon;
            highlightColor1.MaxOcrConfidence = 89;
            highlightColor2.Color = System.Drawing.Color.LightGreen;
            highlightColor2.MaxOcrConfidence = 100;
            this.HighlightColors = new Extract.DataEntry.HighlightColor[] {
        highlightColor1,
        highlightColor2};
            this.Name = "StandardLabDEPanel";
            this.Size = new System.Drawing.Size(593, 854);
            ((System.ComponentModel.ISupportInitialize)(this._laboratoryTestTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._labAddress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._testComponentTable)).EndInit();
            this._patientInfoGroupBox.ResumeLayout(false);
            this._patientInfoGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._patientNameTable)).EndInit();
            this._physicianInfoGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._physicianTable)).EndInit();
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
        private Extract.DataEntry.DataEntryTwoColumnTable _labAddress;
        private Extract.DataEntry.DataEntryGroupBox _physicianInfoGroupBox;
        private System.Windows.Forms.GroupBox _testDetailsGroupBox;
        private Extract.DataEntry.DataEntryTextBox _testResultDate;
        private System.Windows.Forms.Label _testResultTimeLabel;
        private Extract.DataEntry.DataEntryTextBox _testResultTime;
        private System.Windows.Forms.Label _resultDateLabel;
        private System.Windows.Forms.GroupBox _testsGroupBox;
        private Extract.DataEntry.DataEntryTable _physicianTable;
        private System.Windows.Forms.Label _testCommentLabel;
        private Extract.DataEntry.DataEntryTextBox _testComment;
        private Extract.DataEntry.DataEntryComboBox _resultStatus;
        private System.Windows.Forms.Label _resultStatusLabel;
        private Extract.DataEntry.DataEntryComboBox _laboratoryIdentifier;
        private Extract.DataEntry.DataEntryTextBox _orderNumber;
        private System.Windows.Forms.Label _orderNumberLabel;
        private Extract.DataEntry.DataEntryTableColumn _componentName;
        private Extract.DataEntry.DataEntryTableColumn _testCode;
        private Extract.DataEntry.DataEntryTableColumn _componentValue;
        private Extract.DataEntry.DataEntryTableColumn _componentUnits;
        private Extract.DataEntry.DataEntryTableColumn _componentRefRange;
        private Extract.DataEntry.DataEntryTableColumn _componentFlag;
        private Extract.DataEntry.DataEntryTableColumn _componentComment;
        private System.Windows.Forms.Label _labNameLabel;
        private Extract.DataEntry.DataEntryTableColumn _testName;
        private Extract.DataEntry.DataEntryTableColumn _testID;
        private Extract.DataEntry.DataEntryTableColumn _componentDate;
        private Extract.DataEntry.DataEntryTableColumn _laboratoryTestTime;
        private Extract.DataEntry.DataEntryTable _patientNameTable;
        private Extract.DataEntry.DataEntryTableColumn _patientLastNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _patientFirstNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _patientMiddleNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _physicianLastNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _physicianFirstNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _physicianMiddleNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _physicianTypeColumn;
        private Extract.DataEntry.DataEntryTableColumn _physicianCode;
    }
}
