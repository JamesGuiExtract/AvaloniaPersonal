namespace Extract.UtilityApplications.PaginationUtility
{
    partial class PageThumbnailControl
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
            this._fileNameLabel = new System.Windows.Forms.Label();
            this._pageNumberLabel = new System.Windows.Forms.Label();
            this._rasterPictureBox = new Leadtools.WinForms.RasterPictureBox();
            this._tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._borderPanel = new System.Windows.Forms.Panel();
            this._outerPanel = new System.Windows.Forms.Panel();
            this._tableLayoutPanel.SuspendLayout();
            this._borderPanel.SuspendLayout();
            this._outerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _fileNameLabel
            // 
            this._fileNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._fileNameLabel.AutoEllipsis = true;
            this._fileNameLabel.Location = new System.Drawing.Point(3, 134);
            this._fileNameLabel.Name = "_fileNameLabel";
            this._fileNameLabel.Size = new System.Drawing.Size(128, 13);
            this._fileNameLabel.TabIndex = 0;
            this._fileNameLabel.Text = "Filename";
            this._fileNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _pageNumberLabel
            // 
            this._pageNumberLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._pageNumberLabel.Location = new System.Drawing.Point(3, 147);
            this._pageNumberLabel.Name = "_pageNumberLabel";
            this._pageNumberLabel.Size = new System.Drawing.Size(128, 13);
            this._pageNumberLabel.TabIndex = 1;
            this._pageNumberLabel.Text = "Page";
            this._pageNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _rasterPictureBox
            // 
            this._rasterPictureBox.Image = null;
            this._rasterPictureBox.Location = new System.Drawing.Point(3, 3);
            this._rasterPictureBox.Name = "_rasterPictureBox";
            this._rasterPictureBox.Size = new System.Drawing.Size(128, 128);
            this._rasterPictureBox.SizeMode = Leadtools.WinForms.RasterPictureBoxSizeMode.Fit;
            this._rasterPictureBox.TabIndex = 2;
            this._rasterPictureBox.TabStop = false;
            this._rasterPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.HandleRasterPictureBox_Paint);
            // 
            // _tableLayoutPanel
            // 
            this._tableLayoutPanel.AutoSize = true;
            this._tableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._tableLayoutPanel.ColumnCount = 1;
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._tableLayoutPanel.Controls.Add(this._rasterPictureBox, 0, 0);
            this._tableLayoutPanel.Controls.Add(this._pageNumberLabel, 0, 2);
            this._tableLayoutPanel.Controls.Add(this._fileNameLabel, 0, 1);
            this._tableLayoutPanel.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this._tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._tableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this._tableLayoutPanel.Name = "_tableLayoutPanel";
            this._tableLayoutPanel.RowCount = 3;
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.Size = new System.Drawing.Size(134, 160);
            this._tableLayoutPanel.TabIndex = 3;
            // 
            // _borderPanel
            // 
            this._borderPanel.AutoSize = true;
            this._borderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._borderPanel.Controls.Add(this._tableLayoutPanel);
            this._borderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._borderPanel.Location = new System.Drawing.Point(3, 3);
            this._borderPanel.Margin = new System.Windows.Forms.Padding(0);
            this._borderPanel.Name = "_borderPanel";
            this._borderPanel.Size = new System.Drawing.Size(134, 160);
            this._borderPanel.TabIndex = 4;
            // 
            // _outerPanel
            // 
            this._outerPanel.AutoSize = true;
            this._outerPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._outerPanel.Controls.Add(this._borderPanel);
            this._outerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._outerPanel.Location = new System.Drawing.Point(11, 1);
            this._outerPanel.Margin = new System.Windows.Forms.Padding(0);
            this._outerPanel.Name = "_outerPanel";
            this._outerPanel.Padding = new System.Windows.Forms.Padding(3);
            this._outerPanel.Size = new System.Drawing.Size(140, 166);
            this._outerPanel.TabIndex = 5;
            this._outerPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.HandleOuterPanel_Paint);
            // 
            // PageThumbnailControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this._outerPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "PageThumbnailControl";
            this.Padding = new System.Windows.Forms.Padding(11, 1, 0, 1);
            this.Size = new System.Drawing.Size(151, 168);
            this._tableLayoutPanel.ResumeLayout(false);
            this._borderPanel.ResumeLayout(false);
            this._borderPanel.PerformLayout();
            this._outerPanel.ResumeLayout(false);
            this._outerPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _fileNameLabel;
        private System.Windows.Forms.Label _pageNumberLabel;
        private Leadtools.WinForms.RasterPictureBox _rasterPictureBox;
        private System.Windows.Forms.TableLayoutPanel _tableLayoutPanel;
        private System.Windows.Forms.Panel _borderPanel;
        private System.Windows.Forms.Panel _outerPanel;
    }
}
