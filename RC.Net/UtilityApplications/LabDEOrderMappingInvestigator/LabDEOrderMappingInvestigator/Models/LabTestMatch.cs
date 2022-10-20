namespace LabDEOrderMappingInvestigator.Models
{
    /// <summary>
    /// A candidate or existing mapping between a customer test and an Extract test definition
    /// </summary>
    /// <param name="CustomerTest">An actual customer test, including the test definition</param>
    /// <param name="ExtractTest">The Extract test that the customer test is matched against</param>
    /// <param name="Score">The quality of the match, as defined by whatever algorithm was used</param>
    public record LabTestMatch(LabTestActual CustomerTest, LabTestExtract ExtractTest, double Score);
}
