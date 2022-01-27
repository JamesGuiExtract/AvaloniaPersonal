namespace Extract.FileActionManager.FileProcessors
{
    partial class SendEmailTaskAttachmentsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SendEmailTaskAttachmentsDialog));
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._dataGridView = new System.Windows.Forms.DataGridView();
            this._attachmentsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._browseButton = new Extract.Utilities.Forms.BrowseButton();
            this._pathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._clearButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(9, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(102, 13);
            label1.TabIndex = 0;
            label1.Text = "Email attachments...";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(337, 186);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 5;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(418, 186);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 6;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _dataGridView
            // 
            this._dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dataGridView.ColumnHeadersVisible = false;
            this._dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._attachmentsColumn});
            this._dataGridView.Location = new System.Drawing.Point(12, 28);
            this._dataGridView.Name = "_dataGridView";
            this._dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._dataGridView.Size = new System.Drawing.Size(415, 147);
            this._dataGridView.TabIndex = 1;
            this._dataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.HandleDataGridView_CellValidating);
            this._dataGridView.CurrentCellChanged += new System.EventHandler(this.HandleDataGridView_CurrentCellChanged);
            this._dataGridView.SelectionChanged += new System.EventHandler(this._dataGridView_SelectionChanged);
            // 
            // _attachmentsColumn
            // 
            this._attachmentsColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._attachmentsColumn.HeaderText = "Attachments";
            this._attachmentsColumn.Name = "_attachmentsColumn";
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.HeaderText = "Attachments";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // _browseButton
            // 
            this._browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._browseButton.Enabled = false;
            this._browseButton.Location = new System.Drawing.Point(466, 27);
            this._browseButton.Name = "_browseButton";
            this._browseButton.Size = new System.Drawing.Size(27, 20);
            this._browseButton.TabIndex = 3;
            this._browseButton.Text = "...";
            this._browseButton.UseVisualStyleBackColor = true;
            this._browseButton.PathSelected += new System.EventHandler<Extract.Utilities.Forms.PathSelectedEventArgs>(this.HandleBrowseButton_PathSelected);
            // 
            // _pathTagsButton
            // 
            this._pathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._pathTagsButton.Enabled = false;
            this._pathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_pathTagsButton.Image")));
            this._pathTagsButton.Location = new System.Drawing.Point(433, 27);
            this._pathTagsButton.Name = "_pathTagsButton";
            this._pathTagsButton.Size = new System.Drawing.Size(27, 20);
            this._pathTagsButton.TabIndex = 2;
            this._pathTagsButton.UseVisualStyleBackColor = true;
            this._pathTagsButton.TagSelecting += new System.EventHandler<Extract.Utilities.Forms.TagSelectingEventArgs>(this.HandlePathTagsButton_TagSelecting);
            // 
            // _clearButton
            // 
            this._clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._clearButton.Location = new System.Drawing.Point(12, 186);
            this._clearButton.Name = "_clearButton";
            this._clearButton.Size = new System.Drawing.Size(75, 23);
            this._clearButton.TabIndex = 4;
            this._clearButton.Text = "Clear All";
            this._clearButton.UseVisualStyleBackColor = true;
            this._clearButton.Click += new System.EventHandler(this.HandleClearButton_Click);
            // 
            // SendEmailTaskAttachmentsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(505, 221);
            this.Controls.Add(this._clearButton);
            this.Controls.Add(this._browseButton);
            this.Controls.Add(this._pathTagsButton);
            this.Controls.Add(label1);
            this.Controls.Add(this._dataGridView);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(350, 250);
            this.Name = "SendEmailTaskAttachmentsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Core: Send email attachments";
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.DataGridView _dataGridView;
        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _pathTagsButton;
        private Extract.Utilities.Forms.BrowseButton _browseButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn _attachmentsColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.Button _clearButton;
    }
}