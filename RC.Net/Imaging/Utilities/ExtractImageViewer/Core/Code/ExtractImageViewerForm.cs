using Extract.Drawing;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using TD.SandDock;

namespace Extract.Imaging.Utilities.ExtractImageViewer
{
    /// <summary>
    /// Represents a dialog with controls to display and interact with image files.
    /// </summary>
    public partial class ExtractImageViewerForm : Form
    {
        #region Constants

        /// <summary>
        /// The default text for the title bar of the application.
        /// </summary>
        const string _DEFAULT_TITLE_TEXT = "Extract Image Viewer";

        /// <summary>
        /// Height restriction in inches for sending OCR to a text file or clipboard.
        /// </summary>
        const double _OCR_TO_TEXT_HEIGHT_LIMIT = 8.0;

        /// <summary>
        /// Width restriction in inches for sending OCR to a text file or clipboard.
        /// </summary>
        const double _OCR_TO_TEXT_WIDTH_LIMIT = 8.5;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ExtractImageViewerForm).ToString();

        /// <summary>
        /// The license string for the SandDock manager
        /// </summary>
        static readonly string _SANDDOCK_LICENSE_STRING = @"1970|siE7SnF/jzINQg1AOTIaCXLlouA=";

        /// <summary>
        /// The full path to the file that contains information about persisting the 
        /// <see cref="ExtractImageViewerForm"/>.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_FILE = FileSystemMethods.PathCombine(
            FileSystemMethods.ApplicationDataPath, "Extract Image Viewer",
            "ExtractImageViewerForm.xml");

        /// <summary>
        /// The OCR manager to use when performing OCR of highlights
        /// </summary>
        SynchronousOcrManager _ocrManager = new SynchronousOcrManager(OcrTradeoff.Accurate);

        /// <summary>
        /// Whether OCR text should be sent to the clipboard or not.
        /// </summary>
        bool _sendOcrTextToClipboard;

        /// <summary>
        /// The text file where OCR results should be sent.
        /// </summary>
        string _ocrTextFile;

        /// <summary>
        /// Whether OCR text should be sent to a message box.
        /// </summary>
        bool _sendOcrToMessageBox;

        /// <summary>
        /// The search form used to search for image files.
        /// </summary>
        ImageSearchForm _imageSearchForm;

        /// <summary>
        /// Whether or not the image search form should be opened on load.
        /// </summary>
        bool _openImageSearchForm;

        /// <summary>
        /// The .Net remoting object used to communicate between this
        /// instance of the <see cref="ExtractImageViewer"/> and other processes.
        /// </summary>
        RemoteMessageHandler _remoteHandler;

        /// <summary>
        /// The channel used for remote communications.
        /// </summary>
        IpcChannel _ipcChannel;

        /// <summary>
        /// The initial image file to open when the form loads.
        /// </summary>
        string _initialImageFile;

        /// <summary>
        /// The script file to process when the form loads.
        /// </summary>
        string _scriptFile;

        /// <summary>
        /// Collection of temporary highlights.
        /// </summary>
        List<LayerObject> _tempHighlights = new List<LayerObject>();

        /// <summary>
        /// Flag to indicate whether a tool strip was hidden when a script was processed.
        /// If it is <see langword="true"/> then the toolstrips will not be persisted
        /// when the form closes.
        /// </summary>
        bool _buttonOrMenuHidden;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new instance of the <see cref="ExtractImageViewerForm"/> 
        /// class.</overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractImageViewerForm"/> class.
        /// </summary>
        public ExtractImageViewerForm()
            : this(null, null, false, false, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractImageViewerForm"/> class.
        /// </summary>
        /// <param name="scriptFile">The name of the script file to open.</param>
        public ExtractImageViewerForm(string scriptFile)
            : this(null, null, false, false, scriptFile)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractImageViewerForm"/> class opened 
        /// with the specified image file.
        /// <para><b>Note:</b></para>
        /// OCR results by default will be sent to a message box.  If
        /// <paramref name="sendOcrTextToClipboard"/> is <see langword="true"/> AND/OR
        /// <paramref name="ocrTextFile"/> is not <see langword="null"/> then
        /// text will not be sent to a message box.
        /// </summary>
        /// <param name="fileName">The image file to open. <see langword="null"/> if no image file 
        /// should be opened.</param>
        /// <param name="ocrTextFile">If not <see langword="null"/> then when text is
        /// highlighted and OCR is performed, the results will be sent to the specified
        /// text file, otherwise the results will be sent to a message box.</param>
        /// <param name="sendOcrTextToClipboard">If not <see langword="true"/> then OCR
        /// results will be copied into the clipboard, if <see langword="false"/> then
        /// OCR results will be displayed in a message box.</param>
        /// <param name="openImageSearchForm">If <see langword="true"/> then the
        /// <see cref="ImageSearchForm"/> will be displayed when the form is loaded.</param>
        /// <param name="scriptFile">If not <see langword="null"/> then the file will
        /// be read and the script commands will be processed.</param>
        public ExtractImageViewerForm(string fileName, string ocrTextFile,
            bool sendOcrTextToClipboard, bool openImageSearchForm, string scriptFile)
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.InputFunnelCoreObjects,
                    "ELI30181", _OBJECT_NAME);

                // License SandDock before creating the form
                SandDockManager.ActivateProduct(_SANDDOCK_LICENSE_STRING);

                InitializeComponent();

                // Initialize the remoting objects
                _remoteHandler = new RemoteMessageHandler(this);
                _ipcChannel = new IpcChannel(
                    BuildExtractImageViewerUri(SystemMethods.GetCurrentProcessId()));
                ChannelServices.RegisterChannel(_ipcChannel, true);

                // Set whether the image search form should be opened
                _openImageSearchForm = openImageSearchForm;

                // Get the OCR destination information
                _ocrTextFile = ocrTextFile;
                _sendOcrTextToClipboard = sendOcrTextToClipboard;

                // Set whether or not OCR text should be sent to the message box
                _sendOcrToMessageBox =
                    !_sendOcrTextToClipboard && string.IsNullOrEmpty(_ocrTextFile);

                // Set the icon
                Icon =
                    Extract.Imaging.Utilities.ExtractImageViewer.Properties.Resources.ExtractImageViewer;

                // Add the event handler for layer objects being added and deleted
                _imageViewer.LayerObjects.LayerObjectAdded += HandleLayerObjectAdded;
                _imageViewer.LayerObjects.LayerObjectDeleted += HandleLayerObjectDeleted;

                // Store the initial image file and script file names
                _initialImageFile = fileName;
                _scriptFile = scriptFile;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30122", ex);
            }
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                // Call the base class
                base.OnLoad(e);

                // Establish connections with all controls on this form.
                _imageViewer.EstablishConnections(this);

                // If not design time, load the saved state of the extract image viewer
                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    LoadState();
                }

                // Set the dockable window that the thumbnail toolstrip button controls
                _thumbnailsToolStripButton.DockableWindow = _thumbnailDockableWindow;

                if (_openImageSearchForm)
                {
                    _searchForImagesToolStripMenuItem.PerformClick();
                }

                // Check if a filename was specified
                if (!string.IsNullOrEmpty(_initialImageFile))
                {
                    _imageViewer.OpenImage(_initialImageFile, true);
                }

                // Check if a script file was specified
                if (!string.IsNullOrEmpty(_scriptFile))
                {
                    ProcessScriptFile(_scriptFile);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30123", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.FormClosing"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                // If not design time, save the extract image viewer state
                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    SaveState();
                }

                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30130", ex);
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
                // Allow the image viewer to handle this keyboard shortcut.
                // If the image viewer does not process it, bubble it up to the base class.
                return _imageViewer.Shortcuts.ProcessKey(keyData) ||
                    base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30124", ex);
            }

            return true;
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The arguments associated with the event.</param>
        void HandleLayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            try
            {
                using (new TemporaryWaitCursor())
                {
                    Highlight highlight = e.LayerObject as Highlight;
                    if (highlight != null)
                    {
                        // Check the restrictions if not sending to message box
                        if (!_sendOcrToMessageBox)
                        {
                            double width = GeometryMethods.Distance(highlight.StartPoint, highlight.EndPoint);
                            double height = highlight.Height;
                            if (width > height)
                            {
                                UtilityMethods.Swap(ref width, ref height);
                            }
                            width /= _imageViewer.ImageDpiX;
                            height /= _imageViewer.ImageDpiY;

                            // Get the resolution from the image viewer
                            if (width > _OCR_TO_TEXT_WIDTH_LIMIT || height > _OCR_TO_TEXT_HEIGHT_LIMIT)
                            {
                                ExtractException ee = new ExtractException("ELI30154",
                                    "Selected region too large to send results to file or clipboard.");
                                ee.AddDebugData("Height In Inches", height, false);
                                ee.AddDebugData("Width In Inches", width, false);
                                ee.AddDebugData("Max Height In Inches",
                                    _OCR_TO_TEXT_HEIGHT_LIMIT, false);
                                ee.AddDebugData("Max Width In Inches",
                                    _OCR_TO_TEXT_WIDTH_LIMIT, false);
                                throw ee;
                            }
                        }

                        string temp = _ocrManager.GetOcrTextAsString(_imageViewer.ImageFile,
                            highlight.ToRasterZone(), 0.2);

                        highlight.Text = temp;
                        if (_sendOcrToMessageBox)
                        {
                            using (CustomizableMessageBox message = new CustomizableMessageBox())
                            {
                                // Display the OCR result to the user.
                                // Disable selection of text in the message box.
                                message.Caption = "OCR Result";
                                message.StandardIcon = MessageBoxIcon.None;
                                message.AddStandardButtons(MessageBoxButtons.OK);
                                message.Text = temp;
                                message.AllowTextSelection = false;
                                message.Show(this);
                            }
                        }
                        else
                        {
                            if (_sendOcrTextToClipboard)
                            {
                                Clipboard.SetText(temp);
                            }
                            if (!string.IsNullOrEmpty(_ocrTextFile))
                            {
                                // Ensure the output directory exists
                                Directory.CreateDirectory(Path.GetDirectoryName(_ocrTextFile));

                                // Write the text to the specified file (overwrite any existing text)
                                File.WriteAllText(_ocrTextFile, temp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30125", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The arguments associated with the event.</param>
        void HandleLayerObjectDeleted(object sender, LayerObjectDeletedEventArgs e)
        {
            try
            {
                // Check if the deleted object is a highlight
                if (e.LayerObject is Highlight)
                {
                    // Check if the deleted highlight is one of the temporary highlights
                    int index = _tempHighlights.IndexOf(e.LayerObject);
                    if (index != -1)
                    {
                        // Remove the temporary highlight from the collection
                        _tempHighlights.RemoveAt(index);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30165", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleImageViewerImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                // Update the title bar with the name of the open image
                StringBuilder sb = new StringBuilder(_DEFAULT_TITLE_TEXT);
                if (_imageViewer.IsImageAvailable)
                {
                    sb.Append(" - ");
                    sb.Append(_imageViewer.ImageFile);
                }
                Text = sb.ToString();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30126", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the search for
        /// images tool strip menu item.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleSearchForImageFilesClick(object sender, EventArgs e)
        {
            try
            {
                // Lazy instantiation of the form
                if (_imageSearchForm == null)
                {
                    // Create the search form and set the icon
                    _imageSearchForm = new ImageSearchForm(_imageViewer);
                    _imageSearchForm.Icon = Icon;
                }

                if (_imageSearchForm.Visible)
                {
                    _imageSearchForm.BringToFront();
                }
                else
                {
                    // Show the image search form
                    _imageSearchForm.Show(this);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30128", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the file menu exit command.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleFileMenuExitClick(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30182", ex);
            }
        }

        #endregion Event Handlers

        #region Properties

        /// <summary>
        /// Gets the <see cref="ImageViewer"/> <see cref="Control"/> from the <see cref="Form"/>.
        /// </summary>
        /// <returns>The <see cref="ImageViewer"/> <see cref="Control"/> from the
        /// <see cref="Form"/>.</returns>
        internal ImageViewer ImageViewer
        {
            get
            {
                return _imageViewer;
            }
        }

        /// <summary>
        /// Gets/sets whether OCR text should be sent to the clipboard rather than
        /// being displayed in a message box.
        /// </summary>
        /// <returns>Whether OCR text will be sent to the clipboard or not.</returns>
        internal bool SendOcrTextToClipboard
        {
            get
            {
                return _sendOcrTextToClipboard;
            }
            set
            {
                _sendOcrTextToClipboard = value;

                // Set whether or not OCR text should be sent to the message box
                _sendOcrToMessageBox =
                    !_sendOcrTextToClipboard && string.IsNullOrEmpty(_ocrTextFile);
            }
        }

        /// <summary>
        /// Gets/sets the name of the text file that will receive OCR text. If
        /// not <see langword="null"/> then OCR results will be sent to the
        /// specified text file, otherwise they will be displayed in a message box.
        /// </summary>
        /// <returns>The text file to send OCR results to.</returns>
        internal string OcrTextFile
        {
            get
            {
                return _ocrTextFile;
            }
            set
            {
                _ocrTextFile = value;

                // Set whether or not OCR text should be sent to the message box
                _sendOcrToMessageBox =
                    !_sendOcrTextToClipboard && string.IsNullOrEmpty(_ocrTextFile);
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Builds the IPC channel remoting URI for the specified process ID.
        /// </summary>
        /// <param name="processId">The process ID to use for generating
        /// the URI.</param>
        /// <returns>The URI for the specified process ID.</returns>
        internal static string BuildExtractImageViewerUri(int processId)
        {
            StringBuilder sb = new StringBuilder("ExtractImageViewer_");
            sb.Append(processId);
            return sb.ToString();
        }

        /// <summary>
        /// Process the specified script file.
        /// </summary>
        /// <param name="scriptFile">The name of the script file to process.</param>
        internal void ProcessScriptFile(string scriptFile)
        {
            ExtractException.Assert("ELI30169", "Specified script file does not exist.",
                File.Exists(scriptFile), "Script File Name", scriptFile);

            // Load the script file into a commented text file reader
            CommentedTextFileReader reader = new CommentedTextFileReader(scriptFile);

            string[] spaceToken = new string[] { " " };

            // Iterate the script commands and execute them
            foreach (string scriptCommand in reader)
            {
                string[] line = scriptCommand.Split(spaceToken,
                    StringSplitOptions.RemoveEmptyEntries);
                string command = line[0];
                if (command.Equals("SetWindowPos", StringComparison.OrdinalIgnoreCase))
                {
                    if (line.Length <= 1)
                    {
                        throw new ExtractException("ELI30170", "SetWindowPos is missing argument.");
                    }

                    // Set the window position
                    ScriptSetWindowPosition(line[1].Trim());
                }
                else if (command.Equals("HideButtons", StringComparison.OrdinalIgnoreCase))
                {
                    if (line.Length <= 1)
                    {
                        throw new ExtractException("ELI30185", "HideButtons is missing argument.");
                    }

                    // Indicate that a button or menu was hidden
                    _buttonOrMenuHidden = true;

                    // Hide the specified buttons
                    ScriptHideButtons(line[1].Trim());
                }
                else if (command.Equals("HideMenu", StringComparison.OrdinalIgnoreCase))
                {
                    // Indicate that a button or menu was hidden
                    _buttonOrMenuHidden = true;

                    // Hide the menu strip
                    _menuStrip.Visible = false;
                }
                else if (command.Equals("OpenFile", StringComparison.OrdinalIgnoreCase))
                {
                    if (line.Length <= 1)
                    {
                        throw new ExtractException("ELI30171", "OpenFile is missing argument.");
                    }

                    // Open the image file
                    _imageViewer.OpenImage(line[1].Trim(), true);
                }
                else if (command.Equals("AddTempHighlight", StringComparison.OrdinalIgnoreCase))
                {
                    if (line.Length <= 1)
                    {
                        throw new ExtractException("ELI30172", "AddTempHighlight is missing argument.");
                    }
                    if (!_imageViewer.IsImageAvailable)
                    {
                        throw new ExtractException("ELI30173",
                            "Cannot add a temporary highlight without an open image.");
                    }

                    // Add a temporary highlight
                    ScriptAddTemporaryHighlight(line[1].Trim());
                }
                else if (command.Equals("ClearTempHighlights", StringComparison.OrdinalIgnoreCase))
                {
                    ScriptClearTemporaryHighlights();
                }
                else if (command.Equals("ClearImage", StringComparison.OrdinalIgnoreCase))
                {
                    // Check if there is an open image
                    if (_imageViewer.IsImageAvailable)
                    {
                        // Close the open image
                        _imageViewer.CloseImage();
                    }
                }
                else if (command.Equals("SetCurrentPageNumber", StringComparison.OrdinalIgnoreCase))
                {
                    if (line.Length <= 1)
                    {
                        throw new ExtractException("ELI30174", "SetCurrentPageNumber is missing argument.");
                    }
                    int pageNumber = int.Parse(line[1].Trim(), CultureInfo.CurrentCulture);
                    if (_imageViewer.IsImageAvailable)
                    {
                        // Go to the specified page
                        _imageViewer.PageNumber = pageNumber;
                    }
                    else
                    {
                        throw new ExtractException("ELI30175", "Cannot change page, no image is open.");
                    }
                }
                else if (command.Equals("ZoomIn", StringComparison.OrdinalIgnoreCase))
                {
                    if (_imageViewer.IsImageAvailable)
                    {
                        // Zoom in
                        _imageViewer.ZoomIn();
                    }
                }
                else if (command.Equals("ZoomOut", StringComparison.OrdinalIgnoreCase))
                {
                    if (_imageViewer.IsImageAvailable)
                    {
                        // Zoom out
                        _imageViewer.ZoomOut();
                    }
                }
                else if (command.Equals("ZoomExtents", StringComparison.OrdinalIgnoreCase))
                {
                    // Toggle fit to page mode
                    _imageViewer.FitMode = _imageViewer.FitMode == FitMode.FitToPage ?
                        FitMode.None : FitMode.FitToPage;
                }
                else if (command.Equals("CenterOnTempHighlight", StringComparison.OrdinalIgnoreCase))
                {
                    if (_tempHighlights.Count > 0)
                    {
                        // Change to page for first highlight if necessary
                        if (_imageViewer.PageNumber != _tempHighlights[0].PageNumber)
                        {
                            _imageViewer.PageNumber = _tempHighlights[0].PageNumber;
                        }

                        _imageViewer.CenterOnLayerObjects(_tempHighlights[0]);
                    }
                }
                else if (command.Equals("ZoomToTempHighlight", StringComparison.OrdinalIgnoreCase))
                {
                    if (_tempHighlights.Count > 0)
                    {
                        // Change to page for first highlight if necessary
                        if (_imageViewer.PageNumber != _tempHighlights[0].PageNumber)
                        {
                            _imageViewer.PageNumber = _tempHighlights[0].PageNumber;
                        }

                        Rectangle rectangle = _imageViewer.GetTransformedRectangle(
                            _tempHighlights[0].GetBounds(), false);
                        _imageViewer.ZoomToRectangle(rectangle);
                    }
                }
                else if (command.Equals("Pause", StringComparison.OrdinalIgnoreCase))
                {
                    StringBuilder sb = new StringBuilder("File: ");
                    sb.Append(scriptFile);
                    sb.AppendLine();
                    sb.Append("Paused:");

                    // Add any text following the Pause command
                    for (int i = 1; i < line.Length; i++)
                    {
                        sb.Append(" ");
                        sb.Append(line[i]);
                    }

                    MessageBox.Show(sb.ToString(), "Script Paused",
                        MessageBoxButtons.OK, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1, 0);
                }
                else
                {
                    ExtractException ee = new ExtractException("ELI30160",
                        "Unrecognized script command.");
                    ee.AddDebugData("Script File Name", scriptFile, false);
                    ee.AddDebugData("Script Command Line", scriptCommand, false);
                    throw ee;
                }

                // Invalidate the image viewer after each command is processed
                _imageViewer.Invalidate();
            }
        }

        /// <summary>
        /// Handles setting the window position for the <see cref="Form"/>.
        /// </summary>
        /// <param name="argument">The window position argument.
        /// <list type="bullet">
        /// <item>Full</item>
        /// <item>Left</item>
        /// <item>Top</item>
        /// <item>Right</item>
        /// <item>Bottom</item>
        /// <item>left,right,top,bottom - coordinates</item>
        /// </list>
        /// <example>argument = 0,100,0,500</example>
        /// </param>
        void ScriptSetWindowPosition(string argument)
        {
            Screen screen = Screen.PrimaryScreen;
            Rectangle workingArea = screen.WorkingArea;
            Rectangle bounds = workingArea;
            if (argument.Contains(","))
            {
                // Get the specified coordinates
                string[] coords = argument.Split(',');
                if (coords.Length != 4)
                {
                    ExtractException ee = new ExtractException("ELI30161",
                        "Invalid coordinates specified for SetWindowPos.");
                    ee.AddDebugData("Coordinates", argument, false);
                    throw ee;
                }
                int left = int.Parse(coords[0].Trim(), CultureInfo.CurrentCulture);
                int right = int.Parse(coords[1].Trim(), CultureInfo.CurrentCulture);
                int top = int.Parse(coords[2].Trim(), CultureInfo.CurrentCulture);
                int bottom = int.Parse(coords[3].Trim(), CultureInfo.CurrentCulture);
                bounds = Rectangle.FromLTRB(left, top, right, bottom);
            }
            else if (argument.Equals("Left", StringComparison.OrdinalIgnoreCase))
            {
                bounds.Width /= 2;
            }
            else if (argument.Equals("Right", StringComparison.OrdinalIgnoreCase))
            {
                bounds.Width /= 2;
                bounds.Location = new Point(bounds.Width, bounds.Top);
            }
            else if (argument.Equals("Top", StringComparison.OrdinalIgnoreCase))
            {
                bounds.Height /= 2;
            }
            else if (argument.Equals("Bottom", StringComparison.OrdinalIgnoreCase))
            {
                bounds.Height /= 2;
                bounds.Location = new Point(bounds.Left, bounds.Height);
            }
            else if (argument.Equals("Full", StringComparison.OrdinalIgnoreCase))
            {
                // Nothing to do, bounds are already set
            }
            else
            {
                ExtractException ee = new ExtractException("ELI30162",
                    "Invalid position argument for SetWindowPos.");
                ee.AddDebugData("Position Argument", argument, false);
                throw ee;
            }

            // Set the bounds for the form
            Bounds = bounds;

            // Ensure the start position for the form is set to manual
            StartPosition = FormStartPosition.Manual;
        }

        /// <summary>
        /// Handles adding a temporary highlight to the <see cref="ExtractImageViewerForm"/>
        /// </summary>
        /// <param name="argument">The argument containing the coordinates and page number
        /// for the highlight.</param>
        void ScriptAddTemporaryHighlight(string argument)
        {
            try
            {
                string[] data = argument.Split(',');
                if (data.Length != 6)
                {
                    ExtractException ee = new ExtractException("ELI30166",
                        "AddTemporaryHighlight coordinates argument is invalid.");
                    ee.AddDebugData("Coordinates", argument, false);
                    throw ee;
                }

                // Get the coordinates, height and page number
                Point startPoint = new Point(int.Parse(data[0], CultureInfo.CurrentCulture),
                    int.Parse(data[1], CultureInfo.CurrentCulture));
                Point endPoint = new Point(int.Parse(data[2], CultureInfo.CurrentCulture),
                    int.Parse(data[3], CultureInfo.CurrentCulture));
                int height = int.Parse(data[4], CultureInfo.CurrentCulture);
                int pageNumber = int.Parse(data[5], CultureInfo.CurrentCulture);

                try
                {
                    // Remove the layer object added handler while the temp highlight is added
                    _imageViewer.LayerObjects.LayerObjectAdded -= HandleLayerObjectAdded;

                    // Add the highlight to the image viewer
                    Highlight highlight = new Highlight(_imageViewer, "Temp Highlight",
                        startPoint, endPoint, height, pageNumber);
                    _imageViewer.LayerObjects.Add(highlight);
                    _tempHighlights.Add(highlight);

                    // Move to the page containing the highlight if it is not visible
                    if (_imageViewer.PageNumber != pageNumber)
                    {
                        _imageViewer.PageNumber = pageNumber;
                    }
                }
                finally
                {
                    // Add the layer object added handler back now that the temp highlight
                    // has been added
                    _imageViewer.LayerObjects.LayerObjectAdded += HandleLayerObjectAdded;
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI30167",
                    "Unable to add new temporary highlight to image.", ex);
            }
        }

        /// <summary>
        /// Handles clearing all temporary highlights on the <see cref="ExtractImageViewerForm"/>.
        /// </summary>
        void ScriptClearTemporaryHighlights()
        {
            try
            {
                if (_tempHighlights.Count > 0)
                {
                    try
                    {
                        // Remove the layer objects deleted event handler while removing
                        // the temporary highlights
                        _imageViewer.LayerObjects.LayerObjectDeleted -= HandleLayerObjectDeleted;
                        _imageViewer.LayerObjects.Remove(_tempHighlights);
                        _tempHighlights.Clear();
                    }
                    finally
                    {
                        _imageViewer.LayerObjects.LayerObjectDeleted += HandleLayerObjectDeleted;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI30168",
                    "Unable to clear temporary highlights.", ex);
            }
        }

        /// <summary>
        /// Handles hiding the buttons on the <see cref="ExtractImageViewerForm"/>.
        /// </summary>
        /// <param name="argument">The argument containing the ID's of the buttons to hide.</param>
        void ScriptHideButtons(string argument)
        {
            try
            {
                string[] buttonIds = argument.Split(',');
                foreach (string buttonId in buttonIds)
                {
                    // Get the id
                    int id = int.Parse(buttonId, CultureInfo.CurrentCulture);

                    switch (id)
                    {
                        case ImageViewerControlId.FirstPageButton:
                            _firstPageToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.FitToPageButton:
                            _fitToPageToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.FitToWidthButton:
                            _fitToWidthToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.HighlightTextSplitButton:
                            _highlightToolStripSplitButton.Visible = false;
                            break;

                        case ImageViewerControlId.LastPageButton:
                            _lastPageToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.NavigateToPageEditBox:
                            _pageNavigationToolStripTextBox.Visible = false;
                            break;

                        case ImageViewerControlId.NextPageButton:
                            _nextPageToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.NextTileButton:
                            _nextTileToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.OpenImageButton:
                            _openImageToolStripSplitButton.Visible = false;
                            break;

                        case ImageViewerControlId.OpenSubImageWindowButton:
                            // TODO: Hide this button
                            break;

                        case ImageViewerControlId.PanButton:
                            _panToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.PreviousPageButton:
                            _previousPageToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.PreviousTileButton:
                            _previousTileToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.PrintButton:
                            _printImageToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.RotateClockwiseButton:
                            _rotateClockwiseToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.RotateCounterClockwiseButton:
                            _rotateCounterclockwiseToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.SelectLayerObjectButton:
                            _selectLayerObjectToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.ThumbnailViewerButton:
                            _thumbnailsToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.ZoomInButton:
                            _zoomInToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.ZoomNextButton:
                            _zoomNextToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.ZoomOutButton:
                            _zoomOutToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.ZoomPreviousButton:
                            _zoomPreviousToolStripButton.Visible = false;
                            break;

                        case ImageViewerControlId.ZoomWindowButton:
                            _zoomWindowToolStripButton.Visible = false;
                            break;

                        default:
                            ExtractException ee = new ExtractException("ELI30183",
                                "Invalid button id specified");
                            ee.AddDebugData("Invalid ID", id, false);
                            throw ee;
                    }
                }

                // After hiding the buttons, ensure that there are no extra tool strip separators
                FormsMethods.HideUnnecessaryToolStripSeparators(
                    _toolStripContainer.TopToolStripPanel.Controls);
                FormsMethods.HideUnnecessaryToolStripSeparators(
                    _toolStripContainer.BottomToolStripPanel.Controls);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI30184", ex);
                ee.AddDebugData("Argument", argument, false);
                throw ee;
            }
        }

        /// <summary>
        /// Loads the previously saved state of the user interface.
        /// </summary>
        void LoadState()
        {
            try
            {
                // Load memento if it exists
                if (File.Exists(_FORM_PERSISTENCE_FILE))
                {
                    // Load the XML
                    XmlDocument document = new XmlDocument();
                    document.Load(_FORM_PERSISTENCE_FILE);

                    // Convert the XML to the memento
                    XmlElement element = (XmlElement)document.FirstChild;
                    FormMemento memento = FormMemento.FromXmlElement(element);

                    // Restore the saved state
                    memento.Restore(this);
                    FormsMethods.ToolStripManagerLoadHelper(_toolStripContainer);
                    _sandDockManager.LoadLayout();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI30163",
                    "Unable to load previous extract image viewer setting state.", ex);
                ee.AddDebugData("Invalid XML file", _FORM_PERSISTENCE_FILE, false);
                ee.Log();
            }
        }

        /// <summary>
        /// Saves the state of the user interface.
        /// </summary>
        void SaveState()
        {
            try
            {
                // Get the pertinent information
                FormMemento memento = new FormMemento(this);
                _sandDockManager.SaveLayout();
                
                // Only save the toolstrip settings if a toolstrip was not hidden
                if (!_buttonOrMenuHidden)
                {
                    ToolStripManager.SaveSettings(this);
                }

                // Convert the memento to XML
                XmlDocument document = new XmlDocument();
                XmlElement element = memento.ToXmlElement(document);
                document.AppendChild(element);

                // Create the directory for the file if necessary
                string directory = Path.GetDirectoryName(_FORM_PERSISTENCE_FILE);
                Directory.CreateDirectory(directory);

                // Save the XML
                document.Save(_FORM_PERSISTENCE_FILE);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI30164",
                    "Unable to persist extract image viewer state settings.", ex);
                ee.AddDebugData("XML File Name", _FORM_PERSISTENCE_FILE, false);
                ee.Log();
            }
        }

        #endregion Methods
    }
}