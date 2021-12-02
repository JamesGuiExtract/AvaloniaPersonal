using Extract.FileActionManager.Forms;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TD.SandDock;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents a task that displays a form allowing the user to view images.
    /// </summary>
    [CLSCompliant(false)]
    public sealed partial class ViewImageTaskForm : Form, IVerificationForm, IApplicationWithInactivityTimeout
    {
        #region Constants

        /// <summary>
        /// The license string for the SandDock manager
        /// </summary>
        static readonly string _SANDDOCK_LICENSE_STRING = @"1970|siE7SnF/jzINQg1AOTIaCXLlouA=";

        /// <summary>
        /// The full path to the file that contains information about persisting the 
        /// <see cref="ViewImageTaskForm"/>.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_FILE = FileSystemMethods.PathCombine(
            FileSystemMethods.UserApplicationDataPath, "ImageViewerTask", "ImageViewerTaskForm.xml");

        /// <summary>
        /// The title to display for the view image task form.
        /// </summary>
        const string _FORM_TASK_TITLE = "View Image";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ViewImageTaskForm).ToString();

        /// <summary>
        /// Name for the mutex used to serialize persistance of the control and form layout.
        /// </summary>
        static readonly string _MUTEX_STRING = "87DC2DDE-35E2-4892-903E-D87216EA2553";

        #endregion Constants

        #region Fields

        /// <summary>
        /// A <see cref="ViewImageTask"/> representing the settings to use in the form.
        /// </summary>
        ViewImageTask _settings;

        /// <summary>
        /// The file processing database.
        /// </summary>
        FileProcessingDB _fileDatabase;

        /// <summary>
        /// The name of the currently loaded file.
        /// </summary>
        string _currentFileName;

        /// <summary>
        /// The FAMFile ID of the currently loaded file.
        /// </summary>
        int _currentFileId;

        /// <summary>
        /// Used to invoke methods on this control.
        /// </summary>
        readonly ControlInvoker _invoker;

        /// <summary>
        /// Saves/restores window state info and provides full screen mode.
        /// </summary>
        FormStateManager _formStateManager;

        private ExtractTimeout _sessionTimeoutManager;

        #endregion Fields

        #region Events

        /// <summary>
        /// This event indicates the verification form has been initialized and is ready to load a
        /// document.
        /// </summary>
        public event EventHandler<EventArgs> Initialized;

        /// <summary>
        /// Occurs when a file has been "completed" (done viewing in this case).
        /// </summary>
        public event EventHandler<FileCompleteEventArgs> FileComplete;

        /// <summary>
        /// This event is not raised by <see cref="ViewImageTaskForm"/>.
        /// </summary>
        public event EventHandler<FileRequestedEventArgs> FileRequested
        {
            // Since this event is not currently used by this class but is needed by the 
            // IVerificationForm interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        /// <summary>
        /// This event is not raised by <see cref="ViewImageTaskForm"/>.
        /// </summary>
        public event EventHandler<FileDelayedEventArgs> FileDelayed
        {
            // Since this event is not currently used by this class but is needed by the 
            // IVerificationForm interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        /// <summary>
        /// Raised when exceptions are raised from the verification UI that should result in the
        /// document failing. Generally this will be raised as a result of errors loading or saving
        /// the document as opposed to interacting with a successfully loaded document.
        /// </summary>
        public event EventHandler<VerificationExceptionGeneratedEventArgs> ExceptionGenerated;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewImageTaskForm"/> class.
        /// </summary>
        /// <param name="settings">A <see cref="ViewImageTask"/> representing the settings to use in
        /// the form.</param>
        public ViewImageTaskForm(ViewImageTask settings)
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI37051",
                    _OBJECT_NAME);

                // License SandDock before creating the form
                SandDockManager.ActivateProduct(_SANDDOCK_LICENSE_STRING);

                InitializeComponent();

                _settings = settings;

                _tagFileToolStripButton.TagSettings = _settings.TagSettings;

                // [FlexIDSCore:4442]
                // For prefetching purposes, allow the ImageViewer will cache images.
                _imageViewer.CacheImages = true;

                _invoker = new ControlInvoker(this);

                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    _formStateManager = new FormStateManager(
                        this, _FORM_PERSISTENCE_FILE, _MUTEX_STRING, _sandDockManager, false);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI37052", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets whether the control styles of the current Windows theme should be used for the
        /// verification form.
        /// </summary>
        /// <returns><see langword="true"/> to use the control styles of the current Windows theme;
        /// <see langword="false"/> to use Window's classic theme to draw controls.</returns>
        public bool UseVisualStyles
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the verification form should prevent
        /// any attempts to save dirty data. This may be used after experiencing an error or
        /// when the form is being programmatically closed. (when prompts to save in response to
        /// events that occur are not appropriate)
        /// </summary>
        /// <value><see langword="true"/> if the verification form should prevent any
        /// attempts to save dirty data; otherwise, <see langword="false"/>.</value>
        public bool PreventSaveOfDirtyData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether any cancellation of a form closing event should
        /// be disallowed. This is used to ensure that if the FAM requests a verification task to
        /// stop, that the user can't cancel via a save dirty prompt.
        /// </summary>
        /// <value><see langword="true"/> if cancellation of a form closing event should be
        /// disallowed; otherwise <see langword="false"/>.</value>
        public bool PreventCloseCancel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the form supports displaying multiple documents
        /// simultaneously (one for each processing thread).
        /// </summary>
        /// <value><c>true</c> if the form supports multiple documents, <c>false</c> if only one
        /// document at a time can be loaded.</value>
        public bool SupportsMultipleDocuments
        {
            get
            {
                return false;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Updates the properties of the controls based on the currently open image.
        /// </summary>
        void UpdateControls()
        {
            if (_imageViewer.IsImageAvailable)
            {
                Text = Path.GetFileName(_currentFileName) + " - " + _FORM_TASK_TITLE;

                if (_tagFileToolStripButton.Database != null)
                {
                    _tagFileToolStripButton.Enabled = true;
                    _tagFileToolStripButton.FileId = _currentFileId;
                }
                _nextDocumentToolStripButton.Enabled = true;
                _nextDocumentToolStripMenuItem.Enabled = true;
            }
            else
            {
                Text = "(Waiting for file) - " + _FORM_TASK_TITLE;

                // If the database is null, trying to enable the button will trigger an exception.
                if (_tagFileToolStripButton.Database != null)
                {
                    _tagFileToolStripButton.Enabled = true;
                }

                _nextDocumentToolStripButton.Enabled = false;
                _nextDocumentToolStripMenuItem.Enabled = false;
            }
        }

        /// <summary>
        /// Sets the active file processing DB.
        /// </summary>
        /// <param name="database">The file processing database to store.</param>
        void SetFAMDB(FileProcessingDB database)
        {
            if (_fileDatabase != database)
            {
                if (_fileDatabase != null)
                {
                    throw new ExtractException("ELI37053", "File processing database mismatch.");
                }

                // Store the file processing database
                _fileDatabase = database;
                _tagFileToolStripButton.Database = database;

                // Check if the tag file toolstrip button should be displayed [FlexIDSCore #3886]
                // If tagging feature is turned on and either
                // - There are tags available to use (i.e., won't be available if a tag filter
                //   excludes all available tags)
                // - Dynamic tag creation is turned on and the "Display all tags" option is selected.
                //   (i.e., don't allow users the ability to dynamically create tags if there are
                //   any limits placed on the tags available to use).
                if (_settings.AllowTags &&
                    (_settings.TagSettings.GetQualifiedTags(_fileDatabase).Any() ||
                     (_settings.TagSettings.UseAllTags && _fileDatabase.AllowDynamicTagCreation())))
                {
                    _tagFileToolStripButton.Visible = true;
                }
                else
                {
                    _tagFileToolStripButton.Visible = false;
                }
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> 
        /// event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                // Before Loading the state make sure the config is still valid
                // https://extract.atlassian.net/browse/ISSUE-12830
                UserConfigChecker.EnsureValidUserConfigFile();

                // Establish image viewer connections prior to calling base.OnLoad which will
                // potentially remove some IImageViewerControls.
                _imageViewer.EstablishConnections(this);

                // The tag button should not be available if no database is available.
                if (_tagFileToolStripButton.Database == null)
                {
                    _tagFileToolStripButton.Visible = false;
                }

                base.OnLoad(e);

                // Set the dockable window that the thumbnail and magnifier toolstrip button control.
                _thumbnailsToolStripButton.DockableWindow = _thumbnailDockableWindow;
                _magnifierToolStripButton.DockableWindow = _magnifierDockableWindow;

                // It is important that this line comes AFTER EstablishConnections, 
                // because the page summary view needs to handle this event FIRST.
                _imageViewer.ImageFileChanged += HandleImageViewerImageFileChanged;

                // Disable open/close image
                _imageViewer.Shortcuts[Keys.O | Keys.Control] = null;
                _imageViewer.Shortcuts[Keys.F4 | Keys.Control] = null;

                // Disable shortcuts for tools that aren't available:
                _imageViewer.Shortcuts[Keys.Escape] = null;
                _imageViewer.Shortcuts[Keys.H] = null;

                // Disable shortcuts for tile navagation
                _imageViewer.Shortcuts[Keys.OemPeriod] = null;
                _imageViewer.Shortcuts[Keys.Oemcomma] = null;

                // Disable shortcuts for layer object navagation
                _imageViewer.Shortcuts[Keys.F3] = null;
                _imageViewer.Shortcuts[Keys.Control | Keys.OemPeriod] = null;
                _imageViewer.Shortcuts[Keys.F3 | Keys.Shift] = null;
                _imageViewer.Shortcuts[Keys.Control | Keys.Oemcomma] = null;

                // Save and commit
                _imageViewer.Shortcuts[Keys.Tab | Keys.Control] = GoToNextDocument;

                // Magnifier window
                _imageViewer.Shortcuts[Keys.F12] = (() => _magnifierToolStripButton.PerformClick());
                
                // Thumbnails window
                _imageViewer.Shortcuts[Keys.F10] = (() => _thumbnailsToolStripButton.PerformClick());

                UpdateControls();

                OnInitialized();
            }
            catch (Exception ex)
            {
                RaiseVerificationException("ELI37054", ex, false);
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
                if (_imageViewer.Shortcuts.ProcessKey(keyData))
                {
                    return true;
                }

                // This key was not processed, bubble it up to the base class.
                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI37066", ex);
            }

            return true;
        }

        /// <summary>
        /// Raises the <see cref="Initialized"/> event.
        /// </summary>
        void OnInitialized()
        {
            var eventHandler = Initialized;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="FileComplete"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="FileComplete"/> 
        /// event.</param>
        void OnFileComplete(FileCompleteEventArgs e)
        {
            var eventHandler = FileComplete;
            if (eventHandler != null)
            {
                eventHandler(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ExceptionGenerated"/> event.
        /// </summary>
        /// <param name="ee">The <see cref="VerificationExceptionGeneratedEventArgs"/> that has been
        /// generated.</param>
        void OnExceptionGenerated(VerificationExceptionGeneratedEventArgs ee)
        {
            var eventHandler = ExceptionGenerated;
            if (eventHandler != null)
            {
                eventHandler(this, ee);
            }
        }

        /// <overloads>Releases resources used by the <see cref="ViewImageTaskForm"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ViewImageTaskForm"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources
                if (components != null)
                {
                    components.Dispose();
                }
                if (_formStateManager != null)
                {
                    _formStateManager.Dispose();
                    _formStateManager = null;
                }
                if (_sessionTimeoutManager != null)
                {
                    _sessionTimeoutManager.Dispose();
                    _sessionTimeoutManager = null;
                }
                // Collapsed or hidden dockable windows must be disposed explicitly [FIDSC #4246]
                // TODO: Can be removed when Divelements corrects this in the next release (3.0.7+)
                if (_thumbnailDockableWindow != null)
                {
                    _thumbnailDockableWindow.Dispose();
                    _thumbnailDockableWindow = null;
                }
                if (_magnifierDockableWindow != null)
                {
                    _magnifierDockableWindow.Dispose();
                    _magnifierDockableWindow = null;
                }
            }

            // Release unmanaged resources

            // Call base dispose method
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event of the
        /// <see cref="_nextDocumentToolStripMenuItem"/>.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleNextDocumentToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                GoToNextDocument();
            }
            catch (Exception ex)
            {
                RaiseVerificationException("ELI37055", ex, true);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event of the
        /// <see cref="_nextDocumentToolStripButton"/>.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleNextDocumentToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                GoToNextDocument();
            }
            catch (Exception ex)
            {
                RaiseVerificationException("ELI37056", ex, true);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleSkipProcessingToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                SkipFile();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37057");
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleStopProcessingToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37058");
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ImageViewer.ImageFileChanged"/> event.</param>
        void HandleImageViewerImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI37059", 
                    "Failed to " + 
                    (_imageViewer.IsImageAvailable ? "load" : "clear") + 
                    " document data!", ex);
                RaiseVerificationException(ee, true);
            }
        }

        /// <summary>
        /// Handles the <see cref="DockControl.DockSituationChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="DockControl.DockSituationChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="DockControl.DockSituationChanged"/> event.</param>
        void HandleThumbnailDockableWindowDockSituationChanged(object sender, EventArgs e)
        {
            try
            {
                // Don't keep loading thumbnails if _thumbnailDockableWindow is closed.
                // [FlexIDSCore:5015]
                // If the window is collapsed it means it is set to auto-hide. It is likely the user
                // will want to check on them in this configuration, so go ahead and load them.
                _thumbnailViewer.Active = _thumbnailDockableWindow.IsOpen ||
                    _thumbnailDockableWindow.Collapsed;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI37060", ex);
            }
        }

        #endregion Event Handlers

        #region IVerificationForm Members

        /// <summary>
        /// A thread-safe method that opens a document for verification.
        /// </summary>
        /// <param name="fileName">The filename of the document to open.</param>
        /// <param name="fileID">The ID of the file being processed.</param>
        /// <param name="actionID">The ID of the action being processed.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
        public void Open(string fileName, int fileID, int actionID, FAMTagManager tagManager,
            FileProcessingDB fileProcessingDB)
        {
            if (InvokeRequired)
            {
                _invoker.Invoke(new VerificationFormOpen(Open),
                    new object[] { fileName, fileID, actionID, tagManager, fileProcessingDB });
                return;
            }

            try
            {
                // Store the file processing database
                SetFAMDB(fileProcessingDB);

                if (_sessionTimeoutManager == null)
                {
                    _sessionTimeoutManager = new ExtractTimeout(this);
                }

                // Get the full path of the source document
                _currentFileName = Path.GetFullPath(fileName);
                _currentFileId = fileID;

                // If the thumbnail viewer is not visible when a document is opened, don't load the
                // thumbnails. (They will get loaded if the thumbnail window is opened at a later
                // time).
                // [FlexIDSCore:5015]
                // If the window is collapsed it means it is set to auto-hide. It is likely the user
                // will want to check on them in this configuration, so go ahead and load them.
                _thumbnailViewer.Active = _thumbnailDockableWindow.IsOpen ||
                    _thumbnailDockableWindow.Collapsed;

                _imageViewer.OpenImage(_currentFileName, false);

                if (!Focused)
                {
                    FormsMethods.FlashWindow(this, true, true);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI37063", "Unable to open file.", ex);
                ee.AddDebugData("File name", fileName, false);
                RaiseVerificationException(ee, true);
            }
        }

        /// <summary>
        /// A thread-safe method that allows loading of data prior to the <see cref="Open"/> call
        /// so as to reduce the time the <see cref="Open"/> call takes once it is called.
        /// </summary>
        /// <param name="fileName">The filename of the document for which to prefetch data.</param>
        /// <param name="fileID">The ID of the file being prefetched.</param>
        /// <param name="actionID">The ID of the action being prefetched.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
        public void Prefetch(string fileName, int fileID, int actionID, FAMTagManager tagManager,
            FileProcessingDB fileProcessingDB)
        {
            try
            {
                _imageViewer.CacheImage(fileName);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI37064", ex);
            }
        }

        /// <summary>
        /// Called to notify the file processor that the pending document queue is empty, but
        ///	the processing tasks have been configured to remain running until the next document
        ///	has been supplied. If the processor will standby until the next file is supplied it
        ///	should return <see langword="true"/>. If the processor wants to cancel processing,
        ///	it should return <see langword="false"/>. If the processor does not immediately know
        ///	whether processing should be cancelled right away, it may block until it does know,
        ///	and return at that time.
        /// <para><b>Note</b></para>
        /// This call will be made on a different thread than the other calls, so the Standby call
        /// must be thread-safe. This allows the file processor to block on the Standby call, but
        /// it also means the form may be opened or closed while the Standby call is still ocurring.
        /// If this happens, the return value of Standby will be ignored; however, Standby should
        /// promptly return in this case to avoid needlessly keeping a thread alive.
        /// </summary>
        /// <returns><see langword="true"/> to standby until the next file is supplied;
        /// <see langword="false"/> to cancel processing.</returns>
        public bool Standby()
        {
            try
            {
                if (InvokeRequired)
                {
                    return (bool)_invoker.Invoke(new VerificationFormStandby(Standby));
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37065");
            }
        }

        /// <summary>
        /// This event is not implemented by <see cref="ViewImageTaskForm"/>.
        /// </summary>
        public void DelayFile(int fileId)
        {
            throw new ExtractException("ELI37504", "Method not implemented.");
        }

        /// <summary>
        /// Executes disposal of any thread-local or thread-static objects just prior to the UI
        /// thread closing.
        /// </summary>
        public void DisposeThread()
        {
            // Nothing to do
        }

        #endregion IVerificationForm Members

        #region IApplicationWithInactivityTimeout

        public TimeSpan SessionTimeout => TimeSpan.FromSeconds(Int32.Parse(_fileDatabase?.GetDBInfoSetting("VerificationSessionTimeout", true), CultureInfo.InvariantCulture));

        public Action EndProcessingAction => () =>
        {
            CancelFile();
        };

        public Control HostControl => this._imageViewer;
        #endregion IApplicationWithInactivityTimeout

        #region Private Members

        /// <summary>
        /// Closes the current document and advances to the next document in the FAM queue.
        /// </summary>
        void GoToNextDocument()
        {
            try
            {
                _imageViewer.CloseImage();

                OnFileComplete(new FileCompleteEventArgs(_currentFileId, EFileProcessingResult.kProcessingSuccessful));
            }
            catch (Exception ex)
            {
                // This method can be called directly via a keyboard shortcut, so need to deal with
                // exceptions here rather than trusting that the outer scope will do so.
                RaiseVerificationException("ELI37832", ex, true);
            }
        }

        /// <summary>
        /// Skips the current document and advances to the next document in the FAM queue.
        /// </summary>
        void SkipFile()
        {
            _imageViewer.CloseImage();

            OnFileComplete(new FileCompleteEventArgs(_currentFileId, EFileProcessingResult.kProcessingSkipped));
        }

        /// <summary>
        /// Cancels the current document and stops processing in the FAM.
        /// </summary>
        void CancelFile()
        {
            _imageViewer.CloseImage();

            OnFileComplete(new FileCompleteEventArgs(_currentFileId, EFileProcessingResult.kProcessingCancelled));
        }

        /// <summary>
        /// Raises the <see cref="ExceptionGenerated"/> event for handling by
        /// <see cref="VerificationForm{T}"/>.
        /// </summary>
        /// <param name="eliCode">The ELI code to be associated with the excpetion.</param>
        /// <param name="ex">The <see cref="Exception"/> that being raised.</param>
        /// <param name="canProcessingContinue"><see langword="true"/> if the user should be given
        /// the option to continue verification on the next document; <see langword="false"/> if the
        /// error should prevent the possibility of continuing the verification session.</param>
        void RaiseVerificationException(string eliCode, Exception ex, bool canProcessingContinue)
        {
            RaiseVerificationException(ex.AsExtract(eliCode), canProcessingContinue);
        }

        /// <summary>
        /// Raises the <see cref="ExceptionGenerated"/> event for handling by
        /// <see cref="VerificationForm{T}"/>.
        /// </summary>
        /// <param name="ee">The <see cref="ExtractException"/> that being raised.</param>
        /// <param name="canProcessingContinue"><see langword="true"/> if the user should be given
        /// the option to continue verification on the next document; <see langword="false"/> if the
        /// error should prevent the possibility of continuing the verification session.</param>
        void RaiseVerificationException(ExtractException ee, bool canProcessingContinue)
        {
            var verificationException =
                new VerificationExceptionGeneratedEventArgs(ee, canProcessingContinue);
            OnExceptionGenerated(verificationException);
        }

        #endregion Private Members
    }
}
