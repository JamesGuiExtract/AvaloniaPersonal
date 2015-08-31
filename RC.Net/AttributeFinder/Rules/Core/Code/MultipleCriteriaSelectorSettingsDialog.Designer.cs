﻿namespace Extract.AttributeFinder.Rules
{
    partial class MultipleCriteriaSelectorSettingsDialog
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
            System.Windows.Forms.GroupBox groupBox1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultipleCriteriaSelectorSettingsDialog));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this._downButton = new Extract.Utilities.Forms.ExtractDownButton();
            this._upButton = new Extract.Utilities.Forms.ExtractUpButton();
            this._unionSelectedRadioButton = new System.Windows.Forms.RadioButton();
            this._chainSelectorsRadioButton = new System.Windows.Forms.RadioButton();
            this._deleteButton = new System.Windows.Forms.Button();
            this._commandButton = new System.Windows.Forms.Button();
            this._addButton = new System.Windows.Forms.Button();
            this._selectorDataGridView = new System.Windows.Forms.DataGridView();
            this._enabledColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._negatedColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._selectorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._selectorDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._downButton);
            groupBox1.Controls.Add(this._upButton);
            groupBox1.Controls.Add(this._unionSelectedRadioButton);
            groupBox1.Controls.Add(this._chainSelectorsRadioButton);
            groupBox1.Controls.Add(this._deleteButton);
            groupBox1.Controls.Add(this._commandButton);
            groupBox1.Controls.Add(this._addButton);
            groupBox1.Controls.Add(this._selectorDataGridView);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(470, 242);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Select attributes using the following selectors";
            // 
            // _downButton
            // 
            this._downButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._downButton.Image = ((System.Drawing.Image)(resources.GetObject("_downButton.Image")));
            this._downButton.Location = new System.Drawing.Point(429, 106);
            this._downButton.Name = "_downButton";
            this._downButton.Size = new System.Drawing.Size(35, 35);
            this._downButton.TabIndex = 5;
            this._downButton.UseVisualStyleBackColor = true;
            this._downButton.Click += new System.EventHandler(this.HandleDownClicked);
            // 
            // _upButton
            // 
            this._upButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._upButton.Image = ((System.Drawing.Image)(resources.GetObject("_upButton.Image")));
            this._upButton.Location = new System.Drawing.Point(375, 106);
            this._upButton.Name = "_upButton";
            this._upButton.Size = new System.Drawing.Size(35, 35);
            this._upButton.TabIndex = 4;
            this._upButton.UseVisualStyleBackColor = true;
            this._upButton.Click += new System.EventHandler(this.HandleUpClicked);
            // 
            // _unionSelectedRadioButton
            // 
            this._unionSelectedRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._unionSelectedRadioButton.AutoSize = true;
            this._unionSelectedRadioButton.Location = new System.Drawing.Point(6, 219);
            this._unionSelectedRadioButton.Name = "_unionSelectedRadioButton";
            this._unionSelectedRadioButton.Size = new System.Drawing.Size(370, 17);
            this._unionSelectedRadioButton.TabIndex = 7;
            this._unionSelectedRadioButton.TabStop = true;
            this._unionSelectedRadioButton.Text = "Union selected. (Collect the results of running each selector on the input.)";
            this._unionSelectedRadioButton.UseVisualStyleBackColor = true;
            // 
            // _chainSelectorsRadioButton
            // 
            this._chainSelectorsRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._chainSelectorsRadioButton.AutoSize = true;
            this._chainSelectorsRadioButton.Location = new System.Drawing.Point(6, 196);
            this._chainSelectorsRadioButton.Name = "_chainSelectorsRadioButton";
            this._chainSelectorsRadioButton.Size = new System.Drawing.Size(378, 17);
            this._chainSelectorsRadioButton.TabIndex = 6;
            this._chainSelectorsRadioButton.TabStop = true;
            this._chainSelectorsRadioButton.Text = "Chain selectors. (Run each selector on the results of the previous selector).";
            this._chainSelectorsRadioButton.UseVisualStyleBackColor = true;
            // 
            // _deleteButton
            // 
            this._deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._deleteButton.Location = new System.Drawing.Point(375, 47);
            this._deleteButton.Name = "_deleteButton";
            this._deleteButton.Size = new System.Drawing.Size(89, 23);
            this._deleteButton.TabIndex = 2;
            this._deleteButton.Text = "Delete";
            this._deleteButton.UseVisualStyleBackColor = true;
            this._deleteButton.Click += new System.EventHandler(this.HandleDeleteButtonClick);
            // 
            // _commandButton
            // 
            this._commandButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._commandButton.Location = new System.Drawing.Point(375, 77);
            this._commandButton.Name = "_commandButton";
            this._commandButton.Size = new System.Drawing.Size(89, 23);
            this._commandButton.TabIndex = 3;
            this._commandButton.Text = "Commands >";
            this._commandButton.UseVisualStyleBackColor = true;
            this._commandButton.Click += new System.EventHandler(this.HandleCommandsButtonClick);
            // 
            // _addButton
            // 
            this._addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addButton.Location = new System.Drawing.Point(375, 18);
            this._addButton.Name = "_addButton";
            this._addButton.Size = new System.Drawing.Size(89, 23);
            this._addButton.TabIndex = 1;
            this._addButton.Text = "Add...";
            this._addButton.UseVisualStyleBackColor = true;
            this._addButton.Click += new System.EventHandler(this.HandleAddButtonClick);
            // 
            // _selectorDataGridView
            // 
            this._selectorDataGridView.AllowUserToAddRows = false;
            this._selectorDataGridView.AllowUserToDeleteRows = false;
            this._selectorDataGridView.AllowUserToResizeRows = false;
            this._selectorDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._selectorDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this._selectorDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._selectorDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._enabledColumn,
            this._negatedColumn,
            this._selectorColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._selectorDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this._selectorDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._selectorDataGridView.Location = new System.Drawing.Point(6, 19);
            this._selectorDataGridView.MultiSelect = false;
            this._selectorDataGridView.Name = "_selectorDataGridView";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._selectorDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this._selectorDataGridView.RowHeadersVisible = false;
            this._selectorDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._selectorDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._selectorDataGridView.Size = new System.Drawing.Size(363, 169);
            this._selectorDataGridView.StandardTab = true;
            this._selectorDataGridView.TabIndex = 0;
            this._selectorDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleCellContentClick);
            this._selectorDataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleCellDoubleClick);
            this._selectorDataGridView.SelectionChanged += new System.EventHandler(this.HandleSelectionChanged);
            // 
            // _enabledColumn
            // 
            this._enabledColumn.FalseValue = "false";
            this._enabledColumn.HeaderText = "On";
            this._enabledColumn.Name = "_enabledColumn";
            this._enabledColumn.TrueValue = "true";
            this._enabledColumn.Width = 30;
            // 
            // _negatedColumn
            // 
            this._negatedColumn.FalseValue = "false";
            this._negatedColumn.HeaderText = "Not";
            this._negatedColumn.Name = "_negatedColumn";
            this._negatedColumn.TrueValue = "true";
            this._negatedColumn.Width = 30;
            // 
            // _selectorColumn
            // 
            this._selectorColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._selectorColumn.HeaderText = "Selector";
            this._selectorColumn.Name = "_selectorColumn";
            this._selectorColumn.ReadOnly = true;
            this._selectorColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(326, 267);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 1;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(407, 267);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // MultipleCriteriaSelectorSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(494, 302);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MultipleCriteriaSelectorSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Multiple criteria attribute selector settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._selectorDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.DataGridView _selectorDataGridView;
        private System.Windows.Forms.RadioButton _unionSelectedRadioButton;
        private System.Windows.Forms.RadioButton _chainSelectorsRadioButton;
        private System.Windows.Forms.Button _deleteButton;
        private System.Windows.Forms.Button _commandButton;
        private System.Windows.Forms.Button _addButton;
        private System.Windows.Forms.DataGridViewCheckBoxColumn _enabledColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn _negatedColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _selectorColumn;
        private Utilities.Forms.ExtractUpButton _upButton;
        private Utilities.Forms.ExtractDownButton _downButton;
    }
}