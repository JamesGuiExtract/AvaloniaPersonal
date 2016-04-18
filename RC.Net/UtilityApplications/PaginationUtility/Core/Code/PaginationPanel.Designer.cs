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
            this._topToolStrip = new System.Windows.Forms.ToolStrip();
            this._applyToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._revertToolStripButton = new System.Windows.Forms.ToolStripButton();
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
            this._toolStripContainer.ContentPanel.Size = new System.Drawing.Size(325, 281);
            this._toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._toolStripContainer.LeftToolStripPanelVisible = false;
            this._toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._toolStripContainer.Name = "_toolStripContainer";
            this._toolStripContainer.RightToolStripPanelVisible = false;
            this._toolStripContainer.Size = new System.Drawing.Size(325, 306);
            this._toolStripContainer.TabIndex = 0;
            this._toolStripContainer.Text = "toolStripContainer1";
            // 
            // _toolStripContainer.TopToolStripPanel
            // 
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._topToolStrip);
            // 
            // _topToolStrip
            // 
            this._topToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._topToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._topToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._applyToolStripButton,
            this._revertToolStripButton});
            this._topToolStrip.Location = new System.Drawing.Point(3, 0);
            this._topToolStrip.Name = "_topToolStrip";
            this._topToolStrip.Size = new System.Drawing.Size(120, 25);
            this._topToolStrip.TabIndex = 0;
            // 
            // _applyToolStripButton
            // 
            this._applyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._applyToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_applyToolStripButton.Image")));
            this._applyToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._applyToolStripButton.Name = "_applyToolStripButton";
            this._applyToolStripButton.Size = new System.Drawing.Size(42, 22);
            this._applyToolStripButton.Text = "Apply";
            this._applyToolStripButton.Click += new System.EventHandler(this.HandleApplyToolStripButton_Click);
            // 
            // _revertToolStripButton
            // 
            this._revertToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._revertToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_revertToolStripButton.Image")));
            this._revertToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._revertToolStripButton.Name = "_revertToolStripButton";
            this._revertToolStripButton.Size = new System.Drawing.Size(44, 22);
            this._revertToolStripButton.Text = "Revert";
            this._revertToolStripButton.Click += new System.EventHandler(this.HandleRevertToolStripButton_Click);
            // 
            // PaginationControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._toolStripContainer);
            this.Name = "PaginationControl";
            this.Size = new System.Drawing.Size(325, 306);
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
        private System.Windows.Forms.ToolStripButton _revertToolStripButton;
    }
}
