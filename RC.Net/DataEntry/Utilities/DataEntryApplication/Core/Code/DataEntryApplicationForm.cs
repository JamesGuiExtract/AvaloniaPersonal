using Extract;
using Extract.DataEntry;
using Extract.DataEntry.Utilities.DataEntryApplication.Properties;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// The generic application used to run all data entry forms.  The application consists of two
    /// panes:
    /// <list type="bullet">
    /// <item>The Data Entry Panel (DEP) will display the content from a document and allow for the content
    /// to be verifed/corrected.  The DEP consists of a <see cref="DataEntryControlHost"/> instance 
    /// populated by controls which implement <see cref="IDataEntryControl"/>.</item>
    /// <item>The image viewer will display the document image itself and allow for interaction with the
    /// DEP such as highlighting the image area associated with the content currently selected in the DEP
    /// or allowing DEP controls to be populated via OCR "swipes" in the image viewer.</item>
    /// </list>
    /// </summary>
    public partial class DataEntryApplicationForm : Form, IVerificationForm
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataEntryApplicationForm).ToString();

        /// <summary>
        /// The number of pixels to pad around the DEP that is loaded.
        /// </summary>
        const int _DATA_ENTRY_PANEL_PADDING = 3;

        /// <summary>
        /// The value associated with a window's system command message.
        /// </summary>
        const int _WM_SYSCOMMAND = 0x112;

        /// <summary>
        /// The value associated with a window's close system command message.
        /// </summary>
        static readonly IntPtr _SC_CLOSE = (IntPtr)0xF060;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The data entry panel control host implementation to be used by the application.
        /// </summary>
        DataEntryControlHost _dataEntryControlHost;

        /// <summary>
        /// Indicates whether the <see cref="DataEntryApplicationForm"/> is in standalone mode 
        /// (<see langref="true"/>) or whether another application has launched 
        /// <see cref="DataEntryApplicationForm"/> via the COM interface (<see langref="false"/>).
        /// </summary>
        bool _standAloneMode = true;

        /// <summary>
        /// Indicates whether the form has finished loading.
        /// </summary>
        bool _isLoaded;

        /// <summary>
        /// Indicates whether an image is being closed programmatically rather than via a user
        /// action.
        /// </summary>
        bool _forcingClose;

        /// <summary>
        /// The <see cref="FileProcessingDB"/> in use.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The ID of the file being processed.
        /// </summary>
        int _fileID;

        /// <summary>
        /// The ID of the action being processed.
        /// </summary>
        int _actionID;

        /// <summary>
        /// The open file tool strip button.
        /// </summary>
        ToolStripItem _openFileToolStripButton;

        /// <summary>
        /// The open file command
        /// </summary>
        ApplicationCommand _openFileCommand;

        /// <summary>
        /// The close file command
        /// </summary>
        ApplicationCommand _closeFileCommand;

        /// <summary>
        /// The save and commit file command
        /// </summary>
        ApplicationCommand _saveAndCommitFileCommand;

        /// <summary>
        /// The goto next invalid item command.
        /// </summary>
        ApplicationCommand _gotoNextInvalidCommand;

        /// <summary>
        /// The goto next unviewed item command.
        /// </summary>
        ApplicationCommand _gotoNextUnviewedCommand;

        /// <summary>
        /// The toggle highlight tool command.
        /// </summary>
        ApplicationCommand _toggleHighlightCommand;

        /// <summary>
        /// The angular highlight tool command.
        /// </summary>
        ApplicationCommand _selectAngularHighlightCommand;

        /// <summary>
        /// The rectangular highlight tool command.
        /// </summary>
        ApplicationCommand _selectRectangularHighlightCommand;
         
        /// <summary>
        /// The hide tooltips command.
        /// </summary>
        ApplicationCommand _hideToolTipsCommand;

        /// <summary>
        /// The toggle show all data highlights command
        /// </summary>
        ApplicationCommand _toggleShowAllHighlightsCommand;

        /// <summary>
        /// The accept spatial info command
        /// </summary>
        ApplicationCommand _acceptSpatialInfoCommand;

        /// <summary>
        /// The remove spatial info command
        /// </summary>
        ApplicationCommand _removeSpatialInfoCommand;

        /// <summary>
        /// The database connection to be used for any validation or auto-update queries requiring a
        /// database.
        /// </summary>
        DbConnection _dbConnection;

        /// <summary>
        /// The user-specified settings for the data entry application.
        /// </summary>
        UserPreferences _userPreferences;

        /// <summary>
        /// The dialog for setting user preferences.
        /// </summary>
        PropertyPageForm _userPreferencesDialog;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryApplicationForm"/> class.
        /// </summary>
        /// <param name="configFileName">The name of the configuration file used to supply settings
        /// for the <see cref="DataEntryApplicationForm"/>.</param>
        public DataEntryApplicationForm(string configFileName)
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.LabDEVerificationUIObject,
                    "ELI23668", _OBJECT_NAME);

                // Initialize the configuration settings.
                ConfigSettings.Initialize(configFileName);
                
                InitializeComponent();

                // Read the user preferences object from the registry
                _userPreferences = UserPreferences.FromRegistry();

                // Need to set _openFileToolStripButton by searching for it.
                foreach (ToolStripItem item in _fileCommandsToolStrip.Items)
                {
                    if (item is OpenImageToolStripSplitButton)
                    {
                        _openFileToolStripButton = item;

                        break;
                    }
                }

                // Retrieve the name of the DEP assembly
                string dataEntryPanelFileName = 
                    DataEntryMethods.ResolvePath(ConfigSettings.AppSettings.DataEntryPanelFileName);

                // Create the data entry control host from the specified assembly
                this.DataEntryControlHost = CreateDataEntryControlHost(dataEntryPanelFileName);

                // If there's a database available, let the control host know about it.
                if (TryOpenDatabaseConnection())
                {
                    this.DataEntryControlHost.DatabaseConnection = _dbConnection;
                }

                this.DataEntryControlHost.AutoZoomMode = _userPreferences.AutoZoomMode;
                this.DataEntryControlHost.AutoZoomContext = _userPreferences.AutoZoomContext;

                base.Icon = _dataEntryControlHost.ApplicationIcon;
                _appHelpMenuItem.Text = this.DataEntryControlHost.ApplicationTitle + " &Help...";
                _aboutMenuItem.Text = "&About " + this.DataEntryControlHost.ApplicationTitle + "...";
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23669", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the control which implements the data entry panel (DEP).
        /// </summary>
        /// <value>The control which implments the data entry panel (DEP). <see langword="null"/> is 
        /// allowed, but results in a blank DEP.</value>
        /// <returns>The control which implments the data entry panel (DEP).</returns>
        [CLSCompliant(false)]
        public DataEntryControlHost DataEntryControlHost
        {
            get
            {
                return _dataEntryControlHost;
            }
            set
            {
                try
                {
                    if (_dataEntryControlHost != null)
                    {
                        // TODO: un-register SwipingStateChange event
                        _splitContainer.Panel1.Controls.Remove(_dataEntryControlHost);
                        _dataEntryControlHost.Dispose();
                    }

                    _dataEntryControlHost = value;

                    if (_dataEntryControlHost != null)
                    {
                        // TODO: register SwipingStateChange event

                        // Pad by _DATA_ENTRY_PANEL_PADDING around DEP content
                        _dataEntryControlHost.Location
                            = new Point(_DATA_ENTRY_PANEL_PADDING, _DATA_ENTRY_PANEL_PADDING);
                        _splitContainer.SplitterWidth = _DATA_ENTRY_PANEL_PADDING;
                        if (RegistryManager.DefaultSplitterPosition > 0)
                        {
                            _dataEntryControlHost.Width = RegistryManager.DefaultSplitterPosition - 
                                _DATA_ENTRY_PANEL_PADDING - _scrollPanel.AutoScrollMargin.Width;
                            _splitContainer.SplitterDistance = 
                                RegistryManager.DefaultSplitterPosition;
                        }
                        else
                        {
                            _splitContainer.SplitterDistance = _dataEntryControlHost.Size.Width +
                                _DATA_ENTRY_PANEL_PADDING + _scrollPanel.AutoScrollMargin.Width;
                        }

                        _dataEntryControlHost.Anchor = AnchorStyles.Left | AnchorStyles.Top | 
                            AnchorStyles.Right;

                        // The splitter should respect the minimum size of the DEP.
                        _splitContainer.Panel1MinSize =
                            _dataEntryControlHost.MinimumSize.Width +
                            (2 * _DATA_ENTRY_PANEL_PADDING) + _scrollPanel.AutoScrollMargin.Width +
                            SystemInformation.VerticalScrollBarWidth;

                        // Add the DEP to an auto-scroll pane to allow scrolling if the DEP is too
                        // long. (The scroll pane is sized to allow the full width of the DEP to 
                        // display initially) 
                        _scrollPanel.Size = new Size(_splitContainer.SplitterDistance, 
                            _scrollPanel.Height);
                        _scrollPanel.Controls.Add(_dataEntryControlHost);

                        // Handle scroll in order to update the panel position while a scroll is in
                        // progress.
                        _scrollPanel.Scroll += HandleScrollPanelScroll;

                        // If there's a database available, let the control host know about it.
                        if (_dbConnection != null)
                        {
                            _dataEntryControlHost.DatabaseConnection = _dbConnection;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI23884",
                        "Failed to set DataEntryControlHost.", ex);
                    ee.AddDebugData("DataEntryControlHost", value, false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="DataEntryApplicationForm"/> is in standalone mode 
        /// or whether another application has launched.
        /// </summary>
        /// <returns><see langref="true"/> if the <see cref="DataEntryApplicationForm"/> was
        /// launched as a standalone application or <see langref="false"/> if the
        /// <see cref="DataEntryApplicationForm"/> was launched via the COM interface.</returns>
        /// <value><see langref="true"/> if the <see cref="DataEntryApplicationForm"/> is to be
        /// run as a standalone application or <see langref="false"/> if the
        /// <see cref="DataEntryApplicationForm"/> is to be run via the COM interface.</value>
        public bool StandAloneMode
        {
            get
            {
                return _standAloneMode;
            }

            set
            {
                try
                {
                    // If running in standalone mode, the document will be supplied by the
                    // caller so hide the open and close file button and menu option. 
                    // Otherwise, show them.
                    if (_standAloneMode != value)
                    {
                        _standAloneMode = value;

                        if (_openFileCommand != null)
                        {
                            _openFileCommand.Visible = _standAloneMode;
                            _openFileCommand.Enabled = _standAloneMode;
                        }

                        if (_closeFileCommand != null)
                        {
                            _closeFileCommand.Visible = _standAloneMode;
                            _closeFileCommand.Enabled = _standAloneMode;
                        }

                        _exitToolStripMenuItem.Text = _standAloneMode ? "Exit" : "Stop Processing";
                        _saveAndCommitMenuItem.Text = _standAloneMode ? "&Save" : "&Save and Commit";
                        _saveAndCommitButton.Text = _standAloneMode ? "Save" : "Save and Commit";
                        _saveAndCommitButton.ToolTipText = _standAloneMode ? "Save" : "Save and Commit";

                        _toolStripSeparator1.Visible = _standAloneMode;
                        _skipProcessingMenuItem.Visible = !_standAloneMode;
                        _saveMenuItem.Visible = !_standAloneMode;

                        _imageViewer.DefaultStatusMessage =
                            _standAloneMode ? "" : "Waiting for next document...";
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI23885",
                        "Failed to change stand-alone mode.", ex);
                    ee.AddDebugData("StandAloneMode", value, false);
                    throw ee;
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// A thread-safe method that opens the specified document.
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
                Invoke(new VerificationFormOpen(Open),
                    new object[] { fileName, fileID, actionID, tagManager, fileProcessingDB });
                return;
            }

            try
            {
                ExtractException.Assert("ELI26940", "Null argument exception!",
                    fileProcessingDB != null);

                _fileProcessingDB = fileProcessingDB;
                _fileID = fileID;
                _actionID = actionID;

                // TODO: [LegacyRCAndUtils:5382] 
                // We are not currently receiving a valid action ID, so the comment cannot be
                // retrieved from the database.
                _dataEntryControlHost.Comment =
                    _fileProcessingDB.GetFileActionComment(_fileID, _actionID);

                _imageViewer.OpenImage(fileName, false);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23871", ex);
                DisplayCriticalException(ee);
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Initializes the application by establishing connections for all 
        /// <see cref="IImageViewerControl"/>s.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Set the application name
                base.Text = _dataEntryControlHost.ApplicationTitle;

                // Establish shortcut keys

                // Open an image
                _openFileCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.O | Keys.Control }, _imageViewer.SelectOpenImage, 
                    new ToolStripItem[] { _openFileToolStripButton, _openImageToolStripMenuItem },
                    false, _standAloneMode, _standAloneMode);

                // Close an image
                _closeFileCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.Control | Keys.F4 }, _imageViewer.CloseImage,
                    new ToolStripItem[] { _closeImageToolStripMenuItem },
                    false, _standAloneMode, _standAloneMode);

                // Save
                _saveAndCommitFileCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.S | Keys.Control }, SaveAndCommit,
                    new ToolStripItem[] { _saveAndCommitButton, _saveAndCommitMenuItem },
                    false, true, false);

                // Print an image
                _imageViewer.Shortcuts[Keys.P | Keys.Control] = _imageViewer.SelectPrint;

                // Goto next invalid
                _gotoNextInvalidCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.F3 }, _dataEntryControlHost.GoToNextInvalid,
                    new ToolStripItem[] { _nextInvalidToolStripButton, 
                        _nextInvalidToolStripMenuItem }, true, true, false);

                // Goto next unviewed
                _gotoNextUnviewedCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.F4 }, _dataEntryControlHost.GoToNextUnviewed,
                    new ToolStripItem[] { _nextUnviewedToolStripButton, 
                        _nextUnviewedToolStripMenuItem }, true, true, false);

                // Zoom tool
                _imageViewer.Shortcuts[Keys.Alt | Keys.Z] =
                    _imageViewer.SelectZoomWindowTool;

                // Pan tool
                _imageViewer.Shortcuts[Keys.Alt | Keys.A] = _imageViewer.SelectPanTool;

                // Review and select tool
                _imageViewer.Shortcuts[Keys.Alt | Keys.R] = _imageViewer.SelectSelectLayerObjectsTool;

                // Toggle highlight tool
                _toggleHighlightCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] {Keys.Alt | Keys.S}, _imageViewer.ToggleHighlightTool, null, false, 
                    true, false);

                // Swipe angular zone tool
                _selectAngularHighlightCommand = new ApplicationCommand(_imageViewer.Shortcuts, 
                    null, _imageViewer.SelectAngularHighlightTool, 
                    new ToolStripItem[] { _angularHighlightToolStripMenuItem, 
                        _angularHighlightToolStripButton}, false, true, false);

                // Swipe rectangular zone tool
                _selectRectangularHighlightCommand = new ApplicationCommand(_imageViewer.Shortcuts, 
                    null, _imageViewer.SelectRectangularHighlightTool, 
                    new ToolStripItem[] { _rectangularHighlightToolStripMenuItem, 
                        _rectangularHighlightToolStripButton }, false, true, false);

                // Fit to page
                _imageViewer.Shortcuts[Keys.Alt | Keys.P] = _imageViewer.ToggleFitToPageMode;

                // Fit to width
                _imageViewer.Shortcuts[Keys.Alt | Keys.W] = _imageViewer.ToggleFitToWidthMode;

                // Go to first page
                _imageViewer.Shortcuts[Keys.Control | Keys.Home] = _imageViewer.GoToFirstPage;

                // Go to the next page
                _imageViewer.Shortcuts[Keys.PageDown] = _imageViewer.GoToNextPage;

                // Go to the previous page
                _imageViewer.Shortcuts[Keys.PageUp] = _imageViewer.GoToPreviousPage;

                // Go to last page
                _imageViewer.Shortcuts[Keys.Control | Keys.End] = _imageViewer.GoToLastPage;

                // Zoom in
                _imageViewer.Shortcuts[Keys.F7] = _imageViewer.SelectZoomIn;
                _imageViewer.Shortcuts[Keys.Add | Keys.Control] = _imageViewer.SelectZoomIn;
                _imageViewer.Shortcuts[Keys.Oemplus | Keys.Control] = _imageViewer.SelectZoomIn;

                // Zoom out
                _imageViewer.Shortcuts[Keys.F8] = _imageViewer.SelectZoomOut;
                _imageViewer.Shortcuts[Keys.Subtract | Keys.Control] = _imageViewer.SelectZoomOut;
                _imageViewer.Shortcuts[Keys.OemMinus | Keys.Control] = _imageViewer.SelectZoomOut;

                // Zoom previous
                _imageViewer.Shortcuts[Keys.Alt | Keys.Left] = _imageViewer.SelectZoomPrevious;

                // Zoom next
                _imageViewer.Shortcuts[Keys.Alt | Keys.Right] = _imageViewer.SelectZoomNext;

                // Rotate clockwise
                _imageViewer.Shortcuts[Keys.R | Keys.Control] = _imageViewer.SelectRotateClockwise;

                // Rotate counterclockwise
                _imageViewer.Shortcuts[Keys.R | Keys.Control | Keys.Shift] =
                    _imageViewer.SelectRotateCounterclockwise;

                // Hide any visible toolTips
                _hideToolTipsCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.Escape }, _dataEntryControlHost.ToggleHideTooltips,
                    new ToolStripItem[] { _hideToolTipsMenuItem }, false, true, false);

                // Toggle show all data highlights
                _toggleShowAllHighlightsCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.F10 }, ToggleShowAllHighlights,
                    new ToolStripItem[] { _toggleShowAllHighlightsButton, 
                        _toggleShowAllHighlightsMenuItem }, false, true, false);

                // Accept spatial info command
                _acceptSpatialInfoCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.T | Keys.Control }, _dataEntryControlHost.AcceptSpatialInfo,
                    new ToolStripItem[] { _acceptImageHighlightMenuItem }, false, true, false);

                // Remove spatial info command
                _removeSpatialInfoCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.D | Keys.Control }, _dataEntryControlHost.RemoveSpatialInfo,
                    new ToolStripItem[] { _removeImageHighlightMenuItem }, false, true, false);

                // Establish connections between the image viewer and all image viewer controls.
                _imageViewer.EstablishConnections(this);

                // Register for events.
                _imageViewer.ImageFileChanged += HandleImageFileChanged;
                _imageViewer.ImageFileClosing += HandleImageFileClosing;
                _dataEntryControlHost.SwipingStateChanged += HandleSwipingStateChanged;
                _dataEntryControlHost.InvalidItemsFound += HandleInvalidItemsFound;
                _dataEntryControlHost.UnviewedItemsFound += HandleUnviewedItemsFound;
                _dataEntryControlHost.ItemSelectionChanged += HandleItemSelectionChanged;
                _saveAndCommitMenuItem.Click += HandleSaveAndCommitClick;
                _saveAndCommitButton.Click += HandleSaveAndCommitClick;
                _saveMenuItem.Click += HandleSaveClick;
                _skipProcessingMenuItem.Click += HandleSkipFileClick;
                _exitToolStripMenuItem.Click += HandleExitToolStripMenuItemClick;
                _nextUnviewedToolStripButton.Click += HandleGoToNextUnviewedClick;
                _nextUnviewedToolStripMenuItem.Click += HandleGoToNextUnviewedClick;
                _nextInvalidToolStripButton.Click += HandleGoToNextInvalidClick;
                _nextInvalidToolStripMenuItem.Click += HandleGoToNextInvalidClick;
                _toggleShowAllHighlightsButton.Click += HandleToggleShowAllHighlightsClick;
                _toggleShowAllHighlightsMenuItem.Click += HandleToggleShowAllHighlightsClick;
                _hideToolTipsMenuItem.Click += HandleHideToolTipsClick;
                _acceptImageHighlightMenuItem.Click += HandleAcceptImageHighlightClick;
                _removeImageHighlightMenuItem.Click += HandleRemoveImageHighlightClick;
                _splitContainer.SplitterMoved += HandleSplitterMoved;
                _aboutMenuItem.Click += HandleAboutMenuItemClick;
                _appHelpMenuItem.Click += HandleHelpMenuItemClick;
                _optionsToolStripMenuItem.Click += HandleOptionsMenuItemClick;

                // [DataEntry:195] Open the form with the position and size set per the registry 
                // settings. Do this regardless of whether the window will be maximized so that it
                // will restore to the size used the last time the window was in the "normal" state.
                this.DesktopBounds = new Rectangle(
                    new Point(RegistryManager.DefaultWindowPositionX,
                              RegistryManager.DefaultWindowPositionY),
                    new Size(RegistryManager.DefaultWindowWidth,
                             RegistryManager.DefaultWindowHeight));

                if (RegistryManager.DefaultWindowMaximized)
                {
                    // Maximize the window if the registry setting indicates the application should
                    // launch maximized.
                    this.WindowState = FormWindowState.Maximized;
                }

                if (!_standAloneMode)
                {
                    // If running in FAM mode, when a document is not loaded, indicate that the UI is
                    // waiting for the next document.
                    _exitToolStripMenuItem.Enabled = false;
                }

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23670", ex);
                ee.AddDebugData("Event Arguments", e, false);
                DisplayCriticalException(ee);
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.OnResize"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            try
            {
                base.OnResize(e);

                if (_isLoaded && this.WindowState != FormWindowState.Minimized)
                {
                    if (this.WindowState == FormWindowState.Maximized)
                    {
                        // If the user maximized the form, set the form to default to maximized,
                        // but don't adjust the default form size to use in normal mode.
                        RegistryManager.DefaultWindowMaximized = true;
                    }
                    else if (this.WindowState == FormWindowState.Normal)
                    {
                        // If the user restored or moved the form in normal mode, store
                        // the new size as the default size.
                        RegistryManager.DefaultWindowMaximized = false;
                        RegistryManager.DefaultWindowWidth = this.Size.Width;
                        RegistryManager.DefaultWindowHeight = this.Size.Height;
                    }

                    // If there is an image open in the image viewer then restore the previous
                    // scroll position - [DNRCAU #262 - JDS]
                    if (_imageViewer.IsImageAvailable)
                    {
                        _imageViewer.RestoreScrollPosition();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25071", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.OnMove"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnMove(EventArgs e)
        {
            try
            {
                base.OnMove(e);

                if (_isLoaded && this.WindowState == FormWindowState.Normal)
                {
                    // If the user moved the form, store the new position.
                    RegistryManager.DefaultWindowPositionX = this.DesktopLocation.X;
                    RegistryManager.DefaultWindowPositionY = this.DesktopLocation.Y;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25072", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        /// <returns><see langword="true"/> if the character was processed by the control; 
        /// <see langword="false"/> if the character was not processed.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
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
                ExtractException.Display("ELI24060", ex);
                return false;
            }
        }

        /// <summary>
        /// Processes windows messages.
        /// </summary>
        /// <param name="m">The Windows <see cref="Message"/> to process.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            try
            {
                // If a document is not loaded, the DataEntryApplicationForm has no way of informing
                // the FAM of a cancel. Therefore, don't allow the form to be closed in FAM mode
                // when a document is not loaded.
                if (!_standAloneMode && !_imageViewer.IsImageAvailable && 
                    m.Msg == _WM_SYSCOMMAND && m.WParam == _SC_CLOSE)
                {
                    MessageBox.Show(this, "If you are intending to stop processing, " +
                        "press the stop button in the File Action Manager.",
                        _dataEntryControlHost.ApplicationTitle, MessageBoxButtons.OK,
                        MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);

                    return;
                }

                base.WndProc(ref m);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26764", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.Closing"/> event in order to give the user an opportunity to save
        /// data prior to closing the application.
        /// </summary>
        /// <param name="e">The event data associated with the event.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (!_forcingClose)
                {
                    // Check for unsaved data and cancel the close if necessary.
                    if (AttemptSave(false) == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        // Clear data to give the host a chance to clear any static COM objects that will
                        // not be accessible from a different thread due to the single apartment threading
                        // model.
                        _dataEntryControlHost.ClearData();
                    }
                }

                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI24858", ex).Display();
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
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_dbConnection != null)
                {
                    _dbConnection.Dispose();
                    _dbConnection = null;
                }

                if (this.DataEntryControlHost != null)
                {
                    // Will cause the control host to be disposed of.
                    this.DataEntryControlHost = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Events

        /// <summary>
        /// This event indicates that the current document is done processing.
        /// </summary>
        public event EventHandler<FileCompleteEventArgs> FileComplete;

        #endregion Events

        #region Event Handlers

        /// <summary>
        /// Handles the case that the user requested that the data be saved and commited.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleSaveAndCommitClick(object sender, EventArgs e)
        {
            try
            {
                SaveAndCommit();
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI23908",
                    "Failed to output document data!", ex);
                ee.AddDebugData("Event data", e, false);
                DisplayCriticalException(ee);
            }
        }

        /// <summary>
        /// Handles the case that the user requested that the data be saved (but not committed).
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleSaveClick(object sender, EventArgs e)
        {
            try
            {
                SaveData(false);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26948",
                    "Failed to output document data!", ex);
                ee.AddDebugData("Event data", e, false);
                DisplayCriticalException(ee);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validateData"></param>
        /// <returns></returns>
        bool SaveData(bool validateData)
        {
            bool saved = _dataEntryControlHost.SaveData(validateData);

            if (saved && !_standAloneMode)
            {
                _fileProcessingDB.SetFileActionComment(_fileID, _actionID,
                        _dataEntryControlHost.Comment);
            }

            return saved;
        }

        /// <summary>
        /// Handles the case that the user requested to skip the current document
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleSkipFileClick(object sender, EventArgs e)
        {
            try
            {
                if (AttemptSave(false) != DialogResult.Cancel)
                {
                    _forcingClose = true;

                    _imageViewer.CloseImage();

                    OnFileComplete(EFileProcessingResult.kProcessingSkipped);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26943",
                    "Failed to output document data!", ex);
                ee.AddDebugData("Event data", e, false);
                DisplayCriticalException(ee);
            }
            finally
            {
                _forcingClose = false;
            }
        }

        /// <summary>
        /// Handles the case that a new image was loaded into the image viewer.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                // Saving or closing the document or hiding of tooltips should be allowed as long as
                // a document is available.
                _saveAndCommitFileCommand.Enabled = _imageViewer.IsImageAvailable;
                _closeFileCommand.Enabled = _imageViewer.IsImageAvailable;
                _hideToolTipsCommand.Enabled = _imageViewer.IsImageAvailable;
                _hideToolTipsCommand.Enabled = _imageViewer.IsImageAvailable;
                _toggleShowAllHighlightsCommand.Enabled = _imageViewer.IsImageAvailable;

                if (!StandAloneMode)
                {
                    _skipProcessingMenuItem.Enabled = _imageViewer.IsImageAvailable;
                }
                _saveMenuItem.Enabled = _imageViewer.IsImageAvailable;

                // If a document is not loaded, the DataEntryApplicationForm has no way of informing
                // the FAM of a cancel. Therefore, don't allow the form to be closed in FAM mode
                // when a document is not loaded.
                _exitToolStripMenuItem.Enabled = (_standAloneMode || _imageViewer.IsImageAvailable);

                if (!_imageViewer.IsImageAvailable)
                {
                    // The goto next invalid and unviewed buttons and menu options will be enabled via
                    // the DataEntryControlHost.UnviewedItemsFound and InvalidItemsFound events.
                    _gotoNextInvalidCommand.Enabled = false;
                    _gotoNextUnviewedCommand.Enabled = false;

                    // This highlight commands will be enabled via HandleSwipingStateChanged.
                    _toggleHighlightCommand.Enabled = false;
                    _selectAngularHighlightCommand.Enabled = false;
                    _selectRectangularHighlightCommand.Enabled = false;
                }

                // Set the application title to reflect the name of the open document.
                base.Text = _dataEntryControlHost.ApplicationTitle;
                if (_imageViewer.IsImageAvailable)
                {
                    base.Text += " - " + Path.GetFileName(_imageViewer.ImageFile);

                    // Ensure the DEP is scrolled back to the top when a document is loaded.
                    _scrollPanel.AutoScrollPosition = new Point(_scrollPanel.AutoScrollPosition.X, 0);
                }

                // TODO: In when standalone mode == false, need to assert that an image is indeed
                // available.
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI24228",
                    "Failed to output document data!", ex);
                ee.AddDebugData("Event data", e, false);
                DisplayCriticalException(ee);
            }
        }

        /// <summary>
        /// Handles the case that the current image is closing so that the user can be prompted
        /// about unsaved data.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The <see cref="ImageFileClosingEventArgs"/> containing the event data.
        /// </param>
        void HandleImageFileClosing(object sender, ImageFileClosingEventArgs e)
        {
            try
            {
                if (!_forcingClose)
                {
                    // Check for unsaved data and cancel the close if necessary.
                    if (AttemptSave(false) == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        OnFileComplete(EFileProcessingResult.kProcessingCancelled);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24982", ex);
                ee.AddDebugData("Event data", e, false);
                DisplayCriticalException(ee);
            }
        }

        /// <summary>
        /// Handles the case that the <see cref="DataEntryControlHost"/> is indicating that swiping
        /// support has either been enabled or disabled.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleSwipingStateChanged(object sender, SwipingStateChangedEventArgs e)
        {
            try
            {
                // Enable/disable and deselect the highlight cursor tools as needed.
                if (!e.SwipingEnabled &&
                    (_imageViewer.CursorTool == CursorTool.AngularHighlight ||
                     _imageViewer.CursorTool == CursorTool.RectangularHighlight))
                {
                    this._imageViewer.CursorTool = CursorTool.None;
                }

                // Enable or disable highlight commands as appropriate.
                _toggleHighlightCommand.Enabled = e.SwipingEnabled;
                _selectAngularHighlightCommand.Enabled = e.SwipingEnabled;
                _selectRectangularHighlightCommand.Enabled = e.SwipingEnabled;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24063", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the <see cref="DataEntryControlHost"/> now reports unviewed items
        /// to either be present or not present so that the goto next unviewed button and menu item
        /// can be enabled/disabled.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="UnviewedItemsFoundEventArgs"/> instance containing the
        /// event data.</param>
        void HandleUnviewedItemsFound(object sender, UnviewedItemsFoundEventArgs e)
        {
            try
            {
                _gotoNextUnviewedCommand.Enabled = e.UnviewedItemsFound;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24933", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the <see cref="DataEntryControlHost"/> now reports unviewed items
        /// to either be present or not present so that the goto next unviewed button and menu item
        /// can be enabled/disabled.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="InvalidItemsFoundEventArgs"/> instance containing the
        /// event data.</param>
        void HandleInvalidItemsFound(object sender, InvalidItemsFoundEventArgs e)
        {
            try
            {
                _gotoNextInvalidCommand.Enabled = e.InvalidItemsFound;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24916", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that new items have been selected by updating the enabled status of the
        /// confirm and remove spatial info commands accordingly.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="ItemSelectionChangedEventArgs"/> instance containing the
        /// event data.</param>
        void HandleItemSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
        {
            try
            {
                _acceptSpatialInfoCommand.Enabled = e.SelectedItemsWithUnacceptedHighlights > 0;

                _removeSpatialInfoCommand.Enabled =
                    (e.SelectedItemsWithAcceptedHighlights > 0 ||
                     e.SelectedItemsWithUnacceptedHighlights > 0 ||
                     e.SelectedItemsWithDirectHints > 0 || e.SelectedItemsWithIndirectHints > 0);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25981", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected exit from the file menu
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24059", ex);
                ee.AddDebugData("Event arguments", e, false);
                DisplayCriticalException(ee);
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Goto next unviewed" button or menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleGoToNextUnviewedClick(object sender, EventArgs e)
        {
            try
            {
                _dataEntryControlHost.GoToNextUnviewed();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24643", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Goto next invalid" button or menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleGoToNextInvalidClick(object sender, EventArgs e)
        {
            try
            {
                _dataEntryControlHost.GoToNextInvalid();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24644", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Highlight all data in image" button or menu 
        /// item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleToggleShowAllHighlightsClick(object sender, EventArgs e)
        {
            try
            {
                ToggleShowAllHighlights();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25159", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Hide tooltips" menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleHideToolTipsClick(object sender, EventArgs e)
        {
            try
            {
                _dataEntryControlHost.ToggleHideTooltips();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25994", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Accept highlight" menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleAcceptImageHighlightClick(object sender, EventArgs e)
        {
            try
            {
                _dataEntryControlHost.AcceptSpatialInfo();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25992", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Remove highlight" menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleRemoveImageHighlightClick(object sender, EventArgs e)
        {
            try
            {
                _dataEntryControlHost.RemoveSpatialInfo();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25993", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ScrollBar.Scroll"/> event from the _scrollPanel's vertical
        /// scrollbar in order to update the window position while a scroll is in progress.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The <see cref="ScrollEventArgs"/> that contains the event data.</param>
        void HandleScrollPanelScroll(object sender, ScrollEventArgs e)
        {
            _scrollPanel.AutoScrollPosition = new Point(_scrollPanel.AutoScrollPosition.X, e.NewValue);
        }

        /// <summary>
        /// Handles the <see cref="SplitContainer.SplitterMoved"/> event in order to update the
        /// default splitter position in the registry.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleSplitterMoved(object sender, SplitterEventArgs e)
        {
            try
            {
                RegistryManager.DefaultSplitterPosition = _splitContainer.SplitterDistance;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25073", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event for the help menu item.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="ToolStripItem.Click"/> event.
        /// </param>
        /// <param name="e">The event data associated with the <see cref="ToolStripItem.Click"/> 
        /// event.</param>
        void HandleHelpMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Help.ShowHelp(this, 
                    DataEntryMethods.ResolvePath(ConfigSettings.AppSettings.HelpFile));
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26958", "The help file is not available!", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event for the about menu item.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="ToolStripItem.Click"/> event.
        /// </param>
        /// <param name="e">The event data associated with the <see cref="ToolStripItem.Click"/> 
        /// event.</param>
        void HandleAboutMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // Show the about dialog
                using (AboutForm aboutForm = new AboutForm(_dataEntryControlHost))
                {
                    aboutForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26957", ex);
            }
        }

        /// <summary>
        /// Handles the case that the user selected the Tools | Options menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleOptionsMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // Create the preferences dialog if not already created
                if (_userPreferencesDialog == null)
                {
                    _userPreferencesDialog = new PropertyPageForm("Options",
                        (IPropertyPage)_userPreferences.PropertyPage);

                    _userPreferencesDialog.Icon = _dataEntryControlHost.ApplicationIcon;
                }

                // Display the dialog
                DialogResult result = _userPreferencesDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // If the user applied settings, store them to the registry and update the
                    // dataEntryControlHost's settings.
                    _userPreferences.WriteToRegistry();

                    this.DataEntryControlHost.AutoZoomMode = _userPreferences.AutoZoomMode;
                    this.DataEntryControlHost.AutoZoomContext = _userPreferences.AutoZoomContext;
                }
                else
                {
                    _userPreferences.ReadFromRegistry();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27020", ex);
            }
        }


        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Raises the <see cref="FileComplete"/> event.
        /// </summary>
        /// <param name="fileProcessingResult">Specifies under what circumstances
        /// verification of the file completed.</param>
        protected virtual void OnFileComplete(EFileProcessingResult fileProcessingResult)
        {
            if (FileComplete != null)
            {
                FileComplete(this, new FileCompleteEventArgs(fileProcessingResult));
            }
        }

        /// <summary>
        /// Saves and commits current file in FAM (!_standAloneMode).
        /// </summary>
        void SaveAndCommit()
        {
            try
            {
                // Treat SaveAndCommit like "Save" for stand-alone mode.
                if (_standAloneMode)
                {
                    SaveData(false);
                }
                // AttemptSave will return Cancel if there was invalid data in the DEP.
                else if (AttemptSave(true) != DialogResult.Cancel)
                {
                    // If running in FAM mode, close the document until the next one is loaded so it
                    // is clear that the last document has been committed.
                    
                    _forcingClose = true;

                    _imageViewer.CloseImage();

                    OnFileComplete(EFileProcessingResult.kProcessingSuccessful);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26945", ex);
            }
            finally
            {
                _forcingClose = false;
            }
        }

        /// <summary>
        /// Checks for unsaved data and prompts the user to save as necessary.
        /// </summary>
        /// <param name="commitData"><see langword="true"/> if data is being committed and therefore
        /// it should be validated in the DEP or <see langword="false"/> if the purpose is to give
        /// the user a chance to save the data without commiting in which case the user will be
        /// prompted whether to save or not.</param>
        /// <returns><see cref="DialogResult.Yes"/> if the document was successfully saved, 
        /// <see cref="DialogResult.No"/> if the user elected not to save or
        /// <see cref="DialogResult.Cancel"/> if the user elected to cancel the operation which
        /// triggered the save attempt or the data in the DEP failed validation for a commit.
        /// </returns>
        DialogResult AttemptSave(bool commitData)
        {
            DialogResult response = DialogResult.Yes;

            if (_imageViewer.IsImageAvailable && (commitData || _dataEntryControlHost.Dirty))
            {
                // Prompt if the data is not being commited.
                if (!commitData)
                {
                    response = MessageBox.Show(this,
                        "Data has not been saved, would you like to save now?",
                        "Data Not Saved", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1, 0);
                }

                // If commiting data or the user elected to save, attempt the save.
                if (response == DialogResult.Yes && !SaveData(commitData))
                {
                    // Return cancel if the data in the DEP failed validation.
                    response = DialogResult.Cancel;
                }
            }

            return response;
        }

        /// <summary>
        /// Displays and throws an exception.
        /// </summary>
        /// <param name="ee">The <see cref="ExtractException"/> to display.</param>
        static void DisplayCriticalException(ExtractException ee)
        {
            ee.Display();
            throw ee;
        }

        /// <summary>
        /// Instantiates the one and only <see cref="DataEntryControlHost"/> implemented by the
        /// specified assembly.
        /// </summary>
        /// <param name="assemblyFileName">The filename of the assembly to use.</param>
        /// <returns>A <see cref="DataEntryControlHost"/> instantiated from the specified assembly.
        /// </returns>
        static DataEntryControlHost CreateDataEntryControlHost(string assemblyFileName)
        {
            try
            {
                // A variable to store the return value
                DataEntryControlHost controlHost = null;

                ExtractException.Assert("ELI23680", "Cannot find specified assembly!",
                    File.Exists(assemblyFileName));

                // Load the specified assembly
                Assembly assembly = Assembly.LoadFrom(assemblyFileName);

                // Using reflection, iterate the classes in the assembly looking for one that 
                // implements DataEntryControlHost
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.BaseType == typeof(DataEntryControlHost))
                    {
                        ExtractException.Assert("ELI23675",
                            "Assembly implements multiple data entry control hosts!", controlHost == null);

                        // Create and instance of the DEP class.
                        controlHost = (DataEntryControlHost)assembly.CreateInstance(type.ToString());

                        // Keep searching to ensure there are not multiple implementations
                    }
                }

                ExtractException.Assert("ELI23676",
                    "Failed to find data entry control host implementation!", controlHost != null);

                // If HighlightConfidenceBoundary settings has been specified in the config file and
                // the controlHost has exactly two confidence tiers, use the provided value as the
                // minimum OCR confidence value in order to highlight text as confidently OCR'd
                if (!string.IsNullOrEmpty(ConfigSettings.AppSettings.HighlightConfidenceBoundary) &&
                    controlHost.HighlightColors.Length == 2)
                {
                    int confidenceBoundary = Convert.ToInt32(
                        ConfigSettings.AppSettings.HighlightConfidenceBoundary,
                        CultureInfo.CurrentCulture);

                    ExtractException.Assert("ELI25684", "HighlightConfidenceBoundary settings must " +
                        "be a value between 1 and 100",
                        confidenceBoundary >= 1 && confidenceBoundary <= 100);

                    HighlightColor[] highlightColors = controlHost.HighlightColors;
                    highlightColors[0].MaxOcrConfidence = confidenceBoundary - 1;
                    controlHost.HighlightColors = highlightColors;
                }

                controlHost.DisabledControls = ConfigSettings.AppSettings.DisabledControls;
                controlHost.DisabledValidationControls =
                    ConfigSettings.AppSettings.DisabledValidationControls;

                return controlHost;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI23677",
                    "Unable to initialize data entry control host!", ex);
                ee.AddDebugData("Assembly Name", assemblyFileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Toggles whether all data is currently highlighted in the <see cref="ImageViewer"/> or
        /// whether only the currently selected data is highlighted.
        /// </summary>
        void ToggleShowAllHighlights()
        {
            bool showAllHighlights = !_dataEntryControlHost.ShowAllHighlights;

            _toggleShowAllHighlightsButton.CheckState =
                showAllHighlights ? CheckState.Checked : CheckState.Unchecked;
            _toggleShowAllHighlightsMenuItem.CheckState =
                showAllHighlights ? CheckState.Checked : CheckState.Unchecked;

            _dataEntryControlHost.ShowAllHighlights = showAllHighlights;
        }

        /// <summary>
        /// Attempts to open a database connection for use by the DEP for validation and
        /// auto-updates if connection information is specfied in the config settings.
        /// </summary>
        /// <returns><see langword="true"/> if connection information was provided in the
        /// config file and the connection was successfully opened, <see langword="false"/> if
        /// connection information was not provided and no connection was attempted.</returns>
        bool TryOpenDatabaseConnection()
        {
            try
            {
                if (!string.IsNullOrEmpty(ConfigSettings.AppSettings.DatabaseType))
                {
                    string connectionString = "";

                    // A full connection string has been provided.
                    if (!string.IsNullOrEmpty(ConfigSettings.AppSettings.DatabaseConnectionString))
                    {
                        ExtractException.Assert("ELI26157", "Either a database connection string " +
                            "can be specified, or a local datasource-- not both.",
                            string.IsNullOrEmpty(ConfigSettings.AppSettings.LocalDataSource));

                        connectionString = ConfigSettings.AppSettings.DatabaseConnectionString;
                    }
                    // A local datasource has been specfied; compute the connection string.
                    else if (!string.IsNullOrEmpty(ConfigSettings.AppSettings.LocalDataSource))
                    {
                        ExtractException.Assert("ELI26158", "Either a database connection string " +
                            "can be specified, or a local datasource-- not both.",
                            string.IsNullOrEmpty(ConfigSettings.AppSettings.DatabaseConnectionString));

                        string dataSourcePath =
                            DataEntryMethods.ResolvePath(ConfigSettings.AppSettings.LocalDataSource);
                        connectionString = "Data Source='" + dataSourcePath + "';";
                    }

                    // As long as connection information was provieded one way or another,
                    // create and open the database connection.
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        Type dbType = Type.GetType(ConfigSettings.AppSettings.DatabaseType);
                        _dbConnection = (DbConnection)Activator.CreateInstance(dbType);
                        _dbConnection.ConnectionString = connectionString;
                        _dbConnection.Open();

                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26159",
                    "Failed to open database connection!", ex);
                ee.AddDebugData("Database type", ConfigSettings.AppSettings.DatabaseType, false);
                ee.AddDebugData("Local datasource", ConfigSettings.AppSettings.LocalDataSource, false);
                ee.AddDebugData("Connection string",
                    ConfigSettings.AppSettings.DatabaseConnectionString, false);

                throw ee;
            }
        }

        #endregion Private Members
    }
}
