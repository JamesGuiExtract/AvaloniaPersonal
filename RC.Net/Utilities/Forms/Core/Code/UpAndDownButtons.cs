using Extract.Utilities.Forms.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// A button that contains an up arrow icon.
    /// </summary>
    public partial class ExtractUpButton : Button
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractUpButton"/> class.
        /// </summary>
        public ExtractUpButton()
            : base()
        {
            try
            {
                Size = new Size(35, 35);
                Image = Resources.icon_up.ToBitmap();
                Text = "";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31783");
            }
        }
    }

    /// <summary>
    /// A button that contains a down arrow icon.
    /// </summary>
    public partial class ExtractDownButton : Button
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractDownButton"/> class.
        /// </summary>
        public ExtractDownButton()
            : base()
        {
            try
            {
                Size = new Size(35, 35);
                Image = Resources.icon_down.ToBitmap();
                Text = "";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31784");
            }
        }
    }
}
