using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Extract.Reporting
{
    /// <summary>
    /// A user control for managing a <see cref="TextParameter"/>.
    /// </summary>
    public partial class TextParameterControl : UserControl, IExtractParameterControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(TextParameterControl).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="TextParameter"/> that underlies this control.
        /// </summary>
        TextParameter _textParameter;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new <see cref="TextParameterControl"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="TextParameterControl"/> class.
        /// </summary>
        public TextParameterControl()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="TextParameterControl"/> class.
        /// </summary>
        /// <param name="textParameter">The <see cref="TextParameter"/>
        /// to attach to this control.</param>
        public TextParameterControl(TextParameter textParameter)
        {
            try
            {
                // Load licenses if in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23823",
					_OBJECT_NAME);

                InitializeComponent();

                if (textParameter != null)
                {
                    _textParameter = textParameter;
                    _parameterName.Text = _textParameter.ParameterName;
                    _parameterValue.Text = _textParameter.ParameterValue ?? "";
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23824", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the current parameter value edit box. When setting will also
        /// update the underlying <see cref="TextParameter"/>.
        /// </summary>
        /// <returns>The current value in the parameter value edit box.</returns>
        /// <value>The current value in the parameter value edit box. Also updates
        /// the underlying <see cref="TextParameter"/>.</value>
        public string ParameterValue
        {
            get
            {
                return _parameterValue.Text;
            }
            set
            {
                try
                {
                    ExtractException.Assert("ELI23752", "Cannot set value to null!", value != null);
                    _parameterValue.Text = value;
                    _textParameter.ParameterValue = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI23753", ex);
                }
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleTextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_textParameter != null)
                {
                    _textParameter.ParameterValue = _parameterValue.Text;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23754", ex);
                ee.AddDebugData("Event Arguments", e, false);
                throw ee;
            }
        }

        #endregion Event Handler

        #region IExtractParameterControl Members

        /// <summary>
        /// Applies the current control value to the underlying <see cref="TextParameter"/>.
        /// </summary>
        public void Apply()
        {
            try
            {
                string parameterName = _parameterName.Text;

                ExtractException.Assert("ELI23847", "Cannot leave " + parameterName + " empty!",
                    !string.IsNullOrEmpty(_parameterValue.Text));

                if (_textParameter != null)
                {
                    // Replace instances of ' with '' so that sql queries will work correctly
                    // See https://extract.atlassian.net/browse/ISSUE-12591
                    _textParameter.ParameterValue = _parameterValue.Text.Replace("'", "''");
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23825", ex);
            }
        }

        #endregion
    }
}
