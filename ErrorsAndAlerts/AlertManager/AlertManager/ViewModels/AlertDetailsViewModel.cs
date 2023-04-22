using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using Extract.ErrorHandling;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Threading.Tasks;

namespace AlertManager.ViewModels
{
    public class AlertDetailsViewModel : ViewModelBase
    {
        [Reactive]
        public AlertsObject ThisAlert { get; set; }

        private readonly EventsOverallViewModelFactory _eventsOverallViewModelFactory;
        private readonly IElasticSearchLayer _elasticService;
        private readonly IAlertActionLogger _alertResolutionLogger;
        private readonly IWindowService _windowService;
        private readonly IDBService _dbService;

        [Reactive]
        public string AlertResolutionHistory { get; set; } = "";


        public AlertDetailsViewModel(
            IWindowService windowService,
            IDBService dbService,
            EventsOverallViewModelFactory eventsOverallViewModelFactory,
            IElasticSearchLayer elastic,
            IAlertActionLogger alertResolutionLogger,
            AlertsObject alertObject)
        {
            _windowService = windowService;
            _dbService = dbService;
            _eventsOverallViewModelFactory = eventsOverallViewModelFactory;
            _elasticService = elastic;
            _alertResolutionLogger = alertResolutionLogger;

            ThisAlert = alertObject;
            AlertResolutionHistory = AlertHistoryToString();
        }

        /// <summary>
        /// Opens a new window displaying the environment details for the current alert.
        /// </summary>
        /// <returns></returns>
        public async Task<string> OpenEnvironmentView()
        {

            try
            {
                EnvironmentInformationViewModel environmentViewModel = new(ThisAlert, _elasticService);

                return await _windowService.ShowEnvironmentInformationView(environmentViewModel);
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54145", "Issue displaying the events table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

            return "";
        }

        private string AlertHistoryToString()
        {
            string returnString = "";
            try
            {
                if (ThisAlert == null || ThisAlert.Actions == null)
                {
                    throw new Exception(" Issue with retrieving object");
                }

                foreach (var resolution in ThisAlert.Actions)
                {
                    returnString += "Previous Comment: " + resolution.ActionComment +
                        "  Time: " + resolution.ActionTime + "  Type: " + resolution.ActionType + "\n";
                }
            }
            catch (Exception e)
            {
                RxApp.DefaultExceptionHandler.OnNext(e.AsExtractException("ELI54223"));
                return "";
            }

            return returnString;
        }

        public async Task<string> OpenAssociatedEvents()
        {
            try
            {
                if (ThisAlert.AssociatedEvents == null)
                {
                    throw new ExtractException("ELI54134", "Issue with Alert Object");
                }

                EventListWindowViewModel eventViewModel = new(_windowService, _eventsOverallViewModelFactory, ThisAlert.AssociatedEvents, "Associated Events");

                return await _windowService.ShowEventListWindowView(eventViewModel);
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54144", "Issue displaying the events table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);

                return "";
            }
        }

        public async Task<string> ResolveWindow()
        {
            try
            {
                ResolveAlertsViewModel resolveViewModel = new(ThisAlert, _alertResolutionLogger, _elasticService, _dbService);

                return await _windowService.ShowResolveAlertsView(resolveViewModel);
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54139", "Issue displaying the the events table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);

                return "";
            }
        }
    }
}