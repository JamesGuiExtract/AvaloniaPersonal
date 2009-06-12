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
    /// Represents a <see cref="ImageViewerCursorToolStripMenuItem"/> that enables the rectangular
    /// redaction <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(RectangularRedactionToolStripMenuItem),
        ToolStripButtonConstants._RECTANGULAR_REDACTION_BUTTON_IMAGE)]
    public partial class RectangularRedactionToolStripMenuItem : ImageViewerCursorToolStripMenuItem
    {
        #region RectangularRedactionToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="RectangularRedactionToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public RectangularRedactionToolStripMenuItem()
            : base(CursorTool.RectangularRedaction,
            ToolStripButtonConstants._RECTANGULAR_REDACTION_MENU_ITEM_TEXT,
            ToolStripButtonConstants._RECTANGULAR_REDACTION_BUTTON_IMAGE,
            typeof(RectangularRedactionToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region RectangularRedactionToolStripMenuItem Events

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
                base.OnClick(e);

                if (base.ImageViewer != null && base.ImageViewer.IsImageAvailable)
                {
                    base.ImageViewer.CursorTool = CursorTool.RectangularRedaction;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22266", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        #endregion

        #region RectangularRedactionToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return null;
        }

        #endregion RectangularRedactionToolStripMenuItem Methods
    }
}

