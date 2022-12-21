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
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Extract.Web.ApiConfiguration.ViewModels
{
    public sealed class RedactionApiConfigViewModel : CommonApiConfigViewModel
    {
        readonly IFileBrowserDialogService _fileBrowserDialogService;

        [ObservableAsProperty]
        public string VerificationSessionTimeoutMessage { get; }

        public override string ConfigurationDisplayType => "Redaction";

        public override Type ConfigurationType { get; } = typeof(RedactionWebConfiguration);

        [Reactive]
        public string ActiveDirectoryGroups { get; set; }

        public string ActiveDirectoryGroupsToolTip { get; } = "Separate group names with newline, semicolon or comma";

        [Reactive]
        public string RedactionTypes { get; set; }

        public string RedactionTypesToolTip { get; } = "Separate types with space, newline, semicolon or comma";

        [Reactive]
        public bool EnableAllUserPendingQueue { get; set; }

        [Reactive]
        public string DocumentTypeFileLocation { get; set; }

        public ReactiveCommand<Unit, Unit> SelectDocumentTypeFileCommand { get; }

        public RedactionApiConfigViewModel(
            string nameFromDatabase,
            IRedactionWebConfiguration redactionConfig,
            IObservableConfigurationDatabaseService configurationDatabase,
            IFileBrowserDialogService fileBrowserDialogService)
            : base(configurationDatabase)
        {
            try
            {
                SavedConfiguration =
                    CurrentConfiguration = redactionConfig ?? throw new ArgumentNullException(nameof(redactionConfig));

                _fileBrowserDialogService = fileBrowserDialogService ?? throw new ArgumentNullException(nameof(fileBrowserDialogService));

                SetPropertiesFromModel(redactionConfig);

                // Setup change detection
                SetupObservableAsProperties();

                // Now that the change detection is setup, adjust the name to what was stored in the database
                // so that if it is different this object will be considered dirty
                ConfigurationName = nameFromDatabase;

                SetupValidationRules();
                SelectDocumentTypeFileCommand = ReactiveCommand.Create(SelectDocumentTypeFile);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53896");
            }
        }

        /// <inheritdoc/>
        protected override void SetPropertiesFromModel(ICommonWebConfiguration config)
        {
            var redactionConfig = config as IRedactionWebConfiguration ?? throw new ArgumentException("Unexpected type", nameof(config));

            ID = redactionConfig.ID ?? Guid.NewGuid();
            ConfigurationName = redactionConfig.ConfigurationName;
            IsDefault = redactionConfig.IsDefault;
            WorkflowName = redactionConfig.WorkflowName;
            ActiveDirectoryGroups = CreateCsv(redactionConfig.ActiveDirectoryGroups);
            ProcessingAction = redactionConfig.ProcessingAction;
            PostProcessingAction = redactionConfig.PostProcessingAction;
            AttributeSet = redactionConfig.AttributeSet;
            RedactionTypes = CreateCsv(redactionConfig.RedactionTypes);
            EnableAllUserPendingQueue = redactionConfig.EnableAllUserPendingQueue;
            DocumentTypeFileLocation = redactionConfig.DocumentTypeFileLocation;
        }

        IRedactionWebConfiguration CreateModel()
        {
            return new RedactionWebConfiguration(
                configurationName: ConfigurationName,
                isDefault: IsDefault,
                workflowName: WorkflowName,
                activeDirectoryGroups: SplitCsv(ActiveDirectoryGroups, splitOnSpaceChar: false),
                processingAction: ProcessingAction,
                postProcessingAction: PostProcessingAction,
                attributeSet: AttributeSet,
                redactionTypes: SplitCsv(RedactionTypes, splitOnSpaceChar: true),
                enableAllUserPendingQueue: EnableAllUserPendingQueue,
                documentTypeFileLocation: DocumentTypeFileLocation)
            { ID = ID };
        }

        // Redaction types can safely be separated with a space but domain group names can contain spaces
        static readonly char[] _splitChars = new[] { '\u00A0', '\t', '\r', '\n', ',', ';' };
        static readonly char[] _splitCharsWithSpace = _splitChars.Concat(new[] { ' ' }).ToArray();

        static IList<string> SplitCsv(string csv, bool splitOnSpaceChar)
        {
            return csv?.Split(splitOnSpaceChar ? _splitCharsWithSpace : _splitChars)
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToList();
        }

        static string CreateCsv(IList<string> values)
        {
            if (values is null)
            {
                return null;
            }

            return string.Join("\n", values.Distinct());
        }

        void SetupObservableAsProperties()
        {
            // CurrentConfiguration
            this.WhenAnyPropertyChanged(
                nameof(ConfigurationName),
                nameof(IsDefault),
                nameof(WorkflowName),
                nameof(ActiveDirectoryGroups),
                nameof(ProcessingAction),
                nameof(PostProcessingAction),
                nameof(AttributeSet),
                nameof(RedactionTypes),
                nameof(EnableAllUserPendingQueue),
                nameof(DocumentTypeFileLocation))
                .Select(vm => vm.CreateModel())
                .BindTo(this, x => x.CurrentConfiguration);

            // VerificationSessionTimeout
            ConfigurationDatabase.VerificationSessionTimeoutMinutes
                .Select(minutes => minutes > 0
                    ? $"Sessions will automatically close after {minutes} minute(s) of inactivity"
                    : "No session timeout is configured")
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.VerificationSessionTimeoutMessage)
                .DisposeWith(Disposables);
        }

        void SetupValidationRules()
        {
            // ProcessingAction
            this.ValidationRule(x => x.ProcessingAction,
                CreateItemExistsValidationObservableFromStringList(x => x.ProcessingAction, x => x.MainSequenceActions),
                state => state.IsValid,
                state => state.ItemIsEmpty
                    ? "Verify action cannot be empty!"
                    : $"'{ProcessingAction}' is not a main-sequence action!");

            // PostProcessingAction
            this.ValidationRule(x => x.PostProcessingAction,
                CreateItemExistsValidationObservableFromStringList(x => x.PostProcessingAction, x => x.MainSequenceActions),
                state => state.IsValid,
                state => state.ItemIsEmpty
                    ? "Post-verify action cannot be empty!"
                    : $"'{PostProcessingAction}' is not a main-sequence action!");

            // DocumentTypeFileLocation
            var docTypeFileIsValid =
                this.WhenAnyValue(x => x.DocumentTypeFileLocation)
                .Select(docTypeFile =>
                {
                    return new ItemExistsValidationResult
                    {
                        ItemIsEmpty = string.IsNullOrWhiteSpace(docTypeFile),
                        ItemExists = File.Exists(docTypeFile)
                    };
                });
            this.ValidationRule(x => x.DocumentTypeFileLocation, docTypeFileIsValid,
                state => state.ItemIsEmpty || state.ItemExists,
                state => "Document type file does not exist!");
        }

        IObservable<ItemExistsValidationResult> CreateItemExistsValidationObservableFromStringList(
            Expression<Func<RedactionApiConfigViewModel, string>> stringProp,
            Expression<Func<RedactionApiConfigViewModel, IList<string>>> stringListProp)
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

        void SelectDocumentTypeFile()
        {
            if (_fileBrowserDialogService.SelectExistingFile(
                "Select document type file",
                "Document classifier file (*.idx)|*.idx|Text file (*.txt)|*.txt|All files|*.*")
                is string selectedPath)
            {
                DocumentTypeFileLocation = selectedPath;
            }
        }
    }
}