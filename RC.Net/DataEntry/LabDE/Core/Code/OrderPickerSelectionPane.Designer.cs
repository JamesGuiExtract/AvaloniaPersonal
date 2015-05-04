namespace Extract.DataEntry.LabDE
{
    partial class OrderPickerSelectionPane
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
            this._ordersDataGridView = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this._ordersDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // _ordersDataGridView
            // 
            this._ordersDataGridView.AllowUserToAddRows = false;
            this._ordersDataGridView.AllowUserToDeleteRows = false;
            this._ordersDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._ordersDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._ordersDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this._ordersDataGridView.Location = new System.Drawing.Point(3, 3);
            this._ordersDataGridView.MultiSelect = false;
            this._ordersDataGridView.Name = "_ordersDataGridView";
            this._ordersDataGridView.ReadOnly = true;
            this._ordersDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._ordersDataGridView.Size = new System.Drawing.Size(1139, 189);
            this._ordersDataGridView.TabIndex = 0;
            this._ordersDataGridView.SelectionChanged += new System.EventHandler(this.HandleOrdersDataGridView_SelectionChanged);
            this._ordersDataGridView.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(HandleOrdersDataGridView_DataBindingComplete);
            // 
            // OrderPickerSelectionPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._ordersDataGridView);
            this.MinimumSize = new System.Drawing.Size(200, 75);
            this.Name = "OrderPickerSelectionPane";
            this.Size = new System.Drawing.Size(1142, 195);
            ((System.ComponentModel.ISupportInitialize)(this._ordersDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView _ordersDataGridView;
    }
}
