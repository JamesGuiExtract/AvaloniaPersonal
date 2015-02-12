using Extract.Database;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="DataQueryRuleObject"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class DataQueryRuleObjectSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataQueryRuleObjectSettingsDialog).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Allows testing of the <see cref="DataQueryRuleObject.Query"/>.
        /// </summary>
        ExpressionAndQueryTesterForm _testerForm;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataQueryRuleObjectSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="DataQueryRuleObject"/> instance to configure.</param>
        public DataQueryRuleObjectSettingsDialog(DataQueryRuleObject settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                    "ELI34776", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                // Only one of the two DB connection checkboxes may be checked at any one time.
                _useFAMDBCheckBox.CheckedChanged +=
                    ((o, e) => _useSpecifiedDBCheckBox.Checked &= !_useFAMDBCheckBox.Checked);
                _useSpecifiedDBCheckBox.CheckedChanged +=
                    ((o, e) => 
                        {
                            _useFAMDBCheckBox.Checked &= !_useSpecifiedDBCheckBox.Checked;
                            _databaseConnectionControl.Enabled = _useSpecifiedDBCheckBox.Checked;
                        });

                _databaseConnectionControl.PathTags = new AttributeFinderPathTags();

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    _queryScintillaBox.Text = Settings.Query;
                    _useFAMDBCheckBox.Checked = Settings.UseFAMDBConnection;
                    _useSpecifiedDBCheckBox.Checked = Settings.UseSpecifiedDBConnection;
                    _databaseConnectionControl.DatabaseConnectionInfo =
                        new DatabaseConnectionInfo(Settings.DatabaseConnectionInfo);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34777");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="DataQueryRuleObject"/> to configure.
        /// </summary>
        /// <value>The <see cref="DataQueryRuleObject"/> to configure.</value>
        public DataQueryRuleObject Settings
        {
            get;
            set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _queryScintillaBox.ConfigurationManager.Configure();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34778");
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

                if (_testerForm != null)
                {
                    _testerForm.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the test query click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTestQueryClick(object sender, EventArgs e)
        {
            try
            {
                if (_testerForm == null)
                {
                    _testerForm = new ExpressionAndQueryTesterForm();
                    _testerForm.StartPosition = FormStartPosition.CenterParent;
                    _testerForm.FormClosing += HandleTesterFormClosing;
                }

                // Apply the current query and connection settings to the _testerForm.
                _testerForm.TypeOfTest = TypeOfTest.DataQueryTest;
                _testerForm.ExpressionOrQuery = _queryScintillaBox.Text;
                _testerForm.DatabaseConnectionInfo =
                    new DatabaseConnectionInfo(_databaseConnectionControl.DatabaseConnectionInfo);
                _testerForm.ClearResults();

                if (_testerForm.Visible)
                {
                    _testerForm.Activate();
                }
                else
                {
                    _testerForm.Show();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34779");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Form.FormClosing"/> event from <see cref="_testerForm"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs"/> instance
        /// containing the event data.</param>
        void HandleTesterFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Don't allow the tester form to close as it would then be disposed.
                e.Cancel = true;

                // If the query or connection has been edited, prompt for whether to apply the
                // changes back to the rule object.
                if ((_testerForm.ExpressionOrQuery != _queryScintillaBox.Text) ||
                    !_testerForm.DatabaseConnectionInfo.Equals(
                        _databaseConnectionControl.DatabaseConnectionInfo))
                {
                    DialogResult response = MessageBox.Show(_testerForm,
                        "Apply query and connection from test window to the rule object?",
                        "Use tester query?",
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1, 0);

                    switch (response)
                    {
                        case DialogResult.Yes:
                            _queryScintillaBox.Text = _testerForm.ExpressionOrQuery;

                            _testerForm.Hide();
                            break;

                        case DialogResult.No:
                            _testerForm.Hide();
                            break;

                        //case DialogResult.Cancel:
                        // Don't hide.
                    }
                }
                else
                {
                    _testerForm.Hide();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34780");
            }
        }

        /// <summary>
        /// In the case that the OK button is clicked, validates the settings, applies them, and
        /// closes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_queryScintillaBox.Text))
                {
                    UtilityMethods.ShowMessageBox("The query has not been specified.",
                        "Missing query", true);
                    return;
                }

                if (_useSpecifiedDBCheckBox.Checked &&
                    string.IsNullOrWhiteSpace(
                        _databaseConnectionControl.DatabaseConnectionInfo.DataProviderName))
                {
                    UtilityMethods.ShowMessageBox(
                        "A database connection has not been specified.",
                        "Database connection not specified", true);
                }

                if (!_useFAMDBCheckBox.Checked && !_useSpecifiedDBCheckBox.Checked &&
                    _queryScintillaBox.Text.IndexOf("<SQL", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    UtilityMethods.ShowMessageBox(
                        "A database connection must be specified for queries that contain SQL elements.",
                        "Missing database connection", true);
                    return;
                }

                Settings.Query = _queryScintillaBox.Text;
                Settings.UseFAMDBConnection = _useFAMDBCheckBox.Checked;
                Settings.UseSpecifiedDBConnection = _useSpecifiedDBCheckBox.Checked;
                Settings.DatabaseConnectionInfo = new DatabaseConnectionInfo(
                    _databaseConnectionControl.DatabaseConnectionInfo);

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34781");
            }
        }

        #endregion Event Handlers
    }
}
