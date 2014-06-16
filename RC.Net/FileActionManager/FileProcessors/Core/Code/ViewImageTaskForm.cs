using Extract.FileActionManager.Forms;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using TD.SandDock;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents a task that displays a form allowing the user to view images.
    /// </summary>
    [CLSCompliant(false)]
    public sealed partial class ViewImageTaskForm : Form, IVerificationForm
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
            FileSystemMethods.ApplicationDataPath, "ImageViewerTask", "ImageViewerTaskForm.xml");

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

        #endregion Fields

        #region Events

        /// <summary>
        /// Occurs when a file has been "completed" (done viewing in this case).
        /// </summary>
        public event EventHandler<FileCompleteEventArgs> FileComplete;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewImageTaskForm"/> class.
        /// </summary>
        public ViewImageTaskForm()
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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Updates the properties of the controls based on the currently open image.
        /// </summary>
        void UpdateControls()
        {
            if (_imageViewer.IsImageAvailable)
            {
                Text = Path.GetFileName(_currentFileName);

                _tagFileToolStripButton.Enabled = true;
                _tagFileToolStripButton.FileId = _currentFileId;

                _nextDocumentToolStripButton.Enabled = true;
                _nextDocumentToolStripMenuItem.Enabled = true;
            }
            else
            {
                Text = " (Waiting for file)";

                _tagFileToolStripButton.Enabled = false;

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
                if (_fileDatabase != null &&
                    (_fileDatabase.GetTagNames().Size > 0 || _fileDatabase.AllowDynamicTagCreation()))
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
                // Establish image viewer connections prior to calling base.OnLoad which will
                // potentially remove some IImageViewerControls.
                _imageViewer.EstablishConnections(this);

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

                // Save and commit
                _imageViewer.Shortcuts[Keys.Tab | Keys.Control] = GoToNextDocument;

                // Magnifier window
                _imageViewer.Shortcuts[Keys.F12] = (() => _magnifierToolStripButton.PerformClick());
                
                // Thumbnails window
                _imageViewer.Shortcuts[Keys.F10] = (() => _thumbnailsToolStripButton.PerformClick());

                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI37054", ex);
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
        /// Raises the <see cref="FileComplete"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="FileComplete"/> 
        /// event.</param>
        void OnFileComplete(FileCompleteEventArgs e)
        {
            if (FileComplete != null)
            {
                FileComplete(this, e);
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
                ex.ExtractDisplay("ELI37055");
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
                ex.ExtractDisplay("ELI37056");
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
                ExtractException.Display("ELI37059", ex);
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

        /// <summary>
        /// Handles the<see cref="ImageViewer.ImageFileClosing"/> event to ensure the previously
        /// loaded image is unloaded from the image reader cache in stand-alone mode.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.ImageFileClosingEventArgs"/>
        /// instance containing the event data.</param>
        void HandleImageViewerImageFileClosing(object sender, ImageFileClosingEventArgs e)
        {
            try
            {
                CancelFile();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37062");
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
                _invoker.HandleException(ee);
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

        #endregion IVerificationForm Members

        #region Private Members

        /// <summary>
        /// Closes the current document and advances to the next document in the FAM queue.
        /// </summary>
        void GoToNextDocument()
        {
            _imageViewer.CloseImage();

            OnFileComplete(new FileCompleteEventArgs(EFileProcessingResult.kProcessingSuccessful));
        }

        /// <summary>
        /// Skips the current document and advances to the next document in the FAM queue.
        /// </summary>
        void SkipFile()
        {
            _imageViewer.CloseImage();

            OnFileComplete(new FileCompleteEventArgs(EFileProcessingResult.kProcessingSkipped));
        }

        /// <summary>
        /// Cancels the current document and stops processing in the FAM.
        /// </summary>
        void CancelFile()
        {
            _imageViewer.CloseImage();

            OnFileComplete(new FileCompleteEventArgs(EFileProcessingResult.kProcessingCancelled));
        }

        #endregion Private Members
    }
}
