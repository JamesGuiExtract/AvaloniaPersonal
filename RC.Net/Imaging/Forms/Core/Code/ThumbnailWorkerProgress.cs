using Leadtools;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents the loading of a single page.
    /// </summary>
    public class ThumbnailWorkerProgress
    {
        #region ThumbnailWorkerProgress Fields

        readonly int _page;
        readonly RasterImage _image;

        #endregion ThumbnailWorkerProgress Fields

        #region ThumbnailWorkerProgress Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbnailWorkerProgress"/> class.
        /// </summary>
        public ThumbnailWorkerProgress(int page, RasterImage image)
        {
            _page = page;
            _image = image;
        }

        #endregion ThumbnailWorkerProgress Constructors

        #region ThumbnailWorkerProgress Properties

        /// <summary>
        /// Gets the recently loaded page number.
        /// </summary>
        /// <value>The recently loaded page number.</value>
        public int PageNumber
        {
            get
            {
                return _page;
            }
        }
        /// <summary>
        /// Gets the recently loaded page.
        /// </summary>
        /// <value>The recently loaded page.</value>
        public RasterImage Image
        {
            get
            {
                return _image;
            }
        }

        #endregion ThumbnailWorkerProgress Properties
    }
}
