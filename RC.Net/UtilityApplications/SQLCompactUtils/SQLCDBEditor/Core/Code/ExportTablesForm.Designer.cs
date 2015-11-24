namespace Extract.SQLCDBEditor
{
    partial class ExportTablesForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.SelectedOutputDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.SelectDirectoryButton = new System.Windows.Forms.Button();
            this.TablesGridView = new System.Windows.Forms.DataGridView();
            this.SelectColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.TableNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OutputFileColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ExportTablesButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.AddDoubleQuotesCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.TablesGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Selected output directory";
            // 
            // SelectedOutputDirectoryTextBox
            // 
            this.SelectedOutputDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectedOutputDirectoryTextBox.Location = new System.Drawing.Point(12, 28);
            this.SelectedOutputDirectoryTextBox.Name = "SelectedOutputDirectoryTextBox";
            this.SelectedOutputDirectoryTextBox.Size = new System.Drawing.Size(822, 20);
            this.SelectedOutputDirectoryTextBox.TabIndex = 0;
            // 
            // SelectDirectoryButton
            // 
            this.SelectDirectoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectDirectoryButton.Location = new System.Drawing.Point(840, 28);
            this.SelectDirectoryButton.Name = "SelectDirectoryButton";
            this.SelectDirectoryButton.Size = new System.Drawing.Size(24, 20);
            this.SelectDirectoryButton.TabIndex = 1;
            this.SelectDirectoryButton.Text = "...";
            this.SelectDirectoryButton.UseVisualStyleBackColor = true;
            this.SelectDirectoryButton.Click += new System.EventHandler(this.HandleSelectDirectoryButton_Click);
            // 
            // TablesGridView
            // 
            this.TablesGridView.AllowUserToAddRows = false;
            this.TablesGridView.AllowUserToDeleteRows = false;
            this.TablesGridView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TablesGridView.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.TablesGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.TablesGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SelectColumn,
            this.TableNameColumn,
            this.OutputFileColumn});
            this.TablesGridView.Location = new System.Drawing.Point(12, 61);
            this.TablesGridView.Name = "TablesGridView";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.TablesGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.TablesGridView.RowHeadersVisible = false;
            this.TablesGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TablesGridView.Size = new System.Drawing.Size(852, 375);
            this.TablesGridView.TabIndex = 2;
            // 
            // SelectColumn
            // 
            this.SelectColumn.DividerWidth = 1;
            this.SelectColumn.HeaderText = "";
            this.SelectColumn.Name = "SelectColumn";
            this.SelectColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.SelectColumn.Width = 50;
            // 
            // TableNameColumn
            // 
            this.TableNameColumn.DividerWidth = 1;
            this.TableNameColumn.HeaderText = "Table name";
            this.TableNameColumn.MinimumWidth = 100;
            this.TableNameColumn.Name = "TableNameColumn";
            this.TableNameColumn.ReadOnly = true;
            this.TableNameColumn.Width = 300;
            // 
            // OutputFileColumn
            // 
            this.OutputFileColumn.DividerWidth = 1;
            this.OutputFileColumn.HeaderText = "Output file name";
            this.OutputFileColumn.MinimumWidth = 200;
            this.OutputFileColumn.Name = "OutputFileColumn";
            this.OutputFileColumn.Width = 800;
            // 
            // ExportTablesButton
            // 
            this.ExportTablesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportTablesButton.Location = new System.Drawing.Point(668, 446);
            this.ExportTablesButton.Name = "ExportTablesButton";
            this.ExportTablesButton.Size = new System.Drawing.Size(95, 23);
            this.ExportTablesButton.TabIndex = 3;
            this.ExportTablesButton.Text = "Export tables";
            this.ExportTablesButton.UseVisualStyleBackColor = true;
            this.ExportTablesButton.Click += new System.EventHandler(this.HandleExportTables_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(769, 446);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(95, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Done";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // AddDoubleQuotesCheckBox
            // 
            this.AddDoubleQuotesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddDoubleQuotesCheckBox.AutoSize = true;
            this.AddDoubleQuotesCheckBox.Checked = true;
            this.AddDoubleQuotesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AddDoubleQuotesCheckBox.Enabled = false;
            this.AddDoubleQuotesCheckBox.Location = new System.Drawing.Point(12, 450);
            this.AddDoubleQuotesCheckBox.Name = "AddDoubleQuotesCheckBox";
            this.AddDoubleQuotesCheckBox.Size = new System.Drawing.Size(181, 17);
            this.AddDoubleQuotesCheckBox.TabIndex = 5;
            this.AddDoubleQuotesCheckBox.Text = "Add double quotes to text values";
            this.AddDoubleQuotesCheckBox.UseVisualStyleBackColor = true;
            this.AddDoubleQuotesCheckBox.Visible = false;
            // 
            // ExportTables
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(876, 481);
            this.Controls.Add(this.AddDoubleQuotesCheckBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ExportTablesButton);
            this.Controls.Add(this.TablesGridView);
            this.Controls.Add(this.SelectDirectoryButton);
            this.Controls.Add(this.SelectedOutputDirectoryTextBox);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1024, 519);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(892, 519);
            this.Name = "ExportTables";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export Tables";
            ((System.ComponentModel.ISupportInitialize)(this.TablesGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SelectedOutputDirectoryTextBox;
        private System.Windows.Forms.Button SelectDirectoryButton;
        private System.Windows.Forms.DataGridView TablesGridView;
        private System.Windows.Forms.Button ExportTablesButton;
        private System.Windows.Forms.DataGridViewCheckBoxColumn SelectColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn TableNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn OutputFileColumn;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox AddDoubleQuotesCheckBox;
    }
}