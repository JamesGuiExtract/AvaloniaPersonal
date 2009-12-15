using Leadtools.Codecs;

namespace Extract.Imaging
{
    /// <summary>
    /// Represents the properties of a single page of an image.
    /// </summary>
    public class ImagePageProperties
    {
        #region Fields

        /// <summary>
        /// The width of the image in pixels.
        /// </summary>
        readonly int _width;

        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        readonly int _height;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePageProperties"/> class.
        /// </summary>
        internal ImagePageProperties(CodecsImageInfo imageInfo)
        {
            _width = imageInfo.Width;
            _height = imageInfo.Height;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the width of the image in pixels.
        /// </summary>
        /// <value>The width of the image in pixels.</value>
        public int Width
        {
            get
            {
                return _width;
            }
        }

        /// <summary>
        /// Gets the height of the image in pixels.
        /// </summary>
        /// <value>The height of the image in pixels.</value>
        public int Height
        {
            get
            {
                return _height;
            }
        }

        #endregion Properties
    }
}