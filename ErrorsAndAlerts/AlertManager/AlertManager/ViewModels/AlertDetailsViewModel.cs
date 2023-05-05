using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace AlertManager.ViewModels
{
    public class AlertDetailsViewModel : ViewModelBase
    {
        [Reactive]
        public AlertsObject ThisAlert { get; set; }

        [Reactive]
        public string ThisAlertStatus { get; set; } = "";

        [Reactive]
        public string AssociatedFiles { get; set; } = "";

        private readonly EventsOverallViewModelFactory _eventsOverallViewModelFactory;
        private readonly IElasticSearchLayer _elasticService;
        private readonly IAlertActionLogger _alertActionLogger;
        private readonly IWindowService _windowService;
        private readonly IDBService _dbService;

        [Reactive]
        public string AlertActionHistory { get; set; } = "";

        public ReactiveCommand<Unit, Unit> OpenEnvironmentView { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenAssociatedEvents { get; private set; }
        public ReactiveCommand<Unit, Unit> ActionsWindow { get; private set; }

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

            OpenEnvironmentView = ReactiveCommand.CreateFromTask(OpenEnvironmentViewImpl);
            OpenAssociatedEvents = ReactiveCommand.CreateFromTask(OpenAssociatedEventsImpl);
            ActionsWindow = ReactiveCommand.CreateFromTask(ActionsWindowImpl);

            SetAssociatedFiles();

            this.WhenAnyValue(x => x.ThisAlert)
            .Subscribe(alert =>
            {
                SetAlertStatus();
            });
        }

        /// <summary>
        /// Opens a new window displaying the environment details for the current alert.
        /// </summary>
        /// <returns></returns>
        public async Task OpenEnvironmentViewImpl()
        {

            try
            {
                EnvironmentInformationViewModel environmentViewModel = new(ThisAlert, _elasticService);
                await _windowService.ShowEnvironmentInformationView(environmentViewModel);
                RefreshScreen();
                return;
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54145", "Issue displaying the events table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

            return;
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

        public async Task OpenAssociatedEventsImpl()
        {
            try
            {
                if (ThisAlert.AssociatedEvents == null)
                {
                    throw new ExtractException("ELI54134", "Issue with Alert Object");
                }

                EventListWindowViewModel eventViewModel = new(_windowService, _eventsOverallViewModelFactory, ThisAlert.AssociatedEvents, "Associated Events");
                await _windowService.ShowEventListWindowView(eventViewModel);
                //don't need to refresh, nothing being done
                return;
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54144", "Issue displaying the events table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);

                return;
            }
        }

        public async Task ActionsWindowImpl()
        {
            try
            {
                AlertActionsViewModel actionsViewModel = new(ThisAlert, _alertActionLogger, _elasticService, _dbService);
                await _windowService.ShowAlertActionsView(actionsViewModel);
                RefreshScreen();
                return;
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54139", "Issue displaying the the events table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);

                return;
            }
        }

        private void SetAssociatedFiles()
        {
            ThisAlertStatus = "";

            foreach (EventDto eventObj in ThisAlert.AssociatedEvents)
            {
                ThisAlertStatus += eventObj.Context.FileID.ToString();
            }
        }

        private void RefreshScreen()
        {
            SetAlertStatus();
            SetAssociatedFiles();
            AlertActionHistory = AlertHistoryToString();
        }

        private void SetAlertStatus()
        {
            if (ThisAlert == null)
            {
                return;
            }

            ThisAlert.SetCurrentAlertAction();

            try
            {
                int statusCode;

                if (String.IsNullOrEmpty(ThisAlert.CurrentAction.ActionType))
                {
                    statusCode = 0;
                }
                else
                {
                    statusCode = (int)Enum.Parse(typeof(AlertActionType), ThisAlert.CurrentAction.ActionType);
                }

                ThisAlertStatus = ((AlertStatus)statusCode).ToString();
            }
            catch (Exception)
            {

            }
        }
    }
}