using Extract.Database;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.ComponentModel;
using System.Data.SqlServerCe;
using System.Linq;
using System.Windows.Forms;

namespace Extract.SQLCDBEditor
{
    /// <summary>
    /// A <see cref="Form"/> that allows editing of a <see cref="ContextTagDatabase"/>'s context
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
        /// The <see cref="ContextTagDatabase"/> to edit.
        /// </summary>
        ContextTagDatabase _database;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextEditingForm"/> class.
        /// </summary>
        /// <param name="connection">The <see cref="SqlCeConnection"/> of the database to edit.
        /// </param>
        public ContextEditingForm(SqlCeConnection connection)
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI38022",
                    _OBJECT_NAME);

                ExtractException.Assert("ELI38023", "Null argument exception", connection != null);

                InitializeComponent();

                _database = new ContextTagDatabase(connection);
                _dataGridView.DataSource = _database.Context;
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
                DataGridViewColumn nameColumn = _dataGridView.Columns["_nameColumn"];
		        DataGridViewColumn fpsFileDirColumn = _dataGridView.Columns["_fpsFileDirColumn"];

                if (_dataGridView.Rows.OfType<DataGridViewRow>()
                    .Where(row => !row.IsNewRow)
                    .Select(row => row.Cells[nameColumn.Index])
                    .Any(cell => string.IsNullOrWhiteSpace(cell.Value as string)))
                {
                    UtilityMethods.ShowMessageBox(
                        "Context name must be specified.", "Context name missing", true);
                    DialogResult = DialogResult.None;
                    return;
                }

                if (_dataGridView.Rows.OfType<DataGridViewRow>()
                    .Where(row => !row.IsNewRow)
                    .Select(row => row.Cells[fpsFileDirColumn.Index])
                    .Any(cell => string.IsNullOrWhiteSpace(cell.Value as string)))
                {
                    UtilityMethods.ShowMessageBox("FPSFileDir must be specified", 
                        "FPSFileDir missing", true);
                }

                if (_dataGridView.Rows.OfType<DataGridViewRow>()
                    .Where (row => !row.IsNewRow)
                    .Select(row => row.Cells[fpsFileDirColumn.Index])
                    .Any(cell => !((string)cell.Value).StartsWith(@"\\",
                        StringComparison.OrdinalIgnoreCase)))
                {
                    DialogResult response = MessageBox.Show(null, "It is recommended that " +
                        "FPSFileDir be specified with a UNC path to avoid the risk that a " +
                        "context's FPSFileDir will refer to a different actual location " +
                        "depending on what machine or drive mapping is currently being used.",
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
                    // Remove any trailing backslash to ensure as best as possible that paths
                    // will match exactly when identifying the current context.
                    foreach (var cell in _dataGridView.Rows.OfType<DataGridViewRow>()
                                .Where(row => !row.IsNewRow)
                                .Select(row => row.Cells[fpsFileDirColumn.Index])
                                .Where(cell => ((string)cell.Value).EndsWith(@"\", 
                                    StringComparison.OrdinalIgnoreCase)))
                    {
                        string stringValue = (string)cell.Value;
                        cell.Value = stringValue.Substring(0, stringValue.Length - 1);
                    }

                    _database.SubmitChanges();
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

        #endregion Event Handlers
    }
}
