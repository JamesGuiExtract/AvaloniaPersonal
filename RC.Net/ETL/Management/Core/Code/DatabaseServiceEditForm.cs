using Extract.Utilities.Forms;
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
        /// <param name="service">The database service being edited</param>
        public DatabaseServiceEditForm(DatabaseService service)
        {
            InitializeComponent();
            Service = service;
            JsonConfigString = service.ToJson();
        }

        #endregion

        #region Private Properties

        string Description
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

        string JsonConfigString
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
                    JsonConfigString = _service.ToJson();
                    Description = Service.Description;
                }
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
                    // convert the config string to validate
                    DatabaseService.FromJson(JsonConfigString);
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

        void HandleScheduleButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (IsValidData())
                {
                    var tmpService = DatabaseService.FromJson(JsonConfigString);

                    // Set the description of the service using the Description
                    tmpService.Description = Description;
                    Service = tmpService;

                    SelectScheduleForm dlg = new SelectScheduleForm(Service.Schedule);
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {

                        // Update the schedule for the service
                        Service.Schedule = dlg.Schedule;

                        // Update the Json string
                        JsonConfigString = Service.ToJson();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45632");
            }
        }

        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (!IsValidData())
                {
                    DialogResult = DialogResult.None;
                }
                else
                {
                    var tmpService = DatabaseService.FromJson(JsonConfigString);

                    // Set the description of the service using the Description
                    tmpService.Description = Description;
                    Service = tmpService;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45633");
            }
        }

        #endregion
    }
}