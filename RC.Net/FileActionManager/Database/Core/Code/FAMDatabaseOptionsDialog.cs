using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;
using UCLID_COMUTILSLib;
using Extract.Utilities;
using System.Globalization;
using Extract.Utilities.Forms;
using System.Text.RegularExpressions;
using Extract.Licensing;

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
        const string _AUTO_REVERT_LOCKED_FILES = "AutoRevertLockedFiles";
        const string _AUTO_REVERT_TIME_OUT_IN_MINUTES = "AutoRevertTimeOutInMinutes";
        const string _AUTO_REVERT_NOTIFY_EMAIL_LIST = "AutoRevertNotifyEmailList";

        // Constants for History tab
        const string _UPDATE_FAST_TABLE = "UpdateFileActionStateTransitionTable";
        const string _UPDATE_QUEUE_EVENT_TABLE = "UpdateQueueEventTable";
        const string _STORE_SOURCE_DOC_NAME_CHANGE_HISTORY = "StoreDocNameChangeHistory";
        const string _STORE_DOC_TAG_HISTORY = "StoreDocTagHistory";
        const string _STORE_FAM_SESSION_HISTORY = "StoreFAMSessionHistory";
        const string _ENABLE_INPUT_EVENT_TRACKING = "EnableInputEventTracking";
        const string _INPUT_EVENT_HISTORY_SIZE = "InputEventHistorySize";
        const string _STORE_DB_INFO_HISTORY = "StoreDBInfoChangeHistory";

        // Constants for Security tab
        const string _REQUIRE_PASSWORD_TO_PROCESS_SKIPPED = "RequirePasswordToProcessAllSkippedFiles";
        const string _REQUIRE_AUTHENTICATION_BEFORE_RUN = "RequireAuthenticationBeforeRun";
        const string _SKIP_AUTHENTICATION_ON_MACHINES = "SkipAuthenticationOnMachines";

        // Constants for Product Specific tab
        const string _ID_SHIELD_SCHEMA_VERSION_NAME = "IDShieldSchemaVersion";
        const string _STORE_IDSHIELD_PROCESSING_HISTORY = "StoreIDShieldProcessingHistory";
        const string _DATA_ENTRY_SCHEMA_VERSION_NAME = "DataEntrySchemaVersion";
        const string _STORE_DATAENTRY_PROCESSING_HISTORY = "StoreDataEntryProcessingHistory";
        const string _ENABLE_DATA_ENTRY_COUNTERS = "EnableDataEntryCounters";

        // Constant for retrieving the last DB info change
        const string _LAST_DB_INFO_CHANGE = "LastDBInfoChange";

        static readonly char[] _SPLIT_CHARS = new char[] { ',', ';', '|' };

        /// <summary>
        /// Regular expression to validate an email address
        /// </summary>
        static readonly Regex _emailValidate = new Regex(
            @"^[A-Z0-9._%+-]+@(?:[A-Z0-9-]+\.)+[A-Z]{2,4}$", RegexOptions.IgnoreCase);

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
        /// Indicates whether settings where actually updated or not when
        /// the dialog closed.
        /// </summary>
        public bool SettingsUpdated { get; private set; }

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
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            InitializeComponent();

            _server = server;
            _database = database;

            _keysToCheckBox = BuildListOfKeysToCheckBoxes();
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
                FileProcessingDB db = new FileProcessingDB();
                db.DatabaseServer = _server;
                db.DatabaseName = _database;

                db.ResetDBConnection();

                var settings = db.DBInfoSettings;

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

                // Set the input event history value
                int dayCount = 0;
                string days = settings.GetValue(_INPUT_EVENT_HISTORY_SIZE);
                if (!int.TryParse(days, out dayCount)
                    || dayCount > 365 || dayCount < 1)
                {
                    dayCount = 30;
                }
                _upDownInputEventHistory.Value = dayCount;

                // Check for product specific entries
                bool enableIdShield = settings.Contains(_ID_SHIELD_SCHEMA_VERSION_NAME);
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

                    _groupIDShield.Visible = enableIdShield;
                    _groupDataEntry.Visible = enableDataEntry;

                    if (enableIdShield)
                    {
                        SetCheckControl(settings, _STORE_IDSHIELD_PROCESSING_HISTORY,
                            _checkIdShieldHistory);
                    }
                    if (enableDataEntry)
                    {
                        SetCheckControl(settings, _STORE_DATAENTRY_PROCESSING_HISTORY,
                            _checkDataEntryHistory);
                        SetCheckControl(settings, _ENABLE_DATA_ENTRY_COUNTERS,
                            _checkDataEntryEnableCounters);
                    }
                }

                UpdateEnabledStates();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31914");
            }
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

            if (timeOut < 5)
            {
                // Set to 5
                timeOut = 5;
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
            UpdateInputTrackingEnabledState();
            UpdateSkipMachinesEnabledState();
        }

        /// <summary>
        /// Updates the enabled state of the auto revert controls.
        /// </summary>
        void UpdateAutoRevertEnabledState()
        {
            bool autoRevert = _checkAutoRevertFiles.Checked;
            int count = autoRevert ? _listAutoRevertEmailList.SelectedItems.Count : 0;

            _upDownRevertMinutes.Enabled = autoRevert;
            _listAutoRevertEmailList.Enabled = autoRevert;
            _buttonAddEmail.Enabled = autoRevert;
            _buttonModifyEmail.Enabled = count == 1;
            _buttonRemoveEmail.Enabled = count > 0;
        }

        /// <summary>
        /// Updates the enabled state of the input tracking controls.
        /// </summary>
        void UpdateInputTrackingEnabledState()
        {
            _upDownInputEventHistory.Enabled = _checkStoreInputEventTracking.Checked;
        }

        /// <summary>
        /// Updates the enabled state of the skip machines controls.
        /// </summary>
        void UpdateSkipMachinesEnabledState()
        {
            bool requireAuthentication = _checkRequireAuthenticationToRun.Checked;
            int count = requireAuthentication ?
                _listMachinesToAuthenticate.SelectedItems.Count : 0;
            _listMachinesToAuthenticate.Enabled = requireAuthentication;
            _buttonAddMachine.Enabled = requireAuthentication;
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
            dictionary[_AUTO_REVERT_LOCKED_FILES] = _checkAutoRevertFiles;
            dictionary[_UPDATE_FAST_TABLE] = _checkStoreFASTHistory;
            dictionary[_UPDATE_QUEUE_EVENT_TABLE] = _checkStoreQueueEventHistory;
            dictionary[_STORE_SOURCE_DOC_NAME_CHANGE_HISTORY] = _checkStoreSourceDocChangeHistory;
            dictionary[_STORE_DOC_TAG_HISTORY] = _checkStoreDocTagHistory;
            dictionary[_ENABLE_INPUT_EVENT_TRACKING] = _checkStoreInputEventTracking;
            dictionary[_REQUIRE_PASSWORD_TO_PROCESS_SKIPPED] = _checkRequirePasswordForSkipped;
            dictionary[_REQUIRE_AUTHENTICATION_BEFORE_RUN] = _checkRequireAuthenticationToRun;
            dictionary[_STORE_DB_INFO_HISTORY] = _checkStoreDBSettingsChangeHistory;

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
                    "The auto revert timeout must be between 5 and 1,440 minutes.",
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
        /// Handles the input event history value corrected event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleInputEventHistoryValueCorrectedEvent(object sender, EventArgs e)
        {
            try
            {
                UtilityMethods.ShowMessageBox(
                    "The number of days to store input event history must be between 1 and 365.",
                    "Invalid Number Of Days", true);

                // Re-select the _upDownInputEventHistory control, but only after any other events
                // in the message queue have been processed so those event don't undo this selection.
                BeginInvoke((MethodInvoker)(() =>
                {
                    try
                    {
                        _tabControlSettings.SelectedTab = _tabHistory;
                        _upDownInputEventHistory.Focus();
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractDisplay("ELI31918");
                    }
                }));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31919");
            }
        }

        /// <summary>
        /// Handles the auto revert check changed event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleAutoRevertCheckChangedEvent(object sender, EventArgs e)
        {
            try
            {
                UpdateAutoRevertEnabledState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31920");
            }
        }

        /// <summary>
        /// Handles the input event history check changed event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleInputEventHistoryCheckChangedEvent(object sender, EventArgs e)
        {
            try
            {
                UpdateInputTrackingEnabledState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31921");
            }
        }

        /// <summary>
        /// Handles the require authentication check changed event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleRequireAuthenticationCheckChangedEvent(object sender, EventArgs e)
        {
            try
            {
                UpdateSkipMachinesEnabledState();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31922");
            }
        }

        /// <summary>
        /// Handles the ok clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleOkClicked(object sender, EventArgs e)
        {
            try
            {
                // Build a map of settings from the controls
                StrToStrMap map = new StrToStrMap();
                foreach (var entry in _keysToCheckBox)
                {
                    map.Set(entry.Key, entry.Value.Checked ? "1" : "0");
                }

                bool autoRevert = _checkAutoRevertFiles.Checked;
                if (autoRevert)
                {
                    map.Set(_AUTO_REVERT_TIME_OUT_IN_MINUTES,
                        _upDownRevertMinutes.Value.ToString(CultureInfo.InvariantCulture));

                    string value = _listAutoRevertEmailList.Items.Count > 0 ?
                        string.Join(";", _listAutoRevertEmailList.Items.Cast<string>())
                        : string.Empty;
                    map.Set(_AUTO_REVERT_NOTIFY_EMAIL_LIST, value);
                }
                if (_checkStoreInputEventTracking.Checked)
                {
                    map.Set(_INPUT_EVENT_HISTORY_SIZE,
                        _upDownInputEventHistory.Value.ToString(CultureInfo.InvariantCulture));
                }
                if (_checkRequireAuthenticationToRun.Checked)
                {
                    string value = _listMachinesToAuthenticate.Items.Count > 0 ?
                        string.Join(";", _listMachinesToAuthenticate.Items.Cast<string>())
                        : string.Empty;
                    map.Set(_SKIP_AUTHENTICATION_ON_MACHINES, value);
                }

                // Add product specific data if the group boxes are visible
                if (_groupIDShield.Visible)
                {
                    map.Set(_STORE_IDSHIELD_PROCESSING_HISTORY,
                        _checkIdShieldHistory.Checked ? "1" : "0");
                }
                if (_groupDataEntry.Visible)
                {
                    map.Set(_STORE_DATAENTRY_PROCESSING_HISTORY,
                        _checkDataEntryHistory.Checked ? "1" : "0");
                    map.Set(_ENABLE_DATA_ENTRY_COUNTERS,
                        _checkDataEntryEnableCounters.Checked ? "1" : "0");
                }

                // Connect to the database and update the settings
                FileProcessingDB db = new FileProcessingDB();
                db.DatabaseServer = _server;
                db.DatabaseName = _database;
                db.ResetDBConnection();

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

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31923");
            }
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

        #endregion Event Handlers
    }
}
