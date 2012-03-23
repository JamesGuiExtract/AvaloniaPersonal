using Extract.AttributeFinder.Rules;
using Extract.DataEntry;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

namespace Extract.UtilityApplications.ExpressionAndQueryTester
{
    /// <summary>
    /// A form that allows for testing of expressions or DataEntryQueries.
    /// <para><b>Note</b></para>
    /// Since this class makes use of a ScintillNET text box (http://scintillanet.codeplex.com/)
    /// which would require additional steps to comply with its license, this utility should not be
    /// included in any installs and should only be used internally.
    /// </summary>
    public partial class ExpressionAndQueryTesterForm : Form
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ExpressionAndQueryTesterForm).ToString();

        /// <summary>
        /// Expression variables that reference string values will be suffixed with this.
        /// </summary>
        const string _STRING_VALUE = "__STRING__";

        #endregion Constants

        #region Fields

        /// <summary>
        /// An <see cref="AFUtility"/> instance (used for attribute queries).
        /// </summary>
        AFUtility _afUtility;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionAndQueryTesterForm"/> class.
        /// </summary>
        public ExpressionAndQueryTesterForm()
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI34458", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34459");
            }
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the case that the user changed which type of test to perform (expression vs
        /// query).
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTestTypeCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_testExpressionRadioButton.Checked)
                {
                    _targetAttributesLabel.Text = "Attribute(s) to score";
                    _targetOrRootAttributesInfoTip.TipText =
                        "Leave blank to score all attribute or specify an attribute query to \r\n" +
                        "specify a subset of attributes to score.";
                    _rsdOrDbFilenameLabel.Text = "RSD Filename";
                    _rsdOrDbFileNameBrowseButton.FileFilter =
                        "Ruleset definition files (*.rsd;*.rsd.etf)|*.rsd;*.rsd.etf";
                    _expressionOrQueryScintillaBox.ConfigurationManager.Language = "cs";
                }
                else
                {
                    _targetAttributesLabel.Text = "Root attribute";
                    _targetOrRootAttributesInfoTip.TipText =
                        "If a root attribute is needed, specify the attribute query to select it.\r\n" +
                        "If the query matches multiple attributes, the first match will be used.";
                    _rsdOrDbFilenameLabel.Text = "DataEntry DB Filename";
                    _rsdOrDbFileNameBrowseButton.FileFilter = "SQL CE Database (*.sdf)|*.sdf";
                    _expressionOrQueryScintillaBox.ConfigurationManager.Language = "xml";
                }

                // Apply the new configuration of the expression/query text box.
                _expressionOrQueryScintillaBox.ConfigurationManager.Configure();
                _expressionOrQueryScintillaBox.Lexing.Colorize();

                // Clear an existing results
                _resultsDataGridView.DataSource = null;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34460");
            }
        }

        /// <summary>
        /// Handles the text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleTextChanged(object sender, EventArgs e)
        {
            try
            {
                bool enableTestButton =
                        !string.IsNullOrWhiteSpace(_voaFileNameTextBox.Text) &&
                        !string.IsNullOrWhiteSpace(_expressionOrQueryScintillaBox.Text) &&
                        (!_testExpressionRadioButton.Checked ||
                         !string.IsNullOrWhiteSpace(_rsdOrDbFileNameTextBox.Text));

                _testButton.Enabled = enableTestButton;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34466");
            }
        }

        /// <summary>
        /// Handles the clear result button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleClearResultButtonClick(object sender, EventArgs e)
        {
            try
            {
                _resultsDataGridView.DataSource = null;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34461");
            }
        }

        /// <summary>
        /// Handles the test button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTestButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Load the attribute data from the VOA file.
                string voaFileName = _voaFileNameTextBox.Text;
                ExtractException.Assert("ELI34462", "VOA file not found", File.Exists(voaFileName));

                IUnknownVector sourceAttributes = new IUnknownVector();
                sourceAttributes.LoadFrom(voaFileName, false);

                // Perform the selected type of test.
                if (_testExpressionRadioButton.Checked)
                {
                    TestExpression(sourceAttributes);
                }
                else
                {
                    TestQuery(sourceAttributes);
                }

                // Size the last column appropriately.
                DataGridViewColumn lastColumn = _resultsDataGridView.Columns.GetLastColumn(
                        DataGridViewElementStates.Visible, DataGridViewElementStates.None);
                lastColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34464");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets an <see cref="AFUtility"/> instance (used for attribute queries)
        /// </summary>
        AFUtility AFUtility
        {
            get
            {
                if (_afUtility == null)
                {
                    _afUtility = new AFUtility();
                }

                return _afUtility;
            }
        }

        /// <summary>
        /// Tests the expression.
        /// </summary>
        /// <param name="sourceAttributes">The attributes to test against.</param>
        void TestExpression(IUnknownVector sourceAttributes)
        {
            string expression = _expressionOrQueryScintillaBox.Text;
            string targetAttributeQuery = _targetAttributeTextBox.Text;

            string rsdFileName = _rsdOrDbFileNameTextBox.Text;
            ExtractException.Assert("ELI34463", "RSD file not found", File.Exists(rsdFileName));

            // If not blank, limit the attributes tested to the onces selected by targetAttributeQuery.
            if (!string.IsNullOrWhiteSpace(targetAttributeQuery))
            {
                sourceAttributes = AFUtility.QueryAttributes(
                    sourceAttributes, targetAttributeQuery, false);
            }

            // Use RSDDataScorer to evaluate the expression (to guarantee the same results as the
            // RSDDataScorer would obtain.
            Dictionary<string, object> variables;
            int score = RSDDataScorer.EvaluateExpression(expression, rsdFileName, sourceAttributes,
                out variables);

            // Format the variable used in evaluating the expression in to a list of a 2 column
            // string list (sorted by variable name).
            var resultData = variables
                .Where(item => item.Key.Contains(_STRING_VALUE))
                .OrderBy(item => item.Key)
                .SelectMany(item =>
                    ((IEnumerable<string>)item.Value).Select(listItem =>
                        new { Name = item.Key.Replace(_STRING_VALUE, ""), Value = listItem }))
                .ToList();

            // Insert into the first row the overall score.
            resultData.Insert(0,
                new { Name = "Overall score", Value = score.ToString(CultureInfo.InvariantCulture) });

            _resultsDataGridView.DataSource = resultData;
        }

        /// <summary>
        /// Tests the query.
        /// </summary>
        /// <param name="sourceAttributes">The attributes to test against.</param>
        void TestQuery(IUnknownVector sourceAttributes)
        {
            string queryText = _expressionOrQueryScintillaBox.Text;

            string dbFileName = _rsdOrDbFileNameTextBox.Text;
            if (!string.IsNullOrWhiteSpace(dbFileName))
            {
                ExtractException.Assert("ELI34463", "RSD file not found", File.Exists(dbFileName));
            }

            // If not blank, limit make the root attribute for the query be the first attribute
            // matching rootAttributeQuery
            IAttribute rootAttribute = null;
            string rootAttributeQuery = _targetAttributeTextBox.Text;
            if (!string.IsNullOrWhiteSpace(rootAttributeQuery))
            {
                rootAttribute = (IAttribute)AFUtility.QueryAttributes(
                   sourceAttributes, rootAttributeQuery, false)
                   .ToIEnumerable<IAttribute>()
                   .First();
            }

            // Use the first attribute to obtain the source document name that the results are associated
            // with.
            IAttribute firstAttribute =
                sourceAttributes.ToIEnumerable<IAttribute>()
                .Where(attribute => attribute.Value != null)
                .FirstOrDefault();

            using (DbConnection dbConnection = string.IsNullOrEmpty(dbFileName)
                ? null : GetDatabaseConnection(dbFileName))
            {
                if (dbConnection != null)
                {
                    dbConnection.Open();
                }

                // Initialize data and execute the query
                AttributeStatusInfo.ResetData(firstAttribute.Value.SourceDocName,
                    sourceAttributes, dbConnection);
                InitializeAttributes(sourceAttributes);

                DataEntryQuery query =
                    DataEntryQuery.Create(queryText, rootAttribute, dbConnection,
                        MultipleQueryResultSelectionMode.List, true);

                QueryResult result = query.Evaluate();

                // Must select as a list of anonymous types with a named property mapped to
                // the string value for it to show up in the list.
                _resultsDataGridView.DataSource = result.ToStringArray()
                    .Select(value => new { Value = value })
                    .ToList();
            }
        }

        /// <summary>
        /// Gets the database connection.
        /// </summary>
        /// <param name="databaseFileName">Name of the database file.</param>
        /// <returns></returns>
        static DbConnection GetDatabaseConnection(string databaseFileName)
        {
            string connectionString = "Data Source='" + databaseFileName + "';";

            return new SqlCeConnection(connectionString);
        }

        /// <summary>
        /// Initializes the attributes.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        static void InitializeAttributes(IUnknownVector attributes)
        {
            int attributeCount = attributes.Size();
            for (int i = 0; i < attributeCount; i++)
            {
                IAttribute attribute = (IAttribute)attributes.At(i);
                AttributeStatusInfo.Initialize(attribute, attributes, null);

                InitializeAttributes(attribute.SubAttributes);
            }
        }

        #endregion Private Members
    }
}
