using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Provides the <see langword="abstract"/> base class for a
    /// prepopulated <see cref="ToolStrip"/> containing specific
    /// <see cref="ImageViewer"/> commands and tools.
    /// </summary>
    public abstract partial class ImageViewerPrePopulatedToolStrip : ToolStrip
    {
        #region Constructor

        /// <summary>
        /// Initializes a new <see cref="ImageViewerPrePopulatedToolStrip"/> class.
        /// </summary>
        protected ImageViewerPrePopulatedToolStrip()
        {
            InitializeComponent();

            // Add the collection of items for this tool strip
            base.Items.AddRange( BuildToolStripItemCollection() );
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Builds an <see langword="Array"/> of <see cref="ToolStripItem"/> 
        /// objects to be added to this <see cref="ToolStrip"/> when it is created.
        /// </summary>
        /// <returns>The <see cref="ToolStripItem"/> objects to be added
        /// to this <see cref="ToolStrip"/>.</returns>
        protected abstract ToolStripItem[] BuildToolStripItemCollection();

        #endregion

        #region Overrides

        /// <summary>
        /// Override for the <see cref="ToolStripItemCollection"/> contained in
        /// this control. This override is used to set <see cref="BrowsableAttribute"/>
        /// for this control to <see langword="false"/>.
        /// </summary>
        [Browsable(false)]
        public override ToolStripItemCollection Items
        {
            get
            {
                return base.Items;
            }
        }

        #endregion
    }
}
