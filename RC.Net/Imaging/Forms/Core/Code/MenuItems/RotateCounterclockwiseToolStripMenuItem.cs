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
    /// rotate an image counter clockwise.
    /// </summary>
    [ToolboxBitmap(typeof(RotateCounterclockwiseToolStripMenuItem),
       ToolStripButtonConstants._ROTATE_COUNTERCLOCKWISE_BUTTON_IMAGE_SMALL)]
    public partial class RotateCounterclockwiseToolStripMenuItem : ImageViewerCommandToolStripMenuItem
    {
        #region RotateCounterclockwiseToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="RotateCounterclockwiseToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public RotateCounterclockwiseToolStripMenuItem()
            : base(ToolStripButtonConstants._ROTATE_COUNTERCLOCKWISE_MENU_ITEM_TEXT,
            ToolStripButtonConstants._ROTATE_COUNTERCLOCKWISE_BUTTON_IMAGE_SMALL,
            typeof(RotateCounterclockwiseToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region RotateCounterclockwiseToolStripMenuItem Events

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
                    base.ImageViewer.SelectRotateCounterclockwise();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21411", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region RotateCounterclockwiseToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectRotateCounterclockwise);
        }

        #endregion RotateCounterclockwiseToolStripMenuItem Methods
    }
}
