namespace Extract.DataEntry.LabDE
{
    partial class RecordPickerSelectionPane
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._recordsDataGridView = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this._recordsDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // _recordsDataGridView
            // 
            this._recordsDataGridView.AllowUserToAddRows = false;
            this._recordsDataGridView.AllowUserToDeleteRows = false;
            this._recordsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._recordsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._recordsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this._recordsDataGridView.Location = new System.Drawing.Point(3, 3);
            this._recordsDataGridView.MultiSelect = false;
            this._recordsDataGridView.Name = "_recordsDataGridView";
            this._recordsDataGridView.ReadOnly = true;
            this._recordsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._recordsDataGridView.Size = new System.Drawing.Size(1139, 189);
            this._recordsDataGridView.TabIndex = 0;
            this._recordsDataGridView.SelectionChanged += new System.EventHandler(this.HandleRecordsDataGridView_SelectionChanged);
            this._recordsDataGridView.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(HandleRecordsDataGridView_DataBindingComplete);
            // 
            // RecordPickerSelectionPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._recordsDataGridView);
            this.MinimumSize = new System.Drawing.Size(200, 75);
            this.Name = "RecordPickerSelectionPane";
            this.Size = new System.Drawing.Size(1142, 195);
            ((System.ComponentModel.ISupportInitialize)(this._recordsDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView _recordsDataGridView;
    }
}
