using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Leadtools;
using Leadtools.WinForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.Imaging.Forms
{
    [CLSCompliant(false)]
    public partial class RichTextViewer : RichTextBox, IDocumentViewer
    {
        #region Static Fields

        /// <summary>
        /// Contains the <see cref="ImageViewerCursors"/> for each <see cref="CursorTool"/>.
        /// </summary>
        static Dictionary<CursorTool, ImageViewerCursors> _cursorsForCursorTools;

        /// <summary>
        /// The cursors to be displayed based upon the currently active <see cref="CursorTool"/>.
        /// </summary>
        ImageViewerCursors _cursors;

        private static readonly Color BACKGROUND_COLOR = Color.Transparent;
        private static readonly Color TEXT_COLOR = Color.Black;
        private static readonly Color ITEM_TEXT_COLOR = Color.White;
        private static readonly Color CLUE_BACKGROUND_COLOR = Color.LightGray;
        private static readonly Color SELECTED_BACKGROUND_COLOR = Color.LimeGreen;

        #endregion Static Fields

        #region Fields

        Control _parentForm;

        bool _allowHighlight = true;
        CursorTool _cursorTool;
        SpatialString _uss;
        (int index, int length)[] _displayedTextToRawPositions;
        (int index, int length)[] _displayedTextToSpatialStringPositions;
        (int index, int length)[] _rawTextToDisplayedTextPositions;

        bool _loading = false;
        private bool _invalidating;
        private bool _suspendTracking;
        private const int WM_SETREDRAW = 0x000B;
        private const int WM_SETFOCUS = 0x0007;
        private const int WM_KILLFOCUS = 0x0008;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public RichTextViewer()
        {
            InitializeComponent();
            ReadOnly = true;
            WordWrap = false;
            LayerObjects = LayerObjectsCollection.CreateLayerObjectsWithSelection();
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Start redaction drawing if mouse button is down and there is selected text
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            try
            {
                if (SelectionLength > 0 && e.Button == MouseButtons.Left)
                {
                    IsTracking = true;
                }

                base.OnMouseMove(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI48317");
            }
        }

        /// <summary>
        /// Create redaction or clear selection (cancel redaction drawing)
        /// </summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            try
            {
                if (IsTracking)
                {
                    bool cancel = e.Button != MouseButtons.Left;
                    EndTracking(cancel);
                }
                else
                {
                    switch (CursorTool)
                    {
                        case CursorTool.SelectLayerObject:
                            SelectRedactionOrClue(e);
                            break;
                    }
                }

                base.OnMouseUp(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI48318");
            }
        }

        /// <summary>
        /// Cancel redaction drawing if IsTracking
        /// </summary>
        protected override void OnLostFocus(EventArgs e)
        {
            try
            {
                base.OnLostFocus(e);

                if (IsTracking && !_suspendTracking)
                {
                    EndTracking(true);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI48319");
            }
        }

        /// <summary>
        /// Create a redaction if current tool is a redaction tool
        /// </summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            try
            {
                base.OnMouseDoubleClick(e);

                switch (CursorTool)
                {
                    case CursorTool.AngularRedaction:
                    case CursorTool.RectangularRedaction:
                    case CursorTool.WordRedaction:
                        CreateRedaction();
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI48320");
            }
        }

        /// <summary>
        /// Set highlight color
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            // Prevent infinite recursion
            if (_invalidating) return;

            try
            {
                _invalidating = true;

                Draw(() =>
                {
                    var previousSelectionStart = SelectionStart;
                    var previousSelectionLength = SelectionLength;
                    SelectAll();
                    SelectionBackColor = BACKGROUND_COLOR;
                    SelectionColor = TEXT_COLOR;
                    Select(previousSelectionStart, previousSelectionLength);
                });
                    
                foreach (var layerObject in LayerObjects.Except(LayerObjects.Selection).OfType<Redaction>().ToList())
                {
                    SelectRedactionOrClue(layerObject, false);
                }

                foreach (var layerObject in LayerObjects.Selection.OfType<Redaction>().ToList())
                {
                    SelectRedactionOrClue(layerObject, true);
                }

                base.OnInvalidated(e);

                Invalidate();
                Focus();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI48316");
            }
            finally
            {
                _invalidating = false;
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);

            IsTracking = false;
        }

        #endregion Overrides

        #region IDocumentViewer

        public bool AllowBandedSelection { get; set; }

        public bool AllowHighlight
        {
            get => _allowHighlight;
            set
            {
                _allowHighlight = value;
                AllowHighlightStatusChanged?.Invoke(this, new EventArgs());
            }
        }

        public bool AutoOcr { get; set; }

        public bool AutoZoomed => false;

        public int AutoZoomScale { get; set; }

        public bool CacheImages { get; set; }

        public bool CanGoToNextLayerObject => false;

        public bool CanGoToPreviousLayerObject => false;

        public bool CanZoomNext => false;

        public bool CanZoomPrevious => false;

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

                    // If a tracking event is currently in progress, cancel it.
                    if (IsTracking)
                    {
                        EndTracking(true);
                    }

                    // Ensure an image is open
                    ExtractException.Assert("ELI48321", "Image must be open.",
                        IsImageAvailable || value == CursorTool.None);

                    // Set the cursor tool
                    _cursorTool = value;

                    // Get the cursors for the current tool
                    _cursors = GetCursorsForTool(value);

                    // Set the current cursor
                    UpdateCursor();

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

                    CursorToolChanged?.Invoke(this, new CursorToolChangedEventArgs(_cursorTool));
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI48322",
                        "Unable to set cursor tool.", e);
                    ee.AddDebugData("Cursor tool", value, false);
                    throw ee;
                }
            }
        }

        public Color DefaultHighlightColor { get; set; }

        public int DefaultHighlightHeight { get; set; }

        public RedactionColor DefaultRedactionFillColor { get; set; }

        public string DefaultStatusMessage { get; set; }

        public bool DisplayAnnotations { get; set; }

        public FitMode FitMode { get; set; }

        public RasterImage Image { get; set; }

        public string ImageFile { get; private set; }

        public int ImageHeight => 1;

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ReadOnlyCollection<ImagePageData> ImagePageData { get; set; }

        public int ImageWidth { get; }

        public RasterViewerInteractiveMode InteractiveMode { get; set; }

        public RasterRegionCombineMode InteractiveRegionCombineMode { get; set; }

        public RasterViewerInteractiveRegionType InteractiveRegionType { get; set; }

        public bool InvertColors { get; set; }

        public bool IsFirstDocumentTile => true;

        public bool IsFirstPage => true;

        public bool IsFirstTile => true;

        public bool IsLastDocumentTile => true;

        public bool IsLastPage => true;

        public bool IsLastTile => true;

        public bool IsLoadingData => false;

        public bool IsSelectionInView => true;

        public bool IsTracking { get; private set; } = false;

        public LayerObjectsCollection LayerObjects { get; private set; }

        public bool MaintainZoomLevelForNewPages { get; set; }
        public int MinimumAngularHighlightHeight { get; set; }
        public ThreadSafeSpatialString OcrData { get; set; }
        public OcrTradeoff OcrTradeoff { get; set; }
        public int OpenImageFileTypeFilterIndex { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<string> OpenImageFileTypeFilterList => new List<string> { ".txt" };

        public int Orientation { get; set; }

        public int PageCount => 1;

        public int PageNumber { get; set; } = 1;
        public bool RecognizeHighlightText { get; set; } = true;
        public bool RedactionMode { get; set; } = true;

        public IEnumerable<LayerObject> SelectedLayerObjectsOnVisiblePage => new LayerObjectsCollection();

        public RasterPaintSizeMode SizeMode { get; set; }

        public LongToObjectMap SpatialPageInfos => null;

        public Matrix Transform => new Matrix();

        public bool UseAntiAliasing { get; set; }

        public IHasShortcuts ShortcutsManagerManager { get; set; }

        public ICanBlur BlurMechanism { get; set; }

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
                return ShortcutsManagerManager?.Shortcuts;
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
                return ShortcutsManagerManager?.UseDefaultShortcuts ?? false;
            }
            set
            {
                if (ShortcutsManagerManager != null)
                {
                    ShortcutsManagerManager.UseDefaultShortcuts = value;
                }
            }
        }

        public string Watermark { get; set; }
        public bool WordHighlightToolEnabled { get; set; } = false;

        public int ZoomHistoryCount => 0;

        public ZoomInfo ZoomInfo { get; set; }

        public bool IsImageAvailable => !string.IsNullOrEmpty(ImageFile);

        public Rectangle PhysicalViewRectangle => ClientRectangle;

        public float ImageDpiY => 300;

        public float ImageDpiX => 300;

        public double ScaleFactor => 1;

        public Color FrameColor { get; set; }

        public event EventHandler<EventArgs> AllowHighlightStatusChanged;
        public event EventHandler<CursorToolChangedEventArgs> CursorToolChanged;
        public event EventHandler<ImageFileChangedEventArgs> ImageFileChanged;
        public event EventHandler<OpeningImageEventArgs> OpeningImage;
        public event EventHandler<PageChangedEventArgs> PageChanged;

#pragma warning disable 0067
        public event EventHandler<BackgroundProcessStatusUpdateEventArgs> BackgroundProcessStatusUpdate;
        public event EventHandler<LayerObjectEventArgs> CursorEnteredLayerObject;
        public event EventHandler<LayerObjectEventArgs> CursorLeftLayerObject;
        public event EventHandler<DisplayingPrintDialogEventArgs> DisplayingPrintDialog;
        public event EventHandler<ExtendedNavigationEventArgs> ExtendedNavigation;
        public event EventHandler<ExtendedNavigationCheckEventArgs> ExtendedNavigationCheck;
        public event EventHandler<FileOpenErrorEventArgs> FileOpenError;
        public event EventHandler<FitModeChangedEventArgs> FitModeChanged;
        public event EventHandler<ImageExtractedEventArgs> ImageExtracted;
        public event EventHandler<ImageFileClosingEventArgs> ImageFileClosing;
        public event EventHandler<EventArgs> InvertColorsStatusChanged;
        public event EventHandler<LoadingNewImageEventArgs> LoadingNewImage;
        public event EventHandler<OrientationChangedEventArgs> NonDisplayedPageOrientationChanged;
        public event EventHandler<OcrTextEventArgs> OcrLoaded;
        public event EventHandler<OcrTextEventArgs> OcrTextHighlighted;
        public event EventHandler<OrientationChangedEventArgs> OrientationChanged;
        public event EventHandler<ZoomChangedEventArgs> ZoomChanged;
        public event EventHandler ScrollPositionChanged;
        public event PaintEventHandler PostImagePaint;
        public event EventHandler ImageChanged;
#pragma warning restore 0067

        public void AddDisallowedPrinter(string printerName)
        {
        }

        public void BeginUpdate()
        {
        }

        public void BringSelectionIntoView(bool autoZoom)
        {
        }

        public void CacheImage(string fileName)
        {
        }

        public void CenterAtPoint(Point pt)
        {
        }

        public void CenterOnLayerObjects(params LayerObject[] layerObjects)
        {
        }

        public void CloseImage()
        {
            CloseImage(true);
        }

        public void CloseImage(bool unloadImage)
        {
            try
            {
                ImageFile = "";
                LayerObjects = LayerObjectsCollection.CreateLayerObjectsWithSelection();
                ImageFileChanged?.Invoke(this, new ImageFileChangedEventArgs(""));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48384");
            }
        }

        public bool Contains(LayerObject layerObject)
        {
            return false;
        }

        public bool Contains(RectangleF rectangle)
        {
            return false;
        }

        public IEnumerable<CompositeHighlightLayerObject> CreateHighlights(SpatialString spatialString, Color color)
        {
            return null;
        }

        public void DecreaseHighlightHeight()
        {
        }

        public void DisplayRasterImage(RasterImage image, int orientation, string imageFileName)
        {
        }

        public void EndUpdate()
        {
        }

        public void EstablishConnections(Control control)
        {
        }

        public void EstablishConnections(ToolStripItem toolStripItem)
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<LayerObject> GetLayeredObjects(IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement)
        {
            return new List<LayerObject>();
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<LayerObject> GetLayeredObjects(IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement, IEnumerable<Type> requiredTypes, ArgumentRequirement requiredTypesArgumentRequirement, IEnumerable<Type> excludeTypes, ArgumentRequirement excludeTypesArgumentRequirement)
        {
            return new List<LayerObject>();
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<LayerObject> GetLayeredObjectsOnPage(int pageNumber, IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement)
        {
            return new List<LayerObject>();
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<LayerObject> GetLayeredObjectsOnPage(int pageNumber, IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement, IEnumerable<Type> requiredTypes, ArgumentRequirement requiredTypesArgumentRequirement, IEnumerable<Type> excludeTypes, ArgumentRequirement excludeTypesArgumentRequirement)
        {
            return new List<LayerObject>();
        }

        public SpatialString GetOcrTextFromZone(RasterZone rasterZone)
        {
            return new SpatialStringClass();
        }

        public ImagePageProperties GetPageProperties(int pageNumber)
        {
            int length = (int)new FileInfo(ImageFile).Length;
            return new ImagePageProperties(length, 1, 300, 300);
        }

        public double GetScaleFactorY()
        {
            return 1;
        }

        public Rectangle GetTransformedRectangle(Rectangle rectangle, bool clientToImage)
        {
            return new Rectangle();
        }

        public Rectangle GetVisibleImageArea()
        {
            return new Rectangle();
        }

        public void GoToFirstPage()
        {
        }

        public void GoToLastPage()
        {
        }

        public void GoToNextLayerObject()
        {
        }

        public void GoToNextPage()
        {
        }

        public void GoToNextVisibleLayerObject(bool selectObject)
        {
        }

        public void GoToPreviousLayerObject()
        {
        }

        public void GoToPreviousPage()
        {
        }

        public void GoToPreviousVisibleLayerObject(bool selectObject)
        {
        }

        public void IncreaseHighlightHeight()
        {
        }

        public bool Intersects(LayerObject layerObject)
        {
            return false;
        }

        public bool Intersects(RectangleF rectangle)
        {
            return false;
        }

        public void OpenImage(string fileName, bool updateMruList)
        {
            OpenImage(fileName, updateMruList, true);
        }

        /// <summary>
        /// Opens a text file and initializes positional data
        /// </summary>
        /// <param name="fileName">Text or rich text file path</param>
        /// <param name="updateMruList">Whether to update the most recently used file list</param>
        /// <param name="refreshBeforeLoad">Whether to call Refresh() before loading the file</param>
        public void OpenImage(string fileName, bool updateMruList, bool refreshBeforeLoad)
        {
            try
            {
                using (new TemporaryWaitCursor())
                {
                    _loading = true;

                    if (refreshBeforeLoad)
                    {
                        Refresh();
                    }

                    // Raise the opening image event
                    OpeningImageEventArgs opening = new OpeningImageEventArgs(fileName, updateMruList);
                    OpeningImage?.Invoke(this, opening);

                    // Check if the event was cancelled
                    if (opening.Cancel)
                    {
                        return;
                    }

                    // Clear text without raising the image file changed event.
                    if (IsImageAvailable)
                    {
                        Clear();
                    }

                    // Raise the LoadingNewImage event
                    LoadingNewImage?.Invoke(this, new LoadingNewImageEventArgs());

                    if (refreshBeforeLoad)
                    {
                        // Refresh the image viewer before opening the new image
                        RefreshImageViewerAndParent();
                    }

                    SetTextAndPositionData(fileName);

                    // Store the image file name
                    ImageFile = fileName;

                    // Set the cursor tool to SelectLayerObject if none is set
                    if (CursorTool == CursorTool.None)
                    {
                        CursorTool = CursorTool.SelectLayerObject;
                    }

                    // Update the MRU list if needed
                    if (updateMruList)
                    {
                        RegistryManager.AddMostRecentlyUsedImageFile(fileName);
                    }

                    // Raise the image file changed event
                    ImageFileChanged?.Invoke(this, new ImageFileChangedEventArgs(fileName));

                    // Raise the on page changed event
                    PageChanged?.Invoke(this, new PageChangedEventArgs(PageNumber));

                    // Restore the cursor for the active cursor tool.
                    UpdateCursor();

                    AllowHighlight = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46647");
            }
            finally
            {
                _loading = false;
            }
        }

        public Rectangle PadViewingRectangle(Rectangle viewRectangle, int horizontalPadding, int verticalPadding, bool transferPaddingIfOffPage)
        {
            return new Rectangle();
        }

        public void PaintToGraphics(Graphics graphics, Rectangle clip, Point centerPoint, double scaleFactor)
        {
        }

        public void Print()
        {
        }

        public void PrintPreview()
        {
        }

        public void PrintView()
        {
        }

        public void RemoveDisallowedPrinter(string printerName)
        {
        }

        public void RestoreNonSelectionZoom()
        {
        }

        public void RestoreScrollPosition()
        {
        }

        public void Rotate(int angle, bool updateZoomHistory, bool raiseZoomChanged)
        {
        }

        public void RotateAllDocumentPages(int angle, bool updateZoomHistory, bool raiseZoomChanged)
        {
        }

        public void SaveImage(string fileName, RasterImageFormat format)
        {
        }

        public void SelectAngularHighlightTool()
        {
        }

        public void SelectDeleteLayerObjectsTool()
        {
        }

        public void SelectEditHighlightTextTool()
        {
        }

        public void SelectFirstDocumentTile(double scaleFactor, FitMode fitMode)
        {
        }

        public void SelectLastDocumentTile(double scaleFactor, FitMode fitMode)
        {
        }

        public void SelectNextTile()
        {
        }

        public void SelectOpenImage()
        {
        }

        public void SelectPanTool()
        {
        }

        public void SelectPreviousTile()
        {
        }

        public void SelectPrint()
        {
        }

        public void SelectPrintView()
        {
        }

        public void SelectRectangularHighlightTool()
        {
        }

        public void SelectRemoveSelectedLayerObjects()
        {
            try 
            {
                // We are done if there are no selected layer objects
                if (LayerObjects.Selection.Count <= 0)
                {
                    return;
                }

                // Build a delete me collection of all selected objects that are deletable
                LayerObjectsCollection deleteMe = new LayerObjectsCollection();
                foreach (LayerObject layerObject in LayerObjects.Selection)
                {
                    if (layerObject.Deletable)
                    {
                        deleteMe.Add(layerObject);
                    }
                }

                // Check if there are non deleteable objects selected
                bool nonDeletableObjectSelected = deleteMe.Count != LayerObjects.Selection.Count;

                if (deleteMe.Count > 0)
                {
                    // Delete the selected layer objects
                    LayerObjects.Remove(deleteMe, true, true);
                }

                // Refresh the image viewer
                Invalidate();

                if (nonDeletableObjectSelected)
                {
                    // Prompt if there were non-deletable objects that were not removed
                    using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
                    {
                        messageBox.Text = "The selection contained non-deletable objects. These objects have not been removed.";
                        messageBox.Caption = "Some Objects Not Deleted";
                        messageBox.StandardIcon = MessageBoxIcon.Information;
                        messageBox.AddStandardButtons(MessageBoxButtons.OK);
                        messageBox.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI48323", ex);
            }
        }

        public void SelectRotateAllDocumentPagesClockwise()
        {
        }

        public void SelectRotateAllDocumentPagesCounterclockwise()
        {
        }

        public void SelectRotateClockwise()
        {
        }

        public void SelectRotateCounterclockwise()
        {
        }

        public void SelectSelectAllLayerObjects()
        {
        }

        public void SelectSelectLayerObjectsTool()
        {
            // Do not switch to the selection tool if it is already the current tool
            if (CursorTool != CursorTool.SelectLayerObject)
            {
                CursorTool = CursorTool.SelectLayerObject;
            }
        }

        public void SelectWordHighlightTool()
        {
        }

        public void SelectZoomIn()
        {
        }

        public void SelectZoomNext()
        {
        }

        public void SelectZoomOut()
        {
        }

        public void SelectZoomPrevious()
        {
        }

        public void SelectZoomWindowTool()
        {
        }

        public void SetVisibleStateForSpecifiedLayerObjects(IEnumerable<string> tags, IEnumerable<Type> includeTypes, bool visibleState)
        {
        }

        public void ShowPrintPageSetupDialog()
        {
        }

        public void ToggleFitToPageMode()
        {
        }

        public void ToggleFitToWidthMode()
        {
        }

        public void ToggleHighlightTool()
        {
        }

        public void ToggleRedactionTool()
        {
            try
            {
                switch (_cursorTool)
                {
                    case CursorTool.AngularRedaction:
                        CursorTool = CursorTool.RectangularRedaction;
                        break;

                    case CursorTool.RectangularRedaction:
                        CursorTool = CursorTool.WordRedaction;
                        break;

                    case CursorTool.WordRedaction:
                        CursorTool = CursorTool.AngularRedaction;
                        break;

                    default:
                        // Select the last used redaction tool
                        CursorTool = RegistryManager.GetLastUsedRedactionTool();
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48324");
            }
        }

        public void UnloadImage(string fileName)
        {
        }

        /// <summary>
        /// Updates <see cref="Cursor"/> based upon the current state of the current cursor tool,
        /// state of the image viewer, key state and mouse position.
        /// </summary>
        public void UpdateCursor()
        {
            try
            {
                Cursor oldCursor = Cursor;

                Cursor newCursor = Cursors.Default;

                if (_cursors != null)
                {
                    if (IsTracking && CursorTool == CursorTool.SelectLayerObject)
                    {
                        // Don't allow a cursor change during a tracking event with the selection cursor
                        // active. The Highlight class uses the cursor as a flag to indicated the type
                        // of tracking event that is active.
                        newCursor = oldCursor;
                    }
                    else if (IsImageAvailable && CursorTool == CursorTool.SelectLayerObject)
                    {
                        // If using the selection cursor, get the appropriate one based on the mouse
                        // position.
                        newCursor = GetSelectionCursor(PointToClient(MousePosition));
                    }
                    else if (IsTracking && _cursors.Active != null)
                    {
                        newCursor = _cursors.Active;
                    }
                    else if ((ModifierKeys == Keys.Shift) && _cursors.ShiftState != null)
                    {
                        newCursor = _cursors.ShiftState;
                    }
                    else if ((ModifierKeys == (Keys.Control | Keys.Shift)) && _cursors.CtrlShiftState != null)
                    {
                        newCursor = _cursors.CtrlShiftState;
                    }
                    else if (_cursors.Tool != null)
                    {
                        newCursor = _cursors.Tool;
                    }
                }

                if (oldCursor != newCursor)
                {
                    Cursor = newCursor;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48325");
            }
        }


        public bool WaitForOcrData()
        {
            return true;
        }

        public void ZoomIn()
        {
            ZoomFactor = Math.Min(ZoomFactor * 1.1f, 63);
        }

        public void ZoomNext()
        {
        }

        public void ZoomOut()
        {
            ZoomFactor = Math.Max(ZoomFactor * .9f, 1/63);
        }

        public void ZoomPrevious()
        {
        }

        public void ZoomToRectangle(Rectangle rc)
        {
        }

        /// <summary>
        /// Gets the cursor for the select layer object tool based on the mouse position.
        /// </summary>
        /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
        /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
        /// <returns>The cursor for the select layerObject tool based on the mouse position.
        /// </returns>
        public Cursor GetSelectionCursor(int mouseX, int mouseY)
        {
            // The mouse is not over any layer object
            return Cursors.Default;
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public LayerObject GetLayerObjectAtPoint<T>(IEnumerable<LayerObject> layerObjects, int x, int y, bool onlySelectableObjects) where T : LayerObject
        {
            return null;
        }

        public void EnlargeSelectedZones()
        {
            throw new NotImplementedException();
        }

        public void BlockFitSelectedZones()
        {
            throw new NotImplementedException();
        }

        public void ShrinkSelectedZones()
        {
            throw new NotImplementedException();
        }

        #endregion IDocumentViewer

        // Internal methods are being shared with Extract.AttributeFinder.Test to do some unit tests
        #region Internal Methods

        /// <summary>
        /// Create a redaction layer object from the current selection
        /// </summary>
        internal void CreateRedaction()
        {
            Redaction redaction = null;
            Draw(() =>
            {
                // Trim trailing whitespace
                while (SelectedText.Length > 0 && Char.IsWhiteSpace(SelectedText.Last()))
                {
                    SelectionLength--;
                }

                if (!_loading && SelectedText.Length > 0)
                {
                    var zones = GetRasterZonesFromSelection().ToList();
                    Select(SelectionStart, 0);
                    redaction = new Redaction(this, PageNumber, LayerObject.ManualComment, zones, DefaultRedactionFillColor);
                }
            });

            if (redaction != null) LayerObjects.Add(redaction);
        }

        /// <summary>
        /// Append zones made from the current selection to the currently selected redaction
        /// </summary>
        internal void AppendToRedaction()
        {
            Draw(() =>
            {
                // Trim trailing whitespace
                while (SelectedText.Length > 0 && Char.IsWhiteSpace(SelectedText.Last()))
                {
                    SelectionLength--;
                }

                if (!_loading && SelectedText.Length > 0)
                {
                    // Find currently selected redaction or return if none
                    var redaction = (Redaction)LayerObjects.Selection.FirstOrDefault();
                    if (redaction == null) return;

                    var zones = GetRasterZonesFromSelection().ToList();
                    Select(SelectionStart, 0);

                    // Add zones to currently selected redaction
                    redaction.Objects.AddRange(zones.Select(zone => new Highlight(this, LayerObject.ManualComment, zone)));
                    redaction.Dirty = true;
                }
            });
        }

        #endregion Internal Methods

        #region Private Methods

        /// <summary>
        /// Will refresh the image viewer and if a parent form is found will
        /// call refresh on the parent as well.
        /// </summary>
        void RefreshImageViewerAndParent()
        {
            // If _parentForm is null attempt to get it
            if (_parentForm == null)
            {
                _parentForm = TopLevelControl as Form;
            }

            // If there is a parent form just refresh the parent
            if (_parentForm != null)
            {
                _parentForm.Refresh();
            }
            else
            {
                Refresh();
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

        // Suspend painting while updating text/background colors to make these operations quick and smooth
        private void Draw(Action drawAction)
        {
            // Don't do any updating if this control isn't supposed to be doing things
            // https://extract.atlassian.net/browse/ISSUE-16707
            if (!Visible) return;

            try
            {
                Suspend();
                drawAction();
            }
            finally
            {
                Resume();
            }
        }

        private void Suspend()
        {
            if (IsTracking)
            {
                _suspendTracking = true;
            }

            // Kill focus to prevent scrolling around when deselecting
            Blur();

            Message msgSuspendUpdate = Message.Create(Handle, WM_SETREDRAW, IntPtr.Zero,
                IntPtr.Zero);

            NativeWindow window = NativeWindow.FromHandle(Handle);
            window.DefWndProc(ref msgSuspendUpdate);
        }

        private void Resume()
        {
            _suspendTracking = false;

            // Set focus again
            Focus();

            NativeWindow window = NativeWindow.FromHandle(Handle);
            // Create a C "true" boolean as an IntPtr
            IntPtr wparam = new IntPtr(1);
            Message msgResumeUpdate = Message.Create(Handle, WM_SETREDRAW, wparam,
                IntPtr.Zero);
            window.DefWndProc(ref msgResumeUpdate);

            // Don't invalidate if already in the OnInvalidated method
            if (!_invalidating)
            {
                Invalidate();
            }
        }

        private void SetTextAndPositionData(string fileName)
        {
            var encoding = Encoding.GetEncoding("windows-1252");
            ushort[] bomCodes = new ushort[] { 0xEF, 0xBB, 0xBF };
            char[] bomChars = bomCodes.Select(x => (char)x).ToArray();

            // Allow opening of files that are also open in Word, e.g. (ReadAllText fails)
            string rawText = null;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var textReader = new StreamReader(fileStream, encoding, false)) // Match spatial string encoding and don't detect byte order so that length matches spatial string length
            {
                rawText = textReader.ReadToEnd();
            }

            _uss = new SpatialStringClass();
            if (File.Exists(fileName + ".uss"))
            {
                _uss.LoadFrom(fileName + ".uss", false);
            }
            else
            {
                _uss.LoadFrom(fileName, false);
            }
            var extractedText = _uss.String;
            var displayedText = "";

            if (fileName.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase))
            {
                // Try loading the rtf code with the base class
                Rtf = rawText;
                displayedText = Text;

                // If the base class doesn't compute the same text as RichTextExtractor
                // then show the extracted text
                if (!string.Equals(displayedText, extractedText, StringComparison.Ordinal)
                    && !string.Equals(displayedText, extractedText.TrimEnd('\n'), StringComparison.Ordinal)) // Sometimes the RichTextBox strips trailing newline
                {
                    Text = extractedText;
                    displayedText = Text;
                }
            }
            else if (string.Equals(rawText, extractedText, StringComparison.Ordinal))
            {
                Text = rawText.TrimStart(bomChars);
                displayedText = Text;
            }
            else
            {
                throw new ExtractException("ELI48363", "Logic error: Spatial string text doesn't match original text");
            }

            // Get letters all at once because getting a COM class for each letter is very slow, resulting in long load times for large text files
            int spatialStringLength = 0;
            LetterStruct[] letters;
            unsafe
            {
                IntPtr ptr = IntPtr.Zero;
                _uss.GetOCRImageLetterArray(ref spatialStringLength, ref ptr);
                LetterStruct* source = (LetterStruct*)ptr;
                letters = new LetterStruct[spatialStringLength];
                for (int i = 0; i < spatialStringLength; i++)
                {
                    letters[i] = source[i];
                }
            }

            // Create mapping of displayed text to raw positions (for RTF files this means which characters in the original code does the display character match)
            // Also map display to spatial string positions to account for things like \r\n being changed to \n in the display string
            _displayedTextToRawPositions = new (int, int)[displayedText.Length];
            _displayedTextToSpatialStringPositions = new (int, int)[displayedText.Length];
            LetterStruct previousLetter = default(LetterStruct);
            for (int i = 0, j = 0; i < spatialStringLength && j < displayedText.Length; i++, j++)
            {
                int numberOfSpatialChars = 1;
                var letter = letters[i];
                var left = (int)letter.Left;

                // '\r' is stripped from the displayed text by the base class
                if (letter.Guess1 == '\r')
                {
                    j--;
                    previousLetter = letter;
                    continue;
                }
                else if (letter.Guess1 == '\n' && previousLetter.Guess1 == '\r')
                {
                    left = (int)previousLetter.Left;
                    numberOfSpatialChars = 2;
                }
                else if (bomCodes.Contains(letter.Guess1))
                {
                    j--;
                    continue;
                }

                var length = (int)letter.Right - left; // Right is exclusive
                _displayedTextToRawPositions[j] = (left, length);
                _displayedTextToSpatialStringPositions[j] = (i, numberOfSpatialChars);

                previousLetter = letter;
            }

            _rawTextToDisplayedTextPositions = new (int, int)[rawText.Length];
            for (int i = 0; i < displayedText.Length; i++)
            {
                var (index, length) = _displayedTextToRawPositions[i];
                for (int j = 0; j < length; j++)
                {
                    _rawTextToDisplayedTextPositions[index + j] = (i, 1);
                }
            }
        }

        private void SelectRedactionOrClue(MouseEventArgs e)
        {
            var charIndex = GetCharIndexFromPosition(e.Location);
            if (charIndex < 0 || charIndex >= _displayedTextToRawPositions.Length)
            {
                return;
            }

            var (index, _) = _displayedTextToRawPositions[charIndex];
            var selected = LayerObjects
                .OfType<Redaction>()
                .Where(redaction =>
                    redaction
                    .GetRasterZones()
                    .Where(zone => index >= zone.StartX && index <= zone.EndX)
                    .Any())
                .FirstOrDefault();

            if (selected != null)
            {
                selected.Selected = true;
            }
        }

        private void SelectRedactionOrClue(Redaction layerObject, bool select)
        {
            Draw(() =>
            {
                var previousSelectionStart = SelectionStart;
                var previousSelectionLength = SelectionLength;
                var backColor = layerObject.Color == Color.Transparent
                    ? CLUE_BACKGROUND_COLOR
                    : layerObject.Color;

                foreach (var zone in layerObject.GetRasterZones())
                {
                    var startRtf = zone.StartX;
                    var endRtf = zone.EndX - 1; // End is exclusive
                    var start = _rawTextToDisplayedTextPositions[(int)startRtf];
                    var end = _rawTextToDisplayedTextPositions[(int)endRtf];
                    var startIdx = start.index;
                    var endIdx = end.index + end.length;
                    var length = endIdx - startIdx;
                    Select(startIdx, length);

                    SelectionBackColor = select ? SELECTED_BACKGROUND_COLOR : backColor;
                    SelectionColor = ITEM_TEXT_COLOR;
                }

                Select(previousSelectionStart, previousSelectionLength);
            });
        }

        // Return raster zones from spatial string
        private IEnumerable<RasterZone> GetRasterZonesFromSelection()
        {
            int start = SelectionStart;
            int end = start + SelectionLength - 1;
            if (end < start)
            {
                return Enumerable.Empty<RasterZone>();
            }

            start = _displayedTextToSpatialStringPositions[start].index;
            var (index, length) = _displayedTextToSpatialStringPositions[end];
            end = index + length - 1;

            var substring = _uss.GetSubString(start, end);

            if (!substring.HasSpatialInfo())
            {
                return Enumerable.Empty<RasterZone>();
            }

            return substring.GetOCRImageRasterZones()
                .ToIEnumerable<UCLID_RASTERANDOCRMGMTLib.RasterZone>()
                .Select(x => new RasterZone(x));
        }

        private void EndTracking(bool cancel)
        {
            if (cancel)
            {
                SelectionLength = 0;
            }
            else
            {
                switch (CursorTool)
                {
                    case CursorTool.AngularRedaction:
                    case CursorTool.RectangularRedaction:
                    case CursorTool.WordRedaction:
                        if (LayerObjects.Selection.Count > 0 && ModifierKeys.HasFlag(Keys.Control))
                        {
                            AppendToRedaction();
                        }
                        else
                        {
                            CreateRedaction();
                        }
                        break;
                }
            }

            IsTracking = false;
        }

        private void Blur()
        {
            BlurMechanism?.Blur();
        }

        #endregion Private Methods

        #region Static Methods

        /// <summary>
        /// Initializes the static <see cref="Dictionary{T,T}"/> of <see cref="ImageViewerCursors"/>
        /// member of the <see cref="ImageViewer"/> class.
        /// </summary>
        /// <returns>A <see cref="Dictionary{T,T}"/> containing the
        /// <see cref="ImageViewerCursors"/> for all of the
        /// <see cref="CursorTool"/> objects</returns>
        static Dictionary<CursorTool, ImageViewerCursors> LoadCursorsForCursorTools()
        {
            try
            {
                // [DotNetRCAndUtils::297] LoadCursorsForCursorTools is called to initialize
                // the static member _cursorsForCursorTools prior to the constructor being called
                // for any particular instance.  Therefore make sure load the license file here
                // if in design mode.
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                Dictionary<CursorTool, ImageViewerCursors> cursorsForCursorTools =
                    new Dictionary<CursorTool, ImageViewerCursors>();

                // Iterate through each of the CursorTools
                foreach (CursorTool value in Enum.GetValues(typeof(CursorTool)))
                {
                    // Create a new ImageViewerCursors object
                    ImageViewerCursors cursors = new ImageViewerCursors();

                    // If the current tool is the None tool, set the tool
                    // cursor to the Cursors.Default and the active cursor to none.
                    if (value == CursorTool.None)
                    {
                        cursors.Tool = null;
                        cursors.Active = null;
                    }
                    else
                    {
                        // Get the name for this enum value
                        string name;
                        switch (value)
                        {
                            case CursorTool.AngularHighlight:
                                name = "Highlight";
                                break;

                            case CursorTool.AngularRedaction:
                                name = "Redaction";
                                break;

                            case CursorTool.SetHighlightHeight:
                                name = "SetHeight";
                                break;

                            case CursorTool.EditHighlightText:
                                name = "EditText";
                                break;

                            case CursorTool.DeleteLayerObjects:
                                name = "Delete";
                                break;

                            default:
                                name = Enum.GetName(typeof(CursorTool), value);
                                break;
                        }

                        // Load the normal cursor first.
                        cursors.Tool = ExtractCursors.GetCursor(
                            "Resources." + name + ".cur");

                        // Load the active cursor.
                        cursors.Active = ExtractCursors.GetCursor(
                            "Resources.Active" + name + ".cur");

                        // Load the shift state cursor.
                        cursors.ShiftState = ExtractCursors.GetCursor(
                            "Resources.Shift" + name + ".cur");

                        // Load the ctrl-shift state cursor.
                        cursors.CtrlShiftState = ExtractCursors.GetCursor(
                            "Resources.CtrlShift" + name + ".cur");
                    }

                    // Add the cursors for the current cursor tool to the collection
                    cursorsForCursorTools.Add(value, cursors);
                }

                return cursorsForCursorTools;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI48326", "Unable to initialize cursor objects.", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="ImageViewerCursors"/> for the specified <see cref="CursorTool"/>
        /// </summary>
        /// <param name="tool">The <see cref="CursorTool"/> to retrieve the cursors for.</param>
        /// <returns>The cursors for the specified cursor tool.</returns>
        static ImageViewerCursors GetCursorsForTool(CursorTool tool)
        {
            // Ensure the cursors have been loaded.
            if (_cursorsForCursorTools == null)
            {
                _cursorsForCursorTools = LoadCursorsForCursorTools();
            }

            ImageViewerCursors cursor = null;
            if (!_cursorsForCursorTools.TryGetValue(tool, out cursor))
            {
                ExtractException ee = new ExtractException("ELI48327",
                    "Unable to retrieve cursor for specified tool.");
                ee.AddDebugData("CursorTool", tool, false);
                throw ee;
            }

            return cursor;
        }

        #endregion Static Methods

        #region Helper Structs

        // Use same layout as CppLetter so they can be cast to each other
        [StructLayout(LayoutKind.Sequential)]
        struct LetterStruct
        {
            public ushort Guess1;
            public ushort Guess2;
            public ushort Guess3;
            
            // The spatialBoundaries of the letter
            public uint Top;
            public uint Left;
            public uint Right;
            public uint Bottom;
            
            // max number of pages per document is limited to 65535
            // The page on which this character lies
            public ushort PageNumber;

            // true if this is the last character in a paragraph
            public bool EndOfParagraph;

            // true if this is the last character in a zone
            public bool EndOfZone;

            // True if this character has spatial information
            // i.e. is a "Spatial Letter"
            public bool Spatial;

            // This is the font size (in points) of the letter 
            public byte FontSize;

            // This is the font size (in points) of the letter 
            public byte CharConfidence;

            public byte Font;
        }

        #endregion Helper Structs
    }
}
