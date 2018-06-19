using Extract.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Extract.ETL
{
    public partial class HIMStatsForm : Form
    {
        #region Constructors

        /// <summary>
        /// Form to configure HIMStats database service
        /// </summary>
        /// <param name="service">Service to configure</param>
        public HIMStatsForm(HIMStats service)
        {
            InitializeComponent();
            HIMStatsService = service;
            _descriptionTextBox.Text = service.Description;
            _schedulerControl.Value = service.Schedule;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Service that was configured
        /// </summary>
        public HIMStats HIMStatsService { get; }

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
                HIMStatsService.Description = _descriptionTextBox.Text;
                HIMStatsService.Schedule = _schedulerControl.Value;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46057");
            }
        }

        #endregion
    }


}
