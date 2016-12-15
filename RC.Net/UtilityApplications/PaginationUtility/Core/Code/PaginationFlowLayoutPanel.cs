using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

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

        /// <summary>
        /// A control that should be scrolled to after the next _layoutEngine has executed a pending layout.
        /// </summary>
        Control _scrollToControl;

        /// <summary>
        /// The <see cref="PaginationLayoutEngine"/> that manages the layout of the
        /// <see cref="PaginationControl"/>s.
        /// </summary>
        PaginationLayoutEngine _layoutEngine = new PaginationLayoutEngine();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationFlowLayoutPanel"/> class.
        /// </summary>
        public PaginationFlowLayoutPanel()
        {
            _layoutEngine.LayoutCompleted += HandleLayoutEngine_LayoutCompleted;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Allows the specified <see paramref="control"/> to be manually scrolled into into view.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to be scrolled into view.</param>
        public void ScrollControlIntoViewManual(Control control)
        {
            try
            {
                if (_layoutEngine.LayoutPending)
                {
                    // If the scroll is commanded before a layout, the end result may not have the
                    // control in view after all. Wait until the layout has completed before
                    // executing the scroll.
                    _scrollToControl = control;
                }
                else
                {
                    _allowScrollToControl = true;

                    base.ScrollControlIntoView(control);
                }   
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
        /// Gets a cached instance of the panel's layout engine.
        /// </summary>
        /// <returns>The <see cref="T:System.Windows.Forms.Layout.LayoutEngine"/> for the panel's
        /// contents. </returns>
        public override LayoutEngine LayoutEngine
        {
            get
            {
                return _layoutEngine;
            }
        }

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

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="PaginationLayoutEngine.LayoutCompleted"/> event of the
        /// <see cref="_layoutEngine"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LayoutCompletedEventArgs"/> instance containing the event data.</param>
        void HandleLayoutEngine_LayoutCompleted(object sender, LayoutCompletedEventArgs e)
        {
            try
            {
                if (_scrollToControl != null)
                {
                    var control = _scrollToControl;
                    _scrollToControl = null;
                    ScrollControlIntoViewManual(control);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41695");
            }
        }

        #endregion Event Handlers
    }
}
