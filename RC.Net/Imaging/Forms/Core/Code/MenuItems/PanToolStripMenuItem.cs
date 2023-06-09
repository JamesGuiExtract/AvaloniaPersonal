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
    /// Represents a <see cref="ImageViewerCursorToolStripMenuItem"/> that enables the pan
    /// <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(PanToolStripMenuItem),
        ToolStripButtonConstants._PAN_BUTTON_IMAGE)]
    public partial class PanToolStripMenuItem : ImageViewerCursorToolStripMenuItem
    {
        #region PanToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="PanToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public PanToolStripMenuItem()
            : base(CursorTool.Pan,
            ToolStripButtonConstants._PAN_MENU_ITEM_TEXT,
            ToolStripButtonConstants._PAN_BUTTON_IMAGE,
            typeof(PanToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region PanToolStripMenuItem Events

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
                    base.ImageViewer.CursorTool = CursorTool.Pan;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21426", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region PanToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectPanTool);
        }

        #endregion PanToolStripMenuItem Methods
    }
}
