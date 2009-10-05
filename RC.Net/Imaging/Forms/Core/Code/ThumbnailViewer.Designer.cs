namespace Extract.Imaging.Forms
{
    partial class ThumbnailViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="ThumbnailViewer"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ThumbnailViewer"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeThumbnails();

                // Release managed resources
                if (components != null)
                {
                    components.Dispose();
                }
                if (_worker != null)
                {
                    _worker.Dispose();
                    _worker = null;
                }
                
            }

            // Release unmanaged resources

            // Call base dispose method
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._imageList = new Leadtools.WinForms.RasterImageList();
            this.SuspendLayout();
            // 
            // _imageList
            // 
            this._imageList.AutoDisposeImages = false;
            this._imageList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageList.ItemImageSize = new System.Drawing.Size(175, 175);
            this._imageList.ItemSize = new System.Drawing.Size(200, 200);
            this._imageList.Location = new System.Drawing.Point(0, 0);
            this._imageList.Name = "rasterImageList1";
            this._imageList.Size = new System.Drawing.Size(225, 400);
            this._imageList.TabIndex = 0;
            this._imageList.SelectedIndexChanged += new System.EventHandler(this.HandleImageListSelectedIndexChanged);
            this._imageList.Scroll += new System.EventHandler(this.HandleImageListScroll);
            // 
            // ThumbnailViewer
            // 
            this.Controls.Add(this._imageList);
            this.Name = "ThumbnailViewer";
            this.Size = new System.Drawing.Size(225, 400);
            this.ResumeLayout(false);

        }

        #endregion

        private Leadtools.WinForms.RasterImageList _imageList;
    }
}
