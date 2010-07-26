namespace Extract.FileActionManager.Forms
{
    partial class SetProcessingScheduleForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this._scheduleGrid = new System.Windows.Forms.DataGridView();
            this._sunday = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._monday = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._tuesday = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._wednesday = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._thursday = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._friday = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._saturday = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._selectAllButton = new System.Windows.Forms.Button();
            this._selectNoneButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._scheduleGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(412, 32);
            label1.TabIndex = 1;
            label1.Text = "Select individual cells or a range of cells in the schedule below to indicate whe" +
                "n processing should be active.  Green indicates that processing will be active.";
            // 
            // _scheduleGrid
            // 
            this._scheduleGrid.AllowUserToAddRows = false;
            this._scheduleGrid.AllowUserToDeleteRows = false;
            this._scheduleGrid.AllowUserToResizeColumns = false;
            this._scheduleGrid.AllowUserToResizeRows = false;
            this._scheduleGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this._scheduleGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
            this._scheduleGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedHeaders;
            this._scheduleGrid.BackgroundColor = System.Drawing.SystemColors.Control;
            this._scheduleGrid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._scheduleGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this._scheduleGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this._scheduleGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._sunday,
            this._monday,
            this._tuesday,
            this._wednesday,
            this._thursday,
            this._friday,
            this._saturday});
            this._scheduleGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._scheduleGrid.Location = new System.Drawing.Point(12, 44);
            this._scheduleGrid.Name = "_scheduleGrid";
            this._scheduleGrid.ReadOnly = true;
            this._scheduleGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this._scheduleGrid.RowTemplate.Height = 18;
            this._scheduleGrid.RowTemplate.ReadOnly = true;
            this._scheduleGrid.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this._scheduleGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this._scheduleGrid.ShowCellErrors = false;
            this._scheduleGrid.ShowCellToolTips = false;
            this._scheduleGrid.ShowEditingIcon = false;
            this._scheduleGrid.ShowRowErrors = false;
            this._scheduleGrid.Size = new System.Drawing.Size(412, 433);
            this._scheduleGrid.TabIndex = 0;
            this._scheduleGrid.TabStop = false;
            this._scheduleGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.HandleDataGridErrorEvent);
            this._scheduleGrid.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.HandleScheduleGridRowPrePaint);
            this._scheduleGrid.MouseUp += new System.Windows.Forms.MouseEventHandler(this.HandleGridMouseUp);
            // 
            // _sunday
            // 
            this._sunday.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._sunday.HeaderText = "Sun";
            this._sunday.Name = "_sunday";
            this._sunday.ReadOnly = true;
            this._sunday.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this._sunday.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // _monday
            // 
            this._monday.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._monday.HeaderText = "Mon";
            this._monday.Name = "_monday";
            this._monday.ReadOnly = true;
            this._monday.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this._monday.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // _tuesday
            // 
            this._tuesday.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._tuesday.HeaderText = "Tue";
            this._tuesday.Name = "_tuesday";
            this._tuesday.ReadOnly = true;
            this._tuesday.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this._tuesday.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // _wednesday
            // 
            this._wednesday.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._wednesday.HeaderText = "Wed";
            this._wednesday.Name = "_wednesday";
            this._wednesday.ReadOnly = true;
            this._wednesday.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this._wednesday.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // _thursday
            // 
            this._thursday.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._thursday.HeaderText = "Thu";
            this._thursday.Name = "_thursday";
            this._thursday.ReadOnly = true;
            this._thursday.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // _friday
            // 
            this._friday.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._friday.HeaderText = "Fri";
            this._friday.Name = "_friday";
            this._friday.ReadOnly = true;
            this._friday.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // _saturday
            // 
            this._saturday.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._saturday.HeaderText = "Sat";
            this._saturday.Name = "_saturday";
            this._saturday.ReadOnly = true;
            this._saturday.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this._saturday.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // _okButton
            // 
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(430, 44);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 2;
            this._okButton.Text = "Ok";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(430, 73);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 3;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _selectAllButton
            // 
            this._selectAllButton.Location = new System.Drawing.Point(430, 132);
            this._selectAllButton.Name = "_selectAllButton";
            this._selectAllButton.Size = new System.Drawing.Size(75, 23);
            this._selectAllButton.TabIndex = 4;
            this._selectAllButton.Text = "Select all";
            this._selectAllButton.UseVisualStyleBackColor = true;
            this._selectAllButton.Click += new System.EventHandler(this.HandleSelectAllClicked);
            // 
            // _selectNoneButton
            // 
            this._selectNoneButton.Location = new System.Drawing.Point(430, 161);
            this._selectNoneButton.Name = "_selectNoneButton";
            this._selectNoneButton.Size = new System.Drawing.Size(75, 23);
            this._selectNoneButton.TabIndex = 5;
            this._selectNoneButton.Text = "Select none";
            this._selectNoneButton.UseVisualStyleBackColor = true;
            this._selectNoneButton.Click += new System.EventHandler(this.HandleSelectNoneClicked);
            // 
            // SetProcessingScheduleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(517, 489);
            this.Controls.Add(this._selectNoneButton);
            this.Controls.Add(this._selectAllButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(label1);
            this.Controls.Add(this._scheduleGrid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SetProcessingScheduleForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Set Processing Schedule";
            ((System.ComponentModel.ISupportInitialize)(this._scheduleGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView _scheduleGrid;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _selectAllButton;
        private System.Windows.Forms.Button _selectNoneButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn _sunday;
        private System.Windows.Forms.DataGridViewTextBoxColumn _monday;
        private System.Windows.Forms.DataGridViewTextBoxColumn _tuesday;
        private System.Windows.Forms.DataGridViewTextBoxColumn _wednesday;
        private System.Windows.Forms.DataGridViewTextBoxColumn _thursday;
        private System.Windows.Forms.DataGridViewTextBoxColumn _friday;
        private System.Windows.Forms.DataGridViewTextBoxColumn _saturday;
    }
}