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
            /// Allows foreground of the specified <see paramref="pageControl"/> to be painted.
            /// </summary>
            /// <param name="pageControl">The <see cref="PageThumbnailControl"/> to paint.</param>
            /// <param name="e">The <see cref="PaintEventArgs"/> being used for the painting of the
            /// <see paramref="pageControl"/>.</param>
            public override void PaintForeground(PageThumbnailControl pageControl, PaintEventArgs e)
            {
                InterpolationMode savedMode = e.Graphics.InterpolationMode;

                try
                {
                    var outputDocument = (ExtendedOutputDocument)pageControl.Document;

                    if (outputDocument.DocumentData != null && outputDocument.DocumentData.Modified)
                    {
                        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        // Positioned to allow for NewOutputPageStylist and ModifiedPageStylist to
                        // appear above this one.
                        Rectangle drawRectangle = new Rectangle(
                            e.ClipRectangle.Right - 24,
                            40,
                            16,
                            16);

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
