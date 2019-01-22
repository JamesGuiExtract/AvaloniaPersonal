using System;
using System.Drawing;
using Leadtools;
using Leadtools.Codecs;

namespace Extract.Imaging
{
    /// <summary>
    /// Represents the properties of a single page of an image.
    /// </summary>
    public class ImagePageProperties
    {
        #region Constructors

        public ImagePageProperties(int width, int height, int xResolution, int yResolution)
        {
            Width = width;
            Height = height;
            XResolution = xResolution;
            YResolution = yResolution;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePageProperties"/> class.
        /// </summary>
        internal ImagePageProperties(CodecsImageInfo imageInfo)
        {
            Width = imageInfo.Width;
            Height = imageInfo.Height;
            XResolution = imageInfo.XResolution;
            YResolution = imageInfo.YResolution;
            ViewPerspective = imageInfo.ViewPerspective;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the width of the image in pixels.
        /// </summary>
        /// <value>The width of the image in pixels.</value>
        public int Width
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the height of the image in pixels.
        /// </summary>
        /// <value>The height of the image in pixels.</value>
        public int Height
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the horizontal DPI of the image.
        /// </summary>
        /// <value>The horizontal DPI of the image.</value>
        public int XResolution
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the vertical DPI of the image.
        /// </summary>
        /// <value>The horizontal DPI of the image.</value>
        public int YResolution
        {
            get;
            private set;
        }

        /// <summary>
        /// The view perspective of the image
        /// </summary>
        public RasterViewPerspective ViewPerspective { get; }

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
                return rectangle.Left >= 0 && rectangle.Top >= 0 && rectangle.Width <= Width &&
                    rectangle.Height <= Height;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29184", ex);
            }
        }

        #endregion Methods
    }
}