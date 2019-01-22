using Extract.Drawing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;
using UCLID_COMUTILSLib;

using EOrientation = UCLID_RASTERANDOCRMGMTLib.EOrientation;
using SpatialPageInfo = UCLID_RASTERANDOCRMGMTLib.SpatialPageInfo;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;


namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a colored angular rectangle on the <see cref="ImageViewer"/> control.
    /// </summary>
    /// <remarks>
    /// <para>A highlight is 'live' in the sense that changes made to it are instantly reflected 
    /// in the <see cref="ImageViewer"/> control to which it is associated. To delay the update, 
    /// use the <see cref="Forms.IDocumentViewer.BeginUpdate"/> and 
    /// <see cref="Forms.IDocumentViewer.EndUpdate"/> methods of the <see cref="ImageViewer"/>.</para>
    /// <para>A highlight is described spatially by its <see cref="StartPoint"/>, 
    /// <see cref="EndPoint"/>, and <see cref="Height"/> properties. The <see cref="StartPoint"/> 
    /// and <see cref="EndPoint"/> describe the endpoints of a line segment that bisects the 
    /// highlight. The <see cref="Height"/> is the distance between the sides of the highlight, 
    /// measured perpendicular to the bisecting line.</para>
    /// </remarks>
    [CLSCompliant(false)]
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

        /// <summary>
        /// The minimum length in image pixels a highlight can be during an interactive resize.
        /// </summary>
        const int _MINIMUM_LENGTH = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Midpoint of starting side of the highlight in logical (image) coordinates.
        /// <remarks><see cref="_endPoint"/> is the midpoint of the opposing side.</remarks>
        /// </summary>
        /// <seealso cref="StartPoint"/>
        PointF _startPoint;

        /// <summary>
        /// Midpoint of ending side of the highlight in logical (image) coordinates.
        /// <remarks><see cref="_startPoint"/> is the midpoint of the opposing side.</remarks>
        /// </summary>
        /// <seealso cref="EndPoint"/>
        PointF _endPoint;

        /// <summary>
        /// Distance in pixels between the sides of the highlight measured perpendicular to 
        /// the line segment formed by <see cref="_startPoint"/> and <see cref="_endPoint"/>.
        /// </summary>
        /// <seealso cref="Height"/>
        float _height;

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
        /// The original endpoints of the highlight currently being processed during an 
        /// interactive highlight event.
        /// </summary>
        PointF[] _originalLine;

        /// <summary>
        /// The original height of the highlight currently being processed during an interactive
        /// highlight event.
        /// </summary>
        float _originalHeight;

        /// <summary>
        /// Contains a unit vector for each side of a highlight that is being moved. (A corner
        /// resize operation will have two; otherwise there will be only 1).
        /// </summary>
        Dictionary<Side, PointF> _activeTrackingVectors = new Dictionary<Side, PointF>();

        /// <summary>
        /// The angle of the highlight.  Calculated whenever CalculateRegion is called.
        /// </summary>
        double _angle;

        /// <summary>
        /// Indicates whether highlights need to be created such that they are WYSIWYG compatible
        /// with the Spot recognition window.
        /// </summary>
        static bool? _spotIRCompatible;

        /// <summary>
        /// Used to simplify resizing operations and allow for the angle of the highlight to be
        /// maintained as much as possible.
        /// </summary>
        ZoneGeometry _zoneGeometry;

        /// <summary>
        /// Indicates whether the <see cref="EndTracking"/> method is currently executing.
        /// </summary>
        bool _trackingEventEnding;

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
        internal Highlight(string comment, int pageNumber)
            : base(pageNumber, comment)
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
        /// <see cref="Forms.IDocumentViewer.PageNumber"/> property.</para>
        /// <para>The default text is the empty <see cref="string"/> if 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.IDocumentViewer.RecognizeHighlightText"/> 
        /// property is <see langword="false"/> or recognized text if the 
        /// <see cref="Forms.IDocumentViewer.RecognizeHighlightText"/> property is 
        /// <see langword="true"/>.
        /// </para>
        /// <para>The default <see cref="System.Drawing.Color"/> is the value of the 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.IDocumentViewer.DefaultHighlightColor"/> 
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
        /// <seealso cref="Forms.IDocumentViewer.PageNumber"/>
        /// <seealso cref="Forms.IDocumentViewer.RecognizeHighlightText"/>
        /// <seealso cref="Forms.IDocumentViewer.DefaultHighlightColor"/>
        public Highlight(IDocumentViewer imageViewer, string comment, Point start, Point end, int height)
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
        /// <paramref name="imageViewer"/>'s <see cref="Forms.IDocumentViewer.RecognizeHighlightText"/> 
        /// property is <see langword="false"/> or recognized text if the 
        /// <see cref="Forms.IDocumentViewer.RecognizeHighlightText"/> property is 
        /// <see langword="true"/>.
        /// </para>
        /// <para>The default <see cref="System.Drawing.Color"/> is the value of the 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.IDocumentViewer.DefaultHighlightColor"/> 
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
        /// <seealso cref="Forms.IDocumentViewer.RecognizeHighlightText"/>
        /// <seealso cref="Forms.IDocumentViewer.DefaultHighlightColor"/>
        public Highlight(IDocumentViewer imageViewer, string comment, Point start, Point end,
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
        /// <paramref name="imageViewer"/>'s <see cref="Forms.IDocumentViewer.RecognizeHighlightText"/> 
        /// property is <see langword="false"/> or recognized text if the 
        /// <see cref="Forms.IDocumentViewer.RecognizeHighlightText"/> property is 
        /// <see langword="true"/>.
        /// </para>
        /// <para>The default <see cref="System.Drawing.Color"/> is the value of the 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.IDocumentViewer.DefaultHighlightColor"/> 
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
        /// <seealso cref="Forms.IDocumentViewer.RecognizeHighlightText"/>
        public Highlight(IDocumentViewer imageViewer, string comment, PointF start, PointF end,
            float height, int pageNumber, string text, Color color)
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

                // [FlexIDSCore:3860]
                // Ensure the newly created highlight is of minimum size.
                Rectangle bounds = GetBounds();
                if (bounds.Width < MinSize.Width)
                {
                    _endPoint.X += MinSize.Width - bounds.Width;
                }
                if (bounds.Height < MinSize.Height)
                {
                    _height = MinSize.Height;
                }

                if (SpotIRCompatible && MinSize.Equals(DefaultMinSize))
                {
                    // Create a temporary raster zone to ensure that highlight
                    // will be rendered and saved the same way (ie. WYSIWYG)
                    // TODO: This code can be removed when the SpotRecognitionWindow is retired for 
                    // the .Net ImageViewer and the SpatialString code that adjusts the endpoints 
                    // onto the page is also removed.
                    RasterZone zone = GetTempRasterZone();
                    _startPoint = new PointF(zone.StartX, zone.StartY);
                    _endPoint = new PointF(zone.EndX, zone.EndY);
                    _height = zone.Height;
                }

                // Calculate the region
                CalculateRegion(false);
            }
            catch (Exception e)
            {
                ExtractException ee = new ExtractException("ELI21125",
                    "Unable to create highlight.", e);
                ee.AddDebugData("Image viewer", imageViewer.ToString(), false);
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
        /// <paramref name="imageViewer"/>'s <see cref="Forms.IDocumentViewer.RecognizeHighlightText"/> 
        /// property is <see langword="false"/> or recognized text if the 
        /// <see cref="Forms.IDocumentViewer.RecognizeHighlightText"/> property is 
        /// <see langword="true"/>.</para>
        /// <para>The default <see cref="System.Drawing.Color"/> is the value of the 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.IDocumentViewer.DefaultHighlightColor"/> 
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
        /// <seealso cref="Forms.IDocumentViewer.RecognizeHighlightText"/>
        /// <seealso cref="Forms.IDocumentViewer.DefaultHighlightColor"/>
        public Highlight(IDocumentViewer imageViewer, string comment, RasterZone rasterZone)
            : this(imageViewer, comment, rasterZone.Start, rasterZone.End,
                rasterZone.Height, rasterZone.PageNumber, null, imageViewer.DefaultHighlightColor)
        {

        }

        /// <summary>
        /// Initializes a new <see cref="Highlight"/> class from the <see cref="RasterZone"/> that 
        /// contains the specified text and is the specified color.
        /// </summary>
        /// <remarks>
        /// <para>The default text is the empty <see cref="string"/> if 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.IDocumentViewer.RecognizeHighlightText"/> 
        /// property is <see langword="false"/> or recognized text if the 
        /// <see cref="Forms.IDocumentViewer.RecognizeHighlightText"/> property is 
        /// <see langword="true"/>.
        /// </para>
        /// <para>The default <see cref="System.Drawing.Color"/> is the value of the 
        /// <paramref name="imageViewer"/>'s <see cref="Forms.IDocumentViewer.DefaultHighlightColor"/> 
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
        /// <seealso cref="Forms.IDocumentViewer.RecognizeHighlightText"/>
        public Highlight(IDocumentViewer imageViewer, string comment, RasterZone rasterZone,
            string text, Color color)
            : this(imageViewer, comment, rasterZone.Start, rasterZone.End, rasterZone.Height,
                rasterZone.PageNumber, text, color)
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
        public PointF StartPoint
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
                        CalculateRegion(false);
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
        public PointF EndPoint
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
                        CalculateRegion(false);
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
        public float Height
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
                        CalculateRegion(true);
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
        /// the <see cref="Forms.IDocumentViewer.DefaultHighlightColor"/> value of its 
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
        /// <see cref="Forms.IDocumentViewer.RecognizeHighlightText"/> 
        /// property is <see langword="false"/> or recognized text if the 
        /// <see cref="Forms.IDocumentViewer.RecognizeHighlightText"/> property is 
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

        /// <summary>
        /// Gets or sets whether highlights need to be created such that they are WYSIWYG compatible
        /// with the Spot recognition window.
        /// </summary>
        /// <value><see langword="true"/> if the highlights need to be guaranteed to appear at the
        /// same pixel coordinates as they would in the Spot window, <see langword="false"/> if this
        /// guarantee is unnecessary.</value>
        /// <remarks>If a large number of highlights (hundreds+) need to be displayed, there will be
        /// a noticeable performance hit for compatibility. If the property is set to conflicting
        /// values, the property will remain <see langword="true"/> regardless of which value was
        /// set first.</remarks>
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public static bool SpotIRCompatible
        {
            get
            {
                return _spotIRCompatible == null ? true : _spotIRCompatible.Value;
            }

            set
            {
                if (_spotIRCompatible == null)
                {
                    _spotIRCompatible = value;
                }
                else if (_spotIRCompatible.Value != value)
                {
                    new ExtractException("ELI30033", "Application trace: " +
                        "Conflicting SpotIRCompatible modes; setting value to true").Log();

                    _spotIRCompatible = true;
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a temporary raster zone to ensure the highlight will be saved the same way 
        /// it is rendered.
        /// </summary>
        /// <returns>A temporary raster zone to ensure the highlight will be saved the same way 
        /// it is rendered.</returns>
        RasterZone GetTempRasterZone()
        {
            SpatialString spatialString = GetTempSpatialString();
            IUnknownVector vector = spatialString.GetOriginalImageRasterZones();
            RasterZoneCollection zones = new RasterZoneCollection(vector);
            return zones[0];
        }

        /// <summary>
        /// Creates a temporary spatial string to ensure the highlight will be saved the same way 
        /// it is rendered.
        /// </summary>
        /// <returns>A temporary spatial string to ensure the highlight will be saved the same way 
        /// it is rendered.</returns>
        SpatialString GetTempSpatialString()
        {
            RasterZone zone = ToRasterZone();

            IUnknownVector zones = new IUnknownVector();
            zones.PushBack(zone.ToComRasterZone());

            SpatialString spatialString = new SpatialString();
            spatialString.CreateHybridString(zones, "Temp", "Temp", GetPageInfoMap());

            return spatialString;
        }

        /// <summary>
        /// Gets the page info map for the currently open image.
        /// </summary>
        /// <returns>The page info map for the currently open image.</returns>
        LongToObjectMap GetPageInfoMap()
        {
            // Iterate over each page of the image.
            LongToObjectMap pageInfoMap = new LongToObjectMap();
            ImagePageProperties pageProperties = ImageViewer.GetPageProperties(PageNumber);

            // Create the spatial page info for this page
            SpatialPageInfo pageInfo = new SpatialPageInfo();
            int width = pageProperties.Width;
            int height = pageProperties.Height;
            pageInfo.Initialize(width, height, EOrientation.kRotNone, 0);

            // Add it to the map
            pageInfoMap.Set(PageNumber, pageInfo);

            return pageInfoMap;
        }

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
                    using (GdiGraphics gdiGraphics = new GdiGraphics(graphics, RasterDrawMode.MaskPen))
                    {
                        gdiGraphics.DrawPolygon(pen, vertices);
                    }
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
            IDocumentViewer imageViewer = base.ImageViewer;
            if (imageViewer != null)
            {
                // Paint the highlight's region
                if (_color != Color.Transparent)
                {
                    using (GdiGraphics gdiGraphics = new GdiGraphics(graphics, drawMode))
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
                IDocumentViewer imageViewer = base.ImageViewer;
                return imageViewer != null && imageViewer.PageNumber == PageNumber &&
                       _region.IsVisible(point);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22394", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified rectangle intersects with the highlight.
        /// </summary>
        /// <param name="rectangle">The rectangle to test for containment in logical (image)
        /// coordinates.
        /// </param>
        /// <returns><see langword="true"/> if the rectangle intersects the highlight;
        /// <see langword="false"/> if it does not.</returns>
        public override bool HitTest(Rectangle rectangle)
        {
            try
            {
                return _region.IsVisible(rectangle);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31288", ex);
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

                // Check if the highlight is being rotated
                if (GetIsRotation(gripHandleId))
                {
                    return ExtractCursors.Rotate;
                }

                // Get the grip handles in client coordinates
                base.ImageViewer.Transform.TransformPoints(gripPoints);

                // Return the appropriate resize cursor
                return GetResizeCursor(gripPoints, gripHandleId);
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
        /// <param name="gripPoints">The positions of the 8 grip handles for the highlight.</param>
        /// <param name="gripId">The id of the handle to get a resize cursor for.</param>
        /// <returns>The resize cursor to use based on the mouse <paramref name="gripId"/>.</returns>
        Cursor GetResizeCursor(PointF[] gripPoints, int gripId)
        {
            // Get the center of the highlight in client coordinates
            PointF[] center = new PointF[] 
            {
                GetCenterPoint()
            };
            base.ImageViewer.Transform.TransformPoints(center);

            // If this is a corner handle, we want to make its angle 45 degress more than than the
            // angle that is used for the next side counterclockwise from the specified corner.
            bool cornerResize = (gripId >= 4);
            gripId %= 4;

            PointF point = gripPoints[gripId];

            // Get the angle in degrees between the highlight's 
            // center and the center of the selected grip handle.
            double angle = GeometryMethods.GetAngle(center[0], point)
                           * 180.0 / Math.PI;

            if (cornerResize)
            {
                angle += 45;
            }

            // Express the angle as a positive number greater than 22.5
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
            switch ((int)Math.Round(angle / 45.0) % 4)
            {
                // Closest to 0 or 180 degrees
                case 0:
                    return Cursors.SizeWE;

                // Closest to 45 or 225 degrees
                case 1:
                    return Cursors.SizeNWSE;

                // Closest to 90 or 270 degrees
                case 2:
                    return Cursors.SizeNS;

                // Closest to 135 or 315 degrees
                case 3:
                    return Cursors.SizeNESW;

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
                _startPoint.X += offsetBy.X;
                _startPoint.Y += offsetBy.Y;
                _endPoint.X += offsetBy.X;
                _endPoint.Y += offsetBy.Y;

                if (base.ImageViewer != null)
                {
                    CalculateRegion(true);

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
                // Store the original spatial data
                Store();

                // Get the grip handles in image and client coordinates
                PointF[] imageGrips = GetGripPoints();
                PointF[] clientGrips = (PointF[])imageGrips.Clone();
                base.ImageViewer.Transform.TransformPoints(clientGrips);

                // The highlight is being rotated if:
                // 1) The side of a rectangular highlight is clicked with the CTRL key OR
                // 2) The start or end point of an angular highlight is clicked
                bool isRotation = GetIsRotation(gripHandleId);

                // Store the point of reference for the tracking event
                PointF trackingPoint = new PointF(mouseX, mouseY);

                // Set the cross cursor if necessary
                base.ImageViewer.Cursor =
                    isRotation
                        ? ExtractCursors.ActiveRotate
                        : base.ImageViewer.GetSelectionCursor(mouseX, mouseY);

                // Restructure the highlight so that the selected 
                // grip handle is the end point of the highlight.
                if (isRotation)
                {
                    MakeGripHandleEndPoint(imageGrips, gripHandleId);
                }

                // Store the active highlight vector
                UpdateTrackingVectors(gripHandleId);

                // Get the clipping area in client coordinates.
                // [DotNetRCAndUtils #92]
                Rectangle clip = Rectangle.Union(base.ImageViewer.PhysicalViewRectangle,
                    ((Control)base.ImageViewer).ClientRectangle);

                // Start the tracking event
                TrackingData = new TrackingData((Control)base.ImageViewer,
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
        /// Determines whether the current interactive grip handle event is rotation.
        /// </summary>
        /// <param name="gripHandleId">The id of the grip handle that was clicked.</param>
        /// <returns><see langword="true"/> if the grip handle event is rotation; 
        /// <see langword="false"/> if it is resizing or adjusting the height.</returns>
        static bool GetIsRotation(int gripHandleId)
        {
            return gripHandleId < 4 && Control.ModifierKeys == Keys.Control;
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
                _originalLine = new PointF[] { _startPoint, _endPoint };
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
                QuietSetSpatialData(_originalLine[0], _originalLine[1], _originalHeight, false);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22398", ex);
            }
        }

        /// <summary>
        /// Recomputes the <see cref="_activeTrackingVectors"/>, the unit vectors describing the
        /// angle of the original highlight.
        /// </summary>
        /// <param name="gripHandleId">>The id of the grip handle being moved.</param>
        void UpdateTrackingVectors(int gripHandleId)
        {
            _activeTrackingVectors.Clear();

            List<int> trackingSideIds = new List<int>();
            trackingSideIds.Add(gripHandleId % 4);
            if (gripHandleId > 3)
            {
                // If a corner grip handle was clicked, we'll be moving the next side
                // counterclockwise from the first side as well.
                trackingSideIds.Add((trackingSideIds[0] + 3) % 4);
            }

            // Calculate a unit vector for each side being moved.
            foreach (int trackingSideId in trackingSideIds)
            {
                // Get the vector components of the current highlight
                float x, y;

                int oppositeSideId = (trackingSideId + 2) % 4;

                PointF[] gripPoints = GetGripPoints();
                x = gripPoints[trackingSideId].X - gripPoints[oppositeSideId].X;
                y = gripPoints[trackingSideId].Y - gripPoints[oppositeSideId].Y;

                // Convert the vector to a unit vector and store it
                float magnitude = (float)Math.Sqrt(x * x + y * y);
                PointF trackingVector = new PointF(x / magnitude, y / magnitude);

                // Associate the unit vector with the side being moved.
                switch (trackingSideId)
                {
                    case 0: _activeTrackingVectors[Side.Left] = trackingVector; break;
                    case 1: _activeTrackingVectors[Side.Bottom] = trackingVector; break;
                    case 2: _activeTrackingVectors[Side.Right] = trackingVector; break;
                    case 3: _activeTrackingVectors[Side.Top] = trackingVector; break;
                    
                    default:
                        throw new ExtractException("ELI32818", "Internal logic error.");
                }
            }
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

                IDocumentViewer imageViewer = base.ImageViewer;

                // Get the mouse position as a point in image coordinates
                PointF mouse =
                    GeometryMethods.InvertPoint(imageViewer.Transform, new PointF(mouseX, mouseY));

                // Check if the adjust angle tool is being used
                if (imageViewer.Cursor == ExtractCursors.ActiveRotate)
                {
                    // Update the end point
                    QuietSetEndPoint(_trackingEventEnding ? Point.Round(mouse) : mouse);
                }
                else if (imageViewer.Cursor != Cursors.SizeAll)
                {
                    // Resize the highlight
                    ResizeActiveHighlight(mouse);
                }
                else
                {
                    imageViewer.Cursor = imageViewer.Cursor;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22399", ex);
            }
        }

        /// <summary>
        /// Ends an interactive tracking event
        /// </summary>
        /// <param name="mouseX">The physical (client) x-coordinate of the mouse cursor.</param>
        /// <param name="mouseY">The physical (client) y-coordinate of the mouse cursor.</param>
        public override void EndTracking(int mouseX, int mouseY)
        {
            try
            {
                _trackingEventEnding = true;

                // If this wasn't a rotation tracking event, make sure the highlight angle remains
                // locked.
                bool lockAngle = (ImageViewer.Cursor != ExtractCursors.ActiveRotate);

                base.EndTracking(mouseX, mouseY);

                CalculateRegion(lockAngle);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32820");
            }
            finally
            {
                _trackingEventEnding = false;
            }
        }

        /// <summary>
        /// Resizes the active highlight using the specified mouse position.
        /// </summary>
        /// <param name="mouse">The position in logical (image) coordinates.</param>
        /// <remarks>The highlight maintains its start point, height, and angle. Only the end 
        /// point of the highlight is changed.</remarks>
        void ResizeActiveHighlight(PointF mouse)
        {
            // Get the mouse position as a point in image coordinates
            PointF trackingStart =
                GeometryMethods.InvertPoint(ImageViewer.Transform, TrackingData.StartPoint);

            // Construct the vector components for a vector from the start point to the mouse
            double x = mouse.X - trackingStart.X;
            double y = mouse.Y - trackingStart.Y;

            // Retrive a ZoneGeometry representing the new highlight size.
            ZoneGeometry resizedGeometry = GetResizedGeometry(x, y);

            // Apply the resizedGeometry coordinates. 
            PointF startPoint;
            PointF endPoint;
            float height;

            // To ensure the start and end points are kept as far away from each other as
            // possible to minimize angle error after rounding at the end of a resize
            // operation, if the width is less than the height, move the the start and end
            // points to what is currntly the top and bottom sides.
            if (_trackingEventEnding && resizedGeometry.Width < resizedGeometry.Height)
            {
                ChangeTrackingDataOrientation();

                // Recalculate the geometry with a swapped orientation.
                resizedGeometry = GetResizedGeometry(x, y);
            }
            
            // In order to ensure the side(s) being resized do not move "in" from their current
            // positions (thus leaking pixels the user wasn't intending to leak), inflate these
            // sides by .5 pixels before rounding to ensure these sides are rounded "safely".
            // Do not use "safe" rounding for the whole zone since this results in frustrating
            // expansion of all sides as the zone is re-sized.
            if (_trackingEventEnding)
            {
                foreach (Side side in _activeTrackingVectors.Keys)
                {
                    // Add slightly less than half a pixel to prevent modifying coordinates if the
                    // starting point was an integer).
                    resizedGeometry.InflateSide(side, 0.499F);
                }
            }
            
            RoundingMode roundingMode = _trackingEventEnding ? RoundingMode.Simple : RoundingMode.None;
            resizedGeometry.GetZoneCoordinates(roundingMode, out startPoint, out endPoint, out height);

            QuietSetSpatialData(startPoint, endPoint, height, true);
        }

        /// <summary>
        /// Gets a <see cref="ZoneGeometry"/> representing the coordinates of the highlight after
        /// the active grip point is moved to the specified image coordinates.
        /// </summary>
        /// <param name="x">The x image coordinate.</param>
        /// <param name="y">The y image coordinate.</param>
        /// <returns>A <see cref="ZoneGeometry"/> representing the resized highlight.</returns>
        ZoneGeometry GetResizedGeometry(double x, double y)
        {
            ZoneGeometry resizedGeometry = (ZoneGeometry)_zoneGeometry.Clone();

            // Inflate each side that is being moved based on the distance and direction the mouse
            // was moved.
            foreach (KeyValuePair<Side, PointF> trackingVector in _activeTrackingVectors)
            {
                // Compute the dot product of the vectors
                // NOTE: Because the active highlight vector is a unit vector, this is equivalent 
                // to the the scalar projection of the mouse vector onto the active highlight vector.
                double dotProduct = x * trackingVector.Value.X + y * trackingVector.Value.Y;

                resizedGeometry.InflateSide(trackingVector.Key, (float)dotProduct);
            }

            return resizedGeometry;
        }

        /// <summary>
        /// Changes the tracking data orientation such that move the start and end points are
        /// assumed to be on what is currently the top and bottom sides.
        /// </summary>
        void ChangeTrackingDataOrientation()
        {
            _zoneGeometry.RotateOrientation(90);

            Dictionary<Side, PointF> rotatedTrackingVectors = new Dictionary<Side, PointF>();

            foreach (KeyValuePair<Side, PointF> trackingVector in _activeTrackingVectors)
            {
                // Adjust each side being adjusted in the tracking vector to the next side in a
                // counter-clockwise direction to account for 
                Side newSide = (Side)(((int)trackingVector.Key + 3) % 4);
                rotatedTrackingVectors[newSide] = trackingVector.Value;
            }

            _activeTrackingVectors = rotatedTrackingVectors;
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
                if (_zoneGeometry != null)
                {
                    _zoneGeometry.RotateOrientation(180);
                }

                // Swap the start point and end point
                QuietSetSpatialData(_endPoint, _startPoint, _height, true);

                // Done.
                return;
            }

            // This is an endpoint of the height.
            // Find the other endpoint.
            for (int i = 0; i < gripHandles.Length; i++)
            {
                if (gripHandleId != i && gripHandles[i] != start && gripHandles[i] != end)
                {
                    if (_zoneGeometry != null)
                    {
                        _zoneGeometry.RotateOrientation((gripHandleId == 1) ? 90 : -90);
                    }

                    // Define the new highlight
                    PointF[] points = GetGripPoints(_startPoint, _endPoint, _height);
                    QuietSetSpatialData(points[i], points[gripHandleId],
                        (float)GeometryMethods.Distance(_startPoint, _endPoint), true);

                    // Done.
                    return;
                }
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
        internal void QuietSetEndPoint(PointF endPoint)
        {
            _endPoint = endPoint;

            if (base.ImageViewer != null)
            {
                CalculateRegion(false);
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
        /// <param name="lockAngle"><see langword="true"/> if the new spatial data is intended to
        /// maintain the existing angle of the highlight; <see langword="false"/> otherwise.</param>
        // This is not the compound word "endpoint". This is the "end point", meant in contrast to
        // "start point".
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId = "endPoint")]
        internal void QuietSetSpatialData(PointF startPoint, PointF endPoint, float height,
            bool lockAngle)
        {
            // Set the spatial data
            _startPoint = startPoint;
            _endPoint = endPoint;
            _height = height;

            // Check if this highlight is attached to an image viewer
            if (base.ImageViewer != null)
            {
                // Recalculate its region
                CalculateRegion(lockAngle);
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
        /// <param name="rasterZone">The <see cref="RasterZone"/> to use to set
        /// the spatial data.
        /// <para><b>Note:</b></para>
        /// The <paramref name="rasterZone"/> specified must be on the same page
        /// as this <see cref="Highlight"/>.</param>
        /// <param name="quietSetData">If <see langword="true"/> then will quietly
        /// update the spatial data, if <see langword="false"/> then a layer object
        /// changed event will be raised.</param>
        /// <param name="lockAngle"><see langword="true"/> if the new spatial data is intended to
        /// maintain the existing angle of the highlight; <see langword="false"/> otherwise.</param>
        /// <exception cref="ExtractException">If the <paramref name="rasterZone"/>
        /// is on a different page than this <see cref="Highlight"/>.</exception>
        public void SetSpatialData(RasterZone rasterZone, bool quietSetData,
            bool lockAngle)
        {
            try
            {
                ExtractException.Assert("ELI30079", "Raster zone page mismatch.",
                    rasterZone.PageNumber == PageNumber, "Raster Zone Page", rasterZone.PageNumber,
                    "Highlight Page", PageNumber);

                if (quietSetData)
                {
                    QuietSetSpatialData(rasterZone.Start, rasterZone.End, rasterZone.Height, lockAngle);
                }
                else
                {
                    SetSpatialData(rasterZone.Start, rasterZone.End, rasterZone.Height, lockAngle);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30080", ex);
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
        /// <param name="lockAngle"><see langword="true"/> if the new spatial data is intended to
        /// maintain the existing angle of the highlight; <see langword="false"/> otherwise.</param>
        // This is not the compound word "endpoint". This is the "end point", meant in contrast to
        // "start point".
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId = "endPoint")]
        public void SetSpatialData(PointF startPoint, PointF endPoint, float height,
            bool lockAngle)
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
                    CalculateRegion(lockAngle);
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
        /// <param name="roundingMode">The <see cref="RoundingMode"/> to use after inflating the
        /// highlight.</param>
        /// <param name="setDirty">if set to <see langword="true"/> the dirty flag will be set.
        /// </param>
        /// <remarks>
        /// Negative values of <paramref name="size"/> deflate the highlight.
        /// </remarks>
        public void Inflate(float size, RoundingMode roundingMode, bool setDirty)
        {
            try
            {
                // Do nothing if the size is zero
                if (size != 0)
                {
                    Rectangle originalBounds = GetBounds();
                    ZoneGeometry originalZoneGeometry = (ZoneGeometry)_zoneGeometry.Clone();

                    // Inflate each side the specified amount.
                    _zoneGeometry.InflateSide(Side.Left, size);
                    _zoneGeometry.InflateSide(Side.Top, size);
                    _zoneGeometry.InflateSide(Side.Right, size);
                    _zoneGeometry.InflateSide(Side.Bottom, size);

                    PointF startPoint;
                    PointF endPoint;
                    float height;
                    _zoneGeometry.GetZoneCoordinates(roundingMode, out startPoint, out endPoint, out height);

                    QuietSetSpatialData(startPoint, endPoint, height, true);
                    Rectangle newBounds = GetBounds();

                    // If the width or height of the zone has not changed when inflating by a small factor,
                    // double the magnitude of the resize so that all sides move.
                    if (size < 2 &&
                        (newBounds.Width == originalBounds.Width && newBounds.Width > MinSize.Width + 1) ||
                        (newBounds.Height == originalBounds.Height && newBounds.Height > MinSize.Height + 1))
                    {
                        _zoneGeometry = originalZoneGeometry;

                        size *= 2;

                        _zoneGeometry.InflateSide(Side.Left, size);
                        _zoneGeometry.InflateSide(Side.Top, size);
                        _zoneGeometry.InflateSide(Side.Right, size);
                        _zoneGeometry.InflateSide(Side.Bottom, size);

                        _zoneGeometry.GetZoneCoordinates(roundingMode, out startPoint, out endPoint, out height);
                        QuietSetSpatialData(startPoint, endPoint, height, true);
                    }
                }

                // Check if this highlight is attached to an image viewer
                if (base.ImageViewer != null)
                {
                    // Recalculate its region
                    CalculateRegion(true);
                }

                if (setDirty)
                {
                    Dirty = true;
                }
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
        public override IDocumentViewer ImageViewer
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
        /// <param name="lockAngle"><see langword="true"/> if the new spatial data is intended to
        /// maintain the existing angle of the highlight; <see langword="false"/> otherwise.</param>
        /// <remarks>
        /// <para>This method should not be called if the highlight is detached from an image 
        /// viewer.</para>
        /// <para>This method is called whenever the highlight's spatial information is 
        /// modified. For instance, when the <see cref="StartPoint"/>, <see cref="EndPoint"/>, and
        /// <see cref="Height"/> properties are set.</para>
        /// </remarks>
        void CalculateRegion(bool lockAngle)
        {
            // Calculate the angle of the line
            _angle = GeometryMethods.GetAngle(_startPoint, _endPoint);

            // Dispose of the previous region if it exists
            if (_region != null)
            {
                _region.Dispose();
            }

            _region = GetAngularRegion(_startPoint, _endPoint, _height);

            if (TrackingData == null)
            {
                if (!lockAngle || _zoneGeometry == null)
                {
                    // Create entirely new ZoneGeometry based on the current coordinates.
                    _zoneGeometry = new ZoneGeometry(ToRasterZone());
                }
                else
                {
                    // Update the ZoneGeometry vertices, but do not allow the angle to change.
                    _zoneGeometry.UpdateVertices(ToRasterZone());
                }
            }
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
        internal static Region GetAngularRegion(PointF startPoint, PointF endPoint, float height)
        {
            // Get the corners of the specified specified region
            PointF[] vertices = GeometryMethods.GetVertices(startPoint, endPoint, height);

            using (GraphicsPath path = new GraphicsPath(vertices, _RECTANGULAR_PATH_POINT_TYPE))
            {
                // Construct and return the region
                return new Region(path);
            }
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
                RasterZone zone = ToRasterZone();
                Point start;
                Point end;
                int height;
                zone.GetRoundedCoordinates(Imaging.RoundingMode.Safe, out start, out end, out height);

                return GeometryMethods.GetBoundingRectangle(start, end, height);
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
            PointF[] gripHandles = new PointF[] 
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
            double expandBy = (ExtractPens.ThickPenWidth - 1) / 2.0 / ImageViewer.GetScaleFactorY();
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
            double expandBy = GetGripPointDistance();

            GetExpandedZone(expandBy, out start, out end, out height);
        }

        /// <summary>
        /// Gets the distance between the side of the highlight and its grip handle in image 
        /// pixels.
        /// </summary>
        /// <returns>The distance between the side of the highlight and its grip handle in image 
        /// pixels.</returns>
        double GetGripPointDistance()
        {
            double expandBy = SelectionPen.Width / 2.0;
            if (BorderColor != Color.Transparent)
            {
                expandBy += ExtractPens.ThickPenWidth - 1;
            }

            // Convert to image coordinates
            expandBy /= ImageViewer.GetScaleFactorY();
            return expandBy;
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

                // Both endpoints must be on the page [FIDSC #3746]
                if (!IsPointOnPage(_startPoint) || !IsPointOnPage(_endPoint))
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
        bool IsPointOnPage(PointF point)
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
            CalculateRegion(false);
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
