using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="LayoutEngine"/> that manages the layout of <see cref="PaginationControl"/>s.
    /// </summary>
    internal class PaginationLayoutEngine : LayoutEngine
    {
        #region Events

        /// <summary>
        /// Raised when a Layout operation has completed.
        /// </summary>
        public event EventHandler<LayoutCompletedEventArgs> LayoutCompleted;

        #endregion Events

        #region Fields

        /// <summary>
        /// Recursion protection for UpdateScrollPosition.
        /// </summary>
        bool _updatingScrollPosition;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the next layout should be required.
        /// (optimization related).
        /// </summary>
        /// <value><c>true</c> if the next layout should be forced to execute; <c>false</c> if
        /// there is no known reason the layout event can't be skipped for optimization purposes.
        /// </value>
        public bool ForceNextLayout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="ScrollTarget"/> describing a scroll operation to be executed.
        /// </summary>
        public ScrollTarget ScrollTarget
        {
            get;
            set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Requests that the layout engine perform a layout operation.
        /// <para><b>Note</b></para>
        /// This method is based on the MSDN example here:
        /// http://msdn.microsoft.com/en-us/library/system.windows.forms.layout.layoutengine(v=vs.100).aspx
        /// </summary>
        /// <param name="container">The container on which the layout engine will operate.</param>
        /// <param name="layoutEventArgs">An event argument from a
        /// <see cref="E:System.Windows.Forms.Control.Layout"/> event.</param>
        /// <returns><see langword="true"/> if layout should be performed again by the parent of
        /// <paramref name="container"/>; otherwise, <see langword="false"/>.</returns>
        public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
        {
            try
            {
                if (container is Control parent)
                {
                    if (parent.Handle == null)
                    {
                        return false;
                    }

                    DoLayout(parent, layoutEventArgs, out List<PaginationControl> redundantControls);
                    OnLayoutCompleted(redundantControls.ToArray());

                    // Manually update separators that have pending status changes.
                    foreach (var separator in parent.Controls.OfType<PaginationSeparator>()
                        .Where(separator => separator.InvalidatePending))
                    {
                        separator.Invalidate();
                        separator.InvalidatePending = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35656");
            }

            return false;
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Does the layout.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="layoutEventArgs">redundant controls that may be removed as a result of the layout.</param>
        void DoLayout(Control parent, LayoutEventArgs layoutEventArgs, out List<PaginationControl> redundantControls)
        {
            // Use DisplayRectangle so that parent.Padding is honored.
            Rectangle parentDisplayRectangle = parent.DisplayRectangle;
            // But use ClientRectangle width so that the vertical scrollbar is accounted for
            // (when visible).
            parentDisplayRectangle.Width = parent.ClientRectangle.Width;
            Point nextControlLocation = parentDisplayRectangle.Location;

            redundantControls = new List<PaginationControl>();
            PaginationSeparator lastSeparator = null;

            // If the affected control is not currently visible or is offscreen below the current
            // visible ClientRectangle, abort the layout to avoid unnecessary repeated layouts as
            // a large number of documents/pages are loading.
            if (!ForceNextLayout &&
                ScrollTarget?.Control == null &&
                layoutEventArgs?.AffectedControl != null &&
                layoutEventArgs?.AffectedControl != parent &&
                !layoutEventArgs.AffectedProperty.Equals("Visible") &&
                (parent.ClientRectangle.Bottom < layoutEventArgs.AffectedControl.Bounds.Top))
            {
                return;
            }

            ForceNextLayout = false;

            bool foundFirstVisible = false;

            // Layout all PaginationControls (ignore any other kind of control).
            foreach (PaginationControl control in parent.Controls.OfType<PaginationControl>())
            {
                var separator = control as PaginationSeparator;

                // Don't allow a hidden control to qualify as the previousControl
                PaginationControl previousControl = foundFirstVisible
                    ? control.PreviousControl as PaginationControl
                    : null;

                var previousSeparator = previousControl as PaginationSeparator;

                // Only apply layout to visible controls.
                if (!control.Visible)
                {
                    continue;
                }

                foundFirstVisible = true;

                ExtractException.Assert("ELI35655",
                    "The PaginationLayoutEngine does not respect margins.",
                    control.Margin == Padding.Empty);

                if (separator != null && previousSeparator != null)
                {
                    // If this separator is preceded by another separator, it is redundant and
                    // should be ignored.
                    redundantControls.Add(separator);
                    separator.Visible = false;

                    // Separator is being removed, previousSeparator is will be associated with a
                    // new document. Ensure this new association is reflected.
                    previousSeparator.InvalidatePending = true;

                    continue;
                }

                bool newRow = false;
                PageThumbnailControl previousPageControl = previousControl as PageThumbnailControl;

                if (separator != null)
                {
                    if (previousControl != null)
                    {
                        newRow = true;

                        nextControlLocation.X = parentDisplayRectangle.Left;

                        if (previousPageControl != null)
                        {
                            if (previousPageControl.Document.Collapsed)
                            {
                                if (lastSeparator != null)
                                {
                                    nextControlLocation.Y += lastSeparator.Height - 1;
                                }
                            }
                            else
                            {
                                nextControlLocation.Y += PageThumbnailControl.UniformSize.Height;
                            }
                        }
                    }

                    lastSeparator = separator;
                }
                else
                {
                    // Calculate the X position the next control would be at.
                    int nextXPosition =
                        nextControlLocation.X + PageThumbnailControl.UniformSize.Width;

                    // If the next control would start beyond parentDisplayRectangle, wrap.
                    if (nextXPosition > parentDisplayRectangle.Right)
                    {
                        newRow = true;
                        nextControlLocation.X = parentDisplayRectangle.Left;
                        nextControlLocation.Y += (previousSeparator != null)
                            ? previousSeparator.Height
                            : PageThumbnailControl.UniformSize.Height;
                    }
                }

                // Resize the last page control in a row so that it extends all the way to the right
                // edge of the panel. This allows for the (drag)drops to occur anywhere to the right
                // of the last page in each row.
                if (newRow && previousPageControl != null)
                {
                    var newPadding = parentDisplayRectangle.Right - previousPageControl.Right;
                    previousPageControl.Width = parentDisplayRectangle.Right - previousPageControl.Left;
                    var padding = previousPageControl.Padding;
                    padding.Right = newPadding;
                    previousPageControl.Padding = padding;
                }

                // Size the control properly.
                if (separator != null)
                {
                    control.Size = new Size(parentDisplayRectangle.Width, control.Height);
                }
                else
                {
                    control.Padding = ((NavigablePaginationControl)control).NormalPadding;
                    control.Size = PageThumbnailControl.UniformSize;
                }

                control.Location = nextControlLocation;

                nextControlLocation.X += control.Width;
            }

            // Resize the last page control in a row so that it extends all the way to the right
            // edge of the panel. This allows for the (drag)drops to occur anywhere to the right
            // of the last page in each row.
            var lastPageControl = parent.Controls
                .OfType<PaginationControl>()
                .LastOrDefault()
                as PageThumbnailControl;
            if (lastPageControl != null)
            {
                var newPadding = parentDisplayRectangle.Right - lastPageControl.Right;
                lastPageControl.Width = parentDisplayRectangle.Right - lastPageControl.Left;
                var padding = lastPageControl.Padding;
                padding.Right = newPadding;
                lastPageControl.Padding = padding;
            }

            // Prevent unneeded separator when load next document button isn't available.
            var lastControl = parent.Controls
                .OfType<PaginationControl>()
                .LastOrDefault();
            if (lastControl != null && lastControl is PaginationSeparator)
            {
                redundantControls.Add(lastControl);
            }

            UpdateScrollPosition((FlowLayoutPanel)parent);
        }

        /// <summary>
        /// Updates the scroll position per SnapToControl.
        /// </summary>
        /// <param name="flowLayoutPanel">The <see cref="FlowLayoutPanel"/> for which the layout is occurring.</param>
        void UpdateScrollPosition(FlowLayoutPanel flowLayoutPanel)
        {
            if (_updatingScrollPosition || ScrollTarget == null)
            {
                return;
            }

            try
            {
                _updatingScrollPosition = true;

                var newScrollPos = flowLayoutPanel.VerticalScroll.Value + ScrollTarget.Control.Top - ScrollTarget.TopAlignmentOffset ?? 0;
                newScrollPos = Math.Max(newScrollPos, flowLayoutPanel.VerticalScroll.Minimum);
                newScrollPos = Math.Min(newScrollPos, flowLayoutPanel.VerticalScroll.Maximum);
                if (newScrollPos != flowLayoutPanel.VerticalScroll.Value)
                {
                    flowLayoutPanel.VerticalScroll.Value = newScrollPos;
                    flowLayoutPanel.PerformLayout();
                }
                ScrollTarget = null;
            }
            finally
            {
                _updatingScrollPosition = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="LayoutCompleted"/> event.
        /// </summary>
        /// <param name="redundantControls">The redundant <see cref="PaginationControl"/>s.</param>
        void OnLayoutCompleted(PaginationControl[] redundantControls)
        {
            LayoutCompleted?.Invoke(this, new LayoutCompletedEventArgs(redundantControls));
        }

        #endregion Private Members
    }
}
