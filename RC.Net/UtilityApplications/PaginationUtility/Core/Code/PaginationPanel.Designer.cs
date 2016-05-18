namespace Extract.UtilityApplications.PaginationUtility
{
    partial class PaginationPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaginationPanel));
            this._toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._topToolStrip = new System.Windows.Forms.ToolStrip();
            this._applyToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._revertToSourceToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._revertToOriginalToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._toolStripContainer.ContentPanel.SuspendLayout();
            this._toolStripContainer.TopToolStripPanel.SuspendLayout();
            this._toolStripContainer.SuspendLayout();
            this._topToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _toolStripContainer
            // 
            this._toolStripContainer.BottomToolStripPanelVisible = false;
            // 
            // _toolStripContainer.ContentPanel
            // 
            this._toolStripContainer.ContentPanel.Controls.Add(this._tableLayoutPanel);
            this._toolStripContainer.ContentPanel.Size = new System.Drawing.Size(454, 281);
            this._toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._toolStripContainer.LeftToolStripPanelVisible = false;
            this._toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._toolStripContainer.Name = "_toolStripContainer";
            this._toolStripContainer.RightToolStripPanelVisible = false;
            this._toolStripContainer.Size = new System.Drawing.Size(454, 306);
            this._toolStripContainer.TabIndex = 0;
            this._toolStripContainer.Text = "toolStripContainer1";
            // 
            // _toolStripContainer.TopToolStripPanel
            // 
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._topToolStrip);
            // 
            // _tableLayoutPanel
            // 
            this._tableLayoutPanel.ColumnCount = 1;
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._tableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this._tableLayoutPanel.Name = "_tableLayoutPanel";
            this._tableLayoutPanel.RowCount = 2;
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._tableLayoutPanel.Size = new System.Drawing.Size(454, 281);
            this._tableLayoutPanel.TabIndex = 0;
            // 
            // _topToolStrip
            // 
            this._topToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._topToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._topToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._applyToolStripButton,
            this._revertToSourceToolStripButton,
            this._revertToOriginalToolStripButton});
            this._topToolStrip.Location = new System.Drawing.Point(3, 0);
            this._topToolStrip.Name = "_topToolStrip";
            this._topToolStrip.Size = new System.Drawing.Size(379, 25);
            this._topToolStrip.TabIndex = 0;
            // 
            // _applyToolStripButton
            // 
            this._applyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._applyToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._applyToolStripButton.Name = "_applyToolStripButton";
            this._applyToolStripButton.Size = new System.Drawing.Size(42, 22);
            this._applyToolStripButton.Text = "Apply";
            this._applyToolStripButton.Click += new System.EventHandler(this.HandleApplyToolStripButton_Click);
            // 
            // _revertToSourceToolStripButton
            // 
            this._revertToSourceToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._revertToSourceToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._revertToSourceToolStripButton.Name = "_revertToSourceToolStripButton";
            this._revertToSourceToolStripButton.Size = new System.Drawing.Size(159, 22);
            this._revertToSourceToolStripButton.Text = "Revert to pagination on disk";
            this._revertToSourceToolStripButton.Click += new System.EventHandler(this.HandleRevertToSourceToolStripButton_Click);
            // 
            // _revertToOriginalToolStripButton
            // 
            this._revertToOriginalToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._revertToOriginalToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_revertToOriginalToolStripButton.Image")));
            this._revertToOriginalToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._revertToOriginalToolStripButton.Name = "_revertToOriginalToolStripButton";
            this._revertToOriginalToolStripButton.Size = new System.Drawing.Size(175, 22);
            this._revertToOriginalToolStripButton.Text = "Revert to suggested pagination";
            this._revertToOriginalToolStripButton.Click += new System.EventHandler(this.HandleRevertToOriginalToolStripButton_Click);
            // 
            // PaginationPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._toolStripContainer);
            this.Name = "PaginationPanel";
            this.Size = new System.Drawing.Size(454, 306);
            this._toolStripContainer.ContentPanel.ResumeLayout(false);
            this._toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._toolStripContainer.TopToolStripPanel.PerformLayout();
            this._toolStripContainer.ResumeLayout(false);
            this._toolStripContainer.PerformLayout();
            this._topToolStrip.ResumeLayout(false);
            this._topToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer _toolStripContainer;
        private System.Windows.Forms.ToolStrip _topToolStrip;
        private System.Windows.Forms.ToolStripButton _applyToolStripButton;
        private System.Windows.Forms.ToolStripButton _revertToSourceToolStripButton;
        private System.Windows.Forms.ToolStripButton _revertToOriginalToolStripButton;
        private System.Windows.Forms.TableLayoutPanel _tableLayoutPanel;
    }
}
