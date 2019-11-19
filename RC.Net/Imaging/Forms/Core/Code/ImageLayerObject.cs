using Extract.Drawing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents an <see cref="AnchoredObject"/> that contains an image.
    /// </summary>
    public class ImageLayerObject : AnchoredObject
    {
        #region Fields

        /// <summary>
        /// The image displayed by the <see cref="ImageLayerObject"/>.
        /// </summary>
        Image _image;

        /// <summary>
        /// The size of the displayed image in page pixel coordinates.
        /// </summary>
        Size _size;

        /// <summary>
        /// The angle (in degrees) the image layer object should be drawn at in respect to the image
        /// coordinates.
        /// </summary>
        float _orientation;

        /// <summary>
        /// The bounds of the image layer object in logical (image) coordinates.
        /// </summary>
        Rectangle _bounds;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new instance of the <see cref="ImageLayerObject"/> class.</overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageLayerObject"/> class.
        /// </summary>
        protected ImageLayerObject()
        {
            // Needed for serialization
            ShowInMagnifier = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageLayerObject"/> class.
        /// </summary>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> on which the 
        /// <see cref="ImageLayerObject"/> appears.</param>
        /// <param name="pageNumber">The page on which the <see cref="ImageLayerObject"/> appears.
        /// </param>
        /// <param name="comment">The method by which the <see cref="ImageLayerObject"/> was 
        /// created.</param>
        /// <param name="anchorPoint">The anchor point of the <see cref="ImageLayerObject"/> in 
        /// logical (image) coordinates.</param>
        /// <param name="anchorAlignment">The alignment of <paramref name="anchorPoint"/> relative 
        /// to the <see cref="ImageLayerObject"/>.</param>
        /// <param name="image">The <see cref="Image"/> to be displayed. The image will not be
        /// disposed of by the <see cref="ImageLayerObject"/> and the image must not be disposed of
        /// by an outside class during the lifetime of the <see cref="ImageLayerObject"/>.</param>
        /// <param name="size">The size of the displayed image in page pixel coordinates.</param>
        /// <param name="orientation">The angle (in degrees) the image layer object should be drawn
        /// at in respect to the image coordinates.</param>
        public ImageLayerObject(ImageViewer imageViewer, int pageNumber, string comment,
            Point anchorPoint, AnchorAlignment anchorAlignment, Image image, Size size, 
            float orientation)
            : base(imageViewer, pageNumber, comment, anchorPoint, anchorAlignment)
        {
            try
            {
                ShowInMagnifier = false;
                _image = image;
                _size = size;
                _orientation = orientation;

                UpdateBounds();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25921", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Image"/> displayed by the <see cref="ImageLayerObject"/>.
        /// </summary>
        /// <value>The <see cref="Image"/> displayed by the <see cref="ImageLayerObject"/>.</value>
        /// <returns>The <see cref="Image"/> displayed by the <see cref="ImageLayerObject"/>.
        /// </returns>
        public Image Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Size"/> of the <see cref="ImageLayerObject"/>.
        /// </summary>
        /// <value>The <see cref="Size"/> of the <see cref="ImageLayerObject"/>.</value>
        /// <returns>The <see cref="Size"/> of the <see cref="ImageLayerObject"/>.</returns>
        public Size Size
        {
            get
            {
                return _size;
            }
            set
            {
                try
                {
                    _size = value;

                    UpdateBounds();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25693", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the anchor point of the <see cref="ImageLayerObject"/> in logical (image)
        /// coordinates.
        /// </summary>
        /// <value>The anchor point of the <see cref="ImageLayerObject"/> in logical (image) 
        /// coordinates.</value>
        /// <returns>The anchor point of the <see cref="ImageLayerObject"/> in logical (image) 
        /// coordinates.</returns>
        public override Point AnchorPoint
        {
            get
            {
                return base.AnchorPoint;
            }
            set
            {
                try
                {
                    base.AnchorPoint = value;

                    UpdateBounds();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25922", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the <see cref="AnchorPoint"/> relative to the 
        /// <see cref="ImageLayerObject"/>.
        /// </summary>
        /// <value>The alignment of the <see cref="AnchorPoint"/> relative to the 
        /// <see cref="ImageLayerObject"/>.</value>
        /// <returns>The alignment of the <see cref="AnchorPoint"/> relative to the 
        /// <see cref="ImageLayerObject"/>.</returns>
        public override AnchorAlignment AnchorAlignment
        {
            get
            {
                return base.AnchorAlignment;
            }
            set
            {
                try
                {
                    base.AnchorAlignment = value;

                    UpdateBounds();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25923", ex);
                }
            }
        }

        /// <summary>
        /// Gets the top left <see cref="Point"/> for the <see cref="ImageLayerObject"/>.
        /// </summary>
        /// <returns>The top left <see cref="Point"/> for the <see cref="ImageLayerObject"/>.
        /// </returns>
        public override Point Location
        {
            get
            {
                return GetBounds().Location;
            }
        }

        /// <summary>
        /// Gets or sets the image viewer associated with the <see cref="ImageLayerObject"/>.
        /// </summary>
        /// <value>The image viewer associated with the <see cref="ImageLayerObject"/>.</value>
        /// <returns>The image viewer associated with the <see cref="ImageLayerObject"/>.</returns>
        public override ImageViewer ImageViewer
        {
            get
            {
                return base.ImageViewer;
            }
            set
            {
                base.ImageViewer = value;

                // Update the bounds with the new image
                if (value != null)
                {
                    UpdateBounds();
                }
            }
        }

        /// <summary>
        /// The angle (in degrees) the <see cref="ImageLayerObject"/> is drawn at in respect to the
        /// image coordinates.
        /// </summary>
        /// <value>The angle (in degrees) the <see cref="ImageLayerObject"/> should be drawn at in
        /// respect to the image coordinates.</value>
        /// <returns>The angle (in degrees) the <see cref="ImageLayerObject"/> is drawn an at in
        /// respect to the image coordinates.</returns>
        public float Orientation
        {
            get
            {
                return _orientation;
            }

            set
            {
                try
                {
                    if (_orientation != value)
                    {
                        _orientation = value;

                        UpdateBounds();
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25924", ex);
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Translates the <see cref="ImageLayerObject"/> by the specified point and optionally 
        /// raises events.
        /// </summary>
        /// <param name="offsetBy">The point by which to translate the 
        /// <see cref="ImageLayerObject"/> in logical (image) coordinates.</param>
        /// <param name="raiseEvents"><see langword="true"/> if events should be raised; 
        /// <see langword="false"/> if no events should be raised.</param>
        public override void Offset(Point offsetBy, bool raiseEvents)
        {
            try
            {
                base.Offset(offsetBy, raiseEvents);

                UpdateBounds();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25694", ex);
            }
        }

        /// <summary>
        /// Restores the spatial data associated with the <see cref="ImageLayerObject"/> to its 
        /// state at the start of an interactive tracking event.
        /// </summary>
        public override void Restore()
        {
            try
            {
                base.Restore();

                UpdateBounds();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25695", ex);
            }
        }

        /// <summary>
        /// Paints the <see cref="ImageLayerObject"/> within the specified region using the 
        /// specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The graphics object on which to paint in logical (image) 
        /// coordinates.</param>
        /// <param name="clip">The area within which the <see cref="ImageLayerObject"/> should be 
        /// clipped in destination coordinates.</param>
        /// <param name="transform">A 3x3 affine matrix that maps logical (image) coordinates to 
        /// destination coordinates.</param>
        public override void Paint(Graphics graphics, Region clip, Matrix transform)
        {
            try
            {
                // Ensure this image layer object is attached to an image viewer
                if (base.ImageViewer == null)
                {
                    return;
                }

                ExtractException.Assert("ELI25936",
                    "An image must be assigned to an ImageLayerObject before use!", _image != null);

                // Save the original graphics settings
                Matrix originalTransform = graphics.Transform;
                InterpolationMode originalInterpolationMode = graphics.InterpolationMode;
                CompositingQuality originalCompositingQuality = graphics.CompositingQuality;
                SmoothingMode originalSmoothingMode = graphics.SmoothingMode;
                PixelOffsetMode originalPixelOffsetMode = graphics.PixelOffsetMode;

                try
                {
                    // Apply settings to allow the resized image to look good.
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    // Set a new transformation matrix, so that the image will be drawn with the same
                    // orientation as the page.
                    graphics.Transform = transform;

                    // Apply any specified rotation to the to the graphics object.
                    graphics.RotateTransform(_orientation);

                    graphics.DrawImage(_image, _bounds);
                }
                finally
                {
                    // Restore the original graphics settings
                    graphics.Transform = originalTransform;
                    graphics.InterpolationMode = originalInterpolationMode;
                    graphics.CompositingQuality = originalCompositingQuality;
                    graphics.SmoothingMode = originalSmoothingMode;
                    graphics.PixelOffsetMode = originalPixelOffsetMode;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25925", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified point is contained by the 
        /// <see cref="ImageLayerObject"/>.
        /// </summary>
        /// <param name="point">The point to test for containment in logical (image) coordinates.
        /// </param>
        /// <returns><see langword="true"/> if the point is contained by the 
        /// <see cref="ImageLayerObject"/>; <see langword="false"/> if the point is not contained.
        /// </returns>
        public override bool HitTest(Point point)
        {
            try
            {
                // Ensure the image layer is on the active page            
                ImageViewer imageViewer = base.ImageViewer;
                return imageViewer != null && imageViewer.PageNumber == PageNumber &&
                    GetBounds().Contains(point);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25926", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified rectangle intersects with the
        /// <see cref="ImageLayerObject"/>.
        /// </summary>
        /// <param name="rectangle">The rectangle to test for containment in logical (image)
        /// coordinates.
        /// </param>
        /// <returns><see langword="true"/> if the rectangle intersects the
        /// <see cref="ImageLayerObject"/>; <see langword="false"/> if it does not.</returns>
        public override bool HitTest(Rectangle rectangle)
        {
            try
            {
                return GetBounds().IntersectsWith(rectangle);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31289", ex);
            }
        }

        /// <summary>
        /// Gets the smallest rectangle that contains the <see cref="ImageLayerObject"/> in 
        /// physical (client) coordinates.
        /// </summary>
        /// <returns>The smallest rectangle that contains the <see cref="ImageLayerObject"/> in 
        /// physical (client) coordinates.
        /// </returns>
        // This method performs a computation, so is better suited as a method
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Rectangle GetClientBounds()
        {
            try
            {
                // Return the bounds in physical (client) coordinates
                return base.ImageViewer.GetTransformedRectangle(GetBounds(), false);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25927", ex);
            }
        }

        /// <summary>
        /// Gets the smallest rectangle that contains the <see cref="ImageLayerObject"/> in logical 
        /// (image) coordinates.
        /// </summary>
        /// <returns>The smallest rectangle that contains the <see cref="ImageLayerObject"/> in 
        /// logical (image) coordinates.
        /// </returns>
        public override Rectangle GetBounds()
        {
            try
            {
                PointF[] vertices = GetVertices(true);

                // Round the vertices to the nearest pixel
                Point[] rounded = new Point[vertices.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    rounded[i] = Point.Round(vertices[i]);
                }

                // Return the bounds of the image layer object
                return GeometryMethods.GetBoundingRectangle(rounded);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25928", ex);
            }
        }

        /// <summary>
        /// Retrieves the vertices of the <see cref="ImageLayerObject"/> in logical (image) 
        /// coordinates.
        /// </summary>
        /// <returns>The vertices of the <see cref="ImageLayerObject"/> in logical (image) 
        /// coordinates.</returns>
        public override PointF[] GetVertices()
        {
            try
            {
                return GetVertices(true);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25929", ex);
            }
        }

        /// <summary>
        /// Retrieves the vertices of the selection border in logical (image) coordinates.
        /// </summary>
        /// <returns>The vertices of the selection border in logical (image) coordinates.</returns>
        public override PointF[] GetGripVertices()
        {
            return GetVertices(true);
        }

        /// <summary>
        /// Determines whether the specified rectangle intersects the 
        /// <see cref="ImageLayerObject"/>.
        /// </summary>
        /// <param name="rectangle">The rectangle in logical (image) coordinates to check for 
        /// intersection.</param>
        /// <returns><see langword="true"/> if the <paramref name="rectangle"/> intersects the 
        /// <see cref="ImageLayerObject"/>; <see langword="false"/> if the 
        /// <paramref name="rectangle"/> does not intersect the <see cref="ImageLayerObject"/>.
        /// </returns>
        public override bool IsVisible(Rectangle rectangle)
        {
            try
            {
                return base.IsVisible(rectangle) && GetBounds().IntersectsWith(rectangle);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25930", ex);
            }
        }

        #endregion Methods

        #region IXmlSerializable Members

        /// <summary>
        /// Generates a <see cref="ImageLayerObject"/> from its XML representation.
        /// </summary>
        /// <param name="reader">The stream from which the <see cref="ImageLayerObject"/> is 
        /// deserialized.</param>
        public override void ReadXml(XmlReader reader)
        {
            try
            {
                throw new ExtractException("ELI25697", "XML Serialization not supported!");
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25931", ex);
            }
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The stream to which the <see cref="ImageLayerObject"/> is 
        /// serialized.</param>
        public override void WriteXml(XmlWriter writer)
        {
            try
            {
                throw new ExtractException("ELI25698", "XML Serialization not supported!");
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25932", ex);
            }
        }

        #endregion IXmlSerializable Members

        #region Members

        /// <summary>
        /// Calculates the size and top left coordinate of the <see cref="ImageLayerObject"/> in 
        /// logical (image) coordinates.
        /// </summary>
        void UpdateBounds()
        {
            // Calculate the top left coordinate based on the anchor alignment
            Point[] anchorPoint = { base.AnchorPoint };

            // The image layer object may be drawn in a rotated coordinate system.  Rotate the
            // anchor point into that coordinate system.
            using (Matrix transform = new Matrix())
            {
                transform.Rotate(-_orientation);
                transform.TransformPoints(anchorPoint);
            }

            // Calculate the size and position of the image layer object's bounds.
            Point location = anchorPoint[0];

            switch (base.AnchorAlignment)
            {
                case AnchorAlignment.LeftBottom:
                    location.Offset(0, -_size.Height);
                    break;

                case AnchorAlignment.Bottom:
                    location.Offset(-_size.Width / 2, -_size.Height);
                    break;

                case AnchorAlignment.RightBottom:
                    location -= _size;
                    break;

                case AnchorAlignment.Left:
                    location.Offset(0, -_size.Height / 2);
                    break;

                case AnchorAlignment.Center:
                    location.Offset(-_size.Width / 2, -_size.Height / 2);
                    break;

                case AnchorAlignment.Right:
                    location.Offset(-_size.Width, -_size.Height / 2);
                    break;

                case AnchorAlignment.LeftTop:
                    // Do nothing - anchor point is the left top coordinate
                    break;

                case AnchorAlignment.Top:
                    location.Offset(-_size.Width / 2, 0);
                    break;

                case AnchorAlignment.RightTop:
                    location.Offset(-_size.Width, 0);
                    break;

                default:
                    ExtractException ee = new ExtractException("ELI25933",
                        "Unexpected anchor alignment.");
                    ee.AddDebugData("Anchor alignment", base.AnchorAlignment, false);
                    throw ee;
            }

            // Update the bounds
            _bounds = new Rectangle(location, _size);
        }

        /// <summary>
        /// Retrieves the vertices of the <see cref="ImageLayerObject"/> in either internal
        /// (possibly rotated) coordinate system or in logical (image) coordinates.
        /// </summary>
        /// <param name="useImageCoordinates"><see langword="true"/> to get the vertices in image
        /// coordinates or <see langword="false"/> to get the vertices in the internal
        /// coordinate system.</param>
        /// <returns>The vertices of the <see cref="ImageLayerObject"/>in the specified coordinate
        /// system.</returns>
        PointF[] GetVertices(bool useImageCoordinates)
        {
            // Construct the vertices using the bounds of the image layer object
            PointF[] vertices =
            {
                new PointF(_bounds.Left, _bounds.Top),
                new PointF(_bounds.Right, _bounds.Top),
                new PointF(_bounds.Right, _bounds.Bottom),
                new PointF(_bounds.Left, _bounds.Bottom)
            };

            // Rotate the coordinates into the image coordinate system if specified.
            if (useImageCoordinates)
            {
                using (Matrix transform = new Matrix())
                {
                    transform.Rotate(_orientation);
                    transform.TransformPoints(vertices);
                }
            }

            return vertices;
        }

        #endregion Members
    }
}