using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Extract.ETL
{
    public partial class DatabaseCleanupForm : Form
    {
        private readonly DatabaseCleanup _databaseCleanup;
        public DatabaseCleanupForm(DatabaseCleanup databaseCleanup)
        {
            InitializeComponent();
            _databaseCleanup = databaseCleanup;
            _schedulerControl.Value = _databaseCleanup.Schedule;
            this.PurgeAfterDaysSelector.Value = _databaseCleanup.PurgeRecordsOlderThanDays;
            this.MaximumNumberOfFilesToCleanUpSelector.Value = _databaseCleanup.MaxFilesToSelect;
            _descriptionTextBox.Text = _databaseCleanup.Description;
        }

        private void OK_Button_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsValid())
                {
                    _databaseCleanup.Description = _descriptionTextBox.Text;
                    _databaseCleanup.Schedule = _schedulerControl.Value;
                    _databaseCleanup.PurgeRecordsOlderThanDays = (int)this.PurgeAfterDaysSelector.Value;
                    _databaseCleanup.MaxFilesToSelect = (int)this.MaximumNumberOfFilesToCleanUpSelector.Value;
                    return;
                }
                DialogResult = DialogResult.None;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51911");
            }
        }

        private bool IsValid()
        {
            return true;
        }
    }
}
