using Extract.Drawing;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A glyph that indicates a new document that will be created as a result of pagination
    /// (whether the pagination was manual or suggested)
    /// </summary>
    public partial class NewDocumentGlyph : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewDocumentGlyph"/> class.
        /// </summary>
        public NewDocumentGlyph()
        {
            try
            {
                InitializeComponent();

                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40188");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                var brush = ExtractBrushes.GetSolidBrush(Color.Chartreuse);

                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                Rectangle drawRectangle = ClientRectangle;
                drawRectangle.Offset(0, 1);
                drawRectangle.Inflate(0, 1);
                e.Graphics.DrawString("+", Font, brush, drawRectangle, format);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40189");
            }
        }
    }
}
