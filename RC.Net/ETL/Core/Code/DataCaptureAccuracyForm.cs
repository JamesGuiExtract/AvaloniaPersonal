using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace Extract.ETL
{
    public partial class DataCaptureAccuracyForm : Form
    {

        #region Public properties

        /// <summary>
        /// Service that was configured
        /// </summary>
        public DataCaptureAccuracy Service { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize DataCaptureAccuracyForm
        /// </summary>
        /// <param name="captureAccuracy">Instance of DataCaptureAccuracy to configure</param>
        public DataCaptureAccuracyForm(DataCaptureAccuracy captureAccuracy)
        {
            InitializeComponent();
            Service = captureAccuracy;
            LoadComboBoxes();
            _descriptionTextBox.Text = Service.Description;
            _xpathContainerOnlyTextBox.Text = Service.XPathOfContainerOnlyAttributes;
            _xpathToIgnoreTextBox.Text = Service.XPathOfAttributesToIgnore;
            _foundAttributeSetComboBox.SelectedItem = Service.FoundAttributeSetName;
            _expectedAttributeSetComboBox.SelectedItem = Service.ExpectedAttributeSetName;
        }

        #endregion

        #region Event Handlers

        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (IsValid())
                {
                    Service.Description = _descriptionTextBox.Text;
                    Service.XPathOfAttributesToIgnore = _xpathToIgnoreTextBox.Text;
                    Service.XPathOfContainerOnlyAttributes = _xpathContainerOnlyTextBox.Text;
                    Service.FoundAttributeSetName = (string)_foundAttributeSetComboBox.SelectedItem ?? string.Empty;
                    Service.ExpectedAttributeSetName = (string)_expectedAttributeSetComboBox.SelectedItem ?? string.Empty;
                    return;
                }
                DialogResult = DialogResult.None;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45659");
            }

        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Checks if configuration is valid and leaves focus on invalid data
        /// </summary>
        /// <returns><c>true</c> if configuration is valid, <c>false</c> if not valid</returns>
        bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
            {
                MessageBox.Show("Description cannot be empty.", "Invalid configuration", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                _descriptionTextBox.Focus();
                
                return false;
            }
            if (_expectedAttributeSetComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Expected attribute set is required", "Invalid configuration", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                _expectedAttributeSetComboBox.Focus();
                return false;
            }
            if (_foundAttributeSetComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Found attribute set is required", "Invalid configuration", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                _foundAttributeSetComboBox.Focus();
                return false;
            }
            if (_foundAttributeSetComboBox.SelectedItem == _expectedAttributeSetComboBox.SelectedItem)
            {
                MessageBox.Show("Expected and found attribute sets must be different", "Invalid configuration", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                _expectedAttributeSetComboBox.Focus();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Loads attribute set combo boxes
        /// </summary>
        void LoadComboBoxes()
        {
            using (var connection = NewSqlDBConnection())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT [Description] FROM [dbo].[AttributeSetName]";
                    var results = cmd.ExecuteReader()
                        .Cast<IDataRecord>()
                        .Select(r => r.GetString(r.GetOrdinal("Description"))).ToArray();

                    _expectedAttributeSetComboBox.Items.Clear();
                    _expectedAttributeSetComboBox.Items.AddRange(results);

                    _foundAttributeSetComboBox.Items.Clear();
                    _foundAttributeSetComboBox.Items.AddRange(results);
                }
            }
        }

        /// <summary>
        /// Returns a connection to the configured database. Can be overridden if needed
        /// </summary>
        /// <returns>SqlConnection that connects to the <see cref="DatabaseServer"/> and <see cref="DatabaseName"/></returns>
        SqlConnection NewSqlDBConnection()
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = Service.DatabaseServer;
            sqlConnectionBuild.InitialCatalog = Service.DatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

        #endregion

    }
}
