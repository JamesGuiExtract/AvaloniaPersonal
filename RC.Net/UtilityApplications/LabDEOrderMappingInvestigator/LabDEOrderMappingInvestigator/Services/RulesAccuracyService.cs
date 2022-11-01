using LabDEOrderMappingInvestigator.Models;
using System.Collections.Generic;
using System.Linq;

namespace LabDEOrderMappingInvestigator.Services
{
    /// <summary>
    /// Service to calculate missed/incorrect orders/tests
    /// </summary>
    public interface IRulesAccuracyService
    {
        /// <summary>
        /// Compare the expected and found orders and return the missed/incorrect orders and tests
        /// </summary>
        RulesAccuracyResult CalculateAccuracy(IList<LabOrderActual> expectedOrders, IList<LabOrderActual> foundOrders);
    }

    /// <inheritdoc/>
    public class RulesAccuracyService : IRulesAccuracyService
    {
        /// <inheritdoc/>
        public RulesAccuracyResult CalculateAccuracy(IList<LabOrderActual> expectedOrders, IList<LabOrderActual> foundOrders)
        {
            // Initialize missed = expected, incorrect = found
            // and then remove the values that shouldn't exist in those collections
            var missedOrders = expectedOrders.ToList();
            var incorrectOrders = foundOrders.ToList();
            var missedTests = expectedOrders.SelectMany(x => x.Tests).ToList();
            var incorrectTests = foundOrders.SelectMany(x => x.Tests).ToList();

            // Calculate missed/incorrect orders
            // TODO: Make this algorithm more complicated so that it chooses the optimal match when there is more than one choice
            for (int fndIdx = incorrectOrders.Count - 1; fndIdx >= 0; fndIdx--)
            {
                LabOrderActual fnd = incorrectOrders[fndIdx];
                int expIdx = missedOrders.FindIndex(exp => fnd.Code == exp.Code);
                if (expIdx >= 0)
                {
                    missedOrders.RemoveAt(expIdx);
                    incorrectOrders.RemoveAt(fndIdx);
                }
            }

            // Calculate missed/incorrect tests
            for (int fndIdx = incorrectTests.Count - 1; fndIdx >= 0; fndIdx--)
            {
                LabTestActual fnd = incorrectTests[fndIdx];
                int expIdx = missedTests.FindIndex(exp => fnd.Code == exp.Code);
                if (expIdx >= 0)
                {
                    missedTests.RemoveAt(expIdx);
                    incorrectTests.RemoveAt(fndIdx);
                }
            }

            return new(
                MissedOrders: missedOrders,
                IncorrectOrders: incorrectOrders,
                MissedTests: missedTests,
                IncorrectTests: incorrectTests);
        }
    }
}
