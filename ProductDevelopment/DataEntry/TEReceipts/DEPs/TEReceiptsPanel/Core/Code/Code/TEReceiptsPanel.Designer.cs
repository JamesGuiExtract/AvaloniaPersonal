namespace Extract.DataEntry.DEP.TEReceipts
{
    partial class TEReceiptsPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TEReceiptsPanel));
            Extract.DataEntry.HighlightColor highlightColor1 = new Extract.DataEntry.HighlightColor();
            Extract.DataEntry.HighlightColor highlightColor2 = new Extract.DataEntry.HighlightColor();
            this._commentLabel = new System.Windows.Forms.Label();
            this._commentTextBox = new Extract.DataEntry.DataEntryTextBox();
            this._expensesTable = new Extract.DataEntry.DataEntryTable();
            this._expensesLabel = new System.Windows.Forms.Label();
            this._startDateColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._endDateColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._categoryColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._subCategoryColumn = new Extract.DataEntry.DataEntryTableColumn();
            this._amountColumn = new Extract.DataEntry.DataEntryTableColumn();
            ((System.ComponentModel.ISupportInitialize)(this._expensesTable)).BeginInit();
            this.SuspendLayout();
            // 
            // _commentLabel
            // 
            this._commentLabel.AutoSize = true;
            this._commentLabel.Location = new System.Drawing.Point(3, 5);
            this._commentLabel.Name = "_commentLabel";
            this._commentLabel.Size = new System.Drawing.Size(51, 13);
            this._commentLabel.TabIndex = 0;
            this._commentLabel.Text = "Comment";
            // 
            // _commentTextBox
            // 
            this._commentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._commentTextBox.AttributeName = "Comment";
            this._commentTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._commentTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._commentTextBox.Location = new System.Drawing.Point(3, 21);
            this._commentTextBox.Multiline = true;
            this._commentTextBox.Name = "_commentTextBox";
            this._commentTextBox.RemoveNewLineChars = false;
            this._commentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._commentTextBox.Size = new System.Drawing.Size(560, 151);
            this._commentTextBox.TabIndex = 1;
            this._commentTextBox.TabStopMode = Extract.DataEntry.TabStopMode.Never;
            this._commentTextBox.ValidationErrorMessage = "";
            // 
            // _expensesTable
            // 
            this._expensesTable.AllowTabbingByRow = true;
            this._expensesTable.AllowUserToResizeRows = false;
            this._expensesTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._expensesTable.AttributeName = "Expense";
            this._expensesTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._expensesTable.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this._expensesTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._expensesTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._startDateColumn,
            this._endDateColumn,
            this._categoryColumn,
            this._subCategoryColumn,
            this._amountColumn});
            this._expensesTable.Location = new System.Drawing.Point(3, 200);
            this._expensesTable.Name = "_expensesTable";
            this._expensesTable.ParentDataEntryControl = null;
            this._expensesTable.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this._expensesTable.Size = new System.Drawing.Size(560, 382);
            this._expensesTable.TabIndex = 2;
            // 
            // label1
            // 
            this._expensesLabel.AutoSize = true;
            this._expensesLabel.Location = new System.Drawing.Point(3, 184);
            this._expensesLabel.Name = "label1";
            this._expensesLabel.Size = new System.Drawing.Size(53, 13);
            this._expensesLabel.TabIndex = 3;
            this._expensesLabel.Text = "Expenses";
            // 
            // _startDateColumn
            // 
            this._startDateColumn.AttributeName = "StartDate";
            this._startDateColumn.HeaderText = "Start Date";
            this._startDateColumn.Name = "_startDateColumn";
            this._startDateColumn.ValidationErrorMessage = "Date must be a valid date in the format MM/DD/YYYY";
            this._startDateColumn.ValidationPattern = "^((0?[1-9])|(1[0-2]))/((0?[1-9])|(1[0-9])|(2[0-9])|(3[01]))/(19|20)\\d{2}$";
            // 
            // _endDateColumn
            // 
            this._endDateColumn.AttributeName = "EndDate";
            this._endDateColumn.HeaderText = "End Date";
            this._endDateColumn.Name = "_endDateColumn";
            this._endDateColumn.ValidationErrorMessage = "Date must be a valid date in the format MM/DD/YYYY";
            this._endDateColumn.ValidationPattern = "(^$)|(^((0?[1-9])|(1[0-2]))/((0?[1-9])|(1[0-9])|(2[0-9])|(3[01]))/(19|20)\\d{2}$)";
            // 
            // _categoryColumn
            // 
            this._categoryColumn.AttributeName = "Category";
            this._categoryColumn.HeaderText = "Category";
            this._categoryColumn.Name = "_categoryColumn";
            this._categoryColumn.UseComboBoxCells = true;
            this._categoryColumn.ValidationErrorMessage = "Specify a category";
            this._categoryColumn.ValidationQuery = "<Query ValidationListType=\'AutoCompleteOnly\'>[BLANK]<SQL>SELECT * FROM [Category]" +
                "</SQL></Query>\r\n<Query ValidationListType=\'ValidationListOnly\'><SQL>SELECT * FRO" +
                "M [Category]</SQL></Query>";
            // 
            // _subCategoryColumn
            // 
            this._subCategoryColumn.AttributeName = "Subcategory";
            this._subCategoryColumn.AutoUpdateQuery = "<SQL>Select \' \' WHERE <Attribute>.</Attribute> NOT IN (SELECT [SubCategory] FROM " +
                "[SubCategory] WHERE [Category] = <Attribute>../Category</Attribute>)</SQL>";
            this._subCategoryColumn.HeaderText = "Subcategory";
            this._subCategoryColumn.Name = "_subCategoryColumn";
            this._subCategoryColumn.UseComboBoxCells = true;
            this._subCategoryColumn.ValidationErrorMessage = "Specify a subcategory";
            this._subCategoryColumn.ValidationQuery = resources.GetString("_subCategoryColumn.ValidationQuery");
            // 
            // _amountColumn
            // 
            this._amountColumn.AttributeName = "Amount";
            this._amountColumn.HeaderText = "Amount";
            this._amountColumn.Name = "_amountColumn";
            this._amountColumn.ValidationErrorMessage = "Please specify a valid amount";
            this._amountColumn.ValidationPattern = "^\\$?\\d+([\\.]\\d{2})?$";
            this._amountColumn.ValidationQuery = "";
            // 
            // TEReceiptsPanel
            // 
            this.ApplicationDescription = "Travel and Expense Receipts";
            this.ApplicationTitle = "Travel and Expense Receipts";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._expensesLabel);
            this.Controls.Add(this._expensesTable);
            this.Controls.Add(this._commentTextBox);
            this.Controls.Add(this._commentLabel);
            highlightColor1.Color = System.Drawing.Color.LightSalmon;
            highlightColor1.MaxOcrConfidence = 89;
            highlightColor2.Color = System.Drawing.Color.LightGreen;
            highlightColor2.MaxOcrConfidence = 100;
            this.HighlightColors = new Extract.DataEntry.HighlightColor[] {
        highlightColor1,
        highlightColor2};
            this.Name = "TEReceiptsPanel";
            this.Size = new System.Drawing.Size(566, 585);
            ((System.ComponentModel.ISupportInitialize)(this._expensesTable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _commentLabel;
        private DataEntryTextBox _commentTextBox;
        private DataEntryTable _expensesTable;
        private System.Windows.Forms.Label _expensesLabel;
        private DataEntryTableColumn _startDateColumn;
        private DataEntryTableColumn _endDateColumn;
        private DataEntryTableColumn _categoryColumn;
        private DataEntryTableColumn _subCategoryColumn;
        private DataEntryTableColumn _amountColumn;
    }
}
