using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    partial class PaginationPanel
    {
        /// <summary>
        /// A <see cref="PageStylist"/> that indicates document data has been modified via a pencil
        /// glyph.
        /// </summary>
        internal class EditedDocumentPageStylist : PageStylist
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EditedDocumentPageStylist"/> class.
            /// </summary>
            /// <param name="pageControl">The <see cref="PageThumbnailControl"/> for which this stylist
            /// is responsible.</param>
            public EditedDocumentPageStylist(PageThumbnailControl pageControl)
                : base(pageControl)
            {
            }

            /// <summary>
            /// Gets the tooltip text to display for this stylist.
            /// </summary>
            protected override string ToolTipText
            {
                get
                {
                    return "The data for this document has been modified.";
                }
            }

            /// <summary>
            /// Gets whether the stylist is visible.
            /// </summary>
            /// <returns><see langword="true"/> if visible.</returns>
            protected override bool IsVisible
            {
                get
                {
                    var outputDocument = (ExtendedOutputDocument)PageControl.Document;

                    return outputDocument.DocumentData != null &&
                        outputDocument.DocumentData.Modified;
                }
            }

            /// <summary>
            /// Gets the area of <see paramref="hostingControl"/> to which the stylist will do any
            /// foreground drawing.
            /// </summary>
            /// <param name="hostingControl">The <see cref="Control"/> on which the stylist will do any
            /// foreground drawing.</param>
            /// <returns>A <see cref="Rectangle"/> describing the area to which the stylist will draw.
            /// </returns>
            protected override Rectangle GetDrawRectangle(Control hostingControl)
            {
                // 3nd icon to top
                Size size = new Size(16, 16);
                Point location = new Point(hostingControl.ClientRectangle.Right - size.Width - 4, size.Height * 2 + 4);
                return new Rectangle(location, size);
            }

            /// <summary>
            /// Allows foreground of the PageControl to be painted.
            /// </summary>
            /// <param name="paintingControl">The specific <see cref="Control"/> within
            /// PageControl being painted.</param>
            /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
            /// PageControl.</param>
            public override void PaintForeground(Control paintingControl, PaintEventArgs e)
            {
                InterpolationMode savedMode = e.Graphics.InterpolationMode;

                try
                {
                    if (IsVisible)
                    {
                        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        // Positioned to allow for NewOutputPageStylist and ModifiedPageStylist to
                        // appear above this one.
                        Rectangle drawRectangle = GetDrawRectangle(paintingControl);

                        e.Graphics.DrawImage(Properties.Resources.Edit, drawRectangle);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39737");
                }
                finally
                {
                    e.Graphics.InterpolationMode = savedMode;
                }
            }
        }
    }
}
