namespace Extract.Dashboard.Forms
{
    partial class ManageDashboardsForm
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
            this.dashboardDataGridView = new System.Windows.Forms.DataGridView();
            this._importDashboardButton = new System.Windows.Forms.Button();
            this._removeDashboardButton = new System.Windows.Forms.Button();
            this._closeButton = new System.Windows.Forms.Button();
            this._renameDashboardButton = new System.Windows.Forms.Button();
            this._viewButton = new System.Windows.Forms.Button();
            this._exportDashboardButton = new System.Windows.Forms.Button();
            this._replaceDashboardButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dashboardDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // dashboardDataGridView
            // 
            this.dashboardDataGridView.AllowUserToAddRows = false;
            this.dashboardDataGridView.AllowUserToDeleteRows = false;
            this.dashboardDataGridView.AllowUserToResizeRows = false;
            this.dashboardDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dashboardDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dashboardDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dashboardDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dashboardDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dashboardDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dashboardDataGridView.Location = new System.Drawing.Point(12, 12);
            this.dashboardDataGridView.MinimumSize = new System.Drawing.Size(434, 426);
            this.dashboardDataGridView.Name = "dashboardDataGridView";
            this.dashboardDataGridView.RowHeadersVisible = false;
            this.dashboardDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dashboardDataGridView.Size = new System.Drawing.Size(434, 426);
            this.dashboardDataGridView.TabIndex = 0;
            this.dashboardDataGridView.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.HandleDashboardGridViewCellBeginEdit);
            this.dashboardDataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleDashboardDataGridView_CellDoubleClick);
            this.dashboardDataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleDashboardGridViewCellEndEdit);
            // 
            // _importDashboardButton
            // 
            this._importDashboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._importDashboardButton.Location = new System.Drawing.Point(452, 129);
            this._importDashboardButton.Name = "_importDashboardButton";
            this._importDashboardButton.Size = new System.Drawing.Size(75, 23);
            this._importDashboardButton.TabIndex = 5;
            this._importDashboardButton.Text = "Import";
            this._importDashboardButton.UseVisualStyleBackColor = true;
            this._importDashboardButton.Click += new System.EventHandler(this.HandleImportDashboardButtonClick);
            // 
            // _removeDashboardButton
            // 
            this._removeDashboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._removeDashboardButton.Location = new System.Drawing.Point(453, 71);
            this._removeDashboardButton.Name = "_removeDashboardButton";
            this._removeDashboardButton.Size = new System.Drawing.Size(75, 23);
            this._removeDashboardButton.TabIndex = 3;
            this._removeDashboardButton.Text = "Remove";
            this._removeDashboardButton.UseVisualStyleBackColor = true;
            this._removeDashboardButton.Click += new System.EventHandler(this.HandleRemoveDashboardButtonClick);
            // 
            // _closeButton
            // 
            this._closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._closeButton.Location = new System.Drawing.Point(453, 414);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(75, 23);
            this._closeButton.TabIndex = 7;
            this._closeButton.Text = "Close";
            this._closeButton.UseVisualStyleBackColor = true;
            // 
            // _renameDashboardButton
            // 
            this._renameDashboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._renameDashboardButton.Location = new System.Drawing.Point(453, 42);
            this._renameDashboardButton.Name = "_renameDashboardButton";
            this._renameDashboardButton.Size = new System.Drawing.Size(75, 23);
            this._renameDashboardButton.TabIndex = 2;
            this._renameDashboardButton.Text = "Rename";
            this._renameDashboardButton.UseVisualStyleBackColor = true;
            this._renameDashboardButton.Click += new System.EventHandler(this.HandleRenameDashboardButtonClick);
            // 
            // _viewButton
            // 
            this._viewButton.Location = new System.Drawing.Point(453, 13);
            this._viewButton.Name = "_viewButton";
            this._viewButton.Size = new System.Drawing.Size(75, 23);
            this._viewButton.TabIndex = 1;
            this._viewButton.Text = "View";
            this._viewButton.UseVisualStyleBackColor = true;
            this._viewButton.Click += new System.EventHandler(this.HandleViewButtonClick);
            // 
            // _exportDashboardButton
            // 
            this._exportDashboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._exportDashboardButton.Location = new System.Drawing.Point(453, 158);
            this._exportDashboardButton.Name = "_exportDashboardButton";
            this._exportDashboardButton.Size = new System.Drawing.Size(75, 23);
            this._exportDashboardButton.TabIndex = 6;
            this._exportDashboardButton.Text = "Export";
            this._exportDashboardButton.UseVisualStyleBackColor = true;
            this._exportDashboardButton.Click += new System.EventHandler(this.HandleExportDashboardButton_Click);
            // 
            // _replaceDashboardButton
            // 
            this._replaceDashboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._replaceDashboardButton.Location = new System.Drawing.Point(453, 100);
            this._replaceDashboardButton.Name = "_replaceDashboardButton";
            this._replaceDashboardButton.Size = new System.Drawing.Size(75, 23);
            this._replaceDashboardButton.TabIndex = 4;
            this._replaceDashboardButton.Text = "Replace";
            this._replaceDashboardButton.UseVisualStyleBackColor = true;
            this._replaceDashboardButton.Click += new System.EventHandler(this.HandleReplaceDashboardButton_Click);
            // 
            // ManageDashboardsForm
            // 
            this.AcceptButton = this._closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._closeButton;
            this.ClientSize = new System.Drawing.Size(539, 450);
            this.Controls.Add(this._viewButton);
            this.Controls.Add(this._renameDashboardButton);
            this.Controls.Add(this._closeButton);
            this.Controls.Add(this._exportDashboardButton);
            this.Controls.Add(this._replaceDashboardButton);
            this.Controls.Add(this._removeDashboardButton);
            this.Controls.Add(this._importDashboardButton);
            this.Controls.Add(this.dashboardDataGridView);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ManageDashboardsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manage Dashboards";
            this.Load += new System.EventHandler(this.HandleManageDashboardsFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.dashboardDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dashboardDataGridView;
        private System.Windows.Forms.Button _importDashboardButton;
        private System.Windows.Forms.Button _removeDashboardButton;
        private System.Windows.Forms.Button _closeButton;
        private System.Windows.Forms.Button _renameDashboardButton;
        private System.Windows.Forms.Button _viewButton;
        private System.Windows.Forms.Button _exportDashboardButton;
        private System.Windows.Forms.Button _replaceDashboardButton;
    }
}