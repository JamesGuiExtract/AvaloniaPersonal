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
    /// Represents a <see cref="ImageViewerCursorToolStripMenuItem"/> that enables the delete
    /// layer objects <see cref="CursorTool"/>.
    /// </summary>
    [ToolboxBitmap(typeof(DeleteLayerObjectsToolStripMenuItem),
       ToolStripButtonConstants._DELETE_LAYER_OBJECTS_BUTTON_IMAGE_SMALL)]
    public partial class DeleteLayerObjectsToolStripMenuItem : ImageViewerCursorToolStripMenuItem
    {
        #region DeleteLayerObjectsToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="DeleteLayerObjectsToolStripMenuItem"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]        
        public DeleteLayerObjectsToolStripMenuItem()
            : base(CursorTool.DeleteLayerObjects,
            ToolStripButtonConstants._DELETE_LAYER_OBJECTS_MENU_ITEM_TEXT,
            ToolStripButtonConstants._DELETE_LAYER_OBJECTS_BUTTON_IMAGE_SMALL,
            typeof(DeleteLayerObjectsToolStripMenuItem))
        {
            InitializeComponent();
        }

        #endregion

        #region DeleteLayerObjectsToolStripMenuItem Events

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
                    base.ImageViewer.CursorTool = CursorTool.DeleteLayerObjects;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21429", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region DeleteLayerObjectsToolStripMenuItem Methods

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected override Keys[] GetKeys()
        {
            return base.ImageViewer == null ? null :
                base.ImageViewer.Shortcuts.GetKeys(base.ImageViewer.SelectDeleteLayerObjectsTool);
        }

        #endregion DeleteLayerObjectsToolStripMenuItem Methods
    }
}
