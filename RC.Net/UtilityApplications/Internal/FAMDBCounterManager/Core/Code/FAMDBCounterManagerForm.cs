using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extract.FAMDBCounterManager
{
    /// <summary>
    /// Provides information about operations to be performed on FAM DB secure counters.
    /// </summary>
    internal struct CounterOperationInfo
    {
        public string Customer;
        public string Comment;
        public string Description;
        public string Code;
    }

    /// <summary>
    /// Represents FAM database info pertaining to secure counters.
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
            DatabaseID = licenseData.ReadGuid();
            DatabaseServer = licenseData.ReadString();
            DatabaseName = licenseData.ReadString();
            CreationTime = licenseData.ReadCTimeAsDateTime();
            RestoreTime = licenseData.ReadCTimeAsDateTime();
            LastCounterUpdateTime = licenseData.ReadCTimeAsDateTime();
            DateTimeStamp = licenseData.ReadCTimeAsDateTime();
        }
    }

    /// <summary>
    /// Allows for generation of FAM DB counter update codes.
    /// </summary>
    internal partial class FAMDBCounterManagerForm : Form, IMessageFilter
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

        #region Fields

        /// <summary>
        /// The license data parsed from a pasted license string.
        /// </summary>
        ByteArrayManipulator _licenseData;

        /// <summary>
        /// The index of the counter data within <see cref="_licenseData"/>.
        /// </summary>
        int _counterDataPos;

        /// <summary>
        /// Indicates whether the secure counter associated with the current license string are in
        /// a good state.
        /// </summary>
        bool _counterDataIsValid;

        /// <summary>
        /// Information about the FAM DB associated with the currently pasted license code.
        /// </summary>
        DatabaseInfo _dbInfo;

        /// <summary>
        /// The data for each counter currently displayed in the counter grid.
        /// </summary>
        Dictionary<DataGridViewRow, CounterData> _counterData =
            new Dictionary<DataGridViewRow, CounterData>();

        /// <summary>
        /// The default style to use for cells.
        /// </summary>
        DataGridViewCellStyle _defaultCellStyle;

        /// <summary>
        /// The style to use for table cells when the table is disabled.
        /// </summary>
        DataGridViewCellStyle _disabledCellStyle;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMDBCounterManagerForm"/> class.
        /// </summary>
        public FAMDBCounterManagerForm()
        {
            InitializeComponent();

            InitializeCellStyles();
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                Application.AddMessageFilter(this);

                UpdateControls(false);
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        #endregion Overrides

        #region IMessageFilter Members

        /// <summary>
        /// Filters out a message before it is dispatched.
        /// </summary>
        /// <param name="m">The message to be dispatched. You cannot modify this message.</param>
        /// <returns>
        /// true to filter the message and stop it from being dispatched; false to allow the message
        /// to continue to the next filter or control.
        /// </returns>
        public bool PreFilterMessage(ref Message m)
        {
            try
            {
                // Override DataGridView shortcut to select whole row when shift-space is entered so
                // that a literal space is added to the active cell. Avoids annoyance where having
                // the shift key down in anticipation of a capital letter causing selection change
                // rather than adding a space.
                if (m.Msg == 0x100 && m.WParam == (IntPtr)Keys.Space &&
                    Control.ModifierKeys.HasFlag(Keys.Shift))
                {
                    var textBox = _counterDataGridView.EditingControl as TextBoxBase;
                    if (textBox != null)
                    {
                        int pos = textBox.SelectionStart;
                        textBox.Text = textBox.Text.Insert(pos, " ");
                        textBox.SelectionStart = pos + 1;
                        textBox.SelectionLength = 0;
                        return true;
                    }
                }
                else if (m.Msg == 0x100 && m.WParam == (IntPtr)Keys.V &&
                    Control.ModifierKeys.HasFlag(Keys.Control) &&
                    ActiveControl == _licenseStringTextBox)
                {
                    ParseLicenseString(Clipboard.GetText());
                }
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }

            return false;
        }

        #endregion IMessageFilter Members

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
                ParseLicenseString(Clipboard.GetText());
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
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
                            throw new Exception("Internal logic error");
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
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
                            int? counterID = stringValue.ParseInteger(false);
                            if (counterID.HasValue && _counterData.Values
                                .Except(new[] { counter })
                                .Any(other => counterID.Value == other.ID))
                            {
                                throw new Exception("Cannot use the same ID as an existing counter.");
                            }
                            counter.ID = counterID;
                            // Because the ID may trigger a standard counter name to be applied,
                            // invalidated the row to force it to be refreshed.
                            _counterDataGridView.InvalidateRow(e.RowIndex);
                            break;

                        case CounterGridColumn.Name:
                            if (!stringValue.EndsWith("(By Document)", StringComparison.OrdinalIgnoreCase) &&
                                !stringValue.EndsWith("(By Page)", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new Exception("Counter name must end with \"(By Document)\" or \"(By Page)\".");
                            }
                            if (_counterData.Values
                                .Except(new[] { counter })
                                .Any(other => 
                                    stringValue.Equals(other.Name, StringComparison.OrdinalIgnoreCase)))
                            {
                                throw new Exception("Cannot use the same name as an existing counter.");
                            }
                            if (_standardCounterNames
                                    .Values
                                    .Any(standardName =>
                                        standardName.Equals(stringValue, StringComparison.OrdinalIgnoreCase)))
                            {
                                throw new Exception("Cannot use a standard counter name for a custom counter.");
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
                            counter.ApplyValue = stringValue.ParseInteger(true);
                            if (counter.ApplyValue == null &&
                                counter.Operation != CounterOperation.Delete)
                            {

                            }
                            break;

                        default:
                            throw new Exception("Internal logic error.");
                    }
                }
	        }
	        catch (Exception ex)
	        {
                ex.ShowMessageBox();
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
                ex.ShowMessageBox();
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
                        throw new Exception("Internal logic error");
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
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
                ex.ShowMessageBox();
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
                ex.ShowMessageBox();
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
                    licenseData.Write(_dbInfo.DatabaseServer);
                    licenseData.Write(_dbInfo.DatabaseName);
                    licenseData.WriteAsCTime(_dbInfo.CreationTime);
                    licenseData.WriteAsCTime(_dbInfo.RestoreTime);
                    licenseData.WriteAsCTime(_dbInfo.LastCounterUpdateTime);
                    licenseData.WriteAsCTime(DateTime.Now);
                    licenseData.Write(Environment.UserName);
                    licenseData.Write(Environment.MachineName);

                    bool unlock = _generateUnlockCodeRadioButton.Checked;

                    // Get the set of counters for which operations are being applied.
                    var counterUpdates = _counterData
                        .Where(entry => !entry.Key.IsNewRow && (unlock || entry.Value.Operation != CounterOperation.None))
                        .Select(entry => entry.Value);

                    // Write the number of counters for which data is being sent; use a negative
                    // number to signify the counters are being unlocked as opposed to updated.
                    licenseData.Write(unlock
                        ? -counterUpdates.Count()
                        : counterUpdates.Count());

                    // Write data pertaining to the specific counters to update/unlock.
                    var description = new StringBuilder(unlock
                        ? "Restore all counters to a working state."
                        :  "");

                    foreach (var counter in counterUpdates)
                    {
                        if (!unlock)
                        {
                            description.Append(string.Format(CultureInfo.CurrentCulture,
                                "- {0} counter \"{1}\"", counter.Operation.ToReadableValue(), counter.Name));
                        }

                        licenseData.Write(counter.ID.Value);
                        // Counter name does not need to be included for standard counters.
                        if (counter.ID >= 100)
                        {
                            licenseData.Write(counter.Name);
                        }

                        if (unlock)
                        {
                            // When unlocking counters, ensure the counter value hasn't changed
                            // prior to unlocking.
                            licenseData.Write((int)counter.Operation);
                            licenseData.Write((int)counter.PreviousValue.Value);
                        }
                        else
                        {
                            licenseData.Write((int)counter.Operation);
                            // Apply value does not need to be included for deletions
                            if (counter.Operation != CounterOperation.Delete)
                            {
                                licenseData.Write(counter.ApplyValue.Value);

                                description.Append(string.Format(CultureInfo.CurrentCulture,
                                    " {0} {1:n0} counts",
                                    (counter.Operation == CounterOperation.Create)
                                        ? "with"
                                        : (counter.Operation == CounterOperation.Set)
                                            ? "to"
                                            : "by",
                                    counter.ApplyValue.Value));
                            }

                            description.AppendLine(".");
                        }
                    }

                    var encryptedData = NativeMethods.EncryptDecryptBytes(licenseData.GetBytes(8), true);
                    string code = encryptedData.ToHexString();

                    using (var emailForm = new EmailForm(_dbInfo, new CounterOperationInfo
                        {
                            Customer = _customerNameTextBox.Text,
                            Comment = _commentsTextBox.Text.Trim(),
                            Description = description.ToString().Trim(),
                            Code = code
                        }))
                    {
                        emailForm.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        /// <summary>
        /// Handles the <see cref="RadioButtonCheckedChanged"/> event of the
        /// <see cref="_generateUpdateCodeRadioButton"/> or <see cref="_generateUnlockCodeRadioButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // When unlock option is selected, ensure grid directly reflects data in the
                // received license code and not any edits the user has made.
                if (_generateUnlockCodeRadioButton.Checked)
                {
                    UpdateCounteGridFromLicenseData();
                }

                UpdateControls(true);
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets or sets a value indicating whether the secure counter associated with the current
        /// license string are in a good state.
        /// </summary>
        /// <value><see langword="true"/> if the counters are in a good state; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool CounterDataIsValid
        {
            get
            {
                return _counterDataIsValid;
            }

            set
            {
                // Disable the HandleRadioButton_CheckedChanged handler while we programmatically
                // change the check state (to prevent recursion in UpdateCounteGridFromLicenseData)
                _generateUnlockCodeRadioButton.CheckedChanged -= HandleRadioButton_CheckedChanged;
                _generateUpdateCodeRadioButton.CheckedChanged -= HandleRadioButton_CheckedChanged;

                try
                {
                    if (value)
                    {
                        _counterStateTextBox.Text = "Valid";
                        _generateUpdateCodeRadioButton.Checked = true;
                    }
                    else
                    {
                        _counterStateTextBox.Text = "Invalid";
                        _generateUnlockCodeRadioButton.Checked = true;
                    }

                    _counterDataIsValid = value;

                    UpdateControls(true);
                }
                finally
                {
                    _generateUnlockCodeRadioButton.CheckedChanged += HandleRadioButton_CheckedChanged;
                    _generateUpdateCodeRadioButton.CheckedChanged += HandleRadioButton_CheckedChanged;
                }
            }
        }

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
                licenseString = Regex.Replace(licenseString, @"\s+", "");
                _licenseStringTextBox.Text = licenseString;

                byte[] licenseBytes = licenseString.HexStringToBytes();
                var decryptedBytes = NativeMethods.EncryptDecryptBytes(licenseBytes, false);

                // Retrieve the general information about the FAM DB.
                _licenseData = new ByteArrayManipulator(decryptedBytes);
                _dbInfo = new DatabaseInfo(_licenseData);
                _databaseIdTextBox.Text = _dbInfo.DatabaseID.ToString().ToUpperInvariant();
                _databaseServerTextBox.Text = _dbInfo.DatabaseServer;
                _databaseNameTextBox.Text = _dbInfo.DatabaseName;
                _databaseCreationTextBox.Text = _dbInfo.CreationTime.DateTimeToString();
                _databaseRestoreTextBox.Text = _dbInfo.RestoreTime.DateTimeToString();
                _lastCounterUpdateTextBox.Text = _dbInfo.LastCounterUpdateTime.DateTimeToString();
                _dateTimeStampTextBox.Text = _dbInfo.DateTimeStamp.DateTimeToString();
                _customerNameTextBox.Text = "";
                _commentsTextBox.Text = "";
                
                _counterDataPos = _licenseData.ReadPosition;
                UpdateCounteGridFromLicenseData();
                
                // The license string was successfully parsed; enable the controls.
                UpdateControls(true);

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
                _lastCounterUpdateTextBox.Text = "";
                _counterStateTextBox.Text = "Invalid license string";
                _dateTimeStampTextBox.Text = "";
                _customerNameTextBox.Text = "";
                _commentsTextBox.Text = "";

                _counterDataGridView.Rows.Clear();
                // Remove all _counterData entries no longer in the table. Note that the entry for
                // the new row may remain despite the clear.
                var removedRows = _counterData.Keys.Where(row => row.DataGridView == null).ToArray();
                foreach (DataGridViewRow row in removedRows)
                {
                    _counterData.Remove(row);
                }

                UpdateControls(false);

                return false;
            }
        }

        /// <summary>
        /// Updates the counter grid from <see cref="_licenseData"/>.
        /// </summary>
        void UpdateCounteGridFromLicenseData()
        {
            _licenseData.ReadPosition = _counterDataPos;
            
            // Retrieve info about the database's existing secure counters.
            _counterDataGridView.Rows.Clear();
            // Remove all _counterData entries no longer in the table. Note that the entry for
            // the new row may remain despite the clear.
            var removedRows = _counterData.Keys.Where(row => row.DataGridView == null).ToArray();
            foreach (DataGridViewRow row in removedRows)
            {
                _counterData.Remove(row);
            }

            int counterCount = _licenseData.ReadInt32();

            // A negative counter count indicates the counters are in an invalid or non-functioning
            // state.
            CounterDataIsValid = counterCount >= 0;
            if (counterCount < 0)
            {
                counterCount = -counterCount;
            }

            for (int i = 0; i < counterCount; i++)
            {
                CounterData counter = new CounterData(_licenseData.ReadInt32());

                // The license data will not contain the counter name for standard counters.
                if (counter.ID >= 100)
                {
                    counter.Name = _licenseData.ReadString();
                }

                counter.PreviousValue = _licenseData.ReadInt32();

                var index = _counterDataGridView.Rows.Add();
                DataGridViewRow row = _counterDataGridView.Rows[index];
                _counterData[row] = counter;
            }
        }

        /// <summary>
        /// Initializes the cell styles used by <see cref="_counterDataGridView"/>.
        /// </summary>
        void InitializeCellStyles()
        {
            _defaultCellStyle = _counterDataGridView.DefaultCellStyle;

            // Use the control color rather than white for the cell backgrounds when disabled.
            _disabledCellStyle = new DataGridViewCellStyle(_defaultCellStyle);
            _disabledCellStyle.SelectionBackColor = SystemColors.Control;
            _disabledCellStyle.BackColor = SystemColors.Control;
            _disabledCellStyle.SelectionForeColor = _defaultCellStyle.ForeColor;
        }

        /// <summary>
        /// Updates the enabled state of UI controls.
        /// </summary>
        /// <param name="enable">Whether the UI controls should be enabled.</param>
        void UpdateControls(bool enable)
        {
            _generateUnlockCodeRadioButton.Enabled = enable && !CounterDataIsValid;
            _generateUpdateCodeRadioButton.Enabled = enable && CounterDataIsValid;
            _customerNameTextBox.Enabled = enable;
            _counterDataGridView.Enabled = enable && _generateUpdateCodeRadioButton.Checked;
            _commentsTextBox.Enabled = enable;
            _generateCodeButton.Enabled = enable;

            var cellStyle = _counterDataGridView.Enabled ? _defaultCellStyle : _disabledCellStyle;

            // Change the table's default cell style base on the enabled status
            _counterDataGridView.DefaultCellStyle = cellStyle;

            // Update the style of all cells currently displayed.
            foreach (DataGridViewRow row in _counterDataGridView.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style = cellStyle;
                }
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
                UtilityMethods.ShowMessageBox("Please specify the customer.",
                    "Missing customer", true);
                return false;
            }

            if (_generateUnlockCodeRadioButton.Checked)
            {
                // No need to validate counter grid for unlock operations.
                return true;
            }

            if (!_generateUpdateCodeRadioButton.Checked ||
                !configuredCounters.Any(counter => counter.Operation != CounterOperation.None))
            {
                _counterDataGridView.Focus();
                UtilityMethods.ShowMessageBox("You have not specified any operations.",
                    "No operations defined", true);
                return false;
            }

            if (configuredCounters.Any(counter => !counter.ID.HasValue))
            {
                _counterDataGridView.Focus();
                UtilityMethods.ShowMessageBox("Please specify an ID for all counters.",
                    "Missing ID", true);
                return false;
            }

            if (configuredCounters.Any(counter => string.IsNullOrWhiteSpace(counter.Name)))
            {
                _counterDataGridView.Focus();
                UtilityMethods.ShowMessageBox("Please specify a name for all counters.",
                    "Missing name", false);
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
                    UtilityMethods.ShowMessageBox(message, "Apply value missing", true);
                    return false;
                }

                if (counter.Operation == CounterOperation.Decrement && 
                    counter.ApplyValue > counter.PreviousValue)
                {
                    _counterDataGridView.Focus();
                    string message = string.Format(CultureInfo.CurrentCulture,
                        "Cannot decrement more counts from \"{0}\" than it currently has.",
                        counter.Name);
                    UtilityMethods.ShowMessageBox(message, "Invalid decrement count.", true);
                    return false;
                }
            }

            return true;
        }

        #endregion Private Members
    }
}
