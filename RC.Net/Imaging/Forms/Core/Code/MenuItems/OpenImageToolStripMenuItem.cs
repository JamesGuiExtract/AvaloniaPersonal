using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ImageViewerCommandToolStripMenuItem"/> that activates the open
    /// file command.
    /// </summary>
    [ToolboxBitmap(typeof(OpenImageToolStripMenuItem),
        ToolStripButtonConstants._OPEN_IMAGE_BUTTON_IMAGE_SMALL)]
    public partial class OpenImageToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region OpenImageToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="OpenImageToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public OpenImageToolStripMenuItem()
            : base(ToolStripButtonConstants._OPEN_IMAGE_MENU_ITEM_TEXT,
            ToolStripButtonConstants._OPEN_IMAGE_BUTTON_IMAGE_SMALL,
            typeof(OpenImageToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region OpenImageToolStripMenuItem Overrides

        /// <summary>
        /// Sets the enabled state depending on the state of the associated image viewer control.
        /// </summary>
        protected override void SetEnabledState()
        {
            // Enable this button if there is an image viewer
            base.Enabled = base.ImageViewer != null;
        }

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectOpenImage);
        }

        #endregion

        #region OpenImageToolStripMenuItem Events

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        /// <seealso cref="Control.OnClick"/>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                if (base.ImageViewer != null)
                {
                    // Display a dialog and allow the user to open an image file.
                    base.ImageViewer.SelectOpenImage();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21570", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion
    }
}