using Extract;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace Extract.NetDMSCustomComponents
{
    /// <summary>
    /// A <see cref="Form"/> for configuring a <see cref="NetDMSRedactionTask"/> instance.
    /// </summary>
    public partial class NetDMSRedactionTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(NetDMSRedactionTaskSettingsDialog).ToString();

        #endregion Constants

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NetDMSRedactionTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="NetDMSRedactionTask"/> instance to configure.</param>
        public NetDMSRedactionTaskSettingsDialog(NetDMSRedactionTask settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects,
                    "ELI34869", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34870");
            }
        }

        #endregion Contructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="NetDMSRedactionTask"/> to configure.
        /// </summary>
        /// <value>
        /// The <see cref="NetDMSRedactionTask"/> to configure.
        /// </value>
        public NetDMSRedactionTask Settings
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
                    _netDMSConnectionSettingsControl.LoadConnectionSettings(Settings);

                    _dataFileControl.DataFile = Settings.DataFileName;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34871");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the <see cref="_okButton"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // If there are invalid settings, prompt and return without closing.
                if (WarnIfInvalid())
                {
                    return;
                }

                _netDMSConnectionSettingsControl.ApplyConnectionSettings(Settings);

                Settings.DataFileName = _dataFileControl.DataFile;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34873");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            ExtractException.Assert("ELI34876",
                "NetDMS: Add redaction settings have not been provided.",
                    Settings != null);

            if (_netDMSConnectionSettingsControl.WarnIfInvalid())
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(_dataFileControl.DataFile))
            {
                _dataFileControl.Focus();
                UtilityMethods.ShowMessageBox("Please the data file to use to apply the redactions.",
                    "Data file not specified", false);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
