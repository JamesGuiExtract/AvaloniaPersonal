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

namespace AlertManager.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        #region fields
        //this is a private observable collection of type DBAdminTable

        public static IClassicDesktopStyleApplicationLifetime? CurrentInstance = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        #endregion fields

        #region getters and setters for Binding
        [Reactive]
        public ObservableCollection<DBAdminTable> FilesProcessedTable { get; set; } = new();

        [Reactive]
        public ObservableCollection<AlertsObject> _AlertTable { get; set; } = new();

        [Reactive]
        public ObservableCollection<EventObject> _ErrorAlertsCollection { get; set; } = new();
        #endregion getters and setters for Binding

        #region constructors

        /// <summary>
        /// The main constructor that is called when this class is initialized
        /// Must be passed a instance of DBService
        /// </summary>
        /// <param name="db"></param>
        public MainWindowViewModel(IDBService? db, IAlertStatus? loggingTargetSource)
        {
            IList<AlertsObject> alerts = loggingTargetSource!.GetAllAlerts(page:0);

            foreach(AlertsObject alert in alerts)
            {
                alert.CreateAlertWindow = new OpenAlertWindow(alert);
                _AlertTable.Add(alert);
            }

            IList<EventObject> events = loggingTargetSource.GetAllEvents(page:0);
            foreach (EventObject e in events)
            {
                e.open_Event_Window = new OpenEventWindow(e);
                _ErrorAlertsCollection.Add(e);
            }

        }

        //dependency inversion for UI
        public MainWindowViewModel() : this(
            Locator.Current.GetService<IDBService>(),
            Locator.Current.GetService<IAlertStatus>()
            )
        {

        }
        #endregion constructors

        #region Methods


        /// <summary>
        /// This method creates and opens a new window in which to resolve alerts
        /// Sets the ResolveAlertsViewmodel as the datacontext
        /// </summary>
        /// <param name="alertObjectToPass"> AlertObject object that serves as Window Initialization</param>
        public static string DisplayAlertWindow(AlertsObject alertObjectToPass)
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
                e.AsExtractException("ELI53755").Display();
            }

            if (result == null)
            {
                result = "";
            }

            return "";
        }

        /// <summary>
        /// This method creates a new window from data from the database (_dbService)
        /// Initalizes the window with the instance of current database being used
        /// <paramref name="errorObject"/>
        /// </param>
        /// </summary>
        public static string DisplayEventsWindow(EventObject errorObject)
        {
            string? result = "";

            EventsOverallView eventsWindow = new ();
            EventsOverallViewModel eventsWindowView = new EventsOverallViewModel(errorObject, eventsWindow);
            eventsWindow.DataContext = eventsWindowView;

            try
            {
                result = eventsWindow.ShowDialog<string>(CurrentInstance?.MainWindow).ToString();
            }
            catch (Exception e)
            {
                e.AsExtractException("ELI53756").Display();
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
                e.AsExtractException("ELI53757").Display();
            }

            if(result == null)
            {
                result = "";
            }

            return result;
        }


        #endregion Methods
    }
}
