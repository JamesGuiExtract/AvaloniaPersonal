namespace Extract.UtilityApplications.PaginationUtility
{
    partial class DropLocationIndicator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DropLocationIndicator));
            this._topPictureBox = new System.Windows.Forms.PictureBox();
            this._bottomPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this._topPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._bottomPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // _topPictureBox
            // 
            this._topPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._topPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("_topPictureBox.Image")));
            this._topPictureBox.InitialImage = ((System.Drawing.Image)(resources.GetObject("_topPictureBox.InitialImage")));
            this._topPictureBox.Location = new System.Drawing.Point(0, 0);
            this._topPictureBox.Name = "_topPictureBox";
            this._topPictureBox.Size = new System.Drawing.Size(12, 12);
            this._topPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._topPictureBox.TabIndex = 0;
            this._topPictureBox.TabStop = false;
            // 
            // _bottomPictureBox
            // 
            this._bottomPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._bottomPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("_bottomPictureBox.Image")));
            this._bottomPictureBox.InitialImage = ((System.Drawing.Image)(resources.GetObject("_bottomPictureBox.InitialImage")));
            this._bottomPictureBox.Location = new System.Drawing.Point(0, 139);
            this._bottomPictureBox.Name = "_bottomPictureBox";
            this._bottomPictureBox.Size = new System.Drawing.Size(12, 12);
            this._bottomPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._bottomPictureBox.TabIndex = 1;
            this._bottomPictureBox.TabStop = false;
            // 
            // DropLocationIndicator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this._bottomPictureBox);
            this.Controls.Add(this._topPictureBox);
            this.Name = "DropLocationIndicator";
            this.Size = new System.Drawing.Size(12, 150);
            ((System.ComponentModel.ISupportInitialize)(this._topPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._bottomPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox _topPictureBox;
        private System.Windows.Forms.PictureBox _bottomPictureBox;


    }
}
