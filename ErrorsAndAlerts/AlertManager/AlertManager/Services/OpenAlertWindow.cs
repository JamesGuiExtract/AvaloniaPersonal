using AlertManager.Models.AllDataClasses;
using AlertManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AlertManager.Services
{
    //testing atm, but will need to check out
    public class OpenAlertWindow : ICommand
    {
        AlertsObject? alertObject;

        public OpenAlertWindow(AlertsObject alertsObject)
        {
            this.alertObject = alertsObject;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {

            if (alertObject != null)
            {
                MainWindowViewModel.DisplayAlertWindow(alertObject);
            }

        }
    }
}

