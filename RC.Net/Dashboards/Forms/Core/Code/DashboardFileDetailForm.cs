using Extract.AttributeFinder;
using Extract.Imaging.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.Dashboard.Forms
{
    public partial class DashboardFileDetailForm : Form
    {
        #region Public Properties

        /// <summary>
        /// Database server name to connect to
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Database to connect to on the server
        /// </summary>
        public string DatabaseName { get; set; }

        #endregion

        #region Private fields

        /// <summary>
        /// The configuration data to use to display data
        /// </summary>
        GridDetailConfiguration _gridDetailConfiguration;

        /// <summary>
        /// Dictionary that contains name-value pairs for data from the grid row in the dashboard
        /// </summary>
        Dictionary<string, object> _columnValues;

        /// <summary>
        /// Dictionary used to cache the <see cref="XPathContext"/>for each row, so the voa is only loaded once from 
        /// the database
        /// </summary>
        Dictionary<Int64, XPathContext> _attributesByIdAndXPathContext = new Dictionary<long, XPathContext>();

        /// <summary>
        /// Flag to indicate when the grid is being loaded
        /// </summary>
        bool _loading = false;

        /// <summary>
        /// Timer used to populate the grid after the data has been loaded from the database
        /// </summary>
        Timer _loadingTimer;

        /// <summary>
        /// Task that is loading a DataTable with the data to display in the grid
        /// </summary>
        Task<DataTable> _loadingTask;

        #endregion

        #region Constants

        /// <summary>
        /// List of columns that are needed to display highlights, these columns will also be made invisible in the grid
        /// </summary>
        List<string> _columnsRequiredForHighlights = new List<string>
                {
                    "ExpectedSetForFileID",
                    "ExpectedGuid",
                    "ExpectedAttributePath",
                    "FoundSetForFileID",
                    "FoundGuid",
                    "FoundAttributePath"
                };

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the DashboardFileDetailForm
        /// </summary>
        /// <param name="columnValues">Dictionary with name-value pairs containing the data that is on the grid</param>
        /// <param name="serverName">Database server name</param>
        /// <param name="databaseName">Database name</param>
        /// <param name="configuration">Configuration to use</param>
        public DashboardFileDetailForm(Dictionary<string, object> columnValues, string serverName,
            string databaseName, GridDetailConfiguration configuration)
        {
            try
            {
                InitializeComponent();
                if (columnValues.ContainsKey(_gridDetailConfiguration.DataMemberUsedForFileName))
                {
                    _imageViewer.OpenImage((string)columnValues[_gridDetailConfiguration.DataMemberUsedForFileName], false);
                    Text = "Data for " + (string)columnValues[_gridDetailConfiguration.DataMemberUsedForFileName];
                }

                ServerName = serverName;
                DatabaseName = databaseName;
                _gridDetailConfiguration = configuration;
                _columnValues = columnValues;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45723");
            }
        }

        #endregion

        #region Event Overrides

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                _imageViewer.EstablishConnections(this);
                _imageViewer.AllowHighlight = false;
                _imageViewer.ContextMenu = null;

                base.OnLoad(e);

                // Add a handler for the PageChanged event to update the centering on the page
                _imageViewer.PageChanged += HandleImageViewerPageChanged;

                LoadGridData();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45703");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the <see cref="SpatialString"/> for the <see cref="UCLID_AFCORELib.Attribute"/> that is identified by
        /// the given parameters
        /// </summary>
        /// <param name="attributeFileSetID">The ID for the record in the AttributeSetForFile table that has the VOA</param>
        /// <param name="guid">GUID identifier for the <see cref="UCLID_AFCORELib.Attribute"/> to get the <see cref="SpatialString"/></param>
        /// <param name="attributePath">The path to the <see cref="UCLID_AFCORELib.Attribute"/> in the VOA. The path
        /// should use / to separate attribute names. It will be used as XPath query from the root to find the attribute</param>
        /// <returns><see cref="SpatialString"/> from the identified <see cref="UCLID_AFCORELib.Attribute"/></returns>
        SpatialString GetSpatialString(Int64 attributeFileSetID, Guid guid, string attributePath)
        {
            try
            {
                if (!_attributesByIdAndXPathContext.ContainsKey(attributeFileSetID))
                {
                    using (var connection = NewSqlDBConnection())
                    {
                        connection.Open();
                        var cmd = connection.CreateCommand();
                        cmd.CommandTimeout = 60;
                        cmd.CommandText = "SELECT VOA FROM AttributeSetForFile WHERE ID = @AttributeSetForFileID";
                        cmd.Parameters.AddWithValue("@AttributeSetForFileID", attributeFileSetID);
                        using (SqlDataReader voaReader = cmd.ExecuteReader())
                        {
                            int VOAColumn = voaReader.GetOrdinal("VOA");
                            if (voaReader.Read())
                            {
                                Stream voaStream = voaReader.GetStream(VOAColumn);
                                IUnknownVector attributes = AttributeMethods.GetVectorOfAttributesFromSqlBinary(voaStream);

                                _attributesByIdAndXPathContext[attributeFileSetID] = new XPathContext(attributes);
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
                var attributesWithPath = _attributesByIdAndXPathContext[attributeFileSetID].FindAllOfType<IAttribute>("/root/" + attributePath);

                // find the attribute with the guid
                var attribute = attributesWithPath.FirstOrDefault(a => (a as IIdentifiableObject).InstanceGUID == guid);
                return attribute?.Value;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45744");
            }
        }

        /// <summary>
        /// Displays a Highlight on the image
        /// </summary>
        /// <param name="attributeSetId">ID of the AttributeSetForFile record in Database that contains 
        /// the voa that has the attribute being highlighted</param>
        /// <param name="guid">GUID of the Attribute being highlighted</param>
        /// <param name="attributePath">Full path to attribute using attribute names in the voa</param>
        /// <param name="color">Color of the highlight</param>
        void DisplayHighlights(Int64? attributeSetId, Guid? guid, string attributePath, Color color)
        {
            // if any parameters are null there is nothing to display
            if (attributeSetId is null || guid is null || string.IsNullOrWhiteSpace(attributePath))
            {
                return;
            }
            SpatialString spatialString = GetSpatialString((Int64)attributeSetId, (Guid)guid, attributePath);

            // if there is no SpatialString or it doesn't have spatial info there is nothing to display
            if (spatialString is null || !spatialString.HasSpatialInfo())
            {
                return;
            }

            // Create the highlights needed for the SpatialString
            var highlights = _imageViewer.CreateHighlights(spatialString, color);

            // Add highlights to the image
            foreach (var highlight in highlights)
            {
                _imageViewer.LayerObjects.Add(highlight);
            }
        }

        /// <summary>
        /// Loads the data grid using the RowQuery in the configuration data
        /// </summary>
        void LoadGridData()
        {
            try
            {
                // Check if data is already in the process of being loaded
                if (!_loading)
                {
                    _loading = true;

                    DisplayProgress(true);

                    // Setup task to get the data from the database
                    _loadingTask = Task.Run<DataTable>(() =>
                    {
                        using (var connection = NewSqlDBConnection())
                        {
                            connection.Open();

                            var command = connection.CreateCommand();
                            command.CommandTimeout = 0;
                            command.CommandText = _gridDetailConfiguration.RowQuery + " OPTION (RECOMPILE)";

                            // add the column values as parameters for the query
                            foreach (var kp in _columnValues)
                            {
                                command.Parameters.AddWithValue("@" + kp.Key, kp.Value);
                            }
                            var gridDataTable = new DataTable();
                            gridDataTable.Locale = CultureInfo.CurrentCulture;
                            gridDataTable.Load(command.ExecuteReader());
                            return gridDataTable;
                        }
                    });

                    // Set up timer to populate the grid when the task is done
                    _loadingTimer = new Timer();
                    _loadingTimer.Tick += (o, e) =>
                    {
                        try
                        {
                            LoadGridData();
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractLog("ELI46125");
                        }

                    };
                    _loadingTimer.Interval = 1000;
                    _loadingTimer.Start();
                }
                else if (_loadingTask.Status != TaskStatus.Running)
                {
                    _loadingTimer.Stop();

                    DisplayProgress(false);

                    dataGridView.DataSource = _loadingTask.Result;

                    // hide the columns needed for highlights
                    var columnsToHide = dataGridView.Columns.Cast<DataGridViewColumn>()
                        .Where(d => _columnsRequiredForHighlights.Contains(d.Name));
                    foreach (var d in columnsToHide)
                    {
                        d.Visible = false;
                    }

                    SetColumnColors("Expected", Color.Coral);
                    SetColumnColors("Found", Color.Aqua);

                    _loadingTimer = null;
                    _loading = false;
                    _loadingTask = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45705");
            }
        }

        void SetColumnColors(string containsString, Color color)
        {
            var columnsToChange = dataGridView.Columns.Cast<DataGridViewColumn>()
                .Where(d => d.Name.Contains(containsString));
            if (columnsToChange.Count() > 0)
            {
                dataGridView.EnableHeadersVisualStyles = false;
                foreach (var col in columnsToChange)
                {
                    col.HeaderCell.Style.BackColor = color;
                }
            }
        }

        /// <summary>
        /// Returns a connection to the configured database. 
        /// </summary>
        /// <returns>SqlConnection that connects to the <see cref="DatabaseServer"/> and <see cref="DatabaseName"/></returns>
        SqlConnection NewSqlDBConnection()
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = ServerName;
            sqlConnectionBuild.InitialCatalog = DatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

        /// <summary>
        /// Show or Hide the loading progress for the grid
        /// </summary>
        /// <param name="showProgress"></param>
        void DisplayProgress(bool showProgress)
        {
            _loadingLabel.Visible = showProgress;
            _loadProgressBar.Visible = showProgress;

            if (showProgress)
            {
                _loadProgressBar.BringToFront();
                _loadProgressBar.BringToFront();
            }
            else
            {
                _loadingLabel.SendToBack();
                _loadProgressBar.SendToBack();
            }
        }


        #endregion

        #region Event handlers

        void HandleDataGridViewSelectionChanged(object sender, EventArgs e)
        {

            try
            {
                _imageViewer.LayerObjects.Clear();

                // In order to display highlights the results must contain certain columns
                var columnNames = dataGridView.Columns.Cast<DataGridViewColumn>().Select(d => d.Name);
                if (_columnsRequiredForHighlights.All(c => columnNames.Contains(c)))
                {
                    foreach (DataGridViewRow r in dataGridView.SelectedRows)
                    {
                        Int64? id = r.Cells["ExpectedSetForFileID"]?.Value as Int64?;
                        Guid? guid = r.Cells["ExpectedGuid"]?.Value as Guid?;
                        string attributePath = r.Cells["ExpectedAttributePath"]?.Value as string;
                        DisplayHighlights(id, guid, attributePath, Color.Coral);
                        id = r.Cells["FoundSetForFileID"]?.Value as Int64?;
                        if (id != null)
                        {
                            guid = r.Cells["FoundGuid"]?.Value as Guid?;
                            attributePath = r.Cells["FoundAttributePath"]?.Value as string;
                            DisplayHighlights(id, guid, attributePath, Color.Aqua);
                        }
                    }
                }
                if (_imageViewer.LayerObjects.Count() > 0)
                {
                    var pageNumber = _imageViewer.LayerObjects.FirstOrDefault(l => l.PageNumber > 0)?.PageNumber;
                    _imageViewer.PageNumber = pageNumber ?? _imageViewer.PageNumber;
                }
                _imageViewer.Invalidate();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45747");
            }
        }

        void HandleDataGridViewCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                DataGridViewRow row = dataGridView.Rows[e.RowIndex];
                if (row.DataGridView.Columns.Contains("ExpectedOrFound"))
                {
                    row.DefaultCellStyle.BackColor =
                        ((string)row.Cells["ExpectedOrFound"].Value == "Expected") ? Color.LightPink : Color.LightBlue;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45722");
            }
        }

        void HandleImageViewerPageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                if (_imageViewer.LayerObjects.Count() > 0)
                {
                    var objectsToCenter = _imageViewer.LayerObjects.Where(l => l.PageNumber == e.PageNumber).ToArray();
                    if (objectsToCenter.Count() > 0)
                    {
                        _imageViewer.CenterOnLayerObjects(objectsToCenter);
                    }
                }
                _imageViewer.Invalidate();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46127");
            }
        }

        void HandleExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46655");
            }
        }

        #endregion
    }
}

