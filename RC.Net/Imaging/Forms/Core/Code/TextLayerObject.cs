using Extract.Drawing;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Xml;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents an <see cref="AnchoredObject"/> that contains text.
    /// </summary>
    public class TextLayerObject : AnchoredObject
    {
        #region Constants

        /// <summary>
        /// If a border color is specified, the number of pixels outside the text bounds the 
        /// border should be drawn.
        /// </summary>
        const int _BORDER_PADDING = 4;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The text displayed by the <see cref="TextLayerObject"/>.
        /// </summary>
        string _text;

        /// <summary>
        /// The font to use to display the <see cref="_text"/> in logical (image) pixels.
        /// </summary>
        Font _pixelFont;

        /// <summary>
        /// The font to use to display the <see cref="_text"/> in its original units.
        /// </summary>
        Font _font;

        /// <summary>
        /// The angle (in degrees) the text layer object should be drawn at in respect to the image
        /// coordinates.
        /// </summary>
        float _orientation;

        /// <summary>
        /// The bounds of the text layer object in logical (image) coordinates.
        /// </summary>
        Rectangle _bounds;

        /// <summary>
        /// A non-transparent color that should be used to fill in the background.
        /// </summary>
        Color? _backgroundColor;

        /// <summary>
        /// A color that should be used to draw a border around the text.
        /// </summary>
        Color? _borderColor;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new instance of the <see cref="TextLayerObject"/> class.</overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="TextLayerObject"/> class.
        /// </summary>
        protected TextLayerObject()
        {
            // Needed for serialization
            ShowInMagnifier = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextLayerObject"/> class.
        /// </summary>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> on which the 
        /// <see cref="TextLayerObject"/> appears.</param>
        /// <param name="pageNumber">The page on which the <see cref="TextLayerObject"/> appears.
        /// </param>
        /// <param name="comment">The method by which the <see cref="TextLayerObject"/> was 
        /// created.</param>
        /// <param name="text">The text that appears on the <see cref="TextLayerObject"/>.</param>
        /// <param name="font">The font to use to display the <paramref name="text"/>.</param>
        /// <param name="anchorPoint">The anchor point of the <see cref="TextLayerObject"/> in 
        /// logical (image) coordinates.</param>
        /// <param name="backgroundColor">If not <see langword="null"/>, the background of the layer
        /// object will be filled in with the specified color obscuring the image region beneath.
        /// </param>
        /// <param name="borderColor">If not <see langword="null"/>, a border of the specified color
        /// will appear around the text. In this case, the area of the <see cref="TextLayerObject"/>
        /// will be expanded by a few pixels to allow for some space between the border and the text.
        /// </param>
        /// <param name="anchorAlignment">The alignment of <paramref name="anchorPoint"/> relative 
        /// to the <see cref="TextLayerObject"/>.</param>
        public TextLayerObject(ImageViewer imageViewer, int pageNumber, string comment, 
            string text, Font font, Point anchorPoint, AnchorAlignment anchorAlignment,
            Color? backgroundColor, Color? borderColor)
            : this(imageViewer, pageNumber, comment, text, font, anchorPoint, anchorAlignment,
                backgroundColor, borderColor, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextLayerObject"/> class.
        /// </summary>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> on which the 
        /// <see cref="TextLayerObject"/> appears.</param>
        /// <param name="pageNumber">The page on which the <see cref="TextLayerObject"/> appears.
        /// </param>
        /// <param name="comment">The method by which the <see cref="TextLayerObject"/> was 
        /// created.</param>
        /// <param name="text">The text that appears on the <see cref="TextLayerObject"/>.</param>
        /// <param name="font">The font to use to display the <paramref name="text"/>.</param>
        /// <param name="anchorPoint">The anchor point of the <see cref="TextLayerObject"/> in 
        /// logical (image) coordinates.</param>
        /// <param name="anchorAlignment">The alignment of <paramref name="anchorPoint"/> relative 
        /// to the <see cref="TextLayerObject"/>.</param>
        /// <param name="backgroundColor">If not <see langword="null"/>, the background of the layer
        /// object will be filled in with the specified color obscuring the image region beneath.
        /// </param>
        /// <param name="borderColor">If not <see langword="null"/>, a border of the specified color
        /// will appear around the text. In this case, the area of the <see cref="TextLayerObject"/>
        /// will be expanded by a few pixels to allow for some space between the border and the text.
        /// </param>
        /// <param name="orientation">The angle in degrees the <see cref="TextLayerObject"/> should
        /// be drawn at in respect to the image coordinates.</param>
        public TextLayerObject(ImageViewer imageViewer, int pageNumber, string comment,
            string text, Font font, Point anchorPoint, AnchorAlignment anchorAlignment,
            Color? backgroundColor, Color? borderColor, float orientation)
            : base(imageViewer, pageNumber, comment, anchorPoint, anchorAlignment)
        {
            try
            {
                ShowInMagnifier = false;

                _text = text;
                SetFont(font);

                _backgroundColor = backgroundColor;
                _borderColor = borderColor;

                _orientation = orientation;

                UpdateBounds();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23132", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the text on the <see cref="TextLayerObject"/>.
        /// </summary>
        /// <value>The text on the <see cref="TextLayerObject"/>.</value>
        /// <returns>The text on the <see cref="TextLayerObject"/>.</returns>
        [Category("Appearance")]
        [Description("The text of the object.")]
        [ReadOnly(true)]
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;

                UpdateBounds();
            }
        }

        /// <summary>
        /// Gets or sets the font to use to display the <see cref="Text"/>.
        /// </summary>
        /// <value>The font to use to display the <see cref="Text"/>.</value>
        /// <returns>The font to use to display the <see cref="Text"/>.</returns>
        [Category("Appearance")]
        [Description("The font used to display the object.")]
        public Font Font
        {
            get
            {
                return _font;
            }
            set
            {
                SetFont(value);

                UpdateBounds();
            }
        }

        /// <summary>
        /// Gets or sets the anchor point of the <see cref="TextLayerObject"/> in logical (image) 
        /// coordinates.
        /// </summary>
        /// <value>The anchor point of the <see cref="TextLayerObject"/> in logical (image) 
        /// coordinates.</value>
        /// <returns>The anchor point of the <see cref="TextLayerObject"/> in logical (image) 
        /// coordinates.</returns>
        public override Point AnchorPoint
        {
            get
            {
                return base.AnchorPoint;
            }
            set
            {
                base.AnchorPoint = value;

                UpdateBounds();
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the <see cref="AnchorPoint"/> relative to the 
        /// <see cref="TextLayerObject"/>.
        /// </summary>
        /// <value>The alignment of the <see cref="AnchorPoint"/> relative to the 
        /// <see cref="TextLayerObject"/>.</value>
        /// <returns>The alignment of the <see cref="AnchorPoint"/> relative to the 
        /// <see cref="TextLayerObject"/>.</returns>
        public override AnchorAlignment AnchorAlignment
        {
            get
            {
                return base.AnchorAlignment;
            }
            set
            {
                base.AnchorAlignment = value;

                UpdateBounds();
            }
        }

        /// <summary>
        /// Gets the top left <see cref="Point"/> for the <see cref="TextLayerObject"/>.
        /// </summary>
        /// <returns>The top left <see cref="Point"/> for the <see cref="TextLayerObject"/>.
        /// </returns>
        public override Point Location
        {
            get
            {
                return GetBounds().Location;
            }
        }

        /// <summary>
        /// Gets or sets the image viewer associated with the <see cref="TextLayerObject"/>.
        /// </summary>
        /// <value>The image viewer associated with the <see cref="TextLayerObject"/>.</value>
        /// <returns>The image viewer associated with the <see cref="TextLayerObject"/>.</returns>
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
        /// Gets or sets the background color to use.
        /// </summary>
        /// <value>If not <see langword="null"/>, the background of the layer object will be filled
        /// in with the specified <see cref="Color"/> obscuring the image region beneath; If 
        /// <see langword="null"/>, no background color will be used.</value>
        /// <returns>The background <see cref="Color"/> being used or <see langword="null"/> if no
        /// background color is being used.</returns>
        public Color? BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }

            set
            {
                _backgroundColor = value;
            }
        }

        /// <summary>
        /// Gets or sets a <see cref="Color"/> with which to draw a border around the text.
        /// </summary>
        /// <value>If not <see langword="null"/>, a border of the specified color
        /// will appear around the text. In this case, the area of the <see cref="TextLayerObject"/>
        /// will be expanded by a few pixels to allow for some space between the border and the text.
        /// </value>
        /// <returns>The <see cref="Color"/> of the border or <see langword="null"/> if no border is
        /// being used.</returns>
        public Color? BorderColor
        {
            get
            {
                return _borderColor;
            }

            set
            {
                try
                {
                    if (_borderColor != value)
                    {
                        _borderColor = value;

                        // The bounds of the object need to be updated to account for a gap that is
                        // used between the text and the border (if present).
                        UpdateBounds();
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24960", ex);
                }
            }
        }

        /// <summary>
        /// The angle (in degrees) the <see cref="TextLayerObject"/> is drawn at in respect to the
        /// image coordinates.
        /// </summary>
        /// <value>The angle (in degrees) the <see cref="TextLayerObject"/> should be drawn at in
        /// respect to the image coordinates.</value>
        /// <returns>The angle (in degrees) the <see cref="TextLayerObject"/> is drawn an at in
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
                    throw ExtractException.AsExtractException("ELI25683", ex);
                }
            }
        }
        
        #endregion Properties

        #region Methods

        /// <summary>
        /// Sets the <see cref="_font"/> and <see cref="_pixelFont"/> using the specified font.
        /// </summary>
        /// <param name="font">The font to use to set the font.</param>
        void SetFont(Font font)
        {
            if (_font == font)
            {
                return;
            }

            if (_font != null)
            {
                _font.Dispose();
            }
            _font = font;

            if (_pixelFont != null)
            {
                _pixelFont.Dispose();
                _pixelFont = null;
            }
        }

        /// <summary>
        /// Gets the current font in logical (image) pixels.
        /// </summary>
        /// <returns>The current font in logical (image) pixels.</returns>
        Font GetPixelFont()
        {
            if (_pixelFont == null)
            {
                // Get the Y resolution from the image viewer
                int yResolution = (int) base.ImageViewer.ImageDpiY;
                _pixelFont = FontMethods.ConvertFontToUnits(_font, yResolution, GraphicsUnit.Pixel);
            }

            return _pixelFont;
        }
 
        /// <summary>
        /// Translates the <see cref="TextLayerObject"/> by the specified point and optionally 
        /// raises events.
        /// </summary>
        /// <param name="offsetBy">The point by which to translate the 
        /// <see cref="TextLayerObject"/> in logical (image) coordinates.</param>
        /// <param name="raiseEvents"><see langword="true"/> if events should be raised; 
        /// <see langword="false"/> if no events should be raised.</param>
        public override void Offset(Point offsetBy, bool raiseEvents)
        {
            base.Offset(offsetBy, raiseEvents);

            UpdateBounds();
        }

        /// <summary>
        /// Restores the spatial data associated with the <see cref="TextLayerObject"/> to its 
        /// state at the start of an interactive tracking event.
        /// </summary>
        public override void Restore()
        {
            base.Restore();

            UpdateBounds();
        }

        /// <summary>
        /// Paints the <see cref="TextLayerObject"/> within the specified region using the 
        /// specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The graphics object on which to paint in logical (image) 
        /// coordinates.</param>
        /// <param name="clip">The area within which the <see cref="TextLayerObject"/> should be 
        /// clipped in destination coordinates.</param>
        /// <param name="transform">A 3x3 affine matrix that maps logical (image) coordinates to 
        /// destination coordinates.</param>
        public override void Paint(Graphics graphics, Region clip, Matrix transform)
        {
            try
            {
                // Ensure this text layer object is attached to an image viewer
                if (base.ImageViewer == null)
                {
                    return;
                }

                // Draw the text on the specified graphics object
                DrawingMethods.DrawString(_text, graphics, transform, GetPixelFont(),
                    _BORDER_PADDING, _orientation, _bounds, _backgroundColor, _borderColor);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26565", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified point is contained by the 
        /// <see cref="TextLayerObject"/>.
        /// </summary>
        /// <param name="point">The point to test for containment in logical (image) coordinates.
        /// </param>
        /// <returns><see langword="true"/> if the point is contained by the 
        /// <see cref="TextLayerObject"/>; <see langword="false"/> if the point is not contained.
        /// </returns>
        public override bool HitTest(Point point)
        {
            try
            {
                // Ensure the text layer is on the active page            
                ImageViewer imageViewer = base.ImageViewer;
                return imageViewer != null && imageViewer.PageNumber == PageNumber &&
                    GetBounds().Contains(point);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26566", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified rectangle intersects with the
        /// <see cref="TextLayerObject"/>.
        /// </summary>
        /// <param name="rectangle">The rectangle to test for containment in logical (image)
        /// coordinates.
        /// </param>
        /// <returns><see langword="true"/> if the rectangle intersects the
        /// <see cref="TextLayerObject"/>; <see langword="false"/> if it does not.</returns>
        public override bool HitTest(Rectangle rectangle)
        {
            try
            {
                return GetBounds().IntersectsWith(rectangle);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31314", ex);
            }
        }

        /// <summary>
        /// Gets the smallest rectangle that contains the <see cref="TextLayerObject"/> in 
        /// physical (client) coordinates.
        /// </summary>
        /// <returns>The smallest rectangle that contains the <see cref="TextLayerObject"/> in 
        /// physical (client) coordinates.
        /// </returns>
        // This method performs a computation, so is better suited as a method
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Rectangle GetClientBounds()
        {
            // Return the bounds in physical (client) coordinates
            return base.ImageViewer.GetTransformedRectangle(GetBounds(), false);
        }

        /// <summary>
        /// Gets the smallest rectangle that contains the <see cref="TextLayerObject"/> in logical 
        /// (image) coordinates.
        /// </summary>
        /// <returns>The smallest rectangle that contains the <see cref="TextLayerObject"/> in 
        /// logical (image) coordinates.
        /// </returns>
        public override Rectangle GetBounds()
        {
            try
            {
                PointF[] vertices = DrawingMethods.GetVertices(_bounds, _orientation);

                // Round the vertices to the nearest pixel
                Point[] rounded = new Point[vertices.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    rounded[i] = Point.Round(vertices[i]);
                }

                // Return the bounds of the text layer object
                return GeometryMethods.GetBoundingRectangle(rounded);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25681", ex);
            }
        }

        /// <summary>
        /// Retrieves the vertices of the <see cref="TextLayerObject"/> in logical (image) 
        /// coordinates.
        /// </summary>
        /// <returns>The vertices of the <see cref="TextLayerObject"/> in logical (image) 
        /// coordinates.</returns>
        public override PointF[] GetVertices()
        {
            try
            {
                return DrawingMethods.GetVertices(_bounds, _orientation);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25680", ex);
            }
        }

        /// <summary>
        /// Retrieves the vertices of the selection border in logical (image) coordinates.
        /// </summary>
        /// <returns>The vertices of the selection border in logical (image) coordinates.</returns>
        public override PointF[] GetGripVertices()
        {
            return GetVertices();
        }

        /// <summary>
        /// Determines whether the specified rectangle intersects the 
        /// <see cref="TextLayerObject"/>.
        /// </summary>
        /// <param name="rectangle">The rectangle in logical (image) coordinates to check for 
        /// intersection.</param>
        /// <returns><see langword="true"/> if the <paramref name="rectangle"/> intersects the 
        /// <see cref="TextLayerObject"/>; <see langword="false"/> if the 
        /// <paramref name="rectangle"/> does not intersect the <see cref="TextLayerObject"/>.
        /// </returns>
        public override bool IsVisible(Rectangle rectangle)
        {
            return base.IsVisible(rectangle) && GetBounds().IntersectsWith(rectangle);
        }

        #endregion Methods

        #region IDisposable Members

        /// <overloads>Releases resources used by the <see cref="TextLayerObject"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="TextLayerObject"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_font != null)
                {
                    _font.Dispose();
                }
                if (_pixelFont != null)
                {
                    _pixelFont.Dispose();
                }
            }

            // Dispose of unmanaged resources

            // Dispose of base class
            base.Dispose(disposing);
        }

        #endregion IDisposable Members

        #region IXmlSerializable Members

        /// <summary>
        /// Generates a <see cref="TextLayerObject"/> from its XML representation.
        /// </summary>
        /// <param name="reader">The stream from which the <see cref="TextLayerObject"/> is 
        /// deserialized.</param>
        public override void ReadXml(XmlReader reader)
        {
            try
            {
                base.ReadXml(reader);

                // Read the font
                if (reader.Name != "Font")
                {
                    throw new ExtractException("ELI22870", "Invalid format.");
                }
                string family = reader.GetAttribute("Family");
                FontStyle style = (FontStyle)
                    Enum.Parse(typeof(FontStyle), reader.GetAttribute("Style"));
                float size = 
                    Convert.ToSingle(reader.GetAttribute("Size"), CultureInfo.CurrentCulture);
                SetFont(new Font(family, size, style, GraphicsUnit.Point));
                reader.Read();

                // Read the text
                if (reader.Name != "Text")
                {
                    throw new ExtractException("ELI22900", "Invalid format.");
                }
                _text = reader.ReadElementContentAsString();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22924", ex);
            }
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The stream to which the <see cref="TextLayerObject"/> is 
        /// serialized.</param>
        public override void WriteXml(XmlWriter writer)
        {
            try
            {
                base.WriteXml(writer);

                // Write the font
                writer.WriteStartElement("Font");
                writer.WriteAttributeString("Family", _font.FontFamily.Name);
                writer.WriteAttributeString("Style", _font.Style.ToString());
                writer.WriteAttributeString("Size", 
                    _font.SizeInPoints.ToString(CultureInfo.CurrentCulture));
                writer.WriteEndElement();

                // Write the text
                writer.WriteElementString("Text", _text);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22925", ex);
            }
        }

        #endregion IXmlSerializable Members

        #region Private Members

        /// <summary>
        /// Calculates the size and top left coordinate of the <see cref="TextLayerObject"/> in 
        /// logical (image) coordinates.
        /// </summary>
        private void UpdateBounds()
        {
            // Calculate the size and position of the text layer object's bounds.
            using (Graphics graphics = base.ImageViewer.CreateGraphics())
            {
                // Update the bounds
                _bounds = DrawingMethods.ComputeStringBounds(_text, graphics, GetPixelFont(),
                    _BORDER_PADDING, _orientation, base.AnchorPoint, base.AnchorAlignment);
            }
        }

        #endregion Private Members
    }
}
