using System;
using Extract.Utilities.WPF;
using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Services;

namespace Extract.Web.ApiConfiguration.ViewModels
{
    public interface IApiConfigViewModelFactory
    {
        ICommonApiConfigViewModel Create(ConfigurationForEditing wrapper);
    }

    public class ApiConfigViewModelFactory : IApiConfigViewModelFactory
    {
        readonly IObservableConfigurationDatabaseService _configurationDatabase;
        readonly IFileBrowserDialogService _fileBrowserDialogService;

        public ApiConfigViewModelFactory(
            IObservableConfigurationDatabaseService configurationDatabaseService,
            IFileBrowserDialogService fileBrowserDialogService)
        {
            _configurationDatabase = configurationDatabaseService;
            _fileBrowserDialogService = fileBrowserDialogService;
        }

        public ICommonApiConfigViewModel Create(ConfigurationForEditing wrapper)
        {
            if (wrapper?.Configuration is IRedactionWebConfiguration redactionConfig)
            {
                return new RedactionApiConfigViewModel(
                    wrapper.NameColumn,
                    redactionConfig,
                    _configurationDatabase,
                    _fileBrowserDialogService);
            }
            else if (wrapper?.Configuration is IDocumentApiWebConfiguration documentConfig)
            {
                return new DocumentApiConfigViewModel(
                    wrapper.NameColumn,
                    documentConfig,
                    _configurationDatabase,
                    _fileBrowserDialogService);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}