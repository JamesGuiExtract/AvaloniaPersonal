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

namespace Extract.Imaging.Utilities.ExtractImageViewer
{
    /// <summary>
    /// Represents a dialog with controls to display and interact with image files.
    /// </summary>
    public partial class ExtractImageViewerForm : Form
    {
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
            InitializeComponent();
           
            // Check if a filename was specified
            if (fileName != null)
            {
                _imageViewer.OpenImage(fileName, true);
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            // Call the base class
            base.OnLoad(e);

            // Establish connections with all controls on this form.
            _imageViewer.EstablishConnections(this);
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
            // Allow the image viewer to handle this keyboard shortcut.
            // If the image viewer does not process it, bubble it up to the base class.
            return _imageViewer.Shortcuts.ProcessKey(keyData) || 
                base.ProcessCmdKey(ref msg, keyData);
        }
    }
}