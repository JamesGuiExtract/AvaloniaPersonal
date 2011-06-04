using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using Extract.Licensing;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// A dialog that will update progress status from a <see cref="ProgressStatus"/>
    /// object.
    /// </summary>
    public partial class ProgressStatusDialogForm : Form
    {
        #region Constants

        /// <summary>
        /// The maximum height the scroll bar panel can grow to.
        /// </summary>
        const int _MAX_CONTENT_HEIGHT = 350;

        /// <summary>
        /// Default message for the progress status.
        /// </summary>
        const string _DEFAULT_NO_PROGRESS_MESSAGE =
            "No progress status information is available at this time.";

        /// <summary>
        /// Default window title for the progress status dialog.
        /// </summary>
        const string _DEFAULT_WINDOW_TITLE = "Progress Status";

        /// <summary>
        /// Object name used in licensing calls
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ProgressStatusDialogForm).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The progress status object that will be used to update the dialog.
        /// </summary>
        ProgressStatus _progressStatus;

        /// <summary>
        /// The stop button that will be attacked to the form if a stop event handle is provided.
        /// </summary>
        Button _stop;

        /// <summary>
        /// Whether or not the stop button has been clicked.
        /// </summary>
        bool _stopped;

        /// <summary>
        /// The number of progress levels to display in the dialog.
        /// </summary>
        int _progressLevels;

        /// <summary>
        /// The event handle to signal when the stop button is clicked.
        /// </summary>
        IntPtr _stopEventHandle;

        /// <summary>
        /// The collection of progress bars that have been added to the dialog.
        /// </summary>
        List<BetterProgressBar> _progressBars;

        /// <summary>
        /// The collection of labels that have been added to the dialog.
        /// </summary>
        List<Label> _labels;

        /// <summary>
        /// Indicates whether the progress form is to have its status updated manually rather than
        /// via a <see cref="ProgressStatus"/> instance.
        /// </summary>
        bool _manualMode;

        /// <summary>
        /// Mutex used to synchronize methods that operate on the progress status object.
        /// </summary>
        object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="ProgressStatusDialogForm"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressStatusDialogForm"/> class.
        /// </summary>
        public ProgressStatusDialogForm()
            : this(3, 100, true, IntPtr.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressStatusDialogForm"/> class in
        /// "manual" mode with one label and one progress bar. The resulting instance will need to
        /// updated manually via the <see cref="ProgressValue"/> property. Specifying
        /// <see cref="ProgressStatus"/> is not allowed in manual mode.
        /// </summary>
        /// <param name="labelText">The string to apply to the status bar label.</param>
        /// <param name="maxValue">An <see langword="int"/> specifying the <see cref="ProgressValue"/>
        /// at which the progress bar should indicate 100% progress.</param>
        /// <param name="showCloseButton">Whether the close button should be displayed
        /// or not.</param>
        /// <param name="stopEvent">The event handle that should be signaled
        /// when the stop button is pressed. This may be <see cref="IntPtr.Zero"/></param>
        public ProgressStatusDialogForm(string labelText, int maxValue, bool showCloseButton, IntPtr stopEvent)
            : this (1, 0, showCloseButton, stopEvent)
        {
            try
            {
                _manualMode = true;

                _labels[0].Text = labelText;
                _progressBars[0].Maximum = maxValue;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32632");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressStatusDialogForm"/> class.
        /// </summary>
        /// <param name="progressLevels">The number of progress levels to display.</param>
        /// <param name="delayBetweenRefreshes">The amount of time to delay between
        /// status refreshes.
        /// <para><b>Note:</b></para>
        /// This cannot be less than 100, if it is less than 100 it will be set to
        /// 100.</param>
        /// <param name="showCloseButton">Whether the close button should be displayed
        /// or not.</param>
        /// <param name="stopEvent">The event handle that should be signaled
        /// when the stop button is pressed. This may be <see cref="IntPtr.Zero"/></param>
        public ProgressStatusDialogForm(int progressLevels, int delayBetweenRefreshes,
            bool showCloseButton, IntPtr stopEvent)
        {
            try
            {
                // Only validate the license at run time
                if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
                {
                    LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                        "ELI30429", _OBJECT_NAME);
                }

                InitializeComponent();

                // 1000 should be well beyond the width needed.
                _topPanel.MaximumSize = new Size(1000, _MAX_CONTENT_HEIGHT);

                if (showCloseButton)
                {
                    ControlBox = true;
                }

                _progressLevels = progressLevels;
                _stopEventHandle = stopEvent;

                _timer.Interval = delayBetweenRefreshes < 100 ? 100 : delayBetweenRefreshes;

                LayoutControls();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30348", ex);
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Raises the <see cref="Control.VisibleChanged"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            try
            {
                if (!_manualMode)
                {
                    if (Visible)
                    {
                        _timer.Start();
                    }
                    else
                    {
                        _timer.Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30349", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.Closing"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            // Just hide the form
            base.OnClosing(e);
            e.Cancel = true;
            Visible = false;
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the stop button.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleStopButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Only allow the stop button to be clicked once
                _stop.Enabled = false;

                _stopped = true;

                NativeMethods.SignalEvent(_stopEventHandle);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30350", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Timer.Tick"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (!Visible)
                {
                    // No need to update progress when the form is hidden
                    return;
                }

                lock (_lock)
                {
                    ProgressStatus status = _progressStatus;
                    for (int i = 0; i < _progressLevels; i++)
                    {
                        var label = _labels[i];
                        var bar = _progressBars[i];

                        if (_stopped)
                        {
                            label.Text = i == 0 ? "Stopping" : string.Empty;

                            bar.Value = 0;
                            continue;
                        }

                        string text =
                            (status == null && i == 0) ? _DEFAULT_NO_PROGRESS_MESSAGE : string.Empty;
                        int percent = 0;
                        if (status != null)
                        {
                            percent = (int)(status.GetProgressPercent() * 100);
                            text = status.Text;

                            // Get the next level progress status
                            status = status.SubProgressStatus;
                        }

                        label.Text = text;
                        bar.Value = percent;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30351", ex);
            }
        }

        /// <summary>
        /// Performs the layout of the progress bars and labels based on the number of progress
        /// levels that will be reported.
        /// </summary>
        void LayoutControls()
        {
            // Get the location for the first control
            Point startLocation = _topPanel.DisplayRectangle.Location;
            startLocation.Offset(_topPanel.Margin.Left, _topPanel.Margin.Top);

            // Set the next location to the start location
            Point nextControlLocation = startLocation;

            // Add the controls to the dialog
            _progressBars = new List<BetterProgressBar>(_progressLevels);
            _labels = new List<Label>(_progressLevels);
            int width = _topPanel.Width - _topPanel.Padding.Horizontal;
            for (int i = 0; i < _progressLevels; i++)
            {
                var label = new Label();
                label.Width = width - label.Margin.Horizontal;
                _labels.Add(label);
                AddControlToPanel(label, ref nextControlLocation);

                var bar = new BetterProgressBar();
                bar.Width = width - bar.Margin.Horizontal;
                bar.DisplayZeroPercent = false;
                _progressBars.Add(bar);
                AddControlToPanel(bar, ref nextControlLocation);
            }

            // Add the stop button if required
            if (_stopEventHandle != IntPtr.Zero)
            {
                _stop = new Button();
                _stop.Text = "Stop";
                _stop.Location = new Point(_topPanel.ClientRectangle.Right - _stop.Width,
                    _topPanel.ClientRectangle.Bottom + _topPanel.Padding.Bottom);
                _stop.Click += HandleStopButtonClick;
                Controls.Add(_stop);
                _stop.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                nextControlLocation = _stop.Location;
                nextControlLocation.Offset(0, _stop.Height + Padding.Bottom);
            }

            // If a vertical scroll bar is necessary, add horizontal space for it so the horizontal
            // scroll bar doesn't show up.
            if (_topPanel.VerticalScroll.Visible)
            {
                Width += SystemInformation.VerticalScrollBarWidth;
            }
        }

        /// <summary>
        /// Adds the specified control to the top panel.  Also updates the
        /// <paramref name="nextControlLocation"/> to point to Location for next
        /// control.
        /// </summary>
        /// <param name="control">The control to add.</param>
        /// <param name="nextControlLocation">The location to add the control to.</param>
        void AddControlToPanel(Control control, ref Point nextControlLocation)
        {
            ExtractException.Assert("ELI30352", "Control cannot be null!", control != null);

            // Set the controls location and and add it to the top panel
            control.Location = nextControlLocation;
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _topPanel.Controls.Add(control);

            // Update the next control location
            nextControlLocation.Offset(0, control.Height + control.Margin.Vertical);
        }

        /// <summary>
        /// Updates the window title for the progress dialog.
        /// </summary>
        /// <param name="windowTitle">The title to set for the progress dialog.</param>
        internal void UpdateTitle(string windowTitle)
        {
            string title =
                string.IsNullOrEmpty(windowTitle) ? _DEFAULT_WINDOW_TITLE : windowTitle;
            Text = title;
        }

        #endregion Properties

        #region Properties

        /// <summary>
        /// Gets/sets the progress status object associated with the progress dialog.
        /// </summary>
        [CLSCompliant(false)]
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public ProgressStatus ProgressStatus
        {
            get
            {
                lock (_lock)
                {
                    return _progressStatus;
                }
            }
            set
            {
                lock (_lock)
                {
                    ExtractException.Assert("ELI32631",
                        "Progress dialog not initialized in automatic mode.",
                        !_manualMode);

                    _progressStatus = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the amount of progress the progress bar indicates in manual mode.
        /// </summary>
        /// <value>
        /// An <see langword="int"/> indicating the progress toward the maxValue specified in
        /// the <see cref="ProgressStatusDialogForm(string, int, bool, IntPtr)"/> constructor.
        /// </value>
        public int ProgressValue
        {
            get
            {
                return _progressBars[0].Value;
            }

            set
            {
                ExtractException.Assert("ELI32633",
                    "Progress dialog not initialized in manual mode.", _manualMode);

                _progressBars[0].Value = value;
            }
        }

        #endregion Properties
    }
}
