namespace Extract.Utilities.ContextTags
{
    partial class ContextEditingForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContextEditingForm));
            this._dataGridView = new System.Windows.Forms.DataGridView();
            this._idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._fpsFileDirColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._contextTableV1BindingSource = new System.Windows.Forms.BindingSource(this.components);
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._fpsFileDirInfoTip = new Extract.Utilities.Forms.InfoTip();
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._contextTableV1BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // _dataGridView
            // 
            this._dataGridView.AllowUserToResizeRows = false;
            this._dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataGridView.AutoGenerateColumns = false;
            this._dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this._dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._idColumn,
            this._nameColumn,
            this._fpsFileDirColumn});
            this._dataGridView.DataSource = this._contextTableV1BindingSource;
            this._dataGridView.Location = new System.Drawing.Point(0, 0);
            this._dataGridView.Name = "_dataGridView";
            this._dataGridView.Size = new System.Drawing.Size(647, 174);
            this._dataGridView.TabIndex = 0;
            // 
            // _idColumn
            // 
            this._idColumn.DataPropertyName = "ID";
            this._idColumn.HeaderText = "ID";
            this._idColumn.Name = "_idColumn";
            this._idColumn.Visible = false;
            this._idColumn.Width = 43;
            // 
            // _nameColumn
            // 
            this._nameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._nameColumn.DataPropertyName = "Name";
            this._nameColumn.HeaderText = "Name";
            this._nameColumn.Name = "_nameColumn";
            this._nameColumn.Width = 150;
            // 
            // _fpsFileDirColumn
            // 
            this._fpsFileDirColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._fpsFileDirColumn.DataPropertyName = "FPSFileDir";
            this._fpsFileDirColumn.HeaderText = "FPSFileDir";
            this._fpsFileDirColumn.Name = "_fpsFileDirColumn";
            // 
            // _contextTableV1BindingSource
            // 
            this._contextTableV1BindingSource.DataSource = typeof(Extract.Utilities.ContextTags.ContextTableV1);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(560, 180);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 3;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(479, 180);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 2;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _fpsFileDirInfoTip
            // 
            this._fpsFileDirInfoTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._fpsFileDirInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._fpsFileDirInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_fpsFileDirInfoTip.BackgroundImage")));
            this._fpsFileDirInfoTip.Location = new System.Drawing.Point(626, 3);
            this._fpsFileDirInfoTip.Name = "_fpsFileDirInfoTip";
            this._fpsFileDirInfoTip.Size = new System.Drawing.Size(16, 16);
            this._fpsFileDirInfoTip.TabIndex = 11;
            this._fpsFileDirInfoTip.TipText = resources.GetString("_fpsFileDirInfoTip.TipText");
            // 
            // ContextEditingForm
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(647, 215);
            this.Controls.Add(this._fpsFileDirInfoTip);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._dataGridView);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(250, 100);
            this.Name = "ContextEditingForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit contexts";
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._contextTableV1BindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView _dataGridView;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.BindingSource _contextTableV1BindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn _idColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _nameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fpsFileDirColumn;
        private Forms.InfoTip _fpsFileDirInfoTip;
    }
}