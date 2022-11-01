using DynamicData;
using LabDEOrderMappingInvestigator.Models;
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
        // Source for the list of lab tests
        readonly ObservableCollection<LabTestMatchListViewModel> _labTestMatchListsSource;

        // Filtered list of lab tests
        readonly ReadOnlyObservableCollection<LabTestMatchListViewModel> _labTestMatchLists;

        // Missed test codes
        readonly HashSet<string> _missedTestCodes = new(StringComparer.OrdinalIgnoreCase);

        // Incorrectly found test codes
        readonly HashSet<string> _incorrectTestCodes = new(StringComparer.OrdinalIgnoreCase);

        // Enum value representing the current filter (computed from bool properties)
        [ObservableAsProperty]
        LabTestFilter Filter { get; }

        /// <summary>
        /// Plain text/summary information from the analysis
        /// </summary>
        public string TextMessage { get; }

        /// <summary>
        /// The collection of customer tests that have suggested mappings
        /// </summary>
        public ReadOnlyObservableCollection<LabTestMatchListViewModel> LabTestMatchLists => _labTestMatchLists;

        /// <summary>
        /// Whether to show all the expected tests in the list
        /// </summary>
        [Reactive]
        public bool ShowAllExpectedTests { get; set; }

        /// <summary>
        /// Whether to show expected tests that are not mapped to any URS test
        /// </summary>
        [Reactive]
        public bool ShowUnMappedTests { get; set; } = true;

        /// <summary>
        /// Whether to show tests that were missed
        /// </summary>
        [Reactive]
        public bool ShowMissedTests { get; set; }

        /// <summary>
        /// Whether to show tests that were incorrectly found
        /// </summary>
        [Reactive]
        public bool ShowIncorrectTests { get; set; }

        /// <summary>
        /// Whether to include expected tests that were modified since this instance was created
        /// </summary>
        [Reactive]
        public bool ShowRecentlyChanged { get; set; } = true;

        /// <summary>
        /// The currently-selected <see cref="LabTestMatchListViewModel"/> in the main list
        /// </summary>
        [Reactive]
        public LabTestMatchListViewModel? SelectedList { get; set; }

        /// <summary>
        /// Computed text that describes the currently-selected <see cref="LabTestMatchListViewModel"/>
        /// </summary>
        [ObservableAsProperty]
        public string? CustomerTestDescription { get; }

        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <param name="textMessage">A string description of the analysis result</param>
        /// <param name="rulesAccuracyResult">Optional information about missed/incorrectly found orders/tests</param>
        /// <param name="labTestMatches">A collection of <see cref="LabTestMatchListViewModel"/>s to display</param>
        /// <param name="labTestFilter">Optional initial filter for the list of <see cref="LabTestMatchListViewModel"/>s</param>
        public MappingSuggestionsOutputMessageViewModel(
            string textMessage,
            RulesAccuracyResult? rulesAccuracyResult,
            IEnumerable<LabTestMatchListViewModel> labTestMatches,
            LabTestFilter? labTestFilter)
        {
            _ = labTestMatches ?? throw new ArgumentNullException(nameof(labTestMatches));

            TextMessage = textMessage;

            if (rulesAccuracyResult is not null)
            {
                _missedTestCodes.UnionWith(rulesAccuracyResult.MissedTests
                    .Where(x => x.Code.HasValue)
                    .Select(x => x.Code.Value));

                _incorrectTestCodes.UnionWith(rulesAccuracyResult.IncorrectTests
                    .Where(x => x.Code.HasValue)
                    .Select(x => x.Code.Value));
            }

            // Apply the saved filter if provided
            if (labTestFilter.HasValue)
            {
                ApplyFilterEnum(labTestFilter.Value);
            }

            // Setup a filter for the main list of lab tests
            SetupLabTestMatchListFilter();

            // Setup the filtered list
            _labTestMatchListsSource = new(labTestMatches);
            _labTestMatchListsSource
                .AsObservableChangeSet(x => x.CustomerTestCode)
                .AsObservableCache()
                .Connect()
                .AutoRefreshOnObservable(labTestMatchList => labTestMatchList.WhenAnyValue(x => x.MappedTestCount))
                .Filter(this.WhenAnyValue(x => x.Filter).Select(BuildLabTestMatchListsViewModelFilter))
                .Bind(out _labTestMatchLists)
                .Subscribe();

            // Set initial value for the selected item to be the first item in the filtered list
            SelectedList = LabTestMatchLists.FirstOrDefault();

            // CustomerTestDescription
            this.WhenAnyValue(x => x.SelectedList)
                .Select(x => x is null ? "" : $"Suggested mappings for {x.CustomerTestName} [{x.CustomerTestCode}]")
                .ToPropertyEx(this, x => x.CustomerTestDescription);

            // Send a message when the Filter changes so that it can be saved by the main window view model (app state suspension)
            MessageBus.Current.RegisterMessageSource(this.WhenAnyValue(x => x.Filter));
        }

        // Convert boolean flags into a LabTestFilter enum property
        void SetupLabTestMatchListFilter()
        {
            this.WhenAnyValue(
                x => x.ShowAllExpectedTests,
                x => x.ShowUnMappedTests,
                x => x.ShowRecentlyChanged,
                x => x.ShowMissedTests,
                x => x.ShowIncorrectTests,
                (all, unMapped, recent, missed, incorrect) =>
                      (all ? LabTestFilter.All : LabTestFilter.None)
                    | (unMapped ? LabTestFilter.UnMapped : LabTestFilter.None)
                    | (recent ? LabTestFilter.RecentlyChanged : LabTestFilter.None)
                    | (missed ? LabTestFilter.MissedTests : LabTestFilter.None)
                    | (incorrect ? LabTestFilter.IncorrectTests : LabTestFilter.None))
                .Throttle(TimeSpan.FromMilliseconds(100)) // Avoid filtering for intermediate states, as one box is checked another will be unchecked...
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.Filter);
        }

        // Set boolean flags from a LabTestFilter enum
        void ApplyFilterEnum(LabTestFilter filter)
        {
            ShowAllExpectedTests = filter == LabTestFilter.None || filter.HasFlag(LabTestFilter.All);
            ShowRecentlyChanged = filter.HasFlag(LabTestFilter.RecentlyChanged);
            ShowMissedTests = filter.HasFlag(LabTestFilter.MissedTests);
            ShowIncorrectTests = filter.HasFlag(LabTestFilter.IncorrectTests);
            ShowUnMappedTests = filter.HasFlag(LabTestFilter.UnMapped);
        }

        // Build a filter predicate based on the specified enum value
        Func<LabTestMatchListViewModel, bool> BuildLabTestMatchListsViewModelFilter(LabTestFilter filter)
        {
            return labTestMatchList =>
                filter == LabTestFilter.None || filter.HasFlag(LabTestFilter.All)
                || filter.HasFlag(LabTestFilter.RecentlyChanged) && labTestMatchList.RecentlyChanged
                || filter.HasFlag(LabTestFilter.MissedTests) && _missedTestCodes.Contains(labTestMatchList.CustomerTestCode)
                || filter.HasFlag(LabTestFilter.IncorrectTests) && _incorrectTestCodes.Contains(labTestMatchList.CustomerTestCode)
                || filter.HasFlag(LabTestFilter.UnMapped) && labTestMatchList.MappedTestCount == 0;
        }
    }
}
