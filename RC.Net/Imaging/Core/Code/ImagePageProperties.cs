using System;
using System.Drawing;
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

        #region Methods

        /// <summary>
        /// Determines whether the specified rectangle is completly contained by the image page.
        /// </summary>
        /// <param name="rectangle">The rectangle to test for containment.</param>
        /// <returns><see langword="true"/> if the <paramref name="rectangle"/> is completely 
        /// contained within the page; <see langword="false"/> if it is partially contained or 
        /// completely off the page.</returns>
        public bool Contains(Rectangle rectangle)
        {
            try
            {
                return rectangle.Left >= 0 && rectangle.Top >= 0 && rectangle.Width <= _width &&
                    rectangle.Height <= _height;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29184", ex);
            }
        }

        #endregion Methods
    }
}