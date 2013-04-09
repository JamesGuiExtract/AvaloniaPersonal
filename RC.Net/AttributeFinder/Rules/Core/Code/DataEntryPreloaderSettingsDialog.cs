using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// A <see cref="Form"/> that allows for the configuration of a <see cref="DataEntryPreloader"/>
    /// instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class DataEntryPreloaderSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataEntryPreloaderSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryPreloaderSettingsDialog"/>
        /// class.
        /// </summary>
        /// <param name="settings">The <see cref="DataEntryPreloader"/> instance to configure.
        /// </param>
        public DataEntryPreloaderSettingsDialog(DataEntryPreloader settings)
        {

            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                    "ELI35060", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                _configFileNamePathTagsButton.PathTags = new AttributeFinderPathTags();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35061");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="DataEntryPreloader"/> to configure.
        /// </summary>
        /// <value>The <see cref="DataEntryPreloader"/> to configure.</value>
        public DataEntryPreloader Settings
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

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    _configFileNameTextBox.Text = Settings.ConfigFileName;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35062");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// In the case that the OK button is clicked, validates the settings, applies them, and
        /// closes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_configFileNameTextBox.Text))
                {
                    UtilityMethods.ShowMessageBox(
                        "The data entry config file to use has not been specified.",
                        "Missing configuration", true);
                    return;
                }

                Settings.ConfigFileName = _configFileNameTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35063");
            }
        }

        #endregion Event Handlers
    }
}
