﻿using System;
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
                    ? Right
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

                    var previousControl = (index == 0) ? null
                        : Parent.Controls[index - 1];

                    return previousControl as PaginationControl;
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

                    var nextControl = (index + 1 >= Parent.Controls.Count) ? null :
                        Parent.Controls[index + 1];

                    return nextControl as PaginationControl;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35490");
                }
            }
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

        #region Private Members

        /// <summary>
        /// Registers to receive key events from child controls that should be raised as if
        /// they are coming from this control.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> whose children's events should be
        /// forwarded.</param>
        void RegisterForEvents(Control control)
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

        #endregion Private Members
    }
}
