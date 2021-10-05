using Extract.SqlDatabase;
using Extract.Utilities;
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
        public DataCaptureAccuracy DataCaptureAccuracyService { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize DataCaptureAccuracyForm
        /// </summary>
        /// <param name="captureAccuracy">Instance of DataCaptureAccuracy to configure</param>
        public DataCaptureAccuracyForm(DataCaptureAccuracy captureAccuracy)
        {
            InitializeComponent();
            DataCaptureAccuracyService = captureAccuracy;
            LoadComboBoxes();
            _descriptionTextBox.Text = DataCaptureAccuracyService.Description;
            _xpathContainerOnlyTextBox.Text = DataCaptureAccuracyService.XPathOfContainerOnlyAttributes;
            _xpathToIgnoreTextBox.Text = DataCaptureAccuracyService.XPathOfAttributesToIgnore;
            _foundAttributeSetComboBox.SelectedItem = DataCaptureAccuracyService.FoundAttributeSetName;
            _expectedAttributeSetComboBox.SelectedItem = DataCaptureAccuracyService.ExpectedAttributeSetName;
            _schedulerControl.Value = DataCaptureAccuracyService.Schedule;
        }

        #endregion

        #region Event Handlers

        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (IsValid())
                {
                    DataCaptureAccuracyService.Description = _descriptionTextBox.Text;
                    DataCaptureAccuracyService.XPathOfAttributesToIgnore = _xpathToIgnoreTextBox.Text;
                    DataCaptureAccuracyService.XPathOfContainerOnlyAttributes = _xpathContainerOnlyTextBox.Text;
                    DataCaptureAccuracyService.FoundAttributeSetName = (string)_foundAttributeSetComboBox.SelectedItem ?? string.Empty;
                    DataCaptureAccuracyService.ExpectedAttributeSetName = (string)_expectedAttributeSetComboBox.SelectedItem ?? string.Empty;
                    DataCaptureAccuracyService.Schedule = _schedulerControl.Value;
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
                UtilityMethods.ShowMessageBox("Description cannot be empty.", "Invalid configuration", true);
                _descriptionTextBox.Focus();

                return false;
            }
            if (_expectedAttributeSetComboBox.SelectedIndex < 0)
            {
                UtilityMethods.ShowMessageBox("Expected attribute set is required", "Invalid configuration", true);
                _expectedAttributeSetComboBox.Focus();
                return false;
            }
            if (_foundAttributeSetComboBox.SelectedIndex < 0)
            {
                UtilityMethods.ShowMessageBox("Found attribute set is required", "Invalid configuration", true);
                _foundAttributeSetComboBox.Focus();
                return false;
            }
            if (_foundAttributeSetComboBox.SelectedItem == _expectedAttributeSetComboBox.SelectedItem)
            {
                UtilityMethods.ShowMessageBox("Expected and found attribute sets must be different", "Invalid configuration", true);
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
            using var connection = new ExtractRoleConnection(DataCaptureAccuracyService.DatabaseServer, DataCaptureAccuracyService.DatabaseName);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT [Description] FROM [dbo].[AttributeSetName]";
            using var reader = cmd.ExecuteReader();
            var results = reader
                .Cast<IDataRecord>()
                .Select(r => r.GetString(r.GetOrdinal("Description"))).ToArray();

            _expectedAttributeSetComboBox.Items.Clear();
            _expectedAttributeSetComboBox.Items.AddRange(results);

            _foundAttributeSetComboBox.Items.Clear();
            _foundAttributeSetComboBox.Items.AddRange(results);
        }

        #endregion

    }
}
