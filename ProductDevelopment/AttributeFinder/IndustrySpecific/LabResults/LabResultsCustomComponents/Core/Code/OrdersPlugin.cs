using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Extract.SQLCDBEditor;
using System.Data.SqlServerCe;
using Extract.Licensing;
using Extract.DataEntry.LabDE;
using Extract.Database;
using System.Data.Common;
using Extract.Utilities;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// 
    /// </summary>
    public partial class OrdersPlugin : SQLCDBEditorPlugin
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(SQLCDBEditorPlugin).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// 
        /// </summary>
        ISQLCDBEditorPluginManager _pluginManager;

        /// <summary>
        /// The <see cref="SqlCeConnection"/> for the database to be edited.
        /// </summary>
        SqlCeConnection _connection;

        /// <summary>
        /// 
        /// </summary>
        URSDBConnectionManager _ursConnectionManager;

//        /// <summary>
//        /// The data table representing the table contents or query results.
//        /// </summary>
//        DataTable _resultsTable = new DataTable();
//
//        /// <summary>
//        /// The data adapter to populate <see cref="_resultsTable"/> for database tables.
//        /// </summary>
//        DbDataAdapter _adapter;
//
//        /// <summary>
//        /// 
//        /// </summary>
//        DbCommandBuilder _commandBuilder;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersPlugin"/> class.
        /// </summary>
        public OrdersPlugin()
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
                LicenseUtilities.ValidateLicense(LicenseIdName.LabDECoreObjects, "ELI0",
                    _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI0");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Allows plugin to initialize.
        /// </summary>
        /// <param name="pluginManager">The <see cref="ISQLCDBEditorPluginManager"/> manager for
        /// this plugin.</param>
        /// <param name="connection">The <see cref="SqlCeConnection"/> for use by the plugin.</param>
        public override void LoadPlugin(ISQLCDBEditorPluginManager pluginManager,
            SqlCeConnection connection)
        {
            try
            {
                ExtractException.Assert("ELI0", "Null argument exception", connection != null);

                _pluginManager = pluginManager;
                _connection = connection;
                _ursConnectionManager = new URSDBConnectionManager(connection);

                _pluginManager.DataGrid.Dock = DockStyle.Fill;
                _gridPanel.Controls.Add(_pluginManager.DataGrid);

                _pluginManager.DataGrid.CurrentCellChanged += new EventHandler(DataGrid_CurrentCellChanged);

                _pluginManager.AutoSizeColumns();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI0");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ProcessSelectionChange();
        }

        /// <summary>
        /// Handles the CurrentCellChanged event of the DataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            ProcessSelectionChange();
        }

        /// <summary>
        /// Handles the ItemClicked event of the _HandleLinkedComponentsTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.LabResultsCustomComponents.ItemClickedEventArgs"/> instance containing the event data.</param>
        void _HandleLinkedComponentsTextBox_ItemClicked(object sender, ItemClickedEventArgs e)
        {
            string code = (string)e.Item;
            UtilityMethods.ShowMessageBox(code, "This is a test", false);
        }

        /// <summary>
        /// Processes the selection change.
        /// </summary>
        void ProcessSelectionChange()
        {
            int rowIndex = (_pluginManager == null || 
                            _pluginManager.DataGrid == null ||
                            _pluginManager.DataGrid.CurrentCell == null)
                ? -1
                : _pluginManager.DataGrid.CurrentCell.RowIndex;
            if (rowIndex != -1)
            {
                string code = _pluginManager.DataGrid.Rows[rowIndex].Cells[1].Value.ToString();

                using (DataTable components = DBMethods.ExecuteDBQuery(_connection,
                    "SELECT [LabTest].[TestCode], [LabTest].[OfficialName] " +
                    "   FROM [LabTest] " +
                    "   INNER JOIN [LabOrderTest] ON [LabTest].[TestCode] = [LabOrderTest].[TestCode]" +
                    "   WHERE [OrderCode] = @0",
                    new Dictionary<string, string>() { { "@0", code } }))
                {
                    var componentDictionary = components
                        .Rows
                        .OfType<DataRow>()
                        .ToDictionary(row => row.ItemArray[0], row => row.ItemArray[1].ToString());

                    _LinkedComponentsTextBox.SetItems(componentDictionary);
                }
            }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                return "Orders";
            }
        }

        /// <summary>
        /// If not <see langword="null"/>, results of this query are displayed in a pane above the
        /// plugin control.
        /// </summary>
        public override string Query
        {
            get
            {
                return "SELECT " +
                    "       CAST(1 AS BIT) AS Capture," +
                    "       [LabOrder].[Code], [LabOrder].[Name]," +
                    "       SUM (CASE WHEN ([ComponentToESComponentMap].[ComponentCode] IS NOT NULL AND [ComponentToESComponentMap].[ComponentCode] <> 'WBC') THEN 1 ELSE 0 END) AS [# LabDE Components]," +
                    "       SUM (CASE WHEN ([ComponentToESComponentMap].[ComponentCode] = 'WBC') THEN 1 ELSE 0 END) AS [# Custom Components]," +
                    "       SUM (CASE WHEN ([ComponentToESComponentMap].[ComponentCode] IS NULL) THEN 1 ELSE 0 END) AS [# Unmapped]," +
                    "       [LabOrder].[Comment]" +
                    "   FROM [LabOrder]" +
                    "   INNER JOIN [LabOrderTest] ON [LabOrder].[Code] = [LabOrderTest].[OrderCode]" +
                    "   INNER JOIN [ComponentToESComponentMap] ON [LabOrderTest].[TestCode] = [ComponentToESComponentMap].[ComponentCode]" +
                    "   GROUP BY [LabOrder].[Code], [LabOrder].[Name], [LabOrder].[Comment]";
            }
        }

        /// <summary>
        /// Gets a value indicating whether [display grid].
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if [display grid]; otherwise, <see langword="false"/>.
        /// </value>
        public override bool DisplayGrid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Indicates whether the plugin's <see cref="Control"/> should be displayed in the
        /// <see cref="QueryAndResultsControl"/>.
        /// </summary>
        /// <value><see langword="true"/> if the plugin's control should be displayed;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public override bool DisplayControl
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this plugin's data is valid.
        /// </summary>
        /// <value><see langword="true"/> if the plugin data is valid; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public override bool DataIsValid
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Performs any custom refresh logic needed by the plugin. Generally a plugin where
        /// <see cref="SQLCDBEditorPlugin.ProvidesBindingSource"/> is <see langword="true"/> will
        /// need to perform the refresh of the data here.
        /// </summary>
        public override void RefreshData()
        {
            try
            {
//                if (_resultsTable != null)
//                {
//                    _resultsTable.Dispose();
//                    _resultsTable = null;
//                }

//                _resultsTable = DBMethods.ExecuteDBQuery(_connection,
//                    "SELECT " +
//                    "       CAST(1 AS BIT) AS Capture," +
//                    "       [LabOrder].[Code], [LabOrder].[Name]," +
//                    "       SUM (CASE WHEN ([ComponentToESComponentMap].[ComponentCode] IS NOT NULL AND [ComponentToESComponentMap].[ComponentCode] <> 'WBC') THEN 1 ELSE 0 END) AS [# LabDE Components]," +
//                    "       SUM (CASE WHEN ([ComponentToESComponentMap].[ComponentCode] = 'WBC') THEN 1 ELSE 0 END) AS [# Custom Components]," +
//                    "       SUM (CASE WHEN ([ComponentToESComponentMap].[ComponentCode] IS NULL) THEN 1 ELSE 0 END) AS [# Unmapped]," +
//                    "       [LabOrder].[Comment]" +
//                    "   FROM [LabOrder]" +
//                    "   INNER JOIN [LabOrderTest] ON [LabOrder].[Code] = [LabOrderTest].[OrderCode]" +
//                    "   INNER JOIN [ComponentToESComponentMap] ON [LabOrderTest].[TestCode] = [ComponentToESComponentMap].[ComponentCode]" +
//                    "   GROUP BY [LabOrder].[Code], [LabOrder].[Name], [LabOrder].[Comment]");
//                _dataGridView.DataSource = _resultsTable;

                base.RefreshData();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI0");
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }
                if (_ursConnectionManager != null)
                {
                    _ursConnectionManager.Dispose();
                    _ursConnectionManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        #endregion Event Handlers

        #region Private Members


        #endregion Private Members
    }
}
