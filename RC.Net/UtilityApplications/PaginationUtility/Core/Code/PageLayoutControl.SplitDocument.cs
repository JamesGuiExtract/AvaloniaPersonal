using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// PageLayoutControl code to implement UI queue to split one document into two.
    /// </summary>
    partial class PageLayoutControl
    {
        #region Fields

        /// <summary>
        /// The UI element the user can interact with to split documents
        /// </summary>
        SplitDocumentIndicator _splitDocumentIndicator = new SplitDocumentIndicator();

        /// <summary>
        /// The page control that would be the first page of a new document once activiated.
        /// </summary>
        PageThumbnailControl _activeSplitTarget;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Registers the events necessary to implement the split document control
        /// </summary>
        void SplitDocument_Init()
        {
            _flowLayoutPanel.MouseMove += HandleSplitIndicator_MouseMove;
            _flowLayoutPanel.MouseDown += HandleSplitIndicator_MouseDown;
            _flowLayoutPanel.MouseUp += HandleSplitIndicator_MouseUp;
            _splitDocumentIndicator.MouseMove += HandleSplitIndicator_MouseMove;
            _splitDocumentIndicator.MouseDown += HandleSplitIndicator_MouseDown;
            _splitDocumentIndicator.MouseUp += HandleSplitIndicator_MouseUp;
            _splitDocumentIndicator.ActivationComplete += HandleSplitDocumentIndicator_ActivationComplete;
        }

        /// <summary>
        /// Registers events for the specified <see paramref="control"/> necessary to implement the\
        /// split document control.
        /// </summary>
        void RegisterWithSplitterControl(PaginationControl control)
        {
            control.MouseMove += HandleSplitIndicator_MouseMove;
            control.MouseDown += HandleSplitIndicator_MouseDown;
            control.MouseUp += HandleSplitIndicator_MouseUp;
        }

        /// <summary>
        /// Unregisters events for the specified <see paramref="control"/> necessary to implement the\
        /// split document control.
        /// </summary>
        void UnRegisterWithSplitterControl(PaginationControl control)
        {
            control.MouseMove -= HandleSplitIndicator_MouseMove;
            control.MouseDown -= HandleSplitIndicator_MouseDown;
            control.MouseUp -= HandleSplitIndicator_MouseUp;
        }

        /// <summary>
        /// Returns <c>true</c> if the <see cref="SplitDocumentIndicator"/> is available at the
        /// current mouse position. 
        /// </summary>
        bool IsSplitIndicatorAtMousePosition()
        {
            Point mousePosition = PointToClient(MousePosition);

            return (Controls.Contains(_splitDocumentIndicator) &&
                _splitDocumentIndicator.Bounds.Contains(mousePosition));
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles mouse movement to determine if split indicator should be shown/hidden
        /// </summary>
        void HandleSplitIndicator_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var mouseLocation = PointToClient(Control.MousePosition);

                if (GetQualifiedSplitTarget(mouseLocation, 
                        out PageThumbnailControl targetPageControl,
                        out Point indicatorLocation))
                {
                    // If the qualified target page is different than the one the active
                    // splitter is targetting, first deactivate the active splitter.
                    if (_activeSplitTarget != null && _activeSplitTarget != targetPageControl)
                    {
                        DeactivateSplitIndicator();
                    }

                    // If ctrl/shift are down, don't display indicator as clicks are likely to be
                    // selection related.
                    if (!Controls.Contains(_splitDocumentIndicator)
                        && !_dragActive 
                        && Control.ModifierKeys == Keys.None)
                    {
                        _activeSplitTarget = targetPageControl;

                        ShowSplitIndicator(indicatorLocation, _activeSplitTarget.Height);
                    }
                }
                else
                {
                    DeactivateSplitIndicator();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50250");
            }
        }

        /// <summary>
        /// Handles mouse down event to activate the split indicator when clicked
        /// (Starts period of time mouse must be held to trigger the split)
        /// </summary>
        void HandleSplitIndicator_MouseDown(object sender, EventArgs e)
        {
            try
            {
                if (_activeSplitTarget != null)
                {
                    var mouseLocation = PointToClient(Control.MousePosition);
                    if (!GetQualifiedSplitTarget(mouseLocation, out var targetPageControl, out Point _)
                        || (targetPageControl != _activeSplitTarget))
                    {
                        DeactivateSplitIndicator();
                    }
                    // If ctrl/shift are down, don't activate indicator as clicks are likely to be
                    // selection related.
                    else if (!_splitDocumentIndicator.Activating 
                        && Control.MouseButtons == MouseButtons.Left
                        && !_dragActive
                        && Control.ModifierKeys == Keys.None)
                    {
                        // As the indicator is activating, clear the current page selection to ensure
                        // the activation animation is readily apparent.
                        ClearSelection();

                        // Necessary to properly render the portion of the pagination controls where
                        // the selection state has changed via the call above.
                        UpdateOverlappingControls();

                        _splitDocumentIndicator.Activate();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50261");
            }
        }


        /// <summary>
        /// Handles mouse up event to de-activate the splitter control when activation was
        /// initiated, but the mouse was not held long enough.
        /// </summary>
        void HandleSplitIndicator_MouseUp(object sender, EventArgs e)
        {
            try
            {
                DeactivateSplitIndicator();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50262");
            }
        }

        /// <summary>
        /// Handles case mouse has been held long enough to trigger the split to occur.
        /// </summary>
        void HandleSplitDocumentIndicator_ActivationComplete(object sender, EventArgs e)
        {
            using (new UIUpdateLock(this))
            {
                // Keep track of split target that will be lost via DeactivateSplitIndicator.
                var splitTarget = _activeSplitTarget;

                DeactivateSplitIndicator();

                // Index should be calculated only after removing the split indicator
                int index = _flowLayoutPanel.Controls.IndexOf(splitTarget);
                InitializePaginationControl(
                    new PaginationSeparator(this, CommitOnlySelection), ref index);

                // Ensure that the layout is performed when the load next document button
                // might get in the way of the normal layout logic.
                // https://extract.atlassian.net/browse/ISSUE-17396
                if (LoadNextDocumentVisible)
                {
                    ((PaginationLayoutEngine)_flowLayoutPanel.LayoutEngine).ForceNextLayout = true;
                }
            }
        }

        #endregion Event Handlers

        #region Helper Methods

        /// <summary>
        /// Determines whether a document split is qualified based on the specified
        /// <see paramref="mouseLocation"/> which should be:
        /// * Between two page thumbnail controls
        /// * Not to the left of the first page or right of the last page of any document
        /// * Not in a document that has already been processed (output)
        /// </summary>
        /// <param name="mouseLocation">The mouse location being tested.</param>
        /// <param name="targetPageControl">If the position is a valid location to initiate
        /// a document split, returns the <see cref="PageThumbnailControl"/> that would
        /// become the first page of the new document.</param>
        /// <param name="indicatorLocation">If the position is a valid location to initiate
        /// a document split, returns the position at which the split indicator should be
        /// placed.</param>
        /// <returns><c>true</c> if the position is a valid location to initiate a document
        /// split; <c>false</c> if it is not.</returns>
        bool GetQualifiedSplitTarget(Point mouseLocation, out PageThumbnailControl targetPageControl, out Point indicatorLocation)
        {
            indicatorLocation = new Point();
            targetPageControl = GetControlAtPoint<PageThumbnailControl>(mouseLocation);

            if (targetPageControl == null
                || !targetPageControl.Visible
                || targetPageControl.Document.OutputProcessed)
            {
                return false;
            }

            // Because a lot of padding may be added to extend a page control out to the end
            // of a row, take the padding into account when calculating where the split indicator
            // would be positioned.
            int left = targetPageControl.Left + targetPageControl.Padding.Left;
            int right = targetPageControl.Right - targetPageControl.Padding.Right;
            int center = (right - left) / 2;

            if ((mouseLocation.X - targetPageControl.Left) > center)
            {
                indicatorLocation = targetPageControl.TrailingInsertionPoint;
                targetPageControl = targetPageControl.NextControl as PageThumbnailControl;
            }
            else
            {
                indicatorLocation = targetPageControl.PreceedingInsertionPoint;
            }

            // Only if the mouse location is within half the width of _splitDocumentIndicator from
            // the left/right edge of a page control should this location qualify.
            if (targetPageControl?.PreviousControl is PageThumbnailControl
                && Math.Abs(mouseLocation.X - indicatorLocation.X) < _splitDocumentIndicator.Width / 2)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Shows the <see cref="_splitDocumentIndicator"/> at the specified
        /// <see paramref="location"/>.
        /// </summary>
        /// <param name="location">The <see cref="Point"/> where the location indicator should be
        /// drawn.</param>
        /// <param name="height">The height the location indicator should be</param>
        void ShowSplitIndicator(Point location, int height)
        {
            location.Offset(-_splitDocumentIndicator.Width / 2, 0);

            _splitDocumentIndicator.Location = location;
            _splitDocumentIndicator.Height = height;

            Controls.Add(_splitDocumentIndicator);
            _splitDocumentIndicator.BringToFront();
        }

        /// <summary>
        /// Updates any pagination controls overlapping _splitDocumentIndicator in isolation to
        /// prevent stale control states from being rendered behind the indicator.
        /// </summary>
        void UpdateOverlappingControls()
        {
            // Because the indicator control is in front of other pagination controls, the pagination
            // controls will not be rendered correctly while _splitDocumentIndicator is visible.
            _splitDocumentIndicator.Visible = false;

            foreach (Control control in _flowLayoutPanel.Controls
                .OfType<Control>()
                .Where(c => _splitDocumentIndicator.Bounds.IntersectsWith(c.Bounds)))
            {
                control.Update();
            }

            // Now that the overlapping controls are updated, re-display the indicator.
            _splitDocumentIndicator.Visible = true;
        }

        /// <summary>
        /// Cancel any activation of _splitDocumentIndicator that has started and removes the control.
        /// </summary>
        void DeactivateSplitIndicator()
        {
            Controls.Remove(_splitDocumentIndicator);
            _splitDocumentIndicator.Deactivate();
            _activeSplitTarget = null;
        }

        /// <summary>
        /// Disposes of resources related to the split document indicator
        /// </summary>
        void SplitDocument_Dispose()
        {
            if (_splitDocumentIndicator != null)
            {
                _splitDocumentIndicator.Dispose();
                _splitDocumentIndicator = null;
            }
        }

        #endregion Helper Methods
    }
}