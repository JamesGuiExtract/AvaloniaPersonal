using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A UI element that to be displayed between document pages to allow user to click to
    /// split documents
    /// </summary>
    internal partial class SplitDocumentIndicator : UserControl
    {
        /// <summary>
        /// Time in milliseconds the left mouse button must be held down on this indicator before
        /// <see cref="ActivationComplete"/> is raised and the document is split.
        /// </summary>
        const int _ACTIVATION_TIME = 400;

        /// <summary>
        /// Total number of steps to complete activation
        /// </summary>
        const int _ACTIVATION_STEP_COUNT = 10;

        /// <summary>
        /// Used to animate the indicator during activation so it is clear activation is occuring.
        /// </summary>
        Timer _activationTimer;

        /// <summary>
        /// Number of steps complete in any ongoing activation or 0 if no activation is occuring.
        /// </summary>
        int _activationProgress;

        /// <summary>
        /// Tooltip for this indicator to let users know its purpose.
        /// </summary>
        ToolTip _toolTip = new ToolTip();

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitDocumentIndicator"/> class.
        /// </summary>
        // The activation timer is used only temporarily; affecting power state is not a concern.
        [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        public SplitDocumentIndicator()
        {
            try
            {
                InitializeComponent();

                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);

                _activationTimer = new Timer();
                _activationTimer.Interval = _ACTIVATION_TIME / _ACTIVATION_STEP_COUNT;
                _activationTimer.Tick += HandleActivationTimer_Tick;

                _toolTip.AutoPopDelay = 0;
                _toolTip.InitialDelay = 500;
                _toolTip.ReshowDelay = 500;
                _toolTip.SetToolTip(this, "Hold left mouse button to split document here");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50234");
            }
        }

        /// <summary>
        /// Starts the animation. If _ACTIVATION_TIME elapses without deactivation,
        /// <see cref="ActivationComplete"/> will be raised.
        /// </summary>
        public void Activate()
        {
            try
            {
                if (_activationProgress == 0)
                {
                    _activationTimer.Start();
                    _activationProgress = 1;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50264");
            }
        }

        /// <summary>
        /// Cancels an on-going activation animation and prevents <see cref="ActivationComplete"/>
        /// from being raised.
        /// </summary>
        public void Deactivate()
        {
            try
            {
                if (_activationProgress > 0)
                {
                    _activationTimer.Stop();
                    _activationProgress = 0;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50265");
            }
        }

        /// <summary>
        /// Indicates whether an activation animation is currently in-progress and
        /// <see cref="ActivationComplete"/> event is imminent
        /// </summary>
        public bool Activating 
        {
            get
            {
                return _activationProgress > 0;
            }
        }

        /// <summary>
        /// Raised upon the conclusion of the animation activation
        /// </summary>
        public event EventHandler<EventArgs> ActivationComplete;

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                e.Graphics.DrawImage(Properties.Resources.Tear, ClientRectangle);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50235");
            }
        }

        /// <summary>
        /// Handles each step of of the activation animation in order to either render the
        /// next frame or to raise <see cref="ActivationComplete"/>.
        /// </summary>
        void HandleActivationTimer_Tick(object sender, EventArgs e)
        {
            if (_activationProgress > 0)
            {
                if (_activationProgress <= _ACTIVATION_STEP_COUNT)
                {
                    _activationProgress++;

                    // Less overhead to render directly to the control graphics rather than
                    // invalidate/paint the entire control. The Tear bitmap has transparency
                    // and as multiple calls are layered, it will become darker giving the
                    // image the appearance of fading in
                    using (var g = CreateGraphics())
                    {
                        g.DrawImage(Properties.Resources.Tear, ClientRectangle);
                    }
                }
                else
                {
                    _activationProgress = 0;
                    _activationTimer.Stop();
                    ActivationComplete?.Invoke(this, new EventArgs());
                }
            }
            else
            {
                _activationTimer.Stop();
            }
        }

        /// <summary>
        /// Gets the required creation parameters when the control handle is created.
        /// Overridden in order to make the control transparent.
        /// </summary>
        /// <returns>A <see cref="T:System.Windows.Forms.CreateParams"/> that contains the required
        /// creation parameters when the handle to the control is created.</returns>
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_TRANSPARENT = 0x0084;

                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= WS_EX_TRANSPARENT;
                return createParams;
            }
        }

        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Do not paint background so that any controls under this one show through.
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                components = null;

                _activationTimer?.Dispose();
                _activationTimer = null;
            }
            base.Dispose(disposing);
        }
    }
}
