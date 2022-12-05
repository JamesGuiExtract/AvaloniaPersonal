using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using AlertManager.Views;
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

        public ConfigureAlertsViewModel(IDBService? db)
        {
            dbService = db;
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