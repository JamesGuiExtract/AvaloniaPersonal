using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using Extract.Imaging.Forms;
using Extract.Utilities.Forms;

namespace Extract.Imaging.Utilities.ExtractImageViewer
{
    /// <summary>
    /// Represents a dialog with controls to display and interact with image files.
    /// </summary>
    public partial class ExtractImageViewerForm : Form
    {
        #region Fields

        /// <summary>
        /// The OCR manager to use when performing OCR of highlights
        /// </summary>
        SynchronousOcrManager _ocrManager = new SynchronousOcrManager(OcrTradeoff.Accurate);

        #endregion Fields

        #region Constructors
        
        /// <overloads>Initializes a new instance of the <see cref="ExtractImageViewerForm"/> 
        /// class.</overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractImageViewerForm"/> class.
        /// </summary>
        public ExtractImageViewerForm()
            : this(null)
        {
        }
            
        /// <summary>
	    /// Initializes a new instance of the <see cref="ExtractImageViewerForm"/> class opened 
        /// with the specified image file.
	    /// </summary>
        /// <param name="fileName">The image file to open. <see langword="null"/> if no image file 
        /// should be opened.</param>
	    public ExtractImageViewerForm(string fileName)
        {
            try
            {
                InitializeComponent();

                // Set the icon
                Icon =
                    Extract.Imaging.Utilities.ExtractImageViewer.Properties.Resources.ExtractImageViewer;

                // Add the event handler for layer objects being added
                _imageViewer.LayerObjects.LayerObjectAdded += HandleLayerObjectAdded;

                // Check if a filename was specified
                if (fileName != null)
                {
                    _imageViewer.OpenImage(fileName, true);
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

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The arguments associated with the event.</param>
        void HandleLayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            try
            {
                Highlight highlight = e.LayerObject as Highlight;
                if (highlight != null)
                {
                    string temp;
                    using (TemporaryWaitCursor wait = new TemporaryWaitCursor())
                    {
                        temp = _ocrManager.GetOcrTextAsString(_imageViewer.ImageFile, highlight.ToRasterZone(), 0.2);
                    }

                    highlight.Text = temp;
                    using (CustomizableMessageBox message = new CustomizableMessageBox())
                    {
                        // Display the OCR result to the user
                        message.Caption = "OCR Result";
                        message.StandardIcon = MessageBoxIcon.None;
                        message.AddStandardButtons(MessageBoxButtons.OK);
                        message.Text = temp;
                        message.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30125", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleImageViewerImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                _saveGddImageButton.Enabled = _imageViewer.IsImageAvailable;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30126", ex);
            }
        }

        #endregion Event Handlers
    }
}