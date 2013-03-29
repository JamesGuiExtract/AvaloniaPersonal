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
    /// Allows manipulation of the pages of input documents in order to generate output documents
    /// that can contain any combination and sequence of input documents.
    /// </summary>
    public partial class PaginationUtilityForm : Form
    {
        #region ClipboardData class

        /// <summary>
        /// Represents <see cref="PaginationControl"/>s copied to the clipboard by this utility.
        /// </summary>
        [Serializable]
        class ClipboardData
        {
            /// <summary>
            /// A list of <see cref="Tuple"/>s where the first item indicates the input filename the
            /// a page is from and the second item indicates the page from that document. Any
            /// <see langword="null"/> entries in this list indicate an output document boundary
            /// which should be represented by a <see cref="PaginationSeparator"/> when pasted into
            /// a <see cref="PageLayoutControl"/>.
            /// </summary>
            List<Tuple<string, int>> _pageData;

            /// <summary>
            /// Initializes a new instance of the <see cref="ClipboardData"/> class.
            /// </summary>
            /// <param name="pages">The <see cref="Page"/> instances to be copied to the clipboard
            /// where any <see langword="null"/> entries indicate a document boundary.</param>
            public ClipboardData(IEnumerable<Page> pages)
            {
                _pageData = new List<Tuple<string, int>>(
                    pages.Select(page => (page == null)
                        ? null
                        : new Tuple<string, int>(page.OriginalDocumentName, page.OriginalPageNumber)));
            }

            /// <summary>
            /// Gets the <see cref="IEnumerable{Page}"/> represented by this clipboard data.
            /// </summary>
            /// <param name="paginationUtility">The <see cref="PaginationUtilityForm"/> the data is
            /// needed for.</param>
            /// <returns>The <see cref="IEnumerable{Page}"/> represented by this clipboard data.
            /// </returns>
            public IEnumerable<Page> GetPages(PaginationUtilityForm paginationUtility)
            {
                // Convert each entry in _pageData into either null (for a document boundary) or a
                // Page instance.
                foreach(Tuple<string, int> pageData in _pageData)
                {
                    if (pageData == null)
                    {
                        yield return null;
                        continue;
                    }

                    string fileName = pageData.Item1;
                    int pageNumber = pageData.Item2;

                    // See if the document indicated is already open as a SourceDocument.
                    SourceDocument document = paginationUtility._sourceDocuments
                        .Where(doc =>
                            doc.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                        .SingleOrDefault();

                    // If not, open it as a SourceDocument.
                    if (document == null)
                    {
                        document = paginationUtility.OpenDocument(fileName);
                    }

                    // If unable to open then document, don't throw an exception, just act as though
                    // the data was not on the clipboard in the first place.
                    if (document == null)
                    {
                        break;
                    }

                    ExtractException.Assert("ELI35506", "Cannot find source page.",
                        document != null && document.Pages.Count >= pageNumber,
                        "Filename", fileName,
                        "Page number", pageNumber);

                    // Return the correct page from the SourceDocument.
                    yield return document.Pages[pageNumber - 1];
                }
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object"/> is equal to this
            /// instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.
            /// </param>
            /// <returns><see langword="true"/> if the specified <see cref="System.Object"/> is
            /// equal to this instance; otherwise, <see langword="false"/>.
            /// </returns>
            public override bool Equals(object obj)
            {
                try
                {
                    // An equivilant object must be a ClipboardData instance.
                    var clipboardData = obj as ClipboardData;
                    if (clipboardData == null)
                    {
                        return false;
                    }

                    // An equivilant object must have the same number of entries in _pageData.
                    var pageData = clipboardData._pageData;
                    if (pageData.Count != _pageData.Count)
                    {
                        return false;
                    }

                    // An equivilant object must have equivalent entries in _pageData.
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
            /// A hash code for this instance, suitable for use in hashing algorithms and data
            /// structures like a hash table. 
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

        #endregion ClipboardData class

        #region Constants

        /// <summary>
        /// The name of this object.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(PaginationUtilityForm).ToString();

        // Comment back in to support multiple tabs (for scratch area, recycling bin, etc)
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
        /// The name of the custom clipboard data format to use.
        /// </summary>
        static readonly string _CLIPBOARD_DATA_FORMAT = "ExtractPaginationClipboardDataFormat";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates whether this instance is using predetermined settings that cannot be edited.
        /// </summary>
        bool _usingPredeterminedSettings;

        /// <summary>
        /// The settings being used by this instance.
        /// </summary>
        ConfigSettings<Settings> _config;

        /// <summary>
        /// The <see cref="T:IEnumerator{string}"/> which interates input documents to be loaded
        /// into the UI.
        /// </summary>
        IEnumerator<string> _inputFileEnumerator;

        /// <summary>
        /// The <see cref="FileSystemWatcher"/> that will provide notification of new input files to
        /// import into the UI.
        /// </summary>
        FileSystemWatcher _inputFolderWatcher;

        /// <summary>
        /// The <see cref="FileFilter"/> to be used to limit 
        /// </summary>
        FileFilter _fileFilter;

        /// <summary>
        /// Saves/restores window state info
        /// </summary>
        FormStateManager _formStateManager;

        // Comment back in to support multiple tabs (for scratch area, recycling bin, etc)
//        /// <summary>
//        /// The <see cref="TabbedDocument"/> into which input documents are initially loaded.
//        /// </summary>
//        TabbedDocument _primaryWorkAreaTab;

        /// <summary>
        /// The <see cref="PageLayoutControl"/> into which input documents are initially loaded.
        /// </summary>
        PageLayoutControl _primaryPageLayoutControl;

        /// <summary>
        /// The set of all <see cref="SourceDocument"/>s currently active in the UI.
        /// </summary>
        HashSet<SourceDocument> _sourceDocuments = new HashSet<SourceDocument>();

        /// <summary>
        /// The set of all <see cref="OutputDocument"/>s currently active in the UI.
        /// </summary>
        HashSet<OutputDocument> _pendingDocuments = new HashSet<OutputDocument>();

        /// <summary>
        /// The set of all filenames that have been output by this instance of the UI.
        /// </summary>
        HashSet<string> _outputDocumentNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The set of all filenames to which input documents have been copied to by this instance
        /// of the UI.
        /// </summary>
        HashSet<string> _processedDocumentNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Indicates whether a call to check to see if more pages need to be loaded is pending.
        /// </summary>
        bool _pageLoadPending;

        /// <summary>
        /// The name of the file used to indicate this instance is processing documents from the
        /// configured input folder and that no other instance should be allowed to operate on the
        /// folder at the same time.
        /// </summary>
        string _lockFileName;

        /// <summary>
        /// The <see cref="FileStream"/> used to lock the file used to indicate this instance is
        /// processing documents from the configured input folder and that no other instance should
        /// be allowed to operate on the folder at the same time.
        /// </summary>
        FileStream _lockFile;

        /// <summary>
        /// A copy of the last data put on the clipboard.
        /// </summary>
        ClipboardData _currentClipboardData = null;

        /// <summary>
        /// Indicates that this instance is closing and that no more page loads or control events
        /// should be handled.
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
        /// <param name="configurationFileName">If this instance is to use pre-detemined settings,
        /// the name of the configuration file containing the settings to use; <see langword="null"/>
        /// otherwise.</param>
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

                // Comment back in to support multiple tabs (for scratch area, recycling bin, etc)
//                // License SandDock before creating the form.
//                SandDockManager.ActivateProduct(_SANDDOCK_LICENSE_STRING);

                _usingPredeterminedSettings = !string.IsNullOrEmpty(configurationFileName);

                // Auto-create the config file only if not using pre-determined settings.
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

                // Allowing caching of images not only improves performance, but also locks the
                // input documents thereby preventing the documents from being deleted or moved
                // while in use by this utility.
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
        /// Determines whether the specified <see paramref="fileName"/> is available to use as an
        /// output document name.
        /// </overrides>
        /// <summary>
        /// Determines whether the specified <see paramref="fileName"/> is available to use as an
        /// output document name.
        /// </summary>
        /// <param name="fileName">The desired output document name to use.</param>
        /// <returns><see langword="true"/> if the filename is available to use; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        internal bool IsOutputFileNameAvailable(string fileName)
        {
            return IsOutputFileNameAvailable(fileName, null);
        }

        /// <summary>
        /// Determines whether the specified <see paramref="fileName"/> is available to use as an
        /// output document name.
        /// </summary>
        /// <param name="fileName">The desired output document name to use.</param>
        /// <param name="subjectDocument">The <see cref="OutputDocument"/> the filename is to be
        /// used for.</param>
        /// <returns><see langword="true"/> if the filename is available to use; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        internal bool IsOutputFileNameAvailable(string fileName, OutputDocument subjectDocument)
        {
            try
            {
                // If a document by this name has already been output by this instance, don't allow
                // it whether or not that document still exists at that location.
                if (_outputDocumentNames.Contains(fileName))
                {
                    return false;
                }

                // Don't allow the same name to be used as is being used for any active
                // OutputDocument except subjectDocument.
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
        /// Determines whether the specified <see paramref="fileName"/> is available to use as the
        /// location to copy an input document that has been processed.
        /// </summary>
        /// <param name="fileName">The desired document name to use.</param>
        /// <returns><see langword="true"/> if the filename is available to use; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        internal bool IsProcessedFileNameAvailable(string fileName)
        {
            try
            {
                // If this instance has already copied a document to this name, don't allow
                // it whether or not that document still exists at that location.
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
        /// Creates a new <see cref="OutputDocument"/> based on the specified
        /// <see paramref="inputFileName"/>.
        /// </summary>
        /// <param name="inputFileName">The name that should be used as the basis for the new
        /// document's filename.</param>
        /// <returns>The new <see cref="OutputDocument"/>.</returns>
        internal OutputDocument CreateOutputDocument(string inputFileName)
        {
            string outputDocumentName = GenerateOutputDocumentName(inputFileName);
            OutputDocument outputDocument = new OutputDocument(outputDocumentName);
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
                            // Techinically un-necessary, but the clipboard can be flakey, so maybe
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

                            foreach (Page page in pages.Where(page => page != null))
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
        /// Retrieves as an enumerable of <see cref="Page"/>s the data from the clipboard. This will
        /// attempt to use a backup copy if it fails to retrieve the data on the clipboard.
        /// </summary>
        /// <returns>An enumerable of <see cref="Page"/>s that represents the data
        /// currently on the clipboard, or <see langword="null"/> if there is no pagination utility
        /// data on the clipboard.</returns>
        internal IEnumerable<Page> GetClipboardData()
        {
            try
            {
                IDataObject dataObject = Clipboard.GetDataObject();
                ClipboardData clipboardData = dataObject.GetData(_CLIPBOARD_DATA_FORMAT) as ClipboardData;

                if (_currentClipboardData != null && !_currentClipboardData.Equals(clipboardData))
                {
                    DereferenceLastClipboardData();
                }

                // If we found data on the clipboard, convert it to an array of Pages.
                if (clipboardData != null)
                {
                    return clipboardData.GetPages(this);
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

                // Comment back in to support multiple tabs (for scratch area, recycling bin, etc)
//                _sandDockManager.DockSystemContainer = _pageLayoutToolStripContainer.ContentPanel;
//                _primaryWorkAreaTab = new TabbedDocument();
//                _primaryWorkAreaTab.AllowClose = false;
//                _primaryWorkAreaTab.Manager = _sandDockManager;
//                _primaryWorkAreaTab.Text = "Input";
//                _primaryWorkAreaTab.OpenDocument(WindowOpenMethod.OnScreen);

                _primaryPageLayoutControl = new PageLayoutControl(this);
                _primaryPageLayoutControl.Dock = DockStyle.Fill;
                _primaryPageLayoutControl.ImageViewer = _imageViewer;
                _primaryPageLayoutControl.StateChanged += HandlePageLayoutControl_StateChanged;
                _primaryPageLayoutControl.PageDeleted += HandlePageLayoutControl_PageDeleted;
                _pageLayoutToolStripContainer.ContentPanel.Controls.Add(_primaryPageLayoutControl);
                ActiveControl = _primaryPageLayoutControl;

                // If not using pre-determined settings, the settings dialog should be displayed
                // first to allow the user to confirm/edit settings before input documents are
                // loaded.
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
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnEnter(EventArgs e)
        {
            try
            {
                base.OnEnter(e);

                // Ensure _primaryPageLayoutControl it the control that gets focus by default.
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
        /// <param name="disposing"><see langword="true"/> if managed resources should be disposed; otherwise,
        /// <see langword="false"/>.
        /// </param>
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

                    DereferenceLastClipboardData();

                    if (components != null)
                    {
                        components.Dispose();
                    }
                }
                catch (System.Exception ex)
                {
                    ex.ExtractLog("ELI35522");
                }
            }

            // This code is outside of the disposing condition since _lockFile is tied to an
            // unmanaged resource.
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

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_settingsToolStripMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
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
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_restartToolStripMenuItem"/>.
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
        /// Handles the <see cref="SourceDocument.Disposed"/> event of a <see cref="SourceDocument"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSourceDocument_Disposed(object sender, EventArgs e)
        {
            try
            {
                // Ensure we have not already unloaded this document.
                var sourceDocument = (SourceDocument)sender;
                if (!_sourceDocuments.Contains(sourceDocument))
                {
                    return;
                }

                // If the document came from the input folder (as opposed to having been dragged in
                // from another folder), either move or delete the file from the input folder now
                // that it has been processed.
                if ((_config.Settings.IncludeSubfolders && sourceDocument.FileName.StartsWith(
                    _config.Settings.InputFolder, StringComparison.OrdinalIgnoreCase) ||
                    (!_config.Settings.IncludeSubfolders &&
                        Path.GetDirectoryName(sourceDocument.FileName).Equals(
                            _config.Settings.InputFolder, StringComparison.OrdinalIgnoreCase))))
                {
                    // This removes the image from the cache and releases the lock on the file.
                    _imageViewer.UnloadImage(sourceDocument.FileName);

                    if (_config.Settings.DeleteProcessedFiles)
                    {
                        FileSystemMethods.DeleteFile(sourceDocument.FileName);
                    }
                    else
                    {
                        string processedDocumentName = GenerateProcessedDocumentName(sourceDocument);

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
        /// Handles the <see cref="PageLayoutControl.PageDeleted"/> event of an
        /// <see cref="PageLayoutControl"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePageLayoutControl_PageDeleted(object sender, PageDeletedEventArgs e)
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
                    
                    // Whenever pages are removed, call GetClipboardData to remove references to
                    // and pages that were on the clipboard, but are no longer on the clipboard.
                    // This ensures input documents no longer being used are moved/deleted in a
                    // timely fashion.
                    GetClipboardData();

                    CheckIfSourceDocumentsProcessed(e.Page.SourceDocument);

                    OutputDocument outputDocument = e.OutputDocument;
                    if (outputDocument != null && outputDocument.PageControls.Count == 0)
                    {
                        outputDocument.DocumentOutput -= HandleOutputDocument_DocumentOutput;
                        _pendingDocuments.Remove(outputDocument);
                    }

                    InvokeLoadMorePages();
                });
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35528");
            }
        }

        /// <summary>
        /// Handles the <see cref="OutputDocument.DocumentOutput"/> event of an
        /// <see cref="OutputDocument"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOutputDocument_DocumentOutput(object sender, EventArgs e)
        {
            try
            {
                var document = (OutputDocument)sender;
                _outputDocumentNames.Add(document.FileName);

                InvokeLoadMorePages();

                SourceDocument[] sourceDocuments = document.PageControls
                    .Select(pageControl => pageControl.Page.SourceDocument)
                    .Distinct()
                    .ToArray();

                // After outputting a document, delete/move any input documents that are no longer
                // referenced.
                this.SafeBeginInvoke("ELI35560", () => CheckIfSourceDocumentsProcessed(sourceDocuments));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35529");
            }
        }

        /// <summary>
        /// Handles the <see cref="PageLayoutControl.StateChanged"/> event of a
        /// <see cref="PageLayoutControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePageLayoutControl_StateChanged(object sender, EventArgs e)
        {
            try
            {
                // Enable/disable the output document button to match OutputDocumentCommandEnabled.
                _outputDocumentToolStripButton.Enabled =
                    _primaryPageLayoutControl.OutputDocumentCommandEnabled;

                if (_primaryPageLayoutControl.PartiallySelectedDocuments.Count() == 1)
                {
                    OutputDocument selectedDocument =
                        _primaryPageLayoutControl.PartiallySelectedDocuments.Single();

                    // If there is only one document with at least one page selected, set the output
                    // filename.
                    _outputFileNameToolStripTextBox.Text = selectedDocument.FileName;

                    // If the document is fully selected, set the pages label to indicate the number
                    // of pages in the document.
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
                        // If the document is not fully selected, disable the output filename text
                        // box.
                        _outputFileNameToolStripTextBox.Enabled = false;

                        IEnumerable<PageThumbnailControl> selectedPageControls =
                            selectedDocument.PageControls.Where(pageControl => pageControl.Selected);

                        // If a single page is selected, indicate which page is selected, otherwise
                        // clear the page label.
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
                    // If zero or more than one document is partially selected, both output
                    // filename text box and page label should be cleared.
                    _outputFileNameToolStripTextBox.Text = "";
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
        /// Handles the <see cref="FileSystemWatcher.Created"/> event of the
        /// <see cref="_inputFolderWatcher"/>.
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
        /// Handles the <see cref="FileSystemWatcher.Renamed"/> event of the
        /// <see cref="_inputFolderWatcher"/>.
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
        /// Handles the <see cref="Control.Validating"/> event of the
        /// <see cref="_outputFileNameToolStripTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance
        /// containing the event data.</param>
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
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_outputFileNameBrowseToolStripButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOutputFileNameBrowseToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Show a dialog that allows the user to pick a new output filename.
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    string initialFile = _outputFileNameToolStripTextBox.Text;

                    // Try to default to the specified file, but since the user could have entered
                    // invalid data, ignore any exceptions trying to initialize the default filename.
                    try
                    {
                        saveFileDialog.InitialDirectory = Path.GetDirectoryName(initialFile);
                        saveFileDialog.FileName = Path.GetFileName(initialFile);
                    }
                    catch { }

                    saveFileDialog.AddExtension = false;
                    saveFileDialog.CheckPathExists = true;
                    saveFileDialog.CheckFileExists = false;

                    // Show the dialog
                    while (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (!IsOutputFileNameAvailable(saveFileDialog.FileName))
                        {
                            UtilityMethods.ShowMessageBox(
                                "This output filename has already been used.",
                                "Filename Unavailable", true);

                            saveFileDialog.InitialDirectory =
                                Path.GetDirectoryName(saveFileDialog.FileName);
                            saveFileDialog.FileName = Path.GetFileName(saveFileDialog.FileName);
                            continue;
                        }

                        // Return the selected file path.
                        _outputFileNameToolStripTextBox.Text = saveFileDialog.FileName;
                        OutputDocument selectedDocument =
                            _primaryPageLayoutControl.PartiallySelectedDocuments.Single();
                        selectedDocument.FileName = _outputFileNameToolStripTextBox.Text;
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
                // Keep track of the initial value of settings that require a restart to apply if
                // changed.
                string originalInputFolder = _config.Settings.InputFolder;
                string originalFilter = _config.Settings.FileFilter;
                bool originalIncludeSubFolders = _config.Settings.IncludeSubfolders;
                string originalOutputFolder = _config.Settings.OutputFolder;
                bool originalRandomizeOutputFileName = _config.Settings.RandomizeOutputFileName;
                bool originalPreserveSubFoldersInOutput = _config.Settings.PreserveSubFoldersInOutput;

                // Make the settings readonly if using pre-determined settings.
                paginationSettingsDialog.ReadOnly = _usingPredeterminedSettings;
                DialogResult dialogResult = paginationSettingsDialog.ShowDialog();

                if (dialogResult == DialogResult.OK && !_usingPredeterminedSettings)
                {
                    if (_inputFileEnumerator != null &&
                        (!originalInputFolder.Equals(_config.Settings.InputFolder, StringComparison.OrdinalIgnoreCase) ||
                         !originalFilter.Equals(_config.Settings.FileFilter, StringComparison.OrdinalIgnoreCase) ||
                         originalIncludeSubFolders != _config.Settings.IncludeSubfolders ||
                         !originalOutputFolder.Equals(_config.Settings.OutputFolder, StringComparison.OrdinalIgnoreCase) ||
                         originalRandomizeOutputFileName != _config.Settings.RandomizeOutputFileName ||
                         originalPreserveSubFoldersInOutput != _config.Settings.PreserveSubFoldersInOutput))
                    {
                        // If any critical settings have changed, the user needs to confirm a
                        // restart before the new settings can be applied.
                        if (PromptForRestart(true))
                        {
                            _config.Save();

                            Clear();
                        }
                        else
                        {
                            // If the user chose not to restart, restore the previous settings.
                            _config.Load();
                        }
                    }
                    else
                    {
                        // If any non-critical settings change or the utililty was not yet active,
                        // go ahead and apply the settings without a restart.
                        _config.Save();
                    }
                }
            }
        }

        /// <summary>
        /// Prompts the user as to whether they want to clear any existing work and restart.
        /// </summary>
        /// <param name="settingsChange"><see langword="true"/> if the restart is due to a settings
        /// change.</param>
        /// <returns><see langword="true"/> if the user confirmed the restart; otherwise,
        /// <see langword="false"/>.</returns>
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
        /// Clears all active <see cref="PaginationControl"/>s and their associated data and
        /// restores the utility to a state equivilant to just having opened the utility.
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
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35535");
            }
        }

        /// <summary>
        /// Opens the lock file to indicate that this utility is operating on the specified input
        /// folder and that no other instances of the utility should attempt to process this folder.
        /// </summary>
        void OpenLockFile()
        {
            string lockFileName = Path.Combine(_config.Settings.InputFolder, "extract.pagination.lock");
            
            // If we don't already have this lock file open.
            if (lockFileName != _lockFileName)
            {
                // If new lock file is in a different location than the last lock file being used,
                // release the last lock file.
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
        /// Generates a new name an <see cref="OutputDocument"/> based on the specified
        /// <see paramref="fileName"/> that is unique compared to any pending output document, any
        /// existing document or any document that has been output by this instance whether or not
        /// the file still exists.
        /// </summary>
        /// <param name="fileName">The filename that should serve as the basis for the new document
        /// name.</param>
        /// <returns>The unique document name.</returns>
        string GenerateOutputDocumentName(string fileName)
        {
            // If fileName is from the input folder and we are to preserve the sub-folder hierarchy,
            // replace the input folder with the output folder in the path.
            if (_config.Settings.PreserveSubFoldersInOutput &&
                fileName.StartsWith(_config.Settings.InputFolder, StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(_config.Settings.InputFolder.Length);
                fileName = fileName.TrimStart('\\');
                fileName = Path.Combine(_config.Settings.OutputFolder, fileName);
            }
            else
            {
                // Otherwise, user the filename without directory as the basis for a file to be
                // output into the root of the output directory.
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
            
            // If the filename was not available modify the output filename in a loop by either
            // incrementing a number at the end of the file name or generating a new random
            // filename until an available filename is found.
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
        /// Generates a name to which <see paramref="sourceDocument"/> should be moved after being
        /// copied that is unique compared to any existing document or any document that has been
        /// created by this instance whether or not the file still exists.
        /// </summary>
        /// <param name="sourceDocument">The <see cref="SourceDocument"/> to be moved.</param>
        /// <returns>The unique document name.</returns>
        string GenerateProcessedDocumentName(SourceDocument sourceDocument)
        {
            string fileName = sourceDocument.FileName;

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
        /// Starts enumerating the input directory.
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

            // Begin the enumeration in order of creation data using _fileFilter to filter the
            // input as appropriate.
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

            // Initialze a folder watcher that will be activated to watch for added files once there
            // are no more files available in the input folder.
            _inputFolderWatcher = new FileSystemWatcher(_config.Settings.InputFolder);
            _inputFolderWatcher.IncludeSubdirectories = _config.Settings.IncludeSubfolders;
            _inputFolderWatcher.NotifyFilter =
                NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName;
            _inputFolderWatcher.EnableRaisingEvents = false;
            _inputFolderWatcher.Created += HandleInputFolderWatcher_Created;
            _inputFolderWatcher.Renamed += HandleInputFolderWatcher_Renamed;
        }

        /// <summary>
        /// Loads the next input document into the UI.
        /// </summary>
        bool LoadNextDocument()
        {
            return LoadNextDocument(true);
        }

        /// <summary>
        /// Loads the next input document into the UI.
        /// </summary>
        /// <param name="restartIfEmpty"><see langword="true"/> if the enumeration should be
        /// restarted from the beginning once we reach the end.</param>
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

            // If we reached the end of the enumeration, enable the _inputFolderWatcher to notify
            // when any new files are added, then restart the enumeration (unless it is specified
            // that the restart is not necessary to prevent recursion).
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
        /// Opens the specified <see paramref="inputFileName"/> as a <see cref="SourceDocument"/>
        /// instance.
        /// </summary>
        /// <param name="inputFileName">Name of the input file.</param>
        /// <returns>A <see cref="SourceDocument"/> representing <see paramref="inputFileName"/> or
        /// <see langword="null"/> if the file is missing, could not be opened, or is already
        /// opened.</returns>
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
        /// Loads the more pages from input folder documents until there are greater than or equal
        /// number of pages loaded as _config.Settings.InputPageCount.
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
        /// Invokes <see cref="LoadMorePages"/> on the message queue if more pages are needed to
        /// reach <see cref="Settings.InputPageCount"/>
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
        /// Removes any references to pages last copied to the clipboard so that the input document
        /// can be deleted/moved if there are no other existing copies of the pages' document(s).
        /// </summary>
        void DereferenceLastClipboardData()
        {
            if (_currentClipboardData != null)
            {
                Page[] clipboardPages = _currentClipboardData.GetPages(this).ToArray();
                foreach (Page page in clipboardPages.Where(page => page != null))
                {
                    page.RemoveReference(this);
                }

                CheckIfSourceDocumentsProcessed(clipboardPages
                    .Where(page => page != null)
                    .Select(page => page.SourceDocument)
                    .Distinct()
                    .ToArray());
            }
        }

        /// <summary>
        /// Checks to see if all pages of the specified <see paramref="soureDocuments"/> have been
        /// processed; if so, the documents are disposed of so that the input documents get
        /// delete/moved (as configured).
        /// </summary>
        /// <param name="sourceDocuments"></param>
        void CheckIfSourceDocumentsProcessed(params SourceDocument[] sourceDocuments)
        {
            // Check for a sourceDocuments that is no longer being referenced so that it can
            // be disposed and the input file can be deleted/moved as configured.
            IEnumerable<SourceDocument> documentsToDispose = sourceDocuments
                .Where(document => document.Pages.All(page => !page.HasActiveReference));

            foreach (SourceDocument document in documentsToDispose)
            {
                document.Dispose();

                _sourceDocuments.Remove(document);
            }
        }

        #endregion Private Members
    }
}
