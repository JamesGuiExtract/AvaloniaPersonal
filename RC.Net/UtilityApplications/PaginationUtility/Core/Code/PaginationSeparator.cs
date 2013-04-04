using System;
using System.Drawing;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="PaginationControl"/> that indicates the divider between the end of one document
    /// and the start of another.
    /// </summary>
    internal partial class PaginationSeparator : PaginationControl
    {
        #region Constants

        /// <summary>
        /// The width of the control. (PageThumbnailControl relies on this being a consistent value).
        /// </summary>
        internal static readonly int _SEPARATOR_WIDTH = 11;

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSeparator"/> class.
        /// </summary>
        public PaginationSeparator()
            : base()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35496");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Gets or sets whether this control is selected.
        /// </summary>
        /// <value><see langword="true"/> if selected; otherwise, <see langword="false"/>.
        /// </value>
        public override bool Selected
        {
            get
            {
                return base.Selected;
            }

            set
            {
                if (value != base.Selected)
                {
                    base.Selected = value;

                    // Indicate selection with the BackColor
                    BackColor = value
                        ? SystemColors.ControlDark
                        : SystemColors.Control;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.SizeChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnSizeChanged(EventArgs e)
        {
            try
            {
                base.OnSizeChanged(e);

                // Ensure that the width never changes.
                if (Width != _SEPARATOR_WIDTH)
                {
                    Width = _SEPARATOR_WIDTH;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35497");
            }
        }

        #endregion Overrides
    }
}
