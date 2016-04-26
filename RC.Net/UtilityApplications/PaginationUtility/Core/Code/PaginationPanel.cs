using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Pagination functionality encapsulated in a panel to allow for pagination as part of an
    /// application other than PaginationUtility.exe.
    /// </summary>
    public partial class PaginationPanel : UserControl, IPaginationUtility, IImageViewerControl
    {
        #region Fields

        /// <summary>
        /// The <see cref="PageLayoutControl"/> into which input documents are initially loaded.
        /// </summary>
        PageLayoutControl _primaryPageLayoutControl;

        /// <summary>
        /// All <see cref="SourceDocument"/>s currently active in the UI in the order they appeared
        /// before any user edits.
        /// </summary>
        List<SourceDocument> _sourceDocuments = new List<SourceDocument>();

        /// <summary>
        /// Synchronizes access to <see cref="_sourceDocuments"/>
        /// </summary>
        object _sourceDocumentLock = new object();

        /// <summary>
        /// The set of all <see cref="OutputDocument"/>s currently active in the UI.
        /// </summary>
        HashSet<ExtendedOutputDocument> _pendingDocuments = new HashSet<ExtendedOutputDocument>();

        /// <summary>
        /// The <see cref="ExtendedOutputDocument"/>s that were originally present as a result of
        /// the last LoadFile or RemoveSourceFile operation.
        /// </summary>
        List<ExtendedOutputDocument> _originalDocuments = new List<ExtendedOutputDocument>();

        /// <summary>
        /// All <see cref="ExtendedOutputDocument"/> that relate to each
        /// <see cref="SourceDocument"/>.
        /// </summary>
        Dictionary<SourceDocument, HashSet<ExtendedOutputDocument>> _sourceToOriginalDocuments =
            new Dictionary<SourceDocument, HashSet<ExtendedOutputDocument>>();

        /// <summary>
        /// Maintains a temporary files to which <see cref="OutputDocument"/>s should be written
        /// until the owner of this panel has the opportunity to determine where it is going.
        /// </summary>
        Dictionary<ExtendedOutputDocument, TemporaryFile> _tempFiles =
            new Dictionary<ExtendedOutputDocument, TemporaryFile>();

        /// <summary>
        /// Keeps track of the names of documents that have been output as part of the current
        /// <see cref="CommitPendingChanges"/> call.
        /// </summary>
        List<string> _outputDocumentNames = new List<string>();

        /// <summary>
        /// Keeps track of whether pagination is suggested for a particular source file name and the
        /// VOA file data that needs to be assigned.
        /// </summary>
        Dictionary<string, Tuple<bool, object>> _documentData =
            new Dictionary<string, Tuple<bool, object>>();

        /// <summary>
        /// Indicated a manually overridden value for <see cref="PendingChanges"/> or
        /// <see langword="null"/> if there is no manually overridden value
        /// (<see cref="PendingChanges"/> will return a calculated value).
        /// </summary>
        bool? _pendingChangesOverride;

        /// <summary>
        /// Indicates whether pending pagination changes are in the process of being committed.
        /// </summary>
        bool _commitingChanges;

        /// <summary>
        /// The <see cref="ImageViewer"/> to be used by this instance.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// The <see cref="ShortcutsManager"/> managing all keyboard shortcuts for this instance.
        /// </summary>
        ShortcutsManager _shortcuts = new ShortcutsManager();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationPanel"/> class.
        /// </summary>
        public PaginationPanel()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39554");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when a newly paginated document is generated.
        /// </summary>
        public event EventHandler<CreatingOutputDocumentEventArgs> CreatingOutputDocument;

        /// <summary>
        /// Raised when a pagination operation is complete. May follow multiple
        /// <see cref="CreatingOutputDocument"/> events if a single pagination event produced
        /// multiple documents.
        /// </summary>
        public event EventHandler<PaginatedEventArgs> Paginated;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets whether singly selected documents (one and only one document that contains
        /// all of the currently selected pages) should be indicated with a blue background.
        /// </summary>
        /// <value><see langword="true"/> if singly selected documents should be indicated with a
        /// blue background; otherwise, <see langword="false"/>.
        /// </value>
        public bool HighlightSinglySelectedDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether there are any uncommitted changes to document pagination.
        /// </summary>
        /// <value><see langword="true"/> if there are uncommitted changes to document pagination;
        /// otherwise, <see langword="false"/>.
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool PendingChanges
        {
            get
            {
                try
                {
                    // If in the process of committing changes, don't consider changes as pending.
                    if (_commitingChanges)
                    {
                        return false;
                    }
                    // If a PendingChanges status has been manually applied, use that value rather
                    // than calculating whether there have been changes.
                    else if (_pendingChangesOverride.HasValue)
                    {
                        return _pendingChangesOverride.Value;
                    }
                    else
                    {
                        return (_pendingDocuments != null) &&
                            _pendingDocuments.Any(document => !document.InOriginalForm);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39676");
                }
            }

            set
            {
                // Allow the calculated value of PendingChanges to be overridden.
                _pendingChangesOverride = value;
            }
        }

        /// <summary>
        /// Gets the filename of all source documents currently active in the UI in the order they
        /// appeared before any user edits.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public ReadOnlyCollection<string> SourceDocuments
        {
            get
            {
                try
                {
                    return _sourceDocuments
                        .Select(doc => doc.FileName)
                        .ToList()
                        .AsReadOnly();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39596");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the primarily selected <see cref="PageThumbnailControl"/> should
        /// update the displayed page in the <see cref="ImageViewer"/>.
        /// </summary>
        /// <value><see langword="true"/> if the primarily selected page should update the displayed
        /// page in the <see cref="ImageViewer"/>; otherwise, <see langword="false"/>.</value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool EnablePageDisplay
        {
            get
            {
                return _primaryPageLayoutControl.EnablePageDisplay;
            }

            set
            {
                _primaryPageLayoutControl.EnablePageDisplay = value;
            }
        }

        /// <summary>
        /// Gets whether pagination has been suggested for the specified
        /// <see paramref="sourceFileName"/>.
        /// </summary>
        /// <param name="sourceFileName">Name of the source file to which this query relates.</param>
        /// <returns><see langword="true"/> if has been suggested; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsPaginationSuggested(string sourceFileName)
        {
            try
            {
                return _primaryPageLayoutControl.Documents
                    .Cast<ExtendedOutputDocument>()
                    .Where(doc => doc.PageControls.Any(
                        c => c.Page.OriginalDocumentName == sourceFileName))
                    .Any(doc => doc.PaginationSuggested);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39665");
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Loads the specified documents pages into the pane.
        /// </summary>
        /// <param name="fileName">Name of the file to load.</param>
        /// <param name="loadAtFront"><see langword="true"/> to insert the pages at the front (top);
        /// <see langword="false"/> to add them at then end (bottom).</param>
        public void LoadFile(string fileName, bool loadAtFront)
        {
            LoadFile(fileName, loadAtFront, null, false, null);
        }

        /// <summary>
        /// Loads the specified documents pages into the pane.
        /// </summary>
        /// <param name="fileName">Name of the file to load.</param>
        /// <param name="loadAtFront"><see langword="true"/> to insert the pages at the front (top);
        /// <see langword="false"/> to add them at then end (bottom).</param>
        /// <param name="pages">The page numbers from <see paramref="fileName"/> to be loaded or
        /// <see langword="null"/> to load all pages.</param>
        /// <param name="paginationSuggested"><see langword="true"/> if pagination has been
        /// suggested for this document; <see langword="false"/> if it has not been.</param>
        /// <param name="documentData">The VOA file data associated with <see paramref="fileName"/>.
        /// </param>
        public void LoadFile(string fileName, bool loadAtFront, IEnumerable<int> pages,
            bool paginationSuggested, object documentData)
        {
            try
            {
                FormsMethods.ExecuteInUIThread(this, () =>
                {
                    // The ExtendedOutputDocument doesn't get created directly by this method. Use
                    // _documentData to be able to pass this info to when it is needed in
                    // IPaginationUtility.CreateOutputDocument.
                    if (paginationSuggested || documentData != null)
                    {
                        _documentData[fileName] = new Tuple<bool, object>(paginationSuggested, documentData);
                    }

                    var sourceDocument = OpenDocument(fileName);

                    if (sourceDocument != null)
                    {
                        // This will call Application.DoEvents in the midst of loading a document to
                        // keep the UI responsive as pages are loaded. This allows an opportunity
                        // for there to be multiple calls into LoadNextDocument at the same time.
                        var outputDocument = 
                            (ExtendedOutputDocument)_primaryPageLayoutControl.CreateOutputDocument(
                                sourceDocument, pages, loadAtFront, true);

                        _originalDocuments.Add(outputDocument);
                        var setOutputDocs = _sourceToOriginalDocuments.GetOrAdd(
                            sourceDocument, () => new HashSet<ExtendedOutputDocument>());
                        setOutputDocs.Add(outputDocument);
                    }

                    ApplyOrderOfLoadedSourceDocuments();
                });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39555");
            }
            finally
            {
                _documentData.Remove(fileName);
            }
        }

        /// <summary>
        /// Removes any pages or pending output documents associated with the specified
        /// <see paramref="fileName"/> from the panel.
        /// </summary>
        /// <param name="fileName">The name of the file for which pages should be removed.</param>
        public void RemoveSourceFile(string fileName)
        {
            try
            {
                FormsMethods.ExecuteInUIThread(this, () =>
                {
                    lock (_sourceDocumentLock)
                    {
                        var sourceDocument = _sourceDocuments.Single(doc => doc.FileName == fileName);

                        var documentsToDelete = _pendingDocuments
                            .Where(doc =>
                                doc.PageControls.Any(c =>
                                    c.Page.SourceDocument == sourceDocument))
                            .ToArray();

                        foreach (var outputDocument in documentsToDelete)
                        {
                            _primaryPageLayoutControl.DeleteOutputDocument(outputDocument);
                            _pendingDocuments.Remove(outputDocument);
                        }

                        var referencedOriginalDocuments = _originalDocuments
                            .Where(doc => doc.OriginalPages.Any(page => page.OriginalDocumentName == fileName))
                            .ToArray();

                        foreach (var outputDocument in referencedOriginalDocuments)
                        {
                            _originalDocuments.Remove(outputDocument);
                        }

                        _sourceDocuments.Remove(sourceDocument);
                        _sourceToOriginalDocuments.Remove(sourceDocument);
                    }

                    ApplyOrderOfLoadedSourceDocuments();
                });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39556");
            }
        }

        /// <summary>
        /// Selects the first <see cref="PageThumbnailControl"/>.
        /// </summary>
        public void SelectFirstPage()
        {
            try
            {
                _primaryPageLayoutControl.SelectFirstPage();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39666");
            }
        }

        #endregion Methods

        #region IPaginationUtility

        /// <summary>
        /// Creates a new <see cref="OutputDocument"/> based on the specified
        /// <see paramref="originalDocName"/>.
        /// </summary>
        /// <param name="originalDocName">The name that should be used as the basis for the new
        /// document's filename.</param>
        /// <returns>
        /// The new <see cref="OutputDocument"/>.
        /// </returns>
        OutputDocument IPaginationUtility.CreateOutputDocument(string originalDocName)
        {
            try
            {
                string outputDocumentName = GenerateOutputDocumentName(originalDocName);
                ExtendedOutputDocument outputDocument = new ExtendedOutputDocument(
                    outputDocumentName, HighlightSinglySelectedDocument);
                outputDocument.DocumentOutputting += HandleOutputDocument_DocumentOutputting;
                outputDocument.DocumentOutput += HandleOutputDocument_DocumentOutput;

                Tuple<bool, object> documentData;
                if (_documentData.TryGetValue(originalDocName, out documentData))
                {
                    outputDocument.PaginationSuggested = documentData.Item1;
                    outputDocument.DocumentData = documentData.Item2;
                }

                _pendingDocuments.Add(outputDocument);

                return outputDocument;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39558");
            }
        }

        /// <summary>
        /// Generates a new name an <see cref="OutputDocument"/> based on the specified
        /// <see paramref="originalDocName"/> that is unique compared to any pending output
        /// document, any existing document or any document that has been output by this instance
        /// whether or not the file still exists.
        /// </summary>
        /// <param name="originalDocName">The filename that should serve as the basis for the new
        /// document name.</param>
        /// <returns>The unique document name.</returns>
        public string GenerateOutputDocumentName(string originalDocName)
        {
            try
            {
                string baseFilename = originalDocName;
                string directory = Path.GetDirectoryName(baseFilename);
                string extension = Path.GetExtension(baseFilename);
                baseFilename = Path.GetFileNameWithoutExtension(baseFilename);

                return Path.Combine(directory, baseFilename + "_" +
                    UtilityMethods.GetRandomString(10, true, false, true) +
                    extension);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39559");
            }
        }

        /// <summary>
        /// Gets the <see cref="Page"/> instance(s) that represent the page(s) of the specified
        /// <see paramref="fileName"/>.
        /// </summary>
        /// <param name="fileName">The image file for which <see cref="Page"/> instance(s) should be
        /// retrieved.</param>
        /// <param name="pageNumber">If <see langword="null"/>, all pages will be retrieved;
        /// otherwise only the specified page will be retrieved.</param>
        /// <returns>The <see cref="Page"/> instance(s) that represent the page(s) of the specified
        /// <see paramref="fileName"/>.</returns>
        IEnumerable<Page> IPaginationUtility.GetDocumentPages(string fileName, int? pageNumber)
        {
            // See if the document indicated is already open as a SourceDocument.
            SourceDocument document = null;

            lock (_sourceDocumentLock)
            {
                document = _sourceDocuments
                    .Where(doc => doc.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();
            }

            // If not, open it as a SourceDocument.
            if (document == null)
            {
                document = OpenDocument(fileName);
            }

            // If unable to open then document, don't throw an exception, just act as though
            // the data was not on the clipboard in the first place.
            if (document == null)
            {
                yield break;
            }

            ExtractException.Assert("ELI39560", "Cannot find source page(s).",
                document != null && (pageNumber == null || document.Pages.Count >= pageNumber),
                "Filename", fileName,
                "Page number", pageNumber);

            // Return the correct page from the SourceDocument.
            if (pageNumber != null)
            {
                yield return document.Pages[pageNumber.Value - 1];
            }
            else
            {
                foreach (Page page in document.Pages)
                {
                    yield return page;
                }
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a cut operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem CutMenuItem
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a copy operation or 
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem CopyMenuItem
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a insert copied data operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem InsertCopiedMenuItem
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a delete operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem DeleteMenuItem
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a print operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem PrintMenuItem
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to toggle a document separator ahead of the
        /// currently selected page or <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem ToggleDocumentSeparatorMenuItem
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripButton"/> intended to trigger output the currently selected
        /// document(s) or <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripButton OutputDocumentToolStripButton
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger output the currently selected
        /// document(s) or <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripMenuItem OutputSelectedDocumentsMenuItem
        {
            get
            {
                return null;
            }
        }

        #endregion IPaginationUtility

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the <see cref="ImageViewer"/> this instance should use.
        /// </summary>
        /// <value>The <see cref="ImageViewer"/> this instance should use.</value>
        [Browsable(false)]
        public ImageViewer ImageViewer
        {
            get
            {
                return _imageViewer;
            }
            set
            {
                _imageViewer = value;
            }
        }

        #endregion IImageViewerControl Members

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _imageViewer.EstablishConnections(this);

                ResetPrimaryPageLayoutControl();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39561");
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
                if (_shortcuts.ProcessKey(keyData))
                {
                    return true;
                }

                // This key was not processed, bubble it up to the base class.
                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39562", ex);
                return false;
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="OutputDocument.DocumentOutputting"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance
        /// containing the event data.</param>
        void HandleOutputDocument_DocumentOutputting(object sender, CancelEventArgs e)
        {
            try
            {
                var outputDocument = (ExtendedOutputDocument)sender;

                // Don't output the document to its final location at first since a running file
                // supplier would be likely to queue it before this process has a chance to grab it.
                // Only after the file has been added to the DB and checked out for processing by
                // this instance will it be moved to its final location.
                var tempFile = new TemporaryFile(true);
                _tempFiles[outputDocument] = tempFile;

                outputDocument.FileName = tempFile.FileName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39564");
            }
        }

        /// <summary>
        /// Handles the <see cref="OutputDocument.DocumentOutput"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOutputDocument_DocumentOutput(object sender, EventArgs e)
        {
            ExtendedOutputDocument outputDocument = null;
            TemporaryFile tempFile = null;

            try
            {
                // The file has been output at this point according to the PageLayoutControl, but
                // this is actually only to a temporary in order to give the owner of this panel the
                // opportunity to determine where it is going before the file is written to the
                // final location.
                outputDocument = (ExtendedOutputDocument)sender;
                tempFile = _tempFiles[outputDocument];

                var sourceDocNames = outputDocument.PageControls
                    .Select(c => c.Page.SourceDocument.FileName)
                    .Distinct();

                int pageCount = 0;
                using (var codecs = new ImageCodecs())
                using (var reader = codecs.CreateReader(tempFile.FileName))
                {
                    pageCount = reader.PageCount;
                }
                long fileSize = new FileInfo(tempFile.FileName).Length;

                bool? suggestedPaginationAccepted = outputDocument.PaginationSuggested
                    ? (bool ?)(outputDocument.InOriginalForm ? true : false)
                    : null;

                var eventArgs = new CreatingOutputDocumentEventArgs(
                    sourceDocNames, pageCount, fileSize, suggestedPaginationAccepted);
                OnCreatingOutputDocument(eventArgs);
                string outputFileName = eventArgs.OutputFileName;

                ExtractException.Assert("ELI39588",
                    "No filename has been specified for pagination output.",
                    !string.IsNullOrWhiteSpace(outputFileName));

                File.Copy(tempFile.FileName, outputFileName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39565");
            }
            finally
            {
                if (tempFile != null)
                {
                    _tempFiles.Remove(outputDocument);
                    tempFile.Dispose();
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_applyToolStripButton"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleApplyToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                CommitPendingChanges();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39566");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_revertToOriginalToolStripButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRevertToOriginalToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                RevertPendingChanges(revertToSource: false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39664");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_revertToSourceToolStripButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRevertToSourceToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                RevertPendingChanges(revertToSource: true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39567");
            }
        }

        /// <summary>
        /// Handles the <see cref="PageLayoutControl.StateChanged"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePageLayoutControl_StateChanged(object sender, EventArgs e)
        {
            try
            {
                _revertToOriginalToolStripButton.Enabled =
                    _pendingDocuments.Any(doc => doc.PaginationSuggested && !doc.InOriginalForm);
                _revertToSourceToolStripButton.Enabled =
                    _pendingDocuments.Any(doc => !doc.InSourceDocForm);
                _applyToolStripButton.Enabled =
                    PendingChanges || // Value can be overridden; need to enabled apply whenever true.
                    _revertToOriginalToolStripButton.Enabled ||
                    _revertToSourceToolStripButton.Enabled;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39568");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Clears and disposes of any existing <see cref="_primaryPageLayoutControl"/> and
        /// initializes a new one.
        /// </summary>
        void ResetPrimaryPageLayoutControl()
        {
            if (_primaryPageLayoutControl != null)
            {
                _primaryPageLayoutControl.StateChanged -= HandlePageLayoutControl_StateChanged;
//                _primaryPageLayoutControl.PageDeleted -= HandlePageLayoutControl_PageDeleted;
//                _primaryPageLayoutControl.PagesPendingLoad -= HandlePageLayoutControl_PagesPendingLoad;
//                _primaryPageLayoutControl.LoadNextDocumentRequest -= HandleLayoutControl_LoadNextDocumentRequest;
                // If the control contains a lot of pagination controls, it can take a long time to
                // remove-- I am unclear why (it has to do with more than just whether thumbnails
                // are still being loaded. Disposing of the control first allows it to be quickly
                // removed.
                _primaryPageLayoutControl.Dispose();
                _toolStripContainer.ContentPanel.Controls.Remove(_primaryPageLayoutControl);

                // Perform a GC to force a cleanup of everything before loading the new
                // _primaryPageLayoutControl
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            _primaryPageLayoutControl = new PageLayoutControl(this);
            _primaryPageLayoutControl.Shortcuts = _shortcuts;
            _primaryPageLayoutControl.Dock = DockStyle.Fill;
            _primaryPageLayoutControl.ImageViewer = _imageViewer;
            _primaryPageLayoutControl.StateChanged += HandlePageLayoutControl_StateChanged;
//            _primaryPageLayoutControl.PageDeleted += HandlePageLayoutControl_PageDeleted;
            //_primaryPageLayoutControl.PagesPendingLoad += HandlePageLayoutControl_PagesPendingLoad;
            //_primaryPageLayoutControl.LoadNextDocumentRequest += HandleLayoutControl_LoadNextDocumentRequest;
            _toolStripContainer.ContentPanel.Controls.Add(_primaryPageLayoutControl);
            _primaryPageLayoutControl.Focus();
        }

        /// <summary>
        /// Opens the specified <see paramref="inputFileName"/> as a <see cref="SourceDocument"/>
        /// instance.
        /// </summary>
        /// <param name="inputFileName">Name of the input file.</param>
        /// <returns>A <see cref="SourceDocument"/> representing <see paramref="inputFileName"/> or
        /// <see langword="null"/> if the file is missing or could not be opened.</returns>
        SourceDocument OpenDocument(string inputFileName)
        {
            SourceDocument sourceDocument = null;

            lock (_sourceDocumentLock)
            {
                if (!File.Exists(inputFileName))
                {
                    return null;
                }

                sourceDocument = _sourceDocuments.SingleOrDefault(doc =>
                    doc.FileName.Equals(inputFileName, StringComparison.OrdinalIgnoreCase));

                if (sourceDocument == null)
                {
                    sourceDocument = new SourceDocument(inputFileName);
                    if (!sourceDocument.Pages.Any())
                    {
                        sourceDocument.Dispose();
                        return null;
                    }

                    _sourceDocuments.Add(sourceDocument);
                }
            }

            ThreadingMethods.RunInBackgroundThread("ELI39570", () =>
            {
                lock (_sourceDocumentLock)
                {
                    if (_sourceDocuments.Contains(sourceDocument))
                    {
                        _imageViewer.CacheImage(inputFileName);
                    }
                }
            });

            return sourceDocument;
        }

        /// <summary>
        /// Commits any pending document pagination changes.
        /// </summary>
        /// <returns><see langword="true"/> if the currently processing file needs to be released
        /// following the next call to delay file; <see langword="false"/> if the currently
        /// processing file should remain in processing.</returns>
        void CommitPendingChanges()
        {
            try
            {
                _commitingChanges = true;

                _outputDocumentNames.Clear();

                // Depending upon the manipulations that occurred, there may be some pending
                // documents that have been left without any pages. Disregard these.
                _pendingDocuments.RemoveWhere(doc => !doc.PageControls.Any());

                var outputDocuments = _pendingDocuments
                    .Where(document => !document.InSourceDocForm)
                    .ToList();

                var sourceDocuments =
                    outputDocuments.SelectMany(doc => doc.PageControls)
                        .Select(c => c.Page.SourceDocument)
                        .Distinct()
                        .ToArray();

                // Generate the paginated output. _outputDocumentNames will maintain the names files
                // that have been output.
                foreach (var outputDocument in outputDocuments)
                {
                    outputDocument.Output();
                    _pendingDocuments.Remove(outputDocument);
                }

                var disregardedPagination = _pendingDocuments
                    .Where(doc => doc.InSourceDocForm &&
                        doc.PaginationSuggested &&
                        !doc.InOriginalForm)
                    .Select(doc => doc.PageControls.First().Page.OriginalDocumentName);

                OnPaginated(sourceDocuments.Select(doc => doc.FileName), disregardedPagination);

                foreach (var document in _pendingDocuments)
                {
                    document.PaginationSuggested = false;
                }

                ApplyOrderOfLoadedSourceDocuments();

                _pendingChangesOverride = null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39571");
            }
            finally
            {
                _commitingChanges = false;
            }
        }

        /// <summary>
        /// Reverts any uncommitted document pagination changes.
        /// </summary>
        /// <param name="revertToSource"><see langword="true"/> to revert to the state of the source
        /// documents on disk; <see langword="false"/> to revert to the state that was originally
        /// presented (will differ from source state if pagination was suggested).</param>
        void RevertPendingChanges(bool revertToSource)
        {
            _primaryPageLayoutControl.ClearSelection();

            foreach (var outputDocument in _pendingDocuments.ToArray())
            {
                _primaryPageLayoutControl.DeleteOutputDocument(outputDocument);
                _pendingDocuments.Remove(outputDocument);
            }

            SourceDocument[] sourceDocArray;
            lock (_sourceDocumentLock)
            {
                sourceDocArray = _sourceDocuments.ToArray();
            }

            if (revertToSource)
            {
                for (int i = 0; i < sourceDocArray.Length; i++)
                {
                    var sourceDocument = sourceDocArray[i];
                    var outputDocument = _sourceToOriginalDocuments[sourceDocument]
                        .OrderBy(doc => doc.OriginalPages.Select(page => page.OriginalPageNumber).Min())
                        .First();
                    _primaryPageLayoutControl.LoadOutputDocument(
                        outputDocument, sourceDocument.Pages);
                    _pendingDocuments.Add(outputDocument);
                }
            }
            else
            {
                foreach (var outputDocument in sourceDocArray
                    .SelectMany(source => _sourceToOriginalDocuments[source]
                        .OrderBy(doc => doc.OriginalPages.Select(page => page.OriginalPageNumber).Min())))
                {
                    _primaryPageLayoutControl.LoadOutputDocument(
                        outputDocument, outputDocument.OriginalPages);
                    _pendingDocuments.Add(outputDocument);
                }
            }

            _primaryPageLayoutControl.SelectFirstPage();
            _primaryPageLayoutControl.Focus();
        }

        /// <summary>
        /// Orders _sourceDocuments so that it matches the order the corresponding pages currently
        /// appear in the panel.
        /// </summary>
        void ApplyOrderOfLoadedSourceDocuments()
        {
            lock (_sourceDocumentLock)
            {
                // Get the order of the first appearance of SourceDocuments in the pane.
                var orderedFileNames =
                    _primaryPageLayoutControl
                    .Documents
                    .SelectMany(doc => doc.PageControls)
                    .Select(pageControl => pageControl.Page.SourceDocument)
                    .Distinct()
                    .ToList();

                _sourceDocuments = _sourceDocuments
                    .OrderBy(doc => orderedFileNames.IndexOf(doc))
                    .ToList();
            }
        }

        /// <summary>
        /// Raises the <see cref="CreatingOutputDocument"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="CreatingOutputDocumentEventArgs"/> instance to
        /// use when raising the event.</param>
        void OnCreatingOutputDocument(CreatingOutputDocumentEventArgs eventArgs)
        {
            var eventHandler = CreatingOutputDocument;
            if (eventHandler != null)
            {
                eventHandler(this, eventArgs);
            }
        }

        /// <summary>
        /// Raises the <see cref="Paginated"/> event.
        /// </summary>
        /// <param name="paginatedDocumentSources">The source documents that were used to generate
        /// the paginated output. These documents will no longer be referenced by the
        /// <see cref="PaginationPanel"/>.</param>
        /// <param name="disregardedPaginationSources">All documents applied as they exist on disk
        /// but for which there was differing suggested pagination.</param>
        void OnPaginated(IEnumerable<string> paginatedDocumentSources,
            IEnumerable<string> disregardedPaginationSources)
        {
            var eventHandler = Paginated;
            if (eventHandler != null)
            {
                eventHandler(this, new PaginatedEventArgs(paginatedDocumentSources, disregardedPaginationSources));
            }
        }

        #endregion Private Members
    }
}
