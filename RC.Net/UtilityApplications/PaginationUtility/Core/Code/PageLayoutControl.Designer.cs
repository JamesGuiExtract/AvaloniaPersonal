namespace Extract.UtilityApplications.PaginationUtility
{
    partial class PageLayoutControl
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
            this._flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // _flowLayoutPanel
            // 
            this._flowLayoutPanel.AllowDrop = true;
            this._flowLayoutPanel.AutoScroll = true;
            this._flowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._flowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._flowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this._flowLayoutPanel.Name = "_flowLayoutPanel";
            this._flowLayoutPanel.Size = new System.Drawing.Size(459, 425);
            this._flowLayoutPanel.TabIndex = 0;
            this._flowLayoutPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.HandleFlowLayoutPanel_DragDrop);
            this._flowLayoutPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.HandleFlowLayoutPanel_DragEnter);
            this._flowLayoutPanel.DragOver += new System.Windows.Forms.DragEventHandler(this.HandleFlowLayoutPanel_DragOver);
            this._flowLayoutPanel.DragLeave += new System.EventHandler(this.HandleFlowLayoutPanel_DragLeave);
            // 
            // PageLayoutControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._flowLayoutPanel);
            this.Name = "PageLayoutControl";
            this.Size = new System.Drawing.Size(459, 425);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel _flowLayoutPanel;
    }
}
