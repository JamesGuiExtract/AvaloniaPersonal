using Extract.Interfaces;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Extract.UtilityApplications.PaginationUtility.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PaginationUtilityForm : Form
    {
        [Serializable]
        class ClipboardData
        {
            /// <summary>
            /// 
            /// </summary>
            List<Tuple<string, int>> _pageData;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pages"></param>
            public ClipboardData(IEnumerable<Page> pages)
            {
                _pageData = new List<Tuple<string, int>>(
                    pages.Select(page => (page == null)
                        ? null
                        : new Tuple<string, int>(page.OriginalDocumentName, page.OriginalPageNumber)));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="paginationUtility"></param>
            public IEnumerable<Page> GetPages(PaginationUtilityForm paginationUtility)
            {
                foreach(Tuple<string, int> pageData in _pageData)
                {
                    if (pageData == null)
                    {
                        yield return null;
                    }

                    string fileName = pageData.Item1;
                    int pageNumber = pageData.Item2;

                    SourceDocument document = paginationUtility._sourceDocuments
                        .Where(doc =>
                            doc.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                        .SingleOrDefault();

                    if (document == null)
                    {
                        document = paginationUtility.OpenDocument(fileName);
                    }

                    ExtractException.Assert("ELI35506", "Cannot find source page.",
                        document != null && document.Pages.Count >= pageNumber,
                        "Filename", fileName,
                        "Page number", pageNumber);

                    yield return document.Pages[pageNumber - 1];
                }
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
            /// <returns>
            ///   <see langword="true"/> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <see langword="false"/>.
            /// </returns>
            public override bool Equals(object obj)
            {
                try
                {
                    var clipboardData = obj as ClipboardData;
                    if (clipboardData == null)
                    {
                        return false;
                    }

                    var pageData = clipboardData._pageData;
                    if (pageData.Count != _pageData.Count)
                    {
                        return false;
                    }

                    for (int i = 0; i < _pageData.Count; i++)
                    {
                        if ((pageData[i] == null) != (_pageData[i] == null))
                        {
                            return false;
                        }

                        if (pageData[i] != null)
                        {
                            if (!pageData[i].Item1.Equals(_pageData[i].Item1))
                            {
                                return false;
                            }

                            if (!pageData[i].Item2.Equals(_pageData[i].Item2))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35507");
                }
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                int hashCode = base.GetHashCode();

                try
                {
                    hashCode = hashCode ^ _pageData.GetHashCode();

                    for (int i = 0; i < _pageData.Count; i++)
                    {
                        if (_pageData[i] != null)
                        {
                            hashCode ^= _pageData[i].Item1.GetHashCode();
                            hashCode ^= _pageData[i].Item2.GetHashCode();
                        }
                    }

                    return hashCode;
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI35508");
                }

                return hashCode;
            }
        }

        #region Constants

        /// <summary>
        /// The name of this object.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(PaginationUtilityForm).ToString();

//        /// <summary>
//        /// The license string for the SandDock manager
//        /// </summary>
//        static readonly string _SANDDOCK_LICENSE_STRING = @"1970|siE7SnF/jzINQg1AOTIaCXLlouA=";

        /// <summary>
        /// The full path to the file that contains information about persisting the 
        /// <see cref="PaginationUtilityForm"/>.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_FILE = FileSystemMethods.PathCombine(
            FileSystemMethods.ApplicationDataPath, "PaginationUtility", "PaginationUtility.xml");

        /// <summary>
        /// Name for the mutex used to serialize persistance of the control and form layout.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_MUTEX_STRING =
            "97BC14EC-DEED-4826-AE47-6CD1CC502AAB";

        /// <summary>
        /// The number of times clipboard copy operations should be attempted before giving up.
        /// </summary>
        internal static readonly int _CLIPBOARD_RETRY_COUNT = 10;

        /// <summary>
        /// The number of seconds to allow back-up clipboard data to be used.
        /// </summary>
        internal static readonly int _SECONDS_TO_ALLOW_CLIPBOARD_BACKUP = 30;

        /// <summary>
        /// 
        /// </summary>
        static readonly string _CLIPBOARD_DATA_FORMAT = "ExtractPaginationClipboardDataFormat";

        #endregion Constants

        #region Fields

        /// <summary>
        /// 
        /// </summary>
        bool _usingPredeterminedSettings;

        /// <summary>
        /// 
        /// </summary>
        ConfigSettings<Settings> _config;

        /// <summary>
        /// 
        /// </summary>
        IEnumerator<string> _inputFileEnumerator;

        /// <summary>
        /// 
        /// </summary>
        FileSystemWatcher _inputFolderWatcher;

        /// <summary>
        /// 
        /// </summary>
        FileFilter _fileFilter;

        /// <summary>
        /// Saves/restores window state info
        /// </summary>
        FormStateManager _formStateManager;

//        /// <summary>
//        /// 
//        /// </summary>
//        TabbedDocument _primaryWorkAreaTab;

        /// <summary>
        /// 
        /// </summary>
        PageLayoutControl _primaryPageLayoutControl;

        /// <summary>
        /// 
        /// </summary>
        HashSet<SourceDocument> _sourceDocuments = new HashSet<SourceDocument>();

        /// <summary>
        /// 
        /// </summary>
        HashSet<OutputDocument> _pendingDocuments = new HashSet<OutputDocument>();

        /// <summary>
        /// 
        /// </summary>
        HashSet<string> _outputDocumentNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 
        /// </summary>
        HashSet<string> _processedDocumentNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 
        /// </summary>
        bool _pageLoadPending;

        /// <summary>
        /// 
        /// </summary>
        string _lockFileName;

        /// <summary>
        /// 
        /// </summary>
        FileStream _lockFile;

        /// <summary>
        /// A backup copy of the last data put on the clipboard.
        /// </summary>
        IDataObject _lastClipboardData = null;

        /// <summary>
        /// The time <see cref="_lastClipboardData"/> was placed on the clipboard.
        /// </summary>
        DateTime _lastClipboardCopyTime = new DateTime();

        /// <summary>
        /// 
        /// </summary>
        bool _closing;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationUtilityForm"/> class.
        /// </summary>
        public PaginationUtilityForm(string configurationFileName = null)
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI35509", _OBJECT_NAME);

                // License SandDock before creating the form
//                SandDockManager.ActivateProduct(_SANDDOCK_LICENSE_STRING);

                _usingPredeterminedSettings = !string.IsNullOrEmpty(configurationFileName);

                _config = new ConfigSettings<Settings>(configurationFileName, false,
                    !_usingPredeterminedSettings);

                InitializeComponent();

                if (!_inDesignMode)
                {
                    // Loads/save UI state properties
                    _formStateManager = new FormStateManager(this, _FORM_PERSISTENCE_FILE,
                        _FORM_PERSISTENCE_MUTEX_STRING, _sandDockManager,
                        false, "Exit full screen (F11)");
                }

                _imageViewer.CacheImages = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35510");
            }
        }

        #endregion Constructor

        #region Methods

        /// <overrides>
        /// 
        /// </overrides>
        /// <summary>
        /// Determines whether [is output file name available] [the specified file name].
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        ///   <see langword="true"/> if [is output file name available] [the specified file name];
        ///  otherwise, <see langword="false"/>.
        /// </returns>
        internal bool IsOutputFileNameAvailable(string fileName)
        {
            return IsOutputFileNameAvailable(fileName, null);
        }

        /// <summary>
        /// Determines whether [is output file name available] [the specified file name].
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="subjectDocument"></param>
        /// <returns>
        ///   <see langword="true"/> if [is output file name available] [the specified file name];
        ///  otherwise, <see langword="false"/>.
        /// </returns>
        internal bool IsOutputFileNameAvailable(string fileName, OutputDocument subjectDocument)
        {
            try
            {
                if (_outputDocumentNames.Contains(fileName))
                {
                    return false;
                }

                if (_pendingDocuments.Any(document => document != subjectDocument &&
                    document.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                if (File.Exists(fileName))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35511");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        ///   <see langword="true"/> if [is output file name available] [the specified file name];
        ///  otherwise, <see langword="false"/>.
        /// </returns>
        internal bool IsProcessedFileNameAvailable(string fileName)
        {
            try
            {
                if (_processedDocumentNames.Contains(fileName))
                {
                    return false;
                }

                if (File.Exists(fileName))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35512");
            }
        }

        /// <summary>
        /// Generates the name of the output document.
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <returns></returns>
        internal OutputDocument CreateOutputDocument(string inputFileName)
        {
            string outputDocumentName = GenerateOutputDocumentName(inputFileName);
            OutputDocument outputDocument = new OutputDocument(outputDocumentName);
            outputDocument.PageRemoved += HandleOutputDocument_PageRemoved;
            outputDocument.DocumentOutput += HandleOutputDocument_DocumentOutput;
            _pendingDocuments.Add(outputDocument);

            return outputDocument;
        }

        /// <summary>
        /// Places the specified <see paramref="data"/> of the specified <see paramref="format"/>
        /// to the clipboard with retries. A backup copy is kept for use by the pagination utility
        /// even if all attempts fail.
        /// </summary>
        /// <param name="pages">The data.</param>
        internal void SetClipboardData(IEnumerable<Page> pages)
        {
            try
            {
                using (new TemporaryWaitCursor())
                {
                    Exception clipboardException = null;

                    // Register custom data format or get it if it's already registered
                    DataFormats.Format format = DataFormats.GetFormat(_CLIPBOARD_DATA_FORMAT);

                    var clipboardData = new ClipboardData(pages);

                    // Apply the data to _lastClipboardData which can be used internally even if the
                    // the data isn't successfully placed on the clipboard.
                    _lastClipboardData = new DataObject();
                    _lastClipboardData.SetData(format.Name, false, clipboardData);

                    // Retry loop in case copy or clipboard data validation fail.
                    for (int i = 0; i < _CLIPBOARD_RETRY_COUNT; i++)
                    {
                        try
                        {
                            // Techinically un-necessary, but the clipboard is being flakey, so
                            // maybe this will help?
                            if (i > 0)
                            {
                                Clipboard.Clear();

                                System.Threading.Thread.Sleep(200);
                            }

                            // Keep track of the time the data was copied so that we don't allow
                            // a stale backup copy to be used later on.
                            _lastClipboardCopyTime = DateTime.Now;
                            Clipboard.SetDataObject(_lastClipboardData, false);

                            IDataObject dataObject = Clipboard.GetDataObject();
                            var validationClipboardData = dataObject.GetData(format.Name) as ClipboardData;

                            // Validate the data was placed on the clipboard under either of the two
                            // data types currently used by the PaginationUtilityApplication.
                            ExtractException.Assert("ELI35513", "Clipboard data failed validation.",
                                clipboardData.Equals(validationClipboardData));

                            foreach (Page page in pages)
                            {
                                page.AddReference(this);
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
                        throw clipboardException;
                    }
                }
            }
            catch (Exception ex)
            {
                // Since we have a backup copy, don't throw an exception right now.
                var ee = new ExtractException("ELI35514", "Failed to copy data to clipboard.", ex);
                ee.Log();
            }
        }

        /// <summary>
        /// Retrieves into <see paramref="data"/> data from the clipboard of the specified
        /// <see paramref="format"/>. Will attempt to use a backup copy if the data on the clipboard
        /// appears invalid.
        /// </summary>
        /// <returns>An <see cref="IDataObject"/> that represents the data currently on the
        /// clipboard, or <see langword="null"/> if there is no data on the clipboard.</returns>
//        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        internal IEnumerable<Page> GetClipboardData()
        {
            try
            {
                IDataObject data = null;

                try
                {
                    RemoveStaleClipboardBackup(_SECONDS_TO_ALLOW_CLIPBOARD_BACKUP);

                    data = Clipboard.GetDataObject();

                    if (_lastClipboardData != null)
                    {
                        // Get the format; preferably a format shared with _lastClipboardData,
                        // if possible.
                        string format = data.GetFormats().Intersect(
                            _lastClipboardData.GetFormats()).FirstOrDefault()
                            ?? data.GetFormats().FirstOrDefault();

                        // If the data on the clipboard matches the backup clipboard data copy,
                        // don't bother using the real clipboard data which can be flakey; just
                        // use the backup copy.
                        if (!string.IsNullOrEmpty(format) && _lastClipboardData != null &&
                            _lastClipboardData.GetDataPresent(format))
                        {
                            data = _lastClipboardData;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Don't let any attempt at being "smart" about handling the clipboard data
                    // prevent returning the data we already have; just log in this case.
                    ex.ExtractLog("ELI35515");

                    if (data == null)
                    {
                        data = _lastClipboardData;
                    }
                }

                if (data != null)
                {
                    var clipboardData = data.GetData(_CLIPBOARD_DATA_FORMAT) as ClipboardData;
                    if (clipboardData != null)
                    {
                        return clipboardData.GetPages(this);
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
        /// Clipboards the has data.
        /// </summary>
        /// <returns></returns>
        internal bool ClipboardHasData()
        {
            try
            {
                IEnumerable<Page> clipboardData = GetClipboardData();

                return (clipboardData != null && clipboardData.Any());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35517");
            }
        }

        #endregion Methods

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

                // Establish image viewer connections prior to calling base.OnLoad which will
                // potentially remove some IImageViewerControls.
                _imageViewer.EstablishConnections(this);

//                _sandDockManager.DockSystemContainer = _pageLayoutToolStripContainer.ContentPanel;
//                _primaryWorkAreaTab = new TabbedDocument();
//                _primaryWorkAreaTab.AllowClose = false;
//                _primaryWorkAreaTab.Manager = _sandDockManager;
//                _primaryWorkAreaTab.Text = "Input";
//                _primaryWorkAreaTab.OpenDocument(WindowOpenMethod.OnScreen);

                _primaryPageLayoutControl = new PageLayoutControl(this);
                //_primaryPageLayoutControl.Enabled = true;
                _primaryPageLayoutControl.Dock = DockStyle.Fill;
                _primaryPageLayoutControl.ImageViewer = _imageViewer;
                _primaryPageLayoutControl.StateChanged += HandlePageLayoutControl_StateChanged;
                _pageLayoutToolStripContainer.ContentPanel.Controls.Add(_primaryPageLayoutControl);
                ActiveControl = _primaryPageLayoutControl;

                if (!_usingPredeterminedSettings)
                {
                    ShowSettingsDialog();
                }
                
                LoadMorePages();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35518");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"/> that contains
        /// the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                base.OnClosing(e);

                _closing = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35519");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Enter"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnEnter(EventArgs e)
        {
            try
            {
                base.OnEnter(e);

                ActiveControl = _primaryPageLayoutControl;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35520");
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (_formStateManager != null)
                    {
                        _formStateManager.Dispose();
                        _formStateManager = null;
                    }

                    if (components != null)
                    {
                        components.Dispose();
                    }

                    if (_lockFile != null)
                    {
                        _lockFile.Dispose();

                        try
                        {
                            FileSystemMethods.DeleteFileNoRetry(_lockFileName);
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractLog("ELI35521");
                        }

                        _lockFile = null;
                    }
                }
                catch (System.Exception ex)
                {
                    ex.ExtractLog("ELI35522");
                }
            }
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the HandleSettingsMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleSettingsMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ShowSettingsDialog();

                LoadMorePages();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35523");
            }
        }

        /// <summary>
        /// Handles the Click event of the HandleRestartToolStripMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRestartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (PromptForRestart(false))
                {
                    Clear();

                    LoadMorePages();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35524");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_outputDocumentToolStripButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOutputDocumentToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                _primaryPageLayoutControl.OutputSelectedDocuments();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35525");
            }
        }

        /// <summary>
        /// Handles the Disposed event of the HandleSourceDocument control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleSourceDocument_Disposed(object sender, EventArgs e)
        {
            try
            {
                var sourceDocument = (SourceDocument)sender;
                if (!_sourceDocuments.Contains(sourceDocument))
                {
                    return;
                }

                if (sourceDocument.FileName.StartsWith(
                    _config.Settings.InputFolder, StringComparison.OrdinalIgnoreCase))
                {
                    _imageViewer.UnloadImage(sourceDocument.FileName);

                    if (_config.Settings.DeleteProcessedFiles)
                    {
                        FileSystemMethods.DeleteFile(sourceDocument.FileName);
                    }
                    else
                    {
                        string processedDocumentName =
                            GenerateProcessedDocumentName(sourceDocument.FileName);

                        string directory = Path.GetDirectoryName(processedDocumentName);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        FileSystemMethods.MoveFile(sourceDocument.FileName, processedDocumentName,
                            false);
                        _processedDocumentNames.Add(processedDocumentName);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35526");
            }
        }

        /// <summary>
        /// Handles the PageRemoved event of the HandleOutputDocument control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOutputDocument_PageRemoved(object sender, PageRemovedEventArgs e)
        {
            try
            {
                // Invoke this rather than run it directly since at the time of removal, the page
                // control will not yet have been disposed even if that is the intention.
                // By the time the next message is handled, the removed page control will have been
                // disposed if it was going to be, and the below code will delete/move the original
                // input document if appropriate.
                this.SafeBeginInvoke("ELI35527", () =>
                {
                    if (_closing)
                    {
                        return;
                    }

                    RemoveStaleClipboardBackup(_SECONDS_TO_ALLOW_CLIPBOARD_BACKUP);

                    SourceDocument documentToDispose = _sourceDocuments
                        .Where(document => document.Pages.Contains(e.Page) &&
                            document.Pages.All(page => !page.HasActiveReference))
                        .SingleOrDefault();

                    if (documentToDispose != null)
                    {
                        documentToDispose.Dispose();

                        _sourceDocuments.Remove(documentToDispose);
                    }

                    var outputDocument = (OutputDocument)sender;
                    if (outputDocument.PageControls.Count == 0)
                    {
                        outputDocument.PageRemoved -= HandleOutputDocument_PageRemoved;
                        outputDocument.DocumentOutput -= HandleOutputDocument_DocumentOutput;
                        _pendingDocuments.Remove(outputDocument);
                    }

                    if (e.Deleted)
                    {
                        InvokeLoadMorePages();
                    }
                });
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35528");
            }
        }

        /// <summary>
        /// Handles the DocumentOutput event of the HandleOutputDocument control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOutputDocument_DocumentOutput(object sender, EventArgs e)
        {
            try
            {
                var document = (OutputDocument)sender;

                _outputDocumentNames.Add(document.FileName);
                InvokeLoadMorePages();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35529");
            }
        }

        /// <summary>
        /// Handles the CommandStatesUpdated event of the HandlePageLayoutControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePageLayoutControl_StateChanged(object sender, EventArgs e)
        {
            try
            {
                _outputDocumentToolStripButton.Enabled =
                    _primaryPageLayoutControl.OutputDocumentCommandEnabled;

                if (_primaryPageLayoutControl.FullySelectedDocuments.Count() == 1)
                {
                    OutputDocument selectedDocument =
                        _primaryPageLayoutControl.FullySelectedDocuments.Single();

                    _outputFileNameToolStripTextBox.Text = selectedDocument.FileName;

                    if (_primaryPageLayoutControl.FullySelectedDocuments.Count() == 1)
                    {
                        _outputFileNameToolStripTextBox.Enabled = true;
                        if (selectedDocument.PageControls.Count == 1)
                        {
                            _pagesToolStripLabel.Text = "1 page";
                        }
                        else
                        {
                            _pagesToolStripLabel.Text =
                                string.Format(CultureInfo.CurrentCulture, "{0:D} pages",
                                    selectedDocument.PageControls.Count);
                        }
                    }
                    else
                    {
                        _outputFileNameToolStripTextBox.Enabled = false;

                        IEnumerable<PageThumbnailControl> selectedPageControls =
                            selectedDocument.PageControls.Where(pageControl => pageControl.Selected);

                        if (selectedPageControls.Count() == 1)
                        {
                            _pagesToolStripLabel.Text =
                                string.Format(CultureInfo.CurrentCulture, "Page {0:D}", 
                                    selectedPageControls.Single().PageNumber);
                        }
                        else
                        {
                            _pagesToolStripLabel.Text = "";
                        }
                    }
                }
                else
                {
                    _outputFileNameToolStripTextBox.Enabled = false;
                    _pagesToolStripLabel.Text = "";
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35530");
            }
        }

        /// <summary>
        /// Handles the Created event of the HandleInputFolderWatcher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.IO.FileSystemEventArgs"/> instance containing the event data.</param>
        void HandleInputFolderWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (_fileFilter == null || _fileFilter.FileMatchesFilter(e.FullPath))
                {
                    InvokeLoadMorePages();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35531");
            }
        }

        /// <summary>
        /// Handles the Renamed event of the HandleInputFolderWatcher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.IO.RenamedEventArgs"/> instance containing the event data.</param>
        void HandleInputFolderWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            try
            {
                if (_fileFilter == null || _fileFilter.FileMatchesFilter(e.FullPath))
                {
                    InvokeLoadMorePages();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35532");
            }
        }

        /// <summary>
        /// Handles the Validating event of the HandleOutputFileNameToolStripTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        void HandleOutputFileNameToolStripTextBox_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                OutputDocument selectedDocument =
                        _primaryPageLayoutControl.PartiallySelectedDocuments.Single();

                if (!IsOutputFileNameAvailable(_outputFileNameToolStripTextBox.Text, selectedDocument))
                {
                    UtilityMethods.ShowMessageBox("This output filename has already been used.",
                        "Filename Unavailable", true);
                    e.Cancel = true;
                }

                if (selectedDocument.FileName.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    UtilityMethods.ShowMessageBox("This output filename contains invalid char(s).",
                        "Filename Invalid.", true);
                    e.Cancel = true;
                }

                selectedDocument.FileName = _outputFileNameToolStripTextBox.Text;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35533");
            }
        }

        /// <summary>
        /// Handles the Click event of the HandleOutputFileNameBrowseToolStripButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOutputFileNameBrowseToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFile = new OpenFileDialog())
                {
                    string initialFile = _outputFileNameToolStripTextBox.Text;

                    try
                    {
                        openFile.InitialDirectory = Path.GetDirectoryName(initialFile);
                        openFile.FileName = Path.GetFileName(initialFile);
                    }
                    catch { }

                    openFile.AddExtension = false;
                    openFile.Multiselect = false;
                    openFile.CheckPathExists = true;
                    openFile.CheckFileExists = false;

                    // Show the dialog
                    while (openFile.ShowDialog() == DialogResult.OK)
                    {
                        if (!IsOutputFileNameAvailable(openFile.FileName))
                        {
                            UtilityMethods.ShowMessageBox(
                                "This output filename has already been used.",
                                "Filename Unavailable", true);

                            openFile.InitialDirectory = Path.GetDirectoryName(openFile.FileName);
                            openFile.FileName = Path.GetFileName(openFile.FileName);
                            continue;
                        }

                        // Return the selected file path.
                        _outputFileNameToolStripTextBox.Text = openFile.FileName;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35534", ex);
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Shows the settings dialog.
        /// </summary>
        void ShowSettingsDialog()
        {
            using (var paginationSettingsDialog = new PaginationSettingsDialog(_config))
            {
                string lastInputFolder = _config.Settings.InputFolder;
                string lastFilter = _config.Settings.FileFilter;
                bool lastIncludeSubFolders = _config.Settings.IncludeSubfolders;
                string lastOutputFolder = _config.Settings.OutputFolder;
                bool lastRandomizeOutputFileName = _config.Settings.RandomizeOutputFileName;
                bool preserveSubFoldersInOutput = _config.Settings.PreserveSubFoldersInOutput;

                paginationSettingsDialog.ReadOnly = _usingPredeterminedSettings;
                DialogResult dialogResult = paginationSettingsDialog.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    if (_inputFileEnumerator != null && !_usingPredeterminedSettings &&
                        (!lastInputFolder.Equals(_config.Settings.InputFolder, StringComparison.OrdinalIgnoreCase) ||
                         !lastFilter.Equals(_config.Settings.FileFilter, StringComparison.OrdinalIgnoreCase) ||
                         lastIncludeSubFolders != _config.Settings.IncludeSubfolders ||
                         !lastOutputFolder.Equals(_config.Settings.OutputFolder, StringComparison.OrdinalIgnoreCase) ||
                         lastRandomizeOutputFileName != _config.Settings.RandomizeOutputFileName ||
                         preserveSubFoldersInOutput != _config.Settings.PreserveSubFoldersInOutput))
                    {
                        if (PromptForRestart(true))
                        {
                            _config.Save();

                            Clear();
                        }
                        else
                        {
                            _config.Load();
                        }
                    }
                    else
                    {
                        _config.Save();
                    }
                }
            }
        }

        /// <summary>
        /// Prompts for restart.
        /// </summary>
        /// <param name="settingsChange"></param>
        static bool PromptForRestart(bool settingsChange)
        {
            using (var messageBox = new CustomizableMessageBox())
            {
                messageBox.Caption = settingsChange ? "Apply settings?" : "Restart?";
                messageBox.Text = settingsChange
                    ? "To apply the new settings, the pagination process needs to be restarted.\r\n"
                    : "";

                messageBox.Text += "All existing pages to be cleared and reloaded from the specified input folder.\r\n\r\n";
                messageBox.StandardIcon = MessageBoxIcon.Question;
                messageBox.AddButton(settingsChange ? "Apply and Restart" : "Restart", "Restart", false);
                messageBox.AddButton("Cancel", "Cancel", false);
                messageBox.Show();

                return (messageBox.Result == "Restart");
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Clear()
        {
            try
            {
                _inputFileEnumerator.Dispose();
                _inputFileEnumerator = null;
                _inputFolderWatcher.Dispose();
                _inputFolderWatcher = null;

                if (_imageViewer.IsImageAvailable)
                {
                    _imageViewer.CloseImage();
                }

                _primaryPageLayoutControl.Clear();

                foreach (SourceDocument document in _sourceDocuments)
                {
                    document.Disposed -= HandleSourceDocument_Disposed;
                    document.Dispose();
                }
                _sourceDocuments.Clear();

                _outputDocumentNames.Clear();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35535");
            }
        }

        /// <summary>
        /// Opens the lock file.
        /// </summary>
        void OpenLockFile()
        {
            string lockFileName = Path.Combine(_config.Settings.InputFolder, "Extract.Pagination.lock");
            if (lockFileName != _lockFileName)
            {
                if (_lockFile != null)
                {
                    _lockFile.Dispose();
                    _lockFile = null;
                    try
                    {
                        FileSystemMethods.DeleteFileNoRetry(_lockFileName);
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI35536");
                    }
                    _lockFileName = null;
                }

                try
                {
                    // If there is an existing lock file, if it can be openend the instance
                    // that created it is not still open and it can therefore be ignored.
                    _lockFile = File.Open(lockFileName, FileMode.OpenOrCreate,
                        FileAccess.ReadWrite, FileShare.None);
                    File.SetAttributes(lockFileName, 
                        FileAttributes.Hidden | FileAttributes.NotContentIndexed);
                    _lockFileName = lockFileName;
                }
                catch
                {
                    // But if it couldn't be deleted, another instance of the paginiation
                    // utility is already working on this folder; prevent this instance from
                    // using it.
                    ExtractException ee = new ExtractException("ELI35537",
                        "Another instance of the pagination utility is currently active " +
                        "for this input directory.");
                    ee.AddDebugData("Input directory", _config.Settings.InputFolder, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Generates a new name for the document based on the specified <see paramref="fileName"/>
        /// and <see paramref="pathRoot"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        string GenerateOutputDocumentName(string fileName)
        {
            if (_config.Settings.PreserveSubFoldersInOutput &&
                fileName.StartsWith(_config.Settings.InputFolder, StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(_config.Settings.InputFolder.Length);
                fileName = fileName.TrimStart('\\');
                fileName = Path.Combine(_config.Settings.OutputFolder, fileName);
            }
            else
            {
                fileName = Path.Combine(_config.Settings.OutputFolder, Path.GetFileName(fileName));
            }

            string baseFilename = fileName;
            string directory = Path.GetDirectoryName(baseFilename);
            string extension = Path.GetExtension(baseFilename);
            baseFilename = Path.GetFileNameWithoutExtension(baseFilename);

            if (_config.Settings.RandomizeOutputFileName)
            {
                fileName = Path.Combine(directory, baseFilename + "_" +
                    UtilityMethods.GetRandomString(10, true, false, true) +
                    extension);
            }

            if (IsOutputFileNameAvailable(fileName))
            {
                return fileName;
            }
            
            string newFileName;
            for (int number = 2; number < Int16.MaxValue; number++)
            {
                if (_config.Settings.RandomizeOutputFileName)
                {
                    newFileName = Path.Combine(directory, baseFilename + "_" +
                        UtilityMethods.GetRandomString(10, true, false, true) +
                        extension);
                }
                else
                {
                    newFileName = Path.Combine(directory,
                        string.Format(CultureInfo.CurrentCulture,
                            baseFilename + "_{0:D3}" + extension, number));
                }

                if (IsOutputFileNameAvailable(newFileName))
                {
                    return newFileName;
                }
            }

            ExtractException.ThrowLogicException("ELI35538");

            return null;
        }

        /// <summary>
        /// Generates a new name for the document based on the specified <see paramref="fileName"/>
        /// and <see paramref="pathRoot"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        string GenerateProcessedDocumentName(string fileName)
        {
            if (fileName.StartsWith(_config.Settings.InputFolder, StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(_config.Settings.InputFolder.Length);
                fileName = fileName.TrimStart('\\');
                fileName = Path.Combine(_config.Settings.ProcessedFileFolder, fileName);
            }
            else
            {
                fileName = Path.Combine(_config.Settings.ProcessedFileFolder, Path.GetFileName(fileName));
            }

            string baseFilename = fileName;
            string directory = Path.GetDirectoryName(baseFilename);
            string extension = Path.GetExtension(baseFilename);
            baseFilename = Path.GetFileNameWithoutExtension(baseFilename);

            if (IsProcessedFileNameAvailable(fileName))
            {
                return fileName;
            }

            string newFileName;
            for (int number = 2; number < Int16.MaxValue; number++)
            {
                newFileName = Path.Combine(directory,
                    string.Format(CultureInfo.CurrentCulture,
                        baseFilename + "_{0:D3}" + extension, number));

                if (IsOutputFileNameAvailable(newFileName))
                {
                    return newFileName;
                }
            }

            ExtractException.ThrowLogicException("ELI35539");

            return null;
        }

        /// <summary>
        /// Starts the input enumeration.
        /// </summary>
        void StartInputEnumeration()
        {
            ExtractException.Assert("ELI35540", "Input directory has not been specified.",
                _config != null && Directory.Exists(_config.Settings.InputFolder));

            OpenLockFile();

            if (string.IsNullOrWhiteSpace(_config.Settings.FileFilter))
            {
                _fileFilter = null;
            }
            else
            {
                _fileFilter = new FileFilter(null, _config.Settings.FileFilter, false);
            }

            var inputDirectoryInfo = new DirectoryInfo(_config.Settings.InputFolder);
            var inputEnumerable = inputDirectoryInfo.EnumerateFiles("*",
                _config.Settings.IncludeSubfolders
                    ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(file =>
                    !file.FullName.Equals(_lockFileName, StringComparison.OrdinalIgnoreCase) &&
                    (_fileFilter == null || _fileFilter.FileMatchesFilter(file.FullName)))
                .OrderBy(file => file.CreationTime)
                .Select(file => file.FullName);
            _inputFileEnumerator = inputEnumerable.GetEnumerator();

            _inputFolderWatcher = new FileSystemWatcher(_config.Settings.InputFolder);
            _inputFolderWatcher.IncludeSubdirectories = _config.Settings.IncludeSubfolders;
            _inputFolderWatcher.NotifyFilter =
                NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName;
            _inputFolderWatcher.EnableRaisingEvents = false;
            _inputFolderWatcher.Created += HandleInputFolderWatcher_Created;
            _inputFolderWatcher.Renamed += HandleInputFolderWatcher_Renamed;
        }

        /// <summary>
        /// Loads the next document.
        /// </summary>
        /// <param name="restartIfEmpty"></param>
        bool LoadNextDocument(bool restartIfEmpty = true)
        {
            if (_inputFileEnumerator == null)
            {
                StartInputEnumeration();
            }

            while (_inputFileEnumerator.MoveNext())
            {
                var sourceDocument = OpenDocument(_inputFileEnumerator.Current);

                if (sourceDocument != null)
                {
                    _primaryPageLayoutControl.CreateOutputDocument(sourceDocument);

                    return true;
                }
            }
                
            // Display "no more files"
            if (restartIfEmpty)
            {
                _inputFolderWatcher.EnableRaisingEvents = true;

                StartInputEnumeration();
                if (LoadNextDocument(false))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Opens the document.
        /// </summary>
        /// <param name="inputFileName">Name of the input file.</param>
        /// <returns></returns>
        SourceDocument OpenDocument(string inputFileName)
        {
            if (!File.Exists(inputFileName) ||
                _sourceDocuments.Any(doc =>
                    doc.FileName.Equals(inputFileName, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            var sourceDocument = new SourceDocument(inputFileName);
            if (!sourceDocument.Pages.Any())
            {
                sourceDocument.Dispose();
                return null;
            }

            sourceDocument.Disposed += HandleSourceDocument_Disposed;
            _sourceDocuments.Add(sourceDocument);

            ThreadingMethods.RunInBackgroundThread("ELI35541",
                () => _imageViewer.CacheImage(inputFileName));

            return sourceDocument;
        }

        /// <summary>
        /// Loads the more pages.
        /// </summary>
        void LoadMorePages()
        {
            if (_closing)
            {
                return;
            }

            bool ranOutOfDocuments = false;
            while (!ranOutOfDocuments &&
                   _primaryPageLayoutControl.PageCount < _config.Settings.InputPageCount)
            {
                ranOutOfDocuments = !LoadNextDocument();
            }

            if (!ranOutOfDocuments && _inputFolderWatcher != null)
            {
                _inputFolderWatcher.EnableRaisingEvents = false;
            }
        }

        /// <summary>
        /// Invokes the load more pages.
        /// </summary>
        void InvokeLoadMorePages()
        {
            if (!_pageLoadPending && !_closing)
            {
                _pageLoadPending = true;
                this.SafeBeginInvoke("ELI35542", () =>
                {
                    _pageLoadPending = false;

                    if (_primaryPageLayoutControl.PageCount < _config.Settings.InputPageCount)
                    {
                        LoadMorePages();
                    }
                });
            }
        }

        /// <summary>
        /// Removes the stale clipboard backup.
        /// </summary>
        void RemoveStaleClipboardBackup(int gracePeriodSeconds)
        {
            // If the clipboard data backup copy has expired, set it to null so that it
            // is not used.
            if (_lastClipboardData != null &&
                (DateTime.Now - _lastClipboardCopyTime).TotalSeconds > gracePeriodSeconds)
            {
                var clipboardData =
                    (ClipboardData)_lastClipboardData.GetData(_CLIPBOARD_DATA_FORMAT);

                foreach (Page page in clipboardData.GetPages(this))
                {
                    page.RemoveReference(this);
                }

                _lastClipboardData = null;
            }
        }

        #endregion Private Members
    }
}
