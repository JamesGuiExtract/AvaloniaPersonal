using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extract.FAMDBCounterManager
{
    /// <summary>
    /// Allows for generation of FAM DB counter update codes.
    /// </summary>
    internal partial class FAMDBCounterManagerForm : Form
    {
        #region Constants

        /// <summary>
        /// The names of the standard FAM DB counters.
        /// </summary>
        internal static Dictionary<int, string> _standardCounterNames = new Dictionary<int, string>()
        {
            { 1, "FLEX Index - Indexing (By Document)" },
            { 2, "FLEX Index - Pagination (By Page)" },
            { 3, "ID Shield - Redaction (By Page)" },
            { 4, "ID Shield - Redaction (By Document)" }
        };

        /// <summary>
        /// The column indices for <see cref="_counterDataGridView"/>.
        /// </summary>
        internal enum CounterGridColumn
        {
            ID = 0,
            Name = 1,
            PreviousValue = 2,
            Operation = 3,
            ApplyValue = 4
        }

        #endregion Constants

        #region DatabaseInfo

        /// <summary>
        /// Represent counter related info for FAM DB secure counters.
        /// </summary>
        internal struct DatabaseInfo
        {
            public Guid DatabaseID;
            public string DatabaseServer;
            public string DatabaseName;
            public DateTime CreationTime;
            public DateTime RestoreTime;
            public DateTime LastCounterUpdateTime;
            public DateTime DateTimeStamp;

            /// <summary>
            /// Initializes a new instance of the <see cref="DatabaseInfo"/> struct from the
            /// provided data.
            /// </summary>
            /// <param name="licenseData">The <see cref="ByteArrayManipulator"/> containing the data
            /// encoding in a license string.</param>
            public DatabaseInfo(ByteArrayManipulator licenseData)
            {
                try
                {
                    DatabaseID = licenseData.ReadGuid();
                    DatabaseServer = licenseData.ReadString();
                    DatabaseName = licenseData.ReadString();
                    CreationTime = licenseData.ReadCTimeAsDateTime();
                    RestoreTime = licenseData.ReadCTimeAsDateTime();
                    LastCounterUpdateTime = licenseData.ReadCTimeAsDateTime();
                    DateTimeStamp = licenseData.ReadCTimeAsDateTime();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38880");
                }
            }
        }

        #endregion DatabaseInfo

        #region Fields

        /// <summary>
        /// Information about the FAM DB associated with the currently pasted license code.
        /// </summary>
        DatabaseInfo _dbInfo;

        /// <summary>
        /// The data for each counter currently displayed in the counter grid.
        /// </summary>
        Dictionary<DataGridViewRow, CounterData> _counterData =
            new Dictionary<DataGridViewRow, CounterData>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMDBCounterManagerForm"/> class.
        /// </summary>
        public FAMDBCounterManagerForm()
        {
            try
            {
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI38856", "FAM DB Counter Manger");

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38857");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// THE METHOD GENERATES CODES FOR TESTING PURPOSES ONLY AND SHOULD BE REMOVED BEFORE RELEASE.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnDoubleClick(EventArgs e)
        {
            try
            {
                base.OnDoubleClick(e);

                var liceneData = new ByteArrayManipulator();
                liceneData.Write(Guid.Parse("9DCEEBE6-8F10-4823-8440-83DB223A3FE5"));
                liceneData.Write("HAWKEYE");
                liceneData.Write("Demo_LabDE");
                liceneData.WriteAsCTime(DateTime.Now - new TimeSpan(365, 0, 0, 0));
                liceneData.WriteAsCTime(DateTime.Now - new TimeSpan(30, 0, 0, 0));
                liceneData.WriteAsCTime(new DateTime(0));
                liceneData.WriteAsCTime(DateTime.Now);
                liceneData.Write((int)1);
                liceneData.Write((int)1);
                liceneData.Write((int)123000);
//                liceneData.Write((int)101);
//                liceneData.Write("Custom ID Shield Counter 1");
//                liceneData.Write((int)0);
//                liceneData.Write((int)102);
//                liceneData.Write("Custom ID Shield Counter 2");
//                liceneData.Write((int)9234000);
//                liceneData.Write((int)103);
//                liceneData.Write("Custom ID Shield Counter 3");
//                liceneData.Write((int)1000);
//                liceneData.Write((int)104);
//                liceneData.Write("Custom ID Shield Counter 4");
//                liceneData.Write((int)2000);

                var encryptedData = NativeMethods.EncryptDecryptBytes(liceneData.GetBytes(8), true);
                string code = encryptedData.ToHexString();

                Clipboard.SetText(code);
                UtilityMethods.ShowMessageBox(code, "Code", false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38858");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_pasteLicenseStringButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePasteLicenseStringButton_Click(object sender, EventArgs e)
        {
            try
            {
                string licenseString = Clipboard.GetText();
                licenseString = Regex.Replace(licenseString, @"\s+", "");

                _licenseStringTextBox.Text = ParseLicenseString(licenseString)
                    ? licenseString
                    : "Invalid license string!";
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38859");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellValueNeeded"/> event of the
        /// <see cref="_counterDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellValueEventArgs"/> instance containing the
        /// event data.</param>
        void HandleCounterDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            try
            {
                // Retrieve the value to display for the indicated cell from _counterData.
                var row = _counterDataGridView.Rows[e.RowIndex];
                CounterData counter = null;
                if (_counterData.TryGetValue(row, out counter))
                {
                    switch ((CounterGridColumn)e.ColumnIndex)
                    {
                        case CounterGridColumn.ID:
                            e.Value = counter.ID;
                            break;

                        case CounterGridColumn.Name:
                            e.Value = counter.Name;
                            break;

                        case CounterGridColumn.PreviousValue:
                            e.Value = (counter.PreviousValue == null)
                                ? "N/A"
                                : string.Format(CultureInfo.CurrentCulture, "{0:n0}", counter.PreviousValue.Value);
                            break;

                        case CounterGridColumn.Operation:
                            e.Value = (counter.Operation == CounterOperation.None)
                                ? null
                                : counter.Operation.ToString();
                            break;

                        case CounterGridColumn.ApplyValue:
                            e.Value = (counter.ApplyValue == null)
                                ? ""
                                : string.Format(CultureInfo.CurrentCulture, "{0:n0}", counter.ApplyValue.Value);
                            break;

                        default:
                            ExtractException.ThrowLogicException("ELI38881");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38861");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellValuePushed"/> event of the
        /// <see cref="_counterDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellValueEventArgs"/> instance containing the
        /// event data.</param>
        void HandleCounterDataGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            try 
	        {	        
                // Apply the value entered into the specified cell into _counterData.
		        var row = _counterDataGridView.Rows[e.RowIndex];
                CounterData counter = null;
                if (_counterData.TryGetValue(row, out counter))
                {
                    var stringValue = (e.Value ?? "").ToString().Trim();

                    switch ((CounterGridColumn)e.ColumnIndex)
                    {
                        case CounterGridColumn.ID:
                            int? counterID = ParseInteger(stringValue, false);
                            if (counterID.HasValue && _counterData.Values
                                .Except(new[] { counter })
                                .Any(other => counterID.Value == other.ID))
                            {
                                throw new ExtractException("ELI38862",
                                    "Cannot use the same ID as an existing counter.");
                            }
                            counter.ID = counterID;
                            break;

                        case CounterGridColumn.Name:
                            if (!stringValue.EndsWith("(By Document)", StringComparison.OrdinalIgnoreCase) &&
                                !stringValue.EndsWith("(By Page)", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new ExtractException("ELI38863",
                                    "Counter name must end with \"(By Document)\" or \"(By Page)\"");
                            }
                            if (_counterData.Values
                                .Except(new[] { counter })
                                .Any(other => 
                                    stringValue.Equals(other.Name, StringComparison.OrdinalIgnoreCase)))
                            {
                                throw new ExtractException("ELI38864",
                                    "Cannot use the same name as an existing counter.");
                            }
                            if (_standardCounterNames
                                    .Values
                                    .Any(standardName =>
                                        standardName.Equals(stringValue, StringComparison.OrdinalIgnoreCase)))
                            {
                                throw new ExtractException("ELI38865",
                                    "Cannot use a standard counter name for a custom counter.");
                            }

                            counter.Name = stringValue;
                            break;

                        case CounterGridColumn.Operation:
                            counter.Operation = stringValue.ToEnumValue<CounterOperation>();
                            if (counter.Operation == CounterOperation.Delete ||
                                counter.Operation == CounterOperation.None)
                            {
                                // An apply value should not be set.
                                counter.ApplyValue = null;
                            }
                            else if (counter.ApplyValue == null)
                            {
                                // An apply value needs to be set; default to zero.
                                counter.ApplyValue = 0;
                            }
                            // Because we my have altered the value in the ApplyValue column,
                            // invalidated the row to force it to be refreshed.
                            _counterDataGridView.InvalidateRow(e.RowIndex);
                            break;

                        case CounterGridColumn.ApplyValue:
                            counter.ApplyValue = ParseInteger(stringValue, true);
                            if (counter.ApplyValue == null &&
                                counter.Operation != CounterOperation.Delete)
                            {

                            }
                            break;

                        default:
                            ExtractException.ThrowLogicException("ELI38882");
                            break;
                    }
                }
	        }
	        catch (Exception ex)
	        {
		        ex.ExtractDisplay("ELI38866");
	        }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.RowsAdded"/> event of the
        /// <see cref="_counterDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewRowsAddedEventArgs"/> instance containing the
        /// event data.</param>
        void HandleCounterDataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            try 
	        {	        
                // For each created row, create a corresponding CounterData instance.
		        for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                {
                    var row = _counterDataGridView.Rows[i];
                    var counter = new CounterData();
                    _counterData[row] = counter;
                }
	        }
	        catch (Exception ex)
	        {
		        ex.ExtractDisplay("ELI38867");
	        }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.EditingControlShowing"/> event of the
        /// <see cref="_counterDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewEditingControlShowingEventArgs"/> instance
        /// containing the event data.</param>
        void HandleCounterDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            try
            {
                // If the editing control is for the operation column, initialize the combo box with
                // the available options for the corresponding counter.
                if (_counterDataGridView.CurrentCell.ColumnIndex == (int)CounterGridColumn.Operation)
                {
                    var row = _counterDataGridView.Rows[_counterDataGridView.CurrentCell.RowIndex];
                    CounterData counter = null;
                    if (_counterData.TryGetValue(row, out counter) && !counter.UserAdded)
                    {
                        var comboBox = (DataGridViewComboBoxEditingControl)e.Control;
                        comboBox.InitializeWithReadableEnum<CounterOperation>(true);
                        // Cannot create an existing counter.
                        comboBox.Items.Remove("Create");
                    }
                    else
                    {
                        // User should not have been allowed to edit the operation for a new
                        // counter. (The operation must be "Create").
                        ExtractException.ThrowLogicException("ELI38868");
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38869");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellBeginEdit"/> event of the
        /// <see cref="_counterDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellCancelEventArgs"/> instance containing
        /// the event data.</param>
        void HandleCounterDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            try
            {
                var row = _counterDataGridView.Rows[e.RowIndex];
                CounterData counter = null;
                if (_counterData.TryGetValue(row, out counter))
                {
                    // Don't allow editing of:
                    // - ID of existing counters
                    // - Name of standard counter
                    // - Name of existing counters
                    // - Previous value
                    // - Operation for new counters
                    // - Value for delete or None.
                    if ((e.ColumnIndex == (int)CounterGridColumn.ID && !counter.UserAdded) ||
                        (e.ColumnIndex == (int)CounterGridColumn.Name && counter.ID < 100) ||
                        (e.ColumnIndex == (int)CounterGridColumn.Name && !counter.UserAdded) ||
                         e.ColumnIndex == (int)CounterGridColumn.PreviousValue ||
                        (e.ColumnIndex == (int)CounterGridColumn.Operation && counter.UserAdded) ||
                        (e.ColumnIndex == (int)CounterGridColumn.ApplyValue && 
                         (counter.Operation == CounterOperation.Delete || counter.Operation == CounterOperation.None)))
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38870");
            }
        }

        /// <summary>
        /// Handles the <see cref="UserDeletingRow"/> event of the
        /// <see cref="_counterDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewRowCancelEventArgs"/> instance containing the
        /// event data.</param>
        void HandleCounterDataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            try
            {
                // Prevent the deletion of rows for counters that already existed in the FAM DB.
                // (If existing counters are to be deleted, they need to be deleted via the delete
                // operation.)
                CounterData counter = null;
                if (_counterData.TryGetValue(e.Row, out counter) && !counter.UserAdded)
                {
                    e.Cancel = true;
                }
                else
                {
                    _counterData.Remove(e.Row);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38871");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_generateCodeButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleGenerateCodeButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Check that all needed info has been specified correctly.
                if (ValidateSettings())
                {
                    // Initialize the license data with general info about the creator and target DB.
                    var licenseData = new ByteArrayManipulator();
                    licenseData.Write(_dbInfo.DatabaseID);
                    licenseData.WriteAsCTime(_dbInfo.LastCounterUpdateTime);
                    licenseData.WriteAsCTime(DateTime.Now);
                    licenseData.Write(Environment.UserName);
                    licenseData.Write(Environment.MachineName);

                    // Get the set of counters for which operations are being applied.
                    var counterUpdates = _counterData
                        .Where(entry => !entry.Key.IsNewRow && entry.Value.Operation != CounterOperation.None)
                        .Select(entry => entry.Value);

                    // Write the data for the counters to update.
                    licenseData.Write(counterUpdates.Count());
                    foreach (var counter in counterUpdates)
                    {
                        licenseData.Write(counter.ID.Value);
                        // Counter name does not need to be included for standard counters.
                        if (counter.ID >= 100)
                        {
                            licenseData.Write(counter.Name);
                        }
                        licenseData.Write((int)counter.Operation);
                        // Apply value does not need to be included for deletions
                        if (counter.Operation != CounterOperation.Delete)
                        {
                            licenseData.Write(counter.ApplyValue.Value);
                        }
                    }

                    var encryptedData = NativeMethods.EncryptDecryptBytes(licenseData.GetBytes(8), true);
                    string code = encryptedData.ToHexString();

                    // TODO: Create a license email using this code.
                    Clipboard.SetText(code);
                    UtilityMethods.ShowMessageBox(code, "Code", false);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38872");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Decrypts and parses the <see paramref="licenseString"/> and uses the data to initialize
        /// the UI with information about the FAM database an its existing secure counters.
        /// </summary>
        /// <param name="licenseString">The encrypted license string parse.</param>
        /// <returns><see langword="true"/> if the license string was successfully parsed;
        /// otherwise, <see langword="false"/>.</returns>
        bool ParseLicenseString(string licenseString)
        {
            try
            {
                var licenseBytes = StringMethods.ConvertHexStringToBytes(licenseString);
                var decryptedBytes = NativeMethods.EncryptDecryptBytes(licenseBytes, false);

                // Retrieve the general information about the FAM DB.
                ByteArrayManipulator licenseData = new ByteArrayManipulator(decryptedBytes);
                _dbInfo = new DatabaseInfo(licenseData);
                _databaseIdTextBox.Text = _dbInfo.DatabaseID.ToString().ToUpperInvariant();
                _databaseServerTextBox.Text = _dbInfo.DatabaseServer;
                _databaseNameTextBox.Text = _dbInfo.DatabaseName;
                _databaseCreationTextBox.Text = ToString(_dbInfo.CreationTime);
                _databaseRestoreTextBox.Text = ToString(_dbInfo.RestoreTime);
                _lastCounterUpdateTextBox.Text = ToString(_dbInfo.LastCounterUpdateTime);
                _dateTimeStampTextBox.Text = ToString(_dbInfo.DateTimeStamp);

                // Retrieve info about the database's existing secure counters.
                _counterDataGridView.Rows.Clear();
                int counterCount = licenseData.ReadInt32();
                for (int i = 0; i < counterCount; i++)
                {
                    CounterData counter = new CounterData(licenseData.ReadInt32());

                    // The license data will not contain the counter name for standard counters.
                    if (counter.ID >= 100)
                    {
                        counter.Name = licenseData.ReadString();
                    }

                    counter.PreviousValue = licenseData.ReadInt32();

                    var index = _counterDataGridView.Rows.Add();
                    DataGridViewRow row = _counterDataGridView.Rows[index];
                    _counterData[row] = counter;
                }
                
                // The license string was successfully parsed; enable the controls.
                _customerNameTextBox.Enabled = true;
                _counterDataGridView.Enabled = true;
                _commentsTextBox.Enabled = true;
                _generateCodeButton.Enabled = true;

                return true;
            }
            catch
            {
                // The license string could not be parsed; clear and disable the controls.
                _databaseServerTextBox.Text = "";
                _databaseNameTextBox.Text = "";
                _databaseIdTextBox.Text = "";
                _databaseCreationTextBox.Text = "";
                _databaseRestoreTextBox.Text = "";
                _dateTimeStampTextBox.Text = "";
                _counterDataGridView.Rows.Clear();

                _customerNameTextBox.Enabled = false;
                _counterDataGridView.Enabled = false;
                _commentsTextBox.Enabled = false;
                _generateCodeButton.Enabled = false;

                return false;
            }
        }

        /// <summary>
        /// Check that all needed info has been specified correctly.
        /// </summary>
        /// <returns><see langword="true"/> if all needed info has been specified correctly; 
        /// otherwise, <see langword="false"/>.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        bool ValidateSettings()
        {
            // Get all counter data except the on associated with the grid's new row.
            var configuredCounters = _counterData
                .Where(entry => !entry.Key.IsNewRow)
                .Select(entry => entry.Value);

            if (string.IsNullOrWhiteSpace(_customerNameTextBox.Text))
            {
                _customerNameTextBox.Focus();
                MessageBox.Show("Please specify the customer.",
                    "Missing customer", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return false;
            }

            if (!configuredCounters.Any(counter => counter.Operation != CounterOperation.None))
            {
                _counterDataGridView.Focus();
                MessageBox.Show("You have not specified any operations.",
                    "No operations defined", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return false;
            }

            if (configuredCounters.Any(counter => !counter.ID.HasValue))
            {
                _counterDataGridView.Focus();
                MessageBox.Show("Please specify an ID for all counters.",
                    "Missing ID", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return false;
            }

            if (configuredCounters.Any(counter => string.IsNullOrWhiteSpace(counter.Name)))
            {
                _counterDataGridView.Focus();
                MessageBox.Show("Please specify a name for all counters.",
                    "Missing name", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return false;
            }

            foreach (CounterData counter in configuredCounters
                .Where(counter => counter.Operation == CounterOperation.Increment ||
                    counter.Operation == CounterOperation.Decrement))
            {
                if (!counter.ApplyValue.HasValue || counter.ApplyValue.Value == 0)
                {
                    _counterDataGridView.Focus();
                    string message = string.Format(CultureInfo.CurrentCulture,
                        "Need a non-zero value to {0} counter \"{1}\".",
                        counter.Operation.ToReadableValue().ToLowerInvariant(), counter.Name);
                    MessageBox.Show(message, "Apply value missing", MessageBoxButtons.OK,
                        MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                    return false;
                }

                if (counter.Operation == CounterOperation.Decrement && 
                    counter.ApplyValue > counter.PreviousValue)
                {
                    _counterDataGridView.Focus();
                    string message = string.Format(CultureInfo.CurrentCulture,
                        "Cannot decrement more counts from \"{0}\" than it currently has.",
                        counter.Name);
                    MessageBox.Show(message, "Invalid decrement count.", MessageBoxButtons.OK,
                        MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the <see paramref="dateTime"/>.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance or "N/A" if the datetime is
        /// zero/empty.
        /// </returns>
        static string ToString(DateTime dateTime)
        {
            return (dateTime.Ticks == 0)
                ? "N/A"
                : dateTime.ToString();
        }

        /// <summary>
        /// Parses and validates a positive integer from the specified <see paramref="stringValue"/>.
        /// </summary>
        /// <param name="stringValue">The string value to parse</param>
        /// <param name="requireValue"><see langword="true"/> if it is required that
        /// <see paramref="stringValue"/> is not empty; <see langword="false"/> if
        /// <see paramref="stringValue"/> may be empty.</param>
        /// <returns>An <see langword="int?"/> with the parsed value or <see langword="null"/> if
        /// <paramref="stringValue"> was empty.</returns>
        static int? ParseInteger(string stringValue, bool requireValue)
        {
            if (!requireValue && string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            uint value = 0;
            if (!UInt32.TryParse(stringValue,
                NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowThousands,
                CultureInfo.CurrentCulture, out value))
            {
                ExtractException ee = new ExtractException("ELI38879",
                    "Value to apply must be a positive integer");
                ee.AddDebugData("Value", stringValue, false);
                throw ee;
            }

            return (int)value;
        }

        #endregion Private Members
    }
}
