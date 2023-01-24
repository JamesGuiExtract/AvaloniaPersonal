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
            try
            {
                if (databaseService == null)
                {
                    databaseService = new DBService();
                    throw new ExtractException("ELI53774", "Database service is " + databaseService.ToString());
                }
            }
            catch(Exception e)
            {
                ExtractException ex = new("ELI53858", "Issue passing in instance of db service,", e);
                throw ex;
            }
            dbService = databaseService;
        }

    }
}