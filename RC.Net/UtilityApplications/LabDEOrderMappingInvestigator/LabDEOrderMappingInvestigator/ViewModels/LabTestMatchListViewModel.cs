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
        LabTestMatchListViewModel Create(LabTestActual customerTest, IList<LabTestMatch> labTestMatches);
    }

    /// <inheritdoc/>
    public class LabTestMatchListViewModelFactory : ILabTestMatchListViewModelFactory
    {
        readonly ILabTestMatchViewModelFactory _labTestMatchViewModelFactory;
        readonly ILabOrderDatabaseService _labOrderDatabaseService;

        public LabTestMatchListViewModelFactory(ILabTestMatchViewModelFactory labTestMatchViewModelFactory, ILabOrderDatabaseService labOrderDatabaseService)
        {
            _labTestMatchViewModelFactory = labTestMatchViewModelFactory;
            _labOrderDatabaseService = labOrderDatabaseService;
        }

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

        [Reactive]
        LabTestActual CustomerTest { get; set; }

        public ObservableCollection<LabTestMatchViewModel> Matches { get; }

        public string CustomerTestCode { get; }
        public string CustomerTestName { get; }
        public string AKAs { get; }
        public string BelongsToOrders { get; }

        [ObservableAsProperty]
        public int MappedTestCount { get; }

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

            // Update the customer test if any of the matches change it
            Matches
                .Select(labTestMatch => labTestMatch.WhenValueChanged(x => x.CustomerTest))
                .ToObservable()
                .Merge()
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

            this.WhenAnyValue(x => x.CustomerTest)
                .Select(labTest => labTest.LabTestDefinition.HasValue ? labTest.LabTestDefinition.Value.ESComponentCodes.Count : 0)
                .ToPropertyEx(this, x => x.MappedTestCount);
        }
    }
}
