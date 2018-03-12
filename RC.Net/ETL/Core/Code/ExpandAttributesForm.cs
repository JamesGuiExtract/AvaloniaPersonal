using System;
using System.Windows.Forms;

namespace Extract.ETL
{
    public partial class ExpandAttributesForm : Form
    {
        #region Constructors

        /// <summary>
        /// Form to configure ExpandAttributes database servcie
        /// </summary>
        /// <param name="service">Service to configure</param>
        public ExpandAttributesForm(ExpandAttributes service)
        {
            InitializeComponent();
            Service = service;

            _storeSpatialInfoCheckBox.Checked = Service.StoreSpatialInfo;
            _stroreEmptyAttributesCheckBox.Checked = service.StoreEmptyAttributes;
            _descriptionTextBox.Text = service.Description;
            _schedulerControl.Value = service.Schedule;
        } 

        #endregion
        
        #region Public properties

        /// <summary>
        /// Service that was configured
        /// </summary>
        public ExpandAttributes Service { get; }

        #endregion

        #region Event Handlers

        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
                {
                    MessageBox.Show("Description cannot be empty.");
                    _descriptionTextBox.Focus();
                    DialogResult = DialogResult.None;
                    return;
                }
                Service.StoreEmptyAttributes = _stroreEmptyAttributesCheckBox.Checked;
                Service.StoreSpatialInfo = _storeSpatialInfoCheckBox.Checked;
                Service.Description = _descriptionTextBox.Text;
                Service.Schedule = _schedulerControl.Value;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45647");
            }
        } 

        #endregion
    }
}
