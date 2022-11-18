using AvaloniaDashboard.Interfaces;
using AvaloniaDashboard.Models.AllDataClasses;
using AvaloniaDashboard.Models.AllEnums;
using AvaloniaDashboard.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive;

namespace AvaloniaDashboard.ViewModels
{
    /// <summary>
    /// This Class impliments ReactiveObject
    /// </summary>
    public class ConfigureAlertsViewModel : ReactiveObject
    {

        private ConfigureAlertsView? thisWindow;

        #region fields
        [Reactive]
        public string AlertName { get; set; }
        [Reactive]
        public List<string> AlertList { get; set; }
        [Reactive]
        public List<string> EventList { get; set; }
        [Reactive]
        public float TimeMinutes { get; set; }
        //TODO with eli codes set up w/ radio button...
        [Reactive]
        public string? Elicode { get; set; }
        [Reactive]
        public ErrorSeverityEnum? SeverityType { get; set; }

        //radiobutton bindings
        [Reactive]
        public bool EliRadioButton { get; set; }
        [Reactive]
        public bool SeverityRadioButton { get; set; }
        [Reactive]
        public bool IgnoreStateRadioButton { get; set; }
        [Reactive]
        public bool IgnoreEventRadioButton { get; set; }
        [Reactive]
        public bool DisableAlertRadioButton { get; set; }

        private IDBService? dbService;

        //todo make configuration object
        //todo bindings
        //todo methods

        #endregion fields

        public ConfigureAlertsViewModel(ConfigureAlertsView configureAlertsTable) : this(Locator.Current.GetService<IDBService>(), new())
        {

        }

        public ConfigureAlertsViewModel(IDBService? db, ConfigureAlertsView configureAlertsTable)
        {
            dbService = db;
            thisWindow = configureAlertsTable;
        }

        public void RefreshScreen()
        {

        }

        public void RefreshConfigurations()
        {

        }

        public void CreateConfiguration()
        {
            //will need a save to json
        }

        private void CloseWindow() //todo switch to dialog later
        {
            if(thisWindow != null)
            {
                thisWindow.Close("Refresh");
            }
        }
    }
}