using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ImageViewerCursorToolStripMenuItem"/> that enables the select
    /// highlights <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(SelectLayerObjectToolStripMenuItem),
        ToolStripButtonConstants._SELECT_LAYER_OBJECT_BUTTON_IMAGE)]
    public partial class SelectLayerObjectToolStripMenuItem : ImageViewerCursorToolStripMenuItem
    {
        #region Constructor

        /// <summary>
        /// Initializes a new <see cref="SelectLayerObjectToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public SelectLayerObjectToolStripMenuItem()
            : base(CursorTool.SelectLayerObject,
            ToolStripButtonConstants._SELECT_LAYER_OBJECT_BUTTON_TEXT,
            ToolStripButtonConstants._SELECT_LAYER_OBJECT_BUTTON_IMAGE,
            typeof(SelectLayerObjectToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion Constructor

        #region Events
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
                    base.ImageViewer.CursorTool = CursorTool.SelectLayerObject;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21615", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion Events

        #region SelectLayerObjectToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectSelectLayerObjectsTool);
        }

        #endregion SelectLayerObjectToolStripMenuItem Methods
    }
}
