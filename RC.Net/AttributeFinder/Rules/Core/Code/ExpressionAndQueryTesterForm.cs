using Extract.Database;
using Extract.DataEntry;
using Extract.Licensing;
using Extract.Utilities;
using Spring.Core.TypeResolution;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Indicates the type of test a <see cref="ExpressionAndQueryTesterForm"/> is configured to
    /// perform.
    /// </summary>
    public enum TypeOfTest
    {
        /// <summary>
        /// Tests expressions.
        /// </summary>
        ExpressionTest = 0,

        /// <summary>
        /// Tests data queries.
        /// </summary>
        DataQueryTest
    }

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
        /// Indicates if the host is in design mode or not.
        /// </summary>
        bool _inDesignMode;

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
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                // Load licenses in design mode
                if (_inDesignMode)
                {
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI40326", _OBJECT_NAME);

                TypeRegistry.RegisterType("Regex", typeof(System.Text.RegularExpressions.Regex));
                TypeRegistry.RegisterType("StringUtils", typeof(Spring.Util.StringUtils));
                TypeRegistry.RegisterType("CultureInfo", typeof(System.Globalization.CultureInfo));
                TypeRegistry.RegisterType("LabDEUtils", typeof(DataEntry.LabDE.LabDEQueryUtilities));

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40327");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name of the VOA file.
        /// </summary>
        /// <value>
        /// The name of the VOA file.
        /// </value>
        public string VOAFileName
        {
            get
            {
                return _voaFileNameTextBox.Text;
            }

            set
            {
                _voaFileNameTextBox.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TypeOfTest"/>
        /// </summary>
        /// <value>
        /// The <see cref="TypeOfTest"/>.
        /// </value>
        public TypeOfTest TypeOfTest
        {
            get
            {
                return _testQueryRadioButton.Checked
                    ? TypeOfTest.DataQueryTest
                    : TypeOfTest.ExpressionTest;
            }

            set
            {
                try
                {
                    switch (value)
                    {
                        case TypeOfTest.ExpressionTest:
                            {
                                _testExpressionRadioButton.Checked = true;
                                _testQueryRadioButton.Checked = false;
                            }
                            break;

                        case TypeOfTest.DataQueryTest:
                            {
                                _testQueryRadioButton.Checked = true;
                                _testExpressionRadioButton.Checked = false;
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34773");
                }
            }
        }

        /// <summary>
        /// Gets or sets the target or root attribute query.
        /// </summary>
        /// <value>
        /// The target or root attribute query.
        /// </value>
        public string TargetOrRootAttributeQuery
        {
            get
            {
                return _targetAttributeTextBox.Text;
            }

            set
            {
                _targetAttributeTextBox.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the expression or query.
        /// </summary>
        /// <value>
        /// The expression or query.
        /// </value>
        public string ExpressionOrQuery
        {
            get
            {
                return _expressionOrQueryScintillaBox.Text;
            }

            set
            {
                _expressionOrQueryScintillaBox.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseConnectionInfo"/>.
        /// </summary>
        /// <value>
        /// The <see cref="DatabaseConnectionInfo"/>.
        /// </value>
        [CLSCompliant(false)]
        public DatabaseConnectionInfo DatabaseConnectionInfo
        {
            get
            {
                return _databaseConnectionControl.DatabaseConnectionInfo;
            }

            set
            {
                _databaseConnectionControl.DatabaseConnectionInfo = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Clears the results.
        /// </summary>
        public void ClearResults()
        {
            try
            {
                _resultsDataGridView.DataSource = null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34774");
            }
        }

        #endregion Methods

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
                if (_inDesignMode)
                {
                    return;
                }

                if (_testExpressionRadioButton.Checked)
                {
                    _targetAttributesLabel.Text = "Attribute(s) to score";
                    _targetOrRootAttributesInfoTip.TipText =
                        "Leave blank to score all attribute or specify an attribute query to \r\n" +
                        "specify a subset of attributes to score.";
                    _rsdOrDbFilenameLabel.Visible = true;
                    _rsdOrDbFileNameTextBox.Visible = true;
                    _rsdOrDbFileNameBrowseButton.Visible = true;
                    _databaseConnectionControl.Visible = false;
                    _expressionOrQueryScintillaBox.ConfigurationManager.Language = "cs";
                    _splitContainer.Top = _rsdOrDbFileNameTextBox.Bottom + 6;
                }
                else
                {
                    _targetAttributesLabel.Text = "Root attribute";
                    _targetOrRootAttributesInfoTip.TipText =
                        "If a root attribute is needed, specify the attribute query to select it.\r\n" +
                        "If the query matches multiple attributes, the first match will be used.";
                    _rsdOrDbFilenameLabel.Visible = false;
                    _rsdOrDbFileNameTextBox.Visible = false;
                    _rsdOrDbFileNameBrowseButton.Visible = false;
                    _databaseConnectionControl.Visible = true;
                    _expressionOrQueryScintillaBox.ConfigurationManager.Language = "xml";
                    _splitContainer.Top = _databaseConnectionControl.Bottom + 6;
                }

                _splitContainer.Height = _testButton.Top - 6 - _splitContainer.Top;

                // Apply the new configuration of the expression/query text box.
                _expressionOrQueryScintillaBox.ConfigurationManager.Configure();
                _expressionOrQueryScintillaBox.Lexing.Colorize();

                // Clear an existing results
                _resultsDataGridView.DataSource = null;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40328");
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
                ex.ExtractDisplay("ELI40334");
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
                ClearResults();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40329");
            }
        }

        /// <summary>
        /// Handles the open VOA file click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOpenVOAFileClick(object sender, EventArgs e)
        {
            try
            {
                SystemMethods.RunExecutable(FileSystemMethods.GetAbsolutePath("VOAFileViewer.exe"),
                    new[] { _voaFileNameTextBox.Text });
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34775");
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
                ExtractException.Assert("ELI40330", "VOA file not found", File.Exists(voaFileName));

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
                ex.ExtractDisplay("ELI40333");
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
            ExtractException.Assert("ELI40331", "RSD file not found", File.Exists(rsdFileName));

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

            try
            {
                AttributeStatusInfo.InitializeForQuery(sourceAttributes,
                    firstAttribute.Value.SourceDocName,
                        _databaseConnectionControl.DatabaseConnectionInfo.ManagedDbConnection);

                DataEntryQuery query =
                    DataEntryQuery.Create(queryText, rootAttribute,
                        _databaseConnectionControl.DatabaseConnectionInfo.ManagedDbConnection);

                QueryResult result = query.Evaluate();

                // Must select as a list of anonymous types with a named property mapped to
                // the string value for it to show up in the list.
                _resultsDataGridView.DataSource = result.ToStringArray()
                    .Select(value => new { Value = value })
                    .ToList();
            }
            finally
            {
                _databaseConnectionControl.DatabaseConnectionInfo.CloseManagedDbConnection();
            }
        }

        #endregion Private Members
    }
}
