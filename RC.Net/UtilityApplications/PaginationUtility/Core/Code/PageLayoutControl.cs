using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// inadventent display of pages when trying to use keyboard navigation instead of the
        /// mouse.
        /// </summary>
        static readonly int _HOVER_MOVE_EVENT_CRITERIA = 5;

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
        /// Indicates the control from which a drag-and-drop operation was initiated.
        /// </summary>
        PaginationControl _dragSource;

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
        readonly ToolStripMenuItem _cutMenuItem = new ToolStripMenuItem("Cut item(s)");

        /// <summary>
        /// The <see cref="ApplicationCommand"/> that controls the availability of the cut operation.
        /// </summary>
        ApplicationCommand _cutCommand;

        /// <summary>
        /// Context menu option that allows the PaginationControls to be copied.
        /// </summary>
        readonly ToolStripMenuItem _copyMenuItem = new ToolStripMenuItem("Copy item(s)");

        /// <summary>
        /// The <see cref="ApplicationCommand"/> that controls the availability of the copy operation.
        /// </summary>
        ApplicationCommand _copyCommand;

        /// <summary>
        /// Context menu option that allows the PaginationControls to be deleted.
        /// </summary>
        readonly ToolStripMenuItem _deleteMenuItem = new ToolStripMenuItem("Delete item(s)");

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
        /// Context menu option that allows the currently selected document to be renamed.
        /// NOTE: The option text will be set later.
        /// </summary>
        readonly ToolStripMenuItem _insertDocumentSeparator = new ToolStripMenuItem();

        /// <summary>
        /// The <see cref="ApplicationCommand"/> that controls the availability of the insert
        /// pagination separator operation.
        /// </summary>
        ApplicationCommand _insertDocumentSeparatorCommand;

        /// <summary>
        /// Context menu option that allows the currently selected document(s) to be output.
        /// </summary>
        readonly ToolStripMenuItem _outputDocumentMenuItem = new ToolStripMenuItem("Output document(s)");

        /// <summary>
        /// The <see cref="ApplicationCommand"/> that controls the availability of the output operation.
        /// </summary>
        ApplicationCommand _outputDocumentCommand;

        #endregion Fields

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PageLayoutControl"/> class.
        /// </summary>
        /// <param name="paginationUtility">The <see cref="PaginationUtilityForm"/> of which this
        /// instance is a member.</param>
        public PageLayoutControl(PaginationUtilityForm paginationUtility)
            : base()
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI35428", _OBJECT_NAME);

                InitializeComponent();

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
        /// Gets a value indicating whether the output document command is enabled.
        /// </summary>
        /// <value><see langword="true"/> if the output document command is enabled; otherwise,
        /// <see langword="false"/>.</value>
        public bool OutputDocumentCommandEnabled
        {
            get
            {
                return _outputDocumentCommand.Enabled;
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
                foreach (Page page in sourceDocument.Pages)
                {
                    AddPaginationControl(new PageThumbnailControl(outputDocument, page));
                }

                // Indicate if the document is copied in its present form, it can simply be copied
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
                foreach (OutputDocument document in PartiallySelectedDocuments.ToArray())
                {
                    document.Output();

                    // After each document is output, remove its page controls.
                    foreach (var page in document.PageControls.ToArray())
                    {
                        RemovePaginationControl(page, true, false);
                    }
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
        /// <param name="deleted"><see langword="true"/> if the <see paramref="control"/> should be
        /// considered deleted; <see langword="false"/> if it is being removed only temporarily.
        /// </param>
        public void RemovePaginationControl(PaginationControl control, bool dispose, bool deleted)
        {
            try
            {
                if (!_flowLayoutPanel.Controls.Contains(control))
                {
                    return;
                }

                // The removed control should not longer be considered selected.
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

                // Determine the which controls are before and after the removed control to
                // determine how the removal affects the output documents.
                var preceedingPageControl = control.PreceedingControl as PageThumbnailControl;
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

                    if (removedPageControl.Document != null)
                    {
                        removedPageControl.Document.RemovePage(removedPageControl, deleted);
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
                        if (preceedingPageControl != null)
                        {
                            OutputDocument firstDocument = preceedingPageControl.Document;
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
                if (!AddPaginationControl(newControl, index))
                {
                    newControl.Dispose();
                    return false;
                }

                var nextPageControl = newControl.NextControl as PageThumbnailControl;
                var preceedingPageControl = newControl.PreceedingControl as PageThumbnailControl;
                var newPageControl = newControl as PageThumbnailControl;

                if (newPageControl != null)
                {
                    OutputDocument document = (preceedingPageControl == null)
                        ? (nextPageControl == null) ? null : nextPageControl.Document
                        : preceedingPageControl.Document;
                    if (document != null)
                    {
                        newPageControl.Document = document;

                        int newPageNumber = (preceedingPageControl == null)
                            ? 1
                            : preceedingPageControl.PageNumber + 1;
                        document.AddPage(newPageControl, newPageNumber);
                    }
                    else
                    {
                        document = _paginationUtility.CreateOutputDocument(
                            newPageControl.Page.OriginalDocumentName);
                        newPageControl.Document = document;

                        document.AddPage(newPageControl);
                    }
                }
                else if (nextPageControl != null && preceedingPageControl != null)
                {
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

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            try
            {
                _flowLayoutPanel.Controls.Clear();

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35438");
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

                _cutCommand = new ApplicationCommand(ImageViewer.Shortcuts,
                    new Keys[] { Keys.Control | Keys.X }, HandleCutSelectedControls,
                    new[] { _cutMenuItem }, false, true, false);
                _cutMenuItem.Click += HandleCutMenuItem_Click;

                _copyCommand = new ApplicationCommand(ImageViewer.Shortcuts,
                    new Keys[] { Keys.Control | Keys.C }, HandleCopySelectedControls,
                    new[] { _copyMenuItem }, false, true, false);
                _copyMenuItem.Click += HandleCopyMenuItem_Click;

                _insertCopiedCommand = new ApplicationCommand(ImageViewer.Shortcuts,
                    new Keys[] { Keys.Control | Keys.V }, HandleInsertCopied,
                    new[] { _insertCopiedMenuItem }, false, true, false);
                _insertCopiedMenuItem.Click += HandleInsertCopiedMenuItem_Click;

                _deleteCommand = new ApplicationCommand(ImageViewer.Shortcuts,
                    new Keys[] { Keys.Delete }, HandleDeleteSelectedItems,
                    new[] { _deleteMenuItem }, false, true, false);
                _deleteMenuItem.Click += HandleDeleteMenuItem_Click;

                _insertDocumentSeparatorCommand = new ApplicationCommand(ImageViewer.Shortcuts,
                    new Keys[] { Keys.Control | Keys.D }, HandleInsertDocumentSeparator,
                    new[] { _insertDocumentSeparator }, false, true, true);
                _insertDocumentSeparator.Click += HandleInsertDocumentSeparator_Click;

                _outputDocumentCommand = new ApplicationCommand(ImageViewer.Shortcuts,
                    new Keys[] { Keys.Control | Keys.S }, HandleOutputDocument,
                    new[] { _outputDocumentMenuItem }, false, true, false);
                _outputDocumentMenuItem.Click += HandleOutputMenuItem_Click;

                ImageViewer.Shortcuts[Keys.Tab] = HandleSelectNextPage;
                ImageViewer.Shortcuts[Keys.Tab | Keys.Control] = HandleSelectNextDocument;
                ImageViewer.Shortcuts[Keys.Tab | Keys.Shift] = HandleSelectPreviousPage;
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

                ImageViewer.Shortcuts[Keys.Home] = HandleSeletFirstPage;
                ImageViewer.Shortcuts[Keys.Home | Keys.Control] = HandleSeletFirstPage;
                ImageViewer.Shortcuts[Keys.Home | Keys.Shift] = HandleSeletFirstPage;
                ImageViewer.Shortcuts[Keys.Home | Keys.Control | Keys.Shift] = HandleSeletFirstPage;

                ImageViewer.Shortcuts[Keys.End] = HandleSeletLastPage;
                ImageViewer.Shortcuts[Keys.End | Keys.Control] = HandleSeletLastPage;
                ImageViewer.Shortcuts[Keys.End | Keys.Shift] = HandleSeletLastPage;
                ImageViewer.Shortcuts[Keys.End | Keys.Control | Keys.Shift] = HandleSeletLastPage;

                ContextMenuStrip = new ContextMenuStrip();
                ContextMenuStrip.Items.Add(_cutMenuItem);
                ContextMenuStrip.Items.Add(_copyMenuItem);
                ContextMenuStrip.Items.Add(_deleteMenuItem);
                ContextMenuStrip.Items.Add(new ToolStripSeparator());
                ContextMenuStrip.Items.Add(_insertCopiedMenuItem);
                ContextMenuStrip.Items.Add(_insertDocumentSeparator);
                ContextMenuStrip.Items.Add(new ToolStripSeparator());
                ContextMenuStrip.Items.Add(_outputDocumentMenuItem);
                ContextMenuStrip.Opening += HandleContextMenuStrip_Opening;

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

//        /// <summary>
//        /// Selects the next available control and makes it the active control.
//        /// </summary>
//        /// <param name="forward">true to cycle forward through the controls in the
//        /// <see cref="T:System.Windows.Forms.ContainerControl"/>; otherwise, false.</param>
//        /// <returns>
//        /// true if a control is selected; otherwise, false.
//        /// </returns>
//        protected override bool ProcessTabKey(bool forward)
//        {
//            try
//            {
//                SelectNextDocument(forward);
//            }
//            catch (Exception ex)
//            {
//                ex.ExtractDisplay("ELI35441");
//            }
//
//            return true;
//        }

        /// <summary>
        /// </summary>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"/> values that represents the key to process.</param>
        /// <returns>
        /// true if the key was processed by the control; otherwise, false.
        /// </returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            try
            {
                if (keyData == Keys.Escape)
                {
                    ClearSelection(true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35442");
            }

            return base.ProcessDialogKey(keyData);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyUp"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            try
            {
                base.OnKeyUp(e);

                HandleKeyUp(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35443");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the HandleThumbnailControl control.
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
        /// Handles the DoubleClick event of the HandleThumbnailControl control.
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
        /// Handles the MouseMove event of the HandleThumbnailControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance
        /// containing the event data.</param>
        void HandlePaginationControl_MouseMove(object sender, MouseEventArgs e)
        {
            bool startedDragOperation = false;

            try
            {
                if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                {
                    var originControl = sender as PaginationControl;
                    if (originControl != null && originControl.Selected)
                    {
                        Point mouseLocation = PointToClient(Control.MousePosition);
                        var currentControl =
                            _flowLayoutPanel.GetChildAtPoint(mouseLocation) as PaginationControl;

                        if (currentControl != null && currentControl != originControl)
                        {
                            startedDragOperation = true;

                            _dragSource = originControl;
                            var dataObject = new DataObject(_DRAG_DROP_DATA_FORMAT, this);
                            DoDragDrop(dataObject, DragDropEffects.Move);
                        }
                    }
                }
                else if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    // Require enough successive mouse move events with the control key down to
                    // prevent the inadventent display of pages when trying to use keyboard
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
            finally
            {
                if (startedDragOperation)
                {
                    _dragSource = null;
                }
            }
        }

        /// <summary>
        /// Handles the DragEnter event of the HandleFlowLayoutPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
        void HandleFlowLayoutPanel_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                e.Effect = DragDropEffects.Move;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35447");
            }
        }

        /// <summary>
        /// Handles the DragLeave event of the HandleFlowLayoutPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleFlowLayoutPanel_DragLeave(object sender, EventArgs e)
        {
            try
            {
                SuspendLayout();

                _dropLocationIndex = -1;
                Controls.Remove(_dropLocationIndicator);
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
        /// Handles the DragOver event of the HandleFlowLayoutPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance
        /// containing the event data.</param>
        void HandleFlowLayoutPanel_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                var sourceLayoutControl = e.Data.GetData(_DRAG_DROP_DATA_FORMAT) as PageLayoutControl;
                if (sourceLayoutControl == null)
                {
                    return;
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
                if (control != null && (_dragSource == null || _dragSource != control))
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

                    location.Offset(-_dropLocationIndicator.Width / 2, 0);

                    if (!Controls.Contains(_dropLocationIndicator))
                    {
                        Controls.Add(_dropLocationIndicator);
                        _dropLocationIndicator.BringToFront();
                    }
                    _dropLocationIndicator.Location = location;
                    _dropLocationIndicator.Height = control.Height;
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    _dropLocationIndex = -1;
                    Controls.Remove(_dropLocationIndicator);
                    e.Effect = DragDropEffects.Move;
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
        /// Handles the DragDrop event of the HandleFlowLayoutPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
        void HandleFlowLayoutPanel_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                SuspendLayout();

                //using (new LockControlUpdates(this, true, false))
                {
                    var sourceLayoutControl = e.Data.GetData(_DRAG_DROP_DATA_FORMAT) as PageLayoutControl;
                    if (sourceLayoutControl != null && _dropLocationIndex >= 0)
                    {
                        var draggedControls = sourceLayoutControl.SelectedControls.ToArray();

                        int index = _dropLocationIndex;
                        foreach (PaginationControl control in draggedControls)
                        {
                            if (sourceLayoutControl == this &&
                                _flowLayoutPanel.Controls.IndexOf(control) <= index)
                            {
                                index--;
                            }
                            sourceLayoutControl.RemovePaginationControl(control, false, false);
                        }

                        foreach (PaginationControl control in draggedControls)
                        {
                            if (index > 0 && control is PaginationSeparator &&
                                _flowLayoutPanel.Controls[index - 1] is PaginationSeparator)
                            {
                                control.Dispose();
                            }
                            else if (InitializePaginationControl(control, index++))
                            {
                                control.Selected = true;
                            }
                        }
                    }
                    else
                    {
                        e.Effect = DragDropEffects.None;
                    }

                    _dropLocationIndex = -1;
                    Controls.Remove(_dropLocationIndicator);

                    UpdateCommandStates();
                }
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

                //Refresh();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HandleSeparatorControl_LocationChanged(object sender, EventArgs e)
        {
            try
            {
                var separtorControl = sender as PaginationSeparator;
                if (separtorControl != null)
                {
                    if (separtorControl.PreceedingControl == null)
                    {
                        PaginationControl nextControl = separtorControl.NextControl;
                        RemovePaginationControl(separtorControl, true, true);
                        if (nextControl != null)
                        {
                            nextControl.PerformLayout();
                        }
                    }
                    else if (separtorControl.PreceedingControl is PaginationSeparator)
                    {
                        RemovePaginationControl(separtorControl, true, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35452");
            }
        }

        /// <summary>
        /// Handles the Click event of the HandleFlowLayoutPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
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
        /// Handles the Opening event of the HandleContextMenuStrip control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance
        /// containing the event data.</param>
        void HandleContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            try
            {
                Point mouseLocation = PointToClient(MousePosition);
                var paginationControl =
                    _flowLayoutPanel.GetChildAtPoint(mouseLocation) as PaginationControl;
                if (paginationControl != null && !paginationControl.Selected)
                {
                    ProcessControlSelection(paginationControl, true, true);
                }

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35454");
            }
        }

        /// <summary>
        /// Handles the Click event of the HandleCutMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleCutMenuItem_Click(object sender, EventArgs e)
        {
            HandleCutSelectedControls();
        }

        /// <summary>
        /// Handles the Click event of the HandleCopyMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleCopyMenuItem_Click(object sender, EventArgs e)
        {
            HandleCopySelectedControls();
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the _insertCopiedMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleInsertCopiedMenuItem_Click(object sender, EventArgs e)
        {
            HandleInsertCopied();
        }

        /// <summary>
        /// Handles the Click event of the HandleDeleteMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleDeleteMenuItem_Click(object sender, EventArgs e)
        {
            HandleDeleteSelectedItems();
        }

        /// <summary>
        /// Handles the Click event of the HandleInsertDocumentSeparator control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleInsertDocumentSeparator_Click(object sender, EventArgs e)
        {
            HandleInsertDocumentSeparator();
        }

        /// <summary>
        /// Handles the Click event of the HandleMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOutputMenuItem_Click(object sender, EventArgs e)
        {
            HandleOutputDocument();
        }

        /// <summary>
        /// Handles the KeyUp event of the HandlePaginationControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing
        /// the event data.</param>
        void HandlePaginationControl_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                HandleKeyUp(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35455");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets the selected controls.
        /// </summary>
        /// <returns></returns>
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
        /// Gets the page layout control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        static PageLayoutControl GetPageLayoutControl(Control control)
        {
            PageLayoutControl pageLayoutControl = null;
            for (Control current = control;
                 pageLayoutControl == null && current != null; 
                 current = current.Parent)
            {
                pageLayoutControl = current as PageLayoutControl;
            }

            return pageLayoutControl;
        }

        /// <summary>
        /// Gets the active page control.
        /// </summary>
        /// <param name="first">if set to <see langword="true"/> [first].</param>
        PageThumbnailControl GetActivePageControl(bool first = true)
        {
            if (_displayedPageControl != null)
            {
                return _displayedPageControl;
            }
            
            var pageControl = first
                ? SelectedControls.OfType<PageThumbnailControl>().FirstOrDefault()
                : SelectedControls.OfType<PageThumbnailControl>().LastOrDefault();

            return pageControl;
        }

        /// <summary>
        /// Gets the document page controls.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        IEnumerable<PageThumbnailControl> GetDocumentPageControls(SourceDocument document)
        {
            return _flowLayoutPanel.Controls
                .OfType<PageThumbnailControl>()
                .Where(control => document.Pages.Contains(control.Page));
        }

        /// <summary>
        /// Updates the control list.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        bool AddPaginationControl(PaginationControl control, int index = -1)
        {
            bool isPageControl = control is PageThumbnailControl;

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
            else // is separtor
            {
                control.Height = (control.PreceedingControl != null)
                    ? control.PreceedingControl.Height
                    : (control.NextControl != null) ? control.NextControl.Height : Height;
                control.LocationChanged += HandleSeparatorControl_LocationChanged;
            }

            return true;
        }

        /// <summary>
        /// Adds the pages to document.
        /// </summary>
        /// <param name="newDocument">The new document.</param>
        /// <param name="thumbnailControl">The thumbnail control.</param>
        void MovePagesToDocument(OutputDocument newDocument, PageThumbnailControl thumbnailControl)
        {
            int index = _flowLayoutPanel.Controls.IndexOf(thumbnailControl);
            OutputDocument oldDocument = thumbnailControl.Document;

            while (thumbnailControl != null && thumbnailControl.Document == oldDocument)
            {
                thumbnailControl.Document.RemovePage(thumbnailControl, false);
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
        /// Processes the control selection.
        /// </summary>
        /// <param name="activeControl">The active control.</param>
        /// <param name="forceSelect">if set to <see langword="true"/> [force select].</param>
        /// <param name="allowSetActivePage"></param>
        void ProcessControlSelection(PaginationControl activeControl, bool forceSelect,
            bool allowSetActivePage)
        {
            ProcessControlSelection(activeControl, null, forceSelect, allowSetActivePage, 
                Control.ModifierKeys);
        }

        /// <summary>
        /// Processes the control selection.
        /// </summary>
        /// <param name="activeControl">The active control.</param>
        /// <param name="additionalControls">The additional controls.</param>
        /// <param name="forceSelect">if set to <see langword="true"/> [force select].</param>
        /// <param name="allowSetActivePage">if set to <see langword="true"/> [allow set active page].</param>
        /// <param name="modifierKeys"></param>
        void ProcessControlSelection(PaginationControl activeControl,
            IEnumerable<PaginationControl> additionalControls, bool forceSelect,
            bool allowSetActivePage, Keys? modifierKeys)
        {
            bool select = forceSelect || activeControl == null || !activeControl.Selected;
            PaginationControl lastSelectedControl = _lastSelectedControl;

//            if (!select && 
//                (!modifierKeys.HasValue && Control.ModifierKeys == 0) && 
//                SelectedControls.Count() > 1)
//            {
//                select = true;
//            }

            if (!modifierKeys.HasValue || (modifierKeys.Value & Keys.Control) == 0)
            {
                ClearSelection(false);
            }

            var additionalControlSet = (additionalControls == null)
                ? new HashSet<PaginationControl>()
                : new HashSet<PaginationControl>(additionalControls);

            if (modifierKeys.HasValue && (modifierKeys.Value & Keys.Shift) == Keys.Shift &&
                lastSelectedControl != null && activeControl != null &&
                activeControl != lastSelectedControl)
            {
                bool inSelectionRange = false;
                foreach (var control in _flowLayoutPanel.Controls.OfType<PaginationControl>())
                {
                    if (inSelectionRange)
                    {
                        additionalControlSet.Add(control);

                        if (control == activeControl || control == lastSelectedControl)
                        {
                            break;
                        }
                    }
                    else if (control == activeControl || control == lastSelectedControl)
                    {
                        inSelectionRange = true;

                        additionalControlSet.Add(control);
                    }
                }
            }

            foreach (var control in additionalControlSet.Except(new[] { activeControl }))
            {
                control.Selected = select;
            }

            if (activeControl != null)
            {
                SelectControl(activeControl, select, allowSetActivePage);
            }
        }

        /// <summary>
        /// Selects the control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="select">if set to <see langword="true"/> [select].</param>
        /// <param name="allowSetActivePage"></param>
        void SelectControl(PaginationControl control, bool select, bool allowSetActivePage)
        {
            control.Selected = select;

            var selectedPageControl = control as PageThumbnailControl;
            if (select && allowSetActivePage && selectedPageControl != null)
            {
                _displayedPageControl = selectedPageControl;
                _lastSelectedControl = selectedPageControl;
                selectedPageControl.DisplayPage(ImageViewer, true);

                if (Rectangle.Intersect(ClientRectangle, _displayedPageControl.Bounds) != 
                    _displayedPageControl.Bounds)
                {
                    _flowLayoutPanel.ScrollControlIntoView(_displayedPageControl);
                }
            }
            else if (!select && control == _lastSelectedControl)
            {
                _lastSelectedControl = null;
            }
            else if (!select && _displayedPageControl != null &&
                !_displayedPageControl.Selected)
            {
                _displayedPageControl.DisplayPage(ImageViewer, false);
                _displayedPageControl = null;
            }

            UpdateCommandStates();
        }

        /// <summary>
        /// Gets the next page control.
        /// </summary>
        /// <param name="forward">if set to <see langword="true"/> [forward].</param>
        /// <param name="currentControl">The current control.</param>
        /// <returns></returns>
        PageThumbnailControl GetNextPageControl(bool forward, PaginationControl currentControl = null)
        {
            if (currentControl == null)
            {
                currentControl = GetActivePageControl(forward);
            }

            if (currentControl != null)
            {
                currentControl = forward
                    ? currentControl.NextControl
                    : currentControl.PreceedingControl;

                while (currentControl is PaginationSeparator)
                {
                    currentControl = forward
                        ? currentControl.NextControl
                        : currentControl.PreceedingControl;
                }
            }

            if (currentControl == null)
            {
                currentControl = forward
                    ? _flowLayoutPanel.Controls.OfType<PageThumbnailControl>().FirstOrDefault()
                    : _flowLayoutPanel.Controls.OfType<PageThumbnailControl>().LastOrDefault();
            }

            return currentControl as PageThumbnailControl;
        }

        /// <summary>
        /// Gets the next row page control.
        /// </summary>
        /// <param name="down">if set to <see langword="true"/> [down].</param>
        /// <returns></returns>
        PageThumbnailControl GetNextRowPageControl(bool down)
        {
            PageThumbnailControl currentControl = GetActivePageControl(down);
            if (currentControl == null)
            {
                return GetNextPageControl(down);
            }

            PageThumbnailControl result = null;
            for (PageThumbnailControl nextControl = GetNextPageControl(down, currentControl);
                 nextControl != currentControl;
                 nextControl = GetNextPageControl(down, nextControl))
            {
                result = nextControl;

                if (result.Top != currentControl.Top) 
                {
                    if (result.Right == currentControl.Right)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Selects the next document.
        /// </summary>
        /// <param name="forward"></param>
        /// <param name="selectionTarget"></param>
        void SelectNextDocument(bool forward, PageThumbnailControl selectionTarget = null)
        {
            if (!_flowLayoutPanel.Controls.OfType<PageThumbnailControl>().Any())
            {
                return;
            }

            if (selectionTarget == null)
            {
                selectionTarget = GetActivePageControl(forward);
            }

            if (selectionTarget == null)
            {
                selectionTarget = _flowLayoutPanel.Controls.OfType<PageThumbnailControl>().First();
            }

            // If there are no page controls, there are no documents to select.
            if (selectionTarget == null)
            {
                return;
            }

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
                        : control.PreceedingControl;

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

            ProcessControlSelection(selectionTarget, pageControls, true, true, 0);
        }

        /// <summary>
        /// Updates the command states.
        /// </summary>
        void UpdateCommandStates()
        {
            Point mouseLocation = PointToClient(MousePosition);
            _commandTargetControl = _flowLayoutPanel.GetChildAtPoint(mouseLocation) as PaginationControl;
            int contextMenuControlIndex = (_commandTargetControl == null)
                ? _flowLayoutPanel.Controls.Count
                : _flowLayoutPanel.Controls.IndexOf(_commandTargetControl);

            bool enableSelectionBasedCommands =
                _commandTargetControl != null && _commandTargetControl.Selected;

            _cutCommand.Enabled = enableSelectionBasedCommands;
            _copyCommand.Enabled = enableSelectionBasedCommands;
            _deleteCommand.Enabled = enableSelectionBasedCommands;

            _insertCopiedCommand.Enabled = _paginationUtility.ClipboardHasData();

            bool controlIsSepartor = _commandTargetControl is PaginationSeparator;
            bool preceedingControlIsSeparator = contextMenuControlIndex > 0 &&
                _flowLayoutPanel.Controls[contextMenuControlIndex - 1] is PaginationSeparator;
            _insertDocumentSeparatorCommand.Enabled =
                !controlIsSepartor && !preceedingControlIsSeparator;

            if (_commandTargetControl == null)
            {
                _insertCopiedMenuItem.Text = "Append copied item(s)";
                _insertDocumentSeparator.Text = "Append document separator";
            }
            else
            {
                _insertCopiedMenuItem.Text = "Insert copied item(s)";
                _insertDocumentSeparator.Text = "Insert document separator";
            }

            _outputDocumentCommand.Enabled = FullySelectedDocuments.Count() > 0; ;

            OnStateChanged();
        }

        /// <summary>
        /// Clears the selection.
        /// </summary>
        void ClearSelection(bool closeDisplayedPage)
        {
            foreach (PaginationControl selectedControl in SelectedControls)
            {
                selectedControl.Selected = false;               

                if (selectedControl == _lastSelectedControl)
                {
                    _lastSelectedControl = null;
                }
            }

            if (closeDisplayedPage && _displayedPageControl != null)
            {
                _displayedPageControl.DisplayPage(ImageViewer, false);
                _displayedPageControl = null;
            }

            UpdateCommandStates();
        }

        /// <summary>
        /// Copies the selected pages to clipboard.
        /// </summary>
        void CopySelectedPagesToClipboard()
        {
            var copiedPages = new List<Page>(SelectedControls.Count());
            foreach (PaginationControl control in SelectedControls)
            {
                PageThumbnailControl pageControl = control as PageThumbnailControl;
                if (pageControl == null)
                {
                    copiedPages.Add(null);
                }
                else
                {
                    copiedPages.Add(pageControl.Page);
                }
            }

            _paginationUtility.SetClipboardData(copiedPages);
        }

        /// <summary>
        /// Shows the hover page.
        /// </summary>
        void ShowHoverPage()
        {
            Point mouseLocation = PointToClient(Control.MousePosition);
            var pageControl = _flowLayoutPanel.GetChildAtPoint(mouseLocation) as PageThumbnailControl;

            if (pageControl != null && pageControl != _hoverPageControl)
            {
                _hoverPageControl = pageControl;
                pageControl.DisplayPage(ImageViewer, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void HandleSelectNextPage()
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
        /// 
        /// </summary>
        void HandleSelectPreviousPage()
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
        /// 
        /// </summary>
        void HandleSeletFirstPage()
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
        /// 
        /// </summary>
        void HandleSeletLastPage()
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
        /// Handles the select next row page.
        /// </summary>
        void HandleSelectNextRowPage()
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
        /// Handles the select next row page.
        /// </summary>
        void HandleSelectPreviousRowPage()
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
        /// Handles the select next document.
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
        /// Handles the select next document.
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
        /// Handles the key up.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing
        /// the event data.</param>
        void HandleKeyUp(KeyEventArgs e)
        {
            try
            {
                if (_hoverPageControl != null && !e.Control)
                {
                    if (_displayedPageControl != _hoverPageControl)
                    {
                        if (_displayedPageControl == null)
                        {
                            ImageViewer.CloseImage();
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
                ex.ExtractDisplay("ELI35464");
            }
        }

        /// <summary>
        /// Handles the cut selected controls.
        /// </summary>
        void HandleCutSelectedControls()
        {
            try
            {
                CopySelectedPagesToClipboard();

                foreach (var control in SelectedControls.ToArray())
                {
                    RemovePaginationControl(control, true, true);
                }

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35465");
            }
        }

        /// <summary>
        /// Handles the copy selected controls.
        /// </summary>
        void HandleCopySelectedControls()
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
        /// Handles the insert copied.
        /// </summary>
        void HandleInsertCopied()
        {
            try
            {
                SuspendLayout();

                //using (new LockControlUpdates(this, true, false))
                {
                    IEnumerable<Page> copiedPages = _paginationUtility.GetClipboardData();

                    if (copiedPages != null)
                    {
                        int index = (_commandTargetControl == null)
                            ? -1
                            : _flowLayoutPanel.Controls.IndexOf(_commandTargetControl);

                        foreach (var page in copiedPages)
                        {
                            if (page == null)
                            {
                                if (InitializePaginationControl(new PaginationSeparator(), index))
                                {
                                    index++;
                                }
                            }
                            else
                            {
                                var newPageControl = new PageThumbnailControl(null, page);

                                if (InitializePaginationControl(newPageControl, index))
                                {
                                    index++;
                                }
                            }
                        }
                    }
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
                //Refresh();
            }
        }

        /// <summary>
        /// Handles the delete selected items.
        /// </summary>
        void HandleDeleteSelectedItems()
        {
            try
            {
                var selectedControls = SelectedControls.ToArray();

                foreach (var control in selectedControls)
                {
                    RemovePaginationControl(control, true, true);
                }

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35469");
            }
        }

        /// <summary>
        /// Handles the insert document separator.
        /// </summary>
        void HandleInsertDocumentSeparator()
        {
            try
            {
                SuspendLayout();

                int index = (_commandTargetControl == null)
                    ? -1
                    : _flowLayoutPanel.Controls.IndexOf(_commandTargetControl);

                InitializePaginationControl(new PaginationSeparator(), index);
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
                //Refresh();
            }
        }

        /// <summary>
        /// Handles the output menu item.
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
        /// Called when [command state updated].
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
