using Extract.SqlDatabase;
using Extract.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
    [CLSCompliant(false)]
    public partial class EditTableData : Form
    {
        #region Fields

        string _databaseServer;
        string _databaseName;
        string _tableName;
        SqlAppRoleConnection _connection;
        SqlCommand _command;
        SqlDataAdapter _adapter;
        SqlCommandBuilder _builder;
        DataSet _dataSet;
        DataTable _dataTable;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EditTableData"/> class.
        /// </summary>
        /// <param name="databaseServer">The database server name</param>
        /// <param name="databaseName">The database name</param>
        /// <param name="tableName">The table to edit</param>
        public EditTableData(string databaseServer, string databaseName, string tableName)
        {
            InitializeComponent();

            _databaseServer = databaseServer;
            _databaseName = databaseName;
            _tableName = tableName;

            Text = UtilityMethods.FormatInvariant($"Edit {_tableName} table data");
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _connection = new ExtractRoleConnection(_databaseServer, _databaseName);
                _connection.Open();

                var sql = UtilityMethods.FormatInvariant($"SELECT * FROM {_tableName}");
                _command = _connection.CreateCommand();
                _command.CommandText = sql;
                _adapter = new SqlDataAdapter(_command);
                _builder = new SqlCommandBuilder
                {
                    DataAdapter = _adapter
                };
                _dataSet = new DataSet { Locale = CultureInfo.CurrentCulture };
                _adapter.Fill(_dataSet, _tableName);
                _dataTable = _dataSet.Tables[_tableName];

                _dataGridView.DataSource = _dataSet.Tables[_tableName];
                HideColumns(_dataGridView, "ID", "GUID");

                _dataGridView.CellEndEdit += Handle_DataGridView_CellEndEdit;
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI45265").Display();
            }
        }

        #endregion Overrides

        #region Private Methods

        static void HideColumns(DataGridView dg, params string[] columnsToHide)
        {
            var columnNames = dg.Columns.Cast<DataGridViewColumn>().Select(c => c.Name);
            foreach (var name in columnNames.Intersect(columnsToHide, StringComparer.OrdinalIgnoreCase))
            {
                dg.Columns[name].Visible = false;
            }
        }

        #endregion Private Methods

        #region Event Handlers

        /// <summary>
        /// Updates the database. Prevents form closing by setting this form's
        /// <see cref="DialogResult"/> to <see cref="DialogResult.None"/> if
        /// there is an exception.
        /// </summary>
        private void Handle_OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_dataGridView.IsCurrentCellInEditMode)
                {
                    _dataGridView.EndEdit();
                }

                _adapter.Update(_dataTable);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45258");

                DialogResult = DialogResult.None;
            }
        }

        /// <summary>
        /// Clears error icon for row
        /// </summary>
        private void Handle_DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < _dataTable.Rows.Count)
                {
                    var row = _dataTable.Rows[e.RowIndex];
                    if (row.HasErrors)
                    {
                        row.ClearErrors();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45260");
            }
        }

        #endregion Event Handlers
    }
}
