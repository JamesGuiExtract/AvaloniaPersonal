namespace Extract.SQLCDBEditor
{
    partial class ImportTableForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.SelectDataFileButton = new System.Windows.Forms.Button();
            this.SelectTableToImportDataIntoComboBox = new System.Windows.Forms.ComboBox();
            this.ImportButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SelectDataFileTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ResultsGridView = new System.Windows.Forms.DataGridView();
            this.cancelButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ReplaceRadioButton = new System.Windows.Forms.RadioButton();
            this.AppendRadioButton = new System.Windows.Forms.RadioButton();
            this.ColumnNotImportedTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.ResultsGridView)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // SelectDataFileButton
            // 
            this.SelectDataFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectDataFileButton.Location = new System.Drawing.Point(829, 27);
            this.SelectDataFileButton.Name = "SelectDataFileButton";
            this.SelectDataFileButton.Size = new System.Drawing.Size(24, 20);
            this.SelectDataFileButton.TabIndex = 1;
            this.SelectDataFileButton.Text = "...";
            this.SelectDataFileButton.UseVisualStyleBackColor = true;
            this.SelectDataFileButton.Click += new System.EventHandler(this.HandleSelectDataFileButton);
            // 
            // SelectTableToImportDataIntoComboBox
            // 
            this.SelectTableToImportDataIntoComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectTableToImportDataIntoComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SelectTableToImportDataIntoComboBox.DropDownWidth = 450;
            this.SelectTableToImportDataIntoComboBox.FormattingEnabled = true;
            this.SelectTableToImportDataIntoComboBox.Location = new System.Drawing.Point(12, 74);
            this.SelectTableToImportDataIntoComboBox.MaxDropDownItems = 20;
            this.SelectTableToImportDataIntoComboBox.Name = "SelectTableToImportDataIntoComboBox";
            this.SelectTableToImportDataIntoComboBox.Size = new System.Drawing.Size(841, 21);
            this.SelectTableToImportDataIntoComboBox.TabIndex = 2;
            this.SelectTableToImportDataIntoComboBox.SelectionChangeCommitted += new System.EventHandler(this.HandleSelectTableToImportDataInto_SelectionChangeCommitted);
            // 
            // ImportButton
            // 
            this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ImportButton.Location = new System.Drawing.Point(695, 542);
            this.ImportButton.Name = "ImportButton";
            this.ImportButton.Size = new System.Drawing.Size(75, 23);
            this.ImportButton.TabIndex = 3;
            this.ImportButton.Text = "Import";
            this.ImportButton.UseVisualStyleBackColor = true;
            this.ImportButton.Click += new System.EventHandler(this.HandleImportDataToDatabaseClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Select table to import data into";
            // 
            // SelectDataFileTextBox
            // 
            this.SelectDataFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectDataFileTextBox.Location = new System.Drawing.Point(12, 27);
            this.SelectDataFileTextBox.Name = "SelectDataFileTextBox";
            this.SelectDataFileTextBox.Size = new System.Drawing.Size(811, 20);
            this.SelectDataFileTextBox.TabIndex = 0;
            this.SelectDataFileTextBox.TextChanged += new System.EventHandler(this.HandleSelectDataFileTextBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Select data file";
            // 
            // ResultsGridView
            // 
            this.ResultsGridView.AllowUserToResizeRows = false;
            this.ResultsGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ResultsGridView.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.ResultsGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ResultsGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.ResultsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.ResultsGridView.DefaultCellStyle = dataGridViewCellStyle5;
            this.ResultsGridView.Location = new System.Drawing.Point(13, 108);
            this.ResultsGridView.Margin = new System.Windows.Forms.Padding(0);
            this.ResultsGridView.Name = "ResultsGridView";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ResultsGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.ResultsGridView.Size = new System.Drawing.Size(838, 349);
            this.ResultsGridView.TabIndex = 7;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(776, 542);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.ReplaceRadioButton);
            this.groupBox1.Controls.Add(this.AppendRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(10, 466);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(841, 64);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Import configuration";
            // 
            // ReplaceRadioButton
            // 
            this.ReplaceRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ReplaceRadioButton.AutoSize = true;
            this.ReplaceRadioButton.Checked = true;
            this.ReplaceRadioButton.Location = new System.Drawing.Point(6, 41);
            this.ReplaceRadioButton.Name = "ReplaceRadioButton";
            this.ReplaceRadioButton.Size = new System.Drawing.Size(262, 17);
            this.ReplaceRadioButton.TabIndex = 1;
            this.ReplaceRadioButton.TabStop = true;
            this.ReplaceRadioButton.Text = "Replace existing table contents with imported data";
            this.ReplaceRadioButton.UseVisualStyleBackColor = true;
            this.ReplaceRadioButton.CheckedChanged += new System.EventHandler(this.HandleRadioButton_CheckChanged);
            // 
            // AppendRadioButton
            // 
            this.AppendRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AppendRadioButton.AutoSize = true;
            this.AppendRadioButton.Location = new System.Drawing.Point(6, 19);
            this.AppendRadioButton.Name = "AppendRadioButton";
            this.AppendRadioButton.Size = new System.Drawing.Size(249, 17);
            this.AppendRadioButton.TabIndex = 0;
            this.AppendRadioButton.Text = "Append imported data to existing table contents";
            this.AppendRadioButton.UseVisualStyleBackColor = true;
            this.AppendRadioButton.CheckedChanged += new System.EventHandler(this.HandleRadioButton_CheckChanged);
            // 
            // ColumnNotImportedTextBox
            // 
            this.ColumnNotImportedTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ColumnNotImportedTextBox.ForeColor = System.Drawing.Color.Red;
            this.ColumnNotImportedTextBox.Location = new System.Drawing.Point(12, 545);
            this.ColumnNotImportedTextBox.Name = "ColumnNotImportedTextBox";
            this.ColumnNotImportedTextBox.ReadOnly = true;
            this.ColumnNotImportedTextBox.Size = new System.Drawing.Size(677, 13);
            this.ColumnNotImportedTextBox.TabIndex = 8;
            this.ColumnNotImportedTextBox.Visible = false;
            // 
            // ImportTableForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(864, 577);
            this.Controls.Add(this.ColumnNotImportedTextBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.ResultsGridView);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SelectDataFileTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ImportButton);
            this.Controls.Add(this.SelectTableToImportDataIntoComboBox);
            this.Controls.Add(this.SelectDataFileButton);
            this.MaximumSize = new System.Drawing.Size(1400, 1000);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(312, 480);
            this.Name = "ImportTableForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import data into database table";
            ((System.ComponentModel.ISupportInitialize)(this.ResultsGridView)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button SelectDataFileButton;
        private System.Windows.Forms.ComboBox SelectTableToImportDataIntoComboBox;
        private System.Windows.Forms.Button ImportButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SelectDataFileTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView ResultsGridView;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton AppendRadioButton;
        private System.Windows.Forms.RadioButton ReplaceRadioButton;
        private System.Windows.Forms.TextBox ColumnNotImportedTextBox;
    }
}