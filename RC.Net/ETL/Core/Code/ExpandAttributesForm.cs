using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.ETL
{
    public partial class ExpandAttributesForm : Form
    {
        #region Constructors

        /// <summary>
        /// Form to configure ExpandAttributes database service
        /// </summary>
        /// <param name="service">Service to configure</param>
        public ExpandAttributesForm(ExpandAttributes service)
        {
            InitializeComponent();
            ExpandAttributesService = service;

            _storeSpatialInfoCheckBox.Checked = ExpandAttributesService.StoreSpatialInfo;
            _stroreEmptyAttributesCheckBox.Checked = service.StoreEmptyAttributes;
            _descriptionTextBox.Text = service.Description;
            _schedulerControl.Value = service.Schedule;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Service that was configured
        /// </summary>
        public ExpandAttributes ExpandAttributesService { get; }

        #endregion

        #region Event Handlers

        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
                {
                    UtilityMethods.ShowMessageBox("Description cannot be empty.", "Invalid configuration", true);
                    _descriptionTextBox.Focus();
                    DialogResult = DialogResult.None;
                    return;
                }
                ExpandAttributesService.StoreEmptyAttributes = _stroreEmptyAttributesCheckBox.Checked;
                ExpandAttributesService.StoreSpatialInfo = _storeSpatialInfoCheckBox.Checked;
                ExpandAttributesService.Description = _descriptionTextBox.Text;
                ExpandAttributesService.Schedule = _schedulerControl.Value;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45647");
            }
        }

        #endregion
    }
}
