namespace Extract.LabResultsCustomComponents
{
    partial class OrdersPlugin
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this._ordersGridView = new System.Windows.Forms.DataGridView();
            this._linkedComponentsTextBox = new Extract.LabResultsCustomComponents.LinkedItemsTextBox();
            ((System.ComponentModel.ISupportInitialize)(this._ordersGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._linkedComponentsTextBox)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 339);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Included components:";
            // 
            // _ordersGridView
            // 
            this._ordersGridView.AllowUserToAddRows = false;
            this._ordersGridView.AllowUserToDeleteRows = false;
            this._ordersGridView.AllowUserToResizeRows = false;
            this._ordersGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._ordersGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this._ordersGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._ordersGridView.Location = new System.Drawing.Point(4, 4);
            this._ordersGridView.Name = "_ordersGridView";
            this._ordersGridView.ReadOnly = true;
            this._ordersGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._ordersGridView.Size = new System.Drawing.Size(747, 332);
            this._ordersGridView.TabIndex = 3;
            // 
            // _linkedComponentsTextBox
            // 
            this._linkedComponentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._linkedComponentsTextBox.LineWrap.Mode = ScintillaNET.WrapMode.Word;
            this._linkedComponentsTextBox.Location = new System.Drawing.Point(3, 361);
            this._linkedComponentsTextBox.Margins.Margin1.Width = 0;
            this._linkedComponentsTextBox.Name = "_linkedComponentsTextBox";
            this._linkedComponentsTextBox.Size = new System.Drawing.Size(748, 86);
            this._linkedComponentsTextBox.Styles.LastPredefined.ForeColor = System.Drawing.Color.Blue;
            this._linkedComponentsTextBox.Styles.LastPredefined.IsHotspot = true;
            this._linkedComponentsTextBox.Styles.LastPredefined.Underline = true;
            this._linkedComponentsTextBox.TabIndex = 1;
            this._linkedComponentsTextBox.ItemClicked += new System.EventHandler<Extract.LabResultsCustomComponents.ItemClickedEventArgs>(this.HandleLinkedComponentsTextBox_ItemClicked);
            // 
            // OrdersPlugin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._ordersGridView);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._linkedComponentsTextBox);
            this.Name = "OrdersPlugin";
            this.Size = new System.Drawing.Size(755, 450);
            ((System.ComponentModel.ISupportInitialize)(this._ordersGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._linkedComponentsTextBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LinkedItemsTextBox _linkedComponentsTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView _ordersGridView;
        

    }
}
