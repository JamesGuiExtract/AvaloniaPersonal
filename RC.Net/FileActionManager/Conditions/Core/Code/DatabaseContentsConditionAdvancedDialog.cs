using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// Allows the advanced settings of a <see cref="DatabaseContentsCondition"/> instance to be configured.
    /// </summary>
    public partial class DatabaseContentsConditionAdvancedDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(DatabaseContentsConditionAdvancedDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContentsConditionAdvancedDialog"/> class.
        /// </summary>
        public DatabaseContentsConditionAdvancedDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContentsConditionAdvancedDialog"/> class.
        /// </summary>
        /// <param name="settings"><see cref="DatabaseContentsCondition"/> for which advanced properties are
        /// to be configured.</param>
        public DatabaseContentsConditionAdvancedDialog(DatabaseContentsCondition settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI36985",
                    _OBJECT_NAME);

                InitializeComponent();
                
                Settings = settings;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36986");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public DatabaseContentsCondition Settings
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
                    if (((IPaginationCondition)Settings).IsPaginationCondition)
                    {
                        _dataFileTextBox.Text = "Not applicable for proposed pagination output yet to be created.";
                        _dataFileTextBox.Enabled = false;
                        _browseButton.Enabled = false;
                        _pathTagsButton.Enabled = false;
                    }
                    else
                    {
                        _dataFileTextBox.Text = Settings.DataFileName;
                    }

                    switch (Settings.ErrorBehavior)
                    {
                        case DatabaseContentsConditionErrorBehavior.Ignore:
                            _ignoreErrorRadioButton.Checked = true;
                            break;

                        case DatabaseContentsConditionErrorBehavior.Log:
                            _logErrorRadioButton.Checked = true;
                            break;

                        case DatabaseContentsConditionErrorBehavior.Abort:
                            _abortOnErrorRadioButton.Checked = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36987");
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
                if (string.IsNullOrWhiteSpace(_dataFileTextBox.Text))
                {
                    _dataFileTextBox.Focus();
                    UtilityMethods.ShowMessageBox(
                            "The data file name to use has not been specified.",
                            "Invalid configuration", true);
                    return;
                }

                Settings.DataFileName = _dataFileTextBox.Text;
                if (_ignoreErrorRadioButton.Checked)
                {
                    Settings.ErrorBehavior = DatabaseContentsConditionErrorBehavior.Ignore;
                }
                else if (_logErrorRadioButton.Checked)
                {
                    Settings.ErrorBehavior = DatabaseContentsConditionErrorBehavior.Log;
                }
                else if (_abortOnErrorRadioButton.Checked)
                {
                    Settings.ErrorBehavior = DatabaseContentsConditionErrorBehavior.Abort;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI36988", ex);
            }
        }

        #endregion Event Handlers
    }
}
