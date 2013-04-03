using Extract.Drawing;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// This control is used to indicate where <see cref="PaginationControl"/>s will be dropped
    /// during a drag-and-drop operation.
    /// </summary>
    internal partial class DropLocationIndicator : UserControl
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DropLocationIndicator"/> class.
        /// </summary>
        public DropLocationIndicator()
        {
            try
            {
                InitializeComponent();

                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35419");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                // Draw a dashed vertical line down the center.
                int centerX = (Width / 2);
                e.Graphics.DrawLine(ExtractPens.DashedBlack, centerX, 0, centerX, Height);

                // Draw a triangle at the top and bottom (pointing toward the center).
                e.Graphics.DrawImage(Properties.Resources.DownArrow, ClientRectangle.Location);
                e.Graphics.DrawImage(Properties.Resources.UpArrow,
                    new Point(ClientRectangle.Left, ClientRectangle.Bottom - Properties.Resources.UpArrow.Height));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35420");
            }
        }

        /// <summary>
        /// Processes windows messages.
        /// </summary>
        /// <param name="m">The Windows <see cref="Message"/> to process.</param>
        protected override void WndProc(ref Message m)
        {
            try
            {
                // This code makes this control "transparent" to mouse events so that the
                // PageLayoutControl it is hosted in receives any mouse events instead.
                const int WM_NCHITTEST = 0x0084;
                const int HTTRANSPARENT = -1;

                if (m.Msg == WM_NCHITTEST)
                {
                    m.Result = (IntPtr)HTTRANSPARENT;
                }
                else
                {
                    base.WndProc(ref m);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35421");
            }
        }

        /// <summary>
        /// Gets the required creation parameters when the control handle is created.
        /// Overridden in order to make the control transparent.
        /// </summary>
        /// <returns>A <see cref="T:System.Windows.Forms.CreateParams"/> that contains the required
        /// creation parameters when the handle to the control is created.</returns>
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_TRANSPARENT = 0x0084;

                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= WS_EX_TRANSPARENT;
                return createParams;
            }
        }

        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Do not paint background so that any controls under this one show through.
        }

        #endregion Overrides
    }
}
