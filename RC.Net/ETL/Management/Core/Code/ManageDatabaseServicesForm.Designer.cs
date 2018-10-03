namespace Extract.ETL.Management
{
    partial class ManageDatabaseServicesForm
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
            this._closeButton = new System.Windows.Forms.Button();
            this._addButton = new System.Windows.Forms.Button();
            this._deleteButton = new System.Windows.Forms.Button();
            this._refreshButton = new System.Windows.Forms.Button();
            this._modifyButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this._databaseServicesDataGridView = new System.Windows.Forms.DataGridView();
            this._restartETLButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._databaseServicesDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // _closeButton
            // 
            this._closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._closeButton.Location = new System.Drawing.Point(588, 349);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(75, 23);
            this._closeButton.TabIndex = 7;
            this._closeButton.Text = "Close";
            this._closeButton.UseVisualStyleBackColor = true;
            // 
            // _addButton
            // 
            this._addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addButton.Location = new System.Drawing.Point(588, 23);
            this._addButton.Name = "_addButton";
            this._addButton.Size = new System.Drawing.Size(75, 23);
            this._addButton.TabIndex = 2;
            this._addButton.Text = "Add...";
            this._addButton.UseVisualStyleBackColor = true;
            this._addButton.Click += new System.EventHandler(this.HandleAddButtonClick);
            // 
            // _deleteButton
            // 
            this._deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._deleteButton.Location = new System.Drawing.Point(588, 83);
            this._deleteButton.Name = "_deleteButton";
            this._deleteButton.Size = new System.Drawing.Size(75, 23);
            this._deleteButton.TabIndex = 4;
            this._deleteButton.Text = "Delete";
            this._deleteButton.UseVisualStyleBackColor = true;
            this._deleteButton.Click += new System.EventHandler(this.HandleDeleteButtonClick);
            // 
            // _refreshButton
            // 
            this._refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._refreshButton.Location = new System.Drawing.Point(588, 112);
            this._refreshButton.Name = "_refreshButton";
            this._refreshButton.Size = new System.Drawing.Size(75, 23);
            this._refreshButton.TabIndex = 5;
            this._refreshButton.Text = "Refresh";
            this._refreshButton.UseVisualStyleBackColor = true;
            this._refreshButton.Click += new System.EventHandler(this.HandleRefreshButtonClick);
            // 
            // _modifyButton
            // 
            this._modifyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._modifyButton.Location = new System.Drawing.Point(588, 52);
            this._modifyButton.Name = "_modifyButton";
            this._modifyButton.Size = new System.Drawing.Size(75, 23);
            this._modifyButton.TabIndex = 3;
            this._modifyButton.Text = "Modify";
            this._modifyButton.UseVisualStyleBackColor = true;
            this._modifyButton.Click += new System.EventHandler(this.HandleModifyButtonClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Database Services";
            // 
            // _databaseServicesDataGridView
            // 
            this._databaseServicesDataGridView.AllowUserToAddRows = false;
            this._databaseServicesDataGridView.AllowUserToDeleteRows = false;
            this._databaseServicesDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._databaseServicesDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this._databaseServicesDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this._databaseServicesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._databaseServicesDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._databaseServicesDataGridView.Location = new System.Drawing.Point(16, 23);
            this._databaseServicesDataGridView.MultiSelect = false;
            this._databaseServicesDataGridView.Name = "_databaseServicesDataGridView";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._databaseServicesDataGridView.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this._databaseServicesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._databaseServicesDataGridView.Size = new System.Drawing.Size(566, 349);
            this._databaseServicesDataGridView.TabIndex = 1;
            this._databaseServicesDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleDatabaseServicesDataGridViewCellContentClick);
            this._databaseServicesDataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleDatabaseServicesDataGridViewCellDoubleClick);
            // 
            // _restartETLButton
            // 
            this._restartETLButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._restartETLButton.Location = new System.Drawing.Point(588, 320);
            this._restartETLButton.Name = "_restartETLButton";
            this._restartETLButton.Size = new System.Drawing.Size(75, 23);
            this._restartETLButton.TabIndex = 6;
            this._restartETLButton.Text = "Restart ETL";
            this._restartETLButton.UseVisualStyleBackColor = true;
            this._restartETLButton.Click += new System.EventHandler(this.HandleRestartETLButtonClick);
            // 
            // ManageDatabaseServicesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._closeButton;
            this.ClientSize = new System.Drawing.Size(669, 384);
            this.Controls.Add(this._databaseServicesDataGridView);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._refreshButton);
            this.Controls.Add(this._deleteButton);
            this.Controls.Add(this._modifyButton);
            this.Controls.Add(this._addButton);
            this.Controls.Add(this._restartETLButton);
            this.Controls.Add(this._closeButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(660, 214);
            this.Name = "ManageDatabaseServicesForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manage database services";
            ((System.ComponentModel.ISupportInitialize)(this._databaseServicesDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _closeButton;
        private System.Windows.Forms.Button _addButton;
        private System.Windows.Forms.Button _deleteButton;
        private System.Windows.Forms.Button _refreshButton;
        private System.Windows.Forms.Button _modifyButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView _databaseServicesDataGridView;
        private System.Windows.Forms.Button _restartETLButton;
    }
}