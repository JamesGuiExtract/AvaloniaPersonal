using Extract.Drawing;
using Extract.Imaging.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

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

                // Add the event handler for layer objects being added
                _imageViewer.LayerObjects.LayerObjectAdded += HandleLayerObjectAdded;

                // Check if a filename was specified
                if (!string.IsNullOrEmpty(fileName))
                {
                    _imageViewer.OpenImage(fileName, true);
                }

                if (!string.IsNullOrEmpty(scriptFile))
                {
                    ProcessScriptFile(scriptFile);
                }
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

                if (_openImageSearchForm)
                {
                    _searchForImagesToolStripMenuItem.PerformClick();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30123", ex);
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        /// <returns><see langword="true"/> if the character was processed by the control; 
        /// <see langword="false"/> if the character was not processed.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
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

        ///// <summary>
        ///// Raises the <see cref="Form.FormClosing"/> event.
        ///// </summary>
        ///// <param name="e">The data associated with the event.</param>
        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    try
        //    {
        //        base.OnClosing(e);
        //    }
        //    catch (Exception ex)
        //    {
        //        ExtractException.Display("ELI30130", ex);
        //    }
        //}

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

                // Save is enabled if an image is available
                _saveGddImageButton.Enabled = _imageViewer.IsImageAvailable;

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
            ExtractException.Assert("ELI30159", "Specified script file does not exist.",
                File.Exists(scriptFile), "Script File Name", scriptFile);

            // Load the script file into a commented text file reader
            CommentedTextFileReader reader = new CommentedTextFileReader(scriptFile);

            string[] spaceToken = new string[] {" "};

            // Iterate the script commands and execute them
            foreach (string scriptCommand in reader)
            {
                string[] line = scriptCommand.Split(spaceToken, StringSplitOptions.RemoveEmptyEntries);
                string command = line[0];
                if (command.Equals("SetWindowPos", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (command.Equals("HideButtons", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (command.Equals("OpenFile", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (command.Equals("AddTempHighlight", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (command.Equals("ClearTempHighlights", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (command.Equals("ClearImage", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (command.Equals("SetCurrentPageNumber", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (command.Equals("ZoomIn", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (command.Equals("ZoomOut", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (command.Equals("ZoomExtents", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (command.Equals("CenterOnTempHighlight", StringComparison.OrdinalIgnoreCase))
                {
                }
                else if (command.Equals("ZoomToTempHighlight", StringComparison.OrdinalIgnoreCase))
                {
                }
                else
                {
                    ExtractException ee = new ExtractException("ELI30160",
                        "Unrecognized script command.");
                    ee.AddDebugData("Script File Name", scriptFile, false);
                    ee.AddDebugData("Script Command Line", scriptCommand, false);
                    throw ee;
                }
            }
        }

        #endregion Methods
    }
}