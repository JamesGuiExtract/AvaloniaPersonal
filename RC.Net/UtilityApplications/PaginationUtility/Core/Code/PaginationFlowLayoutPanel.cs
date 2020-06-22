using System;
using System.Drawing;
using System.Linq;
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
        /// Pending scroll scheduled via ScheduleScrollToControl
        /// </summary>
        ScrollTarget _scrollToControlTarget;

        /// <summary>
        /// Keeps track of the scroll position (relative to a particular control) that should be
        /// restored once the DEP is closed.
        /// </summary>
        ScrollTarget _scrollRestorePosition;

        /// <summary>
        /// A control that should be scrolled to after the next _layoutEngine has executed a pending
        /// layout paired with a topAlignmentOffset that indicates how far down vertically the control
        /// should appear (0 to leave the control flush with the top of the view, if possible).
        /// </summary>
        ScrollTarget _activeScrollTarget;

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
            Application.Idle += HandleApplicationIdle;
            _layoutEngine.LayoutCompleted += HandleLayoutEngine_LayoutCompleted;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Enables <see cref="RequestScrollToControl"/> called for the ongoing event (message chain)
        /// to be activated. 
        /// </summary>
        public void EnableScrollToControlForEvent()
        {
            _scrollToControlTarget = _scrollToControlTarget ?? new ScrollTarget();
            _scrollToControlTarget.Pending = true;
        }

        /// <summary>
        /// Requests the specified <see paramref="control"/> to be scrolled into view the next time
        /// the UI is idle. 
        /// NOTE: The request will only be honored if either <see paramref="activateScrollToControlForEvent"/>
        /// is specified as true or a separate call to <see cref="EnableScrollToControlForEvent"/> is
        /// made at some point during the ongoing event (message chain).
        /// </summary>
        /// <param name="control">The control to scroll into view.</param>
        /// <param name="topAlignmentOffset">If specified, the vertical pixels from the top of the panel
        /// the control should be positioned; if <c>null</c> the panel will be scrolled just enough to
        /// bring the control completely in view if it is not already.</param>
        /// <param name="activateScrollToControlForEvent"></param>
        public void RequestScrollToControl(Control control,
            int? topAlignmentOffset = null, bool activateScrollToControlForEvent = false)
        {
            try
            {
                if (_scrollToControlTarget?.TopAlignmentOffset == null)
                {
                    _scrollToControlTarget = new ScrollTarget()
                    {
                        Control = control,
                        TopAlignmentOffset = topAlignmentOffset,
                        Pending = _scrollToControlTarget?.Pending ?? false
                    };
                }

                if (activateScrollToControlForEvent)
                {
                    _scrollToControlTarget.Pending = true;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49905");
            }
        }

        /// <summary>
        /// Disregards any active scroll request for the ongoing event (message chain)
        /// </summary>
        public void CancelScrollRequest()
        {
            _scrollToControlTarget = null;
        }

        /// <summary>
        /// The current position of the specified <see paramref="control"/> relative to the top
        /// of the panel is noted as scroll position to be restored via <see cref="RequestScrollPositionRestore"/>.
        /// </summary>
        public void SetScrollRestorePosition(Control control)
        {
            _scrollRestorePosition = new ScrollTarget() {
                Control = control,
                TopAlignmentOffset = control.Location.Y
            };
        }

        /// <summary>
        /// Requests that the scroll position recorded via <see cref="SetScrollRestorePosition"/> be restored.
        /// </summary>
        public void RequestScrollPositionRestore()
        {
            if (_scrollRestorePosition?.Control != null)
            {
                _scrollRestorePosition.Pending = true;
            }
        }

        /// <summary>
        /// Executes the scroll operation specified by the <see paramref="scrollTarget"/>.
        /// </summary>
        void ApplyScrollTarget(ScrollTarget scrollTarget)
        {
            if (scrollTarget.Pending)
            {
                ScrollControlIntoViewManual(scrollTarget.Control, scrollTarget.TopAlignmentOffset);
            }
        }

        /// <summary>
        /// Allows the specified <see paramref="control"/> to be manually scrolled into into view.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to be scrolled into view.</param>
        /// <param name="topAlignmentOffset">The number of pixels vertically the <see paramref="control"/>
        /// should appear from the top of the panel or <c>null</c> to scroll the control into view if
        /// necessary but not modify the scroll position any more than necessary to do so.</param>
        void ScrollControlIntoViewManual(Control control, int? topAlignmentOffset)
        {
            try
            {
                _activeScrollTarget = new ScrollTarget()
                    { Control = control, TopAlignmentOffset = topAlignmentOffset };

                if (!_layoutEngine.LayoutPending)
                {
                    _activeScrollTarget.Pending = true;

                    if (_activeScrollTarget.TopAlignmentOffset.HasValue)
                    {
                        _layoutEngine.ScrollTarget = _activeScrollTarget;
                        PerformLayout();
                    }
                    else
                    {
                        base.ScrollControlIntoView(_activeScrollTarget.Control);
                    }
                }   
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35615");
            }
            finally
            {
                _activeScrollTarget.Pending = false;
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
                if (activeControl == _activeScrollTarget?.Control
                    && _activeScrollTarget.Pending)
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
        /// Any time the application goes idle, check for and apply pending _scrollRestorePosition
        /// or _scrollToControlTarget requests.
        /// </summary>
        void HandleApplicationIdle(object sender, EventArgs e)
        {
            // Restore any specified _scrollRestorePosition before handling _pendingScrollPosition.
            if (_scrollRestorePosition?.Pending == true)
            {
                ApplyScrollTarget(_scrollRestorePosition);
                _scrollRestorePosition = null;
            }

            if (_scrollToControlTarget?.Control == null
                || _scrollToControlTarget.Pending != true)
            {
                _scrollToControlTarget = null;
                return;
            }

            var targetControl = _scrollToControlTarget.Control;
            var topAlignmentOffset = _scrollToControlTarget.TopAlignmentOffset;
            _scrollToControlTarget = null;

            // If targetControl is an expanded document's separator without a specific
            // topAlignmentOffset specified and we need to scroll down, scroll down
            // far enough to see the first row of pages as well.
            if (topAlignmentOffset == null)
            {
                var separator = targetControl as PaginationSeparator;
                if (separator?.Collapsed == false
                    && separator.Document.PageControls.First().Bottom > ClientRectangle.Bottom)
                {
                    targetControl = separator.Document.PageControls.First();
                }
            }

            ScrollControlIntoViewManual(targetControl, topAlignmentOffset);

            _scrollToControlTarget = null;
        }

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
                // If there is a pending ScrollToControl or ScrollRestore, don't bother applying the current
                // active scroll target as it will soon be replaced. This saves unnecessary scrolls such as
                // when all docs are expanded/collapsed.
                if (_activeScrollTarget != null
                    && !ScrollToControlPending
                    && !ScrollRestorePending)
                {
                    var control = _activeScrollTarget.Control;
                    var topAlignmentOffset = _activeScrollTarget.TopAlignmentOffset;

                    if (control?.Visible == true)
                    {
                        ScrollControlIntoViewManual(control, topAlignmentOffset);
                    }

                    _activeScrollTarget = null;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41695");
            }
        }

        #endregion Event Handlers

        #region Private Members

        bool ScrollToControlPending
        {
            get
            {
                return _scrollToControlTarget?.Control != null
                    && _scrollToControlTarget.Pending == true;
            }
        }
        bool ScrollRestorePending
        {
            get
            {
                return _scrollRestorePosition?.Control != null
                    && _scrollRestorePosition.Pending == true;
            }
        }

        #endregion Private Members
    }

    /// <summary>
    /// Info to track scroll operation requests.
    /// </summary>
    class ScrollTarget
    {
        /// <summary>
        /// The control relative to which the scroll operation is to be executed
        /// </summary>
        public Control Control { get; set; }

        /// <summary>
        /// If specified, the vertical pixels from the top of the panel the Control should be
        /// positioned; if <c>null</c> the panel will be scrolled just enough to bring the control
        /// completely in view if it is not already.
        /// </summary>
        public int? TopAlignmentOffset { get; set; }

        /// <summary>
        /// Indicates whether this instance is currently pending execution.
        /// </summary>
        public bool Pending { get; set; }
    }
}
