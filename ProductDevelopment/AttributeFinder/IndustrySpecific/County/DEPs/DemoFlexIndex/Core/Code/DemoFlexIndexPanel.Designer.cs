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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DemoFlexIndexPanel));
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow1 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow2 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow3 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow4 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.DataEntryTableRow dataEntryTableRow5 = new Extract.DataEntry.DataEntryTableRow();
            Extract.DataEntry.HighlightColor highlightColor1 = new Extract.DataEntry.HighlightColor();
            Extract.DataEntry.HighlightColor highlightColor2 = new Extract.DataEntry.HighlightColor();
            this._partiesTable = new Extract.DataEntry.DataEntryTable();
            this._partyFirstNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._partyMiddleNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._partyLastOrCompanyNameColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._partyTypeColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._partyMappedTo = new Extract.DataEntry.DataEntryTableColumn();
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
            this._grantorsLabel = new System.Windows.Forms.Label();
            this._legalDescriptionLabel = new System.Windows.Forms.Label();
            this._documentTypeComboBox = new Extract.DataEntry.DataEntryComboBox();
            ((System.ComponentModel.ISupportInitialize)(this._partiesTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._legalDescriptionTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._returnAddressTable)).BeginInit();
            this.SuspendLayout();
            // 
            // _partiesTable
            // 
            this._partiesTable.AllowDrop = true;
            this._partiesTable.AllowTabbingByRow = true;
            this._partiesTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._partiesTable.AttributeName = "Party";
            this._partiesTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._partiesTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this._partiesTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._partiesTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._partyFirstNameColumn,
            this._partyMiddleNameColumn,
            this._partyLastOrCompanyNameColumn,
            this._partyTypeColumn,
            this._partyMappedTo});
            this._partiesTable.CompatibleAttributeNames.Add("Grantee");
            this._partiesTable.Location = new System.Drawing.Point(3, 208);
            this._partiesTable.Name = "_partiesTable";
            this._partiesTable.ParentDataEntryControl = null;
            this._partiesTable.RowFormattingRuleFile = "..\\Rules\\RubberbandSplitters\\Party.rsd.etf";
            this._partiesTable.RowSwipingEnabled = true;
            this._partiesTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._partiesTable.Size = new System.Drawing.Size(615, 156);
            this._partiesTable.TabIndex = 5;
            // 
            // _partyFirstNameColumn
            // 
            this._partyFirstNameColumn.AttributeName = "First";
            this._partyFirstNameColumn.FillWeight = 40F;
            this._partyFirstNameColumn.HeaderText = "First";
            this._partyFirstNameColumn.Name = "_partyFirstNameColumn";
            this._partyFirstNameColumn.ValidationErrorMessage = "";
            // 
            // _partyMiddleNameColumn
            // 
            this._partyMiddleNameColumn.AttributeName = "Middle";
            this._partyMiddleNameColumn.FillWeight = 27F;
            this._partyMiddleNameColumn.HeaderText = "Middle";
            this._partyMiddleNameColumn.Name = "_partyMiddleNameColumn";
            this._partyMiddleNameColumn.ValidationErrorMessage = "";
            // 
            // _partyLastOrCompanyNameColumn
            // 
            this._partyLastOrCompanyNameColumn.AttributeName = "LastOrCompany";
            this._partyLastOrCompanyNameColumn.FillWeight = 120F;
            this._partyLastOrCompanyNameColumn.HeaderText = "Last / Company";
            this._partyLastOrCompanyNameColumn.Name = "_partyLastOrCompanyNameColumn";
            this._partyLastOrCompanyNameColumn.ValidationErrorMessage = "";
            // 
            // _partyTypeColumn
            // 
            this._partyTypeColumn.AttributeName = "PartyType";
            this._partyTypeColumn.AutoUpdateQuery = "";
            this._partyTypeColumn.FillWeight = 80F;
            this._partyTypeColumn.HeaderText = "Party Type";
            this._partyTypeColumn.Name = "_partyTypeColumn";
            this._partyTypeColumn.ValidationErrorMessage = "Party type is not valid for the current document type.";
            this._partyTypeColumn.ValidationQuery = resources.GetString("_partyTypeColumn.ValidationQuery");
            // 
            // _partyMappedTo
            // 
            this._partyMappedTo.AttributeName = "MappedTo";
            this._partyMappedTo.FillWeight = 45F;
            this._partyMappedTo.HeaderText = "Mapped To";
            this._partyMappedTo.Name = "_partyMappedTo";
            this._partyMappedTo.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._partyMappedTo.UseComboBoxCells = true;
            this._partyMappedTo.ValidationErrorMessage = "Please specify either Grantor or Grantee";
            this._partyMappedTo.ValidationQuery = "Grantor\r\nGrantee";
            // 
            // _legalDescriptionTextBox
            // 
            this._legalDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._legalDescriptionTextBox.AttributeName = "FullText";
            this._legalDescriptionTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._legalDescriptionTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._legalDescriptionTextBox.AutoUpdateQuery = "";
            this._legalDescriptionTextBox.Location = new System.Drawing.Point(3, 469);
            this._legalDescriptionTextBox.Multiline = true;
            this._legalDescriptionTextBox.Name = "_legalDescriptionTextBox";
            this._legalDescriptionTextBox.ParentDataEntryControl = this._legalDescriptionTable;
            this._legalDescriptionTextBox.RemoveNewLineChars = false;
            this._legalDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._legalDescriptionTextBox.Size = new System.Drawing.Size(615, 87);
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
            this._legalDescriptionTable.Location = new System.Drawing.Point(3, 395);
            this._legalDescriptionTable.MinimumNumberOfRows = 1;
            this._legalDescriptionTable.Name = "_legalDescriptionTable";
            this._legalDescriptionTable.ParentDataEntryControl = null;
            this._legalDescriptionTable.RowFormattingRuleFile = "..\\Rules\\RubberbandSplitters\\LegalDescription.rsd.etf";
            this._legalDescriptionTable.RowSwipingEnabled = true;
            this._legalDescriptionTable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._legalDescriptionTable.Size = new System.Drawing.Size(615, 68);
            this._legalDescriptionTable.TabIndex = 7;
            // 
            // Lot
            // 
            this.Lot.AttributeName = "Lot";
            this.Lot.FillWeight = 30F;
            this.Lot.HeaderText = "Lot";
            this.Lot.Name = "Lot";
            this.Lot.ValidationErrorMessage = "";
            // 
            // Block
            // 
            this.Block.AttributeName = "Block";
            this.Block.FillWeight = 30F;
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
            this._considerationAmountLabel.AutoSize = true;
            this._considerationAmountLabel.Location = new System.Drawing.Point(154, 3);
            this._considerationAmountLabel.Name = "_considerationAmountLabel";
            this._considerationAmountLabel.Size = new System.Drawing.Size(110, 13);
            this._considerationAmountLabel.TabIndex = 5;
            this._considerationAmountLabel.Text = "Consideration Amount";
            // 
            // _docTypeLabel
            // 
            this._docTypeLabel.AutoSize = true;
            this._docTypeLabel.Location = new System.Drawing.Point(308, 3);
            this._docTypeLabel.Name = "_docTypeLabel";
            this._docTypeLabel.Size = new System.Drawing.Size(83, 13);
            this._docTypeLabel.TabIndex = 6;
            this._docTypeLabel.Text = "Document Type";
            // 
            // _considerationAmountTextBox
            // 
            this._considerationAmountTextBox.AttributeName = "ConsiderationAmount";
            this._considerationAmountTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._considerationAmountTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._considerationAmountTextBox.Location = new System.Drawing.Point(157, 19);
            this._considerationAmountTextBox.Name = "_considerationAmountTextBox";
            this._considerationAmountTextBox.Size = new System.Drawing.Size(148, 20);
            this._considerationAmountTextBox.TabIndex = 2;
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
            this._returnAddressTable.Location = new System.Drawing.Point(3, 45);
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
            this._returnAddressTable.Size = new System.Drawing.Size(615, 134);
            this._returnAddressTable.TabIndex = 4;
            this._returnAddressTable.TableFormattingRuleFile = "..\\Rules\\RubberbandSplitters\\ReturnAddress.rsd.etf";
            this._returnAddressTable.TableSwipingEnabled = true;
            // 
            // _parcelNumberLabel
            // 
            this._parcelNumberLabel.AutoSize = true;
            this._parcelNumberLabel.Location = new System.Drawing.Point(0, 3);
            this._parcelNumberLabel.Name = "_parcelNumberLabel";
            this._parcelNumberLabel.Size = new System.Drawing.Size(77, 13);
            this._parcelNumberLabel.TabIndex = 3;
            this._parcelNumberLabel.Text = "Parcel Number";
            // 
            // _parcelNumberTextBox
            // 
            this._parcelNumberTextBox.AttributeName = "ParcelNumber";
            this._parcelNumberTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._parcelNumberTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._parcelNumberTextBox.Location = new System.Drawing.Point(3, 19);
            this._parcelNumberTextBox.Name = "_parcelNumberTextBox";
            this._parcelNumberTextBox.Size = new System.Drawing.Size(148, 20);
            this._parcelNumberTextBox.TabIndex = 1;
            this._parcelNumberTextBox.ValidationErrorMessage = "Invalid value";
            // 
            // _grantorsLabel
            // 
            this._grantorsLabel.AutoSize = true;
            this._grantorsLabel.Location = new System.Drawing.Point(3, 192);
            this._grantorsLabel.Name = "_grantorsLabel";
            this._grantorsLabel.Size = new System.Drawing.Size(39, 13);
            this._grantorsLabel.TabIndex = 9;
            this._grantorsLabel.Text = "Parties";
            // 
            // _legalDescriptionLabel
            // 
            this._legalDescriptionLabel.AutoSize = true;
            this._legalDescriptionLabel.Location = new System.Drawing.Point(3, 379);
            this._legalDescriptionLabel.Name = "_legalDescriptionLabel";
            this._legalDescriptionLabel.Size = new System.Drawing.Size(89, 13);
            this._legalDescriptionLabel.TabIndex = 11;
            this._legalDescriptionLabel.Text = "Legal Description";
            // 
            // _documentTypeComboBox
            // 
            this._documentTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._documentTypeComboBox.AttributeName = "DocumentType";
            this._documentTypeComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._documentTypeComboBox.AutoUpdateQuery = "<Query Default=\'1\'>Unknown</Query>";
            this._documentTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._documentTypeComboBox.FormattingEnabled = true;
            this._documentTypeComboBox.Location = new System.Drawing.Point(311, 19);
            this._documentTypeComboBox.Name = "_documentTypeComboBox";
            this._documentTypeComboBox.Size = new System.Drawing.Size(306, 21);
            this._documentTypeComboBox.TabIndex = 3;
            this._documentTypeComboBox.ValidationErrorMessage = "";
            this._documentTypeComboBox.ValidationQuery = "<SQL>SELECT Name FROM DocumentType ORDER BY Name</SQL>Unknown";
            // 
            // DemoFlexIndexPanel
            // 
            this.ApplicationTitle = "FLEX Index Demo";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this._documentTypeComboBox);
            this.Controls.Add(this._legalDescriptionLabel);
            this.Controls.Add(this._legalDescriptionTable);
            this.Controls.Add(this._legalDescriptionTextBox);
            this.Controls.Add(this._grantorsLabel);
            this.Controls.Add(this._partiesTable);
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
            this.MinimumSize = new System.Drawing.Size(500, 543);
            this.Name = "DemoFlexIndexPanel";
            this.Size = new System.Drawing.Size(621, 561);
            ((System.ComponentModel.ISupportInitialize)(this._partiesTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._legalDescriptionTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._returnAddressTable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DataEntryTable _partiesTable;
        private DataEntryTextBox _legalDescriptionTextBox;
        private DataEntryTable _legalDescriptionTable;
        private System.Windows.Forms.Label _considerationAmountLabel;
        private System.Windows.Forms.Label _docTypeLabel;
        private DataEntryTextBox _considerationAmountTextBox;
        private DataEntryTwoColumnTable _returnAddressTable;
        private System.Windows.Forms.Label _parcelNumberLabel;
        private DataEntryTextBox _parcelNumberTextBox;
        private System.Windows.Forms.Label _grantorsLabel;
        private System.Windows.Forms.Label _legalDescriptionLabel;
        private DataEntryTableColumn Lot;
        private DataEntryTableColumn Block;
        private DataEntryTableColumn Subdivision;
        private DataEntryComboBox _documentTypeComboBox;
        private DataEntryTableColumn _partyFirstNameColumn;
        private DataEntryTableColumn _partyMiddleNameColumn;
        private DataEntryTableColumn _partyLastOrCompanyNameColumn;
        private DataEntryTableColumn _partyTypeColumn;
        private DataEntryTableColumn _partyMappedTo;
    }
}
