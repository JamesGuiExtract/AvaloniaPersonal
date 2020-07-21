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
            this._tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._documentTypeComboBox = new System.Windows.Forms.ComboBox();
            this._documentTypeLabel = new System.Windows.Forms.Label();
            this._undoButton = new System.Windows.Forms.Button();
            this._redoButton = new System.Windows.Forms.Button();
            this._scrollPanel = new Extract.Utilities.Forms.ScrollPanel();
            this._documentTypePanel.SuspendLayout();
            this._tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _documentTypePanel
            // 
            this._documentTypePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._documentTypePanel.BackColor = System.Drawing.SystemColors.Info;
            this._documentTypePanel.Controls.Add(this._tableLayoutPanel);
            this._documentTypePanel.ForeColor = System.Drawing.Color.Black;
            this._documentTypePanel.Location = new System.Drawing.Point(0, 0);
            this._documentTypePanel.Margin = new System.Windows.Forms.Padding(0);
            this._documentTypePanel.Name = "_documentTypePanel";
            this._documentTypePanel.Size = new System.Drawing.Size(464, 27);
            this._documentTypePanel.TabIndex = 1;
            // 
            // _tableLayoutPanel
            // 
            this._tableLayoutPanel.ColumnCount = 4;
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._tableLayoutPanel.Controls.Add(this._documentTypeComboBox, 1, 0);
            this._tableLayoutPanel.Controls.Add(this._documentTypeLabel, 0, 0);
            this._tableLayoutPanel.Controls.Add(this._undoButton, 2, 0);
            this._tableLayoutPanel.Controls.Add(this._redoButton, 3, 0);
            this._tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._tableLayoutPanel.Name = "_tableLayoutPanel";
            this._tableLayoutPanel.RowCount = 1;
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanel.Size = new System.Drawing.Size(464, 27);
            this._tableLayoutPanel.TabIndex = 2;
            // 
            // _documentTypeComboBox
            // 
            this._documentTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._documentTypeComboBox.Enabled = false;
            this._documentTypeComboBox.Location = new System.Drawing.Point(92, 3);
            this._documentTypeComboBox.MaxDropDownItems = 25;
            this._documentTypeComboBox.Name = "_documentTypeComboBox";
            this._documentTypeComboBox.Size = new System.Drawing.Size(239, 21);
            this._documentTypeComboBox.Sorted = true;
            this._documentTypeComboBox.TabIndex = 1;
            this._documentTypeComboBox.MouseWheel += HandleDocumentTypeComboBox_MouseWheel;
            // 
            // _documentTypeLabel
            // 
            this._documentTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._documentTypeLabel.AutoSize = true;
            this._documentTypeLabel.Location = new System.Drawing.Point(3, 7);
            this._documentTypeLabel.Margin = new System.Windows.Forms.Padding(3);
            this._documentTypeLabel.Name = "_documentTypeLabel";
            this._documentTypeLabel.Size = new System.Drawing.Size(83, 13);
            this._documentTypeLabel.TabIndex = 0;
            this._documentTypeLabel.Text = "Document Type";
            this._documentTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _undoButton
            // 
            this._undoButton.BackColor = System.Drawing.SystemColors.Control;
            this._undoButton.Enabled = false;
            this._undoButton.Image = global::Extract.UtilityApplications.PaginationUtility.Properties.Resources.Undo;
            this._undoButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._undoButton.Location = new System.Drawing.Point(337, 2);
            this._undoButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 1);
            this._undoButton.Name = "_undoButton";
            this._undoButton.Size = new System.Drawing.Size(59, 23);
            this._undoButton.TabIndex = 2;
            this._undoButton.Text = "Undo";
            this._undoButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this._undoButton.UseVisualStyleBackColor = true;
            this._undoButton.Click += new System.EventHandler(this.HandleUndoButton_Click);
            // 
            // _redoButton
            // 
            this._redoButton.BackColor = System.Drawing.SystemColors.Control;
            this._redoButton.Enabled = false;
            this._redoButton.Image = global::Extract.UtilityApplications.PaginationUtility.Properties.Resources.Redo;
            this._redoButton.Location = new System.Drawing.Point(402, 2);
            this._redoButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 1);
            this._redoButton.Name = "_redoButton";
            this._redoButton.Size = new System.Drawing.Size(59, 23);
            this._redoButton.TabIndex = 3;
            this._redoButton.Text = "Redo";
            this._redoButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this._redoButton.UseVisualStyleBackColor = true;
            this._redoButton.Click += new System.EventHandler(this.HandleRedoButton_Click);
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
            this._scrollPanel.Size = new System.Drawing.Size(464, 244);
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
            this.Size = new System.Drawing.Size(464, 271);
            this._documentTypePanel.ResumeLayout(false);
            this._tableLayoutPanel.ResumeLayout(false);
            this._tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _documentTypePanel;
        private System.Windows.Forms.Label _documentTypeLabel;
        private System.Windows.Forms.ComboBox _documentTypeComboBox;
        private Utilities.Forms.ScrollPanel _scrollPanel;
        private System.Windows.Forms.TableLayoutPanel _tableLayoutPanel;
        private System.Windows.Forms.Button _undoButton;
        private System.Windows.Forms.Button _redoButton;
    }
}
