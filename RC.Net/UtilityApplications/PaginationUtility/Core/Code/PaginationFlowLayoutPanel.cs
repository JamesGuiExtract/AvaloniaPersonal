using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="FlowLayoutPanel"/> where automatic scrolling to controls
    /// (via <see cref="ScrollableControl.ScrollControlIntoView"/> and
    /// <see cref="ScrollableControl.ScrollToControl"/>) is disabled to prevent unexpected resets of
    /// the scroll position. A control can be programmatically scrolled into view using
    /// <see cref="ScrollControlIntoViewManual"/>, however.
    /// </summary>
    internal class PaginationFlowLayoutPanel : FlowLayoutPanel
    {
        #region Fields

        /// <summary>
        /// Indicates whether the <see cref="ScrollToControl"/> method has been enabled by a call to
        /// <see cref="ScrollControlIntoViewManual"/>.
        /// </summary>
        bool _allowScrollToControl;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Allows the specified <see paramref="control"/> to be manually scrolled into into view.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to be scrolled into view.</param>
        public void ScrollControlIntoViewManual(Control control)
        {
            try
            {
                _allowScrollToControl = true;

                base.ScrollControlIntoView(control);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35615");
            }
            finally
            {
                _allowScrollToControl = false;
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Overrides <see cref="ScrollableControl.ScrollToControl"/> in order to prevent
        /// automatically scrolling to a control.
        /// </summary>
        /// <param name="activeControl">The child control to scroll into view.</param>
        /// <returns>The upper-left hand <see cref="Point"/> of the display area relative to the
        /// client area that should be to scrolled to.</returns>
        protected override Point ScrollToControl(Control activeControl)
        {
            try
            {
                if (_allowScrollToControl)
                {
                    return base.ScrollToControl(activeControl);
                }
                else
                {
                    return base.AutoScrollPosition;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35616");
            }

            return base.ScrollToControl(activeControl);
        }

        #endregion Overrides
    }
}
