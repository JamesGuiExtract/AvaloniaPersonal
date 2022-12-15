using AlertManager.Models.AllDataClasses;
using AlertManager.ViewModels;
using Extract.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AlertManager.Services
{
    public class OpenEventWindow : ICommand
    {
        public EventObject? eventObject;

        public OpenEventWindow(EventObject? eventObject)
        {
            this.eventObject = eventObject;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {

            return true;

        }

        public void Execute(object? parameter)
        {
            if (eventObject != null)
            {
                try
                {
                    MainWindowViewModel.DisplayEventsWindow(eventObject);
                }
                catch(Exception e)
                {
                    ExtractException ex = new("ELI53856", "Issue executing opening a new window, parameter passed in is: " + parameter?.ToString(), e);
                    throw ex;
                }
            }
        }
    }
}
