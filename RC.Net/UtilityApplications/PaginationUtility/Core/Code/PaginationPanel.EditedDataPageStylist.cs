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
            /// Gets whether the stylist is visible on the specified <see paramref="pageControl"/>.
            /// </summary>
            /// <param name="pageControl">The <see cref="PageThumbnailControl"/> for which visibility
            /// should be checked.</param>
            /// <returns><see langword="true"/> if visible for the <see paramref="pageControl"/>.
            /// </returns>
            protected override bool IsVisibleFor(PageThumbnailControl pageControl)
            {
                var outputDocument = (ExtendedOutputDocument)pageControl.Document;

                return outputDocument.DocumentData != null &&
                    outputDocument.DocumentData.Modified;
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
            /// Allows foreground of the specified <see paramref="pageControl"/> to be painted.
            /// </summary>
            /// <param name="pageControl">The <see cref="PageThumbnailControl"/> to paint.</param>
            /// <param name="paintingControl">The specific <see cref="Control"/> within
            /// <see paramref="pageControl"/>being painted.</param>
            /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
            /// <see paramref="pageControl"/>.</param>
            public override void PaintForeground(PageThumbnailControl pageControl,
                Control paintingControl, PaintEventArgs e)
            {
                InterpolationMode savedMode = e.Graphics.InterpolationMode;

                try
                {
                    if (IsVisibleFor(pageControl))
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
