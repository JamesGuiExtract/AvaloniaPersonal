using AvaloniaDashboard.Views;
using ReactiveUI;

namespace AvaloniaDashboard.ViewModels
{
    /// <summary>
    /// This Class impliments ReactiveObject
    /// </summary>
    public class ConfigureAlertsViewModel : ReactiveObject
    {

        private ConfigureAlertsView? ThisWindow;

        public ConfigureAlertsViewModel(ConfigureAlertsView configureAlertsTable)
        {
            ThisWindow = configureAlertsTable;
        }

        public ConfigureAlertsViewModel()
        {

        }

        private void CloseWindow() //todo switch to dialog later
        {
            if(ThisWindow != null)
            {
                ThisWindow.Close("Refresh");
            }
        }
    }
}