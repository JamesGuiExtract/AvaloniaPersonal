using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Extract.Web.ApiConfiguration.ViewModels
{
    public abstract class CommonApiConfigViewModel : ViewModelBase, ICommonApiConfigViewModel, IDisposable
    {
        bool _isDisposed;

        protected CompositeDisposable Disposables { get; } = new();

        protected IObservableConfigurationDatabaseService ConfigurationDatabase { get; }

        public Guid ID { get; protected set; }

        [ObservableAsProperty]
        public IList<string> AllWorkflows { get; }

        [ObservableAsProperty]
        public IList<string> MainSequenceActions { get; }

        [ObservableAsProperty]
        public IList<string> NonMainSequenceActions { get; }

        [ObservableAsProperty]
        public IList<string> AllAttributeSets { get; }

        [ObservableAsProperty]
        public IList<string> AllMetadataFields { get; }

        [ObservableAsProperty]
        public string ConfigurationNamePlus { get; }

        [ObservableAsProperty]
        public bool IsDirty { get; }

        public bool IsSaving { get; private set; }

        [Reactive]
        public string ConfigurationName { get; set; }

        [Reactive]
        public bool IsDefault { get; set; }

        [Reactive]
        public string WorkflowName { get; set; }

        public abstract string ConfigurationDisplayType { get; }

        public abstract Type ConfigurationType { get; }

        [Reactive]
        protected ICommonWebConfiguration CurrentConfiguration { get; set; }

        [Reactive]
        protected ICommonWebConfiguration SavedConfiguration { get; set; }

        [Reactive]
        public string AttributeSet { get; set; }

        [Reactive]
        public string ProcessingAction { get; set; }

        [Reactive]
        public string PostProcessingAction { get; set; }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        /// <summary>
        /// Setup common logic
        /// </summary>
        protected CommonApiConfigViewModel(IObservableConfigurationDatabaseService configurationDatabase)
        {
            try
            {
                ConfigurationDatabase = configurationDatabase;
                SetupObservableAsProperties();
                SetupValidationRules();
                SaveCommand = ReactiveCommand.Create(Save, this.WhenAnyValue(x => x.IsDirty));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53768");
            }
        }

        /// <summary>
        /// Save this configuration to the database, which will trigger a reload via the ApiConfigMgmtViewModel
        /// </summary>
        public void Save()
        {
            IsSaving = true;
            try
            {
                ConfigurationDatabase.SaveConfiguration(CurrentConfiguration);
            }
            catch (Exception ex)
            {
                IsSaving = false;
                throw ex.AsExtract("ELI53765");
            }
        }

        /// <summary>
        /// Set all properties and the saved configuration to the supplied configuration
        /// </summary>
        public void UpdateFromModel(ICommonWebConfiguration config)
        {
            try
            {
                _ = config ?? throw new ArgumentNullException(nameof(config));

                SavedConfiguration = config;

                SetPropertiesFromModel(config);

                IsSaving = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53898");
            }
        }

        /// <summary>
        /// Set all properties to the supplied configuration
        /// </summary>
        protected abstract void SetPropertiesFromModel(ICommonWebConfiguration config);

        void SetupObservableAsProperties()
        {
            // AllWorkflows
            ConfigurationDatabase.Workflows
                .Select(workflows => workflows.Select(wf => wf.WorkflowName).ToList())
                .ToPropertyEx(this, x => x.AllWorkflows)
                .DisposeWith(Disposables);

            // MainSequenceActions
            Observable.CombineLatest(this.WhenAnyValue(x => x.WorkflowName),
                ConfigurationDatabase.Workflows,
                ConfigurationDatabase.MainSequenceWorkflowActions,
                (workflowName, allWorkflows, workflowActions) =>
                    workflowActions
                    .Where(workflowAction => !IsWorkflowValid(workflowName, allWorkflows)
                        || workflowAction.WorkflowName.Equals(workflowName, StringComparison.OrdinalIgnoreCase))
                    .Select(workflowAction => workflowAction.ActionName)
                    .ToList())
                .ToPropertyEx(this, x => x.MainSequenceActions)
                .DisposeWith(Disposables);

            // NonMainSequenceActions
            Observable.CombineLatest(this.WhenAnyValue(x => x.WorkflowName),
                ConfigurationDatabase.Workflows,
                ConfigurationDatabase.NonMainSequenceWorkflowActions,
                (workflowName, allWorkFlows, workflowActions) =>
                    workflowActions
                    .Where(workflowAction => !IsWorkflowValid(workflowName, allWorkFlows)
                        || workflowAction.WorkflowName.Equals(workflowName, StringComparison.OrdinalIgnoreCase))
                    .Select(workflowAction => workflowAction.ActionName)
                    .ToList())
                .ToPropertyEx(this, x => x.NonMainSequenceActions)
                .DisposeWith(Disposables);

            // AllAttributeSets
            ConfigurationDatabase.AttributeSetNames
                .ToPropertyEx(this, x => x.AllAttributeSets)
                .DisposeWith(Disposables);

            // AllMetadataFields
            ConfigurationDatabase.MetadataFieldNames
                .ToPropertyEx(this, x => x.AllMetadataFields)
                .DisposeWith(Disposables);

            // IsDirty
            this.WhenAnyValue(x => x.CurrentConfiguration, x => x.SavedConfiguration,
                (currentConfig, savedConfig) => currentConfig is not null && !currentConfig.Equals(savedConfig))
                .ToPropertyEx(this, x => x.IsDirty);

            // ConfigurationNamePlus
            this.WhenAnyValue(x => x.ConfigurationName, x => x.IsDirty,
                (name, isDirty) => isDirty ? "*" + name : name)
                .ToPropertyEx(this, x => x.ConfigurationNamePlus);
        }

        static bool IsWorkflowValid(string workflowName, IList<Workflow> allWorkflows)
        {
            return !string.IsNullOrEmpty(workflowName)
                && allWorkflows is not null
                && allWorkflows.Select(w => w.WorkflowName).Contains(workflowName, StringComparer.OrdinalIgnoreCase);
        }

        void SetupValidationRules()
        {
            // ConfigurationName
            var nameIsValid =
                Observable.CombineLatest
                (this.WhenAnyValue(x => x.ConfigurationName)
                , ConfigurationDatabase.ConfigurationsForEditing
                , (configName, allConfigs) =>
                {
                    return new ItemIsUniqueValidationResult
                    {
                        ItemIsEmpty = string.IsNullOrWhiteSpace(configName),
                        ItemIsUnique = allConfigs is not null && !allConfigs
                            .Where(config => config.Configuration.ID != ID)
                            .Any(config => string.Equals(config.Configuration.ConfigurationName, configName, StringComparison.OrdinalIgnoreCase))
                    };
                });
            this.ValidationRule(x => x.ConfigurationName, nameIsValid,
                state => !state.ItemIsEmpty && state.ItemIsUnique,
                state => state.ItemIsEmpty
                    ? "Configuration name cannot be empty!"
                    : $"Configuration name '{ConfigurationName}' already exists in the database!")
                // Dispose this because it is connected to the ConfigurationDatabase, which has a longer lifetime than this VM
                .DisposeWith(Disposables);

            // IsDefault
            var isDefaultIsValid =
                Observable.CombineLatest
                (this.WhenAnyValue(x => x.IsDefault)
                , this.WhenAnyValue(x => x.WorkflowName)
                , ConfigurationDatabase.ConfigurationsForEditing
                , (isDefault, workflowName, allConfigs) =>
                {
                    return !isDefault
                        || allConfigs is not null && !allConfigs.Any(wrapper =>
                            {
                                var other = wrapper.Configuration;
                                return other.ID != ID
                                    && other.GetType() == ConfigurationType
                                    && other.IsDefault
                                    && other.WorkflowName.Equals(workflowName, StringComparison.OrdinalIgnoreCase);
                            });
                });
            this.ValidationRule(x => x.IsDefault, isDefaultIsValid,
                state => state,
                state => "Another configuration is the default for this workflow!")
                // Dispose this because it is connected to the ConfigurationDatabase, which has a longer lifetime than this VM
                .DisposeWith(Disposables);

            // WorkflowName
            this.ValidationRule(x => x.WorkflowName,
                CreateItemExistsValidationObservableFromStringList(x => x.WorkflowName, x => x.AllWorkflows),
                state => state.IsValid,
                state => state.ItemIsEmpty
                    ? "Workflow name cannot be empty!"
                    : $"'{WorkflowName}' workflow does not exist!");

            // AttributeSet
            this.ValidationRule(x => x.AttributeSet,
                CreateItemExistsValidationObservableFromStringList(x => x.AttributeSet, x => x.AllAttributeSets),
                state => state.IsValid,
                state => state.ItemIsEmpty
                    ? "Attribute set cannot be empty!"
                    : $"'{AttributeSet}' attribute set does not exist!");
        }

        IObservable<ItemExistsValidationResult> CreateItemExistsValidationObservableFromStringList(
            Expression<Func<CommonApiConfigViewModel, string>> stringProp,
            Expression<Func<CommonApiConfigViewModel, IList<string>>> stringListProp)
        {
            return Observable.CombineLatest
                (this.WhenAnyValue(stringProp)
                , this.WhenAnyValue(stringListProp)
                , (item, items) =>
                {
                    return new ItemExistsValidationResult()
                    {
                        ItemIsEmpty = string.IsNullOrWhiteSpace(item),
                        ItemExists = items is not null && items.Contains(item, StringComparer.OrdinalIgnoreCase)
                    };
                });
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Disposables.Dispose();
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
