using DynamicData.Kernel;
using LabDEOrderMappingInvestigator.Models;
using LabDEOrderMappingInvestigator.SqliteModels;
using LinqToDB;
using System.Collections.Generic;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

namespace LabDEOrderMappingInvestigator.Services
{
    /// <summary>
    /// Service with methods to read order/test data from VOA files and Sqlite databases
    /// </summary>
    public interface ILabOrderService
    {
        IList<LabOrderActual> LoadFromFile(string attributesFileName, string customerOMDBPath);
    }

    /// <inheritdoc/>
    public class LabOrderService : ILabOrderService
    {
        private readonly IAFUtility _afutil;

        public LabOrderService(IAFUtility afutil)
        {
            _afutil = afutil;
        }

        /// <summary>
        /// Load lab orders from a VOA file/Sqlite database
        /// </summary>
        public IList<LabOrderActual> LoadFromFile(string attributesFileName, string customerOMDBPath)
        {
            var voa = _afutil.GetAttributesFromFile(attributesFileName);

            (voa != null).Assert("Unexpected null vector of attributes!");

            return voa.ToIEnumerable<IAttribute>()
                .Select(a =>
                {
                    if (a.Name == "Test")
                    {
                        Optional<string> orderCode = GetFirstValueFromVoa(a.SubAttributes, "OrderCode");
                        Optional<string> orderName = GetFirstValueFromVoa(a.SubAttributes, "Name");
                        Optional<LabOrderDefinition> orderDefinition = null;
                        if (orderCode.HasValue)
                        {
                            orderDefinition = LoadLabOrderFromDatabase(customerOMDBPath, orderCode.Value);
                        }

                        var labTests = _afutil.QueryAttributes(a.SubAttributes, "Component", false)
                            .ToIEnumerable<IAttribute>()
                            .Select(test =>
                            {
                                Optional<LabTestDefinition> testDefinition = null;
                                var testCode = GetFirstValueFromVoa(test.SubAttributes, "TestCode");
                                if (testCode.HasValue)
                                {
                                    testDefinition = LoadLabTestFromDatabase(customerOMDBPath, testCode.Value);
                                }
                                return new LabTestActual(testDefinition, test.Value.String,
                                    GetFirstValueFromVoa(test.SubAttributes, "Value").ValueOrDefault() ?? "",
                                    GetFirstValueFromVoa(test.SubAttributes, "Range").ValueOrDefault() ?? "",
                                    GetFirstValueFromVoa(test.SubAttributes, "Units").ValueOrDefault() ?? "",
                                    GetFirstValueFromVoa(test.SubAttributes, "Flag").ValueOrDefault() ?? "");
                            })
                            .ToList();

                        return Optional<LabOrderActual>.Create(new LabOrderActual(orderName, orderCode, orderDefinition, labTests));
                    }
                    return null;
                })
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();
        }

        // Query for an attribute name and return the string value, or none if not found
        Optional<string> GetFirstValueFromVoa(IUnknownVector voa, string name)
        {
            Optional<string> value = null;
            var values = _afutil.QueryAttributes(voa, name, false);
            if (values.Size() > 0)
            {
                value = ((IAttribute)values.At(0)).Value.String;
            }

            return value;
        }

        // Load the definition of a lab test using a sqlite model
        static LabTestDefinition GetLabTestDefinitionFromSqliteModel(LabTest labTest)
        {
            var akas = labTest.AlternateTestNames
                .Where(x => x.StatusCode == 'A')
                .Select(x => x.Name)
                .ToList();

            var esComponentCodes = labTest.ComponentToESComponentMaps.Select(x => x.ESComponentCode).ToList();

            return new LabTestDefinition(labTest.OfficialName, labTest.TestCode, esComponentCodes, akas);
        }

        // Load the definition of a lab test from a sqlite db using the test code
        static Optional<LabTestDefinition> LoadLabTestFromDatabase(string fileName, string testCode)
        {
            using CustomerOrderMappingDB db = new(SqliteUtils.BuildConnectionOptions(fileName));
            var labTest = db.LabTests
                .Where(x => x.TestCode == testCode)
                .LoadWith(x => x.AlternateTestNames)
                .LoadWith(x => x.ComponentToESComponentMaps)
                .FirstOrDefault();

            if (labTest is null) return null;

            return GetLabTestDefinitionFromSqliteModel(labTest);
        }

        // Load the definition of a lab order from a sqlite db using the order code
        static Optional<LabOrderDefinition> LoadLabOrderFromDatabase(string fileName, string orderCode)
        {
            using CustomerOrderMappingDB db = new(SqliteUtils.BuildConnectionOptions(fileName));
            var labOrder = db.LabOrders
                .Where(x => x.Code == orderCode)
                .LoadWith(x => x.LabOrderTests.First().LabTest.AlternateTestNames)
                .LoadWith(x => x.LabOrderTests.First().LabTest.ComponentToESComponentMaps)
                .FirstOrDefault();

            if (labOrder is null) return null;

            List<LabTestDefinition> mandatoryTests = new();
            List<LabTestDefinition> optionalTests = new();
            foreach (var labOrderTest in labOrder.LabOrderTests)
            {
                var labTestDefinition = GetLabTestDefinitionFromSqliteModel(labOrderTest.LabTest);
                if (labOrderTest.Mandatory)
                {
                    mandatoryTests.Add(labTestDefinition);
                }
                else
                {
                    optionalTests.Add(labTestDefinition);
                }
            }

            return new LabOrderDefinition(
                labOrder.Name,
                labOrder.Code,
                labOrder.FilledRequirement ?? 0,
                labOrder.TieBreaker,
                mandatoryTests,
                optionalTests);
        }
    }
}
