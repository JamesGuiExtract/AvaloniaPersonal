using System.Drawing;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// A derived <see cref="ToolStripButton"/> that contains the image for the thumbnail viewer.
    /// </summary>
    [ToolboxBitmap(typeof(ThumbnailViewerToolStripButton),
        ToolStripButtonConstants._THUMBNAIL_VIEWER_BUTTON_IMAGE)]
    public partial class ThumbnailViewerToolStripButton : DockableWindowToolStripButton
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbnailViewerToolStripButton"/>
        /// class.
        /// </summary>
        public ThumbnailViewerToolStripButton()
            : base(typeof(ThumbnailViewerToolStripButton),
            ToolStripButtonConstants._THUMBNAIL_VIEWER_BUTTON_IMAGE)
        {
            base.Text = ToolStripButtonConstants._THUMBNAIL_VIEWER_BUTTON_TEXT;
        }

        #endregion Constructors
    }
}
