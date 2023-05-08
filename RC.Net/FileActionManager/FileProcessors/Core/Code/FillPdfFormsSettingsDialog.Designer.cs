namespace Extract.FileActionManager.FileProcessors
{
    partial class FillPdfFormsSettingsDialog
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
            Extract.Utilities.Forms.InfoTip _taskToolTip;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FillPdfFormsSettingsDialog));
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.FieldsToFillDataGrid = new System.Windows.Forms.DataGridView();
            this._browseToFillPdfKeyValues = new Extract.Utilities.Forms.BrowseButton();
            _taskToolTip = new Extract.Utilities.Forms.InfoTip();
            ((System.ComponentModel.ISupportInitialize)(this.FieldsToFillDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(285, 297);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 7;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this._okButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(366, 297);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 8;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // FieldsToFillDataGrid
            // 
            this.FieldsToFillDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FieldsToFillDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.FieldsToFillDataGrid.Location = new System.Drawing.Point(12, 41);
            this.FieldsToFillDataGrid.Name = "FieldsToFillDataGrid";
            this.FieldsToFillDataGrid.Size = new System.Drawing.Size(429, 241);
            this.FieldsToFillDataGrid.TabIndex = 19;
            // 
            // _browseToFillPdfKeyValues
            // 
            this._browseToFillPdfKeyValues.EnsureFileExists = true;
            this._browseToFillPdfKeyValues.EnsurePathExists = false;
            this._browseToFillPdfKeyValues.FileFilter = "PDF Files (*.pdf)|*.pdf|All files (*.*)|*.*";
            this._browseToFillPdfKeyValues.Location = new System.Drawing.Point(12, 12);
            this._browseToFillPdfKeyValues.Name = "_browseToFillPdfKeyValues";
            this._browseToFillPdfKeyValues.Size = new System.Drawing.Size(247, 20);
            this._browseToFillPdfKeyValues.TabIndex = 20;
            this._browseToFillPdfKeyValues.Text = "Load PDF Fields Into Grid Below";
            this._browseToFillPdfKeyValues.UseVisualStyleBackColor = true;
            this._browseToFillPdfKeyValues.PathSelected += new System.EventHandler<Extract.Utilities.Forms.PathSelectedEventArgs>(this.BrowseToFillPdf_PathSelected);
            // 
            // _taskToolTip
            // 
            _taskToolTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            _taskToolTip.BackColor = System.Drawing.Color.Transparent;
            _taskToolTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_taskToolTip.BackgroundImage")));
            _taskToolTip.Location = new System.Drawing.Point(425, 12);
            _taskToolTip.Name = "_taskToolTip";
            _taskToolTip.Size = new System.Drawing.Size(16, 16);
            _taskToolTip.TabIndex = 21;
            _taskToolTip.TabStop = false;
            _taskToolTip.TipText = @"Values may contain:
- Path tag expressions
- Attribute references in the format </AttributeQuery>
(where 'AttributeQuery' is the path to an attribute in <SourceDocName>.voa)
- Data query expresssions in the format <Query>...</Query>";
            // 
            // FillPdfFormsSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(453, 332);
            this.Controls.Add(_taskToolTip);
            this.Controls.Add(this._browseToFillPdfKeyValues);
            this.Controls.Add(this.FieldsToFillDataGrid);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(469, 370);
            this.Name = "FillPdfFormsSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Core: Fill PDF Forms";
            ((System.ComponentModel.ISupportInitialize)(this.FieldsToFillDataGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.DataGridView FieldsToFillDataGrid;
        private Extract.Utilities.Forms.BrowseButton _browseToFillPdfKeyValues;
    }
}