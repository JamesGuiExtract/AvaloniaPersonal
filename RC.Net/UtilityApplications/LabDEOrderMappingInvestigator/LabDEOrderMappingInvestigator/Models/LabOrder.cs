using DynamicData.Kernel;
using System.Collections.Generic;

namespace LabDEOrderMappingInvestigator.Models
{
    public record LabTestDefinition(string OfficialName, string Code, IList<string> ESComponentCodes, IList<string> AKAs);

    public record LabTestActual(
        Optional<LabTestDefinition> LabTestDefinition,
        string Name,
        string Value,
        string Range,
        string Units,
        string Flag);

    public record LabOrderDefinition(
        string Name,
        string Code,
        int FilledRequirement,
        string Tiebreaker,
        IList<LabTestDefinition> MandatoryTests,
        IList<LabTestDefinition> OptionalTests);

    public record LabOrderActual(Optional<string> Name, Optional<string> Code, Optional<LabOrderDefinition> LabOrderDefinition, IList<LabTestActual> Tests);
}
