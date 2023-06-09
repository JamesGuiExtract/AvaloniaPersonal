﻿using Extract.Utilities;
using System;
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
            _purgeRecordsOlderThanDays.Value = _databaseCleanup.PurgeRecordsOlderThanDays;
            _maximumDaysToProcessPerRun.Value = _databaseCleanup.MaxDaysToProcessPerRun;
            _descriptionTextBox.Text = _databaseCleanup.Description;
            _purgeRecordsOlderThanDays.Validating += _purgeRecordsOlderThanDays_Validating;
            CleanupWarningToolTip.SetToolTip(_purgeRecordsOlderThanDays, "Warning, a value less than 365 can lead to catastrophic data loss. Please consult with a product services member if you are unsure what you are doing.");
            CheckForPurgeRecordsOlderThanDaysWarning();
        }

        private void _purgeRecordsOlderThanDays_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CheckForPurgeRecordsOlderThanDaysWarning();
        }

        private void CheckForPurgeRecordsOlderThanDaysWarning()
        {
            if (_purgeRecordsOlderThanDays.Value < 365)
            {
                _purgeRecordsOlderThanDays.BackColor = System.Drawing.Color.Red;
                CleanupWarningToolTip.Active = true;
            }
            else
            {
                _purgeRecordsOlderThanDays.BackColor = System.Drawing.Color.White;
                CleanupWarningToolTip.Active = false;
            }
        }

        private void CalculateNumberOfRowsToBeDeletedButton_Click(object sender, EventArgs e)
        {
            this.CalculateNumberOfRowsToBeDeletedButton.Text = "Calculating....";
            this.CalculateNumberOfRowsToBeDeletedButton.Enabled = false;
            Task.Run(() =>
            {
                _databaseCleanup.CalculateNumberOfRowsToDelete((int)_purgeRecordsOlderThanDays.Value);
                this.CalculateNumberOfRowsToBeDeletedButton.Invoke((MethodInvoker)delegate
                {
                    this.CalculateNumberOfRowsToBeDeletedButton.Text = "Calculate number of rows to be deleted";
                    this.CalculateNumberOfRowsToBeDeletedButton.Enabled = true;
                });
            });
        }

        private void OK_Button_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsValid())
                {
                    _databaseCleanup.Schedule = _schedulerControl.Value;
                    _databaseCleanup.PurgeRecordsOlderThanDays = (int)_purgeRecordsOlderThanDays.Value;
                    _databaseCleanup.MaxDaysToProcessPerRun = (int)_maximumDaysToProcessPerRun.Value;
                    _databaseCleanup.Description = _descriptionTextBox.Text;
                    return;
                }
                DialogResult = DialogResult.None;

            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51948");
            }
        }

        private bool IsValid()
        {
            if (string.IsNullOrEmpty(_descriptionTextBox.Text))
            {
                UtilityMethods.ShowMessageBox("Description cannot be empty.", "Invalid configuration", true);
                _descriptionTextBox.Focus();

                return false;
            }
            return true;
        }
    }
}
