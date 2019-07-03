namespace Extract.AttributeFinder.Rules
{
    partial class BarcodeFinderSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BarcodeFinderSettingsDialog));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._configFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._micrSplitterTextBox = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.browseButton1 = new Extract.Utilities.Forms.BrowseButton();
            this._barcodeTypesDataGridView = new System.Windows.Forms.DataGridView();
            this._passCountLabel = new System.Windows.Forms.Label();
            this._barcodeEnableColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._barcodeNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._barcodeImageColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this._passNumberColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._barcodeTypesDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            label1.Location = new System.Drawing.Point(13, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(561, 63);
            label1.TabIndex = 0;
            label1.Text = resources.GetString("label1.Text");
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(418, 628);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 3;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(499, 628);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 4;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _configFileNameBrowseButton
            // 
            this._configFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._configFileNameBrowseButton.EnsureFileExists = false;
            this._configFileNameBrowseButton.EnsurePathExists = false;
            this._configFileNameBrowseButton.Location = new System.Drawing.Point(490, 114);
            this._configFileNameBrowseButton.Name = "_configFileNameBrowseButton";
            this._configFileNameBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._configFileNameBrowseButton.TabIndex = 16;
            this._configFileNameBrowseButton.Text = "...";
            this._configFileNameBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _micrSplitterTextBox
            // 
            this._micrSplitterTextBox.Location = new System.Drawing.Point(15, 131);
            this._micrSplitterTextBox.Name = "_micrSplitterTextBox";
            this._micrSplitterTextBox.Size = new System.Drawing.Size(427, 20);
            this._micrSplitterTextBox.TabIndex = 18;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(15, 163);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(357, 17);
            this.checkBox1.TabIndex = 17;
            this.checkBox1.Text = "Remove special MICR chars and spaces from sub-attribute values";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // browseButton1
            // 
            this.browseButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton1.EnsureFileExists = false;
            this.browseButton1.EnsurePathExists = false;
            this.browseButton1.Location = new System.Drawing.Point(534, 131);
            this.browseButton1.Name = "browseButton1";
            this.browseButton1.Size = new System.Drawing.Size(27, 20);
            this.browseButton1.TabIndex = 9;
            this.browseButton1.Text = "...";
            this.browseButton1.UseVisualStyleBackColor = true;
            // 
            // _barcodeTypesDataGridView
            // 
            this._barcodeTypesDataGridView.AllowUserToAddRows = false;
            this._barcodeTypesDataGridView.AllowUserToDeleteRows = false;
            this._barcodeTypesDataGridView.AllowUserToResizeRows = false;
            this._barcodeTypesDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._barcodeTypesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._barcodeTypesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._barcodeEnableColumn,
            this._barcodeNameColumn,
            this._barcodeImageColumn,
            this._passNumberColumn});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._barcodeTypesDataGridView.DefaultCellStyle = dataGridViewCellStyle1;
            this._barcodeTypesDataGridView.Location = new System.Drawing.Point(12, 79);
            this._barcodeTypesDataGridView.MultiSelect = false;
            this._barcodeTypesDataGridView.Name = "_barcodeTypesDataGridView";
            this._barcodeTypesDataGridView.RowHeadersVisible = false;
            this._barcodeTypesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._barcodeTypesDataGridView.Size = new System.Drawing.Size(562, 508);
            this._barcodeTypesDataGridView.TabIndex = 1;
            this._barcodeTypesDataGridView.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.HandleBarcodeTypesDataGridView_CellMouseUp);
            this._barcodeTypesDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleBarcodeTypesDataGridView_CellValueChanged);
            // 
            // _passCountLabel
            // 
            this._passCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._passCountLabel.AutoSize = true;
            this._passCountLabel.Location = new System.Drawing.Point(322, 600);
            this._passCountLabel.Name = "_passCountLabel";
            this._passCountLabel.Size = new System.Drawing.Size(252, 13);
            this._passCountLabel.TabIndex = 2;
            this._passCountLabel.Text = "The current configuration will require 10 passes";
            this._passCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _barcodeEnableColumn
            // 
            this._barcodeEnableColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this._barcodeEnableColumn.FillWeight = 60F;
            this._barcodeEnableColumn.HeaderText = "";
            this._barcodeEnableColumn.Name = "_barcodeEnableColumn";
            this._barcodeEnableColumn.Width = 5;
            // 
            // _barcodeNameColumn
            // 
            this._barcodeNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._barcodeNameColumn.HeaderText = "Barcode Types";
            this._barcodeNameColumn.Name = "_barcodeNameColumn";
            this._barcodeNameColumn.ReadOnly = true;
            // 
            // _barcodeImageColumn
            // 
            this._barcodeImageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._barcodeImageColumn.FillWeight = 150F;
            this._barcodeImageColumn.HeaderText = "";
            this._barcodeImageColumn.Name = "_barcodeImageColumn";
            this._barcodeImageColumn.Visible = false;
            this._barcodeImageColumn.Width = 150;
            // 
            // _passNumberColumn
            // 
            this._passNumberColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this._passNumberColumn.HeaderText = "Pass";
            this._passNumberColumn.Name = "_passNumberColumn";
            this._passNumberColumn.Width = 54;
            // 
            // BarcodeFinderSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(586, 663);
            this.Controls.Add(label1);
            this.Controls.Add(this._passCountLabel);
            this.Controls.Add(this._barcodeTypesDataGridView);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BarcodeFinderSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Barcode finder settings";
            ((System.ComponentModel.ISupportInitialize)(this._barcodeTypesDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private Utilities.Forms.BrowseButton _configFileNameBrowseButton;
        private System.Windows.Forms.TextBox _micrSplitterTextBox;
        private System.Windows.Forms.CheckBox checkBox1;
        private Utilities.Forms.BrowseButton browseButton1;
        private System.Windows.Forms.DataGridView _barcodeTypesDataGridView;
        private System.Windows.Forms.Label _passCountLabel;
        private System.Windows.Forms.DataGridViewCheckBoxColumn _barcodeEnableColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _barcodeNameColumn;
        private System.Windows.Forms.DataGridViewImageColumn _barcodeImageColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _passNumberColumn;
    }
}