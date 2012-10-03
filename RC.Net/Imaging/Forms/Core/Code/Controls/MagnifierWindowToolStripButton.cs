using System.Drawing;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// A derived <see cref="ToolStripButton"/> that contains the image for a dockable window.
    /// </summary>
    [ToolboxBitmap(typeof(MagnifierWindowToolStripButton),
        ToolStripButtonConstants._MAGNIFIER_WINDOW_BUTTON_IMAGE)]
    public partial class MagnifierWindowToolStripButton : DockableWindowToolStripButton
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MagnifierWindowToolStripButton"/>
        /// class.
        /// </summary>
        public MagnifierWindowToolStripButton()
            : base(typeof(MagnifierWindowToolStripButton),
            ToolStripButtonConstants._MAGNIFIER_WINDOW_BUTTON_IMAGE)
        {
            base.Text = ToolStripButtonConstants._MAGNIFIER_WINDOW_BUTTON_TEXT;
        }

        #endregion Constructors
    }
}
