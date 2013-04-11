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
            this._blankPanel = new System.Windows.Forms.Panel();
            this._outerPanel = new System.Windows.Forms.Panel();
            this._outerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _blankPanel
            // 
            this._blankPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._blankPanel.BackColor = System.Drawing.Color.Black;
            this._blankPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._blankPanel.Location = new System.Drawing.Point(3, 1);
            this._blankPanel.Margin = new System.Windows.Forms.Padding(0);
            this._blankPanel.Name = "_blankPanel";
            this._blankPanel.Size = new System.Drawing.Size(5, 146);
            this._blankPanel.TabIndex = 0;
            // 
            // _outerPanel
            // 
            this._outerPanel.Controls.Add(this._blankPanel);
            this._outerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._outerPanel.Location = new System.Drawing.Point(1, 1);
            this._outerPanel.Name = "_outerPanel";
            this._outerPanel.Padding = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this._outerPanel.Size = new System.Drawing.Size(11, 148);
            this._outerPanel.TabIndex = 0;
            // 
            // PaginationSeparator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this._outerPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "PaginationSeparator";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Size = new System.Drawing.Size(13, 150);
            this._outerPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _blankPanel;
        private System.Windows.Forms.Panel _outerPanel;

    }
}
