using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
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
        /// The number of consecutive <see cref="Control.MouseMove"/> events with the control key
        /// down needed to trigger the display of the page under the mouse. This prevents the
        /// inadvertent display of pages when trying to use keyboard navigation instead of the
        /// mouse.
        /// </summary>
        static readonly int _HOVER_MOVE_EVENT_CRITERIA = 10;

        /// <summary>
        /// The number of pixels from the top or bottom of the control where scrolling will be
        /// triggered during a drag/drop operation.
        /// </summary>
        static readonly int _DRAG_DROP_SCROLL_AREA = 50;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="PaginationUtilityForm"/> of which this instance is a member.
        /// </summary>
        PaginationUtilityForm _paginationUtility;

        /// <summary>
        /// Indicates where <see cref="PaginationControl"/>s will be dropped during a
        /// drag-and-drop operation.
        /// </summary>
        DropLocationIndicator _dropLocationIndicator = new DropLocationIndicator();

        /// <summary>
        /// Indicates the control index at which controls should be dropped during a
        /// drag-and-drop operation.
        /// </summary>
        int _dropLocationIndex = -1;

        /// <summary>
        /// The page control who's page is currently displayed (unless pre-empted by the
        /// control-hover feature).
        /// </summary>
        PageThumbnailControl _displayedPageControl;

        /// <summary>
        /// The page control who's page is currently displayed as a result of the control-hover
        /// feature.
        /// </summary>
        PageThumbnailControl _hoverPageControl;

        /// <summary>
        /// The number of consecutive hover events with the control key down toward
        /// <see cref="_HOVER_MOVE_EVENT_CRITERIA"/>.
        /// </summary>
        int _hoverMoveEventTotal;

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
        /// Context menu option that allows the copied PaginationControls to be inserted.
        /// NOTE: The option text will be set later.
        /// </summary>
        readonly ToolStripMenuItem _insertCopiedMenuItem = new ToolStripMenuItem();

        /// <summary>
        /// The <see cref="ApplicationCommand"/> that controls the availability of the insert
        /// copied items operation.
        /// </summary>
        ApplicationCommand _insertCopiedCommand;

        /// <summary>
        /// Context menu option that allows the a document separator prior to the selected page to
        /// be toggle on or off.
        /// NOTE: The option text will be set later.
        /// </summary>
        readonly ToolStripMenuItem _toggleDocumentSeparatorMenuItem = new ToolStripMenuItem();

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
        /// The last scroll position that was set programatically during drag/drop scrolling. There
        /// are situations outside of the code here that cause the scroll position to be adjusted
        /// after we have set it. By keeping track of what we last wanted it to be we can prevent
        /// the scroll position from jumping around in an unexpected fashion.
        /// </summary>
        int _dragDropScrollPos;

        #endregion Fields

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PageLayoutControl"/> class.
        /// </summary>
        /// <param name="paginationUtility">The <see cref="PaginationUtilityForm"/> of which this
        /// instance is a member.</param>
        // We do not need to worry about preventing sleep mode with the _dragDropScrollTimer as it
        // will be active only during a drag/drop operation.
        [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        public PageLayoutControl(PaginationUtilityForm paginationUtility)
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
                _dropLocationIndicator.DragOver += Handlel_DragOver;
                _dropLocationIndicator.DragLeave += Handle_DragLeave;

                // Set scrolling during a drag/drop scroll event to occur 20 times / sec.
                _dragDropScrollTimer = new Timer();
                _dragDropScrollTimer.Interval = 50;
                _dragDropScrollTimer.Tick += HandleDragDropScrollTimer_Tick;

                _paginationUtility = paginationUtility;
                _flowLayoutPanel.Click += HandleFlowLayoutPanel_Click;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35429");
            }
        }

        #endregion Contructors

        #region Events

        /// <summary>
        /// Raised whenever a <see cref="PageThumbnailControl"/> is removed deleted or cut from
        /// this instance.
        /// </summary>
        public event EventHandler<PageDeletedEventArgs> PageDeleted;

        /// <summary>
        /// Raised when the state of the <see cref="PaginationControl"/>s has changed.
        /// </summary>
        public event EventHandler<EventArgs> StateChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="ImageViewer"/> that is to display the images from selected
        /// <see cref="PageThumbnailControl"/>s.
        /// </summary>
        /// <value>
        /// The <see cref="ImageViewer"/> that is to display the images from selected
        /// <see cref="PageThumbnailControl"/>s.
        /// </value>
        public ImageViewer ImageViewer
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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a new <see cref="OutputDocument"/> based on the specified
        /// <paramref name="sourceDocument"/>, and adds <see cref="PageThumbnailControl"/>s for its
        /// pages.
        /// </summary>
        /// <param name="sourceDocument">The <see cref="SourceDocument"/> to be loaded as an
        /// <see cref="OutputDocument"/>.</param>
        public void CreateOutputDocument(SourceDocument sourceDocument)
        {
            try
            {
                SuspendLayout();

                OutputDocument outputDocument = _paginationUtility.CreateOutputDocument(
                    sourceDocument.FileName);

                Control lastControl = _flowLayoutPanel.Controls
                    .OfType<Control>()
                    .LastOrDefault();

                // If the last control is currently a page control, we need to add a separator.
                if (lastControl != null && lastControl is PageThumbnailControl)
                {
                    AddPaginationControl(new PaginationSeparator());
                }

                // Create a page control for every page in sourceDocument.
                foreach (Page page in sourceDocument.Pages.ToArray())
                {
                    AddPaginationControl(new PageThumbnailControl(outputDocument, page));

                    // [DotNetRCAndUtils:928]
                    // As pages are being added, allow the application to be responsive.
                    Application.DoEvents();

                    // [DotNetRCAndUtils:928]
                    // The parent form will dispose of this control when closing or restarting.
                    // If this control has been disposed of, stop loading any currently loading
                    // document.
                    if (IsDisposed)
                    {
                        return;
                    }
                }

                // Indicate if the document is output in its present form, it can simply be copied
                // to the output path rather than require it to be re-assembled.
                outputDocument.InOriginalForm = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35433");
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        /// <summary>
        /// Outputs the currently selected documents.
        /// </summary>
        public void OutputSelectedDocuments()
        {
            try
            {
                foreach (OutputDocument document in FullySelectedDocuments.ToArray())
                {
                    // If there was a problem outputting, abort the rest of the operation even if
                    // there were more documents to have been output.
                    if (!document.Output())
                    {
                        break;
                    }

                    // After each document is output, remove its page controls.
                    DeleteControls(document.PageControls.ToArray());
                }

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35434");
            }
        }

        /// <summary>
        /// Removes the specified <paramref name="control"/>.
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
                    control.Selected = false;

                    if (control == _lastSelectedControl)
                    {
                        _lastSelectedControl = null;
                    }
                }

                control.Click -= HandlePaginationControl_Click;
                control.MouseMove -= HandlePaginationControl_MouseMove;
                control.KeyUp -= HandlePaginationControl_KeyUp;

                // Determine which controls are before and after the removed control to determine
                // how the removal affects the output documents.
                var previousPageControl = control.PreviousControl as PageThumbnailControl;
                var nextPageControl = control.NextControl as PageThumbnailControl;
                _flowLayoutPanel.Controls.Remove(control);

                // If the removed control was a page control, it should no longer be diplayed or be
                // part of any OutputDocument.
                var removedPageControl = control as PageThumbnailControl;
                if (removedPageControl != null)
                {
                    if (removedPageControl.PageIsDisplayed && dispose)
                    {
                        removedPageControl.DisplayPage(ImageViewer, false);
                        _displayedPageControl = null;
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
                    control.LocationChanged -= HandleSeparatorControl_LocationChanged;

                    if (nextPageControl != null)
                    {
                        if (previousPageControl != null)
                        {
                            OutputDocument firstDocument = previousPageControl.Document;
                            MovePagesToDocument(firstDocument, nextPageControl);
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
        /// added.</param>
        /// <returns><see langword="true"/> if the control was successfully initialized;
        /// <see langword="false"/> if it was not (in which case it will have been disposed.
        /// </returns>
        public bool InitializePaginationControl(PaginationControl newControl, int index)
        {
            try
            {
                if (!InsertPaginationControl(newControl, index))
                {
                    newControl.Dispose();
                    return false;
                }

                // Determine which controls are before and after the removed control to determine
                // how the new control affects the output documents.
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

                        int newPageNumber = (previousPageControl == null)
                            ? 1
                            : previousPageControl.PageNumber + 1;
                        document.InsertPage(newPageControl, newPageNumber);
                    }
                    else
                    {
                        // There is no page control on either side of this document, a new document
                        // should be created with this as the one and only page.
                        document = _paginationUtility.CreateOutputDocument(
                            newPageControl.Page.OriginalDocumentName);
                        newPageControl.Document = document;

                        document.AddPage(newPageControl);
                    }
                }
                else if (nextPageControl != null && previousPageControl != null)
                {
                    // If this is a separator that is dividing two pages currently on the same
                    // document, generate a new document based on the second page and move all
                    // remaining page controls from the original document into the new document.
                    OutputDocument newDocument = _paginationUtility.CreateOutputDocument(
                        nextPageControl.Page.OriginalDocumentName);

                    MovePagesToDocument(newDocument, nextPageControl);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35437");
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

                _cutMenuItem.ShortcutKeys = Keys.Control | Keys.X;
                _cutMenuItem.ShowShortcutKeys = true;
                _cutCommand = new ApplicationCommand(ImageViewer.Shortcuts,
                    new Keys[] { Keys.Control | Keys.X }, HandleCutSelectedControls,
                    new[] { _cutMenuItem, _paginationUtility._cutMenuItem }, false, true, false);
                _cutMenuItem.Click += HandleCutMenuItem_Click;

                _copyMenuItem.ShortcutKeys = Keys.Control | Keys.C;
                _copyMenuItem.ShowShortcutKeys = true;
                _copyCommand = new ApplicationCommand(ImageViewer.Shortcuts,
                    new Keys[] { Keys.Control | Keys.C }, HandleCopySelectedControls,
                    new[] { _copyMenuItem, _paginationUtility._copyMenuItem }, false, true, false);
                _copyMenuItem.Click += HandleCopyMenuItem_Click;

                _insertCopiedMenuItem.ShortcutKeys = Keys.Control | Keys.V;
                _insertCopiedMenuItem.ShowShortcutKeys = true;
                _insertCopiedCommand = new ApplicationCommand(ImageViewer.Shortcuts,
                    new Keys[] { Keys.Control | Keys.V }, HandleInsertCopied,
                    new[] { _insertCopiedMenuItem, _paginationUtility._insertCopiedMenuItem },
                    false, true, false);
                _insertCopiedMenuItem.Click += HandleInsertCopiedMenuItem_Click;

                _deleteMenuItem.ShortcutKeys = Keys.Delete;
                _deleteMenuItem.ShowShortcutKeys = true;
                _deleteCommand = new ApplicationCommand(ImageViewer.Shortcuts,
                    new Keys[] { Keys.Delete }, HandleDeleteSelectedItems,
                    new[] { _deleteMenuItem, _paginationUtility._deleteMenuItem }, false, true, false);
                _deleteMenuItem.Click += HandleDeleteMenuItem_Click;

                _toggleDocumentSeparatorCommand = new ApplicationCommand(ImageViewer.Shortcuts,
                    new Keys[] { Keys.Space }, HandleToggleDocumentSeparator,
                    new[] { _toggleDocumentSeparatorMenuItem, _paginationUtility._insertDocumentSeparatorMenuItem }, 
                    false, true, true);
                _toggleDocumentSeparatorMenuItem.Click += HandleToggleDocumentSeparator_Click;

                _outputDocumentCommand = new ApplicationCommand(ImageViewer.Shortcuts,
                    new Keys[] { Keys.Control | Keys.S }, HandleOutputDocument,
                    null, false, true, false);

                ImageViewer.Shortcuts[Keys.Escape] = ClearSelection;

                ImageViewer.Shortcuts[Keys.Tab] = HandleSelectNextPage;
                ImageViewer.Shortcuts[Keys.Tab | Keys.Control] = HandleSelectNextDocument;
                ImageViewer.Shortcuts[Keys.Tab | Keys.Shift] = HandleSelectPreviousSinglePage;
                ImageViewer.Shortcuts[Keys.Tab | Keys.Control | Keys.Shift] = HandleSelectPreviousDocument;

                ImageViewer.Shortcuts[Keys.Left] = HandleSelectPreviousPage;
                ImageViewer.Shortcuts[Keys.Left | Keys.Control] = HandleSelectPreviousPage;
                ImageViewer.Shortcuts[Keys.Left | Keys.Shift] = HandleSelectPreviousPage;
                ImageViewer.Shortcuts[Keys.Left | Keys.Control | Keys.Shift] = HandleSelectPreviousPage;

                ImageViewer.Shortcuts[Keys.Right] = HandleSelectNextPage;
                ImageViewer.Shortcuts[Keys.Right | Keys.Control] = HandleSelectNextPage;
                ImageViewer.Shortcuts[Keys.Right | Keys.Shift] = HandleSelectNextPage;
                ImageViewer.Shortcuts[Keys.Right | Keys.Control | Keys.Shift] = HandleSelectNextPage;

                ImageViewer.Shortcuts[Keys.PageUp] = HandleSelectPreviousPage;
                ImageViewer.Shortcuts[Keys.PageUp | Keys.Control] = HandleSelectPreviousRowPage;
                ImageViewer.Shortcuts[Keys.PageUp | Keys.Shift] = HandleSelectPreviousRowPage;
                ImageViewer.Shortcuts[Keys.PageUp | Keys.Control | Keys.Shift] = HandleSelectPreviousRowPage;

                ImageViewer.Shortcuts[Keys.PageDown] = HandleSelectNextPage;
                ImageViewer.Shortcuts[Keys.PageDown | Keys.Control] = HandleSelectNextPage;
                ImageViewer.Shortcuts[Keys.PageDown | Keys.Shift] = HandleSelectNextPage;
                ImageViewer.Shortcuts[Keys.PageDown | Keys.Control | Keys.Shift] = HandleSelectNextPage;

                ImageViewer.Shortcuts[Keys.Up] = HandleSelectPreviousRowPage;
                ImageViewer.Shortcuts[Keys.Up | Keys.Control] = HandleSelectPreviousRowPage;
                ImageViewer.Shortcuts[Keys.Up | Keys.Shift] = HandleSelectPreviousRowPage;
                ImageViewer.Shortcuts[Keys.Up | Keys.Control | Keys.Shift] = HandleSelectPreviousRowPage;

                ImageViewer.Shortcuts[Keys.Down] = HandleSelectNextRowPage;
                ImageViewer.Shortcuts[Keys.Down | Keys.Control] = HandleSelectNextRowPage;
                ImageViewer.Shortcuts[Keys.Down | Keys.Shift] = HandleSelectNextRowPage;
                ImageViewer.Shortcuts[Keys.Down | Keys.Control | Keys.Shift] = HandleSelectNextRowPage;

                ImageViewer.Shortcuts[Keys.Home] = HandleSelectFirstPage;
                ImageViewer.Shortcuts[Keys.Home | Keys.Control] = HandleSelectFirstPage;
                ImageViewer.Shortcuts[Keys.Home | Keys.Shift] = HandleSelectFirstPage;
                ImageViewer.Shortcuts[Keys.Home | Keys.Control | Keys.Shift] = HandleSelectFirstPage;

                ImageViewer.Shortcuts[Keys.End] = HandleSelectLastPage;
                ImageViewer.Shortcuts[Keys.End | Keys.Control] = HandleSelectLastPage;
                ImageViewer.Shortcuts[Keys.End | Keys.Shift] = HandleSelectLastPage;
                ImageViewer.Shortcuts[Keys.End | Keys.Control | Keys.Shift] = HandleSelectLastPage;

                ContextMenuStrip = new ContextMenuStrip();
                ContextMenuStrip.Items.Add(_cutMenuItem);
                ContextMenuStrip.Items.Add(_copyMenuItem);
                ContextMenuStrip.Items.Add(_deleteMenuItem);
                ContextMenuStrip.Items.Add(new ToolStripSeparator());
                ContextMenuStrip.Items.Add(_insertCopiedMenuItem);
                ContextMenuStrip.Items.Add(_toggleDocumentSeparatorMenuItem);
                ContextMenuStrip.Opening += HandleContextMenuStrip_Opening;
                ContextMenuStrip.Closing += HandleContextMenuStrip_Closing;

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
        /// otherwise, <see langword="false"/>.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                // Allow the image viewer to handle keyboard input for shortcuts.
                if (ImageViewer.Shortcuts.ProcessKey(keyData))
                {
                    return true;
                }

                // This key was not processed, bubble it up to the base class.
                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35440", ex);
            }

            return true;
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
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_dragDropScrollTimer != null)
                {
                    _dragDropScrollTimer.Dispose();
                    _dragDropScrollTimer = null;
                }
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

                ProcessControlSelection(clickedControl, false, true);
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
                        pageControl, pageControl.Document.PageControls, true, true,
                        Control.ModifierKeys);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35445");
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
                // If the mouse button is down and the sending control is already selected, start a
                // drag-and-drop operation.
                if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                {
                    var originControl = sender as PaginationControl;
                    if (originControl != null && originControl.Selected)
                    {
                        // Don't start a drag and drop operation unless the user has dragged out of
                        // the origin control to help prevent accidental drag/drops.
                        Point mouseLocation = PointToClient(Control.MousePosition);
                        var currentControl =
                            _flowLayoutPanel.GetChildAtPoint(mouseLocation) as PaginationControl;

                        if (currentControl != null && currentControl != originControl)
                        {
                            var dataObject = new DataObject(_DRAG_DROP_DATA_FORMAT, this);
                            DoDragDrop(dataObject, DragDropEffects.Move);

                            // If drag/drop scrolling was active when the drag/drop event ends, stop
                            // the scrolling now.
                            if (_dragDropScrollTimer.Enabled)
                            {
                                _dragDropScrollTimer.Stop();
                            }
                        }
                    }
                }
                else if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    // Require enough successive mouse move events with the control key down to
                    // prevent the inadvertent display of pages when trying to use keyboard
                    // navigation instead of the mouse.
                    if (_hoverMoveEventTotal < _HOVER_MOVE_EVENT_CRITERIA)
                    {
                        _hoverMoveEventTotal++;
                    }
                    else
                    {
                        ShowHoverPage();
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
                else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // Files from the Windows shell.
                    e.Effect = DragDropEffects.Copy;
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
                SuspendLayout();

                // During a drag/drop from the Window's shell, when the mouse is over the
                // _dropLocationIndicator, it will cause a DragLeave event for the _flowLayoutPanel.
                // Don't allow the drop availability to end unless the mouse is now outside the
                // bounds of this control.
                Point mousePosition = PointToClient(Control.MousePosition);
                if (!Bounds.Contains(mousePosition))
                {
                    _dropLocationIndex = -1;
                    Controls.Remove(_dropLocationIndicator);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35448");
            }
            finally
            {
                // After adding/removing the _dropLocationIndicator, the scroll position will
                // get reset back to the top if a layout is done.
                ResumeLayout(false);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DragOver"/> event of a child control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance
        /// containing the event data.</param>
        void Handlel_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                // Determine if scrolling should occur based upon the mouse location being close to
                // the top/bottom of the screen
                Point screenLocation = PointToScreen(Location);
                int topScrollZone = screenLocation.Y + _DRAG_DROP_SCROLL_AREA;
                int bottomScrollZone =
                    screenLocation.Y + DisplayRectangle.Height - _DRAG_DROP_SCROLL_AREA;

                // If the control should scroll up
                if (e.Y <= topScrollZone)
                {
                    _scrollSpeed = -Math.Min(topScrollZone - e.Y, _DRAG_DROP_SCROLL_AREA);

                    if (!_dragDropScrollTimer.Enabled)
                    {
                        _dragDropScrollPos = -1;
                        _dragDropScrollTimer.Start();
                    }
                }
                // If the control should scroll down
                else if (e.Y >= bottomScrollZone)
                {
                    _scrollSpeed = Math.Min(e.Y - bottomScrollZone, _DRAG_DROP_SCROLL_AREA);

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
            catch (Exception ex)
            {
                ex.ExtractLog("ELI35449");
            }

            try
            {
                SuspendLayout();

                Point dragLocation = PointToClient(new Point(e.X, e.Y));
                var control = _flowLayoutPanel.GetChildAtPoint(dragLocation) as PaginationControl;
                if (control != null)
                {
                    _dropLocationIndex = _flowLayoutPanel.Controls.IndexOf(control);
                    Point location;

                    if ((dragLocation.X - control.Left) > (control.Width / 2))
                    {
                        _dropLocationIndex++;
                        location = control.TrailingInsertionPoint;
                    }
                    else
                    {
                        location = control.PreceedingInsertionPoint;
                    }

                    ShowDropLocationIndicator(location, control.Height);
                }
                else
                {
                    _dropLocationIndex = -1;
                    Controls.Remove(_dropLocationIndicator);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35450");
            }
            finally
            {
                // After adding/removing the _dropLocationIndicator, the scroll position will
                // get reset back to the top if a layout is done.
                ResumeLayout(false);
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
                SuspendLayout();

                if (_dropLocationIndex >= 0)
                {
                    var sourceLayoutControl = e.Data.GetData(_DRAG_DROP_DATA_FORMAT) as PageLayoutControl;
                    if (sourceLayoutControl != null)
                    {
                        MoveSelectedControls(sourceLayoutControl, _dropLocationIndex);
                    }
                    else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        IEnumerable<Page> pages = _paginationUtility.GetPagesFromFileDrop(e.Data);
                        InsertPages(pages, _dropLocationIndex);
                    }
                }

                _dropLocationIndex = -1;
                Controls.Remove(_dropLocationIndicator);

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35451");
            }
            finally
            {
                // After adding/removing the _dropLocationIndicator, the scroll position will
                // get reset back to the top if a layout is done.
                ResumeLayout(false);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.LocationChanged"/> event of a
        /// <see cref="PaginationSeparator"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSeparatorControl_LocationChanged(object sender, EventArgs e)
        {
            try
            {
                // Don't automatically remove leading separator controls in the middle of an
                // operation as the rest of ongoing operation could mean the deletion is obsolete
                // (and may, in fact, cause exceptions). Check the separator location on separate
                // event handlers after the primary operation is complete.
                this.SafeBeginInvoke("ELI35603", () =>
                {
                    var separatorControl = sender as PaginationSeparator;
                    if (separatorControl != null && _flowLayoutPanel.Controls.Contains(separatorControl))
                    {
                        if (separatorControl.PreviousControl == null)
                        {
                            PaginationControl nextControl = separatorControl.NextControl;
                            RemovePaginationControl(separatorControl, true);
                            if (nextControl != null)
                            {
                                nextControl.PerformLayout();
                            }
                        }
                        else if (separatorControl.PreviousControl is PaginationSeparator)
                        {
                            RemovePaginationControl(separatorControl, true);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35452");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.KeyUp"/> event of a <see cref="PaginationControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing
        /// the event data.</param>
        void HandlePaginationControl_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (_hoverPageControl != null && !e.Control)
                {
                    if (_displayedPageControl != _hoverPageControl)
                    {
                        if (_displayedPageControl == null)
                        {
                            _hoverPageControl.DisplayPage(ImageViewer, false);
                        }
                        else
                        {
                            _displayedPageControl.DisplayPage(ImageViewer, true);
                        }
                    }

                    _hoverMoveEventTotal = 0;
                    _hoverPageControl = null;
                }
                else
                {
                    // Any time a key is pressed, reset _hoverMoveEventTotal as the keypress indicates
                    // the user is using the keyboard rather than the mouse hover feature.
                    _hoverMoveEventTotal = 0;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35455");
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
                ProcessControlSelection(null, false, false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35453");
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
                _commandTargetControl =
                    _flowLayoutPanel.GetChildAtPoint(mouseLocation) as PaginationControl;
                if (_commandTargetControl != null && !_commandTargetControl.Selected)
                {
                    ProcessControlSelection(_commandTargetControl, true, true);
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
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_insertCopiedMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleInsertCopiedMenuItem_Click(object sender, EventArgs e)
        {
            HandleInsertCopied();
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
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_toggleDocumentSeparatorMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleToggleDocumentSeparator_Click(object sender, EventArgs e)
        {
            HandleToggleDocumentSeparator();
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
            if (_lastSelectedControl != null && _lastSelectedControl.Selected)
            {
                return _lastSelectedControl;
            }

            var pageControl = first
                ? SelectedControls.OfType<PageThumbnailControl>().FirstOrDefault()
                : SelectedControls.OfType<PageThumbnailControl>().LastOrDefault();

            return pageControl;
        }

        /// <summary>
        /// Adds the specified <see paramref="control"/> to this instance's
        /// <see cref="PaginationControl"/> as the last control
        /// </summary>
        /// <param name="control">The <see cref="PaginationControl"/> to add.</param>
        /// <returns></returns>
        bool AddPaginationControl(PaginationControl control)
        {
            return InsertPaginationControl(control, -1);
        }

        /// <summary>
        /// Adds the specified <see paramref="control"/> to this instance's
        /// <see cref="PaginationControl"/> collection as the specified <see paramref="index"/>.
        /// </summary>
        /// <param name="index">The index at which the control should be added or -1 to add it to
        /// the end.</param>
        /// <param name="control">The <see cref="PaginationControl"/> to add.</param>
        /// <returns></returns>
        bool InsertPaginationControl(PaginationControl control, int index = -1)
        {
            bool isPageControl = control is PageThumbnailControl;

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

            control.Click += HandlePaginationControl_Click;
            control.MouseMove += HandlePaginationControl_MouseMove;
            control.KeyUp += HandlePaginationControl_KeyUp;

            if (isPageControl)
            {
                control.DoubleClick += HandleThumbnailControl_DoubleClick;
            }
            else // is a separator
            {
                if (index == 0)
                {
                    // A separator control is meaningless as the first control, so don't bother
                    // adding one as the first control).
                    return false;
                }
                else
                {
                    control.Height = (control.PreviousControl != null)
                        ? control.PreviousControl.Height
                        : (control.NextControl != null) ? control.NextControl.Height : Height;
                    control.LocationChanged += HandleSeparatorControl_LocationChanged;
                }
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
            var selectedControls = sourceLayoutControl.SelectedControls.ToArray();

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
                else if (InitializePaginationControl(control, targetIndex++))
                {
                    control.Selected = true;
                }
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
                if (index == _flowLayoutPanel.Controls.Count)
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

            // To make the background of _dropLocationIndicator "transparent", update the the region
            // of this control under the _dropLocationIndicator.
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
        /// <param name="forceSelect"><see langword="true"/> if selection should be applied rather
        /// than toggled; otherwise <see langword="false"/>.</param>
        /// <param name="allowSetActivePage"><see langword="true"/> if the
        /// <see paramref="activeControl"/> should be able to be displayed and become the one and
        /// only "active" page; otheriwse, <see langword="false"/>.</param>
        void ProcessControlSelection(PaginationControl activeControl, bool forceSelect,
            bool allowSetActivePage)
        {
            ProcessControlSelection(activeControl, null, forceSelect, allowSetActivePage, 
                Control.ModifierKeys);
        }

        /// <summary>
        /// Processes the control selection.
        /// </summary>
        /// <param name="activeControl">The <see cref="PaginationControl"/> that should be
        /// considered active.</param>
        /// <param name="additionalControls">Any additional <see cref="PaginationControl"/>s whose
        /// selection state should be changed along with <see paramref="activeControl"/>.</param>
        /// <param name="forceSelect"><see langword="true"/> if selection should be applied rather
        /// than toggled; otherwise <see langword="false"/>.</param>
        /// <param name="allowSetActivePage"><see langword="true"/> if the
        /// <see paramref="activeControl"/> should be able to be displayed and become the one and
        /// only "active" page; otheriwse, <see langword="false"/>.</param>
        /// <param name="modifierKeys">The <see cref="Keys"/> that should be used as the active
        /// modifier keys.</param>
        void ProcessControlSelection(PaginationControl activeControl,
            IEnumerable<PaginationControl> additionalControls, bool forceSelect,
            bool allowSetActivePage, Keys? modifierKeys)
        {
            // Determine whether to select or deselect.
            bool select = forceSelect || activeControl == null || !activeControl.Selected
                || activeControl != _lastSelectedControl;
            PaginationControl lastSelectedControl = _lastSelectedControl;

            // Clear any currently selected controls first unless the control key is down.
            if (!modifierKeys.HasValue || (modifierKeys.Value & Keys.Control) == 0)
            {
                ClearSelection();
            }

            var additionalControlSet = (additionalControls == null)
                ? new HashSet<PaginationControl>()
                : new HashSet<PaginationControl>(additionalControls);

            // If the shift key is down and activeControl is not the same as the lastSelectedControl,
            // select all controls between activeControl and lastSelectedControl.
            if (modifierKeys.HasValue && (modifierKeys.Value & Keys.Shift) == Keys.Shift &&
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
                control.Selected = select;
            }

            // Then select activeControl, making it the new active control if necessary.
            if (activeControl != null)
            {
                SelectControl(activeControl, select, allowSetActivePage);
            }
        }

        /// <summary>
        /// Selects the specified <see paramref="control"/>.
        /// </summary>
        /// <param name="control">The <see cref="PaginationControl"/> to select.</param>
        /// <param name="select"><see langword="true"/> to select the control or
        /// <see langword="false"/> to de-select it.</param>
        /// <param name="allowSetActivePage"><see langword="true"/> if the
        /// <see paramref="control"/> should be able to be displayed and become the one and
        /// only "active" page; otheriwse, <see langword="false"/>.</param>
        void SelectControl(PaginationControl control, bool select, bool allowSetActivePage)
        {
            control.Selected = select;

            if (select)
            {
                _lastSelectedControl = control;
                _commandTargetControl = control;
            }

            var selectedPageControl = control as PageThumbnailControl;

            if (select && allowSetActivePage && selectedPageControl != null)
            {
                // If selecting and activating a page control, display the image...
                _displayedPageControl = selectedPageControl;
                selectedPageControl.DisplayPage(ImageViewer, true);

                // ... and make sure it is scrolled into view.
                if (Rectangle.Intersect(ClientRectangle, _displayedPageControl.Bounds) !=
                    _displayedPageControl.Bounds)
                {
                    _flowLayoutPanel.ScrollControlIntoView(_displayedPageControl);
                }
            }
            else if (!select)
            {
                if (control == _lastSelectedControl)
                {
                    _lastSelectedControl = null;
                }

                if (_commandTargetControl != null)
                {
                    _commandTargetControl = null;
                }

                if (_displayedPageControl != null &&
                    !_displayedPageControl.Selected)
                {
                    _displayedPageControl.DisplayPage(ImageViewer, false);
                    _displayedPageControl = null;
                }
            }

            UpdateCommandStates();
        }

        /// <summary>
        /// Gets the next <see cref="PageThumbnailControl"/> before or after
        /// <see paramref="currentControl"/>.
        /// </summary>
        /// <param name="forward"><see langword="true"/> to get the next
        /// <see cref="PageThumbnailControl"/>; <see langword="false"/> to get the previous.</param>
        /// <param name="currentControl">The <see cref="PaginationControl"/> relative to which to
        /// search.</param>
        /// <returns>the next <see cref="PageThumbnailControl"/> or <see langword="null"/> if there
        /// is no such control.</returns>
        PageThumbnailControl GetNextPageControl(bool forward, PaginationControl currentControl = null)
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
                    ? _flowLayoutPanel.Controls.OfType<PageThumbnailControl>().FirstOrDefault()
                    : _flowLayoutPanel.Controls.OfType<PageThumbnailControl>().LastOrDefault();
            }

            PageThumbnailControl pageControl = null;

            // Iterate from currentControl until the next page control is encountered.
            if (currentControl != null)
            {
                currentControl = forward
                    ? currentControl.NextControl
                    : currentControl.PreviousControl;

                pageControl = currentControl as PageThumbnailControl;
                while ((currentControl != null) && pageControl == null)
                {
                    currentControl = forward
                        ? currentControl.NextControl
                        : currentControl.PreviousControl;
                    pageControl = currentControl as PageThumbnailControl;
                }
            }

            return pageControl;
        }

        /// <summary>
        /// Gets the <see cref="PageThumbnailControl"/> down or up from from the active control.
        /// </summary>
        /// <param name="down"><see langword="true"/> to find the next control down;
        /// <see langword="false"/> to find the next control up.</param>
        /// <returns>The <see cref="PageThumbnailControl"/> down or up from from the active control.
        /// </returns>
        PageThumbnailControl GetNextRowPageControl(bool down)
        {
            PaginationControl currentControl = GetActiveControl(down);
            if (currentControl == null)
            {
                return GetNextPageControl(down);
            }

            // Iterate from currentControl until the next page control is encountered that is
            // vertically aligned with the current control.
            PageThumbnailControl result = null;
            for (PageThumbnailControl nextControl = GetNextPageControl(down, currentControl);
                 nextControl != null;
                 nextControl = GetNextPageControl(down, nextControl))
            {
                result = nextControl;

                // The left sides of controls in the same column may not line up due to padding
                // added to allow for separators, so compare the right side of the controls.
                if (result.Right == currentControl.Right)
                {
                    break;
                }
            }

            if (result == null || result.Right != currentControl.Right)
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Gets the <see cref="PaginationControl"/> down or up from from the active control.
        /// </summary>
        /// <param name="down"><see langword="true"/> to find the next control down;
        /// <see langword="false"/> to find the next control up.</param>
        /// <returns>The <see cref="PaginationControl"/> down or up from from the active control.
        /// </returns>
        PaginationControl GetNextRowControl(bool down)
        {
            PaginationControl currentControl = GetActiveControl(down);
            if (currentControl == null)
            {
                // If we got to the last control without finding anything, restart from the front/back
                // of the control sequence.
                currentControl = down
                    ? _flowLayoutPanel.Controls.OfType<PageThumbnailControl>().FirstOrDefault()
                    : _flowLayoutPanel.Controls.OfType<PageThumbnailControl>().LastOrDefault();
            }

            // Iterate from currentControl until the next page control is encountered that is
            // vertically aligned with the current control.
            PaginationControl result = null;
            for (PaginationControl nextControl =
                    down ? currentControl.NextControl : currentControl.PreviousControl;
                 nextControl != null && nextControl != currentControl;
                 nextControl = down ? nextControl.NextControl : nextControl.PreviousControl)
            {
                result = nextControl;

                // The left sides of controls in the same column may not line up due to padding
                // added to allow for separators, so compare the right side of the controls.
                if (result.Right == currentControl.Right)
                {
                    break;
                }
            }

            if (result.Right == currentControl.Right)
            {
                return result;
            }
            else
            {
                return null;
            }
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
                selectionTarget = GetNextPageControl(forward);
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
            ProcessControlSelection(selectionTarget, pageControls, true, true, 0);
        }

        /// <summary>
        /// Updates the availability of the context menu and shortcut key commands based on the
        /// current selection and control state.
        /// </summary>
        internal void UpdateCommandStates()
        {
            int contextMenuControlIndex = (_commandTargetControl == null)
                ? _flowLayoutPanel.Controls.Count
                : _flowLayoutPanel.Controls.IndexOf(_commandTargetControl);

            // Commands that operate on the active selection require there to be a selection and for
            // the _commandTargetControl to be one of the selected items.
            bool enableSelectionBasedCommands =
                _commandTargetControl != null && _commandTargetControl.Selected;

            _cutCommand.Enabled = enableSelectionBasedCommands;
            _copyCommand.Enabled = enableSelectionBasedCommands;
            _deleteCommand.Enabled = enableSelectionBasedCommands;

            // Insertied copied items requires there to be copied items.
            _insertCopiedCommand.Enabled = PaginationUtilityForm.ClipboardHasData();

            // Separators cannot be added next to other separators and cannot be the first control.
            bool controlIsSeparator = _commandTargetControl is PaginationSeparator;
            _toggleDocumentSeparatorCommand.Enabled =
                contextMenuControlIndex != 0 && !controlIsSeparator;

            // Adjust the text of the insertion commands to be append commands if there is no
            // _commandTargetControl.
            if (_commandTargetControl == null)
            {
                _insertCopiedMenuItem.Text = "Append copied item(s)";
                _paginationUtility._insertCopiedMenuItem.Text = "Append copied item(s)";
                _toggleDocumentSeparatorMenuItem.Text = "Toggle ending document separator   Space";
                _paginationUtility._insertDocumentSeparatorMenuItem.Text = "Toggle ending document separator   Space";
            }
            else
            {
                _insertCopiedMenuItem.Text = "Insert copied item(s)";
                _paginationUtility._insertCopiedMenuItem.Text = "Insert copied item(s)";
                _toggleDocumentSeparatorMenuItem.Text = "Toggle document separator   Space";
                _paginationUtility._insertDocumentSeparatorMenuItem.Text = "Toggle document separator   Space";
            }

            // Outputting a document is only allowed if the document(s) are fully selected.
            bool enableOutputCommand = FullySelectedDocuments.Count() > 0 &&
                PartiallySelectedDocuments.Count() == FullySelectedDocuments.Count();
            _outputDocumentCommand.Enabled = enableOutputCommand;
            _paginationUtility._outputDocumentToolStripButton.Enabled = enableOutputCommand;
            _paginationUtility._outputSelectedDocumentsMenuItem.Enabled = enableOutputCommand;

            OnStateChanged();
        }

        /// <summary>
        /// Clears any selection and closes the currently diplayed page in the
        /// <see cref="ImageViewer"/> if specfied by <see paramref="closeDisplayedPage"/>.
        /// </summary>
        void ClearSelection()
        {
            // Deselect all currently selected controls
            foreach (PaginationControl selectedControl in SelectedControls.ToArray())
            {
                selectedControl.Selected = false;

                if (selectedControl == _lastSelectedControl)
                {
                    _lastSelectedControl = null;
                }

                if (selectedControl == _commandTargetControl)
                {
                    _commandTargetControl = null;
                }
            }

            // The close the currently displayed image if specified..
            if (_displayedPageControl != null)
            {
                _displayedPageControl.DisplayPage(ImageViewer, false);
                _displayedPageControl = null;
            }

            UpdateCommandStates();
        }

        /// <summary>
        /// Copies the selected <see cref="Page"/>s to clipboard.
        /// </summary>
        void CopySelectedPagesToClipboard()
        {
            var copiedPages = new List<Page>(SelectedControls.Count());
            foreach (PaginationControl control in SelectedControls)
            {
                PageThumbnailControl pageControl = control as PageThumbnailControl;
                if (pageControl == null)
                {
                    // Add null to represent a document boundary (separator).
                    copiedPages.Add(null);
                }
                else
                {
                    copiedPages.Add(pageControl.Page);
                }
            }

            _paginationUtility.SetClipboardData(copiedPages);

            UpdateCommandStates();
        }

        /// <summary>
        /// Deletes the specified <see paramref="paginationControls"/>.
        /// </summary>
        /// <param name="paginationControls">The <see cref="PaginationControl"/>s to delete.</param>
        void DeleteControls(PaginationControl[] paginationControls)
        {
            int newSelectionPosition = -1;
            foreach (var control in paginationControls)
            {
                // Use the index of the active control at the point it is deleted as the index to
                // select following the deletion.
                PageThumbnailControl pageControl = control as PageThumbnailControl;
                if ((control.Selected && newSelectionPosition == -1) || 
                    (pageControl != null && pageControl.PageIsDisplayed))
                {
                    newSelectionPosition = _flowLayoutPanel.Controls.IndexOf(control);
                }

                RemovePaginationControl(control, true);
            }

            if (newSelectionPosition != -1 &&
                newSelectionPosition < _flowLayoutPanel.Controls.Count)
            {
                PaginationControl controlToSelect =
                    _flowLayoutPanel.Controls[newSelectionPosition] as PaginationControl;
                if (!(controlToSelect is PageThumbnailControl))
                {
                    controlToSelect = GetNextPageControl(true, controlToSelect);
                }

                if (controlToSelect != null && !SelectedControls.Any())
                {
                    ProcessControlSelection(controlToSelect, true, true);
                }
            }
        }

        /// <summary>
        /// Inserts the <see cref="Page"/>s from <see paramref="copiedPages"/> at the specified
        /// <see pararef="index"/>.
        /// </summary>
        /// <param name="copiedPages">The <see cref="Page"/>s to be inserted into this control.
        /// </param>
        /// <param name="index">The index at which the pages should be inserted.</param>
        void InsertPages(IEnumerable<Page> copiedPages, int index)
        {
            bool duplicatePagesExist = false;
            List<PaginationControl> insertedPaginationControls = new List<PaginationControl>();

            foreach (var page in copiedPages)
            {
                // A null page represents a document boundary; insert a separator.
                if (page == null)
                {
                    var separator = new PaginationSeparator();
                    insertedPaginationControls.Add(separator);

                    if (InitializePaginationControl(separator, index))
                    {
                        index++;
                    }
                }
                // Insert a new page control that uses the specified page.
                else
                {
                    var newPageControl = new PageThumbnailControl(null, page);
                    insertedPaginationControls.Add(newPageControl);

                    if (InitializePaginationControl(newPageControl, index))
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
            ProcessControlSelection(insertedPaginationControls.First(), 
                insertedPaginationControls, true, true, null);
        }

        /// <summary>
        /// Shows in the <see cref="ImageViewer"/> the page the mouse is currently hovering over.
        /// </summary>
        void ShowHoverPage()
        {
            Point mouseLocation = PointToClient(Control.MousePosition);
            var pageControl = _flowLayoutPanel.GetChildAtPoint(mouseLocation) as PageThumbnailControl;

            if (pageControl != null && pageControl != _hoverPageControl)
            {
                _hoverPageControl = pageControl;
                _hoverPageControl = pageControl;
                pageControl.DisplayPage(ImageViewer, true);
            }
        }

        /// <summary>
        /// Handles a UI command to select the next page.
        /// </summary>
        internal void HandleSelectNextPage()
        {
            try
            {
                PageThumbnailControl pageControl = GetNextPageControl(true);

                if (pageControl != null)
                {
                    ProcessControlSelection(pageControl, true, true);
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
                PageThumbnailControl pageControl = GetNextPageControl(false);

                if (pageControl != null)
                {
                    ProcessControlSelection(pageControl, true, true);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35457");
            }
        }

        /// <summary>
        /// Handles a UI command to select the previous page.
        /// </summary>
        void HandleSelectPreviousSinglePage()
        {
            try
            {
                PageThumbnailControl pageControl = GetNextPageControl(false);

                if (pageControl != null)
                {
                    ProcessControlSelection(pageControl, null, true, true, 0);
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
                PageThumbnailControl pageControl =
                    _flowLayoutPanel.Controls.OfType<PageThumbnailControl>().FirstOrDefault();

                if (pageControl != null)
                {
                    ProcessControlSelection(pageControl, true, true);
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
                PageThumbnailControl pageControl =
                    _flowLayoutPanel.Controls.OfType<PageThumbnailControl>().LastOrDefault();

                if (pageControl != null)
                {
                    ProcessControlSelection(pageControl, true, true);
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
                PageThumbnailControl pageControl = GetNextRowPageControl(true);

                if (pageControl != null)
                {
                    ProcessControlSelection(pageControl, true, true);
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
                PageThumbnailControl pageControl = GetNextRowPageControl(false);

                if (pageControl != null)
                {
                    ProcessControlSelection(pageControl, true, true);
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
                CopySelectedPagesToClipboard();

                DeleteControls(SelectedControls.ToArray());

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
                CopySelectedPagesToClipboard();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35466");
            }
        }

        /// <summary>
        /// Handles a UI command to insert the pages on the clipboard.
        /// </summary>
        internal void HandleInsertCopied()
        {
            try
            {
                SuspendLayout();

                IEnumerable<Page> copiedPages = _paginationUtility.GetClipboardData();

                if (copiedPages != null)
                {
                    int index = (_commandTargetControl == null)
                        ? -1
                        : _flowLayoutPanel.Controls.IndexOf(_commandTargetControl);

                    InsertPages(copiedPages, index);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35467");
            }
            finally
            {
                try
                {
                    UpdateCommandStates();
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI35468");
                }

                ResumeLayout(true);
            }
        }

        /// <summary>
        /// Handles a UI command to delete the selected items.
        /// </summary>
        internal void HandleDeleteSelectedItems()
        {
            try
            {
                DeleteControls(SelectedControls.ToArray());

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35469");
            }
        }

        /// <summary>
        /// Handles a UI command to insert or remove a document separator from before this control.
        /// </summary>
        internal void HandleToggleDocumentSeparator()
        {
            try
            {
                SuspendLayout();

                PaginationSeparator targetSeparator = (_commandTargetControl == null)
                    ? _flowLayoutPanel.Controls.OfType<Control>().Last() as PaginationSeparator
                    : _commandTargetControl.PreviousControl as PaginationSeparator;

                if (targetSeparator != null)
                {
                    RemovePaginationControl(targetSeparator, true);
                }
                else
                {
                    int index = (_commandTargetControl == null)
                    ? -1
                    : _flowLayoutPanel.Controls.IndexOf(_commandTargetControl);

                    InitializePaginationControl(new PaginationSeparator(), index);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35470");
            }
            finally
            {
                try
                {
                    UpdateCommandStates();
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI35471");
                }

                ResumeLayout(true);
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
        /// Raised the <see cref="StateChanged"/> event.
        /// </summary>
        void OnStateChanged()
        {
            var eventHandler = StateChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
