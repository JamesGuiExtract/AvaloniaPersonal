using System;
using System.Windows.Forms;

namespace Extract.ETL.Management
{
    public partial class DatabaseServiceEditForm : Form
    {
        #region Private fields

        DatabaseService _service;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the DatabaseServiceEditForm
        /// </summary>
        /// <param name="description">Description for the database service</param>
        /// <param name="service">The database service being edited</param>
        public DatabaseServiceEditForm(string description, DatabaseService service)
        {
            InitializeComponent();
            Description = description;
            _service = service;
            JSONConfigString = service.ToJson();
        } 

        #endregion

        #region Private Properties

        string JSONConfigString
        {
            get
            {
                return _jsonTextBox.Text;
            }

            set
            {
                _jsonTextBox.Text = value;
            }
        }

        #endregion

        #region Public Properties

        public DatabaseService Service
        {
            get
            {
                return _service;
            }
            set
            {
                if (value != _service)
                {
                    _service = value;
                    JSONConfigString = _service.ToJson();
                }
            }
        }
        public string Description
        {
            get
            {
                return _descriptionTextBox.Text;
            }

            set
            {
                _descriptionTextBox.Text = value;
            }
        } 

        #endregion

        #region Private methods

        /// <summary>
        /// Checks data for validity
        /// </summary>
        /// <returns><c>true</c> if data is valid. <c>false</c> if data is invalid</returns>
        bool IsValidData()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Description))
                {
                    _descriptionTextBox.Focus();
                    MessageBox.Show("Description cannot be empty.", "Invalid description", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // to validate the Database service json attempt to create the object
                try
                {
                    var tmpService = DatabaseService.FromJson(JSONConfigString);

                    // if the service created ok set the tmpService value to the _service value
                    _service = tmpService;
                }
                catch (Exception)
                {
                    _jsonTextBox.Focus();
                    MessageBox.Show("Database service json string is invalid.", "Invalid json config", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45607");
            }
            return true;
        } 

        #endregion

        #region Event Handlers

        void HandleOkkButtonClick(object sender, EventArgs e)
        {
            if (!IsValidData())
            {
                DialogResult = DialogResult.None;
            }
        } 

        #endregion
    }
}