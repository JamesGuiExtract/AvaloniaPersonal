using System.ComponentModel;
using System.Windows.Forms;
using Extract.Licensing;
using System;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// An extension of <see cref="NumericUpDown"/> that allow the user to be notified when a text
    /// value entered by the user was corrected due to being outside of the valid range, etc.
    /// </summary>
    public partial class BetterNumericUpDown : NumericUpDown
    {
        #region Constants

        /// <summary>
        /// Object name string used in licensing calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(BetterNumericUpDown).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates whether the user has made an edit that could require the UserTextCorrected
        /// event to be raised.
        /// </summary>
        bool _checkUserEdit = false;

        #endregion Fields

        #region Events

        /// <summary>
        /// Raised when the control corrects text entered by the user so that the value falls
        /// within the valid range, etc.
        /// </summary>
        public event EventHandler<EventArgs> UserTextCorrected;

        #endregion Event

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BetterNumericUpDown"/> class.
        /// </summary>
        public BetterNumericUpDown()
            : base()
        {
            try
            {
                // Only validate the license at run time
                if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
                {
                    LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                        "ELI31593", _OBJECT_NAME);
                }

                InitializeComponent();
            }
            catch (System.Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31594", ex);
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.TextChanged"/> event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnTextBoxTextChanged(object source, EventArgs e)
        {
            try
            {
                base.OnTextBoxTextChanged(source, e);

                _checkUserEdit |= UserEdit;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31596", ex);
            }
        }

        /// <summary>
        /// Displays the current value of the spin box (also known as an up-down control) in the
        /// appropriate format.
        /// </summary>
        protected override void UpdateEditText()
        {
            try
            {
                string originalText = Text;

                base.UpdateEditText();

                // If the user has made an edit to the control text and the UpdateEditText resulted
                // in a change to the text, raise an event indicating the text value was corrected.
                if (_checkUserEdit && originalText != Text)
                {
                    OnUserTextCorrected();
                }

                _checkUserEdit = false;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31595", ex);
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Raises the <see cref="UserTextCorrected"/> event.
        /// </summary>
        void OnUserTextCorrected()
        {
            if (UserTextCorrected != null)
            {
                UserTextCorrected(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
