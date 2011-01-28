using Extract.Drawing;
using Extract.Imaging.Utilities;
using Extract.Licensing;
using Extract.Utilities.Forms;
using Leadtools;
using Leadtools.Annotations;
using Leadtools.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a control that can display and interact with image files.
    /// </summary>
    [ToolboxBitmap(typeof(ImageViewer), ToolStripButtonConstants._IMAGE_VIEWER_ICON_IMAGE)]
    public sealed partial class ImageViewer : RasterImageViewer
    {
        #region Constants

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
        /// The minimum height of a block in the line fitting algorithm.
        /// </summary>
        const int _MIN_SPLIT_HEIGHT = 10;

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

        /// <summary>
        /// One-based index of the default image file type. This value corresponds to the all 
        /// image files option.
        /// </summary>
        /// <seealso cref="_IMAGE_FILE_TYPES"/>
        const int _IMAGE_FILE_TYPE_DEFAULT_INDEX = 9;

        #endregion Constants

        #region Static Fields

        /// <summary>
        /// Contains the <see cref="ImageViewerCursors"/> for each <see cref="CursorTool"/>.
        /// </summary>
        static Dictionary<CursorTool, ImageViewerCursors> _cursorsForCursorTools;

        /// <summary>
        /// Mutex used to increment the form count
        /// </summary>
        static object _lock = new object();

        /// <summary>
        /// Variable to monitor the form count
        /// </summary>
        volatile static int _activeFormCount = 0;

        #endregion Static Fields

        #region Fields

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
        List<LayerObject> _layerObjectsUnderCursor;

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
        ImageCodecs _codecs;

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
        /// The image reader for the currently open image.
        /// </summary>
        ImageReader _reader;

        /// <summary>
        /// Specifies whether the context menu should be prevented from opening.
        /// </summary>
        bool _suppressContextMenu;

        /// <summary>
        /// Specifies whether selection of multiple layer object via a banded box is enabled.
        /// </summary>
        bool _allowBandedSelection = true;

        /// <summary>
        /// Specifies whether highlight/redaction tools are enabled or not.
        /// </summary>
        bool _allowHighlight = true;

        /// <summary>
        /// Specifies what the minimum highlight height is for angular highlights.
        /// Note: This cannot be set less than the LayerObject.MinSize.Height
        /// </summary>
        int _minimumAngularHighlightHeight = LayerObject.MinSize.Height;

        /// <summary>
        /// Indicates whether document images should be cached rather than loaded/unloaded on
        /// open/close.
        /// </summary>
        bool _cacheImages;

        /// <summary>
        /// Helper class that creates and displays <see cref="Highlight"/>s for the word
        /// redaction/highlighter tools by using image OCR data.
        /// </summary>
        WordHighlightManager _wordHighlightManager;

        /// <summary>
        /// A <see cref="PostPaintDelegate"/> that is to be called in order to update the tracking 
        /// data and display tracking-related graphics in PostImagePaint.
        /// </summary>
        PostPaintDelegate _trackingUpdateCall;

        /// <summary>
        /// Indicates whether calls to Invalidate should be blocked if they are known to be
        /// generated by an internal process and are un-wanted.
        /// </summary>
        bool _preventInvalidate = false;

        #endregion Fields

        #region Delegates

        /// <summary>
        /// Delegate for methods that are to run at the end of the next paint operation.
        /// </summary>
        delegate void PostPaintDelegate(PaintEventArgs e);

        #endregion Delegates

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
        /// Occurs when the cursor enters the bounds of a <see cref="LayerObject"/>.
        /// <para><b>NOTE:</b></para>
        /// This event is raised even for <see cref="LayerObject"/>s that are not visible or that
        /// are not selectable. It is up to the receiver to filter events for
        /// <see cref="LayerObject"/>s that should be ignored.
        /// </summary>
        public event EventHandler<LayerObjectEventArgs> CursorEnteredLayerObject;

        /// <summary>
        /// Occurs when the cursor leaves the bounds of a <see cref="LayerObject"/>.
        /// <para><b>NOTE:</b></para>
        /// This event is raised even for <see cref="LayerObject"/>s that are not visible or that
        /// are not selectable. It is up to the receiver to filter events for
        /// <see cref="LayerObject"/>s that should be ignored.
        /// </summary>
        public event EventHandler<LayerObjectEventArgs> CursorLeftLayerObject;

        /// <summary>
        /// Occurs when a sub image has been extracted from the present image.
        /// </summary>
        public event EventHandler<ImageExtractedEventArgs> ImageExtracted;

        /// <summary>
        /// Occurs when the <see cref="AllowHighlight"/> property changes.
        /// </summary>
        public event EventHandler<EventArgs> AllowHighlightStatusChanged;

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

                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23109",
                    _OBJECT_NAME);

                // Increment the active form count
                IncrementFormCount();

                // Attempt to unlock PDF support (returns an extract exception if unlocking failed).
                ExtractException ee = UnlockLeadtools.UnlockPdfSupport(false);
                if (ee != null)
                {
                    ee.Display();
                }

                // Attempt to unlock document support
                if (UnlockLeadtools.UnlockDocumentSupport(false) != null)
                {
                    // Turn off display of annotation if document support is locked
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
                    RasterPaintProperties properties = PaintProperties;
                    properties.PaintDisplayMode |= RasterPaintDisplayModeFlags.ScaleToGray;
                    PaintProperties = properties;
                }

                // Set the fit mode
                SetFitMode(RegistryManager.FitMode, false, false, true);

                // Store the original transformation matrix
                _transform = base.Transform;

                // Handle layer object remove events
                _layerObjects.LayerObjectDeleted += HandleLayerObjectDeleted;

                _wordHighlightManager = new WordHighlightManager(this);
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21138", "Unable to initialize image viewer.", e);
            }
        }

        #endregion

        #region Properties

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
                    if (value == _cursorTool)
                    {
                        return;
                    }

                    // [FlexIDSCore:4507]
                    // If a tracking event is currently in progress, cancel it.
                    if (_trackingData != null)
                    {
                        EndTracking(0, 0, true);
                    }

                    // Ensure an image is open
                    ExtractException.Assert("ELI21321", "Image must be open.",
                        base.Image != null || value == CursorTool.None);

                    switch (value)
                    {
                        case CursorTool.Pan:

                            // Allow the user to pan the image using drag and drop
                            base.InteractiveMode = RasterViewerInteractiveMode.Pan;

                            // Store this as the last active continuous use cursor tool
                            _lastContinuousUseTool = value;
                            break;

                        case CursorTool.DeleteLayerObjects:
                        case CursorTool.SetHighlightHeight:
                        case CursorTool.ExtractImage:

                            // Turn off interactive mode
                            base.InteractiveMode = RasterViewerInteractiveMode.None;

                            break;

                        case CursorTool.None:
                        case CursorTool.AngularHighlight:
                        case CursorTool.AngularRedaction:
                        case CursorTool.EditHighlightText:
                        case CursorTool.RectangularHighlight:
                        case CursorTool.RectangularRedaction:
                        case CursorTool.WordHighlight:
                        case CursorTool.WordRedaction:
                        case CursorTool.ZoomWindow:

                            // Turn off interactive mode
                            base.InteractiveMode = RasterViewerInteractiveMode.None;

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

                    // Redraw the grip handles of any layer objects [FIDSC #3993]
                    Invalidate();

                    // Disable the active linked layer object unless using select layer object
                    if (value != CursorTool.SelectLayerObject)
                    {
                        _activeLinkedLayerObject = null;

                        // If the cursor tool is no longer the selection tool, remove all entries
                        // from _layerObjectsUnderCursor (and raise the CursorLeftLayerObject for each)
                        UpdateLayerObjectsUnderCursor(null);
                    }

                    // Set the cursor tool
                    _cursorTool = value;

                    // Get the cursor values for the current tool
                    ImageViewerCursors cursors = GetCursorForTool(value);

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
                        _cursorTool == CursorTool.RectangularRedaction ||
                        _cursorTool == CursorTool.WordHighlight ||
                        _cursorTool == CursorTool.WordRedaction)
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
                    SetFitMode(value, true, true, true);
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
                            LicenseUtilities.ValidateLicense(LicenseIdName.AnnotationFeature,
                                "ELI21920", "Annotation Objects");
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
                            LicenseUtilities.ValidateLicense(LicenseIdName.AntiAliasingFeature,
                                "ELI21921", "Anti-aliasing Component");

                            // Turn on anti-aliasing
                            RasterPaintProperties properties = PaintProperties;
                            properties.PaintDisplayMode |= RasterPaintDisplayModeFlags.ScaleToGray;
                            PaintProperties = properties;
                        }
                    }
                    else if (_useAntiAliasing)
                    {
                        // Turn off anti-aliasing
                        RasterPaintProperties properties = PaintProperties;
                        properties.PaintDisplayMode &= ~RasterPaintDisplayModeFlags.ScaleToGray;
                        PaintProperties = properties;
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
                Rectangle imageArea = PhysicalViewRectangle;

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
                Rectangle imageArea = PhysicalViewRectangle;

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
        void SetPageNumber(int pageNumber, bool updateZoom, bool raisePageChanged)
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

                // Add a zoom history entry for the current page so that when the user navigates
                // back to this page, they will be looking at the same region they were before
                // they moved away.
                if (updateZoom && _pageNumber != pageNumber)
                {
                    UpdateZoom(true, false);
                }

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
                            ScrollPosition = Point.Empty;
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
        /// Sets a <see cref="RasterImage"/> to be displayed in the image viewer.
        /// <para><b>Note:</b></para>
        /// This will also disable highlight/redaction creation. This method
        /// is intended to be used to display sub images that have been extracted
        /// from a larger image.
        /// </summary>
        /// <param name="image">The image to display.</param>
        /// <param name="orientation">The orientation to set for this image.</param>
        /// <param name="imageFileName">The file name for the image.</param>
        public void DisplayRasterImage(RasterImage image, int orientation, string imageFileName)
        {
            try
            {
                ExtractException.Assert("ELI30221",
                    "Image must be a single page to use this method.", image.PageCount == 1);
                ExtractException.Assert("ELI30249", "Orientation must be multiple of 90.",
                    orientation % 90 == 0, "Orientation", orientation);

                // Raise the opening image event
                OpeningImageEventArgs opening = new OpeningImageEventArgs(imageFileName, false);
                OnOpeningImage(opening);

                // Check if the event was cancelled
                if (opening.Cancel)
                {
                    return;
                }

                // Close the currently open image without raising
                // the image file changed event.
                if (IsImageAvailable && !CloseImage(false))
                {
                    return;
                }

                // Raise the LoadingNewImage event
                OnLoadingNewImage(new LoadingNewImageEventArgs());

                // Refresh the image viewer before opening the new image
                RefreshImageViewerAndParent();
                using (new TemporaryWaitCursor())
                {
                    _validAnnotations = true;

                    _pageNumber = 1;
                    _pageCount = 1;
                    _imagePages = new List<ImagePageData>(1);
                    _imagePages.Add(new ImagePageData());

                    _imageFile = imageFileName;

                    base.Image = image;

                    // If a fit mode is not specified, display the whole page 
                    // [DotNetRCAndUtils #102]
                    if (_fitMode == FitMode.None)
                    {
                        ShowFitToPage();
                    }

                    // Set the cursor tool to zoom window if none is set
                    if (_cursorTool == CursorTool.None)
                    {
                        CursorTool = CursorTool.ZoomWindow;
                    }

                    // If the orientation is not 0, rotate the image by the orientation
                    if (orientation != 0)
                    {
                        Rotate(orientation);
                    }

                    // Disable adding highlights
                    AllowHighlight = false;
                }

                // Raise the image file changed event
                OnImageFileChanged(new ImageFileChangedEventArgs(imageFileName));

                // Raise the on page changed event
                OnPageChanged(new PageChangedEventArgs(_pageNumber));

                // Update the zoom history and raise the zoom changed event
                UpdateZoom(true, true);

                // Restore the cursor for the active cursor tool.
                Cursor = _toolCursor ?? Cursors.Default;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30222", ex);
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
                image = _reader.ReadPage(pageNumber);

                int rotation = _imagePages[pageNumber - 1].Orientation;
                ImageMethods.RotateImageByDegrees(image, rotation);
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
        /// Gets the properties for the specified page of the currently open image.
        /// </summary>
        /// <param name="pageNumber">The 1-based page number from which to retrieve properties.
        /// </param>
        /// <returns>The properties for the specified <paramref name="pageNumber"/> of the 
        /// currently open image.</returns>
        public ImagePageProperties GetPageProperties(int pageNumber)
        {
            try
            {
                if (!IsImageAvailable)
                {
                    throw new ExtractException("ELI28829",
                        "No image is open.");
                }

                return _reader.ReadPageProperties(pageNumber);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28830",
                    "Cannot get page properties.", ex);
                ee.AddDebugData("Page Number", pageNumber, false);
                throw ee;
            }
        }

        /// <summary>
        /// Displays the current page fit exactly within the <see cref="ImageViewer"/>.
        /// </summary>
        /// <remarks>This method does not change the fit mode.</remarks>
        void ShowFitToPage()
        {
            base.SizeMode = RasterPaintSizeMode.FitAlways;
            ScaleFactor = _DEFAULT_SCALE_FACTOR;

            // [DataEntry:837]
            // If a zero-size rectangle has been specified (as is the case with a minimized
            // window), zooming is not possible and would throw an exception if attempted.
            if (DisplayRectangle.Width >= 1 && DisplayRectangle.Height >= 1)
            {
                // Zoom the specified rectangle
                base.ZoomToRectangle(DisplayRectangle);
            }
        }

        /// <summary>
        /// Displays the width of the current page fit exactly within the 
        /// <see cref="ImageViewer"/>.
        /// </summary>
        /// <remarks>This method does not change the fit mode.</remarks>
        void ShowFitToWidth()
        {
            base.SizeMode = RasterPaintSizeMode.FitWidth;
            ScaleFactor = _DEFAULT_SCALE_FACTOR;

            // [DataEntry:837]
            // If a zero-size rectangle has been specified (as is the case with a minimized
            // window), zooming is not possible and would throw an exception if attempted.
            if (DisplayRectangle.Width >= 1 && DisplayRectangle.Height >= 1)
            {
                // Zoom the specified rectangle
                base.ZoomToRectangle(DisplayRectangle);
            }
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
        /// Gets/sets whether highlight/redaction tools are allowed or not.
        /// </summary>
        /// <value><see langword="true"/> to allow drawing of highlights/redactions,
        /// <see langword="false"/> otherwise.</value>
        /// <returns>Whether highlight/redaction tools are allowed.</returns>
        [DefaultValue(true)]
        public bool AllowHighlight
        {
            get
            {
                return _allowHighlight;
            }
            set
            {
                _allowHighlight = value;

                // Raise the Highlight status changed event
                OnAllowHighlightStatusChanged();
            }
        }

        /// <summary>
        /// Gets/sets the minimum height for an angular highlight.
        /// <para><b>Note:</b></para>
        /// Cannot be less than LayerObject.MinSize.Height.
        /// </summary>
        /// <value>The minimum height for an angular highlight.</value>
        /// <returns>The minimum height for an angular highlight.</returns>
        public int MinimumAngularHighlightHeight
        {
            get
            {
                return _minimumAngularHighlightHeight;
            }
            set
            {
                _minimumAngularHighlightHeight = Math.Max(LayerObject.MinSize.Height, value);
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

        /// <summary>
        /// Gets the codecs used to encode and decode images.
        /// </summary>
        /// <value>The codecs used to encode and decode images.</value>
        ImageCodecs Codecs
        {
            get 
            { 
                if (_codecs == null)
                {
                    _codecs = new ImageCodecs();
                }

                return _codecs;
            }
        }

        /// <summary>
        /// Gets whether there is a current tracking event taking place.
        /// (i.e. drawing a highlight).
        /// </summary>
        /// <returns><see langword="true"/> if a tracking event is occurring and
        /// <see langword="false"/> if no tracking event is occurring.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsTracking
        {
            get
            {
                return _trackingData != null;
            }
        }

        /// <summary>
        /// Gets or sets whether <see cref="ImageReader"/>s for documents should be cached rather
        /// than loaded/unloaded on open/close.
        /// <para><b>Note</b></para>
        /// It is important to ensure <see cref="UnloadImage"/> is called for every image for which
        /// either <see cref="OpenImage"/> or <see cref="CacheImage"/> is called when
        /// <see cref="CacheImages"/> is <see langword="true"/>.
        /// </summary>
        /// <value><see langword="true"/> if <see cref="ImageReader"/>s should be cached in memory,
        /// <see langword="false"/> if they should be created/disposed as the images are opened and
        /// closed.</value>
        [DefaultValue(false)]
        public bool CacheImages
        {
            get
            {
                return _cacheImages;
            }

            set
            {
                _cacheImages = value;
            }
        }

        #endregion Properties

        #region OnEvents

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
                    Cursor = GetSelectionCursor(PointToClient(MousePosition));
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
                    Cursor = GetSelectionCursor(PointToClient(MousePosition));
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

                // Update _layerObjectsUnderCursor contents and raise the CursorEnteredLayerObject
                // and CursorLeftLayerObject events as necessary.
                UpdateLayerObjectsUnderCursor(e);

                // Process this mouse event if an interactive region is being created
                if (_trackingData != null && e.Button == MouseButtons.Left)
                {
                    // Assign a tracking update call to update the tracking region and display
                    // tracking graphics and invalidate to trigger the graphics to be drawn.
                    _trackingUpdateCall =
                        ((paintEventArgs) => UpdateTracking(paintEventArgs, e.X, e.Y));
                    Invalidate();
                }
                else if (IsImageAvailable && _cursorTool == CursorTool.SelectLayerObject)
                {
                    UpdateActiveLinkedLayerObject(e.X, e.Y);

                    Cursor cursor = GetSelectionCursor(e.X, e.Y);

                    // Set the cursor
                    Cursor = cursor;
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
            try
            {
                base.OnMouseLeave(e);

                // If the mouse has left the image viewer, remove all entries from 
                // _layerObjectsUnderCursor (and raise the CursorLeftLayerObject for each)
                UpdateLayerObjectsUnderCursor(null);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31376", ex);
            }
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
        /// Updates the _layerObjectsUnderCursor and raises the <see cref="CursorEnteredLayerObject"/>
        /// and <see cref="CursorLeftLayerObject"/> events as appropriate.
        /// </summary>
        /// <param name="e">If not <see langword="null"/>, contains a <see cref="MouseEventArgs"/>
        /// that contains infomation about the mouse event that triggered this call. If
        /// <see langword="null"/>, <see cref="CursorLeftLayerObject"/> will be raised for
        /// all <see cref="LayerObject"/>s previously under the cursor.</param>
        void UpdateLayerObjectsUnderCursor(MouseEventArgs e)
        {
            // Determine whether the method should look for any layer objects under the selection
            // tool. (If false, all current _layerObjectsUnderCursor members will be removed).
            bool lookForLayerObjectsUnderCursor = (e != null && IsImageAvailable);

            // Don't bother looking for layerobjects under the cursor if no one is listening
            // for the events.
            lookForLayerObjectsUnderCursor &=
                (CursorEnteredLayerObject != null || CursorLeftLayerObject != null);

            // If looking for new layer objects, initialize _layerObjectsUnderCursor if
            // necessary.
            if (lookForLayerObjectsUnderCursor && _layerObjectsUnderCursor == null)
            {
                _layerObjectsUnderCursor = new List<LayerObject>();
            }

            // Initialize a list of layer objects no longer under the selection tool.
            List<LayerObject> removedLayerObjects = new List<LayerObject>();

            // Initialize a list of new layer objects under the selection tool.
            List<LayerObject> newLayerObjects = new List<LayerObject>();

            bool useRegion = false;
            Point imagePoint = Point.Empty;
            Rectangle imageRegion = Rectangle.Empty;

            if (lookForLayerObjectsUnderCursor)
            {
                // If a tracking operation is active, look for all layer objects within the tracking
                // operation's rectangle rather than under only the cursor.
                if (_trackingData != null)
                {
                    // Convert the rectangle from client to image coordinates
                    imageRegion =
                        GetTransformedRectangle(_trackingData.Rectangle, true);

                    // Ensure imageRegion width & height > 0, otherwise it will not intersect with
                    // anything. 
                    if (imageRegion.Width == 0)
                    {
                        imageRegion.Width++;
                    }
                    if (imageRegion.Height == 0)
                    {
                        imageRegion.Height++;
                    }

                    useRegion = true;
                }
                // Otherwise, look only under the cursor itself.
                else
                {
                    // Obtain the mouse position in image coordinates.
                    imagePoint = GeometryMethods.InvertPoint(_transform, e.Location);
                }
            }

            // For all current members of _layerObjectsUnderCursor, ensure they are still under the
            // cursor.
            if (_layerObjectsUnderCursor != null)
            {
                // If not looking for any objects under the selection tool, simply remove all
                // existing objects from _layerObjectsUnderCursor
                if (!lookForLayerObjectsUnderCursor)
                {
                    removedLayerObjects.AddRange(_layerObjectsUnderCursor);
                    _layerObjectsUnderCursor.Clear();
                }

                for (int i = 0; i < _layerObjectsUnderCursor.Count; i++)
                {
                    LayerObject layerObject = _layerObjectsUnderCursor[i];

                    // Presume the cursor or selection area is not contained in the layer object
                    // until we find otherwise.
                    bool contained = false;

                    // If we are looking for layer objects, the layer object is in the image viewer
                    // and the layer object is on this page, test to see if it is under the
                    // selection tool or selection box.
                    if (lookForLayerObjectsUnderCursor &&
                        _layerObjects.Contains(layerObject) &&
                        layerObject.PageNumber == _pageNumber)
                    {
                        contained = (useRegion && layerObject.HitTest(imageRegion)) ||
                                    (!useRegion && layerObject.HitTest(imagePoint));
                    }

                    // If the cursor or selection area is not contained in this layer object, remove
                    // from _layerObjectsUnderCursor.
                    if (!contained)
                    {
                        removedLayerObjects.Add(layerObject);
                        _layerObjectsUnderCursor.RemoveAt(i);
                        i--;
                    }
                }
            }

            // If searching for layer objects, try to find any underneath the selection tool.
            if (lookForLayerObjectsUnderCursor)
            {
                // Iterate through all layer objects
                foreach (LayerObject layerObject in _layerObjects)
                {
                    // Skip this layer object if it is on a different page
                    if (layerObject.PageNumber != _pageNumber)
                    {
                        continue;
                    }

                    // Check if the mouse cursor or tracking rectangle is over a layer object and
                    // that _layerObjectsUnderCursor doesn't already contain the layer
                    // object.
                    bool contained = (useRegion && layerObject.HitTest(imageRegion)) ||
                                     (!useRegion && layerObject.HitTest(imagePoint));
                    if (contained && !_layerObjectsUnderCursor.Contains(layerObject))
                    {
                        newLayerObjects.Add(layerObject);
                    }
                }
            }
            else
            {
                // If not looking for layer objects under the cursor, un-initialize 
                // _layerObjectsUnderCursor after all event have been raised.
                _layerObjectsUnderCursor = null;
            }

            // Raise the CursorLeftLayerObject for all layer objects no longer in
            // _layerObjectsUnderCursor
            foreach (LayerObject layerObject in removedLayerObjects)
            {
                OnCursorlLeftLayerObject(new LayerObjectEventArgs(layerObject));
            }

            // Raise the CursorEnteredLayerObject for all layer objects that have been added
            // to _layerObjectsUnderCursor.
            foreach (LayerObject layerObject in newLayerObjects)
            {
                OnCursorEnteredLayerObject(new LayerObjectEventArgs(layerObject));
                _layerObjectsUnderCursor.Add(layerObject);
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

            // If _layerObjectsUnderCursor is initialized we can use its members rather than
            // iterating all layer objects to find one which should be made active.
            if (_layerObjectsUnderCursor != null)
            {
                // Check if the mouse is over a visible linked layer object
                foreach (LayerObject layerObject in _layerObjectsUnderCursor)
                {
                    if (layerObject.IsLinked && layerObject.Visible)
                    {
                        _activeLinkedLayerObject = layerObject;
                        Invalidate(layerObject.GetLinkArea());
                        return;
                    }
                }
            }
            // If _layerObjectsUnderCursor is null, search all layer objects to find one that
            // should be made active.
            else
            {
                // Convert the mouse position to image coordinates
                Point hitPoint = GeometryMethods.InvertPoint(_transform, mousePoint);

                // Check if the mouse is over a visible linked layer object
                foreach (LayerObject layerObject in _layerObjects)
                {
                    if (layerObject.IsLinked && layerObject.Visible &&
                        layerObject.HitTest(hitPoint))
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

            // If _layerObjectsUnderCursor is initialized we can use its members
            // rather than iterating all layer objects to find one which should change the cursor.
            if (_layerObjectsUnderCursor != null)
            {
                foreach (LayerObject layerObject in _layerObjectsUnderCursor)
                {
                    if (layerObject.Selectable && layerObject.Visible)
                    {
                        // Return the sizing mouse cursor
                        return Cursors.SizeAll;
                    }
                }
            }
            // If _layerObjectsUnderCursor is null, search all layer objects to find any that should
            // change the cursor.
            else
            {
                // Convert the mouse click to image coordinates
                Point hitPoint = GeometryMethods.InvertPoint(_transform, mousePoint);

                // Iterate through all layer objects
                foreach (LayerObject layerObject in _layerObjects)
                {
                    // Check if the mouse cursor is over a layer object
                    // Only perform hit test if object is selectable and visible
                    if (layerObject.Selectable && layerObject.Visible
                        && layerObject.HitTest(hitPoint))
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
            // NOTE: As per [DNRCAU #468] This code has been refactored to translate
            // the rectangular bounds into image coordinates first and then compute
            // the height and y intersect in image coordinates.

            // Calculate the top left and bottom right points in physical coordinates
            Point[] topAndBottom = new Point[] 
            {
               new Point(rectangle.Left, rectangle.Top), 
               new Point(rectangle.Right, rectangle.Bottom)
            };

            height = rectangle.Height;

            // Convert the points from physical to logical coordinates
            if (_transform.IsInvertible)
            {
                using (Matrix inverseMatrix = _transform.Clone())
                {
                    // Invert the transformation matrix
                    inverseMatrix.Invert();

                    // Transform the top left and bottom right points to image coordinates
                    inverseMatrix.TransformPoints(topAndBottom);

                    // Compute height and start & end points based on image rotation.
                    // NOTE: The coordinates that were translated above are in original
                    // non-rotated image coordinates
                    switch (Orientation)
                    {
                        // If rotated 90 or 270 then compute X intercept and
                        // height in relation to X coordinates
                        case 90:
                        case 270:
                            {
                                // Compute the height from the X perspective
                                height = Math.Abs(topAndBottom[1].X - topAndBottom[0].X);

                                // Get the X intersect
                                int xIntersect =
                                    Math.Min(topAndBottom[0].X, topAndBottom[1].X) + height / 2;
                                points = new Point[]
                                {
                                    new Point(xIntersect, Math.Min(topAndBottom[0].Y, topAndBottom[1].Y)),
                                    new Point(xIntersect, Math.Max(topAndBottom[0].Y, topAndBottom[1].Y))
                                };
                            }
                            break;

                        // If rotated 0 or 180 compute Y intercept and height in relation to
                        // Y coordinates
                        case 0:
                        case 180:
                            {
                                // Compute the height from Y perspective
                                height = Math.Abs(topAndBottom[1].Y - topAndBottom[0].Y);

                                // Get the y intersect
                                int yIntersect = Math.Min(topAndBottom[0].Y, topAndBottom[1].Y) + height / 2;

                                // Compute the start and end points in image coordinates
                                points = new Point[]
                                {
                                    new Point(Math.Min(topAndBottom[0].X, topAndBottom[1].X), yIntersect),
                                    new Point(Math.Max(topAndBottom[0].X, topAndBottom[1].X), yIntersect)
                                };
                            }
                            break;

                        default:
                            ExtractException.ThrowLogicException("ELI30243");

                            // Dummy code since compiler can't figure out that ThrowLogicException
                            // throws an exception
                            points = new Point[] { Point.Empty };
                            height = 0;
                            break;
                    }
                }
            }
            else
            {
                // If the matrix is not invertible, these points are probably meaningless,
                // but they need to be set to something.
                int rasterLineY = rectangle.Top + rectangle.Height / 2;
                points = new Point[] 
                {
                    new Point(rectangle.Left, rasterLineY), 
                    new Point(rectangle.Right, rasterLineY)
                };
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

                // Draw the grip handles if the selection tool is activated [FIDSC #3993]
                bool drawGripPoints = _cursorTool == CursorTool.SelectLayerObject;
                foreach (LayerObject layerObject in _layerObjects.Selection)
                {
                    layerObject.DrawSelection(e.Graphics, drawGripPoints);
                }

                // Draw the link arrows
                if (_activeLinkedLayerObject != null)
                {
                    _activeLinkedLayerObject.DrawLinkArrows(e.Graphics);
                }

                // Run _trackingUpdateCall if assigned to update the tracking region and display
                // tracking related graphics.
                if (_trackingUpdateCall != null)
                {
                    _trackingUpdateCall(e);
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
        void OnImageFileChanged(ImageFileChangedEventArgs e)
        {
            if (ImageFileChanged != null)
            {
                ImageFileChanged(this, e);
            }

            // Update the anti-aliasing behavior based on registry settings
            // [DNRCAU #422]
            UseAntiAliasing = RegistryManager.UseAntiAliasing;

            // Update the annotation behavior based on registry settings
            DisplayAnnotations = RegistryManager.DisplayAnnotations;
        }

        /// <summary>
        /// Raises the <see cref="ImageFileClosing"/> event.
        /// </summary>
        /// <param name="e">An <see cref="ImageFileClosingEventArgs"/> that contains
        /// the event data.</param>
        void OnImageFileClosing(ImageFileClosingEventArgs e)
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
        void OnZoomChanged(ZoomChangedEventArgs e)
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
        void OnOrientationChanged(OrientationChangedEventArgs e)
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
        void OnCursorToolChanged(CursorToolChangedEventArgs e)
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
        void OnFitModeChanged(FitModeChangedEventArgs e)
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
        void OnPageChanged(PageChangedEventArgs e)
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
        void OnOpeningImage(OpeningImageEventArgs e)
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
        void OnLoadingNewImage(LoadingNewImageEventArgs e)
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
        void OnDisplayingPrintDialog(DisplayingPrintDialogEventArgs e)
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
        void OnFileOpenError(FileOpenErrorEventArgs e)
        {
            if (FileOpenError != null)
            {
                FileOpenError(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="CursorEnteredLayerObject"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="LayerObjectEventArgs"/>
        /// event.</param>
        void OnCursorEnteredLayerObject(LayerObjectEventArgs e)
        {
            if (CursorEnteredLayerObject != null)
            {
                CursorEnteredLayerObject(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="CursorLeftLayerObject"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="LayerObjectEventArgs"/> 
        /// event.</param>
        void OnCursorlLeftLayerObject(LayerObjectEventArgs e)
        {
            if (CursorLeftLayerObject != null)
            {
                CursorLeftLayerObject(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ImageExtracted"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        void OnImageExtracted(ImageExtractedEventArgs e)
        {
            if (ImageExtracted != null)
            {
                ImageExtracted(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="AllowHighlightStatusChanged"/> event.
        /// </summary>
        void OnAllowHighlightStatusChanged()
        {
            if (AllowHighlightStatusChanged != null)
            {
                AllowHighlightStatusChanged(this, new EventArgs());
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
                        {

                            // Get the current mouse position in image coordinates
                            Point[] mousePosition = new Point[] {new Point(e.X, e.Y)};
                            GeometryMethods.InvertPoints(_transform, mousePosition);

                            // Zoom in based on the direction of the mouse wheel event
                            Zoom(e.Delta > 0);

                            // Get the mouse position in client coordinates
                            _transform.TransformPoints(mousePosition);

                            // Adjust the scroll so that the mouse is 
                            // over the same point on the image
                            Point scroll = ScrollPosition;
                            scroll.Offset(mousePosition[0].X - e.X, mousePosition[0].Y - e.Y);
                            ScrollPosition = scroll;

                            break;
                        }

                        // Scroll left/right
                        case Keys.Shift:
                        {
                            // Check if a horizontal scroll bar is visible
                            if (HScroll)
                            {
                                // Scroll horizontally
                                Point scroll = ScrollPosition;
                                scroll.X += AutoScrollSmallChange.Width*
                                            (e.Delta > 0
                                                 ? -_MOUSEWHEEL_SCROLL_FACTOR
                                                 : _MOUSEWHEEL_SCROLL_FACTOR);
                                ScrollPosition = scroll;
                            }
                            break;
                        }

                        // Scroll up/down
                        default:
                        {
                            // Check if a vertical scroll bar is visible
                            if (VScroll)
                            {
                                // Scroll vertically
                                Point scroll = ScrollPosition;
                                scroll.Y += AutoScrollSmallChange.Height*
                                            (e.Delta > 0
                                                 ? -_MOUSEWHEEL_SCROLL_FACTOR
                                                 : _MOUSEWHEEL_SCROLL_FACTOR);
                                ScrollPosition = scroll;
                            }
                            break;
                        }
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

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Invalidated"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.Forms.InvalidateEventArgs"/>
        /// that contains the event data.</param>
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            try
            {
                // Don't invalidate if currently blocking invalidate calls.
                if (!_preventInvalidate)
                {
                    base.OnInvalidated(e);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31452", ex);
            }
        }

        #endregion OnEvents

        #region Event Handlers

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
                case PrintRange.Selection:

                    // Print the current page or view
                    PrintPage(e);
                    break;

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
                Print(false, false);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23372", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region Overrides

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

        #endregion Overrides
    }
}
