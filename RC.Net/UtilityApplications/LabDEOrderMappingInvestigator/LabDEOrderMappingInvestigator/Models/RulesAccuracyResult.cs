using System.Collections.Generic;
using System.Linq;

namespace LabDEOrderMappingInvestigator.Models
{
    /// <summary>
    /// The result of comparing found orders/tests the expected orders/tests
    /// </summary>
    /// <param name="MissedOrders">Orders that were expected but not found</param>
    /// <param name="IncorrectOrders">Orders that were found but not expected (e.g., extra orders)</param>
    /// <param name="MissedTests">Tests that were expected but not found</param>
    /// <param name="IncorrectTests">Tests that were found but not expected (e.g., extra tests)</param>
    public record RulesAccuracyResult(
        IList<LabOrderActual> MissedOrders,
        IList<LabOrderActual> IncorrectOrders,
        IList<LabTestActual> MissedTests,
        IList<LabTestActual> IncorrectTests)
    {
        /// <summary>
        /// Whether there is anything missed or incorrect
        /// </summary>
        public bool IsAnythingMissingOrIncorrect =>
            MissedOrders.Any() || IncorrectOrders.Any()
            || MissedTests.Any() || IncorrectTests.Any();
    }
}
