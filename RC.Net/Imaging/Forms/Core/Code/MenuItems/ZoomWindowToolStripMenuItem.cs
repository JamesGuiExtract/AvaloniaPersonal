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
    /// Represents a <see cref="ImageViewerCursorToolStripMenuItem"/> that enables the zoom window
    /// <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(ZoomWindowToolStripMenuItem),
       ToolStripButtonConstants._ZOOM_WINDOW_BUTTON_IMAGE_SMALL)]
    public partial class ZoomWindowToolStripMenuItem : ImageViewerCursorToolStripMenuItem
    {
        #region ZoomWindowToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="ZoomWindowToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public ZoomWindowToolStripMenuItem()
            : base(CursorTool.ZoomWindow,
            ToolStripButtonConstants._ZOOM_WINDOW_MENU_ITEM_TEXT,
            ToolStripButtonConstants._ZOOM_WINDOW_BUTTON_IMAGE_SMALL,
            typeof(ZoomWindowToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region ZoomWindowToolStripMenuItem Events

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
                    base.ImageViewer.CursorTool = CursorTool.ZoomWindow;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21419", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region ZoomWindowToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectZoomWindowTool);
        }

        #endregion ZoomWindowToolStripMenuItem Methods
    }
}
