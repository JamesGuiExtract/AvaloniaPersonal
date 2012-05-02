namespace Extract.SQLCDBEditor
{
    partial class QueryAndResultsControl
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueryAndResultsControl));
            this._queryScintillaBox = new ScintillaNET.Scintilla();
            this._queryAndResultsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._buttonsFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._executeQueryButton = new System.Windows.Forms.Button();
            this._resultsSplitContainer = new System.Windows.Forms.SplitContainer();
            this._queryPanel = new System.Windows.Forms.Panel();
            this._resultsPanel = new System.Windows.Forms.Panel();
            this.queryAndResultTableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this._parametersTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._resultsStatusLabel = new System.Windows.Forms.Label();
            this._resultsGrid = new System.Windows.Forms.DataGridView();
            this._showHideQueryButton = new System.Windows.Forms.Button();
            this._sendToSeparateTabButton = new System.Windows.Forms.Button();
            this._newQueryButton = new System.Windows.Forms.Button();
            this._saveButton = new System.Windows.Forms.Button();
            this._renameButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._queryScintillaBox)).BeginInit();
            this._queryAndResultsTableLayoutPanel.SuspendLayout();
            this._buttonsFlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._resultsSplitContainer)).BeginInit();
            this._resultsSplitContainer.Panel1.SuspendLayout();
            this._resultsSplitContainer.Panel2.SuspendLayout();
            this._resultsSplitContainer.SuspendLayout();
            this._queryPanel.SuspendLayout();
            this._resultsPanel.SuspendLayout();
            this.queryAndResultTableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._resultsGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // _queryScintillaBox
            // 
            this._queryScintillaBox.ConfigurationManager.Language = "mssql";
            this._queryScintillaBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._queryScintillaBox.LineWrap.Mode = ScintillaNET.WrapMode.Word;
            this._queryScintillaBox.Location = new System.Drawing.Point(0, 0);
            this._queryScintillaBox.Margins.Margin1.Width = 0;
            this._queryScintillaBox.Name = "_queryScintillaBox";
            this._queryScintillaBox.Size = new System.Drawing.Size(148, 90);
            this._queryScintillaBox.TabIndex = 0;
            // 
            // _queryAndResultsTableLayoutPanel
            // 
            this._queryAndResultsTableLayoutPanel.AutoSize = true;
            this._queryAndResultsTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._queryAndResultsTableLayoutPanel.ColumnCount = 1;
            this._queryAndResultsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._queryAndResultsTableLayoutPanel.Controls.Add(this._buttonsFlowLayoutPanel, 0, 0);
            this._queryAndResultsTableLayoutPanel.Controls.Add(this._resultsSplitContainer, 0, 1);
            this._queryAndResultsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._queryAndResultsTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._queryAndResultsTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this._queryAndResultsTableLayoutPanel.Name = "_queryAndResultsTableLayoutPanel";
            this._queryAndResultsTableLayoutPanel.RowCount = 2;
            this._queryAndResultsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._queryAndResultsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._queryAndResultsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._queryAndResultsTableLayoutPanel.Size = new System.Drawing.Size(624, 332);
            this._queryAndResultsTableLayoutPanel.TabIndex = 3;
            // 
            // _buttonsFlowLayoutPanel
            // 
            this._buttonsFlowLayoutPanel.AutoSize = true;
            this._buttonsFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._buttonsFlowLayoutPanel.Controls.Add(this._showHideQueryButton);
            this._buttonsFlowLayoutPanel.Controls.Add(this._sendToSeparateTabButton);
            this._buttonsFlowLayoutPanel.Controls.Add(this._newQueryButton);
            this._buttonsFlowLayoutPanel.Controls.Add(this._saveButton);
            this._buttonsFlowLayoutPanel.Controls.Add(this._renameButton);
            this._buttonsFlowLayoutPanel.Controls.Add(this._executeQueryButton);
            this._buttonsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._buttonsFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._buttonsFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this._buttonsFlowLayoutPanel.Name = "_buttonsFlowLayoutPanel";
            this._buttonsFlowLayoutPanel.Size = new System.Drawing.Size(624, 29);
            this._buttonsFlowLayoutPanel.TabIndex = 0;
            // 
            // _executeQueryButton
            // 
            this._executeQueryButton.AutoSize = true;
            this._executeQueryButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._executeQueryButton.Enabled = false;
            this._executeQueryButton.Image = global::Extract.SQLCDBEditor.Properties.Resources.ExecuteQuery;
            this._executeQueryButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this._executeQueryButton.Location = new System.Drawing.Point(506, 3);
            this._executeQueryButton.Name = "_executeQueryButton";
            this._executeQueryButton.Size = new System.Drawing.Size(72, 23);
            this._executeQueryButton.TabIndex = 0;
            this._executeQueryButton.Text = "Execute";
            this._executeQueryButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this._executeQueryButton.UseVisualStyleBackColor = true;
            this._executeQueryButton.Click += new System.EventHandler(this.HandleExecuteButtonClick);
            // 
            // _resultsSplitContainer
            // 
            this._resultsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._resultsSplitContainer.Location = new System.Drawing.Point(0, 29);
            this._resultsSplitContainer.Margin = new System.Windows.Forms.Padding(0);
            this._resultsSplitContainer.Name = "_resultsSplitContainer";
            this._resultsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _resultsSplitContainer.Panel1
            // 
            this._resultsSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this._resultsSplitContainer.Panel1.Controls.Add(this._queryPanel);
            this._resultsSplitContainer.Panel1Collapsed = true;
            // 
            // _resultsSplitContainer.Panel2
            // 
            this._resultsSplitContainer.Panel2.Controls.Add(this._resultsPanel);
            this._resultsSplitContainer.Size = new System.Drawing.Size(624, 303);
            this._resultsSplitContainer.SplitterDistance = 92;
            this._resultsSplitContainer.TabIndex = 4;
            // 
            // _queryPanel
            // 
            this._queryPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._queryPanel.Controls.Add(this._queryScintillaBox);
            this._queryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._queryPanel.Location = new System.Drawing.Point(0, 0);
            this._queryPanel.Name = "_queryPanel";
            this._queryPanel.Size = new System.Drawing.Size(150, 92);
            this._queryPanel.TabIndex = 1;
            // 
            // _resultsPanel
            // 
            this._resultsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._resultsPanel.Controls.Add(this.queryAndResultTableLayoutPanel2);
            this._resultsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._resultsPanel.Location = new System.Drawing.Point(0, 0);
            this._resultsPanel.Margin = new System.Windows.Forms.Padding(0);
            this._resultsPanel.Name = "_resultsPanel";
            this._resultsPanel.Size = new System.Drawing.Size(624, 303);
            this._resultsPanel.TabIndex = 4;
            // 
            // queryAndResultTableLayoutPanel2
            // 
            this.queryAndResultTableLayoutPanel2.ColumnCount = 1;
            this.queryAndResultTableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.queryAndResultTableLayoutPanel2.Controls.Add(this._parametersTableLayoutPanel, 0, 0);
            this.queryAndResultTableLayoutPanel2.Controls.Add(this._resultsStatusLabel, 0, 1);
            this.queryAndResultTableLayoutPanel2.Controls.Add(this._resultsGrid, 0, 2);
            this.queryAndResultTableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.queryAndResultTableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.queryAndResultTableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.queryAndResultTableLayoutPanel2.Name = "queryAndResultTableLayoutPanel2";
            this.queryAndResultTableLayoutPanel2.RowCount = 3;
            this.queryAndResultTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.queryAndResultTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.queryAndResultTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.queryAndResultTableLayoutPanel2.Size = new System.Drawing.Size(622, 301);
            this.queryAndResultTableLayoutPanel2.TabIndex = 3;
            // 
            // _parametersTableLayoutPanel
            // 
            this._parametersTableLayoutPanel.AutoSize = true;
            this._parametersTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._parametersTableLayoutPanel.ColumnCount = 2;
            this._parametersTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._parametersTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._parametersTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._parametersTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._parametersTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this._parametersTableLayoutPanel.Name = "_parametersTableLayoutPanel";
            this._parametersTableLayoutPanel.RowCount = 1;
            this._parametersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._parametersTableLayoutPanel.Size = new System.Drawing.Size(622, 1);
            this._parametersTableLayoutPanel.TabIndex = 3;
            // 
            // _resultsStatusLabel
            // 
            this._resultsStatusLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._resultsStatusLabel.AutoSize = true;
            this._resultsStatusLabel.ForeColor = System.Drawing.Color.Red;
            this._resultsStatusLabel.Location = new System.Drawing.Point(3, 3);
            this._resultsStatusLabel.Margin = new System.Windows.Forms.Padding(3);
            this._resultsStatusLabel.Name = "_resultsStatusLabel";
            this._resultsStatusLabel.Size = new System.Drawing.Size(238, 13);
            this._resultsStatusLabel.TabIndex = 3;
            this._resultsStatusLabel.Text = "Query has been modified since the last execution";
            this._resultsStatusLabel.Visible = false;
            this._resultsStatusLabel.Click += new System.EventHandler(this.HandleInvalidDataLabelClicked);
            // 
            // _resultsGrid
            // 
            this._resultsGrid.AllowUserToResizeRows = false;
            this._resultsGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._resultsGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this._resultsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._resultsGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this._resultsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._resultsGrid.Location = new System.Drawing.Point(0, 19);
            this._resultsGrid.Margin = new System.Windows.Forms.Padding(0);
            this._resultsGrid.Name = "_resultsGrid";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._resultsGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this._resultsGrid.Size = new System.Drawing.Size(622, 282);
            this._resultsGrid.TabIndex = 2;
            this._resultsGrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.HandleResultsGridCurrentCellDirtyStateChanged);
            // 
            // _showHideQueryButton
            // 
            this._showHideQueryButton.AutoSize = true;
            this._showHideQueryButton.Image = global::Extract.SQLCDBEditor.Properties.Resources.DbQuerySmall;
            this._showHideQueryButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this._showHideQueryButton.Location = new System.Drawing.Point(3, 3);
            this._showHideQueryButton.Name = "_showHideQueryButton";
            this._showHideQueryButton.Size = new System.Drawing.Size(89, 23);
            this._showHideQueryButton.TabIndex = 7;
            this._showHideQueryButton.Text = "Show query";
            this._showHideQueryButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this._showHideQueryButton.UseVisualStyleBackColor = true;
            this._showHideQueryButton.Click += new System.EventHandler(this.HandleShowHideQueryButtonClick);
            // 
            // _sendToSeparateTabButton
            // 
            this._sendToSeparateTabButton.AutoSize = true;
            this._sendToSeparateTabButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._sendToSeparateTabButton.Image = ((System.Drawing.Image)(resources.GetObject("_sendToSeparateTabButton.Image")));
            this._sendToSeparateTabButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this._sendToSeparateTabButton.Location = new System.Drawing.Point(98, 3);
            this._sendToSeparateTabButton.Name = "_sendToSeparateTabButton";
            this._sendToSeparateTabButton.Size = new System.Drawing.Size(132, 23);
            this._sendToSeparateTabButton.TabIndex = 1;
            this._sendToSeparateTabButton.Text = "Send to separate tab";
            this._sendToSeparateTabButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this._sendToSeparateTabButton.UseVisualStyleBackColor = true;
            this._sendToSeparateTabButton.Click += new System.EventHandler(this.HandleSendToSeparateTabClick);
            // 
            // _newQueryButton
            // 
            this._newQueryButton.AutoSize = true;
            this._newQueryButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._newQueryButton.Image = global::Extract.SQLCDBEditor.Properties.Resources.DbNewQuery;
            this._newQueryButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this._newQueryButton.Location = new System.Drawing.Point(236, 3);
            this._newQueryButton.Name = "_newQueryButton";
            this._newQueryButton.Size = new System.Drawing.Size(121, 23);
            this._newQueryButton.TabIndex = 5;
            this._newQueryButton.Text = "Copy to new query";
            this._newQueryButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this._newQueryButton.UseVisualStyleBackColor = true;
            this._newQueryButton.Click += new System.EventHandler(this.HandleCopyToNewQueryButtonClick);
            // 
            // _saveButton
            // 
            this._saveButton.AutoSize = true;
            this._saveButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._saveButton.Enabled = false;
            this._saveButton.Image = global::Extract.SQLCDBEditor.Properties.Resources.SaveImageButtonSmall;
            this._saveButton.Location = new System.Drawing.Point(363, 3);
            this._saveButton.Name = "_saveButton";
            this._saveButton.Size = new System.Drawing.Size(58, 23);
            this._saveButton.TabIndex = 2;
            this._saveButton.Text = "Save";
            this._saveButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this._saveButton.UseVisualStyleBackColor = true;
            this._saveButton.Click += new System.EventHandler(this.HandleSaveButtonClick);
            // 
            // _renameButton
            // 
            this._renameButton.AutoSize = true;
            this._renameButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._renameButton.Image = global::Extract.SQLCDBEditor.Properties.Resources.Rename;
            this._renameButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this._renameButton.Location = new System.Drawing.Point(427, 3);
            this._renameButton.Name = "_renameButton";
            this._renameButton.Size = new System.Drawing.Size(73, 23);
            this._renameButton.TabIndex = 4;
            this._renameButton.Text = "Rename";
            this._renameButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this._renameButton.UseVisualStyleBackColor = true;
            this._renameButton.Click += new System.EventHandler(this.HandleRenameButtonClick);
            // 
            // QueryAndResultsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this._queryAndResultsTableLayoutPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "QueryAndResultsControl";
            this.Size = new System.Drawing.Size(624, 332);
            ((System.ComponentModel.ISupportInitialize)(this._queryScintillaBox)).EndInit();
            this._queryAndResultsTableLayoutPanel.ResumeLayout(false);
            this._queryAndResultsTableLayoutPanel.PerformLayout();
            this._buttonsFlowLayoutPanel.ResumeLayout(false);
            this._buttonsFlowLayoutPanel.PerformLayout();
            this._resultsSplitContainer.Panel1.ResumeLayout(false);
            this._resultsSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._resultsSplitContainer)).EndInit();
            this._resultsSplitContainer.ResumeLayout(false);
            this._queryPanel.ResumeLayout(false);
            this._resultsPanel.ResumeLayout(false);
            this.queryAndResultTableLayoutPanel2.ResumeLayout(false);
            this.queryAndResultTableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._resultsGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _queryAndResultsTableLayoutPanel;
        private ScintillaNET.Scintilla _queryScintillaBox;
        private System.Windows.Forms.FlowLayoutPanel _buttonsFlowLayoutPanel;
        private System.Windows.Forms.DataGridView _resultsGrid;
        private System.Windows.Forms.Button _sendToSeparateTabButton;
        private System.Windows.Forms.Button _saveButton;
        private System.Windows.Forms.Button _executeQueryButton;
        private System.Windows.Forms.TableLayoutPanel _parametersTableLayoutPanel;
        private System.Windows.Forms.Label _resultsStatusLabel;
        private System.Windows.Forms.Button _renameButton;
        private System.Windows.Forms.Button _newQueryButton;
        private System.Windows.Forms.SplitContainer _resultsSplitContainer;
        private System.Windows.Forms.TableLayoutPanel queryAndResultTableLayoutPanel2;
        private System.Windows.Forms.Panel _queryPanel;
        private System.Windows.Forms.Panel _resultsPanel;
        private System.Windows.Forms.Button _showHideQueryButton;
    }
}
