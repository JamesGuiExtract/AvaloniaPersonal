using Extract.Database;
using Extract.Licensing;
using Extract.SQLCDBEditor;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Windows.Forms;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// A <see cref="SQLCDBEditorPlugin"/> implementation that is part of the LabDE Configuration
    /// Editor (https://extract.atlassian.net/browse/ISSUE-13603).
    /// This particular tab allows browsing the orders in the database and to initiate a number of
    /// editing operations (add/delete/edit/duplicate order as well as import data).
    /// </summary>
    public partial class OrdersPlugin : SQLCDBEditorPlugin
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(OrdersPlugin).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="DbConnection"/> to use for this plugin.
        /// </summary>
        DbConnection _connection;

        /// <summary>
        /// Indicates whether <see cref="_ordersGridView"/> has been populated and laid out for the
        /// first time.
        /// </summary>
        bool _ordersGridInitialized = false;

        /// <summary>
        /// A right-aligned <see cref="DataGridViewCellStyle"/> to use for numerical columns.
        /// </summary>
        DataGridViewCellStyle _rightAlignedCellStyle;

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
                LicenseUtilities.ValidateLicense(LicenseIdName.LabDECoreObjects, "ELI39344",
                    _OBJECT_NAME);

                InitializeComponent();

                _rightAlignedCellStyle = new DataGridViewCellStyle(_ordersGridView.DefaultCellStyle);
                _rightAlignedCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39345");
            }
        }

        #endregion Constructors

        #region Overrides

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
        /// Gets a value indicating whether the plugin will display the editor provided grid with
        /// data populated with the results of the <see cref="P:Query"/> property.
        /// </summary>
        /// <value><see langword="true"/> if the plugin will display the editor provided grid;
        /// otherwise, <see langword="false"/>.
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
                base.RefreshData();

                RefreshOrdersGrid(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39350");
            }
        }

        /// <summary>
        /// Allows plugin to initialize.
        /// </summary>
        /// <param name="pluginManager">The <see cref="ISQLCDBEditorPluginManager"/> manager for
        /// this plugin.</param>
        /// <param name="connection">The <see cref="DbConnection"/> for use by the plugin.</param>
        public override void LoadPlugin(ISQLCDBEditorPluginManager pluginManager,
            DbConnection connection)
        {
            try
            {
                ExtractException.Assert("ELI39346", "Null argument exception", connection != null);

                _connection = connection;

                _ordersGridView.CurrentCellChanged += HandleOrdersGridView_CurrentCellChanged;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39347");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Layout"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.LayoutEventArgs"/> that contains
        /// the event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            try
            {
                base.OnLayout(e);

                // If LoadPlugin has been called but the grid has not yet been populated and laid
                // out, this is where the initial population should take place.
                if (_connection != null && !_ordersGridInitialized)
                {
                    RefreshOrdersGrid(true);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39348");
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
            }

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="DataGridView.CurrentCellChanged"/> event of
        /// <see cref="_ordersGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOrdersGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            try
            {
                ProcessSelectionChange();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39349");
            }
        }

        /// <summary>
        /// Handles the <see cref="LinkedItemsTextBox.ItemClicked"/> event of the
        /// <see cref="_linkedComponentsTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemClickedEventArgs"/> instance containing the event
        /// data.</param>
        void HandleLinkedComponentsTextBox_ItemClicked(object sender, ItemClickedEventArgs e)
        {
            try
            {
                // This handler will open the component edit dialog fro the selected order once such
                // a dialog exists.
                string code = (string)e.Item;
                UtilityMethods.ShowMessageBox(code, "This is a test", false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39358");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Processes the selection change.
        /// </summary>
        void ProcessSelectionChange()
        {
            int rowIndex = (_ordersGridView.CurrentCell == null)
                ? -1
                : _ordersGridView.CurrentCell.RowIndex;

            if (rowIndex != -1)
            {
                string code = _ordersGridView.Rows[rowIndex].Cells[1].Value.ToString();

                using (DataTable components = DBMethods.ExecuteDBQuery(_connection,
                    "SELECT [LabTest].[TestCode], [LabTest].[OfficialName] " +
                    "   FROM [LabTest] " +
                    "   INNER JOIN [LabOrderTest] ON [LabTest].[TestCode] = [LabOrderTest].[TestCode]" +
                    "   WHERE [OrderCode] = @0",
                    new Dictionary<string, string>() { { "@0", code } }))
                {
                    var componentDictionary = components
                        .AsEnumerable()
                        .ToDictionary(row => row.Field<object>("TestCode"),
                            row => row.Field<string>("OfficialName"));

                    _linkedComponentsTextBox.SetItems(componentDictionary);
                }
            }
        }

        /// <summary>
        /// Updates the contents of <see cref="_ordersGridView"/> with the current order data from
        /// the database.
        /// </summary>
        /// <param name="forceLayout"><see langword="true"/> the grid's layout should be reset
        /// (column sizes, cell formatting, etc); <see langword="false"/> to only update
        /// <see cref="_ordersGridView"/>'s data (not layout).</param>
        void RefreshOrdersGrid(bool forceLayout)
        {
            // NOTE: Implement query selection of Status column, plus update "# LabDE Components"
            // and "# Custom Components" columns to calculate correctly based on that status column.
            string ordersQuery =
                "SELECT " +
                "       CAST('Pending' AS NVARCHAR(10)) AS [Status], " +
                "       [LabOrder].[Code], [LabOrder].[Name], " +
                "       SUM (CASE WHEN ([ComponentToESComponentMap].[ComponentCode] IS NOT NULL " +
                "           AND [ComponentToESComponentMap].[ComponentCode] <> '[CUSTOM]') THEN 1 ELSE 0 END) AS [# LabDE Components], " +
                "       SUM (CASE WHEN ([ComponentToESComponentMap].[ComponentCode] = '[CUSTOM]') THEN 1 ELSE 0 END) AS [# Custom Components], " +
                "       SUM (CASE WHEN ([ComponentToESComponentMap].[ComponentCode] IS NULL) THEN 1 ELSE 0 END) AS [# Unmapped], " +
                "       [LabOrder].[Comment] " +
                "   FROM [LabOrder] " +
                "   INNER JOIN [LabOrderTest] ON [LabOrder].[Code] = [LabOrderTest].[OrderCode] " +
                "   INNER JOIN [ComponentToESComponentMap] ON [LabOrderTest].[TestCode] = [ComponentToESComponentMap].[ComponentCode] " +
                "   GROUP BY [LabOrder].[Code], [LabOrder].[Name], [LabOrder].[Comment] ";

            DBMethods.FormatDataIntoGrid(_connection, ordersQuery, null, _ordersGridView, forceLayout, forceLayout, true,
                new[]
                { 
                    null, // Status
                    new ColumnLayout(width: 75), // Code
                    null, // Name
                    new ColumnLayout(style: _rightAlignedCellStyle), // # LabDE Components
                    new ColumnLayout(style: _rightAlignedCellStyle), // # Custom Components
                    new ColumnLayout(style: _rightAlignedCellStyle), // # Unmapped Components
                    null // Comment
                });

            _ordersGridInitialized = true;
        }

        #endregion Private Members
    }
}