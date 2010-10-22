using Extract.Drawing;
using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// A tab with a <see cref="LinkLabel"/> that will retract into the top of the screen and
    /// redisplay when the mouse is moved to the top of the screen. The <see cref="LabelClicked"/>
    /// event will indicate when the label itself was clicked as opposed to a click anywhere in
    /// the tab.
    /// </summary>
    public partial class AutoHideScreenTab : Form
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(AutoHideScreenTab).ToString();

        /// <summary>
        /// The amount of time the tab will remain visible when it is first shown.
        /// </summary>
        const int _INITIAL_DISPLAY_TIME = 1000;

        /// <summary>
        /// The frequency (in ms) that the mouse will be polled for position and the animation will
        /// be updated.
        /// </summary>
        const int _ANIMATION_INTERVAL = 50;

        /// <summary>
        /// The maximum number of pixels per frame the tab will open at.
        /// </summary>
        const int _MAX_OPEN_SPEED = 20;

        /// <summary>
        /// The maximum number of pixels per frame the tab will close at.
        /// </summary>
        const int _MAX_CLOSE_SPEED = 5;

        /// <summary>
        /// The number of pixels per frame of acceleration when beginning/ending movement.
        /// </summary>
        const int _ACCELERATION = 2;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        bool _inDesignMode;

        /// <summary>
        /// <see langword="true"/> if the mouse is hovering over the tab to open it,
        /// <see langword="false"/> if it should be closed.
        /// </summary>
        bool _isMouseHovering = false;

        /// <summary>
        /// <see langword="true"/> if the tab is completely opened, <see langword="false"/> if it
        /// is completely closed, or <see langword="null"/> if it is in transit.
        /// </summary>
        bool? _opened = true;

        /// <summary>
        /// The current speed of the tab in pixels per frame.
        /// </summary>
        int _speed = 0;

        /// <summary>
        /// The Y-coordinate of the control when in the opened position.
        /// </summary>
        int _openedPosition;

        /// <summary>
        /// The bounds defining where the tab will actually be drawn.
        /// </summary>
        Rectangle _tabArea;

        /// <summary>
        /// A path tracing the outline of the tab.
        /// </summary>
        GraphicsPath _tabOutline;

        /// <summary>
        /// Ticks for every frame of animation or mouse polling.
        /// </summary>
        Timer _animationTimer = new Timer();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="AutoHideScreenTab"/> instance.
        /// </summary>
        /// <param name="labelText">The text to be displayed on the tab's label.</param>
        public AutoHideScreenTab(string labelText)
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.ExtractCoreObjects, "ELI30812", _OBJECT_NAME);

                InitializeComponent();

                // Set the transparency key to a color that is likely to closely match the
                // menus/buttons behind the tab so that anti-aliasing of the curved corners appears
                // as smooth as possible.
                BackColor = SystemColors.InactiveCaptionText;
                TransparencyKey = SystemColors.InactiveCaptionText;

                _linkLabel.Text = labelText;
                _linkLabel.LinkClicked += HandleLinkClicked;

                _animationTimer.Tick += HandleAnimationTimerTick;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30811", ex);
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when the <see cref="AutoHideScreenTab"/>'s <see cref="LinkLabel"/> is clicked.
        /// </summary>
        public event EventHandler<EventArgs> LabelClicked;

        #endregion Events

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // We don't want to try to hide the tab at the top of the screen in design mode.
                if (!_inDesignMode)
                {
                    // Link label is not auto-sized in design mode for centering purposes, but to
                    // determine the tab size, make it auto-sized now.
                    _linkLabel.AutoSize = true;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30813", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.VisibleChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            try
            {
                base.OnVisibleChanged(e);

                // We don't want to try to hide the tab at the top of the screen in design mode.
                if (!_inDesignMode)
                {
                    if (Visible)
                    {
                        InitializePosition();
                    }
                    else
                    {
                        _animationTimer.Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI30814", ex);
            }
        }

        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Keep track of the original smoothing mode so it can be restored.
            SmoothingMode originalSmoothingMode = e.Graphics.SmoothingMode;

            try
            {
                base.OnPaintBackground(e);

                // Both draw and fill the TabOutline since only filling it can make it appear
                // lop-sided.
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(ExtractPens.GetPen(Color.Black), TabOutline);
                e.Graphics.FillPath(ExtractBrushes.GetSolidBrush(Color.Black), TabOutline);
            }
            catch (Exception ex)
            {
                _animationTimer.Stop();

                ExtractException.AsExtractException("ELI30815", ex);

                // Hide to prevent a constant stream of exceptions.
                Hide();
            }
            finally
            {
                e.Graphics.SmoothingMode = originalSmoothingMode;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if managed resources should be disposed;
        /// otherwise, <see langword="false"/>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_tabOutline != null)
                {
                    _tabOutline.Dispose();
                    _tabOutline = null;
                }

                if (_animationTimer != null)
                {
                    _animationTimer.Dispose();
                    _animationTimer = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region EventHandlers

        /// <summary>
        /// Handles the animation timer tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        // _ANIMATION_INTERVAL doesn't appear to prevent screen savers from being activiated which
        // would be the primary concern here.
        [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        void HandleAnimationTimerTick(object sender, EventArgs e)
        {
            try
            {
                // In case this is the first poll after _INITIAL_DISPLAY_TIME, reset the timer.
                if (_animationTimer.Interval != _ANIMATION_INTERVAL)
                {
                    _animationTimer.Interval = _ANIMATION_INTERVAL;
                }

                // Determine whether the tab should be open or closed based on the mouse position.
                // If it is already in the desired position, do nothing further.
                // NOTE: I had intended to have the animation timer stop when the tab was not in
                // transit and use OnMouseEnter and OnMouseLeave to kick off the timer again, but
                // that was complicated by the fact that those events don't tread either the
                // transparent portion of the form or the label as "within" the form. Polling
                // provides the desired behavior and at 50ms doesn't have any discernable
                // performance impact.
                bool isMouseHovering = IsActive && Bounds.Contains(Control.MousePosition);
                if (!isMouseHovering && _isMouseHovering && Owner != null && IsActive)
                {
                    // If the mouse is no longer hovering, automatically pass focus back to the
                    // owning form.
                    Owner.Activate();
                }
                _isMouseHovering = isMouseHovering;
                if (_opened.HasValue)
                {
                    if (_isMouseHovering == _opened.Value)
                    {
                        // The tab is already in its desired position.
                        return;
                    }
                    else
                    {
                        // The tab is now in transit.
                        _opened = null;
                    }
                }

                // Determine the target position of the tab and the distance from that position.
                int targetPosition =
                    _isMouseHovering ? _openedPosition : _openedPosition - _tabArea.Height;
                int targetDistance = Math.Abs(targetPosition - Location.Y);

                // Determine the maximum speed the tab can be traveling while still having enough
                // time to stop smoothly.
                int maxSpeed;
                for (maxSpeed = Math.Min(_ACCELERATION, targetDistance);
                        maxSpeed + _ACCELERATION <= targetDistance;
                        maxSpeed += _ACCELERATION)
                {
                    targetDistance -= maxSpeed;
                }
                maxSpeed = Math.Min(maxSpeed, _isMouseHovering ? _MAX_OPEN_SPEED : _MAX_CLOSE_SPEED);

                // Accelerate the tab in the appropriate direction.
                _speed += _isMouseHovering ? _ACCELERATION : -_ACCELERATION;

                // If over the speed limit, cap the speed.
                if (Math.Abs(_speed) > maxSpeed)
                {
                    _speed = _isMouseHovering ? maxSpeed : -maxSpeed;
                }

                // Move the tab according to the new speed.
                Location = new Point(Location.X, Location.Y + _speed);

                // If the tab has arrived at its target position, stop it.
                if (Location.Y == targetPosition)
                {
                    _opened = _isMouseHovering;
                    _speed = 0;
                }
            }
            catch (Exception ex)
            {
                _animationTimer.Stop();

                ExtractException.Display("ELI30816", ex);

                // Hide to prevent a constant stream of exceptions.
                Hide();
            }
        }

        /// <summary>
        /// Handles the <see cref="LinkLabel.LinkClicked"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.LinkLabelLinkClickedEventArgs"/>
        /// instance containing the event data.</param>
        void HandleLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                OnLabelClicked();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30817", ex);
            }
        }

        #endregion EventHandlers

        #region Protected members

        /// <summary>
        /// Raises the <see cref="LabelClicked"/> event.
        /// </summary>
        protected virtual void OnLabelClicked()
        {
            if (LabelClicked != null)
            {
                LabelClicked(this, new EventArgs());
            }
        }

        /// <summary>
        /// Gets the tab outline.
        /// </summary>
        /// <value>The tab outline.</value>
        protected virtual GraphicsPath TabOutline
        {
            get
            {
                try
                {
                    if (_tabOutline == null)
                    {
                        // Make the radius of the rounded corners half the height of the tab.
                        int cornerRadius = _tabArea.Height / 2;

                        _tabOutline = new GraphicsPath();
                        _tabOutline.StartFigure();
                        
                        // The top
                        dynamic segment = GetPathSegment(_tabArea.Location, _tabArea.Width, 0);
                        _tabOutline.AddLine(segment.Start, segment.End);

                        // The right side
                        segment = GetPathSegment(segment.End, 0, cornerRadius);
                        _tabOutline.AddLine(segment.Start, segment.End);

                        // The bottom-right corner
                        segment = GetPathSegment(segment.End, -cornerRadius, cornerRadius);
                        _tabOutline.AddArc(Rectangle.FromLTRB(
                            segment.End.X, segment.Start.Y, segment.Start.X, segment.End.Y), 0, 90);

                        // The bottom
                        segment = GetPathSegment(segment.End, -_tabArea.Width + (2 * cornerRadius), 0);
                        _tabOutline.AddLine(segment.Start, segment.End);

                        // The bottom-left corner
                        segment = GetPathSegment(segment.End, -cornerRadius, -cornerRadius);
                        _tabOutline.AddArc(Rectangle.FromLTRB(
                            segment.End.X, segment.End.Y, segment.Start.X, segment.Start.Y), 90, 90);

                        // The left side
                        segment = GetPathSegment(segment.End, 0, -cornerRadius);
                        _tabOutline.AddLine(segment.Start, segment.End);

                        _tabOutline.CloseFigure();
                    }

                    return _tabOutline;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30818", ex);
                }
            }      
        }

        /// <summary>
        /// Gets a value indicating whether the tab should open in response to the mouse hovering
        /// at the top of the screen.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the tab should open; otherwise, <see langword="false"/>.
        /// </value>
        protected bool IsActive
        {
            get
            {
                return (Owner == null || Form.ActiveForm == Owner || Form.ActiveForm == this);
            }
        }

        #endregion Protected members

        #region Private members

        /// <summary>
        /// Initializes the <see cref="AutoHideScreenTab"/>'s size an position.
        /// </summary>
        void InitializePosition()
        {
            // Force the tab outline to be re-calculated if it already exists.
            if (_tabOutline != null)
            {
                _tabOutline.Dispose();
                _tabOutline = null;
            }

            // Initialize rectangle representing the size of the tab.
            _tabArea = new Rectangle(Point.Empty,
                new Size(_linkLabel.Width + _linkLabel.Height, _linkLabel.Height * 2));

            // Size the form based on the tab size; the form extends across the entire
            // length of the screen even though the tab is only drawn in the center.
            Screen screen = Screen.FromControl(Owner ?? this);
            _openedPosition = screen.Bounds.Location.Y - 1;
            Location = new Point(screen.Bounds.Location.X, _openedPosition);
            Width = screen.Bounds.Width;
            Height = _tabArea.Height + 2;

            // Center the tab and _linkLabel
            _tabArea.Offset((Width - _tabArea.Width) / 2, 0);
            Point linkLocation = _tabArea.Location;
            linkLocation.Offset((_tabArea.Width - _linkLabel.Width) / 2,
                                (_tabArea.Height - _linkLabel.Height) / 2);
            _linkLabel.Location = linkLocation;

            _speed = 0;
            _opened = true;

            // Whenever the tab is re-initialized, keep it open for the _INITIAL_DISPLAY_TIME.
            _animationTimer.Interval = _INITIAL_DISPLAY_TIME;
            _animationTimer.Start();

            // The owning form should get focus back after the tab has been displayed. Activate the
            // form ansynchronously via the message queue since InitializePosition is run as part
            // of a sequence of events that activates the form and therefore would nullify a
            // synchronous call to Owner.Activate.
            if (Owner != null && IsActive)
            {
                BeginInvoke((MethodInvoker)(() => { Owner.Activate(); }));
            }
        }

        /// <summary>
        /// Gets a path segment.
        /// </summary>
        /// <param name="startPoint">The start point of the segment</param>
        /// <param name="xOffset">The x offset of the end point.</param>
        /// <param name="yOffset">The y offset of the end point.</param>
        /// <returns>A <see langword="Tuple"/> with the values:
        /// <list type="bullet">
        /// <item><b>Start</b>:The <see cref="Point"/> representing the start of the segment.</item>
        /// <item><b>End</b>:The <see cref="Point"/> representing the start of the segment.</item>  
        /// </list></returns>
        static dynamic GetPathSegment(Point startPoint, int xOffset, int yOffset)
        {
            Point endPoint = startPoint;
            endPoint.Offset(xOffset, yOffset);
            
            return new
            {
                Start = startPoint,
                End = endPoint
            };
        }

        #endregion Private members
    }
}
