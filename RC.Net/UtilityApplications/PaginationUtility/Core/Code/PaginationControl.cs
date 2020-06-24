using System;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A control that can be positioned within a <see cref="PageLayoutControl"/> in order to
    /// compose <see cref="OutputDocument"/>s.
    /// </summary>
    internal partial class PaginationControl : UserControl
    {
        #region Fields

        /// <summary>
        /// Indicates whether this control is in the process of raising an event passed on from a
        /// child control.
        /// </summary>
        bool _raisingEvent;

        /// <summary>
        /// Indicates whether there is a postponed layout call that should be made when the control
        /// again becomes visible.
        /// </summary>
        bool _pendingLayout;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationControl"/> class.
        /// </summary>
        public PaginationControl()
            : base()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35485");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PaginationControl"/> is
        /// selected.
        /// </summary>
        /// <value><see langword="true"/> if selected; otherwise, <see langword="false"/>.</value>
        public virtual bool Selected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the point at which an insertion indicator should be drawn for controls to
        /// be dropped in front of this control.
        /// </summary>
        /// <value>
        /// The point at which an insertion indicator should be drawn for controls to be dropped in
        /// front of this control.
        /// </value>
        public virtual Point PreceedingInsertionPoint
        {
            get
            {
                return new Point(Left + (Padding.Left / 2), Top);
            }
        }

        /// <summary>
        /// Gets or sets the point at which an insertion indicator should be drawn for controls to
        /// be dropped after this control.
        /// </summary>
        /// <value>
        /// The point at which an insertion indicator should be drawn for controls to be dropped
        /// after this control.
        /// </value>
        public virtual Point TrailingInsertionPoint
        {
            get
            {
                int left = (NextControl == null || NextControl.Left < Right)
                    ? Right - Padding.Right
                    : NextControl.Left + (NextControl.Padding.Left / 2);

                return new Point(left, Top);
            }
        }

        /// <summary>
        /// The <see cref="PaginationControl"/> before this instance in a
        /// <see cref="PageLayoutControl"/>'s sequence of controls.
        /// </summary>
        /// <returns>The <see cref="PaginationControl"/> before this instance.</returns>
        public PaginationControl PreviousControl
        {
            get
            {
                try
                {
                    // If not currently in a PageLayoutControl, there is no previous control.
                    if (Parent == null)
                    {
                        return null;
                    }

                    int index = Parent.Controls.IndexOf(this);
                    ExtractException.Assert("ELI35487", "Unexpected control state.",
                        index >= 0);

                    PaginationControl previousControl = null;
                    for (index = index - 1; previousControl == null && index >= 0; index--)
                    {
                        previousControl = Parent.Controls[index] as PaginationControl;
                    }

                    return previousControl;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35488");
                }
            }
        }

        /// <summary>
        /// The <see cref="PaginationControl"/> after this instance in a
        /// <see cref="PageLayoutControl"/>'s sequence of controls.
        /// </summary>
        /// <returns>The <see cref="PaginationControl"/> after this instance.</returns>
        public PaginationControl NextControl
        {
            get
            {
                try
                {
                    // If not currently in a PageLayoutControl, there is no previous control.
                    if (Parent == null)
                    {
                        return null;
                    }

                    int index = Parent.Controls.IndexOf(this);
                    ExtractException.Assert("ELI35489", "Unexpected control state.",
                        index >= 0);

                    PaginationControl nextControl = null;
                    for (index = index + 1; nextControl == null && index < Parent.Controls.Count; index++)
                    {
                        nextControl = Parent.Controls[index] as PaginationControl;
                    }

                    return nextControl;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35490");
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="OutputDocument"/> to which this instance belongs.
        /// </summary>
        public virtual OutputDocument Document
        {
            get;
            protected set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.HandleCreated"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnHandleCreated(EventArgs e)
        {
            try
            {
                base.OnHandleCreated(e);

                // Register to receive key events from child controls that should be raised as if
                // they are coming from this control.
                RegisterForEvents(this);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35491");
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
                if (this is PaginationSeparator ||
                    Parent == null ||
                    Parent.ClientRectangle.IntersectsWith(Bounds))
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
            if (!e.ClipRectangle.IsEmpty)
            {
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

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of a child control in order that it can be
        /// raised by this control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleControl_Click(object sender, EventArgs e)
        {
            // Ensure not to re-raise an event already being raised.
            if (_raisingEvent)
            {
                return;
            }

            try
            {
                _raisingEvent = true;

                OnClick(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35492");
            }
            finally
            {
                _raisingEvent = false;
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DoubleClick"/> event of a child control in order that it
        /// can be raised by this control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleControl_DoubleClick(object sender, EventArgs e)
        {
            // Ensure not to re-raise an event already being raised.
            if (_raisingEvent)
            {
                return;
            }

            try
            {
                _raisingEvent = true;

                OnDoubleClick(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35493");
            }
            finally
            {
                _raisingEvent = false;
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.MouseMove"/> event of a child control in order that it
        /// can be raised by this control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance
        /// containing the event data.</param>
        void HandleControl_MouseMove(object sender, MouseEventArgs e)
        {
            // Ensure not to re-raise an event already being raised.
            if (_raisingEvent)
            {
                return;
            }

            try
            {
                _raisingEvent = true;

                // Translate the coordinates of the event so that they are relative to this control's
                // coordinate system.
                Point thisLocation = PointToScreen(Location);
                Point eventLocation = PointToScreen(e.Location);
                thisLocation.Offset(-eventLocation.X, -eventLocation.Y);
                var eventArgs =
                    new MouseEventArgs(e.Button, e.Clicks, thisLocation.X, thisLocation.Y, e.Delta);

                OnMouseMove(eventArgs);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35494");
            }
            finally
            {
                _raisingEvent = false;
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.KeyUp"/> event of a child control in order that it can be
        /// raised by this control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing
        /// the event data.</param>
        void HandleControl_KeyUp(object sender, KeyEventArgs e)
        {
            // Ensure not to re-raise an event already being raised.
            if (_raisingEvent)
            {
                return;
            }

            try
            {
                _raisingEvent = true;

                OnKeyUp(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35495");
            }
            finally
            {
                _raisingEvent = false;
            }
        }

        #endregion Event Handlers

        #region Protected Members

        /// <summary>
        /// Registers to receive key events from child controls that should be raised as if
        /// they are coming from this control.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> whose children's events should be
        /// forwarded.</param>
        protected virtual void RegisterForEvents(Control control)
        {
            foreach (Control childControl in control.Controls)
            {
                // If the child control raises any of these events, this control should forward the
                // event by raising it itself.
                childControl.Click += HandleControl_Click;
                childControl.DoubleClick += HandleControl_DoubleClick;
                childControl.MouseMove += HandleControl_MouseMove;
                childControl.KeyUp += HandleControl_KeyUp;

                RegisterForEvents(childControl);
            }
        }

        /// <summary>
        /// Unregisters to receive key events from child controls
        /// </summary>
        /// <param name="control">The <see cref="Control"/> whose children's events should be
        /// unregistered.</param>
        protected virtual void UnRegisterForEvents(Control control)
        {
            foreach (Control childControl in control.Controls)
            {
                childControl.Click -= HandleControl_Click;
                childControl.DoubleClick -= HandleControl_DoubleClick;
                childControl.MouseMove -= HandleControl_MouseMove;
                childControl.KeyUp -= HandleControl_KeyUp;

                UnRegisterForEvents(childControl);
            }
        }

        #endregion Protected Members
    }
}
