using DynamicData;
using Extract.FileActionManager.FileProcessors.Models;
using Extract.FileActionManager.Forms;
using Extract.Utilities;
using Extract.Utilities.ReactiveUI;
using Extract.Utilities.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using UCLID_FILEPROCESSINGLib;

using SettingsModel = Extract.FileActionManager.FileProcessors.Models.CombinePagesTaskSettingsModelV1;

namespace Extract.FileActionManager.FileProcessors.ViewModels
{
    [CLSCompliant(false)]
    public class CombinePagesTaskSettingsViewModel : ViewModelBase
    {
        readonly IFileProcessingDB _fileProcessingDB;
        readonly IPathTags _famTagManager;
        readonly IFileBrowserDialogService _fileBrowserDialogService;
        readonly IMessageDialogService _messageDialogService;

        public ObservableCollection<PageSourceV1> PageSources { get; } = new();

        public IEnumerable<string> ActionNames { get; }

        public IEnumerable<string> TagNames { get; }

        [Reactive]
        public int SelectedPageSourceIndex { get; set; }

        [Reactive]
        public string OutputPath { get; set; }

        [Reactive]
        public bool UpdateData { get; set; }

        public ReactiveCommand<Unit, Unit> OkCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public ReactiveCommand<Unit, Unit> AddPageSourceCommand { get; }

        public ReactiveCommand<Unit, Unit> DeletePageSourceCommand { get; }

        public ReactiveCommand<Unit, Unit> SelectOutputPathCommand { get; }

        public ReactiveCommand<Unit, Unit> GetConfigurationHelpCommand { get; }

        public CombinePagesTaskSettingsViewModel(SettingsModel settingsModel,
            IFileProcessingDB fileProcessingDB,
            IFileBrowserDialogService fileBrowserDialogService,
            IMessageDialogService messageDialogService)
            : base()
        {
            settingsModel = settingsModel ?? throw new ArgumentNullException(nameof(settingsModel));
            _fileProcessingDB = fileProcessingDB;
            _fileBrowserDialogService = fileBrowserDialogService ?? throw new ArgumentNullException(nameof(fileBrowserDialogService));
            _messageDialogService = messageDialogService;

            ActionNames = _fileProcessingDB.GetAllActions()
                .ComToDictionary()
                .Keys
                .OrderBy(name => name)
                .ToList();

            _famTagManager = new FileActionManagerPathTags();
            TagNames = _famTagManager.CustomTags
                .OrderBy(tag => tag)
                .Union(_famTagManager.BuiltInTags
                    .OrderBy(tag => tag))
                .Select(tag =>
                    tag.StartsWith("<", StringComparison.OrdinalIgnoreCase)
                    ? tag
                    : "");

            PageSources.AddRange(
                settingsModel.PageSources
                    .Select(p => new PageSourceV1(p.Document, p.Pages)));

            OutputPath = settingsModel.OutputPath;
            UpdateData = settingsModel.UpdateData;

            OkCommand = ReactiveCommand.Create(() => MessageBus.Current.SendMessage(OkWindowMessage.Instance));
            CancelCommand = ReactiveCommand.Create(() => MessageBus.Current.SendMessage(CloseWindowMessage.Instance));
            AddPageSourceCommand = ReactiveCommand.Create(() => PageSources.Add(new("","")));
            DeletePageSourceCommand = ReactiveCommand.Create(() => PageSources.RemoveAt(SelectedPageSourceIndex),
                this.WhenAnyValue(x => x.SelectedPageSourceIndex)
                    .Select(rowIndex => rowIndex >= 0));
            SelectOutputPathCommand = ReactiveCommand.Create(SelectDocumentFolder);
            GetConfigurationHelpCommand = ReactiveCommand.Create(GetConfigurationHelp);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public SettingsModel GetSettings()
        {
            try
            {
                return new SettingsModel()
                {
                    PageSources = PageSources
                        .Select(p => new PageSourceV1(p.Document, p.Pages))
                        .ToList()
                        .AsReadOnly(),
                    OutputPath = OutputPath,
                    UpdateData = UpdateData
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53907");
            }
        }

        public IEnumerable<string> ValidationErrors
        {
            get
            {
                if (PageSources.Count == 0)
                {
                    yield return "At least one page source must be defined.";
                }

                if (PageSources.Any(pageSource =>
                    string.IsNullOrWhiteSpace(pageSource.Document)
                    || string.IsNullOrWhiteSpace(pageSource.Pages)))
                {
                    yield return "The source document and page(s) for each source must be specified.";
                }

                if (string.IsNullOrWhiteSpace(OutputPath))
                {
                    yield return "The output path must be specified";
                }
            }
        }

        public bool VerifyOutputPath()
        {
            if (OutputPath == "<SourceDocName>")
            {
                string sourceDocNameConfiguration = "Output path is set to <SourceDocName>. \r\nConfirm overwriting source document?";
                return _messageDialogService.ShowYesNoDialog("<SourceDocName>", sourceDocNameConfiguration) == MessageDialogResult.Yes;
            }
            return true;
        }

        void SelectDocumentFolder()
        {
            if (_fileBrowserDialogService.SelectFolder("Select document folder")
                is string selectedPath)
            {
                OutputPath = selectedPath;
            }
        }

        void GetConfigurationHelp()
        {
            string help = @"The sources are combined in the order they appear in the list with the selected pages.
    - Pages are selected using a comma separated list of either single pages or ranges.
    - Ranges are specified with .. such as 
           1.. would be all pages, 
           2.. would be all but the first page, 
           1..4 would be the first 4 pages
    - If the OutputPath is set to <SourceDocName>, the original document will be overwritten.";

            _messageDialogService.ShowOkDialog("Combine Pages Source Help", help);
        }
    }
}
