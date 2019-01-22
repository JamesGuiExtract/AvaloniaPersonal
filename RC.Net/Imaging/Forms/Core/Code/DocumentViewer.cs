using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Extract.Utilities.Forms;
using Leadtools;
using Leadtools.WinForms;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.Imaging.Forms {
    [CLSCompliant(false)]
    public partial class DocumentViewer : UserControl, IDocumentViewer, IHasShortcuts, ICanBlur {
        #region Fields

        bool _documentIsImage = true;

        #endregion Fields

        IDocumentViewer DocumentViewerControl => _documentIsImage ? (IDocumentViewer) imageViewer1 : richTextViewer1;

        public DocumentViewer () {
            InitializeComponent ();
            imageViewer1.ShortcutsManagerManager = this;
            richTextViewer1.ShortcutsManagerManager = this;
            richTextViewer1.BlurMechanism = this;
        }

        public bool AllowBandedSelection {
            get => DocumentViewerControl.AllowBandedSelection;
            set { imageViewer1.AllowBandedSelection = value; richTextViewer1.AllowBandedSelection = value; }
        }
        public bool AllowHighlight {
            get => DocumentViewerControl.AllowHighlight;
            set { imageViewer1.AllowHighlight = value; richTextViewer1.AllowHighlight = value; }
        }
        public bool AutoOcr {
            get => DocumentViewerControl.AutoOcr;
            set { imageViewer1.AutoOcr = value; richTextViewer1.AutoOcr = value; }
        }

        public bool AutoZoomed => DocumentViewerControl.AutoZoomed;

        public int AutoZoomScale {
            get => DocumentViewerControl.AutoZoomScale;
            set { imageViewer1.AutoZoomScale = value; richTextViewer1.AutoZoomScale = value; }
        }
        public bool CacheImages {
            get => DocumentViewerControl.CacheImages;
            set { imageViewer1.CacheImages = value; richTextViewer1.CacheImages = value; }
        }

        public bool CanGoToNextLayerObject => DocumentViewerControl.CanGoToNextLayerObject;

        public bool CanGoToPreviousLayerObject => DocumentViewerControl.CanGoToPreviousLayerObject;

        public bool CanZoomNext => DocumentViewerControl.CanZoomNext;

        public bool CanZoomPrevious => DocumentViewerControl.CanZoomPrevious;

        public CursorTool CursorTool {
            get => DocumentViewerControl.CursorTool;
            set => DocumentViewerControl.CursorTool = value;
        }
        public Color DefaultHighlightColor {
            get => DocumentViewerControl.DefaultHighlightColor;
            set { imageViewer1.DefaultHighlightColor = value; richTextViewer1.DefaultHighlightColor = value; }
        }
        public int DefaultHighlightHeight {
            get => DocumentViewerControl.DefaultHighlightHeight;
            set { imageViewer1.DefaultHighlightHeight = value; richTextViewer1.DefaultHighlightHeight = value; }
        }
        public RedactionColor DefaultRedactionFillColor {
            get => DocumentViewerControl.DefaultRedactionFillColor;
            set { imageViewer1.DefaultRedactionFillColor = value; richTextViewer1.DefaultRedactionFillColor = value; }
        }
        public string DefaultStatusMessage {
            get => DocumentViewerControl.DefaultStatusMessage;
            set { imageViewer1.DefaultStatusMessage = value; richTextViewer1.DefaultStatusMessage = value; }
        }
        public bool DisplayAnnotations {
            get => DocumentViewerControl.DisplayAnnotations;
            set { imageViewer1.DisplayAnnotations = value; richTextViewer1.DisplayAnnotations = value; }
        }
        public FitMode FitMode {
            get => DocumentViewerControl.FitMode;
            set { imageViewer1.FitMode = value; richTextViewer1.FitMode = value; }
        }
        public RasterImage Image {
            get => DocumentViewerControl.Image;
            set => DocumentViewerControl.Image = value;
        }

        public string ImageFile => DocumentViewerControl.ImageFile;

        public int ImageHeight => DocumentViewerControl.ImageHeight;

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ReadOnlyCollection<ImagePageData> ImagePageData {
            get => DocumentViewerControl.ImagePageData;
            set => DocumentViewerControl.ImagePageData = value;
        }

        public int ImageWidth => DocumentViewerControl.ImageWidth;

        //public RasterViewerInteractiveMode InteractiveMode {
        //    get => DocumentViewerControl.InteractiveMode;
        //    set { imageViewer1.InteractiveMode = value; richTextViewer1.InteractiveMode = value; }
        //}
        //public RasterRegionCombineMode InteractiveRegionCombineMode {
        //    get => DocumentViewerControl.InteractiveRegionCombineMode;
        //    set { imageViewer1.InteractiveRegionCombineMode = value; richTextViewer1.InteractiveRegionCombineMode = value; }
        //}
        //public RasterViewerInteractiveRegionType InteractiveRegionType {
        //    get => DocumentViewerControl.InteractiveRegionType;
        //    set { imageViewer1.InteractiveRegionType = value; richTextViewer1.InteractiveRegionType = value; }
        //}
        public bool InvertColors {
            get => DocumentViewerControl.InvertColors;
            set { imageViewer1.InvertColors = value; richTextViewer1.InvertColors = value; }
        }

        public bool IsFirstDocumentTile => DocumentViewerControl.IsFirstDocumentTile;

        public bool IsFirstPage => DocumentViewerControl.IsFirstPage;

        public bool IsFirstTile => DocumentViewerControl.IsFirstTile;

        public bool IsLastDocumentTile => DocumentViewerControl.IsLastDocumentTile;

        public bool IsLastPage => DocumentViewerControl.IsLastPage;

        public bool IsLastTile => DocumentViewerControl.IsLastTile;

        public bool IsLoadingData => DocumentViewerControl.IsLoadingData;

        public bool IsImageAvailable => DocumentViewerControl.IsImageAvailable;

        public bool IsSelectionInView => DocumentViewerControl.IsSelectionInView;

        public bool IsTracking => DocumentViewerControl.IsTracking;

        public LayerObjectsCollection LayerObjects => DocumentViewerControl.LayerObjects;

        public bool MaintainZoomLevelForNewPages {
            get => DocumentViewerControl.MaintainZoomLevelForNewPages;
            set { imageViewer1.MaintainZoomLevelForNewPages = value; richTextViewer1.MaintainZoomLevelForNewPages = value; }
        }
        public int MinimumAngularHighlightHeight {
            get => DocumentViewerControl.MinimumAngularHighlightHeight;
            set { imageViewer1.MinimumAngularHighlightHeight = value; richTextViewer1.MinimumAngularHighlightHeight = value; }
        }
        public ThreadSafeSpatialString OcrData {
            get => DocumentViewerControl.OcrData;
            set { imageViewer1.OcrData = value; richTextViewer1.OcrData = value; }
        }
        public OcrTradeoff OcrTradeoff {
            get => DocumentViewerControl.OcrTradeoff;
            set { imageViewer1.OcrTradeoff = value; richTextViewer1.OcrTradeoff = value; }
        }
        public int OpenImageFileTypeFilterIndex {
            get => DocumentViewerControl.OpenImageFileTypeFilterIndex;
            set { imageViewer1.OpenImageFileTypeFilterIndex = value; richTextViewer1.OpenImageFileTypeFilterIndex = value; }
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<string> OpenImageFileTypeFilterList => DocumentViewerControl.OpenImageFileTypeFilterList;

        public int Orientation {
            get => DocumentViewerControl.Orientation;
            set => DocumentViewerControl.Orientation = value;
        }

        public int PageCount => DocumentViewerControl.PageCount;

        public int PageNumber {
            get => DocumentViewerControl.PageNumber;
            set => DocumentViewerControl.PageNumber = value;
        }
        public bool RecognizeHighlightText {
            get => DocumentViewerControl.RecognizeHighlightText;
            set { imageViewer1.RecognizeHighlightText = value; richTextViewer1.RecognizeHighlightText = value; }
        }
        public bool RedactionMode {
            get => DocumentViewerControl.RedactionMode;
            set { imageViewer1.RedactionMode = value; richTextViewer1.RedactionMode = value; }
        }

        public IEnumerable<LayerObject> SelectedLayerObjectsOnVisiblePage => DocumentViewerControl.SelectedLayerObjectsOnVisiblePage;

        public ShortcutsManager Shortcuts => Capture ? _captureShortcuts : _mainShortcuts;

        //public RasterPaintSizeMode SizeMode {
        //    get => DocumentViewerControl.SizeMode;
        //    set { imageViewer1.SizeMode = value; richTextViewer1.SizeMode = value; }
        //}

        public LongToObjectMap SpatialPageInfos => DocumentViewerControl.SpatialPageInfos;

        public Matrix Transform => DocumentViewerControl.Transform;

        public bool UseAntiAliasing {
            get => DocumentViewerControl.UseAntiAliasing;
            set { imageViewer1.UseAntiAliasing = value; richTextViewer1.UseAntiAliasing = value; }
        }
        //public bool UseDefaultShortcuts {
        //    get => DocumentViewerControl.UseDefaultShortcuts;
        //    set { imageViewer1.UseDefaultShortcuts = value; richTextViewer1.UseDefaultShortcuts = value; }
        //}

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
        /// Loads the default shortcut keys into <see cref="_mainShortcuts"/>.
        /// </summary>
        void LoadDefaultShortcuts()
        {
            // Open an image
            _mainShortcuts[Keys.O | Keys.Control] = SelectOpenImage;

            // Close an image
            _mainShortcuts[Keys.Control | Keys.F4] = CloseImage;

            // Print an image
            _mainShortcuts[Keys.P | Keys.Control] = SelectPrint;

            // Go to the next page
            _mainShortcuts[Keys.PageDown] = GoToNextPage;

            // Go to the previous page
            _mainShortcuts[Keys.PageUp] = GoToPreviousPage;

            // Fit to page
            _mainShortcuts[Keys.P] = ToggleFitToPageMode;

            // Fit to width
            _mainShortcuts[Keys.W] = ToggleFitToWidthMode;

            // Zoom window tool
            _mainShortcuts[Keys.Z] = SelectZoomWindowTool;

            // Zoom in
            _mainShortcuts[Keys.F7] = SelectZoomIn;
            _mainShortcuts[Keys.Add | Keys.Control] = SelectZoomIn;
            _mainShortcuts[Keys.Oemplus | Keys.Control] = SelectZoomIn;

            // Zoom out
            _mainShortcuts[Keys.F8] = SelectZoomOut;
            _mainShortcuts[Keys.Subtract | Keys.Control] = SelectZoomOut;
            _mainShortcuts[Keys.OemMinus | Keys.Control] = SelectZoomOut;

            // Zoom previous
            _mainShortcuts[Keys.Back] = SelectZoomPrevious;
            _mainShortcuts[Keys.Left | Keys.Alt] = SelectZoomPrevious;

            // Zoom next
            _mainShortcuts[Keys.Right | Keys.Alt] = SelectZoomNext;

            // Pan tool
            _mainShortcuts[Keys.A] = SelectPanTool;

            // Select layer object tool
            _mainShortcuts[Keys.Escape] = SelectSelectLayerObjectsTool;

            // Highlight tool
            _mainShortcuts[Keys.H] = ToggleHighlightTool;

            // Go to first page
            _mainShortcuts[Keys.Control | Keys.Home] = GoToFirstPage;

            // Go to last page
            _mainShortcuts[Keys.Control | Keys.End] = GoToLastPage;

            // Rotate clockwise
            _mainShortcuts[Keys.R | Keys.Control] = SelectRotateClockwise;

            // Rotate all document pages clockwise
            _mainShortcuts[Keys.R | Keys.Alt] = SelectRotateAllDocumentPagesClockwise;

            // Rotate counterclockwise
            _mainShortcuts[Keys.R | Keys.Control | Keys.Shift] = SelectRotateCounterclockwise;

            // Rotate all document pages counterclockwise
            _mainShortcuts[Keys.R | Keys.Alt | Keys.Shift] = SelectRotateAllDocumentPagesCounterclockwise;

            // Delete selected highlights
            _mainShortcuts[Keys.Delete] = SelectRemoveSelectedLayerObjects;

            // Select all highlights
            _mainShortcuts[Keys.A | Keys.Control] = SelectSelectAllLayerObjects;

            // Go to previous tile
            _mainShortcuts[Keys.Oemcomma] = SelectPreviousTile;

            // Go to next tile
            _mainShortcuts[Keys.OemPeriod] = SelectNextTile;

            // Go to next layer object
            _mainShortcuts[Keys.F3] = GoToNextLayerObject;
            _mainShortcuts[Keys.Control | Keys.OemPeriod] = GoToNextLayerObject;

            // Go to previous layer object
            _mainShortcuts[Keys.F3 | Keys.Shift] = GoToPreviousLayerObject;
            _mainShortcuts[Keys.Control | Keys.Oemcomma] = GoToPreviousLayerObject;

            // Fit/shrink/enlarge selected zones.
            _mainShortcuts[Keys.K] = BlockFitSelectedZones;
            _mainShortcuts[Keys.OemMinus] = ShrinkSelectedZones;
            _mainShortcuts[Keys.Subtract] = ShrinkSelectedZones;
            _mainShortcuts[Keys.Oemplus] = EnlargeSelectedZones;
            _mainShortcuts[Keys.Add] = EnlargeSelectedZones;

            // Increase highlight height
            _captureShortcuts[Keys.Oemplus] = IncreaseHighlightHeight;
            _captureShortcuts[Keys.Add] = IncreaseHighlightHeight;

            // Decrease highlight height
            _captureShortcuts[Keys.OemMinus] = DecreaseHighlightHeight;
            _captureShortcuts[Keys.Subtract] = DecreaseHighlightHeight;
        }

        public string Watermark {
            get => DocumentViewerControl.Watermark;
            set { imageViewer1.Watermark = value; richTextViewer1.Watermark = value; }
        }
        public bool WordHighlightToolEnabled {
            get => DocumentViewerControl.WordHighlightToolEnabled;
            set { imageViewer1.WordHighlightToolEnabled = value; richTextViewer1.WordHighlightToolEnabled = value; }
        }

        public int ZoomHistoryCount => DocumentViewerControl.ZoomHistoryCount;

        [Browsable (false)]
        [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
        public ZoomInfo ZoomInfo {
            get => DocumentViewerControl.ZoomInfo;
            set { imageViewer1.ZoomInfo = value; richTextViewer1.ZoomInfo = value; }
        }

        public Rectangle PhysicalViewRectangle => DocumentViewerControl.PhysicalViewRectangle;

        public float ImageDpiY => DocumentViewerControl.ImageDpiY;

        public float ImageDpiX => DocumentViewerControl.ImageDpiX;

        public double ScaleFactor => DocumentViewerControl.ScaleFactor;

        public Color FrameColor {
            get => DocumentViewerControl.FrameColor;
            set { imageViewer1.FrameColor = value; richTextViewer1.FrameColor = value; }
        }

        public event EventHandler<EventArgs> AllowHighlightStatusChanged {
            add {
                imageViewer1.AllowHighlightStatusChanged += value;
                richTextViewer1.AllowHighlightStatusChanged += value;
            }

            remove {
                imageViewer1.AllowHighlightStatusChanged -= value;
                richTextViewer1.AllowHighlightStatusChanged -= value;
            }
        }

        public event EventHandler<BackgroundProcessStatusUpdateEventArgs> BackgroundProcessStatusUpdate {
            add {
                imageViewer1.BackgroundProcessStatusUpdate += value;
                richTextViewer1.BackgroundProcessStatusUpdate += value;
            }

            remove {
                imageViewer1.BackgroundProcessStatusUpdate -= value;
                richTextViewer1.BackgroundProcessStatusUpdate -= value;
            }
        }

        public event EventHandler<LayerObjectEventArgs> CursorEnteredLayerObject {
            add {
                imageViewer1.CursorEnteredLayerObject += value;
                richTextViewer1.CursorEnteredLayerObject += value;
            }

            remove {
                imageViewer1.CursorEnteredLayerObject -= value;
                richTextViewer1.CursorEnteredLayerObject -= value;
            }
        }

        public event EventHandler<LayerObjectEventArgs> CursorLeftLayerObject {
            add {
                imageViewer1.CursorLeftLayerObject += value;
                richTextViewer1.CursorLeftLayerObject += value;
            }

            remove {
                imageViewer1.CursorLeftLayerObject -= value;
                richTextViewer1.CursorLeftLayerObject -= value;
            }
        }

        public event EventHandler<CursorToolChangedEventArgs> CursorToolChanged {
            add {
                imageViewer1.CursorToolChanged += value;
                richTextViewer1.CursorToolChanged += value;
            }

            remove {
                imageViewer1.CursorToolChanged -= value;
                richTextViewer1.CursorToolChanged -= value;
            }
        }

        public event EventHandler<DisplayingPrintDialogEventArgs> DisplayingPrintDialog {
            add {
                imageViewer1.DisplayingPrintDialog += value;
                richTextViewer1.DisplayingPrintDialog += value;
            }

            remove {
                imageViewer1.DisplayingPrintDialog -= value;
                richTextViewer1.DisplayingPrintDialog -= value;
            }
        }

        public event EventHandler<ExtendedNavigationEventArgs> ExtendedNavigation {
            add {
                imageViewer1.ExtendedNavigation += value;
                richTextViewer1.ExtendedNavigation += value;
            }

            remove {
                imageViewer1.ExtendedNavigation -= value;
                richTextViewer1.ExtendedNavigation -= value;
            }
        }

        public event EventHandler<ExtendedNavigationCheckEventArgs> ExtendedNavigationCheck {
            add {
                imageViewer1.ExtendedNavigationCheck += value;
                richTextViewer1.ExtendedNavigationCheck += value;
            }

            remove {
                imageViewer1.ExtendedNavigationCheck -= value;
                richTextViewer1.ExtendedNavigationCheck -= value;
            }
        }

        public event EventHandler<FileOpenErrorEventArgs> FileOpenError {
            add {
                imageViewer1.FileOpenError += value;
                richTextViewer1.FileOpenError += value;
            }

            remove {
                imageViewer1.FileOpenError -= value;
                richTextViewer1.FileOpenError -= value;
            }
        }

        public event EventHandler<FitModeChangedEventArgs> FitModeChanged {
            add {
                imageViewer1.FitModeChanged += value;
                richTextViewer1.FitModeChanged += value;
            }

            remove {
                imageViewer1.FitModeChanged -= value;
                richTextViewer1.FitModeChanged -= value;
            }
        }

        public event EventHandler<ImageExtractedEventArgs> ImageExtracted {
            add {
                imageViewer1.ImageExtracted += value;
                richTextViewer1.ImageExtracted += value;
            }

            remove {
                imageViewer1.ImageExtracted -= value;
                richTextViewer1.ImageExtracted -= value;
            }
        }

        public event EventHandler<ImageFileChangedEventArgs> ImageFileChanged {
            add {
                imageViewer1.ImageFileChanged += value;
                richTextViewer1.ImageFileChanged += value;
            }

            remove {
                imageViewer1.ImageFileChanged -= value;
                richTextViewer1.ImageFileChanged -= value;
            }
        }

        public event EventHandler<ImageFileClosingEventArgs> ImageFileClosing {
            add {
                imageViewer1.ImageFileClosing += value;
                richTextViewer1.ImageFileClosing += value;
            }

            remove {
                imageViewer1.ImageFileClosing -= value;
                richTextViewer1.ImageFileClosing -= value;
            }
        }

        public event EventHandler<EventArgs> InvertColorsStatusChanged {
            add {
                imageViewer1.InvertColorsStatusChanged += value;
                richTextViewer1.InvertColorsStatusChanged += value;
            }

            remove {
                imageViewer1.InvertColorsStatusChanged -= value;
                richTextViewer1.InvertColorsStatusChanged -= value;
            }
        }

        public event EventHandler<LoadingNewImageEventArgs> LoadingNewImage {
            add {
                imageViewer1.LoadingNewImage += value;
                richTextViewer1.LoadingNewImage += value;
            }

            remove {
                imageViewer1.LoadingNewImage -= value;
                richTextViewer1.LoadingNewImage -= value;
            }
        }

        public event EventHandler<OrientationChangedEventArgs> NonDisplayedPageOrientationChanged {
            add {
                imageViewer1.NonDisplayedPageOrientationChanged += value;
                richTextViewer1.NonDisplayedPageOrientationChanged += value;
            }

            remove {
                imageViewer1.NonDisplayedPageOrientationChanged -= value;
                richTextViewer1.NonDisplayedPageOrientationChanged -= value;
            }
        }

        public event EventHandler<OcrTextEventArgs> OcrLoaded {
            add {
                imageViewer1.OcrLoaded += value;
                richTextViewer1.OcrLoaded += value;
            }

            remove {
                imageViewer1.OcrLoaded -= value;
                richTextViewer1.OcrLoaded -= value;
            }
        }

        public event EventHandler<OcrTextEventArgs> OcrTextHighlighted {
            add {
                imageViewer1.OcrTextHighlighted += value;
                richTextViewer1.OcrTextHighlighted += value;
            }

            remove {
                imageViewer1.OcrTextHighlighted -= value;
                richTextViewer1.OcrTextHighlighted -= value;
            }
        }

        public event EventHandler<OpeningImageEventArgs> OpeningImage {
            add {
                imageViewer1.OpeningImage += value;
                richTextViewer1.OpeningImage += value;
            }

            remove {
                imageViewer1.OpeningImage -= value;
                richTextViewer1.OpeningImage -= value;
            }
        }

        public event EventHandler<OrientationChangedEventArgs> OrientationChanged {
            add {
                imageViewer1.OrientationChanged += value;
                richTextViewer1.OrientationChanged += value;
            }

            remove {
                imageViewer1.OrientationChanged -= value;
                richTextViewer1.OrientationChanged -= value;
            }
        }

        public event EventHandler<PageChangedEventArgs> PageChanged {
            add {
                imageViewer1.PageChanged += value;
                richTextViewer1.PageChanged += value;
            }

            remove {
                imageViewer1.PageChanged -= value;
                richTextViewer1.PageChanged -= value;
            }
        }

        public event EventHandler<ZoomChangedEventArgs> ZoomChanged {
            add {
                imageViewer1.ZoomChanged += value;
                richTextViewer1.ZoomChanged += value;
            }

            remove {
                imageViewer1.ZoomChanged -= value;
                richTextViewer1.ZoomChanged -= value;
            }
        }

        public event EventHandler ScrollPositionChanged {
            add {
                imageViewer1.ScrollPositionChanged += value;
                richTextViewer1.ScrollPositionChanged += value;
            }

            remove {
                imageViewer1.ScrollPositionChanged -= value;
                richTextViewer1.ScrollPositionChanged -= value;
            }
        }

        public event PaintEventHandler PostImagePaint {
            add {
                imageViewer1.PostImagePaint += value;
                richTextViewer1.PostImagePaint += value;
            }

            remove {
                imageViewer1.PostImagePaint -= value;
                richTextViewer1.PostImagePaint -= value;
            }
        }

        public event EventHandler ImageChanged {
            add {
                imageViewer1.ImageChanged += value;
                richTextViewer1.ImageChanged += value;
            }

            remove {
                imageViewer1.ImageChanged -= value;
                richTextViewer1.ImageChanged -= value;
            }
        }

        public void AddDisallowedPrinter (string printerName) {
            DocumentViewerControl.AddDisallowedPrinter (printerName);
        }

        public void BeginUpdate () {
            DocumentViewerControl.BeginUpdate ();
        }

        public void BringSelectionIntoView (bool autoZoom) {
            DocumentViewerControl.BringSelectionIntoView (autoZoom);
        }

        public void CacheImage (string fileName) {
            DocumentViewerControl.CacheImage (fileName);
        }

        public void CenterAtPoint (Point pt) {
            DocumentViewerControl.CenterAtPoint (pt);
        }

        public void CenterOnLayerObjects (params LayerObject[] layerObjects) {
            DocumentViewerControl.CenterOnLayerObjects (layerObjects);
        }

        public void CloseImage () {
            DocumentViewerControl.CloseImage ();
        }

        public void CloseImage (bool unloadImage) {
            DocumentViewerControl.CloseImage (unloadImage);
        }

        public bool Contains (LayerObject layerObject) {
            return DocumentViewerControl.Contains (layerObject);
        }

        public bool Contains (RectangleF rectangle) {
            return DocumentViewerControl.Contains (rectangle);
        }

        public IEnumerable<CompositeHighlightLayerObject> CreateHighlights (SpatialString spatialString, Color color) {
            return DocumentViewerControl.CreateHighlights (spatialString, color);
        }

        public void DecreaseHighlightHeight () {
            DocumentViewerControl.DecreaseHighlightHeight ();
        }

        public void DisplayRasterImage (RasterImage image, int orientation, string imageFileName) {
            DocumentViewerControl.DisplayRasterImage (image, orientation, imageFileName);
        }

        public void EndUpdate () {
            DocumentViewerControl.EndUpdate ();
        }

        /// <summary>
        /// Establishes a connection with the specified control and any sub-controls that 
        /// implement the <see cref="IImageViewerControl"/> interface.
        /// </summary>
        /// <param name="control">The top-level control of all image viewer controls to which the 
        /// <see cref="DocumentViewer"/> class should establish a connection.</param>
        /// <remarks>The <see cref="DocumentViewer"/> control will pass itself to the specified 
        /// <paramref name="control"/> if it implements the <see cref="IImageViewerControl"/> 
        /// interface. It will also recursively pass itself to each child control of 
        /// <paramref name="control"/> and each <see cref="ToolStripItem"/> of each 
        /// <see cref="ToolStrip"/> of <paramref name="control"/>.</remarks>
        /// <seealso cref="IImageViewerControl"/>
        public void EstablishConnections (Control control) {
            try {
                if (control is IImageViewerControl imageViewerControl) {
                    imageViewerControl.ImageViewer = this;
                }

                if (control is ToolStrip toolStrip) {
                    foreach (ToolStripItem toolStripItem in toolStrip.Items) {
                        if (toolStripItem is IImageViewerControl imageViewerItem) {
                            imageViewerItem.ImageViewer = this;
                        }
                    }
                }

                if (control is MenuStrip menuStrip) {
                    foreach (ToolStripItem toolStripItem in menuStrip.Items) {
                        EstablishConnections (toolStripItem);
                    }
                }

                if (control is Form form) {
                    foreach (Form childForm in form.OwnedForms) {
                        EstablishConnections (childForm);
                    }
                }

                // Check each sub control of this control
                ControlCollection controls = control.Controls;
                foreach (Control subcontrol in controls) {
                    // Recursively establish connections with the sub control and its children
                    EstablishConnections (subcontrol);
                }

                // Recursively establish connections with the 
                // context menu strip associated with this control.
                if (control.ContextMenuStrip != null) {
                    EstablishConnections (control.ContextMenuStrip);
                }
            } catch (Exception e) {
                ExtractException ee = new ExtractException ("ELI46646",
                    "Unable to establish connections.", e);
                ee.AddDebugData ("Control", control, false);
                throw ee;
            }
        }

        /// <overloads>Establishes a connection with the specified component.</overloads>
        /// <summary>
        /// Establishes a connection with the specified tool strip item and any child tool strip 
        /// items that implement the <see cref="IImageViewerControl"/> interface.
        /// </summary>
        /// <param name="toolStripItem">The top-level tool strip item to which the 
        /// <see cref="ImageViewer"/> class should establish a connection.</param>
        /// <remarks>The <see cref="ImageViewer"/> control will pass itself to the specified 
        /// <paramref name="toolStripItem"/> if it implements the <see cref="IImageViewerControl"/> 
        /// interface. It will also recursively pass itself to each child tool strip item of 
        /// <paramref name="toolStripItem"/>.</remarks>
        /// <seealso cref="IImageViewerControl"/>
        public void EstablishConnections (ToolStripItem toolStripItem) {
            try {
                // Check whether this control is an image viewer control
                IImageViewerControl imageViewerControl = toolStripItem as IImageViewerControl;
                if (imageViewerControl != null) {
                    // Pass a reference to this image viewer
                    imageViewerControl.ImageViewer = this;
                }

                // Check whether this is a tool strip menu item
                ToolStripMenuItem menuItem = toolStripItem as ToolStripMenuItem;
                if (menuItem != null) {
                    // Iterate through each child tool strip item
                    foreach (ToolStripItem childItem in menuItem.DropDownItems) {
                        // Recursively establish connections with this item and its children
                        EstablishConnections (childItem);
                    }
                }
            } catch (Exception e) {
                ExtractException ee = new ExtractException ("ELI21597",
                    "Unable to establish connections.", e);
                ee.AddDebugData ("Tool strip item", toolStripItem == null ? "null" :
                    toolStripItem.Name, false);
                throw ee;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<LayerObject> GetLayeredObjects (IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement) {
            return DocumentViewerControl.GetLayeredObjects (requiredTags, tagsArgumentRequirement);
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<LayerObject> GetLayeredObjects (IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement, IEnumerable<Type> requiredTypes, ArgumentRequirement requiredTypesArgumentRequirement, IEnumerable<Type> excludeTypes, ArgumentRequirement excludeTypesArgumentRequirement) {
            return DocumentViewerControl.GetLayeredObjects (requiredTags, tagsArgumentRequirement, requiredTypes, requiredTypesArgumentRequirement, excludeTypes, excludeTypesArgumentRequirement);
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<LayerObject> GetLayeredObjectsOnPage (int pageNumber, IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement) {
            return DocumentViewerControl.GetLayeredObjectsOnPage (pageNumber, requiredTags, tagsArgumentRequirement);
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<LayerObject> GetLayeredObjectsOnPage (int pageNumber, IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement, IEnumerable<Type> requiredTypes, ArgumentRequirement requiredTypesArgumentRequirement, IEnumerable<Type> excludeTypes, ArgumentRequirement excludeTypesArgumentRequirement) {
            return DocumentViewerControl.GetLayeredObjectsOnPage (pageNumber, requiredTags, tagsArgumentRequirement, requiredTypes, requiredTypesArgumentRequirement, excludeTypes, excludeTypesArgumentRequirement);
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public LayerObject GetLayerObjectAtPoint<T> (IEnumerable<LayerObject> layerObjects, int x, int y, bool onlySelectableObjects) where T : LayerObject {
            return DocumentViewerControl.GetLayerObjectAtPoint<T> (layerObjects, x, y, onlySelectableObjects);
        }

        public SpatialString GetOcrTextFromZone (RasterZone rasterZone) {
            return DocumentViewerControl.GetOcrTextFromZone (rasterZone);
        }

        public ImagePageProperties GetPageProperties (int pageNumber) {
            return DocumentViewerControl.GetPageProperties (pageNumber);
        }

        public double GetScaleFactorY () {
            return DocumentViewerControl.GetScaleFactorY ();
        }

        public Cursor GetSelectionCursor (int mouseX, int mouseY) {
            return DocumentViewerControl.GetSelectionCursor (mouseX, mouseY);
        }

        public Rectangle GetTransformedRectangle (Rectangle rectangle, bool clientToImage) {
            return DocumentViewerControl.GetTransformedRectangle (rectangle, clientToImage);
        }

        public Rectangle GetVisibleImageArea () {
            return DocumentViewerControl.GetVisibleImageArea ();
        }

        public void GoToFirstPage () {
            DocumentViewerControl.GoToFirstPage ();
        }

        public void GoToLastPage () {
            DocumentViewerControl.GoToLastPage ();
        }

        public void GoToNextLayerObject () {
            DocumentViewerControl.GoToNextLayerObject ();
        }

        public void GoToNextPage () {
            DocumentViewerControl.GoToNextPage ();
        }

        public void GoToNextVisibleLayerObject (bool selectObject) {
            DocumentViewerControl.GoToNextVisibleLayerObject (selectObject);
        }

        public void GoToPreviousLayerObject () {
            DocumentViewerControl.GoToPreviousLayerObject ();
        }

        public void GoToPreviousPage () {
            DocumentViewerControl.GoToPreviousPage ();
        }

        public void GoToPreviousVisibleLayerObject (bool selectObject) {
            DocumentViewerControl.GoToPreviousVisibleLayerObject (selectObject);
        }

        public void IncreaseHighlightHeight () {
            DocumentViewerControl.IncreaseHighlightHeight ();
        }

        public bool Intersects (LayerObject layerObject) {
            return DocumentViewerControl.Intersects (layerObject);
        }

        public bool Intersects (RectangleF rectangle) {
            return DocumentViewerControl.Intersects (rectangle);
        }

        public void OpenImage (string fileName, bool updateMruList) {
            OpenImage (fileName, updateMruList, true);
        }

        public void OpenImage (string fileName, bool updateMruList, bool refreshBeforeLoad) {
            SetViewerBasedOnFileType (fileName);
            DocumentViewerControl.OpenImage (fileName, updateMruList, refreshBeforeLoad);
        }

        public Rectangle PadViewingRectangle (Rectangle viewRectangle, int horizontalPadding, int verticalPadding, bool transferPaddingIfOffPage) {
            return DocumentViewerControl.PadViewingRectangle (viewRectangle, horizontalPadding, verticalPadding, transferPaddingIfOffPage);
        }

        public void PaintToGraphics (Graphics graphics, Rectangle clip, Point centerPoint, double scaleFactor) {
            DocumentViewerControl.PaintToGraphics (graphics, clip, centerPoint, scaleFactor);
        }

        public void Print () {
            DocumentViewerControl.Print ();
        }

        public void PrintPreview () {
            DocumentViewerControl.PrintPreview ();
        }

        public void PrintView () {
            DocumentViewerControl.PrintView ();
        }

        public void RemoveDisallowedPrinter (string printerName) {
            DocumentViewerControl.RemoveDisallowedPrinter (printerName);
        }

        public void RestoreNonSelectionZoom () {
            DocumentViewerControl.RestoreNonSelectionZoom ();
        }

        public void RestoreScrollPosition () {
            DocumentViewerControl.RestoreScrollPosition ();
        }

        public void Rotate (int angle, bool updateZoomHistory, bool raiseZoomChanged) {
            DocumentViewerControl.Rotate (angle, updateZoomHistory, raiseZoomChanged);
        }

        public void RotateAllDocumentPages (int angle, bool updateZoomHistory, bool raiseZoomChanged) {
            DocumentViewerControl.RotateAllDocumentPages (angle, updateZoomHistory, raiseZoomChanged);
        }

        public void SaveImage (string fileName, RasterImageFormat format) {
            DocumentViewerControl.SaveImage (fileName, format);
        }

        public void SelectAngularHighlightTool () {
            DocumentViewerControl.SelectAngularHighlightTool ();
        }

        public void SelectDeleteLayerObjectsTool () {
            DocumentViewerControl.SelectDeleteLayerObjectsTool ();
        }

        public void SelectEditHighlightTextTool () {
            DocumentViewerControl.SelectEditHighlightTextTool ();
        }

        public void SelectFirstDocumentTile (double scaleFactor, FitMode fitMode) {
            DocumentViewerControl.SelectFirstDocumentTile (scaleFactor, fitMode);
        }

        public void SelectLastDocumentTile (double scaleFactor, FitMode fitMode) {
            DocumentViewerControl.SelectLastDocumentTile (scaleFactor, fitMode);
        }

        public void SelectNextTile () {
            DocumentViewerControl.SelectNextTile ();
        }

        public void SelectOpenImage () {
            DocumentViewerControl.SelectOpenImage ();
        }

        public void SelectPanTool () {
            DocumentViewerControl.SelectPanTool ();
        }

        public void SelectPreviousTile () {
            DocumentViewerControl.SelectPreviousTile ();
        }

        public void SelectPrint () {
            DocumentViewerControl.SelectPrint ();
        }

        public void SelectPrintView () {
            DocumentViewerControl.SelectPrintView ();
        }

        public void SelectRectangularHighlightTool () {
            DocumentViewerControl.SelectRectangularHighlightTool ();
        }

        public void SelectRemoveSelectedLayerObjects () {
            DocumentViewerControl.SelectRemoveSelectedLayerObjects ();
        }

        public void SelectRotateAllDocumentPagesClockwise () {
            DocumentViewerControl.SelectRotateAllDocumentPagesClockwise ();
        }

        public void SelectRotateAllDocumentPagesCounterclockwise () {
            DocumentViewerControl.SelectRotateAllDocumentPagesCounterclockwise ();
        }

        public void SelectRotateClockwise () {
            DocumentViewerControl.SelectRotateClockwise ();
        }

        public void SelectRotateCounterclockwise () {
            DocumentViewerControl.SelectRotateCounterclockwise ();
        }

        public void SelectSelectAllLayerObjects () {
            DocumentViewerControl.SelectSelectAllLayerObjects ();
        }

        public void SelectSelectLayerObjectsTool () {
            DocumentViewerControl.SelectSelectLayerObjectsTool ();
        }

        public void SelectWordHighlightTool () {
            DocumentViewerControl.SelectWordHighlightTool ();
        }

        public void SelectZoomIn () {
            DocumentViewerControl.SelectZoomIn ();
        }

        public void SelectZoomNext () {
            DocumentViewerControl.SelectZoomNext ();
        }

        public void SelectZoomOut () {
            DocumentViewerControl.SelectZoomOut ();
        }

        public void SelectZoomPrevious () {
            DocumentViewerControl.SelectZoomPrevious ();
        }

        public void SelectZoomWindowTool () {
            DocumentViewerControl.SelectZoomWindowTool ();
        }

        public void SetVisibleStateForSpecifiedLayerObjects (IEnumerable<string> tags, IEnumerable<Type> includeTypes, bool visibleState) {
            DocumentViewerControl.SetVisibleStateForSpecifiedLayerObjects (tags, includeTypes, visibleState);
        }

        public void ShowPrintPageSetupDialog () {
            DocumentViewerControl.ShowPrintPageSetupDialog ();
        }

        public void ToggleFitToPageMode () {
            DocumentViewerControl.ToggleFitToPageMode ();
        }

        public void ToggleFitToWidthMode () {
            DocumentViewerControl.ToggleFitToWidthMode ();
        }

        public void ToggleHighlightTool () {
            DocumentViewerControl.ToggleHighlightTool ();
        }

        public void ToggleRedactionTool () {
            DocumentViewerControl.ToggleRedactionTool ();
        }

        public void UnloadImage (string fileName) {
            DocumentViewerControl.UnloadImage (fileName);
        }

        public void UpdateCursor () {
            DocumentViewerControl.UpdateCursor ();
        }

        public bool WaitForOcrData () {
            return DocumentViewerControl.WaitForOcrData ();
        }

        public void ZoomIn () {
            DocumentViewerControl.ZoomIn ();
        }

        public void ZoomNext () {
            DocumentViewerControl.ZoomNext ();
        }

        public void ZoomOut () {
            DocumentViewerControl.ZoomOut ();
        }

        public void ZoomPrevious () {
            DocumentViewerControl.ZoomPrevious ();
        }

        public void ZoomToRectangle (Rectangle rc) {
            DocumentViewerControl.ZoomToRectangle (rc);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308")]
        private void SetViewerBasedOnFileType (string fileName) {
            string ext = Path.GetExtension (fileName).ToLowerInvariant ();
            _documentIsImage = !(ext == ".txt" || ext == ".rtf");
            if (_documentIsImage) {
                imageViewer1.Visible = true;
                richTextViewer1.Visible = false;
                LayerObject.MinSize = LayerObject.DefaultMinSize;
            } else {
                imageViewer1.Visible = false;
                richTextViewer1.Visible = true;
                LayerObject.MinSize = new Size (0, 0);
            }
        }

        public void EnlargeSelectedZones () {
            DocumentViewerControl.EnlargeSelectedZones ();
        }

        public void BlockFitSelectedZones () {
            DocumentViewerControl.BlockFitSelectedZones ();
        }

        public void ShrinkSelectedZones () {
            DocumentViewerControl.ShrinkSelectedZones ();
        }

        protected override void OnInvalidated (InvalidateEventArgs e) {
            if (_documentIsImage) {
                imageViewer1.Invalidate ();
            } else {
                richTextViewer1.Invalidate ();
            }

            base.OnInvalidated (e);
        }

        public void Blur()
        {
            blur.Focus();
        }
    }
}