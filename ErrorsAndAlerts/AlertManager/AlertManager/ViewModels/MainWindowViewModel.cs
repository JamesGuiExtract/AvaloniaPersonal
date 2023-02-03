using AlertManager.Services;
using AlertManager.Interfaces;
using AlertManager.Models;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using AlertManager.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Models.TreeDataGrid;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Extract.ErrorHandling;
using System.Diagnostics;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace AlertManager.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        #region fields
        //this is a private observable collection of type DBAdminTable

        public static IClassicDesktopStyleApplicationLifetime? CurrentInstance = 
            Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        public IAlertStatus loggingTarget;

        private string? webpageLocation = ConfigurationManager.AppSettings["ConfigurationWebPath"];


        #endregion fields

        #region getters and setters for Binding

        [Reactive]
        public ObservableCollection<AlertsObject> _AlertTable { get; set; } = new();

        [Reactive]
        public UserControl LoggingTab { get; set; } = new();

        #endregion getters and setters for Binding

        #region constructors

        /// <summary>
        /// The main constructor that is called when this class is initialized
        /// Must be passed a instance of DBService
        /// </summary>
        /// <param name="db"></param>
        public MainWindowViewModel(IAlertStatus? loggingTargetSource)
        {
            loggingTargetSource = (loggingTargetSource == null) ? new AlertStatusElasticSearch() : loggingTargetSource;
            loggingTarget = loggingTargetSource;

            IList<AlertsObject> alerts = new List<AlertsObject>();
            try
            {
                alerts = loggingTargetSource!.GetAllAlerts(page:0);
            }
            catch (Exception e)
            {
                new ExtractException( "ELI53771" , "Error retrieving alerts from logging target", e ).Log();
            }

            foreach(AlertsObject alert in alerts)
            {
                alert.CreateAlertWindow = ReactiveCommand.Create<int>(_ => DisplayAlertDetailsWindow(alert));
                alert.ResolveAlert = ReactiveCommand.Create<int>(_ => DisplayResolveWindow(alert));
                _AlertTable.Add(alert);
            }

            IList<ExceptionEvent> events = new List<ExceptionEvent>();

            try
            {
                events = loggingTargetSource.GetAllEvents(page: 0);

                LoggingTab = new EventListUserControl();

                EventListViewModel eventViewModel = new(events.ToList(), loggingTargetSource);

                LoggingTab.DataContext = eventViewModel;

            }
            catch (Exception e)
            {
                new ExtractException("ELI53777", "Error retrieving events from the logging target from page 0", e).Log();
            }

        }

        //dependency inversion for UI
        public MainWindowViewModel() : this(
            Locator.Current.GetService<IAlertStatus>()
            )
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
                IList<AlertsObject> alerts = loggingTarget.GetAllAlerts(page: 0);

                foreach (AlertsObject alert in alerts)
                {
                    alert.CreateAlertWindow = ReactiveCommand.Create<int>(_ => DisplayAlertDetailsWindow(alert)); //TODO change to alert window in subsiquent jira
                    alert.ResolveAlert = ReactiveCommand.Create<int>(_ => DisplayResolveWindow(alert));
                    _AlertTable.Add(alert);
                }
            }
            catch(Exception e)
            {
                ExtractException ex = new("ELI53871", "Issue refreshing the alert table getting information from page 0", e);
                ex.Log();
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
                ex.Log();
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
                ExtractException ex = new("ELI53874", "Issue displaying the the events table", e);
                ex.Log();
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
                ex.Log();
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
                ex.Log();
            }
        }


        #endregion Methods
    }
}
