using AlertManager.Interfaces;
using AlertManager.Services;
using Extract.ErrorHandling;
using ReactiveUI;
using Splat;
using System;
using UCLID_FILEPROCESSINGLib;

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
                    databaseService = new DBService(new FileProcessingDB());
                    ExtractException ex = new ExtractException("ELI53774", "Database service is " + databaseService.ToString());
                    RxApp.DefaultExceptionHandler.OnNext(ex);
                }
            }
            catch(Exception e)
            {
                ExtractException ex = new("ELI53858", "Issue passing in instance of db service,", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }
            dbService = databaseService;
        }

    }
}