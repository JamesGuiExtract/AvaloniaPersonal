namespace Extract.FAMDBCounterManager
{
    partial class FAMDBCounterManagerForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label8;
            System.Windows.Forms.Label label9;
            System.Windows.Forms.Label label10;
            System.Windows.Forms.Label label11;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FAMDBCounterManagerForm));
            this._licenseStringTextBox = new System.Windows.Forms.TextBox();
            this._databaseIdTextBox = new System.Windows.Forms.TextBox();
            this._databaseServerTextBox = new System.Windows.Forms.TextBox();
            this._databaseNameTextBox = new System.Windows.Forms.TextBox();
            this._counterDataGridView = new System.Windows.Forms.DataGridView();
            this._counterIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._counterNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._counterPreviousValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._counterOperationColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this._counterApplyValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._counterChangeLogValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._counterValidityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._dateTimeStampTextBox = new System.Windows.Forms.TextBox();
            this._customerNameTextBox = new System.Windows.Forms.TextBox();
            this._commentsTextBox = new System.Windows.Forms.TextBox();
            this._generateCodeButton = new System.Windows.Forms.Button();
            this._databaseCreationTextBox = new System.Windows.Forms.TextBox();
            this._databaseRestoreTextBox = new System.Windows.Forms.TextBox();
            this._pasteLicenseStringButton = new System.Windows.Forms.Button();
            this._lastCounterUpdateTextBox = new System.Windows.Forms.TextBox();
            this._generateUnlockCodeRadioButton = new System.Windows.Forms.RadioButton();
            this._generateUpdateCodeRadioButton = new System.Windows.Forms.RadioButton();
            this._counterStateTextBox = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            label11 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._counterDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(117, 13);
            label1.TabIndex = 0;
            label1.Text = "Database license string";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 225);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(67, 13);
            label2.TabIndex = 7;
            label2.Text = "Database ID";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 145);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(85, 13);
            label3.TabIndex = 3;
            label3.Text = "Database server";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(12, 185);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(82, 13);
            label4.TabIndex = 5;
            label4.Text = "Database name";
            // 
            // label6
            // 
            label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(342, 265);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(94, 13);
            label6.TabIndex = 17;
            label6.Text = "Counter data as of";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(11, 305);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(80, 13);
            label7.TabIndex = 19;
            label7.Text = "Customer name";
            // 
            // label8
            // 
            label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(11, 489);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(56, 13);
            label8.TabIndex = 24;
            label8.Text = "Comments";
            // 
            // label9
            // 
            label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(341, 145);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(92, 13);
            label9.TabIndex = 11;
            label9.Text = "Database created";
            // 
            // label10
            // 
            label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(342, 185);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(109, 13);
            label10.TabIndex = 13;
            label10.Text = "Last database restore";
            // 
            // label11
            // 
            label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(342, 225);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(102, 13);
            label11.TabIndex = 15;
            label11.Text = "Last counter update";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(12, 264);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(103, 13);
            label5.TabIndex = 9;
            label5.Text = "Counter data validity";
            // 
            // _licenseStringTextBox
            // 
            this._licenseStringTextBox.AllowDrop = true;
            this._licenseStringTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._licenseStringTextBox.Location = new System.Drawing.Point(15, 26);
            this._licenseStringTextBox.Multiline = true;
            this._licenseStringTextBox.Name = "_licenseStringTextBox";
            this._licenseStringTextBox.ReadOnly = true;
            this._licenseStringTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._licenseStringTextBox.Size = new System.Drawing.Size(655, 89);
            this._licenseStringTextBox.TabIndex = 1;
            this._licenseStringTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.HandleLicenseStringTextBox_DragDrop);
            this._licenseStringTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.HandleLicenseStringTextBox_DragEnter);
            // 
            // _databaseIdTextBox
            // 
            this._databaseIdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._databaseIdTextBox.Location = new System.Drawing.Point(15, 241);
            this._databaseIdTextBox.Name = "_databaseIdTextBox";
            this._databaseIdTextBox.ReadOnly = true;
            this._databaseIdTextBox.Size = new System.Drawing.Size(324, 20);
            this._databaseIdTextBox.TabIndex = 8;
            this._databaseIdTextBox.TabStop = false;
            // 
            // _databaseServerTextBox
            // 
            this._databaseServerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._databaseServerTextBox.Location = new System.Drawing.Point(15, 161);
            this._databaseServerTextBox.Name = "_databaseServerTextBox";
            this._databaseServerTextBox.ReadOnly = true;
            this._databaseServerTextBox.Size = new System.Drawing.Size(324, 20);
            this._databaseServerTextBox.TabIndex = 4;
            this._databaseServerTextBox.TabStop = false;
            // 
            // _databaseNameTextBox
            // 
            this._databaseNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._databaseNameTextBox.Location = new System.Drawing.Point(15, 201);
            this._databaseNameTextBox.Name = "_databaseNameTextBox";
            this._databaseNameTextBox.ReadOnly = true;
            this._databaseNameTextBox.Size = new System.Drawing.Size(324, 20);
            this._databaseNameTextBox.TabIndex = 6;
            this._databaseNameTextBox.TabStop = false;
            // 
            // _counterDataGridView
            // 
            this._counterDataGridView.AllowUserToResizeRows = false;
            this._counterDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._counterDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._counterDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._counterDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._counterIdColumn,
            this._counterNameColumn,
            this._counterPreviousValueColumn,
            this._counterOperationColumn,
            this._counterApplyValueColumn,
            this._counterChangeLogValueColumn,
            this._counterValidityColumn});
            this._counterDataGridView.Enabled = false;
            this._counterDataGridView.Location = new System.Drawing.Point(15, 370);
            this._counterDataGridView.Name = "_counterDataGridView";
            this._counterDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._counterDataGridView.Size = new System.Drawing.Size(655, 116);
            this._counterDataGridView.TabIndex = 23;
            this._counterDataGridView.VirtualMode = true;
            this._counterDataGridView.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.HandleCounterDataGridView_CellBeginEdit);
            this._counterDataGridView.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.HandleCounterDataGridView_CellValueNeeded);
            this._counterDataGridView.CellValuePushed += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.HandleCounterDataGridView_CellValuePushed);
            this._counterDataGridView.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.HandleCounterDataGridView_EditingControlShowing);
            this._counterDataGridView.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.HandleCounterDataGridView_RowsAdded);
            this._counterDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.HandleCounterDataGridView_UserDeletingRow);
            // 
            // _counterIdColumn
            // 
            this._counterIdColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle1.NullValue = null;
            this._counterIdColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this._counterIdColumn.FillWeight = 20F;
            this._counterIdColumn.HeaderText = "ID";
            this._counterIdColumn.MinimumWidth = 40;
            this._counterIdColumn.Name = "_counterIdColumn";
            this._counterIdColumn.Width = 40;
            // 
            // _counterNameColumn
            // 
            this._counterNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._counterNameColumn.FillWeight = 125F;
            this._counterNameColumn.HeaderText = "Name";
            this._counterNameColumn.Name = "_counterNameColumn";
            // 
            // _counterPreviousValueColumn
            // 
            this._counterPreviousValueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._counterPreviousValueColumn.FillWeight = 45F;
            this._counterPreviousValueColumn.HeaderText = "Value";
            this._counterPreviousValueColumn.MinimumWidth = 90;
            this._counterPreviousValueColumn.Name = "_counterPreviousValueColumn";
            // 
            // _counterOperationColumn
            // 
            this._counterOperationColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._counterOperationColumn.FillWeight = 44F;
            this._counterOperationColumn.HeaderText = "Operation";
            this._counterOperationColumn.Items.AddRange(new object[] {
            "Create",
            "Delete",
            "Set",
            "Increment",
            "Decrement"});
            this._counterOperationColumn.MinimumWidth = 90;
            this._counterOperationColumn.Name = "_counterOperationColumn";
            this._counterOperationColumn.Width = 98;
            // 
            // _counterApplyValueColumn
            // 
            this._counterApplyValueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle2.NullValue = null;
            this._counterApplyValueColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this._counterApplyValueColumn.FillWeight = 45F;
            this._counterApplyValueColumn.HeaderText = "Apply Value";
            this._counterApplyValueColumn.MinimumWidth = 90;
            this._counterApplyValueColumn.Name = "_counterApplyValueColumn";
            this._counterApplyValueColumn.Width = 95;
            // 
            // _counterChangeLogValueColumn
            // 
            this._counterChangeLogValueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._counterChangeLogValueColumn.FillWeight = 45F;
            this._counterChangeLogValueColumn.HeaderText = "Value Log";
            this._counterChangeLogValueColumn.MinimumWidth = 90;
            this._counterChangeLogValueColumn.Name = "_counterChangeLogValueColumn";
            this._counterChangeLogValueColumn.Visible = false;
            this._counterChangeLogValueColumn.Width = 90;
            // 
            // _counterValidityColumn
            // 
            this._counterValidityColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this._counterValidityColumn.FillWeight = 125F;
            this._counterValidityColumn.HeaderText = "Validity";
            this._counterValidityColumn.MinimumWidth = 150;
            this._counterValidityColumn.Name = "_counterValidityColumn";
            this._counterValidityColumn.Visible = false;
            // 
            // _dateTimeStampTextBox
            // 
            this._dateTimeStampTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._dateTimeStampTextBox.Location = new System.Drawing.Point(345, 281);
            this._dateTimeStampTextBox.Name = "_dateTimeStampTextBox";
            this._dateTimeStampTextBox.ReadOnly = true;
            this._dateTimeStampTextBox.Size = new System.Drawing.Size(324, 20);
            this._dateTimeStampTextBox.TabIndex = 18;
            this._dateTimeStampTextBox.TabStop = false;
            // 
            // _customerNameTextBox
            // 
            this._customerNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._customerNameTextBox.Enabled = false;
            this._customerNameTextBox.Location = new System.Drawing.Point(15, 321);
            this._customerNameTextBox.Name = "_customerNameTextBox";
            this._customerNameTextBox.Size = new System.Drawing.Size(655, 20);
            this._customerNameTextBox.TabIndex = 20;
            // 
            // _commentsTextBox
            // 
            this._commentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._commentsTextBox.Enabled = false;
            this._commentsTextBox.Location = new System.Drawing.Point(15, 505);
            this._commentsTextBox.Multiline = true;
            this._commentsTextBox.Name = "_commentsTextBox";
            this._commentsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._commentsTextBox.Size = new System.Drawing.Size(654, 55);
            this._commentsTextBox.TabIndex = 25;
            // 
            // _generateCodeButton
            // 
            this._generateCodeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._generateCodeButton.Enabled = false;
            this._generateCodeButton.Location = new System.Drawing.Point(544, 566);
            this._generateCodeButton.Name = "_generateCodeButton";
            this._generateCodeButton.Size = new System.Drawing.Size(125, 23);
            this._generateCodeButton.TabIndex = 26;
            this._generateCodeButton.Text = "Generate Code";
            this._generateCodeButton.UseVisualStyleBackColor = true;
            this._generateCodeButton.Click += new System.EventHandler(this.HandleGenerateCodeButton_Click);
            // 
            // _databaseCreationTextBox
            // 
            this._databaseCreationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._databaseCreationTextBox.Location = new System.Drawing.Point(345, 161);
            this._databaseCreationTextBox.Name = "_databaseCreationTextBox";
            this._databaseCreationTextBox.ReadOnly = true;
            this._databaseCreationTextBox.Size = new System.Drawing.Size(324, 20);
            this._databaseCreationTextBox.TabIndex = 12;
            this._databaseCreationTextBox.TabStop = false;
            // 
            // _databaseRestoreTextBox
            // 
            this._databaseRestoreTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._databaseRestoreTextBox.Location = new System.Drawing.Point(345, 201);
            this._databaseRestoreTextBox.Name = "_databaseRestoreTextBox";
            this._databaseRestoreTextBox.ReadOnly = true;
            this._databaseRestoreTextBox.Size = new System.Drawing.Size(324, 20);
            this._databaseRestoreTextBox.TabIndex = 14;
            this._databaseRestoreTextBox.TabStop = false;
            // 
            // _pasteLicenseStringButton
            // 
            this._pasteLicenseStringButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pasteLicenseStringButton.Location = new System.Drawing.Point(544, 121);
            this._pasteLicenseStringButton.Name = "_pasteLicenseStringButton";
            this._pasteLicenseStringButton.Size = new System.Drawing.Size(125, 23);
            this._pasteLicenseStringButton.TabIndex = 2;
            this._pasteLicenseStringButton.Text = "Paste License String";
            this._pasteLicenseStringButton.UseVisualStyleBackColor = true;
            this._pasteLicenseStringButton.Click += new System.EventHandler(this.HandlePasteLicenseStringButton_Click);
            // 
            // _lastCounterUpdateTextBox
            // 
            this._lastCounterUpdateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._lastCounterUpdateTextBox.Location = new System.Drawing.Point(345, 241);
            this._lastCounterUpdateTextBox.Name = "_lastCounterUpdateTextBox";
            this._lastCounterUpdateTextBox.ReadOnly = true;
            this._lastCounterUpdateTextBox.Size = new System.Drawing.Size(324, 20);
            this._lastCounterUpdateTextBox.TabIndex = 16;
            this._lastCounterUpdateTextBox.TabStop = false;
            // 
            // _generateUnlockCodeRadioButton
            // 
            this._generateUnlockCodeRadioButton.AutoSize = true;
            this._generateUnlockCodeRadioButton.Enabled = false;
            this._generateUnlockCodeRadioButton.Location = new System.Drawing.Point(192, 347);
            this._generateUnlockCodeRadioButton.Name = "_generateUnlockCodeRadioButton";
            this._generateUnlockCodeRadioButton.Size = new System.Drawing.Size(131, 17);
            this._generateUnlockCodeRadioButton.TabIndex = 22;
            this._generateUnlockCodeRadioButton.TabStop = true;
            this._generateUnlockCodeRadioButton.Text = "Generate unlock code";
            this._generateUnlockCodeRadioButton.UseVisualStyleBackColor = true;
            this._generateUnlockCodeRadioButton.CheckedChanged += new System.EventHandler(this.HandleRadioButton_CheckedChanged);
            // 
            // _generateUpdateCodeRadioButton
            // 
            this._generateUpdateCodeRadioButton.AutoSize = true;
            this._generateUpdateCodeRadioButton.Enabled = false;
            this._generateUpdateCodeRadioButton.Location = new System.Drawing.Point(15, 347);
            this._generateUpdateCodeRadioButton.Name = "_generateUpdateCodeRadioButton";
            this._generateUpdateCodeRadioButton.Size = new System.Drawing.Size(171, 17);
            this._generateUpdateCodeRadioButton.TabIndex = 21;
            this._generateUpdateCodeRadioButton.TabStop = true;
            this._generateUpdateCodeRadioButton.Text = "Generate counter update code";
            this._generateUpdateCodeRadioButton.UseVisualStyleBackColor = true;
            this._generateUpdateCodeRadioButton.CheckedChanged += new System.EventHandler(this.HandleRadioButton_CheckedChanged);
            // 
            // _counterStateTextBox
            // 
            this._counterStateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._counterStateTextBox.Location = new System.Drawing.Point(15, 281);
            this._counterStateTextBox.Name = "_counterStateTextBox";
            this._counterStateTextBox.ReadOnly = true;
            this._counterStateTextBox.Size = new System.Drawing.Size(324, 20);
            this._counterStateTextBox.TabIndex = 10;
            this._counterStateTextBox.TabStop = false;
            // 
            // FAMDBCounterManagerForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(682, 601);
            this.Controls.Add(label5);
            this.Controls.Add(this._counterStateTextBox);
            this.Controls.Add(this._generateUpdateCodeRadioButton);
            this.Controls.Add(this._generateUnlockCodeRadioButton);
            this.Controls.Add(this._lastCounterUpdateTextBox);
            this.Controls.Add(label11);
            this.Controls.Add(this._pasteLicenseStringButton);
            this.Controls.Add(this._databaseRestoreTextBox);
            this.Controls.Add(label10);
            this.Controls.Add(this._databaseCreationTextBox);
            this.Controls.Add(label9);
            this.Controls.Add(this._generateCodeButton);
            this.Controls.Add(this._commentsTextBox);
            this.Controls.Add(label8);
            this.Controls.Add(this._customerNameTextBox);
            this.Controls.Add(label7);
            this.Controls.Add(this._dateTimeStampTextBox);
            this.Controls.Add(label6);
            this.Controls.Add(this._counterDataGridView);
            this.Controls.Add(this._databaseNameTextBox);
            this.Controls.Add(label4);
            this.Controls.Add(this._databaseServerTextBox);
            this.Controls.Add(label3);
            this.Controls.Add(this._databaseIdTextBox);
            this.Controls.Add(label2);
            this.Controls.Add(this._licenseStringTextBox);
            this.Controls.Add(label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(594, 600);
            this.Name = "FAMDBCounterManagerForm";
            this.Text = "FAM DB Counter Manager";
            ((System.ComponentModel.ISupportInitialize)(this._counterDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _licenseStringTextBox;
        private System.Windows.Forms.TextBox _databaseIdTextBox;
        private System.Windows.Forms.TextBox _databaseServerTextBox;
        private System.Windows.Forms.TextBox _databaseNameTextBox;
        private System.Windows.Forms.DataGridView _counterDataGridView;
        private System.Windows.Forms.TextBox _dateTimeStampTextBox;
        private System.Windows.Forms.TextBox _customerNameTextBox;
        private System.Windows.Forms.TextBox _commentsTextBox;
        private System.Windows.Forms.Button _generateCodeButton;
        private System.Windows.Forms.TextBox _databaseCreationTextBox;
        private System.Windows.Forms.TextBox _databaseRestoreTextBox;
        private System.Windows.Forms.Button _pasteLicenseStringButton;
        private System.Windows.Forms.TextBox _lastCounterUpdateTextBox;
        private System.Windows.Forms.RadioButton _generateUnlockCodeRadioButton;
        private System.Windows.Forms.RadioButton _generateUpdateCodeRadioButton;
        private System.Windows.Forms.TextBox _counterStateTextBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn _counterIdColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _counterNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _counterPreviousValueColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn _counterOperationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _counterApplyValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _counterChangeLogValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _counterValidityColumn;
    }
}

