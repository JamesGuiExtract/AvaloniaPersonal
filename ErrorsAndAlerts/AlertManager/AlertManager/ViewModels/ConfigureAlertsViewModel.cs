using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using AlertManager.Services;
using AlertManager.Views;
using Extract.ErrorHandling;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive;

namespace AlertManager.ViewModels
{
    /// <summary>
    /// This Class impliments ReactiveObject
    /// </summary>
    public class ConfigureAlertsViewModel : ReactiveObject
    {

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

        public IDBService? GetService { get => dbService; }

        //todo make configuration object
        //todo bindings
        //todo methods

        #endregion fields

        public ConfigureAlertsViewModel() : this(Locator.Current.GetService<IDBService>())
        {

        }

        public ConfigureAlertsViewModel(IDBService? databaseService)
        {
            if(databaseService == null)
            {
                databaseService = new DBService();
                new ExtractException("ELI53774", "issue passing in instance of db service").Log();
            }
            dbService = databaseService;
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

    }
}