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
    /// Represents a <see cref="ImageViewerCommandToolStripMenuItem"/> that allows the user to
    /// zoom into an image.
    /// </summary>
    [ToolboxBitmap(typeof(ZoomInToolStripMenuItem),
       ToolStripButtonConstants._ZOOM_IN_BUTTON_IMAGE_SMALL)]
    public partial class ZoomInToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region ZoomInToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="ZoomInToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public ZoomInToolStripMenuItem()
            : base(ToolStripButtonConstants._ZOOM_IN_MENU_ITEM_TEXT,
            ToolStripButtonConstants._ZOOM_IN_BUTTON_IMAGE_SMALL,
            typeof(ZoomInToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region ZoomInToolStripMenuItem Events

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
                if (base.ImageViewer != null && base.ImageViewer.IsImageAvailable)
                {
                    base.ImageViewer.ZoomIn();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21386", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region ZoomInToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectZoomIn);
        }

        #endregion ZoomInToolStripMenuItem Methods
    }
}
