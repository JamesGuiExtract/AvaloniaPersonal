using Extract.Utilities.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using Extract.Imaging.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="PaginationControl"/> that can be traversed with keyboard navigation controls
    /// and that will dynamically update its padding to allow room for a preceding
    /// <see cref="PaginationSeparator"/> to be inserted without having to be moved.
    /// </summary>
    internal partial class NavigablePaginationControl : PaginationControl
    {
        #region Constants

        /// <summary>
        /// The padding that should be used for an instance that is preceeded by a separator control.
        /// </summary>
        static readonly Padding _NORMAL_PADDING = new Padding(0, 1, 0, 1);

        /// <summary>
        /// The padding that should be used for an instance that is not preceeded by a separator
        /// control such that one can be added later without shifting the position of this control.
        /// </summary>
        static readonly Padding _SEPARATOR_ALLOWANCE_PADDING =
            new Padding(PaginationSeparator._SEPARATOR_WIDTH, 1, 0, 1);

        #endregion Constants

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

                Padding = _NORMAL_PADDING;
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
        /// <value><see langword="true"/> if this instance is hilighted; otherwise,
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
        /// Raises the <see cref="E:System.Windows.Forms.Control.LocationChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLocationChanged(EventArgs e)
        {
            try
            {
                base.OnLocationChanged(e);

                CheckPadding();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35478");
            }
        }

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

                    CheckPadding();
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

        #region Private Members

        /// <summary>
        /// Ensures that the padding is such that it will cause all
        /// <see cref="PageThumbnailControl"/>s in the <see cref="PageLayoutControl"/> to align
        /// vertically whether or not they are preceeded by a <see cref="PaginationSeparator"/>
        /// control.
        /// </summary>
        void CheckPadding()
        {
            // Don't bother checking padding until this has actually been added to a layout panel.
            if (Parent != null)
            {
                bool hasPaddingAllowance = Padding.Equals(_SEPARATOR_ALLOWANCE_PADDING);
                bool shouldHavePaddingAllowance =
                    PreviousControl == null ||
                    PreviousControl.Left > Left ||
                    PreviousControl is PageThumbnailControl;

                if (hasPaddingAllowance != shouldHavePaddingAllowance)
                {
                    try
                    {
                        // For performance reasons, don't allow padding and size changes to
                        // immediately trigger layout changes (which can cause a lot of
                        // recursive layout calls).
                        SuspendLayout();

                        Padding oldPadding = Padding;

                        Padding = shouldHavePaddingAllowance
                            ? _SEPARATOR_ALLOWANCE_PADDING
                            : _NORMAL_PADDING;

                        // After setting new padding, immediately adjust the overall size of the
                        // control to match the change in padding size so that controls can be
                        // correctly positioned without layout having been called.
                        Size sizeDifference = Padding.Size - oldPadding.Size;
                        Size += sizeDifference;
                    }
                    finally
                    {
                        // Once the padding and size have changed, a layout needs to occur, but not
                        // as part of this event, otherwise the control position may not correctly
                        // reflect the new padding/size. Either invoke the layout the message queue
                        // or schedule it to occur with the control becomes visible.
                        _pendingLayout = true;
                        ResumeLayout(false);

                        if (_isVisible)
                        {
                            this.SafeBeginInvoke("ELI35610", () =>
                            {
                                // Check to be sure a layout wasn't performed between the time the
                                // call was invoked and now.
                                if (_pendingLayout)
                                {
                                    PerformLayout();
                                }
                            });
                        }
                    }
                }
            }
        }

        #endregion Private Members
    }
}
