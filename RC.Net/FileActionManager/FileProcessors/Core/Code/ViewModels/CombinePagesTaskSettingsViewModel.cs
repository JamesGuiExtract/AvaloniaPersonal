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
                    OutputPath = OutputPath
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
                string sourceDocNameConfiguration =
@"Output path is set to <SourceDocName>.
This will replace the current document and update its uss and voa file data to reflect new page numbers in the updated document.

Confirm overwriting source document?";
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
            string help =
@"- An output document will be produced by combining the source documents in the order they appear in the list with the selected pages from each.
- Separate single pages or ranges with a comma
- Use ""-"" to indicate a page number relative to the last page
- Specify a range with "".."" 
  Examples:
        1 for the first page
        1.. for all pages
        2.. for all but the first page
        ..4 for the first 4 pages
        1,3..5 for pages 1, 3, 4, 5
        1,-1 for the first and last page
        2..-2 for all pages except the first and last

- If the OutputPath is set to <SourceDocName>, the original document will be overwritten and uss and voa file data from <SourceDocName> will be updated to reflect page numbers in the updated.
- Path tags and functions will both be evaluated though only path tags will be offered as options to choose
- Example configuration to attach a cover page to <SourceDocName> (presumes custom tag <CoverPage> is defined):
        Source Documents:
        [ <CoverPage>             | 1    ]
        [ <SourceDocName>   | 1..  ]
        OutputPath:
        [ <SourceDocName> ]";
            _messageDialogService.ShowOkDialog("Combine Pages Source Help", help);
        }
    }
}
