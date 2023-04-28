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
        private readonly IAlertActionLogger _alertActionLogger;
        private readonly IWindowService _windowService;
        private readonly IDBService _dbService;

        [Reactive]
        public string AlertActionHistory { get; set; } = "";


        public AlertDetailsViewModel(
            IWindowService windowService,
            IDBService dbService,
            EventsOverallViewModelFactory eventsOverallViewModelFactory,
            IElasticSearchLayer elastic,
            IAlertActionLogger alertActionLogger,
            AlertsObject alertObject)
        {
            _windowService = windowService;
            _dbService = dbService;
            _eventsOverallViewModelFactory = eventsOverallViewModelFactory;
            _elasticService = elastic;
            _alertActionLogger = alertActionLogger;

            ThisAlert = alertObject;
            AlertActionHistory = AlertHistoryToString();
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
            //Needs to be fixed. Currently not displaying history for alerts with actions.
            //https://extract.atlassian.net/browse/ISSUE-19260
            string returnString = "";
            try
            {
                if (ThisAlert == null || ThisAlert.Actions == null)
                {
                    throw new Exception(" Issue with retrieving object");
                }

                foreach (var action in ThisAlert.Actions)
                {
                    returnString += "Previous Comment: " + action.ActionComment +
                        "  Time: " + action.ActionTime + "  Type: " + action.ActionType + "\n";
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

        public async Task<string> ActionsWindow()
        {
            try
            {
                AlertActionsViewModel actionsViewModel = new(ThisAlert, _alertActionLogger, _elasticService, _dbService);

                return await _windowService.ShowAlertActionsView(actionsViewModel);
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