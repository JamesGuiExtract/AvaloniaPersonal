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
    internal class PaginationLayoutEngine : LayoutEngine, IDisposable
    {
        #region Events

        /// <summary>
        /// Raised when a Layout operation has completed.
        /// </summary>
        public event EventHandler<LayoutCompletedEventArgs> LayoutCompleted;

        #endregion Events

        #region Fields

        /// <summary>
        /// Indicates when a layout operation has been invoked. (Layout operations are delayed to be
        /// able to perform one layout rather than many as individual control properties change).
        /// </summary>
        bool _layoutInvoked;

        /// <summary>
        /// In order to allow SnapToControl to be able to snap all the way to the top, extra space
        /// may need to be added to the bottom so that there is enough scrolling available to get the
        /// control to the top.
        /// </summary>
        Panel _bottomMarginControl = null;

        /// <summary>
        /// Recursion protection for UpdateBottomMargin.
        /// </summary>
        bool _updatingMargin;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets a value indicating whether a layout operation is pending.
        /// </summary>
        public bool LayoutPending
        {
            get
            {
                return _layoutInvoked;
            }
        }

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
        /// Gets or sets the control which should be snapped to the top of the pane (<c>null</c> if
        /// no control should be snapped to the top.
        /// </summary>
        public Control SnapToControl
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
                // Delay all triggered layouts to occur as part of one layout invoked to occur on the
                // next windows message to avoid unnecessary layout work.
                if (_layoutInvoked)
                {
                    return false;
                }

                Control parent = container as Control;
                if (parent != null)
                {
                    if (parent.Handle == null)
                    {
                        return false;
                    }

                    parent.SafeBeginInvoke("ELI40230", () =>
                    {
                        // Locking control update here prevents flicker in document headers as the
                        // layout occurs.
                        using (new LockControlUpdates(parent, initiallyLock: true, invalidateOnUnlock: true))
                        {
                            DoLayout(parent, layoutEventArgs, out List<PaginationControl> redundantControls);

                            // Reset _layoutInvoked before OnLayoutCompleted, otherwise pending
                            // _scrollToControl actions will be missed.
                            _layoutInvoked = false;
                            OnLayoutCompleted(redundantControls.ToArray());

                            // Manually update separators that have pending status changes.
                            foreach (var separator in parent.Controls.OfType<PaginationSeparator>()
                                .Where(separator => separator.InvalidatePending))
                            {
                                separator.Invalidate();
                                separator.InvalidatePending = false;
                            }

                            parent.ResumeLayout();
                        }
                    },
                    true,
                    (e) =>
                        {
                            parent.ResumeLayout();
                            _layoutInvoked = false;
                        });

                    parent.SuspendLayout();
                    _layoutInvoked = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35656");
            }

            return false;
        }

        #endregion Overrides

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="PaginationLayoutEngine"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="PaginationLayoutEngine"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="PaginationLayoutEngine"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    // Dispose of managed resources
                    if (_bottomMarginControl != null)
                    {
                        _bottomMarginControl.Dispose();
                        _bottomMarginControl = null;
                    }
                }
                catch { }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

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
                SnapToControl == null &&
                layoutEventArgs?.AffectedControl != null &&
                layoutEventArgs?.AffectedControl != parent &&
                !layoutEventArgs.AffectedProperty.Equals("Visible") &&
                (parent.ClientRectangle.Bottom < layoutEventArgs.AffectedControl.Bounds.Top))
            {
                return;
            }

            ForceNextLayout = false;

            // Layout all PaginationControls (ignore any other kind of control).
            foreach (PaginationControl control in parent.Controls.OfType<PaginationControl>())
            {
                var separator = control as PaginationSeparator;
                var previousControl = control.PreviousControl as PaginationControl;
                var previousSeparator = previousControl as PaginationSeparator;

                // Only apply layout to visible controls.
                if (!control.Visible)
                {
                    continue;
                }

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

                if (separator != null)
                {
                    // Resize the last page control before each separator so that it extends all
                    // the way to the right edge of the panel. This allows for the drops to
                    // occur anywhere to the right of that page.
                    if (previousControl != null)
                    {
                        var newPadding = parentDisplayRectangle.Right - previousControl.Right;
                        previousControl.Width = parentDisplayRectangle.Right - previousControl.Left;
                        var padding = previousControl.Padding;
                        padding.Right = newPadding;
                        previousControl.Padding = padding;

                        nextControlLocation.X = parentDisplayRectangle.Left;

                        PageThumbnailControl previousPageControl = previousControl as PageThumbnailControl;
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
                        nextControlLocation.X = parentDisplayRectangle.Left;
                        nextControlLocation.Y += (previousSeparator != null)
                            ? previousSeparator.Height
                            : PageThumbnailControl.UniformSize.Height;
                    }
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

            // Prevent unneeded separator when load next document button isn't available.
            var lastControl = parent.Controls
                .OfType<PaginationControl>()
                .LastOrDefault();
            if (lastControl != null && lastControl is PaginationSeparator)
            {
                redundantControls.Add(lastControl);
            }

            UpdateBottomMargin((FlowLayoutPanel)parent, layoutEventArgs);
        }

        /// <summary>
        /// Updates the size and position of _bottomMarginControl based on SnapToControl and
        /// current control positions.
        /// </summary>
        /// <param name="flowLayoutPanel">The <see cref="FlowLayoutPanel"/> for which the layout is occurring.</param>
        /// <param name="layoutEventArgs">The <see cref="LayoutEventArgs"/> instance containing the event data.</param>
        void UpdateBottomMargin(FlowLayoutPanel flowLayoutPanel, LayoutEventArgs layoutEventArgs)
        {
            if (_updatingMargin)
            {
                return;
            }

            try
            {
                _updatingMargin = true;

                if (_bottomMarginControl?.Parent == flowLayoutPanel)
                {
                    if (flowLayoutPanel.Controls.IndexOf(_bottomMarginControl) != flowLayoutPanel.Controls.Count - 1)
                    {
                        flowLayoutPanel.Controls.SetChildIndex(_bottomMarginControl, flowLayoutPanel.Controls.Count - 1);
                    }
                }
                else
                {
                    if (_bottomMarginControl == null)
                    {
                        _bottomMarginControl = new Panel();
                        _bottomMarginControl.Margin = new Padding();
                    }

                    flowLayoutPanel.Controls.Add(_bottomMarginControl);
                }

                var lastVisibleControlBottom = flowLayoutPanel.Controls
                    .OfType<PaginationControl>()
                    .LastOrDefault(control => control.Visible)?.Bottom ?? flowLayoutPanel.Top;

                if (SnapToControl == null)
                {
                    if (_bottomMarginControl != null)
                    {
                        _bottomMarginControl.Location = new Point(0, lastVisibleControlBottom);
                        _bottomMarginControl.Width = flowLayoutPanel.Width - SystemInformation.VerticalScrollBarWidth;
                    }
                }
                else
                {
                    bool newlyVisible = !flowLayoutPanel.VerticalScroll.Visible;

                    var currentHeight = flowLayoutPanel.VerticalScroll.Visible 
                        ? flowLayoutPanel.VerticalScroll.Maximum
                        : flowLayoutPanel.Height;
                    var scrollPosition = flowLayoutPanel.VerticalScroll.Visible
                        ? flowLayoutPanel.VerticalScroll.Value
                        : 0;
                    var currentMargin = _bottomMarginControl?.Height ?? 0;

                    var bottomMargin = Math.Max(0,
                        SnapToControl.Top -
                            (currentHeight
                                - flowLayoutPanel.Height
                                - scrollPosition
                                - currentMargin));

                    if (bottomMargin != _bottomMarginControl.Height)
                    {
                        _bottomMarginControl.Width = flowLayoutPanel.Width - SystemInformation.VerticalScrollBarWidth;
                        _bottomMarginControl.Height = bottomMargin;
                        _bottomMarginControl.Location = new Point(0, lastVisibleControlBottom);
                    }

                    if (bottomMargin > 0 && newlyVisible)
                    {
                        DoLayout(flowLayoutPanel, layoutEventArgs, out List<PaginationControl> _);
                    }
                    else
                    {
                        flowLayoutPanel.VerticalScroll.Value =
                            flowLayoutPanel.VerticalScroll.Value + SnapToControl.Top;

                        SnapToControl = null;
                    }
                }
            }
            finally
            {
                _updatingMargin = false;
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
