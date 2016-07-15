using Extract.Drawing;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A glyph that indicates document pagination has been manually changed.
    /// </summary>
    public partial class EditedPaginationGlyph : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditedPaginationGlyph"/> class.
        /// </summary>
        public EditedPaginationGlyph()
        {
            try
            {
                InitializeComponent();

                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40186");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                var brush = ExtractBrushes.GetSolidBrush(Color.Red);

                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                // Hack fix-- The asterisk seems to insist to draw to high (50% above the top of
                // ClientRectangle).
                Rectangle drawRectangle = ClientRectangle;
                drawRectangle.Offset(1, 11);
                drawRectangle.Inflate(1, 11);
                e.Graphics.DrawString("*", Font, brush, drawRectangle, format);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40187");
            }
        }
    }
}
