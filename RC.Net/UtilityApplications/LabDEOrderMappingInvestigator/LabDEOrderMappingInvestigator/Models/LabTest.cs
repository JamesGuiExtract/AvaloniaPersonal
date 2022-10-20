using DynamicData.Kernel;
using System.Collections.Generic;

namespace LabDEOrderMappingInvestigator.Models
{
    /// <summary>
    /// A lab test as defined by a customer
    /// </summary>
    /// <param name="OfficialName">The name of the test (long key)</param>
    /// <param name="Code">The code for the test (alpha-numeric short key)</param>
    /// <param name="ESComponentCodes">Map this test to an Extract version to aid in matching actual tests from a document to their definitions</param>
    /// <param name="AKAs">Alternate test names (versions that appear on lab result documents)</param>
    /// <param name="BelongsToOrderCodes">The order codes that this test is associated with via the LabOrderTest table</param>
    /// <param name="DatabasePath">The customer OMDB path that this test is defined in</param>
    public record LabTestDefinition(
        string OfficialName,
        string Code,
        IList<string> ESComponentCodes,
        IList<string> AKAs,
        IList<string> BelongsToOrderCodes,
        string DatabasePath);

    /// <summary>
    /// A lab test found on a document and/or entered/edited by a user
    /// </summary>
    /// <param name="LabTestDefinition">Optional information about the test (official name/code, AKAs, mapping to Extract definitions, etc</param>
    /// <param name="Name">Either the official name or the name found on the document</param>
    /// <param name="Code">Code that maps the actual test on a document to a customer definition of a lab test</param>
    /// <param name="Value">The test result</param>
    /// <param name="Range">The reference range</param>
    /// <param name="Units">The units of the result</param>
    /// <param name="Flag">An abnormal marker</param>
    public record LabTestActual(
        Optional<LabTestDefinition> LabTestDefinition,
        string Name,
        Optional<string> Code,
        string Value,
        string Range,
        string Units,
        string Flag);

    /// <summary>
    /// A lab test defined by Extract (URS/FKB/ComponentData OMDB)
    /// </summary>
    /// <param name="Name">The Extract name for the test</param>
    /// <param name="Code">The Extract code for the test</param>
    /// <param name="AKAs">Alternate test names (versions that appear on lab result documents)</param>
    /// <param name="SampleType">Blood, Urine or empty (helps to disambiguate names)</param>
    /// <param name="OrderOfMagnitude">Typical values range for the result (helps to disambiguate names)</param>
    public record LabTestExtract(
        string Name,
        string Code,
        IList<string> AKAs,
        string SampleType,
        Optional<int> OrderOfMagnitude);
}
