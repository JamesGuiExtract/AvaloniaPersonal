namespace Extract.UtilityApplications.PaginationUtility
{
    partial class PaginationSeparator
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
            this.components = new System.ComponentModel.Container();
            this._tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._editDocumentDataButton = new System.Windows.Forms.Button();
            this._collapseDocumentButton = new System.Windows.Forms.Button();
            this._summaryLabel = new System.Windows.Forms.Label();
            this._pagesLabel = new System.Windows.Forms.Label();
            this._editedDataPictureBox = new System.Windows.Forms.PictureBox();
            this._newDocumentGlyph = new Extract.UtilityApplications.PaginationUtility.NewDocumentGlyph();
            this._editedPaginationGlyph = new Extract.UtilityApplications.PaginationUtility.EditedPaginationGlyph();
            this._toolTip = new System.Windows.Forms.ToolTip(this.components);
            this._tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._editedDataPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // _tableLayoutPanel
            // 
            this._tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tableLayoutPanel.AutoSize = true;
            this._tableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._tableLayoutPanel.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this._tableLayoutPanel.ColumnCount = 7;
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this._tableLayoutPanel.Controls.Add(this._editDocumentDataButton, 1, 0);
            this._tableLayoutPanel.Controls.Add(this._collapseDocumentButton, 0, 0);
            this._tableLayoutPanel.Controls.Add(this._summaryLabel, 2, 0);
            this._tableLayoutPanel.Controls.Add(this._pagesLabel, 3, 0);
            this._tableLayoutPanel.Controls.Add(this._editedDataPictureBox, 6, 0);
            this._tableLayoutPanel.Controls.Add(this._newDocumentGlyph, 4, 0);
            this._tableLayoutPanel.Controls.Add(this._editedPaginationGlyph, 5, 0);
            this._tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._tableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this._tableLayoutPanel.Name = "_tableLayoutPanel";
            this._tableLayoutPanel.RowCount = 2;
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.Size = new System.Drawing.Size(471, 23);
            this._tableLayoutPanel.TabIndex = 3;
            this._tableLayoutPanel.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this.HandleTableLayoutPanel_ControlRemoved);
            // 
            // _editDocumentDataButton
            // 
            this._editDocumentDataButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._editDocumentDataButton.BackColor = System.Drawing.Color.White;
            this._editDocumentDataButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this._editDocumentDataButton.FlatAppearance.BorderSize = 0;
            this._editDocumentDataButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this._editDocumentDataButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this._editDocumentDataButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this._editDocumentDataButton.Image = global::Extract.UtilityApplications.PaginationUtility.Properties.Resources.Edit11;
            this._editDocumentDataButton.Location = new System.Drawing.Point(21, 3);
            this._editDocumentDataButton.Name = "_editDocumentDataButton";
            this._editDocumentDataButton.Size = new System.Drawing.Size(16, 16);
            this._editDocumentDataButton.TabIndex = 12;
            this._editDocumentDataButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this._editDocumentDataButton.UseVisualStyleBackColor = false;
            this._editDocumentDataButton.Click += new System.EventHandler(this.HandleEditDocumentDataButton_Click);
            // 
            // _collapseDocumentButton
            // 
            this._collapseDocumentButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._collapseDocumentButton.BackColor = System.Drawing.SystemColors.Control;
            this._collapseDocumentButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this._collapseDocumentButton.FlatAppearance.BorderSize = 0;
            this._collapseDocumentButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this._collapseDocumentButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this._collapseDocumentButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this._collapseDocumentButton.Image = global::Extract.UtilityApplications.PaginationUtility.Properties.Resources.Collapse;
            this._collapseDocumentButton.Location = new System.Drawing.Point(3, 5);
            this._collapseDocumentButton.Name = "_collapseDocumentButton";
            this._collapseDocumentButton.Size = new System.Drawing.Size(12, 12);
            this._collapseDocumentButton.TabIndex = 11;
            this._collapseDocumentButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this._collapseDocumentButton.UseVisualStyleBackColor = false;
            this._collapseDocumentButton.Click += new System.EventHandler(this.HandleCollapseDocumentButton_Click);
            // 
            // _summaryLabel
            // 
            this._summaryLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._summaryLabel.AutoSize = true;
            this._summaryLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._summaryLabel.ForeColor = System.Drawing.Color.White;
            this._summaryLabel.Location = new System.Drawing.Point(43, 5);
            this._summaryLabel.Name = "_summaryLabel";
            this._summaryLabel.Size = new System.Drawing.Size(0, 13);
            this._summaryLabel.TabIndex = 0;
            this._summaryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _pagesLabel
            // 
            this._pagesLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._pagesLabel.AutoSize = true;
            this._pagesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._pagesLabel.ForeColor = System.Drawing.Color.White;
            this._pagesLabel.Location = new System.Drawing.Point(405, 5);
            this._pagesLabel.Name = "_pagesLabel";
            this._pagesLabel.Size = new System.Drawing.Size(0, 13);
            this._pagesLabel.TabIndex = 13;
            // 
            // _editedDataPictureBox
            // 
            this._editedDataPictureBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._editedDataPictureBox.Image = global::Extract.UtilityApplications.PaginationUtility.Properties.Resources.Edit16;
            this._editedDataPictureBox.Location = new System.Drawing.Point(452, 3);
            this._editedDataPictureBox.Margin = new System.Windows.Forms.Padding(0);
            this._editedDataPictureBox.Name = "_editedDataPictureBox";
            this._editedDataPictureBox.Size = new System.Drawing.Size(17, 17);
            this._editedDataPictureBox.TabIndex = 14;
            this._editedDataPictureBox.TabStop = false;
            // 
            // _newDocumentGlyph
            // 
            this._newDocumentGlyph.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._newDocumentGlyph.BackColor = System.Drawing.Color.Transparent;
            this._newDocumentGlyph.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._newDocumentGlyph.Location = new System.Drawing.Point(411, 3);
            this._newDocumentGlyph.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this._newDocumentGlyph.MaximumSize = new System.Drawing.Size(17, 17);
            this._newDocumentGlyph.MinimumSize = new System.Drawing.Size(17, 17);
            this._newDocumentGlyph.Name = "_newDocumentGlyph";
            this._newDocumentGlyph.Size = new System.Drawing.Size(17, 17);
            this._newDocumentGlyph.TabIndex = 18;
            // 
            // _editedPaginationGlyph
            // 
            this._editedPaginationGlyph.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._editedPaginationGlyph.BackColor = System.Drawing.Color.Transparent;
            this._editedPaginationGlyph.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._editedPaginationGlyph.Location = new System.Drawing.Point(431, 3);
            this._editedPaginationGlyph.Margin = new System.Windows.Forms.Padding(0);
            this._editedPaginationGlyph.MaximumSize = new System.Drawing.Size(17, 17);
            this._editedPaginationGlyph.MinimumSize = new System.Drawing.Size(17, 17);
            this._editedPaginationGlyph.Name = "_editedPaginationGlyph";
            this._editedPaginationGlyph.Size = new System.Drawing.Size(17, 17);
            this._editedPaginationGlyph.TabIndex = 19;
            // 
            // PaginationSeparator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this._tableLayoutPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "PaginationSeparator";
            this.Size = new System.Drawing.Size(471, 23);
            this._tableLayoutPanel.ResumeLayout(false);
            this._tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._editedDataPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _tableLayoutPanel;
        private System.Windows.Forms.Button _collapseDocumentButton;
        private System.Windows.Forms.Label _summaryLabel;
        private System.Windows.Forms.Button _editDocumentDataButton;
        private System.Windows.Forms.Label _pagesLabel;
        private System.Windows.Forms.PictureBox _editedDataPictureBox;
        private NewDocumentGlyph _newDocumentGlyph;
        private EditedPaginationGlyph _editedPaginationGlyph;
        private System.Windows.Forms.ToolTip _toolTip;


    }
}
