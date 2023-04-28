using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace AlertManager.ViewModels
{

    public class MainWindowViewModel : ViewModelBase, IActivatableViewModel
    {
        public record AlertTableRow(
            AlertsObject Alert,
            AlertActionDto RecentAction,
            string AlertStatus,
            ReactiveCommand<int, Unit> DisplayAlertDetails,
            ReactiveCommand<int, Unit> DisplayAction);

        #region fields

        private readonly IWindowService _windowService;
        private readonly EventsOverallViewModelFactory _eventsOverallViewModelFactory;
        private readonly IElasticSearchLayer _elasticService;
        private readonly IDBService _databaseService;
        private readonly IAlertActionLogger _alertActionLogger;

        private int currentPage;

        private int maxPage;

        #endregion fields

        #region getters and setters for binding

        [Reactive]
        public ObservableCollection<AlertTableRow> AlertTable { get; set; } = new();

        [Reactive]
        public EventListViewModel? LoggingTab { get; set; }

        [Reactive]
        public string PageLabel { get; set; } = string.Empty;

        [Reactive]
        public bool PreviousEnabled { get; set; } = false;

        [Reactive]
        public bool NextEnabled { get; set; } = false;

        [Reactive]
        public ReactiveCommand<string, Unit> LoadPage { get; set; }

        public ViewModelActivator Activator { get; }

        #endregion getters and setters for Binding

        #region constructors

        /// <summary>
        /// The main constructor that is called when this class is initialized
        /// Must be passed a instance of DBService
        /// </summary>
        /// <param name="elasticSearch">Instance of elastic service singleton</param>
        public MainWindowViewModel(
            IWindowService windowService,
            EventsOverallViewModelFactory eventsOverallViewModelFactory,
            IElasticSearchLayer elasticSearch,
            IAlertActionLogger alertActionLogger,
            IDBService databaseService)
        {
            _windowService = windowService;
            _eventsOverallViewModelFactory = eventsOverallViewModelFactory;
            _elasticService = elasticSearch;
            _alertActionLogger = alertActionLogger;
            _databaseService = databaseService;
            LoadPage = ReactiveCommand.Create<string>(LoadPageImpl);

            Activator = new ViewModelActivator();
            this.WhenActivated((CompositeDisposable disposables) =>
            {
                maxPage = _elasticService.GetMaxAlertPages();
                UpdatePageCounts("first");

                IList<AlertsObject> alerts = new List<AlertsObject>();
                try
                {
                    alerts = elasticSearch!.GetAllAlerts(page: 0);
                }
                catch (Exception e)
                {
                    ExtractException ex = new("ELI53771", "Error retrieving alerts from logging target", e);
                    RxApp.DefaultExceptionHandler.OnNext(ex);
                }

                AlertTable = CreateAlertTable(alerts);

                IList<ExceptionEvent> events = new List<ExceptionEvent>();

                try
                {
                    LoggingTab = new(_windowService, _eventsOverallViewModelFactory, _elasticService, "Logging");
                }
                catch (Exception e)
                {
                    ExtractException ex = new("ELI53777", "Error retrieving events from the logging target from page 0", e);
                    RxApp.DefaultExceptionHandler.OnNext(ex);
                }
            });
        }

        #endregion constructors

        #region methods

        /// <summary>
        /// Refreshes the observable collection bound to the Alerts table
        /// </summary>
        public void RefreshAlertTable()
        {
            try
            {
                AlertTable.Clear();
                IList<AlertsObject> alerts = _elasticService.GetAllAlerts(page: 0);
                AlertTable = CreateAlertTable(alerts);
                maxPage = _elasticService.GetMaxAlertPages();
                UpdatePageCounts("first");
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI53871", "Issue refreshing the alert table getting information from page 0", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

        }

        /// <summary>
        /// This method creates and opens a new window in which to take alert actions
        /// Sets the AlertActionsViewmodel as the datacontext
        /// </summary>
        /// <param name="alertObjectToPass"> AlertObject object that serves as Window Initialization</param>
        public async Task<string> DisplayActionsWindow(AlertsObject alertObjectToPass)
        {
            try
            {
                AlertActionsViewModel alertActionsViewModel = new(alertObjectToPass, _alertActionLogger, _elasticService, _databaseService);

                return await _windowService.ShowAlertActionsView(alertActionsViewModel);
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI53873", "Issue displaying the the alerts table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);

                return "";
            }
        }

        public async Task<string> DisplayAlertDetailsWindow(AlertsObject alertObjectToPass)
        {
            try
            {
                AlertDetailsViewModel alertsViewModel = new(
                    _windowService,
                    _databaseService,
                    _eventsOverallViewModelFactory,
                    _elasticService,
                    _alertActionLogger,
                    alertObjectToPass);

                return await _windowService.ShowAlertDetailsView(alertsViewModel);
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54141", "Issue displaying the the events table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);

                return "";
            }
        }


        /// <summary>
        /// Creates the Window to configure Alerts, sets the datacontext of window to ConfigureAlertsViewModel
        /// </summary>
        public async Task<string> DisplayAlertsIgnoreWindow()
        {
            try
            {
                ConfigureAlertsViewModel newWindowViewModel = new(_databaseService);

                return await _windowService.ShowConfigureAlertsViewModel(newWindowViewModel);
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI53875", "Issue displaying the the alerts ignore window", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);

                return "";
            }
        }

        /// <summary>
        /// Command function run when a user changes what page they are viewing on the alerts table
        /// </summary>
        /// <param name="direction">Command parameter indicating what page to display next</param>
        private void LoadPageImpl(string direction)
        {
            maxPage = this._elasticService.GetMaxAlertPages();
            bool successfulUpdate = UpdatePageCounts(direction);
            if (!successfulUpdate)
            {
                ExtractException ex = new("ELI53982", "Invalid Page Update Command");
                RxApp.DefaultExceptionHandler.OnNext(ex);
                return;
            }
            IList<AlertsObject> alerts = new List<AlertsObject>();
            try
            {
                alerts = _elasticService.GetAllAlerts(page: currentPage - 1);
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54146", "Error retrieving alerts from logging target", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }
            AlertTable.Clear();
            AlertTable = CreateAlertTable(alerts);
        }

        /// <summary>
        /// Updates the appropriate page count fields based on the user-entered direction
        /// </summary>
        /// <param name="direction">user entered direction</param>
        /// <returns>true if valid updates were made, false otherwise</returns>
        private bool UpdatePageCounts(string direction)
        {
            switch (direction)
            {
                case "first":
                    currentPage = 1;
                    if (maxPage > currentPage) NextEnabled = true;
                    PreviousEnabled = false;
                    break;
                case "previous":
                    if (currentPage > 1) currentPage -= 1;
                    if (maxPage > currentPage) NextEnabled = true;
                    if (currentPage == 1) PreviousEnabled = false;
                    break;
                case "next":
                    if (currentPage < maxPage) currentPage += 1;
                    if (currentPage == maxPage) NextEnabled = false;
                    if (currentPage > 1) PreviousEnabled = true;
                    break;
                case "last":
                    currentPage = maxPage;
                    NextEnabled = false;
                    if (currentPage > 1) PreviousEnabled = true;
                    break;
                default:
                    return false;
            }
            PageLabel = $"Page {currentPage} of {maxPage}";
            return true;
        }

        /// <summary>
        /// Creates easy-to-use data for displaying in the page's main table.
        /// </summary>
        /// <param name="alerts">List of alerts to be displayed.</param>
        /// <returns>Collection of table rows for display</returns>
        private ObservableCollection<AlertTableRow> CreateAlertTable(IList<AlertsObject> alerts)
        {
            ObservableCollection<AlertTableRow> newAlertTable = new();
            try
            {
                foreach (var alert in alerts)
                {
                    AlertActionDto newestAction = GetNewestStatusAction(alert);
                    string alertStatus = GetAlertStatus(alert);
                    ReactiveCommand<int, Unit> displayAlertDetails = ReactiveCommand.CreateFromTask<int>(_ => DisplayAlertDetailsWindow(alert));
                    ReactiveCommand<int, Unit> displayAlertActions = ReactiveCommand.CreateFromTask<int>(_ => DisplayActionsWindow(alert)); ;

                    newAlertTable.Add(new AlertTableRow(alert, newestAction, alertStatus, displayAlertDetails, displayAlertActions));
                }
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54255", "", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }
            return newAlertTable;
        }

        /// <summary>
        /// Gets the action with the most recent action time from an alert.
        /// </summary>
        /// <param name="alert">Alert to get action for.</param>
        /// <returns>AlertActionDto most recently added to alert.</returns>
        private static AlertActionDto GetNewestStatusAction(AlertsObject alert)
        {
            AlertActionDto toReturn = new();

            foreach (AlertActionDto action in alert.Actions)
            {
                //Ignore comment actions as they are purely to add comments and do not change status 
                if (action.ActionType == "Comment")
                    continue;

                if (toReturn.ActionTime == null || action.ActionTime > toReturn.ActionTime)
                    toReturn = action;
            }

            return toReturn;
        }

        /// <summary>
        /// Determines the status of an alert based on the type of the most recent action taken on the alert.
        /// </summary>
        /// <param name="alert">Alert to get status of</param>
        /// <returns>String representing the alert's status.</returns>
        private static string GetAlertStatus(AlertsObject alert)
        {
            AlertActionDto statusAction = GetNewestStatusAction(alert);
            int statusCode;
            string? alertActionType = statusAction.ActionType;

            if (String.IsNullOrEmpty(alertActionType))
            {
                statusCode = 0;
            }
            else
            {
                statusCode = (int)Enum.Parse(typeof(AlertActionType), alertActionType);
            }

            var statusEnum = (AlertStatus)statusCode;

            return statusEnum.ToString();
        }
        #endregion Methods
    }
}
