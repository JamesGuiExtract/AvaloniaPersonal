using Extract.Licensing;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Displays a dialog with a scrolling marquee until a particular
    /// wait handle has been signaled.
    /// </summary>
    public partial class PleaseWaitForm : Form
    {
        #region Constants

        /// <summary>
        /// Minimum amount of time that the dialog should be displayed.
        /// </summary>
        const int _MIN_SECONDS_TO_DISPLAY = 2;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Object name string used in licensing calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(PleaseWaitForm).ToString();

        /// <summary>
        /// The event handle to watch for.
        /// </summary>
        EventWaitHandle _handle;

        /// <summary>
        /// The time out value to wait for the event to signal.
        /// </summary>
        int _timeOut;

        /// <summary>
        /// Flag to indicate whether the dialog closed due to a timeout or not.
        /// </summary>
        bool _timedOut;

        /// <summary>
        /// The minimum amount of time the dialog should be displayed
        /// </summary>
        int _minimumDisplayTime;

        /// <summary>
        /// Stop watch used to time how long the form has been displayed.
        /// </summary>
        Stopwatch _stopWatch = new Stopwatch();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">A <see cref="T:System.Windows.Forms.Message"/>, passed by reference, that represents the Win32 message to process.</param>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"/> values that represents the key to process.</param>
        /// <returns>
        /// true if the keystroke was processed and consumed by the control; otherwise, false to allow further processing.
        /// </returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Eat the Alt+F4 key command
            if (keyData == (Keys.Alt | Keys.F4))
            {
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PleaseWaitForm"/> class.
        /// </summary>
        /// <param name="messageText">The message text.</param>
        /// <param name="watchEvent">The event whose signal will close this form.</param>
        public PleaseWaitForm(string messageText, EventWaitHandle watchEvent)
            : this(messageText, watchEvent, _MIN_SECONDS_TO_DISPLAY, Timeout.Infinite)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PleaseWaitForm"/> class.
        /// </summary>
        /// <param name="messageText">The message text.</param>
        /// <param name="watchEvent">The event whose signal will close this form.</param>
        /// <param name="minimumDisplayTime">The minimum amount of time (in seconds) that the
        /// dialog should be displayed.</param>
        public PleaseWaitForm(string messageText, EventWaitHandle watchEvent,
            int minimumDisplayTime)
            : this(messageText, watchEvent, minimumDisplayTime, Timeout.Infinite)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PleaseWaitForm"/> class.
        /// </summary>
        /// <param name="messageText">The message text.</param>
        /// <param name="watchEvent">The event whose signal will close this form.</param>
        /// <param name="minimumDisplayTime">The minimum amount of time (in seconds) that the
        /// dialog should be displayed.</param>
        /// <param name="timeout">A timeout value for waiting.</param>
        public PleaseWaitForm(string messageText, EventWaitHandle watchEvent,
            int minimumDisplayTime, int timeout)
        {
            try
            {
                InitializeComponent();

                if (string.IsNullOrWhiteSpace(messageText))
                {
                    throw new ArgumentException("Message cannot be null or empty.", "messageText");
                }
                if (watchEvent == null)
                {
                    throw new ArgumentException("Event handle cannot be null.", "watchEvent");
                }

                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI30989",
                    _OBJECT_NAME);

                _labelMessage.Text = messageText;
                _handle = watchEvent;
                _timeOut = timeout;
                _minimumDisplayTime = minimumDisplayTime;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30990", ex);
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Shown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnShown(EventArgs e)
        {
            try
            {
                _stopWatch.Start();

                base.OnShown(e);
                if (!_handle.WaitOne(0))
                {
                    Task.Factory.StartNew(() =>
                        {
                            _timedOut = !_handle.WaitOne(_timeOut);
                            CloseForm();
                        }
                    );
                }
                else
                {
                    CloseForm();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30991", ex);
            }
        }

        /// <summary>
        /// Closes the form.
        /// </summary>
        void CloseForm()
        {
            try
            {
                _stopWatch.Stop();
                var span = new TimeSpan(0, 0, _minimumDisplayTime);
                if (_stopWatch.Elapsed < span)
                {
                    span = span.Subtract(_stopWatch.Elapsed);
                    Task.Factory.StartNew(() =>
                        {
                            Thread.Sleep(span);
                            BeginInvoke((MethodInvoker)(
                                () => { DialogResult = DialogResult.OK; }));
                        });
                }
                else
                {
                    DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30992", ex);
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets a value indicating whether or not the dialog closed due to
        /// a timeout on the wait handle or not.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the wait timed out; otherwise, <see langword="false"/>.
        /// </value>
        public bool TimedOut
        {
            get
            {
                return _timedOut;
            }
        }

        #endregion Properties
    }
}
