using Extract.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Dashboard.ETL
{
    public partial class DashboardExtractedDataServiceConfigurationForm : Form
    {
        public DashboardExtractedDataServiceConfigurationForm(DashboardExtractedDataService dataService)
        {
            InitializeComponent();
            ExtractedDataService = dataService;
            _descriptionTextBox.Text = dataService.Description;
            _schedulerControl.Value = dataService.Schedule;
        }

        public DashboardExtractedDataService ExtractedDataService { get; }

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
                ExtractedDataService.Description = _descriptionTextBox.Text;
                ExtractedDataService.Schedule = _schedulerControl.Value;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46926");
            }
        }
    }
}
