using Extract.FileActionManager.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Email;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// Dialog for displaying and updating the DB UI settings
    /// </summary>
    public partial class FAMDatabaseOptionsDialog : Form
    {
        #region Constants

        // Constants for General tab
        const string _ALLOW_DYNAMIC_TAG_CREATION = "AllowDynamicTagCreation";
        const string _AUTO_CREATE_ACTIONS = "AutoCreateActions";
        const string _AUTO_DELETE_FILE_ACTION_COMMENT = "AutoDeleteFileActionCommentOnComplete";
        const string _AUTO_REVERT_TIME_OUT_IN_MINUTES = "AutoRevertTimeOutInMinutes";
        const string _AUTO_REVERT_NOTIFY_EMAIL_LIST = "AutoRevertNotifyEmailList";
        const string _MIN_TIME_BETWEEN_PROCESSING_DB_CHECK = "MinMillisecondsBetweenCheckForFilesToProcess";
        const string _MAX_TIME_BETWEEN_PROCESSING_DB_CHECK = "MaxMillisecondsBetweenCheckForFilesToProcess";
        const string _ALTERNATE_COMPONENT_DATA_DIR = "AlternateComponentDataDir";

        // Constants for History tab
        const string _UPDATE_FAST_TABLE = "UpdateFileActionStateTransitionTable";
        const string _UPDATE_QUEUE_EVENT_TABLE = "UpdateQueueEventTable";
        const string _STORE_SOURCE_DOC_NAME_CHANGE_HISTORY = "StoreDocNameChangeHistory";
        const string _STORE_DOC_TAG_HISTORY = "StoreDocTagHistory";
        const string _STORE_DB_INFO_HISTORY = "StoreDBInfoChangeHistory";
        const string _STORE_FTP_EVENT_HISTORY = "StoreFTPEventHistory";

        // Constants for Security tab
        const string _REQUIRE_PASSWORD_TO_PROCESS_SKIPPED = "RequirePasswordToProcessAllSkippedFiles";
        const string _REQUIRE_AUTHENTICATION_BEFORE_RUN = "RequireAuthenticationBeforeRun";
        const string _SKIP_AUTHENTICATION_ON_MACHINES = "SkipAuthenticationForServiceOnMachines";
        const string _VERIFICATION_SESSION_TIMEOUT = "VerificationSessionTimeout";
        const string _AZURE_TENNANT = "AzureTenant";
        const string _AZURE_CLIENT_ID = "AzureClientId";
        const string _AZURE_INSTANCE = "AzureInstance";

        // Constants for Product Specific tab
        const string _ID_SHIELD_SCHEMA_VERSION_NAME = "IDShieldSchemaVersion";
        const string _DATA_ENTRY_SCHEMA_VERSION_NAME = "DataEntrySchemaVersion";
        const string _ENABLE_DATA_ENTRY_COUNTERS = "EnableDataEntryCounters";

        // Constant for retrieving the last DB info change
        const string _LAST_DB_INFO_CHANGE = "LastDBInfoChange";

        static readonly char[] _SPLIT_CHARS = new char[] { ',', ';', '|' };

        /// <summary>
        /// Regular expression to validate an email address
        /// </summary>
        static readonly Regex _emailValidate = new Regex(
            @"^[A-Z0-9._%+-]+@(?:[A-Z0-9-]+\.)+[A-Z]{2,4}$", RegexOptions.IgnoreCase);

        // Min and max values for the processing db check for files edit boxes
        const double _MIN_DB_CHECK_FOR_FILES_VALUE = 500.0;
        const double _MAX_DB_CHECK_FOR_FILES_VALUE = 300000.0;

        // Constants for Dashboard tab
        const string _DASHBOARD_INCLUDE_FILTER = "DashboardIncludeFilter";
        const string _DASHBOARD_EXCLUDE_FILTER = "DashboardExcludeFilter";

        const string _PASSWORD_COMPLEXITY_REQUIREMENTS = "PasswordComplexityRequirements";

        const int _VERIFICATION_SESSION_TIMEOUT_DISABLED = 0;
        const int _DEFAULT_VERIFICATION_SESSION_TIMEOUT_IN_MINUTES = 5;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The server to connect to
        /// </summary>
        string _server;

        /// <summary>
        /// The database to connect to
        /// </summary>
        string _database;

        /// <summary>
        /// Collection of setting keys and check boxes
        /// </summary>
        Dictionary<string, CheckBox> _keysToCheckBox;

        /// <summary>
        /// The last settings change time retrieved from the database
        /// </summary>
        DateTime _lastSettingChange;

        /// <summary>
        /// The <see cref="SmtpEmailSettings"/> representing the email settings in the DBInfo table.
        /// </summary>
        SmtpEmailSettings _emailSettings = new SmtpEmailSettings();

        /// <summary>
        /// Indicates whether settings where actually updated or not when
        /// the dialog closed.
        /// </summary>
        public bool SettingsUpdated { get; private set; }

        /// Encoded password complexity requirements
        string _passwordComplexityRequirements;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMDatabaseOptionsDialog"/> class.
        /// </summary>
        public FAMDatabaseOptionsDialog()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMDatabaseOptionsDialog"/> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="database">The database.</param>
        public FAMDatabaseOptionsDialog(string server, string database)
        {
            try
            {
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                InitializeComponent();

                // Set the min and max values for the get files to process interval.
                // Require a min difference of 100 between min and max values so adjust
                // the range accordingly
                _numberMinTimeBetweenChecks.MinimumValue = _MIN_DB_CHECK_FOR_FILES_VALUE;
                _numberMinTimeBetweenChecks.MaximumValue = _MAX_DB_CHECK_FOR_FILES_VALUE - 100.0;
                _numberMaxTimeBetweenChecks.MinimumValue = _MIN_DB_CHECK_FOR_FILES_VALUE + 100.0;
                _numberMaxTimeBetweenChecks.MaximumValue = _MAX_DB_CHECK_FOR_FILES_VALUE;

                // Add the out of range handlers for the min and max times
                _numberMinTimeBetweenChecks.ValueOutOfRange += HandleValueOutOfRange;
                _numberMaxTimeBetweenChecks.ValueOutOfRange += HandleValueOutOfRange;

                _checkSessionTimeout.CheckedChanged += (o, e)
                    => _numberSessionTimeout.Enabled = _checkSessionTimeout.Checked;

                _server = server;
                _database = database;

                _keysToCheckBox = BuildListOfKeysToCheckBoxes();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32308");
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Refreshes the UI from database.
        /// </summary>
        void RefreshUIFromDatabase()
        {
            try
            {
                // Create the database connection and set the server and database name
                FAMDBUtils dbUtils = new FAMDBUtils();
                Type mgrType = Type.GetTypeFromProgID(dbUtils.GetFAMDBProgId());
                FileProcessingDB db = (FileProcessingDB)Activator.CreateInstance(mgrType);

                db.DatabaseServer = _server;
                db.DatabaseName = _database;

                db.ResetDBConnection(false, false);

                var settings = db.DBInfoSettings;

                if (AddMissingSettings(db, settings))
                    settings = db.DBInfoSettings;

                // Use a FAMDatabaseSettings instance to persist the email settings so that the
                // EmailSettingsControl can be re-used to configure the settings.
                _emailSettings.LoadSettings(
                    new FAMDatabaseSettings<ExtractSmtp>(
                        db, false, SmtpEmailSettings.PropertyNameLookup));

                string lastModifyTime = settings.GetValue(_LAST_DB_INFO_CHANGE);
                DateTime lastChange;
                bool logged = false;
                while (!DateTime.TryParse(lastModifyTime, out lastChange))
                {
                    if (!logged)
                    {
                        var ee = new ExtractException("ELI32170",
                            "The " + _LAST_DB_INFO_CHANGE + " value was not a valid time stamp.");
                        ee.AddDebugData("Last Time Stamp", lastModifyTime, false);
                        ee.Log();
                        logged = true;
                    }

                    // Update to a valid time stamp
                    db.ExecuteCommandQuery("UPDATE [DBInfo] SET [Value] = "
                        + "CONVERT(NVARCHAR(MAX), GETDATE(), 21) WHERE "
                        + "[Name] = '" + _LAST_DB_INFO_CHANGE + "'");

                    // Get the modified timestamp
                    lastModifyTime = db.GetDBInfoSetting(_LAST_DB_INFO_CHANGE, true);
                }
                _lastSettingChange = lastChange;

                // Set the object to null so the COM object can be released sooner.
                db = null;

                // Update the standard check box controls
                SetStandardCheckControls(settings);
                SetAutoRevertControls(settings);
                FillSkipMachineList(settings);

                // Update the min and max times between DB checks for files
                _numberMinTimeBetweenChecks.Text =
                    settings.GetValue(_MIN_TIME_BETWEEN_PROCESSING_DB_CHECK);
                _numberMaxTimeBetweenChecks.Text =
                    settings.GetValue(_MAX_TIME_BETWEEN_PROCESSING_DB_CHECK);

                if (!int.TryParse(settings.GetValue(_VERIFICATION_SESSION_TIMEOUT)
                    , out int verificationSessionTimeout))
                {
                    verificationSessionTimeout = _VERIFICATION_SESSION_TIMEOUT_DISABLED;
                }

                bool sessionTimeoutConfigured = verificationSessionTimeout != _VERIFICATION_SESSION_TIMEOUT_DISABLED;
                _checkSessionTimeout.Checked = sessionTimeoutConfigured;
                if (sessionTimeoutConfigured)
                {
                    // DBInfo VerificationSessionTimeout value is stored in seconds; convert to minutes
                    _numberSessionTimeout.Value = Math.Ceiling((decimal)verificationSessionTimeout / 60);
                    _numberSessionTimeout.Enabled = true;
                }
                else
                {
                    _numberSessionTimeout.Value = _DEFAULT_VERIFICATION_SESSION_TIMEOUT_IN_MINUTES;
                    _numberSessionTimeout.Enabled = false;
                }

                _azureClientID.Text =
                    settings.GetValue(_AZURE_CLIENT_ID);
                _azureInstance.Text =
                    settings.GetValue(_AZURE_INSTANCE);
                _azureTenant.Text =
                    settings.GetValue(_AZURE_TENNANT);

                _alternateComponentDataDirectoryTextBox.Text =
                    settings.GetValue(_ALTERNATE_COMPONENT_DATA_DIR);

                // Check for product specific entries
                // There are no longer any settings associated with IDShield.
                bool enableIdShield = false; //settings.Contains(_ID_SHIELD_SCHEMA_VERSION_NAME);
                bool enableDataEntry = settings.Contains(_DATA_ENTRY_SCHEMA_VERSION_NAME);

                // No product specific entries, remove the tab
                if (!enableIdShield && !enableDataEntry)
                {
                    _tabControlSettings.TabPages.Remove(_tabProductSpecific);
                }
                else
                {
                    if (!_tabControlSettings.TabPages.Contains(_tabProductSpecific))
                    {
                        _tabControlSettings.TabPages.Add(_tabProductSpecific);
                    }

                    _groupDataEntry.Visible = enableDataEntry;

                    if (enableDataEntry)
                    {
                        SetCheckControl(settings, _ENABLE_DATA_ENTRY_COUNTERS,
                            _checkDataEntryEnableCounters);
                    }
                }

                _emailSettingsControl.LoadSettings(_emailSettings);

                // Update Dashboard Settings
                textBoxDashboardIncludeFilter.Text = settings.GetValue(_DASHBOARD_INCLUDE_FILTER);
                textBoxDashboardExcludeFilter.Text = settings.GetValue(_DASHBOARD_EXCLUDE_FILTER);

                _passwordComplexityRequirements = settings.Contains(_PASSWORD_COMPLEXITY_REQUIREMENTS)
                    ? settings.GetValue(_PASSWORD_COMPLEXITY_REQUIREMENTS)
                    : "";

                UpdateEnabledStates();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31914");
            }
        }

        static bool AddMissingSettings(FileProcessingDB db, StrToStrMap settings)
        {
            bool added = false;
            // Check for the DashboardSettings
            if (!settings.Contains(_DASHBOARD_INCLUDE_FILTER))
            {
                // Set the include filter and default it to include all
                db.SetDBInfoSetting(_DASHBOARD_INCLUDE_FILTER, ".+", false, true);
                added = true;
            }
            if (!settings.Contains(_DASHBOARD_EXCLUDE_FILTER))
            {
                db.SetDBInfoSetting(_DASHBOARD_EXCLUDE_FILTER, "", false, true);
                added = true;
            }
            return added;
        }

        /// <summary>
        /// Sets the auto revert controls with the values from the database.
        /// </summary>
        /// <param name="settings">The settings.</param>
        void SetAutoRevertControls(StrToStrMap settings)
        {
            string revertTimeout = settings.GetValue(_AUTO_REVERT_TIME_OUT_IN_MINUTES);
            int timeOut = 0;
            if (!int.TryParse(revertTimeout, out timeOut))
            {
                timeOut = 60;
            }

            if (timeOut < 2)
            {
                timeOut = 2;
            }
            else if (timeOut > 1440)
            {
                // Set to 1440 (1 day)
                timeOut = 1440;
            }

            _upDownRevertMinutes.Value = timeOut;

            // Update the list of email addresses
            string autoRevertList = settings.GetValue(_AUTO_REVERT_NOTIFY_EMAIL_LIST);
            _listAutoRevertEmailList.Items.Clear();
            _listAutoRevertEmailList.Items.AddRange(autoRevertList.Split(_SPLIT_CHARS,
                StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Fills the skip machine list.
        /// </summary>
        /// <param name="settings">The settings.</param>
        void FillSkipMachineList(StrToStrMap settings)
        {
            string machineList = settings.GetValue(_SKIP_AUTHENTICATION_ON_MACHINES);
            _listMachinesToAuthenticate.Items.Clear();
            _listMachinesToAuthenticate.Items.AddRange(machineList.Split(_SPLIT_CHARS,
                StringSplitOptions.RemoveEmptyEntries));

        }

        /// <summary>
        /// Updates the check state of the standard db info check controls.
        /// </summary>
        /// <param name="settings">The settings to get the value from.</param>
        void SetStandardCheckControls(StrToStrMap settings)
        {
            foreach (var entry in _keysToCheckBox)
            {
                entry.Value.Checked = settings.GetValue(entry.Key).Equals("1",
                    StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Updates the check state of the control based on the settings value.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="key">The key.</param>
        /// <param name="control">The control.</param>
        static void SetCheckControl(StrToStrMap settings, string key, CheckBox control)
        {
            control.Checked = settings.GetValue(key).Equals("1", StringComparison.Ordinal);
        }

        /// <summary>
        /// Updates the enabled states for all controls.
        /// </summary>
        void UpdateEnabledStates()
        {
            UpdateAutoRevertEnabledState();
            UpdateSkipMachinesEnabledState();
        }

        /// <summary>
        /// Updates the enabled state of the auto revert controls.
        /// </summary>
        void UpdateAutoRevertEnabledState()
        {
            int count = _listAutoRevertEmailList.SelectedItems.Count;

            _buttonModifyEmail.Enabled = count == 1;
            _buttonRemoveEmail.Enabled = count > 0;
        }

        /// <summary>
        /// Updates the enabled state of the skip machines controls.
        /// </summary>
        void UpdateSkipMachinesEnabledState()
        {
            _listMachinesToAuthenticate.Enabled = true;
            _buttonAddMachine.Enabled = true;
            int count = _listMachinesToAuthenticate.SelectedItems.Count;
            _buttonModifyMachine.Enabled = count == 1;
            _buttonRemoveMachine.Enabled = count > 0;
        }

        /// <summary>
        /// Builds the list of keys to check boxes.
        /// </summary>
        /// <returns>A collection of keys and check boxes</returns>
        Dictionary<string, CheckBox> BuildListOfKeysToCheckBoxes()
        {
            var dictionary = new Dictionary<string, CheckBox>();

            dictionary[_ALLOW_DYNAMIC_TAG_CREATION] = _checkAllowdynamicTagCreation;
            dictionary[_AUTO_CREATE_ACTIONS] = _checkAutoCreateActions;
            dictionary[_AUTO_DELETE_FILE_ACTION_COMMENT] = _checkAutoDeleteFileActionComments;
            dictionary[_UPDATE_FAST_TABLE] = _checkStoreFASTHistory;
            dictionary[_UPDATE_QUEUE_EVENT_TABLE] = _checkStoreQueueEventHistory;
            dictionary[_STORE_SOURCE_DOC_NAME_CHANGE_HISTORY] = _checkStoreSourceDocChangeHistory;
            dictionary[_STORE_DOC_TAG_HISTORY] = _checkStoreDocTagHistory;
            dictionary[_REQUIRE_PASSWORD_TO_PROCESS_SKIPPED] = _checkRequirePasswordForSkipped;
            dictionary[_REQUIRE_AUTHENTICATION_BEFORE_RUN] = _checkRequireAuthenticationToRun;

            dictionary[_STORE_DB_INFO_HISTORY] = _checkStoreDBSettingsChangeHistory;
            dictionary[_STORE_FTP_EVENT_HISTORY] = _checkStoreFTPEventHistory;

            return dictionary;
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!string.IsNullOrWhiteSpace(_server) && !string.IsNullOrWhiteSpace(_database))
                {
                    RefreshUIFromDatabase();
                }
                else
                {
                    // No database connection, disable everything but the cancel button
                    _tabControlSettings.Enabled = false;
                    _buttonRefresh.Enabled = false;
                    _buttonRefresh.Visible = false;
                    _buttonOK.Enabled = false;
                    _buttonOK.Visible = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31913");
            }
        }

        /// <summary>
        /// Handles the refresh dialog click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleRefreshDialog(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_server) && !string.IsNullOrWhiteSpace(_database))
                {
                    RefreshUIFromDatabase();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31915");
            }
        }

        /// <summary>
        /// Handles the auto revert interval corrected event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleAutoRevertValueCorrectedEvent(object sender, EventArgs e)
        {
            try
            {
                UtilityMethods.ShowMessageBox(
                    "The auto revert timeout must be between 2 and 1,440 minutes.",
                    "Invalid Timeout", true);

                // Re-select the _upDownRevertMinutes control, but only after any other events
                // in the message queue have been processed so those event don't undo this selection.
                BeginInvoke((MethodInvoker)(() =>
                {
                    try
                    {
                        _tabControlSettings.SelectedTab = _tabGeneral;
                        _upDownRevertMinutes.Focus();
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractDisplay("ELI31916");
                    }
                }));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31917");
            }
        }

        /// <summary>
        /// Handles the ok clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions_RightAlign")]        
        private void HandleOkClicked(object sender, EventArgs e)
        {
            try
            {
                // Validate email settings only if it appears the user has attempted to enter valid settings.
                if (_emailSettingsControl.HasAnySettings && !_emailSettingsControl.ValidateSettings())
                {
                    return;
                }

                // Build a map of settings from the controls
                StrToStrMap map = new StrToStrMap();
                foreach (var entry in _keysToCheckBox)
                {
                    map.Set(entry.Key, entry.Value.Checked ? "1" : "0");
                }

                if (!IsMinimumAndMaximumTimeBetweenChecksValid())
                {
                    return;
                }

                map.Set(_MIN_TIME_BETWEEN_PROCESSING_DB_CHECK, _numberMinTimeBetweenChecks.Text);
                map.Set(_MAX_TIME_BETWEEN_PROCESSING_DB_CHECK, _numberMaxTimeBetweenChecks.Text);

                map.Set(_ALTERNATE_COMPONENT_DATA_DIR,
                    _alternateComponentDataDirectoryTextBox.Text.TrimEnd('\\', '/'));

                map.Set(_AUTO_REVERT_TIME_OUT_IN_MINUTES,
                    _upDownRevertMinutes.Value.ToString(CultureInfo.InvariantCulture));

                string value = _listAutoRevertEmailList.Items.Count > 0 ?
                    string.Join(";", _listAutoRevertEmailList.Items.Cast<string>())
                    : string.Empty;
                map.Set(_AUTO_REVERT_NOTIFY_EMAIL_LIST, value);

                string machines = _listMachinesToAuthenticate.Items.Count > 0 ?
                    string.Join(";", _listMachinesToAuthenticate.Items.Cast<string>()) :
                    string.Empty;
                map.Set(_SKIP_AUTHENTICATION_ON_MACHINES, machines);

                map.Set(_DASHBOARD_INCLUDE_FILTER, textBoxDashboardIncludeFilter.Text);
                map.Set(_DASHBOARD_EXCLUDE_FILTER, textBoxDashboardExcludeFilter.Text);

                // Store DBInfo VerificationSessionTimeout value in seconds
                map.Set(_VERIFICATION_SESSION_TIMEOUT, _checkSessionTimeout.Checked
                    ? (_numberSessionTimeout.Value * 60).ToString(CultureInfo.InvariantCulture)
                    : "0");

                map.Set(_AZURE_TENNANT, _azureTenant.Text);
                map.Set(_AZURE_CLIENT_ID, _azureClientID.Text);
                map.Set(_AZURE_INSTANCE, _azureInstance.Text);

                // Add product specific data if the group boxes are visible

                if (_groupDataEntry.Visible)
                {
                    map.Set(_ENABLE_DATA_ENTRY_COUNTERS,
                        _checkDataEntryEnableCounters.Checked ? "1" : "0");
                }

                map.Set(_PASSWORD_COMPLEXITY_REQUIREMENTS, _passwordComplexityRequirements);

                // Connect to the database and update the settings
                FAMDBUtils dbUtils = new FAMDBUtils();
                Type mgrType = Type.GetTypeFromProgID(dbUtils.GetFAMDBProgId());
                FileProcessingDB db = (FileProcessingDB)Activator.CreateInstance(mgrType);
                db.DatabaseServer = _server;
                db.DatabaseName = _database;
                db.ResetDBConnection(true, false);

                // Check last db info update time
                string lastTime = db.GetDBInfoSetting(_LAST_DB_INFO_CHANGE, true);
                DateTime last = DateTime.Parse(lastTime, CultureInfo.InvariantCulture);
                if (_lastSettingChange < last)
                {
                    UtilityMethods.ShowMessageBox(
                        "The database settings have been modified by another user. "
                        + "Please refresh the configuration window to see latest settings."
                        + Environment.NewLine + "Note: This will reset your changes.",
                        "Settings Modified", false);
                    return;
                }

                SettingsUpdated = db.SetDBInfoSettings(map) > 0;
                db = null;

                // Save the email settings as well.
                _emailSettingsControl.ApplySettings(_emailSettings);
                if (_emailSettings.HasUnsavedChanges)
                {
                    _emailSettings.SaveSettings();
                    SettingsUpdated = true;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31923");
            }
        }

        /// <summary>
        /// Determines whether minimum and maximum time between checks are valid values.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if minimum and maximum time between checks are valid;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        bool IsMinimumAndMaximumTimeBetweenChecksValid()
        {
            if (string.IsNullOrWhiteSpace(_numberMinTimeBetweenChecks.Text))
            {
                UtilityMethods.ShowMessageBox(
                    "The minimum time between checking for files to process cannot be blank.",
                    "Invalid Minimum Time", true);
                _numberMinTimeBetweenChecks.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(_numberMaxTimeBetweenChecks.Text))
            {
                UtilityMethods.ShowMessageBox(
                    "The maximum time between checking for files to process cannot be blank.",
                    "Invalid Maximum Time", true);
                _numberMaxTimeBetweenChecks.Focus();
                return false;
            }

            // Get the min and max times (validate them) from the general tab
            var min = _numberMinTimeBetweenChecks.Int32Value;
            var max = _numberMaxTimeBetweenChecks.Int32Value;
            if (min < _MIN_DB_CHECK_FOR_FILES_VALUE)
            {
                UtilityMethods.ShowMessageBox("The minimum time must be >= "
                    + _MIN_DB_CHECK_FOR_FILES_VALUE.ToString("G", CultureInfo.CurrentCulture),
                    "Minimum Out Of Range", true);
                _numberMinTimeBetweenChecks.Focus();
                return false;
            }
            if (max > _MAX_DB_CHECK_FOR_FILES_VALUE)
            {
                UtilityMethods.ShowMessageBox("The maximum time must be <= "
                    + _MAX_DB_CHECK_FOR_FILES_VALUE.ToString("G", CultureInfo.CurrentCulture),
                    "Maximum Out Of Range", true);
                _numberMaxTimeBetweenChecks.Focus();
                return false;
            }
            if ((max - min) < 0)
            {
                UtilityMethods.ShowMessageBox(
                    "The maximum time between checking for files must be greater than the minimum time.",
                    "Invalid Max And Min", true);
                _numberMaxTimeBetweenChecks.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles the add auto revert email clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleAddAutoRevertEmailClicked(object sender, EventArgs e)
        {
            try
            {
                var response = string.Empty;
                while (string.IsNullOrWhiteSpace(response))
                {
                    if (InputBox.Show(this, "Please enter an email address:", "Add Email Address",
                        ref response) == DialogResult.Cancel)
                    {
                        break;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(response)
                            || !_emailValidate.IsMatch(response))
                        {
                            response = string.Empty;
                            UtilityMethods.ShowMessageBox(
                                "The email address you entered is not valid.",
                                "Invalid Email Address", true);
                        }
                        else
                        {
                            _listAutoRevertEmailList.Items.Add(response);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31924");
            }
        }

        /// <summary>
        /// Handles the modify auto revert email clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleModifyAutoRevertEmailClicked(object sender, EventArgs e)
        {
            try
            {
                int index = _listAutoRevertEmailList.SelectedIndex;
                string item = _listAutoRevertEmailList.Items[index].ToString();
                while (true)
                {
                    string response = item;
                    if (InputBox.Show(this, "Updated email address:", "Update Email Address",
                        ref response) == DialogResult.Cancel)
                    {
                        break;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(response)
                            || !_emailValidate.IsMatch(response))
                        {
                            response = string.Empty;
                            UtilityMethods.ShowMessageBox(
                                "The email address you entered is not valid.",
                                "Invalid Email Address", true);
                        }
                        else
                        {
                            _listAutoRevertEmailList.Items[index] = response;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31925");
            }
        }

        /// <summary>
        /// Handles the auto revert email remove clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleAutoRevertEmailRemoveClicked(object sender, EventArgs e)
        {
            try
            {
                if (_listAutoRevertEmailList.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show("Delete the selected email address(es)?",
                        "Delete Address(es)?", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 0)
                        == DialogResult.Yes)
                    {
                        // Get the indices in descending order
                        var indexes = _listAutoRevertEmailList.SelectedIndices
                            .Cast<int>()
                            .OrderByDescending(x => x)
                            .ToList();

                        // Clear the selection
                        _listAutoRevertEmailList.ClearSelected();

                        // Remove the selected items
                        foreach (var index in indexes)
                        {
                            _listAutoRevertEmailList.Items.RemoveAt(index);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31926");
            }
        }

        /// <summary>
        /// Handles the add machine button clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleAddMachineNameButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var response = string.Empty;
                while (string.IsNullOrWhiteSpace(response))
                {
                    if (InputBox.Show(this, "Please enter a machine name:",
                        "Machines To Skip Authentication", ref response) == DialogResult.Cancel)
                    {
                        break;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(response))
                        {
                            UtilityMethods.ShowMessageBox(
                                "Machine name cannot be blank.",
                                "Invalid Machine Name", true);
                        }
                        else
                        {
                            _listMachinesToAuthenticate.Items.Add(response);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31927");
            }
        }

        /// <summary>
        /// Handles the modify machine name click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleModifyMachineNameClick(object sender, EventArgs e)
        {
            try
            {
                int index = _listMachinesToAuthenticate.SelectedIndex;
                var item = _listMachinesToAuthenticate.Items[index].ToString();

                while (true)
                {
                    var response = item;
                    if (InputBox.Show(this, "Change machine name:",
                        "Machines To Skip Authentication", ref response) == DialogResult.Cancel)
                    {
                        break;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(response))
                        {
                            UtilityMethods.ShowMessageBox(
                                "Machine name cannot be blank.",
                                "Invalid Machine Name", true);
                        }
                        else
                        {
                            _listMachinesToAuthenticate.Items[index] = response;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31928");
            }
        }

        /// <summary>
        /// Handles the remove machines to skip clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleRemoveMachineNamesClicked(object sender, EventArgs e)
        {
            try
            {
                if (_listMachinesToAuthenticate.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show("Delete the selected machine name(s)?",
                        "Delete Machine Name(s)?", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 0)
                        == DialogResult.Yes)
                    {
                        // Get the indices in descending order
                        var indexes = _listMachinesToAuthenticate.SelectedIndices
                            .Cast<int>()
                            .OrderByDescending(x => x)
                            .ToList();

                        // Clear the selection
                        _listMachinesToAuthenticate.ClearSelected();

                        // Remove the selected items
                        foreach (var index in indexes)
                        {
                            _listMachinesToAuthenticate.Items.RemoveAt(index);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31929");
            }
        }

        /// <summary>
        /// Handles the auto revert selection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleAutoRevertSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateAutoRevertEnabledState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31934");
            }
        }

        /// <summary>
        /// Handles the machine to skip selection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleMachineToSkipSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateSkipMachinesEnabledState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31935");
            }
        }

        /// <summary>
        /// Handles the value out of range.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.Utilities.ValueOutOfRangeEventArgs"/> instance containing the event data.</param>
        private void HandleValueOutOfRange(object sender, ValueOutOfRangeEventArgs e)
        {
            try
            {
                var control = sender as NumericEntryTextBox;
                if (control == null)
                {
                    return;
                }

                UtilityMethods.ShowMessageBox(
                    string.Concat("The entered value was out of range. The value must be >= ",
                    e.MinimumValue, " and <= ", e.MaximumValue),
                    "Value Out Of Range", true);
                    
                // Set the value of the text box to the closest value, select it, and set
                // focus to it
                control.Int64Value = (long)e.ClosestValidValue;
                control.SelectAll();
                control.Focus();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32604");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_emailTestButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleEmailTestButton_Click(object sender, EventArgs e)
        {
            try
            {
                _emailSettingsControl.SendTestEmail();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35929");
            }
        }

        /// <summary>
        /// Handles the <see cref="EmailSettingsControl.SettingsChanged"/> event of the
        /// <see cref="_emailSettingsControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleEmailSettingsControl_SettingsChanged(object sender, EventArgs e)
        {
            try
            {
                _emailTestButton.Enabled = _emailSettingsControl.ValidateSettings(doNotDisplayErrors: true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35930");
            }
        }

        /// <summary>
        /// Handles the <see cref="TabControl.SelectedIndexChanged"/> event of the
        /// <see cref="_tabControlSettings"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _emailTestButton.Visible = (_tabControlSettings.SelectedTab == _tabEmail);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35944");
            }
        }

        void HandleConfigurePasswordRequirementsButton_Click(object sender, EventArgs e)
        {
            try
            {
                using PasswordComplexityRequirementsDialog config = new(new(_passwordComplexityRequirements));

                DialogResult result = config.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // Set the field to the encoded requirements (the DB will be updated with this when this dialog closes)
                    _passwordComplexityRequirements = config.Settings.EncodeRequirements();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51873");
            }
        }

        #endregion Event Handlers
    }
}
