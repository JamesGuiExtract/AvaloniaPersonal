using AlertManager.Interfaces;

namespace AlertManager.ViewModels
{
    /// <summary>
    /// This Class impliments ReactiveObject
    /// </summary>
    public class ConfigureAlertsViewModel : ViewModelBase
    {

        #region fields


        private readonly IDBService _dbService;

        public IDBService GetService { get => _dbService; }

        //todo make configuration object
        //todo bindings
        //todo methods

        #endregion fields

        public ConfigureAlertsViewModel(IDBService databaseService)
        {
            _dbService = databaseService;
        }
    }
}