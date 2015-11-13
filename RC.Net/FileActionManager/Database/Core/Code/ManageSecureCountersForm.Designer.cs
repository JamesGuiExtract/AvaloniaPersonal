namespace Extract.FileActionManager.Database
{
    partial class ManageSecureCountersForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this._counterDataGridView = new System.Windows.Forms.DataGridView();
            this._counterIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._counterNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._counterValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._counterAlertLevelColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._counterAlertMultipleColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._emailSupportCheckBox = new System.Windows.Forms.CheckBox();
            this._emailSpecifiedRecipientsCheckBox = new System.Windows.Forms.CheckBox();
            this._emailAlertRecipients = new System.Windows.Forms.TextBox();
            this._generateRequestButton = new System.Windows.Forms.Button();
            this._applyUpdateCodeButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._counterDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // _counterDataGridView
            // 
            this._counterDataGridView.AllowUserToAddRows = false;
            this._counterDataGridView.AllowUserToDeleteRows = false;
            this._counterDataGridView.AllowUserToResizeRows = false;
            this._counterDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._counterDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._counterDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this._counterDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._counterDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._counterIdColumn,
            this._counterNameColumn,
            this._counterValueColumn,
            this._counterAlertLevelColumn,
            this._counterAlertMultipleColumn});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._counterDataGridView.DefaultCellStyle = dataGridViewCellStyle3;
            this._counterDataGridView.Location = new System.Drawing.Point(12, 12);
            this._counterDataGridView.Name = "_counterDataGridView";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._counterDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this._counterDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._counterDataGridView.Size = new System.Drawing.Size(602, 114);
            this._counterDataGridView.TabIndex = 0;
            this._counterDataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.HandleCounterDataGridView_CellValidating);
            this._counterDataGridView.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.HandleCounterDataGridView_EditingControlShowing);
            // 
            // _counterIdColumn
            // 
            this._counterIdColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle2.NullValue = null;
            this._counterIdColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this._counterIdColumn.FillWeight = 20F;
            this._counterIdColumn.HeaderText = "ID";
            this._counterIdColumn.Name = "_counterIdColumn";
            this._counterIdColumn.ReadOnly = true;
            this._counterIdColumn.Width = 37;
            // 
            // _counterNameColumn
            // 
            this._counterNameColumn.HeaderText = "Name";
            this._counterNameColumn.Name = "_counterNameColumn";
            this._counterNameColumn.ReadOnly = true;
            // 
            // _counterValueColumn
            // 
            this._counterValueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._counterValueColumn.FillWeight = 1F;
            this._counterValueColumn.HeaderText = "Value";
            this._counterValueColumn.Name = "_counterValueColumn";
            this._counterValueColumn.ReadOnly = true;
            this._counterValueColumn.Width = 90;
            // 
            // _counterAlertLevelColumn
            // 
            this._counterAlertLevelColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._counterAlertLevelColumn.FillWeight = 1F;
            this._counterAlertLevelColumn.HeaderText = "Alert Level";
            this._counterAlertLevelColumn.Name = "_counterAlertLevelColumn";
            this._counterAlertLevelColumn.Width = 90;
            // 
            // _counterAlertMultipleColumn
            // 
            this._counterAlertMultipleColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._counterAlertMultipleColumn.FillWeight = 1F;
            this._counterAlertMultipleColumn.HeaderText = "Alert Multiple";
            this._counterAlertMultipleColumn.Name = "_counterAlertMultipleColumn";
            this._counterAlertMultipleColumn.Width = 90;
            // 
            // _emailSupportCheckBox
            // 
            this._emailSupportCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._emailSupportCheckBox.AutoSize = true;
            this._emailSupportCheckBox.Location = new System.Drawing.Point(13, 132);
            this._emailSupportCheckBox.Name = "_emailSupportCheckBox";
            this._emailSupportCheckBox.Size = new System.Drawing.Size(267, 17);
            this._emailSupportCheckBox.TabIndex = 1;
            this._emailSupportCheckBox.Text = "Enable email alerts to support@extractsystems.com";
            this._emailSupportCheckBox.UseVisualStyleBackColor = true;
            // 
            // _emailSpecifiedRecipientsCheckBox
            // 
            this._emailSpecifiedRecipientsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._emailSpecifiedRecipientsCheckBox.AutoSize = true;
            this._emailSpecifiedRecipientsCheckBox.Location = new System.Drawing.Point(13, 155);
            this._emailSpecifiedRecipientsCheckBox.Name = "_emailSpecifiedRecipientsCheckBox";
            this._emailSpecifiedRecipientsCheckBox.Size = new System.Drawing.Size(129, 17);
            this._emailSpecifiedRecipientsCheckBox.TabIndex = 2;
            this._emailSpecifiedRecipientsCheckBox.Text = "Enable email alerts to:";
            this._emailSpecifiedRecipientsCheckBox.UseVisualStyleBackColor = true;
            // 
            // _emailAlertRecipients
            // 
            this._emailAlertRecipients.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailAlertRecipients.Enabled = false;
            this._emailAlertRecipients.Location = new System.Drawing.Point(148, 152);
            this._emailAlertRecipients.Name = "_emailAlertRecipients";
            this._emailAlertRecipients.Size = new System.Drawing.Size(466, 20);
            this._emailAlertRecipients.TabIndex = 3;
            // 
            // _generateRequestButton
            // 
            this._generateRequestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._generateRequestButton.Location = new System.Drawing.Point(148, 179);
            this._generateRequestButton.Name = "_generateRequestButton";
            this._generateRequestButton.Size = new System.Drawing.Size(149, 23);
            this._generateRequestButton.TabIndex = 4;
            this._generateRequestButton.Text = "Generate update request";
            this._generateRequestButton.UseVisualStyleBackColor = true;
            this._generateRequestButton.Click += new System.EventHandler(this.HandleGenerateRequestButton_Click);
            // 
            // _applyUpdateCodeButton
            // 
            this._applyUpdateCodeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._applyUpdateCodeButton.Location = new System.Drawing.Point(303, 179);
            this._applyUpdateCodeButton.Name = "_applyUpdateCodeButton";
            this._applyUpdateCodeButton.Size = new System.Drawing.Size(149, 23);
            this._applyUpdateCodeButton.TabIndex = 5;
            this._applyUpdateCodeButton.Text = "Apply update code";
            this._applyUpdateCodeButton.UseVisualStyleBackColor = true;
            this._applyUpdateCodeButton.Click += new System.EventHandler(this.HandleApplyUpdateCodeButton_Click);
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(458, 179);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 6;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(539, 179);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 7;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // ManageSecureCountersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._okButton;
            this.ClientSize = new System.Drawing.Size(626, 214);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._applyUpdateCodeButton);
            this.Controls.Add(this._generateRequestButton);
            this.Controls.Add(this._emailAlertRecipients);
            this.Controls.Add(this._emailSpecifiedRecipientsCheckBox);
            this.Controls.Add(this._emailSupportCheckBox);
            this.Controls.Add(this._counterDataGridView);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(535, 240);
            this.Name = "ManageSecureCountersForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manage Rule Execution Counters";
            ((System.ComponentModel.ISupportInitialize)(this._counterDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView _counterDataGridView;
        private System.Windows.Forms.CheckBox _emailSupportCheckBox;
        private System.Windows.Forms.CheckBox _emailSpecifiedRecipientsCheckBox;
        private System.Windows.Forms.TextBox _emailAlertRecipients;
        private System.Windows.Forms.Button _generateRequestButton;
        private System.Windows.Forms.Button _applyUpdateCodeButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn _counterIdColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _counterNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _counterValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _counterAlertLevelColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _counterAlertMultipleColumn;
        private System.Windows.Forms.Button _cancelButton;
    }
}