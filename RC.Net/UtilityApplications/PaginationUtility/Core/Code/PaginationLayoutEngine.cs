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

                    bool useExtraPadding = false;
                    var separator = control as PaginationSeparator;

                    PaginationControl previousSeparator =
                        control.PreviousControl as PaginationSeparator;
                    if (separator == null)
                    {
                        // If not a separator, if necessary, this control should allow extra space
                        // in front of it so that a separator can be added without changing the
                        // position of this control.
                        if (previousSeparator == null || firstControl)
                        {
                            useExtraPadding = true;
                        }

                        firstControl = false;
                    }
                    else if (firstControl || previousSeparator != null)
                    {
                        // If this separator is preceeded by another separator or is the first
                        // control, it is redundant and should be ignored.
                        redundantControls.Add(separator);
                        separator.Visible = false;
                        firstControl = false;
                        continue;
                    }

                    // Calculate the X position the next control would be at.
                    int nextXPosition = nextControlLocation.X;
                    if (separator != null)
                    {
                        // For visual consistency, make sure the last control in a row is never a
                        // separator. If this control is a separator and a following page control
                        // would wrap, wrap this separator instead.
                        nextXPosition += PaginationSeparator.UniformSize.Width +
                            PageThumbnailControl.UniformSize.Width;
                    }
                    else
                    {
                        nextXPosition += PageThumbnailControl.UniformSize.Width;
                        if (useExtraPadding)
                        {
                            // Allow for padding that will be added below.
                            nextXPosition += PaginationSeparator.UniformSize.Width;
                        }
                    }

                    // If the next control would start beyond parentDisplayRectangle, wrap.
                    if (nextXPosition > parentDisplayRectangle.Right)
                    {
                        nextControlLocation.X = parentDisplayRectangle.Left;

                        if (separator == null)
                        {
                            // Allow space for a separator to be added at the start of the row.
                            useExtraPadding = true;
                        }

                        nextControlLocation.Y += PageThumbnailControl.UniformSize.Height;
                    }

                    // Size the control properly.
                    if (separator != null)
                    {
                        control.Size = PaginationSeparator.UniformSize;
                    }
                    else
                    {
                        Padding normalPadding = ((NavigablePaginationControl)control).NormalPadding;

                        if (useExtraPadding)
                        {
                            control.Padding = new Padding(
                                normalPadding.Left + PaginationSeparator.UniformSize.Width,
                                normalPadding.Top, normalPadding.Right, normalPadding.Bottom);
                            control.Size = new Size(
                                PageThumbnailControl.UniformSize.Width + PaginationSeparator.UniformSize.Width,
                                PageThumbnailControl.UniformSize.Height);
                        }
                        else
                        {
                            control.Padding = normalPadding;
                            control.Size = PageThumbnailControl.UniformSize;
                        }
                    }

                    control.Location = nextControlLocation;

                    nextControlLocation.X += control.Width;
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
