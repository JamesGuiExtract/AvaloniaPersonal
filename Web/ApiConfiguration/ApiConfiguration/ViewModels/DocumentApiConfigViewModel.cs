using DynamicData;
using DynamicData.Binding;
using Extract.Utilities.WPF;
using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;

namespace Extract.Web.ApiConfiguration.ViewModels
{
    public sealed class DocumentApiConfigViewModel : CommonApiConfigViewModel
    {
        readonly IFileBrowserDialogService _fileBrowserDialogService;

        public override string ConfigurationDisplayType => "Document";

        public override Type ConfigurationType { get; } = typeof(DocumentApiConfiguration);

        [Reactive]
        public string DocumentFolder { get; set; }

        [Reactive]
        public string StartWorkflowAction { get; set; }

        [Reactive]
        public string EndWorkflowAction { get; set; }

        [Reactive]
        public string PostWorkflowAction { get; set; }

        [Reactive]
        public string OutputFileNameMetadataField { get; set; }

        [Reactive]
        public string OutputFileNameMetadataInitialValueFunction { get; set; }

        public ReactiveCommand<Unit, Unit> SelectDocumentFolderCommand { get; }

        public DocumentApiConfigViewModel(
            string nameFromDatabase,
            IDocumentApiWebConfiguration documentConfig,
            IObservableConfigurationDatabaseService configurationDatabase,
            IFileBrowserDialogService fileBrowserDialogService)
            : base(configurationDatabase)
        {
            try
            {
                SavedConfiguration =
                    CurrentConfiguration = documentConfig ?? throw new ArgumentNullException(nameof(documentConfig));

                _fileBrowserDialogService = fileBrowserDialogService ?? throw new ArgumentNullException(nameof(fileBrowserDialogService));

                SetPropertiesFromModel(documentConfig);

                // Setup change detection
                SetupObservableAsProperties();

                // Now that the change detection is setup, adjust the name to what was stored in the database
                // so that if it is different this object will be considered dirty
                ConfigurationName = nameFromDatabase;

                SetupValidationRules();
                SelectDocumentFolderCommand = ReactiveCommand.Create(SelectDocumentFolder);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53897");
            }
        }

        /// <inheritdoc/>
        protected override void SetPropertiesFromModel(ICommonWebConfiguration config)
        {
            var documentConfig = config as IDocumentApiWebConfiguration ?? throw new ArgumentException("Unexpected type", nameof(config));

            ID = documentConfig.ID ?? Guid.NewGuid();
            ConfigurationName = documentConfig.ConfigurationName;
            IsDefault = documentConfig.IsDefault;
            WorkflowName = documentConfig.WorkflowName;
            AttributeSet = documentConfig.AttributeSet;
            ProcessingAction = documentConfig.ProcessingAction;
            PostProcessingAction = documentConfig.PostProcessingAction;
            DocumentFolder = documentConfig.DocumentFolder;
            StartWorkflowAction = documentConfig.StartWorkflowAction;
            EndWorkflowAction = documentConfig.EndWorkflowAction;
            PostWorkflowAction = documentConfig.PostWorkflowAction;
            OutputFileNameMetadataField = documentConfig.OutputFileNameMetadataField;
            OutputFileNameMetadataInitialValueFunction = documentConfig.OutputFileNameMetadataInitialValueFunction;
        }

        IDocumentApiWebConfiguration CreateModel()
        {
            return new DocumentApiConfiguration(
                configurationName: ConfigurationName,
                isDefault: IsDefault,
                workflowName: WorkflowName,
                attributeSet: AttributeSet,
                processingAction: ProcessingAction,
                postProcessingAction: PostProcessingAction,
                documentFolder: DocumentFolder,
                startAction: StartWorkflowAction,
                endAction: EndWorkflowAction,
                postWorkflowAction: PostWorkflowAction,
                outputFileNameMetadataField: OutputFileNameMetadataField,
                outputFileNameMetadataInitialValueFunction: OutputFileNameMetadataInitialValueFunction)
            { ID = ID };
        }

        void SetupObservableAsProperties()
        {
            // CurrentConfiguration
            this.WhenAnyPropertyChanged(
                nameof(ConfigurationName),
                nameof(IsDefault),
                nameof(WorkflowName),
                nameof(AttributeSet),
                nameof(ProcessingAction),
                nameof(PostProcessingAction),
                nameof(DocumentFolder),
                nameof(StartWorkflowAction),
                nameof(EndWorkflowAction),
                nameof(PostWorkflowAction),
                nameof(OutputFileNameMetadataField),
                nameof(OutputFileNameMetadataInitialValueFunction))
                .Select(vm => vm.CreateModel())
                .BindTo(this, x => x.CurrentConfiguration);
        }

        void SetupValidationRules()
        {
            // DocumentFolder
            var docFolderIsValid =
                this.WhenAnyValue(x => x.DocumentFolder)
                .Select(docFolder =>
                {
                    return new ItemExistsValidationResult
                    {
                        ItemIsEmpty = string.IsNullOrWhiteSpace(docFolder),
                        ItemExists = Directory.Exists(docFolder)
                    };
                });
            this.ValidationRule(x => x.DocumentFolder, docFolderIsValid,
                state => !state.ItemIsEmpty && state.ItemExists,
                state => state.ItemIsEmpty ? "Document folder cannot be empty!" : "Document folder does not exist!");

            // ProcessingAction
            this.ValidationRule(x => x.ProcessingAction,
                CreateItemExistsValidationObservableFromStringList(x => x.ProcessingAction, x => x.MainSequenceActions),
                state => state.IsValid,
                state => state.ItemIsEmpty
                    ? "Processing action cannot be empty!"
                    : $"'{ProcessingAction}' is not a main-sequence action!");

            // PostProcessingAction
            this.ValidationRule(x => x.PostProcessingAction,
                CreateItemExistsValidationObservableFromStringList(x => x.PostProcessingAction, x => x.MainSequenceActions),
                state => state.ItemIsEmpty || state.IsValid,
                state => $"'{PostProcessingAction}' is not a main-sequence action!");

            // StartWorkflowAction
            this.ValidationRule(x => x.StartWorkflowAction,
                CreateItemExistsValidationObservableFromStringList(x => x.StartWorkflowAction, x => x.MainSequenceActions),
                state => state.IsValid,
                state => state.ItemIsEmpty
                    ? "Start-workflow action cannot be empty!"
                    : $"'{StartWorkflowAction}' is not a main-sequence action!");

            // EndWorkflowAction
            this.ValidationRule(x => x.EndWorkflowAction,
                CreateItemExistsValidationObservableFromStringList(x => x.EndWorkflowAction, x => x.MainSequenceActions),
                state => state.IsValid,
                state => state.ItemIsEmpty
                    ? "End-workflow action cannot be empty!"
                    : $"'{EndWorkflowAction}' is not a main-sequence action!");

            // PostWorkflowAction
            this.ValidationRule(x => x.PostWorkflowAction,
                CreateItemExistsValidationObservableFromStringList(x => x.PostWorkflowAction, x => x.NonMainSequenceActions),
                state => state.ItemIsEmpty || state.IsValid,
                state => $"'{PostWorkflowAction}' is not a non-main-sequence action!");

            // OutputFileNameMetadataField
            this.ValidationRule(x => x.OutputFileNameMetadataField,
                CreateItemExistsValidationObservableFromStringList(x => x.OutputFileNameMetadataField, x => x.AllMetadataFields),
                state => state.IsValid,
                state => state.ItemIsEmpty
                    ? "Output-filename metadata field cannot be empty!"
                    : $"'{OutputFileNameMetadataField}' metadata field does not exist!");
        }

        IObservable<ItemExistsValidationResult> CreateItemExistsValidationObservableFromStringList(
            Expression<Func<DocumentApiConfigViewModel, string>> stringProp,
            Expression<Func<DocumentApiConfigViewModel, IList<string>>> stringListProp)
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

        void SelectDocumentFolder()
        {
            if (_fileBrowserDialogService.SelectFolder("Select document folder")
                is string selectedPath)
            {
                DocumentFolder = selectedPath;
            }
        }
    }
}