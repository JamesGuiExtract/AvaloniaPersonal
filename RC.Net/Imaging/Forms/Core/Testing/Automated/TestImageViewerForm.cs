using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms.Test
{
    /// <summary>
    /// Represents a dialog that contains all the Extract.Imaging.Forms components for testing 
    /// purposes.
    /// </summary>
    public partial class TestImageViewerForm : Form
    {
        #region TestImageViewerForm Constructors

        /// <summary>
        /// Initializes a new <see cref="TestImageViewerForm"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public TestImageViewerForm()
        {
            InitializeComponent();
        }

        #endregion TestImageViewerForm Constructors

        #region TestImageViewerForm OnEvents

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

            // Load the toolstrip settings
            ToolStripManager.LoadSettings(this);
        }

        /// <summary>
        /// Raises the <see cref="Form.Closing"/> event.
        /// </summary>
        /// <param name="e">An <see cref="CancelEventArgs"/> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // Save the toolstrip settings
            ToolStripManager.SaveSettings(this);
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        /// <returns><see langword="true"/> if the character was processed by the control; 
        /// <see langword="false"/> if the character was not processed.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Allow the image viewer to handle this keyboard shortcut.
            // If the image viewer does not process it, bubble it up to the base class.
            return _imageViewer.Shortcuts.ProcessKey(keyData) ||
                base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion TestImageViewerForm OnEvents
    }
}