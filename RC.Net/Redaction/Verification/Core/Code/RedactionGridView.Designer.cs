namespace Extract.Redaction.Verification
{
    partial class RedactionGridView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="RedactionGridView"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="RedactionGridView"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources
                if (components != null)
                {
                    components.Dispose();
                }
                if (_exemptionsDialog != null)
	            {
                    _exemptionsDialog.Dispose();
                    _exemptionsDialog = null;
	            }
            }

            // Release unmanaged resources

            // Call base dispose method
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._dataGridView = new System.Windows.Forms.DataGridView();
            this._textColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._categoryColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._typeColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this._pageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._exemptionsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // _dataGridView
            // 
            this._dataGridView.AllowUserToAddRows = false;
            this._dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._textColumn,
            this._categoryColumn,
            this._typeColumn,
            this._pageColumn,
            this._exemptionsColumn});
            this._dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dataGridView.Location = new System.Drawing.Point(0, 0);
            this._dataGridView.Name = "_dataGridView";
            this._dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._dataGridView.Size = new System.Drawing.Size(575, 300);
            this._dataGridView.TabIndex = 0;
            this._dataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleDataGridViewCellDoubleClick);
            // 
            // _textColumn
            // 
            this._textColumn.DataPropertyName = "Text";
            this._textColumn.HeaderText = "Text";
            this._textColumn.MinimumWidth = 160;
            this._textColumn.Name = "_textColumn";
            this._textColumn.ReadOnly = true;
            // 
            // _categoryColumn
            // 
            this._categoryColumn.DataPropertyName = "Category";
            this._categoryColumn.FillWeight = 50F;
            this._categoryColumn.HeaderText = "Category";
            this._categoryColumn.MinimumWidth = 80;
            this._categoryColumn.Name = "_categoryColumn";
            this._categoryColumn.ReadOnly = true;
            // 
            // _typeColumn
            // 
            this._typeColumn.DataPropertyName = "RedactionType";
            this._typeColumn.FillWeight = 50F;
            this._typeColumn.HeaderText = "Type";
            this._typeColumn.MinimumWidth = 80;
            this._typeColumn.Name = "_typeColumn";
            // 
            // _pageColumn
            // 
            this._pageColumn.DataPropertyName = "PageNumber";
            this._pageColumn.FillWeight = 25F;
            this._pageColumn.HeaderText = "Page";
            this._pageColumn.MinimumWidth = 40;
            this._pageColumn.Name = "_pageColumn";
            this._pageColumn.ReadOnly = true;
            // 
            // _exemptionsColumn
            // 
            this._exemptionsColumn.DataPropertyName = "Exemptions";
            this._exemptionsColumn.HeaderText = "Exemptions";
            this._exemptionsColumn.MinimumWidth = 160;
            this._exemptionsColumn.Name = "_exemptionsColumn";
            this._exemptionsColumn.ReadOnly = true;
            // 
            // RedactionGridView
            // 
            this.Controls.Add(this._dataGridView);
            this.Name = "RedactionGridView";
            this.Size = new System.Drawing.Size(575, 300);
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView _dataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn _textColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _categoryColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn _typeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _pageColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _exemptionsColumn;
    }
}
