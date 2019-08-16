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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BarcodeFinderSettingsDialog));
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._configFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._micrSplitterTextBox = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.browseButton1 = new Extract.Utilities.Forms.BrowseButton();
            this._barcodeTypesDataGridView = new System.Windows.Forms.DataGridView();
            this._barcodeEnableColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._barcodeNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._barcodeImageColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this._defaultColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._passCountLabel = new System.Windows.Forms.Label();
            this.infoTip1 = new Extract.Utilities.Forms.InfoTip();
            this.label1 = new System.Windows.Forms.Label();
            this._selectAllButton = new System.Windows.Forms.Button();
            this._selectNoneButton = new System.Windows.Forms.Button();
            this._selectDefaultButton = new System.Windows.Forms.Button();
            this._inheritOCRParametersCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this._barcodeTypesDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(418, 628);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 8;
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
            this._cancelButton.TabIndex = 9;
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
            this._defaultColumn});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._barcodeTypesDataGridView.DefaultCellStyle = dataGridViewCellStyle1;
            this._barcodeTypesDataGridView.Location = new System.Drawing.Point(11, 65);
            this._barcodeTypesDataGridView.MultiSelect = false;
            this._barcodeTypesDataGridView.Name = "_barcodeTypesDataGridView";
            this._barcodeTypesDataGridView.RowHeadersVisible = false;
            this._barcodeTypesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._barcodeTypesDataGridView.Size = new System.Drawing.Size(562, 527);
            this._barcodeTypesDataGridView.TabIndex = 5;
            this._barcodeTypesDataGridView.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.HandleBarcodeTypesDataGridView_CellMouseUp);
            this._barcodeTypesDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleBarcodeTypesDataGridView_CellValueChanged);
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
            // _defaultColumn
            // 
            this._defaultColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this._defaultColumn.HeaderText = "Default";
            this._defaultColumn.Name = "_defaultColumn";
            this._defaultColumn.Width = 70;
            // 
            // _passCountLabel
            // 
            this._passCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._passCountLabel.AutoSize = true;
            this._passCountLabel.Location = new System.Drawing.Point(322, 604);
            this._passCountLabel.Name = "_passCountLabel";
            this._passCountLabel.Size = new System.Drawing.Size(252, 13);
            this._passCountLabel.TabIndex = 7;
            this._passCountLabel.Text = "The current configuration will require 10 passes";
            this._passCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // infoTip1
            // 
            this.infoTip1.BackColor = System.Drawing.Color.Transparent;
            this.infoTip1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip1.BackgroundImage")));
            this.infoTip1.Location = new System.Drawing.Point(228, 43);
            this.infoTip1.Name = "infoTip1";
            this.infoTip1.Size = new System.Drawing.Size(16, 16);
            this.infoTip1.TabIndex = 4;
            this.infoTip1.TipText = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(210, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Search for the following barcode types:";
            // 
            // _selectAllButton
            // 
            this._selectAllButton.Location = new System.Drawing.Point(13, 10);
            this._selectAllButton.Name = "_selectAllButton";
            this._selectAllButton.Size = new System.Drawing.Size(179, 23);
            this._selectAllButton.TabIndex = 0;
            this._selectAllButton.Text = "Select all";
            this._selectAllButton.UseVisualStyleBackColor = true;
            this._selectAllButton.Click += new System.EventHandler(this.Handle_SelectAllButton_Click);
            // 
            // _selectNoneButton
            // 
            this._selectNoneButton.Location = new System.Drawing.Point(201, 10);
            this._selectNoneButton.Name = "_selectNoneButton";
            this._selectNoneButton.Size = new System.Drawing.Size(179, 23);
            this._selectNoneButton.TabIndex = 1;
            this._selectNoneButton.Text = "Select none";
            this._selectNoneButton.UseVisualStyleBackColor = true;
            this._selectNoneButton.Click += new System.EventHandler(this.Handle_SelectNoneButton_Click);
            // 
            // _selectDefaultButton
            // 
            this._selectDefaultButton.Location = new System.Drawing.Point(389, 10);
            this._selectDefaultButton.Name = "_selectDefaultButton";
            this._selectDefaultButton.Size = new System.Drawing.Size(179, 23);
            this._selectDefaultButton.TabIndex = 2;
            this._selectDefaultButton.Text = "Select default";
            this._selectDefaultButton.UseVisualStyleBackColor = true;
            this._selectDefaultButton.Click += new System.EventHandler(this.Handle_SelectDefaultButton_Click);
            // 
            // _inheritOCRParametersCheckBox
            // 
            this._inheritOCRParametersCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._inheritOCRParametersCheckBox.AutoSize = true;
            this._inheritOCRParametersCheckBox.Location = new System.Drawing.Point(11, 603);
            this._inheritOCRParametersCheckBox.Name = "_inheritOCRParametersCheckBox";
            this._inheritOCRParametersCheckBox.Size = new System.Drawing.Size(204, 17);
            this._inheritOCRParametersCheckBox.TabIndex = 6;
            this._inheritOCRParametersCheckBox.Text = "Inherit OCR parameters (advanced)";
            this._inheritOCRParametersCheckBox.UseVisualStyleBackColor = true;
            // 
            // BarcodeFinderSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(586, 663);
            this.Controls.Add(this._inheritOCRParametersCheckBox);
            this.Controls.Add(this._selectDefaultButton);
            this.Controls.Add(this._selectNoneButton);
            this.Controls.Add(this._selectAllButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.infoTip1);
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
        private System.Windows.Forms.DataGridViewTextBoxColumn _defaultColumn;
        private Utilities.Forms.InfoTip infoTip1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _selectAllButton;
        private System.Windows.Forms.Button _selectNoneButton;
        private System.Windows.Forms.Button _selectDefaultButton;
        private System.Windows.Forms.CheckBox _inheritOCRParametersCheckBox;
    }
}
