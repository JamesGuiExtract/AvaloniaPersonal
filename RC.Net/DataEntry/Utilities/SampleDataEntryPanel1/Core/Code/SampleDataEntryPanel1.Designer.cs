namespace Extract.DataEntry
{
    partial class SampleDataEntryPanel1
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow1 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow2 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow3 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow4 = new Extract.DataEntry.DataEntryTableRow();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this._laboratoryGroupBox = new System.Windows.Forms.GroupBox();
            this._labAddress = new Extract.DataEntry.DataEntryTwoColumnTable();
            this._labInfo = new Extract.DataEntry.DataEntryTextBox();
            this._labPhoneLabel = new System.Windows.Forms.Label();
            this._labPhoneNumber = new Extract.DataEntry.DataEntryTextBox();
            this._labName = new Extract.DataEntry.DataEntryTextBox();
            this._labIDLabel = new System.Windows.Forms.Label();
            this._labID = new Extract.DataEntry.DataEntryTextBox();
            this._patientInfo = new Extract.DataEntry.DataEntryTextBox();
            this._laboratoryTestTable = new Extract.DataEntry.DataEntryTable();
            this._testName = new Extract.DataEntry.DataEntryTableColumn();
            this._componentDate = new Extract.DataEntry.DataEntryTableColumn();
            this._laboratoryTestTime = new Extract.DataEntry.DataEntryTableColumn();
            this._testComponentTable = new Extract.DataEntry.DataEntryTable();
            this._componentName = new Extract.DataEntry.DataEntryTableColumn();
            this._componentValue = new Extract.DataEntry.DataEntryTableColumn();
            this._componentUnits = new Extract.DataEntry.DataEntryTableColumn();
            this._componentRefRange = new Extract.DataEntry.DataEntryTableColumn();
            this._componentFlag = new Extract.DataEntry.DataEntryTableColumn();
            this._componentComment = new Extract.DataEntry.DataEntryTableColumn();
            this._patientInfoGroupBox = new System.Windows.Forms.GroupBox();
            this._patientName = new Extract.DataEntry.DataEntryTextBox();
            this._patientNameLabel = new System.Windows.Forms.Label();
            this._genderLabel = new System.Windows.Forms.Label();
            this._patientGender = new Extract.DataEntry.DataEntryTextBox();
            this._patientBirthDate = new Extract.DataEntry.DataEntryTextBox();
            this._birthDateLabel = new System.Windows.Forms.Label();
            this._patientRecordNum = new Extract.DataEntry.DataEntryTextBox();
            this._patientMRLabel = new System.Windows.Forms.Label();
            this._physicianInfoGroupBox = new System.Windows.Forms.GroupBox();
            this._physicianTable = new Extract.DataEntry.DataEntryTable();
            this._physicianNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._physicianTypeColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._physicianInfo = new Extract.DataEntry.DataEntryTextBox();
            this._testDetailsGroupBox = new System.Windows.Forms.GroupBox();
            this._testCommentLabel = new System.Windows.Forms.Label();
            this._testComment = new Extract.DataEntry.DataEntryTextBox();
            this._testOrderNumberLabel = new System.Windows.Forms.Label();
            this._testOrderNumber = new Extract.DataEntry.DataEntryTextBox();
            this._orderCodeLabel = new System.Windows.Forms.Label();
            this._testOrderCode = new Extract.DataEntry.DataEntryTextBox();
            this._testResultTimeLabel = new System.Windows.Forms.Label();
            this._testResultTime = new Extract.DataEntry.DataEntryTextBox();
            this._resultDateLabel = new System.Windows.Forms.Label();
            this._testResultDate = new Extract.DataEntry.DataEntryTextBox();
            this._testsGroupBox = new System.Windows.Forms.GroupBox();
            this._laboratoryGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._labAddress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._laboratoryTestTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._testComponentTable)).BeginInit();
            this._patientInfoGroupBox.SuspendLayout();
            this._physicianInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._physicianTable)).BeginInit();
            this._testDetailsGroupBox.SuspendLayout();
            this._testsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _laboratoryGroupBox
            // 
            this._laboratoryGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._laboratoryGroupBox.Controls.Add(this._labAddress);
            this._laboratoryGroupBox.Controls.Add(this._labPhoneLabel);
            this._laboratoryGroupBox.Controls.Add(this._labPhoneNumber);
            this._laboratoryGroupBox.Controls.Add(this._labName);
            this._laboratoryGroupBox.Controls.Add(this._labIDLabel);
            this._laboratoryGroupBox.Controls.Add(this._labInfo);
            this._laboratoryGroupBox.Controls.Add(this._labID);
            this._laboratoryGroupBox.Location = new System.Drawing.Point(0, 233);
            this._laboratoryGroupBox.Name = "_laboratoryGroupBox";
            this._laboratoryGroupBox.Size = new System.Drawing.Size(521, 163);
            this._laboratoryGroupBox.TabIndex = 3;
            this._laboratoryGroupBox.TabStop = false;
            this._laboratoryGroupBox.Text = "Laboratory";
            // 
            // _labAddress
            // 
            this._labAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._labAddress.AttributeName = "Address";
            this._labAddress.CellSwipingEnabled = true;
            this._labAddress.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._labAddress.ColumnHintsEnabled = true;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.LightGray;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._labAddress.DefaultCellStyle = dataGridViewCellStyle1;
            this._labAddress.DisplayName = "Address";
            this._labAddress.Location = new System.Drawing.Point(7, 46);
            this._labAddress.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._labAddress.Name = "_labAddress";
            this._labAddress.ParentDataEntryControl = this._labInfo;
            this._labAddress.RowHeadersWidth = 86;
            dataEntryTableRow1.AttributeName = "Address1";
            dataEntryTableRow1.FormattingRuleFile = null;
            dataEntryTableRow1.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            dataEntryTableRow1.Name = "Street";
            dataEntryTableRow1.ValidationErrorMessage = "Bad value";
            dataEntryTableRow1.ValidationListFileName = null;
            dataEntryTableRow1.ValidationPattern = null;
            dataEntryTableRow2.AttributeName = "City";
            dataEntryTableRow2.FormattingRuleFile = null;
            dataEntryTableRow2.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            dataEntryTableRow2.Name = "City";
            dataEntryTableRow2.ValidationErrorMessage = "Bad value";
            dataEntryTableRow2.ValidationListFileName = null;
            dataEntryTableRow2.ValidationPattern = null;
            dataEntryTableRow3.AttributeName = "State";
            dataEntryTableRow3.FormattingRuleFile = null;
            dataEntryTableRow3.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            dataEntryTableRow3.Name = "State";
            dataEntryTableRow3.ValidationErrorMessage = "Specify the name or 2 letter abbreviation for a valid state";
            dataEntryTableRow3.ValidationListFileName = "C:\\Demo-LabDE\\Validation Files\\States.txt";
            dataEntryTableRow3.ValidationPattern = null;
            dataEntryTableRow4.AttributeName = "ZipCode";
            dataEntryTableRow4.FormattingRuleFile = null;
            dataEntryTableRow4.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            dataEntryTableRow4.Name = "Zip";
            dataEntryTableRow4.ValidationErrorMessage = "Must specify a zip code in \\\"XXXXX\\\" or \\\"XXXXX-XXXX\\\" format";
            dataEntryTableRow4.ValidationListFileName = null;
            dataEntryTableRow4.ValidationPattern = "(^$)|(^(\\d{5}|\\d{5}[-\\s]\\d{4})$)";
            this._labAddress.Rows.Add(dataEntryTableRow1);
            this._labAddress.Rows.Add(dataEntryTableRow2);
            this._labAddress.Rows.Add(dataEntryTableRow3);
            this._labAddress.Rows.Add(dataEntryTableRow4);
            this._labAddress.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this._labAddress.Size = new System.Drawing.Size(369, 112);
            this._labAddress.TabIndex = 2;
            this._labAddress.TableFormattingRuleFile = "C:\\Demo-LabDE\\Rules\\Swiping\\AddressSplitter.rsd.etf";
            this._labAddress.TableSwipingEnabled = true;
            // 
            // _labInfo
            // 
            this._labInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._labInfo.AttributeName = "LabInfo";
            this._labInfo.FormattingRuleFile = null;
            this._labInfo.Location = new System.Drawing.Point(100, 0);
            this._labInfo.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._labInfo.Name = "_labInfo";
            this._labInfo.ParentDataEntryControl = null;
            this._labInfo.Size = new System.Drawing.Size(13, 20);
            this._labInfo.SupportsSwiping = true;
            this._labInfo.TabIndex = 0;
            this._labInfo.TabStop = false;
            this._labInfo.ValidationErrorMessage = "";
            this._labInfo.ValidationListFileName = null;
            this._labInfo.ValidationPattern = null;
            this._labInfo.Visible = false;
            // 
            // _labPhoneLabel
            // 
            this._labPhoneLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._labPhoneLabel.AutoSize = true;
            this._labPhoneLabel.Location = new System.Drawing.Point(382, 42);
            this._labPhoneLabel.Name = "_labPhoneLabel";
            this._labPhoneLabel.Size = new System.Drawing.Size(38, 13);
            this._labPhoneLabel.TabIndex = 0;
            this._labPhoneLabel.Text = "Phone";
            // 
            // _labPhoneNumber
            // 
            this._labPhoneNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._labPhoneNumber.AttributeName = "Phone_Number";
            this._labPhoneNumber.FormattingRuleFile = null;
            this._labPhoneNumber.Location = new System.Drawing.Point(384, 58);
            this._labPhoneNumber.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._labPhoneNumber.Name = "_labPhoneNumber";
            this._labPhoneNumber.ParentDataEntryControl = this._labInfo;
            this._labPhoneNumber.Size = new System.Drawing.Size(118, 20);
            this._labPhoneNumber.SupportsSwiping = true;
            this._labPhoneNumber.TabIndex = 3;
            this._labPhoneNumber.ValidationErrorMessage = "The laboratory phone number is not in a valid format.";
            this._labPhoneNumber.ValidationListFileName = null;
            this._labPhoneNumber.ValidationPattern = "(^$)|(^(1[\\-\\s])?(\\(?[\\d]{3}\\s?[\\)\\-]\\s?)?\\d{3}\\s?[\\-\\s]\\s?\\d{4}$)";
            // 
            // _labName
            // 
            this._labName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._labName.AttributeName = "Name";
            this._labName.FormattingRuleFile = null;
            this._labName.Location = new System.Drawing.Point(6, 19);
            this._labName.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._labName.Name = "_labName";
            this._labName.ParentDataEntryControl = this._labInfo;
            this._labName.Size = new System.Drawing.Size(496, 20);
            this._labName.SupportsSwiping = true;
            this._labName.TabIndex = 1;
            this._labName.ValidationErrorMessage = "The laboratory name must be specified";
            this._labName.ValidationListFileName = null;
            this._labName.ValidationPattern = "\\S";
            // 
            // _labIDLabel
            // 
            this._labIDLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._labIDLabel.AutoSize = true;
            this._labIDLabel.Location = new System.Drawing.Point(382, 81);
            this._labIDLabel.Name = "_labIDLabel";
            this._labIDLabel.Size = new System.Drawing.Size(39, 13);
            this._labIDLabel.TabIndex = 0;
            this._labIDLabel.Text = "Lab ID";
            // 
            // _labID
            // 
            this._labID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._labID.AttributeName = "Lab_Number";
            this._labID.FormattingRuleFile = null;
            this._labID.Location = new System.Drawing.Point(384, 97);
            this._labID.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._labID.Name = "_labID";
            this._labID.ParentDataEntryControl = this._labInfo;
            this._labID.Size = new System.Drawing.Size(118, 20);
            this._labID.SupportsSwiping = true;
            this._labID.TabIndex = 4;
            this._labID.ValidationErrorMessage = "Unknown laboratory ID (Specify \"N/A\" if the laboratory ID is not available)";
            this._labID.ValidationListFileName = "";
            this._labID.ValidationPattern = null;
            // 
            // _patientInfo
            // 
            this._patientInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._patientInfo.AttributeName = "PatientInfo";
            this._patientInfo.FormattingRuleFile = null;
            this._patientInfo.Location = new System.Drawing.Point(134, -3);
            this._patientInfo.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._patientInfo.Name = "_patientInfo";
            this._patientInfo.ParentDataEntryControl = null;
            this._patientInfo.Size = new System.Drawing.Size(12, 20);
            this._patientInfo.SupportsSwiping = true;
            this._patientInfo.TabIndex = 0;
            this._patientInfo.TabStop = false;
            this._patientInfo.ValidationErrorMessage = "";
            this._patientInfo.ValidationListFileName = null;
            this._patientInfo.ValidationPattern = null;
            this._patientInfo.Visible = false;
            // 
            // _laboratoryTestTable
            // 
            this._laboratoryTestTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._laboratoryTestTable.AttributeName = "Test";
            this._laboratoryTestTable.CellSwipingEnabled = true;
            this._laboratoryTestTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._laboratoryTestTable.ColumnHintsEnabled = true;
            this._laboratoryTestTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._testName,
            this._componentDate,
            this._laboratoryTestTime});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.LightGray;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._laboratoryTestTable.DefaultCellStyle = dataGridViewCellStyle2;
            this._laboratoryTestTable.Location = new System.Drawing.Point(6, 19);
            this._laboratoryTestTable.Name = "_laboratoryTestTable";
            this._laboratoryTestTable.ParentDataEntryControl = null;
            this._laboratoryTestTable.RowFormattingRuleFile = "C:\\Demo-LabDE\\Rules\\Swiping\\TestRow.rsd.etf";
            this._laboratoryTestTable.RowHintsEnabled = true;
            this._laboratoryTestTable.RowSwipingEnabled = true;
            this._laboratoryTestTable.Size = new System.Drawing.Size(509, 132);
            this._laboratoryTestTable.SmartHintsEnabled = true;
            this._laboratoryTestTable.TabIndex = 1;
            // 
            // _testName
            // 
            this._testName.AttributeName = "Name";
            this._testName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._testName.FormattingRuleFile = null;
            this._testName.HeaderText = "Laboratory Test Results";
            this._testName.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._testName.Name = "_testName";
            this._testName.ValidationErrorMessage = "Value is not allowed.";
            this._testName.ValidationListFileName = null;
            this._testName.ValidationPattern = null;
            // 
            // _componentDate
            // 
            this._componentDate.AttributeName = "CollectionDate";
            this._componentDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._componentDate.FormattingRuleFile = "C:\\Demo-LabDE\\Rules\\Swiping\\CollectionDate.rsd.etf";
            this._componentDate.HeaderText = "Collection Date";
            this._componentDate.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentDate.Name = "_componentDate";
            this._componentDate.ValidationErrorMessage = "Value is not allowed.";
            this._componentDate.ValidationListFileName = null;
            this._componentDate.ValidationPattern = null;
            // 
            // _laboratoryTestTime
            // 
            this._laboratoryTestTime.AttributeName = "CollectionTime";
            this._laboratoryTestTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._laboratoryTestTime.FormattingRuleFile = "C:\\Demo-LabDE\\Rules\\Swiping\\CollectionTime.rsd.etf";
            this._laboratoryTestTime.HeaderText = "Collection Time";
            this._laboratoryTestTime.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._laboratoryTestTime.Name = "_laboratoryTestTime";
            this._laboratoryTestTime.ValidationErrorMessage = "Bad value";
            this._laboratoryTestTime.ValidationListFileName = null;
            this._laboratoryTestTime.ValidationPattern = null;
            // 
            // _testComponentTable
            // 
            this._testComponentTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._testComponentTable.AttributeName = "Component";
            this._testComponentTable.CellSwipingEnabled = true;
            this._testComponentTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._testComponentTable.ColumnHintsEnabled = true;
            this._testComponentTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._componentName,
            this._componentValue,
            this._componentUnits,
            this._componentRefRange,
            this._componentFlag,
            this._componentComment});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.LightGray;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._testComponentTable.DefaultCellStyle = dataGridViewCellStyle3;
            this._testComponentTable.Location = new System.Drawing.Point(6, 163);
            this._testComponentTable.Name = "_testComponentTable";
            this._testComponentTable.ParentDataEntryControl = this._laboratoryTestTable;
            this._testComponentTable.RowFormattingRuleFile = "C:\\Demo-LabDE\\Rules\\Swiping\\ComponentRow.rsd.etf";
            this._testComponentTable.RowHintsEnabled = true;
            this._testComponentTable.RowSwipingEnabled = true;
            this._testComponentTable.Size = new System.Drawing.Size(509, 225);
            this._testComponentTable.SmartHintsEnabled = true;
            this._testComponentTable.TabIndex = 5;
            // 
            // _componentName
            // 
            this._componentName.AttributeName = ".";
            this._componentName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._componentName.FormattingRuleFile = null;
            this._componentName.HeaderText = "Component Name";
            this._componentName.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentName.Name = "_componentName";
            this._componentName.ValidationErrorMessage = "Test component name must be specified.";
            this._componentName.ValidationListFileName = null;
            this._componentName.ValidationPattern = "\\S";
            // 
            // _componentValue
            // 
            this._componentValue.AttributeName = "Value";
            this._componentValue.FormattingRuleFile = null;
            this._componentValue.HeaderText = "Value";
            this._componentValue.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentValue.Name = "_componentValue";
            this._componentValue.ValidationErrorMessage = "Test component value must be specified.";
            this._componentValue.ValidationListFileName = null;
            this._componentValue.ValidationPattern = "\\S";
            this._componentValue.Width = 60;
            // 
            // _componentUnits
            // 
            this._componentUnits.AttributeName = "Units";
            this._componentUnits.FormattingRuleFile = null;
            this._componentUnits.HeaderText = "Units";
            this._componentUnits.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentUnits.Name = "_componentUnits";
            this._componentUnits.ValidationErrorMessage = "Unrecognized unit designation.";
            this._componentUnits.ValidationListFileName = "C:\\Demo-LabDE\\Validation Files\\Units.txt";
            this._componentUnits.ValidationPattern = null;
            this._componentUnits.Width = 75;
            // 
            // _componentRefRange
            // 
            this._componentRefRange.AttributeName = "Range";
            this._componentRefRange.FormattingRuleFile = null;
            this._componentRefRange.HeaderText = "Ref. Range";
            this._componentRefRange.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentRefRange.Name = "_componentRefRange";
            this._componentRefRange.ValidationErrorMessage = "Value is not allowed.";
            this._componentRefRange.ValidationListFileName = null;
            this._componentRefRange.ValidationPattern = null;
            this._componentRefRange.Width = 80;
            // 
            // _componentFlag
            // 
            this._componentFlag.AttributeName = "Flag";
            this._componentFlag.FormattingRuleFile = null;
            this._componentFlag.HeaderText = "Flag";
            this._componentFlag.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentFlag.Name = "_componentFlag";
            this._componentFlag.ValidationErrorMessage = "Value is not allowed.";
            this._componentFlag.ValidationListFileName = null;
            this._componentFlag.ValidationPattern = null;
            this._componentFlag.Width = 40;
            // 
            // _componentComment
            // 
            this._componentComment.AttributeName = "Comment";
            this._componentComment.FormattingRuleFile = null;
            this._componentComment.HeaderText = "Comment";
            this._componentComment.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._componentComment.Name = "_componentComment";
            this._componentComment.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._componentComment.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this._componentComment.ValidationErrorMessage = "Bad value";
            this._componentComment.ValidationListFileName = null;
            this._componentComment.ValidationPattern = null;
            this._componentComment.Width = 75;
            // 
            // _patientInfoGroupBox
            // 
            this._patientInfoGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._patientInfoGroupBox.Controls.Add(this._patientName);
            this._patientInfoGroupBox.Controls.Add(this._patientNameLabel);
            this._patientInfoGroupBox.Controls.Add(this._genderLabel);
            this._patientInfoGroupBox.Controls.Add(this._patientGender);
            this._patientInfoGroupBox.Controls.Add(this._patientBirthDate);
            this._patientInfoGroupBox.Controls.Add(this._birthDateLabel);
            this._patientInfoGroupBox.Controls.Add(this._patientRecordNum);
            this._patientInfoGroupBox.Controls.Add(this._patientMRLabel);
            this._patientInfoGroupBox.Controls.Add(this._patientInfo);
            this._patientInfoGroupBox.Location = new System.Drawing.Point(0, 3);
            this._patientInfoGroupBox.Name = "_patientInfoGroupBox";
            this._patientInfoGroupBox.Size = new System.Drawing.Size(521, 100);
            this._patientInfoGroupBox.TabIndex = 1;
            this._patientInfoGroupBox.TabStop = false;
            this._patientInfoGroupBox.Text = "Patient Information";
            // 
            // _patientName
            // 
            this._patientName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._patientName.AttributeName = "Name";
            this._patientName.FormattingRuleFile = null;
            this._patientName.Location = new System.Drawing.Point(7, 33);
            this._patientName.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._patientName.Name = "_patientName";
            this._patientName.ParentDataEntryControl = this._patientInfo;
            this._patientName.Size = new System.Drawing.Size(287, 20);
            this._patientName.SupportsSwiping = true;
            this._patientName.TabIndex = 1;
            this._patientName.ValidationErrorMessage = "Full patient name must be specified";
            this._patientName.ValidationListFileName = null;
            this._patientName.ValidationPattern = "\\S[,\\s]+\\S";
            // 
            // _patientNameLabel
            // 
            this._patientNameLabel.AutoSize = true;
            this._patientNameLabel.Location = new System.Drawing.Point(6, 17);
            this._patientNameLabel.Name = "_patientNameLabel";
            this._patientNameLabel.Size = new System.Drawing.Size(35, 13);
            this._patientNameLabel.TabIndex = 11;
            this._patientNameLabel.Text = "Name";
            // 
            // _genderLabel
            // 
            this._genderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._genderLabel.AutoSize = true;
            this._genderLabel.Location = new System.Drawing.Point(237, 56);
            this._genderLabel.Name = "_genderLabel";
            this._genderLabel.Size = new System.Drawing.Size(42, 13);
            this._genderLabel.TabIndex = 10;
            this._genderLabel.Text = "Gender";
            // 
            // _patientGender
            // 
            this._patientGender.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._patientGender.AttributeName = "Gender";
            this._patientGender.FormattingRuleFile = null;
            this._patientGender.Location = new System.Drawing.Point(240, 72);
            this._patientGender.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._patientGender.Name = "_patientGender";
            this._patientGender.ParentDataEntryControl = this._patientInfo;
            this._patientGender.Size = new System.Drawing.Size(54, 20);
            this._patientGender.SupportsSwiping = true;
            this._patientGender.TabIndex = 4;
            this._patientGender.ValidationErrorMessage = "Specify \"M\" for Male or \"F\" for Female";
            this._patientGender.ValidationListFileName = null;
            this._patientGender.ValidationPattern = "^(()|M|F)$";
            // 
            // _patientBirthDate
            // 
            this._patientBirthDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._patientBirthDate.AttributeName = "DOB";
            this._patientBirthDate.FormattingRuleFile = null;
            this._patientBirthDate.Location = new System.Drawing.Point(6, 72);
            this._patientBirthDate.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._patientBirthDate.Name = "_patientBirthDate";
            this._patientBirthDate.ParentDataEntryControl = this._patientInfo;
            this._patientBirthDate.Size = new System.Drawing.Size(211, 20);
            this._patientBirthDate.SupportsSwiping = true;
            this._patientBirthDate.TabIndex = 3;
            this._patientBirthDate.ValidationErrorMessage = "Date of birth must be specified in the format MM/DD/YYYY";
            this._patientBirthDate.ValidationListFileName = null;
            this._patientBirthDate.ValidationPattern = "(^$)|(^\\d{2}/\\d{2}/\\d{4}$)";
            // 
            // _birthDateLabel
            // 
            this._birthDateLabel.AutoSize = true;
            this._birthDateLabel.Location = new System.Drawing.Point(4, 56);
            this._birthDateLabel.Name = "_birthDateLabel";
            this._birthDateLabel.Size = new System.Drawing.Size(66, 13);
            this._birthDateLabel.TabIndex = 0;
            this._birthDateLabel.Text = "Date of Birth";
            // 
            // _patientRecordNum
            // 
            this._patientRecordNum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._patientRecordNum.AttributeName = "MR_Number";
            this._patientRecordNum.FormattingRuleFile = null;
            this._patientRecordNum.Location = new System.Drawing.Point(315, 33);
            this._patientRecordNum.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._patientRecordNum.Name = "_patientRecordNum";
            this._patientRecordNum.ParentDataEntryControl = this._patientInfo;
            this._patientRecordNum.Size = new System.Drawing.Size(188, 20);
            this._patientRecordNum.SupportsSwiping = true;
            this._patientRecordNum.TabIndex = 2;
            this._patientRecordNum.ValidationErrorMessage = "";
            this._patientRecordNum.ValidationListFileName = null;
            this._patientRecordNum.ValidationPattern = null;
            // 
            // _patientMRLabel
            // 
            this._patientMRLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._patientMRLabel.AutoSize = true;
            this._patientMRLabel.Location = new System.Drawing.Point(312, 17);
            this._patientMRLabel.Name = "_patientMRLabel";
            this._patientMRLabel.Size = new System.Drawing.Size(92, 13);
            this._patientMRLabel.TabIndex = 0;
            this._patientMRLabel.Text = "Medical Record #";
            // 
            // _physicianInfoGroupBox
            // 
            this._physicianInfoGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._physicianInfoGroupBox.Controls.Add(this._physicianTable);
            this._physicianInfoGroupBox.Controls.Add(this._physicianInfo);
            this._physicianInfoGroupBox.Location = new System.Drawing.Point(0, 110);
            this._physicianInfoGroupBox.Name = "_physicianInfoGroupBox";
            this._physicianInfoGroupBox.Size = new System.Drawing.Size(521, 117);
            this._physicianInfoGroupBox.TabIndex = 2;
            this._physicianInfoGroupBox.TabStop = false;
            this._physicianInfoGroupBox.Text = "Physicians";
            // 
            // _physicianTable
            // 
            this._physicianTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._physicianTable.AttributeName = "Physician_Name";
            this._physicianTable.CellSwipingEnabled = true;
            this._physicianTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._physicianTable.ColumnHintsEnabled = false;
            this._physicianTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._physicianNameColumn,
            this._physicianTypeColumn});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.LightGray;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._physicianTable.DefaultCellStyle = dataGridViewCellStyle4;
            this._physicianTable.Location = new System.Drawing.Point(6, 19);
            this._physicianTable.Name = "_physicianTable";
            this._physicianTable.ParentDataEntryControl = this._physicianInfo;
            this._physicianTable.RowFormattingRuleFile = "";
            this._physicianTable.RowHintsEnabled = true;
            this._physicianTable.RowSwipingEnabled = false;
            this._physicianTable.Size = new System.Drawing.Size(509, 90);
            this._physicianTable.SmartHintsEnabled = false;
            this._physicianTable.TabIndex = 2;
            // 
            // _physicianNameColumn
            // 
            this._physicianNameColumn.AttributeName = ".";
            this._physicianNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._physicianNameColumn.FormattingRuleFile = null;
            this._physicianNameColumn.HeaderText = "Name";
            this._physicianNameColumn.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._physicianNameColumn.Name = "_physicianNameColumn";
            this._physicianNameColumn.ValidationErrorMessage = "Value is not allowed.";
            this._physicianNameColumn.ValidationListFileName = null;
            this._physicianNameColumn.ValidationPattern = null;
            // 
            // _physicianTypeColumn
            // 
            this._physicianTypeColumn.AttributeName = "Physician_Type";
            this._physicianTypeColumn.FormattingRuleFile = null;
            this._physicianTypeColumn.HeaderText = "Type";
            this._physicianTypeColumn.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._physicianTypeColumn.Name = "_physicianTypeColumn";
            this._physicianTypeColumn.ValidationErrorMessage = "Bad value";
            this._physicianTypeColumn.ValidationListFileName = null;
            this._physicianTypeColumn.ValidationPattern = null;
            this._physicianTypeColumn.Width = 150;
            // 
            // _physicianInfo
            // 
            this._physicianInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._physicianInfo.AttributeName = "PhysicianInfo";
            this._physicianInfo.FormattingRuleFile = null;
            this._physicianInfo.Location = new System.Drawing.Point(102, -7);
            this._physicianInfo.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._physicianInfo.Name = "_physicianInfo";
            this._physicianInfo.ParentDataEntryControl = null;
            this._physicianInfo.Size = new System.Drawing.Size(11, 20);
            this._physicianInfo.SupportsSwiping = true;
            this._physicianInfo.TabIndex = 6;
            this._physicianInfo.TabStop = false;
            this._physicianInfo.ValidationErrorMessage = "";
            this._physicianInfo.ValidationListFileName = null;
            this._physicianInfo.ValidationPattern = null;
            this._physicianInfo.Visible = false;
            // 
            // _testDetailsGroupBox
            // 
            this._testDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._testDetailsGroupBox.Controls.Add(this._testCommentLabel);
            this._testDetailsGroupBox.Controls.Add(this._testComment);
            this._testDetailsGroupBox.Controls.Add(this._testOrderNumberLabel);
            this._testDetailsGroupBox.Controls.Add(this._testOrderNumber);
            this._testDetailsGroupBox.Controls.Add(this._orderCodeLabel);
            this._testDetailsGroupBox.Controls.Add(this._testOrderCode);
            this._testDetailsGroupBox.Controls.Add(this._testResultTimeLabel);
            this._testDetailsGroupBox.Controls.Add(this._testResultTime);
            this._testDetailsGroupBox.Controls.Add(this._resultDateLabel);
            this._testDetailsGroupBox.Controls.Add(this._testResultDate);
            this._testDetailsGroupBox.Controls.Add(this._testComponentTable);
            this._testDetailsGroupBox.Location = new System.Drawing.Point(0, 567);
            this._testDetailsGroupBox.Name = "_testDetailsGroupBox";
            this._testDetailsGroupBox.Size = new System.Drawing.Size(521, 395);
            this._testDetailsGroupBox.TabIndex = 5;
            this._testDetailsGroupBox.TabStop = false;
            this._testDetailsGroupBox.Text = "Selected Test Details";
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
            this._testComment.FormattingRuleFile = null;
            this._testComment.Location = new System.Drawing.Point(6, 110);
            this._testComment.Multiline = true;
            this._testComment.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._testComment.Name = "_testComment";
            this._testComment.ParentDataEntryControl = this._laboratoryTestTable;
            this._testComment.Size = new System.Drawing.Size(497, 47);
            this._testComment.SupportsSwiping = true;
            this._testComment.TabIndex = 6;
            this._testComment.ValidationErrorMessage = "";
            this._testComment.ValidationListFileName = null;
            this._testComment.ValidationPattern = null;
            // 
            // _testOrderNumberLabel
            // 
            this._testOrderNumberLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._testOrderNumberLabel.AutoSize = true;
            this._testOrderNumberLabel.Location = new System.Drawing.Point(312, 55);
            this._testOrderNumberLabel.Name = "_testOrderNumberLabel";
            this._testOrderNumberLabel.Size = new System.Drawing.Size(73, 13);
            this._testOrderNumberLabel.TabIndex = 0;
            this._testOrderNumberLabel.Text = "Order Number";
            // 
            // _testOrderNumber
            // 
            this._testOrderNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._testOrderNumber.AttributeName = "OrderNumber";
            this._testOrderNumber.FormattingRuleFile = null;
            this._testOrderNumber.Location = new System.Drawing.Point(314, 71);
            this._testOrderNumber.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._testOrderNumber.Name = "_testOrderNumber";
            this._testOrderNumber.ParentDataEntryControl = this._laboratoryTestTable;
            this._testOrderNumber.Size = new System.Drawing.Size(188, 20);
            this._testOrderNumber.SupportsSwiping = true;
            this._testOrderNumber.TabIndex = 4;
            this._testOrderNumber.ValidationErrorMessage = "";
            this._testOrderNumber.ValidationListFileName = null;
            this._testOrderNumber.ValidationPattern = null;
            // 
            // _orderCodeLabel
            // 
            this._orderCodeLabel.AutoSize = true;
            this._orderCodeLabel.Location = new System.Drawing.Point(6, 55);
            this._orderCodeLabel.Name = "_orderCodeLabel";
            this._orderCodeLabel.Size = new System.Drawing.Size(61, 13);
            this._orderCodeLabel.TabIndex = 0;
            this._orderCodeLabel.Text = "Order Code";
            // 
            // _testOrderCode
            // 
            this._testOrderCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._testOrderCode.AttributeName = "OrderCode";
            this._testOrderCode.FormattingRuleFile = null;
            this._testOrderCode.Location = new System.Drawing.Point(6, 71);
            this._testOrderCode.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._testOrderCode.Name = "_testOrderCode";
            this._testOrderCode.ParentDataEntryControl = this._laboratoryTestTable;
            this._testOrderCode.Size = new System.Drawing.Size(288, 20);
            this._testOrderCode.SupportsSwiping = true;
            this._testOrderCode.TabIndex = 3;
            this._testOrderCode.ValidationErrorMessage = "";
            this._testOrderCode.ValidationListFileName = null;
            this._testOrderCode.ValidationPattern = null;
            // 
            // _testResultTimeLabel
            // 
            this._testResultTimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._testResultTimeLabel.AutoSize = true;
            this._testResultTimeLabel.Location = new System.Drawing.Point(312, 16);
            this._testResultTimeLabel.Name = "_testResultTimeLabel";
            this._testResultTimeLabel.Size = new System.Drawing.Size(63, 13);
            this._testResultTimeLabel.TabIndex = 0;
            this._testResultTimeLabel.Text = "Result Time";
            // 
            // _testResultTime
            // 
            this._testResultTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._testResultTime.AttributeName = "ResultTime";
            this._testResultTime.FormattingRuleFile = "C:\\Demo-LabDE\\Rules\\Swiping\\ResultTime.rsd.etf";
            this._testResultTime.Location = new System.Drawing.Point(314, 32);
            this._testResultTime.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._testResultTime.Name = "_testResultTime";
            this._testResultTime.ParentDataEntryControl = this._laboratoryTestTable;
            this._testResultTime.Size = new System.Drawing.Size(188, 20);
            this._testResultTime.SupportsSwiping = true;
            this._testResultTime.TabIndex = 2;
            this._testResultTime.ValidationErrorMessage = "";
            this._testResultTime.ValidationListFileName = null;
            this._testResultTime.ValidationPattern = null;
            // 
            // _resultDateLabel
            // 
            this._resultDateLabel.AutoSize = true;
            this._resultDateLabel.Location = new System.Drawing.Point(6, 16);
            this._resultDateLabel.Name = "_resultDateLabel";
            this._resultDateLabel.Size = new System.Drawing.Size(63, 13);
            this._resultDateLabel.TabIndex = 0;
            this._resultDateLabel.Text = "Result Date";
            // 
            // _testResultDate
            // 
            this._testResultDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._testResultDate.AttributeName = "ResultDate";
            this._testResultDate.FormattingRuleFile = "C:\\Demo-LabDE\\Rules\\Swiping\\ResultDate.rsd.etf";
            this._testResultDate.Location = new System.Drawing.Point(6, 32);
            this._testResultDate.MultipleMatchSelectionMode = Extract.DataEntry.MultipleMatchSelectionMode.First;
            this._testResultDate.Name = "_testResultDate";
            this._testResultDate.ParentDataEntryControl = this._laboratoryTestTable;
            this._testResultDate.Size = new System.Drawing.Size(288, 20);
            this._testResultDate.SupportsSwiping = true;
            this._testResultDate.TabIndex = 1;
            this._testResultDate.ValidationErrorMessage = "";
            this._testResultDate.ValidationListFileName = null;
            this._testResultDate.ValidationPattern = null;
            // 
            // _testsGroupBox
            // 
            this._testsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._testsGroupBox.Controls.Add(this._laboratoryTestTable);
            this._testsGroupBox.Location = new System.Drawing.Point(0, 402);
            this._testsGroupBox.Name = "_testsGroupBox";
            this._testsGroupBox.Size = new System.Drawing.Size(521, 159);
            this._testsGroupBox.TabIndex = 4;
            this._testsGroupBox.TabStop = false;
            this._testsGroupBox.Text = "Completed Tests";
            // 
            // SampleDataEntryPanel1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this._testsGroupBox);
            this.Controls.Add(this._testDetailsGroupBox);
            this.Controls.Add(this._physicianInfoGroupBox);
            this.Controls.Add(this._patientInfoGroupBox);
            this.Controls.Add(this._laboratoryGroupBox);
            this.Name = "SampleDataEntryPanel1";
            this.Size = new System.Drawing.Size(521, 963);
            this._laboratoryGroupBox.ResumeLayout(false);
            this._laboratoryGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._labAddress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._laboratoryTestTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._testComponentTable)).EndInit();
            this._patientInfoGroupBox.ResumeLayout(false);
            this._patientInfoGroupBox.PerformLayout();
            this._physicianInfoGroupBox.ResumeLayout(false);
            this._physicianInfoGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._physicianTable)).EndInit();
            this._testDetailsGroupBox.ResumeLayout(false);
            this._testDetailsGroupBox.PerformLayout();
            this._testsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox _laboratoryGroupBox;
        private DataEntryTable _laboratoryTestTable;
        private DataEntryTable _testComponentTable;
        private DataEntryTextBox _labName;
        private System.Windows.Forms.Label _labIDLabel;
        private DataEntryTextBox _labID;
        private System.Windows.Forms.Label _labPhoneLabel;
        private DataEntryTextBox _labPhoneNumber;
        private System.Windows.Forms.GroupBox _patientInfoGroupBox;
        private DataEntryTextBox _patientInfo;
        private DataEntryTextBox _labInfo;
        private System.Windows.Forms.Label _patientMRLabel;
        private DataEntryTextBox _patientRecordNum;
        private DataEntryTextBox _patientBirthDate;
        private System.Windows.Forms.Label _birthDateLabel;
        private System.Windows.Forms.Label _genderLabel;
        private DataEntryTextBox _patientGender;
        private DataEntryTextBox _patientName;
        private System.Windows.Forms.Label _patientNameLabel;
        private DataEntryTwoColumnTable _labAddress;
        private System.Windows.Forms.GroupBox _physicianInfoGroupBox;
        private DataEntryTextBox _physicianInfo;
        private System.Windows.Forms.GroupBox _testDetailsGroupBox;
        private DataEntryTextBox _testResultDate;
        private System.Windows.Forms.Label _testResultTimeLabel;
        private DataEntryTextBox _testResultTime;
        private System.Windows.Forms.Label _resultDateLabel;
        private System.Windows.Forms.Label _testOrderNumberLabel;
        private DataEntryTextBox _testOrderNumber;
        private System.Windows.Forms.Label _orderCodeLabel;
        private DataEntryTextBox _testOrderCode;
        private System.Windows.Forms.GroupBox _testsGroupBox;
        private DataEntryTableColumn _testName;
        private DataEntryTableColumn _componentDate;
        private DataEntryTableColumn _laboratoryTestTime;
        private DataEntryTable _physicianTable;
        private DataEntryTableColumn _physicianNameColumn;
        private DataEntryTableColumn _physicianTypeColumn;
        private System.Windows.Forms.Label _testCommentLabel;
        private DataEntryTextBox _testComment;
        private DataEntryTableColumn _componentName;
        private DataEntryTableColumn _componentValue;
        private DataEntryTableColumn _componentUnits;
        private DataEntryTableColumn _componentRefRange;
        private DataEntryTableColumn _componentFlag;
        private DataEntryTableColumn _componentComment;
    }
}
