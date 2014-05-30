namespace Extract.FileActionManager.Conditions
{
    partial class DatabaseContentsConditionSettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label2;
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label label3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseContentsConditionSettingsDialog));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this._databaseConnectionControl = new Extract.Database.DatabaseConnectionControl();
            this._specifiedDbRadioButton = new System.Windows.Forms.RadioButton();
            this._useFAMDbRadioButton = new System.Windows.Forms.RadioButton();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._queryPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._queryTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._searchModifierComboBox = new System.Windows.Forms.ComboBox();
            this._checkFieldsRadioButton = new System.Windows.Forms.RadioButton();
            this._doNotCheckFieldsRadioButton = new System.Windows.Forms.RadioButton();
            this._fieldsPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._fieldsDataGridView = new System.Windows.Forms.DataGridView();
            this._queryFieldColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._queryValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._caseSensitiveColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._fuzzyColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._advancedButton = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._tableOrQueryComboBox = new System.Windows.Forms.ComboBox();
            this._tableComboBox = new System.Windows.Forms.ComboBox();
            this._doNotCheckFieldsRowCountComboBox = new System.Windows.Forms.ComboBox();
            this._checkFieldsRowCountComboBox = new System.Windows.Forms.ComboBox();
            label2 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label3 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._fieldsDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(322, 385);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(115, 13);
            label2.TabIndex = 12;
            label2.Text = "of the following values:";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._databaseConnectionControl);
            groupBox1.Controls.Add(this._specifiedDbRadioButton);
            groupBox1.Controls.Add(this._useFAMDbRadioButton);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(630, 165);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Database connection";
            // 
            // _databaseConnectionControl
            // 
            this._databaseConnectionControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._databaseConnectionControl.Location = new System.Drawing.Point(7, 66);
            this._databaseConnectionControl.Name = "_databaseConnectionControl";
            this._databaseConnectionControl.PathTags = null;
            this._databaseConnectionControl.ShowOpenDataFileMenuOption = false;
            this._databaseConnectionControl.Size = new System.Drawing.Size(617, 90);
            this._databaseConnectionControl.TabIndex = 2;
            this._databaseConnectionControl.ConnectionChanged += new System.EventHandler<System.EventArgs>(this.HandleDatabaseConnectionControl_ConnectionChanged);
            // 
            // _specifiedDbRadioButton
            // 
            this._specifiedDbRadioButton.AutoSize = true;
            this._specifiedDbRadioButton.Location = new System.Drawing.Point(6, 42);
            this._specifiedDbRadioButton.Name = "_specifiedDbRadioButton";
            this._specifiedDbRadioButton.Size = new System.Drawing.Size(213, 17);
            this._specifiedDbRadioButton.TabIndex = 1;
            this._specifiedDbRadioButton.TabStop = true;
            this._specifiedDbRadioButton.Text = "Use the specified database connection:";
            this._specifiedDbRadioButton.UseVisualStyleBackColor = true;
            this._specifiedDbRadioButton.CheckedChanged += new System.EventHandler(this.HandleSpecifiedDbRadioButton_CheckedChanged);
            // 
            // _useFAMDbRadioButton
            // 
            this._useFAMDbRadioButton.AutoSize = true;
            this._useFAMDbRadioButton.Location = new System.Drawing.Point(6, 19);
            this._useFAMDbRadioButton.Name = "_useFAMDbRadioButton";
            this._useFAMDbRadioButton.Size = new System.Drawing.Size(190, 17);
            this._useFAMDbRadioButton.TabIndex = 0;
            this._useFAMDbRadioButton.TabStop = true;
            this._useFAMDbRadioButton.Text = "Use the FAM database connection";
            this._useFAMDbRadioButton.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(204, 385);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(26, 13);
            label3.TabIndex = 10;
            label3.Text = "with";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(486, 557);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(74, 23);
            this._okButton.TabIndex = 16;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(568, 557);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(74, 23);
            this._cancelButton.TabIndex = 17;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _queryPathTagsButton
            // 
            this._queryPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._queryPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_queryPathTagsButton.Image")));
            this._queryPathTagsButton.Location = new System.Drawing.Point(624, 210);
            this._queryPathTagsButton.Name = "_queryPathTagsButton";
            this._queryPathTagsButton.Size = new System.Drawing.Size(18, 23);
            this._queryPathTagsButton.TabIndex = 5;
            this._queryPathTagsButton.TextControl = this._queryTextBox;
            this._queryPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _queryTextBox
            // 
            this._queryTextBox.AcceptsReturn = true;
            this._queryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._queryTextBox.Location = new System.Drawing.Point(12, 210);
            this._queryTextBox.Multiline = true;
            this._queryTextBox.Name = "_queryTextBox";
            this._queryTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._queryTextBox.Size = new System.Drawing.Size(606, 134);
            this._queryTextBox.TabIndex = 4;
            this._queryTextBox.TextChanged += new System.EventHandler(this.HandleQueryOrTable_TextChanged);
            this._queryTextBox.Leave += new System.EventHandler(this.HandleQueryOrTable_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 186);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(172, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Consider the condition as met if the";
            // 
            // _searchModifierComboBox
            // 
            this._searchModifierComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._searchModifierComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._searchModifierComboBox.FormattingEnabled = true;
            this._searchModifierComboBox.Location = new System.Drawing.Point(236, 382);
            this._searchModifierComboBox.Name = "_searchModifierComboBox";
            this._searchModifierComboBox.Size = new System.Drawing.Size(80, 21);
            this._searchModifierComboBox.TabIndex = 11;
            // 
            // _checkFieldsRadioButton
            // 
            this._checkFieldsRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._checkFieldsRadioButton.AutoSize = true;
            this._checkFieldsRadioButton.Location = new System.Drawing.Point(12, 383);
            this._checkFieldsRadioButton.Name = "_checkFieldsRadioButton";
            this._checkFieldsRadioButton.Size = new System.Drawing.Size(62, 17);
            this._checkFieldsRadioButton.TabIndex = 8;
            this._checkFieldsRadioButton.TabStop = true;
            this._checkFieldsRadioButton.Text = "Returns";
            this._checkFieldsRadioButton.UseVisualStyleBackColor = true;
            this._checkFieldsRadioButton.CheckedChanged += new System.EventHandler(this.HandleCheckFieldsRadioButton_CheckedChanged);
            // 
            // _doNotCheckFieldsRadioButton
            // 
            this._doNotCheckFieldsRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._doNotCheckFieldsRadioButton.AutoSize = true;
            this._doNotCheckFieldsRadioButton.Location = new System.Drawing.Point(12, 356);
            this._doNotCheckFieldsRadioButton.Name = "_doNotCheckFieldsRadioButton";
            this._doNotCheckFieldsRadioButton.Size = new System.Drawing.Size(62, 17);
            this._doNotCheckFieldsRadioButton.TabIndex = 6;
            this._doNotCheckFieldsRadioButton.TabStop = true;
            this._doNotCheckFieldsRadioButton.Text = "Returns";
            this._doNotCheckFieldsRadioButton.UseVisualStyleBackColor = true;
            // 
            // _fieldsPathTagsButton
            // 
            this._fieldsPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._fieldsPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_fieldsPathTagsButton.Image")));
            this._fieldsPathTagsButton.Location = new System.Drawing.Point(624, 409);
            this._fieldsPathTagsButton.Name = "_fieldsPathTagsButton";
            this._fieldsPathTagsButton.Size = new System.Drawing.Size(18, 23);
            this._fieldsPathTagsButton.TabIndex = 14;
            this._fieldsPathTagsButton.UseVisualStyleBackColor = true;
            this._fieldsPathTagsButton.TagSelecting += new System.EventHandler<Extract.Utilities.Forms.TagSelectingEventArgs>(this.HandleFieldsPathTagsButton_TagSelecting);
            // 
            // _fieldsDataGridView
            // 
            this._fieldsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._fieldsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this._fieldsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._fieldsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._queryFieldColumn,
            this._queryValueColumn,
            this._caseSensitiveColumn,
            this._fuzzyColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._fieldsDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this._fieldsDataGridView.Location = new System.Drawing.Point(12, 409);
            this._fieldsDataGridView.Name = "_fieldsDataGridView";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._fieldsDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this._fieldsDataGridView.Size = new System.Drawing.Size(606, 138);
            this._fieldsDataGridView.TabIndex = 13;
            this._fieldsDataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleFieldsDataGridView_CellEndEdit);
            this._fieldsDataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.HandleFieldsDataGridView_CellValidating);
            this._fieldsDataGridView.CurrentCellChanged += new System.EventHandler(this.HandleFieldsDataGridView_CurrentCellChanged);
            this._fieldsDataGridView.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.HandleFieldsDataGridView_EditingControlShowing);
            // 
            // _queryFieldColumn
            // 
            this._queryFieldColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._queryFieldColumn.FillWeight = 50F;
            this._queryFieldColumn.HeaderText = "Field";
            this._queryFieldColumn.Name = "_queryFieldColumn";
            // 
            // _queryValueColumn
            // 
            this._queryValueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._queryValueColumn.HeaderText = "Value";
            this._queryValueColumn.Name = "_queryValueColumn";
            // 
            // _caseSensitiveColumn
            // 
            this._caseSensitiveColumn.FillWeight = 90F;
            this._caseSensitiveColumn.HeaderText = "Case-sensitive?";
            this._caseSensitiveColumn.Name = "_caseSensitiveColumn";
            this._caseSensitiveColumn.Width = 90;
            // 
            // _fuzzyColumn
            // 
            this._fuzzyColumn.FillWeight = 50F;
            this._fuzzyColumn.HeaderText = "Fuzzy?";
            this._fuzzyColumn.Name = "_fuzzyColumn";
            this._fuzzyColumn.Width = 50;
            // 
            // _advancedButton
            // 
            this._advancedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._advancedButton.Location = new System.Drawing.Point(12, 557);
            this._advancedButton.Name = "_advancedButton";
            this._advancedButton.Size = new System.Drawing.Size(106, 23);
            this._advancedButton.TabIndex = 15;
            this._advancedButton.Text = "Advanced...";
            this._advancedButton.UseVisualStyleBackColor = true;
            this._advancedButton.Click += new System.EventHandler(this.HandleAdvancedButtonClick);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.FillWeight = 50F;
            this.dataGridViewTextBoxColumn1.HeaderText = "Field";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn2.HeaderText = "Value";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // _tableOrQueryComboBox
            // 
            this._tableOrQueryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._tableOrQueryComboBox.FormattingEnabled = true;
            this._tableOrQueryComboBox.Items.AddRange(new object[] {
            "Database table",
            "SQL Query"});
            this._tableOrQueryComboBox.Location = new System.Drawing.Point(191, 183);
            this._tableOrQueryComboBox.Name = "_tableOrQueryComboBox";
            this._tableOrQueryComboBox.Size = new System.Drawing.Size(122, 21);
            this._tableOrQueryComboBox.TabIndex = 2;
            this._tableOrQueryComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleTableOrQueryComboBox_SelectedIndexChanged);
            // 
            // _tableComboBox
            // 
            this._tableComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tableComboBox.FormattingEnabled = true;
            this._tableComboBox.Location = new System.Drawing.Point(319, 183);
            this._tableComboBox.Name = "_tableComboBox";
            this._tableComboBox.Size = new System.Drawing.Size(299, 21);
            this._tableComboBox.TabIndex = 3;
            this._tableComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleTableComboBox_SelectedIndexChanged);
            this._tableComboBox.TextChanged += new System.EventHandler(this.HandleQueryOrTable_TextChanged);
            this._tableComboBox.Leave += new System.EventHandler(this.HandleQueryOrTable_Leave);
            // 
            // _doNotCheckFieldsRowCountComboBox
            // 
            this._doNotCheckFieldsRowCountComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._doNotCheckFieldsRowCountComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._doNotCheckFieldsRowCountComboBox.FormattingEnabled = true;
            this._doNotCheckFieldsRowCountComboBox.Location = new System.Drawing.Point(80, 355);
            this._doNotCheckFieldsRowCountComboBox.Name = "_doNotCheckFieldsRowCountComboBox";
            this._doNotCheckFieldsRowCountComboBox.Size = new System.Drawing.Size(118, 21);
            this._doNotCheckFieldsRowCountComboBox.TabIndex = 7;
            // 
            // _checkFieldsRowCountComboBox
            // 
            this._checkFieldsRowCountComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._checkFieldsRowCountComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._checkFieldsRowCountComboBox.FormattingEnabled = true;
            this._checkFieldsRowCountComboBox.Location = new System.Drawing.Point(80, 382);
            this._checkFieldsRowCountComboBox.Name = "_checkFieldsRowCountComboBox";
            this._checkFieldsRowCountComboBox.Size = new System.Drawing.Size(118, 21);
            this._checkFieldsRowCountComboBox.TabIndex = 9;
            // 
            // DatabaseContentsConditionSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(654, 590);
            this.Controls.Add(label3);
            this.Controls.Add(this._checkFieldsRowCountComboBox);
            this.Controls.Add(this._doNotCheckFieldsRowCountComboBox);
            this.Controls.Add(this._tableComboBox);
            this.Controls.Add(this._tableOrQueryComboBox);
            this.Controls.Add(this._queryTextBox);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._advancedButton);
            this.Controls.Add(this._fieldsPathTagsButton);
            this.Controls.Add(this._fieldsDataGridView);
            this.Controls.Add(label2);
            this.Controls.Add(this._searchModifierComboBox);
            this.Controls.Add(this._checkFieldsRadioButton);
            this.Controls.Add(this._doNotCheckFieldsRadioButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._queryPathTagsButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(660, 453);
            this.Name = "DatabaseContentsConditionSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Database contents condition settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._fieldsDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private Utilities.Forms.PathTagsButton _queryPathTagsButton;
        private System.Windows.Forms.TextBox _queryTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox _searchModifierComboBox;
        private System.Windows.Forms.RadioButton _checkFieldsRadioButton;
        private System.Windows.Forms.RadioButton _doNotCheckFieldsRadioButton;
        private Utilities.Forms.PathTagsButton _fieldsPathTagsButton;
        private System.Windows.Forms.DataGridView _fieldsDataGridView;
        private System.Windows.Forms.Button _advancedButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.RadioButton _specifiedDbRadioButton;
        private System.Windows.Forms.RadioButton _useFAMDbRadioButton;
        private Extract.Database.DatabaseConnectionControl _databaseConnectionControl;
        private System.Windows.Forms.ComboBox _tableOrQueryComboBox;
        private System.Windows.Forms.ComboBox _tableComboBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn _queryFieldColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _queryValueColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn _caseSensitiveColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn _fuzzyColumn;
        private System.Windows.Forms.ComboBox _doNotCheckFieldsRowCountComboBox;
        private System.Windows.Forms.ComboBox _checkFieldsRowCountComboBox;
    }
}