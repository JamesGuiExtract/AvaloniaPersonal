using Extract.Licensing;
using Extract.SQLCDBEditor;
using Extract.Database;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// A <see cref="SQLCDBEditorPlugin"/> implementation that is part of the LabDE Configuration
    /// Editor (https://extract.atlassian.net/browse/ISSUE-13603).
    /// This particular tab allows browsing the result components in the database and to initiate a number of
    /// editing operations (add/delete/edit component)
    /// </summary>
    public partial class ComponentsPlugin : SQLCDBEditorPlugin
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ComponentsPlugin).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// A right-aligned <see cref="DataGridViewCellStyle"/> to use for numerical columns.
        /// </summary>
        DataGridViewCellStyle _rightAlignedCellStyle;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        /// <summary>
        /// The components data source
        /// </summary>
        ComponentsDataSource _componentsDataSource = null;

        /// <summary>
        /// The table that contains the datagridview data.
        /// </summary>
        DataTable _componentsTable = null;

        private bool _componentsGridInitialized;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentsPlugin"/> class.
        /// </summary>
        public ComponentsPlugin()
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
                LicenseUtilities.ValidateLicense(LicenseIdName.LabDECoreObjects, "ELI39320",
                    _OBJECT_NAME);

                InitializeComponent();

                _rightAlignedCellStyle = new DataGridViewCellStyle(_componentsGridView.DefaultCellStyle);
                _rightAlignedCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39321");
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
                return "Components";
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

                RefreshComponentsGrid(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39324");
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
                ExtractException.Assert("ELI39322", "Null argument exception", connection != null);

                _componentsDataSource = new ComponentsDataSource(connection);

                _ordersThatContainComponentLinkLabel.LinkClicked += HandleClick_Link;
                _deleteSelectedComponentsButton.Click += HandleClick_DeleteSelectedComponentsButton;
                _applyFilterButton.Click += HandleClick_ApplyFilterButton;
                _clearFilterButton.Click += HandleClick_ClearFilterButton;
                _componentsGridView.SelectionChanged += HandleSelectionChanged_ComponentsDataGridView;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39323");
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
                if (_componentsDataSource != null && !_componentsGridInitialized)
                {
                    RefreshComponentsGrid(true);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39360");
            }
        }

        /// <overloads>Releases resources used by the <see cref="ComponentsPlugin"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ComponentsPlugin"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_componentsTable != null)
                {
                    _componentsTable.Dispose();
                    _componentsTable = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        private void HandleClick_DeleteSelectedComponentsButton(object sender, EventArgs e)
        {
            try
            {
                if (_componentsGridView.SelectedRows.Count <= 0)
                    return;


                var rowsToDelete = _componentsGridView.SelectedRows.Cast<DataGridViewRow>()
                    .Select(r => ((DataRowView)r.DataBoundItem).Row);

                string[] results = _componentsDataSource.DeleteRows(rowsToDelete);
                string msg = String.Join("\n", results);
                UtilityMethods.ShowMessageBox(msg, "Result of delete operation", false);

                OnDataChanged(true, true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39351");
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the TableDataGridView control. This method updates the
        /// "Orders that contain the component:" text box.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleSelectionChanged_ComponentsDataGridView(object sender, System.EventArgs e)
        {
            try
            {
                UpdateOrdersThatContainComponent();
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI39337").Display();
            }
        }

        /// <summary>
        /// Handles the Click event of the ClearFilterButton. This method clears the
        /// GUI filter text boxes, etc.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleClick_ClearFilterButton(object sender, EventArgs e)
        {
            try
            {
                CodeTextBox.Text = "";
                NameTextBox.Text = "";
                SetMappingStatusComboBoxToDefault();
                _containedInTheOrdersTextBox.Text = "";
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI39368").Display();
            }

        }

        private void SetMappingStatusComboBoxToDefault()
        {
            const int AllItemIndex = 0;
            _mappingStatusComboBox.SelectedIndex = AllItemIndex;
        }

        /// <summary>
        /// Handles the Click event of the ApplyFilterButton. This method updates the
        /// components data grid view.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleClick_ApplyFilterButton(object sender, EventArgs e)
        {
            try
            {
                RefreshComponentsGrid(false);
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI39352").Display();
            }
        }

        /// <summary>
        /// Handles the Click event of the orders containing these components links.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleClick_Link(object sender,
            System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var orderCode = e.Link.LinkData as string;
                UtilityMethods.ShowMessageBox(orderCode, "Order code clicked", false);
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI39353").Display();
            }
        }

        #endregion Event Handlers

        #region Private Members

        // Updates 'orders that contain components' label and links
        private void UpdateOrdersThatContainComponent()
        {
            _ordersThatContainComponentTextBox.Text = "";
            _ordersThatContainComponentLinkLabel.Links.Clear();
            _ordersThatContainComponentLinkLabel.Text = "";
            if (_componentsGridView.SelectedRows.Count <= 0)
            {
                return;
            }
            var selectedRows = _componentsGridView.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => ((DataRowView)r.DataBoundItem).Row);

            IEnumerable<CodeNamePair> components = ComponentsDataSource.GetSelectedComponents(selectedRows);
            IEnumerable<CodeNamePair> orders =
                _componentsDataSource.GetOrdersThatContainComponents(components.Select(c => c.Code));

            string labelPrefix = components.Count() > 1
                ? "Orders that contain the components '"
                : "Orders that contain the component '";

            var labelText = labelPrefix + String.Join("', '", components.Select(c => c.Name + " (" + c.Code + ")")) + "':";

            // If label is too long nothing shows so why not truncate to 1000?
            if (labelText.Length > 1000)
            {
                labelText = labelText.Substring(0, 1000);
            }
            _ordersThatContainComponentTextBox.Text = labelText;

            // Set links
            var linkText = new StringBuilder();
            var links = new List<LinkLabel.Link>(orders.Count());
            foreach (var order in orders.OrderBy(o => o.Name))
            {
                if (linkText.Length > 0)
                {
                    linkText.Append(", ");
                }
                var desc = String.Concat(order.Name, "(", order.Code, ")");
                var link = new LinkLabel.Link(linkText.Length, desc.Length-1, order.Code);
                links.Add(link);
                linkText.Append(desc);
            }
            _ordersThatContainComponentLinkLabel.Text = linkText.ToString();
            foreach (var link in links)
            {
                _ordersThatContainComponentLinkLabel.Links.Add(link);
            }
        }

        /// <summary>
        /// Updates the contents of <see cref="_componentsGridView"/> with the current component data from
        /// the database.
        /// </summary>
        /// <param name="forceLayout"><see langword="true"/> the grid's layout should be reset
        /// (column sizes, cell formatting, etc); <see langword="false"/> to only update
        /// <see cref="_componentsGridView"/>'s data (not layout).</param>
        private void RefreshComponentsGrid(bool forceLayout)
        {
            var oldComponentsTable = _componentsTable;

            ComponentsFilter filter = new ComponentsFilter(CodeTextBox.Text.Trim(),
                                                           NameTextBox.Text.Trim(),
                                                           _containedInTheOrdersTextBox.Text.Trim(),
                                                           (ComponentMappingStatus)_mappingStatusComboBox.SelectedIndex);

            _componentsTable = _componentsDataSource.ComponentsTable(filter);

            DBMethods.FormatDataIntoGrid(_componentsTable, _componentsGridView, forceLayout, forceLayout, true,
            new[]
            { 
                null, // Code
                null, // Name
                null, // MappedTo
                new ColumnLayout(style: _rightAlignedCellStyle), // # LabDE Components
                new ColumnLayout(style: _rightAlignedCellStyle) // # Mapped Components
            });

            if (oldComponentsTable != null)
            {
                oldComponentsTable.Dispose();
            }

            UpdateOrdersThatContainComponent();
            _componentsGridInitialized = true;
        }

        #endregion Private Members
    }
}
    