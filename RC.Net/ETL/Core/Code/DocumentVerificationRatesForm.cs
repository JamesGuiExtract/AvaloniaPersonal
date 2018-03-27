using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.ETL
{
    public partial class DocumentVerificationRatesForm : Form
    {
        #region Constructors

        /// <summary>
        /// Form to configure DocumentVerificationRates database service
        /// </summary>
        /// <param name="verificationRates">Service to configure</param>
        public DocumentVerificationRatesForm(DocumentVerificationRates verificationRates)
        {
            InitializeComponent();

            _descriptionTextBox.Text = verificationRates.Description;
            _schedulerControl.Value = verificationRates.Schedule;
            DocumentVerificationRatesService = verificationRates;

        }

        #endregion

        #region Public properties

        /// <summary>
        ///  Service that was configured
        /// </summary>
        public DocumentVerificationRates DocumentVerificationRatesService { get; set; }

        #endregion

        #region Event Handers

        void HandleOKButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
                {
                    UtilityMethods.ShowMessageBox("Description must not be empty.", "Invalid configuration", true);
                    DialogResult = DialogResult.None;
                    return;
                }

                DocumentVerificationRatesService.Description = _descriptionTextBox.Text;
                DocumentVerificationRatesService.Schedule = _schedulerControl.Value;
                DialogResult = DialogResult.OK;

            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45687");
            }
        }

        #endregion
    }
}
