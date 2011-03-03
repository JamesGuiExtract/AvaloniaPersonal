using System;
using System.Globalization;
using System.Windows.Forms;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// A <see cref="ToolStripStatusLabel"/> used to display the total amount of time a document
    /// has been displayed for verification in the current session.
    /// </summary>
    public partial class VerificationTimeStatusStripLabel : ToolStripStatusLabel
    {
        #region Constants

        /// <summary>
        /// The text of the label (to be formatted with the number of seconds a document has been
        /// displayed for verification later.
        /// </summary>
        static readonly string _LABEL_TEXT = "Seconds verifying this document: {0:#}";

        #endregion Constants
        
        #region Fields

        /// <summary>
        /// The total amount of time the current document has been displayed for verification this
        /// session.
        /// </summary>
        double _secondsVerifyingThisDocument;

        /// <summary>
        /// A <see cref="Timer"/> used to trigger the next update to _secondsVerifyingThisDocument.
        /// </summary>
        Timer _timer = new Timer();
        
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationTimeStatusStripLabel"/> class.
        /// </summary>
        public VerificationTimeStatusStripLabel()
            : base()
        {
            try
            {
                InitializeComponent();

                _timer.Tick += HandleTimerTick;

                // Initialize the label text.
                UpdateLabel();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31936");
            }
        }

        #endregion Constructors

        #region Members

        /// <summary>
        /// Starts updating the seconds a document has been displayed starting with the specified
        /// <see paramref="elapsedSeconds"/> value.
        /// </summary>
        /// <param name="elapsedSeconds">The number of seconds at which to start the timer.</param>
        public void Start(double elapsedSeconds)
        {
            try
            {
                _secondsVerifyingThisDocument = elapsedSeconds;

                // Set the timer so that it fires at as it reaches the next full second.
                _timer.Interval = 1000 - ((int)(elapsedSeconds * 1000) % 1000);
                _timer.Start();
                
                // Update the label using the starting value.
                UpdateLabel();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31937");
            }
        }

        /// <summary>
        /// Stops updating the seconds the current document has been displayed for verification and
        /// clears the value.
        /// </summary>
        public void Stop()
        {
            try
            {
                _timer.Stop();

                _secondsVerifyingThisDocument = 0;

                UpdateLabel();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31938");
            }
        }

        #endregion Members

        /// <summary>
        /// Handles the <see cref="Timer.Tick"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTimerTick(object sender, EventArgs e)
        {
            try
            {
                // If the interval is not set to 1000, the timer was just started with an fractional
                // initial value. _secondsVerifyingThisDocument should be set to the next higher int
                // and the interval should be adjusted to fire once per second from this point on.
                if (_timer.Interval != 1000)
                {
                    _secondsVerifyingThisDocument = Math.Ceiling(_secondsVerifyingThisDocument);

                    _timer.Interval = 1000;
                }
                // With each passing second, increment _secondsVerifyingThisDocument by one.
                else
                {
                    _secondsVerifyingThisDocument += 1;
                }

                UpdateLabel();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31939");
            }
        }

        #region Private Members

        /// <summary>
        /// Updates the label based on the current value of _secondsVerifyingThisDocument.
        /// </summary>
        void UpdateLabel()
        {
            Text = string.Format(CultureInfo.CurrentCulture,
                _LABEL_TEXT, _secondsVerifyingThisDocument);
        }

        #endregion Private Members
    }
}
