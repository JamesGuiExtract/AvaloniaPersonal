using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Main form for the FAMDBLockChecker application.
    /// </summary>
    partial class FAMDBLockCheckerForm : Form
    {
        #region Internal Classes

        /// <summary>
        /// Empty class to provide a "typesafe" connection to the LockTable for Linq queries
        /// </summary>
        [Table(Name = "LockTable")]
        abstract class LockTable
        {
        }

        #endregion Internal Classes

        #region Fields

        /// <summary>
        /// Event handle used to signal the DB lock check thread should end.
        /// </summary>
        ManualResetEvent _endThread = new ManualResetEvent(false);

        /// <summary>
        /// Indicates that the thread has ended.
        /// </summary>
        ManualResetEvent _threadEnded = new ManualResetEvent(true);

        /// <summary>
        /// The number of checks that were made.
        /// </summary>
        volatile int _numChecks;

        /// <summary>
        /// The number of locks that were seen.
        /// </summary>
        volatile int _numLocks;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMDBLockCheckerForm"/> class.
        /// </summary>
        public FAMDBLockCheckerForm()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31057", ex);
            }
        }

        #endregion Constructors

        #region Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"/> that contains the event data.</param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Ensure the end thread is set
                _endThread.Set();

                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31055", ex);
            }
        }
        /// <summary>
        /// Handles the check run clicked event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleCheckRunClicked(object sender, System.EventArgs e)
        {
            bool checkState = _checkRun.Checked;
            try
            {
                // Check if going to the running state
                if (checkState)
                {
                    if (string.IsNullOrWhiteSpace(_textServerName.Text))
                    {
                        MessageBox.Show("Server name must be specified.", "No Server",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        _textServerName.Focus();
                    }
                    else if (string.IsNullOrWhiteSpace(_textDatabaseName.Text))
                    {
                        MessageBox.Show("Database name must be specified.", "No Database",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        _textDatabaseName.Focus();
                    }
                    else if (string.IsNullOrWhiteSpace(_numericTextMilliseconds.Text)
                        || _numericTextMilliseconds.Int32Value == 0)
                    {
                        MessageBox.Show("A polling time > 0 must be specified.", "No Interval",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        _numericTextMilliseconds.Focus();
                    }
                    else
                    {
                        // Validate the database connection
                        using (var connection = new SqlConnection(BuildConnectionString()))
                        {
                            connection.Open();
                            connection.Close();
                        }

                        _numChecks = 0;
                        _numLocks = 0;
                        _endThread.Reset();
                        _checkRun.Text = "Stop";
                        var thread = new Thread(CheckDatabaseLock);
                        thread.SetApartmentState(ApartmentState.MTA);
                        thread.Start();
                        return;
                    }

                    _checkRun.Checked = false;
                    return;
                }
                else
                {
                    _checkRun.Text = "Run";
                    _endThread.Set();
                }

                UpdateControlStates(!_checkRun.Checked);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31053", ex);
                _checkRun.Checked = !checkState;
            }
        }

        #endregion Handlers

        #region Methods

        /// <summary>
        /// Builds the database connection string.
        /// </summary>
        /// <returns>The database connection string.</returns>
        string BuildConnectionString()
        {
            var sb = new SqlConnectionStringBuilder();
            sb.DataSource = _textServerName.Text.Trim();
            sb.InitialCatalog = _textDatabaseName.Text.Trim();
            sb.IntegratedSecurity = true;
            return sb.ConnectionString;
        }

        /// <summary>
        /// Updates the control states.
        /// </summary>
        /// <param name="enable">if set to <see langword="true"/> the controls will
        /// be enabled.</param>
        void UpdateControlStates(bool enable)
        {
            _textServerName.Enabled = enable;
            _textDatabaseName.Enabled = enable;
            _numericTextMilliseconds.Enabled = enable;
        }

        /// <summary>
        /// Updates the counts in the UI.
        /// </summary>
        void UpdateCounts()
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)(() => { UpdateCounts(); }));
                return;
            }

            try
            {
                double checks = _numChecks;
                double locks = _numLocks;
                double percentage = locks / checks;
                _textNumberChecks.Text = checks.ToString("G", CultureInfo.CurrentCulture);
                _textLocksSeen.Text = locks.ToString("G", CultureInfo.CurrentCulture);
                _textPercentage.Text = percentage.ToString("P1", CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31052", ex);
            }
        }

        /// <summary>
        /// Displays the exception in the UI thread.
        /// </summary>
        /// <param name="ee">The exception to display.</param>
        void DisplayException(ExtractException ee)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)(() => { DisplayException(ee); }));
                return;
            }

            try
            {
                // Disable the thread if it is running
                if (_checkRun.Checked)
                {
                    _endThread.Set();
                    _checkRun.Checked = false;
                    UpdateControlStates(true);
                }

                ee.Display();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31054", ex);
            }
        }

        /// <summary>
        /// Thread function that checks the database lock.
        /// </summary>
        void CheckDatabaseLock()
        {
            try
            {
                // Thread started, reset ended event
                _threadEnded.Reset();

                var random = new Random();
                var sleepTime = _numericTextMilliseconds.Int32Value;
                sleepTime *= 2;

                using (var connection = new SqlConnection(BuildConnectionString()))
                {
                    connection.Open();
                    do
                    {
                        using (var context = new DataContext(connection))
                        {
                            // Get the count of locks from the lock table
                            var lockTables = context.GetTable<LockTable>();
                            if (lockTables.Count() > 0)
                            {
                                // If there is at least 1 lock, increment count
                                ++_numLocks;
                            }

                            // Always increment the number of checks performed
                            ++_numChecks;

                            // Update the UI with the new counts
                            UpdateCounts();
                        }
                    }
                    while (!_endThread.WaitOne(random.Next(1, sleepTime)));
                }
            }
            catch (ThreadAbortException)
            {
                // Just ignore this exception
            }
            catch (Exception ex)
            {
                DisplayException(ExtractException.AsExtractException("ELI31045", ex));
            }
            finally
            {
                if (_threadEnded != null)
                {
                    _threadEnded.Set();
                }
            }
        }

        #endregion Methods
    }
}
