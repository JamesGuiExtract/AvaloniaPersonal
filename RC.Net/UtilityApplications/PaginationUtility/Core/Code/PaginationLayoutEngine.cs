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
        /// Raised when redundant and unnecessary <see cref="PaginationControl"/>s are found during
        /// a <see cref="Layout"/> call.
        /// </summary>
        public event EventHandler<RedundantControlsFoundEventArgs> RedundantControlsFound;

        #endregion Events

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
                Control parent = container as Control;

                // Use DisplayRectangle so that parent.Padding is honored.
                Rectangle parentDisplayRectangle = parent.DisplayRectangle;
                // But use ClientRectangle width so that the vertical scrollbar is accounted for
                // (when visible).
                parentDisplayRectangle.Width = parent.ClientRectangle.Width;
                Point nextControlLocation = parentDisplayRectangle.Location;

                List<PaginationControl> redundantControls = new List<PaginationControl>();
                bool firstControl = true;

                // Layout all PaginationControls (ignore any other kind of control).
                foreach (PaginationControl control in parent.Controls.OfType<PaginationControl>())
                {
                    // Only apply layout to visible controls.
                    if (!control.Visible)
                    {
                        continue;
                    }

                    ExtractException.Assert("ELI35655",
                        "The PaginationLayoutEngine does not respect margins.",
                        control.Margin == Padding.Empty);

                    var separator = control as PaginationSeparator;

                    PaginationControl previousControl = control.PreviousControl as PaginationControl;
                    PaginationControl previousSeparator = previousControl as PaginationSeparator;
                    if (separator == null)
                    {
                        firstControl = false;
                    }
                    else if ((firstControl && parent.Controls.OfType<PageThumbnailControl>().Any())
                        || previousSeparator != null)
                    {
                        // If this separator is preceded by another separator or is the first
                        // control (but not the only control), it is redundant and should be ignored.
                        redundantControls.Add(separator);
                        separator.Visible = false;
                        firstControl = false;
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
                        }

                        nextControlLocation.X = parentDisplayRectangle.Left;
                        nextControlLocation.Y += PageThumbnailControl.UniformSize.Height;
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
                        control.Size = new Size(parentDisplayRectangle.Width, PaginationSeparator.UniformSize.Height);
                    }
                    else
                    {
                        control.Padding = ((NavigablePaginationControl)control).NormalPadding;
                        control.Size = PageThumbnailControl.UniformSize;
                    }

                    control.Location = nextControlLocation;

                    nextControlLocation.X += control.Width;
                    control.Invalidate();
                }

                OnRedundantControlsFound(redundantControls.ToArray());
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
        /// Raises the <see cref="RedundantControlsFound"/> event.
        /// </summary>
        /// <param name="redundantControls">The redundant <see cref="PaginationControl"/>s.</param>
        void OnRedundantControlsFound(PaginationControl[] redundantControls)
        {
            var eventHandler = RedundantControlsFound;
            if (eventHandler != null)
            {
                eventHandler(this, new RedundantControlsFoundEventArgs(redundantControls));
            }
        }

        #endregion Private Members
    }
}
