using Extract.Drawing;
using System;
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

                // The background should be transparent so it appears the indicator is drawn on top
                // of the underlying controls.
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
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

                int x = (Width / 2);
                e.Graphics.DrawLine(ExtractPens.DashedBlack, x, 0, x, Height);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35420");
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

        #endregion Overrides
    }
}
