using AlertManager.ViewModels;
using System.Threading.Tasks;

namespace AlertManager.Interfaces
{
    public interface IWindowService
    {
        Task<string> ShowEnvironmentInformationView(EnvironmentInformationViewModel environmentInformationViewModel);

        Task<string> ShowEventListWindowView(EventListWindowViewModel eventListWindowViewModel);

        Task<string> ShowAlertActionsView(AlertActionsViewModel alertActionsViewModel);

        void ShowEventsOverallView(EventsOverallViewModel eventsOverallViewModel);

        Task<string> ShowAlertDetailsView(AlertDetailsViewModel alertDetailsViewModel);

        Task<string> ShowConfigureAlertsViewModel(ConfigureAlertsViewModel configureAlertsViewModel);
    }
}
