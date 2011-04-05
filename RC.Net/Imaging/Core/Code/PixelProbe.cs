using Leadtools;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace Extract.Imaging
{
    /// <summary>
    /// Represents a tool for testing the individual pixels of a black and white image.
    /// </summary>
    public sealed class PixelProbe : IDisposable
    {
        #region Fields

        /// <summary>
        /// The image being tested.
        /// </summary>
        RasterImage _image;

        /// <summary>
        /// The width of the image.
        /// </summary>
        int _width;

        /// <summary>
        /// The height of the image.
        /// </summary>
        int _height;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelProbe"/> class.
        /// </summary>
        internal PixelProbe(RasterImage image)
        {
            // Must be top-left view perspective to access pixel data accurately
            if (image.ViewPerspective != RasterViewPerspective.TopLeft)
            {
                image.ChangeViewPerspective(RasterViewPerspective.TopLeft);
            }

            _image = image;
            _width = _image.ImageWidth;
            _height = _image.ImageHeight;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Determines the pixel at the specified coordinates is black.
        /// </summary>
        /// <param name="point">The coordinates of the pixel to test.</param>
        /// <returns><see langword="true"/> if the pixel is black; <see langword="false"/> if the 
        /// pixel is white.</returns>
        public bool IsPixelBlack(Point point)
        {
            return IsPixelBlack(point.X, point.Y);
        }

        /// <summary>
        /// Determines the pixel at the specified coordinates is black.
        /// </summary>
        /// <param name="x">The x coordinate of the pixel to test.</param>
        /// <param name="y">The y coordinate of the pixel to test.</param>
        /// <returns><see langword="true"/> if the pixel is black; <see langword="false"/> if the 
        /// pixel is white.</returns>
        // X and Y are descriptive names for a coordinate
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y")]
        public bool IsPixelBlack(int x, int y)
        {
            try
            {
                RasterColor color = _image.GetPixelColor(y, x);

                return color.ToRgb() == 0;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28614",
                    "Unable to check if pixel is black.", ex);
                ee.AddDebugData("X", x, false);
                ee.AddDebugData("Y", y, false);
                throw ee;
            }
        }

        /// <summary>
        /// Determines if the specified pixel exists on the page.
        /// </summary>
        /// <param name="point">The coordinates of the pixel to test.</param>
        /// <returns><see langword="true"/> if the pixel exists on the page; 
        /// <see langword="false"/> if the coordinate describes an area outside of the page.</returns>
        public bool Contains(Point point)
        {
            return Contains(point.X, point.Y);
        }

        /// <summary>
        /// Determines if the specified pixel exists on the page.
        /// </summary>
        /// <param name="x">The x coordinate of the pixel to test.</param>
        /// <param name="y">The y coordinate of the pixel to test.</param>
        /// <returns><see langword="true"/> if the pixel exists on the page; 
        /// <see langword="false"/> if the coordinate describes an area outside of the page.</returns>
        // X and Y are descriptive names for a coordinate
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y")]
        public bool Contains(int x, int y)
        {
            try
            {
                return x >= 0 && y >= 0 && x < _width && y < _height;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28613",
                    "Unable to check point for containment.", ex);
                ee.AddDebugData("X", x, false);
                ee.AddDebugData("Y", y, false);
                throw ee;
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="PixelProbe"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="PixelProbe"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="PixelProbe"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>		
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_image != null)
                {
                    _image.Dispose();
                    _image = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}