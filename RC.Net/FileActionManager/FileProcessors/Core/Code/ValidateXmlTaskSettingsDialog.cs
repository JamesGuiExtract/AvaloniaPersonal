using Extract.Licensing;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// A <see cref="Form"/> to view and modify settings for an <see cref="ValidateXmlTask"/> instance.
    /// </summary>
    public partial class ValidateXmlTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(ValidateXmlTaskSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateXmlTaskSettingsDialog"/> class.
        /// </summary>
        public ValidateXmlTaskSettingsDialog()
            : this(null)
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38384");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateXmlTaskSettingsDialog"/> class.
        /// </summary>
        public ValidateXmlTaskSettingsDialog(ValidateXmlTask settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI38385",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38386");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public ValidateXmlTask Settings
        {
            get;
            set;
        }

        #endregion Properties

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

                _xmlFileNameTextBox.Text = Settings.XmlFileName;
                _treatWarningsAsErrorCheckBox.Checked = Settings.TreatWarningsAsErrors;
                _noSchemaValidationRadioButton.Checked =
                    (Settings.XmlSchemaValidation == XmlSchemaValidation.None);
                _validateInlineSchemaRadioButton.Checked =
                    (Settings.XmlSchemaValidation == XmlSchemaValidation.InlineSchema);
                _validateSpecifiedSchemaRadioButton.Checked =
                    (Settings.XmlSchemaValidation == XmlSchemaValidation.SpecifiedSchema);
                _requireInlineSchemaCheckBox.Checked = Settings.RequireInlineSchema;
                _schemaFilenameTextBox.Text = Settings.SchemaFileName;

                _validateInlineSchemaRadioButton.CheckedChanged += (o, args) => UpdateUI();
                _validateSpecifiedSchemaRadioButton.CheckedChanged += (o, args) => UpdateUI();

                UpdateUI();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38387");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the <see cref="Control.Click"/> event.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (WarnIfInvalid())
                {
                    return;
                }

                Settings.XmlFileName = _xmlFileNameTextBox.Text;
                Settings.TreatWarningsAsErrors = _treatWarningsAsErrorCheckBox.Checked;

                if (_noSchemaValidationRadioButton.Checked)
                {
                    Settings.XmlSchemaValidation = XmlSchemaValidation.None;
                }
                else if (_validateInlineSchemaRadioButton.Checked)
                {
                    Settings.XmlSchemaValidation = XmlSchemaValidation.InlineSchema;
                }
                else if (_validateSpecifiedSchemaRadioButton.Checked)
                {
                    Settings.XmlSchemaValidation = XmlSchemaValidation.SpecifiedSchema;
                }
                else
                {
                    ExtractException.ThrowLogicException("ELI38398");
                }

                Settings.RequireInlineSchema = _requireInlineSchemaCheckBox.Checked;
                Settings.SchemaFileName = _schemaFilenameTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI38388", ex);
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Updates the enabled status of the controls based on the current selections.
        /// </summary>
        void UpdateUI()
        {
            try
            {
                _requireInlineSchemaCheckBox.Enabled = _validateInlineSchemaRadioButton.Checked;
                _schemaFilenameTextBox.Enabled = _validateSpecifiedSchemaRadioButton.Checked;
                _schemaFileNameBrowseButton.Enabled = _validateSpecifiedSchemaRadioButton.Checked;
                _schemaFileNamePathTagButton.Enabled = _validateSpecifiedSchemaRadioButton.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38400");
            }
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            if (string.IsNullOrWhiteSpace(_xmlFileNameTextBox.Text))
            {
                _xmlFileNameTextBox.Focus();
                MessageBox.Show("Please specify the name of the XML file to validate.",
                    "Missing XML filename", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            if (_validateSpecifiedSchemaRadioButton.Checked &&
                string.IsNullOrWhiteSpace(_schemaFilenameTextBox.Text))
            {
                _schemaFilenameTextBox.Focus();
                MessageBox.Show("Please specify the name of the schema definition file.",
                    "Missing schema definition filename", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
