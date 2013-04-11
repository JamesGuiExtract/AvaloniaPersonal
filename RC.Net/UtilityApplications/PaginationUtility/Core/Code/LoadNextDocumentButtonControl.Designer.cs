namespace Extract.UtilityApplications.PaginationUtility
{
    partial class LoadNextDocumentButtonControl
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
            this._outerPanel = new System.Windows.Forms.Panel();
            this._borderPanel = new System.Windows.Forms.Panel();
            this._loadNextDocumentButton = new System.Windows.Forms.Button();
            this._outerPanel.SuspendLayout();
            this._borderPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _outerPanel
            // 
            this._outerPanel.AutoSize = true;
            this._outerPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._outerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._outerPanel.Controls.Add(this._borderPanel);
            this._outerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._outerPanel.Location = new System.Drawing.Point(1, 1);
            this._outerPanel.Margin = new System.Windows.Forms.Padding(0);
            this._outerPanel.Name = "_outerPanel";
            this._outerPanel.Padding = new System.Windows.Forms.Padding(3);
            this._outerPanel.Size = new System.Drawing.Size(140, 166);
            this._outerPanel.TabIndex = 5;
            this._outerPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.HandleOuterPanel_Paint);
            // 
            // _borderPanel
            // 
            this._borderPanel.AutoSize = true;
            this._borderPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._borderPanel.BackColor = System.Drawing.SystemColors.Control;
            this._borderPanel.Controls.Add(this._loadNextDocumentButton);
            this._borderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._borderPanel.Location = new System.Drawing.Point(3, 3);
            this._borderPanel.Margin = new System.Windows.Forms.Padding(0);
            this._borderPanel.Name = "_borderPanel";
            this._borderPanel.Padding = new System.Windows.Forms.Padding(17, 30, 17, 30);
            this._borderPanel.Size = new System.Drawing.Size(132, 158);
            this._borderPanel.TabIndex = 6;
            // 
            // _loadNextDocumentButton
            // 
            this._loadNextDocumentButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._loadNextDocumentButton.Location = new System.Drawing.Point(20, 33);
            this._loadNextDocumentButton.Name = "_loadNextDocumentButton";
            this._loadNextDocumentButton.Size = new System.Drawing.Size(92, 92);
            this._loadNextDocumentButton.TabIndex = 5;
            this._loadNextDocumentButton.Text = "Load next document";
            this._loadNextDocumentButton.UseVisualStyleBackColor = true;
            this._loadNextDocumentButton.Click += new System.EventHandler(this.HandleLoadNextDocumentButton_Click);
            // 
            // LoadNextDocumentButtonControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this._outerPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "LoadNextDocumentButtonControl";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Size = new System.Drawing.Size(142, 168);
            this._outerPanel.ResumeLayout(false);
            this._outerPanel.PerformLayout();
            this._borderPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel _outerPanel;
        private System.Windows.Forms.Button _loadNextDocumentButton;
        private System.Windows.Forms.Panel _borderPanel;
    }
}
