using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// A <see langword="class"/> to allow auto-scrolling of content that is too big for the
    /// <see cref="Control"/> in which it is contained, but with behavior that is improved compared
    /// with that of <see cref="Panel"/> with auto scroll enabled.
    /// </summary>
    internal class DataEntryScrollPanel : Panel
    {
        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="DataEntryScrollPanel"/> instance.
        /// </summary>
        public DataEntryScrollPanel()
            : base()
        {
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Overrides Panel.ScrollToControl in order to prevent automatically scrolling to a control
        /// when the mouse button is depressed. (Prevents errant selections of objects as the panel 
        /// scrolls).
        /// </summary>
        /// <param name="activeControl">The child control to scroll into view.</param>
        /// <returns>The upper-left hand <see cref="Point"/> of the display area relative to the
        /// client area that should be to scrolled to.</returns>
        protected override Point ScrollToControl(Control activeControl)
        {
            Point scrollTarget = base.AutoScrollPosition;

            try
            { 
                Point mousePosition = base.PointToClient(Control.MousePosition);

                // If the mouse button is depressed and within the bounds of the panel, return the
                // current position of the scroll bar to prevent it from scrolling.
                if (base.Bounds.Contains(mousePosition) && 
                    (Control.MouseButtons & MouseButtons.Left) != 0)
                {
                    return base.AutoScrollPosition;
                }

                // If the mouse button is not depressed, allow the base class to be called as it
                // normally would.
                scrollTarget = base.ScrollToControl(activeControl);
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI25051", ex).Display();
            }

            return scrollTarget;
        }

        #endregion Overrides
    }
}
