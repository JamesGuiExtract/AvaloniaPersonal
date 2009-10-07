using Extract.Licensing;
using Extract.Utilities.Forms;
using Leadtools;
using Leadtools.Annotations;
using Leadtools.Codecs;
using Leadtools.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Specifies tools that involve user interaction with the mouse cursor.
    /// </summary>
    public enum CursorTool
    {
        /// <summary>
        /// No cursor tool selected.
        /// </summary>
        None,

        /// <summary>
        /// Allows the user to zoom in to a particular rectangular region.
        /// </summary>
        ZoomWindow,

        /// <summary>
        /// Allows the user to pan the image in all directions.
        /// </summary>
        Pan,

        /// <summary>
        /// Allows the user to draw an angled rectangular highlight.
        /// </summary>
        AngularHighlight,

        /// <summary>
        /// Allows the user to draw a rectangular highlight with sides perpendicular and parallel 
        /// to the sides of the image.
        /// </summary>
        RectangularHighlight,

        /// <summary>
        /// Allows the user to draw an angled rectangular redaction.
        /// </summary>
        AngularRedaction,

        /// <summary>
        /// Allows the user to draw a rectangular redaction with sides perpendicular and parallel 
        /// to the sides of the image.
        /// </summary>
        RectangularRedaction,

        /// <summary>
        /// Allows the user to specify the default highlight height of angular highlights.
        /// </summary>
        SetHighlightHeight,

        /// <summary>
        /// Allows the user to edit the highlight text of a particular highlight.
        /// </summary>
        EditHighlightText,

        /// <summary>
        /// Allows the user to delete one or more layer objects.
        /// </summary>
        DeleteLayerObjects,

        /// <summary>
        /// Allows the user to select a layer object.
        /// </summary>
        SelectLayerObject
    }

    /// <summary>
    /// Specifies the mode to scale the image within the visible area.
    /// </summary>
    public enum FitMode
    {
        /// <summary>
        /// Image is not scaled in any particular way.
        /// </summary>
        None,

        /// <summary>
        /// Image is scaled so that the whole page fills the visible area and the original 
        /// proportions of the image are maintained.
        /// </summary>
        FitToPage,

        /// <summary>
        /// Image is scaled so the width of the image fills the visible area and the original 
        /// proportions of the image are maintained.
        /// </summary>
        FitToWidth
    }

    /// <summary>
    /// Represents a particular zoom setting.
    /// </summary>
    public struct ZoomInfo : IEquatable<ZoomInfo>
    {
        #region ZoomInfo Fields

        /// <summary>
        /// The center point of the visible image in logical (image) coordinates.
        /// </summary>
        Point _zoomCenter;

        /// <summary>
        /// The scale factor applied to the image.
        /// </summary>
        /// <remarks>This value is undefined if <see cref="_fitMode"/> is not 
        /// <see cref="Extract.Imaging.Forms.FitMode.None"/>.</remarks>
        double _scaleFactor;

        /// <summary>
        /// The mode to scale the image within the visible area.
        /// </summary>
        FitMode _fitMode;

        #endregion ZoomInfo Fields

        #region ZoomInfo Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoomInfo"/> class with the specified 
        /// zoom area and fit mode.
        /// </summary>
        /// <param name="center">The center of the visible image area in logical (image) 
        /// coordinates.</param>
        /// <param name="scaleFactor">The ratio of physical (client) pixel size to logical (image) 
        /// pixel size.</param>
        /// <param name="fitMode">The mode to scale the image within the visible area.</param>
        public ZoomInfo(Point center, double scaleFactor, FitMode fitMode)
        {
            _zoomCenter = center;
            _scaleFactor = scaleFactor;
            _fitMode = fitMode;
        }

        #endregion ZoomInfo Constructors

        #region ZoomInfo Properties

        /// <summary>
        /// Gets or sets the center point of the visible image in logical (image) coordinates.
        /// </summary>
        /// <value>The center point of the visible image in logical (image) coordinates.
        /// </value>
        /// <returns>The center point of the visible image in logical (image) coordinates.
        /// </returns>
        public Point Center
        {
            get
            {
                return _zoomCenter;
            }
            set
            {
                _zoomCenter = value;
            }
        }

        /// <summary>
        /// Gets or sets the scale factor applied to the image.
        /// </summary>
        /// <value>The scale factor applied to the image.</value>
        /// <returns>The scale factor applied to the image. This value is undefined if the
        /// <see cref="FitMode"/> property is not 
        /// <see cref="Extract.Imaging.Forms.FitMode.None"/>.</returns>
        public double ScaleFactor
        {
            get
            {
                return _scaleFactor;
            }
            set
            {
                _scaleFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the mode to scale the image within the visible area.
        /// </summary>
        /// <value>The mode to scale the image within the visible area.</value>
        /// <returns>The mode to scale the image within the visible area.</returns>
        public FitMode FitMode
        {
            get
            {
                return _fitMode;
            }
            set
            {
                _fitMode = value;
            }
        }

        #endregion ZoomInfo Properties

        #region ZoomInfo Methods

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is a <see cref="ZoomInfo"/> and 
        /// whether it describes the same zoom setting as this <see cref="ZoomInfo"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="ZoomInfo"/> and 
        /// describes the same zoom setting as this <see cref="ZoomInfo"/>; <see langword="false"/>
        /// if they differ.</returns>
        public override bool Equals(object obj)
        {
            return obj is ZoomInfo ? Equals((ZoomInfo)obj) : false;
        }

        /// <summary>
        /// Returns a hash code for this <see cref="ZoomInfo"/>
        /// </summary>
        /// <returns>A hash value for this <see cref="ZoomInfo"/>.</returns>
        public override int GetHashCode()
        {
            return (int)_fitMode ^ _scaleFactor.GetHashCode() ^ _zoomCenter.GetHashCode();
        }

        #endregion ZoomInfo Methods

        #region ZoomInfo Operators

        /// <summary>
        /// Compares two <see cref="ZoomInfo"/> objects. The result specifies whether the two 
        /// <see cref="ZoomInfo"/> objects are equal.
        /// </summary>
        /// <param name="left">A <see cref="ZoomInfo"/> to compare.</param>
        /// <param name="right">A <see cref="ZoomInfo"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> describes the same zoom 
        /// setting as <paramref name="right"/>; <see langword="false"/> if <paramref name="left"/>
        /// and <paramref name="right"/> differ.</returns>
        public static bool operator ==(ZoomInfo left, ZoomInfo right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="ZoomInfo"/> objects. The result specifies whether the two 
        /// <see cref="ZoomInfo"/> objects are unequal.
        /// </summary>
        /// <param name="left">A <see cref="ZoomInfo"/> to compare.</param>
        /// <param name="right">A <see cref="ZoomInfo"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>
        /// differ; <see langword="false"/> if <paramref name="left"/> describes the same zoom 
        /// setting as <paramref name="right"/>.</returns>
        public static bool operator !=(ZoomInfo left, ZoomInfo right)
        {
            return !left.Equals(right);
        }

        #endregion ZoomInfo Operators

        #region IEquatable<ZoomInfo> Members

        /// <summary>
        /// Determines whether this <see cref="ZoomInfo"/> describes the same zoom setting as the 
        /// specified <see cref="ZoomInfo"/>.
        /// </summary>
        /// <param name="other">The <see cref="ZoomInfo"/> object to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> describes the same zoom 
        /// setting as this <see cref="ZoomInfo"/>; <see langword="false"/> if they differ.</returns>
        public bool Equals(ZoomInfo other)
        {
            return _fitMode == other._fitMode &&
                _scaleFactor == other._scaleFactor &&
                _zoomCenter == other._zoomCenter;
        }

        #endregion IEquatable<ZoomInfo> Members
    }

    /// <summary>
    /// Represents a control that can display and interact with image files.
    /// </summary>
    [ToolboxBitmap(typeof(ImageViewer), ToolStripButtonConstants._IMAGE_VIEWER_ICON_IMAGE)]
    public partial class ImageViewer : RasterImageViewer
    {
        #region Image Viewer Private Structs

        /// <summary>
        /// Represents a collection of mouse cursors associated with a particular
        /// <see cref="CursorTool"/>.
        /// </summary>
        struct ImageViewerCursors
        {
            /// <summary>
            /// The normal <see cref="Cursor"/> to be displayed by the
            /// <see cref="CursorTool"/>.
            /// </summary>
            Cursor _tool;

            /// <summary>
            /// The <see cref="Cursor"/> to be displayed by the
            /// <see cref="CursorTool"/> when it has been activated (i.e. mouse down).
            /// </summary>
            Cursor _active;

            /// <summary>
            /// Gets and sets the normal <see cref="Cursor"/>
            /// </summary>
            /// <returns>The normal <see cref="Cursor"/> to be displayed. <see langword="null"/> 
            /// if the default cursor should be used.</returns>
            /// <value>The normal <see cref="Cursor"/> to be displayed.</value>
            public Cursor Tool
            {
                get
                {
                    return _tool;
                }
                set
                {
                    _tool = value;
                }
            }

            /// <summary>
            /// Gets and sets the active <see cref="Cursor"/>
            /// </summary>
            /// <returns>The active <see cref="Cursor"/> to be displayed.</returns>
            /// <value>The active <see cref="Cursor"/> to be displayed.</value>
            public Cursor Active
            {
                get
                {
                    return _active;
                }
                set
                {
                    _active = value;
                }
            }
        }

        #endregion

        #region Image Viewer Constants

        /// <summary>
        /// Leadtools document (annotations and view perspective) support key constant.
        /// </summary>
        static readonly string _DOCUMENT_SUPPORT_KEY = "vhG42tyuh9";

        /// <summary>
        /// Leadtools pdf save support key constant.
        /// </summary>
        static readonly string _PDF_SAVE_SUPPORT_KEY = "8ksiHnPymr";

        /// <summary>
        /// Leadtools pdf read support key constant.
        /// </summary>
        static readonly string _PDF_READ_SUPPORT_KEY = "xrzGPkmYui";

        /// <summary>
        /// Image file types constant for the open file dialog.
        /// </summary>
        static readonly string[] _IMAGE_FILE_TYPES = new string[] {
            "BMP files (*.bmp;*.rle;*.dib)|*.bmp;*.rle;*.dib|",
            "GIF files (*.gif)|*.gif|",
            "JFIF files (*.jpg;*.jpeg)|*.jpg;*.jpeg|",
            "PCX files (*.pcx)|*.pcx|",
            "PICT files (*.pct)|*.pct|",
            "PNG files (*.png)|*.png|",
            "TIFF files (*.tif;*.tiff)|*.tif;*.tiff|",
            "PDF files (*.pdf)|*.pdf|",
            "All image files|*.bmp;*.rle;*.dib;*.rst;*.gp4;*.mil;*.cal;*.cg4;*.flc;",
            "*.fli;*.gif;*.jpg;*.jpeg;*.pcx;*.pct;*.png;*.tga;*.tif;*.tiff;*.pdf|",
            "All files (*.*)|*.*|" };

        /// <summary>
        /// The default scale factor. Value corresponds to no zoom (image pixels and client pixels 
        /// are the same size).
        /// </summary>
        const double _DEFAULT_SCALE_FACTOR = 1.0;

        /// <summary>
        /// The maximum amount one can zoom in (1 image pixel is 50 screen pixels across)
        /// </summary>
        const double _MAX_ZOOM_IN_SCALE_FACTOR = 50;

        /// <summary>
        /// The maximum amount one can zoom out (1 screen pixel is 50 image pixels across)
        /// </summary>
        const double _MAX_ZOOM_OUT_SCALE_FACTOR = 0.02;

        /// <summary>
        /// The value that the scale factor is multiplied or divided by when zooming in and 
        /// zooming out, respectively. 
        /// </summary>
        /// <seealso cref="ZoomIn"/>
        /// <seealso cref="ZoomOut"/>
        const double _ZOOM_FACTOR = 1.2;

        /// <summary>
        /// The scroll value multiplier applied to 
        /// <see cref="RasterImageViewer.AutoScrollSmallChange"/> during a mousewheel scroll event.
        /// </summary>
        /// <seealso cref="OnMouseWheel"/>
        const int _MOUSEWHEEL_SCROLL_FACTOR = 2;

        /// <summary>
        /// The amount the <see cref="DefaultHighlightHeight"/> is incremented or decremeneted 
        /// during a mousewheel adjust highlight height event.
        /// </summary>
        /// <seealso cref="OnMouseWheel"/>
        const int _MOUSEWHEEL_HEIGHT_INCREMENT = 4;

        /// <summary>
        /// The default height of an angular highlight in logical (image) pixels.
        /// </summary>
        const int _DEFAULT_HIGHLIGHT_HEIGHT = 40;

        /// <summary>
        /// The maximum distance a tile viewing area can be from the edge of the image area to be 
        /// considered on the edge of the image area.
        /// </summary>
        /// <remarks>The Leadtools properties for determining the image area and the zoom setting 
        /// are occasionally imprecise by a few pixels. This value establishes a margin of error 
        /// for proper functionality of zoom tiling.</remarks>
        const int _TILE_EDGE_DISTANCE = 4;

        /// <summary>
        /// The amount of padding space to add to the left and right of a layer object when zooming
        /// it into view.
        /// </summary>
        const int _ZOOM_TO_OBJECT_WIDTH_PADDING = 21;

        /// <summary>
        /// The amount of padding space to add to the top and bottom of a layer object when zooming
        /// it into view.
        /// </summary>
        const int _ZOOM_TO_OBJECT_HEIGHT_PADDING = 21;

        /// <summary>
        /// The default color that redactions will be printed as.
        /// </summary>
        const RedactionColor _DEFAULT_REDACTION_FILL_COLOR = RedactionColor.Black;

        /// <summary>
        /// The default color that redactions will be painted as.
        /// </summary>
        static readonly Color _DEFAULT_REDACTION_PAINT_COLOR = Redaction.BlackPaint;

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ImageViewer).ToString();

        /// <summary>
        /// The number of times to retry acquring the device context for saving.
        /// </summary>
        static readonly int _SAVE_RETRY_COUNT = RegistryManager.SaveRetries;

        #endregion

        #region Image Viewer Static Fields

        /// <summary>
        /// Contains the <see cref="ImageViewerCursors"/> for each <see cref="CursorTool"/>.
        /// </summary>
        static Dictionary<CursorTool, ImageViewerCursors> _cursorsForCursorTools =
            LoadCursorsForCursorTools();

        /// <summary>
        /// One-based index of the default image file type. This value corresponds to the all 
        /// image files option.
        /// </summary>
        /// <seealso cref="_IMAGE_FILE_TYPES"/>
        static readonly int _IMAGE_FILE_TYPE_DEFAULT_INDEX = 9;

        #endregion

        #region Image Viewer Fields

        /// <summary>
        /// File name of the currently open image file.
        /// </summary>
        /// <seealso cref="ImageFile"/>
        string _imageFile = "";

        /// <summary>
        /// The 1-based page number of the image currently being displayed.
        /// </summary>
        int _pageNumber;

        /// <summary>
        /// The number of pages in the currently displayed image.
        /// </summary>
        int _pageCount;

        /// <summary>
        /// The currently activated cursor tool.
        /// </summary>
        /// <seealso cref="ImageViewer.CursorTool"/>
        CursorTool _cursorTool;

        /// <summary>
        /// The last used continuous-use cursor tool.
        /// </summary>
        /// <remarks>Some cursor tools are continuous-use, meaning they can be used repeatedly 
        /// without having to be reactivated. Some cursor tools such as the 
        /// <see cref="Extract.Imaging.Forms.CursorTool.SetHighlightHeight"/> and 
        /// <see cref="Extract.Imaging.Forms.CursorTool.DeleteLayerObjects"/> are single-use tools, 
        /// meaning after they have been used they will reset themselves to the last activated 
        /// continuous-use tool.
        /// </remarks>
        CursorTool _lastContinuousUseTool;

        /// <summary>
        /// The cursor to be displayed based upon the currently active
        /// <see cref="CursorTool"/>.
        /// </summary>
        Cursor _toolCursor;

        /// <summary>
        /// The cursor to be displayed when the currently active
        /// <see cref="CursorTool"/> is activated (i.e. mouse down).
        /// </summary>
        Cursor _activeCursor;

        /// <summary>
        /// The currently active fit mode.
        /// </summary>
        /// <seealso cref="ImageViewer.FitMode"/>
        FitMode _fitMode;

        /// <summary>
        /// Whether to display annotations.
        /// </summary>
        /// <seealso cref="DisplayAnnotations"/>
        bool _displayAnnotations = true;

        /// <summary>
        /// Whether to use anti-aliasing.
        /// </summary>
        /// <seealso cref="UseAntiAliasing"/>
        bool _useAntiAliasing = true;

        /// <summary>
        /// The collection of shortcut keys and shortcut handlers during times when no interactive 
        /// mouse event is occurring.
        /// </summary>
        /// <seealso cref="Shortcuts"/>
        readonly ShortcutsManager _mainShortcuts = new ShortcutsManager();

        /// <summary>
        /// The collection of shortcut keys and shortcut handlers during an interactive mouse 
        /// event (e.g. drawing a highlight).
        /// </summary>
        /// <seealso cref="Shortcuts"/>
        /// <seealso cref="Control.Capture"/>
        readonly ShortcutsManager _captureShortcuts = new ShortcutsManager();

        /// <summary>
        /// Whether to load the default shortcuts.
        /// </summary>
        /// <seealso cref="UseDefaultShortcuts"/>
        bool _useDefaultShortcuts;

        /// <summary>
        /// The collection of <see cref="LayerObject"/> items.
        /// </summary>
        /// <seealso cref="LayerObjects"/>
        LayerObjectsCollection _layerObjects =
            LayerObjectsCollection.CreateLayerObjectsWithSelection();

        /// <summary>
        /// The link layer object that currently has its link arrows displaying.
        /// </summary>
        LayerObject _activeLinkedLayerObject;

        /// <summary>
        /// If non-null, contains the layer objects under the selection tool.
        /// </summary>
        List<LayerObject> _layerObjectsUnderSelectionTool;

        /// <summary>
        /// The default highlight color for newly created highlights.
        /// </summary>
        /// <seealso cref="DefaultHighlightColor"/>
        Color _defaultHighlightColor = Color.Yellow;

        /// <summary>
        /// The default highlight height for newly created highlights.
        /// </summary>
        /// <seealso cref="DefaultHighlightHeight"/>
        int _defaultHighlightHeight = _DEFAULT_HIGHLIGHT_HEIGHT;

        /// <summary>
        /// The default fill color for newly created redactions.
        /// </summary>
        /// <seealso cref="DefaultRedactionFillColor"/>
        RedactionColor _defaultRedactionFillColor = _DEFAULT_REDACTION_FILL_COLOR;

        /// <summary>
        /// The default paint color for newly created redactions.
        /// </summary>
        Color _defaultRedactionPaintColor = _DEFAULT_REDACTION_PAINT_COLOR;

        /// <summary>
        /// Whether to recognize highlight text for newly created highlights.
        /// </summary>
        /// <seealso cref="RecognizeHighlightText"/>
        bool _recognizeHighlightText = true;

        /// <summary>
        /// A <see cref="TrackingData"/> object associated with the interactive cursor tool event 
        /// currently in progess (e.g. drag and drop).
        /// </summary>
        /// <remarks>Value is <see langword="null"/> if no interactive cursor tool event is being 
        /// tracked.</remarks>
        TrackingData _trackingData;

        /// <summary>
        /// Indicates whether a tracking event is currently ending. Used to prevent calls from
        /// multiple threads calling EndTracking at the same time which can lead to exceptions.
        /// </summary>
        volatile bool _trackingEventEnding;

        /// <summary>
        /// Collection of data associated with each page of currently open image.
        /// </summary>
        /// <remarks>May be <see langword="null"/> if no image is open.</remarks>
        List<ImagePageData> _imagePages;

        /// <summary>
        /// Codes for saving and loading raster image files.
        /// </summary>
        /// <remarks>For performance reasons, the codecs are started once for each instance of the 
        /// <see cref="ImageViewer"/> and shutdown in the <see cref="Dispose"/> method.</remarks>
        RasterCodecs _codecs;

        /// <summary>
        /// <see langword="true"/> if the codecs have been started; <see langword="false"/> if 
        /// they have not yet been started.
        /// </summary>
        bool _codecsStarted;

        /// <summary>
        /// Collection of annotation objects.
        /// </summary>
        AnnContainer _annotations;

        /// <summary>
        /// 3x3 affine transformation matrix that translates logical (original image) coordinates 
        /// into physical (client) coordinates.
        /// </summary>
        /// <remarks>
        /// <para>Unlike the base class property, this matrix takes rotation into account.</para>
        /// <para>This value only has meaning if an image is open.</para>
        /// </remarks>
        /// <seealso cref="Transform"/>
        Matrix _transform;

        /// <summary>
        /// The dialog for setting print options.
        /// </summary>
        /// <remarks><see langword="null"/> until at least one image has been printed.</remarks>
        PrintDialog _printDialog;

        /// <summary>
        /// The dialog for displaying a print preview to the user.
        /// </summary>
        PrintPreviewDialog _printPreview;

        /// <summary>
        /// The document object used for printing.
        /// </summary>
        PrintDocument _printDocument;

        /// <summary>
        /// The page number of the image that is currently being printed.
        /// </summary>
        /// <remarks>Zero indicates that the first page has not yet been printed.</remarks>
        int _printPage;

        /// <summary>
        /// The text of the watermark to add to saved or printed pages, or <see langword="null"/> 
        /// if no watermark should be added.
        /// </summary>
        string _watermark;

        /// <summary>
        /// Stores the current scroll position for the image viewer.
        /// </summary>
        // Added as per [DNRCAU #262 - JDS]
        Point _scrollPosition;

        /// <summary>
        /// Flag for indicating whether the annotation tags on a particular image are valid or not
        /// </summary>
        bool _validAnnotations = true;

        /// <summary>
        /// The list of strings that should be displayed in the open file dialog file type
        /// drop down list.
        /// </summary>
        readonly List<string> _openImageFileTypeFilter = new List<string>(_IMAGE_FILE_TYPES);

        /// <summary>
        /// The default index for the open file dialog file type filter.
        /// </summary>
        int _openImageFilterIndex = _IMAGE_FILE_TYPE_DEFAULT_INDEX;

        /// <summary>
        /// The list of printers that the <see cref="ImageViewer"/> should not print to.
        /// </summary>
        readonly List<string> _disallowedPrinters = new List<string>();

        /// <summary>
        /// The parent form containing the <see cref="ImageViewer"/> control.
        /// </summary>
        Form _parentForm;

        /// <summary>
        /// The default status message that should be displayed when no image is loaded.
        /// </summary>
        string _defaultStatusMessage = "";

        /// <summary>
        /// The file stream for the currently open image.
        /// </summary>
        FileStream _currentOpenFile;

        /// <summary>
        /// Specifies whether the context menu should be prevented from opening.
        /// </summary>
        bool _suppressContextMenu;

        /// <summary>
        /// Specifies whether selection of multiple layer object via a banded box is enabled.
        /// </summary>
        bool _allowBandedSelection = true;

        /// <summary>
        /// License cache for validating the core license.
        /// </summary>
        static readonly LicenseStateCache _licenseCore =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

        /// <summary>
        /// License cache for validating the annotation license.
        /// </summary>
        static readonly LicenseStateCache _licenseAnnotation =
            new LicenseStateCache(LicenseIdName.AnnotationFeature, "Annotation Objects");

        /// <summary>
        /// License cache for validating the anti-alias license.
        /// </summary>
        static readonly LicenseStateCache _licenseAntiAlias =
            new LicenseStateCache(LicenseIdName.AntiAliasingFeature, "Anti-aliasing Component");

        #endregion

        #region Image Viewer Events

        /// <summary>
        /// Occurs when the image file changes.
        /// </summary>
        /// <seealso cref="OpenImage"/>
        public event EventHandler<ImageFileChangedEventArgs> ImageFileChanged;

        /// <summary>
        /// Occurs when the zoom changes.
        /// </summary>
        /// <seealso cref="ZoomIn"/>
        /// <seealso cref="ZoomOut"/>
        /// <seealso cref="ZoomNext"/>
        /// <seealso cref="ZoomPrevious"/>
        public event EventHandler<ZoomChangedEventArgs> ZoomChanged;

        /// <summary>
        /// Occurs when the page orientation changes.
        /// </summary>
        /// <seealso cref="Orientation"/>
        public event EventHandler<OrientationChangedEventArgs> OrientationChanged;

        /// <summary>
        /// Occurs when a new cursor tool is activated.
        /// </summary>
        /// <seealso cref="ImageViewer.CursorTool"/>
        public event EventHandler<CursorToolChangedEventArgs> CursorToolChanged;

        /// <summary>
        /// Occurs when a new fit mode is activated.
        /// </summary>
        /// <seealso cref="ImageViewer.FitMode"/>
        public event EventHandler<FitModeChangedEventArgs> FitModeChanged;

        /// <summary>
        /// Occurs when the currently visible page changes.
        /// </summary>
        /// <seealso cref="PageNumber"/>
        public event EventHandler<PageChangedEventArgs> PageChanged;

        /// <summary>
        /// Occurs when a new image is about to be opened.
        /// </summary>
        public event EventHandler<OpeningImageEventArgs> OpeningImage;

        /// <summary>
        /// Occurs when an image is about to close.
        /// </summary>
        public event EventHandler<ImageFileClosingEventArgs> ImageFileClosing;

        /// <summary>
        /// Occurs when a new image is about to begin loading.
        /// </summary>
        public event EventHandler<LoadingNewImageEventArgs> LoadingNewImage;

        /// <summary>
        /// Occurs when the print dialog is about to be displayed to the user.
        /// </summary>
        public event EventHandler<DisplayingPrintDialogEventArgs> DisplayingPrintDialog;

        /// <summary>
        /// Occurs when an error is encountered when opening a file.
        /// </summary>
        public event EventHandler<FileOpenErrorEventArgs> FileOpenError;

        /// <summary>
        /// Occurs when the selection tool enters the bounds of a <see cref="LayerObject"/>.
        /// <para><b>NOTE:</b></para>
        /// This event is raised even for <see cref="LayerObject"/>s that are not visible or that
        /// are not selectable. It is up to the receiver to filter events for
        /// <see cref="LayerObject"/>s that should be ignored.
        /// </summary>
        public event EventHandler<LayerObjectEventArgs> SelectionToolEnteredLayerObject;

        /// <summary>
        /// Occurs when the selection tool leaves the bounds of a <see cref="LayerObject"/>.
        /// <para><b>NOTE:</b></para>
        /// This event is raised even for <see cref="LayerObject"/>s that are not visible or that
        /// are not selectable. It is up to the receiver to filter events for
        /// <see cref="LayerObject"/>s that should be ignored.
        /// </summary>
        public event EventHandler<LayerObjectEventArgs> SelectionToolLeftLayerObject;

        #endregion

        #region Image Viewer Constructors

        /// <summary>
        /// Initializes a new <see cref="ImageViewer"/> class without any image open.
        /// </summary>
        public ImageViewer()
        {
            try
            {
                // LoadLicenseFilesFromFolder for design mode is now called in 
                // LoadCursorsForCursorTools (ie, the static constructor)
                InitializeComponent();

                _licenseCore.Validate("ELI23109");

                // Check if PDF Read and Write is licensed
                if (LicenseUtilities.IsLicensed(LicenseIdName.PdfReadWriteFeature))
                {
                    // Unlock pdf read and write support
                    RasterSupport.Unlock(RasterSupportType.PdfRead, _PDF_READ_SUPPORT_KEY);
                    RasterSupport.Unlock(RasterSupportType.PdfSave, _PDF_SAVE_SUPPORT_KEY);

                    // Ensure pdf support was unlocked
                    bool pdfReadLocked = RasterSupport.IsLocked(RasterSupportType.PdfRead);
                    bool pdfWriteLocked = RasterSupport.IsLocked(RasterSupportType.PdfSave);
                    if (pdfReadLocked || pdfWriteLocked)
                    {
                        ExtractException ee = new ExtractException("ELI21229",
                            "Unable to unlock pdf support. Pdf support will be limited.");
                        ee.AddDebugData("Pdf reading",
                            pdfReadLocked ? "Locked" : "Unlocked", false);
                        ee.AddDebugData("Pdf writing",
                            pdfWriteLocked ? "Locked" : "Unlocked", false);
                        ee.Display();
                    }
                }

                // Check if Annotations are licensed
                if (LicenseUtilities.IsLicensed(LicenseIdName.AnnotationFeature))
                {
                    // Unlock document (ie. annotations) support
                    RasterSupport.Unlock(RasterSupportType.Document, _DOCUMENT_SUPPORT_KEY);

                }

                // Turn off display of annotation if document support is locked
                if (RasterSupport.IsLocked(RasterSupportType.Document))
                {
                    _displayAnnotations = false;
                }

                // Turn off _useAntiAliasing if bitonal support is locked
                if (RasterSupport.IsLocked(RasterSupportType.Bitonal))
                {
                    _useAntiAliasing = false;
                }
                else
                {
                    // Turn on anti-aliasing
                    RasterPaintProperties properties = base.PaintProperties;
                    properties.PaintDisplayMode |= RasterPaintDisplayModeFlags.ScaleToGray;
                    base.PaintProperties = properties;
                }

                // Set the fit mode
                SetFitMode(RegistryManager.FitMode, false, false);

                // Store the original transformation matrix
                _transform = base.Transform;

                // Handle layer object remove events
                _layerObjects.LayerObjectDeleted += HandleLayerObjectDeleted;
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21138", "Unable to initialize image viewer.", e);
            }
        }

        #endregion

        #region Image Viewer Properties

        /// <summary>
        /// Gets the name of the currently open image file.
        /// </summary>
        /// <returns>The name of the currently open image file. If no image is open, returns the 
        /// empty string.</returns>
        [Browsable(false)]
        public string ImageFile
        {
            get
            {
                return _imageFile;
            }
        }

        /// <summary>
        /// Gets or sets the currently active cursor tool.
        /// </summary>
        /// <value>The new cursor tool.</value>
        /// <returns>The currently active cursor tool. The default is 
        /// <see cref="Extract.Imaging.Forms.CursorTool.None"/> if no image is open or 
        /// <see cref="Extract.Imaging.Forms.CursorTool.ZoomWindow"/> if an image is open.</returns>
        /// <event cref="CursorToolChanged">Raised when the property is set.</event>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CursorTool CursorTool
        {
            get
            {
                return _cursorTool;
            }
            set
            {
                try
                {
                    // Ensure an image is open
                    ExtractException.Assert("ELI21321", "Image must be open.",
                        base.Image != null || value == CursorTool.None);

                    switch (value)
                    {
                        case CursorTool.ZoomWindow:

                            // Allow the user to define a region to zoom to using drag and drop
                            base.InteractiveMode = RasterViewerInteractiveMode.ZoomTo;

                            // Set the region combine mode
                            base.InteractiveRegionCombineMode = RasterRegionCombineMode.Set;

                            // Zoom to a rectangular region
                            base.InteractiveRegionType =
                                RasterViewerInteractiveRegionType.Rectangle;

                            // Allow for the animation of the rectangular region 
                            base.AnimateRegion = true;

                            // Clear selections if necessary
                            if (_cursorTool == CursorTool.SelectLayerObject)
                            {
                                _layerObjects.Selection.Clear();
                                Invalidate();
                            }

                            // Store this as the last active continuous use cursor tool
                            _lastContinuousUseTool = value;
                            break;

                        case CursorTool.Pan:

                            // Allow the user to pan the image using drag and drop
                            base.InteractiveMode = RasterViewerInteractiveMode.Pan;

                            // Clear selections if necessary
                            if (_cursorTool == CursorTool.SelectLayerObject)
                            {
                                _layerObjects.Selection.Clear();
                                Invalidate();
                            }

                            // Store this as the last active continuous use cursor tool
                            _lastContinuousUseTool = value;
                            break;

                        case CursorTool.DeleteLayerObjects:
                        case CursorTool.SetHighlightHeight:

                            // Turn off interactive mode
                            base.InteractiveMode = RasterViewerInteractiveMode.None;

                            // Clear selections if necessary
                            if (_cursorTool == CursorTool.SelectLayerObject)
                            {
                                _layerObjects.Selection.Clear();
                                Invalidate();
                            }
                            break;

                        case CursorTool.None:
                        case CursorTool.AngularHighlight:
                        case CursorTool.AngularRedaction:
                        case CursorTool.EditHighlightText:
                        case CursorTool.RectangularHighlight:
                        case CursorTool.RectangularRedaction:

                            // Turn off interactive mode
                            base.InteractiveMode = RasterViewerInteractiveMode.None;

                            // Clear selections if necessary
                            if (_cursorTool == CursorTool.SelectLayerObject)
                            {
                                _layerObjects.Selection.Clear();
                                Invalidate();
                            }

                            // Store this as the last active continuous use cursor tool
                            _lastContinuousUseTool = value;
                            break;

                        case CursorTool.SelectLayerObject:

                            // Turn off interactive mode
                            base.InteractiveMode = RasterViewerInteractiveMode.None;

                            // Store this as the last active continuous use cursor tool
                            _lastContinuousUseTool = value;
                            break;

                        default:

                            throw new ExtractException("ELI21233",
                                "Unrecognized CursorTool value.");
                    }

                    // Disable the active linked layer object unless using select layer object
                    if (value != CursorTool.SelectLayerObject)
                    {
                        _activeLinkedLayerObject = null;

                        // If the cursor tool is no longer the selection tool, remove all entries
                        // from _layerObjectsUnderSelectionTool (and raise the 
                        // SelectionToolLeftLayerObject for each)
                        UpdateLayerObjectsUnderSelectionTool(null);
                    }

                    // Set the cursor tool
                    _cursorTool = value;

                    // Get the cursor values for the current tool
                    ImageViewerCursors cursors = _cursorsForCursorTools[value];

                    // Set the cursor values for the current tool
                    _toolCursor = cursors.Tool;
                    _activeCursor = cursors.Active;

                    // Set the current cursor
                    Cursor = _toolCursor ?? Cursors.Default;

                    // If the cursor tool has changed to one of the highlight tools then
                    // set the appropriate selection tool value in the registry
                    if (_cursorTool == CursorTool.AngularHighlight ||
                        _cursorTool == CursorTool.RectangularHighlight ||
                        _cursorTool == CursorTool.AngularRedaction ||
                        _cursorTool == CursorTool.RectangularRedaction)
                    {
                        // Store the current selection tool in the registry
                        RegistryManager.SetLastUsedSelectionTool(_cursorTool);
                    }

                    OnCursorToolChanged(new CursorToolChangedEventArgs(_cursorTool));
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21139",
                        "Unable to set cursor tool.", e);
                    ee.AddDebugData("Cursor tool", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets or sets the currently active fit mode.
        /// </summary>
        /// <value>The new fit mode.</value>
        /// <returns>The currently active fit mode. The default is 
        /// <see cref="Extract.Imaging.Forms.FitMode.None"/>.</returns>
        /// <event cref="ZoomChanged">The property has been set and an image is open.</event>
        /// <event cref="FitModeChanged">The property has been set.</event>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FitMode FitMode
        {
            get
            {
                return _fitMode;
            }
            set
            {
                try
                {
                    // Set the fit mode and update the zoom
                    SetFitMode(value, true, true);
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21140",
                        "Unable to set fit mode.", e);
                    ee.AddDebugData("Fit mode", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to display annotations.
        /// </summary>
        /// <value><see langword="true"/> if annotations should be displayed; 
        /// <see langword="false"/> if annotations should not be displayed.</value>
        /// <returns><see langword="true"/> if annotations should be displayed; 
        /// <see langword="false"/> if annotations should not be displayed. The default is 
        /// <see langword="true"/>.</returns>
        [DefaultValue(true)]
        public bool DisplayAnnotations
        {
            get
            {
                return _displayAnnotations;
            }
            set
            {
                try
                {
                    if (value)
                    {
                        // If this is design-time, do not check licensing, but also do not unlock
                        // LeadTools support
                        if (!DesignMode)
                        {
                            _licenseAnnotation.Validate("ELI21920");
                        }

                        // Set display annotations to true
                        _displayAnnotations = true;

                        // If an image is open and annotations haven't been loaded yet, 
                        // load the annotations for the current image
                        if (base.Image != null && _annotations == null)
                        {
                            UpdateAnnotations();
                        }
                    }
                    else
                    {
                        if (_annotations != null)
                        {
                            // Annotations no longer need to be displayed. Dispose of them.
                            _annotations.Dispose();
                            _annotations = null;
                        }

                        // Set display annotations to false
                        _displayAnnotations = false;
                    }
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21141",
                        "Unable to set display annotations option.", e);
                    ee.AddDebugData("Display annotations", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to display image with anti-aliasing.
        /// </summary>
        /// <value><see langword="true"/> if image should be displayed with anti-aliasing; 
        /// <see langword="false"/> if image should be displayed without anti-aliasing.</value>
        /// <returns><see langword="true"/> if image should be displayed with anti-aliasing; 
        /// <see langword="false"/> if image should be displayed without anti-aliasing. The 
        /// default is <see langword="true"/>.</returns>
        /// <exception cref="ExtractException"><paramref name="value"/> is <see langword="true"/> 
        /// and anti-aliasing is not properly licensed.</exception>
        [DefaultValue(true)]
        public bool UseAntiAliasing
        {
            get
            {
                return _useAntiAliasing;
            }
            set
            {
                try
                {
                    if (value)
                    {
                        // If this is design-time, do not check licensing, but also do not unlock
                        // LeadTools support
                        if (!DesignMode)
                        {
                            _licenseAntiAlias.Validate("ELI21921");

                            // Turn on anti-aliasing
                            RasterPaintProperties properties = base.PaintProperties;
                            properties.PaintDisplayMode |= RasterPaintDisplayModeFlags.ScaleToGray;
                            base.PaintProperties = properties;
                        }
                    }
                    else if (_useAntiAliasing)
                    {
                        // Turn off anti-aliasing
                        RasterPaintProperties properties = base.PaintProperties;
                        properties.PaintDisplayMode &= ~RasterPaintDisplayModeFlags.ScaleToGray;
                        base.PaintProperties = properties;
                    }

                    _useAntiAliasing = value;
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI21268",
                        "Unable to set anti-aliasing", ex);
                    ee.AddDebugData("Use anti-aliasing", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets whether the current zoom region is at the top left of the first page.
        /// </summary>
        /// <value><see langword="true"/> if the current zoom region is at the top left of the 
        /// first page; <see langword="false"/> if it is not.</value>
        public bool IsFirstTile
        {
            get
            {
                // The first page must be visible for this to be the first zone
                if (base.Image == null || _pageNumber != 1)
                {
                    return false;
                }

                // Get the visible image area
                Rectangle tileArea = GetVisibleImageArea();

                // Get the viewing rectangle in physical (client) coordinates
                Rectangle imageArea = base.PhysicalViewRectangle;

                // Determine whether the bottom right point of 
                // tile area is within the margin of error.
                return (tileArea.Left - imageArea.Left) < _TILE_EDGE_DISTANCE &&
                    (tileArea.Top - imageArea.Top) < _TILE_EDGE_DISTANCE;
            }
        }

        /// <summary>
        /// Gets whether the current zoom region is at the bottom right of the last page.
        /// </summary>
        /// <returns><see langword="true"/> if the current zoom region is at the bottom right of the 
        /// last page; <see langword="false"/> if it is not.</returns>
        public bool IsLastTile
        {
            get
            {
                // The last page must be visible for this to be the last zone
                if (base.Image == null || _pageNumber != _pageCount)
                {
                    return false;
                }

                // Get the visible image area
                Rectangle tileArea = GetVisibleImageArea();

                // Get the viewing rectangle in physical (client) coordinates
                Rectangle imageArea = base.PhysicalViewRectangle;

                // Determine whether the bottom right point of 
                // tile area is within the margin of error.
                return (imageArea.Right - tileArea.Right) < _TILE_EDGE_DISTANCE &&
                    (imageArea.Bottom - tileArea.Bottom) < _TILE_EDGE_DISTANCE;
            }
        }

        /// <summary>
        /// Gets whether there is a next <see cref="LayerObject"/> to goto.
        /// </summary>
        /// <returns><see langword="true"/> if there is a next <see cref="LayerObject"/>;
        /// <see langword="false"/> if there is not.</returns>
        [Browsable(false)]
        public bool CanGoToNextLayerObject
        {
            get
            {
                return base.Image != null && GetNextVisibleLayerObjectIndex() != -1;
            }
        }

        /// <summary>
        /// Gets whether there is a previous <see cref="LayerObject"/> to goto.
        /// </summary>
        /// <returns><see langword="true"/> if there is a previous <see cref="LayerObject"/>;
        /// <see langword="false"/> if there is not.</returns>
        [Browsable(false)]
        public bool CanGoToPreviousLayerObject
        {
            get
            {
                return base.Image != null && GetPreviousVisibleLayerObjectIndex() != -1;
            }
        }

        /// <summary>
        /// Gets whether there is a previous entry in the current zoom history.
        /// </summary>
        /// <returns><see langword="true"/> if a previous entry exists; <see langword="false"/> if 
        /// a previous entry does not exist.</returns>
        [Browsable(false)]
        public bool CanZoomPrevious
        {
            get
            {
                return base.Image != null && _imagePages[_pageNumber - 1].CanZoomPrevious;
            }
        }

        /// <summary>
        /// Gets whether there is a subsequent entry in the current zoom history.
        /// </summary>
        /// <returns><see langword="true"/> if a subsequent entry exists; <see langword="false"/> 
        /// if a subsequent entry does not exist.</returns>
        [Browsable(false)]
        public bool CanZoomNext
        {
            get
            {
                return base.Image != null && _imagePages[_pageNumber - 1].CanZoomNext;
            }
        }

        /// <summary>
        /// <para>Gets or sets the current zoom setting.</para>
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// </summary>
        /// <value>The zoom setting to set.</value>
        /// <returns>The current zoom setting.</returns>
        /// <exception cref="ExtractException">No image is open.</exception>
        /// <event cref="ZoomChanged">The property has been set.</event>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ZoomInfo ZoomInfo
        {
            // Always throw ExtractException as per Extract Coding Guidelines
            [SuppressMessage("Microsoft.Design",
                "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            get
            {
                try
                {
                    // Ensure an image has been opened
                    ExtractException.Assert("ELI21205", "No image is open.", base.Image != null);

                    return _imagePages[_pageNumber - 1].ZoomInfo;
                }
                catch (Exception e)
                {
                    throw new ExtractException("ELI21206", "Unable to get zoom info.", e);
                }
            }
            set
            {
                try
                {
                    // Set the zoom info and update the zoom history
                    SetZoomInfo(value, true);
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21142",
                        "Unable to set current zoom.", e);
                    ee.AddDebugData("Zoom info", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets the number of zoom history entries for the currently displayed page.
        /// <para><b>Requirements</b></para>
        /// An image must be open.
        /// </summary>
        /// <value>The number of zoom history entries for the currently displayed page.</value>
        /// <exception cref="ExtractException">No image is open.</exception>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ZoomHistoryCount
        {
            // Always throw ExtractException as per Extract Coding Guidelines
            [SuppressMessage("Microsoft.Design",
                "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            get
            {
                try
                {
                    // Ensure an image has been opened
                    ExtractException.Assert("ELI21916", "No image is open.", base.Image != null);

                    // Return the zoom history count for the current page
                    return _imagePages[_pageNumber - 1].ZoomHistoryCount;
                }
                catch (Exception ex)
                {
                    throw new ExtractException("ELI21917", "Unable to get zoom history count.", ex);
                }
            }
        }

        /// <summary>
        /// The currently visible one-based page number.
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// </summary>
        /// <value>The one-based page number to go to.</value>
        /// <returns>The currently visible one-based page number.</returns>
        /// <event cref="PageChanged">The property has been set.</event>
        /// <event cref="ZoomChanged">The property has been set.</event>
        /// <exception cref="ExtractException">No image is open.</exception>
        /// <exception cref="ExtractException"><paramref name="value"/> is not a valid page.
        /// </exception>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PageNumber
        {
            // Always throw ExtractException as per Extract Coding Guidelines
            [SuppressMessage("Microsoft.Design",
                "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            get
            {
                try
                {
                    // Ensure an image has been opened
                    ExtractException.Assert("ELI21143", "No image is open.", base.Image != null);

                    // Return the current page number
                    return _pageNumber;
                }
                catch (Exception e)
                {
                    throw new ExtractException("ELI21144",
                        "Unable to get page number.", e);
                }
            }
            set
            {
                try
                {
                    SetPageNumber(value, true, true);
                }
                catch (Exception e)
                {
                    throw new ExtractException("ELI21146",
                        "Unable to set page number.", e);
                }
            }
        }

        /// <summary>
        /// Sets the current page.
        /// </summary>
        /// <param name="pageNumber">The one-based page number to be visible.</param>
        /// <param name="updateZoom">Whether to update the zoom setting.</param>
        /// <param name="raisePageChanged">Whether to raise the page changed event.</param>
        public void SetPageNumber(int pageNumber, bool updateZoom, bool raisePageChanged)
        {
            try
            {
                // Ensure an image has been opened
                ExtractException.Assert("ELI21145", "No image is open.", base.Image != null);

                // Ensure the page number is valid
                ExtractException.Assert("ELI21786", "Invalid page number.",
                    pageNumber >= 1 && pageNumber <= _pageCount);
                
                // Prepare variable for whether the zoom history should be stored
                bool updateZoomHistory = false;

                // Don't update the image viewer until all changes have been made
                base.BeginUpdate();
                try
                {
                    // Go to the specified page
                    _pageNumber = pageNumber;
                    base.Image = GetPage(pageNumber);

                    // Check whether the zoom setting should be updated, too.
                    if (updateZoom)
                    {
                        // Check if this is the first time this page has been viewed
                        if (_imagePages[pageNumber - 1].ZoomHistoryCount == 0)
                        {
                            // Update the zoom history
                            updateZoomHistory = true;

                            // If this is the first time the page is viewed, display the whole page
                            // [DotNetRCAndUtils #102]
                            if (_fitMode == FitMode.None)
                            {
                                ShowFitToPage();
                            }
                            else if (_fitMode == FitMode.FitToWidth)
                            {
                                // This is the first time the page is viewed, show the top of the page
                                // [DotNetRCAndUtils #202]
                                base.ScrollPosition = Point.Empty;
                            }
                        }
                        else
                        {
                            // Get the previous zoom history entry
                            ZoomInfo zoomInfo = _imagePages[pageNumber - 1].ZoomInfo;

                            // This page has been viewed before, restore the previous zoom setting
                            // [DotNetRCAndUtils #107]
                            if (_fitMode == FitMode.None)
                            {
                                switch (zoomInfo.FitMode)
                                {
                                    case FitMode.None:
                                        SetZoomInfo(zoomInfo, false);
                                        break;

                                    case FitMode.FitToPage:

                                        ShowFitToPage();
                                        break;

                                    case FitMode.FitToWidth:

                                        ShowFitToWidth();
                                        break;

                                    default:
                                        throw new ExtractException("ELI21787",
                                            "Unexpected fit mode.");
                                }
                            }
                            else if (_fitMode != zoomInfo.FitMode)
                            {
                                // The previous zoom entry had a different zoom history. 
                                // Update the zoom history.
                                updateZoomHistory = true;
                            }
                        }
                    }

                    // Update the annotations
                    UpdateAnnotations();
                }
                finally
                {
                    base.EndUpdate();
                }

                if (raisePageChanged)
                {
                    // Raise the page changed event
                    OnPageChanged(new PageChangedEventArgs(pageNumber));
                }

                // Raise the zoom changed event AND
                // update the zoom history if specified.
                UpdateZoom(updateZoomHistory, updateZoom);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI23309",
                    "Unable to set page number.", ex);
                ee.AddDebugData("Page number", pageNumber, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets an image for the specified page of the currently open image.
        /// </summary>
        /// <param name="pageNumber">The 1-based page number to open.</param>
        /// <returns>The image with <paramref name="pageNumber"/> from the currently open image.
        /// </returns>
        RasterImage GetPage(int pageNumber)
        {
            RasterImage image = null;
            try
            {
                image = _codecs.Load(_currentOpenFile, 0, CodecsLoadByteOrder.BgrOrGray, 
                    pageNumber, pageNumber);

                int rotation = _imagePages[pageNumber - 1].Orientation;
                RotateImageByDegrees(image, rotation);
            }
            catch (Exception)
            {
                if (image != null)
                {
                    image.Dispose();
                }                
                throw;
            }

            return image;
        }

        /// <summary>
        /// Displays the current page fit exactly within the <see cref="ImageViewer"/>.
        /// </summary>
        /// <remarks>This method does not change the fit mode.</remarks>
        void ShowFitToPage()
        {
            base.SizeMode = RasterPaintSizeMode.FitAlways;
            base.ScaleFactor = _DEFAULT_SCALE_FACTOR;
            base.ZoomToRectangle(base.DisplayRectangle);
        }

        /// <summary>
        /// Displays the width of the current page fit exactly within the 
        /// <see cref="ImageViewer"/>.
        /// </summary>
        /// <remarks>This method does not change the fit mode.</remarks>
        void ShowFitToWidth()
        {
            base.SizeMode = RasterPaintSizeMode.FitWidth;
            base.ScaleFactor = _DEFAULT_SCALE_FACTOR;
            base.ZoomToRectangle(base.DisplayRectangle);
        }


        /// <summary>
        /// Gets the number of pages in currently open file.
        /// </summary>
        /// <returns>The number of pages in currently open file. Will be zero if no image is open.
        /// </returns>
        [Browsable(false)]
        public int PageCount
        {
            get
            {
                return base.Image == null ? 0 : _pageCount;
            }
        }

        /// <summary>
        /// Gets the active collection of shortcut keys and shortcut handlers.
        /// </summary>
        /// <remarks>There are two <see cref="ShortcutsManager"/>. One is used during interactive 
        /// mouse events (e.g. creating a highlight); the other is used the rest of the time. Use 
        /// <see cref="Control.Capture"/> to determine if an interactive mouse event is occurring.
        /// </remarks>
        /// <returns>The active collection of shortcut keys and shortcut handlers.</returns>
        [Browsable(false)]
        public ShortcutsManager Shortcuts
        {
            get
            {
                return Capture ? _captureShortcuts : _mainShortcuts;
            }
        }

        // TODO: make this design-time visible only
        /// <summary>
        /// Gets or sets whether to load default shortcut keys.
        /// </summary>
        /// <value><see langword="true"/> if default shortcuts should be loaded; 
        /// <see langword="false"/> if default shortcuts should be cleared.</value>
        /// <returns><see langword="true"/> if default shortcuts were loaded; 
        /// <see langword="false"/> if default shortcuts were cleared. The default is 
        /// <see langword="false"/>.</returns>
        [DefaultValue(false)]
        public bool UseDefaultShortcuts
        {
            get
            {
                return _useDefaultShortcuts;
            }
            set
            {
                try
                {
                    // Clear the shortcuts
                    _mainShortcuts.Clear();
                    _captureShortcuts.Clear();

                    // Load default shortcuts if requested
                    if (value)
                    {
                        LoadDefaultShortcuts();
                    }

                    // Store whether default shortcuts were loaded
                    _useDefaultShortcuts = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26538", ex);
                }
            }
        }

        /// <summary>
        /// Gets the collection of highlights.
        /// </summary>
        /// <returns>The collection of highlights.</returns>
        [Browsable(false)]
        public LayerObjectsCollection LayerObjects
        {
            get
            {
                return _layerObjects;
            }
        }

        /// <summary>
        /// Gets or sets the default highlight color for newly created highlights.
        /// </summary>
        /// <value>The default highlight color for newly created highlights.</value>
        /// <returns>The default highlight color for newly created highlights. The initial value 
        /// is <see cref="Color.Yellow"/>.</returns>
        [DefaultValue(typeof(Color), "Yellow")]
        public Color DefaultHighlightColor
        {
            get
            {
                return _defaultHighlightColor;
            }
            set
            {
                _defaultHighlightColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the default fill color for newly created redactions.
        /// </summary>
        /// <value>The default fill color for newly created redactions.</value>
        /// <returns>The default fill color for newly created redactions. The initial value 
        /// is <see cref="Color.DimGray"/>.</returns>
        [DefaultValue(RedactionColor.Black)]
        public RedactionColor DefaultRedactionFillColor
        {
            get
            {
                return _defaultRedactionFillColor;
            }
            set
            {
                _defaultRedactionFillColor = value;

                // Update the redaction paint color when updating the fill color
                _defaultRedactionPaintColor =
                    value == RedactionColor.Black ? Redaction.BlackPaint : Redaction.WhitePaint;
            }
        }

        /// <summary>
        /// Gets or sets the default highlight height for newly created highlights in logical 
        /// (image) pixels.
        /// </summary>
        /// <value>The default highlight height for newly created highlights in logical (image) 
        /// pixels.</value>
        /// <returns>The default highlight height for newly created highlights in logical (image) 
        /// pixels. The initial value is 40.</returns>
        [DefaultValue(40)]
        public int DefaultHighlightHeight
        {
            get
            {
                return _defaultHighlightHeight;
            }
            set
            {
                _defaultHighlightHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to recognize text for newly created highlights.
        /// </summary>
        /// <value><see langword="true"/> if a newly created highlight's text should be  
        /// recognized text from the highlight area; <see langword="false"/> if newly created 
        /// highlight's text should be the empty string.</value>
        /// <returns>The default highlight height for newly created highlights. The initial value 
        /// is <see langword="true"/>.</returns>
        [DefaultValue(true)]
        public bool RecognizeHighlightText
        {
            get
            {
                return _recognizeHighlightText;
            }
            set
            {
                _recognizeHighlightText = value;
            }
        }

        /// <summary>
        /// Gets or sets the visible rotation in degrees from the original image of the current 
        /// page.
        /// <para><b>Requirements</b></para>
        /// <para>An image must be open.</para>
        /// </summary>
        /// <value>The visible rotation in degrees from the original image of the current 
        /// page. Must be a multiple of 90.</value>
        /// <returns>The visible rotation in degrees from the original image of the current 
        /// page. Is always either 0, 90, 180, or 270. The default is zero.</returns>
        /// <event cref="OrientationChanged">Raised when the property is set.</event>
        /// <exception cref="ExtractException">No image is open.</exception>
        /// <exception cref="ExtractException"><paramref name="value"/> is not a multiple of 90.
        /// </exception>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Orientation
        {
            // Always throw ExtractException as per Extract Coding Guidelines
            [SuppressMessage("Microsoft.Design",
                "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            get
            {
                // Ensure the image is not null
                ExtractException.Assert("ELI21220", "Cannot get orientation. No image is open.",
                    base.Image != null);

                return _imagePages[_pageNumber - 1].Orientation;
            }
            set
            {
                try
                {
                    // Ensure the image is not null
                    ExtractException.Assert("ELI21221", "No image is open.", base.Image != null);

                    // Ensure the orientation is a multiple of 90.
                    ExtractException.Assert("ELI21100", "Orientation must be a multiple of 90.",
                        value % 90 == 0);

                    // Rotate the orientation
                    Rotate(_imagePages[_pageNumber - 1].Orientation - value);
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21222",
                        "Unable to set orientation.", e);
                    ee.AddDebugData("Orientation", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets the 3x3 affine transformation matrix that translates logical (original image) 
        /// coordinates into physical (client) coordinates.
        /// </summary>
        /// <returns>The 3x3 affine transformation matrix that translates logical (original image) 
        /// coordinates into physical (client) coordinates.</returns>
        /// <exception cref="ExtractException">No image is open.</exception>
        [Browsable(false)]
        public new Matrix Transform
        {
            get
            {
                // Ensure an image is open
                ExtractException.Assert("ELI21412",
                    "Cannot get transformation matrix. No image is open.", base.Image != null);

                return _transform;
            }
        }

        /// <summary>
        /// Gets the height of the image in logical (image) pixels.
        /// </summary>
        /// <value>The height of the image in logical (image) pixels.</value>
        /// <exception cref="ExtractException">No image is open.</exception>
        [Browsable(false)]
        public int ImageHeight
        {
            get
            {
                // Ensure an image is open
                ExtractException.Assert("ELI21503", "No image is open.", base.Image != null);

                return _imagePages[_pageNumber - 1].Orientation % 180 == 0 ?
                    base.Image.ImageHeight : base.Image.ImageWidth;
            }
        }

        /// <summary>
        /// Gets the width of the image in logical (image) pixels.
        /// </summary>
        /// <value>The width of the image in logical (image) pixels.</value>
        /// <exception cref="ExtractException">No image is open.</exception>
        [Browsable(false)]
        public int ImageWidth
        {
            get
            {
                // Ensure an image is open
                ExtractException.Assert("ELI21504", "No image is open.", base.Image != null);

                return _imagePages[_pageNumber - 1].Orientation % 180 == 0 ?
                    base.Image.ImageWidth : base.Image.ImageHeight;
            }
        }

        /// <summary>
        /// Gets or sets the text of the watermark to apply to saved and printed images.
        /// </summary>
        /// <value>The text of the watermark to apply to saved and printed images or 
        /// <see langword="null"/> if no watermark should be applied.</value>
        /// <returns>The text of the watermark to apply to saved and printed images or 
        /// <see langword="null"/> if no watermark should be applied.</returns>
        [DefaultValue(null)]
        public string Watermark
        {
            get
            {
                return _watermark;
            }
            set
            {
                _watermark = value;
            }
        }

        /// <summary>
        /// Gets the list of strings containing the file types that will be listed in the
        /// open file dialog file type drop down list.
        /// </summary>
        /// <example> Add a file type to the filter list
        /// <code lang="C#">
        /// // Create a new image viewer
        /// ImageViewer imageViewer = new ImageViewer();
        /// 
        /// // Add Post script to file type to the beginning of the list
        /// imageViewer.OpenImageFileTypeFilterList.Insert(0, "Post script (*.ps)|*.ps|");
        /// 
        /// // Adjust index to point to Post script (this index is 1 based)
        /// imageViewer.OpenImageFileTypeFilterIndex = 1;
        /// </code>
        /// </example> 
        /// <returns>The list of file types that will be displayed in the
        /// open file dialog file type drop down list.</returns>
        // We are exposing the generic list here because the caller needs to have access to the
        // underlying list so that modifications can be made to it and the changes will be
        // reflected in the image viewer
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<string> OpenImageFileTypeFilterList
        {
            get
            {
                return _openImageFileTypeFilter;
            }
        }

        /// <summary>
        /// Gets and sets the index for the file filter that will be displayed by default
        /// in the open file dialog box.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        // Just setting the filter index value, nothing complex, should not throw an exception 
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public int OpenImageFileTypeFilterIndex
        {
            get
            {
                return _openImageFilterIndex;
            }
            set
            {
                ExtractException.Assert("ELI23245",
                    "Filter index must be valid index for file type filter list.",
                    value <= _openImageFileTypeFilter.Count, "Value", value,
                    "List length", _openImageFileTypeFilter.Count);

                _openImageFilterIndex = value;
            }
        }

        /// <summary>
        /// Gets or sets the last used printer name.
        /// </summary>
        /// <value>The name of the last used printer.</value>
        /// <returns>The name of the last used printer.</returns>
        string LastPrinter
        {
            get
            {
                string lastPrinter = RegistryManager.GetLastUsedPrinter();

                if (_disallowedPrinters.Contains(lastPrinter.ToUpperInvariant()))
                {
                    lastPrinter = "";
                }

                return lastPrinter;
            }
            set
            {
                RegistryManager.SetLastUsedPrinter(value);
            }
        }

        /// <summary>
        /// Gets or sets the default status message that should be displayed when no image is 
        /// loaded.</summary>
        /// <value>The default status message that should be displayed when no image is loaded.
        /// </value>
        /// <returns>The default status message that should be displayed when no image is loaded.
        /// </returns>
        [DefaultValue("")]
        public string DefaultStatusMessage
        {
            get
            {
                return _defaultStatusMessage;
            }
            set
            {
                _defaultStatusMessage = value ?? "";
            }
        }

        /// <summary>
        /// Gets or sets whether selection of multiple <see cref="LayerObject"/>s via a banded box
        /// is enabled.
        /// </summary>
        /// <value><see langword="true"/> to enable selecting multiple objects via banding,
        /// <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if selecting multiple objects via banding is enabled,
        /// <see langword="false"/> otherwise.
        /// </returns>
        [DefaultValue(true)]
        public bool AllowBandedSelection
        {
            get
            {
                return _allowBandedSelection;
            }

            set
            {
                _allowBandedSelection = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ContextMenuStrip"/> the <see cref="ImageViewer"/> is to use.
        /// </summary>
        /// <value>The <see cref="ContextMenuStrip"/> to be used.</value>
        /// <returns>The <see cref="ContextMenuStrip"/> being used.</returns>
        public override ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return base.ContextMenuStrip;
            }
            set
            {
                try
                {
                    if (base.ContextMenuStrip != value)
                    {
                        if (base.ContextMenuStrip != null)
                        {
                            base.ContextMenuStrip.Opening -= HandleContextMenuStripOpening;
                        }

                        base.ContextMenuStrip = value;

                        if (base.ContextMenuStrip != null)
                        {
                            base.ContextMenuStrip.Opening += HandleContextMenuStripOpening;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27115", ex);
                }
            }
        }

        #endregion

        #region Image Viewer OnEvents

        /// <summary>
        /// Raises the <see cref="Control.OnKeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                base.OnKeyDown(e);

                if (_trackingData == null && _cursorTool == CursorTool.SelectLayerObject)
                {
                    // Upate the mouse cursor
                    base.Cursor = GetSelectionCursor(PointToClient(MousePosition));
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21617", ex);
                ee.AddDebugData("Key event", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.OnKeyUp"/> event.
        /// </summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            try
            {
                base.OnKeyUp(e);

                if (_trackingData == null && _cursorTool == CursorTool.SelectLayerObject)
                {
                    // Update the mouse cursor
                    base.Cursor = GetSelectionCursor(PointToClient(MousePosition));
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21618", ex);
                ee.AddDebugData("Key event", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        // This event handler has undergone a security review.
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers",
            MessageId = "0#")]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void OnMouseDown(MouseEventArgs e)
        {
            try
            {
                base.OnMouseDown(e);

                // Ensure an image is open
                if (base.Image != null)
                {
                    // Start tracking a mouse event if the left mouse button was clicked
                    if (e.Button == MouseButtons.Left)
                    {
                        // Activate the mouse cursor
                        if (_activeCursor != null)
                        {
                            Cursor = _activeCursor;
                        }

                        StartTracking(e.X, e.Y);

                        // Disable the active linked layer object 
                        // during interactive tracking events
                        if (_trackingData != null)
                        {
                            _activeLinkedLayerObject = null;
                        }
                    }
                    else if (_trackingData != null)
                    {
                        // Some other mouse button was clicked, perhaps the right mouse button.
                        // Cancel the tracking event.
                        EndTracking(0, 0, true);

                        // If the right mouse button was used to cancel a tracking event, don't
                        // allow it to show the context menu.
                        if (e.Button == MouseButtons.Right && base.ContextMenuStrip != null)
                        {
                            _suppressContextMenu = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21322", ex);
                ee.AddDebugData("Mouse event", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseMove"/> event.
        /// </summary>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        // This event handler has undergone a security review.
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers",
            MessageId = "0#")]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void OnMouseMove(MouseEventArgs e)
        {
            try
            {
                base.OnMouseMove(e);

                // Update _layerObjectsUnderSelectionTool contents and raise the 
                // SelectionToolEnteredLayerObject and SelectionToolLeftLayerObject events as
                // necessary.
                UpdateLayerObjectsUnderSelectionTool(e);

                // Process this mouse event if an interactive region is being created
                if (_trackingData != null && e.Button == MouseButtons.Left)
                {
                    // Update the tracking event
                    UpdateTracking(e.X, e.Y);
                }
                else if (IsImageAvailable && _cursorTool == CursorTool.SelectLayerObject)
                {
                    UpdateActiveLinkedLayerObject(e.X, e.Y);

                    Cursor cursor = GetSelectionCursor(e.X, e.Y);

                    // Set the cursor
                    base.Cursor = cursor;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21353", ex);
                ee.AddDebugData("Mouse event", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseLeave"/> event.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            // If the mouse has left the image viewer, remove all entries from 
            // _layerObjectsUnderSelectionTool (and raise the SelectionToolLeftLayerObject for each)
            UpdateLayerObjectsUnderSelectionTool(null);
        }

        /// <summary>
        /// Handles the <see cref="Control.LostFocus"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            try
            {
                base.OnLostFocus(e);

                if (_trackingData != null)
                {
                    // If the image viewer has lost focus during a tracking event, cancel the event.
                    EndTracking(0, 0, true);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27111", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Updates the _layerObjectsUnderSelectionTool and raises the
        /// <see cref="SelectionToolEnteredLayerObject"/> and 
        /// <see cref="SelectionToolLeftLayerObject"/> events as appropriate.
        /// </summary>
        /// <param name="e">If not <see langword="null"/>, contains a <see cref="MouseEventArgs"/>
        /// that contains infomation about the mouse event that triggered this call. If
        /// <see langword="null"/>, <see cref="SelectionToolLeftLayerObject"/> will be raised for
        /// all <see cref="LayerObject"/>s previously under the selection tool.</param>
        void UpdateLayerObjectsUnderSelectionTool(MouseEventArgs e)
        {
            // Determine whether the method should look for any layer objects under the selection
            // tool. (If false, all current _layerObjectsUnderSelectionTool members will be removed).
            bool lookForLayerObjectsUnderSelectionTool =
                (e != null && _cursorTool == CursorTool.SelectLayerObject && IsImageAvailable);

            // If looking for new layer objects, initialize _layerObjectsUnderSelectionTool if
            // necessary.
            if (lookForLayerObjectsUnderSelectionTool && _layerObjectsUnderSelectionTool == null)
            {
                _layerObjectsUnderSelectionTool = new List<LayerObject>();
            }

            // Initialize a list of layer objects no longer under the selection tool.
            List<LayerObject> removedLayerObjects = new List<LayerObject>();

            // Initialize a list of new layer objects under the selection tool.
            List<LayerObject> newLayerObjects = new List<LayerObject>();

            Point imagePoint = new Point();
            if (lookForLayerObjectsUnderSelectionTool)
            {
                // Convert the physical point to image coordinates
                Point[] transformPoint = new Point[] { e.Location };
                using (Matrix clientToImage = _transform.Clone())
                {
                    clientToImage.Invert();
                    clientToImage.TransformPoints(transformPoint);

                    // Obtain the mouse position in image coordinates.
                    imagePoint = transformPoint[0];
                }
            }

            // For all current members of _layerObjectsUnderSelectionTool, ensure they are still
            // under the selection tool.
            if (_layerObjectsUnderSelectionTool != null)
            {
                for (int i = 0; i < _layerObjectsUnderSelectionTool.Count; i++)
                {
                    LayerObject layerObject = _layerObjectsUnderSelectionTool[0];

                    // Remove from _layerObjectsUnderSelectionTool if we are not looking for layer 
                    // objects, the layer object is no longer in the image viewer or the layer object
                    // is no longer found under the selection tool.
                    if (!lookForLayerObjectsUnderSelectionTool ||
                        !_layerObjects.Contains(layerObject) ||
                        layerObject.PageNumber != _pageNumber ||
                        !layerObject.HitTest(imagePoint))
                    {
                        removedLayerObjects.Add(layerObject);
                        _layerObjectsUnderSelectionTool.RemoveAt(i);
                        i--;
                    }
                }
            }

            // If searching for layer objects, try to find any underneath the selection tool.
            if (lookForLayerObjectsUnderSelectionTool)
            {
                // Iterate through all layer objects
                foreach (LayerObject layerObject in _layerObjects)
                {
                    // Skip this layer object if it is on a different page
                    if (layerObject.PageNumber != _pageNumber)
                    {
                        continue;
                    }

                    // Check if the mouse cursor is over a layer object and that 
                    // _layerObjectsUnderSelectionTool doesn't already contain the layer object.
                    if (layerObject.HitTest(imagePoint) &&
                        !_layerObjectsUnderSelectionTool.Contains(layerObject))
                    {
                        newLayerObjects.Add(layerObject);
                    }
                }
            }
            else
            {
                // If not looking for layer objects under the cursor, un-initialize 
                // _layerObjectsUnderSelectionTool after all event have been raised.
                _layerObjectsUnderSelectionTool = null;
            }

            // Raise the SelectionToolLeftLayerObject for all layer objects no longer in
            // _layerObjectsUnderSelectionTool
            foreach (LayerObject layerObject in removedLayerObjects)
            {
                OnSelectionToolLeftLayerObject(new LayerObjectEventArgs(layerObject));
            }

            // Raise the SelectionToolEnteredLayerObject for all layer objects that have been
            // added to _layerObjectsUnderSelectionTool.
            foreach (LayerObject layerObject in newLayerObjects)
            {
                OnSelectionToolEnteredLayerObject(new LayerObjectEventArgs(layerObject));
                _layerObjectsUnderSelectionTool.Add(layerObject);
            }
        }

        /// <summary>
        /// Updates the active linked layer object based on the specified mouse coordinates.
        /// </summary>
        /// <param name="x">The physical (client) x-coordinate of the mouse.</param>
        /// <param name="y">The physical (client) y-coordinate of the mouse.</param>
        void UpdateActiveLinkedLayerObject(int x, int y)
        {
            Point mousePoint = new Point(x, y);

            // Check if there is an active linked layer object
            if (_activeLinkedLayerObject != null)
            {
                // Check if the mouse is still over the active linked layer object
                if (_activeLinkedLayerObject.HitLinkAreaTest(mousePoint))
                {
                    return;
                }
                else
                {
                    // The linked layer object is no longer active
                    Invalidate(_activeLinkedLayerObject.GetLinkArea());
                    _activeLinkedLayerObject = null;
                }
            }
            // If we reached this point _activeLinkedLayerObject is null

            // If _layerObjectsUnderSelectionTool is initialized we can use its members
            // rather than iterating all layer objects to find one which should be made active.
            if (_layerObjectsUnderSelectionTool != null)
            {
                // Check if the mouse is over a visible linked layer object
                foreach (LayerObject layerObject in _layerObjectsUnderSelectionTool)
                {
                    if (layerObject.IsLinked && layerObject.Visible)
                    {
                        _activeLinkedLayerObject = layerObject;
                        Invalidate(layerObject.GetLinkArea());
                        return;
                    }
                }
            }
            // If _layerObjectsUnderSelectionTool is null, search all layer objects to find one that
            // should be made active.
            else
            {
                // Convert the mouse position to image coordinates
                Point[] hitPoint = new Point[] { mousePoint };
                using (Matrix clientToImage = _transform.Clone())
                {
                    clientToImage.Invert();
                    clientToImage.TransformPoints(hitPoint);
                }

                // Check if the mouse is over a visible linked layer object
                foreach (LayerObject layerObject in _layerObjects)
                {
                    if (layerObject.IsLinked && layerObject.Visible &&
                        layerObject.HitTest(hitPoint[0]))
                    {
                        _activeLinkedLayerObject = layerObject;
                        Invalidate(layerObject.GetLinkArea());
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the cursor for the select layerObject tool based on the mouse position.
        /// </summary>
        /// <param name="mouse">A <see cref="Point"/> object representing the
        /// physical (client) x and y coordinates of the mouse.</param>
        /// <returns>The cursor for the select layerObject tool based on the mouse position.
        /// </returns>
        Cursor GetSelectionCursor(Point mouse)
        {
            return GetSelectionCursor(mouse.X, mouse.Y);
        }

        /// <summary>
        /// Gets the cursor for the select layer object tool based on the mouse position.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        /// <returns>The cursor for the select layerObject tool based on the mouse position.
        /// </returns>
        internal Cursor GetSelectionCursor(int mouseX, int mouseY)
        {
            // Get the point for the mouse position
            Point mousePoint = new Point(mouseX, mouseY);

            // Check if the mouse is over one of the link arrows
            if (_activeLinkedLayerObject != null && 
                _activeLinkedLayerObject.GetLinkArrowId(mousePoint) >= 0)
            {
                return Cursors.Hand;
            }

            // Iterate through all selected layer object
            foreach (LayerObject layerObject in _layerObjects.Selection)
            {
                // Skip this layer object if it is on a different page
                if (layerObject.PageNumber != _pageNumber)
                {
                    continue;
                }

                // Check if the mouse cursor is over one of the grip handles
                int gripHandleId = layerObject.GetGripHandleId(mousePoint);
                if (gripHandleId >= 0)
                {
                    return layerObject.GetGripCursor(gripHandleId);
                }
            }

            // If _layerObjectsUnderSelectionTool is initialized we can use its members
            // rather than iterating all layer objects to find one which should change the cursor.
            if (_layerObjectsUnderSelectionTool != null)
            {
                foreach (LayerObject layerObject in _layerObjectsUnderSelectionTool)
                {
                    if (layerObject.Selectable && layerObject.Visible)
                    {
                        // Return the sizing mouse cursor
                        return Cursors.SizeAll;
                    }
                }
            }
            // If _layerObjectsUnderSelectionTool is null, search all layer objects to find any that
            // should change the cursor.
            else
            {
                // Convert the mouse click to image coordinates
                Point[] hitPoint = new Point[] { mousePoint };
                using (Matrix clientToImage = _transform.Clone())
                {
                    clientToImage.Invert();
                    clientToImage.TransformPoints(hitPoint);
                }

                // Iterate through all layer objects
                foreach (LayerObject layerObject in _layerObjects)
                {
                    // Check if the mouse cursor is over a layer object
                    // Only perform hit test if object is selectable and visible
                    if (layerObject.Selectable && layerObject.Visible
                        && layerObject.HitTest(hitPoint[0]))
                    {
                        // Return the sizing mouse cursor
                        return Cursors.SizeAll;
                    }
                }
            }

            // The mouse is not over any layer object
            return Cursors.Default;
        }

        /// <summary>
        /// Gets the spatial data associated with the specified client-coordinate rectangle in 
        /// image coordinates.
        /// </summary>
        /// <param name="rectangle">A rectangle in client coordinates.</param>
        /// <param name="points">An array of holding the midpoints of two opposing sides of the 
        /// rectangle in image coordinates.</param>
        /// <param name="height">The distance in image pixels between two sides of the rectangle 
        /// measured perpendicular to the line formed by <paramref name="points"/>.</param>
        void GetSpatialDataFromClientRectangle(Rectangle rectangle, out Point[] points, out int height)
        {
            // Calculate the points of the raster zone's bisecting line
            int rasterLineY = rectangle.Top + rectangle.Height / 2;
            points = new Point[] 
            {
               new Point(rectangle.Left, rasterLineY), 
               new Point(rectangle.Right, rasterLineY)
            };

            // Convert the points from physical to logical coordinates
            using (Matrix inverseMatrix = _transform.Clone())
            {
                // Invert the transformation matrix
                inverseMatrix.Invert();

                // Transform the midpoints of the sides of the highlight
                inverseMatrix.TransformPoints(points);

                // Calculate the height
                // NOTE: The height is in physical (client) coordinates, so is 
                // scaled and rounded to the nearest logical (image) coordinates.
                // ALSO NOTE: base.ScaleFactor is incorrect in FitToPage or 
                // FitToWidth modes, so the correct scale factor must be computed. 
                // [DotNetRCAndUtils #24]
                height = (int)(rectangle.Height * GetScaleFactorY(inverseMatrix) + 0.5);
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseUp"/> event.
        /// </summary>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        // This event handler has undergone a security review.
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers",
            MessageId = "0#")]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void OnMouseUp(MouseEventArgs e)
        {
            try
            {
                base.OnMouseUp(e);

                // Process this mouse event if an interactive region is being created
                if (_trackingData != null && e.Button == MouseButtons.Left)
                {
                    // Finish tracking this event
                    EndTracking(e.X, e.Y, false);
                }

                if (IsImageAvailable)
                {
                    // Restore the original cursor tool
                    Cursor = _toolCursor ?? Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21323", ex);
                ee.AddDebugData("Mouse event", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Leadtools.WinForms.RasterImageViewer.TransformChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnTransformChanged(EventArgs e)
        {
            try
            {
                // Store the current matrix, accounting for rotation
                _transform = GetRotatedMatrix(base.Transform, false);

                // Check if annotations exist
                if (_annotations != null)
                {
                    // Update the annotations transformation matrix
                    _annotations.Transform = _transform.Clone();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI21324", ex);
            }

            base.OnTransformChanged(e);
        }

        /// <summary>
        /// Raises the <see cref="Leadtools.WinForms.RasterImageViewer.PostImagePaint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        /// <permission cref="SecurityPermission">Demands permission for unmanaged code.
        /// </permission>
        // This event handler has undergone a security review.
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers",
            MessageId = "0#")]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void OnPostImagePaint(PaintEventArgs e)
        {
            try
            {
                // Draw the annotations if they exist
                if (_annotations != null)
                {
                    _annotations.Draw(e.Graphics);
                }

                // Draw the layer objects
                using (Region clip = new Region(GetVisibleImageArea()))
                {
                    // Ensure the clipping region is no larger than the Graphics clipping region
                    clip.Intersect(e.Graphics.Clip);

                    // Draw each layerObject in order of the layer object collection's z-order.
                    foreach (LayerObject layerObject in _layerObjects.InZOrder)
                    {
                        if (layerObject.Visible && layerObject.PageNumber == _pageNumber)
                        {
                            layerObject.Paint(e.Graphics, clip);
                        }
                    }
                }

                // Draw the grip handles
                foreach (LayerObject layerObject in _layerObjects.Selection)
                {
                    layerObject.DrawGripHandles(e.Graphics);
                }

                // Draw the link arrows
                if (_activeLinkedLayerObject != null)
                {
                    _activeLinkedLayerObject.DrawLinkArrows(e.Graphics);
                }
                
                base.OnPostImagePaint(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI21133",
                    "Unable to draw image.", ex);
                ee.AddDebugData("Paint event", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="ImageFileChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="ImageFileChangedEventArgs"/> that contains the event 
        /// data.</param>
        protected virtual void OnImageFileChanged(ImageFileChangedEventArgs e)
        {
            if (ImageFileChanged != null)
            {
                ImageFileChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ImageFileClosing"/> event.
        /// </summary>
        /// <param name="e">An <see cref="ImageFileClosingEventArgs"/> that contains
        /// the event data.</param>
        protected virtual void OnImageFileClosing(ImageFileClosingEventArgs e)
        {
            if (ImageFileClosing != null)
            {
                ImageFileClosing(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ZoomChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="ZoomChangedEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnZoomChanged(ZoomChangedEventArgs e)
        {
            if (ZoomChanged != null)
            {
                ZoomChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="OrientationChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="OrientationChangedEventArgs"/> that contains the event 
        /// data.</param>
        protected virtual void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (OrientationChanged != null)
            {
                OrientationChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="CursorToolChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="CursorToolChangedEventArgs"/> that contains the event 
        /// data.</param>
        protected virtual void OnCursorToolChanged(CursorToolChangedEventArgs e)
        {
            if (CursorToolChanged != null)
            {
                CursorToolChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="FitModeChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="FitModeChangedEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnFitModeChanged(FitModeChangedEventArgs e)
        {
            if (FitModeChanged != null)
            {
                FitModeChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="PageChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="PageChangedEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnPageChanged(PageChangedEventArgs e)
        {
            if (PageChanged != null)
            {
                PageChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="OpeningImage"/> event.
        /// </summary>
        /// <param name="e">A <see cref="OpeningImageEventArgs"/> that contains the
        /// event data.</param>
        protected virtual void OnOpeningImage(OpeningImageEventArgs e)
        {
            if (OpeningImage != null)
            {
                OpeningImage(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="LoadingNewImage"/> event.
        /// </summary>
        /// <param name="e">A <see cref="LoadingNewImageEventArgs"/> that contains
        /// the event data.</param>
        protected virtual void OnLoadingNewImage(LoadingNewImageEventArgs e)
        {
            if (LoadingNewImage != null)
            {
                LoadingNewImage(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="DisplayingPrintDialog"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="DisplayingPrintDialog"/> 
        /// event.</param>
        protected virtual void OnDisplayingPrintDialog(DisplayingPrintDialogEventArgs e)
        {
            if (DisplayingPrintDialog != null)
            {
                DisplayingPrintDialog(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="OnFileOpenError"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="FileOpenError"/> event.
        /// </param>
        protected virtual void OnFileOpenError(FileOpenErrorEventArgs e)
        {
            if (FileOpenError != null)
            {
                FileOpenError(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="SelectionToolEnteredLayerObject"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="LayerObjectEventArgs"/> 
        /// event.</param>
        protected virtual void OnSelectionToolEnteredLayerObject(LayerObjectEventArgs e)
        {
            if (SelectionToolEnteredLayerObject != null)
            {
                SelectionToolEnteredLayerObject(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="SelectionToolLeftLayerObject"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="LayerObjectEventArgs"/> 
        /// event.</param>
        protected virtual void OnSelectionToolLeftLayerObject(LayerObjectEventArgs e)
        {
            if (SelectionToolLeftLayerObject != null)
            {
                SelectionToolLeftLayerObject(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseWheel"/> event.
        /// </summary>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            try
            {
                base.OnMouseWheel(e);

                // Check if an interactive mouse tracking event is occurring
                if (_trackingData == null)
                {
                    // No interactive tracking event.

                    // Check which modifier keys are being pressed
                    switch (ModifierKeys)
                    {
                        // Zoom in/out
                        case Keys.Control:

                            // Get the current mouse position in image coordinates
                            Point[] mousePosition = new Point[] { new Point(e.X, e.Y) };
                            using (Matrix clientToImage = _transform.Clone())
                            {
                                clientToImage.Invert();
                                clientToImage.TransformPoints(mousePosition);
                            }

                            // Suspend the paint event until all changes have been made
                            base.BeginUpdate();
                            try
                            {
                                // Zoom in based on the direction of the mouse wheel event
                                Zoom(e.Delta > 0);

                                // Get the mouse position in client coordinates
                                _transform.TransformPoints(mousePosition);

                                // Adjust the scroll so that the mouse is 
                                // over the same point on the image
                                Point scroll = base.ScrollPosition;
                                scroll.Offset(mousePosition[0].X - e.X, mousePosition[0].Y - e.Y);
                                base.ScrollPosition = scroll;
                            }
                            finally
                            {
                                base.EndUpdate();
                            }
                            break;

                        // Scroll left/right
                        case Keys.Shift:

                            // Check if a horizontal scroll bar is visible
                            if (HScroll)
                            {
                                // Scroll horizontally
                                Point scroll = base.ScrollPosition;
                                scroll.X += base.AutoScrollSmallChange.Width * (e.Delta > 0 ?
                                    -_MOUSEWHEEL_SCROLL_FACTOR : _MOUSEWHEEL_SCROLL_FACTOR);
                                base.ScrollPosition = scroll;
                            }
                            break;

                        // Scroll up/down
                        default:

                            // Check if a vertical scroll bar is visible
                            if (VScroll)
                            {
                                // Scroll vertically
                                Point scroll = base.ScrollPosition;
                                scroll.Y += base.AutoScrollSmallChange.Height * (e.Delta > 0 ?
                                    -_MOUSEWHEEL_SCROLL_FACTOR : _MOUSEWHEEL_SCROLL_FACTOR);
                                base.ScrollPosition = scroll;
                            }
                            break;
                    }
                }
                else
                {
                    // Adjust the highlight height
                    AdjustHighlightHeight(e.X, e.Y, e.Delta > 0);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21585", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.DragEnter"/> event.
        /// </summary>
        /// <param name="drgevent">A <see cref="DragEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            try
            {
                // Call the base class
                base.OnDragEnter(drgevent);

                // Check if this is a file drop
                if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    drgevent.Effect = DragDropEffects.Copy;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21584", ex);
                ee.AddDebugData("Event data", drgevent, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.DragDrop"/> event.
        /// </summary>
        /// <param name="drgevent">A <see cref="DragEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            try
            {
                base.OnDragDrop(drgevent);

                // Check if this is a file drop event
                if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Get the files being dragged
                    string[] fileNames = (string[])drgevent.Data.GetData(DataFormats.FileDrop);

                    // Open the file in the image viewer
                    if (fileNames.Length == 1)
                    {
                        OpenImage(fileNames[0], true);
                    }
                    else if(fileNames.Length > 1)
                    {
                        // If trying to open more than one file, display an error message
                        MessageBox.Show("Cannot open more than one file.", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning, 
                            MessageBoxDefaultButton.Button1, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21583", ex);
                ee.AddDebugData("Event data", drgevent, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the RasterImageViewer.ScrollPositionChanged event.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains data for the event.</param>
        protected override void OnScrollPositionChanged(EventArgs e)
        {
            try
            {
                base.OnScrollPositionChanged(e);

                // Store the scroll position [DNRCAU #262 - JDS]
                _scrollPosition = ScrollPosition;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23241", ex);
                ee.AddDebugData("Event args", e, false);
                ee.Display();
            }
        }

        #endregion

        #region ImageViewer Event Handlers

        /// <summary>
        /// Handles the <see cref="PrintDocument.BeginPrint"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="PrintDocument.BeginPrint"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="PrintDocument.BeginPrint"/> event.</param>
        void HandleBeginPrint(object sender, PrintEventArgs e)
        {
            // Reset the print page
            _printPage = 0;

            // If there is a print preview dialog, need to invalidate it
            if (_printPreview != null)
            {
                // TODO: Look into removing DoEvents, will Refresh work?
                // Invalidate the print preview dialog and the image viewer and call do events so
                // that the toolstrip will draw
                _printPreview.Invalidate();
                Invalidate();
                Application.DoEvents();
            }
        }

        /// <summary>
        /// Handles the <see cref="PrintDocument.PrintPage"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="PrintDocument.PrintPage"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="PrintDocument.PrintPage"/> event.</param>
        void HandlePrintPage(object sender, PrintPageEventArgs e)
        {
            // Print the image
            PrinterSettings settings = e.PageSettings.PrinterSettings;
            switch (settings.PrintRange)
            {
                case PrintRange.AllPages:
                case PrintRange.SomePages:

                    // Increment the page number to print
                    _printPage += _printPage == 0 ? settings.FromPage : 1;

                    // Check if we are printing the current page
                    if (_printPage == _pageNumber)
                    {
                        // Print the current page
                        PrintPage(e);
                    }
                    else
                    {
                        // Store the current page and suppress the paint event
                        int originalPage = _pageNumber;
                        base.BeginUpdate();
                        try
                        {
                            // Switch to the page being printed
                            SetPageNumber(_printPage, false, false);

                            // Print the current page
                            PrintPage(e);
                        }
                        finally
                        {
                            // Restore the original page
                            SetPageNumber(originalPage, false, false);

                            // Unsuppress the paint event
                            base.EndUpdate();
                        }
                    }

                    // Check whether more pages need to be printed
                    e.HasMorePages = _printPage != settings.ToPage;
                    break;

                case PrintRange.CurrentPage:

                    // Print the current page
                    PrintPage(e);
                    break;

                case PrintRange.Selection:

                    // This option is not supported
                    throw new ExtractException("ELI21524",
                        "Invalid print option. Cannot print selection.");

                default:

                    ExtractException ee = new ExtractException("ELI21525",
                        "Unexpected print range.");
                    ee.AddDebugData("Print range", settings.PrintRange, false);
                    throw ee;
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        void HandleLayerObjectDeleted(object sender, LayerObjectDeletedEventArgs e)
        {
            try
            {
                // Check if the deleted layer object is the active linked layer object
                if (e.LayerObject == _activeLinkedLayerObject)
                {
                    _activeLinkedLayerObject = null;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22659", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripDropDown.Opening"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> associated with the event.</param>
        void HandleContextMenuStripOpening(object sender, CancelEventArgs e)
        {
            try
            {
                // If the context menu is to be suppressed, cancel the opening event then clear the
                // _suppressContextMenu flag.
                if (_suppressContextMenu)
                {
                    e.Cancel = true;
                    _suppressContextMenu = false;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27117", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the Load event of the print preview dialog.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandlePrintPreviewLoad(object sender, EventArgs e)
        {
            try
            {
                Form parent = TopLevelControl as Form;

                // If found the parent form for the image viewer, use its coordinates
                // as the coordinates and size of the print preview dialog
                if (parent != null)
                {
                    // Set the print preview icon
                    _printPreview.Icon = parent.Icon;

                    // Set the dimensions and location of the print preview form
                    // from the parent form of the image viewer control
                    _printPreview.ClientSize = parent.ClientSize;
                    _printPreview.Location = parent.Location;
                }

                // Invalidate the image viewer before displaying the form
                RefreshImageViewerAndParent();
            }
            catch (Exception ex)
            {
                ExtractException ee =  ExtractException.AsExtractException("ELI23371", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the click event from the <see cref="ToolStripButton"/> that is
        /// added to the print preview dialog.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandlePrintPreviewPrintButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Check if the caller is a ToolStripItem
                ToolStripItem toolStripItem = sender as ToolStripItem;
                if (toolStripItem != null)
                {
                    // Handle the user's first mouse click [DotNetRCAndUtils #58]
                    toolStripItem.Owner.Capture = false;
                }

                // Call print and do not raise the DisplayingPrintDialog event
                Print(false);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23372", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion ImageViewer Event Handlers

        #region RasterImageViewer Overrides

        /// <summary>
        /// Prevents <see cref="ImageViewer"/> from being drawn until <see cref="EndUpdate"/> is 
        /// called.
        /// </summary>
        public override void BeginUpdate()
        {
            base.BeginUpdate();
        }

        /// <summary>
        /// Resumes paint operations after <see cref="BeginUpdate"/> has been called.
        /// </summary>
        public override void EndUpdate()
        {
            base.EndUpdate();
        }

        /// <summary>
        /// Zooms the image to the specified rectangle.
        /// </summary>
        /// <param name="rc">The rectangle to which the image should be zoomed in client 
        /// coordinates.</param>
        /// <exception cref="ExtractException">No image is open.</exception>
        /// <event cref="ZoomChanged">The method was successful.</event>
        public override void ZoomToRectangle(Rectangle rc)
        {
            try
            {
                // Zoom to the specified rectangle and update the zoom
                ZoomToRectangle(rc, true, true, true);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI21456",
                    "Unable to zoom to rectangle.", ex);
                ee.AddDebugData("Rectangle", rc, false);
                throw ee;
            }
        }

        #endregion
    }
}
