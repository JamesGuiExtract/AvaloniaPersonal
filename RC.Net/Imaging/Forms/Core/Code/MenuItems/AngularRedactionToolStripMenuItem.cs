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
    /// Represents a <see cref="ImageViewerCursorToolStripMenuItem"/> that enables the angular
    /// redaction <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(AngularRedactionToolStripMenuItem),
       ToolStripButtonConstants._ANGULAR_REDACTION_BUTTON_IMAGE_SMALL)]
    public partial class AngularRedactionToolStripMenuItem : ImageViewerCursorToolStripMenuItem
    {
        #region AngularRedactionToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="AngularRedactionToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public AngularRedactionToolStripMenuItem()
            : base(CursorTool.AngularRedaction,
            ToolStripButtonConstants._ANGULAR_REDACTION_MENU_ITEM_TEXT,
            ToolStripButtonConstants._ANGULAR_REDACTION_BUTTON_IMAGE_SMALL,
            typeof(AngularRedactionToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region AngularRedactionToolStripMenuItem Events

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
                    base.ImageViewer.CursorTool = CursorTool.AngularRedaction;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI22267", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        #endregion

        #region AngularRedactionToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return null;
        }

        #endregion AngularRedactionToolStripMenuItem Methods
    }
}

