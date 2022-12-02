using DynamicData;
using DynamicData.Binding;
using Extract.Utilities.WPF;
using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Extract.Web.ApiConfiguration.ViewModels
{
    public class ApiConfigMgmtViewModel : ViewModelBase, IDisposable
    {
        const string _DOCUMENT_API_TYPE = "DocumentAPI";
        const string _REDACTION_TYPE = "Redaction";

        readonly CompositeDisposable _disposables = new();
        bool _isDisposed;

        readonly IObservableConfigurationDatabaseService _configurationDatabase;
        readonly IApiConfigViewModelFactory _apiConfigurationViewModelFactory;
        readonly IMessageDialogService _messageDialogService;

        readonly SourceCache<ConfigurationForEditing, Guid> _configurationsSourceCache =
            new(wrapper => wrapper.Configuration.ID ?? Guid.Empty);

        readonly ReadOnlyObservableCollection<ICommonApiConfigViewModel> _configurations;

        public IList<string> ConfigurationTypes { get; } = new string[] { _DOCUMENT_API_TYPE, _REDACTION_TYPE };

        /// <summary>
        /// The web API configurations
        /// </summary>
        public ReadOnlyObservableCollection<ICommonApiConfigViewModel> Configurations => _configurations;

        [Reactive]
        public Guid? SelectedConfigurationID { get; set; }

        [Reactive]
        public string NewConfigurationType { get; set; }

        [ObservableAsProperty]
        public bool HasAddNewConfiguration { get; }

        [Reactive]
        bool ShowAddNewConfiguration { get; set; }

        [ObservableAsProperty]
        public ICommonApiConfigViewModel SelectedConfiguration { get; }

        [ObservableAsProperty]
        public bool HasSelectedConfiguration { get; }

        public ReactiveCommand<Unit, Unit> ShowAddConfigurationCommand { get; }

        public ReactiveCommand<Unit, Unit> AddConfigurationCommand { get; }

        public ReactiveCommand<Unit, Unit> DeleteSelectedConfigurationCommand { get; }

        public ReactiveCommand<Unit, Unit> CloseCommand { get; }

        /// <summary>
        /// Create an instance of the VM
        /// </summary>
        public ApiConfigMgmtViewModel(
            IObservableConfigurationDatabaseService configurationDatabase,
            IApiConfigViewModelFactory apiConfigurationViewModelFactory,
            IMessageDialogService messageDialogService)
        {
            _configurationDatabase = configurationDatabase;
            _apiConfigurationViewModelFactory = apiConfigurationViewModelFactory;
            _messageDialogService = messageDialogService;

            // Maintain the configuration models as a source cache for the view models
            _configurationDatabase.ConfigurationsForEditing.Subscribe(configsFromDB =>
            {
                _configurationsSourceCache.Edit(cache =>
                {
                    IList<ConfigurationForEditing> configsToReplace;

                    if (_configurations is null || !_configurations.Any())
                    {
                        configsToReplace = configsFromDB;
                    }
                    else
                    {
                        // Don't update items from the DB if they have uncomitted changes
                        var configsToPreserve = _configurations.Where(vm => vm.IsDirty && !vm.IsSaving)
                            .Select(vm => vm.ID)
                            .ToHashSet();
                        configsToReplace = configsFromDB
                            .Where(c => !configsToPreserve.Contains(c.Configuration.ID ?? Guid.Empty))
                            .ToList();

                        // Remove any items from the cache that are no longer in the DB
                        var configIDsToRemove = _configurations
                            .Select(vm => vm.ID)
                            .ToHashSet();
                        configIDsToRemove.ExceptWith(configsFromDB.Select(c => c.Configuration.ID ?? Guid.Empty));
                        configIDsToRemove.ExceptWith(configsToPreserve);
                        cache.RemoveKeys(configIDsToRemove);
                    }

                    cache.AddOrUpdate(configsToReplace);
                });
            }).DisposeWith(_disposables);

            // Transform the configuration models into view models
            _configurationsSourceCache
              .Connect()
              .TransformWithInlineUpdate(
                _apiConfigurationViewModelFactory.Create,
                (configVM, config) => configVM.UpdateFromModel(config.Configuration),
                error => RxApp.DefaultExceptionHandler.OnNext(error.Exception))
              .SortBy(config => config.WorkflowName + config.ConfigurationName)
              .Bind(out _configurations)
              .DisposeMany()
              .Subscribe()
              .DisposeWith(_disposables);

            // SelectedConfiguration
            this.WhenAnyValue(x => x.SelectedConfigurationID)
                .Select(guid => Configurations.FirstOrDefault(c => c.ID == guid))
                .ToPropertyEx(this, x => x.SelectedConfiguration);

            // HasAddNewConfiguration
            this.WhenAnyValue(x => x.SelectedConfigurationID, x => x.ShowAddNewConfiguration,
                (selectedID, showNew) => selectedID is null && showNew)
                .ToPropertyEx(this, x => x.HasAddNewConfiguration);

            // HasSelectedConfiguration
            this.WhenAnyValue(x => x.SelectedConfiguration)
                .Select(config => config is not null)
                .ToPropertyEx(this, x => x.HasSelectedConfiguration);

            ShowAddConfigurationCommand = ReactiveCommand.Create(ShowAddConfiguration);

            AddConfigurationCommand = ReactiveCommand.Create(AddConfiguration,
                this.WhenAnyValue(x => x.NewConfigurationType).Select(type => !string.IsNullOrEmpty(type)));

            DeleteSelectedConfigurationCommand = ReactiveCommand.Create(
                DeleteSelectedConfiguration,
                this.WhenAnyValue(x => x.SelectedConfiguration).Select(configuration => configuration is not null));

            CloseCommand = ReactiveCommand.Create(() => MessageBus.Current.SendMessage(CloseWindowMessage.Instance));
        }

        public bool HasUnsavedChanges()
        {
            try
            {
                return Configurations.Where(c => c.IsDirty).Any();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53770");
            }
        }
        public void SaveAllChanges()
        {
            try
            {
                var unsavedConfigs = Configurations.Where(c => c.IsDirty).ToList();
                foreach (var configVM in unsavedConfigs)
                {
                    configVM.Save();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53801");
            }
        }

        void ShowAddConfiguration()
        {
            SelectedConfigurationID = null;
            ShowAddNewConfiguration = true;
        }

        void AddConfiguration()
        {
            ICommonWebConfiguration emptyConfig =
                NewConfigurationType switch
                {
                    _DOCUMENT_API_TYPE => new DocumentApiConfiguration() { ID = Guid.NewGuid() },
                    _REDACTION_TYPE => new RedactionWebConfiguration() { ID = Guid.NewGuid() },
                    _ => throw new NotSupportedException()
                };

            _configurationsSourceCache.AddOrUpdate(new ConfigurationForEditing("New Configuration", emptyConfig));
            SelectedConfigurationID = emptyConfig.ID;
            ShowAddNewConfiguration = false;
        }

        void DeleteSelectedConfiguration()
        {
            ICommonApiConfigViewModel configToDelete = SelectedConfiguration;

            if (configToDelete is null)
            {
                return;
            }

            if (_messageDialogService.ShowYesNoDialog(
                "Confirm Removal",
                $"Permanently remove configuration '{configToDelete.ConfigurationName}' from the database?")
                == MessageDialogResult.No)
            {
                return;
            }

            // The reload logic won't remove a modified configuration from the cache
            // when refreshing from the DB so remove it explicitly
            if (configToDelete.IsDirty)
            {
                _configurationsSourceCache.RemoveKey(configToDelete.ID);
            }

            _configurationDatabase.DeleteConfiguration(configToDelete.ID);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Should this always be disposed instead?
                    _disposables.Dispose();
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
