namespace Extract.Imaging.Forms
{
    partial class DocumentViewer
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
            this.imageViewer1 = new Extract.Imaging.Forms.ImageViewer();
            this.richTextViewer1 = new Extract.Imaging.Forms.RichTextViewer();
            this.blur = new System.Windows.Forms.Label();

            this.SuspendLayout();
            // 
            // imageViewer1
            // 
            this.imageViewer1.AutoOcr = false;
            this.imageViewer1.AutoZoomScale = 0;
            this.imageViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageViewer1.InvertColors = false;
            this.imageViewer1.Location = new System.Drawing.Point(0, 0);
            this.imageViewer1.MaintainZoomLevelForNewPages = true;
            this.imageViewer1.MinimumAngularHighlightHeight = 4;
            this.imageViewer1.Name = "imageViewer1";
            this.imageViewer1.OcrTradeoff = Extract.Imaging.OcrTradeoff.Accurate;
            this.imageViewer1.RedactionMode = false;
            this.imageViewer1.Size = new System.Drawing.Size(631, 348);
            this.imageViewer1.TabIndex = 0;
            // 
            // richTextViewer1
            // 
            this.richTextViewer1.AutoOcr = false;
            this.richTextViewer1.AutoWordSelection = false;
            this.richTextViewer1.AutoZoomScale = 0;
            this.richTextViewer1.BackColor = System.Drawing.Color.White;
            this.richTextViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextViewer1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextViewer1.Location = new System.Drawing.Point(0, 0);
            this.richTextViewer1.MaintainZoomLevelForNewPages = true;
            this.richTextViewer1.Name = "richTextViewer1";
            this.richTextViewer1.ReadOnly = true;
            this.richTextViewer1.Size = new System.Drawing.Size(631, 348);
            this.richTextViewer1.TabIndex = 1;
            this.richTextViewer1.Text = "";
            this.richTextViewer1.WordWrap = true;
            // 
            // DocumentViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.richTextViewer1);
            this.Controls.Add(this.imageViewer1);
            this.Controls.Add(this.blur);
            this.Name = "DocumentViewer";
            this.Size = new System.Drawing.Size(631, 348);
            this.ResumeLayout(false);

        }

        #endregion

        private ImageViewer imageViewer1;
        private RichTextViewer richTextViewer1;
        private System.Windows.Forms.Label blur;
    }
}
