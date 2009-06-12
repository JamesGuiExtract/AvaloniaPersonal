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
    /// zoom out from an image.
    /// </summary>
    [ToolboxBitmap(typeof(ZoomOutToolStripMenuItem),
       ToolStripButtonConstants._ZOOM_OUT_BUTTON_IMAGE_SMALL)]
    public partial class ZoomOutToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region ZoomOutToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="ZoomOutToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public ZoomOutToolStripMenuItem()
            : base(ToolStripButtonConstants._ZOOM_OUT_MENU_ITEM_TEXT,
            ToolStripButtonConstants._ZOOM_OUT_BUTTON_IMAGE_SMALL,
            typeof(ZoomOutToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region ZoomOutToolStripMenuItem Events

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
                    base.ImageViewer.ZoomOut();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21391", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region ZoomOutToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectZoomOut);
        }

        #endregion ZoomOutToolStripMenuItem Methods
    }
}
