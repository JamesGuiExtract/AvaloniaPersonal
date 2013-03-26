using System;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    internal partial class PaginationControl : UserControl
    {
        #region Fields

        /// <summary>
        /// 
        /// </summary>
        bool _selected;

        /// <summary>
        /// 
        /// </summary>
        bool _handlingEvent;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSeparator"/> class.
        /// </summary>
        public PaginationControl()
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
        /// Gets or sets the selection area control.
        /// </summary>
        /// <value>
        /// The selection area control.
        /// </value>
        public virtual Control SelectionAreaControl
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PaginationControl"/> is
        /// selected.
        /// </summary>
        /// <value><see langword="true"/> if selected; otherwise, <see langword="false"/>.</value>
        public virtual bool Selected
        {
            get
            {
                return _selected;
            }

            set
            {
                try
                {
                    if (value != _selected)
                    {
                        _selected = value;

                        SelectionAreaControl.BackColor = _selected
                            ? SystemColors.ActiveBorder
                            : SystemColors.Control;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35486");
                }
            }
        }

        /// <summary>
        /// Gets or sets the preceeding insertion point.
        /// </summary>
        /// <value>
        /// The preceeding insertion point.
        /// </value>
        public virtual Point PreceedingInsertionPoint
        {
            get
            {
                return new Point(Left + (Padding.Left / 2), Top);
            }
        }

        /// <summary>
        /// Gets or sets the trailing insertion point.
        /// </summary>
        /// <value>
        /// The trailing insertion point.
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
        /// 
        /// </summary>
        /// <returns></returns>
        public PaginationControl PreceedingControl
        {
            get
            {
                try
                {
                    if (Parent == null)
                    {
                        return null;
                    }

                    int index = Parent.Controls.IndexOf(this);
                    ExtractException.Assert("ELI35487", "Unexpected control state.",
                        index >= 0);

                    var preceedingControl = (index == 0) ? null
                        : Parent.Controls[index - 1];

                    return preceedingControl as PaginationControl;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35488");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public PaginationControl NextControl
        {
            get
            {
                try
                {
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
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            try
            {
                base.OnHandleCreated(e);

                RegisterForEvents(this);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35491");
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Registers for events.
        /// </summary>
        /// <param name="control"></param>
        void RegisterForEvents(Control control)
        {
            foreach (Control childControl in control.Controls)
            {
                childControl.Click += HandleControl_Click;
                childControl.DoubleClick += HandleControl_DoubleClick;
                childControl.MouseMove += HandleControl_MouseMove;
                childControl.KeyUp += HandleControl_KeyUp;

                RegisterForEvents(childControl);
            }
        }

        #endregion Private Members

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the HandleControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleControl_Click(object sender, EventArgs e)
        {
            if (_handlingEvent)
            {
                return;
            }

            try
            {
                _handlingEvent = true;

                OnClick(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35492");
            }
            finally
            {
                _handlingEvent = false;
            }
        }

        /// <summary>
        /// Handles the DoubleClick event of the HandleControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleControl_DoubleClick(object sender, EventArgs e)
        {
            if (_handlingEvent)
            {
                return;
            }

            try
            {
                _handlingEvent = true;

                OnDoubleClick(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35493");
            }
            finally
            {
                _handlingEvent = false;
            }
        }

        /// <summary>
        /// Handles the MouseMove event of the HandleControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        void HandleControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_handlingEvent)
            {
                return;
            }

            try
            {
                _handlingEvent = true;

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
                _handlingEvent = false;
            }
        }

        /// <summary>
        /// Handles the KeyUp event of the HandleControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing
        /// the event data.</param>
        void HandleControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (_handlingEvent)
            {
                return;
            }

            try
            {
                _handlingEvent = true;

                OnKeyUp(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35495");
            }
            finally
            {
                _handlingEvent = false;
            }
        }

        #endregion Event Handlers
    }
}
