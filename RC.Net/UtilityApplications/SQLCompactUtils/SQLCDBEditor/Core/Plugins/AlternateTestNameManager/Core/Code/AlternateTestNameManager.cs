using Extract.Database;
using Extract.Imaging;
using Extract.Imaging.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Extract.SQLCDBEditor.Plugins
{
    /// <summary>
    /// An <see cref="SQLCDBEditor"/> plugin that allows for easy review and approval of candidate
    /// AKAs for LabDE.
    /// </summary>
    public partial class AlternateTestNameManager : SQLCDBEditorPlugin
    {
        #region AKAExample

        /// <summary>
        /// Represents a specific example of where a candidate AKA appeared in a document.
        /// </summary>
        struct AKAExample
        {
            /// <summary>
            /// Gets or sets the name of the file in which the example occurred.
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// Gets or sets the page on the document where the example occurred.
            /// </summary>
            public int Page { get; set; }

            /// <summary>
            /// Gets or sets the X coordinate of the raster zone start point.
            /// </summary>
            public int StartX { get; set; }

            /// <summary>
            /// Gets or sets the Y coordinate of the raster zone start point.
            /// </summary>
            public int StartY { get; set; }

            /// <summary>
            /// Gets or sets the X coordinate of the raster zone end point.
            /// </summary>
            public int EndX { get; set; }

            /// <summary>
            /// Gets or sets the Y coordinate of the raster zone end point.
            /// </summary>
            public int EndY { get; set; }

            /// <summary>
            /// Gets or sets the height of the raster zone.
            /// </summary>
            public int Height { get; set; }
        }

        #endregion AKAExample

        #region Fields

        /// <summary>
        /// The <see cref="ISQLCDBEditorPluginManager"/> for this plugin.
        /// </summary>
        ISQLCDBEditorPluginManager _pluginManager;

        /// <summary>
        /// The <see cref="SqlCeConnection"/> to use for this plugin.
        /// </summary>
        SqlCeConnection _connection;

        /// <summary>
        /// The <see cref="DataRow"/> associated with the currently selected row in the query
        /// results.
        /// </summary>
        DataRow _selectedRow;

        /// <summary>
        /// A list of <see cref="AKAExample"/> associated with the currently selected row in the
        /// query results.
        /// </summary>
        List<AKAExample> _examples = new List<AKAExample>();

        /// <summary>
        /// The index of the currently displayed <see cref="AKAExample"/> from
        /// <see cref="_examples"/>.
        /// </summary>
        int _exampleIndex = 0;

        /// <summary>
        /// The <see cref="Highlight"/> indicating the candidate AKA on the document.
        /// </summary>
        Highlight _exampleHighlight;

        /// <summary>
        /// The <see cref="Button"/> that allows the currently selected candidate AKA to be added as
        /// an official AKA.
        /// </summary>
        Button _addAKAButton;

        /// <summary>
        /// The <see cref="Button"/> that allows the currently selected candidate AKA to be ignored
        /// (not displayed anymore).
        /// </summary>
        Button _ignoreAKAButton;

        /// <summary>
        /// The <see cref="Button"/> that allows all previously ignored AKAs to be re-displayed.
        /// </summary>
        Button _unIgnoreAKAButton;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AlternateTestNameManager"/> class.
        /// </summary>
        public AlternateTestNameManager()
            : base()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the display name of this plugin.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                return "Add AKAs";
            }
        }

        /// <summary>
        /// Gets a query which will return all un-ignored candidate AKAs along with the number of
        /// times the AKAs have appeared in the documents.
        /// </summary>
        public override string Query
        {
            get
            {
                return "SELECT [LabTest].[TestCode], [OfficialName] AS [Official Name], [Name] AS [Candidate AKA], COUNT(*) AS Count " +
                            "FROM [CandidateAlternateTestName] " +
                            "INNER JOIN [LabTest] ON [CandidateAlternateTestName].[TestCode] = [LabTest].[TestCode] " +
                            "GROUP BY [LabTest].[TestCode], [OfficialName], [Name] " +
                            "HAVING MAX(CAST([Ignore] AS INT)) = 0";
            }
        }

        /// <summary>
        /// Gets the plugin pane.
        /// </summary>
        public Control PluginPane
        {
            get
            {
                return this;
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Loads this plugin.
        /// </summary>
        /// <param name="pluginManager">The <see cref="ISQLCDBEditorPluginManager"/> for this
        /// plugin.</param>
        /// <param name="connection">The <see cref="SqlCeConnection"/> this plugin should use to
        /// query/update the database.</param>
        public override void LoadPlugin(ISQLCDBEditorPluginManager pluginManager,
            SqlCeConnection connection)
        {
            try
            {
                _pluginManager = pluginManager;
                _connection = connection;

                _addAKAButton = _pluginManager.GetNewButton();
                _addAKAButton.Text = "Add AKA";
                _addAKAButton.Click += HandleAddAKAButtonClick;

                _ignoreAKAButton = _pluginManager.GetNewButton();
                _ignoreAKAButton.Text = "Ignore AKA";
                _ignoreAKAButton.Click += HandleIgnoreAKAButtonClick;

                _unIgnoreAKAButton = _pluginManager.GetNewButton();
                _unIgnoreAKAButton.Text = "Reset ignore for all";
                _unIgnoreAKAButton.Click += HandleUnIgnoreAKAButtonClick;

                _pluginManager.SelectionChanged += HandlePluginManagerSelectionChanged;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34834");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                // Establish image viewer connections prior to calling base.OnLoad which will
                // potentially remove some IImageViewerControls.
                _imageViewer.EstablishConnections(this);

                base.OnLoad(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34835");
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_exampleHighlight != null)
                {
                    _exampleHighlight.Dispose();
                    _exampleHighlight = null;
                }

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
        /// Handles the add AKA button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAddAKAButtonClick(object sender, EventArgs e)
        {
            try
            {
                // As long as there is a single candidate AKA row selected, add the AKA into the
                // AlternateTestName table, and remove all instances from the
                // CandidateAlternateTestName table.
                if (_selectedRow != null)
                {
                    string testCode = (string)_selectedRow.ItemArray[0];
                    string name = (string)_selectedRow.ItemArray[2];

                    if (MessageBox.Show("Are you sure you wish to add \"" + name +
                        "\" as an AKA for \"" + testCode + "\"?", "Add AKA?",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes)
                    {
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters.Add("@TestCode", testCode);
                        parameters.Add("@Name", name);

                        DBMethods.ExecuteDBQuery(_connection,
                            "INSERT INTO [AlternateTestName] ([TestCode], [Name]) VALUES " +
                                "(@TestCode, @Name)", parameters, "");

                        DBMethods.ExecuteDBQuery(_connection,
                            "DELETE FROM [CandidateAlternateTestName] " +
                            "WHERE [TestCode] = @TestCode AND [Name] = @Name", parameters, "");

                        _pluginManager.RefreshQueryResults();

                        OnDataChanged(true);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34836");
            }
        }

        /// <summary>
        /// Handles the ignore AKA button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleIgnoreAKAButtonClick(object sender, EventArgs e)
        {
            try
            {
                // As long as there is a single candidate AKA row selected, mark all instances in
                // the CandidateAlternateTestName table with the ignore flag to prevent it from
                // being displayed again. (Note: even if additional examples of this AKA are later
                // added to the CandidateAlternateTestName table, they will not be displayed on the
                // basis that at least one instance has the ignore flag set.)
                if (_selectedRow != null)
                {
                    string testCode = (string)_selectedRow.ItemArray[0];
                    string name = (string)_selectedRow.ItemArray[2];

                    if (MessageBox.Show("Are you sure you wish to ingore \"" + name +
                        "\" as an AKA for \"" + testCode + "\"? If ignored this name will not " +
                        "be displayed as an AKA candidate even if additional examples are found.",
                        "Ignore AKA?",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes)
                    {
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters.Add("@TestCode", testCode);
                        parameters.Add("@Name", name);

                        DBMethods.ExecuteDBQuery(_connection,
                            "UPDATE [CandidateAlternateTestName] " +
                            "SET [Ignore] = 1 " +
                            "WHERE [TestCode] = @TestCode AND [Name] = @Name",
                            parameters, "");

                        _pluginManager.RefreshQueryResults();

                        OnDataChanged(true);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34837");
            }
        }

        /// <summary>
        /// Handles the un-ignore AKA button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleUnIgnoreAKAButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you wish to redisplay all AKA candidates that " +
                    "were previously ignored?", "Reset ignore for all?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes)
                {
                    // Clear all ignore flags in the CandidateAlternateTestName table.
                    DBMethods.ExecuteDBQuery(_connection,
                            "UPDATE [CandidateAlternateTestName] SET [Ignore] = 0 ");

                    _pluginManager.RefreshQueryResults();

                    OnDataChanged(true);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34838");
            }
        }     

        /// <summary>
        /// Handles the <see cref="ISQLCDBEditorPluginManager.SelectionChanged"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.SQLCDBEditor.GridSelectionEventArgs"/> instance
        /// containing the event data.</param>
        void HandlePluginManagerSelectionChanged(object sender, GridSelectionEventArgs e)
        {
            try
            {
                // If there is a single row selected, load the examples for the selected candidate
                // AKA.
                _selectedRow = e.SelectedRows
                    .Where(row => e.SelectedRows.Count() == 1)
                    .FirstOrDefault();

                if (_selectedRow != null)
                {
                    // Query for allow rows in CandidateAlternateTestName for this candidate AKA.
                    string testCode = (string)_selectedRow.ItemArray[0];
                    string name = (string)_selectedRow.ItemArray[2];

                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@TestCode", testCode);
                    parameters.Add("@Name", name);

                    string[] examples = DBMethods.ExecuteDBQuery(_connection,
                        "SELECT [Filename], [Page], [StartX], [StartY], [EndX], [EndY], [Height] " +
                            "FROM [CandidateAlternateTestName] " +
                            "WHERE [TestCode] = @TestCode and [Name] = @Name", parameters, "|");

                    _examples.Clear();
                    _exampleIndex = 0;

                    // Create a new AKAExample for each row that has assosciate spatial information
                    // in a document that exists.
                    foreach (string exampleData in examples)
                    {
                        string[] exampleFields = exampleData.Split(new[] { '|' },
                            StringSplitOptions.None);
                        AKAExample example = new AKAExample();
                        example.FileName = exampleFields[0];
                        if (!File.Exists(example.FileName))
                        {
                            // The file name is null or the document doesn't exist. An AKAExample
                            // cannot be created.
                            continue;
                        }

                        int value;
                        if (int.TryParse(exampleFields[1], out value))
                        {
                            example.Page = value;
                        }
                        if (int.TryParse(exampleFields[2], out value))
                        {
                            example.StartX = value;
                        }
                        if (int.TryParse(exampleFields[3], out value))
                        {
                            example.StartY = value;
                        }
                        if (int.TryParse(exampleFields[4], out value))
                        {
                            example.EndX = value;
                        }
                        if (int.TryParse(exampleFields[5], out value))
                        {
                            example.EndY = value;
                        }
                        if (int.TryParse(exampleFields[6], out value))
                        {
                            example.Height = value;
                        }

                        _examples.Add(example);
                    }

                    if (_examples.Count > 0)
                    {
                        // If a least one example was created, show the first one.
                        ShowExample(0);
                    }
                    else
                    {
                        // Otherwise, ensure any previously displayed document is closed.
                        _imageViewer.CloseImage();
                    }

                    // Whether or not any examples are created, the add and ignore AKA buttons
                    // should be enabled.
                    _addAKAButton.Enabled = true;
                    _ignoreAKAButton.Enabled = true;
                }
                else
                {
                    // There is no selection or multiple rows are selected. Clear any examples and
                    // disable the add and ignore AKA buttons.
                    _examples.Clear();
                    _imageViewer.CloseImage();

                    _addAKAButton.Enabled = false;
                    _ignoreAKAButton.Enabled = false;
                    _previousAKAToolStripButton.Enabled = false;
                    _nextAKAToolStripButton.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34839");
            }
        }

        /// <summary>
        /// Handles the previous example click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePreviousExampleClick(object sender, EventArgs e)
        {
            try
            {
                if (_exampleIndex > 0)
                {
                    ShowExample(_exampleIndex - 1);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34840");
            }
        }

        /// <summary>
        /// Handles the next example click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleNextExampleClick(object sender, EventArgs e)
        {
            try
            {
                if (_exampleIndex < (_examples.Count - 1))
                {
                    ShowExample(_exampleIndex + 1);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34841");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Shows the <see cref="AKAExample"/> at the specified <see paramref="index"/> in
        /// <see cref="_examples"/>.
        /// </summary>
        /// <param name="index">The index of <see cref="_examples"/> that should be shown.</param>
        void ShowExample(int index)
        {
            // Remove any previously existing example highlight.
            if (_exampleHighlight != null && _imageViewer.LayerObjects.Contains(_exampleHighlight))
            {
                _imageViewer.LayerObjects.Remove(_exampleHighlight);
                _exampleHighlight = null;
            }

            // Load the document and page for the new example, and create an highlight for it.
            AKAExample example = _examples[index];

            string imageFile = example.FileName;
            _imageViewer.OpenImage(imageFile, false);
            _imageViewer.PageNumber = example.Page;

            // The image viewer's default zoom will be for the full page. But as part of this plugin,
            // the image viewer control is likely to be not very high and wider than it is high.
            // Therefore, make the default zoom level to the image width so that the entire image
            // isn't squeezed into a very small vertical area by default.
            if (_imageViewer.FitMode == FitMode.None)
            {
                _imageViewer.FitMode = FitMode.FitToWidth;
                _imageViewer.FitMode = FitMode.None;
            }

            if (example.EndX != 0 && example.EndY != 0 && example.Height != 0)
            {
                RasterZone rasterZone = new RasterZone(example.StartX, example.StartY, example.EndX, 
                    example.EndY, example.Height, example.Page);

                _exampleHighlight = new Highlight(_imageViewer, "", rasterZone);
                _exampleHighlight.Selectable = false;
                _exampleHighlight.CanRender = false;
                _imageViewer.LayerObjects.Add(_exampleHighlight);

                // Ensure visibility of the new layer object.
                _imageViewer.CenterOnLayerObjects(_exampleHighlight);
            }

            // Enable the next/previous example buttons as appropriate.
            _previousAKAToolStripButton.Enabled = index > 0;
            _nextAKAToolStripButton.Enabled = index < (_examples.Count - 1);
            _exampleIndex = index;

            _imageViewer.Invalidate();
        }

        #endregion Private Members
    }
}
