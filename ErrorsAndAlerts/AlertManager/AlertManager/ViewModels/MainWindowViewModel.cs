using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Models.TreeDataGrid;
using AvaloniaDashboard.Interfaces;
using AvaloniaDashboard.Models;
using AvaloniaDashboard.Models.AllDataClasses;
using AvaloniaDashboard.Models.AllEnums;
using AvaloniaDashboard.Services;
using AvaloniaDashboard.Views;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AvaloniaDashboard.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        #region fields
        //this is a private observable collection of type DBAdminTable
        private ObservableCollection<DBAdminTable> FilesProcessedTable = new();
        private ObservableCollection<AlertsObject> _AlertTable = new();
        private ObservableCollection<EventObject> _ErrorAlertsCollection = new();

        public static IClassicDesktopStyleApplicationLifetime? CurrentInstance = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        private IMainWindowContextMenu? ContextMenuInterface;
        #endregion fields

        #region getters and setters for Binding
        [Reactive]
        public FlatTreeDataGridSource<DBAdminTable>? Source { get; set; }

        [Reactive]
        public FlatTreeDataGridSource<AlertsObject>? AlertsSource { get; set; } 

        [Reactive]
        public FlatTreeDataGridSource<EventObject>? ErrorSource { get; set; }

        #endregion getters and setters for Binding

        #region constructors
        /// <summary>
        /// The main constructor that is called when this class is initialized
        /// Must be passed a instance of DBService
        /// </summary>
        /// <param name="db"></param>
        public MainWindowViewModel(IDBService? db)
        {
            //sets the interfaces to their implimentation
            SetContextMenu(new MainWindowContextMenuTextbox());

            FileProcessedTable();

            //TODO remove this later as well
            AlertsObject testObject = new AlertsObject(0, 0, "TestAction", "TestType", "testconfig", new DateTime(2008, 5, 1, 8, 30, 52), "testUser", "testMachine", "testResolution", TypeOfResolutionAlerts.Snoozed, new DateTime(2008, 5, 1, 8, 30, 52), "testingAlertHistory");
            _AlertTable.Add(testObject);

            //TODO remove this later 
            EventObject errorObject = new EventObject("testEliCode", "testMessage", 12, true, new DateTime(2008, 5, 1, 8, 30, 52), ErrorSeverityEnum.medium, "no details", new MachineAndCustomerInformation(), "some stuff sfsaafds");
            _ErrorAlertsCollection.Add(errorObject);

            CreateAlertTable(testObject);
            ErrorTable(errorObject);
        }

        //dependency inversion for UI
        public MainWindowViewModel() : this(Locator.Current.GetService<IDBService>())
        {

        }
        #endregion constructors

        #region interface setters
        private void SetContextMenu(IMainWindowContextMenu interfaceToSet)
        {
            ContextMenuInterface = interfaceToSet;
        }
        #endregion interface setters

        #region Methods
        /// <summary>
        /// Creates the context menu for a item in a table
        /// </summary>
        /// <param name="textBoxValue">string is th textbox value to create</param>
        /// <returns></returns>
        public TextBlock CreateTableItemWithContextMenu(string textBoxValue)
        {
            List<MenuItem> listOfMenuItems = new List<MenuItem>();

            if(ContextMenuInterface == null)
            {
                return new();
            }
            
            MenuItem button1 = ContextMenuInterface.CreateMenuItem("Manually Set Action Status...");
            MenuItem button2 = ContextMenuInterface.CreateMenuItem("Export File List...");
            MenuItem button3 = ContextMenuInterface.CreateMenuItem("Inspect Files...");
            MenuItem button4 = ContextMenuInterface.CreateMenuItem("View Exceptions For Failed Files...");
            MenuItem button5 = ContextMenuInterface.CreateMenuItem("Copy Count");
            MenuItem button6 = ContextMenuInterface.CreateMenuItem("Cancel");

            listOfMenuItems.Add(button1);
            listOfMenuItems.Add(button2);
            listOfMenuItems.Add(button3);
            listOfMenuItems.Add(button4);
            listOfMenuItems.Add(button5);
            listOfMenuItems.Add(button6);

            //have the textblock be modified w/ INotifyWhatever so I can attach it via xaml
            return ContextMenuInterface.ContextMenuTextBlock(listOfMenuItems, textBoxValue);

        }

        /// <summary>
        /// Creates a Context menu based on the parameters below
        /// </summary>
        /// <param name="textBoxValue"></param>
        /// <returns></returns>
        public TextBlock CreateContextMenuViewExceptionFailedFiles(string textBoxValue)
        {
            List<MenuItem> listOfMenuItems = new List<MenuItem>();

            if (ContextMenuInterface == null)
            {
                return new();
            }

            
            MenuItem button1 = ContextMenuInterface.CreateMenuItem("View Exceptions for Failed Files");
            MenuItem button2 = ContextMenuInterface.CreateMenuItem("Copy Action Name");
            MenuItem button3 = ContextMenuInterface.CreateMenuItem("Cancel");

            listOfMenuItems.Add(button1);
            listOfMenuItems.Add(button2);
            listOfMenuItems.Add(button3);

            return ContextMenuInterface.ContextMenuTextBlock(listOfMenuItems, textBoxValue);
        }

        /// <summary>
        /// Creates a FlatTreeDataGridSource from the field collection _alertTable, uses Avolonia FileTreeDataGridSource -
        /// to create said table
        /// </summary>
        /// <param name="alertObject">AlertObject new window will be initialized with</param>
        public void CreateAlertTable(AlertsObject alertObject)
        {
            AlertsSource = new FlatTreeDataGridSource<AlertsObject>(_AlertTable)
            {
                Columns =
                {
                    new TextColumn<AlertsObject, string>("Action Type", x => x.action_Type),
                    //new TextColumn<AlertsObject, string>("Alert", y => y.AlertType), 
                    
                    new TemplateColumn<AlertsObject>(
                        "Alerts",
                        new DisplayAlertsWindowButton(new DisplayAlertWindowCommand(alertObject), "please figure out a way to change this")
                    ),
                    new TemplateColumn<AlertsObject>(
                        "Testing ContextMenu",
                        new ContextMenuTextBlock(CreateTableItemWithContextMenu("testing"))),
                    new TextColumn<AlertsObject, string>("Configuration", x => x.configuration),
                    new TextColumn<AlertsObject, DateTime>("Activated", x => x.activation_Time),
                    new TextColumn<AlertsObject, string>("User", x => x.user_Found),
                    new TextColumn<AlertsObject, string>("Machine", x => x.machine_Found_Error),
                    new TextColumn<AlertsObject, string>("Resolution", x => x.resolution_Type),
                    new TextColumn<AlertsObject, DateTime?>("Time Resolved", x => x.resolution_Time),

                }
            };
        }

        /// <summary>
        /// Creates a FlatTreeDataGridSource from the field collection filesProcessedTable
        /// </summary>
        public void FileProcessedTable()
        {
            Source = new FlatTreeDataGridSource<DBAdminTable>(FilesProcessedTable)
            {
                //creates the table columns
                Columns =
                {
                    new TextColumn<DBAdminTable, string>("Action", x => x.Action),
                    new TextColumn<DBAdminTable, int>("Unattempted", x => x.Unattempted), //TODO probs put somewhere easier for people to find, cant use bool in table unfortunetly
                    new TextColumn<DBAdminTable, int>("Pending", x => x.Pending),
                    new TextColumn<DBAdminTable, int>("Processing", x => x.Processing),
                    new TextColumn<DBAdminTable, int>("Complete", x => x.Complete),
                    new TextColumn<DBAdminTable, int>("Skipped", x => x.Skipped),
                    new TextColumn<DBAdminTable, int>("Failed", x => x.Failed),
                    new TextColumn<DBAdminTable, int>("Total", x => x.Total),

                },
            };
        }

        /// <summary>
        /// Creates a FlatTreeDataGridSource from the field collection _errorAlertsCollection
        /// </summary>
        /// <param name="errorObject">ErrorObject that is passed through to a function in a method</param>
        public void ErrorTable(EventObject errorObject)
        {
            ErrorSource = new FlatTreeDataGridSource<EventObject>(_ErrorAlertsCollection)
            {
                Columns =
                {
                    new TextColumn<EventObject, string>("Event Message", x => x.message),
                    new TextColumn<EventObject, int>("Debug #", x => x.number_Debug),
                    new TextColumn<EventObject, bool>("Has Stack Trace", x => x.contains_Stack_Trace),
                    new TextColumn<EventObject, DateTime>("Time Event Occurred", x => x.time_Of_Error),
                    new TextColumn<EventObject, ErrorSeverityEnum>("Severity of Event", x => x.severity_Of_Error),
                    new TemplateColumn<EventObject>(
                        "Error Details",
                        new DetailsButton(new EventsWindowDisplayer(errorObject))),
                    new TextColumn<EventObject, string>("Stack Trace", x => x.stack_Trace),
                    
                    new TextColumn<EventObject, string>("Additional Error Details", x => x.additional_Details)
                }
            };
        }//README this is for xaml writing only, use dependency injection otherwise, comment this out from working model


        /// <summary>
        /// ICommand Implementation for displaying StatsWindow, uses the method DisplayStatsWindow to create the window
        /// </summary>
        public class EventsWindowDisplayer : ICommand
        {
            public EventObject? errorObject;

            public EventsWindowDisplayer(EventObject errorObject)
            {
                this.errorObject = errorObject;
               
            }

            public bool CanExecute(object? param)
            {
                return true;
            }

            public event EventHandler? CanExecuteChanged;
            public void Execute(object? parameter)
            {
                if (CanExecuteChanged != null && errorObject != null)
                {
                    DisplayEventsWindow(errorObject);
                }
            }
        }

        /// <summary>
        /// class that implements ICommand interface, serves as a command that triggers method DisplayAlertWindow()
        /// param AlertsObject, is the object passed into DisplayAlertWindow that will initialize the values in the new window
        /// created by DisplayAlertWindow()
        /// </summary>
        public class DisplayAlertWindowCommand : ICommand
        {
            //field
            public AlertsObject? alertObjectToPass;

            public event EventHandler? CanExecuteChanged;

            //constructor
            public DisplayAlertWindowCommand(AlertsObject alertObjectToPass)
            {
                this.alertObjectToPass = alertObjectToPass;
            }

            //always exectues
            public bool CanExecute(object? parameter)
            {
                return true;
            }

            //what is executed, executes method DisplayAlertWindow
            public void Execute(object? parameter)
            {
                if (alertObjectToPass == null)
                {
                    alertObjectToPass = new();
                }
                DisplayAlertWindow(alertObjectToPass);
            }
        }

        /// <summary>
        /// This method creates and opens a new window in which to resolve alerts
        /// Sets the ResolveAlertsViewmodel as the datacontext
        /// </summary>
        /// <param name="alertObjectToPass"> AlertObject object that serves as Window Initialization</param>
        public static void DisplayAlertWindow(AlertsObject alertObjectToPass)
        {
            
            ResolveAlertsView resolveAlerts = new ResolveAlertsView();

            ResolveAlertsViewModel resolveAlertsViewModel = new ResolveAlertsViewModel(alertObjectToPass, resolveAlerts);
            resolveAlerts.DataContext = resolveAlertsViewModel;
            resolveAlerts.ShowDialog(CurrentInstance?.MainWindow);
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
                Console.WriteLine(e.StackTrace);
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
        public static void DisplayAlertsIgnoreWindow()
        {
            ConfigureAlertsView newWindow = new();
            ConfigureAlertsViewModel newWindowViewModel = new ConfigureAlertsViewModel(newWindow);
            newWindow.DataContext = newWindowViewModel;
            newWindow.ShowDialog(CurrentInstance?.MainWindow);
        }

        #endregion Methods
    }
}
