using AlertManager.Interfaces;
using AlertManager.ViewModels;
using AlertManager.Views;
using Avalonia.Controls;
using System;
using System.Threading.Tasks;

namespace AlertManager.Services
{
    public class WindowService : IWindowService
    {
        Func<Window?> _getActiveWindow;

        Window? ActiveWindow => _getActiveWindow();

        public WindowService(Func<Window?> getActiveWindow)
        {
            _getActiveWindow = getActiveWindow;
        }

        public Task<string> ShowAlertDetailsView(AlertDetailsViewModel alertDetailsViewModel)
        {
            AlertDetailsView alertDetailsView = new()
            {
                DataContext = alertDetailsViewModel
            };

            return alertDetailsView.ShowDialog<string>(ActiveWindow);
        }

        public Task<string> ShowConfigureAlertsViewModel(ConfigureAlertsViewModel configureAlertsViewModel)
        {
            ConfigureAlertsView configureAlertsView = new()
            {
                DataContext = configureAlertsViewModel
            };

            return configureAlertsView.ShowDialog<string>(ActiveWindow);
        }

        public Task<string> ShowEnvironmentInformationView(EnvironmentInformationViewModel environmentInformationViewModel)
        {
            EnvironmentInformationView environmentWindow = new()
            {
                DataContext = (environmentInformationViewModel)
            };

            return environmentWindow.ShowDialog<string>(ActiveWindow);
        }

        public Task<string> ShowEventListWindowView(EventListWindowViewModel eventListWindowViewModel)
        {
            EventListWindowView eventWindow = new()
            {
                DataContext = (eventListWindowViewModel)
            };

            return eventWindow.ShowDialog<string>(ActiveWindow);
        }

        public void ShowEventsOverallView(EventsOverallViewModel eventsOverallViewModel)
        {
            EventsOverallView eventsWindow = new()
            {
                DataContext = eventsOverallViewModel
            };
            eventsWindow.Show();
        }

        public Task<string> ShowResolveAlertsView(ResolveAlertsViewModel resolveAlertsViewModel)
        {
            ResolveAlertsView resolveAlerts = new()
            {
                DataContext = resolveAlertsViewModel
            };

            return resolveAlerts.ShowDialog<string>(ActiveWindow);
        }
    }
}
