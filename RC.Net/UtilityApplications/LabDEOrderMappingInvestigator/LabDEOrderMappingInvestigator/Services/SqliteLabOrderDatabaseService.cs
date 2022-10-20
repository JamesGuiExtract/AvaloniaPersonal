using DynamicData.Kernel;
using LabDEOrderMappingInvestigator.Models;
using LabDEOrderMappingInvestigator.SqliteModels;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LabDEOrderMappingInvestigator.Services
{
    /// <summary>
    /// Service with methods to read order/test data from databases
    /// </summary>
    public interface ILabOrderDatabaseService
    {
        /// <summary>
        /// Update orders/tests to have definitions from a customer database
        /// </summary>
        IList<LabOrderActual> UpdateDefinitions(IList<LabOrderActual> labOrders, string customerOMDBPath);

        /// <summary>
        /// Update test to have the definition from a customer database
        /// </summary>
        LabTestActual UpdateLabTestDefinition(LabTestActual labTest, string customerOMDBPath);

        /// <summary>
        /// Load the definition of a lab test from a customer database
        /// </summary>
        Optional<LabTestDefinition> LoadLabTestFromDatabase(string databasePath, string testCode);

        /// <summary>
        /// Load the definition of a lab order from a customer database
        /// </summary>
        Optional<LabOrderDefinition> LoadLabOrderFromDatabase(string databasePath, string orderCode);

        /// <summary>
        /// Load Extract's lab test definitions from the URS database
        /// </summary>
        IList<LabTestExtract> LoadLabTestsFromExtractDatabase(string extractOMDBPath, string customerOMDBPath);
    }

    /// <inheritdoc/>
    public class SqliteLabOrderDatabaseService : ILabOrderDatabaseService
    {
        /// <summary>
        /// Add lab test definitions from sqlite database
        /// </summary>
        public IList<LabOrderActual> UpdateDefinitions(IList<LabOrderActual> labOrders, string customerOMDBPath)
        {
            return labOrders
                .Select(labOrderActual =>
                {
                    var orderCode = labOrderActual.Code;
                    Optional<LabOrderDefinition> orderDefinition = null;
                    if (orderCode.HasValue)
                    {
                        orderDefinition = LoadLabOrderFromDatabase(customerOMDBPath, orderCode.Value);
                    }

                    var labTests = labOrderActual.Tests
                        .Select(test =>
                        {
                            if (test.Code.HasValue)
                            {
                                return test with { LabTestDefinition = LoadLabTestFromDatabase(customerOMDBPath, test.Code.Value) };
                            }
                            return test;
                        })
                        .ToList();

                    return labOrderActual with { LabOrderDefinition = orderDefinition, Tests = labTests };
                })
                .ToList();
        }

        /// <summary>
        /// Update test to have the definition from a customer database
        /// </summary>
        public LabTestActual UpdateLabTestDefinition(LabTestActual labTest, string customerOMDBPath)
        {
            _ = labTest ?? throw new ArgumentNullException(nameof(labTest));

            if (labTest.Code.HasValue)
            {
                return labTest with { LabTestDefinition = LoadLabTestFromDatabase(customerOMDBPath, labTest.Code.Value) };
            }

            return labTest;
        }

        /// <summary>
        /// Load the definition of a lab test from a sqlite db using the test code
        /// </summary>
        public Optional<LabTestDefinition> LoadLabTestFromDatabase(string databasePath, string testCode)
        {
            using CustomerOrderMappingDB db = new(SqliteUtils.BuildConnectionOptions(databasePath));
            var labTest = db.LabTests
                .Where(x => x.TestCode == testCode)
                .LoadWith(x => x.AlternateTestNames)
                .LoadWith(x => x.ComponentToESComponentMaps)
                .LoadWith(x => x.LabOrderTests)
                .FirstOrDefault();

            if (labTest is null) return null;

            return GetLabTestDefinitionFromSqliteModel(databasePath, labTest);
        }

        /// <summary>
        /// Load the definition of a lab order from a sqlite db using the order code
        /// </summary>
        public Optional<LabOrderDefinition> LoadLabOrderFromDatabase(string databasePath, string orderCode)
        {
            using CustomerOrderMappingDB db = new(SqliteUtils.BuildConnectionOptions(databasePath));
            var labOrder = db.LabOrders
                .Where(x => x.Code == orderCode)
                .LoadWith(x => x.LabOrderTests)
                .FirstOrDefault();

            if (labOrder is null) return null;

            return GetLabOrderDefinitionFromSqliteModel(databasePath, labOrder);
        }

        /// <summary>
        /// Load lab test definitions from the URS (aka FKB, aka ComponentData) OrderMappingDB file
        /// </summary>
        public IList<LabTestExtract> LoadLabTestsFromExtractDatabase(string extractOMDBPath, string customerOMDBPath)
        {
            using URSOrderMappingDB db = new(SqliteUtils.BuildConnectionOptions(extractOMDBPath));
            var labTests = db.ESComponents
                .LoadWith(x => x.ESComponentAkas);

            return labTests
                .Select(x => new LabTestExtract(
                    x.Name,
                    x.Code,
                    x.ESComponentAkas.Select(aka => aka.Name).ToList(),
                    x.SampleType,
                    x.OrderOfMagnitude.HasValue ? Optional<int>.Create(x.OrderOfMagnitude.Value) : Optional<int>.None))
                .ToList();
        }

        // Load the definition of a lab test using a sqlite model
        static LabTestDefinition GetLabTestDefinitionFromSqliteModel(string databasePath, LabTest labTest)
        {
            const char ACCEPTED = 'A';

            var akas = labTest.AlternateTestNames
                .Where(x => x.StatusCode == ACCEPTED)
                .Select(x => x.Name)
                .ToList();

            var orders = labTest.LabOrderTests
                .Select(x => x.OrderCode)
                .ToList();

            var esComponentCodes = labTest.ComponentToESComponentMaps.Select(x => x.ESComponentCode).ToList();

            return new LabTestDefinition(labTest.OfficialName, labTest.TestCode, esComponentCodes, akas, orders, databasePath);
        }

        // Load the definition of a lab order using a sqlite model
        LabOrderDefinition GetLabOrderDefinitionFromSqliteModel(string databasePath, LabOrder labOrder)
        {
            List<LabTestDefinition> mandatoryTests = new();
            List<LabTestDefinition> optionalTests = new();
            foreach (var labOrderTest in labOrder.LabOrderTests)
            {
                var labTestDefinition = LoadLabTestFromDatabase(databasePath, labOrderTest.TestCode);
                if (labTestDefinition.HasValue)
                {
                    if (labOrderTest.Mandatory)
                    {
                        mandatoryTests.Add(labTestDefinition.Value);
                    }
                    else
                    {
                        optionalTests.Add(labTestDefinition.Value);
                    }
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
