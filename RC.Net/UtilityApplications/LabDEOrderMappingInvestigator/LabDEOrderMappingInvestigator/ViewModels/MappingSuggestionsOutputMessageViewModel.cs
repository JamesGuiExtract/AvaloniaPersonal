using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace LabDEOrderMappingInvestigator.ViewModels
{
    /// <summary>
    /// View model that represents a collection of lab tests with suggested URS mappings
    /// </summary>
    public class MappingSuggestionsOutputMessageViewModel : OutputMessageViewModelBase
    {
        /// <summary>
        /// Plain text/summary information from the analysis
        /// </summary>
        public string TextMessage { get; }

        /// <summary>
        /// The collection of customer tests that have suggested mappings
        /// </summary>
        public ObservableCollection<LabTestMatchListViewModel> LabTestMatchLists { get; }

        [Reactive]
        public LabTestMatchListViewModel? SelectedList { get; set; }

        [ObservableAsProperty]
        public string? CustomerTestDescription { get; }

        public MappingSuggestionsOutputMessageViewModel(string textMessage, IEnumerable<LabTestMatchListViewModel> labTestMatches)
        {
            _ = labTestMatches ?? throw new ArgumentNullException(nameof(labTestMatches));

            TextMessage = textMessage;
            LabTestMatchLists = new ObservableCollection<LabTestMatchListViewModel>(labTestMatches);
            SelectedList = LabTestMatchLists.FirstOrDefault();

            this.WhenAnyValue(x => x.SelectedList)
                .WhereNotNull()
                .Select(x => $"Suggested mappings for {x.CustomerTestName} [{x.CustomerTestCode}]")
                .ToPropertyEx(this, x => x.CustomerTestDescription);
        }
    }
}
