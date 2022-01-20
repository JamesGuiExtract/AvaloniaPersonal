using Extract.Database.Sqlite;
using Extract.Licensing;
using Extract.Utilities.ContextTags.SqliteModels.Version3;
using LinqToDB;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Utilities.ContextTags
{
    /// <summary>
    /// A <see cref="Form"/> that allows editing of a <see cref="CustomTagsDB"/>'s context
    /// table.
    /// </summary>
    public partial class ContextEditingForm : Form
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ContextEditingForm).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="CustomTagsDB"/> to edit.
        /// </summary>
        CustomTagsDB _database;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextEditingForm"/> class.
        /// </summary>
        /// <param name="databaseFile">The path to the database to edit.
        /// </param>
        public ContextEditingForm(string databasePath)
        {
            try
            {
                _ = databasePath ?? throw new ArgumentNullException(nameof(databasePath));

                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI38022",
                    _OBJECT_NAME);

                InitializeComponent();

                _database = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(databasePath));
                _database.BeginTransaction();

                _contextTableBindingSource.DataSource = _database.Contexts.ToList();
                _contextTableBindingSource.ListChanged += ContextTableBindingSource_ListChanged;

                _contextTableDataGridView.UserDeletingRow += ContextTableDataGridView_UserDeletingRow;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38024");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if managed resources should be disposed;
        /// otherwise, <see langword="false"/>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_database != null)
                {
                    _database.Dispose();
                    _database = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_okButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButton_Click(object sender, EventArgs e)
        {
            try 
	        {
                DataGridViewColumn nameColumn = _contextTableDataGridView.Columns["_nameColumn"];
		        DataGridViewColumn fpsFileDirColumn = _contextTableDataGridView.Columns["_fpsFileDirColumn"];

                if (_contextTableDataGridView.Rows.Cast<DataGridViewRow>()
                    .Where(row => !row.IsNewRow)
                    .Select(row => row.Cells[nameColumn.Index])
                    .Any(cell => string.IsNullOrWhiteSpace(cell.Value as string)))
                {
                    UtilityMethods.ShowMessageBox(
                        "Context name must be specified.", "Context name missing", true);
                    DialogResult = DialogResult.None;
                    return;
                }

                if (_contextTableDataGridView.Rows.Cast<DataGridViewRow>()
                    .Where(row => !row.IsNewRow)
                    .Select(row => row.Cells[fpsFileDirColumn.Index])
                    .Any(cell => string.IsNullOrWhiteSpace(cell.Value as string)))
                {
                    UtilityMethods.ShowMessageBox("FPSFileDir must be specified", 
                        "FPSFileDir missing", true);
                    DialogResult = DialogResult.None;
                    return;
                }

                if (_contextTableDataGridView.Rows.Cast<DataGridViewRow>()
                    .Where (row => !row.IsNewRow)
                    .Select(row => row.Cells[fpsFileDirColumn.Index])
                    .Any(cell => !((string)cell.Value).StartsWith(@"\\",
                        StringComparison.OrdinalIgnoreCase)))
                {
                    DialogResult response = MessageBox.Show(null, "It is recommended that " +
                        "FPSFileDir be specified with a UNC path to avoid the risk that a " +
                        "context's FPSFileDir will refer to a different actual location " +
                        "depending on what machine or drive mapping is currently being used." +
                        "\r\n\r\nUse non-UNC path?",
                        "UNC path recommended", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button1, 0);
                    if (response == DialogResult.No)
                    {
                        DialogResult = DialogResult.None;
                        return;
                    }
                }

                try
                {
                    // Add new rows to the database
                    foreach (Context context in _contextTableBindingSource.List)
                    {
                        if (context.ID == 0)
                        {
                            TrimFPSDir(context);
                            _database.Insert(context);
                        }
                    }

                    // Commit all changes
                    _database.CommitTransaction();
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI38025");
                    DialogResult = DialogResult.None;
                }
	        }
	        catch (Exception ex)
	        {
		        ex.ExtractDisplay("ELI38026");
	        }
        }

        /// Delete the context from the database before the row is removed from the bound list
        private void ContextTableDataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            try
            {
                Context context = (Context)e.Row.DataBoundItem;
                _database.Delete(context);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51826");

                // Not sure why this would fail but seems like the delete should be canceled if it does...
                e.Cancel = true;
            }
        }

        /// Trim trailing backslash from FPSFileDir and update the database when an existing row is changed
        private void ContextTableBindingSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            try
            {
                if (e.ListChangedType == ListChangedType.ItemChanged)
                {
                    Context context = (Context)_contextTableBindingSource.List[e.NewIndex];

                    TrimFPSDir(context);

                    if (context.ID > 0)
                    {
                        try
                        {
                            _database.Update(context);
                        }
                        catch (System.Data.SQLite.SQLiteException sqlExn)
                        {
                            sqlExn.ExtractDisplay("ELI51827");

                            // Revert the failed change so that the UI is correct
                            if (_database.Contexts.Find(context.ID) is Context previousVersion)
                            {
                                _contextTableBindingSource.List[e.NewIndex] = previousVersion;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51828");
            }
        }

        #endregion Event Handlers

        #region Private Methods

        // Remove any trailing backslash to ensure as best as possible that paths
        // will match exactly when identifying the current context.
        private static void TrimFPSDir(Context context)
        {
            if (context.FPSFileDir is string dir && dir.EndsWith("\\", StringComparison.Ordinal))
            {
                context.FPSFileDir = dir.Substring(0, dir.Length - 1);
            }
        }

        #endregion Private Methods
    }
}
