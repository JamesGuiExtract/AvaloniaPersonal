namespace Extract.UtilityApplications.PaginationUtility
{
    partial class DataEntryPanelContainer
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
            this._documentTypePanel = new System.Windows.Forms.Panel();
            this._documentTypeLabel = new System.Windows.Forms.Label();
            this._documentTypeComboBox = new System.Windows.Forms.ComboBox();
            this._scrollPanel = new Extract.Utilities.Forms.ScrollPanel();
            this._documentTypePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _documentTypePanel
            // 
            this._documentTypePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._documentTypePanel.BackColor = System.Drawing.SystemColors.Info;
            this._documentTypePanel.Controls.Add(this._documentTypeLabel);
            this._documentTypePanel.Controls.Add(this._documentTypeComboBox);
            this._documentTypePanel.ForeColor = System.Drawing.Color.Black;
            this._documentTypePanel.Location = new System.Drawing.Point(0, 0);
            this._documentTypePanel.Margin = new System.Windows.Forms.Padding(0);
            this._documentTypePanel.Name = "_documentTypePanel";
            this._documentTypePanel.Size = new System.Drawing.Size(363, 27);
            this._documentTypePanel.TabIndex = 1;
            // 
            // _documentTypeLabel
            // 
            this._documentTypeLabel.AutoSize = true;
            this._documentTypeLabel.Location = new System.Drawing.Point(3, 6);
            this._documentTypeLabel.Name = "_documentTypeLabel";
            this._documentTypeLabel.Size = new System.Drawing.Size(83, 13);
            this._documentTypeLabel.TabIndex = 0;
            this._documentTypeLabel.Text = "Document Type";
            // 
            // _documentTypeComboBox
            // 
            this._documentTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._documentTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._documentTypeComboBox.Enabled = false;
            this._documentTypeComboBox.Location = new System.Drawing.Point(92, 3);
            this._documentTypeComboBox.MaxDropDownItems = 25;
            this._documentTypeComboBox.Name = "_documentTypeComboBox";
            this._documentTypeComboBox.Size = new System.Drawing.Size(268, 21);
            this._documentTypeComboBox.Sorted = true;
            this._documentTypeComboBox.TabIndex = 1;
            // 
            // _scrollPanel
            // 
            this._scrollPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._scrollPanel.AutoScroll = true;
            this._scrollPanel.Location = new System.Drawing.Point(0, 27);
            this._scrollPanel.Margin = new System.Windows.Forms.Padding(0);
            this._scrollPanel.Name = "_scrollPanel";
            this._scrollPanel.Size = new System.Drawing.Size(363, 244);
            this._scrollPanel.TabIndex = 2;
            // 
            // DataEntryPanelContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._scrollPanel);
            this.Controls.Add(this._documentTypePanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "DataEntryPanelContainer";
            this.Size = new System.Drawing.Size(363, 271);
            this._documentTypePanel.ResumeLayout(false);
            this._documentTypePanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _documentTypePanel;
        private System.Windows.Forms.Label _documentTypeLabel;
        private System.Windows.Forms.ComboBox _documentTypeComboBox;
        private Utilities.Forms.ScrollPanel _scrollPanel;
    }
}
