using DynamicData.Binding;
using DynamicData.Kernel;
using LabDEOrderMappingInvestigator.Models;
using LabDEOrderMappingInvestigator.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;

namespace LabDEOrderMappingInvestigator.ViewModels
{
    /// <summary>
    /// Factory to create LabTestMatchViewModel instances with injected dependencies
    /// </summary>
    public interface ILabTestMatchViewModelFactory
    {
        LabTestMatchViewModel Create(LabTestMatch labTestMatch);
    }

    /// <inheritdoc/>
    public class LabTestMatchViewModelFactory : ILabTestMatchViewModelFactory
    {
        readonly ICustomerDatabaseService _customerDatabaseService;
        readonly ILabOrderDatabaseService _labOrderDatabaseService;

        public LabTestMatchViewModelFactory(ICustomerDatabaseService customerDatabaseService, ILabOrderDatabaseService labOrderDatabaseService)
        {
            _customerDatabaseService = customerDatabaseService;
            _labOrderDatabaseService = labOrderDatabaseService;
        }

        public LabTestMatchViewModel Create(LabTestMatch labTestMatch)
        {
            return new LabTestMatchViewModel(labTestMatch, _customerDatabaseService, _labOrderDatabaseService);
        }
    }

    /// <summary>
    /// View model for <see cref="LabTestMatch"/> objects
    /// </summary>
    public sealed class LabTestMatchViewModel : ViewModelBase
    {

        readonly ICustomerDatabaseService _customerOMDBService;
        readonly ILabOrderDatabaseService _labOrderDatabaseService;

        [ObservableAsProperty]
        LabTestDefinition? TestDefinition { get; }

        /// <summary>
        /// The customer lab test that this match is for
        /// </summary>
        [Reactive]
        public LabTestActual CustomerTest { get; set; }

        /// <summary>
        /// Whether the customer and URS tests represented by this match object are mapped in the customer's database
        /// </summary>
        [Reactive]
        public bool IsMapped { get; set; }

        public string CustomerTestCode { get; }
        public string CustomerTestName { get; }
        public string ExtractTestCode { get; }
        public string ExtractTestName { get; }
        public double Score { get; }
        public string AKAs { get; }

        public LabTestMatchViewModel(
            LabTestMatch labTestMatch,
            ICustomerDatabaseService customerOMDBService,
            ILabOrderDatabaseService labOrderDatabaseService)
        {
            _ = labTestMatch ?? throw new ArgumentNullException(nameof(labTestMatch));
            _customerOMDBService = customerOMDBService ?? throw new ArgumentNullException(nameof(customerOMDBService));
            _labOrderDatabaseService = labOrderDatabaseService ?? throw new ArgumentNullException(nameof(labOrderDatabaseService));

            CustomerTest = labTestMatch.CustomerTest;

            CustomerTestName = CustomerTest.Name;
            CustomerTestCode = CustomerTest.Code.ValueOr(() => "");
            ExtractTestCode = labTestMatch.ExtractTest.Code;
            ExtractTestName = labTestMatch.ExtractTest.Name;
            Score = labTestMatch.Score;

            AKAs = string.Join("|", labTestMatch.ExtractTest.AKAs);

            this.WhenAnyValue(x => x.CustomerTest)
                .Select(labTest => labTest.LabTestDefinition.ValueOrDefault())
                .WhereNotNull()
                .ToPropertyEx(this, x => x.TestDefinition);

            this.WhenAnyValue(x => x.TestDefinition)
                .WhereNotNull()
                .Select(testDefinition => testDefinition.ESComponentCodes.Contains(ExtractTestCode))
                .BindTo(this, x => x.IsMapped);

            this.WhenValueChanged(x => x.IsMapped, notifyOnInitialValue: false)
                .Subscribe(isChecked => UpdateMapping(isChecked));
        }

        // Add or remove a ComponentToESComponentMap entry for this match (toggle the mapping)
        void UpdateMapping(bool addEntry)
        {
            if (TestDefinition is null)
            {
                return;
            }

            try
            {
                if (addEntry)
                {
                    _customerOMDBService.AddESComponentMapEntry(TestDefinition.DatabasePath, CustomerTestCode, ExtractTestCode);
                }
                else
                {
                    _customerOMDBService.RemoveESComponentMapEntry(TestDefinition.DatabasePath, CustomerTestCode, ExtractTestCode);
                }

                // Refresh the test info from the database
                CustomerTest = _labOrderDatabaseService.UpdateLabTestDefinition(CustomerTest, TestDefinition.DatabasePath);
            }
            catch (Exception ex)
            {
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }
        }
    }
}
