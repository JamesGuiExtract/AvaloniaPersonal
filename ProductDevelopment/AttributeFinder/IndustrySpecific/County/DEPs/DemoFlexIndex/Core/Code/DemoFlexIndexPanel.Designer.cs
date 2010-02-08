namespace Extract.DataEntry.DEP.DemoFlexIndex
{
    partial class DemoFlexIndexPanel
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
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow1 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow2 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow3 = new Extract.DataEntry.DataEntryTableRow();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DemoFlexIndexPanel));
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow4 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow5 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.HighlightColor highlightColor1 = new Extract.DataEntry.HighlightColor();
            Extract.DataEntry.HighlightColor highlightColor2 = new Extract.DataEntry.HighlightColor();
            this._grantorTable = new Extract.DataEntry.DataEntryTable();
            this._grantorFirstNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._grantorMiddleNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._grantorLastOrCompanyNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._grantorSuffixColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._grantorTypeColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._granteeTable = new Extract.DataEntry.DataEntryTable();
            this._granteeFirstNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._granteeMiddleNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._granteeLastOrCompanyNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._granteeSuffixColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._granteeTypeColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._legalDescriptionTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._legalDescriptionTable = new Extract.DataEntry.DataEntryTable();
            this.Lot = new Extract.DataEntry.DataEntryTableColumn();
            this.Block = new Extract.DataEntry.DataEntryTableColumn();
            this.Subdivision = new Extract.DataEntry.DataEntryTableColumn();
            this._considerationAmountLabel = new System.Windows.Forms.Label();
            this._docTypeLabel = new System.Windows.Forms.Label();
            this._considerationAmountTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._returnAddressTable = new Extract.DataEntry.DataEntryTwoColumnTable();
            this._parcelNumberLabel = new System.Windows.Forms.Label();
            this._parcelNumberTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._docTypeTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._grantorsLabel = new System.Windows.Forms.Label();
            this._granteesLabel = new System.Windows.Forms.Label();
            this._legalDescriptionLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._grantorTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._granteeTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._legalDescriptionTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._returnAddressTable)).BeginInit();
            this.SuspendLayout();
            // 
            // _grantorTable
            // 
            this._grantorTable.AllowDrop = true;
            this._grantorTable.AllowTabbingByRow = true;
            this._grantorTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._grantorTable.AttributeName = "Grantor";
            this._grantorTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._grantorTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this._grantorTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._grantorTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._grantorFirstNameColumn,
            this._grantorMiddleNameColumn,
            this._grantorLastOrCompanyNameColumn,
            this._grantorSuffixColumn,
            this._grantorTypeColumn});
            this._grantorTable.CompatibleAttributeNames.Add("Grantee");
            this._grantorTable.Location = new System.Drawing.Point(3, 164);
            this._grantorTable.Name = "_grantorTable";
            this._grantorTable.ParentDataEntryControl = null;
            this._grantorTable.RowFormattingRuleFile = "..\\Rules\\RubberbandSplitters\\Grantor.rsd.etf";
            this._grantorTable.RowSwipingEnabled = true;
            this._grantorTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._grantorTable.Size = new System.Drawing.Size(562, 90);
            this._grantorTable.TabIndex = 5;
            // 
            // _grantorFirstNameColumn
            // 
            this._grantorFirstNameColumn.AttributeName = "First";
            this._grantorFirstNameColumn.FillWeight = 80F;
            this._grantorFirstNameColumn.HeaderText = "First";
            this._grantorFirstNameColumn.Name = "_grantorFirstNameColumn";
            this._grantorFirstNameColumn.ValidationErrorMessage = "";
            // 
            // _grantorMiddleNameColumn
            // 
            this._grantorMiddleNameColumn.AttributeName = "Middle";
            this._grantorMiddleNameColumn.FillWeight = 60F;
            this._grantorMiddleNameColumn.HeaderText = "Middle";
            this._grantorMiddleNameColumn.Name = "_grantorMiddleNameColumn";
            this._grantorMiddleNameColumn.ValidationErrorMessage = "";
            // 
            // _grantorLastOrCompanyNameColumn
            // 
            this._grantorLastOrCompanyNameColumn.AttributeName = "LastOrCompany";
            this._grantorLastOrCompanyNameColumn.FillWeight = 120F;
            this._grantorLastOrCompanyNameColumn.HeaderText = "Last / Company";
            this._grantorLastOrCompanyNameColumn.Name = "_grantorLastOrCompanyNameColumn";
            this._grantorLastOrCompanyNameColumn.ValidationErrorMessage = "";
            // 
            // _grantorSuffixColumn
            // 
            this._grantorSuffixColumn.AttributeName = "Suffix";
            this._grantorSuffixColumn.FillWeight = 50F;
            this._grantorSuffixColumn.HeaderText = "Suffix";
            this._grantorSuffixColumn.Name = "_grantorSuffixColumn";
            this._grantorSuffixColumn.ValidationErrorMessage = "";
            // 
            // _grantorTypeColumn
            // 
            this._grantorTypeColumn.AttributeName = "Type";
            this._grantorTypeColumn.AutoUpdateQuery = "";
            this._grantorTypeColumn.FillWeight = 60F;
            this._grantorTypeColumn.HeaderText = "Type";
            this._grantorTypeColumn.Name = "_grantorTypeColumn";
            this._grantorTypeColumn.ValidationErrorMessage = "";
            // 
            // _granteeTable
            // 
            this._granteeTable.AllowDrop = true;
            this._granteeTable.AllowTabbingByRow = true;
            this._granteeTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._granteeTable.AttributeName = "Grantee";
            this._granteeTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._granteeTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this._granteeTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._granteeTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._granteeFirstNameColumn,
            this._granteeMiddleNameColumn,
            this._granteeLastOrCompanyNameColumn,
            this._granteeSuffixColumn,
            this._granteeTypeColumn});
            this._granteeTable.CompatibleAttributeNames.Add("Grantor");
            this._granteeTable.Location = new System.Drawing.Point(3, 282);
            this._granteeTable.Name = "_granteeTable";
            this._granteeTable.ParentDataEntryControl = null;
            this._granteeTable.RowFormattingRuleFile = "..\\Rules\\RubberbandSplitters\\Grantee.rsd.etf";
            this._granteeTable.RowSwipingEnabled = true;
            this._granteeTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._granteeTable.Size = new System.Drawing.Size(562, 90);
            this._granteeTable.TabIndex = 6;
            // 
            // _granteeFirstNameColumn
            // 
            this._granteeFirstNameColumn.AttributeName = "First";
            this._granteeFirstNameColumn.FillWeight = 80F;
            this._granteeFirstNameColumn.HeaderText = "First";
            this._granteeFirstNameColumn.Name = "_granteeFirstNameColumn";
            this._granteeFirstNameColumn.ValidationErrorMessage = "";
            // 
            // _granteeMiddleNameColumn
            // 
            this._granteeMiddleNameColumn.AttributeName = "Middle";
            this._granteeMiddleNameColumn.FillWeight = 60F;
            this._granteeMiddleNameColumn.HeaderText = "Middle";
            this._granteeMiddleNameColumn.Name = "_granteeMiddleNameColumn";
            this._granteeMiddleNameColumn.ValidationErrorMessage = "";
            // 
            // _granteeLastOrCompanyNameColumn
            // 
            this._granteeLastOrCompanyNameColumn.AttributeName = "LastOrCompany";
            this._granteeLastOrCompanyNameColumn.FillWeight = 120F;
            this._granteeLastOrCompanyNameColumn.HeaderText = "Last / Company";
            this._granteeLastOrCompanyNameColumn.Name = "_granteeLastOrCompanyNameColumn";
            this._granteeLastOrCompanyNameColumn.ValidationErrorMessage = "";
            // 
            // _granteeSuffixColumn
            // 
            this._granteeSuffixColumn.AttributeName = "Suffix";
            this._granteeSuffixColumn.FillWeight = 50F;
            this._granteeSuffixColumn.HeaderText = "Suffix";
            this._granteeSuffixColumn.Name = "_granteeSuffixColumn";
            this._granteeSuffixColumn.ValidationErrorMessage = "";
            // 
            // _granteeTypeColumn
            // 
            this._granteeTypeColumn.AttributeName = "Type";
            this._granteeTypeColumn.AutoUpdateQuery = "";
            this._granteeTypeColumn.FillWeight = 60F;
            this._granteeTypeColumn.HeaderText = "Type";
            this._granteeTypeColumn.Name = "_granteeTypeColumn";
            this._granteeTypeColumn.ValidationErrorMessage = "";
            // 
            // _legalDescriptionTextBox
            // 
            this._legalDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._legalDescriptionTextBox.AttributeName = "FullText";
            this._legalDescriptionTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._legalDescriptionTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._legalDescriptionTextBox.AutoUpdateQuery = "";
            this._legalDescriptionTextBox.Location = new System.Drawing.Point(3, 474);
            this._legalDescriptionTextBox.Multiline = true;
            this._legalDescriptionTextBox.Name = "_legalDescriptionTextBox";
            this._legalDescriptionTextBox.ParentDataEntryControl = this._legalDescriptionTable;
            this._legalDescriptionTextBox.RemoveNewLineChars = false;
            this._legalDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._legalDescriptionTextBox.Size = new System.Drawing.Size(562, 87);
            this._legalDescriptionTextBox.TabIndex = 8;
            this._legalDescriptionTextBox.ValidationErrorMessage = "Invalid value";
            // 
            // _legalDescriptionTable
            // 
            this._legalDescriptionTable.AllowUserToResizeRows = false;
            this._legalDescriptionTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._legalDescriptionTable.AttributeName = "LegalDescription";
            this._legalDescriptionTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._legalDescriptionTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this._legalDescriptionTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._legalDescriptionTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Lot,
            this.Block,
            this.Subdivision});
            this._legalDescriptionTable.Location = new System.Drawing.Point(3, 400);
            this._legalDescriptionTable.MinimumNumberOfRows = 1;
            this._legalDescriptionTable.Name = "_legalDescriptionTable";
            this._legalDescriptionTable.ParentDataEntryControl = null;
            this._legalDescriptionTable.RowFormattingRuleFile = "..\\Rules\\RubberbandSplitters\\LegalDescription.rsd.etf";
            this._legalDescriptionTable.RowSwipingEnabled = true;
            this._legalDescriptionTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._legalDescriptionTable.Size = new System.Drawing.Size(562, 68);
            this._legalDescriptionTable.TabIndex = 7;
            // 
            // Lot
            // 
            this.Lot.AttributeName = "Lot";
            this.Lot.FillWeight = 50F;
            this.Lot.HeaderText = "Lot";
            this.Lot.Name = "Lot";
            this.Lot.ValidationErrorMessage = "";
            // 
            // Block
            // 
            this.Block.AttributeName = "Block";
            this.Block.FillWeight = 75F;
            this.Block.HeaderText = "Block";
            this.Block.Name = "Block";
            this.Block.ValidationErrorMessage = "";
            // 
            // Subdivision
            // 
            this.Subdivision.AttributeName = "Subdivision";
            this.Subdivision.FillWeight = 150F;
            this.Subdivision.HeaderText = "Subdivision";
            this.Subdivision.Name = "Subdivision";
            this.Subdivision.ValidationErrorMessage = "";
            // 
            // _considerationAmountLabel
            // 
            this._considerationAmountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._considerationAmountLabel.AutoSize = true;
            this._considerationAmountLabel.Location = new System.Drawing.Point(414, 44);
            this._considerationAmountLabel.Name = "_considerationAmountLabel";
            this._considerationAmountLabel.Size = new System.Drawing.Size(110, 13);
            this._considerationAmountLabel.TabIndex = 5;
            this._considerationAmountLabel.Text = "Consideration Amount";
            // 
            // _docTypeLabel
            // 
            this._docTypeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._docTypeLabel.AutoSize = true;
            this._docTypeLabel.Location = new System.Drawing.Point(414, 84);
            this._docTypeLabel.Name = "_docTypeLabel";
            this._docTypeLabel.Size = new System.Drawing.Size(83, 13);
            this._docTypeLabel.TabIndex = 6;
            this._docTypeLabel.Text = "Document Type";
            // 
            // _considerationAmountTextBox
            // 
            this._considerationAmountTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._considerationAmountTextBox.AttributeName = "ConsiderationAmount";
            this._considerationAmountTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._considerationAmountTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._considerationAmountTextBox.Location = new System.Drawing.Point(417, 61);
            this._considerationAmountTextBox.Name = "_considerationAmountTextBox";
            this._considerationAmountTextBox.Size = new System.Drawing.Size(148, 20);
            this._considerationAmountTextBox.TabIndex = 3;
            this._considerationAmountTextBox.ValidationErrorMessage = "";
            // 
            // _returnAddressTable
            // 
            this._returnAddressTable.AllowUserToResizeRows = false;
            this._returnAddressTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._returnAddressTable.AttributeName = "ReturnAddress";
            this._returnAddressTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._returnAddressTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this._returnAddressTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._returnAddressTable.DisplayName = "Return Address";
            this._returnAddressTable.Location = new System.Drawing.Point(3, 5);
            this._returnAddressTable.Name = "_returnAddressTable";
            this._returnAddressTable.ParentDataEntryControl = null;
            this._returnAddressTable.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dataEntryTableRow1.AttributeName = "Recipient1";
            dataEntryTableRow1.Name = "Recipient";
            dataEntryTableRow1.ValidationErrorMessage = "";
            dataEntryTableRow2.AttributeName = "Address1";
            dataEntryTableRow2.Name = "Street Address";
            dataEntryTableRow2.ValidationErrorMessage = "";
            dataEntryTableRow2.ValidationPattern = "\\S";
            dataEntryTableRow3.AttributeName = "City";
            dataEntryTableRow3.AutoUpdateQuery = resources.GetString("dataEntryTableRow3.AutoUpdateQuery");
            dataEntryTableRow3.Name = "City";
            dataEntryTableRow3.ValidationErrorMessage = "Unknown city name";
            dataEntryTableRow3.ValidationQuery = "[BLANK]\r\n<SQL>SELECT DISTINCT [City] FROM [ZipCode]</SQL>";
            dataEntryTableRow4.AttributeName = "State";
            dataEntryTableRow4.AutoUpdateQuery = resources.GetString("dataEntryTableRow4.AutoUpdateQuery");
            dataEntryTableRow4.Name = "State";
            dataEntryTableRow4.ValidationErrorMessage = "Unknown state";
            dataEntryTableRow4.ValidationQuery = "[BLANK]\r\n<SQL>SELECT [Abbreviation] FROM [State] WHERE LEN([Abbreviation]) > 0 UN" +
                "ION SELECT [Name] FROM [State] WHERE  LEN([Name]) > 0</SQL>";
            dataEntryTableRow5.AttributeName = "ZipCode";
            dataEntryTableRow5.AutoUpdateQuery = resources.GetString("dataEntryTableRow5.AutoUpdateQuery");
            dataEntryTableRow5.Name = "Zip";
            dataEntryTableRow5.ValidationErrorMessage = "Invalid zip code format";
            dataEntryTableRow5.ValidationPattern = "(^$)|(^\\d{5}(-\\d{4})?$)";
            dataEntryTableRow5.ValidationQuery = resources.GetString("dataEntryTableRow5.ValidationQuery");
            this._returnAddressTable.Rows.Add(dataEntryTableRow1);
            this._returnAddressTable.Rows.Add(dataEntryTableRow2);
            this._returnAddressTable.Rows.Add(dataEntryTableRow3);
            this._returnAddressTable.Rows.Add(dataEntryTableRow4);
            this._returnAddressTable.Rows.Add(dataEntryTableRow5);
            this._returnAddressTable.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this._returnAddressTable.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this._returnAddressTable.Size = new System.Drawing.Size(405, 134);
            this._returnAddressTable.TabIndex = 1;
            this._returnAddressTable.TableFormattingRuleFile = "..\\Rules\\RubberbandSplitters\\ReturnAddress.rsd.etf";
            this._returnAddressTable.TableSwipingEnabled = true;
            // 
            // _parcelNumberLabel
            // 
            this._parcelNumberLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._parcelNumberLabel.AutoSize = true;
            this._parcelNumberLabel.Location = new System.Drawing.Point(414, 5);
            this._parcelNumberLabel.Name = "_parcelNumberLabel";
            this._parcelNumberLabel.Size = new System.Drawing.Size(77, 13);
            this._parcelNumberLabel.TabIndex = 3;
            this._parcelNumberLabel.Text = "Parcel Number";
            // 
            // _parcelNumberTextBox
            // 
            this._parcelNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._parcelNumberTextBox.AttributeName = "ParcelNumber";
            this._parcelNumberTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._parcelNumberTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._parcelNumberTextBox.Location = new System.Drawing.Point(417, 21);
            this._parcelNumberTextBox.Name = "_parcelNumberTextBox";
            this._parcelNumberTextBox.Size = new System.Drawing.Size(148, 20);
            this._parcelNumberTextBox.TabIndex = 2;
            this._parcelNumberTextBox.ValidationErrorMessage = "Invalid value";
            // 
            // _docTypeTextBox
            // 
            this._docTypeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._docTypeTextBox.AttributeName = "DocumentType";
            this._docTypeTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._docTypeTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._docTypeTextBox.Location = new System.Drawing.Point(417, 101);
            this._docTypeTextBox.Name = "_docTypeTextBox";
            this._docTypeTextBox.Size = new System.Drawing.Size(148, 20);
            this._docTypeTextBox.TabIndex = 4;
            this._docTypeTextBox.ValidationErrorMessage = "Invalid value";
            // 
            // _grantorsLabel
            // 
            this._grantorsLabel.AutoSize = true;
            this._grantorsLabel.Location = new System.Drawing.Point(3, 148);
            this._grantorsLabel.Name = "_grantorsLabel";
            this._grantorsLabel.Size = new System.Drawing.Size(47, 13);
            this._grantorsLabel.TabIndex = 9;
            this._grantorsLabel.Text = "Grantors";
            // 
            // _granteesLabel
            // 
            this._granteesLabel.AutoSize = true;
            this._granteesLabel.Location = new System.Drawing.Point(3, 266);
            this._granteesLabel.Name = "_granteesLabel";
            this._granteesLabel.Size = new System.Drawing.Size(50, 13);
            this._granteesLabel.TabIndex = 10;
            this._granteesLabel.Text = "Grantees";
            // 
            // _legalDescriptionLabel
            // 
            this._legalDescriptionLabel.AutoSize = true;
            this._legalDescriptionLabel.Location = new System.Drawing.Point(3, 384);
            this._legalDescriptionLabel.Name = "_legalDescriptionLabel";
            this._legalDescriptionLabel.Size = new System.Drawing.Size(89, 13);
            this._legalDescriptionLabel.TabIndex = 11;
            this._legalDescriptionLabel.Text = "Legal Description";
            // 
            // DemoFlexIndexPanel
            // 
            this.ApplicationTitle = "FlexIndex Demo";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this._legalDescriptionLabel);
            this.Controls.Add(this._legalDescriptionTable);
            this.Controls.Add(this._legalDescriptionTextBox);
            this.Controls.Add(this._granteesLabel);
            this.Controls.Add(this._granteeTable);
            this.Controls.Add(this._grantorsLabel);
            this.Controls.Add(this._grantorTable);
            this.Controls.Add(this._docTypeTextBox);
            this.Controls.Add(this._parcelNumberTextBox);
            this.Controls.Add(this._considerationAmountTextBox);
            this.Controls.Add(this._parcelNumberLabel);
            this.Controls.Add(this._docTypeLabel);
            this.Controls.Add(this._returnAddressTable);
            this.Controls.Add(this._considerationAmountLabel);
            highlightColor1.Color = System.Drawing.Color.LightSalmon;
            highlightColor1.MaxOcrConfidence = 89;
            highlightColor2.Color = System.Drawing.Color.LightGreen;
            highlightColor2.MaxOcrConfidence = 100;
            this.HighlightColors = new Extract.DataEntry.HighlightColor[] {
        highlightColor1,
        highlightColor2};
            this.Name = "DemoFlexIndexPanel";
            this.Size = new System.Drawing.Size(568, 564);
            ((System.ComponentModel.ISupportInitialize)(this._grantorTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._granteeTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._legalDescriptionTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._returnAddressTable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DataEntryTable _grantorTable;
        private DataEntryTextBox _legalDescriptionTextBox;
        private DataEntryTable _legalDescriptionTable;
        private DataEntryTableColumn Lot;
        private DataEntryTableColumn Block;
        private DataEntryTableColumn Subdivision;
        private DataEntryTableColumn _grantorFirstNameColumn;
        private DataEntryTableColumn _grantorMiddleNameColumn;
        private DataEntryTableColumn _grantorLastOrCompanyNameColumn;
        private DataEntryTableColumn _grantorSuffixColumn;
        private DataEntryTableColumn _grantorTypeColumn;
        private DataEntryTable _granteeTable;
        private DataEntryTableColumn _granteeFirstNameColumn;
        private DataEntryTableColumn _granteeMiddleNameColumn;
        private DataEntryTableColumn _granteeLastOrCompanyNameColumn;
        private DataEntryTableColumn _granteeSuffixColumn;
        private DataEntryTableColumn _granteeTypeColumn;
        private System.Windows.Forms.Label _considerationAmountLabel;
        private System.Windows.Forms.Label _docTypeLabel;
        private DataEntryTextBox _considerationAmountTextBox;
        private DataEntryTwoColumnTable _returnAddressTable;
        private System.Windows.Forms.Label _parcelNumberLabel;
        private DataEntryTextBox _parcelNumberTextBox;
        private DataEntryTextBox _docTypeTextBox;
        private System.Windows.Forms.Label _grantorsLabel;
        private System.Windows.Forms.Label _granteesLabel;
        private System.Windows.Forms.Label _legalDescriptionLabel;
    }
}
