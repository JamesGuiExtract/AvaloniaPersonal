using AlertManager.Services;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Extract.ErrorHandling;
using System.Diagnostics;
using System.Configuration;
using System.Linq;
using System.Reactive;

namespace AlertManager.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        #region fields
        //this is a private observable collection of type DBAdminTable

        public static IClassicDesktopStyleApplicationLifetime? CurrentInstance = 
            Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        public IElasticSearchLayer elasticService;

        private string? webpageLocation = ConfigurationManager.AppSettings["ConfigurationWebPath"];

        private int currentPage;

        private int maxPage;

        #endregion fields

        #region getters and setters for Binding

        [Reactive]
        public ObservableCollection<AlertsObject> _AlertTable { get; set; } = new();

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
            maxPage = elasticService.GetMaxAlertPages();
            updatePageCounts("first");

            IList<AlertsObject> alerts = new List<AlertsObject>();
            try
            {
                alerts = elasticSearch!.GetAllAlerts(page:0);
            }
            catch (Exception e)
            {
                ExtractException ex = new ExtractException( "ELI53771" , "Error retrieving alerts from logging target", e );
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

            _AlertTable = prepAlertList(alerts);

            IList<ExceptionEvent> events = new List<ExceptionEvent>();

            try
            {
                LoggingTab = new EventListUserControl();

                EventListViewModel eventViewModel = new(elasticSearch, "Logging");

                LoggingTab.DataContext = eventViewModel;
            }
            catch (Exception e)
            {
                ExtractException ex = new ExtractException("ELI53777", "Error retrieving events from the logging target from page 0", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

        }

        //dependency inversion for UI
        public MainWindowViewModel() : this(Locator.Current.GetService<IElasticSearchLayer>())
        {
           
        }
        #endregion constructors

        #region Methods


        /// <summary>
        /// Refreshes the observable collection bound to the Alerts table
        /// </summary>
        public void RefreshAlertTable()
        {
            try
            {
                _AlertTable.Clear();
                IList<AlertsObject> alerts = elasticService.GetAllAlerts(page: 0);
                _AlertTable = prepAlertList(alerts);
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
            
            ResolveAlertsView resolveAlerts = new ResolveAlertsView();
            string? result = "";

            try
            {
                ResolveAlertsViewModel resolveAlertsViewModel = new ResolveAlertsViewModel(alertObjectToPass, resolveAlerts);
                resolveAlerts.DataContext = resolveAlertsViewModel;
                result = resolveAlerts.ShowDialog<string>(CurrentInstance?.MainWindow).ToString();
            }
            catch(Exception e)
            {
                ExtractException ex = new("ELI53873", "Issue displaying the the alerts table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

            if (result == null)
            {
                result = "";
            }

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

            if (result == null)
            {
                result = "";
            }

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
                ConfigureAlertsViewModel newWindowViewModel = new ConfigureAlertsViewModel();
                newWindow.DataContext = newWindowViewModel;
                result  = newWindow.ShowDialog(CurrentInstance?.MainWindow).ToString();
            }
            catch(Exception e)
            {
                ExtractException ex = new("ELI53875", "Issue displaying the the alerts ignore window", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

            if(result == null)
            {
                result = "";
            }

            return result;
        }


        public void OpenElasticConfigurations()
        {
            try
            {
                if(webpageLocation == null)
                {
                    throw new Exception("null webpage configuration path");
                }

                Process.Start(new ProcessStartInfo(webpageLocation) { UseShellExecute = true });
            }
            catch(Exception e)
            {
                ExtractException ex = new ExtractException("ELI53962", "Issue opening webpage", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }
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
                ExtractException ex = new ExtractException("ELI53982", "Invalid Page Update Command");
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
                ExtractException ex = new ExtractException("ELI54146", "Error retrieving alerts from logging target", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }
            _AlertTable.Clear();
            _AlertTable = prepAlertList(alerts);
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
        /// Take a list of alert objects and converts them into an Observable collection with the appropriate commands instantiated
        /// </summary>
        /// <param name="alerts">Incoming list of alerts</param>
        /// <returns>ObservableCollection of alerts, each with CreateAlertWindow and ResolveAlert populated</returns>
        private ObservableCollection<AlertsObject> prepAlertList(IList<AlertsObject> alerts)
        {
            ObservableCollection<AlertsObject> alertTable = new ObservableCollection<AlertsObject>();
            try
            {
                foreach (AlertsObject alert in alerts)
                {
                    alert.CreateAlertWindow = ReactiveCommand.Create<int>(_ => DisplayAlertDetailsWindow(alert));
                    alert.ResolveAlert = ReactiveCommand.Create<int>(_ => DisplayResolveWindow(alert));
                    alertTable.Add(alert);
                }
            }
            catch(Exception e)
            {
                ExtractException ex = new ExtractException("ELI54070", "Error preparing alerts list", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }
            return alertTable;
        }


        #endregion Methods
    }
}
