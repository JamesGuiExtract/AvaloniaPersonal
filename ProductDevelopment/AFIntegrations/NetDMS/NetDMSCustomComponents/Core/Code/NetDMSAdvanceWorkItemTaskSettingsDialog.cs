using Extract;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace NetDMSCustomComponents
{
    /// <summary>
    /// A <see cref="Form"/> for configuring a <see cref="NetDMSAdvanceWorkItemTask"/> instance.
    /// </summary>
    public partial class NetDMSAdvanceWorkItemTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(NetDMSAdvanceWorkItemTaskSettingsDialog).ToString();

        #endregion Constants

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NetDMSAdvanceWorkItemTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="NetDMSAdvanceWorkItemTask"/> instance to configure.</param>
        public NetDMSAdvanceWorkItemTaskSettingsDialog(NetDMSAdvanceWorkItemTask settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI34880",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34881");
            }
        }

        #endregion Contructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="NetDMSAdvanceWorkItemTask"/> to configure.
        /// </summary>
        /// <value>
        /// The <see cref="NetDMSAdvanceWorkItemTask"/> to configure.
        /// </value>
        public NetDMSAdvanceWorkItemTask Settings
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
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34882");
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

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34883");
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
            ExtractException.Assert("ELI34923",
                "NetDMS: Advance work item settings have not been provided.",
                    Settings != null);

            if (_netDMSConnectionSettingsControl.WarnIfInvalid())
            {
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
