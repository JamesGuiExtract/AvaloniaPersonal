using DynamicData.Kernel;
using System.Collections.Generic;

namespace LabDEOrderMappingInvestigator.Models
{
    /// <summary>
    /// An abstract lab order defined by a customer
    /// </summary>
    /// <param name="Name">The name of the order (long key)</param>
    /// <param name="Code">The code of the order (alpha-numeric short key)</param>
    /// <param name="FilledRequirement">The number of tests required* for the automated mapping process to be able to use this order</param>
    /// <param name="Tiebreaker">A sorting key (alphabetic order) that is used to decide between to otherwise equally good matches</param>
    /// <param name="MandatoryTests">The tests that are required* to be part of an order in the automated mapping process</param>
    /// <param name="OptionalTests"></param>
    public record LabOrderDefinition(
        string Name,
        string Code,
        int FilledRequirement,
        string Tiebreaker,
        IList<LabTestDefinition> MandatoryTests,
        IList<LabTestDefinition> OptionalTests);

    /// <summary>
    /// A concrete lab order found/entered by a user from a lab result document
    /// </summary>
    /// <param name="Name">The name of the order, assigned by a user or automatically by the mapping process</param>
    /// <param name="Code">The code of the order, assigned by a user or automatically by the mapping process</param>
    /// <param name="LabOrderDefinition">The customer defintion of the order (filled in from the customer's database based on the Code)</param>
    /// <param name="Tests">The actual tests from a lab result document that make up the order</param>
    public record LabOrderActual(Optional<string> Name, Optional<string> Code, Optional<LabOrderDefinition> LabOrderDefinition, IList<LabTestActual> Tests);
}
