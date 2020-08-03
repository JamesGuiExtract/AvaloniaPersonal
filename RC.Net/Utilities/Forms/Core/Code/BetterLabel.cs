using System;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// A <see cref="Label"/> that doesn't copy its text to the clipboard when double-clicked.
    /// https://extract.atlassian.net/browse/ISSUE-17140
    /// </summary>
    public class BetterLabel : Label
    {
        const int WM_LBUTTONDBLCLK = 0x203;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDBLCLK)
            {
                OnDoubleClick(EventArgs.Empty);
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}
