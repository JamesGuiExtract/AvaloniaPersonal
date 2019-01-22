using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a prepopulated <see cref="ToolStrip"/> containing the
    /// Extract <see cref="DocumentViewer"/> basic tools commands.
    /// </summary>
    [ToolboxBitmap(typeof(BasicToolsImageViewerToolStrip),
        ToolStripButtonConstants._BASIC_TOOLS_TOOLSTRIP_IMAGE)]
    public partial class BasicToolsImageViewerToolStrip : ImageViewerPrePopulatedToolStrip
    {
        #region Constructor

        /// <summary>
        /// Initializes a new <see cref="BasicToolsImageViewerToolStrip"/> class.
        /// </summary>
        public BasicToolsImageViewerToolStrip()
            : base()
        {
            InitializeComponent();
        }

        #endregion

        #region ImageViewerPrepopulatedToolStrip

        /// <summary>
        /// Builds an <see langword="Array"/> of <see cref="ToolStripItem"/> 
        /// objects to be added to this <see cref="ToolStrip"/> when it is created.
        /// </summary>
        /// <returns>The <see cref="ToolStripItem"/> objects to be added
        /// to this <see cref="ToolStrip"/>.</returns>
        protected override ToolStripItem[] BuildToolStripItemCollection()
        {
            return new ToolStripItem[] { 
                new ZoomWindowToolStripButton(),
                new PanToolStripButton(),
                new HighlightToolStripSplitButton(),
                new SelectLayerObjectToolStripButton()};
        }

        #endregion
    }
}
