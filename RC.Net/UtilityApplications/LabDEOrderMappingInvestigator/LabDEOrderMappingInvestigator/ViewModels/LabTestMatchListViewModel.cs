using DynamicData;
using DynamicData.Binding;
using DynamicData.Kernel;
using LabDEOrderMappingInvestigator.Models;
using LabDEOrderMappingInvestigator.Services;
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
    /// Factory to create LabTestMatchListViewModel instances with injected dependencies
    /// </summary>
    public interface ILabTestMatchListViewModelFactory
    {
        /// <summary>
        /// Create a view model with the supplied parameters and injected dependencies
        /// </summary>
        /// <param name="customerTest">The customer test that the matches apply to</param>
        /// <param name="labTestMatches">List of mappings (existant or suggested) from the customer to an extract test</param>
        LabTestMatchListViewModel Create(LabTestActual customerTest, IList<LabTestMatch> labTestMatches);
    }

    /// <inheritdoc/>
    public class LabTestMatchListViewModelFactory : ILabTestMatchListViewModelFactory
    {
        readonly ILabTestMatchViewModelFactory _labTestMatchViewModelFactory;
        readonly ILabOrderDatabaseService _labOrderDatabaseService;

        public LabTestMatchListViewModelFactory(
            ILabTestMatchViewModelFactory labTestMatchViewModelFactory,
            ILabOrderDatabaseService labOrderDatabaseService)
        {
            _labTestMatchViewModelFactory = labTestMatchViewModelFactory;
            _labOrderDatabaseService = labOrderDatabaseService;
        }

        /// <inheritdoc/>
        public LabTestMatchListViewModel Create(LabTestActual customerTest, IList<LabTestMatch> labTestMatches)
        {
            return new LabTestMatchListViewModel(customerTest, labTestMatches, _labTestMatchViewModelFactory, _labOrderDatabaseService);
        }
    }

    /// <summary>
    /// View model for displaying a collection of <see cref="LabTestMatch"/> objects
    /// </summary>
    public sealed class LabTestMatchListViewModel : ViewModelBase
    {
        readonly ILabTestMatchViewModelFactory _labTestMatchViewModelFactory;
        readonly ILabOrderDatabaseService _labTestService;

        // The customer test object that the list of matches represents
        [Reactive]
        LabTestActual CustomerTest { get; set; }

        /// <summary>
        /// Each suggested/actual mapping
        /// </summary>
        public ObservableCollection<LabTestMatchViewModel> Matches { get; }

        /// <summary>
        /// The customer test code that the list of matches is for
        /// </summary>
        public string CustomerTestCode { get; }

        /// <summary>
        /// The customer test name that the list of matches is for
        /// </summary>
        public string CustomerTestName { get; }

        /// <summary>
        /// The list of AKAs associated with this customer test
        /// </summary>
        public string AKAs { get; }

        /// <summary>
        /// The list of Orders associated with this customer test
        /// </summary>
        public string BelongsToOrders { get; }

        /// <summary>
        /// Whether this object was modified (a mapping added/removed) during it's lifetime
        /// </summary>
        [Reactive]
        public bool RecentlyChanged { get; set; }

        /// <summary>
        /// The number of URS tests mapped to the customer test
        /// </summary>
        [ObservableAsProperty]
        public int MappedTestCount { get; }

        /// <summary>
        /// Create an instance
        /// </summary>
        public LabTestMatchListViewModel(
            LabTestActual customerTest,
            IList<LabTestMatch> labTestMatches,
            ILabTestMatchViewModelFactory labTestMatchViewModelFactory,
            ILabOrderDatabaseService labTestService)
        {
            CustomerTest = customerTest ?? throw new ArgumentNullException(nameof(customerTest));
            _ = labTestMatches ?? throw new ArgumentNullException(nameof(labTestMatches));
            _labTestMatchViewModelFactory = labTestMatchViewModelFactory ?? throw new ArgumentNullException(nameof(labTestMatchViewModelFactory));
            _labTestService = labTestService ?? throw new ArgumentNullException(nameof(labTestService));

            Matches = new ObservableCollection<LabTestMatchViewModel>(labTestMatches.Select(_labTestMatchViewModelFactory.Create));

            // Watch each match object for changes
            // Update the customer test if any of the matches change it
            var matchSubscription = Matches
                .Select(labTestMatch => labTestMatch.WhenValueChanged(x => x.CustomerTest, false))
                .ToObservable()
                .Merge()
                .Select(x =>
                {
                    RecentlyChanged = true;
                    return x;
                })
                .BindTo(this, x => x.CustomerTest);

            CustomerTestName = customerTest.Name;
            CustomerTestCode = customerTest.Code.ValueOr(() => "");

            AKAs = string.Join("|", customerTest.LabTestDefinition
                .ConvertOr(x => x!.AKAs, () => Array.Empty<string>())!);

            string? dbPath = CustomerTest.LabTestDefinition.ValueOrDefault()?.DatabasePath;
            (dbPath is not null).Assert("Logic error: Customer database is not set for a lab test definition");

            BelongsToOrders = string.Join("|", customerTest.LabTestDefinition
                .ConvertOr(x => x!.BelongsToOrderCodes
                    .Select(x => _labTestService.LoadLabOrderFromDatabase(dbPath, x).Value.Name), () => Array.Empty<string>())!);

            // MappedTestCount
            this.WhenAnyValue(x => x.CustomerTest)
                .Select(labTest => labTest.LabTestDefinition.HasValue ? labTest.LabTestDefinition.Value.ESComponentCodes.Count : 0)
                .ToPropertyEx(this, x => x.MappedTestCount);
        }
    }
}
