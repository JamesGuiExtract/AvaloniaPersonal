namespace StandardMedDE
{
    partial class StandardMedDEPanel
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
            Extract.DataEntry.HighlightColor highlightColor1 = new Extract.DataEntry.HighlightColor();
            Extract.DataEntry.HighlightColor highlightColor2 = new Extract.DataEntry.HighlightColor();
            this._patientInfoGroupBox = new Extract.DataEntry.DataEntryGroupBox();
            this._patientRecordLabel = new System.Windows.Forms.Label();
            this._patientDOBLabel = new System.Windows.Forms.Label();
            this._patientRecordTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._patientDOBTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._patientNameTable = new Extract.DataEntry.DataEntryTable();
            this._patientLastName = new Extract.DataEntry.DataEntryTableColumn();
            this._patientFirstName = new Extract.DataEntry.DataEntryTableColumn();
            this._patientMiddleName = new Extract.DataEntry.DataEntryTableColumn();
            this._physicianInfoGroupBox = new Extract.DataEntry.DataEntryGroupBox();
            this._physicianLocationLabel = new System.Windows.Forms.Label();
            this._physicianLocationTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._physicianNameTable = new Extract.DataEntry.DataEntryTable();
            this._physicianLastNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._physicianFirstNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._physicianMiddleNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._vitalsGroupBox = new Extract.DataEntry.DataEntryGroupBox();
            this._vitalsRespTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._vitalsRespLabel = new System.Windows.Forms.Label();
            this._vitalsPulseTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._vitalsPulseLabel = new System.Windows.Forms.Label();
            this._vitalsWeightTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._vitalWeightLabel = new System.Windows.Forms.Label();
            this._vitalsTempTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._vitalsTempLabel = new System.Windows.Forms.Label();
            this._vitalsBPLabel = new System.Windows.Forms.Label();
            this._vitalsBPDiastolicTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._vitalsBPTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._vitalsBPSlashLabel = new System.Windows.Forms.Label();
            this._vitalsBPSystolicTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._vitalsTimeLabel = new System.Windows.Forms.Label();
            this._vitalsTimeTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._vitalsDateLabel = new System.Windows.Forms.Label();
            this._vitalsDateTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._visitDetailsGroupBox = new Extract.DataEntry.DataEntryGroupBox();
            this._visitDetailsVisitNotesLabel = new System.Windows.Forms.Label();
            this._visitDetailsVisitNotesTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._visitDetailsReasonForVisitLabel = new System.Windows.Forms.Label();
            this._visitDetailsReasonForVisitTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._visitDetailsLocation = new System.Windows.Forms.Label();
            this._visitDetailLocationTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._visitDetailsProviderNameLabel = new System.Windows.Forms.Label();
            this._visitDetailsProviderNameTable = new Extract.DataEntry.DataEntryTable();
            this._providerLastNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._providerFirstNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._providerMiddleNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._visitDetailsTimeLabel = new System.Windows.Forms.Label();
            this._visitDetailsDateTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._visitDetailsTimeTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._visitDetailsDateLabel = new System.Windows.Forms.Label();
            this._allergiesGroupBox = new System.Windows.Forms.GroupBox();
            this._allergiesTable = new Extract.DataEntry.DataEntryTable();
            this._allergyNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._allergyDateColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._allergyReactionsColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._patientInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._patientNameTable)).BeginInit();
            this._physicianInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._physicianNameTable)).BeginInit();
            this._vitalsGroupBox.SuspendLayout();
            this._visitDetailsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._visitDetailsProviderNameTable)).BeginInit();
            this._allergiesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._allergiesTable)).BeginInit();
            this.SuspendLayout();
            // 
            // _patientInfoGroupBox
            // 
            this._patientInfoGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._patientInfoGroupBox.AttributeName = "PatientInfo";
            this._patientInfoGroupBox.Controls.Add(this._patientRecordLabel);
            this._patientInfoGroupBox.Controls.Add(this._patientDOBLabel);
            this._patientInfoGroupBox.Controls.Add(this._patientRecordTextBox);
            this._patientInfoGroupBox.Controls.Add(this._patientDOBTextBox);
            this._patientInfoGroupBox.Controls.Add(this._patientNameTable);
            this._patientInfoGroupBox.Location = new System.Drawing.Point(3, 3);
            this._patientInfoGroupBox.Name = "_patientInfoGroupBox";
            this._patientInfoGroupBox.ParentDataEntryControl = null;
            this._patientInfoGroupBox.Size = new System.Drawing.Size(451, 114);
            this._patientInfoGroupBox.TabIndex = 1;
            this._patientInfoGroupBox.TabStop = false;
            this._patientInfoGroupBox.Text = "Patient Info";
            // 
            // _patientRecordLabel
            // 
            this._patientRecordLabel.AutoSize = true;
            this._patientRecordLabel.Location = new System.Drawing.Point(112, 70);
            this._patientRecordLabel.Name = "_patientRecordLabel";
            this._patientRecordLabel.Size = new System.Drawing.Size(42, 13);
            this._patientRecordLabel.TabIndex = 4;
            this._patientRecordLabel.Text = "Record";
            // 
            // _patientDOBLabel
            // 
            this._patientDOBLabel.AutoSize = true;
            this._patientDOBLabel.Location = new System.Drawing.Point(7, 70);
            this._patientDOBLabel.Name = "_patientDOBLabel";
            this._patientDOBLabel.Size = new System.Drawing.Size(30, 13);
            this._patientDOBLabel.TabIndex = 3;
            this._patientDOBLabel.Text = "DOB";
            // 
            // _patientRecordTextBox
            // 
            this._patientRecordTextBox.AttributeName = "Record";
            this._patientRecordTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._patientRecordTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._patientRecordTextBox.Location = new System.Drawing.Point(115, 86);
            this._patientRecordTextBox.Name = "_patientRecordTextBox";
            this._patientRecordTextBox.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientRecordTextBox.Size = new System.Drawing.Size(139, 20);
            this._patientRecordTextBox.TabIndex = 3;
            this._patientRecordTextBox.ValidationErrorMessage = "Invalid or missing record number";
            this._patientRecordTextBox.ValidationPattern = "^\\d+\\s?$";
            // 
            // _patientDOBTextBox
            // 
            this._patientDOBTextBox.AttributeName = "DOB";
            this._patientDOBTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._patientDOBTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._patientDOBTextBox.FormattingRuleFile = "Rules\\Swiping\\DOB.rsd.etf";
            this._patientDOBTextBox.Location = new System.Drawing.Point(7, 86);
            this._patientDOBTextBox.Name = "_patientDOBTextBox";
            this._patientDOBTextBox.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientDOBTextBox.Size = new System.Drawing.Size(89, 20);
            this._patientDOBTextBox.TabIndex = 2;
            this._patientDOBTextBox.ValidationErrorMessage = "Date of birth must be a valid date in the format MM/DD/YYYY";
            this._patientDOBTextBox.ValidationPattern = "(^$)|(^((0?[1-9])|(1[0-2]))/((0?[1-9])|(1[0-9])|(2[0-9])|(3[01]))/(18|19|20)\\d{2}" +
                "$)";
            // 
            // _patientNameTable
            // 
            this._patientNameTable.AllowTabbingByRow = true;
            this._patientNameTable.AllowUserToAddRows = false;
            this._patientNameTable.AllowUserToDeleteRows = false;
            this._patientNameTable.AllowUserToResizeRows = false;
            this._patientNameTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._patientNameTable.AttributeName = "Name";
            this._patientNameTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._patientNameTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this._patientNameTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._patientNameTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._patientLastName,
            this._patientFirstName,
            this._patientMiddleName});
            this._patientNameTable.Location = new System.Drawing.Point(7, 20);
            this._patientNameTable.Name = "_patientNameTable";
            this._patientNameTable.ParentDataEntryControl = this._patientInfoGroupBox;
            this._patientNameTable.RowFormattingRuleFile = "Rules\\Swiping\\Name.rsd.etf";
            this._patientNameTable.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._patientNameTable.RowSwipingEnabled = true;
            this._patientNameTable.Size = new System.Drawing.Size(438, 47);
            this._patientNameTable.TabIndex = 1;
            // 
            // _patientLastName
            // 
            this._patientLastName.AttributeName = "Last";
            this._patientLastName.HeaderText = "Last";
            this._patientLastName.Name = "_patientLastName";
            this._patientLastName.ValidationErrorMessage = "Invalid value";
            // 
            // _patientFirstName
            // 
            this._patientFirstName.AttributeName = "First";
            this._patientFirstName.HeaderText = "First";
            this._patientFirstName.Name = "_patientFirstName";
            this._patientFirstName.ValidationErrorMessage = "Invalid value";
            // 
            // _patientMiddleName
            // 
            this._patientMiddleName.AttributeName = "Middle";
            this._patientMiddleName.FillWeight = 50F;
            this._patientMiddleName.HeaderText = "Middle";
            this._patientMiddleName.Name = "_patientMiddleName";
            this._patientMiddleName.ValidationErrorMessage = "Invalid value";
            // 
            // _physicianInfoGroupBox
            // 
            this._physicianInfoGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._physicianInfoGroupBox.AttributeName = "PhysicianInfo";
            this._physicianInfoGroupBox.Controls.Add(this._physicianLocationLabel);
            this._physicianInfoGroupBox.Controls.Add(this._physicianLocationTextBox);
            this._physicianInfoGroupBox.Controls.Add(this._physicianNameTable);
            this._physicianInfoGroupBox.Location = new System.Drawing.Point(4, 123);
            this._physicianInfoGroupBox.Name = "_physicianInfoGroupBox";
            this._physicianInfoGroupBox.ParentDataEntryControl = null;
            this._physicianInfoGroupBox.Size = new System.Drawing.Size(450, 113);
            this._physicianInfoGroupBox.TabIndex = 2;
            this._physicianInfoGroupBox.TabStop = false;
            this._physicianInfoGroupBox.Text = "Physician Info";
            // 
            // _physicianLocationLabel
            // 
            this._physicianLocationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._physicianLocationLabel.AutoSize = true;
            this._physicianLocationLabel.Location = new System.Drawing.Point(6, 69);
            this._physicianLocationLabel.Name = "_physicianLocationLabel";
            this._physicianLocationLabel.Size = new System.Drawing.Size(48, 13);
            this._physicianLocationLabel.TabIndex = 5;
            this._physicianLocationLabel.Text = "Location";
            // 
            // _physicianLocationTextBox
            // 
            this._physicianLocationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._physicianLocationTextBox.AttributeName = "Location";
            this._physicianLocationTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._physicianLocationTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._physicianLocationTextBox.Location = new System.Drawing.Point(6, 85);
            this._physicianLocationTextBox.Name = "_physicianLocationTextBox";
            this._physicianLocationTextBox.ParentDataEntryControl = this._physicianInfoGroupBox;
            this._physicianLocationTextBox.Size = new System.Drawing.Size(438, 20);
            this._physicianLocationTextBox.TabIndex = 2;
            this._physicianLocationTextBox.ValidationErrorMessage = "Invalid value";
            // 
            // _physicianNameTable
            // 
            this._physicianNameTable.AllowTabbingByRow = true;
            this._physicianNameTable.AllowUserToAddRows = false;
            this._physicianNameTable.AllowUserToDeleteRows = false;
            this._physicianNameTable.AllowUserToResizeRows = false;
            this._physicianNameTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._physicianNameTable.AttributeName = "Name";
            this._physicianNameTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._physicianNameTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this._physicianNameTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._physicianNameTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._physicianLastNameColumn,
            this._physicianFirstNameColumn,
            this._physicianMiddleNameColumn});
            this._physicianNameTable.Location = new System.Drawing.Point(6, 19);
            this._physicianNameTable.Name = "_physicianNameTable";
            this._physicianNameTable.ParentDataEntryControl = this._physicianInfoGroupBox;
            this._physicianNameTable.RowFormattingRuleFile = "Rules\\Swiping\\Name.rsd.etf";
            this._physicianNameTable.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._physicianNameTable.RowSwipingEnabled = true;
            this._physicianNameTable.Size = new System.Drawing.Size(438, 47);
            this._physicianNameTable.TabIndex = 1;
            // 
            // _physicianLastNameColumn
            // 
            this._physicianLastNameColumn.AttributeName = "Last";
            this._physicianLastNameColumn.HeaderText = "Last";
            this._physicianLastNameColumn.Name = "_physicianLastNameColumn";
            this._physicianLastNameColumn.ValidationErrorMessage = "Invalid value";
            // 
            // _physicianFirstNameColumn
            // 
            this._physicianFirstNameColumn.AttributeName = "First";
            this._physicianFirstNameColumn.HeaderText = "First";
            this._physicianFirstNameColumn.Name = "_physicianFirstNameColumn";
            this._physicianFirstNameColumn.ValidationErrorMessage = "Invalid value";
            // 
            // _physicianMiddleNameColumn
            // 
            this._physicianMiddleNameColumn.AttributeName = "Middle";
            this._physicianMiddleNameColumn.FillWeight = 50F;
            this._physicianMiddleNameColumn.HeaderText = "Middle";
            this._physicianMiddleNameColumn.Name = "_physicianMiddleNameColumn";
            this._physicianMiddleNameColumn.ValidationErrorMessage = "Invalid value";
            // 
            // _vitalsGroupBox
            // 
            this._vitalsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._vitalsGroupBox.AttributeName = "Vitals";
            this._vitalsGroupBox.Controls.Add(this._vitalsRespTextBox);
            this._vitalsGroupBox.Controls.Add(this._vitalsRespLabel);
            this._vitalsGroupBox.Controls.Add(this._vitalsPulseTextBox);
            this._vitalsGroupBox.Controls.Add(this._vitalsPulseLabel);
            this._vitalsGroupBox.Controls.Add(this._vitalsWeightTextBox);
            this._vitalsGroupBox.Controls.Add(this._vitalWeightLabel);
            this._vitalsGroupBox.Controls.Add(this._vitalsTempTextBox);
            this._vitalsGroupBox.Controls.Add(this._vitalsTempLabel);
            this._vitalsGroupBox.Controls.Add(this._vitalsBPLabel);
            this._vitalsGroupBox.Controls.Add(this._vitalsBPDiastolicTextBox);
            this._vitalsGroupBox.Controls.Add(this._vitalsBPSlashLabel);
            this._vitalsGroupBox.Controls.Add(this._vitalsBPSystolicTextBox);
            this._vitalsGroupBox.Controls.Add(this._vitalsBPTextBox);
            this._vitalsGroupBox.Controls.Add(this._vitalsTimeLabel);
            this._vitalsGroupBox.Controls.Add(this._vitalsTimeTextBox);
            this._vitalsGroupBox.Controls.Add(this._vitalsDateLabel);
            this._vitalsGroupBox.Controls.Add(this._vitalsDateTextBox);
            this._vitalsGroupBox.Location = new System.Drawing.Point(4, 242);
            this._vitalsGroupBox.Name = "_vitalsGroupBox";
            this._vitalsGroupBox.ParentDataEntryControl = null;
            this._vitalsGroupBox.Size = new System.Drawing.Size(450, 137);
            this._vitalsGroupBox.TabIndex = 3;
            this._vitalsGroupBox.TabStop = false;
            this._vitalsGroupBox.Text = "Vitals";
            // 
            // _vitalsRespTextBox
            // 
            this._vitalsRespTextBox.AttributeName = "Resp";
            this._vitalsRespTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._vitalsRespTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._vitalsRespTextBox.Location = new System.Drawing.Point(323, 108);
            this._vitalsRespTextBox.Name = "_vitalsRespTextBox";
            this._vitalsRespTextBox.ParentDataEntryControl = this._vitalsGroupBox;
            this._vitalsRespTextBox.Size = new System.Drawing.Size(89, 20);
            this._vitalsRespTextBox.TabIndex = 8;
            this._vitalsRespTextBox.ValidationErrorMessage = "Pulse must be a positive number";
            this._vitalsRespTextBox.ValidationPattern = "^$|^\\d+\\s?$";
            // 
            // _vitalsRespLabel
            // 
            this._vitalsRespLabel.AutoSize = true;
            this._vitalsRespLabel.Location = new System.Drawing.Point(224, 113);
            this._vitalsRespLabel.Name = "_vitalsRespLabel";
            this._vitalsRespLabel.Size = new System.Drawing.Size(32, 13);
            this._vitalsRespLabel.TabIndex = 19;
            this._vitalsRespLabel.Text = "Resp";
            // 
            // _vitalsPulseTextBox
            // 
            this._vitalsPulseTextBox.AttributeName = "Pulse";
            this._vitalsPulseTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._vitalsPulseTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._vitalsPulseTextBox.Location = new System.Drawing.Point(114, 110);
            this._vitalsPulseTextBox.Name = "_vitalsPulseTextBox";
            this._vitalsPulseTextBox.ParentDataEntryControl = this._vitalsGroupBox;
            this._vitalsPulseTextBox.Size = new System.Drawing.Size(89, 20);
            this._vitalsPulseTextBox.TabIndex = 7;
            this._vitalsPulseTextBox.ValidationErrorMessage = "Pulse must be a positive number";
            this._vitalsPulseTextBox.ValidationPattern = "^$|^\\d+\\s?$";
            // 
            // _vitalsPulseLabel
            // 
            this._vitalsPulseLabel.AutoSize = true;
            this._vitalsPulseLabel.Location = new System.Drawing.Point(6, 113);
            this._vitalsPulseLabel.Name = "_vitalsPulseLabel";
            this._vitalsPulseLabel.Size = new System.Drawing.Size(33, 13);
            this._vitalsPulseLabel.TabIndex = 17;
            this._vitalsPulseLabel.Text = "Pulse";
            // 
            // _vitalsWeightTextBox
            // 
            this._vitalsWeightTextBox.AttributeName = "Weight";
            this._vitalsWeightTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._vitalsWeightTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._vitalsWeightTextBox.Location = new System.Drawing.Point(323, 81);
            this._vitalsWeightTextBox.Name = "_vitalsWeightTextBox";
            this._vitalsWeightTextBox.ParentDataEntryControl = this._vitalsGroupBox;
            this._vitalsWeightTextBox.Size = new System.Drawing.Size(89, 20);
            this._vitalsWeightTextBox.TabIndex = 6;
            this._vitalsWeightTextBox.ValidationErrorMessage = "Weight must be a positive number";
            this._vitalsWeightTextBox.ValidationPattern = "^$|^\\d+\\s?$";
            // 
            // _vitalWeightLabel
            // 
            this._vitalWeightLabel.AutoSize = true;
            this._vitalWeightLabel.Location = new System.Drawing.Point(224, 84);
            this._vitalWeightLabel.Name = "_vitalWeightLabel";
            this._vitalWeightLabel.Size = new System.Drawing.Size(41, 13);
            this._vitalWeightLabel.TabIndex = 15;
            this._vitalWeightLabel.Text = "Weight";
            // 
            // _vitalsTempTextBox
            // 
            this._vitalsTempTextBox.AttributeName = "Temp";
            this._vitalsTempTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._vitalsTempTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._vitalsTempTextBox.Location = new System.Drawing.Point(114, 83);
            this._vitalsTempTextBox.Name = "_vitalsTempTextBox";
            this._vitalsTempTextBox.ParentDataEntryControl = this._vitalsGroupBox;
            this._vitalsTempTextBox.Size = new System.Drawing.Size(89, 20);
            this._vitalsTempTextBox.TabIndex = 5;
            this._vitalsTempTextBox.ValidationErrorMessage = "Body temperature must be a positive number";
            this._vitalsTempTextBox.ValidationPattern = "^$|^[\\d\\.]+\\s?$";
            // 
            // _vitalsTempLabel
            // 
            this._vitalsTempLabel.AutoSize = true;
            this._vitalsTempLabel.Location = new System.Drawing.Point(6, 86);
            this._vitalsTempLabel.Name = "_vitalsTempLabel";
            this._vitalsTempLabel.Size = new System.Drawing.Size(34, 13);
            this._vitalsTempLabel.TabIndex = 13;
            this._vitalsTempLabel.Text = "Temp";
            // 
            // _vitalsBPLabel
            // 
            this._vitalsBPLabel.AutoSize = true;
            this._vitalsBPLabel.Location = new System.Drawing.Point(6, 57);
            this._vitalsBPLabel.Name = "_vitalsBPLabel";
            this._vitalsBPLabel.Size = new System.Drawing.Size(21, 13);
            this._vitalsBPLabel.TabIndex = 12;
            this._vitalsBPLabel.Text = "BP";
            // 
            // _vitalsBPDiastolicTextBox
            // 
            this._vitalsBPDiastolicTextBox.AttributeName = "Diastolic";
            this._vitalsBPDiastolicTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._vitalsBPDiastolicTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._vitalsBPDiastolicTextBox.Location = new System.Drawing.Point(227, 55);
            this._vitalsBPDiastolicTextBox.Name = "_vitalsBPDiastolicTextBox";
            this._vitalsBPDiastolicTextBox.ParentDataEntryControl = this._vitalsBPTextBox;
            this._vitalsBPDiastolicTextBox.Size = new System.Drawing.Size(89, 20);
            this._vitalsBPDiastolicTextBox.TabIndex = 2;
            this._vitalsBPDiastolicTextBox.ValidationErrorMessage = "Diastolic blood pressure must be a positive number";
            this._vitalsBPDiastolicTextBox.ValidationPattern = "^$|^\\d+\\s?$";
            // 
            // _vitalsBPTextBox
            // 
            this._vitalsBPTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._vitalsBPTextBox.AttributeName = "BP";
            this._vitalsBPTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._vitalsBPTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._vitalsBPTextBox.Location = new System.Drawing.Point(434, 13);
            this._vitalsBPTextBox.Name = "_vitalsBPTextBox";
            this._vitalsBPTextBox.ParentDataEntryControl = this._vitalsGroupBox;
            this._vitalsBPTextBox.Size = new System.Drawing.Size(10, 20);
            this._vitalsBPTextBox.TabIndex = 3;
            this._vitalsBPTextBox.ValidationErrorMessage = "Invalid value";
            this._vitalsBPTextBox.Visible = false;
            // 
            // _vitalsBPSlashLabel
            // 
            this._vitalsBPSlashLabel.AutoSize = true;
            this._vitalsBPSlashLabel.Location = new System.Drawing.Point(209, 58);
            this._vitalsBPSlashLabel.Name = "_vitalsBPSlashLabel";
            this._vitalsBPSlashLabel.Size = new System.Drawing.Size(12, 13);
            this._vitalsBPSlashLabel.TabIndex = 10;
            this._vitalsBPSlashLabel.Text = "/";
            // 
            // _vitalsBPSystolicTextBox
            // 
            this._vitalsBPSystolicTextBox.AttributeName = "Systolic";
            this._vitalsBPSystolicTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._vitalsBPSystolicTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._vitalsBPSystolicTextBox.Location = new System.Drawing.Point(114, 57);
            this._vitalsBPSystolicTextBox.Name = "_vitalsBPSystolicTextBox";
            this._vitalsBPSystolicTextBox.ParentDataEntryControl = this._vitalsBPTextBox;
            this._vitalsBPSystolicTextBox.Size = new System.Drawing.Size(89, 20);
            this._vitalsBPSystolicTextBox.TabIndex = 1;
            this._vitalsBPSystolicTextBox.ValidationErrorMessage = "Systolic blood pressure must be a positive number";
            this._vitalsBPSystolicTextBox.ValidationPattern = "^$|^\\d+\\s?$";
            // 
            // _vitalsTimeLabel
            // 
            this._vitalsTimeLabel.AutoSize = true;
            this._vitalsTimeLabel.Location = new System.Drawing.Point(111, 15);
            this._vitalsTimeLabel.Name = "_vitalsTimeLabel";
            this._vitalsTimeLabel.Size = new System.Drawing.Size(30, 13);
            this._vitalsTimeLabel.TabIndex = 7;
            this._vitalsTimeLabel.Text = "Time";
            // 
            // _vitalsTimeTextBox
            // 
            this._vitalsTimeTextBox.AttributeName = "Time";
            this._vitalsTimeTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._vitalsTimeTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._vitalsTimeTextBox.FormattingRuleFile = "Rules\\Swiping\\Time.rsd.etf";
            this._vitalsTimeTextBox.Location = new System.Drawing.Point(114, 31);
            this._vitalsTimeTextBox.Name = "_vitalsTimeTextBox";
            this._vitalsTimeTextBox.ParentDataEntryControl = this._vitalsGroupBox;
            this._vitalsTimeTextBox.Size = new System.Drawing.Size(89, 20);
            this._vitalsTimeTextBox.TabIndex = 2;
            this._vitalsTimeTextBox.ValidationErrorMessage = "Time must be a valid time formatted HH:MM";
            this._vitalsTimeTextBox.ValidationPattern = "(^$)|(^((0?[0-9])|(1[0-9])|(2[0-3])):[0-5][0-9]$)";
            // 
            // _vitalsDateLabel
            // 
            this._vitalsDateLabel.AutoSize = true;
            this._vitalsDateLabel.Location = new System.Drawing.Point(6, 16);
            this._vitalsDateLabel.Name = "_vitalsDateLabel";
            this._vitalsDateLabel.Size = new System.Drawing.Size(30, 13);
            this._vitalsDateLabel.TabIndex = 5;
            this._vitalsDateLabel.Text = "Date";
            // 
            // _vitalsDateTextBox
            // 
            this._vitalsDateTextBox.AttributeName = "Date";
            this._vitalsDateTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._vitalsDateTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._vitalsDateTextBox.FormattingRuleFile = "Rules\\Swiping\\Date.rsd.etf";
            this._vitalsDateTextBox.Location = new System.Drawing.Point(6, 31);
            this._vitalsDateTextBox.Name = "_vitalsDateTextBox";
            this._vitalsDateTextBox.ParentDataEntryControl = this._vitalsGroupBox;
            this._vitalsDateTextBox.Size = new System.Drawing.Size(89, 20);
            this._vitalsDateTextBox.TabIndex = 1;
            this._vitalsDateTextBox.ValidationErrorMessage = "Date must be a valid date in the format MM/DD/YYYY";
            this._vitalsDateTextBox.ValidationPattern = "(^$)|(^((0?[1-9])|(1[0-2]))/((0?[1-9])|(1[0-9])|(2[0-9])|(3[01]))/(19|20)\\d{2}$)";
            // 
            // _visitDetailsGroupBox
            // 
            this._visitDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._visitDetailsGroupBox.AttributeName = "VisitDetails";
            this._visitDetailsGroupBox.Controls.Add(this._visitDetailsVisitNotesLabel);
            this._visitDetailsGroupBox.Controls.Add(this._visitDetailsVisitNotesTextBox);
            this._visitDetailsGroupBox.Controls.Add(this._visitDetailsReasonForVisitLabel);
            this._visitDetailsGroupBox.Controls.Add(this._visitDetailsReasonForVisitTextBox);
            this._visitDetailsGroupBox.Controls.Add(this._visitDetailsLocation);
            this._visitDetailsGroupBox.Controls.Add(this._visitDetailLocationTextBox);
            this._visitDetailsGroupBox.Controls.Add(this._visitDetailsProviderNameLabel);
            this._visitDetailsGroupBox.Controls.Add(this._visitDetailsProviderNameTable);
            this._visitDetailsGroupBox.Controls.Add(this._visitDetailsTimeLabel);
            this._visitDetailsGroupBox.Controls.Add(this._visitDetailsDateTextBox);
            this._visitDetailsGroupBox.Controls.Add(this._visitDetailsTimeTextBox);
            this._visitDetailsGroupBox.Controls.Add(this._visitDetailsDateLabel);
            this._visitDetailsGroupBox.Location = new System.Drawing.Point(4, 385);
            this._visitDetailsGroupBox.Name = "_visitDetailsGroupBox";
            this._visitDetailsGroupBox.ParentDataEntryControl = null;
            this._visitDetailsGroupBox.Size = new System.Drawing.Size(450, 281);
            this._visitDetailsGroupBox.TabIndex = 4;
            this._visitDetailsGroupBox.TabStop = false;
            this._visitDetailsGroupBox.Text = "Visit Details";
            // 
            // _visitDetailsVisitNotesLabel
            // 
            this._visitDetailsVisitNotesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._visitDetailsVisitNotesLabel.AutoSize = true;
            this._visitDetailsVisitNotesLabel.Location = new System.Drawing.Point(6, 200);
            this._visitDetailsVisitNotesLabel.Name = "_visitDetailsVisitNotesLabel";
            this._visitDetailsVisitNotesLabel.Size = new System.Drawing.Size(55, 13);
            this._visitDetailsVisitNotesLabel.TabIndex = 29;
            this._visitDetailsVisitNotesLabel.Text = "Visit notes";
            // 
            // _visitDetailsVisitNotesTextBox
            // 
            this._visitDetailsVisitNotesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._visitDetailsVisitNotesTextBox.AttributeName = "VisitNotes";
            this._visitDetailsVisitNotesTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._visitDetailsVisitNotesTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._visitDetailsVisitNotesTextBox.Location = new System.Drawing.Point(6, 216);
            this._visitDetailsVisitNotesTextBox.Multiline = true;
            this._visitDetailsVisitNotesTextBox.Name = "_visitDetailsVisitNotesTextBox";
            this._visitDetailsVisitNotesTextBox.ParentDataEntryControl = this._visitDetailsGroupBox;
            this._visitDetailsVisitNotesTextBox.RemoveNewLineChars = false;
            this._visitDetailsVisitNotesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._visitDetailsVisitNotesTextBox.Size = new System.Drawing.Size(438, 59);
            this._visitDetailsVisitNotesTextBox.TabIndex = 6;
            this._visitDetailsVisitNotesTextBox.ValidationErrorMessage = "Invalid value";
            // 
            // _visitDetailsReasonForVisitLabel
            // 
            this._visitDetailsReasonForVisitLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._visitDetailsReasonForVisitLabel.AutoSize = true;
            this._visitDetailsReasonForVisitLabel.Location = new System.Drawing.Point(6, 161);
            this._visitDetailsReasonForVisitLabel.Name = "_visitDetailsReasonForVisitLabel";
            this._visitDetailsReasonForVisitLabel.Size = new System.Drawing.Size(78, 13);
            this._visitDetailsReasonForVisitLabel.TabIndex = 27;
            this._visitDetailsReasonForVisitLabel.Text = "ReasonForVisit";
            // 
            // _visitDetailsReasonForVisitTextBox
            // 
            this._visitDetailsReasonForVisitTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._visitDetailsReasonForVisitTextBox.AttributeName = "ReasonForVisit";
            this._visitDetailsReasonForVisitTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._visitDetailsReasonForVisitTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._visitDetailsReasonForVisitTextBox.Location = new System.Drawing.Point(6, 177);
            this._visitDetailsReasonForVisitTextBox.Name = "_visitDetailsReasonForVisitTextBox";
            this._visitDetailsReasonForVisitTextBox.ParentDataEntryControl = this._visitDetailsGroupBox;
            this._visitDetailsReasonForVisitTextBox.Size = new System.Drawing.Size(438, 20);
            this._visitDetailsReasonForVisitTextBox.TabIndex = 5;
            this._visitDetailsReasonForVisitTextBox.ValidationErrorMessage = "Invalid value";
            // 
            // _visitDetailsLocation
            // 
            this._visitDetailsLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._visitDetailsLocation.AutoSize = true;
            this._visitDetailsLocation.Location = new System.Drawing.Point(6, 122);
            this._visitDetailsLocation.Name = "_visitDetailsLocation";
            this._visitDetailsLocation.Size = new System.Drawing.Size(48, 13);
            this._visitDetailsLocation.TabIndex = 7;
            this._visitDetailsLocation.Text = "Location";
            // 
            // _visitDetailLocationTextBox
            // 
            this._visitDetailLocationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._visitDetailLocationTextBox.AttributeName = "Location";
            this._visitDetailLocationTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._visitDetailLocationTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._visitDetailLocationTextBox.Location = new System.Drawing.Point(6, 138);
            this._visitDetailLocationTextBox.Name = "_visitDetailLocationTextBox";
            this._visitDetailLocationTextBox.ParentDataEntryControl = this._visitDetailsGroupBox;
            this._visitDetailLocationTextBox.Size = new System.Drawing.Size(438, 20);
            this._visitDetailLocationTextBox.TabIndex = 4;
            this._visitDetailLocationTextBox.ValidationErrorMessage = "Invalid value";
            // 
            // _visitDetailsProviderNameLabel
            // 
            this._visitDetailsProviderNameLabel.AutoSize = true;
            this._visitDetailsProviderNameLabel.Location = new System.Drawing.Point(6, 54);
            this._visitDetailsProviderNameLabel.Name = "_visitDetailsProviderNameLabel";
            this._visitDetailsProviderNameLabel.Size = new System.Drawing.Size(77, 13);
            this._visitDetailsProviderNameLabel.TabIndex = 25;
            this._visitDetailsProviderNameLabel.Text = "Provider Name";
            // 
            // _visitDetailsProviderNameTable
            // 
            this._visitDetailsProviderNameTable.AllowTabbingByRow = true;
            this._visitDetailsProviderNameTable.AllowUserToAddRows = false;
            this._visitDetailsProviderNameTable.AllowUserToDeleteRows = false;
            this._visitDetailsProviderNameTable.AllowUserToResizeRows = false;
            this._visitDetailsProviderNameTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._visitDetailsProviderNameTable.AttributeName = "Name";
            this._visitDetailsProviderNameTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._visitDetailsProviderNameTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this._visitDetailsProviderNameTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._visitDetailsProviderNameTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._providerLastNameColumn,
            this._providerFirstNameColumn,
            this._providerMiddleNameColumn});
            this._visitDetailsProviderNameTable.Location = new System.Drawing.Point(6, 70);
            this._visitDetailsProviderNameTable.MinimumNumberOfRows = 1;
            this._visitDetailsProviderNameTable.Name = "_visitDetailsProviderNameTable";
            this._visitDetailsProviderNameTable.ParentDataEntryControl = this._visitDetailsGroupBox;
            this._visitDetailsProviderNameTable.RowFormattingRuleFile = "Rules\\Swiping\\Name.rsd.etf";
            this._visitDetailsProviderNameTable.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._visitDetailsProviderNameTable.RowSwipingEnabled = true;
            this._visitDetailsProviderNameTable.Size = new System.Drawing.Size(438, 50);
            this._visitDetailsProviderNameTable.TabIndex = 3;
            // 
            // _providerLastNameColumn
            // 
            this._providerLastNameColumn.AttributeName = "Last";
            this._providerLastNameColumn.HeaderText = "Last";
            this._providerLastNameColumn.Name = "_providerLastNameColumn";
            this._providerLastNameColumn.ValidationErrorMessage = "Invalid value";
            // 
            // _providerFirstNameColumn
            // 
            this._providerFirstNameColumn.AttributeName = "First";
            this._providerFirstNameColumn.HeaderText = "First";
            this._providerFirstNameColumn.Name = "_providerFirstNameColumn";
            this._providerFirstNameColumn.ValidationErrorMessage = "Invalid value";
            // 
            // _providerMiddleNameColumn
            // 
            this._providerMiddleNameColumn.AttributeName = "Middle";
            this._providerMiddleNameColumn.FillWeight = 50F;
            this._providerMiddleNameColumn.HeaderText = "Middle";
            this._providerMiddleNameColumn.Name = "_providerMiddleNameColumn";
            this._providerMiddleNameColumn.ValidationErrorMessage = "Invalid value";
            // 
            // _visitDetailsTimeLabel
            // 
            this._visitDetailsTimeLabel.AutoSize = true;
            this._visitDetailsTimeLabel.Location = new System.Drawing.Point(111, 16);
            this._visitDetailsTimeLabel.Name = "_visitDetailsTimeLabel";
            this._visitDetailsTimeLabel.Size = new System.Drawing.Size(30, 13);
            this._visitDetailsTimeLabel.TabIndex = 24;
            this._visitDetailsTimeLabel.Text = "Time";
            // 
            // _visitDetailsDateTextBox
            // 
            this._visitDetailsDateTextBox.AttributeName = "Date";
            this._visitDetailsDateTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._visitDetailsDateTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._visitDetailsDateTextBox.FormattingRuleFile = "Rules\\Swiping\\Date.rsd.etf";
            this._visitDetailsDateTextBox.Location = new System.Drawing.Point(6, 31);
            this._visitDetailsDateTextBox.Name = "_visitDetailsDateTextBox";
            this._visitDetailsDateTextBox.ParentDataEntryControl = this._visitDetailsGroupBox;
            this._visitDetailsDateTextBox.Size = new System.Drawing.Size(89, 20);
            this._visitDetailsDateTextBox.TabIndex = 1;
            this._visitDetailsDateTextBox.ValidationErrorMessage = "Date must be a valid date in the format MM/DD/YYYY";
            this._visitDetailsDateTextBox.ValidationPattern = "(^$)|(^((0?[1-9])|(1[0-2]))/((0?[1-9])|(1[0-9])|(2[0-9])|(3[01]))/(19|20)\\d{2}$)";
            // 
            // _visitDetailsTimeTextBox
            // 
            this._visitDetailsTimeTextBox.AttributeName = "Time";
            this._visitDetailsTimeTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._visitDetailsTimeTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._visitDetailsTimeTextBox.FormattingRuleFile = "Rules\\Swiping\\Time.rsd.etf";
            this._visitDetailsTimeTextBox.Location = new System.Drawing.Point(114, 31);
            this._visitDetailsTimeTextBox.Name = "_visitDetailsTimeTextBox";
            this._visitDetailsTimeTextBox.ParentDataEntryControl = this._visitDetailsGroupBox;
            this._visitDetailsTimeTextBox.Size = new System.Drawing.Size(89, 20);
            this._visitDetailsTimeTextBox.TabIndex = 2;
            this._visitDetailsTimeTextBox.ValidationErrorMessage = "Time must be a valid time formatted HH:MM";
            this._visitDetailsTimeTextBox.ValidationPattern = "(^$)|(^((0?[0-9])|(1[0-9])|(2[0-3])):[0-5][0-9]$)";
            // 
            // _visitDetailsDateLabel
            // 
            this._visitDetailsDateLabel.AutoSize = true;
            this._visitDetailsDateLabel.Location = new System.Drawing.Point(6, 16);
            this._visitDetailsDateLabel.Name = "_visitDetailsDateLabel";
            this._visitDetailsDateLabel.Size = new System.Drawing.Size(30, 13);
            this._visitDetailsDateLabel.TabIndex = 21;
            this._visitDetailsDateLabel.Text = "Date";
            // 
            // _allergiesGroupBox
            // 
            this._allergiesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._allergiesGroupBox.Controls.Add(this._allergiesTable);
            this._allergiesGroupBox.Location = new System.Drawing.Point(4, 672);
            this._allergiesGroupBox.Name = "_allergiesGroupBox";
            this._allergiesGroupBox.Size = new System.Drawing.Size(450, 137);
            this._allergiesGroupBox.TabIndex = 5;
            this._allergiesGroupBox.TabStop = false;
            this._allergiesGroupBox.Text = "Allergies";
            // 
            // _allergiesTable
            // 
            this._allergiesTable.AllowUserToResizeRows = false;
            this._allergiesTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._allergiesTable.AttributeName = "Allergy";
            this._allergiesTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._allergiesTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this._allergiesTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this._allergiesTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._allergyNameColumn,
            this._allergyDateColumn,
            this._allergyReactionsColumn});
            this._allergiesTable.Location = new System.Drawing.Point(6, 19);
            this._allergiesTable.Name = "_allergiesTable";
            this._allergiesTable.ParentDataEntryControl = null;
            this._allergiesTable.RowFormattingRuleFile = "Rules\\Swiping\\Allergy.rsd.etf";
            this._allergiesTable.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this._allergiesTable.RowSwipingEnabled = true;
            this._allergiesTable.Size = new System.Drawing.Size(438, 112);
            this._allergiesTable.TabIndex = 0;
            // 
            // _allergyNameColumn
            // 
            this._allergyNameColumn.AttributeName = "Name";
            this._allergyNameColumn.FillWeight = 75F;
            this._allergyNameColumn.HeaderText = "Name";
            this._allergyNameColumn.Name = "_allergyNameColumn";
            this._allergyNameColumn.ValidationErrorMessage = "Invalid value";
            // 
            // _allergyDateColumn
            // 
            this._allergyDateColumn.AttributeName = "Date";
            this._allergyDateColumn.FillWeight = 50F;
            this._allergyDateColumn.HeaderText = "Date noted";
            this._allergyDateColumn.Name = "_allergyDateColumn";
            this._allergyDateColumn.ValidationErrorMessage = "Date must be a valid date in the format MM/DD/YYYY";
            this._allergyDateColumn.ValidationPattern = "(^$)|(^((0?[1-9])|(1[0-2]))/((0?[1-9])|(1[0-9])|(2[0-9])|(3[01]))/(19|20)\\d{2}$)";
            // 
            // _allergyReactionsColumn
            // 
            this._allergyReactionsColumn.AttributeName = "Reactions";
            this._allergyReactionsColumn.HeaderText = "Reactions";
            this._allergyReactionsColumn.Name = "_allergyReactionsColumn";
            this._allergyReactionsColumn.ValidationErrorMessage = "Invalid value";
            // 
            // StandardMedDEPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._allergiesGroupBox);
            this.Controls.Add(this._visitDetailsGroupBox);
            this.Controls.Add(this._vitalsGroupBox);
            this.Controls.Add(this._physicianInfoGroupBox);
            this.Controls.Add(this._patientInfoGroupBox);
            highlightColor1.Color = System.Drawing.Color.LightSalmon;
            highlightColor1.MaxOcrConfidence = 89;
            highlightColor2.Color = System.Drawing.Color.LightGreen;
            highlightColor2.MaxOcrConfidence = 100;
            this.HighlightColors = new Extract.DataEntry.HighlightColor[] {
        highlightColor1,
        highlightColor2};
            this.MinimumSize = new System.Drawing.Size(425, 0);
            this.Name = "StandardMedDEPanel";
            this.Size = new System.Drawing.Size(457, 812);
            this._patientInfoGroupBox.ResumeLayout(false);
            this._patientInfoGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._patientNameTable)).EndInit();
            this._physicianInfoGroupBox.ResumeLayout(false);
            this._physicianInfoGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._physicianNameTable)).EndInit();
            this._vitalsGroupBox.ResumeLayout(false);
            this._vitalsGroupBox.PerformLayout();
            this._visitDetailsGroupBox.ResumeLayout(false);
            this._visitDetailsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._visitDetailsProviderNameTable)).EndInit();
            this._allergiesGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._allergiesTable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Extract.DataEntry.DataEntryGroupBox _patientInfoGroupBox;
        private Extract.DataEntry.DataEntryTable _patientNameTable;
        private Extract.DataEntry.DataEntryTextBox _patientRecordTextBox;
        private Extract.DataEntry.DataEntryTextBox _patientDOBTextBox;
        private System.Windows.Forms.Label _patientRecordLabel;
        private System.Windows.Forms.Label _patientDOBLabel;
        private Extract.DataEntry.DataEntryGroupBox _physicianInfoGroupBox;
        private Extract.DataEntry.DataEntryTable _physicianNameTable;
        private Extract.DataEntry.DataEntryTextBox _physicianLocationTextBox;
        private System.Windows.Forms.Label _physicianLocationLabel;
        private Extract.DataEntry.DataEntryGroupBox _vitalsGroupBox;
        private System.Windows.Forms.Label _vitalsDateLabel;
        private Extract.DataEntry.DataEntryTextBox _vitalsDateTextBox;
        private System.Windows.Forms.Label _vitalsTimeLabel;
        private Extract.DataEntry.DataEntryTextBox _vitalsTimeTextBox;
        private Extract.DataEntry.DataEntryTextBox _vitalsBPTextBox;
        private Extract.DataEntry.DataEntryTextBox _vitalsBPDiastolicTextBox;
        private System.Windows.Forms.Label _vitalsBPSlashLabel;
        private Extract.DataEntry.DataEntryTextBox _vitalsBPSystolicTextBox;
        private System.Windows.Forms.Label _vitalsBPLabel;
        private Extract.DataEntry.DataEntryTextBox _vitalsWeightTextBox;
        private System.Windows.Forms.Label _vitalWeightLabel;
        private Extract.DataEntry.DataEntryTextBox _vitalsTempTextBox;
        private System.Windows.Forms.Label _vitalsTempLabel;
        private Extract.DataEntry.DataEntryTextBox _vitalsRespTextBox;
        private System.Windows.Forms.Label _vitalsRespLabel;
        private Extract.DataEntry.DataEntryTextBox _vitalsPulseTextBox;
        private System.Windows.Forms.Label _vitalsPulseLabel;
        private Extract.DataEntry.DataEntryGroupBox _visitDetailsGroupBox;
        private System.Windows.Forms.Label _visitDetailsTimeLabel;
        private Extract.DataEntry.DataEntryTextBox _visitDetailsDateTextBox;
        private Extract.DataEntry.DataEntryTextBox _visitDetailsTimeTextBox;
        private System.Windows.Forms.Label _visitDetailsDateLabel;
        private Extract.DataEntry.DataEntryTable _visitDetailsProviderNameTable;
        private System.Windows.Forms.Label _visitDetailsProviderNameLabel;
        private System.Windows.Forms.Label _visitDetailsReasonForVisitLabel;
        private Extract.DataEntry.DataEntryTextBox _visitDetailsReasonForVisitTextBox;
        private System.Windows.Forms.Label _visitDetailsLocation;
        private Extract.DataEntry.DataEntryTextBox _visitDetailLocationTextBox;
        private System.Windows.Forms.Label _visitDetailsVisitNotesLabel;
        private Extract.DataEntry.DataEntryTextBox _visitDetailsVisitNotesTextBox;
        private System.Windows.Forms.GroupBox _allergiesGroupBox;
        private Extract.DataEntry.DataEntryTable _allergiesTable;
        private Extract.DataEntry.DataEntryTableColumn _patientLastName;
        private Extract.DataEntry.DataEntryTableColumn _patientFirstName;
        private Extract.DataEntry.DataEntryTableColumn _patientMiddleName;
        private Extract.DataEntry.DataEntryTableColumn _physicianLastNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _physicianFirstNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _physicianMiddleNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _providerLastNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _providerFirstNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _providerMiddleNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _allergyNameColumn;
        private Extract.DataEntry.DataEntryTableColumn _allergyDateColumn;
        private Extract.DataEntry.DataEntryTableColumn _allergyReactionsColumn;
    }
}
