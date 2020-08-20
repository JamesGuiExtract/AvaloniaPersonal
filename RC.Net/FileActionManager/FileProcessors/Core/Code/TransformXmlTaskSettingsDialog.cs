using System;
using System.Windows.Forms;
using Extract.Licensing;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// A <see cref="Form"/> to view and modify settings for a
    /// <see cref="TransformXmlTask"/> instance.
    /// </summary>
    public partial class TransformXmlTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name used for license validation.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(TransformXmlTaskSettingsDialog).ToString();

        #endregion Constants

        #region Fields

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformXmlTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="TransformXmlTask"/> to configure</param>
        public TransformXmlTaskSettingsDialog(TransformXmlTask settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI50287",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50288");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public TransformXmlTask Settings
        {
            get;
            set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                inputXmlTextBox.Text = Settings.InputPath;
                styleSheetTextBox.Text = Settings.StyleSheet;
                specifiedStyleSheetRadioButton.Checked = Settings.UseSpecifiedStyleSheet;
                alphaSortRadioButton.Checked = !Settings.UseSpecifiedStyleSheet;
                outputTextBox.Text = Settings.OutputPath;

                specifiedStyleSheetRadioButton.CheckedChanged += (o, args) => UpdateUI();

                UpdateUI();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50289");
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

                Settings.InputPath = inputXmlTextBox.Text;
                Settings.StyleSheet = styleSheetTextBox.Text;
                Settings.UseSpecifiedStyleSheet = specifiedStyleSheetRadioButton.Checked;
                Settings.OutputPath = outputTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI50290", ex);
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
                styleSheetTextBox.Enabled =
                styleSheetPathTagButton.Enabled =
                styleSheetBrowseButton.Enabled =
                    specifiedStyleSheetRadioButton.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50291");
            }
        }
        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><c>true</c> if the settings are invalid; <c>false</c> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            if (string.IsNullOrEmpty(inputXmlTextBox.Text))
            {
                inputXmlTextBox.Focus();
                MessageBox.Show("Please specify the name of the input XML file.",
                    "Missing input filename", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            if (specifiedStyleSheetRadioButton.Checked
                && string.IsNullOrEmpty(styleSheetTextBox.Text))
            {
                styleSheetTextBox.Focus();
                MessageBox.Show("Please specify the name of the XSLT file.",
                    "Missing XSLT filename", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            if (string.IsNullOrEmpty(outputTextBox.Text))
            {
                outputTextBox.Focus();
                MessageBox.Show("Please specify the name of the output file.",
                    "Missing output filename", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
