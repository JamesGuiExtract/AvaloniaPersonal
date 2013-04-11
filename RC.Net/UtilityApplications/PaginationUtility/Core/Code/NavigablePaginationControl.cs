using System;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="PaginationControl"/> that can be traversed with keyboard navigation controls
    /// and that will dynamically update its padding to allow room for a preceding
    /// <see cref="PaginationSeparator"/> to be inserted without having to be moved.
    /// </summary>
    internal partial class NavigablePaginationControl : PaginationControl
    {
        #region Fields

        /// <summary>
        /// Indicates whether this control is currently visible in the PageLayoutControl; layout
        /// calls can be postponed for controls that are not currently visible to improve
        /// performance when a lot of pages are loaded into the UI.
        /// </summary>
        bool _isVisible;

        /// <summary>
        /// Indicates whether there is a postponed layout call that should be made when the control
        /// again becomes visible.
        /// </summary>
        bool _pendingLayout;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigablePaginationControl"/> class.
        /// </summary>
        public NavigablePaginationControl()
            : base()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35620");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is highlighted.
        /// </summary>
        /// <value><see langword="true"/> if this instance is highlighted; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public virtual bool Highlighted
        {
            get;
            set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Layout"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.LayoutEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            try
            {
                // To improve performance, when a lot of pages are loaded, skip any layout calls for
                // controls that aren't currently visible. 
                if (_isVisible)
                {
                    _pendingLayout = false;

                    base.OnLayout(e);
                }
                else
                {
                    // This schedules a layout to occur once the control becomes visible.
                    _pendingLayout = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35479");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (e.ClipRectangle.IsEmpty)
            {
                // If ClipRectangle is empty, this control is not currently visible in the
                // PageLayoutControl. 
                _isVisible = false;
            }
            else
            {
                _isVisible = true;

                // If one or more layout calls were skipped while the control was not visible,
                // perform a layout now.
                if (_pendingLayout)
                {
                    PerformLayout();
                }
            }

            base.OnPaint(e);
        }

        #endregion Overrides
    }
}
