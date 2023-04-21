using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using AlertManager.Services;
using AlertManager.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Reactive;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using System.Reactive.Disposables;

namespace AlertManager.ViewModels
{

    public class MainWindowViewModel : ViewModelBase, IActivatableViewModel
    {
        public record AlertTableRow(
            AlertsObject alert, 
            AlertActionDto recentAction, 
            string alertStatus,
            ReactiveCommand<int, Unit> displayAlertDetails,
            ReactiveCommand<int, Unit> displayAction);

        #region fields
        public static IClassicDesktopStyleApplicationLifetime? CurrentInstance = 
            Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        public IElasticSearchLayer elasticService;

        private string? webpageLocation = ConfigurationManager.AppSettings["ConfigurationWebPath"];

        private int currentPage;

        private int maxPage;

        #endregion fields

        #region getters and setters for binding

        [Reactive]
        public ObservableCollection<AlertTableRow> _AlertTable { get; set; } = new();

        [Reactive]
        public UserControl LoggingTab { get; set; } = new();

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
        public MainWindowViewModel(IElasticSearchLayer? elasticSearch)
        {
            elasticSearch = (elasticSearch == null) ? new ElasticSearchService() : elasticSearch;
            elasticService = elasticSearch;
            LoadPage = ReactiveCommand.Create<string>(loadPage);

            Activator = new ViewModelActivator();
            this.WhenActivated((CompositeDisposable disposables) => 
            {        
                maxPage = elasticService.GetMaxAlertPages();
                updatePageCounts("first");

                IList<AlertsObject> alerts = new List<AlertsObject>();
                try
                {
                    alerts = elasticSearch!.GetAllAlerts(page:0);
                }
                catch (Exception e)
                {
                    ExtractException ex = new("ELI53771", "Error retrieving alerts from logging target", e);
                    RxApp.DefaultExceptionHandler.OnNext(ex);
                }

                _AlertTable = createAlertTable(alerts);

                IList<ExceptionEvent> events = new List<ExceptionEvent>();

                try
                {
                    LoggingTab = new EventListUserControl();

                    EventListViewModel eventViewModel = new(elasticSearch, "Logging");

                    LoggingTab.DataContext = eventViewModel;
                }
                catch (Exception e)
                {
                    ExtractException ex = new("ELI53777", "Error retrieving events from the logging target from page 0", e);
                    RxApp.DefaultExceptionHandler.OnNext(ex);
                }
            });
        }

        //dependency inversion for UI
        public MainWindowViewModel() : this(Locator.Current.GetService<IElasticSearchLayer>())
        {
           
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
                _AlertTable.Clear();
                IList<AlertsObject> alerts = elasticService.GetAllAlerts(page: 0);
                _AlertTable = createAlertTable(alerts);
                maxPage = elasticService.GetMaxAlertPages();
                updatePageCounts("first");
            }
            catch(Exception e)
            {
                ExtractException ex = new("ELI53871", "Issue refreshing the alert table getting information from page 0", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

        }

        /// <summary>
        /// This method creates and opens a new window in which to resolve alerts
        /// Sets the ResolveAlertsViewmodel as the datacontext
        /// </summary>
        /// <param name="alertObjectToPass"> AlertObject object that serves as Window Initialization</param>
        public string DisplayResolveWindow(AlertsObject alertObjectToPass)
        {
            
            ResolveAlertsView resolveAlerts = new();
            string? result = "";

            try
            {
                ResolveAlertsViewModel resolveAlertsViewModel = new(alertObjectToPass, resolveAlerts);
                resolveAlerts.DataContext = resolveAlertsViewModel;
                result = resolveAlerts.ShowDialog<string>(CurrentInstance?.MainWindow).ToString();
            }
            catch(Exception e)
            {
                ExtractException ex = new("ELI53873", "Issue displaying the the alerts table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

            result ??= "";

            return "";
        }

        public static string DisplayAlertDetailsWindow(AlertsObject alertObjectToPass)
        {
            string? result = "";

            AlertDetailsView alertsWindow = new();
            AlertDetailsViewModel alertsViewModel = new (alertObjectToPass, alertsWindow);
            alertsWindow.DataContext = alertsViewModel;

            try
            {
                result = alertsWindow.ShowDialog<string>(CurrentInstance?.MainWindow).ToString();
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54141", "Issue displaying the the events table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

            result ??= "";

            return result;
        }


        /// <summary>
        /// Creates the Window to configure Alerts, sets the datacontext of window to ConfigureAlertsViewModel
        /// </summary>
        public static string DisplayAlertsIgnoreWindow()
        {
            string? result = "";

            ConfigureAlertsView newWindow = new();

            try
            {
                ConfigureAlertsViewModel newWindowViewModel = new();
                newWindow.DataContext = newWindowViewModel;
                result  = newWindow.ShowDialog(CurrentInstance?.MainWindow).ToString();
            }
            catch(Exception e)
            {
                ExtractException ex = new("ELI53875", "Issue displaying the the alerts ignore window", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

            result ??= "";

            return result;
        }

        /// <summary>
        /// Command function run when a user changes what page they are viewing on the alerts table
        /// </summary>
        /// <param name="direction">Command parameter indicating what page to display next</param>
        private void loadPage(string direction)
        {
            maxPage = this.elasticService.GetMaxAlertPages();
            bool successfulUpdate = updatePageCounts(direction);
            if (!successfulUpdate)
            {
                ExtractException ex = new("ELI53982", "Invalid Page Update Command");
                RxApp.DefaultExceptionHandler.OnNext(ex);
                return;
            }
            IList<AlertsObject> alerts = new List<AlertsObject>();
            try
            {
                alerts = elasticService.GetAllAlerts(page: currentPage - 1);
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54146", "Error retrieving alerts from logging target", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }
            _AlertTable.Clear();
            _AlertTable = createAlertTable(alerts);
        }

        /// <summary>
        /// Updates the appropriate page count fields based on the user-entered direction
        /// </summary>
        /// <param name="direction">user entered direction</param>
        /// <returns>true if valid updates were made, false otherwise</returns>
        private bool updatePageCounts(string direction)
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
        private ObservableCollection<AlertTableRow> createAlertTable(IList<AlertsObject> alerts)
        {
            ObservableCollection<AlertTableRow> newAlertTable = new();
            try
            {
                foreach (var alert in alerts)
                {
                    AlertActionDto newestAction = getNewestAction(alert);
                    string alertStatus = getAlertStatus(alert);
                    ReactiveCommand<int, Unit> displayAlertDetails = ReactiveCommand.Create<int>(_ => DisplayAlertDetailsWindow(alert));
                    ReactiveCommand<int, Unit> displayAlertResolution = ReactiveCommand.Create<int>(_ => DisplayResolveWindow(alert)); ;

                    newAlertTable.Add(new AlertTableRow(alert, newestAction, alertStatus, displayAlertDetails, displayAlertResolution));
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
        private static AlertActionDto getNewestAction(AlertsObject alert)
        {
            AlertActionDto toReturn = new();

            foreach (AlertActionDto action in alert.Actions)
            { 
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
        private static string getAlertStatus(AlertsObject alert) 
        {
            AlertActionDto statusAction = getNewestAction(alert);
            int statusCode;
            string alertActionType = statusAction.ActionType;

            if (alertActionType == "" || alertActionType == null)
            {
                statusCode = 0;
            }
            else
            {
                //Eventually this should switch to use the AlertActionType enum
                statusCode = (int)Enum.Parse(typeof(AlertStatus), alertActionType);
            }

            var statusEnum = (AlertStatus)statusCode;

            return statusEnum.ToString();
        }
        #endregion Methods
    }
}
