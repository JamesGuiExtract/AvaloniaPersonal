using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.ObjectModel;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a prepopulated <see cref="ToolStrip"/> containing the
    /// Extract <see cref="ImageViewer"/> view commands.
    /// </summary>
    [ToolboxBitmap(typeof(ViewCommandsImageViewerToolStrip),
        ToolStripButtonConstants._VIEW_COMMANDS_TOOLSTRIP_IMAGE)]
    public partial class ViewCommandsImageViewerToolStrip : ImageViewerPrePopulatedToolStrip
    {
        #region Constructor

        /// <summary>
        /// Initializes a new <see cref="ViewCommandsImageViewerToolStrip"/> class.
        /// </summary>
        public ViewCommandsImageViewerToolStrip()
            : base()
        {
            InitializeComponent();
        }

        #endregion

        #region ImageViewerToolStrip

        /// <summary>
        /// Builds an <see langword="Array"/> of <see cref="ToolStripItem"/> 
        /// objects to be added to this <see cref="ToolStrip"/> when it is created.
        /// </summary>
        /// <returns>The <see cref="ToolStripItem"/> objects to be added
        /// to this <see cref="ToolStrip"/>.</returns>
        protected override ToolStripItem[] BuildToolStripItemCollection()
        {
            return new ToolStripItem[] {
                new ZoomInToolStripButton(),
                new ZoomOutToolStripButton(),
                new ZoomPreviousToolStripButton(),
                new ZoomNextToolStripButton(),
                new ToolStripSeparator(),
                new FitToPageToolStripButton(),
                new FitToWidthToolStripButton(),
                new OneToOneZoomToolStripButton(),
                new ToolStripSeparator(),
                new RotateCounterclockwiseToolStripButton(),
                new RotateClockwiseToolStripButton()};
        }

        #endregion
    }
}
