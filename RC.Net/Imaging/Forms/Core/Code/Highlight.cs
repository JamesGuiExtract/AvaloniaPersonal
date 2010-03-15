using Extract.Drawing;
using Extract.Utilities.Forms;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a colored angular rectangle on the <see cref="ImageViewer"/> control.
    /// </summary>
    /// <remarks>
    /// <para>A highlight is 'live' in the sense that changes made to it are instantly reflected 
    /// in the <see cref="ImageViewer"/> control to which it is associated. To delay the update, 
    /// use the <see cref="Forms.ImageViewer.BeginUpdate"/> and 
    /// <see cref="Forms.ImageViewer.EndUpdate"/> methods of the <see cref="ImageViewer"/>.</para>
    /// <para>A highlight is described spatially by its <see cref="StartPoint"/>, 
    /// <see cref="EndPoint"/>, and <see cref="Height"/> properties. The <see cref="StartPoint"/> 
    /// and <see cref="EndPoint"/> describe the endpoints of a line segment that bisects the 
    /// highlight. The <see cref="Height"/> is the distance between the sides of the highlight, 
    /// measured perpendicular to the bisecting line.</para>
    /// </remarks>
    public sealed class Highlight : LayerObject, IComparable<Highlight>
    {
        #region Constants

        /// <summary>
        /// An array of the path point types for the <see cref="GraphicsPath"/> object.
        /// </summary>
        static readonly byte[] _RECTANGULAR_PATH_POINT_TYPE = new byte[] 
        {
            (byte)PathPointType.Line,   // the first corner of the rectangle
            (byte)PathPointType.Line,   // the next corner
            (byte)PathPointType.Line,   // the next corner
            (byte)PathPointType.Line    // the last corner
        };

        #endregion Constants

        #region Fields

        /// <summary>
        /// Midpoint of starting side of the highlight in logical (image) coordinates.
        /// <remarks><see cref="_endPoint"/> is the midpoint of the opposing side.</remarks>
        /// </summary>
        /// <seealso cref="StartPoint"/>
        Point _startPoint;

        /// <summary>
        /// Midpoint of ending side of the highlight in logical (image) coordinates.
        /// <remarks><see cref="_startPoint"/> is the midpoint of the opposing side.</remarks>
        /// </summary>
        /// <seealso cref="EndPoint"/>
        Point _endPoint;

        /// <summary>
        /// Distance in pixels between the sides of the highlight measured perpendicular to 
        /// the line segment formed by <see cref="_startPoint"/> and <see cref="_endPoint"/>.
        /// </summary>
        /// <seealso cref="Height"/>
        int _height;

        /// <summary>
        /// <see cref="System.Drawing.Color"/> of the highlight.
        /// </summary>
        /// <seealso cref="Color"/>
        Color _color;

        /// <summary>
        /// <see cref="System.Drawing.Color"/> of the highlight outline. <see langword="null"/>
        /// for no outline.
        /// </summary>
        /// <seealso cref="Color"/>
        Color? _outlineColor;

        /// <summary>
        /// Color of the highlight's border; <see langword="null"/> for no border.
        /// </summary>
        Color? _borderColor;

        /// <summary>
        /// Text associated with the highlight.
        /// </summary>
        /// <seealso cref="Text"/>
        string _text;

        /// <summary>
        /// The region associated with the highlight in logical (image) coordinates.
        /// </summary>
        /// <remarks>This value is recalculated each time the highlight's spatial information is 
        /// modified. For instance, when the <see cref="StartPoint"/>, <see cref="EndPoint"/>, and
        /// <see cref="Height"/> are set.</remarks>
        /// <seealso cref="Region"/>
        /// <seealso cref="CalculateRegion"/>
        Region _region;

        /// <summary>
        /// Whether the highlight was angular when the tracking event began. Only intended to be
        /// used during a tracking event.
        /// </summary>
        /// <remarks>
        /// <para>A highlight that is angular at the start of an interactive adjust angle event 
        /// may become rectangular by the end of the event.</para>
        /// </remarks>
        bool _originalIsAngular;

        /// <summary>
        /// The original endpoints of the highlight currently being processed during an 
        /// interactive highlight event.
        /// </summary>
        Point[] _originalLine;

        /// <summary>
        /// The original height of the highlight currently being processed during an interactive
        /// highlight event.
        /// </summary>
        int _originalHeight;

        /// <summary>
        /// The x and y components of the unit vector that describes the angle of the highlight 
        /// during an interactive tracking event.
        /// </summary>
        /// <remarks>This is needed because during interactive highlight resizing, it is often not 
        /// possible to resize the highlight without losing at least some precision of the 
        /// original angle. This angle is stored at the beginning of the resizing.</remarks>
        PointF _activeHighlightVector;

        /// <summary>
        /// The angle of the highlight.  Calculated whenever CalculateRegion is called.
        /// </summary>
        double _angle;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Highlight"/> class.
        /// </summary>
        Highlight()
        {
            // Needed for serialization
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Highlight"/> class.
        /// </summary>
        internal Highlight(string comment, int pageNumber) : base(pageNumber, comment)
        {
            // Used by the composite highlight layer object
        }

        /// <overloads>Initializes a new <see cref="Highlight"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="Highlight"/> class on the currently visible page that
        /// contains the default text and is the default color.
        /// </summary>
        /// <remarks>
        /// <para>The default page number is value of the <paramref name="imageViewer"/>'s 
        /// <see cref="Forms.ImageViewer.PageNumber"/> property.</para>
        /// <para>The default text is the empty <see cref="string"/> if 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.ImageViewer.RecognizeHighlightText"/> 
        /// property is <see langword="false"/> or recognized text if the 
        /// <see cref="Forms.ImageViewer.RecognizeHighlightText"/> property is 
        /// <see langword="true"/>.
        /// </para>
        /// <para>The default <see cref="System.Drawing.Color"/> is the value of the 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.ImageViewer.DefaultHighlightColor"/> 
        /// property.</para>
        /// </remarks>
        /// <param name="imageViewer">Image viewer on which this highlight appears. Cannot be
        /// <see langword="null"/>. An image must be open on the viewer.</param>
        /// <param name="comment">The method by which the <see cref="Highlight"/> was created.
        /// </param>
        /// <param name="height">Distance between opposing sides of highlight in logical (image) 
        /// pixels, measured perpendicular to the line segment defined by <paramref name="start"/> 
        /// and <paramref name="end"/>.</param>
        /// <param name="start">Midpoint of the one side of the highlight in logical (image) 
        /// coordinates.</param>
        /// <param name="end">Midpoint of the opposing side of the highlight in logical (image) 
        /// coordinates.</param>
        /// <exception cref="ExtractException"><paramref name="imageViewer"/> is 
        /// <see langword="null"/>.</exception>
        /// <exception cref="ExtractException"><paramref name="imageViewer"/> does not contain an 
        /// open image.</exception>
        /// <seealso cref="Forms.ImageViewer.PageNumber"/>
        /// <seealso cref="Forms.ImageViewer.RecognizeHighlightText"/>
        /// <seealso cref="Forms.ImageViewer.DefaultHighlightColor"/>
        public Highlight(ImageViewer imageViewer, string comment, Point start, Point end, int height)
            : this(imageViewer, comment, start, end, height, imageViewer.PageNumber, null, 
                imageViewer.DefaultHighlightColor)
        {

        }

        /// <summary>
        /// Initializes a new <see cref="Highlight"/> class on the specified page that 
        /// contains the default text and is the default color.
        /// </summary>
        /// <remarks>
        /// <para>The default text is the empty <see cref="string"/> if 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.ImageViewer.RecognizeHighlightText"/> 
        /// property is <see langword="false"/> or recognized text if the 
        /// <see cref="Forms.ImageViewer.RecognizeHighlightText"/> property is 
        /// <see langword="true"/>.
        /// </para>
        /// <para>The default <see cref="System.Drawing.Color"/> is the value of the 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.ImageViewer.DefaultHighlightColor"/> 
        /// property.</para>
        /// </remarks>
        /// <param name="imageViewer">Image viewer on which this highlight appears. Cannot be
        /// <see langword="null"/>. An image must be open on the viewer.</param>
        /// <param name="start">Midpoint of the one side of the highlight in logical (image) 
        /// coordinates.</param>
        /// <param name="comment">The method by which the <see cref="Highlight"/> was created.
        /// </param>
        /// <param name="end">Midpoint of the opposing side of the highlight in logical (image) 
        /// coordinates.</param>
        /// <param name="height">Distance between opposing sides of highlight in logical (image) 
        /// pixels, measured perpendicular to the line segment defined by <paramref name="start"/>
        /// and <paramref name="end"/>.</param>
        /// <param name="pageNumber">One-based page number on which the highlight appears.</param>
        /// <exception cref="ExtractException"><paramref name="imageViewer"/> is 
        /// <see langword="null"/>.</exception>
        /// <exception cref="ExtractException"><paramref name="imageViewer"/> does not contain an 
        /// open image.</exception>
        /// <seealso cref="Forms.ImageViewer.RecognizeHighlightText"/>
        /// <seealso cref="Forms.ImageViewer.DefaultHighlightColor"/>
        public Highlight(ImageViewer imageViewer, string comment, Point start, Point end, 
            int height, int pageNumber)
            : this(imageViewer, comment, start, end, height, pageNumber, null, 
                imageViewer.DefaultHighlightColor)
        {

        }

        /// <summary>
        /// Initializes a new <see cref="Highlight"/> class on the specified page that contains 
        /// the specified text and is the specified color.
        /// </summary>
        /// <remarks>
        /// <para>The default text is the empty <see cref="string"/> if 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.ImageViewer.RecognizeHighlightText"/> 
        /// property is <see langword="false"/> or recognized text if the 
        /// <see cref="Forms.ImageViewer.RecognizeHighlightText"/> property is 
        /// <see langword="true"/>.
        /// </para>
        /// <para>The default <see cref="System.Drawing.Color"/> is the value of the 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.ImageViewer.DefaultHighlightColor"/> 
        /// property.</para>
        /// </remarks>
        /// <param name="imageViewer">Image viewer on which this highlight appears. Cannot be
        /// <see langword="null"/>. An image must be open on the viewer.</param>
        /// <param name="comment">The method by which the <see cref="Highlight"/> was created.
        /// </param>
        /// <param name="start">Midpoint of the one side of the highlight in logical (image) 
        /// coordinates.</param>
        /// <param name="end">Midpoint of the opposing side of the highlight in logical (image) 
        /// coordinates.</param>
        /// <param name="height">Distance between opposing sides of highlight in logical (image) 
        /// pixels, measured perpendicular to the line segment defined by <paramref name="start"/>
        /// and <paramref name="end"/>.</param>
        /// <param name="pageNumber">One-based page number on which the highlight appears.</param>
        /// <param name="text">Text associated with the highlight. If <see langword="null"/>, the 
        /// <paramref name="imageViewer"/>'s default highlight text.</param>
        /// <param name="color"><see cref="System.Drawing.Color"/> of the highlight.</param>
        /// <exception cref="ExtractException"><paramref name="imageViewer"/> is 
        /// <see langword="null"/>.</exception>
        /// <exception cref="ExtractException"><paramref name="imageViewer"/> does not contain an 
        /// open image.</exception>
        /// <seealso cref="Forms.ImageViewer.RecognizeHighlightText"/>
        public Highlight(ImageViewer imageViewer, string comment, Point start, Point end, 
            int height, int pageNumber, string text, Color color)
            : base(imageViewer, pageNumber, comment)
        {
            try
            {
                // Store the parameters
                _startPoint = start;
                _endPoint = end;
                _height = height;
                _color = color;

                // OCR the area if that was requested, otherwise set the text specified
                // TODO: actually perform OCR
                _text = (text == null && imageViewer.RecognizeHighlightText ? "OCR text" : text);
                
                // [DataEntry:3860]
                // Ensure the newly created highlight is of minimum size.
                Rectangle bounds = GetBounds();
                if (bounds.Width < MinSize.Width)
                {
                    _endPoint.Offset(1, 0);
                }
                if (bounds.Height < MinSize.Height)
                {
                    _height = MinSize.Height;
                }

                // Calculate the region
                CalculateRegion();
            }
            catch (Exception e)
            {
                ExtractException ee = new ExtractException("ELI21125",
                    "Unable to create highlight.", e);
                ee.AddDebugData("Image viewer", imageViewer, false);
                ee.AddDebugData("Start point", start, false);
                ee.AddDebugData("End point", end, false);
                ee.AddDebugData("Height", height, false);
                ee.AddDebugData("Page number", pageNumber, false);
                ee.AddDebugData("Text", text, false);
                ee.AddDebugData("Color", color, false);
                throw ee;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="Highlight"/> class from the specifed 
        /// <see cref="RasterZone"/> that contains the default text and is the default color.
        /// </summary>
        /// <remarks>
        /// <para>The default text is the empty <see cref="string"/> if 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.ImageViewer.RecognizeHighlightText"/> 
        /// property is <see langword="false"/> or recognized text if the 
        /// <see cref="Forms.ImageViewer.RecognizeHighlightText"/> property is 
        /// <see langword="true"/>.</para>
        /// <para>The default <see cref="System.Drawing.Color"/> is the value of the 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.ImageViewer.DefaultHighlightColor"/> 
        /// property.</para>
        /// </remarks>
        /// <param name="imageViewer">Image viewer on which this highlight appears. Cannot be
        /// <see langword="null"/>. An image must be open on the viewer.</param>
        /// <param name="comment">The method by which the <see cref="Highlight"/> was created.
        /// </param>
        /// <param name="rasterZone">Raster zone from which to initialize the highlight. Cannot be
        /// <see langword="null"/>.</param>
        /// <exception cref="ExtractException"><paramref name="imageViewer"/> is 
        /// <see langword="null"/>.</exception>
        /// <exception cref="ExtractException"><paramref name="imageViewer"/> does not contain an 
        /// open image.</exception>
        /// <seealso cref="Forms.ImageViewer.RecognizeHighlightText"/>
        /// <seealso cref="Forms.ImageViewer.DefaultHighlightColor"/>
        public Highlight(ImageViewer imageViewer, string comment, RasterZone rasterZone)
            : this(imageViewer, comment, new Point(rasterZone.StartX, rasterZone.StartY), 
                new Point(rasterZone.EndX, rasterZone.EndY), rasterZone.Height, rasterZone.PageNumber,
                null, imageViewer.DefaultHighlightColor)
        {
            
        }

        /// <summary>
        /// Initializes a new <see cref="Highlight"/> class from the <see cref="RasterZone"/> that 
        /// contains the specified text and is the specified color.
        /// </summary>
        /// <remarks>
        /// <para>The default text is the empty <see cref="string"/> if 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.ImageViewer.RecognizeHighlightText"/> 
        /// property is <see langword="false"/> or recognized text if the 
        /// <see cref="Forms.ImageViewer.RecognizeHighlightText"/> property is 
        /// <see langword="true"/>.
        /// </para>
        /// <para>The default <see cref="System.Drawing.Color"/> is the value of the 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.ImageViewer.DefaultHighlightColor"/> 
        /// property.</para>
        /// </remarks>
        /// <param name="imageViewer">Image viewer on which this highlight appears. Cannot be
        /// <see langword="null"/>. An image must be open on the viewer.</param>
        /// <param name="rasterZone">Raster zone from which to initialize the highlight. Cannot be
        /// <see langword="null"/>.</param>
        /// <param name="comment">The method by which the <see cref="Highlight"/> was created.
        /// </param>
        /// <param name="text">Text associated with the highlight. If <see langword="null"/>, the 
        /// <paramref name="imageViewer"/>'s default highlight text.</param>
        /// <param name="color"><see cref="System.Drawing.Color"/> of the highlight.</param>
        /// <exception cref="ExtractException"><paramref name="imageViewer"/> is 
        /// <see langword="null"/>.</exception>
        /// <exception cref="ExtractException"><paramref name="imageViewer"/> does not contain an 
        /// open image.</exception>
        /// <seealso cref="Forms.ImageViewer.RecognizeHighlightText"/>
        public Highlight(ImageViewer imageViewer, string comment, RasterZone rasterZone, 
            string text, Color color)
            : this(imageViewer, comment, new Point(rasterZone.StartX, rasterZone.StartY),
                new Point(rasterZone.EndX, rasterZone.EndY), rasterZone.Height, rasterZone.PageNumber,
                text, color)
        {

        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the midpoint of the one side of the highlight in logical (image) 
        /// coordinates.
        /// </summary>
        /// <value>The midpoint of the one side of the highlight in logical (image) 
        /// coordinates.</value>
        /// <returns>The midpoint of the one side of the highlight in logical (image) 
        /// coordinates.</returns>
        /// <remarks><see cref="EndPoint"/> is the midpoint of the opposing side.</remarks>
        public Point StartPoint
        {
            get
            {
                return _startPoint;
            }
            set
            {
                try
                {
                    _startPoint = value;

                    // Check if this highlight is attached to an image viewer
                    if (base.ImageViewer != null)
                    {
                        // Recalculate its region
                        CalculateRegion();
                    }

                    Dirty = true;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26534", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the midpoint of the one side of the highlight in logical (image) 
        /// coordinates.
        /// </summary>
        /// <remarks><see cref="StartPoint"/> is the midpoint of the opposing side.</remarks>
        /// <value>The midpoint of the one side of the highlight in logical (image) 
        /// coordinates.</value>
        /// <returns>The midpoint of the one side of the highlight in logical (image) 
        /// coordinates.</returns>
        // This is not the compound word "endpoint". This is the "end point", meant in contrast to
        // "start point".
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", 
            MessageId = "EndPoint")]
        public Point EndPoint
        {
            get
            {
                return _endPoint;
            }   
            set
            {
                try
                {
                    _endPoint = value;

                    // Check if this highlight is attached to an image viewer
                    if (base.ImageViewer != null)
                    {
                        // Recalculate its region
                        CalculateRegion();
                    }

                    Dirty = true;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26535", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the distance in logical (image) pixels between the sides of the 
        /// highlight.
        /// </summary>
        /// <value>The distance in logical (image) pixels between the sides of the 
        /// highlight.</value>
        /// <returns>The distance in logical (image) pixels between the sides of the 
        /// highlight.</returns>
        /// <remarks>The height is measured perpendicular to the line segment formed by the
        /// <see cref="StartPoint"/> and <see cref="EndPoint"/>.
        /// </remarks>
        public int Height
        {
            get
            {
                return _height;
            }
            set
            {
                try
                {
                    _height = value;

                    // Check if this highlight is attached to an image viewer
                    if (base.ImageViewer != null)
                    {
                        // Recalculate its region
                        CalculateRegion();
                    }

                    Dirty = true;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26536", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Drawing.Color"/> of the highlight.
        /// </summary>
        /// <value>The <see cref="System.Drawing.Color"/> of the highlight.</value>
        /// <returns>The <see cref="System.Drawing.Color"/> of the highlight. The default value is 
        /// the <see cref="Forms.ImageViewer.DefaultHighlightColor"/> value of its 
        /// <see cref="ImageViewer"/>.</returns>
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                SetColor(value, true);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Drawing.Color"/> of the highlight's outline.
        /// <see landword="null"> if the highlight should not have a outline.</see>
        /// </summary>
        /// <value>The <see cref="System.Drawing.Color"/> of the highlight's outline.</value>
        /// <returns>The <see cref="System.Drawing.Color"/> of the highlight's outline.</returns>
        public Color? OutlineColor
        {
            get
            {
                return _outlineColor;
            }
            set
            {
                _outlineColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the color of the highlight's border.
        /// </summary>
        /// <value>The color of the highlight's border; <see langword="null"/> if no value should 
        /// be used.</value>
        public Color? BorderColor
        {
            get
            {
                return _borderColor;
            }
            set
            {
                _borderColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the text associated with the highlight.
        /// </summary>
        /// <value>The text associated with the highlight. Cannot be <see langword="null"/>
        /// </value>
        /// <returns>The text associated with the highlight.</returns>
        /// <remarks> The default text is the empty <see cref="string"/> if 
        /// <see cref="Forms.ImageViewer.RecognizeHighlightText"/> 
        /// property is <see langword="false"/> or recognized text if the 
        /// <see cref="Forms.ImageViewer.RecognizeHighlightText"/> property is 
        /// <see langword="true"/>.
        /// </remarks>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;

                Dirty = true;
            }
        }

        /// <summary>
        /// Gets the region associated with the highlight in logical (image) coordinates.
        /// </summary>
        /// <returns>The region associated with the highlight in logical (image) coordinates.
        /// </returns>
        /// <remarks>This value is recalculated each time the highlight's spatial information is 
        /// modified. For instance, when the <see cref="StartPoint"/>, <see cref="EndPoint"/>, and
        /// <see cref="Height"/> properties are set.</remarks>
        /// <seealso cref="CalculateRegion"/>
        internal Region Region
        {
            get
            {
                return _region;
            }
        }

        /// <summary>
        /// Gets the top left <see cref="Point"/> for this <see cref="Highlight"/>.
        /// </summary>
        /// <returns>The top left <see cref="Point"/> for this <see cref="Highlight"/></returns>
        public override Point Location
        {
            get
            {
                return GetBounds().Location;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Paints the highlight within the specified region using the specified 
        /// <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The graphics object on which to paint in logical (image) 
        /// coordinates.</param>
        /// <param name="clip">The area within which the <see cref="Highlight"/> should be clipped 
        /// in destination coordinates.</param>
        /// <param name="transform">A 3x3 affine matrix that maps logical (image) coordinates to 
        /// destination coordinates.</param>
        public override void Paint(Graphics graphics, Region clip, Matrix transform)
        {
            Paint(graphics, clip, transform, Color);
        }

        /// <summary>
        /// Paints the highlight within the specified region using the specified 
        /// <see cref="Graphics"/> object and the specified <see cref="Color"/>.
        /// </summary>
        /// <param name="graphics">The graphics object on which to paint in logical (image) 
        /// coordinates.</param>
        /// <param name="clip">The area within which the <see cref="Highlight"/> should be clipped 
        /// in destination coordinates.</param>
        /// <param name="transform">A 3x3 affine matrix that maps logical (image) coordinates to 
        /// destination coordinates.</param>
        /// <param name="color">The color to paint the highlight.</param>
        public void Paint(Graphics graphics, Region clip, Matrix transform, Color color)
        {
            try
            {
                DrawRegion(graphics, clip, transform, color, RasterDrawMode.MaskPen);

                // This is done outside of DrawRegion so it is not rendered in a printed document
                if (BorderColor != null)
                {
                    PointF start;
                    PointF end;
                    float height;
                    GetBorderZone(out start, out end, out height);

                    PointF[] vertices = GeometryMethods.GetVertices(start, end, height);

                    transform.TransformPoints(vertices);

                    // Draw the border
                    GdiPen pen = ExtractPens.GetThickGdiPen(BorderColor.Value);
                    GdiGraphics gdiGraphics = new GdiGraphics(graphics, RasterDrawMode.MaskPen);
                    gdiGraphics.DrawPolygon(pen, vertices);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22393", ex);
            }
        }

        /// <summary>
        /// Paints the highlight within the specified region using the specified 
        /// <see cref="Graphics"/> object and the specified <see cref="Color"/>.
        /// </summary>
        /// <param name="graphics">The graphics object on which to paint in logical (image) 
        /// coordinates.</param>
        /// <param name="clip">The area within which the <see cref="Highlight"/> should be clipped 
        /// in destination coordinates.</param>
        /// <param name="transform">A 3x3 affine matrix that maps logical (image) coordinates to 
        /// destination coordinates.</param>
        /// <param name="color">The color to paint the highlight.</param>
        /// <param name="drawMode">The mix mode to use when drawing the highlight.</param>
        internal void DrawRegion(Graphics graphics, Region clip, Matrix transform, Color color, 
            RasterDrawMode drawMode)
        {
            // Check if this highlight is attached to an image viewer
            ImageViewer imageViewer = base.ImageViewer;
            if (imageViewer != null)
            {
                // Paint the highlight's region
                if (_color != Color.Transparent)
                {
                    GdiGraphics gdiGraphics = new GdiGraphics(graphics, drawMode);
                    using (Region region = Region.Clone())
                    {
                        // Transform region coordinates from logical to destination
                        region.Transform(transform);

                        // Clip the region
                        region.Intersect(clip);

                        // Draw the highlight
                        gdiGraphics.FillRegion(region, color);
                    }
                }

                // If OutlineColor is non-null, draw a outline using the specified color.
                if (OutlineColor != null)
                {
                    // Get the vertices of the highlight in logical (image) coordinates
                    PointF[] vertices = GetVertices();

                    // Get the center of the highlight in logical (image) coordinates
                    PointF center = GetCenterPoint();

                    // Inflate each corner by 6 client pixels (expressed in image pixels)
                    double expandBy = 6.0 / ImageViewer.GetScaleFactorY();
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        // Get the angle from the center to this vertex
                        double angle = GeometryMethods.GetAngle(center, vertices[i]);

                        // Get the difference in angle from the angle of the highlight as
                        // a whole and the angle to this vertex
                        double anglediff = (angle - _angle);
                        if (anglediff < 0)
                        {
                            anglediff += (Math.PI * 2.0);
                        }

                        // Expand this vertex to in the direction of the nearest diagonal angle
                        // (45, 135, 225 or 315 degrees)
                        double angleExpansion = (int)(2.0 * anglediff / Math.PI);
                        angleExpansion *= Math.PI / 2.0;

                        // Make this angle relative to angle of the highlight
                        angleExpansion += (Math.PI / 4.0) + _angle;

                        // Use the angle of the expansion to expand this vertex
                        vertices[i].X += (float)(expandBy * Math.Cos(angleExpansion));
                        vertices[i].Y += (float)(expandBy * Math.Sin(angleExpansion));
                    }

                    // Draw the outline
                    ImageViewer.Transform.TransformPoints(vertices);
                    graphics.DrawPolygon(
                        ExtractPens.GetThickDashedPen(_outlineColor.Value), vertices);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified point is contained by the highlight.
        /// </summary>
        /// <param name="point">The point to test for containment in logical (image) coordinates.
        /// </param>
        /// <returns><see langword="true"/> if the point is contained by the highlight; 
        /// <see langword="false"/> if the point is not contained.</returns>
        public override bool HitTest(Point point)
        {
            try
            {
                // Ensure the highlight is on the active page            
                ImageViewer imageViewer = base.ImageViewer;
                return imageViewer != null && imageViewer.PageNumber == PageNumber &&
                       _region.IsVisible(point);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22394", ex);
            }
        }

        /// <summary>
        /// Retrieves the cursor when the mouse is over a grip handle.
        /// </summary>
        /// <returns>The cursor when the mouse is over a grip handle.</returns>
        public override Cursor GetGripCursor(int gripHandleId)
        {
            try
            {
                // Ensure the grip handle id is valid
                PointF[] gripPoints = GetGripPoints();
                if (gripHandleId < 0 || gripHandleId >= gripPoints.Length)
                {
                    throw new ExtractException("ELI22027", "Invalid grip handle id.");
                }

                // Check if the control key is pressed and rotational grip handled is selected
                if (Control.ModifierKeys == Keys.Control && gripHandleId < 4)
                {
                    return ExtractCursors.Rotate;
                }

                // Get the grip handles in client coordinates
                base.ImageViewer.Transform.TransformPoints(gripPoints);


                // Return the appropriate resize cursor
                bool cornerResize = gripHandleId >= 4;
                return GetResizeCursor(gripPoints[gripHandleId], cornerResize);
            }
            catch
            {
                ExtractException ee = new ExtractException("ELI22028", 
                    "Unable to get grip handle cursor.");
                ee.AddDebugData("Grip handle id", gripHandleId, false);
                throw ee;
            }
        }

        /// <summary>
        /// Determines the direction of the resize cursor based on the mouse position and the 
        /// type of resize event.
        /// </summary>
        /// <param name="point">The position of the mouse.</param>
        /// <param name="cornerResize"><see langword="true"/> if it is a corner resize; 
        /// <see langword="false"/> if it is a side resize.</param>
        /// <returns>The resize cursor to use based on the mouse <paramref name="point"/> and 
        /// whether it is a <paramref name="cornerResize"/>.</returns>
        Cursor GetResizeCursor(PointF point, bool cornerResize)
        {
            // Get the center of the highlight in client coordinates
            PointF[] center = new PointF[] 
            {
                GetCenterPoint()
            };
            base.ImageViewer.Transform.TransformPoints(center);

            // Check if this is a rectangular highlight's vertex
            if (cornerResize)
            {
                // Return the cursor based on whether the vertex 
                // is to the top-left/bottom-right of the center.
                return point.X < center[0].X ^ 
                       point.Y < center[0].Y
                           ? Cursors.SizeNESW : Cursors.SizeNWSE;
            }

            // Get the angle in degrees between the highlight's 
            // center and the center of the selected grip handle.
            double angle = GeometryMethods.GetAngle(center[0], point)
                           * 180.0 / Math.PI;

            // Express the angle as a positive number greater than 22.5
            // NOTE: This is done so that when the number is divided by 45,
            // it will round to a number between one and eight.
            if (angle < 22.5)
            {
                angle += 360;
            }

            // Set the cursor based on the nearest 45 degree angle
            // NOTE: The degree measured is expressed in a Cartesian
            // coordinate system, not the top-left client coordinate
            // system. For this reason the result is mirrored on the 
            // y-axis, and the first & third quadrants become the
            // second & fourth quadrants respectively and vice versa.
            switch ((int)Math.Round(angle / 45.0))
            {
                    // Closest to 45 or 225 degrees
                case 1:
                case 5:

                    // Second and fourth quadrant
                    return Cursors.SizeNWSE;

                    // Closest to 90 or 270 degrees
                case 2:
                case 6:

                    return Cursors.SizeNS;

                    // Closest to 135 or 315 degrees
                case 3:
                case 7:

                    // First and third quadrant
                    return Cursors.SizeNESW;

                    // Closest to 0 or 180 degrees
                case 4:
                case 8:

                    return Cursors.SizeWE;

                default:

                    // This is a non-serious logic error. Return the default cursor.
                    return Cursors.Default;
            }
        }

        /// <summary>
        /// Translates the highlight by the specified point.
        /// </summary>
        /// <param name="offsetBy">The point by which to translate the highlight in logical 
        /// (image) coordinates.</param>
        /// <param name="raiseEvents"><see langword="true"/> if the 
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event should be raised; 
        /// <see langword="false"/> if no events should be raised.</param>
        public override void Offset(Point offsetBy, bool raiseEvents)
        {
            try
            {
                _startPoint.Offset(offsetBy);
                _endPoint.Offset(offsetBy);

                if (base.ImageViewer != null)
                {
                    CalculateRegion();

                    if (raiseEvents)
                    {
                        Dirty = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22395", ex);
            }
        }

        /// <summary>
        /// Begins a grip handle tracking event.
        /// </summary>
        /// <param name="mouseX">The physical (client) x-coordinate of the mouse cursor.</param>
        /// <param name="mouseY">The physical (client) y-coordinate of the mouse cursor.</param>
        /// <param name="gripHandleId">The id of the grip handle to track.</param>
        public override void StartTrackingGripHandle(int mouseX, int mouseY, int gripHandleId)
        {
            try
            {
                // Determine whether the highlight is being rotated
                bool isRotation = Control.ModifierKeys == Keys.Control;

                // Get the grip handles in image and client coordinates
                PointF[] imageGrips = GetGripPoints();
                PointF[] clientGrips = (PointF[])imageGrips.Clone();
                base.ImageViewer.Transform.TransformPoints(clientGrips);

                // Store the point of reference for the tracking event
                PointF trackingPoint = new PointF(mouseX, mouseY);

                // Check if this is a corner resize
                if (gripHandleId >= 4)
                {
                    // Restructure the highlight so that the left or right 
                    // center grip handle (from the client perspective) is
                    // the end point of the highlight.
                    // Choose the center grip handle with the smallest X value difference
                    // with the selected corner grip handle to be the new end point.
                    // [FlexIDSCore #4184]
                    float minimumDiff = float.MaxValue;
                    int index = -1;
                    float x = clientGrips[gripHandleId].X;
                    for (int j = 0; j < 4; j++)
                    {
                        float diff = Math.Abs(x - clientGrips[j].X);
                        if (diff < minimumDiff)
                        {
                            minimumDiff = diff;
                            index = j;
                        }
                    }

                    // Ensure an index has been selected
                    if (index == -1)
                    {
                        ExtractException.ThrowLogicException("ELI29908");
                    }

                    MakeGripHandleEndPoint(imageGrips, index);

                    // Get the center point in image coordinates
                    PointF center = GetCenterPoint();

                    // Set the tracking point to the opposing corner
                    trackingPoint = new PointF(
                        center.X * 2F - imageGrips[gripHandleId].X,
                        center.Y * 2F - imageGrips[gripHandleId].Y);
                }
                else
                {
                    // Set the cross cursor if necessary
                    base.ImageViewer.Cursor =
                        isRotation
                            ? ExtractCursors.ActiveRotate
                            : base.ImageViewer.GetSelectionCursor(mouseX, mouseY);

                    // Restructure the highlight so that the selected 
                    // grip handle is the end point of the highlight.
                    MakeGripHandleEndPoint(imageGrips, gripHandleId);
                }

                // Preserve the angularity of the highlight unless it is being rotated
                _originalIsAngular = isRotation || imageGrips.Length <= 4;

                // Ensure that rectangular highlights are truly rectangular
                // [DotNetRCAndUtils #91]
                if (!_originalIsAngular)
                {
                    int deltaX = _startPoint.X - _endPoint.X;
                    int deltaY = _startPoint.Y - _endPoint.Y;

                    if (deltaX != 0 && deltaY != 0)
                    {
                        if (Math.Abs(deltaX) < Math.Abs(deltaY))
                        {
                            QuietSetStartPoint(_startPoint - new Size(deltaX, 0));
                        }
                        else
                        {
                            QuietSetStartPoint(_startPoint - new Size(0, deltaY));
                        }
                    }
                }

                // Store the original spatial data
                Store();

                // Store the active highlight vector
                UpdateHighlightVector();

                // Get the clipping area in client coordinates.
                // [DotNetRCAndUtils #92]
                Rectangle clip = Rectangle.Union(base.ImageViewer.PhysicalViewRectangle,
                    base.ImageViewer.ClientRectangle);

                // Start the tracking event
                TrackingData = new TrackingData(base.ImageViewer,
                    trackingPoint.X, trackingPoint.Y,
                    base.ImageViewer.GetTransformedRectangle(clip, true));

                base.ImageViewer.Invalidate();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22396", ex);
            }
        }

        /// <summary>
        /// Stores the spatial data associated with the highlight at the start of an interactive 
        /// highlight event.
        /// </summary>
        public override void Store()
        {
            try
            {
                // Store the original spatial data
                _originalLine = new Point[] { _startPoint, _endPoint };
                _originalHeight = _height;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22397", ex);
            }
        }

        /// <summary>
        /// Restores the spatial data associated with the highlight to its state at the start of 
        /// the interactive highlight event.
        /// </summary>
        public override void Restore()
        {
            try
            {
                // Restore the original highlight position and dimensions
                QuietSetSpatialData(_originalLine[0], _originalLine[1], _originalHeight);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22398", ex);
            }
        }

        /// <summary>
        /// Recomputes <see cref="_activeHighlightVector"/>, the unit vector describing the angle 
        /// of the original highlight.
        /// </summary>
        void UpdateHighlightVector()
        {
            // Get the vector components of the current highlight
            float x = _endPoint.X - _startPoint.X;
            float y = _endPoint.Y - _startPoint.Y;

            // Convert the vector to a unit vector and store it
            float magnitude = (float)Math.Sqrt(x * x + y * y);
            _activeHighlightVector = new PointF(x / magnitude, y / magnitude);
        }

        /// <summary>
        /// Updates an interactive select highlight event using the mouse position specified.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        public override void UpdateTracking(int mouseX, int mouseY)
        {
            try
            {
                base.UpdateTracking(mouseX, mouseY);

                ImageViewer imageViewer = base.ImageViewer;

                // Get the mouse position as a point in image coordinates
                Point[] mouse = new Point[] { new Point(mouseX, mouseY) };
                using (Matrix clientToImage = imageViewer.Transform.Clone())
                {
                    clientToImage.Invert();
                    clientToImage.TransformPoints(mouse);
                }

                // Check if the adjust angle tool is being used
                if (imageViewer.Cursor == ExtractCursors.ActiveRotate)
                {
                    // Update the end point
                    QuietSetEndPoint(mouse[0]);

                    // Check if the control key is still active
                    if (Control.ModifierKeys != Keys.Control)
                    {
                        // Reset the highlight cursor
                        imageViewer.Cursor = GetResizeCursor(_endPoint, false);

                        // Don't preserve the angularity of the original highlight
                        _originalIsAngular = true;

                        // Update the highlight vector
                        UpdateHighlightVector();
                    }
                }
                else if (!_originalIsAngular &&
                         (imageViewer.Cursor == Cursors.SizeNWSE
                          || imageViewer.Cursor == Cursors.SizeNESW))
                {
                    // This is a corner resize event
                    TrackingData.UpdateRectangle(mouse[0].X, mouse[0].Y);

                    // Get the y-coordinate for the raster points
                    int y = (int)(TrackingData.Rectangle.Y
                                  + TrackingData.Rectangle.Height / 2.0 + 0.5);

                    // Set the spatial data for the highlight
                    QuietSetSpatialData(new Point(TrackingData.Rectangle.X, y),
                        new Point(TrackingData.Rectangle.Right, y),
                        TrackingData.Rectangle.Height);
                }
                else if (imageViewer.Cursor != Cursors.SizeAll)
                {
                    // Resize the highlight
                    ResizeActiveHighlight(mouse[0]);

                    // Check if the adjust highlight tool has been activated
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        imageViewer.Cursor = ExtractCursors.ActiveRotate;
                    }
                }

                imageViewer.Invalidate();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22399", ex);
            }
        }

        /// <summary>
        /// Resizes the active highlight using the specified mouse position.
        /// </summary>
        /// <param name="mouse">The position in logical (image) coordinates.</param>
        /// <remarks>The highlight maintains its start point, height, and angle. Only the end 
        /// point of the highlight is changed.</remarks>
        void ResizeActiveHighlight(Point mouse)
        {
            // Construct the vector components for a vector from the start point to the mouse
            int x = mouse.X - _startPoint.X;
            int y = mouse.Y - _startPoint.Y;

            // Compute the dot product of the vectors
            // NOTE: Because the active highlight vector is a unit vector, this is equivalent 
            // to the the scalar projection of the mouse vector onto the active highlight vector.
            double dotProduct = x * _activeHighlightVector.X + y * _activeHighlightVector.Y;

            // Compute the new point
            QuietSetEndPoint(new Point(
                (int)(_startPoint.X + _activeHighlightVector.X * dotProduct + 0.5),
                (int)(_startPoint.Y + _activeHighlightVector.Y * dotProduct + 0.5)));
        }

        /// <summary>
        /// Restructures the specified highlight so that the specified grip handle corresponds to
        /// its <see cref="Highlight.EndPoint"/>.
        /// </summary>
        /// <param name="gripHandles">The four midpoints of the sides of the 
        /// <see cref="Highlight"/>.</param>
        /// <param name="gripHandleId">The index of the point in <paramref name="gripHandles"/> 
        /// that should be the new end point.</param>
        void MakeGripHandleEndPoint(PointF[] gripHandles, int gripHandleId)
        {
            PointF start;
            PointF end;
            float height;
            GetSelectionZone(out start, out end, out height);

            // If this grip handle is already the end point, we are done.
            if (gripHandles[gripHandleId] == end)
            {
                return;
            }

            // Check if this grip handle is the start point
            if (gripHandles[gripHandleId] == start)
            {
                // Swap the start point and end point
                QuietSetSpatialData(_endPoint, _startPoint, _height);

                // Done.
                return;
            }

            // This is an endpoint of the height.
            // Find the other endpoint.
            for (int i = 0; i < gripHandles.Length; i++)
            {
                if (gripHandleId != i && gripHandles[i] != start && gripHandles[i] != end)
                {
                    // Define the new highlight
                    PointF[] points = GetGripPoints(_startPoint, _endPoint, _height);
                    QuietSetSpatialData(Point.Round(points[i]), Point.Round(points[gripHandleId]),
                        (int) GeometryMethods.Distance(_startPoint, _endPoint));

                    // Done.
                    return;
                }
            }
        }

        /// <summary>
        /// Sets the <see cref="StartPoint"/> without raising any events.
        /// </summary>
        /// <param name="startPoint">The midpoint of one side of the highlight in logical (image) 
        /// coordinates.</param>
        internal void QuietSetStartPoint(Point startPoint)
        {
            _startPoint = startPoint;

            if (base.ImageViewer != null)
            {
                CalculateRegion();
            }
        }

        /// <summary>
        /// Sets the <see cref="EndPoint"/> without raising any events.
        /// </summary>
        /// <param name="endPoint">The midpoint of an opposing side of the highlight in logical 
        /// (image) coordinates.</param>
        // This is not the compound word "endpoint". This is the "end point", meant in contrast to
        // "start point".
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId = "endPoint")]
        internal void QuietSetEndPoint(Point endPoint)
        {
            _endPoint = endPoint;

            if (base.ImageViewer != null)
            {
                CalculateRegion();
            }
        }

        /// <summary>
        /// Sets all the spatial properties of the highlight without raising any events.
        /// </summary>
        /// <param name="startPoint">The midpoint of one side of the highlight in logical (image) 
        /// coordinates.</param>
        /// <param name="endPoint">The midpoint of the opposing side of the highlight in logical 
        /// (image) coordinates.</param>
        /// <param name="height">The distance between two sides of the highlight measured 
        /// perpendicular to the line formed by <paramref name="startPoint"/> and 
        /// <paramref name="endPoint"/>.</param>
        // This is not the compound word "endpoint". This is the "end point", meant in contrast to
        // "start point".
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId = "endPoint")]
        internal void QuietSetSpatialData(Point startPoint, Point endPoint, int height)
        {
            // Set the spatial data
            _startPoint = startPoint;
            _endPoint = endPoint;
            _height = height;

            // Check if this highlight is attached to an image viewer
            if (base.ImageViewer != null)
            {
                // Recalculate its region
                CalculateRegion();
            }
        }

        /// <summary>
        /// Sets the <see cref="Color"/> without raising any events.
        /// </summary>
        /// <param name="color">The color to set the <see cref="Highlight"/>.</param>
        /// <param name="markAsDirty"><see langword="true"/> to set the dirty flag to 
        /// <see langword="true"/>; <see langword="false"/> to leave the dirty flag unchanged.
        /// </param>
        internal void SetColor(Color color, bool markAsDirty)
        {
            _color = color;

            if (markAsDirty)
            {
                Dirty = true;
            }
        }

        /// <summary>
        /// Sets all the spatial properties of the highlight.
        /// </summary>
        /// <param name="startPoint">The midpoint of one side of the highlight in logical (image) 
        /// coordinates.</param>
        /// <param name="endPoint">The midpoint of the opposing side of the highlight in logical 
        /// (image) coordinates.</param>
        /// <param name="height">The distance between two sides of the highlight measured 
        /// perpendicular to the line formed by <paramref name="startPoint"/> and 
        /// <paramref name="endPoint"/>.</param>
        // This is not the compound word "endpoint". This is the "end point", meant in contrast to
        // "start point".
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", 
            MessageId="endPoint")]
        public void SetSpatialData(Point startPoint, Point endPoint, int height)
        {
            try
            {
                // Set the spatial data
                _startPoint = startPoint;
                _endPoint = endPoint;
                _height = height;

                // Check if this highlight is attached to an image viewer
                if (base.ImageViewer != null)
                {
                    // Recalculate its region
                    CalculateRegion();
                }

                Dirty = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22400", ex);
            }
        }

        /// <summary>
        /// Inflates each side of the <see cref="Highlight"/> by the specified amount.
        /// </summary>
        /// <param name="size">The amount to inflate each side of the <see cref="Highlight"/> in 
        /// logical (image) pixels.</param>
        /// <remarks>Negative values of <paramref name="size"/> deflate the highlight.</remarks>
        public void Inflate(double size)
        {
            try
            {
                // Do nothing if the size is zero
                if (size != 0)
                {
                    // Find the change in x and y
                    int deltaX = _endPoint.X - _startPoint.X;
                    int deltaY = _endPoint.Y - _startPoint.Y;

                    // Handle the special case of a widthless, heightless highlight
                    if (deltaX == 0 && deltaY == 0)
                    {
                        // Seperate the start and end points horizontally
                        _startPoint.Offset((int)(-size + 0.5), 0);
                        _endPoint.Offset((int)(size + 0.5), 0);

                        // Set the new height
                        _height = (int)(size * 2 + 0.5);
                    }
                    else
                    {
                        // Calculate the vector to apply to the start and end points
                        double magnitude = Math.Sqrt(deltaX * deltaX + deltaY * deltaY) * size;
                        Point vector = new Point(
                            (int)(deltaX / magnitude + 0.5), (int)(deltaY / magnitude + 0.5));

                        // Offset the endpoints by the amount specified
                        _startPoint.Offset(-vector.X, -vector.Y);
                        _endPoint.Offset(vector);

                        // Inflate the height
                        _height += (int)(size * 2 + 0.5);
                    }
                }

                // Check if this highlight is attached to an image viewer
                if (base.ImageViewer != null)
                {
                    // Recalculate its region
                    CalculateRegion();
                }

                Dirty = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22401", ex);
            }
        }

        /// <summary>
        /// Copies the information of this highlight into a <see cref="RasterZone"/>.
        /// </summary>
        /// <returns>A <see cref="RasterZone"/> with the spatial information of this highlight.
        /// </returns>
        public RasterZone ToRasterZone()
        {
            try
            {
                // Construct the raster zone   
                RasterZone rasterZone = new RasterZone();
                rasterZone.StartX = _startPoint.X;
                rasterZone.StartY = _startPoint.Y;
                rasterZone.EndX = _endPoint.X;
                rasterZone.EndY = _endPoint.Y;
                rasterZone.Height = _height;
                rasterZone.PageNumber = PageNumber;

                // Return the raster zone
                return rasterZone;
            }
            catch (Exception e)
            {
                ExtractException ee = new ExtractException("ELI21126",
                    "Unable to construct raster zone.", e);
                ee.AddDebugData("Start point", _startPoint, false);
                ee.AddDebugData("End point", _endPoint, false);
                ee.AddDebugData("Height", _height, false);
                ee.AddDebugData("Page number", PageNumber, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the image viewer associated with the <see cref="Highlight"/>.
        /// </summary>
        /// <value>The image viewer associated with the <see cref="Highlight"/>.</value>
        public override ImageViewer ImageViewer
        {
            get
            {
                return base.ImageViewer;
            }
            set
            {
                try
                {
                    // Check if the region should be disposed
                    if (value == null && _region != null)
                    {
                        // The region is no longer needed, now that the highlight 
                        // is no longer associated with an image viewer.
                        _region.Dispose();
                        _region = null;
                    }

                    base.ImageViewer = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26537", ex);
                }
            }
        }

        /// <summary>
        /// Calculates the <see cref="Region"/> of this highlight from the spatial information 
        /// of the highlight.
        /// </summary>
        /// <remarks>
        /// <para>This method should not be called if the highlight is detached from an image 
        /// viewer.</para>
        /// <para>This method is called whenever the highlight's spatial information is 
        /// modified. For instance, when the <see cref="StartPoint"/>, <see cref="EndPoint"/>, and
        /// <see cref="Height"/> properties are set.</para>
        /// </remarks>
        void CalculateRegion()
        {
            // Calculate the angle of the line
            _angle = GeometryMethods.GetAngle(_startPoint, _endPoint);

            _region = GetAngularRegion(_startPoint, _endPoint, _height);
        }

        /// <summary>
        /// Returns the angled rectangular <see cref="Region"/> defined by the line segment 
        /// between two opposing sides and the height.
        /// </summary>
        /// <param name="startPoint">The midpoint of one side of the angled rectangle.</param>
        /// <param name="endPoint">The midpoint of the opposing side of the angled rectangle.
        /// </param>
        /// <param name="height">The distance between two sides measured perpendicular to the line 
        /// segment formed by <paramref name="startPoint"/> and <paramref name="endPoint"/>.
        /// </param>
        /// <returns>The angled rectangular <see cref="Region"/> defined by the line segment 
        /// between two opposing sides and the height.</returns>
        internal static Region GetAngularRegion(Point startPoint, Point endPoint, int height)
        {
            // Get the corners of the specified specified region
            PointF[] vertices = GeometryMethods.GetVertices(startPoint, endPoint, height);

            // Construct and return the region
            return new Region(new GraphicsPath(vertices, _RECTANGULAR_PATH_POINT_TYPE));
        }

        /// <summary>
        /// Gets the smallest rectangle that contains the <see cref="Highlight"/> in
        /// logical image coordinates.
        /// </summary>
        /// <returns>The smallest rectangle that contains the <see cref="Highlight"/>
        /// in logical image coordinates.</returns>
        // FXCop thinks this should be a property but this data is not stored in a field
        // it is recomputed each time the method is called
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public override Rectangle GetBounds()
        {
            try
            {
                return GeometryMethods.GetBoundingRectangle(_startPoint, _endPoint, _height);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22402", ex);
            }
        }

        /// <summary>
        /// Retrieves the center point of the highlight in logical (image) coordinates.
        /// </summary>
        /// <returns>The center point of the highlight in logical (image) coordinates.</returns>
        // This is not a property because it needs to be calculated.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public override PointF GetCenterPoint()
        {
            try
            {
                // Return midpoint of the line segment formed by the start point and end point
                return GeometryMethods.GetCenterPoint(_startPoint, _endPoint);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22403", ex);
            }
        }

        /// <summary>
        /// Retrieves the vertices of the <see cref="Highlight"/> in logical image coordinates.
        /// </summary>
        /// <returns>The vertices of the <see cref="Highlight"/> in logical image coordinates.</returns>
        public override PointF[] GetVertices()
        {
            try
            {
                return GeometryMethods.GetVertices(_startPoint, _endPoint, _height);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22406", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified rectangle intersects the <see cref="Highlight"/>.
        /// </summary>
        /// <param name="rectangle">The rectangle in logical (image) coordinates to check for 
        /// intersection.</param>
        /// <returns><see langword="true"/> if the <paramref name="rectangle"/> intersects the 
        /// <see cref="Highlight"/>; <see langword="false"/> if the <paramref name="rectangle"/> 
        /// does not intersect the <see cref="LayerObject"/>.</returns>
        public override bool IsVisible(Rectangle rectangle)
        {
            try
            {
                // true iff the base class is visible and the
                // region intersects the specified rectangle
                return base.IsVisible(rectangle) && _region != null && _region.IsVisible(rectangle);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22407", ex);
            }
        }

        /// <summary>
        /// Retrieves the center points of grip handles appropriate for the highlight in logical 
        /// (image) coordinates.
        /// </summary>
        /// <returns>The center points of grip handles appropriate for the highlight in logical 
        /// (image) coordinates.</returns>
        /// <remarks>
        /// <para>If the highlight is rectangular there will be eight grip handles, one for 
        /// each side and one for each vertex. If the highlight is angular there will be four grip 
        /// handles, one for each side.</para>
        /// <para>The midpoints of the sides are the first four elements. If the highlight is 
        /// rectangular, the vertices are the last four elements.</para>
        /// </remarks>
        public override PointF[] GetGripPoints()
        {
            try
            {
                // Handle the special case of an empty highlight
                if (_startPoint == _endPoint)
                {
                    return new PointF[] { GetCenterPoint() };
                }

                // Get the midpoints of the sides of the selection border
                PointF start;
                PointF end;
                float height;
                GetSelectionZone(out start, out end, out height);

                return GetGripPoints(start, end, height);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22408", ex);
            }
        }


        /// <summary>
        /// Retrieves the center points of grip handles appropriate for the highlight in logical 
        /// (image) coordinates.
        /// </summary>
        /// <returns>The center points of grip handles appropriate for the highlight in logical 
        /// (image) coordinates.</returns>
        /// <remarks>
        /// <para>If the highlight is rectangular there will be eight grip handles, one for 
        /// each side and one for each vertex. If the highlight is angular there will be four grip 
        /// handles, one for each side.</para>
        /// <para>The midpoints of the sides are the first four elements. If the highlight is 
        /// rectangular, the vertices are the last four elements.</para>
        /// </remarks>
        PointF[] GetGripPoints(PointF start, PointF end, float height)
        {
            // Get the center point of the highlight
            PointF center = GeometryMethods.GetCenterPoint(start, end);

            // Calculate the vertical and horizontal modifiers. These are the values to add and 
            // subtract from the center to determine the "top" and "bottom" of the rectangle.
            double xModifier = height / 2.0 * Math.Sin(_angle);
            double yModifier = height / 2.0 * Math.Cos(_angle);

            // Calculate the grip handles
            PointF[] gripHandles;
            if (Math.Abs(_angle % (Math.PI / 2)) <= 1e-10)
            {
                // This is a rectangular highlight. There should be eight grip handles.
                gripHandles = new PointF[] 
                {
                    start,
                    new PointF((float)(center.X - xModifier), (float)(center.Y + yModifier)),
                    end,
                    new PointF((float)(center.X + xModifier), (float)(center.Y - yModifier)),
                    new PointF((float)(start.X + xModifier), (float)(start.Y - yModifier)),
                    new PointF((float)(start.X - xModifier), (float)(start.Y + yModifier)),
                    new PointF((float)(end.X - xModifier), (float)(end.Y + yModifier)),
                    new PointF((float)(end.X + xModifier), (float)(end.Y - yModifier))
                };
            }
            else
            {
                // This is an angular highlight. There should be four grip handles.
                gripHandles = new PointF[] 
                {
                    start,
                    new PointF((float)(center.X - xModifier), (float)(center.Y + yModifier)),
                    end,
                    new PointF((float)(center.X + xModifier), (float)(center.Y - yModifier))
                };
            }

            // Return the grip handles
            return gripHandles;
        }

        /// <summary>
        /// Retrieves the vertices of the selection border in logical (image) coordinates.
        /// </summary>
        /// <returns>The vertices of the selection border in logical (image) coordinates.</returns>
        public override PointF[] GetGripVertices()
        {
            try
            {
                // Get the center point of the highlight
                PointF center = GetCenterPoint();

                // Handle the special case of an empty highlight
                if (_startPoint == _endPoint)
                {
                    return new PointF[] { center };
                }

                PointF start;
                PointF end;
                float height;
                GetSelectionZone(out start, out end, out height);

                // Calculate the vertical and horizontal modifiers. These are the values to add and 
                // subtract from the center to determine the "top" and "bottom" of the rectangle.
                double xModifier = height / 2.0 * Math.Sin(_angle);
                double yModifier = height / 2.0 * Math.Cos(_angle);

                // Calculate the grip vertices
                PointF[] vertices = new PointF[] 
                {
                    new PointF((float)(start.X + xModifier), (float)(start.Y - yModifier)),
                    new PointF((float)(start.X - xModifier), (float)(start.Y + yModifier)),
                    new PointF((float)(end.X - xModifier), (float)(end.Y + yModifier)),
                    new PointF((float)(end.X + xModifier), (float)(end.Y - yModifier))
                };

                return vertices;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28783", ex);
            }
        }

        /// <summary>
        /// Gets a zone that represents the border of the highlight.
        /// </summary>
        /// <param name="start">The start point of the zone in image pixels.</param>
        /// <param name="end">The end point of the zone in image pixels.</param>
        /// <param name="height">The height of the zone in image pixels.</param>
        void GetBorderZone(out PointF start, out PointF end, out float height)
        {
            double expandBy = (ExtractPens.ThickPenWidth-1) / 2.0 / ImageViewer.GetScaleFactorY();
            GetExpandedZone(expandBy, out start, out end, out height);
        }

        /// <summary>
        /// Gets a zone that represents the selection border of the highlight.
        /// </summary>
        /// <param name="start">The start point of the zone in image pixels.</param>
        /// <param name="end">The end point of the zone in image pixels.</param>
        /// <param name="height">The height of the zone in image pixels.</param>
        void GetSelectionZone(out PointF start, out PointF end, out float height)
        {
            // The selection border is around the regular border [FIDSC #3888]
            double expandBy = SelectionPen.Width/2.0;
            if (BorderColor != Color.Transparent)
            {
                expandBy += ExtractPens.ThickPenWidth - 1;
            }

            // Convert to image coordinates
            expandBy /= ImageViewer.GetScaleFactorY();
            
            GetExpandedZone(expandBy, out start, out end, out height);
        }

        /// <summary>
        /// Expands the points of a zone by the specified amount.
        /// </summary>
        /// <param name="expandBy">The amount to expand the vertices in image pixels.</param>
        /// <param name="start">The start point of the zone in image pixels.</param>
        /// <param name="end">The end point of the zone in image pixels.</param>
        /// <param name="height">The height of the zone in image pixels.</param>
        void GetExpandedZone(double expandBy, out PointF start, out PointF end, out float height)
        {
            // Get the amount to modify the points
            SizeF delta =
                new SizeF((float)(expandBy * Math.Cos(_angle)), (float)(expandBy * Math.Sin(_angle)));

            // Expand the zone by expandBy pixels on all sides
            start = (PointF)(_startPoint) - delta;
            end = (PointF)(_endPoint) + delta;
            height = _height + (float)(expandBy * 2F);
        }

        /// <summary>
        /// Determines whether the <see cref="Highlight"/> is positioned in a valid place.
        /// </summary>
        /// <returns><see langword="true"/> if the layer object is positioned in a valid place; 
        /// <see langword="false"/> if the layer object is not positioned in a valid place.
        /// </returns>
        public override bool IsValid()
        {
            try
            {
                // TODO: This code can be removed when the SpotRecognitionWindow is retired for 
                // the .Net ImageViewer and the SpatialString code that adjusts the endpoints 
                // onto the page is also removed.

                // At least one endpoint must be on the page [FIDSC #3746]
                if (!IsPointOnPage(_startPoint) && !IsPointOnPage(_endPoint))
                {
                    return false;
                }

                return base.IsValid();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29892", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified point is on the page.
        /// </summary>
        /// <param name="point">The point to test for containment.</param>
        /// <returns><see langword="true"/> if the point is on the page; <see langword="false"/> 
        /// if the point is not on the page.</returns>
        bool IsPointOnPage(Point point)
        {
            return point.X >= 0 && point.Y >= 0 && point.X < ImageViewer.ImageWidth &&
                   point.Y < ImageViewer.ImageHeight;
        }

        /// <summary>
        /// Reads the zone data as XML.
        /// </summary>
        /// <param name="reader">The stream from which the zone should be read.</param>
        internal void ReadXmlZone(XmlReader reader)
        {
            if (reader.Name != "Zone")
            {
                throw new ExtractException("ELI22797", "Invalid format.");
            }
            reader.Read();

            // Get the start point
            if (reader.Name != "Start")
            {
                throw new ExtractException("ELI22860", "Invalid format.");
            }
            _startPoint = new Point(Convert.ToInt32(reader.GetAttribute("X"), CultureInfo.CurrentCulture),
                Convert.ToInt32(reader.GetAttribute("Y"), CultureInfo.CurrentCulture));
            reader.Read();

            // Get the end point
            if (reader.Name != "End")
            {
                throw new ExtractException("ELI22861", "Invalid format.");
            }
            _endPoint = new Point(Convert.ToInt32(reader.GetAttribute("X"), CultureInfo.CurrentCulture),
                Convert.ToInt32(reader.GetAttribute("Y"), CultureInfo.CurrentCulture));
            reader.Read();

            // Get the height
            if (reader.Name != "Height")
            {
                throw new ExtractException("ELI22862", "Invalid format.");
            }
            _height = reader.ReadElementContentAsInt();
            reader.Read();

            // Calculate the region
            CalculateRegion();
        }

        /// <summary>
        /// Writes the zone data as XML.
        /// </summary>
        /// <param name="writer">The stream to which the zone should be written.</param>
        internal void WriteXmlZone(XmlWriter writer)
        {
            writer.WriteStartElement("Zone");
            writer.WriteStartElement("Start");
            writer.WriteAttributeString("X", _startPoint.X.ToString(CultureInfo.CurrentCulture));
            writer.WriteAttributeString("Y", _startPoint.Y.ToString(CultureInfo.CurrentCulture));
            writer.WriteEndElement();
            writer.WriteStartElement("End");
            writer.WriteAttributeString("X", _endPoint.X.ToString(CultureInfo.CurrentCulture));
            writer.WriteAttributeString("Y", _endPoint.Y.ToString(CultureInfo.CurrentCulture));
            writer.WriteEndElement();
            writer.WriteElementString("Height", _height.ToString(CultureInfo.CurrentCulture));
            writer.WriteEndElement();
        }

        #endregion Methods

        #region IDisposable Members

        /// <overloads>Releases resources used by the <see cref="Highlight"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="Highlight"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed objects
                if (_region != null)
                {
                    _region.Dispose();
                    _region = null;
                }
            }

            // No unmanaged resources to free

            // Dispose of base class
            base.Dispose(disposing);
        }

        #endregion IDisposable Members

        #region IComparable<Highlight> Members

        /// <summary>
        /// Compares this <see cref="Highlight"/> with another <see cref="Highlight"/>.
        /// </summary>
        /// <param name="other">A <see cref="Highlight"/> to compare with this
        /// <see cref="Highlight"/>.</param>
        /// <returns>An <see cref="int"/> that indicates the relative order of the
        /// <see cref="Highlight"/> objects that are being compared.</returns>
        public int CompareTo(Highlight other)
        {
            // Convert to RasterZone and compare
            return ToRasterZone().CompareTo(other.ToRasterZone());
        }

        /// <summary>
        /// Checks whether the specified <see cref="object"/> is equal to
        /// this <see cref="Highlight"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with.</param>
        /// <returns><see langword="true"/> if the objects are equal and
        /// <see langword="false"/> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // Check if it is a Highlight object
            Highlight highlight = obj as Highlight;
            if (highlight == null)
            {
                return false;
            }

            // Check if they are equal
            return this == highlight;
        }

        /// <summary>
        /// Checks whether the specified <see cref="Highlight"/> is equal to
        /// this <see cref="Highlight"/>.
        /// </summary>
        /// <param name="highlight">The <see cref="Highlight"/> to compare with.</param>
        /// <returns><see langword="true"/> if the zones are equal and
        /// <see langword="false"/> otherwise.</returns>
        public bool Equals(Highlight highlight)
        {
            return this == highlight;
        }

        /// <summary>
        /// Returns a hashcode for this <see cref="Highlight"/>.
        /// </summary>
        /// <returns>The hashcode for this <see cref="Highlight"/>.</returns>
        public override int GetHashCode()
        {
            return ToRasterZone().GetHashCode() ^ Id.GetHashCode() ^ _color.GetHashCode();
        }

        /// <summary>
        /// Checks whether the two specified <see cref="Highlight"/> objects
        /// are equal.
        /// </summary>
        /// <param name="highlight1">A <see cref="Highlight"/> to compare.</param>
        /// <param name="highlight2">A <see cref="Highlight"/> to compare.</param>
        /// <returns><see langword="true"/> if the highlights are equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator ==(Highlight highlight1, Highlight highlight2)
        {
            if (ReferenceEquals(highlight1, highlight2))
            {
                return true;
            }

            if (((object)highlight1 == null) || ((object)highlight2 == null))
            {
                return false;
            }

            // If they are spatially equal, check their color value
            bool equal = highlight1.ToRasterZone() == highlight2.ToRasterZone();
            if (equal)
            {
                equal = highlight1._color == highlight2._color;
            }

            return equal;
        }

        /// <summary>
        /// Checks whether the two specified <see cref="Highlight"/> objects
        /// are not equal.
        /// </summary>
        /// <param name="highlight1">A <see cref="Highlight"/> to compare.</param>
        /// <param name="highlight2">A <see cref="Highlight"/> to compare.</param>
        /// <returns><see langword="true"/> if the highlights are not equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator !=(Highlight highlight1, Highlight highlight2)
        {
            return !(highlight1 == highlight2);
        }

        /// <summary>
        /// Checks whether the first specified <see cref="Highlight"/>
        /// is less than the second specified <see cref="Highlight"/>.
        /// </summary>
        /// <param name="highlight1">A <see cref="Highlight"/> to compare.</param>
        /// <param name="highlight2">A <see cref="Highlight"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="highlight1"/> is less
        /// than <paramref name="highlight2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator <(Highlight highlight1, Highlight highlight2)
        {
            return highlight1.CompareTo(highlight2) < 0;
        }

        /// <summary>
        /// Checks whether the first specified <see cref="Highlight"/>
        /// is greater than the second specified <see cref="Highlight"/>.
        /// </summary>
        /// <param name="highlight1">A <see cref="Highlight"/> to compare.</param>
        /// <param name="highlight2">A <see cref="Highlight"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="highlight1"/> is greater
        /// than <paramref name="highlight2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator >(Highlight highlight1, Highlight highlight2)
        {
            return highlight1.CompareTo(highlight2) > 0;
        }

        #endregion IComparable<Highlight> Members

        #region IXmlSerializable Members

        /// <summary>
        /// Generates a <see cref="Highlight"/> from its XML representation.
        /// </summary>
        /// <param name="reader">The stream from which the <see cref="Highlight"/> is 
        /// deserialized.</param>
        public override void ReadXml(XmlReader reader)
        {
            try
            {
                // Read the base class data
                base.ReadXml(reader);

                // Get the zone
                ReadXmlZone(reader);

                // Get the color
                if (reader.Name != "Color")
                {
                    throw new ExtractException("ELI22863", "Invalid format.");
                }
                _color = Color.FromArgb(
                    Convert.ToInt32(reader.GetAttribute("Red"), CultureInfo.CurrentCulture),
                    Convert.ToInt32(reader.GetAttribute("Green"), CultureInfo.CurrentCulture),
                    Convert.ToInt32(reader.GetAttribute("Blue"), CultureInfo.CurrentCulture));
                reader.Read();

                // Get the text
                if (reader.Name != "Text")
                {
                    throw new ExtractException("ELI22864", "Invalid format.");
                }
                _text = reader.ReadElementContentAsString();
                reader.Read();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22918", ex);
            }
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The stream to which the <see cref="Highlight"/> is serialized.
        /// </param>
        public override void WriteXml(XmlWriter writer)
        {
            try
            {
                // Write the base class xml data
                base.WriteXml(writer);

                // Write the zone
                WriteXmlZone(writer);

                // Write the color
                writer.WriteStartElement("Color");
                writer.WriteAttributeString("Red", ((int)_color.R).ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("Green", ((int)_color.G).ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("Blue", ((int)_color.B).ToString(CultureInfo.CurrentCulture));
                writer.WriteEndElement();

                // Write the text
                writer.WriteElementString("Text", _text);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22919", ex);
            }
        }

        #endregion IXmlSerializable Members
    }
}
