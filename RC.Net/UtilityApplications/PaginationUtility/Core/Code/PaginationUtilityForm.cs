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
    internal partial class PaginationUtilityForm : Form, IPaginationUtility
    {
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
            FileSystemMethods.UserApplicationDataPath, "PaginationUtility", "PaginationUtility.xml");

        /// <summary>
        /// Name for the mutex used to serialize persistence of the control and form layout.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_MUTEX_STRING =
            "97BC14EC-DEED-4826-AE47-6CD1CC502AAB";

        /// <summary>
        /// The number of pages that can be safely loaded at once without exceeding the limit of
        /// Window's user objects.
        /// </summary>
        internal static readonly int _MAX_LOADED_PAGES = 1000;

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
        /// The <see cref="T:IEnumerator{string}"/> which iterates input documents to be loaded
        /// into the UI.
        /// </summary>
        IEnumerator<string> _inputFileEnumerator;

        /// <summary>
        /// The <see cref="FileSystemWatcher"/> that will provide notification of new input files to
        /// import into the UI.
        /// </summary>
        FileSystemWatcher _inputFolderWatcher;

        /// <summary>
        /// Indicates whether currently in a call to <see cref="LoadNextDocument(bool)"/>.
        /// </summary>
        bool _loadingNextDocument;

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
        /// Synchronizes access to <see cref="_sourceDocuments"/>
        /// </summary>
        object _sourceDocumentLock = new object();

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
        /// Any documents that have failed to load and should not be retried on subsequent
        /// iterations of LoadNextDocument.
        /// </summary>
        HashSet<string> _failedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Indicates whether a call to check to see if more pages need to be loaded is pending.
        /// </summary>
        bool _pageLoadPending;

        /// <summary>
        /// The pages pending to be loaded once there is available room beneath
        /// <see cref="_MAX_LOADED_PAGES"/>.
        /// </summary>
        Queue<Page> _pagesPendingLoad = new Queue<Page>();

        /// <summary>
        /// The output document the filename edit box is currently active for (if any).
        /// </summary>
        OutputDocument _fileNameEditableDocument;

        /// <summary>
        /// Keeps track of the last know valid filename for the currently selected
        /// <see cref="_fileNameEditableDocument"/>.
        /// </summary>
        string _lastValidDocumentName;

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
        /// <see langword="true"/> if the user has accepted the current settings or they have been
        /// specified via command line; otherwise <see langword="false"/>.
        /// </summary>
        bool _hasAcceptedSettings;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationUtilityForm"/> class.
        /// </summary>
        /// <param name="configurationFileName">If this instance is to use pre-determined settings,
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

                LicenseUtilities.ValidateLicense(LicenseIdName.PaginationUIObject,
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
                        _FORM_PERSISTENCE_MUTEX_STRING, null, null);
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
        /// <param name="promptForOverwrite"><see langword="true"/> if the user should be prompted
        /// about a filename that conflicts with an existing filename; <see langword="false"/> to
        /// disallow the conflicting name without prompting.</param>
        /// <param name="outputImminent"><see langword="true"/> if this call is being made because
        /// the document is currently being output; <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> if the filename is available to use; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        internal bool IsOutputFileNameAvailable(string fileName, bool promptForOverwrite,
            bool outputImminent)
        {
            return IsOutputFileNameAvailable(fileName, null, promptForOverwrite, outputImminent);
        }

        /// <summary>
        /// Determines whether the specified <see paramref="fileName"/> is available to use as an
        /// output document name.
        /// </summary>
        /// <param name="fileName">The desired output document name to use.</param>
        /// <param name="subjectDocument">The <see cref="OutputDocument"/> the filename is to be
        /// used for.</param>
        /// <param name="promptForOverwrite"><see langword="true"/> if the user should be prompted
        /// about a filename that conflicts with an existing filename; <see langword="false"/> to
        /// disallow the conflicting name without prompting.</param>
        /// <param name="outputImminent"><see langword="true"/> if this call is being made because
        /// the document is currently being output; <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> if the filename is available to use; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        internal bool IsOutputFileNameAvailable(string fileName, OutputDocument subjectDocument,
            bool promptForOverwrite, bool outputImminent)
        {
            try
            {
                ExtractException.Assert("ELI35607", "Specified output filename is not valid",
                    !string.IsNullOrWhiteSpace(fileName) &&
                    FileSystemMethods.IsFileNameValid(Path.GetFileName(fileName)),
                    "Filename", fileName);

                // If !promptForOverwrite and a document by this name has already been output by
                // this instance, don't allow it whether or not that document still exists at that
                // location.
                // If promptForOverwrite, don't be concerned with whether a document was previously
                // output with this name... if this path doesn't current exist, assume the user will
                // want to output to it.
                if (!promptForOverwrite && _outputDocumentNames.Contains(fileName))
                {
                    return false;
                }

                // Don't allow the same name to be used as is being used for any active
                // OutputDocument except subjectDocument.
                if (_pendingDocuments.Any(document => document != subjectDocument &&
                    document.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (promptForOverwrite)
                    {
                        DialogResult response = MessageBox.Show(
                            "A pending output document is already using this name; use this filename anyway?",
                            "Filename conflict", MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2, 0);

                        if (response == DialogResult.OK)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                if (File.Exists(fileName))
                {
                    if (promptForOverwrite)
                    {
                        DialogResult response = outputImminent
                            ? MessageBox.Show("Overwrite existing file?",
                                "Overwrite?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2, 0)
                            : MessageBox.Show("A file by this name already exists; use this filename anyway?",
                                "File already exists", MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2, 0);

                        if (response == DialogResult.OK)
                        {
                            return true;
                        }
                    }

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
        /// Validates the specified <see cref="OutputDocument"/> has a name that can be used.
        /// </summary>
        /// <returns><see langword="true"/> if the filename is valid; <see langword="false"/> if it
        /// is invalid.</returns>
        bool ValidateOutputFileName()
        {
            if (_fileNameEditableDocument != null)
            {
                return ValidateOutputFileName(_fileNameEditableDocument, false);
            }

            return true;
        }

        /// <summary>
        /// Validates the specified <see cref="OutputDocument"/> has a name that can be used.
        /// </summary>
        /// <param name="selectedDocument">The <see cref="OutputDocument"/> whose name is to be
        /// validated.</param>
        /// <param name="outputImminent"><see langword="true"/> if this call is being made because
        /// the document is currently being output; <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> if the filename is valid; <see langword="false"/> if it
        /// is invalid.</returns>
        bool ValidateOutputFileName(OutputDocument selectedDocument, bool outputImminent)
        {
            try
            {
                if (!IsOutputFileNameAvailable(
                        selectedDocument.FileName, selectedDocument, true, outputImminent))
                {
                    return false;
                }

                if (selectedDocument.FileName.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    UtilityMethods.ShowMessageBox("This output filename contains invalid char(s).",
                        "Filename Invalid.", true);
                    return false;
                }

                // If the document name passed validation, we can update _lastValidDocumentName.
                _lastValidDocumentName = selectedDocument.FileName;

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35604");
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
        /// <see paramref="originalDocName"/>.
        /// </summary>
        /// <param name="originalDocName">The name that should be used as the basis for the new
        /// document's filename.</param>
        /// <returns>The new <see cref="OutputDocument"/>.</returns>
        public OutputDocument CreateOutputDocument(string originalDocName)
        {
            string outputDocumentName = GenerateOutputDocumentName(originalDocName);
            OutputDocument outputDocument = new OutputDocument(outputDocumentName);
            outputDocument.DocumentOutputting += HandelOutputDocument_DocumentOutputting;
            outputDocument.DocumentOutput += HandleOutputDocument_DocumentOutput;
            _pendingDocuments.Add(outputDocument);

            return outputDocument;
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
        public IEnumerable<Page> GetDocumentPages(string fileName, int? pageNumber)
        {
            // See if the document indicated is already open as a SourceDocument.
            SourceDocument document = _sourceDocuments
                .Where(doc => doc.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                .SingleOrDefault();

            // If not, open it as a SourceDocument.
            if (document == null)
            {
                document = OpenDocument(fileName);
            }

            // Document may be null if the document was already open; don't treat null as an error,
            // just ignore.
            if (document == null)
            {
                yield break;
            }

            ExtractException.Assert("ELI35506", "Cannot find source page(s).",
                pageNumber == null || document.Pages.Count >= pageNumber,
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
                return _cutMenuItem;
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
                return _copyMenuItem;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a paste operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem PasteMenuItem
        {
            get
            {
                return _pasteMenuItem;
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
                return _deleteMenuItem;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger an un-delete operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem UnDeleteMenuItem
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
                return _printMenuItem;
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
                return _toggleDocumentSeparatorMenuItem;
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
                return _outputDocumentToolStripButton;
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
                return _outputSelectedDocumentsMenuItem;
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

                ResetPrimaryPageLayoutControl();

                // If not using pre-determined settings, the settings dialog should be displayed
                // first to allow the user to confirm/edit settings before input documents are
                // loaded.
                if (!_usingPredeterminedSettings)
                {
                    if (ShowSettingsDialog() == DialogResult.OK)
                    {
                        // Cause window beneath settings dialog to update before load so as not to leave
                        // "ghost" of settings dialog while loading.
                        Update();

                        LoadMorePages();
                    }
                }
                else
                {
                    // If the settings have been specified via command line, consider them accepted.
                    _hasAcceptedSettings = true;

                    LoadMorePages();
                }
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
                // Make sure the user.config file is in a good state
                // https://extract.atlassian.net/browse/ISSUE-12830
                UserConfigChecker.EnsureValidUserConfigFile();

                if (_pendingDocuments.Any(document => !document.InOriginalForm))
                {
                    DialogResult response = MessageBox.Show(
                        "All changes not yet output will be lost.\r\n\r\n" +
                        "Are you sure you want to exit?", "Exit?",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, 0);

                    if (response == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                // [DotNetRCAndUtils:960]
                // Dereference clipboard data to allow documents that should be considered
                // "processed" to be deleted/moved, but do it before any page controls are removed
                // or disposed of, otherwise this will cause documents to moved/processed when they
                // haven't been.
                _primaryPageLayoutControl.DereferenceLastClipboardData();

                base.OnClosing(e);
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
                _primaryPageLayoutControl.Focus();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35520");
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
                // When the output filename textbox has focus, allow only the Ctrl + S shortcut to
                // be handled (saves output file). Otherwise the text box should get all input.
                if (!_outputFileNameToolStripTextBox.Focused ||
                    (Control.ModifierKeys == Keys.Control && keyData == Keys.S))
                {
                    // Otherwise, allow the image viewer to handle keyboard input for shortcuts.
                    if (_imageViewer.Shortcuts.ProcessKey(keyData))
                    {
                        return true;
                    }
                }

                // This key was not processed, bubble it up to the base class.
                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35663", ex);
            }

            return true;
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

                    if (_inputFileEnumerator != null)
                    {
                        _inputFileEnumerator.Dispose();
                        _inputFileEnumerator = null;
                    }

                    if (_inputFolderWatcher != null)
                    {
                        _inputFolderWatcher.Dispose();
                        _inputFolderWatcher = null;
                    }

                    lock (_sourceDocumentLock)
                    {
                        // Unload the source documents from the image viewer to unlock the files.
                        foreach (var document in _sourceDocuments)
                        {
                            _imageViewer.UnloadImage(document.FileName);
                        }

                        CollectionMethods.ClearAndDispose(_sourceDocuments);
                    }

                    if (_pagesPendingLoad != null)
                    {
                        foreach (var page in _pagesPendingLoad)
                        {
                            page.Dispose();
                        }
                        _pagesPendingLoad = null;
                    }

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
                if (ShowSettingsDialog() == DialogResult.OK)
                {
                    // Cause window beneath settings dialog to update before load so as not to leave
                    // "ghost" of settings dialog while loading.
                    Update();

                    LoadMorePages();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35523");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_loadNextDocumentMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleLoadNextDocumentMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_loadingNextDocument && !LoadNextDocument())
                {
                    UtilityMethods.ShowMessageBox("No more input documents were found.",
                        "No more input documents", false);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35590");
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
                // [DotNetRCAndUtils:963, 970]
                // If the settings have not yet been accepted, display the settings dialog.
                if (!_hasAcceptedSettings)
                {
                    if (ShowSettingsDialog() == DialogResult.OK)
                    {
                        Restart();
                    }
                }
                // Otherwise, display a prompt to have user confirm restart.
                else if (PromptForRestart(false))
                {
                    Restart();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35524");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_exitToolStripMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35662");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:ToolStripMenuItem.DropDownOpening"/> event of the
        /// <see cref="_editMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleEditMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    _primaryPageLayoutControl.UpdateCommandStates();
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI35594");
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35591");
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
            try
            {
                _primaryPageLayoutControl.HandleCutSelectedControls();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35595");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_copyMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCopyMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _primaryPageLayoutControl.HandleCopySelectedControls();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35596");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_toggleDocumentSeparatorMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleInsertDocumentSeparator_Click(object sender, EventArgs e)
        {
            try
            {
                _primaryPageLayoutControl.HandleToggleDocumentSeparator(addOnly: false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35597");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_pasteMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePasteMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _primaryPageLayoutControl.HandlePaste();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35598");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_deleteMenuItem"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDeleteMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _primaryPageLayoutControl.HandleDeleteSelectedItems();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35599");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_aboutToolStripMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AboutBox aboutBox = new AboutBox();
                aboutBox.ShowDialog();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35589");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_outputDocumentToolStripButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOutputDocuments_Click(object sender, EventArgs e)
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
                    // Whenever pages are removed, call GetClipboardData to remove references to
                    // and pages that were on the clipboard, but are no longer on the clipboard.
                    // This ensures input documents no longer being used are moved/deleted in a
                    // timely fashion.
                    _primaryPageLayoutControl.GetClipboardData();

                    CheckIfSourceDocumentsProcessed(e.Page.SourceDocument);

                    OutputDocument outputDocument = e.OutputDocument;
                    if (outputDocument != null && outputDocument.PageControls.Count == 0)
                    {
                        outputDocument.DocumentOutputting -= HandelOutputDocument_DocumentOutputting;
                        outputDocument.DocumentOutput -= HandleOutputDocument_DocumentOutput;
                        _pendingDocuments.Remove(outputDocument);
                    }

                    InvokeLoadMorePages();
                });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35528");
            }
        }

        /// <summary>
        /// Handles the <see cref="PageLayoutControl.PagesDereferenced"/> event of an
        /// <see cref="PageLayoutControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaginationUtility.PagesDereferencedEventArgs"/> instance
        /// containing the event data.</param>
        void HandlePageLayoutControl_PagesDereferenced(object sender, PagesDereferencedEventArgs e)
        {
            try
            {
                CheckIfSourceDocumentsProcessed(e.Pages
                    .Where(page => page != null)
                    .Select(page => page.SourceDocument)
                    .Distinct()
                    .ToArray());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39506");
            }
        }

        /// <summary>
        /// Handles the <see cref="PageLayoutControl.LoadNextDocumentRequest"/> event of the
        /// <see cref="_primaryPageLayoutControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleLayoutControl_LoadNextDocumentRequest(object sender, EventArgs e)
        {
            try
            {
                if (!_loadingNextDocument && !LoadNextDocument())
                {
                    UtilityMethods.ShowMessageBox("No more input documents were found.",
                        "No more input documents", false);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35627");
            }
        }

        /// <summary>
        /// Handles the <see cref="OutputDocument.DocumentOutputting"/> event of an
        /// <see cref="OutputDocument"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance
        /// containing the event data.</param>
        void HandelOutputDocument_DocumentOutputting(object sender, CancelEventArgs e)
        {
            try
            {
                if (!ValidateOutputFileName((OutputDocument)sender, true))
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                throw ex.AsExtract("ELI35605");
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
                throw ex.AsExtract("ELI35529");
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
                // Set to null until enabled and active so that any changes to the
                // _outputFileNameToolStripTextBox in this method don't get treated as a user
                // specified name.
                _fileNameEditableDocument = null;

                if (_primaryPageLayoutControl.PartiallySelectedDocuments.Count() == 1)
                {
                    OutputDocument selectedDocument =
                        _primaryPageLayoutControl.PartiallySelectedDocuments.Single();

                    // If there is only one document with at least one page selected, set the output
                    // filename.
                    _lastValidDocumentName = selectedDocument.FileName;
                    _outputFileNameToolStripTextBox.Text = selectedDocument.FileName;

                    // If the document is fully selected, set the pages label to indicate the number
                    // of pages in the document.
                    if (_primaryPageLayoutControl.FullySelectedDocuments.Count() == 1)
                    {
                        _outputFileNameToolStripTextBox.Enabled = true;
                        _outputFileNameBrowseToolStripButton.Enabled = true;
                        _fileNameEditableDocument = selectedDocument;

                        int pageCount = selectedDocument.PageControls.Where(page => !page.Deleted).Count();
                        if (pageCount == 1)
                        {
                            _pagesToolStripLabel.Text = "1 page";
                        }
                        else
                        {
                            _pagesToolStripLabel.Text =
                                string.Format(CultureInfo.CurrentCulture, "{0:D} pages",
                                    pageCount);
                        }
                    }
                    else
                    {
                        // If the document is not fully selected, disable the output filename text
                        // box.
                        _outputFileNameToolStripTextBox.Enabled = false;
                        _outputFileNameBrowseToolStripButton.Enabled = false;

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
                    _lastValidDocumentName = null;
                    _outputFileNameToolStripTextBox.Text = "";
                    _outputFileNameToolStripTextBox.Enabled = false;
                    _outputFileNameBrowseToolStripButton.Enabled = false;
                    _pagesToolStripLabel.Text = "";
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35530");
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
        /// Handles the <see cref="Control.TextChanged"/> event of the
        /// <see cref="_outputFileNameToolStripTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOutputFileNameToolStripTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_fileNameEditableDocument != null)
                {
                    _fileNameEditableDocument.FileName = _outputFileNameToolStripTextBox.Text;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35606");
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
                if (!ValidateOutputFileName() && !string.IsNullOrEmpty(_lastValidDocumentName))
                {
                    // If validation failed, restore the last valid filename.
                    _outputFileNameToolStripTextBox.Text = _lastValidDocumentName;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35533");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Leave"/> event of the
        /// <see cref="_outputFileNameToolStripTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOutputFileNameToolStripTextBox_Leave(object sender, EventArgs e)
        {
            try
            {
                // Make sure that focus is returned to _primaryPageLayoutControl whenever the
                // filename text box loses focus, otherwise the application may appear
                // non-responsive.
                _primaryPageLayoutControl.Focus();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35609");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.EnabledChanged"/> event of the
        /// <see cref="_outputFileNameToolStripTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOutputFileNameToolStripTextBox_EnabledChanged(object sender, EventArgs e)
        {
            if (!_outputFileNameToolStripTextBox.Enabled)
            {
                // Make sure that focus is returned to _primaryPageLayoutControl whenever the
                // filename text box is disabled, otherwise the application may appear
                // non-responsive.
                _primaryPageLayoutControl.Focus();
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
                    saveFileDialog.OverwritePrompt = false;

                    // Show the dialog
                    while (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        ExtractException.Assert("ELI35608", "Filename not editable.",
                            _fileNameEditableDocument != null);

                        // Return the selected file path if it is valid
                        _outputFileNameToolStripTextBox.Text = saveFileDialog.FileName;
                        _fileNameEditableDocument.FileName = _outputFileNameToolStripTextBox.Text;
                        if (ValidateOutputFileName())
                        {
                            return;
                        }
                    }

                    // If the user cancelled, restore the name that had previously been in the text
                    // box.
                    _outputFileNameToolStripTextBox.Text = _lastValidDocumentName;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35534", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.KeyDown"/> event of the <see cref="_splitContainer"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing
        /// the event data.</param>
        void HandleSplitContainer_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                // Don't allow the split container to handle any keyboard input, instead redirect
                // focus back to _primaryPageLayoutControl.
                _primaryPageLayoutControl.Focus();
                e.Handled = true;

                // If an arrow key is pressed, go ahead an execute the corresponding selection
                // command.
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        _primaryPageLayoutControl.HandleSelectPreviousPage();
                        break;

                    case Keys.Right:
                        _primaryPageLayoutControl.HandleSelectNextPage();
                        break;

                    case Keys.Up:
                        _primaryPageLayoutControl.HandleSelectPreviousRowPage();
                        break;

                    case Keys.Down:
                        _primaryPageLayoutControl.HandleSelectNextRowPage();
                        break;

                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35592");
            }
        }

        /// <summary>
        /// handler for selection of "Print selected document(s)" from File menu
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        void HandlePrintMenuItem_Click(object sender, EventArgs e)
        {
            _primaryPageLayoutControl.HandlePrintSelectedItems();
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Shows the settings dialog and applies the settings (if applicable).
        /// </summary>
        /// <returns>The <see cref="DialogResult"/>.</returns>
        DialogResult ShowSettingsDialog()
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

                            Restart();
                        }
                        else
                        {
                            // If the user chose not to restart, restore the previous settings.
                            _config.Load();
                        }
                    }
                    else
                    {
                        // If any non-critical settings change or the utility was not yet active,
                        // go ahead and apply the settings without a restart.
                        _config.Save();
                    }
                }

                _hasAcceptedSettings |= (dialogResult == DialogResult.OK);

                return dialogResult;
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
                messageBox.AddButton("Cancel", "Cancel", true);
                messageBox.Show();

                return (messageBox.Result == "Restart");
            }
        }

        /// <summary>
        /// Clears all active <see cref="PaginationControl"/>s and their associated data and
        /// restores the utility to a state equivalent to just having opened the utility.
        /// </summary>
        void Restart()
        {
            try
            {
                using (new TemporaryWaitCursor())
                {
                    // Hide the page layout control and close the image viewer when resetting to
                    // avoid having previous displayed controls/images visible while the restart is
                    // in progress.
                    // ResetPrimaryPageLayoutControl will create a new _primaryPageLayoutControl, so
                    // there will be no need to make this one visible again.
                    _primaryPageLayoutControl.Visible = false;
                    if (_imageViewer.IsImageAvailable)
                    {
                        _imageViewer.CloseImage();
                    }
                    Refresh();

                    if (_inputFileEnumerator != null)
                    {
                        _inputFileEnumerator.Dispose();
                        _inputFileEnumerator = null;
                    }

                    if (_inputFolderWatcher != null)
                    {
                        _inputFolderWatcher.Dispose();
                        _inputFolderWatcher = null;
                    }

                    lock (_sourceDocumentLock)
                    {
                        // Unload the source documents from the image viewer to unlock the files.
                        foreach (var document in _sourceDocuments)
                        {
                            _imageViewer.UnloadImage(document.FileName);
                        }

                        CollectionMethods.ClearAndDispose(_sourceDocuments);
                    }

                    ResetPrimaryPageLayoutControl();

                    _pendingDocuments.Clear();
                    _failedFileNames.Clear();

                    LoadMorePages();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35535");
            }
        }

        /// <summary>
        /// Clears and disposes of any existing <see cref="_primaryPageLayoutControl"/> and
        /// initializes a new one.
        /// </summary>
        void ResetPrimaryPageLayoutControl()
        {
            if (_primaryPageLayoutControl != null)
            {
                _primaryPageLayoutControl.StateChanged -= HandlePageLayoutControl_StateChanged;
                _primaryPageLayoutControl.PageDeleted -= HandlePageLayoutControl_PageDeleted;
                _primaryPageLayoutControl.PagesDereferenced -= HandlePageLayoutControl_PagesDereferenced;
                _primaryPageLayoutControl.LoadNextDocumentRequest -= HandleLayoutControl_LoadNextDocumentRequest;
                // If the control contains a lot of pagination controls, it can take a long time to
                // remove-- I am unclear why (it has to do with more than just whether thumbnails
                // are still being loaded. Disposing of the control first allows it to be quickly
                // removed.
                _primaryPageLayoutControl.Dispose();
                _pageLayoutToolStripContainer.ContentPanel.Controls.Remove(_primaryPageLayoutControl);

                // Perform a GC to force a cleanup of everything before loading the new
                // _primaryPageLayoutControl
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            _primaryPageLayoutControl = new PageLayoutControl(this);
            _primaryPageLayoutControl.Dock = DockStyle.Fill;
            _primaryPageLayoutControl.ImageViewer = _imageViewer;
            _primaryPageLayoutControl.LoadNextDocumentVisible = true;
            _primaryPageLayoutControl.StateChanged += HandlePageLayoutControl_StateChanged;
            _primaryPageLayoutControl.PageDeleted += HandlePageLayoutControl_PageDeleted;
            _primaryPageLayoutControl.PagesDereferenced += HandlePageLayoutControl_PagesDereferenced;
            _primaryPageLayoutControl.LoadNextDocumentRequest += HandleLayoutControl_LoadNextDocumentRequest;
            _pageLayoutToolStripContainer.ContentPanel.Controls.Add(_primaryPageLayoutControl);
            _primaryPageLayoutControl.Focus();
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
                    // If there is an existing lock file, if it can be opened the instance
                    // that created it is not still open and it can therefore be ignored.
                    _lockFile = File.Open(lockFileName, FileMode.OpenOrCreate,
                        FileAccess.ReadWrite, FileShare.None);
                    File.SetAttributes(lockFileName, 
                        FileAttributes.Hidden | FileAttributes.NotContentIndexed);
                    _lockFileName = lockFileName;
                }
                catch
                {
                    // But if it couldn't be deleted, another instance of the pagination
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
        /// <see paramref="originalDocName"/> that is unique compared to any pending output document, any
        /// existing document or any document that has been output by this instance whether or not
        /// the file still exists.
        /// </summary>
        /// <param name="originalDocName">The filename that should serve as the basis for the new document
        /// name.</param>
        /// <returns>The unique document name.</returns>
        public string GenerateOutputDocumentName(string originalDocName)
        {
            // If fileName is from the input folder and we are to preserve the sub-folder hierarchy,
            // replace the input folder with the output folder in the path.
            if (_config.Settings.PreserveSubFoldersInOutput &&
                originalDocName.StartsWith(_config.Settings.InputFolder, StringComparison.OrdinalIgnoreCase))
            {
                originalDocName = originalDocName.Substring(_config.Settings.InputFolder.Length);
                originalDocName = originalDocName.TrimStart('\\');
                originalDocName = Path.Combine(_config.Settings.OutputFolder, originalDocName);
            }
            else
            {
                // Otherwise, user the filename without directory as the basis for a file to be
                // output into the root of the output directory.
                originalDocName = Path.Combine(_config.Settings.OutputFolder, Path.GetFileName(originalDocName));
            }

            string baseFilename = originalDocName;
            string directory = Path.GetDirectoryName(baseFilename);
            string extension = Path.GetExtension(baseFilename);
            baseFilename = Path.GetFileNameWithoutExtension(baseFilename);

            if (_config.Settings.RandomizeOutputFileName)
            {
                originalDocName = Path.Combine(directory, baseFilename + "_" +
                    UtilityMethods.GetRandomString(10, true, false, true) +
                    extension);
            }

            if (IsOutputFileNameAvailable(originalDocName, false, false))
            {
                return originalDocName;
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

                if (IsOutputFileNameAvailable(newFileName, false, false))
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
        /// <param name="sourceDocumentName">The name of the source document to be moved.</param>
        /// <returns>The unique document name.</returns>
        string GenerateProcessedDocumentName(string sourceDocumentName)
        {
            string fileName = sourceDocumentName;

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

                if (IsOutputFileNameAvailable(newFileName, false, false))
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

            // Initialize a folder watcher that will be activated to watch for added files once there
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
            return LoadNextDocument(false);
        }

        /// <summary>
        /// Loads the next input document into the UI.
        /// </summary>
        /// <param name="recursiveCall"><see langword="true"/> if the this is a recursive call;
        /// <see langword="false"/> otherwise.</param>
        bool LoadNextDocument(bool recursiveCall)
        {
            // [DotNetRCAndUtils:940, 943]
            // Don't allow multiple simultaneous calls into LoadNextDocument unless via direct
            // recursion. Though all calls to this method should happen on the same thread, because
            // the CreateOutputDocument call below calls Application.DoEvents, it can allow for a
            // second call to occur while the first is still in progress.
            if (!recursiveCall)
            {
                if (_loadingNextDocument)
                {
                    return false;
                }
                else
                {
                    _loadingNextDocument = true;
                }
            }

            try
            {
                if (_inputFileEnumerator == null)
                {
                    StartInputEnumeration();
                }

                while (_inputFileEnumerator.MoveNext())
                {
                    string fileName = _inputFileEnumerator.Current;
                    if (_failedFileNames.Contains(fileName))
                    {
                        continue;
                    }

                    // Do not load hidden or system files.
                    FileAttributes attributes = File.GetAttributes(fileName);
                    if (attributes.HasFlag(FileAttributes.Hidden) ||
                        attributes.HasFlag(FileAttributes.System))
                    {
                        continue;
                    }

                    try
                    {
                        var sourceDocument = OpenDocument(fileName);

                        if (sourceDocument != null)
                        {
                            // This will call Application.DoEvents in the midst of loading a document to
                            // keep the UI responsive as pages are loaded. This allows an opportunity
                            // for there to be multiple calls into LoadNextDocument at the same time.
                            _primaryPageLayoutControl.CreateOutputDocument(sourceDocument,
                                pages: null, deletedPages: null, viewedPages: null, position: -1,
                                insertSeparator: _config.Settings.AutoInsertDocumentBoundaries);

                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        var ee = new ExtractException("ELI35588",
                            "Unable to load document; document will be ignored until restarted", ex);
                        ee.AddDebugData("Filename", fileName, false);
                        ee.Display();

                        _failedFileNames.Add(fileName);
                    }
                }

                // If we reached the end of the enumeration, enable the _inputFolderWatcher to notify
                // when any new files are added, then restart the enumeration (unless it is specified
                // that the restart is not necessary to prevent recursion).
                if (!recursiveCall)
                {
                    _inputFolderWatcher.EnableRaisingEvents = true;

                    StartInputEnumeration();
                    if (LoadNextDocument(true))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35631");
            }
            finally
            {
                if (!recursiveCall)
                {
                    _loadingNextDocument = false;
                }
            }
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
            SourceDocument sourceDocument = null;

            lock (_sourceDocumentLock)
            {
                if (!File.Exists(inputFileName) ||
                    _sourceDocuments.Any(doc =>
                        doc.FileName.Equals(inputFileName, StringComparison.OrdinalIgnoreCase)))
                {
                    return null;
                }

                sourceDocument = new SourceDocument(inputFileName, -1);
                if (!sourceDocument.Pages.Any())
                {
                    sourceDocument.Dispose();
                    return null;
                }

                _sourceDocuments.Add(sourceDocument);
            }

            ThreadingMethods.RunInBackgroundThread("ELI35541", () =>
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
        /// Gets a value indicating whether more pages can attempt to be loaded.
        /// </summary>
        /// <value><see langword="true"/> if more pages can attempt to be loaded; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool CanAttemptPageLoad
        {
            get
            {
                // True if there are pages from previously loaded documents pending and room beneath
                // _MAX_LOADED_PAGES
                int pendingPagesToLoadCount = Math.Min(_pagesPendingLoad.Count,
                    _MAX_LOADED_PAGES - _primaryPageLayoutControl.PageCount);
                if (pendingPagesToLoadCount > 0)
                {
                    return true;
                }

                // True if there are not the configured number of pages currently loaded.
                if (_primaryPageLayoutControl.PageCount < _config.Settings.InputPageCount)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Loads the more pages from input folder documents until there are greater than or equal
        /// number of pages loaded as _config.Settings.InputPageCount.
        /// </summary>
        void LoadMorePages()
        {
            if (_loadingNextDocument)
            {
                return;
            }

            // Check to see if there are any pages from previously loaded documents that are still
            // waiting to be loaded.
            int pendingPagesToLoadCount = Math.Min(_pagesPendingLoad.Count,
                _MAX_LOADED_PAGES - _primaryPageLayoutControl.PageCount);
            if (pendingPagesToLoadCount > 0)
            {
                Page[] pagesToLoad = new Page[pendingPagesToLoadCount];
                for (int i = 0; i < pendingPagesToLoadCount; i++)
                {
                    pagesToLoad[i] = _pagesPendingLoad.Dequeue();
                }
                
                _primaryPageLayoutControl.AddPages(pagesToLoad);
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

            if (_primaryPageLayoutControl.PageCount == 0)
            {
                // If no documents were loaded, call CreateOutputDocument so that the load next
                // document button gets added.
                _primaryPageLayoutControl.CreateOutputDocument(null,
                    pages: null, deletedPages: null, viewedPages: null, position: -1,
                    insertSeparator: _config.Settings.AutoInsertDocumentBoundaries);
            }

            // As long as there are no pages pending to be loaded, enable the load next document
            // options.
            if (_pagesPendingLoad.Count == 0)
            {
                _loadNextDocumentMenuItem.Enabled = true;
                _primaryPageLayoutControl.EnableLoadNextDocument = true;
            }
        }

        /// <summary>
        /// Invokes <see cref="LoadMorePages"/> on the message queue if more pages are needed to
        /// reach <see cref="Settings.InputPageCount"/>
        /// </summary>
        void InvokeLoadMorePages()
        {
            if (!_pageLoadPending)
            {
                _pageLoadPending = true;
                this.SafeBeginInvoke("ELI35542", () =>
                {
                    _pageLoadPending = false;

                    if (CanAttemptPageLoad)
                    {
                        LoadMorePages();
                    }
                });
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
            // the input file can be deleted/moved as configured.
            IEnumerable<SourceDocument> processedDocuments = sourceDocuments
                .Where(document => document.Pages.All(page => !page.HasActiveReference));

            lock (_sourceDocumentLock)
            {
                foreach (SourceDocument document in processedDocuments
                        .Where(document => _sourceDocuments.Contains(document)))
                {
                    string documentName = document.FileName;

                    // [DotNetRCAndUtils:975]
                    // Ensure the source document is disposed of before calling
                    // HandleProcessedSourceDocument, otherwise the thumbnail worker may still have
                    // a lock on the source document when the delete/move attempt occurs.
                    document.Dispose();
                    _sourceDocuments.Remove(document);

                    // Unload the document from the image viewer to unlock the file.
                    _imageViewer.UnloadImage(documentName);

                    HandleProcessedSourceDocument(documentName);
                } 
            }
        }

        /// <summary>
        /// Handles the <see paramref="document"/> that has completed processing due to all its
        /// pages either being output or deleted.
        /// </summary>
        /// <param name="sourceDocumentName">The name of the source document that has been processed.
        /// </param>
        void HandleProcessedSourceDocument(string sourceDocumentName)
        {
            // If the document came from the input folder (as opposed to having been dragged in
            // from another folder), either move or delete the file from the input folder now
            // that it has been processed.
            if ((_config.Settings.IncludeSubfolders && sourceDocumentName.StartsWith(
                _config.Settings.InputFolder, StringComparison.OrdinalIgnoreCase) ||
                (!_config.Settings.IncludeSubfolders &&
                    Path.GetDirectoryName(sourceDocumentName).Equals(
                        _config.Settings.InputFolder, StringComparison.OrdinalIgnoreCase))))
            {
                // This removes the image from the cache and releases the lock on the file.
                _imageViewer.UnloadImage(sourceDocumentName);

                if (_config.Settings.DeleteProcessedFiles)
                {
                    FileSystemMethods.DeleteFile(sourceDocumentName);
                }
                else
                {
                    string processedDocumentName = GenerateProcessedDocumentName(sourceDocumentName);

                    string directory = Path.GetDirectoryName(processedDocumentName);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    FileSystemMethods.MoveFile(sourceDocumentName, processedDocumentName,
                        false);
                    _processedDocumentNames.Add(processedDocumentName);
                }
            }
        }

        #endregion Private Members
    }
}
