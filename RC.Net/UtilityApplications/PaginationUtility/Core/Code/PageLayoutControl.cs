using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Leadtools;
using Leadtools.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Displays and manages <see cref="PaginationControl"/>s that allow a user to manipulate image
    /// pages into output documents.
    /// </summary>
    internal partial class PageLayoutControl : UserControl
    {
        /// <summary>
        /// The number of pages that can be safely loaded at once without exceeding the limit of
        /// Window's user objects.
        /// </summary>
        internal static readonly int _MAX_LOADED_PAGES = 1000;

        /// <summary>
        /// Displays a wait cursor and suspends layout and painting of the _flowLayoutPanel during
        /// an operation which is re-organizing controls in the panel.
        /// </summary>
        class PageLayoutControlUpdateLock : IDisposable
        {
            #region Fields

            /// <summary>
            /// The <see cref="PageLayoutControl"/> for which the update is taking place.
            /// </summary>
            PageLayoutControl _pageLayoutControl;

            /// <summary>
            /// The wait cursor that is displayed during the operation.
            /// </summary>
            TemporaryWaitCursor _waitCursor;

            /// <summary>
            /// Prevents painting of the control during the operation.
            /// </summary>
            LockControlUpdates _controlUpdateLock;

            /// <summary>
            /// Indicates whether a full layout should be performed when this instance is disposed.
            /// </summary>
            bool _forceFullLayout;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="PageLayoutControlUpdateLock"/> class.
            /// </summary>
            /// <param name="pageLayoutControl">The <see cref="PageLayoutControl"/> for which the
            /// update is taking place.</param>
            /// <param name="forceFullRefresh"><c>true</c> to force a full layout when layout is
            /// resumed.</param>
            public PageLayoutControlUpdateLock(PageLayoutControl pageLayoutControl, bool forceFullLayout = false)
            {
                if (!pageLayoutControl._inUpdateOperation)
                {
                    _pageLayoutControl = pageLayoutControl;
                    _forceFullLayout = forceFullLayout;
                    pageLayoutControl._inUpdateOperation = true;

                    _waitCursor = new TemporaryWaitCursor();

                    _pageLayoutControl.SuspendLayout();
                    _pageLayoutControl._flowLayoutPanel.SuspendLayout();

                    _controlUpdateLock =
                        new LockControlUpdates(_pageLayoutControl._flowLayoutPanel, true, true);

                    
                }
            }

            #endregion Constructors

            #region IDisposable Members

            /// <summary>
            /// Releases all resources used by the <see cref="PageLayoutControlUpdateLock"/>.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <overloads>Releases resources used by the <see cref="PageLayoutControlUpdateLock"/>.
            /// </overloads>
            /// <summary>
            /// Releases all unmanaged resources used by the <see cref="PageLayoutControlUpdateLock"/>.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
            /// resources; <see langword="false"/> to release only unmanaged resources.</param>
            // These fields are disposed of, just not directly.
            [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_waitCursor")]
            [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_controlUpdateLock")]
            void Dispose(bool disposing)
            {
                if (disposing)
                {
                    try
                    {
                        // Dispose of managed resources
                        if (_pageLayoutControl != null && _waitCursor != null &&
                            _controlUpdateLock != null)
                        {
                            var pageLayoutControl = _pageLayoutControl;
                            var waitCursor = _waitCursor;
                            var controlUpdateLock = _controlUpdateLock;

                            // Invoke the dispose of waitCursor and controlUpdateLock so that they
                            // are in effect until all events associated with the operation have
                            // been processed.
                            _pageLayoutControl.SafeBeginInvoke("ELI35613", () =>
                            {
                                waitCursor.Dispose();
                                controlUpdateLock.Dispose();
                                // Ensure the page layout control has keyboard focus after each
                                //event.
                                pageLayoutControl.Focus();
                            });

                            _controlUpdateLock = null;
                            _waitCursor = null;
                        }

                        if (_pageLayoutControl != null)
                        {
                            _pageLayoutControl._inUpdateOperation = false;
                            if (_forceFullLayout)
                            {
                                ((PaginationLayoutEngine)_pageLayoutControl._flowLayoutPanel.LayoutEngine).ForceNextLayout = true;
                            }
                            _pageLayoutControl._flowLayoutPanel.ResumeLayout(true);
                            _pageLayoutControl.ResumeLayout(true);
                            _pageLayoutControl.UpdateCommandStates();
                            _pageLayoutControl = null;
                        }
                    }
                    catch { }
                }
                // Dispose of unmanaged resources
            }

            #endregion IDisposable Members
        }

        #region Constants

        /// <summary>
        /// The name of this object.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(PageLayoutControl).ToString();

        /// <summary>
        /// The name of the data format for drag and drop operations.
        /// </summary>
        static readonly string _DRAG_DROP_DATA_FORMAT = "ExtractPaginationDragDropDataFormat";

        /// <summary>
        /// The name of the custom clipboard data format to use.
        /// </summary>
        static readonly string _CLIPBOARD_DATA_FORMAT = "ExtractPaginationClipboardDataFormat";

        /// <summary>
        /// The number of times clipboard copy operations should be attempted before giving up.
        /// </summary>
        internal static readonly int _CLIPBOARD_RETRY_COUNT = 10;

        /// <summary>
        /// The number of pixels from the top or bottom of the control where scrolling will be
        /// triggered during a drag/drop operation.
        /// </summary>
        static readonly int _DRAG_DROP_SCROLL_AREA = 50;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="IPaginationUtility"/> to which this instance belongs.
        /// </summary>
        IPaginationUtility _paginationUtility;

        /// <summary>
        /// Indicates where <see cref="PaginationControl"/>s will be dropped during a
        /// drag-and-drop operation.
        /// </summary>
        DropLocationIndicator _dropLocationIndicator = new DropLocationIndicator();

        /// <summary>
        /// Button that appears as the last <see cref="PaginationControl"/> and that causes the next
        /// document to be loaded when pressed.
        /// </summary>
        LoadNextDocumentButtonControl _loadNextDocumentButtonControl =
            new LoadNextDocumentButtonControl();

        /// <summary>
        /// Indicates the control index at which controls should be dropped during a
        /// drag-and-drop operation.
        /// </summary>
        int _dropLocationIndex = -1;

        /// <summary>
        /// The <see cref="PaginationControl"/> that is currently the primarily selected
        /// control. If a <see cref="PageThumbnailControl"/>, the image page will be displayed in
        /// the <see cref="ImageViewer"/>.
        /// </summary>
        PaginationControl _primarySelection;

        /// <summary>
        /// The last selected <see cref="PaginationControl"/>.
        /// </summary>
        PaginationControl _lastSelectedControl;

        /// <summary>
        /// The <see cref="PaginationControl"/> that should be the target of any shortcut key or
        /// context menu commands.
        /// </summary>
        PaginationControl _commandTargetControl;

        /// <summary>
        /// The <see cref="PageThumbnailControl"/> whose page is current displayed in the
        /// <see cref="ImageViewer"/>.
        /// </summary>
        PageThumbnailControl _displayedPage;

        /// <summary>
        /// Indicates whether an operation is in progress that might otherwise cause a document to
        /// be closed and re-opened. A value of <see langword="true"/> will prevent the otherwise
        /// unnecessary close from occurring.
        /// </summary>
        bool _preventTransientDocumentClose;

        /// <summary>
        /// Indicates whether the <see cref="PrimarySelection"/> should update the displayed page in
        /// the <see cref="ImageViewer"/>.
        /// </summary>
        bool _enablePageDisplay = true;

        /// <summary>
        /// Context menu option that allows the selected PaginationControls to be cut.
        /// </summary>
        readonly ToolStripMenuItem _cutMenuItem = new ToolStripMenuItem("Cut");

        /// <summary>
        /// The <see cref="ApplicationCommand"/> that controls the availability of the cut operation.
        /// </summary>
        ApplicationCommand _cutCommand;

        /// <summary>
        /// Context menu option that allows the PaginationControls to be copied.
        /// </summary>
        readonly ToolStripMenuItem _copyMenuItem = new ToolStripMenuItem("Copy");

        /// <summary>
        /// The <see cref="ApplicationCommand"/> that controls the availability of the copy operation.
        /// </summary>
        ApplicationCommand _copyCommand;

        /// <summary>
        /// Context menu option that allows the PaginationControls to be deleted.
        /// </summary>
        readonly ToolStripMenuItem _deleteMenuItem = new ToolStripMenuItem("Delete");

        /// <summary>
        /// The <see cref="ApplicationCommand"/> that controls the availability of the delete operation.
        /// </summary>
        ApplicationCommand _deleteCommand;

        /// <summary>
        /// Context menu option that allows the PaginationControls to be un-deleted.
        /// </summary>
        readonly ToolStripMenuItem _unDeleteMenuItem = new ToolStripMenuItem("Un-delete");

        /// <summary>
        /// The <see cref="ApplicationCommand"/> that controls the availability of the un-delete
        /// operation.
        /// </summary>
        ApplicationCommand _unDeleteCommand;

        /// <summary>
        /// Context menu option that allows the PaginationControls to be printed.
        /// </summary>
        readonly ToolStripMenuItem _printMenuItem = new ToolStripMenuItem("Print selected page(s)...");

        /// <summary>
        /// The <see cref="ApplicationCommand"/> that controls the availability of the print operation.
        /// </summary>
        ApplicationCommand _printCommand;

        /// <summary>
        /// Context menu option that allows the copied PaginationControls to be inserted.
        /// </summary>
        readonly ToolStripMenuItem _pasteMenuItem = new ToolStripMenuItem("Paste");

        /// <summary>
        /// The <see cref="ApplicationCommand"/> that controls the availability of the paste
        /// operation.
        /// </summary>
        ApplicationCommand _pasteCommand;

        /// <summary>
        /// Context menu option that allows the a document separator prior to the selected page to
        /// be toggled on or off.
        /// </summary>
        readonly ToolStripMenuItem _toggleDocumentSeparatorMenuItem =
            new ToolStripMenuItem("Toggle document separator");

        /// <summary>
        /// The <see cref="ApplicationCommand"/> that controls the availability of the insert
        /// pagination separator operation.
        /// </summary>
        ApplicationCommand _toggleDocumentSeparatorCommand;

        /// <summary>
        /// The <see cref="ApplicationCommand"/> that controls the availability of the output operation.
        /// </summary>
        ApplicationCommand _outputDocumentCommand;

        /// <summary>
        /// A <see cref="Timer"/> which fires to trigger scrolling during drag/drop operation while
        /// the cursor is close to either the top or bottom of the control.
        /// </summary>
        Timer _dragDropScrollTimer;

        /// <summary>
        /// The number of pixels per <see cref="_dragDropScrollTimer"/> fire the control should
        /// scroll while drag/drop scrolling is active.
        /// </summary>
        int _scrollSpeed;

        /// <summary>
        /// The last scroll position that was set programmatically during drag/drop scrolling. There
        /// are situations outside of the code here that cause the scroll position to be adjusted
        /// after we have set it. By keeping track of what we last wanted it to be we can prevent
        /// the scroll position from jumping around in an unexpected fashion.
        /// </summary>
        int _dragDropScrollPos;

        /// <summary>
        /// Indicates whether an operation that re-organizes controls in the panel is taking place.
        /// </summary>
        bool _inUpdateOperation;

        /// <summary>
        /// Indicates whether that the _flowLayoutPanel is resizing.
        /// </summary>
        bool _flowLayoutPanelResizing;

        /// <summary>
        /// The <see cref="ToolTip"/> used to display the full source document path for
        /// <see cref="PageThumbnailControl"/>s.
        /// </summary>
        ToolTip _toolTip = new ToolTip();

        /// <summary>
        /// The <see cref="PageThumbnailControl"/> with which the <see cref="_toolTip"/> is
        /// currently associated.
        /// </summary>
        PageThumbnailControl _toolTipControl;

        /// <summary>
        /// Used for printing. This is a member because there is an event handler attached to it, so it 
        /// would leak memory if not explicitly cleaned up after each use. 
        /// </summary>
        PrintDocument _printDocument = null;

        /// <summary>
        /// Used for printing, to maintain the pages to print.
        /// </summary>
        List<RasterImage> _rastersForPrinting = null;

        /// <summary>
        /// Used for printing, to maintain the index of the page being printed.
        /// </summary>
        int _printPageCounter;

        /// <summary>
        /// A copy of the last data put on the clipboard.
        /// </summary>
        ClipboardData _currentClipboardData = null;

        /// <summary>
        /// Manages the keyboard shortcuts for this instance.
        /// </summary>
        ShortcutsManager _shortcuts;

        /// <summary>
        /// Prevents UI updates of this control.
        /// </summary>
        PageLayoutControlUpdateLock _updateLock = null;

        /// <summary>
        /// Indicates whether <see cref="UpdateCommandStates"/> has been invoked to execute on a
        /// following windows message.
        /// </summary>
        bool _updateCommandStatesInvoked;

        /// <summary>
        /// Indicates whether this panel should appear as having input focus.
        /// </summary>
        bool _indicateFocus = true;

        /// <summary>
        /// Indicates when a DEP is scheduled to be "snapped" to the top of the layout control.
        /// </summary>
        bool _pendingSnapDataPanelToTop;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PageLayoutControl"/> class.
        /// </summary>
        /// <param name="paginationUtility">The <see cref="IPaginationUtility"/> to which this
        /// instance belongs.</param>
        // We do not need to worry about preventing sleep mode with the _dragDropScrollTimer as it
        // will be active only during a drag/drop operation.
        [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        public PageLayoutControl(IPaginationUtility paginationUtility)
            : base()
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.PaginationUIObject,
                    "ELI35428", _OBJECT_NAME);

                InitializeComponent();

                this.SetStyle(ControlStyles.Selectable, true);

                // When dragging files in from the Windows shell, _dropLocationIndicator receives
                // drag/drop events if the mouse is over the indicator.
                _dropLocationIndicator.DragDrop += Handle_DragDrop;
                _dropLocationIndicator.DragEnter += Handle_DragEnter;
                _dropLocationIndicator.DragOver += Handle_DragOver;
                _dropLocationIndicator.DragLeave += Handle_DragLeave;

                // Set scrolling during a drag/drop scroll event to occur 20 times / sec.
                _dragDropScrollTimer = new Timer();
                _dragDropScrollTimer.Interval = 50;
                _dragDropScrollTimer.Tick += HandleDragDropScrollTimer_Tick;

                _toolTip.AutoPopDelay = 0;
                _toolTip.InitialDelay = 500;
                _toolTip.ReshowDelay = 500;

                _paginationUtility = paginationUtility;
                _flowLayoutPanel.Click += HandleFlowLayoutPanel_Click;
                _flowLayoutPanel.ClientSizeChanged += HandleFlowLayoutPanel_ClientSizeChanged;
                ((PaginationLayoutEngine)_flowLayoutPanel.LayoutEngine).LayoutCompleted +=
                    PaginationLayoutEngine_LayoutCompleted;

                _printPageCounter = 0;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35429");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised whenever a <see cref="PageThumbnailControl"/> is removed deleted or cut from
        /// this instance.
        /// </summary>
        public event EventHandler<PageDeletedEventArgs> PageDeleted;

        /// <summary>
        /// Raised when references have been removed from pages and, therefore, some files may no
        /// longer be referenced.
        /// </summary>
        public event EventHandler<PagesDereferencedEventArgs> PagesDereferenced;

        /// <summary>
        /// Raised when the state of the <see cref="PaginationControl"/>s has changed.
        /// </summary>
        public event EventHandler<EventArgs> StateChanged;

        /// <summary>
        /// Raised when the selection in the <see cref="PaginationControl"/>s has changed.
        /// </summary>
        public event EventHandler<EventArgs> SelectionChanged;

        /// <summary>
        /// Raised when the next input document should be loaded per explicit request from user.
        /// </summary>
        public event EventHandler<EventArgs> LoadNextDocumentRequest;

        /// <summary>
        /// Raised when a new <see cref="IPaginationDocumentDataPanel"/> instance is needed to allow
        /// editing of the data associated with an <see cref="OutputDocument"/>.
        /// </summary>
        public event EventHandler<DocumentDataPanelRequestEventArgs> DocumentDataPanelRequest;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets whether this panel should appear as having input focus.
        /// </summary>
        /// <value> <c>true</c> if the panel should appear to have input focus;
        /// otherwise, <c>false</c>.
        /// </value>
        public bool IndicateFocus
        {
            get
            {
                return _indicateFocus;
            }

            set
            {
                try
                {
                    if (value != _indicateFocus)
                    {
                        _indicateFocus = value;

                        if (value)
                        {
                            PrimarySelection?.Invalidate();
                        }
                        else
                        {
                            // If losing focus, reset selection (which would no longer be indicated).
                            // This prevents any possible surprises as focus in returned to the panel.
                            ProcessControlSelection(
                                activeControl: PrimarySelection,
                                additionalControls: null,
                                select: true,
                                modifierKeys: Keys.None);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI44705");
                }
            }
        } 

        /// <summary>
        /// Gets or sets the <see cref="ImageViewer"/> that is to display the images from selected
        /// <see cref="PageThumbnailControl"/>s.
        /// </summary>
        /// <value>
        /// The <see cref="ImageViewer"/> that is to display the images from selected
        /// <see cref="PageThumbnailControl"/>s.
        /// </value>
        public IDocumentViewer ImageViewer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the current number of pages contained in this instance (across all documents).
        /// </summary>
        public int PageCount
        {
            get
            {
                return _flowLayoutPanel.Controls.OfType<PageThumbnailControl>().Count();
            }
        }

        /// <summary>
        /// Gets all potential output documents currently represented in this instance.
        /// </summary>
        public IEnumerable<OutputDocument> Documents
        {
            get
            {
                try
                {
                    var documents = _flowLayoutPanel.Controls
                        .OfType<PageThumbnailControl>()
                        .Where(pageControl => pageControl.Document != null)
                        .Select(pageControl => pageControl.Document)
                        .Distinct();

                    return documents;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35430");
                }
            }
        }

        /// <summary>
        /// Gets all documents for which there is at least one page currently selected.
        /// </summary>
        public IEnumerable<OutputDocument> PartiallySelectedDocuments
        {
            get
            {
                try
                {
                    var selectedDocuments = Documents.Where(document =>
                        document.PageControls.Count > 0 &&
                        document.PageControls.Intersect(
                            SelectedControls.OfType<PageThumbnailControl>())
                            .Any());

                    return selectedDocuments;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35431");
                }
            }
        }

        /// <summary>
        /// Gets all documents for which all pages are currently selected and for which a document
        /// separator follows the last page.
        /// </summary>
        public IEnumerable<OutputDocument> FullySelectedDocuments
        {
            get
            {
                try
                {
                    var selectedDocuments = Documents.Where(document =>
                        document.PageControls.Count > 0 &&
                        document.PageControls.Intersect(
                            SelectedControls.OfType<PageThumbnailControl>())
                                .Count() == document.PageControls.Count());

                    return selectedDocuments.Where(document =>
                        document.PageControls.Last().NextControl is PaginationSeparator);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35432");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the load next document button should be available.
        /// </summary>
        public bool LoadNextDocumentVisible
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether changes should be committed only for the selected documents.
        /// </summary>
        /// <value><see langword="true"/> if changes should be committed only for the selected
        /// document(s) or <see langword="false"/> if changes should be committed for all documents
        /// loaded into this instance.</value>
        public bool CommitOnlySelection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="LoadNextDocumentButtonControl"/>
        /// is enabled.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="LoadNextDocumentButtonControl"/> is
        /// enabled; otherwise, <see langword="false"/>.
        /// </value>
        public bool EnableLoadNextDocument
        {
            get
            {
                try
                {
                    return _flowLayoutPanel.Controls.Contains(_loadNextDocumentButtonControl) &&
                        _loadNextDocumentButtonControl.ButtonEnabled;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35675");
                }
            }

            set
            {
                try
                {
                    _loadNextDocumentButtonControl.ButtonEnabled = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35676");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether pages should automatically be oriented to match
        /// the orientation of the text (per OCR).
        /// </summary>
        /// <value>
        ///   <c>true</c> if pages should automatically be oriented to match the orientation of the
        ///   text; otherwise, <c>false</c>.
        /// </value>
        public bool AutoRotateImages
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the primarily selected <see cref="PageThumbnailControl"/> should
        /// update the displayed page in the <see cref="ImageViewer"/>.
        /// </summary>
        /// <value><see langword="true"/> if the primarily selected page should update the displayed
        /// page in the <see cref="ImageViewer"/>; otherwise, <see langword="false"/>.</value>
        public bool EnablePageDisplay
        {
            get
            {
                return _enablePageDisplay;
            }

            set
            {
                try
                {
                    if (value != _enablePageDisplay)
                    {
                        // If page display is not enabled, the ImageViewer needs to be deactivated
                        // for all pages so that when selection changes the pages don't try to close
                        // the currently displayed document.
                        if (!value)
                        {
                            foreach (var pageControl in
                                _flowLayoutPanel.Controls.OfType<PageThumbnailControl>())
                            {
                                pageControl.DeactivateImageViewer();
                            }
                        }

                        _enablePageDisplay = value;

                        if (value)
                        {
                            var primaryPage = PrimarySelection as PageThumbnailControl;
                            if (primaryPage != null)
                            {
                                DisplayPage(primaryPage, true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39663");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether document output should only be able to be
        /// initiated by the <see cref="IPaginationUtility"/>.
        /// </summary>
        /// <value><see langword="true"/> if document output should only be able to be initiated by
        /// the <see cref="IPaginationUtility"/>; <see langword="false"/> if document output should
        /// be able to be initiated by this control.
        /// </value>
        public bool ExternalOutputOnly
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether document pages for newly loaded documents should
        /// be collapsed by default.
        /// </summary>
        public bool DefaultToCollapsed
        {
            get;
            set;
        }

        /// <summary>
        /// The <see cref="ShortcutsManager"/> managing all keyboard shortcuts for this instance.
        /// </summary>
        public ShortcutsManager Shortcuts
        {
            get
            {
                return _shortcuts ?? ImageViewer.Shortcuts;
            }

            set
            {
                _shortcuts = value;
            }
        }

        /// <summary>
        /// Gets or sets a value whether UI updates are currently suspended.
        /// </summary>
        /// <value><see langword="true"/> if UI updates suspended; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool UIUpdatesSuspended
        {
            get
            {
                return _updateLock != null;
            }

            set
            {
                try
                {
                    if (value && _updateLock == null)
                    {
                        _updateLock = new PageLayoutControlUpdateLock(this);
                        if (LoadNextDocumentVisible)
                        {
                            RemovePaginationControl(_loadNextDocumentButtonControl, false);
                        }
                    }
                    else if (!value && _updateLock != null)
                    {
                        if (LoadNextDocumentVisible)
                        {
                            AddPaginationControl(_loadNextDocumentButtonControl);
                        }
                        _updateLock.Dispose();
                        _updateLock = null;
                        ((PaginationLayoutEngine)_flowLayoutPanel.LayoutEngine).ForceNextLayout = true;
                        UpdateCommandStates();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40220");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the current keystroke should be ignored as a shortcut key. Used in
        /// the case that data is being edited.
        /// </summary>
        public bool IgnoreShortcutKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="PaginationControl"/> that should be considered the primary selection
        /// and the basis for all keyboard navigation. If a <see cref="PageThumbnailControl"/> the
        /// corresponding image page will be displayed in the <see cref="ImageViewer"/> as well.
        /// </summary>
        /// <value>
        /// The <see cref="PaginationControl"/> that should be considered the primary
        /// selection.
        /// </value>
        public PaginationControl PrimarySelection
        {
            get
            {
                return _primarySelection;
            }

            private set
            {
                if (value != _primarySelection)
                {
                    if (_primarySelection != null)
                    {
                        SetHighlightedAndDisplayed(_primarySelection, false);
                    }

                    _primarySelection = value;

                    if (_primarySelection != null)
                    {
                        if (DocumentDataPanel != null)
                        {
                            // Let the data entry panel know if the current page corresponds to the document
                            // it has loaded.
                            DocumentDataPanel.PrimaryPageIsForActiveDocument =
                                (DocumentInDataEdit != null) &&
                                DocumentInDataEdit == (_primarySelection as PageThumbnailControl)?.Document;
                        }

                        SetHighlightedAndDisplayed(_primarySelection, true);

                        // _commandTargetControl is used for shortcut keys as well as for the
                        // context menu. Set _commandTargetControl whenever the primary control is
                        // being set to allow shortcuts keys to work even if the control hasn't been
                        // clicked.
                        _commandTargetControl = _primarySelection;
                    }
                    else if (!_preventTransientDocumentClose && ImageViewer.IsImageAvailable)
                    {
                        ImageViewer.CloseImage();
                    }
                }
            }
        }

        /// <summary>
        /// The <see cref="IPaginationDocumentDataPanel"/> that is currently open for editing
        /// or <see langword="null"/> if there is no data panel currently open for editing.
        /// </summary>
        public IPaginationDocumentDataPanel DocumentDataPanel
        {
            get;
            private set;
        }

        /// <summary>
        /// The <see cref="OutputDocument"/> for which <see cref="DocumentDataPanel"/> is currently
        /// displayed.
        /// </summary>
        public OutputDocument DocumentInDataEdit
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a new <see cref="OutputDocument"/> based on the specified
        /// <paramref name="sourceDocument"/>, and adds <see cref="PageThumbnailControl"/>s for its
        /// pages.
        /// </summary>
        /// <param name="sourceDocument">The <see cref="SourceDocument"/> to be loaded as an
        /// <see cref="OutputDocument"/>.</param>
        /// <param name="pages">The page numbers from <see paramref="sourceDocument"/> that should
        /// be used or <see landword="null"/> to us all pages.</param>
        /// <param name="deletedPages">The page numbers from <see paramref="fileName"/> to be
        /// loaded but shown as deleted.</param>
        /// <param name="viewedPages">The page numbers from <see paramref="fileName"/> to be
        /// loaded but shown as viewed.</param>
        /// <param name="position">The position at which a document should be loaded. 0 = Load at
        /// the front (top), -1 = load at the end (bottom). Any other value should be a value
        /// passed via <see cref="CreatingOutputDocumentEventArgs"/> and not a value the caller
        /// should expect to be able to calculate.
        /// NOTE: If position != -1, all pages will be loaded even it if results in more than
        /// _MAX_LOADED_PAGES.
        /// </param>
        /// <param name="insertSeparator"><see langword="true"/> if a separator should be inserted
        /// before creating the new; otherwise, <see langword="false"/>.</param>
        /// <returns>The <see cref="OutputDocument"/> that was created.</returns>
        public OutputDocument CreateOutputDocument(SourceDocument sourceDocument,
            IEnumerable<int> pages, IEnumerable<int> deletedPages, IEnumerable<int> viewedPages,
             int position, bool insertSeparator)
        {
            var pagesList = (pages == null && deletedPages == null)
                    ? null
                    : new List<int>((pages ?? new int[0]).Union(deletedPages ?? new int[0]));
            var pagesToLoad = (pagesList == null)
                ? sourceDocument.Pages.ToArray()
                : sourceDocument.Pages
                    .Where(page => pagesList
                        .Contains(page.OriginalPageNumber))
                    .OrderBy(page => pagesList.IndexOf(page.OriginalPageNumber))
                    .ToArray();

            return CreateOutputDocument(sourceDocument, pagesToLoad, position, insertSeparator, deletedPages, viewedPages);
        }

        /// <summary>
        /// Creates a new <see cref="OutputDocument"/> based on the specified
        /// <paramref name="sourceDocument"/>, and adds <see cref="PageThumbnailControl"/>s for its
        /// pages.
        /// </summary>
        /// <param name="sourceDocument">The <see cref="SourceDocument"/> to be loaded as an
        /// <see cref="OutputDocument"/>.</param>
        /// <param name="paginationRequest">The <see cref="PaginationRequest"/> to define the pages
        /// in the document.</param>
        /// <param name="position">The position at which a document should be loaded. 0 = Load at
        /// the front (top), -1 = load at the end (bottom). Any other value should be a value
        /// passed via <see cref="CreatingOutputDocumentEventArgs"/> and not a value the caller
        /// should expect to be able to calculate.
        /// NOTE: If position != -1, all pages will be loaded even it if results in more than
        /// _MAX_LOADED_PAGES.
        /// </param>
        /// <param name="insertSeparator"><see langword="true"/> if a separator should be inserted
        /// before creating the new; otherwise, <see langword="false"/>.</param>
        /// <returns>The <see cref="OutputDocument"/> that was created.</returns>
        public OutputDocument CreateOutputDocument(SourceDocument sourceDocument,
            PaginationRequest paginationRequest, int position, bool insertSeparator)
        {
            var pagesToLoad = paginationRequest.ImagePages
                .Select(imagePage => imagePage.DocumentName == sourceDocument.FileName
                    ? sourceDocument.Pages.Single(sourcePage => sourcePage.OriginalPageNumber == imagePage.PageNumber)
                    : new Page(null, imagePage.PageNumber))
                .ToArray();

            var deletedPages = paginationRequest.ImagePages
                .Where(imagePage => imagePage.Deleted && imagePage.DocumentName == sourceDocument.FileName)
                .Select(imagePage => imagePage.PageNumber);

            return CreateOutputDocument(sourceDocument, pagesToLoad, position, insertSeparator, deletedPages);
        }

        /// <summary>
        /// Creates a new <see cref="OutputDocument"/> based on the specified
        /// <paramref name="sourceDocument"/>, and adds <see cref="PageThumbnailControl"/>s for its
        /// pages.
        /// </summary>
        /// <param name="sourceDocument">The <see cref="SourceDocument"/> to be loaded as an
        /// <see cref="OutputDocument"/>.</param>
        /// <param name="pages">The array of <see cref="Page"/>s to be included in the document.</param>
        /// <param name="deletedPages">The page numbers from <see paramref="fileName"/> to be
        /// loaded but shown as deleted.</param>
        /// <param name="viewedPages">The page numbers fro <see paramref="fileName"/> to be
        /// loaded but shown as viewed.</param>
        /// <param name="position">The position at which a document should be loaded. 0 = Load at
        /// the front (top), -1 = load at the end (bottom). Any other value should be a value
        /// passed via <see cref="CreatingOutputDocumentEventArgs"/> and not a value the caller
        /// should expect to be able to calculate.
        /// NOTE: If position != -1, all pages will be loaded even it if results in more than
        /// _MAX_LOADED_PAGES.
        /// </param>
        /// <param name="insertSeparator"><see langword="true"/> if a separator should be inserted
        /// before creating the new; otherwise, <see langword="false"/>.</param>
        /// <returns>The <see cref="OutputDocument"/> that was created.</returns>
        public OutputDocument CreateOutputDocument(SourceDocument sourceDocument, 
            Page[] pages, int position, bool insertSeparator,
            IEnumerable<int> deletedPages = null, IEnumerable<int> viewedPages = null)
        {
            bool removedLoadNextDocumentButton = false;

            try
            {
                if (position == -1)
                {
                    // While new pages are being added, remove the load next document control.
                    if (LoadNextDocumentVisible &&
                        _flowLayoutPanel.Controls.Contains(_loadNextDocumentButtonControl))
                    {
                        RemovePaginationControl(_loadNextDocumentButtonControl, false);
                        removedLoadNextDocumentButton = true;
                    }
                }

                if (sourceDocument == null)
                {
                    return null;
                }

                // Find the page control immediately before the current location. Will be null
                // if position == 0 or there are no documents currently loaded.
                var lastPageControl = _flowLayoutPanel.Controls.OfType<PageThumbnailControl>()
                    .Where(c => position == -1 ||
                        _flowLayoutPanel.Controls.GetChildIndex(c) < position)
                    .OrderBy(c => _flowLayoutPanel.Controls.GetChildIndex(c))
                    .LastOrDefault();

                OutputDocument outputDocument = null;
                bool usingExistingDocument = false;
                int pageIndex = position;

                // If insertSeparator == true and the last control is currently a page control,
                // we need to add a separator.
                if (insertSeparator)
                {
                    // lastPageControl means the document is being inserted at the front; place
                    // the separator ahead of any existing documents
                    if (lastPageControl == null)
                    {
                        InsertPaginationControl(
                            new PaginationSeparator(CommitOnlySelection), index: 0);
                        pageIndex = 1;
                    }
                    // Otherwise, place the separator immediately after the previous document.
                    else
                    {
                        pageIndex = _flowLayoutPanel.Controls.GetChildIndex(lastPageControl) + 1;
                        InsertPaginationControl(
                            new PaginationSeparator(CommitOnlySelection), index: pageIndex);
                        pageIndex++;
                    }
                }
                else if (lastPageControl != null)
                {
                    // [DotNetRCAndUtils:1049]
                    // If separators are not being inserted, but the last control is a document
                    // page, append to that page's document rather than create a new
                    // OutputDocument.
                    outputDocument = lastPageControl.Document;
                    usingExistingDocument = true;
                }

                outputDocument = outputDocument ??
                    GetOutputDocumentFromUtility(sourceDocument.FileName);

                if (position == -1)
                {
                    // Handle case that loading pagesToLoad would exceed _MAX_LOADED_PAGES.
                    AssertAllPagesBeLoaded(pages.Length);
                }

                // Retrieve spatialPageInfos, which will trigger auto-page rotation, only if
                // AutoRotateImages is true.
                var spatialPageInfos = AutoRotateImages
                    ? ImageMethods.GetSpatialPageInfos(sourceDocument.FileName)
                    : null;

                // Create a page control for every page in sourceDocument.
                foreach (Page page in pages)
                {
                    var orientation = ImageMethods.GetPageRotation(spatialPageInfos, page.OriginalPageNumber);
                    if (orientation != null)
                    {
                        page.ProposedOrientation = orientation.Value;
                        page.ImageOrientation = orientation.Value;
                    }

                    // For output documents that were the result of two or more source documents
                    // merged, we may not have the source document available. Display a blank page
                    // in this case.
                    if (page.SourceDocument == null)
                    {
                        using (var blankImage = new Bitmap(1, 1))
                        {
                            blankImage.SetPixel(0, 0, Color.White);
                            page.ThumbnailImage = RasterImageConverter.ConvertFromImage(blankImage, ConvertFromImageOptions.None);
                        }
                    }

                    var pageControl = new PageThumbnailControl(outputDocument, page);

                    if (deletedPages != null && deletedPages.Contains(page.OriginalPageNumber))
                    {
                        pageControl.Deleted = true;
                    }

                    if (viewedPages != null && viewedPages.Contains(page.OriginalPageNumber))
                    {
                        pageControl.Viewed = true;
                    }

                    InsertPaginationControl(pageControl, pageIndex);

                    if (pageIndex != -1)
                    {
                        pageIndex++;
                    }
                }

                if (DefaultToCollapsed || outputDocument.OutputProcessed)
                {
                    outputDocument.Collapsed = true;
                }

                // As long as we haven't appended to the end of an existing document, indicate that
                // if the document is output in its present form, it can simply be copied to the
                // output path rather than require it to be re-assembled.
                if (!usingExistingDocument)
                {
                    outputDocument.SetOriginalForm();
                }

                this.SafeBeginInvoke("ELI35612", () =>
                {
                    // Ensure this control has keyboard focus after loading a document.
                    Focus();
                });

                return outputDocument;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35433");
            }
            finally
            {
                try
                {
                    if (LoadNextDocumentVisible && removedLoadNextDocumentButton)
                    {
                        AddPaginationControl(_loadNextDocumentButtonControl);
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI35624");
                }
            }
        }

        /// <summary>
        /// Loads the specified <see paramref="outputDocument"/> into the panel using the specified
        /// <see paramref="pages"/>.
        /// </summary>
        /// <param name="outputDocument">The <see cref="OutputDocument"/> to load.</param>
        /// <param name="pages">The <see cref="Page"/> instances to load into the document.</param>
        /// <param name="deletedPages">The <see cref="Page"/> instances in <see paramref="pages"/>
        /// that should be loaded in a deleted state.</param>
        /// <param name="autoRotatePages">Indicates whether pages should automatically be oriented
        /// to match the orientation of the text (per OCR).</param>
        public void LoadOutputDocument(OutputDocument outputDocument, IEnumerable<Page> pages,
             IEnumerable<Page> deletedPages, IEnumerable<Page> viewedPages, bool autoRotatePages)
        {
            bool removedLoadNextDocumentButton = false;

            try
            {
                using (new PageLayoutControlUpdateLock(this))
                {
                    // While new pages are being added, remove the load next document control.
                    if (LoadNextDocumentVisible)
                    {
                        RemovePaginationControl(_loadNextDocumentButtonControl, false);
                        removedLoadNextDocumentButton = true;
                    }

                    Control lastPageControl = _flowLayoutPanel.Controls
                        .OfType<PaginationControl>()
                        .LastOrDefault()
                        as PageThumbnailControl;

                    // If the last control is currently a page control, we need to add a separator.
                    if (lastPageControl != null)
                    {
                        InsertPaginationControl(
                            new PaginationSeparator(CommitOnlySelection), index: -1);
                    }

                    // Create a page control for every page in sourceDocument.
                    foreach (var page in pages)
                    {
                        var pageControl = new PageThumbnailControl(outputDocument, page);
                        if (deletedPages != null && deletedPages.Contains(page))
                        {
                            pageControl.Deleted = true;
                        }

                        if (viewedPages != null && viewedPages.Contains(page))
                        {
                            pageControl.Viewed = true;
                        }

                        if (autoRotatePages && page.ProposedOrientation != 0)
                        {
                            page.ImageOrientation = page.ProposedOrientation;
                        }

                        InsertPaginationControl(pageControl, index: -1);
                    }

                    if (DefaultToCollapsed)
                    {
                        outputDocument.Collapsed = true;
                    }

                    outputDocument.DocumentOutput += HandleOutputDocument_DocumentOutput;

                    this.SafeBeginInvoke("ELI39641", () =>
                    {
                        // Ensure this control has keyboard focus after loading a document.
                        Focus();
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39642");
            }
            finally
            {
                try
                {
                    if (removedLoadNextDocumentButton)
                    {
                        AddPaginationControl(_loadNextDocumentButtonControl);
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI39643");
                }
            }
        }

        /// <summary>
        /// Deletes the pages of the specified <see paramref="outputDocument"/> from this instance.
        /// </summary>
        /// <param name="outputDocument">The <see cref="OutputDocument"/> whose pages are to be
        /// removed.</param>
        /// <returns>Returns the index at which the document existed.
        /// <para><b>Note</b></para>
        /// This is not a document index. The caller should not try to interpret this value;
        /// it's use should be limited to passing as the position argument of
        /// PaginationPanel.LoadFile or SelectDocumentAtPosition.
        /// </returns>
        public int DeleteOutputDocument(OutputDocument outputDocument)
        {
            try
            {
                int docPosition = GetDocumentPosition(outputDocument);
                DeleteControls(outputDocument.PageControls.ToArray());
                outputDocument.DocumentOutput -= HandleOutputDocument_DocumentOutput;

                // Delete and remove association with separator when removing from layout to ensure
                // proper associations of separators to documents and to avoid bad selection states
                // https://extract.atlassian.net/browse/ISSUE-13916
                // https://extract.atlassian.net/browse/ISSUE-15293
                if (outputDocument.PaginationSeparator != null)
                {
                    DeleteControls(new[] { outputDocument.PaginationSeparator });
                    outputDocument.PaginationSeparator = null;
                }

                return docPosition;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39507");
            }
        }

        /// <summary>
        /// Outputs the currently selected documents.
        /// </summary>
        public void OutputSelectedDocuments()
        {
            try
            {
                using (new TemporaryWaitCursor())
                {
                    foreach (OutputDocument document in FullySelectedDocuments.ToArray())
                    {
                        // If there was a problem outputting, abort the rest of the operation even
                        // if there were more documents to have been output.
                        if (!document.Output())
                        {
                            break;
                        }
                    }

                    UpdateCommandStates();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35434");
            }
        }

        /// <summary>
        /// Adds the specified <see paramref="pages"/> to this instance.
        /// <para><b>Note</b></para>
        /// It is assumed that none of these pages are duplicates of existing ones.
        /// </summary>
        /// <param name="pages">The <see cref="Page"/>s to add.</param>
        public void AddPages(params Page[] pages)
        {
            try
            {
                using (new PageLayoutControlUpdateLock(this))
                {
                    // While new pages are being added, remove the load next document control.
                    if (LoadNextDocumentVisible)
                    {
                        RemovePaginationControl(_loadNextDocumentButtonControl, false);
                    }

                    foreach (Page page in pages)
                    {
                        var newPageControl = new PageThumbnailControl(null, page);

                        int index = -1;
                        InitializePaginationControl(newPageControl, ref index);

                        // Now that these pages have been loaded, they are referenced by the page controls
                        // so the reference from this object can be released.
                        page.RemoveReference(this);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35672");
            }
            finally
            {
                try
                {
                    if (LoadNextDocumentVisible)
                    {
                        AddPaginationControl(_loadNextDocumentButtonControl);
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI35673");
                }
            }
        }

        /// <summary>
        /// Removes the specified <paramref name="control"/>.
        /// <para><b>Note</b>
        /// Any calls to this method outside of the
        /// <see cref="PaginationLayoutEngine.LayoutCompleted"/> event handler should be done
        /// in a <see cref="PageLayoutControlUpdateLock"/> that suspends all layouts until all
        /// controls that are to be moved/removed have been moved/removed. Otherwise controls may be
        /// unexpectedly removed in the midst of an operation.
        /// </para>
        /// </summary>
        /// <param name="control">The <see cref="PaginationControl"/> to remove.</param>
        /// <param name="dispose"><see langword="true"/> if the <see paramref="control"/> should be
        /// disposed; otherwise, <see langword="false"/>.</param>
        public void RemovePaginationControl(PaginationControl control, bool dispose)
        {
            try
            {
                if (!_flowLayoutPanel.Controls.Contains(control))
                {
                    return;
                }

                // The removed control should no longer be considered selected.
                if (control.Selected)
                {
                    SetSelected(control, false, true);
                }

                control.Click -= HandlePaginationControl_Click;
                control.MouseMove -= HandlePaginationControl_MouseMove;

                // Determine which controls are before and after the removed control to determine
                // how the removal affects the output documents.
                var previousPageControl = control.PreviousControl as PageThumbnailControl;
                var nextPageControl = control.NextControl as PageThumbnailControl;
                _flowLayoutPanel.Controls.Remove(control);

                if (control == PrimarySelection)
                {
                    PrimarySelection = null;
                }

                // If the removed control was a page control, it should no longer be displayed or be
                // part of any OutputDocument.
                var removedPageControl = control as PageThumbnailControl;
                if (removedPageControl != null)
                {
                    if (removedPageControl.PageIsDisplayed && dispose)
                    {
                        DisplayPage(removedPageControl, false);
                    }

                    if (removedPageControl == _toolTipControl)
                    {
                        _toolTip.RemoveAll();
                        _toolTipControl = null;
                    }

                    OutputDocument document = removedPageControl.Document;

                    if (document != null)
                    {
                        document.RemovePage(removedPageControl);
                    }

                    if (dispose)
                    {
                        OnPageDeleted(removedPageControl.Page, document);

                        // If multiple copies existed, refresh to remove the copy indicator on any
                        // single remaining copy.
                        if (removedPageControl.Page.MultipleCopiesExist)
                        {
                            this.SafeBeginInvoke("ELI35563", () => Refresh());
                        }
                    }

                    control.DoubleClick -= HandleThumbnailControl_DoubleClick;
                }

                // If the removed control was a separator, it will cause the bordering documents
                // to be merged.
                var separator = control as PaginationSeparator;
                if (separator != null)
                {
                    if (separator.DocumentDataPanel != null)
                    {
                        separator.CloseDataPanel(false, false);
                    }

                    separator.DocumentDataPanelRequest -=
                        HandlePaginationSeparator_DocumentDataPanelRequest;
                    separator.DocumentDataPanelClosed -=
                        HandlePaginationSeparator_DocumentDataPanelClosed;
                    separator.DocumentCollapsedChanged -=
                        HandlePaginationSeparator_DocumentCollapsedChanged;
                    separator.DocumentSelectedToCommitChanged -=
                        HandlePaginationSeparator_DocumentSelectedChanged;

                    if (nextPageControl != null)
                    {
                        if (previousPageControl != null)
                        {
                            OutputDocument firstDocument = previousPageControl.Document;
                            firstDocument.Collapsed = false;
                            MovePagesToDocument(firstDocument, nextPageControl);

                            OnSelectionChanged();
                        }
                    }
                }

                if (dispose)
                {
                    control.Dispose();
                }

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35436");
            }
        }

        /// <summary>
        /// Initializes the specified <see paramref="newControl"/> into this instance.
        /// </summary>
        /// <param name="newControl">The control <see cref="PaginationControl"/> to initialize.
        /// </param>
        /// <param name="index">The control index at which <see paramref="newControl"/> should be
        /// added.
        /// <para><b>Note</b></para>
        /// This index may be updated if inserting it at the specified index causes a deleted copy
        /// of the page prior to this one to be removed.</param>
        /// <returns><see langword="true"/> if the control was successfully initialized;
        /// <see langword="false"/> if it was not (in which case it will have been disposed.
        /// </returns>
        public bool InitializePaginationControl(PaginationControl newControl, ref int index)
        {
            try
            {
                if (!InsertPaginationControl(newControl, index))
                {
                    newControl.Dispose();
                    return false;
                }

                // Determine which controls are before and after the added control to determine how
                // the new control affects the output documents.
                var nextPageControl = newControl.NextControl as PageThumbnailControl;
                var previousPageControl = newControl.PreviousControl as PageThumbnailControl;
                var newPageControl = newControl as PageThumbnailControl;

                if (newPageControl != null)
                {
                    // This is a page control; figure out which document it should be added to.
                    OutputDocument document = (previousPageControl == null)
                        ? (nextPageControl == null) ? null : nextPageControl.Document
                        : previousPageControl.Document;
                    if (document != null)
                    {
                        newPageControl.Document = document;

                        int newPageIndex = (previousPageControl == null)
                            ? 0
                            : previousPageControl.DocumentPageIndex + 1;
                        document.InsertPage(newPageControl, newPageIndex);

                        // If this means the first page of the document is now from a different
                        // source document, rename the document based upon the new first page.
                        // Note: If newPageIndex == 0 then previousPageControl is null and thus there
                        // must be a non-null nextPageControl or we wouldn't be here so no need to check
                        if (newPageIndex == 0
                            // Don't update if the new page is deleted
                            // https://extract.atlassian.net/browse/ISSUE-13998
                            && !newPageControl.Deleted
                            && newPageControl.Page.SourceDocument != nextPageControl.Page.SourceDocument)
                        {
                            document.FileName =
                                _paginationUtility.GenerateOutputDocumentName(
                                    newPageControl.Page.OriginalDocumentName);
                        }
                    }
                    else
                    {
                        // There is no page control on either side of this document, a new document
                        // should be created with this as the one and only page.
                        document = GetOutputDocumentFromUtility(
                            newPageControl.Page.OriginalDocumentName);
                        newPageControl.Document = document;

                        document.AddPage(newPageControl);
                    }

                    var deletedCopies = newPageControl.Page.PageControlReferences
                        .Where(c => c != newPageControl && c.Deleted)
                        .ToArray();

                    // Any deleted copies of the page that exist in the pane should be deleted.
                    if (deletedCopies.Length > 0)
                    {
                        DeleteControls(deletedCopies);
                        index = _flowLayoutPanel.Controls.GetChildIndex(newPageControl);
                    }
                }
                else if (newControl is PaginationSeparator &&
                         nextPageControl != null && previousPageControl != null)
                {
                    // If this is a separator that is dividing two pages currently on the same
                    // document, generate a new document based on the second page and move all
                    // remaining page controls from the original document into the new document.
                    OutputDocument newDocument = GetOutputDocumentFromUtility(
                        nextPageControl.Page.OriginalDocumentName);

                    MovePagesToDocument(newDocument, nextPageControl);
                }

                OnSelectionChanged();

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35437");
            }
        }

        /// <summary>
        /// Removes any references to pages last copied to the clipboard so that the input document
        /// can be deleted/moved if there are no other existing copies of the pages' document(s).
        /// </summary>
        public void DereferenceLastClipboardData()
        {
            if (_currentClipboardData != null)
            {
                KeyValuePair<Page, bool>[] clipboardPages =
                    _currentClipboardData.GetPages(_paginationUtility)
                    .ToArray();
                foreach (var page in clipboardPages.Where(page => page.Key != null))
                {
                    page.Key.RemoveReference(this);
                }

                OnPagesDereferenced(clipboardPages.Select(p => p.Key).ToArray());
            }
        }

        /// <summary>
        /// Clears any existing selection of pages/separators.
        /// </summary>
        public void ClearSelection()
        {
            try
            {
                ClearSelection(true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39509");
            }
        }

        /// <summary>
        /// Selects the first <see cref="PageThumbnailControl"/>.
        /// </summary>
        public void SelectFirstPage()
        {
            try
            {
                var firstPage = _flowLayoutPanel.Controls
                    .OfType<PageThumbnailControl>()
                    .FirstOrDefault();

                if (firstPage != null)
                {
                    ClearSelection();
                    SelectControl(firstPage, true, true, true);
                    ProcessControlSelection(
                        activeControl: firstPage,
                        additionalControls: null,
                        select: true, modifierKeys:
                        Keys.None);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39646");
            }
        }

        /// <summary>
        /// Selects the specified <see paramref="pageControls"/>.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> to select.</param>
        /// <param name="scrollToPage"><c>true</c> if the <see paramref="pageControl"/> should be
        /// scrolled into view after being selected; otherwise, <c>false</c>.</param>
        public void SelectPage(PageThumbnailControl pageControl, bool scrollToPage = true)
        {
            try
            {
                if (pageControl != PrimarySelection)
                {
                    ProcessControlSelection(pageControl, scrollToPage);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40254");
            }
        }

        /// <summary>
        /// Selects all pages of the specified <see paramref="outputDocument"/>.
        /// </summary>
        /// <param name="outputDocument">The <see cref="OutputDocument"/> whose pages are to be
        /// selected.</param>
        /// <param name="pageIndex">The page number of <see paramref="outputDocument"/> to load
        /// or <see langword="null"/> to load the first page.</param>
        public void SelectDocument(OutputDocument outputDocument, int? pageNumber = null)
        {
            try
            {
                if (outputDocument != null && outputDocument.PageControls.Any())
                {
                    var firstControl = outputDocument.PageControls.First();
                    List<PaginationControl> selectedControls =
                        outputDocument.PageControls.ToList<PaginationControl>();
                    var separator = firstControl.PreviousControl as PaginationSeparator;
                    if (separator != null)
                    {
                        selectedControls.Add(separator);
                    }

                    firstControl = (pageNumber == null || pageNumber > outputDocument.PageControls.Count)
                        ? firstControl
                        : outputDocument.PageControls[pageNumber.Value - 1];

                    ProcessControlSelection(
                        activeControl: firstControl,
                        additionalControls: selectedControls,
                        select: true,
                        modifierKeys: Keys.None);
                }
                else
                {
                    ClearSelection();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39715");
            }
        }

        /// <summary>
        /// Selected the document at the specified <see paramref="position"/>.
        /// </summary>
        /// <param name="position">The position of the document to select.
        /// <para><b>Note</b></para>
        /// This is not a document index. The value used here should be an index reported by
        /// <see cref="GetDocumentPosition"/>.
        /// </param>
        public bool SelectDocumentAtPosition(int position)
        {
            try
            {
                if (position >= 0 && position < _flowLayoutPanel.Controls.OfType<PaginationControl>().Count())
                {
                    var pageControl = _flowLayoutPanel.Controls
                        .OfType<Control>()
                        .Skip(position)
                        .OfType<PageThumbnailControl>()
                        .FirstOrDefault();
                    if (pageControl != null && pageControl.Document != null)
                    {
                        SelectDocument(pageControl.Document);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40147");
            }
        }

        /// <summary>
        /// Gets the position of the document in <see cref="PaginationPanel"/>.
        /// <para><b>Note</b></para>
        /// This is not a document index. The caller should not try to interpret this value;
        /// it's use should be limited to passing as the position argument of
        /// PaginationPanel.LoadFile or SelectDocumentAtPosition.
        /// </summary>
        /// <param name="outputDocument">The <see cref="OutputDocument"/> for which the position is
        /// needed.</param>
        /// <returns>The position of the document or -1 if none of the document pages are currently
        /// displayed.</returns>
        public int GetDocumentPosition(OutputDocument outputDocument)
        {
            var pageControls = outputDocument.PageControls
                .Where(c => _flowLayoutPanel.Controls.Contains(c));

            if (pageControls.Any())
            {
                return pageControls
                    .Select(c => _flowLayoutPanel.Controls.GetChildIndex(c))
                    .Min();
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets the loaded <see cref="PageThumbnailControl"/>s.
        /// </summary>
        /// <returns>The loaded <see cref="PageThumbnailControl"/>s.</returns>
        public IEnumerable<PageThumbnailControl> PageControls
        {
            get
            {
                try
                {
                    var selectedControls = _flowLayoutPanel.Controls
                        .OfType<PageThumbnailControl>();

                    return selectedControls;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40173");
                }
            }
        }

        /// <summary>
        /// Gets the currently selected <see cref="PageThumbnailControl"/>s. 
        /// </summary>
        /// <returns>The currently selected <see cref="PageThumbnailControl"/>s.</returns>
        public IEnumerable<PageThumbnailControl> SelectedPageControls
        {
            get
            {
                try
                {
                    var selectedControls = _flowLayoutPanel.Controls
                        .OfType<PageThumbnailControl>()
                        .Where(control => control.Selected);

                    return selectedControls;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40174");
                }
            }
        }

        /// <summary>
        /// Performs a full layout of this control
        /// </summary>
        public void PerformFullLayout()
        {
            try
            {
                _flowLayoutPanel.PerformLayout();
                PerformLayout();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40221");
            }
        }

        /// <summary>
        /// Updates the content of the document separator for the specified <see paramref="document"/>.
        /// This includes assigning the separator to the document to the if this has not already been
        /// done.
        /// </summary>
        /// <param name="document">The document.</param>
        public void UpdateDocumentSeparator(OutputDocument document)
        {
            try
            {
                var separator = _flowLayoutPanel.Controls
                    .OfType<PaginationSeparator>()
                    .SingleOrDefault(s => document == s.Document);

                separator?.Invalidate();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44721");
            }
        }

        /// <summary>
        /// Snaps the active data panel so that it is flush with the top of the view by adjusting
        /// scroll position.
        /// </summary>
        public void SnapDataPanelToTop()
        {
            try
            {
                if (DocumentInDataEdit.PaginationSeparator != null)
                {
                    _flowLayoutPanel.ScrollControlIntoViewManual(DocumentInDataEdit.PaginationSeparator,
                        flushWithTop: true);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45750");
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _cutMenuItem.ShortcutKeyDisplayString = "Ctrl + X";
                _cutMenuItem.ShowShortcutKeys = true;
                _cutCommand = new ApplicationCommand(Shortcuts,
                    new Keys[] { Keys.Control | Keys.X }, HandleCutSelectedControls,
                    JoinToolStripItems(_cutMenuItem, _paginationUtility.CutMenuItem),
                    false, true, false);
                _cutMenuItem.Click += HandleCutMenuItem_Click;

                _copyMenuItem.ShortcutKeyDisplayString = "Ctrl + C";
                _copyMenuItem.ShowShortcutKeys = true;
                _copyCommand = new ApplicationCommand(Shortcuts,
                    new Keys[] { Keys.Control | Keys.C }, HandleCopySelectedControls,
                    JoinToolStripItems(_copyMenuItem, _paginationUtility.CopyMenuItem),
                    false, true, false);
                _copyMenuItem.Click += HandleCopyMenuItem_Click;

                _pasteMenuItem.ShortcutKeyDisplayString = "Ctrl + V";
                _pasteMenuItem.ShowShortcutKeys = true;
                _pasteCommand = new ApplicationCommand(Shortcuts,
                    new Keys[] { Keys.Control | Keys.V }, HandlePaste,
                    JoinToolStripItems(_pasteMenuItem, _paginationUtility.PasteMenuItem),
                    false, true, false);
                _pasteMenuItem.Click += HandlePasteMenuItem_Click;

                _deleteMenuItem.ShortcutKeyDisplayString = "Del";
                _deleteMenuItem.ShowShortcutKeys = true;
                _deleteCommand = new ApplicationCommand(Shortcuts,
                    new Keys[] { Keys.Delete }, HandleDeleteSelectedItems,
                    JoinToolStripItems(_deleteMenuItem, _paginationUtility.DeleteMenuItem),
                    false, true, false);
                _deleteMenuItem.Click += HandleDeleteMenuItem_Click;

                _unDeleteMenuItem.ShortcutKeyDisplayString = "Shift + Del";
                _unDeleteMenuItem.ShowShortcutKeys = true;
                _unDeleteCommand = new ApplicationCommand(Shortcuts,
                    new Keys[] { Keys.Shift | Keys.Delete }, HandleUnDeleteSelectedItems,
                    JoinToolStripItems(_unDeleteMenuItem),
                    false, true, false);
                _unDeleteMenuItem.Click += HandleUnDeleteMenuItem_Click;

                _printMenuItem.ShortcutKeyDisplayString = "Ctrl + P";
                _printMenuItem.ShowShortcutKeys = true;
                _printCommand = new ApplicationCommand(Shortcuts,
                    new Keys[] { Keys.Control | Keys.P }, HandlePrintSelectedItems,
                    JoinToolStripItems(_printMenuItem, _paginationUtility.PrintMenuItem),
                    false, true, false);
                _printMenuItem.Click += HandlePrintMenuItem_Click;

                _toggleDocumentSeparatorMenuItem.ShortcutKeyDisplayString = "Space";
                _toggleDocumentSeparatorMenuItem.ShowShortcutKeys = true;
                _toggleDocumentSeparatorCommand = new ApplicationCommand(Shortcuts,
                    new Keys[] { Keys.Space }, HandleAddDocumentSeparator,
                    JoinToolStripItems(
                        _toggleDocumentSeparatorMenuItem, _paginationUtility.ToggleDocumentSeparatorMenuItem),
                    false, true, false);
                _toggleDocumentSeparatorMenuItem.Click += HandleToggleDocumentSeparator_Click;

                _outputDocumentCommand = new ApplicationCommand(Shortcuts,
                    new Keys[] { Keys.Control | Keys.S }, HandleOutputDocument,
                    null, false, true, false);

                InitializeShortcuts();

                ContextMenuStrip = new ContextMenuStrip();
                ContextMenuStrip.Items.Add(_cutMenuItem);
                ContextMenuStrip.Items.Add(_copyMenuItem);
                ContextMenuStrip.Items.Add(_pasteMenuItem);
                ContextMenuStrip.Items.Add(_deleteMenuItem);
                ContextMenuStrip.Items.Add(_unDeleteMenuItem);
                ContextMenuStrip.Items.Add(new ToolStripSeparator());
                ContextMenuStrip.Items.Add(_printMenuItem);
                ContextMenuStrip.Items.Add(new ToolStripSeparator());
                ContextMenuStrip.Items.Add(_toggleDocumentSeparatorMenuItem);
                ContextMenuStrip.Opening += HandleContextMenuStrip_Opening;
                ContextMenuStrip.Closing += HandleContextMenuStrip_Closing;

                if (LoadNextDocumentVisible)
                {
                    _loadNextDocumentButtonControl.ButtonClick +=
                        HandleLoadNextDocumentButtonControl_ButtonClick;
                }

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35439");
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        /// <returns><see langword="true"/> if the character was processed by the control; 
        /// <see langword="false"/> if the character was not processed.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                // If the _documentDataPanelControl has focus, keystrokes shouldn't be treated as
                // pagination shortcut keys.
                if (DocumentDataPanel?.PanelControl != null &&
                    DocumentDataPanel.PanelControl.ContainsFocus)
                {
                    IgnoreShortcutKey = true;
                }

                // This key was not processed, bubble it up to the base class.
                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI41275", ex);
                return false;
            }
            finally
            {
                IgnoreShortcutKey = false;
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if managed resources should be disposed;
        /// otherwise, <see langword="false"/>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (_updateLock != null)
                    {
                        _updateLock.Dispose();
                        _updateLock = null;
                    }

                    // Allows a much quick response to remove and dispose of the controls first
                    // rather than disposing with the controls still loaded.
                    PaginationControl[] controls = _flowLayoutPanel.Controls
                        .OfType<PaginationControl>()
                        .Where(control => control != _loadNextDocumentButtonControl)
                        .ToArray();
                    _flowLayoutPanel.SuspendLayout();
                    _flowLayoutPanel.Controls.Clear();

                    foreach (Control control in controls)
                    {
                        control.Dispose();
                    }

                    if (components != null)
                    {
                        components.Dispose();
                        components = null;
                    }

                    if (_dropLocationIndicator != null)
                    {
                        _dropLocationIndicator.Dispose();
                        _dropLocationIndicator = null;
                    }

                    if (_dragDropScrollTimer != null)
                    {
                        _dragDropScrollTimer.Dispose();
                        _dragDropScrollTimer = null;
                    }

                    if (_loadNextDocumentButtonControl != null)
                    {
                        _loadNextDocumentButtonControl.Dispose();
                        _loadNextDocumentButtonControl = null;
                    }

                    if (_toolTip != null)
                    {
                        _toolTipControl = null;
                        _toolTip.Dispose();
                        _toolTip = null;
                    }
                }
                catch { }
            }
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of a <see cref="PaginationControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePaginationControl_Click(object sender, EventArgs e)
        {
            try
            {
                var clickedControl = (PaginationControl)sender;

                if (clickedControl != _loadNextDocumentButtonControl)
                {
                    // [DotNetRCAndUtils:965]
                    // Clicking on page thumbnail controls should always select unless the control key
                    // is the only modifier key.
                    bool select = !clickedControl.Selected || (Control.ModifierKeys & Keys.Control) == 0;
                    ProcessControlSelection(
                        activeControl: clickedControl,
                        additionalControls: null,
                        select: select,
                        modifierKeys: Control.ModifierKeys);
                }

                clickedControl.Focus();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35444");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DoubleClick"/> event of a <see cref="PaginationControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleThumbnailControl_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                var pageControl = sender as PageThumbnailControl;
                if (pageControl != null)
                {
                    ProcessControlSelection(
                        activeControl: pageControl,
                        additionalControls: pageControl.Document.PageControls,
                        select: true,
                        modifierKeys: Control.ModifierKeys);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35445");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.VisibleChanged"/> event of
        /// <see cref="PageThumbnailControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleThumbnailControl_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                var pageControl = (PageThumbnailControl)sender;
                if (!pageControl.Visible && pageControl == _commandTargetControl)
                {
                    // If the selected page was collapsed, select the next visible page instead.
                    this.SafeBeginInvoke("ELI40223", () =>
                    {
                        for (PaginationControl c = pageControl.NextControl; c != null; c = c.NextControl)
                        {
                            PageThumbnailControl nextPageControl = c as PageThumbnailControl;
                            if (nextPageControl != null && nextPageControl.Visible)
                            {
                                ProcessControlSelection(nextPageControl);
                                break;
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40222");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.MouseMove"/> event of a <see cref="PaginationControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance
        /// containing the event data.</param>
        void HandlePaginationControl_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var originControl = sender as PaginationControl;

                // Do not allow drag events to start by dragging over a separator.
                if (originControl is PaginationSeparator)
                {
                    return;
                }

                // Do not allow modification of documents that have already been output.
                if (SelectedControls
                        .OfType<PageThumbnailControl>()
                        .Any(pageControl => pageControl.Document.OutputProcessed))
                {
                    return;
                }

                // Assigning a ToolTip instance for all page controls uses a lot of GDI handles.
                // Instead, dynamically assign a single _toolTip instance the control the mouse is
                // currently over.
                if (_toolTipControl != originControl)
                {
                    if (_toolTipControl != null)
                    {
                        _toolTip.RemoveAll();
                    }

                    _toolTipControl = originControl as PageThumbnailControl;
                }

                if (_toolTipControl != null)
                {
                    _toolTipControl.SetToolTip(_toolTip);
                }

                // If the mouse button is down and the sending control is already selected, start a
                // drag-and-drop operation.
                if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                {
                    if (originControl != null && originControl != _loadNextDocumentButtonControl)
                    {
                        // Don't start a drag and drop operation unless the user has dragged out of
                        // the origin control to help prevent accidental drag/drops.
                        Point mouseLocation = PointToClient(Control.MousePosition);
                        var pageThumbnailControl = GetThumbnailControlAtPoint(mouseLocation);
                        var originThumbnailControl = sender as PageThumbnailControl;

                        if (pageThumbnailControl != null
                            && pageThumbnailControl != originControl
                            && originThumbnailControl?.Document?.OutputProcessed != true
                            && !pageThumbnailControl.Document.OutputProcessed)
                        {
                            // [DotNetRCAndUtils:968]
                            // If the control where the drag originated is not selected, imply
                            // selection of that control when starting the drag operation.
                            if (!originControl.Selected)
                            {
                                ProcessControlSelection(originControl);
                            }

                            var dataObject = new DataObject(_DRAG_DROP_DATA_FORMAT, this);

                            try
                            {
                                DoDragDrop(dataObject, DragDropEffects.Move);
                            }
                            finally
                            {
                                if (_dropLocationIndex >= 0)
                                {
                                    _dropLocationIndex = -1;
                                    Controls.Remove(_dropLocationIndicator);
                                }

                                // If drag/drop scrolling was active when the drag/drop event ends,
                                // stop the scrolling now.
                                if (_dragDropScrollTimer.Enabled)
                                {
                                    _dragDropScrollTimer.Stop();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35446");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DragEnter"/> event of a child control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing
        /// the event data.</param>
        void Handle_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(_DRAG_DROP_DATA_FORMAT))
                {
                    // Controls from within this application
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    // No data or unsupported data type.
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35447");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DragLeave"/> event of a child control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Handle_DragLeave(object sender, EventArgs e)
        {
            try
            {
                EnableDragScrolling(Control.MousePosition);

                if (_dropLocationIndex >= 0)
                {
                    _dropLocationIndex = -1;
                    Controls.Remove(_dropLocationIndicator);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35448");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DragOver"/> event of a child control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance
        /// containing the event data.</param>
        void Handle_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                EnableDragScrolling(new Point(e.X, e.Y));
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI35449");
            }

            try
            {
                Point dragLocation = PointToClient(new Point(e.X, e.Y));
                var pageThumbnailControl = GetThumbnailControlAtPoint(dragLocation);

                if (pageThumbnailControl != null
                    && pageThumbnailControl.Visible
                    && pageThumbnailControl.Document.OutputProcessed != true)
                {
                    _dropLocationIndex = _flowLayoutPanel.Controls.IndexOf(pageThumbnailControl);
                    Point location;

                    // Because a lot of padding may be added to extend a page control out to the end
                    // of a row, take the padding into account when deciding whether a drop should
                    // occur before or after the page.
                    int left = pageThumbnailControl.Left + pageThumbnailControl.Padding.Left;
                    int right = pageThumbnailControl.Right - pageThumbnailControl.Padding.Right;
                    int center = (right - left) / 2;

                    if ((dragLocation.X - pageThumbnailControl.Left) > center)
                    {
                        _dropLocationIndex++;
                        location = pageThumbnailControl.TrailingInsertionPoint;
                    }
                    else
                    {
                        location = pageThumbnailControl.PreceedingInsertionPoint;
                    }

                    ShowDropLocationIndicator(location, pageThumbnailControl.Height);
                    e.Effect = e.AllowedEffect;
                }
                else
                {
                    _dropLocationIndex = -1;
                    Controls.Remove(_dropLocationIndicator);
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35450");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DragDrop"/> event of a child control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing
        /// the event data.</param>
        void Handle_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (_dropLocationIndex >= 0)
                {
                    var sourceLayoutControl =
                        e.Data.GetData(_DRAG_DROP_DATA_FORMAT) as PageLayoutControl;
                    if (sourceLayoutControl != null)
                    {
                        MoveSelectedControls(sourceLayoutControl, _dropLocationIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35451");
            }
            finally
            {
                try
                {
                    if (_dropLocationIndex >= 0)
                    {
                        _dropLocationIndex = -1;
                        Controls.Remove(_dropLocationIndicator);
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI35619");
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_flowLayoutPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFlowLayoutPanel_Click(object sender, EventArgs e)
        {
            try
            {
                ClearSelection();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35453");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.ClientSizeChanged"/> event of the
        /// <see cref="_flowLayoutPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFlowLayoutPanel_ClientSizeChanged(object sender, EventArgs e)
        {
            try
            {
                // Invoke a Layout of the _flowLayoutPanel when the client size changes to prevent
                // horizontal scroll bars from being displayed.
                if (!_flowLayoutPanelResizing)
                {
                    _flowLayoutPanelResizing = true;

                    this.SafeBeginInvoke("ELI39997", () =>
                    {
                        _flowLayoutPanel.PerformLayout();
                        _flowLayoutPanelResizing = false;
                    },
                        true,
                        (ex) => _flowLayoutPanelResizing = false);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39998");
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationLayoutEngine.LayoutCompleted"/> event of the
        /// <see cref="_flowLayoutPanel"/>'s <see cref="PaginationLayoutEngine"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LayoutCompletedEventArgs"/> instance containing
        /// the event data.</param>
        void PaginationLayoutEngine_LayoutCompleted(object sender, LayoutCompletedEventArgs e)
        {
            try
            {
                // Remove any redundant controls found during the layout.
                // NOTE: It is assumed that any operations that result in the removal of controls
                // happen in a PageLayoutControlUpdateLock which will have suspended layouts.
                // Otherwise this handler may result in the removal of controls the operation
                // expects to still be around.
                // This call itself does not need to be in a PageLayoutControlUpdateLock since this
                // event will be raised during an layout operation.
                foreach (PaginationControl control in e.RedundantControls)
                {
                    RemovePaginationControl(control, true);
                }

                // https://extract.atlassian.net/browse/ISSUE-15414
                // When a DEP has been opened, "snap" it to the top of the panel to help ensure the
                // is clear about which data is currently being edited.
                if (_pendingSnapDataPanelToTop)
                {
                    _pendingSnapDataPanelToTop = false;

                    SnapDataPanelToTop();

                    PerformLayout();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35653");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:ContextMenuStrip.Opening"/> event of the
        /// <see cref="ContextMenuStrip"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance
        /// containing the event data.</param>
        void HandleContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            try
            {
                // Whenever the context menu opens, use the current mouse position as the target of
                // any context menu command rather than the active control.
                Point mouseLocation = PointToClient(MousePosition);
                _commandTargetControl = GetThumbnailControlAtPoint(mouseLocation);
                if (_commandTargetControl != null && !_commandTargetControl.Selected)
                {
                    ProcessControlSelection(_commandTargetControl);
                }
                else if (_commandTargetControl == null)
                {
                    ClearSelection();
                }

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35454");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:ContextMenuStrip.Closing"/> event of the
        /// <see cref="ContextMenuStrip"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ToolStripDropDownClosingEventArgs"/>
        /// instance containing the event data.</param>
        void HandleContextMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            try
            {
                // Once the context menu is closed, _commandTargetControl should go back to being
                // the _commandTargetControl.
                _commandTargetControl = GetActiveControl();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35558");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_cutMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCutMenuItem_Click(object sender, EventArgs e)
        {
            HandleCutSelectedControls();
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_copyMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCopyMenuItem_Click(object sender, EventArgs e)
        {
            HandleCopySelectedControls();
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_pasteMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePasteMenuItem_Click(object sender, EventArgs e)
        {
            HandlePaste();
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_deleteMenuItem"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDeleteMenuItem_Click(object sender, EventArgs e)
        {
            HandleDeleteSelectedItems();
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_unDeleteMenuItem"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleUnDeleteMenuItem_Click(object sender, EventArgs e)
        {
            HandleUnDeleteSelectedItems();
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_printMenuItem"/> 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePrintMenuItem_Click(object sender, EventArgs e)
        {
            HandlePrintSelectedItems();
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_toggleDocumentSeparatorMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleToggleDocumentSeparator_Click(object sender, EventArgs e)
        {
            HandleToggleDocumentSeparator(addOnly: false);
        }

        /// <summary>
        /// Handles the <see cref="Timer.Tick"/> event of the <see cref="_dragDropScrollTimer"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDragDropScrollTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Determine the existing scroll position (or what it should be as a result of the
                // last tick event).
                int lastScrollPos = _flowLayoutPanel.VerticalScroll.Value;
                if (_dragDropScrollPos >= 0)
                {
                    lastScrollPos = (_scrollSpeed > 0)
                        ? Math.Max(_dragDropScrollPos, lastScrollPos)
                        : Math.Min(_dragDropScrollPos, lastScrollPos);
                }

                _dragDropScrollPos = lastScrollPos + _scrollSpeed;

                // Ensure the scroll position stays within range.
                if (_dragDropScrollPos < _flowLayoutPanel.VerticalScroll.Minimum)
                {
                    _dragDropScrollPos = _flowLayoutPanel.VerticalScroll.Minimum;
                }
                else if (_dragDropScrollPos > _flowLayoutPanel.VerticalScroll.Maximum)
                {
                    _dragDropScrollPos = _flowLayoutPanel.VerticalScroll.Maximum;
                }

                _flowLayoutPanel.VerticalScroll.Value = _dragDropScrollPos;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35581");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_loadNextDocumentButtonControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleLoadNextDocumentButtonControl_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                OnLoadNextDocumentRequest();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35628");
            }
        }

        /// <summary>
        /// Handles the <see cref="OutputDocument.DocumentOutput"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOutputDocument_DocumentOutput(object sender, EventArgs e)
        {
            try
            {
                var outputDocument = (OutputDocument)sender;
                DeleteControls(outputDocument.PageControls.ToArray());

                outputDocument.DocumentOutput -= HandleOutputDocument_DocumentOutput;
            }
            catch (Exception ex)
            {
                // This is only being called as the results of UI event handler that itself will
                // catch and display; can throw here.
                throw ex.AsExtract("ELI39510");
            }
        }

        /// <summary>
        /// Handles the <see cref="DocumentDataPanelRequest"/> event of a
        /// <see cref="PaginationSeparator"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DocumentDataPanelRequestEventArgs"/> instance containing
        /// the event data.</param>
        void HandlePaginationSeparator_DocumentDataPanelRequest(object sender,
            DocumentDataPanelRequestEventArgs e)
        {
            try
            {
                OnDocumentDataPanelRequest(e);

                // If a data panel was provided to be opened...
                if (e.DocumentDataPanel != null)
                {
                    DocumentDataPanel = e.DocumentDataPanel;
                    DocumentInDataEdit = e.OutputDocument;

                    var activePage = PrimarySelection as PageThumbnailControl;
                    if (activePage == null || activePage.Document != e.OutputDocument)
                    {
                        // Always select a page to prevent UI from hanging
                        // https://extract.atlassian.net/browse/ISSUE-14206
                        var pageToSelect =
                               e.OutputDocument.PageControls.FirstOrDefault(c => !c.Deleted)
                            ?? e.OutputDocument.PageControls.First();
                        ProcessControlSelection(pageToSelect);
                    }

                    // Let the data entry panel know if the current page corresponds to the document
                    // it has loaded.
                    DocumentDataPanel.PrimaryPageIsForActiveDocument =
                        (DocumentInDataEdit != null) &&
                        (DocumentInDataEdit == (_primarySelection as PageThumbnailControl)?.Document);

                    // https://extract.atlassian.net/browse/ISSUE-15414
                    // When a DEP has been opened, "snap" it to the top of the panel to help ensure the
                    // is clear about which data is currently being edited.
                    _pendingSnapDataPanelToTop = true;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40175");
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationSeparator.DocumentDataPanelClosed"/> event of a
        /// <see cref="PaginationSeparator"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DocumentDataPanelRequestEventArgs"/> instance containing
        /// the event data.</param>
        void HandlePaginationSeparator_DocumentDataPanelClosed(object sender, EventArgs e)
        {
            try
            {
                DocumentDataPanel = null;
                DocumentInDataEdit = null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40229");
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationSeparator.DocumentSelectedToCommitChanged"/> event of a
        /// <see cref="PaginationSeparator"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePaginationSeparator_DocumentSelectedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40224");
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationSeparator.DocumentCollapsedChanged"/> event of a
        /// <see cref="PaginationSeparator"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePaginationSeparator_DocumentCollapsedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateCommandStates();

                // https://extract.atlassian.net/browse/ISSUE-14326
                // Ensure that collapsing a document doesn't cause it to scroll out of view.
                var separator = (PaginationSeparator)sender;
                if (separator.Collapsed)
                {
                    _flowLayoutPanel.ScrollControlIntoViewManual(separator, flushWithTop: false);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40219");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets the currently selected <see cref="PaginationControl"/>s.
        /// </summary>
        /// <returns>The currently selected <see cref="PaginationControl"/>s.</returns>
        IEnumerable<PaginationControl> SelectedControls
        {
            get
            {
                var selectedControls = _flowLayoutPanel.Controls
                    .OfType<PaginationControl>()
                    .Where(control => control.Selected);

                return selectedControls;
            }
        }

        /// <summary>
        /// Gets the active <see cref="PaginationControl"/>.
        /// </summary>
        /// <param name="first"><see langword="true"/> to select the first of multiple selected
        /// pages where none of the pages are being displayed or <see langword="false"/> to select
        /// the last.</param>
        /// <returns>The active <see cref="PaginationControl"/> if there is one; otherwise,
        /// <see langword="null"/></returns>
        PaginationControl GetActiveControl(bool first = true)
        {
            if (PrimarySelection != null && PrimarySelection.Selected)
            {
                return _primarySelection;
            }

            var pageControl = first
                ? SelectedControls.OfType<NavigablePaginationControl>().FirstOrDefault()
                : SelectedControls.OfType<NavigablePaginationControl>().LastOrDefault();

            return pageControl;
        }

        /// <summary>
        /// Adds the specified <see paramref="control"/> to this instance's
        /// <see cref="PaginationControl"/> as the last control
        /// </summary>
        /// <param name="control">The <see cref="PaginationControl"/> to add.</param>
        /// <returns><see langword="true"/> if the pagination control was added; otherwise,
        /// <see langword="false"/> (such as if the control is redundant).</returns>
        bool AddPaginationControl(PaginationControl control)
        {
            return InsertPaginationControl(control, index: -1);
        }

        /// <summary>
        /// Adds the specified <see paramref="control"/> to this instance's
        /// <see cref="PaginationControl"/> collection as the specified <see paramref="index"/>.
        /// </summary>
        /// <param name="index">The index at which the control should be added or -1 to add it to
        /// the end.</param>
        /// <param name="control">The <see cref="PaginationControl"/> to add.</param>
        /// <returns><see langword="true"/> if the pagination control was added; otherwise,
        /// <see langword="false"/>. (such as if the control is redundant).</returns>
        bool InsertPaginationControl(PaginationControl control, int index = -1)
        {
            // Always place a pagination separator ahead of the load next document button to help
            // distinguish it from the document above it.
            if (index == -1 && control is LoadNextDocumentButtonControl)
            {
                var lastControl = _flowLayoutPanel.Controls.OfType<PaginationControl>().LastOrDefault();
                if (lastControl != null && !(lastControl is PaginationSeparator))
                {
                    AddPaginationControl(new PaginationSeparator(CommitOnlySelection));
                }
            }

            bool isPageControl = control is PageThumbnailControl;
            bool areAnyPaginationControls = _flowLayoutPanel.Controls.OfType<PaginationControl>().Any();

            if (!areAnyPaginationControls && _flowLayoutPanel.Controls.Count > 0)
            {
                _flowLayoutPanel.Controls.Clear();
            }

            // Precede the first page with a separator to serve as a header for the document.
            if (isPageControl && !areAnyPaginationControls)
            {
                AddPaginationControl(new PaginationSeparator(CommitOnlySelection));
            }

            // A pagination separator is meaningless as the first control.
            if (!isPageControl && index > 0 &&
                _flowLayoutPanel.Controls[index - 1] is PaginationSeparator)
            {
                return false;
            }

            _flowLayoutPanel.Controls.Add(control);
            if (index >= 0)
            {
                _flowLayoutPanel.Controls.SetChildIndex(control, index);
            }
            else
            {
                _flowLayoutPanel.Controls.SetChildIndex(control, _flowLayoutPanel.Controls.Count - 1);
            }

            control.Click += HandlePaginationControl_Click;
            control.MouseMove += HandlePaginationControl_MouseMove;

            var asPaginationSeparator = control as PaginationSeparator;

            if (isPageControl)
            {
                control.DoubleClick += HandleThumbnailControl_DoubleClick;
                control.VisibleChanged += new EventHandler(HandleThumbnailControl_VisibleChanged);
            }
            else if (asPaginationSeparator != null)
            {
                asPaginationSeparator.DocumentDataPanelRequest +=
                    HandlePaginationSeparator_DocumentDataPanelRequest;
                asPaginationSeparator.DocumentDataPanelClosed +=
                    HandlePaginationSeparator_DocumentDataPanelClosed;
                asPaginationSeparator.DocumentCollapsedChanged +=
                    HandlePaginationSeparator_DocumentCollapsedChanged;
                asPaginationSeparator.DocumentSelectedToCommitChanged +=
                    HandlePaginationSeparator_DocumentSelectedChanged;
            }

            return true;
        }

        /// <summary>
        /// Moves the selected controls in <see paramref="sourceLayoutControl"/> to the specified
        /// <see paramref="targetIndex"/> of this control.
        /// </summary>
        /// <param name="sourceLayoutControl">The <see cref="PageLayoutControl"/> who's selected
        /// controls are to be moved.</param>
        /// <param name="targetIndex">The index to which the selected controls should be moved.
        /// </param>
        void MoveSelectedControls(PageLayoutControl sourceLayoutControl, int targetIndex)
        {
            using (new PageLayoutControlUpdateLock(this, forceFullLayout: true))
            {
                var primarySelection = PrimarySelection;
                var selectedControls = sourceLayoutControl.SelectedControls
                    .OfType<PageThumbnailControl>()
                    .ToArray();

                foreach (PaginationControl control in selectedControls)
                {
                    if (sourceLayoutControl == this &&
                        _flowLayoutPanel.Controls.IndexOf(control) < targetIndex)
                    {
                        targetIndex--;
                    }
                    sourceLayoutControl.RemovePaginationControl(control, false);
                }

                foreach (PaginationControl control in selectedControls)
                {
                    if (targetIndex > 0 && control is PaginationSeparator &&
                        _flowLayoutPanel.Controls[targetIndex - 1] is PaginationSeparator)
                    {
                        control.Dispose();
                    }
                    else if (InitializePaginationControl(control, ref targetIndex))
                    {
                        SetSelected(control, true, true);
                        targetIndex++;
                    }
                }

                // Restore the original PrimarySelection.
                PrimarySelection = primarySelection;
            }
        }

        /// <summary>
        /// Moves all <see cref="PageThumbnailControl"/>s starting with
        /// <see paramref="thumbnailControl"/> to <see paramref="newDocument"/> up to the next
        /// <see cref="PaginationSeparator"/> to follow <see paramref="thumbnailControl"/>.
        /// </summary>
        /// <param name="newDocument">The <see cref="OutputDocument"/> to which the
        /// <see cref="PageThumbnailControl"/>s should be added.</param>
        /// <param name="thumbnailControl">The first <see cref="PageThumbnailControl"/> of the
        /// sequence that should be moved to <see paramref="newDocument"/>.</param>
        void MovePagesToDocument(OutputDocument newDocument, PageThumbnailControl thumbnailControl)
        {
            OutputDocument oldDocument = thumbnailControl.Document;
            int index = _flowLayoutPanel.Controls.IndexOf(thumbnailControl);

            // Move each page control starting with thumbnailControl to newDocument until the page
            // controls are no longer from oldDocument.
            while (thumbnailControl != null && thumbnailControl.Document == oldDocument)
            {
                thumbnailControl.Document.RemovePage(thumbnailControl);
                newDocument.AddPage(thumbnailControl);

                index++;
                if (index == _flowLayoutPanel.Controls.OfType<PaginationControl>().Count())
                {
                    break;
                }

                thumbnailControl =
                    _flowLayoutPanel.Controls[index] as PageThumbnailControl;
            }
        }

        /// <summary>
        /// Shows the <see cref="_dropLocationIndicator"/> at the specified
        /// <see paramref="location"/>.
        /// </summary>
        /// <param name="location">The <see cref="Point"/> where the location indicator should be
        /// drawn.</param>
        /// <param name="height">The height the location indicator should be</param>
        void ShowDropLocationIndicator(Point location, int height)
        {
            location.Offset(-_dropLocationIndicator.Width / 2, 0);

            // If the _dropLocationIndicator is already visible, but needs to be moved, remove it
            // completely, otherwise the background may retain some artifacts from the controls it
            // was previously over.
            if (Controls.Contains(_dropLocationIndicator) &&
                _dropLocationIndicator.Location != location)
            {
                Controls.Remove(_dropLocationIndicator);
            }

            if (!Controls.Contains(_dropLocationIndicator))
            {
                Controls.Add(_dropLocationIndicator);
                _dropLocationIndicator.BringToFront();
            }
            _dropLocationIndicator.Location = location;
            _dropLocationIndicator.Height = height;

            // Make sure the rectangle we are updating is large enough to intersect with bordering
            // controls.
            Rectangle updateRect = _dropLocationIndicator.Bounds;
            updateRect.Inflate(5, 5);

            // To make the background of _dropLocationIndicator "transparent", update the region of
            // this control under the _dropLocationIndicator.
            Invalidate(updateRect, true);
            Update();

            // Now, the pagination controls under it should be refreshed as well.
            foreach (Control paginationControl in _flowLayoutPanel.Controls)
            {
                if (updateRect.IntersectsWith(paginationControl.Bounds))
                {
                    paginationControl.Refresh();
                }
            }

            // Finally, trigger the _dropLocationIndicator itself to paint on top of the
            // "background" that has just been drawn.
            _dropLocationIndicator.Invalidate();
        }

        /// <summary>
        /// Applies/toggles selection based on the specified <see paramref="activeControl"/> and the
        /// currently depressed modifier keys.
        /// </summary>
        /// <param name="activeControl">The <see cref="PaginationControl"/> that should be
        /// considered active.</param>
        /// <param name="scrollToControl"><c>true</c> if the <see paramref="activeControl"/> should be
        /// scrolled into view after being selected; otherwise, <c>false</c>.</param></param>
        void ProcessControlSelection(PaginationControl activeControl, bool scrollToControl = true)
        {
            ProcessControlSelection(activeControl, null, true, Control.ModifierKeys, scrollToControl);
        }

        /// <summary>
        /// Processes the control selection.
        /// </summary>
        /// <param name="activeControl">The <see cref="PaginationControl"/> that should be
        /// considered active.</param>
        /// <param name="additionalControls">Any additional <see cref="PaginationControl"/>s whose
        /// selection state should be changed along with <see paramref="activeControl"/>.</param>
        /// <param name="select"><see langword="true"/> if selection should be set,
        /// <see langword="false"/> if it should be cleared.</param>
        /// <param name="modifierKeys">The <see cref="Keys"/> that should be used as the active
        /// modifier keys.</param>
        /// <param name="scrollToControl"><c>true</c> if the <see paramref="activeControl"/> should be
        /// scrolled into view after being selected; otherwise, <c>false</c>.</param></param>
        void ProcessControlSelection(PaginationControl activeControl,
            IEnumerable<PaginationControl> additionalControls, bool select, Keys modifierKeys, bool scrollToControl = true)
        {
            try
            {
                PaginationControl lastSelectedControl = _lastSelectedControl;

                // Clear any currently selected controls first unless the control key is down.
                if ((modifierKeys & Keys.Control) == 0)
                {
                    // In most cases, the image close that will occur as a result of the cleared
                    // selection here will be immediately followed by re-opening the same image as
                    // the new selection is applied. Avoid this unnecessary close/re-open.
                    _preventTransientDocumentClose = true;

                    ClearSelection((modifierKeys & Keys.Shift) == 0);
                }

                // If a document separator is selected, all related document pages should also be
                // selected.
                additionalControls = additionalControls ?? new PaginationControl[0];
                var includedDocumentPages = new[] { activeControl }
                    .Union(additionalControls)
                    .OfType<PaginationSeparator>()
                    .SelectMany(c => (c.Document ?? new OutputDocument("")).PageControls);
                var additionalControlSet = new HashSet<PaginationControl>(
                    additionalControls.Concat(includedDocumentPages));

                // If the shift key is down and activeControl is not the same as the lastSelectedControl,
                // select all controls between activeControl and lastSelectedControl.
                if ((modifierKeys & Keys.Shift) == Keys.Shift &&
                    lastSelectedControl != null && activeControl != null &&
                    activeControl != lastSelectedControl)
                {
                    // Loop through all controls until we are to the end of the selection range.
                    bool inSelectionRange = false;
                    foreach (var control in _flowLayoutPanel.Controls.OfType<PaginationControl>())
                    {
                        if (inSelectionRange)
                        {
                            // If currently in the selected range, add this control
                            additionalControlSet.Add(control);

                            // If we are at the end of the range, break out of the loop.
                            if (control == activeControl || control == lastSelectedControl)
                            {
                                break;
                            }
                        }
                        else if (control == activeControl || control == lastSelectedControl)
                        {
                            // If we were not in the range, but we have now reached activeControl or
                            // lastSelectedControl, we are now in the range.
                            inSelectionRange = true;

                            additionalControlSet.Add(control);
                        }
                    }
                }

                // Set the selection state for all controls except activeControl first.
                foreach (var control in additionalControlSet.Except(new[] { activeControl }))
                {
                    SetSelected(control, select, true);
                }

                // Ensure all document pages are selected along with a document separator
                if (select)
                {
                    foreach (var separator in _flowLayoutPanel.Controls.OfType<PaginationSeparator>())
                    {
                        if (separator.Document != null)
                        {
                            if (separator.Selected)
                            {
                                foreach (var control in separator.Document.PageControls.
                                    Where(c => !c.Selected))
                                {
                                    SetSelected(control, select: true, resetLastSelected: true);
                                }
                            }
                        }
                    }
                }

                // Then select activeControl, making it the new active control if necessary.
                if (activeControl != null)
                {
                    // If a separator is being selected, make the active control be the previously
                    // selected page (if from the same document), or the first page of the
                    // separator's document.
                    if (select)
                    {
                        var lastSelectedPage = lastSelectedControl as PageThumbnailControl;
                        var activeSeparator = activeControl as PaginationSeparator;
                        if (activeSeparator != null)
                        {
                            if (activeSeparator.Document == null
                                || activeSeparator.Document == lastSelectedPage?.Document)
                            {
                                activeControl = lastSelectedPage;
                            }
                            else if (additionalControlSet
                                .OfType<PageThumbnailControl>()
                                .All(pageControl => pageControl.Document == activeSeparator.Document))
                            {
                                activeControl = activeSeparator.Document.PageControls.First();
                            }
                        }
                    }

                    // Allow _lastSelectedControl to become activeControl unless the shift modifier key
                    // is down.
                    bool resetLastSelected = ((modifierKeys & Keys.Shift) == 0);
                    if (activeControl == null)
                    {
                        ClearSelection();
                    }
                    else
                    {
                        // Only ever scroll to the control if a new control as been selected.
                        scrollToControl &= (lastSelectedControl != activeControl);
                        SelectControl(activeControl, select, resetLastSelected, scrollToControl);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41343");
            }
            finally
            {
                _preventTransientDocumentClose = false;

                // If after completing the selection change there isn't a page thumbnail selected,
                // we can now close the image knowing it is not about to be immediately re-opened.
                if (!(PrimarySelection is PageThumbnailControl) && ImageViewer.IsImageAvailable)
                {
                    ImageViewer.CloseImage();
                }
            }
        }

        /// <summary>
        /// Selects the specified <see paramref="control" />.
        /// </summary>
        /// <param name="control">The <see cref="PaginationControl" /> to select.</param>
        /// <param name="select"><c>true</c> to select or <c>false</c> to de-select.</param>
        /// <param name="resetLastSelected"><see langword="true" /> if <see paramref="control" /> can
        /// become the new <see cref="_lastSelectedControl" />; <see langword="false" /> otherwise.
        /// <param name="scrollToControl"><c>true</c> to scroll the control into view if selected;
        /// otherwise, <c>false</c>.</param>
        void SelectControl(PaginationControl control, bool select, bool resetLastSelected, bool scrollToControl)
        {
            SetSelected(control, select, resetLastSelected);

            if (select)
            {
                if (resetLastSelected || _lastSelectedControl == null)
                {
                    _lastSelectedControl = control;
                }
                _commandTargetControl = control;

                PrimarySelection = control;

                // Make sure the selected control is scrolled into view.
                if (scrollToControl &&
                    control.Visible &&
                    Rectangle.Intersect(ClientRectangle, control.Bounds) != control.Bounds)
                {
                    _flowLayoutPanel.ScrollControlIntoViewManual(control, flushWithTop: false);
                }
            }
            else
            {
                if (control == _lastSelectedControl)
                {
                    _lastSelectedControl = null;
                }

                if (_commandTargetControl != null)
                {
                    _commandTargetControl = null;
                }

                if (control == PrimarySelection)
                {
                    PrimarySelection = null;
                }
            }

            UpdateCommandStates();
        }

        /// <summary>
        /// Changes the specified <see paramref="control"/>'s selection state.
        /// </summary>
        /// <param name="control">The control for which the selection state should be set.</param>
        /// <param name="select"><see langword="true"/> to select the control;
        /// <see langword="false"/> to deselect it.</param>
        /// <param name="resetLastSelected"><see langword="true"/> if
        /// <see cref="_lastSelectedControl"/> should be reset if it is deselected;
        /// otherwise, <see langword="false"/>.</param>
        void SetSelected(PaginationControl control, bool select, bool resetLastSelected)
        {
            control.Selected = select;

            if (!select)
            {
                if (resetLastSelected && control == _lastSelectedControl)
                {
                    _lastSelectedControl = null;
                }

                if (control == _commandTargetControl)
                {
                    _commandTargetControl = null;
                }

                if (control == PrimarySelection)
                {
                    PrimarySelection = null;
                }
            }

            OnSelectionChanged();
        }

        /// <summary>
        /// Set the highlight state if the <see paramref="control"/> if it is a
        /// <see cref="NavigablePaginationControl"/>.
        /// Sets the displayed state of the image page if it is a <see cref="PageThumbnailControl"/>.
        /// </summary>
        /// <param name="control">The <see cref="PaginationControl"/>.</param>
        /// <param name="highlight"><see langword="true"/> to highlight and display the
        /// page; <see langword="false"/> to un-highlight and close the page.</param>
        void SetHighlightedAndDisplayed(PaginationControl control, bool highlight)
        {
            NavigablePaginationControl navigableControl =
                control as NavigablePaginationControl;
            if (navigableControl != null)
            {
                navigableControl.Highlighted = highlight;

                var pageControl = control as PageThumbnailControl;
                if (pageControl != null && (highlight || !_preventTransientDocumentClose))
                {
                    DisplayPage(pageControl, highlight);
                }
            }
        }

        /// <summary>
        /// Displays or closes the image associated with the specified <see paramref="pageControl"/>
        /// in the<see cref="ImageViewer"/>.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> whose page should be displayed.</param>
        /// <param name="display"><see langword="true"/> to display the image;
        /// <see langword="false"/> to close it.</param>
        void DisplayPage(PageThumbnailControl pageControl, bool display)
        {
            bool changedPage = (display != pageControl.PageIsDisplayed || pageControl != _displayedPage);
            pageControl.DisplayPage(ImageViewer, display);

            if (changedPage)
            {
                pageControl.Document?.PaginationSeparator?.Invalidate();

                if (pageControl != _displayedPage)
                {
                    _displayedPage?.Document?.PaginationSeparator?.Invalidate();
                    _displayedPage = pageControl;
                }
            }
        }

        /// <summary>
        /// Gets the next <see cref="NavigablePaginationControl"/> before or after
        /// <see paramref="currentControl"/>.
        /// </summary>
        /// <param name="forward"><see langword="true"/> to get the next
        /// <see cref="NavigablePaginationControl"/> or load next document; <see langword="false"/>
        /// to get the previous.</param>
        /// <param name="currentControl">The <see cref="PaginationControl"/> relative to which to
        /// search.</param>
        /// <returns>The next <see cref="NavigablePaginationControl"/> or <see langword="null"/>
        /// if there is no such control.</returns>
        NavigablePaginationControl GetNextNavigableControl(bool forward,
            PaginationControl currentControl = null)
        {
            // If not specified, use the active control as currentControl.
            if (currentControl == null)
            {
                currentControl = GetActiveControl(forward);
            }

            // If there was no active control, start from the front/back of the control sequence
            if (currentControl == null)
            {
                return forward
                    ? _flowLayoutPanel.Controls.OfType<NavigablePaginationControl>().FirstOrDefault()
                    : _flowLayoutPanel.Controls.OfType<NavigablePaginationControl>().LastOrDefault();
            }

            NavigablePaginationControl result = null;

            // Iterate from currentControl until the next page control is encountered.
            currentControl = forward
                ? currentControl.NextControl
                : currentControl.PreviousControl;

            result = currentControl as NavigablePaginationControl;
            while ((currentControl != null) && result == null)
            {
                currentControl = forward
                    ? currentControl.NextControl
                    : currentControl.PreviousControl;
                if (currentControl != null && currentControl.Visible)
                {
                    result = currentControl as NavigablePaginationControl;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the <see cref="NavigablePaginationControl"/> down or up from the active control.
        /// </summary>
        /// <param name="down"><see langword="true"/> to find the next control down;
        /// <see langword="false"/> to find the next control up.</param>
        /// <returns>The <see cref="NavigablePaginationControl"/> down or up from the active
        /// control.
        /// </returns>
        NavigablePaginationControl GetNextRowNavigableControl(bool down)
        {
            PaginationControl currentControl = GetActiveControl(down);
            if (currentControl == null)
            {
                return GetNextNavigableControl(down);
            }

            // Iterate from currentControl until the next page control is encountered that is
            // vertically aligned with the current control.
            NavigablePaginationControl result = null;
            for (NavigablePaginationControl nextControl = GetNextNavigableControl(down, currentControl);
                 nextControl != null;
                 nextControl = GetNextNavigableControl(down, nextControl))
            {
                result = nextControl;

                // For dragging purposes, page controls will be sized to take up the remainder of a
                // row.
                if (down && result.Top > currentControl.Top && result.Left >= currentControl.Left)
                {
                    break;
                }
                else if (!down && result.Top < currentControl.Top && result.Left <= currentControl.Left)
                {
                    break;
                }
            }

            if (result == null)
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Selects the document containing <see paramref="selectionTarget"/> unless all pages of
        /// that document are already selected, in which case the next document before/after that
        /// document is selected.
        /// </summary>
        /// <param name="forward"><see langword="true"/> to select the next document if the target
        /// document is already entirely selected; <see langword="false"/> to select the previous.</param>
        /// <param name="selectionTarget">The <see cref="PageThumbnailControl"/> the document
        /// selection is to be based off of.</param>
        void SelectNextDocument(bool forward, PageThumbnailControl selectionTarget = null)
        {
            // If there are no page controls, there is nothing to do.
            if (!_flowLayoutPanel.Controls.OfType<PageThumbnailControl>().Any())
            {
                return;
            }

            // If not specified, use the active control as the selectionTarget.
            if (selectionTarget == null)
            {
                selectionTarget = GetActiveControl(forward) as PageThumbnailControl;
            }

            // If there is no active control or the active control is not a PageThumbnailControl
            // control, use the next navigable control.
            if (selectionTarget == null)
            {
                var nextControl = GetNextNavigableControl(forward);

                // Ignore the load next document button.
                if (nextControl == _loadNextDocumentButtonControl)
                {
                    nextControl = GetNextNavigableControl(forward, nextControl);
                }

                selectionTarget = nextControl as PageThumbnailControl;
            }

            // If there are no page controls, there are no documents to select.
            if (selectionTarget == null)
            {
                return;
            }

            // If this document is fully selected, iterate the page controls forward/backward until
            // we get to the next document.
            IEnumerable<PageThumbnailControl> pageControls = selectionTarget.Document.PageControls;
            if (pageControls.All(page => page.Selected))
            {
                PaginationControl control = forward
                    ? pageControls.Last()
                    : pageControls.First();

                do
                {
                    control = forward
                        ? control.NextControl
                        : control.PreviousControl;

                    var pageControl = control as PageThumbnailControl;
                    if (pageControl != null)
                    {
                        selectionTarget = pageControl;
                        break;
                    }
                }
                while (control != null);

                pageControls = selectionTarget.Document.PageControls;
            }

            // Select the selectionTarget and the pageControls that make up the document it is in.
            // Do not allow handling of modifier keys since modifier keys have a different meaning
            // for document navigation.
            ProcessControlSelection(
                activeControl: selectionTarget,
                additionalControls: pageControls,
                select: true,
                modifierKeys: Keys.None);
        }

        /// <summary>
        /// Updates the availability of the context menu and shortcut key commands based on the
        /// current selection and control state.
        /// </summary>
        internal void UpdateCommandStates()
        {
            if (UIUpdatesSuspended)
            {
                return;
            }

            if (!_updateCommandStatesInvoked)
            {
                // Allow whatever else is occurring in the context of this event before proceeding
                // with the update so that we can be sure we are update command states against the
                // final control configuration.
                _updateCommandStatesInvoked = true;
                this.SafeBeginInvoke("ELI40225", () => UpdateCommandStates());

                return;
            }

            _updateCommandStatesInvoked = false;

            int contextMenuControlIndex = (_commandTargetControl == null)
                ? _flowLayoutPanel.Controls.OfType<PaginationControl>().Count()
                : _flowLayoutPanel.Controls.IndexOf(_commandTargetControl);

            // Commands that operate on the active selection require there to be a selection and for
            // the _commandTargetControl to be one of the selected items.
            bool enableSelectionBasedCommands =
                _commandTargetControl != null && _commandTargetControl.Selected &&
                    SelectedControls.Where(control => control != _loadNextDocumentButtonControl).Any();

            if (enableSelectionBasedCommands 
                && SelectedControls.OfType<PageThumbnailControl>()
                    .Any(page => page.Page.SourceDocument == null))
            {
                enableSelectionBasedCommands = false;
            }

            // Do not allow modification of documents that have already been output.
            bool enablePageModificationCommands =
                !SelectedControls.OfType<PageThumbnailControl>().Any(c => c.Document.OutputProcessed);

            // The cut command is applicable to separators if the only thing selected is a
            // separator. Depending upon SetSelected to prevent mixed selection.
            bool enabledCutCommand = enableSelectionBasedCommands && enablePageModificationCommands &&
                (!SelectedControls
                    .OfType<PageThumbnailControl>()
                    .Any(page => page.Deleted) ||
                SelectedControls.Any(c => c.GetType() == typeof(PaginationSeparator)));

            // The delete command is applicable to separators if the only thing selected is a
            // separator. Depending upon SetSelected to prevent mixed selection.
            bool enableDeleteCommand = enablePageModificationCommands &&
                _commandTargetControl != null && _commandTargetControl.Selected &&
                (SelectedControls
                    .OfType<PageThumbnailControl>()
                    .Any(c => !c.Deleted) ||
                SelectedControls.Any(c => c.GetType() == typeof(PaginationSeparator)));
            _deleteCommand.Enabled = enableDeleteCommand;

            // The un-delete command will be enabled only in the case that there are deleted
            // pages included in the selection.
            bool enableUnDeleteCommand = enablePageModificationCommands &&
                _commandTargetControl != null && _commandTargetControl.Selected &&
                SelectedControls
                    .OfType<PageThumbnailControl>()
                    .Any(c => c.Deleted);
            _unDeleteCommand.Enabled = enableUnDeleteCommand;

            bool enablePrintCommand = enableSelectionBasedCommands &&
                SelectedPageControls.ToArray().Length > 0;

            _cutCommand.Enabled = enabledCutCommand;
            _copyCommand.Enabled = enableSelectionBasedCommands;
            _printCommand.Enabled = enablePrintCommand;

            // Adjust the text of the insertion commands to be append commands if there is no
            // _commandTargetControl.
            if (_commandTargetControl == null)
            {
                _toggleDocumentSeparatorCommand.Enabled = false;
                _pasteCommand.Enabled = false;
                _toggleDocumentSeparatorMenuItem.Text = "Start new document on this page";
            }
            else
            {
                // Separators cannot be added next to other separators, cannot be the first control
                // and should only be able to be toggle when there is a single selection.
                bool controlIsSeparator = _commandTargetControl is PaginationSeparator;
                OutputDocument singlySelectedDocument =
                    (FullySelectedDocuments.Count() == 1 && PartiallySelectedDocuments.Count() == 1)
                    ? FullySelectedDocuments.Single()
                    : (SelectedPageControls.Count() == 1)
                        ? SelectedPageControls.Single().Document
                        : null;
                _toggleDocumentSeparatorCommand.Enabled =
                    (enablePageModificationCommands
                    && contextMenuControlIndex > 1
                    && singlySelectedDocument != null
                    && !singlySelectedDocument.Collapsed);
                var asPageThumbnailControl = _commandTargetControl as PageThumbnailControl;

                // If a separator is selected, or the first page of a document 
                // (whether that page is marked as deleted or not)
                if (_toggleDocumentSeparatorCommand.Enabled &&
                    (controlIsSeparator ||
                        (asPageThumbnailControl != null 
                        && asPageThumbnailControl == asPageThumbnailControl.Document?.PageControls.FirstOrDefault())))
                {
                    _toggleDocumentSeparatorMenuItem.Text = "Merge with previous document";
                    _toggleDocumentSeparatorMenuItem.ShortcutKeyDisplayString = "";

                    // Search for the previous page control to _commandTargetControl.
                    if (!(_commandTargetControl.PreviousControl is PageThumbnailControl previousPage))
                    {
                        previousPage = _commandTargetControl.PreviousControl?.PreviousControl
                            as PageThumbnailControl;
                    }

                    // If the previous page belongs to an alread output document, don't allow this document to be merged.
                    if (previousPage == null || previousPage.Document.OutputProcessed)
                    {
                        _toggleDocumentSeparatorCommand.Enabled = false;
                    }
                }
                else
                {
                    _toggleDocumentSeparatorMenuItem.Text = "Start new document on this page";
                    _toggleDocumentSeparatorMenuItem.ShortcutKeyDisplayString = "Space";
                }

                // Inserted copied items requires there to be copied items and a single selection.
                _pasteCommand.Enabled =
                    enablePageModificationCommands
                    && ClipboardHasData()
                    && SelectedControls.Count() == 1;
            }

            // Initiating document output via this control is only allowed if ExternalOutputOnly is
            // false and document(s) are fully selected.
            bool enableOutputCommand = !ExternalOutputOnly && FullySelectedDocuments.Count() > 0 &&
                PartiallySelectedDocuments.Count() == FullySelectedDocuments.Count();
            _outputDocumentCommand.Enabled = enableOutputCommand;

            if (_paginationUtility.OutputDocumentToolStripButton != null)
            {
                _paginationUtility.OutputDocumentToolStripButton.Enabled = enableOutputCommand;
            }
            if (_paginationUtility.OutputSelectedDocumentsMenuItem != null)
            {
                _paginationUtility.OutputSelectedDocumentsMenuItem.Enabled = enableOutputCommand;
            }

            OnStateChanged();
        }

        /// <summary>
        /// Initializes the page navigation and operations associated with keystrokes.
        /// </summary>
        void InitializeShortcuts()
        {
            _cutCommand.Enabled = false;
            _copyCommand.Enabled = false;
            _deleteCommand.Enabled = false;
            _unDeleteCommand.Enabled = false;
            _printCommand.Enabled = false;
            _pasteCommand.Enabled = false;
            _toggleDocumentSeparatorCommand.Enabled = false;
            _outputDocumentCommand.Enabled = false;

            Shortcuts[Keys.Escape] = ClearSelection;

            Shortcuts[Keys.Tab] = HandleSelectNextPage;
            Shortcuts[Keys.Tab | Keys.Control] = HandleSelectNextDocument;
            // [DotNetRCAndUtils:984]
            // Don't allow the shift key to expand selection when used in conjunction with tab.
            Shortcuts[Keys.Tab | Keys.Shift] = HandleSelectPreviousPageNoShift;
            Shortcuts[Keys.Tab | Keys.Control | Keys.Shift] = HandleSelectPreviousDocument;

            Shortcuts[Keys.Left] = HandleSelectPreviousPage;
            Shortcuts[Keys.Left | Keys.Control] = HandleSelectPreviousPage;
            Shortcuts[Keys.Left | Keys.Shift] = HandleSelectPreviousPage;
            Shortcuts[Keys.Left | Keys.Control | Keys.Shift] = HandleSelectPreviousPage;

            Shortcuts[Keys.Right] = HandleSelectNextPage;
            Shortcuts[Keys.Right | Keys.Control] = HandleSelectNextPage;
            Shortcuts[Keys.Right | Keys.Shift] = HandleSelectNextPage;
            Shortcuts[Keys.Right | Keys.Control | Keys.Shift] = HandleSelectNextPage;

            Shortcuts[Keys.PageUp] = HandleSelectPreviousPage;
            Shortcuts[Keys.PageUp | Keys.Control] = HandleSelectPreviousPage;
            Shortcuts[Keys.PageUp | Keys.Shift] = HandleSelectPreviousPage;
            Shortcuts[Keys.PageUp | Keys.Control | Keys.Shift] = HandleSelectPreviousPage;

            Shortcuts[Keys.PageDown] = HandleSelectNextPage;
            Shortcuts[Keys.PageDown | Keys.Control] = HandleSelectNextPage;
            Shortcuts[Keys.PageDown | Keys.Shift] = HandleSelectNextPage;
            Shortcuts[Keys.PageDown | Keys.Control | Keys.Shift] = HandleSelectNextPage;

            Shortcuts[Keys.Up] = HandleSelectPreviousRowPage;
            Shortcuts[Keys.Up | Keys.Control] = HandleSelectPreviousRowPage;
            Shortcuts[Keys.Up | Keys.Shift] = HandleSelectPreviousRowPage;
            Shortcuts[Keys.Up | Keys.Control | Keys.Shift] = HandleSelectPreviousRowPage;

            Shortcuts[Keys.Down] = HandleSelectNextRowPage;
            Shortcuts[Keys.Down | Keys.Control] = HandleSelectNextRowPage;
            Shortcuts[Keys.Down | Keys.Shift] = HandleSelectNextRowPage;
            Shortcuts[Keys.Down | Keys.Control | Keys.Shift] = HandleSelectNextRowPage;

            Shortcuts[Keys.Home] = HandleSelectFirstPage;
            Shortcuts[Keys.Home | Keys.Control] = HandleSelectFirstPage;
            Shortcuts[Keys.Home | Keys.Shift] = HandleSelectFirstPage;
            Shortcuts[Keys.Home | Keys.Control | Keys.Shift] = HandleSelectFirstPage;

            Shortcuts[Keys.End] = HandleSelectLastPage;
            Shortcuts[Keys.End | Keys.Control] = HandleSelectLastPage;
            Shortcuts[Keys.End | Keys.Shift] = HandleSelectLastPage;
            Shortcuts[Keys.End | Keys.Control | Keys.Shift] = HandleSelectLastPage;

            Shortcuts[Keys.Oemcomma] = HandleSelectPreviousPage;
            Shortcuts[Keys.OemPeriod] = HandleSelectNextPage;

            // Clear shortcuts that don't apply to this application.
            Shortcuts[Keys.O | Keys.Control] = null;
            Shortcuts[Keys.Control | Keys.F4] = null;
            Shortcuts[Keys.Control | Keys.P] = null;
        }

        /// <summary>
        /// Clears any selection and closes the currently displayed page in the
        /// <see cref="ImageViewer"/> if specified by <see paramref="closeDisplayedPage"/>.
        /// </summary>
        /// <param name="resetLastSelected"><see langword="true"/> if
        /// <see cref="_lastSelectedControl"/> should be set to <see langword="null"/>;
        /// otherwise <see langword="false"/>.</param>
        void ClearSelection(bool resetLastSelected)
        {
            // Deselect all currently selected controls
            foreach (PaginationControl selectedControl in SelectedControls.ToArray())
            {
                SetSelected(selectedControl, false, resetLastSelected);
            }

            // Setting the primary selection to null will also close the displayed image.
            PrimarySelection = null;

            UpdateCommandStates();
        }

        /// <summary>
        /// Copies the selected <see cref="PaginationControl"/>s to the clipboard.
        /// </summary>
        void CopySelectionToClipboard()
        {
            var copiedPages = new List<KeyValuePair<Page, bool>>();
            foreach (var pageControl in SelectedControls
                .OfType<PageThumbnailControl>())
            {
                copiedPages.Add(
                    new KeyValuePair<Page, bool>(pageControl.Page, pageControl.Deleted));
            }

            SetClipboardData(copiedPages);

            UpdateCommandStates();
        }

        /// <summary>
        /// Gets the <see cref="PageThumbnailControl"/> that exists at the specified
        /// <see paramref="location"/>.
        /// </summary>
        /// <param name="location">The location to check for a <see cref="PageThumbnailControl"/>.
        /// </param>
        /// <returns>The <see cref="PageThumbnailControl"/> that exists at the specified
        /// <see paramref="location"/> or <see langword="null"/> if no control exists at the
        /// specified location.</returns>
        PageThumbnailControl GetThumbnailControlAtPoint(Point location)
        {
            return _flowLayoutPanel
                .Controls
                .OfType<PageThumbnailControl>()
                .Where(c => c.Visible && c.Bounds.Contains(location))
                .FirstOrDefault();
        }

        /// <summary>
        /// Deletes the specified <see paramref="paginationControls"/>.
        /// </summary>
        /// <param name="paginationControls">The <see cref="PaginationControl"/>s to delete.</param>
        void DeleteControls(PaginationControl[] paginationControls)
        {
            using (new PageLayoutControlUpdateLock(this))
            {
                int newSelectionPosition = -1;
                foreach (var control in paginationControls
                    .Where(control => control != _loadNextDocumentButtonControl))
                {
                    // Use the index of the active control at the point it is deleted as the
                    // index to select following the deletion.
                    PageThumbnailControl pageControl = control as PageThumbnailControl;
                    if ((control.Selected && newSelectionPosition == -1) ||
                        (pageControl != null && pageControl.PageIsDisplayed))
                    {
                        newSelectionPosition = _flowLayoutPanel.Controls.IndexOf(control);
                    }

                    RemovePaginationControl(control, true);
                }

                if (newSelectionPosition != -1 &&
                    newSelectionPosition < _flowLayoutPanel.Controls.OfType<PaginationControl>().Count())
                {
                    PaginationControl controlToSelect =
                        _flowLayoutPanel.Controls[newSelectionPosition] as PaginationControl;
                    if (!(controlToSelect is PageThumbnailControl))
                    {
                        controlToSelect = GetNextNavigableControl(true, controlToSelect);
                    }

                    if (controlToSelect != null && !SelectedControls.Any())
                    {
                        ProcessControlSelection(controlToSelect);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if all <see cref="Page"/>s <see paramref="pagesToLoad"/> can be loaded
        /// without exceeding <see cref="_MAX_LOADED_PAGES"/> and if not takes
        /// appropriate action (including adjusting the pages in <see paramref="pagesToLoad"/>).
        /// </summary>
        /// <param name="pageCountToLoad">The number of <see cref="Page"/>s to about to be loaded.
        /// </param>
        void AssertAllPagesBeLoaded(int pageCountToLoad)
        {
            int remainingPages = _MAX_LOADED_PAGES - PageCount;
            if (remainingPages < pageCountToLoad)
            {
                throw new ExtractException("ELI45516",
                    "No more than " +
                    _MAX_LOADED_PAGES.ToString(CultureInfo.CurrentCulture) +
                    " pages may be loaded at once.\r\n\r\n");
            }
        }

        /// <summary>
        /// Inserts the <see cref="Page"/>s from <see paramref="copiedPages"/> at the specified
        /// <see pararef="index"/>.
        /// </summary>
        /// <param name="copiedPages">The <see cref="Page"/>s to be inserted into this control.
        /// </param>
        /// <param name="index">The index at which the pages should be inserted.</param>
        void InsertPages(IEnumerable<KeyValuePair<Page, bool>> copiedPages, int index)
        {
            bool duplicatePagesExist = false;
            List<PaginationControl> insertedPaginationControls = new List<PaginationControl>();

            foreach (var page in copiedPages)
            {
                // A null page represents a document boundary; insert a separator.
                if (page.Key == null)
                {
                    var separator = new PaginationSeparator(CommitOnlySelection);
                    insertedPaginationControls.Add(separator);

                    if (InitializePaginationControl(separator, ref index))
                    {
                        index++;
                    }
                }
                // Insert a new page control that uses the specified page.
                else
                {
                    var newPageControl = new PageThumbnailControl(null, page.Key);
                    newPageControl.Deleted = page.Value;
                    insertedPaginationControls.Add(newPageControl);

                    if (InitializePaginationControl(newPageControl, ref index))
                    {
                        duplicatePagesExist |= newPageControl.Page.MultipleCopiesExist;
                        index++;
                    }
                }
            }

            // If multiple copies of a pasted page exist, refresh to add the copy indicator
            // on all other copies.
            if (duplicatePagesExist)
            {
                Refresh();
            }

            // Select the newly inserted pages.
            ProcessControlSelection(
                activeControl: insertedPaginationControls.First(),
                additionalControls: insertedPaginationControls,
                select: true,
                modifierKeys: Keys.None);
        }

        /// <summary>
        /// Starts/stops scrolling during a drag event based upon the location of the mouse.
        /// </summary>
        /// <param name="mouseLocation">The current mouse location in screen coordinates.</param>
        void EnableDragScrolling(Point mouseLocation)
        {
            // Determine if scrolling should occur based upon the mouse location being close to
            // the top/bottom of the screen
            Point screenLocation = PointToScreen(Location);
            int topScrollZone = screenLocation.Y + _DRAG_DROP_SCROLL_AREA;
            int bottomScrollZone =
                screenLocation.Y + DisplayRectangle.Height - _DRAG_DROP_SCROLL_AREA;

            // If the control should scroll up
            if (mouseLocation.Y <= topScrollZone)
            {
                _scrollSpeed = -Math.Min(topScrollZone - mouseLocation.Y, _DRAG_DROP_SCROLL_AREA);

                if (!_dragDropScrollTimer.Enabled)
                {
                    _dragDropScrollPos = -1;
                    _dragDropScrollTimer.Start();
                }
            }
            // If the control should scroll down
            else if (mouseLocation.Y >= bottomScrollZone)
            {
                _scrollSpeed = Math.Min(mouseLocation.Y - bottomScrollZone, _DRAG_DROP_SCROLL_AREA);

                if (!_dragDropScrollTimer.Enabled)
                {
                    _dragDropScrollPos = -1;
                    _dragDropScrollTimer.Start();
                }
            }
            // If scrolling should be stopped
            else if (_dragDropScrollTimer.Enabled)
            {
                _dragDropScrollTimer.Stop();
            }
        }

        /// <summary>
        /// Places the specified <see paramref="pages"/> on the clipboard with retries. 
        /// </summary>
        /// <param name="pages">The <see cref="Page"/> instances and corresponding deleted
        /// states of those instances to be copied to the clipboard where any
        /// <see langword="null"/> page references indicate a document boundary.</param>
        internal void SetClipboardData(IEnumerable<KeyValuePair<Page, bool>> pages)
        {
            try
            {
                using (new TemporaryWaitCursor())
                {
                    Exception clipboardException = null;

                    DereferenceLastClipboardData();

                    // Register custom data format or get it if it's already registered
                    DataFormats.Format format = DataFormats.GetFormat(_CLIPBOARD_DATA_FORMAT);

                    _currentClipboardData = new ClipboardData(pages);

                    IDataObject dataObject = new DataObject();
                    dataObject.SetData(format.Name, false, _currentClipboardData);

                    // Retry loop in case copy or clipboard data validation fail.
                    for (int i = 0; i < _CLIPBOARD_RETRY_COUNT; i++)
                    {
                        try
                        {
                            // Technically un-necessary, but the clipboard can be flaky, so maybe
                            // this will help?
                            if (i > 0)
                            {
                                Clipboard.Clear();

                                System.Threading.Thread.Sleep(200);
                            }

                            Clipboard.SetDataObject(dataObject, false);

                            // Validate the data can be retrieved and that it matches the data that
                            // was placed on the clipboard.
                            var validataionData = GetClipboardData();
                            ExtractException.Assert("ELI35513", "Clipboard data failed validation.",
                                validataionData != null);

                            foreach (var page in pages.Where(page => page.Key != null))
                            {
                                page.Key.AddReference(this);
                            }

                            // Success; break out of loop.
                            clipboardException = null;
                            return;
                        }
                        catch (Exception ex)
                        {
                            clipboardException = ex;
                        }
                    }

                    // Throw the last exception that was caught in the retry loop.
                    if (clipboardException != null)
                    {
                        _currentClipboardData = null;
                        throw clipboardException;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI35514", "Failed to copy data to clipboard.", ex);
            }
        }

        /// <summary>
        /// Retrieves as an enumerable of <see cref="Page"/>s the data from the clipboard where any
        /// instances of <see langword="null"/> in the enumerable represent document boundaries.
        /// </summary>
        /// <returns>The pages represented by this clipboard data where each <see cref="Page"/> is
        /// paired with a <see langword="bool"/> indicating whether the page was copied in a deleted
        /// state.
        /// </returns>
        internal IEnumerable<KeyValuePair<Page, bool>> GetClipboardData()
        {
            try
            {
                IDataObject dataObject = Clipboard.GetDataObject();
                ClipboardData clipboardData =
                    dataObject.GetData(_CLIPBOARD_DATA_FORMAT) as ClipboardData;

                if (_currentClipboardData != null && !_currentClipboardData.Equals(clipboardData))
                {
                    DereferenceLastClipboardData();
                }

                // If we found ClipboardData on the clipboard, convert it to an array of Pages.
                if (clipboardData != null)
                {
                    var pages = clipboardData.GetPages(_paginationUtility);
                    if (pages.Any())
                    {
                        return pages;
                    }
                    else
                    {
                        // The pages on the clipboard may no longer be loaded.
                        _currentClipboardData = null;
                        return null;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35516");
            }
        }

        /// <summary>
        /// Indicates whether the clipboard has data of type _CLIPBOARD_DATA_FORMAT.
        /// </summary>
        /// <returns><see langword="true"/> if the there is data of type _CLIPBOARD_DATA_FORMAT on
        /// the clipboard, <see langword="false"/> otherwise.</returns>
        internal static bool ClipboardHasData()
        {
            try
            {
                IDataObject dataObject = Clipboard.GetDataObject();
                if (dataObject != null)
                {
                    return dataObject.GetDataPresent(_CLIPBOARD_DATA_FORMAT);
                }

                return false;
            }
            // If there was an exception reading data from the clipboard, treat it as if there
            // was not data on the clipboard.
            catch { }

            return false;
        }

        /// <summary>
        /// Gets an <see cref="OutputDocument"/> based upon the specified
        /// <see paramref="originalDocName"/>.
        /// </summary>
        /// <param name="originalDocName">The name of the document on the file system the
        /// <see cref="OutputDocument"/> is to represent.</param>
        /// <returns>An <see cref="OutputDocument"/> based upon the specified
        /// <see paramref="originalDocName"/>.</returns>
        OutputDocument GetOutputDocumentFromUtility(string originalDocName)
        {
            var outputDocument = _paginationUtility.CreateOutputDocument(originalDocName);
            outputDocument.DocumentOutput += HandleOutputDocument_DocumentOutput;

            return outputDocument;
        }

        /// <summary>
        /// Handles a UI command to select the next page.
        /// </summary>
        internal void HandleSelectNextPage()
        {
            try
            {
                NavigablePaginationControl navigableControl = GetNextNavigableControl(true);

                if (navigableControl == null)
                {
                    _flowLayoutPanel.VerticalScroll.Value = _flowLayoutPanel.VerticalScroll.Maximum;
                }
                else
                {
                    ProcessControlSelection(navigableControl);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35456");
            }
        }

        /// <summary>
        /// Handles a UI command to select the previous page.
        /// </summary>
        internal void HandleSelectPreviousPage()
        {
            try
            {
                NavigablePaginationControl navigableControl = GetNextNavigableControl(false);

                if (navigableControl == null)
                {
                    _flowLayoutPanel.VerticalScroll.Value = _flowLayoutPanel.VerticalScroll.Minimum;
                }
                else
                {
                    ProcessControlSelection(navigableControl);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35457");
            }
        }

        /// <summary>
        /// Handles a UI command to select the previous page while ignoring the shift key modifier.
        /// </summary>
        internal void HandleSelectPreviousPageNoShift()
        {
            try
            {
                NavigablePaginationControl navigableControl = GetNextNavigableControl(false);

                if (navigableControl != null)
                {
                    ProcessControlSelection(
                        activeControl: navigableControl,
                        additionalControls: null,
                        select: true,
                        modifierKeys: Control.ModifierKeys & ~Keys.Shift);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35832");
            }
        }

        /// <summary>
        /// Handles a UI command to select the previous page.
        /// </summary>
        void HandleSelectPreviousSinglePage()
        {
            try
            {
                NavigablePaginationControl navigableControl = GetNextNavigableControl(false);

                if (navigableControl != null)
                {
                    ProcessControlSelection(navigableControl);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35561");
            }
        }

        /// <summary>
        /// Handles a UI command to select the first page.
        /// </summary>
        void HandleSelectFirstPage()
        {
            try
            {
                NavigablePaginationControl navigableControl =
                    _flowLayoutPanel.Controls
                        .OfType<NavigablePaginationControl>()
                        .FirstOrDefault();

                if (navigableControl != null)
                {
                    ProcessControlSelection(navigableControl);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35458");
            }
        }

        /// <summary>
        /// Handles a UI command to select the last page.
        /// </summary>
        void HandleSelectLastPage()
        {
            try
            {
                NavigablePaginationControl navigableControl =
                    _flowLayoutPanel.Controls
                        .OfType<NavigablePaginationControl>()
                        .LastOrDefault();

                if (navigableControl != null)
                {
                    ProcessControlSelection(navigableControl);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35459");
            }
        }

        /// <summary>
        /// Handles a UI command to select page down from the currently selected control.
        /// </summary>
        internal void HandleSelectNextRowPage()
        {
            try
            {
                NavigablePaginationControl navigableControl = GetNextRowNavigableControl(true);

                if (navigableControl == null)
                {
                    _flowLayoutPanel.VerticalScroll.Value = _flowLayoutPanel.VerticalScroll.Maximum;
                }
                else
                {
                    ProcessControlSelection(navigableControl);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35460");
            }
        }

        /// <summary>
        /// Handles a UI command to select page up from the currently selected control.
        /// </summary>
        internal void HandleSelectPreviousRowPage()
        {
            try
            {
                NavigablePaginationControl navigableControl = GetNextRowNavigableControl(false);

                if (navigableControl == null)
                {
                    _flowLayoutPanel.VerticalScroll.Value = _flowLayoutPanel.VerticalScroll.Minimum;
                }
                else
                {
                    ProcessControlSelection(navigableControl);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35461");
            }
        }

        /// <summary>
        /// Handles a UI command to select the next document.
        /// </summary>
        void HandleSelectNextDocument()
        {
            try
            {
                SelectNextDocument(true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35462");
            }
        }

        /// <summary>
        /// Handles a UI command to select the previous document.
        /// </summary>
        void HandleSelectPreviousDocument()
        {
            try
            {
                SelectNextDocument(false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35463");
            }
        }

        /// <summary>
        /// Handles a UI command to cut the selected controls.
        /// </summary>
        internal void HandleCutSelectedControls()
        {
            try
            {
                CopySelectionToClipboard();

                // Mixed selection of pages and separators is not allowed.
                if (SelectedControls.All(c => c is PaginationSeparator))
                {
                    DeleteControls(SelectedControls.ToArray());
                }
                else
                {
                    DeletePagesInCurrentSelection();
                }

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35465");
            }
        }

        /// <summary>
        /// Handles a UI command to copy the selected controls.
        /// </summary>
        internal void HandleCopySelectedControls()
        {
            try
            {
                CopySelectionToClipboard();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35466");
            }
        }

        /// <summary>
        /// Handles a UI command to paste the pages on the clipboard.
        /// </summary>
        internal void HandlePaste()
        {
            try
            {
                using (new PageLayoutControlUpdateLock(this))
                {
                    IEnumerable<KeyValuePair<Page, bool>> copiedPages = GetClipboardData();

                    if (copiedPages != null && _commandTargetControl != null)
                    {
                        int pasteMaxCount = _MAX_LOADED_PAGES - PageCount;
                        if (copiedPages.Count(page => page.Key != null) > pasteMaxCount)
                        {
                            MessageBox.Show("No more than " +
                                _MAX_LOADED_PAGES.ToString(CultureInfo.CurrentCulture) +
                                " pages may be loaded at once.\r\n\r\n" +
                                "The pages on the clipboard cannot be inserted until existing\r\n" +
                                "pages are output or deleted.", "Page limit reached",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);

                            return;
                        }

                        int index = _flowLayoutPanel.Controls.IndexOf(_commandTargetControl);

                        InsertPages(copiedPages, index);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35467");
            }
        }

        /// <summary>
        /// Handles a UI command to delete the selected items.
        /// </summary>
        internal void HandleDeleteSelectedItems()
        {
            try
            {
                // Mixed selection of pages and separators is not allowed.
                if (SelectedControls.All(c => c is PaginationSeparator))
                {
                    DeleteControls(SelectedControls.ToArray());
                }
                else
                {
                    DeletePagesInCurrentSelection();
                }

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35469");
            }
        }

        /// <summary>
        /// Deletes the pages in current selection.
        /// </summary>
        void DeletePagesInCurrentSelection()
        {
            var pageControlsToDelete = new HashSet<PageThumbnailControl>(
                SelectedControls.OfType<PageThumbnailControl>());
            bool clearSelection = true;

            foreach (var pageControl in pageControlsToDelete.ToArray())
            {
                // For any deleted page that is not referenced in another page control outside
                // of the current selection, rather than be removed from the page, it should be
                // marked as deleted.
                if (!pageControl.Page.PageControlReferences.Any(
                    c => c != pageControl && !pageControlsToDelete.Contains(c)))
                {
                    pageControlsToDelete.Remove(pageControl);
                    pageControl.Deleted = true;
                    clearSelection = false;
                }
            }

            if (clearSelection)
            {
                ClearSelection();
            }

            DeleteControls(pageControlsToDelete.ToArray());
        }

        /// <summary>
        /// Handles a UI command to un-delete the selected items.
        /// </summary>
        internal void HandleUnDeleteSelectedItems()
        {
            try
            {
                foreach (var pageControl in SelectedControls
                    .OfType<PageThumbnailControl>()
                    .ToArray())
                {
                    pageControl.Deleted = false;
                }

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40045");
            }
        }

        /// <summary>
        /// Handles a UI command to print the selected items. 
        /// https://extract.atlassian.net/browse/ISSUE-13114, Print from Pagination
        /// </summary>
        internal void HandlePrintSelectedItems()
        {
            try
            {
                using (PrintDialog printDialog = new PrintDialog())
                {

                    _printDocument = new PrintDocument();
                    _printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);       // reset margins to none - default is 1"
                    _printDocument.DefaultPageSettings.Color = true;

                    printDialog.Document = _printDocument;
                    printDialog.UseEXDialog = true;
                    printDialog.AllowSomePages = false;     // disable - user to specify a range of pages
                    printDialog.AllowSelection = true;      // enable current selection only
                    printDialog.PrinterSettings.PrintRange = PrintRange.Selection;  // set the Selection radio button
                    printDialog.AllowCurrentPage = false;

                    DialogResult result = printDialog.ShowDialog();
                    if (result != DialogResult.OK)
                    {
                        return;
                    }
                }

                // Now add the event handlers, so that a cancel won't cause a memory leak... ;-)
                _printDocument.PrintPage += HandlePrintPage;
                _printDocument.EndPrint += HandlePrintHasEnded;

                // Setup for printing
                var imagePages = SelectedControls
                    .OfType<PageThumbnailControl>()
                    .Select(pageControl =>
                                new ImagePage(pageControl.Page.OriginalDocumentName,
                                              pageControl.Page.OriginalPageNumber,
                                              pageControl.Page.ImageOrientation));

                _rastersForPrinting = ImagePagesToRasterImages(imagePages);
                _printPageCounter = 0;

                _printDocument.Print();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38440");
            }
        }

        /// <summary>
        /// Utility function to convert image pages into raster images, which can be converted 
        /// into a image, which can then be printed. 
        /// </summary>
        /// <param name="imagePages">The collection of image pages to convert.</param>
        /// <returns>Array of RasterImage objects</returns>
        static List<RasterImage> ImagePagesToRasterImages(IEnumerable<ImagePage> imagePages)
        {
            List<RasterImage> rasters = new List<RasterImage>(imagePages.Count());
            foreach (var imagePage in imagePages)
            {
                var raster = imagePage.ToRasterImage();
                rasters.Add(raster);
            }

            return rasters;
        }

        /// <summary>
        /// This function handles the actual printing of a page - it writes the graphics device context. 
        /// Note that the original code was copied from ImagePrinter.cs, HandlePrintPage(), then 
        /// substantially modified. See NOTE: below. What that note doesn't mention is that the original 
        /// assumes it is printing from a file, and it also assumes that it is printing annotations. 
        /// </summary>
        /// <param name="pageImage">The RasterImage object to print.</param>
        /// <param name="e">The PrintPageEventArgs associated with the print task.</param>
        static void PrintOnePage(RasterImage pageImage, PrintPageEventArgs e)
        {
            // [IDSD #318], [DotNetRCAndUtils:1078]
            // The margin bounds should be the margin rectangle intersected with the
            // printable area offset by the inverse of the top-left of the printable area.
            // NOTE: I have modified this after some testing - basically the best fit comes
            // from setting the margin bounds as follows, and then directly mapping the 
            // source to the margin bounds. I found that the previous algorithm compressed the
            // image vertically a little.
            var printableArea = Rectangle.Round(e.PageSettings.PrintableArea);
            Rectangle marginBounds = e.MarginBounds;
            marginBounds.Height = printableArea.Height;
            marginBounds.Width = printableArea.Width;
            Rectangle sourceBounds = new Rectangle(0, 0, pageImage.Width, pageImage.Height);

            using (var image = RasterImageConverter.ConvertToImage(pageImage, ConvertToImageOptions.None))
            {
                e.Graphics.DrawImage(image, marginBounds, sourceBounds, GraphicsUnit.Pixel);
            }
        }

        /// <summary>
        /// Combines all specified non-<see langword="null"/> <see cref="ToolStripItem"/>s into an
        /// array.
        /// </summary>
        /// <param name="items">The <see cref="ToolStripItem"/>s to join into the array.</param>
        /// <returns>An array of non-<see langword="null"/> <see cref="ToolStripItem"/>s.</returns>
        static ToolStripItem[] JoinToolStripItems(params ToolStripItem[] items)
        {
            return items.Where(item => item != null).ToArray();
        }

        /// <summary>
        /// The Handle Print Page event handler. This handler is responsible for retrieving and tracking 
        /// the correct image, printing the current page, and tracking whether there are more pages 
        /// to process. 
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">PrintPageEventArgs, passed to page print function</param>
        void HandlePrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                var image = _rastersForPrinting[_printPageCounter];
                ++_printPageCounter;

                PrintOnePage(image, e);

                e.HasMorePages = _printPageCounter < _rastersForPrinting.Count;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38443");
            }
        }

        /// <summary>
        /// Print Ended event handler. This function resets internal state and removes event handlers 
        /// from the print document so that the print document memory can be GC'ed. 
        /// </summary>
        /// <param name="sender">Unused</param>
        /// <param name="e">Unused</param>
        void HandlePrintHasEnded(object sender, PrintEventArgs e)
        {
            try
            {
                CollectionMethods.ClearAndDispose(_rastersForPrinting);
                _rastersForPrinting = null;

                _printDocument.PrintPage -= HandlePrintPage;
                _printDocument.EndPrint -= HandlePrintHasEnded;
                _printDocument = null;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38444");
            }
        }

        /// <summary>
        /// Handles a UI command to add a document separator from before this control.
        /// </summary>
        internal void HandleAddDocumentSeparator()
        {
            HandleToggleDocumentSeparator(addOnly: true);
        }

        /// <summary>
        /// Handles a UI command to insert or remove a document separator from before this control.
        /// </summary>
        /// <param name="addOnly"><c>true</c> if a separator should only ever be added; <c>false</c>
        /// if the separator can be both added and removed.</param>
        internal void HandleToggleDocumentSeparator(bool addOnly)
        {
            try
            {
                using (new PageLayoutControlUpdateLock(this))
                {
                    if (_commandTargetControl != null)
                    {
                        PaginationSeparator targetSeparator = _commandTargetControl as PaginationSeparator;
                        if (targetSeparator == null)
                        {
                            targetSeparator = _commandTargetControl.PreviousControl as PaginationSeparator;
                        }

                        if (targetSeparator != null)
                        {
                            if (!addOnly)
                            {
                                RemovePaginationControl(targetSeparator, true);
                            }
                        }
                        else
                        {
                            int index = _flowLayoutPanel.Controls.IndexOf(_commandTargetControl);

                            InitializePaginationControl(
                                new PaginationSeparator(CommitOnlySelection), ref index);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35470");
            }
        }

        /// <summary>
        /// Handles a UI command to output any fully selected documents.
        /// </summary>
        void HandleOutputDocument()
        {
            try
            {
                OutputSelectedDocuments();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35472");
            }
        }

        /// <summary>
        /// Raises the <see cref="PageDeleted"/> event.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> that was deleted.</param>
        /// <param name="outputDocument">Gets the <see cref="OutputDocument"/> the
        /// <see paramref="page"/> was deleted from.</param>
        void OnPageDeleted(Page page, OutputDocument outputDocument)
        {
            var eventHandler = PageDeleted;
            if (eventHandler != null)
            {
                eventHandler(this, new PageDeletedEventArgs(page, outputDocument));
            }
        }

        /// <summary>
        /// Raises the <see cref="PagesDereferenced"/> event.
        /// </summary>
        /// <param name="pagesDereferenced">The <see cref="Page"/>s that have been dereferenced.
        /// </param>
        void OnPagesDereferenced(Page[] pagesDereferenced)
        {
            var eventHandler = PagesDereferenced;
            if (eventHandler != null)
            {
                eventHandler(this, new PagesDereferencedEventArgs(pagesDereferenced));
            }
        }

        /// <summary>
        /// Raises the <see cref="StateChanged"/> event.
        /// </summary>
        void OnStateChanged()
        {
            var eventHandler = StateChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="SelectionChanged"/> event.
        /// </summary>
        void OnSelectionChanged()
        {
            var eventHandler = SelectionChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="LoadNextDocumentRequest"/> event.
        /// </summary>
        void OnLoadNextDocumentRequest()
        {
            var eventHandler = LoadNextDocumentRequest;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="DocumentDataPanelRequest"/> event.
        /// </summary>
        /// <param name="e">The <see cref="DocumentDataPanelRequestEventArgs"/> instance containing
        /// the event data.</param>
        void OnDocumentDataPanelRequest(DocumentDataPanelRequestEventArgs e)
        {
            var eventHandler = DocumentDataPanelRequest;
            if (eventHandler != null)
            {
                eventHandler(this, e);
            }
        }

        #endregion Private Members
    }
}
