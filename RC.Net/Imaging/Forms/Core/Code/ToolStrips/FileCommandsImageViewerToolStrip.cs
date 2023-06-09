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
    /// Extract <see cref="ImageViewer"/> file commands.
    /// </summary>
    [ToolboxBitmap(typeof(FileCommandsImageViewerToolStrip),
        ToolStripButtonConstants._FILE_COMMANDS_TOOLSTRIP_IMAGE)]
    public partial class FileCommandsImageViewerToolStrip : ImageViewerPrePopulatedToolStrip
    {
        #region Constructor

        /// <summary>
        /// Initializes a new <see cref="FileCommandsImageViewerToolStrip"/> class.
        /// </summary>
        public FileCommandsImageViewerToolStrip()
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
                new OpenImageToolStripSplitButton(),
                new PrintImageToolStripButton()};
        }

        #endregion
    }
}
